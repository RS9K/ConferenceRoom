using ConferenceRoom.Domain;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoom.Infrastructure;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
    {
    }

    public DbSet<Booking> Bookings => Set<Booking>();
}

