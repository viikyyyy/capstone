namespace Capstone.Modelos;

public class CodigoRecuperacion
{
    public int IdCodigo { get; set; }
    public int IdUsuario { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public DateTime Expiracion { get; set; }
    public bool Usado { get; set; }
    public Usuario? Usuario { get; set; }
}
