using System;
using System.Collections.Generic;

namespace Capstone.Modelos
{
    public class Ficha
    {
        public int IdFicha { get; set; }
        public int IdPaciente { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaAtencion { get; set; }
        public string MotivoConsulta { get; set; } = string.Empty;
        public string? Diagnostico { get; set; }
        public string? Tratamiento { get; set; }
        public string? Observaciones { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }

        public Paciente? Paciente { get; set; }
        public Usuario? Usuario { get; set; }
        public ICollection<Archivo> Archivos { get; set; } = new List<Archivo>();
    }
}
