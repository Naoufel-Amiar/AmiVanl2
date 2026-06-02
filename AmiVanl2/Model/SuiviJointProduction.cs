namespace AmiVanl2.Model
{
    public class JointProduction
    {
        public string Reference { get; set; } = "";
        public string AncienCode { get; set; } = "";
        public string Equipe { get; set; } = "";

        public double ObjectifSemaine { get; set; }
        public double ObjectifEquipe { get; set; }

        public double LundiEqu1 { get; set; }
        public double LundiEqu2 { get; set; }
        public double LundiEqu3 { get; set; }

        public double MardiEqu1 { get; set; }
        public double MardiEqu2 { get; set; }
        public double MardiEqu3 { get; set; }

        public double MercrediEqu1 { get; set; }
        public double MercrediEqu2 { get; set; }
        public double MercrediEqu3 { get; set; }

        public double JeudiEqu1 { get; set; }
        public double JeudiEqu2 { get; set; }
        public double JeudiEqu3 { get; set; }

        public double VendrediEqu1 { get; set; }
        public double VendrediEqu2 { get; set; }
        public double VendrediEqu3 { get; set; }

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

        public double TotalLundi =>
            LundiEqu1 + LundiEqu2 + LundiEqu3;

        public double TotalMardi =>
            MardiEqu1 + MardiEqu2 + MardiEqu3;

        public double TotalMercredi =>
            MercrediEqu1 + MercrediEqu2 + MercrediEqu3;

        public double TotalJeudi =>
            JeudiEqu1 + JeudiEqu2 + JeudiEqu3;

        public double TotalVendredi =>
            VendrediEqu1 + VendrediEqu2 + VendrediEqu3;

        public double TotalProduction =>
            TotalLundi
            + TotalMardi
            + TotalMercredi
            + TotalJeudi
            + TotalVendredi
            + ProdSamedi
            + ProdDimanche;

    }
}