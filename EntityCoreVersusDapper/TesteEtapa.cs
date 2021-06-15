using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityCoreVersusDapper
{
    public class TesteEtapa
    {
        public TesteEtapa(string descricao)
        {
            Descricao = descricao;
            ResultadosEntityCore = new Dictionary<int, double>();
            ResultadosDapper = new Dictionary<int, double>();
        }
        public string Descricao { get; }
        public Dictionary<int, double> ResultadosEntityCore { get; }
        public Dictionary<int, double> ResultadosDapper { get; }
    }
}
