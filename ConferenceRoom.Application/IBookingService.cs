using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ConferenceRoom.Domain;

namespace ConferenceRoom.Application
{
    public interface IBookingService
    {
        Task<Guid> CreateBookingAsync(string roomId, DateTime start, DateTime end);
        Task<bool> IsRoomAvailableAsync(string roomId, DateTime start, DateTime end);
        Task<IReadOnlyList<Booking>> ListBookingsForRoomAsync(string roomId);
        Task<List<Booking>> GetAllBookingsAsync();
    }
}
