using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceRoom.UI;

public class MainMenu
{
    private readonly BookingCommands _commands;

    public MainMenu(BookingCommands commands)
    {
        _commands = commands;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            Console.WriteLine("\n === Conference Room === ");
            Console.WriteLine("1. Book available room");
            Console.WriteLine("2. List bookings");
            Console.WriteLine("0. Exit");

            Console.Write("Choice: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await _commands.BookAvailableRoomAsync();
                    break;

                case "2":
                    await _commands.ListBookingsAsync();
                    break;

                case "0":
                    Console.WriteLine("Exiting Program...");
                    Environment.Exit(0);
                    break;

                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }
    }
}