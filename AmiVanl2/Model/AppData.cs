using System.Collections.Generic;

namespace AmiVanl2.Model
{
    public static class AppData
    {
        // Fichier importé
        public static string ExcelFilePath { get; set; } = "";

        // Données récupérées feuille PRESSE
        public static List<PresseProduction> Presses { get; set; }
            = new List<PresseProduction>();

        // Données récupérées feuille ASS AUTO
        public static List<AssAutoProduction> AssAutos { get; set; }
            = new List<AssAutoProduction>();

        // Données récupérées feuille JOINT
        public static List<JointProduction> Joints { get; set; }
            = new List<JointProduction>();

        // Données récupérées feuille Tri
        public static List<TriProduction> Tris { get; set; }
            = new List<TriProduction>();

        // Données récupérées feuille Ass Manu
        public static List<AssManuelProduction> AssManuels { get; set; }
            = new List<AssManuelProduction>();

        public static bool DonneesGenerees { get; set; } = false;

        // Vérifie qu'un fichier est chargé
        public static bool HasExcelFile()
        {
            return !string.IsNullOrWhiteSpace(
                ExcelFilePath
            );
        }


        // Reset global
        public static void Reset()
        {
            Joints.Clear();

            AssAutos.Clear();
            AssManuels.Clear();

            Presses.Clear();
            Tris.Clear();

            ExcelFilePath = "";

            DonneesGenerees = false;
        }
    }
}