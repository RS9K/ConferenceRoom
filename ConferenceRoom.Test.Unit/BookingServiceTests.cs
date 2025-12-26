using ConferenceRoom.Application;
using ConferenceRoom.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ConferenceRoom.Test.Unit
{
    public class BookingServiceTests
    {
        [Fact]
        public async Task CreateBooking_Should_SaveBooking_When_NoExistingBooking()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<BookingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var db = new BookingDbContext(options);
            var sut = new BookingService(db);

            var roomId = "A1";
            var start = new DateTime(2025, 01, 01, 10, 00, 00, DateTimeKind.Local);
            var end = new DateTime(2025, 01, 01, 11, 00, 00, DateTimeKind.Local);

            // Act
            var createdId = await sut.CreateBookingAsync(roomId, start, end);

            // Assert
            Assert.NotEqual(Guid.Empty, createdId);

            var saved = await db.Bookings.SingleAsync();
            Assert.Equal(roomId, saved.RoomId);
            Assert.Equal(start, saved.Start);
            Assert.Equal(end, saved.End);

        }

        [Fact]
        public async Task CreateBooking_Should_Reject_When_OverlappingBookingExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<BookingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var db = new BookingDbContext(options);
            var sut = new BookingService(db);

            var roomId = "A1";

            var existingStart = new DateTime(2025, 01, 01, 10, 00, 00, DateTimeKind.Local);
            var existingEnd = new DateTime(2025, 01, 01, 11, 00, 00, DateTimeKind.Local);

            var newStart = new DateTime(2025, 01, 01, 10, 30, 00, DateTimeKind.Local);
            var newEnd = new DateTime(2025, 01, 01, 11, 30, 00, DateTimeKind.Local);

            await sut.CreateBookingAsync(roomId, existingStart, existingEnd);

            // Act
            var act = async () => await sut.CreateBookingAsync(roomId, newStart, newEnd);

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(act);
        }

        [Fact]
        public async Task IsRoomAvailable_Should_ReturnFalse_When_OverlappingBookingExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<BookingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var db = new BookingDbContext(options);
            var sut = new BookingService(db);

            var roomId = "A1";

            var existingStart = new DateTime(2025, 01, 01, 10, 00, 00, DateTimeKind.Local);
            var existingEnd = new DateTime(2025, 01, 01, 11, 00, 00, DateTimeKind.Local);

            await sut.CreateBookingAsync(roomId, existingStart, existingEnd);

            var checkStart = new DateTime(2025, 01, 01, 10, 30, 00, DateTimeKind.Local);
            var checkEnd = new DateTime(2025, 01, 01, 11, 30, 00, DateTimeKind.Local);

            // Act
            var available = await sut.IsRoomAvailableAsync(roomId, checkStart, checkEnd);

            // Assert
            Assert.False(available);
        }

        [Fact]
        public async Task IsRoomAvailable_Should_ReturnTrue_When_NoOverlap()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<BookingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var db = new BookingDbContext(options);
            var sut = new BookingService(db);

            var roomId = "A1";

            await sut.CreateBookingAsync(
                roomId,
                new DateTime(2025, 01, 01, 10, 00, 00, DateTimeKind.Local),
                new DateTime(2025, 01, 01, 11, 00, 00, DateTimeKind.Local)
            );

            // Act
            var available = await sut.IsRoomAvailableAsync(
                roomId,
                new DateTime(2025, 01, 01, 11, 00, 00, DateTimeKind.Local),
                new DateTime(2025, 01, 01, 12, 00, 00, DateTimeKind.Local)
            );

            // Assert
            Assert.True(available);
        }
        [Fact]
        public async Task ListBookingsForRoom_Should_ReturnOnlyThatRoom_OrderedByStart()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<BookingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var db = new BookingDbContext(options);
            var sut = new BookingService(db);

            await sut.CreateBookingAsync(
                "A1",
                new DateTime(2025, 01, 01, 12, 00, 00, DateTimeKind.Local),
                new DateTime(2025, 01, 01, 13, 00, 00, DateTimeKind.Local));

            await sut.CreateBookingAsync(
                "B1",
                new DateTime(2025, 01, 01, 09, 00, 00, DateTimeKind.Local),
                new DateTime(2025, 01, 01, 10, 00, 00, DateTimeKind.Local));

            await sut.CreateBookingAsync(
                "A1",
                new DateTime(2025, 01, 01, 10, 00, 00, DateTimeKind.Local),
                new DateTime(2025, 01, 01, 11, 00, 00, DateTimeKind.Local));

            // Act
            var result = await sut.ListBookingsForRoomAsync("A1");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, b => Assert.Equal("A1", b.RoomId));
            Assert.True(result[0].Start <= result[1].Start);
            Assert.Equal(new DateTime(2025, 01, 01, 10, 00, 00, DateTimeKind.Local), result[0].Start);
        }
    }
}
