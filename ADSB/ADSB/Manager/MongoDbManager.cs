using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppClient7.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Serilog;

namespace AppClient7.Manager
{
    public class MongoDbManager
    {
        private readonly IMongoCollection<AircraftData> _collection; // MongoDB koleksiyonunu temsil eden alan

        // Bağlantı bilgilerini alarak MongoDbManager sınıfını başlatan constructor
        public MongoDbManager(string connectionString, string databaseName, string collectionName)
        {
            var client = new MongoClient(connectionString); // MongoClient nesnesi oluşturuluyor
            var database = client.GetDatabase(databaseName); // Veritabanı seçiliyor
            _collection = database.GetCollection<AircraftData>(collectionName); // Verilen koleksiyon adıyla koleksiyon nesnesi oluşturuluyor
        }

        // Asenkron olarak uçak verisi ekleyen metot
        public async Task InsertAircraftDataAsync(AircraftData aircraft)
        {
            try
            {
                await _collection.InsertOneAsync(aircraft); // Belirtilen koleksiyona bir belge ekleniyor
            }
            catch (Exception ex)
            {
                SerilogManager.LogError(ex, "Error inserting data into MongoDB.");
                throw;
            }
        }

        // Tüm uçuş verilerini MongoDB'den alan metot
        public List<AircraftData> GetAllFlightData()
        {
            return _collection.Find(new BsonDocument()).ToList(); // Boş bir filtre ile tüm belgeleri alıp listeye dönüştürüyor
        }

        // Belirli bir filtre ile uçuş verilerini MongoDB'den alan metot
        public List<AircraftData> GetFlightData(FilterDefinition<AircraftData> filter)
        {
            return _collection.Find(filter).ToList(); // Verilen filtreye göre belgeleri alıp listeye dönüştürüyor
        }

        // Gönderilmemiş uçuş verilerini alıp güncelleyen ve döndüren metot
        public List<AircraftData> GetSendFlightData()
        {
            var filter = Builders<AircraftData>.Filter.Eq(x => x.IsSend, false) & // IsSend özelliği false olanları filtreliyor
                         Builders<AircraftData>.Filter.Ne(x => x.Lat, null) & // Lat özelliği null olmayanları filtreliyor
                         Builders<AircraftData>.Filter.Ne(x => x.Lon, null); // Lon özelliği null olmayanları filtreliyor

            var aircraftDataList = _collection.Find(filter).ToList(); // Filtre ile belgeleri alıp listeye dönüştürüyor

            if (aircraftDataList.Count > 0)
            {
                var update = Builders<AircraftData>.Update.Set(x => x.IsSend, true); // IsSend özelliğini true olarak güncellemek için update tanımlanıyor
                var ids = aircraftDataList.Select(x => x.Id).ToList(); // Güncellenecek belgelerin Id'lerini alıyor
                var idFilter = Builders<AircraftData>.Filter.In(x => x.Id, ids); // Id'ler ile filtre oluşturuyor

                try
                {
                    _collection.UpdateMany(idFilter, update); // Belirtilen Id'ler için belgeleri güncelliyor
                }
                catch (Exception ex) { 
                    Console.WriteLine($"Error occurred in UpdateMany operation: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("No record found to update.");
            }


            return aircraftDataList;
        }
        // Birden fazla uçak verisini asenkron olarak MongoDB'ye ekleyen metot
        public async Task InsertManyAircraftDataAsync(List<AircraftData> aircraftDataList)
        {
            if (aircraftDataList != null && aircraftDataList.Count > 0)
            {
                await _collection.InsertManyAsync(aircraftDataList); // Belirtilen koleksiyona birden fazla belge ekliyor
            }
        }

        // Time bileşenlerine göre uçak verilerini gruplandıran metot
        public List<AircraftData> GroupAircraftDataByTime()
        {
            var pipeline = new BsonDocument[]
          {
            new BsonDocument("$group",
                new BsonDocument
                {
                    { "_id", new BsonDocument
                        {
                            { "year", new BsonDocument("$year", "$Time") },
                            { "month", new BsonDocument("$month", "$Time") },
                            { "day", new BsonDocument("$dayOfMonth", "$Time") },
                             { "hour", new BsonDocument("$hour", "$Time") },
                            { "minute", new BsonDocument("$minute", "$Time") },
                            { "second", new BsonDocument("$second", "$Time") }
                        }
                    },
                    { "count", new BsonDocument("$sum", 1) }
                }
            )
          };

            var result = _collection.Aggregate<BsonDocument>(pipeline).ToList();

            List<AircraftData> groupedAircraft = new List<AircraftData>();

            result.ForEach(group =>
            {
                var year = group["_id"]["year"].ToInt32();
                var month = group["_id"]["$month"].ToInt32();
                var day = group["_id"]["day"].ToInt32();
                var hour = group["_id"]["hour"].ToInt32();
                var minute = group["_id"]["minute"].ToInt32();
                var second = group["_id"]["second"].ToInt32();

                var documents = group["documents"].AsBsonArray;
                foreach (var document in documents)
                {
                    var bsonDocument = document.AsBsonDocument;
                    var aircraftData = BsonSerializer.Deserialize<AircraftData>(bsonDocument);
                    groupedAircraft.Add(aircraftData);
                }
            });
         

            return groupedAircraft;
        }

        // Serilog ile uçak verilerini loglama metotu
        public void LogAircraftData(List<AircraftData> aircraftDataList)
        {
            foreach (var aircraftData in aircraftDataList)
            {
                var data = new { type10 = aircraftData.Lat, type12 = aircraftData.Lon, type22 = aircraftData.Spd, type32 = aircraftData.Hex };
                SerilogManager.LogInformationJson("Veri {@Data}", data); // JSON formatında loglama yapılıyor
            }
        }
    }

    }

