using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Seyyedi Added
namespace CY_BM
{
    public class Numerator
    {
        public string Name { get; set; }
        public int NumProperty { get; set; }

        public Numerator(string name,int NumP)
        {
            Name = name;
            NumProperty = NumP;
        }
    }
}
