package com.bekiremirhanakay;

import java.io.BufferedWriter;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStream;
import java.net.ServerSocket;
import java.net.Socket;
import java.util.concurrent.TimeoutException;

public class adsb {
    public static void main(String[] args) throws IOException, TimeoutException {
        try (ServerSocket serverSocket = new ServerSocket(56615)) {
            System.out.println("Server is listening on port " + 56615);

            while (true) {
                Socket socket = serverSocket.accept();
                System.out.println("New client connected");

                InputStream input = socket.getInputStream();
                byte[] buffer = new byte[1024];
                int bytesRead = input.read(buffer);

                // Decode the received bytes into a string
                String message = new String(buffer, 0, bytesRead, "ASCII");
                System.out.println("Received: " + message);

                // Write the received message to a file
                try (FileWriter fileWriter = new FileWriter("C:\\Users\\staj\\Desktop\\aselsanStaj-main\\DataModel-sunum\\received_messages.txt", true);
                     BufferedWriter bufferedWriter = new BufferedWriter(fileWriter)) {
                    bufferedWriter.write(message);
                    bufferedWriter.newLine();
                } catch (IOException e) {
                    e.printStackTrace();
                }

                socket.close();
            }
        } catch (IOException ex) {
            ex.printStackTrace();
        }



    }
}
