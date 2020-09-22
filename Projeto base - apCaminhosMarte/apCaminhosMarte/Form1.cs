using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace apCaminhosMarte
{
    public partial class Form1 : Form
    {
        Arvore<CidadeMarte> arvoreCidades;
        PilhaVetor<CaminhoMarte> pilhaCaminhosPossiveis = new PilhaVetor<CaminhoMarte>();
        int[,] matrizCaminhos;
        int idOrigem;
        int idDestino;

        List<int> descartados = new List<int>(); // vetor que guarda os caminhos ja descartados 
        List<int> pontesJaUsadas = new List<int>();
 
        static int tamanhoArray = 15;
        int[,] matrizCaminhosAchados = new int[tamanhoArray, tamanhoArray];
        int caminhosAchados = 0;

        int menorDistancia = int.MaxValue;
        int atualDistancia;
        PilhaVetor<CaminhoMarte> pilhaMenorCaminho;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            arvoreCidades = new Arvore<CidadeMarte>();
            matrizCaminhos = new int[23, 23];

            dgvCaminhosEncontrados.RowCount = 0;



            //arquivo cidades
            try
            {
                // vai ler o arquivo e armazenar em uma árvore 
                StreamReader arquivo;

                arquivo = new StreamReader(@"C:\Temp\CidadesMarte.txt", Encoding.Default);
                while (!arquivo.EndOfStream)
                {
                    string linha = arquivo.ReadLine();
                    arvoreCidades.Incluir(new CidadeMarte(int.Parse(linha.Substring(1, 2)),      // id
                                                            linha.Substring(3, 16),              // nome
                                                            int.Parse(linha.Substring(19, 4)),   // coordx
                                                            int.Parse(linha.Substring(24, 4)))); // coordy
                }
            }
            catch
            { //caso não consiga achar o arquivo ele da um openfile
                StreamReader arquivo;
                MessageBox.Show("Selecione o arquivo das cidades (CidadesMarte.txt)");
                if (dlgAbrir.ShowDialog() == DialogResult.OK)
                    arquivo = new StreamReader(dlgAbrir.FileName);
                else
                    arquivo = new StreamReader(@"C:\Temp\CidadesMarte.txt", Encoding.Default);

                while (!arquivo.EndOfStream)
                {
                    string linha = arquivo.ReadLine();
                    arvoreCidades.Incluir(new CidadeMarte(int.Parse(linha.Substring(1, 2)),      // id
                                                            linha.Substring(3, 16),              // nome
                                                            int.Parse(linha.Substring(19, 4)),   // coordx
                                                            int.Parse(linha.Substring(24, 4)))); // coordy
                }
            }


            // arquivo caminhos
            try
            {
                StreamReader arquivo;
                arquivo = new StreamReader(@"C:\Temp\CaminhosEntreCidadesMarte.txt", Encoding.Default);
                while (!arquivo.EndOfStream)
                {
                    string linha = arquivo.ReadLine();
                    matrizCaminhos[int.Parse(linha.Substring(1, 2)), int.Parse(linha.Substring(4, 2))] = int.Parse(linha.Substring(7, 4));
                    // id origem             // id destino                    // distancia
                }
            }
            catch
            {
                StreamReader arquivo;
                MessageBox.Show("Selecione o arquivo dos caminhos (CaminhosEntreCidadesMarte.txt)");
                if (dlgAbrir.ShowDialog() == DialogResult.OK)
                    arquivo = new StreamReader(dlgAbrir.FileName);
                else
                    arquivo = new StreamReader(@"C:\Temp\CaminhosEntreCidadesMarte.txt", Encoding.Default);

                while (!arquivo.EndOfStream)
                {
                    string linha = arquivo.ReadLine();
                    matrizCaminhos[int.Parse(linha.Substring(1, 2)), int.Parse(linha.Substring(4, 2))] = int.Parse(linha.Substring(7, 4));
                    // id origem             // id destino                    // distancia
                }
            }

            pnlArvore.Invalidate();
        }


        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            Resetar(); //metodo que reseta as variáveis e pilhas, para achar novas rotas posteriormente
            idOrigem = lsbOrigem.SelectedIndex;
            idDestino = lsbDestino.SelectedIndex;


            if (idOrigem == -1 || idDestino == -1)  // não selecionou um destino ou origem
            {
                MessageBox.Show("Selecione uma origem e um destino");
                return;
            }
            if (idOrigem == idDestino)  //selecionou a origem eo destino iguais
            {
                MessageBox.Show("Origem e destino iguais");
                return;
            }

            lsbOrigem.Items.Clear();
            lsbDestino.Items.Clear();

            BuscarCaminho(idOrigem);  // método de buscar o caminho
            AdicionarNoGridView();

            if (pilhaMenorCaminho != null)
                DesenhaMelhorCaminho();
        }


        private void DesenhaMelhorCaminho()
        {
            lblMenorCaminho.Text = "Menor caminho: " + menorDistancia + " km";
            pbMapa.Invalidate();
        }


        private void BuscarMenorCaminho(PilhaVetor<CaminhoMarte> pilha) //metodo que valida se o caminho é o menor
        {
            PilhaVetor<CaminhoMarte> ret = pilha.Copia();
            for (int i = 0; !pilha.EstaVazia(); i++)
            {
                CaminhoMarte aux = pilha.Desempilhar();
                atualDistancia += matrizCaminhos[aux.IdCidadeOrigem, aux.IdCidadeDestino]; //calcula a distancia utilizando a matriz adjacente
            }
            
            if (atualDistancia < menorDistancia) //se o novo caminho tem uma distancia menor que o último, atualiza
            {
                menorDistancia = atualDistancia;
                pilhaMenorCaminho = ret; //guarda o caminho menor
            }
            
        }


        private void BuscarCaminho(int origem) //metodo recursivo para buscar o caminho
        {
            for (int dest = 0; dest < 23; dest++) //irá percorrer a matriz adjacente para encontrar os destinos que a origem passada como parâmetro tem
                if (matrizCaminhos[origem, dest] != 0 && !Contem(dest)) //metodo contém para seber se já passamos por uma cidade que não leva ao destino desejado
                {
                    while (!pilhaCaminhosPossiveis.EstaVazia() && pilhaCaminhosPossiveis.OTopo().IdCidadeDestino != origem) //enquanto o último destino que eu fui não é onde eu estou (deve ser onde eu estou)
                        pilhaCaminhosPossiveis.Desempilhar(); //desempilha (descarta esse caminho)
                    pilhaCaminhosPossiveis.Empilhar(new CaminhoMarte(origem, dest, matrizCaminhos[origem, dest])); //se eu avanço, empilho um novo caminho

                    if (dest == idDestino && !PonteJaUsada(origem)) //chego no meu destino
                    {
                        caminhosAchados++;
                        BuscarMenorCaminho(pilhaCaminhosPossiveis.Copia()); // busca menor caminho

                        //pilha temporaria para poder desempilha-la
                        PilhaVetor<CaminhoMarte> temp = pilhaCaminhosPossiveis.Copia().Inverte(); //para escrever no gridview dos camnhos inverte para facilitar

                        //escreve em uma matriz os caminhos achados para facilitar na inserção do gridview posteriormente
                        matrizCaminhosAchados[caminhosAchados, 1] = idOrigem;
                        int i = 2;
                        while (!temp.EstaVazia())
                        {
                            matrizCaminhosAchados[caminhosAchados, i] = temp.Desempilhar().IdCidadeDestino;  // começa no [1,1]
                            i++;
                        }


                        pontesJaUsadas.Add(pilhaCaminhosPossiveis.OTopo().IdCidadeOrigem);
                    }
                    else
                    {
                        BuscarCaminho(dest);
                    }
                }

        }


    
        private bool Contem(int valor) //função que checa se ja foi usado um caminho
        {
            for (int i = 0; i < descartados.Count; i++)
                if (descartados[i] == valor)
                    return true;
            return false;
        }

        private bool PonteJaUsada(int ori) //função que checa se ja foi usada uma ponte
        {
            for (int i = 0; i < pontesJaUsadas.Count; i++)
                if (pontesJaUsadas[i] == ori)
                    return true;
            return false;
        }

        private void AdicionarNoGridView() //metodo que percorre a matriz dos caminhos e passa para o gridview
        {
            int tamanhoColunas = 0;
            int tamanhoLinhas = 0;

            if (idOrigem == 0)   // caso o origem fosse 0, ele nao aumentaria nas colunas abaixo
                tamanhoColunas++;

            //calcula quantas colunas e linhas terão
            for (int i = 1; i < tamanhoArray; i++)
                for (int j = 1; j < tamanhoArray; j++)
                {
                    if (matrizCaminhosAchados[i, j] != 0)
                    {
                        if (tamanhoColunas < j)
                            tamanhoColunas++;
                        if (tamanhoLinhas < i)
                            tamanhoLinhas++;
                    }
                }


            if (tamanhoLinhas == 0) //se não existe dados no gridview é porque rotas não foram encontradas
            {
                MessageBox.Show("Não existem caminhos entre esses destinos");
                Resetar();
                return;
            }

            List<CidadeMarte> listaCidades =  arvoreCidades.ParaLista();

            //escreve os dados no gridview 
            dgvCaminhosEncontrados.RowCount = tamanhoLinhas;
            dgvCaminhosEncontrados.ColumnCount = tamanhoColunas;
            for (int lin = 0; lin < tamanhoLinhas; lin++)
                for (int col = 0; col < tamanhoColunas; col++)
                    dgvCaminhosEncontrados.Rows[lin].Cells[col].Value = matrizCaminhosAchados[lin +1, col+1]+" - "+ listaCidades.ElementAt(matrizCaminhosAchados[lin + 1, col + 1]).Nome;

            int copiaTamanhoColunas = tamanhoColunas;
            for (int lin = 1; tamanhoColunas > 1 && lin <= tamanhoLinhas; lin++)
            {
                while (matrizCaminhosAchados[lin, tamanhoColunas] == 0)
                {
                    dgvCaminhosEncontrados.Rows[lin - 1].Cells[tamanhoColunas - 1].Value = "";
                    tamanhoColunas--;
                }
                tamanhoColunas = copiaTamanhoColunas;
            }

            dgvMelhorCaminho.ColumnCount = pilhaMenorCaminho.Tamanho() + 1;
            dgvMelhorCaminho.RowCount = 1;

            PilhaVetor<CaminhoMarte> pilhaParaMostrar = pilhaMenorCaminho.Copia().Inverte();
            dgvMelhorCaminho.Rows[0].Cells[0].Value =  idOrigem + " - " + listaCidades.ElementAt(idOrigem).Nome;
            for (int i = 1; !pilhaParaMostrar.EstaVazia(); i++)
                dgvMelhorCaminho.Rows[0].Cells[i].Value = pilhaParaMostrar.OTopo().IdCidadeDestino + " - " + listaCidades.ElementAt(pilhaParaMostrar.Desempilhar().IdCidadeDestino).Nome;
        }

        private void Resetar() //metodo que reseta as pilhas e variáveis
        {
            dgvCaminhosEncontrados.Rows.Clear();
            dgvCaminhosEncontrados.Columns.Clear();
            dgvMelhorCaminho.Rows.Clear();
            dgvMelhorCaminho.Columns.Clear();

            descartados.Clear();
            pontesJaUsadas.Clear();

            caminhosAchados = 0;

            for (int i = 0; i < tamanhoArray; i++)
                for (int j = 0; j < tamanhoArray; j++)
                    matrizCaminhosAchados[i, j] = 0;


            pbMapa.Refresh();
            pilhaMenorCaminho = null;
            pnlArvore.Invalidate();
            menorDistancia = int.MaxValue;
            atualDistancia = 0;
        }


        // DESENHA ARVORE NA ABA 2
        private void desenhaArvore(bool primeiraVez, NoArvore<CidadeMarte> raiz,
                   int x, int y, double angulo, double incremento,
                   double comprimento, Graphics g)
        {
            int xf, yf;
            if (raiz != null)
            {
                Pen caneta = new Pen(Color.Red);
                xf = (int)Math.Round(x + Math.Cos(angulo) * comprimento);
                yf = (int)Math.Round(y + Math.Sin(angulo) * comprimento);
                if (primeiraVez)
                    yf = 25;
                g.DrawLine(caneta, x, y, xf, yf);
                // sleep(100);
                desenhaArvore(false, raiz.Esq, xf, yf, Math.PI / 2 + incremento,
                                                 incremento * 0.60, comprimento * 0.8, g);
                desenhaArvore(false, raiz.Dir, xf, yf, Math.PI / 2 - incremento,
                                                  incremento * 0.60, comprimento * 0.8, g);
                // sleep(100);
                SolidBrush preenchimento = new SolidBrush(Color.Blue);
                g.FillEllipse(preenchimento, xf - 15, yf - 15, 30, 30);
                g.DrawString(Convert.ToString(raiz.Info.Id), new Font("Comic Sans", 12),
                              new SolidBrush(Color.Yellow), xf - 15, yf - 10);

                g.DrawString(Convert.ToString(raiz.Info.Nome), new Font("Comic Sans", 8),
                              new SolidBrush(Color.Black), xf - 25, yf - 28);

            }
        }


        private void pnlArvore_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            desenhaArvore(true, arvoreCidades.Raiz, (int)pnlArvore.Width / 2, 0, Math.PI / 2,
                                 Math.PI / 2.5, 300, g);
        }

        // DESENHA MAPA
        private void pbMapa_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            SizeF tamanho = new SizeF(12, 12);
            Point ponto;

            List<CidadeMarte> listaCidades = arvoreCidades.ParaLista();

            for (int i = 0; i < listaCidades.Count; i++)
            {
                ponto = new Point();
                ponto.X = listaCidades.ElementAt(i).CoordX * pbMapa.Width / 4096 - 5;
                ponto.Y = listaCidades.ElementAt(i).CoordY * pbMapa.Height / 2048 - 5;
                RectangleF rect = new RectangleF(ponto, tamanho);
                g.DrawString(listaCidades.ElementAt(i).Nome, new Font(this.Font, FontStyle.Bold), Brushes.Black, ponto.X - 18, ponto.Y + 15);
                g.DrawString(listaCidades.ElementAt(i).Id + "", new Font(this.Font, FontStyle.Bold), Brushes.WhiteSmoke, ponto.X, ponto.Y + 28);


                g.FillEllipse(Brushes.Black, rect);

                lsbOrigem.Items.Add((i) + " - " + listaCidades.ElementAt(i).Nome);
                lsbDestino.Items.Add((i) + " - " + listaCidades.ElementAt(i).Nome);
            }

            if (pilhaMenorCaminho != null)
            {
                var aux = pilhaMenorCaminho.Copia();
                while(!aux.EstaVazia())
                {
                    CaminhoMarte c = aux.Desempilhar();
                    CidadeMarte origem = listaCidades.ElementAt(c.IdCidadeOrigem);
                    CidadeMarte destino = listaCidades.ElementAt(c.IdCidadeDestino);

                    Point ori = new Point();
                    Point dest = new Point();
                    ori.X = origem.CoordX * pbMapa.Width / 4096 - 5;
                    ori.Y = origem.CoordY * pbMapa.Height / 2048 - 5;
                    dest.X = destino.CoordX * pbMapa.Width / 4096 - 5;
                    dest.Y = destino.CoordY * pbMapa.Height / 2048 -5;

                    g.DrawLine(new Pen(Color.Black), ori.X + 5, ori.Y + 5, dest.X + 5, dest.Y + 5);
                    
                    //destaca a origem e o destino em azul 
                    RectangleF rectOri = new RectangleF(ori, tamanho);
                    RectangleF rectDest = new RectangleF(dest, tamanho);
                    g.FillEllipse(Brushes.Blue, rectOri);
                    g.FillEllipse(Brushes.Blue, rectDest);
                }
            }
        }
    }
}
