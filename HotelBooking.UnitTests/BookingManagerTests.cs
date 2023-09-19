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

            bookingRepositoryMock.Setup();

            // Act
            bool result = bookingService.CreateBooking(booking);

            // Assert
            Assert.True(result);
            Assert.True(booking.IsActive);
            Assert.Equal(1, booking.RoomId); // Ensure RoomId is set to the expected value
        }

        //[Fact]
        //public void CreateBooking_WhenRoomNotAvailable_ShouldReturnFalse()
        //{
        //    // Arrange
        //    var bookingRepositoryMock = new Mock<IBookingManager>();
        //    var bookingService = new BookingManager(bookingRepositoryMock.Object); // Inject the mock repository
        //    var booking = new Booking
        //    {
        //        StartDate = DateTime.Now.AddDays(1),
        //        EndDate = DateTime.Now.AddDays(3)
        //    };
        //    bookingRepositoryMock.Setup(repo => repo.FindAvailableRoom(booking.StartDate, booking.EndDate))
        //        .Returns(-1); // Assuming no room is available
        //    // Act
        //    bool result = bookingService.CreateBooking(booking);
        //    // Assert
        //    Assert.False(result);
        //    Assert.False(booking.IsActive); // Booking should not be active when room is not available
        //    Assert.Equal(-1, booking.RoomId); // RoomId should remain unchanged
        //}
    }
}
