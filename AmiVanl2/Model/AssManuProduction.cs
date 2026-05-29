public class AssManuelProduction
{
    public string Reference { get; set; } = "";
    public string Equipe { get; set; } = "";
    public string Operation { get; set; } = "";

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

    public string Commentaire { get; set; } = "";

    public string LabelLundi { get; set; } = "";
    public string LabelMardi { get; set; } = "";
    public string LabelMercredi { get; set; } = "";
    public string LabelJeudi { get; set; } = "";
    public string LabelVendredi { get; set; } = "";
    public string LabelSamedi { get; set; } = "";
    public string LabelDimanche { get; set; } = "";
}