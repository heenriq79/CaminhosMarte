using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace apCaminhosMarte
{
    class ArvoreDeBusca<Tipo> : IComparable<Tipo>
        where Tipo : IComparable<Tipo>
    {
        public NoArvore<Tipo> raiz, atual, antecessor;

        public Panel painelArvore;

        public Panel OndeExibir
        {
            get { return painelArvore; }
            set { painelArvore = value; }
        }

        public int CompareTo(Tipo other)
        {
            throw new NotImplementedException();
        }
    }
}