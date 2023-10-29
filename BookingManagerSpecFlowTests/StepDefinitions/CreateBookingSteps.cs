using HotelBooking.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelBooking.UnitTests.Fakes;
using TechTalk.SpecFlow;

namespace BookingManagerSpecFlowTests.StepDefinitions
{
    [Binding]
    public class CreateBookingSteps
    {
        private Booking booking = new Booking();
        private bool bookingResult;
        IRepository<Booking> bookingRepository;
        private IBookingManager bookingManager;
        ArgumentException exception;

        public CreateBookingSteps()
        {
            DateTime start = DateTime.Today.AddDays(10);
            DateTime end = DateTime.Today.AddDays(20);
            bookingRepository = new FakeBookingRepository(start, end);
            IRepository<Room> roomRepository = new FakeRoomRepository();
            bookingManager = new BookingManager(bookingRepository, roomRepository);
        }

        [Given("a booking with a valid start and end date")]
        public void GivenABookingWithValidDates()
        {
            booking.StartDate = DateTime.Now.AddDays(1);
            booking.EndDate = DateTime.Now.AddDays(3);
        }

        [Given("a booking with a start date in the past")]
        public void GivenABookingWithInvalidDates()
        {
            booking.StartDate = DateTime.Now.AddDays(-1);
            booking.EndDate = DateTime.Now.AddDays(3);
        }

        // The fake booking repository has two bookings with the same date, and the fake room repository only has two rooms.
        [Given("a new booking")]
        public void ANewBooking()
        {
            booking.StartDate = DateTime.Now.AddDays(10);
            booking.EndDate = DateTime.Now.AddDays(20);
        }

        [When("I create the booking")]
        public void WhenICreateTheBooking()
        {
            try
            {
                bookingResult = bookingManager.CreateBooking(booking);
            }
            catch (ArgumentException ae)
            {
                exception = ae;
            }
        }

        [Then("the booking should be added successfully")]
        public void ThenTheBookingShouldBeAddedSuccessfully()
        {
            Assert.True(bookingResult);
        }

        [Then("the booking creation should fail with an exception for incorrect date")]
        public void ThenTheBookingShouldFailWithException()
        {
            Assert.NotNull(exception);
            Assert.Equal("The start date cannot be in the past or later than the end date.", exception.Message);
        }

        [Then("the booking creation should fail")]
        public void ThenTheBookingShouldFail()
        {
            Assert.False(bookingResult);
        }
    }
}
