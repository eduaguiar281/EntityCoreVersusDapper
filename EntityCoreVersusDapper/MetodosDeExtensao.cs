using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityCoreVersusDapper
{
    public static class MetodosDeExtensao
    {
        public static string ConverterStringDuasCasasDecimais(this double numero)
        {
            return numero.ToString("0.000");
        }
    }
}
