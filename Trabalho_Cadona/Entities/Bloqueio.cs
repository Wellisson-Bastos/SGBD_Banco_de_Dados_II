namespace Trabalho_Cadona.Entities
{
    public class Bloqueio
    {
        public TipoBloqueio Tipo { get; set; }
        public string CodigoTransacao { get; set; }

        public Bloqueio() { }

        public Bloqueio(TipoBloqueio tipo, string codigoTransacao)
        {
            Tipo = tipo;
            CodigoTransacao = codigoTransacao;
        }
    }
}
