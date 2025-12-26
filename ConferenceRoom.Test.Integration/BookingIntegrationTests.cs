using ConferenceRoom.Application;
using ConferenceRoom.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ConferenceRoom.Test.Integration;

public class BookingServiceSqlServerIntegrationTests
{
    private static DbContextOptions<BookingDbContext> CreateSqlServerOptions(string dbName)
    {
        var connectionString =
            $"Server=(localdb)\\MSSQLLocalDB;" +
            $"Database={dbName};" +
            $"Trusted_Connection=True;" +
            $"MultipleActiveResultSets=true;" +
            $"TrustServerCertificate=True;";

        return new DbContextOptionsBuilder<BookingDbContext>()
            .UseSqlServer(connectionString)
            .Options;
    }

    [Fact]
    public async Task CreateBooking_Should_Persist_ToSqlServer()
    {
        // Arrange
        var dbName = "ConferenceRoom_IT_" + Guid.NewGuid().ToString("N");
        var options = CreateSqlServerOptions(dbName);

        await using (var db = new BookingDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }

        Guid createdId;
        await using (var db = new BookingDbContext(options))
        {
            var sut = new BookingService(db);

            // Act
            createdId = await sut.CreateBookingAsync(
                "A101",
                new DateTime(2025, 01, 01, 10, 00, 00, DateTimeKind.Local),
                new DateTime(2025, 01, 01, 11, 00, 00, DateTimeKind.Local));
        }

        // Assert
        await using (var verifyDb = new BookingDbContext(options))
        {
            var saved = await verifyDb.Bookings.SingleAsync();
            Assert.Equal(createdId, saved.Id);
            Assert.Equal("A101", saved.RoomId);
            Assert.Equal(new DateTime(2025, 01, 01, 10, 00, 00, DateTimeKind.Local), saved.Start);
            Assert.Equal(new DateTime(2025, 01, 01, 11, 00, 00, DateTimeKind.Local), saved.End);
        }
    }

    [Fact]
    public async Task CreateBooking_Should_Reject_Overlap_InSqlServer()
    {
        // Arrange
        var dbName = "ConferenceRoom_IT_" + Guid.NewGuid().ToString("N");
        var options = CreateSqlServerOptions(dbName);

        await using (var db = new BookingDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }

        await using var ctx = new BookingDbContext(options);
        var sut = new BookingService(ctx);

        await sut.CreateBookingAsync(
            "A101",
            new DateTime(2025, 01, 01, 10, 00, 00, DateTimeKind.Local),
            new DateTime(2025, 01, 01, 11, 00, 00, DateTimeKind.Local));

        // Act
        var act = async () => await sut.CreateBookingAsync(
            "A101",
            new DateTime(2025, 01, 01, 10, 30, 00, DateTimeKind.Local),
            new DateTime(2025, 01, 01, 11, 30, 00, DateTimeKind.Local));

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }

    [Fact]
    public async Task ListBookingsForRoom_Should_ReturnOnlyThatRoom_Ordered_InSqlServer()
    {
        // Arrange
        var dbName = "ConferenceRoom_IT_" + Guid.NewGuid().ToString("N");
        var options = CreateSqlServerOptions(dbName);

        await using (var db = new BookingDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }

        await using var ctx = new BookingDbContext(options);
        var sut = new BookingService(ctx);

        await sut.CreateBookingAsync(
            "A101",
            new DateTime(2025, 01, 01, 12, 00, 00, DateTimeKind.Local),
            new DateTime(2025, 01, 01, 13, 00, 00, DateTimeKind.Local));

        await sut.CreateBookingAsync(
            "B202",
            new DateTime(2025, 01, 01, 09, 00, 00, DateTimeKind.Local),
            new DateTime(2025, 01, 01, 10, 00, 00, DateTimeKind.Local));

        await sut.CreateBookingAsync(
            "A101",
            new DateTime(2025, 01, 01, 10, 00, 00, DateTimeKind.Local),
            new DateTime(2025, 01, 01, 11, 00, 00, DateTimeKind.Local));

        // Act
        var result = await sut.ListBookingsForRoomAsync("A101");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, b => Assert.Equal("A101", b.RoomId));
        Assert.Equal(new DateTime(2025, 01, 01, 10, 00, 00, DateTimeKind.Local), result[0].Start);
        Assert.Equal(new DateTime(2025, 01, 01, 12, 00, 00, DateTimeKind.Local), result[1].Start);
    }
}
