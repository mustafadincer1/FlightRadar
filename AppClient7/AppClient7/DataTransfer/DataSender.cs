using Amazon.Runtime.Internal;
using AppClient7.Configuration;
using AppClient7.Manager;
using AppClient7.Models;
using MongoDB.Driver.Core.Servers;
using Newtonsoft.Json;
using Serilog;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


public static class DataSender
{

    static DateTime? oncekininZamanı = null;
    private static long startTime;
    public static int port;
    public static List<AircraftData> lastFlights = new List<AircraftData>();
   

    public static async Task SendAircraftDataFromDbAsync(MongoDbManager mongoDbManager, CancellationToken cancellationToken)
    {
        List<AircraftData> aircraftList = mongoDbManager.GetAllFlightData();
        var groupedAircraftList = aircraftList.GroupBy(p => p.Hex);
        Dictionary<string, DateTime> FlightTime = groupedAircraftList.ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.Time).First().Time);
        var groupedByDateAircraftList = aircraftList.GroupBy(p => p.Time.ToString());
        List<AircraftData> lastFlight = new List<AircraftData>();

        try
        {
            DateTime time1;
            DateTime time2;
            int itemTime = 0;
            int milliseconds = 1000;

            foreach (var group in groupedByDateAircraftList)
            {
                if (groupedByDateAircraftList.Count() > itemTime + 1)
                {
                    time1 = groupedByDateAircraftList.ElementAt(itemTime).ElementAt(0).Time;
                    time2 = groupedByDateAircraftList.ElementAt(itemTime + 1).ElementAt(0).Time;
                    var timeDifference = time2 - time1;
                    milliseconds = ((int)timeDifference.TotalMilliseconds / 2);
                    itemTime++;
                }
                foreach (var item in group)
                {

                    if (item.Time == FlightTime[item.Hex])
                    {
                        lastFlight.Add(item);
                    }

                    await SendDataAsync(item, false);
                }
                await Task.Delay(milliseconds, cancellationToken);

                foreach (var item in lastFlight)
                {
                    await SendDataAsync(item, true);

                }
                lastFlight.Clear();
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

    }


    public static async Task SendAircraftDataFromAPIAsync(MongoDbManager mongoDbManager, CancellationToken cancellationToken)
    {
        List<AircraftData> allAircraftList = mongoDbManager.GetAllFlightData();
        using HttpClient client = new HttpClient();

        try
        {
            Task controlTask = Task.Run(() => ControlFunctionAsync(cancellationToken), cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    string jsonData = await FetchDataFromAPIAsync(client, cancellationToken);

                    List<AircraftData> aircraftList = JsonConvert.DeserializeObject<List<AircraftData>>(jsonData);

                    foreach (var aircraft in aircraftList)
                    {
                        if (!string.IsNullOrWhiteSpace(aircraft.Fli) &&
                            !allAircraftList.Any(a => a.Hex == aircraft.Hex && a.Lat == aircraft.Lat && a.Lon == aircraft.Lon))
                        {
                            TimeZoneInfo turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById(ConfigurationHelper.TimeZone);
                            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone);

                            aircraft.Time = localTime;
                            aircraft.IsSend = false;

                            AddOrUpdateAircraft(lastFlights, aircraft);
                            allAircraftList.Add(aircraft);
                            await mongoDbManager.InsertAircraftDataAsync(aircraft);
                            await SendDataAsync(aircraft, false);
                        }
                    }
                }
                catch (HttpRequestException e)
                {
                    Log.Error(e, "HTTP request error");
                }
                catch (TaskCanceledException)
                {
                    Log.Information("Task canceled");
                    break;
                }
                catch (Exception e)
                {
                    Log.Error(e, "Unexpected error");
                }


            }
        }
        catch (OperationCanceledException)
        {
            Log.Information("Program cancelled.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static async Task SendDataAsync(AircraftData aircraft, bool isLast)
    {
        using (TcpClient client = new TcpClient(ConfigurationHelper.serverIp, port))
        {
            if (aircraft.Lat != null && aircraft.Lon != null)
            {
                var serializedData = new
                {
                    FlightId = aircraft.Hex,
                    Latitude = aircraft.Lat,
                    Longitude = aircraft.Lon,
                    Velocity = aircraft.Spd,
                    Type = ConfigurationHelper.Type,
                    Status = ConfigurationHelper.Status,
                    DataType = ConfigurationHelper.DataType,
                    DeviceUnit = ConfigurationHelper.DeviceUnit,
                    IsLast = isLast.ToString(),
                };
                string aircraftJson = JsonConvert.SerializeObject(serializedData);
                byte[] data = Encoding.UTF8.GetBytes(aircraftJson);

                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
                Console.WriteLine($"Sent: {aircraftJson}");

                stream.Close();
            }
        }
    }

    static void AddOrUpdateAircraft(List<AircraftData> aircraftList, AircraftData newAircraft)
    {
        // Eğer aynı Hex değerine sahip bir uçak varsa, Time değerini güncelle
        // Aksi halde yeni bir uçak ekle
        bool updated = false;
        for (int i = 0; i < aircraftList.Count; i++)
        {
            if (aircraftList[i].Hex == newAircraft.Hex)
            {
                aircraftList[i].Time = newAircraft.Time;
                updated = true;
                break;
            }
        }

        if (!updated)
        {
            aircraftList.Add(newAircraft);
        }
    }

    public static async Task ControlFunctionAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TimeZoneInfo turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById(ConfigurationHelper.TimeZone);
                DateTime currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone);
             
                var toRemove = new List<AircraftData>();

                foreach (var aircraft in lastFlights)
                {
                    if ((currentTime - aircraft.Time).TotalSeconds > 30)
                    {
                        await SendDataAsync(aircraft, true);
                        toRemove.Add(aircraft);
                    }
                }

                // Remove the processed aircraft from the list after iteration to avoid modifying the collection while iterating
                
                 foreach (var aircraft in toRemove)
                {
                    lastFlights.Remove(aircraft);
                }

                // Wait for a short period before checking again
                await Task.Delay(5000, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            Log.Information("Control task canceled");
        }
        catch (Exception e)
        {
            Log.Error(e, "Unexpected error in control task");
        }
    }

   

    private static async Task<String> FetchDataFromAPIAsync(HttpClient client, CancellationToken cancellationToken)
    {
        try
        {
            //await Task.Delay(4000, cancellationToken);
            HttpResponseMessage response = await client.GetAsync(ConfigurationHelper.apiUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
            string jsonData = await response.Content.ReadAsStringAsync(cancellationToken);
            return jsonData;
        }
        catch (HttpRequestException e)
        {
            Log.Error(e, "Error fetching data from API");
            throw;
        }
    }


    








}



