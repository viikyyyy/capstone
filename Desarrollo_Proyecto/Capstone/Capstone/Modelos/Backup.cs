using System;

namespace Capstone.Modelos
{
    public class Backup
    {
        public int IdBackup { get; set; }
        public string TipoBackup { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public long? TamanoBytes { get; set; }
        public string Destino { get; set; } = string.Empty;
        public string? MensajeError { get; set; }
    }
}
