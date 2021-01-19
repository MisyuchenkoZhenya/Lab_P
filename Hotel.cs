﻿using Lab_MP_AP.Loggers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Lab_MP_AP
{
    public class Hotel
    {
        private List<Room> rooms;
        private WaitingRoom waitingRoom;
        private List<Client> clientsForSettle;
        private ILogger logger;

        public DateTime currentDate { get; set; }
        public DateTime finalDate { get; set; }

        public int OccupiedRoomsCount
        {
            get {return rooms.Where(r => !r.IsAvailabe).Count();}
        }

        public int awaitersCount
        {
            get {return waitingRoom.Awaiters.Count();}
        }

        public Hotel(ILogger realizedLogger)
        {
            rooms = new List<Room>();
            waitingRoom = new WaitingRoom();
            clientsForSettle = new List<Client>();
            logger = realizedLogger;

            currentDate = new DateTime(2021, 01, 01);
            finalDate = currentDate.AddMonths(1);
        }

        public void StartWork()
        {
            while (currentDate < finalDate)
            {
                logger.Log(Environment.NewLine +
                            $"Day {currentDate.ToString("dd, HH:mm:ss")}. " +
                            $"({OccupiedRoomsCount} rooms are occupied, {awaitersCount} peoples in the waiting room)");

                var movedClientsCount = MoveOutClients(currentDate);
                if (movedClientsCount > 0)
                {
                    logger.Log($"{movedClientsCount} clients were moved out from hotel.");
                }

                if (new Random().Next(0, 100) > 85) // check if there are new clients
                {
                    int newClientsCount = new Random().Next(1, 5);
                    for (int i = 0; i < newClientsCount; i++)
                    {
                        clientsForSettle.Add(new Client());
                    }

                    logger.Log($"{newClientsCount} new clients arrived.");
                }

                var clientsFromWaitingRoom = waitingRoom.GetAvailableClients(currentDate);
                if (clientsFromWaitingRoom.Count > 0)
                {
                    clientsForSettle.AddRange(clientsFromWaitingRoom);
                    logger.Log($"{clientsFromWaitingRoom.Count} clients came again.");
                }

                if (clientsForSettle.Any())
                {
                    var settledClientsCount = 0;
                    var clientsWithoutRoomCount = 0;

                    while (clientsForSettle.Any())
                    {
                        var newClient = clientsForSettle.First();
                        clientsForSettle.Remove(newClient);
                        var room = GetAvailableRoom(newClient.Money);

                        if (room != null)
                        {
                            room.SettleClient(newClient, currentDate);
                            settledClientsCount++;
                            Thread.Sleep(2);
                        }
                        else
                        {
                            waitingRoom.AddClient(newClient, currentDate);
                            clientsWithoutRoomCount++;
                        }
                    }
                    logger.Log($"{settledClientsCount} clients were settled. " +
                                        $"{clientsWithoutRoomCount} clients didn't find the room.");
                }

                Thread.Sleep(6);
                currentDate = currentDate.AddMinutes(60);
            }
            
        }

        public void AddRooms(int count, int price)
        {
            for(int i = 0; i < count; i++)
            {
                rooms.Add(new Room(price));
            }
        }

        public Room GetAvailableRoom(int clientMoney)
        {
            return rooms.Where(r => (r.IsAvailabe && r.Price <= clientMoney)).
                         OrderByDescending(r => r.Price).
                         FirstOrDefault();
        }

        public int MoveOutClients(DateTime currentDate)
        {
            List<Room> roomsForMovingOut = rooms.Where(r => !r.IsAvailabe && r.CheckOutDate <= currentDate).ToList();
            foreach (var room in roomsForMovingOut)
            {
                room.RemoveClient();
            }

            return roomsForMovingOut.Count();
        }
    }
}
