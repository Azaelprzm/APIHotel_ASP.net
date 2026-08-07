using HotelAPI.Services;
using Xunit;

namespace HotelAPI.Tests;

public class ReservationRulesTests
{
    [Fact]
    public void HasValidDateRange_RequiresEndAfterStart()
    {
        var start = new DateOnly(2026, 8, 10);

        Assert.True(ReservationRules.HasValidDateRange(start, start.AddDays(1)));
        Assert.False(ReservationRules.HasValidDateRange(start, start));
        Assert.False(ReservationRules.HasValidDateRange(start, start.AddDays(-1)));
    }

    [Fact]
    public void Overlaps_AllowsConsecutiveReservations()
    {
        var existingStart = new DateOnly(2026, 8, 10);
        var existingEnd = new DateOnly(2026, 8, 12);

        Assert.False(ReservationRules.Overlaps(
            new DateOnly(2026, 8, 8),
            existingStart,
            existingStart,
            existingEnd));

        Assert.False(ReservationRules.Overlaps(
            existingEnd,
            new DateOnly(2026, 8, 14),
            existingStart,
            existingEnd));
    }

    [Fact]
    public void Overlaps_DetectsIntersectingDates()
    {
        Assert.True(ReservationRules.Overlaps(
            new DateOnly(2026, 8, 11),
            new DateOnly(2026, 8, 13),
            new DateOnly(2026, 8, 10),
            new DateOnly(2026, 8, 12)));
    }

    [Fact]
    public void CalculateTotal_UsesNightsAndNightlyRate()
    {
        var total = ReservationRules.CalculateTotal(
            new DateOnly(2026, 8, 10),
            new DateOnly(2026, 8, 13),
            850m);

        Assert.Equal(2550m, total);
    }

    [Theory]
    [InlineData(0, 1000, "Pendiente")]
    [InlineData(250, 1000, "Parcial")]
    [InlineData(1000, 1000, "Pagado")]
    [InlineData(1200, 1000, "Pagado")]
    public void GetPaymentStatus_ReturnsExpectedStatus(
        decimal paid,
        decimal total,
        string expected)
    {
        Assert.Equal(expected, ReservationRules.GetPaymentStatus(paid, total));
    }
}
