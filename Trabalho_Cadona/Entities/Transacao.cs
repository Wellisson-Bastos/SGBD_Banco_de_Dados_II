using Trabalho_Cadona.Forms;

namespace Trabalho_Cadona.Entities
{
    public class Transacao
    {
        public string NomeTransacao { get; set; }
        public string CodigoTransacao { get; set; }
        public Lock Bloqueado { get; set; }
        public TransacaoForm Form;

        public Transacao(string nome, string codigo)
        {
            NomeTransacao = nome;
            CodigoTransacao = codigo;
            Bloqueado = new Lock() { Locked = false };
        }
    }
}
