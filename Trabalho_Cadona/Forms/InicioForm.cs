using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Trabalho_Cadona.Entities;
using Trabalho_Cadona.Forms;

namespace Trabalho_Cadona
{
    public partial class Inicio : Form
    {
        public static int Count = 1;
        public InicioClass inicio = new InicioClass();

        public Inicio()
        {
            InitializeComponent();
            CarregarLinhas();
            CarregarListView();
        }

        public void CarregarListView()
        {
            listView1.Items.Clear();

            foreach (var i in inicio.Linhas)
            {
                ListViewItem Item = new ListViewItem();
                Item.Text = i.Codigo;
                listView1.Items.Add(Item);
                Item.SubItems.Add(i.Descricao);
                if (i.Bloqueios != null)
                    foreach (var Bloqueio in i.Bloqueios)
                    {
                        Item.SubItems.Add(Bloqueio.Tipo.ToString());
                        break;
                    }
            }
        }

        public void CarregarLinhas()
        {
            foreach (var i in File.ReadAllLines(Trabalho_Cadona.Program.PathBanco))
            {
                Linha linha = new Linha(i);
                inicio.Linhas.Add(linha);
            }
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            Transacao transacaoAtual = new Transacao("Transacao " + Count, DateTime.Now.ToString("yyyyMMddHHmmss"));
            inicio.Transacoes.Add(transacaoAtual);
            Form NovaTransacao = new TransacaoForm(inicio, this, transacaoAtual);
            NovaTransacao.Show();
            Count++;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            File.Delete(Trabalho_Cadona.Program.PathBanco);
            for (int i = 0; i < inicio.Linhas.Count; i++)
            {
                var Linhas = inicio.Linhas.ToArray();

                if (i == 0)
                {
                    using (StreamWriter sw = File.AppendText(Trabalho_Cadona.Program.PathBanco))
                    {
                        sw.Write(Linhas[i].Codigo + "|" + Linhas[i].Descricao);
                    }
                }
                else
                {
                    foreach(var line in inicio.Linhas)
                    {

                    }
                    using (StreamWriter sw = File.AppendText(Trabalho_Cadona.Program.PathBanco))
                    {
                        sw.Write("\r\n" + Linhas[i].Codigo + "|" + Linhas[i].Descricao);
                    }
                }
            }

            using (StreamWriter sw = File.AppendText(Trabalho_Cadona.Program.PathLog))
            {
                sw.Write("\r\n -Checkpoint-");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (var transacao in inicio.Transacoes.OrderByDescending(p => p.CodigoTransacao))
            {
                if (transacao.Bloqueado.Locked)
                {
                    if (VerificaDeadLock(transacao.CodigoTransacao))
                    {
                        transacao.Form.Rollback();
                        break;
                    }
                }
            }
        }

        private bool VerificaDeadLock(string pTransacaoInicial)
        {
            string transacaoPercorrida = null;

            do {
                foreach (var transacao in inicio.Transacoes)
                {
                    transacaoPercorrida = string.IsNullOrEmpty(transacao.Bloqueado.ToString()) ? null : transacao.Bloqueado.AguardandoTransacao;
                    if (transacaoPercorrida == pTransacaoInicial) break;
                }
            }while ((pTransacaoInicial != transacaoPercorrida) || (transacaoPercorrida == null));

            if (transacaoPercorrida == null)
            {
                return false;
            }

            return true;
        }
    }
}
