using System;

namespace Capstone.Modelos
{
    public class Archivo
    {
        public int IdArchivo { get; set; }
        public int IdFicha { get; set; }
        public int IdUsuario { get; set; }
        public string TipoArchivo { get; set; } = string.Empty;
        public string NombreArchivo { get; set; } = string.Empty;
        public string UrlStorage { get; set; } = string.Empty;
        public string HashIntegridad { get; set; } = string.Empty;
        public long TamanoBytes { get; set; }
        public DateTime FechaCarga { get; set; }

        public Ficha? Ficha { get; set; }
        public Usuario? Usuario { get; set; }
    }
}
