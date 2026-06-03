namespace AmiVanl2.Model
{
    public class TriProduction
    {
        public string Reference { get; set; } = "";
        public string AncienCode { get; set; } = "";
        public string Equipe { get; set; } = "";

        public double ObjectifSemaine { get; set; }
        public double ObjectifEquipe { get; set; }

        public double LundiEqu1 { get; set; }
        public double LundiEqu2 { get; set; }
        public double LundiEqu3 { get; set; }
        public double LundiEqu1b { get; set; }
        public double LundiEqu2b { get; set; }
        public double LundiEqu3b { get; set; }

        public double MardiEqu1 { get; set; }
        public double MardiEqu2 { get; set; }
        public double MardiEqu3 { get; set; }
        public double MardiEqu1b { get; set; }
        public double MardiEqu2b { get; set; }
        public double MardiEqu3b { get; set; }

        public double MercrediEqu1 { get; set; }
        public double MercrediEqu2 { get; set; }
        public double MercrediEqu3 { get; set; }
        public double MercrediEqu1b { get; set; }
        public double MercrediEqu2b { get; set; }
        public double MercrediEqu3b { get; set; }

        public double JeudiEqu1 { get; set; }
        public double JeudiEqu2 { get; set; }
        public double JeudiEqu3 { get; set; }
        public double JeudiEqu1b { get; set; }
        public double JeudiEqu2b { get; set; }
        public double JeudiEqu3b { get; set; }

        public double VendrediEqu1 { get; set; }
        public double VendrediEqu2 { get; set; }
        public double VendrediEqu3 { get; set; }
        public double VendrediEqu1b { get; set; }
        public double VendrediEqu2b { get; set; }
        public double VendrediEqu3b { get; set; }

        public double ProdSamedi { get; set; }
        public double ProdDimanche { get; set; }

        public string Commentaire { get; set; } = "";

        public string LabelLundi { get; set; } = "";
        public string LabelMardi { get; set; } = "";
        public string LabelMercredi { get; set; } = "";
        public string LabelJeudi { get; set; } = "";
        public string LabelVendredi { get; set; } = "";
        public string LabelSamedi { get; set; } = "";
        public string LabelDimanche { get; set; } = "";

        public double TotalProduction =>
            LundiEqu1 + LundiEqu2 + LundiEqu3
            + MardiEqu1 + MardiEqu2 + MardiEqu3
            + MercrediEqu1 + MercrediEqu2 + MercrediEqu3
            + JeudiEqu1 + JeudiEqu2 + JeudiEqu3
            + VendrediEqu1 + VendrediEqu2 + VendrediEqu3
            + ProdSamedi + ProdDimanche;
    }
}