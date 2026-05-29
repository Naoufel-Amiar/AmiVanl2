using AmiVanl2.Model;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace AmiVanl2.Service
{
    public class SuiviTriService
    {
        public async Task<List<TriProduction>> LireTrisAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                List<TriProduction> resultats =
                    new List<TriProduction>();

                ExcelPackage.License
                    .SetNonCommercialPersonal("Naoufel Amiar");

                FileInfo fichier =
                    new FileInfo(filePath);

                using (ExcelPackage package = new ExcelPackage(fichier))
                {
                    ExcelWorksheet feuille =
                        TrouverFeuille(package, "Suivi TRI");

                    if (feuille == null)
                    {
                        throw new Exception(
                            "La feuille 'Suivi TRI' est introuvable.");
                    }

                    if (feuille.Dimension == null)
                    {
                        return resultats;
                    }

                    int ligneDebut = 7;
                    int ligneFin = feuille.Dimension.End.Row;

                    string labelLundi = LireDateExcel(feuille, 4, 6);
                    string labelMardi = LireDateExcel(feuille, 4, 9);
                    string labelMercredi = LireDateExcel(feuille, 4, 12);
                    string labelJeudi = LireDateExcel(feuille, 4, 15);
                    string labelVendredi = LireDateExcel(feuille, 4, 18);
                    string labelSamedi = LireDateExcel(feuille, 4, 21);
                    string labelDimanche = LireDateExcel(feuille, 4, 22);

                    int lignesVidesConsecutives = 0;

                    for (int ligne = ligneDebut; ligne <= ligneFin; ligne++)
                    {
                        string referenceTexte =
                            LireReference(feuille, ligne, 1);

                        if (!EstReferenceValide(referenceTexte))
                        {
                            lignesVidesConsecutives++;

                            if (lignesVidesConsecutives >= 5)
                            {
                                break;
                            }

                            continue;
                        }

                        lignesVidesConsecutives = 0;

                        TriProduction tri =
                            new TriProduction();

                        tri.LabelLundi = labelLundi;
                        tri.LabelMardi = labelMardi;
                        tri.LabelMercredi = labelMercredi;
                        tri.LabelJeudi = labelJeudi;
                        tri.LabelVendredi = labelVendredi;
                        tri.LabelSamedi = labelSamedi;
                        tri.LabelDimanche = labelDimanche;

                        tri.Reference = referenceTexte;
                        tri.AncienCode = LireTexte(feuille, ligne, 2);
                        tri.Equipe = LireTexte(feuille, ligne, 3);

                        tri.ObjectifSemaine = LireDouble(feuille, ligne, 4);
                        tri.ObjectifEquipe = LireDouble(feuille, ligne, 5);

                        tri.LundiEqu1 = LireDouble(feuille, ligne, 6);
                        tri.LundiEqu2 = LireDouble(feuille, ligne, 7);
                        tri.LundiEqu3 = LireDouble(feuille, ligne, 8);

                        tri.MardiEqu1 = LireDouble(feuille, ligne, 9);
                        tri.MardiEqu2 = LireDouble(feuille, ligne, 10);
                        tri.MardiEqu3 = LireDouble(feuille, ligne, 11);

                        tri.MercrediEqu1 = LireDouble(feuille, ligne, 12);
                        tri.MercrediEqu2 = LireDouble(feuille, ligne, 13);
                        tri.MercrediEqu3 = LireDouble(feuille, ligne, 14);

                        tri.JeudiEqu1 = LireDouble(feuille, ligne, 15);
                        tri.JeudiEqu2 = LireDouble(feuille, ligne, 16);
                        tri.JeudiEqu3 = LireDouble(feuille, ligne, 17);

                        tri.VendrediEqu1 = LireDouble(feuille, ligne, 18);
                        tri.VendrediEqu2 = LireDouble(feuille, ligne, 19);
                        tri.VendrediEqu3 = LireDouble(feuille, ligne, 20);

                        tri.ProdSamedi = LireDouble(feuille, ligne, 21);
                        tri.ProdDimanche = LireDouble(feuille, ligne, 22);

                        tri.Commentaire = LireTexte(feuille, ligne, 23);

                        resultats.Add(tri);
                    }
                }

                return resultats;
            });
        }

        private ExcelWorksheet TrouverFeuille(ExcelPackage package, string nomFeuille)
        {
            foreach (ExcelWorksheet feuille in package.Workbook.Worksheets)
            {
                if (NormaliserNom(feuille.Name) == NormaliserNom(nomFeuille))
                {
                    return feuille;
                }
            }

            return null;
        }

        private string NormaliserNom(string texte)
        {
            return texte
                .Replace(" ", "")
                .Replace("_", "")
                .ToUpperInvariant();
        }

        private bool EstReferenceValide(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return false;
            }

            reference = reference.Trim();

            if (reference.Length != 6)
            {
                return false;
            }

            return int.TryParse(reference, out _);
        }

        private string LireReference(ExcelWorksheet feuille, int ligne, int colonne)
        {
            string texte =
                feuille.Cells[ligne, colonne].Text;

            if (!string.IsNullOrWhiteSpace(texte))
            {
                return texte.Trim();
            }

            object valeur =
                feuille.Cells[ligne, colonne].Value;

            if (valeur == null)
            {
                return "";
            }

            if (int.TryParse(valeur.ToString(), out int referenceNumerique))
            {
                return referenceNumerique.ToString("000000");
            }

            return valeur.ToString().Trim();
        }

        private string LireTexte(ExcelWorksheet feuille, int ligne, int colonne)
        {
            object valeur =
                feuille.Cells[ligne, colonne].Value;

            return valeur == null
                ? ""
                : valeur.ToString().Trim();
        }

        private string LireDateExcel(ExcelWorksheet feuille, int ligne, int colonne)
        {
            object valeur =
                feuille.Cells[ligne, colonne].Value;

            if (valeur == null)
            {
                return "";
            }

            if (valeur is double nombre)
            {
                DateTime date =
                    DateTime.FromOADate(nombre);

                return date.ToString("ddd dd/MM");
            }

            if (DateTime.TryParse(valeur.ToString(), out DateTime dateTexte))
            {
                return dateTexte.ToString("ddd dd/MM");
            }

            return valeur.ToString();
        }

        private double LireDouble(ExcelWorksheet feuille, int ligne, int colonne)
        {
            string texte =
                feuille.Cells[ligne, colonne].Text;

            if (string.IsNullOrWhiteSpace(texte))
            {
                object valeur =
                    feuille.Cells[ligne, colonne].Value;

                if (valeur == null)
                {
                    return 0;
                }

                texte = valeur.ToString();
            }

            texte = texte
                .Replace(" ", "")
                .Replace("\u00A0", "")
                .Replace(",", ".")
                .Trim();

            if (double.TryParse(
                    texte,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out double resultat))
            {
                return resultat;
            }

            return 0;
        }
    }
}

