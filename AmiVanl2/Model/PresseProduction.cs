using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmiVanl2.Model
{
    public class PresseProduction
    {
        public int Reference { get; set; }
        public string AncienCode { get; set; } = "";
        public string Equipe { get; set; } = "";
        public string Machine { get; set; } = "";

        public double ObjectifSemaine { get; set; }
        public double ObjectifEquipe { get; set; }

        public double ProdLundi { get; set; }
        public double ProdMardi { get; set; }
        public double ProdMercredi { get; set; }
        public double ProdJeudi { get; set; }
        public double ProdVendredi { get; set; }
        public double ProdSamedi { get; set; }
        public double ProdDimanche { get; set; }

        public string LabelLundi { get; set; } = "";
        public string LabelMardi { get; set; } = "";
        public string LabelMercredi { get; set; } = "";
        public string LabelJeudi { get; set; } = "";
        public string LabelVendredi { get; set; } = "";
        public string LabelSamedi { get; set; } = "";
        public string LabelDimanche { get; set; } = "";

        public string Commentaire { get; set; } = "";

        public double TotalProduction =>
            ProdLundi + ProdMardi + ProdMercredi + ProdJeudi + ProdVendredi + ProdSamedi + ProdDimanche;

        public double EcartObjectif =>
            TotalProduction - ObjectifSemaine;

        public double TauxAtteinte =>
            ObjectifSemaine == 0 ? 0 : TotalProduction / ObjectifSemaine * 100;
    }
}
