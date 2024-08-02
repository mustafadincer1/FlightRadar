package com.bekiremirhanakay;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.IOException;
import java.io.ObjectOutputStream;
import java.net.Socket;

import com.bekiremirhanakay.Core.DataSender;
import com.bekiremirhanakay.Presentation.MainMenu;


public class App 
{
    
    public static void main( String[] args ) throws IOException {

        MainMenu mainMenu = new MainMenu();
        DataSender dataSender = new DataSender(mainMenu);
        dataSender.open();
        Socket socket = null;
        ObjectOutputStream objectOutputStream = null;
        BufferedWriter fileWriter = null;
        BufferedReader socketReader = null;
            
         
        }


    }
