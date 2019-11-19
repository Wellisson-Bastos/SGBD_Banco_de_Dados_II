using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Trabalho_Cadona.Entities;

namespace Trabalho_Cadona.Forms
{
    public partial class TransacaoForm : Form
    {
        #region Variáveis

        InicioClass Inicio;
        Inicio InicioForm;
        Transacao TransacaoAtual;
        List<Linha> LinhasTransacao = new List<Linha>();

        #endregion

        public TransacaoForm(InicioClass lInicio, Inicio inicioform, Transacao transacao)
        {
            InitializeComponent();
            Inicio = lInicio;
            TransacaoAtual = transacao;
            InicioForm = inicioform;
            CarregarLinhas();
            CarregarListView();
            TransacaoAtual.Form = this;

            using (StreamWriter sw = File.AppendText(Trabalho_Cadona.Program.PathLog))
            {
                sw.Write("\r\n" + transacao.CodigoTransacao + "|Iniciou");
            }
        }

        #region Popular Componentes e Variáveis

        private void TransacaoForm_Load(object sender, EventArgs e)
        {
            this.Text = TransacaoAtual.NomeTransacao;
            this.label2.Text = TransacaoAtual.NomeTransacao;
            this.label5.Text = "Código: " + TransacaoAtual.CodigoTransacao;
        }

        private void CarregarListView()
        {
            listView3.Items.Clear();
            foreach (var i in LinhasTransacao)
            {
                Linha linha = new Linha(i);
                ListViewItem Item = new ListViewItem();
                Item.Text = linha.Codigo;
                listView3.Items.Add(Item);
                Item.SubItems.Add(linha.Descricao);
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
            foreach (var i in Inicio.Linhas)
            {
                Linha linha = new Linha() { Codigo = i.Codigo, Descricao = i.Descricao };
                LinhasTransacao.Add(linha);
            }
        }

        #endregion

        #region Funções

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Digite um código para buscar um registro!");
            }
            else
            {
                bool RegistroEncontrado = false;
                string Codigo = textBox1.Text.Trim(' ');

                foreach (var line in LinhasTransacao)
                {
                    if (line.Codigo == Codigo)
                    {
                        RegistroEncontrado = true;
                        break;
                    }
                }

                if (RegistroEncontrado)
                {
                    var block = new Bloqueio(TipoBloqueio.Compartilhado, TransacaoAtual.CodigoTransacao);

                    foreach (var lineTransaction in LinhasTransacao)
                    {
                        if (lineTransaction.Codigo == textBox1.Text.Trim(' ') && Inicio.Linhas.Where(p => p.Codigo == lineTransaction.Codigo).Count() > 0)
                        {
                            lineTransaction.Bloqueios.Add(block);
                            textBox2.Text = lineTransaction.Descricao;
                            CarregarListView();

                            break;
                        }

                        if (lineTransaction.Codigo == textBox1.Text.Trim(' ') && Inicio.Linhas.Where(p => p.Codigo == lineTransaction.Codigo).Count() == 0)
                        {
                            textBox2.Text = lineTransaction.Descricao;
                            CarregarListView();

                            break;
                        }
                    }

                    foreach (var line in Inicio.Linhas)
                    {
                        if (line.Codigo == Codigo)
                        {
                            line.Bloqueios.Add(block);
                            InicioForm.CarregarListView();

                            break;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Não há registros com o código informado!");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text) || (string.IsNullOrEmpty(textBox2.Text)))
            {
                MessageBox.Show("Entre com todos os dados para inserir um registro!");
            }
            else
            {
                bool RegistroEncontrado = false;
                string Codigo = textBox1.Text.Trim(' ');
                string Descricao = textBox2.Text;

                foreach (var line in LinhasTransacao)
                {
                    if (line.Codigo == Codigo)
                    {
                        RegistroEncontrado = true;
                        MessageBox.Show("Já existe um registro com o mesmo código. Escolha outro!");
                        break;
                    }
                }

                if (!RegistroEncontrado)
                {
                    Linha newline = new Linha(Codigo + "|" + Descricao);

                    LinhasTransacao.Add(newline);
                    CarregarListView();

                    using (StreamWriter sw = File.AppendText(Trabalho_Cadona.Program.PathLog))
                    {
                        sw.Write("\r\n" + TransacaoAtual.CodigoTransacao + "|Inseriu|" + Codigo + "|" + Descricao);
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text) || (string.IsNullOrEmpty(textBox2.Text)))
            {
                MessageBox.Show("Entre com todos os dados para alterar um registro!");
            }
            else
            {
                bool RegistroEncontrado = false;
                string Codigo = textBox1.Text.Trim(' ');
                string Descricao = textBox2.Text;

                foreach (var line in LinhasTransacao)
                {
                    if (line.Codigo == Codigo)
                    {
                        RegistroEncontrado = true;
                        break;
                    }
                }

                if (!RegistroEncontrado)
                {
                    MessageBox.Show("Não há registros com o código informado!");
                }
                else
                {
                    bool BloqueioCompartilhado = false;
                    bool BloqueioExclusivo = false;
                    bool PodeEditar = true;
                    string TransacaoBloqueadora = null;

                    foreach (var line in Inicio.Linhas)
                    {
                        if (line.Codigo == Codigo)
                        {
                            bool PossuiBloqueio = line.Bloqueios == null ? false : true;

                            if (PossuiBloqueio)
                            {
                                foreach (var i in line.Bloqueios)
                                {
                                    if (i.CodigoTransacao != TransacaoAtual.CodigoTransacao && i.Tipo == TipoBloqueio.Exclusivo)
                                    {
                                        BloqueioExclusivo = true;
                                        PodeEditar = false;
                                        TransacaoBloqueadora = i.CodigoTransacao;
                                        break;
                                    }

                                    if (i.CodigoTransacao != TransacaoAtual.CodigoTransacao && i.Tipo == TipoBloqueio.Compartilhado)
                                    {
                                        BloqueioCompartilhado = true;
                                        PodeEditar = false;
                                        break;
                                    }
                                }

                                break;
                            }
                        }
                    }

                    if (!PodeEditar)
                    {
                        if (BloqueioCompartilhado && !BloqueioExclusivo)
                        {
                            MessageBox.Show("Outra transação compartilha esses dados. Não é possível alterar no momento!");
                        }
                        else if (!BloqueioCompartilhado && BloqueioExclusivo)
                        {
                            DesabilitarComponentesAlter();
                            MessageBox.Show("Registro bloqueado por outra transação! \nClique no botão 'Verificar Bloqueio' para verificar se foi desbloqueado");
                            TransacaoAtual.Bloqueado.Locked = true;
                            TransacaoAtual.Bloqueado.AguardandoTransacao = TransacaoBloqueadora;
                        }
                        else
                        {
                            DesabilitarComponentesAlter();
                            MessageBox.Show("Registro bloqueado por outra transação! \nClique no botão 'Verificar Bloqueio' para verificar se foi desbloqueado");
                            TransacaoAtual.Bloqueado.Locked = true;
                            TransacaoAtual.Bloqueado.AguardandoTransacao = TransacaoBloqueadora;
                        }
                    }
                    else
                    {
                        var block = new Bloqueio(TipoBloqueio.Exclusivo, TransacaoAtual.CodigoTransacao);

                        foreach (var lineTransaction in LinhasTransacao)
                        {
                            if (lineTransaction.Codigo == textBox1.Text.Trim(' ') && Inicio.Linhas.Where(p => p.Codigo == lineTransaction.Codigo).Count() > 0)
                            {
                                using (StreamWriter sw = File.AppendText(Trabalho_Cadona.Program.PathLog))
                                {
                                    sw.Write("\r\n" + TransacaoAtual.CodigoTransacao + "|Alterou| " + Codigo + "|" + lineTransaction.Descricao + " para " + Codigo + "|" + Descricao);
                                }

                                if (lineTransaction.Bloqueios != null)
                                    lineTransaction.Bloqueios.Clear();
                                lineTransaction.Bloqueios.Add(block);
                                lineTransaction.Descricao = textBox2.Text;
                                CarregarListView();


                                break;
                            }

                            if (lineTransaction.Codigo == textBox1.Text.Trim(' ') && Inicio.Linhas.Where(p => p.Codigo == lineTransaction.Codigo).Count() == 0)
                            {
                                lineTransaction.Descricao = textBox2.Text;
                                CarregarListView();

                                break;
                            }
                        }

                        foreach (var line in Inicio.Linhas)
                        {
                            if (line.Codigo == Codigo)
                            {
                                if (line.Bloqueios != null)
                                    line.Bloqueios.Clear();
                                line.Bloqueios.Add(block);
                                InicioForm.CarregarListView();

                                break;
                            }
                        }
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Entre com o código do registro que deve ser deletado!");
            }
            else
            {
                bool RegistroEncontrado = false;
                string Codigo = textBox1.Text.Trim(' ');

                foreach (var line in LinhasTransacao)
                {
                    if (line.Codigo == Codigo)
                    {
                        RegistroEncontrado = true;
                        break;
                    }
                }

                if (!RegistroEncontrado)
                {
                    MessageBox.Show("Não há registros com o código informado!");
                }
                else
                {
                    bool BloqueioCompartilhado = false;
                    bool BloqueioExclusivo = false;
                    bool PodeEditar = true;
                    string TransacaoBloqueadora = null;

                    foreach (var line in Inicio.Linhas)
                    {
                        if (line.Codigo == Codigo)
                        {
                            bool PossuiBloqueio = line.Bloqueios == null ? false : true;

                            if (PossuiBloqueio)
                            {
                                foreach (var i in line.Bloqueios)
                                {
                                    if (i.CodigoTransacao != TransacaoAtual.CodigoTransacao && i.Tipo == TipoBloqueio.Exclusivo)
                                    {
                                        TransacaoBloqueadora = i.CodigoTransacao;
                                        BloqueioExclusivo = true;
                                        PodeEditar = false;
                                        break;
                                    }

                                    if (i.CodigoTransacao != TransacaoAtual.CodigoTransacao && i.Tipo == TipoBloqueio.Compartilhado)
                                    {
                                        BloqueioCompartilhado = true;
                                        PodeEditar = false;
                                        break;
                                    }
                                }

                                break;
                            }
                        }
                    }

                    if (!PodeEditar)
                    {
                        if (BloqueioCompartilhado && !BloqueioExclusivo)
                        {
                            MessageBox.Show("Outra transação compartilha esses dados. Não é possível deletar no momento!");
                        }
                        else if (!BloqueioCompartilhado && BloqueioExclusivo)
                        {
                            DesabilitarComponentesDelete();
                            MessageBox.Show("Registro bloqueado por outra transação! \nClique no botão 'Verificar Bloqueio' para verificar se foi desbloqueado");
                            TransacaoAtual.Bloqueado.Locked = true;
                            TransacaoAtual.Bloqueado.AguardandoTransacao = TransacaoBloqueadora;
                        }
                        else
                        {
                            DesabilitarComponentesDelete();
                            MessageBox.Show("Registro bloqueado por outra transação! \nClique no botão 'Verificar Bloqueio' para verificar se foi desbloqueado");
                            TransacaoAtual.Bloqueado.Locked = true;
                            TransacaoAtual.Bloqueado.AguardandoTransacao = TransacaoBloqueadora;
                        }
                    }
                    else
                    {
                        var block = new Bloqueio(TipoBloqueio.Exclusivo, TransacaoAtual.CodigoTransacao);

                        foreach (var lineTransaction in LinhasTransacao)
                        {
                            if (lineTransaction.Codigo == Codigo)
                            {
                                using (StreamWriter sw = File.AppendText(Trabalho_Cadona.Program.PathLog))
                                {
                                    sw.Write("\r\n" + TransacaoAtual.CodigoTransacao + "|Deletou|" + Codigo + "|" + lineTransaction.Descricao);
                                }

                                LinhasTransacao.Remove(lineTransaction);
                                textBox2.Text = null;
                                CarregarListView();

                                break;
                            }
                        }

                        foreach (var line in Inicio.Linhas)
                        {
                            if (line.Codigo == Codigo)
                            {
                                if (line.Bloqueios != null)
                                    line.Bloqueios.Clear();
                                line.Bloqueios.Add(block);
                                InicioForm.CarregarListView();

                                break;
                            }
                        }
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            List<Bloqueio> BloqueiosRemocao = new List<Bloqueio>();
            List<Linha> RegistrosInsercao = new List<Linha>();
            List<Linha> RegistrosDelete = new List<Linha>();

            foreach (var linha in Inicio.Linhas)
            {
                foreach (var line in LinhasTransacao)
                {
                    bool PossuiBloqueio = string.IsNullOrEmpty(linha.Bloqueios.ToString()) ? false : true;

                    if (linha.Codigo == line.Codigo && linha.Descricao != line.Descricao)
                    {
                        linha.Descricao = line.Descricao;

                        if (PossuiBloqueio)
                        {
                            foreach (var i in linha.Bloqueios)
                            {
                                if (i.CodigoTransacao == TransacaoAtual.CodigoTransacao)
                                {
                                    BloqueiosRemocao.Add(i);
                                }
                            }

                            foreach (var i in BloqueiosRemocao)
                            {
                                linha.Bloqueios.Remove(i);
                            }
                        }
                    }
                }
            }

            foreach (var line in LinhasTransacao)
            {
                if (Inicio.Linhas.Where(p => p.Codigo == line.Codigo).Count() == 0)
                {
                    Inicio.Linhas.Add(line);
                }
            }

            foreach (var line in Inicio.Linhas)
            {
                if (LinhasTransacao.Where(p => p.Codigo == line.Codigo).Count() == 0)
                {
                    RegistrosDelete.Add(line);
                }
            }

            foreach (var line in RegistrosDelete)
            {
                Inicio.Linhas.Remove(line);
            }

            using (StreamWriter sw = File.AppendText(Trabalho_Cadona.Program.PathLog))
            {
                sw.Write("\r\n" + TransacaoAtual.CodigoTransacao + "|Commit");
            }

            InicioForm.CarregarListView();
            this.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            List<Bloqueio> BloqueiosRemocao = new List<Bloqueio>();

            foreach (var linha in Inicio.Linhas)
            {
                foreach (var line in LinhasTransacao)
                {
                    bool PossuiBloqueio = string.IsNullOrEmpty(linha.Bloqueios.ToString()) ? false : true;

                    if (linha.Codigo == line.Codigo)
                    {
                        if (PossuiBloqueio)
                        {
                            foreach (var i in linha.Bloqueios)
                            {
                                if (i.CodigoTransacao == TransacaoAtual.CodigoTransacao)
                                {
                                    BloqueiosRemocao.Add(i);
                                }
                            }

                            foreach (var i in BloqueiosRemocao)
                            {
                                linha.Bloqueios.Remove(i);
                            }
                        }
                    }
                }
            }

            using (StreamWriter sw = File.AppendText(Trabalho_Cadona.Program.PathLog))
            {
                sw.Write("\r\n" + TransacaoAtual.CodigoTransacao + "|Rollback");
            }

            InicioForm.CarregarListView();
            this.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string Codigo = textBox1.Text.Trim(' ');
            string Descricao = textBox2.Text;
            bool PodeEditar = true;

            foreach (var line in Inicio.Linhas)
            {
                bool PossuiBloqueio = string.IsNullOrEmpty(line.Bloqueios.ToString()) ? false : true;

                if (line.Codigo == Codigo && PossuiBloqueio)
                {
                    if (line.Bloqueios.Count > 1)
                    {
                        PodeEditar = false;
                        break;
                    }

                    foreach (var i in line.Bloqueios)
                    {
                        if (i.CodigoTransacao != TransacaoAtual.CodigoTransacao && i.Tipo == TipoBloqueio.Exclusivo)
                        {
                            PodeEditar = false;
                            break;
                        }
                    }

                    break;
                }
            }

            if (PodeEditar)
            {
                foreach (var line in Inicio.Linhas)
                {
                    if (line.Codigo == Codigo)
                    {
                        if (line.Bloqueios != null)
                            line.Bloqueios.Clear();
                        var block = new Bloqueio(TipoBloqueio.Exclusivo, TransacaoAtual.CodigoTransacao);
                        line.Bloqueios.Add(block);
                        InicioForm.CarregarListView();

                        foreach (var lineTransaction in LinhasTransacao)
                        {
                            if (lineTransaction.Codigo == textBox1.Text.Trim(' '))
                            {
                                using (StreamWriter sw = File.AppendText(Trabalho_Cadona.Program.PathLog))
                                {
                                    sw.Write("\r\n" + TransacaoAtual.CodigoTransacao + "|Alterou| " + Codigo + "|" + lineTransaction.Descricao + " para " + Codigo + "|" + Descricao);
                                }

                                MessageBox.Show("Alterado com sucesso!");

                                if (lineTransaction.Bloqueios != null)
                                    lineTransaction.Bloqueios.Clear();
                                lineTransaction.Bloqueios.Add(block);
                                lineTransaction.Descricao = textBox2.Text;
                                CarregarListView();

                                break;
                            }
                        }

                        HabilitarComponentesAlter();
                        TransacaoAtual.Bloqueado.Locked = false;
                        TransacaoAtual.Bloqueado.AguardandoTransacao = null;

                        break;
                    }
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string Codigo = textBox1.Text.Trim(' ');
            string Descricao = textBox2.Text;
            bool PodeEditar = true;

            foreach (var line in Inicio.Linhas)
            {
                if (Inicio.Linhas.Where(p => p.Codigo == Codigo).Count() > 0)
                {
                    bool PossuiBloqueio = string.IsNullOrEmpty(line.Bloqueios.ToString()) ? false : true;

                    if (line.Codigo == Codigo && PossuiBloqueio)
                    {
                        if (line.Bloqueios.Count > 1)
                        {
                            PodeEditar = false;
                            break;
                        }

                        foreach (var i in line.Bloqueios)
                        {
                            if (i.CodigoTransacao != TransacaoAtual.CodigoTransacao && i.Tipo == TipoBloqueio.Exclusivo)
                            {
                                PodeEditar = false;
                                break;
                            }
                        }

                        break;
                    }
                }
                else
                {
                    PodeEditar = false;
                }
            }

            if (PodeEditar)
            {
                foreach (var line in Inicio.Linhas)
                {
                    if (line.Codigo == Codigo)
                    {
                        if (line.Bloqueios != null)
                            line.Bloqueios.Clear();
                        var block = new Bloqueio(TipoBloqueio.Exclusivo, TransacaoAtual.CodigoTransacao);
                        line.Bloqueios.Add(block);
                        InicioForm.CarregarListView();

                        foreach (var lineTransaction in LinhasTransacao)
                        {
                            if (lineTransaction.Codigo == textBox1.Text.Trim(' '))
                            {
                                using (StreamWriter sw = File.AppendText(Trabalho_Cadona.Program.PathLog))
                                {
                                    sw.Write("\r\n" + TransacaoAtual.CodigoTransacao + "|Deletou| " + Codigo + "|" + lineTransaction.Descricao);
                                }

                                MessageBox.Show("Deletado com sucesso!");

                                LinhasTransacao.Remove(lineTransaction);
                                CarregarListView();

                                break;
                            }
                        }

                        HabilitarComponentesDelete();
                        TransacaoAtual.Bloqueado.Locked = false;
                        TransacaoAtual.Bloqueado.AguardandoTransacao = null;

                        break;
                    }
                }
            }
            else
            {
                foreach (var lineTransaction in LinhasTransacao)
                {
                    if (lineTransaction.Codigo == textBox1.Text.Trim(' '))
                    {
                        using (StreamWriter sw = File.AppendText(Trabalho_Cadona.Program.PathLog))
                        {
                            sw.Write("\r\n" + TransacaoAtual.CodigoTransacao + "|Deletou| " + Codigo + "|" + lineTransaction.Descricao);
                        }

                        MessageBox.Show("Deletado com sucesso!");

                        LinhasTransacao.Remove(lineTransaction);
                        CarregarListView();

                        break;
                    }
                }

                HabilitarComponentesDelete();
                TransacaoAtual.Bloqueado.Locked = false;
                TransacaoAtual.Bloqueado.AguardandoTransacao = null;
            }
        }

        #endregion

        #region Auxiliares

        private void DesabilitarComponentesAlter()
        {
            this.UseWaitCursor = true;
            button7.Visible = true;
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
        }

        private void HabilitarComponentesAlter()
        {
            this.UseWaitCursor = false;
            button7.Visible = false;
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
        }

        private void DesabilitarComponentesDelete()
        {
            this.UseWaitCursor = true;
            button8.Visible = true;
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
        }

        private void HabilitarComponentesDelete()
        {
            this.UseWaitCursor = false;
            button8.Visible = false;
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
        }

        public void Rollback()
        {
            List<Bloqueio> BloqueiosRemocao = new List<Bloqueio>();

            foreach (var linha in Inicio.Linhas)
            {
                foreach (var line in LinhasTransacao)
                {
                    bool PossuiBloqueio = string.IsNullOrEmpty(linha.Bloqueios.ToString()) ? false : true;

                    if (linha.Codigo == line.Codigo)
                    {
                        if (PossuiBloqueio)
                        {
                            foreach (var i in linha.Bloqueios)
                            {
                                if (i.CodigoTransacao == TransacaoAtual.CodigoTransacao)
                                {
                                    BloqueiosRemocao.Add(i);
                                }
                            }

                            foreach (var i in BloqueiosRemocao)
                            {
                                linha.Bloqueios.Remove(i);
                            }
                        }
                    }
                }
            }

            using (StreamWriter sw = File.AppendText(Trabalho_Cadona.Program.PathLog))
            {
                sw.Write("\r\n" + TransacaoAtual.CodigoTransacao + "|Rollback");
            }

            InicioForm.CarregarListView();
            this.Close();
        }

        #endregion

        private void TransacaoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            List<Bloqueio> BloqueiosRemocao = new List<Bloqueio>();

            foreach (var linha in Inicio.Linhas)
            {
                foreach (var line in LinhasTransacao)
                {
                    bool PossuiBloqueio = string.IsNullOrEmpty(linha.Bloqueios.ToString()) ? false : true;

                    if (linha.Codigo == line.Codigo)
                    {
                        if (PossuiBloqueio)
                        {
                            foreach (var i in linha.Bloqueios)
                            {
                                if (i.CodigoTransacao == TransacaoAtual.CodigoTransacao)
                                {
                                    BloqueiosRemocao.Add(i);
                                }
                            }

                            foreach (var i in BloqueiosRemocao)
                            {
                                linha.Bloqueios.Remove(i);
                            }
                        }
                    }
                }
            }

            using (StreamWriter sw = File.AppendText(Trabalho_Cadona.Program.PathLog))
            {
                sw.Write("\r\n" + TransacaoAtual.CodigoTransacao + "|Rollback");
            }

            InicioForm.CarregarListView();
        }
    }
}

