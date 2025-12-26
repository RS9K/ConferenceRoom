using ConferenceRoom.Domain;
using ConferenceRoom.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ConferenceRoom.Application;

public class BookingService : IBookingService
{
    private readonly BookingDbContext _db;

    public BookingService(BookingDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> CreateBookingAsync(string roomId, DateTime start, DateTime end)
    {
        // Guard
        roomId = NormalizeRoomId(roomId);
        (start, end) = NormalizeLocalRange(start, end);

        if (await HasOverlapAsync(roomId, start, end))
        {
            throw new InvalidOperationException("The booking overlaps with an existing booking.");
        }

        var booking = new Booking
        {
            RoomId = roomId,
            Start = start,
            End = end
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();
        return booking.Id;
    }

    private static string NormalizeRoomId(string roomId)
    {
        if (string.IsNullOrWhiteSpace(roomId))
            throw new ArgumentException("Room ID is required!", nameof(roomId));

        return roomId.Trim();
    }
    
    private static (DateTime Start, DateTime End) NormalizeLocalRange(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new ArgumentException("End time must be AFTER start time");

        start = DateTime.SpecifyKind(start, DateTimeKind.Local);
        end = DateTime.SpecifyKind(end, DateTimeKind.Local);
        return (start, end);
    }

    private Task<bool> HasOverlapAsync(string roomId, DateTime start, DateTime end)
    {
        return _db.Bookings.AnyAsync(b =>

        b.RoomId == roomId &&
        start < b.End &&
        end > b.Start);
    }

    public async Task<bool> IsRoomAvailableAsync(string roomId, DateTime start, DateTime end)
    {
        roomId = NormalizeRoomId(roomId);
        (start, end) = NormalizeLocalRange(start, end);

        return !await HasOverlapAsync(roomId, start, end);
    }
    public async Task<IReadOnlyList<Booking>> ListBookingsForRoomAsync(string roomId)
    {
        // Arrange / Guard
        roomId = NormalizeRoomId(roomId);

        // Act
        var bookings = await _db.Bookings
            .Where(b => b.RoomId == roomId)
            .OrderBy(b => b.Start)
            .ToListAsync();

        // Assert / Return
        return bookings;
    }
    public async Task<List<Booking>> GetAllBookingsAsync()
    {
        return await _db.Bookings
            .OrderBy(b => b.Start)
            .ToListAsync();
    }
}
