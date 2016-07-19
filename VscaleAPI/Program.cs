using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VscaleAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            var v = new Vscale("5b6d89f6fa91cf39bb73eb87da022ca31edc407a12d6bb1ce095365a4a49b12e");
            Console.WriteLine(v.DomainsAdd("aduciicba.com", ""));
            Console.ReadLine();
        }
    }
}
