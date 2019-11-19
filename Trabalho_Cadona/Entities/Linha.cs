using System.Collections.Generic;

namespace Trabalho_Cadona.Entities
{
    public class Linha
    {
        public string Codigo { get; set; }
        public string Descricao { get; set; }
        public List<Bloqueio> Bloqueios { get; set; }

        public Linha()
        {
            Bloqueios = new List<Bloqueio>();
        }

        public Linha(string s)
        {
            string[] Dados = s.Split('|');
            Codigo = Dados[0];
            Descricao = Dados[1];
            Bloqueios = new List<Bloqueio>();
        }

        public Linha(Linha linha)
        {
            Codigo = linha.Codigo;
            Descricao = linha.Descricao;
            if(linha.Bloqueios != null) Bloqueios = new List<Bloqueio>(linha.Bloqueios);
            else Bloqueios = new List<Bloqueio>();
        }

        public override string ToString()
        {
            return Codigo + "|" + Descricao + "|" + Bloqueios;
        }
    }
}
