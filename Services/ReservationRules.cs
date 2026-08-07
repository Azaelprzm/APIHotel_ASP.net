namespace HotelAPI.Services;

public static class ReservationRules
{
    public static bool HasValidDateRange(DateOnly start, DateOnly end) => start < end;

    public static bool Overlaps(
        DateOnly requestedStart,
        DateOnly requestedEnd,
        DateOnly existingStart,
        DateOnly existingEnd) =>
        requestedStart < existingEnd && requestedEnd > existingStart;

    public static decimal CalculateTotal(DateOnly start, DateOnly end, decimal nightlyRate) =>
        end.DayNumber > start.DayNumber
            ? (end.DayNumber - start.DayNumber) * nightlyRate
            : 0;

    public static string GetPaymentStatus(decimal paid, decimal total) =>
        paid <= 0 ? "Pendiente" : paid >= total ? "Pagado" : "Parcial";
}
