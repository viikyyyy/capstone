using System;
using System.Collections.Generic;

namespace Capstone.Modelos
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Rut { get; set; } = string.Empty;
        public string DvRut { get; set; } = string.Empty;
        public string Pnombre { get; set; } = string.Empty;
        public string? Snombre { get; set; }
        public string Apellidop { get; set; } = string.Empty;
        public string? Apellidom { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string ContrasenaHash { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Estado { get; set; } = "activo";
        public DateTime FechaCreacion { get; set; } = HoraChile.Ahora;

        public ICollection<Paciente> Pacientes { get; set; } = new List<Paciente>();
        public ICollection<Ficha> Fichas { get; set; } = new List<Ficha>();
        public ICollection<Archivo> Archivos { get; set; } = new List<Archivo>();
        public ICollection<Auditoria> Auditorias { get; set; } = new List<Auditoria>();
    }
}
