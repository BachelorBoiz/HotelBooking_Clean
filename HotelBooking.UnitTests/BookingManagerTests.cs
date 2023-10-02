using System;
using System.Collections.Generic;
using HotelBooking.Core;
using HotelBooking.UnitTests.Fakes;
using Xunit;
using System.Linq;
using Moq;


namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private IBookingManager bookingManager;
        IRepository<Booking> bookingRepository;

        public BookingManagerTests(){
            DateTime start = DateTime.Today.AddDays(10);
            DateTime end = DateTime.Today.AddDays(20);
            bookingRepository = new FakeBookingRepository(start, end);
            IRepository<Room> roomRepository = new FakeRoomRepository();
            bookingManager = new BookingManager(bookingRepository, roomRepository);
        }

        [Fact]
        public void FindAvailableRoom_StartDateNotInTheFuture_ThrowsArgumentException()
        {
            // Arrange
            DateTime date = DateTime.Today;

            // Act
            Action act = () => bookingManager.FindAvailableRoom(date, date);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            // Act
            int roomId = bookingManager.FindAvailableRoom(date, date);
            // Assert
            Assert.NotEqual(-1, roomId);
        }

        [Fact]
        public void FindAvailableRoom_RoomAvailable_ReturnsAvailableRoom()
        {
            // This test was added to satisfy the following test design
            // principle: "Tests should have strong assertions".

            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            // Act
            int roomId = bookingManager.FindAvailableRoom(date, date);

            // Assert
            var bookingForReturnedRoomId = bookingRepository.GetAll().Where(
                b => b.RoomId == roomId
                && b.StartDate <= date
                && b.EndDate >= date
                && b.IsActive);

            Assert.Empty(bookingForReturnedRoomId);
        }
        
        [Fact]
        public void CreateBooking_WhenRoomAvailable_ShouldReturnTrue()
        {
            // Arrange
            var bookingRepositoryMock = new Mock<IRepository<Booking>>();
            var roomRepositoryMock = new Mock<IRepository<Room>>();
            var bookingService = new BookingManager(bookingRepositoryMock.Object,roomRepositoryMock.Object); // Inject the mock repository
            var booking = new Booking
            {
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3)
            };
            var rooms = new List<Room>
            {
                new Room { Id=1, Description="A" },
                new Room { Id=2, Description="B" },
            };

            // Set up mock repository behavior
            roomRepositoryMock.Setup(repo => repo.GetAll()).Returns(rooms); // Mock room retrieval
            bookingRepositoryMock.Setup(repo => repo.Add(It.IsAny<Booking>())).Callback<Booking>(b =>
            {
                // Simulate saving the booking by modifying its properties
                booking.Id = 123; // Assign a fake booking ID
                booking.IsActive = true; // Set IsActive to true
                booking.RoomId = 1; // Assign a room ID to the booking
            });

            // Act
            bool result = bookingService.CreateBooking(booking);

            // Assert
            Assert.True(result); // Booking creation should succeed
            Assert.True(booking.IsActive); // Ensure IsActive is set to true
            Assert.Equal(1, booking.RoomId); // Ensure RoomId is set to the expected value
            bookingRepositoryMock.Verify(repo => repo.Add(It.IsAny<Booking>()), Times.Once); // Verify that the Add method was called once
        }

        [Fact]
        public void CreateBooking_WhenRoomNotAvailable_ShouldReturnFalse()
        {
            // Arrange
            var bookingRepositoryMock = new Mock<IRepository<Booking>>();
            var roomRepositoryMock = new Mock<IRepository<Room>>();
            var bookingManager = new BookingManager(bookingRepositoryMock.Object, roomRepositoryMock.Object);

            var booking = new Booking
            {
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddDays(4)
            };

            var rooms = new List<Room>
            {
                new Room { Id = 1, Description = "A" }
            };

            var existingBooking = new Booking
            {
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                IsActive = true,
                RoomId = 1
            };

            bookingRepositoryMock.Setup(repo => repo.GetAll()).Returns(new List<Booking> { existingBooking });
            roomRepositoryMock.Setup(repo => repo.GetAll()).Returns(rooms);

            // Act
            bool result = bookingManager.CreateBooking(booking);

            // Assert
            Assert.False(result);
            Assert.False(booking.IsActive);
            Assert.Equal(-1, booking.RoomId);
            bookingRepositoryMock.Verify(repo => repo.Add(booking), Times.Never);
        }

        [Fact]
        public void GetFullyOccupiedDates_ReturnsCorrectDates()
        {
            // Arrange
            var startDate = new DateTime(2023, 10, 1);
            var endDate = new DateTime(2023, 10, 3);

            // Mock the IRepository<Booking> and IRepository<Room>
            var bookingRepositoryMock = new Mock<IRepository<Booking>>();
            var roomRepositoryMock = new Mock<IRepository<Room>>();

            // Define some sample data for the mock repositories
            var bookings = new List<Booking>
            {
                new Booking { IsActive = true, StartDate = startDate, EndDate = endDate }, // Booking that spans the entire period
                new Booking { IsActive = true, StartDate = startDate, EndDate = endDate }, // Another booking that spans the entire period
            };

            var rooms = new List<Room>
            {
                new Room(),
                new Room(),
            };

            // Set up the mock repository methods
            bookingRepositoryMock.Setup(repo => repo.GetAll()).Returns(bookings.AsQueryable());
            roomRepositoryMock.Setup(repo => repo.GetAll()).Returns(rooms.AsQueryable());

            var bookingManager = new BookingManager(bookingRepositoryMock.Object, roomRepositoryMock.Object);


            // Act
            var fullyOccupiedDates = bookingManager.GetFullyOccupiedDates(startDate, endDate);

            // Assert
            // In this example, both bookings span the entire period, so all dates in the range should be fully occupied.
            Assert.Equal(new List<DateTime> { startDate, startDate.AddDays(1), endDate}, fullyOccupiedDates);
        }
    }
}
