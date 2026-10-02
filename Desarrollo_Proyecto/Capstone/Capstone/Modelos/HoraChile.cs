namespace Capstone.Modelos;

public static class HoraChile
{
    public static DateTime Ahora
    {
        get
        {
            var zona = TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows() ? "Pacific SA Standard Time" : "America/Santiago");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zona);
        }
    }
}
