using System;
using System.Collections.Generic;

namespace Capstone.Modelos
{
    public class Paciente
    {
        public int IdPaciente { get; set; }
        public int? IdUsuario { get; set; }
        public string Rut { get; set; } = string.Empty;
        public string DvRut { get; set; } = string.Empty;
        public string Pnombre { get; set; } = string.Empty;
        public string? Snombre { get; set; }
        public string Apellidop { get; set; } = string.Empty;
        public string? Apellidom { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string? Prevision { get; set; }
        public string? Fono { get; set; }

        public Usuario? Usuario { get; set; }
        public ICollection<Ficha> Fichas { get; set; } = new List<Ficha>();
    }
}
