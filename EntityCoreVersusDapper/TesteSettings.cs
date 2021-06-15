using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityCoreVersusDapper
{
    public class TesteSettings
    {
        public int QuantidadeDeCategorias { get; set; }
        public int QuantidadeDeBlogsPorCategoria { get; set; }
        public int QuantidadeDePrimeirosRegistros { get; set; }
        public int QuantidadeDeTestes { get; set; }

        public static TesteSettings Default => new ()
        {
            QuantidadeDeBlogsPorCategoria = 1000,
            QuantidadeDeCategorias = 10,
            QuantidadeDePrimeirosRegistros = 15,
            QuantidadeDeTestes = 6
        };
    }
}
