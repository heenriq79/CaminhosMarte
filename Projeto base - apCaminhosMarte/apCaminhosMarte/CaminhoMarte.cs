using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apCaminhosMarte //clase dos caminhos
{
    class CaminhoMarte : IComparable<CaminhoMarte>
    {
        int idCidadeOrigem;
        int idCidadeDestino;
        int distancia;

        public int IdCidadeOrigem { get => idCidadeOrigem; set => idCidadeOrigem = value; }
        public int IdCidadeDestino { get => idCidadeDestino; set => idCidadeDestino = value; }
        public int Distancia { get => distancia; set => distancia = value; }

        public CaminhoMarte(int idCidadeOrigem, int idCidadeDestino, int distancia)
        {
            IdCidadeOrigem = idCidadeOrigem;
            IdCidadeDestino = idCidadeDestino;
            Distancia = distancia;
        }

        public int CompareTo(CaminhoMarte other)
        {
            return -1;
        }
    }
}
