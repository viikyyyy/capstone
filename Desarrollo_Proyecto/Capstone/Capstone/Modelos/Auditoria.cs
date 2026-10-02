using System;

namespace Capstone.Modelos
{
    public class Auditoria
    {
        public int IdAuditoria { get; set; }
        public int? IdUsuario { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string EntidadAfectada { get; set; } = string.Empty;
        public int IdEntidadAfectada { get; set; }
        public string? Detalle { get; set; }
        public string IpOrigen { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }

        public Usuario? Usuario { get; set; }
    }
}
