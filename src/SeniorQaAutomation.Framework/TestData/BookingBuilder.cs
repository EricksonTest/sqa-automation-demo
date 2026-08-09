using System.Security.Cryptography;
using SeniorQaAutomation.Framework.Models;

namespace SeniorQaAutomation.Framework.TestData;

public sealed class BookingBuilder
{
    private static int _dateOffset = RandomNumberGenerator.GetInt32(3_650, 14_000);
    private int _roomId = 1;
    private string _firstName = $"Api{Guid.NewGuid():N}"[..11];
    private string _lastName = $"Test{Guid.NewGuid():N}"[..12];
    private bool _depositPaid = true;
    private BookingDates _dates = NextAvailableDates();
    private string? _email;
    private string? _phone;

    private BookingBuilder()
    {
    }

    public static BookingBuilder CreateValid() => new();

    public BookingBuilder WithGuest(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
        return this;
    }

    public BookingBuilder WithRoom(int roomId)
    {
        _roomId = roomId;
        return this;
    }

    public BookingBuilder WithDeposit(bool depositPaid)
    {
        _depositPaid = depositPaid;
        return this;
    }

    public BookingBuilder WithStayStartingIn(int daysFromNow, int nights = 2)
    {
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(daysFromNow);
        return WithStay(checkIn, checkIn.AddDays(nights));
    }

    public BookingBuilder WithStay(DateOnly checkIn, DateOnly checkOut)
    {
        _dates = new BookingDates(checkIn, checkOut);
        return this;
    }

    public BookingBuilder WithContactDetails(string email, string phone)
    {
        _email = email;
        _phone = phone;
        return this;
    }

    public Booking Build() => new()
    {
        RoomId = _roomId,
        FirstName = _firstName,
        LastName = _lastName,
        DepositPaid = _depositPaid,
        BookingDates = _dates,
        Email = _email,
        Phone = _phone
    };

    private static BookingDates NextAvailableDates()
    {
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow)
            .AddDays(Interlocked.Add(ref _dateOffset, 4));
        return new BookingDates(checkIn, checkIn.AddDays(2));
    }
}
