using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConferenceRoom.Application;

namespace ConferenceRoom.UI;

public class BookingCommands
{
    private readonly IBookingService _service;
    private readonly InputReader _input;
    private readonly OutputWriter _output;

    // Statisk rumlista 
    private static readonly string[] Rooms =
    {
        "A1",
        "A2",
        "B1",
        "B2",
        "C1",
        "C2"
    };

    public BookingCommands(
        IBookingService service,
        InputReader input,
        OutputWriter output)
    {
        _service = service;
        _input = input;
        _output = output;
    }

    // Ange önskad tid -> visa endast lediga rum -> välj rum -> boka
    public async Task BookAvailableRoomAsync()
    {
        // Arrange
        var start = _input.ReadDateTime("Start (yyyy-MM-dd HH:mm): ");
        var end = _input.ReadDateTime("End   (yyyy-MM-dd HH:mm): ");

        // Act: filtrera rum på availability
        var availableRooms = new List<string>();
        foreach (var room in Rooms)
        {
            if (await _service.IsRoomAvailableAsync(room, start, end))
                availableRooms.Add(room);
        }

        // Assert / output
        if (availableRooms.Count == 0)
        {
            _output.Info("No available rooms during this time.");
            return;
        }

        _output.Info("Available rooms:");
        for (int i = 0; i < availableRooms.Count; i++)
            Console.WriteLine($"{i + 1}. {availableRooms[i]}");

        var selectedIndex = _input.ReadIntInRange("Choose Room (number): ", 1, availableRooms.Count);
        var selectedRoom = availableRooms[selectedIndex - 1];

        // Skapa bokningen (sparas i DB)
        var id = await _service.CreateBookingAsync(selectedRoom, start, end);

        _output.Info($"Booking created! Room: {selectedRoom}. Id: {id}");
    }

    public async Task ListBookingsAsync()
    {
        var bookings = await _service.GetAllBookingsAsync();

        if (!bookings.Any())
        {
            _output.Info("No bookings found.");
            return;
        }

        _output.Info("All bookings:");

        foreach (var b in bookings)
        {
            Console.WriteLine(
                $"{b.RoomId} | {b.Start:yyyy-MM-dd HH:mm} → {b.End:yyyy-MM-dd HH:mm}");
        }
    }
}