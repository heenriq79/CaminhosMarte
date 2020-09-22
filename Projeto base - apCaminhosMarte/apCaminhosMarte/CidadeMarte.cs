using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apCaminhosMarte
{
    class CidadeMarte : IComparable<CidadeMarte>
    {
        int id;
        string nome;
        int coordX;
        int coordY;

        public int Id { get => id; set => id = value; }
        public string Nome { get => nome; set => nome = value; }
        public int CoordX { get => coordX; set => coordX = value; }
        public int CoordY { get => coordY; set => coordY = value; }

        public CidadeMarte(int id, string nome, int coordX, int coordY)
        {
            Id = id;
            Nome = nome;
            CoordX = coordX;
            CoordY = coordY;
        }

        public int CompareTo(CidadeMarte other)
        {
            return Id.CompareTo(other.Id);
        }
    }
}
