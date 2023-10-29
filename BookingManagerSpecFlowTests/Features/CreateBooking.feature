Feature: Create Booking

  Scenario: Creating a Booking
    Given a booking with a valid start and end date
    When I create the booking
    Then the booking should be added successfully

  Scenario: Creating a Booking with Invalid Date
    Given a booking with a start date in the past
    When I create the booking
    Then the booking creation should fail with an exception for incorrect date

  Scenario: Creating a Booking with No Available Rooms
    Given a new booking
    When I create the booking
    Then the booking creation should fail
