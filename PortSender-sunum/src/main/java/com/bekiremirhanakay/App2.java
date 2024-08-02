package com.bekiremirhanakay;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.ServerSocket;
import java.net.Socket;

import com.bekiremirhanakay.Core.DataSender;
import com.bekiremirhanakay.Infrastructure.dto.TraceData;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;

public class App2 {

    private static final ObjectMapper objectMapper = new ObjectMapper();
    private static TraceData data;
    private static DataSender dataSender;

    public static void main(String[] args) {
        try {
            // Initialize DataSender
            dataSender = new DataSender();

            // Start the server to listen for connections
            try (ServerSocket serverSocket = new ServerSocket(56610)) {
                System.out.println("Server is listening on port 56600");

                while (true) {
                    // Accept a new client connection
                    Socket socket = serverSocket.accept();
                    System.out.println("New client connected");

                    // Handle the client connection in a new thread
                    new ClientHandler(socket).start();
                }
            } catch (IOException e) {
                e.printStackTrace();
            }
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    private static class ClientHandler extends Thread {
        private Socket socket;

        public ClientHandler(Socket socket) {
            this.socket = socket;
        }

        @Override
        public void run() {
            try (InputStream input = socket.getInputStream();
                 BufferedReader reader = new BufferedReader(new InputStreamReader(input))) {

                String line;
                while ((line = reader.readLine()) != null) {
                    // Print the raw JSON message
                    System.out.println("Received raw message: " + line);

                    try {
                        JsonNode jsonMessage = objectMapper.readTree(line);
                        String flightId = jsonMessage.get("FlightId").asText();
                        String latitude = jsonMessage.get("Latitude").asText();
                        String longitude = jsonMessage.get("Longitude").asText();
                        String velocity = jsonMessage.get("Velocity").asText();
                        String type = jsonMessage.get("Type").asText();
                        String datatype = jsonMessage.get("DataType").asText();
                        String status = jsonMessage.get("Status").asText();
                        String IsLast = jsonMessage.get("IsLast").asText();

                        // Print the parsed data
                        System.out.println("Parsed Data - FlightId: " + flightId +
                                ", Latitude: " + latitude +
                                ", Longitude: " + longitude +
                                ", Velocity: " + velocity +
                                ", Type: " + type +
                                ", DataType: " + datatype +
                                ", Status: " + status +
                                ", IsLast: " + IsLast) ;

                        data = new TraceData();
                        data.setDeviceID("A205");
                        data.setFlightID(flightId);
                        data.setLatitude(latitude);
                        data.setLongitude(longitude);
                        data.setVelocity(velocity);
                        data.setType(type);
                        data.setDataType(datatype);
                        data.setStatus(status);
                        data.setIsLast(IsLast);
                        dataSender.send(data);
                    } catch (IOException e) {
                        System.out.println("Failed to parse JSON: " + line);
                        e.printStackTrace();
                    }
                }
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    }
}
