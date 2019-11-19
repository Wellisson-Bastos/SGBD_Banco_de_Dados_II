namespace Trabalho_Cadona.Entities
{
    class LinhaLog
    {
        public string Codigo { get; set; }
        public string Acao { get; set; }
        public string Dados { get; set; }

        public LinhaLog(string lLinha)
        {
            string[] lDados = lLinha.Split('|');
            Codigo = lDados[0];
            Acao = lDados[1];
            Dados = lDados[2];
        }
    }
}
