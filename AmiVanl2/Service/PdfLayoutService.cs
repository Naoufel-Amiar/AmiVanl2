using AmiVanl2.Model;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AmiVanl2.Service
{
    internal class PdfLayoutService
    {
        // A3 paysage en points (1pt = 1/72 pouce). A3 = 420×297mm
        private const double W = 1190.55;
        private const double H = 841.89;
        private const double Marge = 28;
        private const double HeaderH = 52;

        // Palette couleurs sections (identiques aux vues WPF)
        private static readonly XColor ColPresse    = XColor.FromArgb(206, 195, 193);
        private static readonly XColor ColAssAuto   = XColor.FromArgb(246, 225, 207);
        private static readonly XColor ColJoints    = XColor.FromArgb(195, 228, 195);
        private static readonly XColor ColTri       = XColor.FromArgb(195, 215, 235);
        private static readonly XColor ColAssManuel = XColor.FromArgb(230, 210, 235);
        private static readonly XColor ColNavy      = XColor.FromArgb(47,  93,  140);
        private static readonly XColor ColVertClair = XColor.FromArgb(160, 220, 160);
        private static readonly XColor ColRougeClair= XColor.FromArgb(240, 180, 180);
        private static readonly XColor ColGris      = XColor.FromArgb(200, 200, 200);

        // Polices
        private readonly XFont FH1    = new XFont("Arial", 20, XFontStyleEx.Bold);
        private readonly XFont FNorm  = new XFont("Arial", 10, XFontStyleEx.Regular);
        private readonly XFont FBold  = new XFont("Arial", 10, XFontStyleEx.Bold);
        private readonly XFont FSmall = new XFont("Arial",  8, XFontStyleEx.Regular);

        // ===================================================================
        // POINT D'ENTRÉE
        // ===================================================================

        public void GenererRapport(string cheminPdf)
        {
            PdfDocument doc = new PdfDocument();
            doc.Info.Title = "Rapport de Suivi de Production";

            // Filtrage : on exclut les lignes sans aucune production sur la semaine
            var presses    = AppData.Presses?
                .Where(p => ProdTotalPresse(p) > 0).ToList()
                ?? new List<PresseProduction>();

            var assAutos   = AppData.AssAutos?
                .Where(a => ProdTotalAssAuto(a) > 0).ToList()
                ?? new List<AssAutoProduction>();

            var joints     = AppData.Joints?
                .Where(j => j.TotalProduction > 0).ToList()
                ?? new List<JointProduction>();

            var tris       = AppData.Tris?
                .Where(t => ProdTotalTri(t) > 0).ToList()
                ?? new List<TriProduction>();

            var assManuels = AppData.AssManuels?
                .Where(m => ProdTotalAssManu(m) > 0).ToList()
                ?? new List<AssManuelProduction>();

            PageGarde(doc, presses, assAutos, joints, tris, assManuels);

            if (presses.Count > 0)
            {
                PageSeparateur(doc, "SUIVI PRESSE", ColPresse);
                PagePresseBarres(doc, presses);
                PagePresseCamemberts(doc, presses);
            }

            if (assAutos.Count > 0)
            {
                PageSeparateur(doc, "ASSEMBLAGE AUTOMATIQUE", ColAssAuto);
                PageAssAutoBarres(doc, assAutos);
                PageAssAutoCamemberts(doc, assAutos);
            }

            if (joints.Count > 0)
            {
                PageSeparateur(doc, "JOINTS", ColJoints);
                PageJoints(doc, joints);
            }

            if (tris.Count > 0)
            {
                PageSeparateur(doc, "TRI", ColTri);
                PageTri(doc, tris);
            }

            if (assManuels.Count > 0)
            {
                PageSeparateur(doc, "ASSEMBLAGE MANUEL", ColAssManuel);
                PageAssManuel(doc, assManuels);
            }

            doc.Save(cheminPdf);
            doc.Close();
        }

        private double ProdTotalPresse(PresseProduction p) =>
            p.ProdLundi + p.ProdMardi + p.ProdMercredi + p.ProdJeudi +
            p.ProdVendredi + p.ProdSamedi + p.ProdDimanche;

        private double ProdTotalAssAuto(AssAutoProduction a) =>
            a.ProdLundi + a.ProdMardi + a.ProdMercredi + a.ProdJeudi +
            a.ProdVendredi + a.ProdSamedi + a.ProdDimanche;

        private double ProdTotalTri(TriProduction t) =>
            t.LundiEqu1 + t.LundiEqu2 + t.LundiEqu3 +
            t.MardiEqu1 + t.MardiEqu2 + t.MardiEqu3 +
            t.MercrediEqu1 + t.MercrediEqu2 + t.MercrediEqu3 +
            t.JeudiEqu1 + t.JeudiEqu2 + t.JeudiEqu3 +
            t.VendrediEqu1 + t.VendrediEqu2 + t.VendrediEqu3 +
            t.ProdSamedi + t.ProdDimanche;

        private double ProdTotalAssManu(AssManuelProduction m) =>
            m.LundiEqu1 + m.LundiEqu2 + m.LundiEqu3 +
            m.MardiEqu1 + m.MardiEqu2 + m.MardiEqu3 +
            m.MercrediEqu1 + m.MercrediEqu2 + m.MercrediEqu3 +
            m.JeudiEqu1 + m.JeudiEqu2 + m.JeudiEqu3 +
            m.VendrediEqu1 + m.VendrediEqu2 + m.VendrediEqu3 +
            m.ProdSamedi + m.ProdDimanche;

        // ===================================================================
        // HELPERS GÉNÉRAUX
        // ===================================================================

        private PdfPage NouvellePageA3(PdfDocument doc)
        {
            var p = doc.AddPage();
            p.Size = PageSize.A3;
            p.Orientation = PageOrientation.Landscape;
            return p;
        }

        private void DessinerEnTete(XGraphics gfx, string titre, XColor couleur)
        {
            gfx.DrawRectangle(new XSolidBrush(couleur), 0, 0, W, HeaderH);
            gfx.DrawString(titre, FH1,
                new XSolidBrush(XColor.FromArgb(30, 30, 30)),
                new XRect(Marge, 0, W - 2 * Marge, HeaderH),
                XStringFormats.CenterLeft);
        }

        private void DessinerNumeroPage(XGraphics gfx, int numero)
        {
            gfx.DrawString("Page " + numero, FSmall, XBrushes.Gray,
                new XRect(W - 80, H - 20, 70, 15),
                XStringFormats.CenterRight);
        }

        private void DessinerCellule(XGraphics gfx, double x, double y, double w, double h,
            string texte, XFont police, XBrush fond, bool centrer = false)
        {
            gfx.DrawRectangle(fond, x, y, w, h);
            gfx.DrawRectangle(new XPen(XColor.FromArgb(175, 175, 175), 0.4), x, y, w, h);
            gfx.DrawString(texte, police, XBrushes.Black,
                new XRect(x + 3, y, w - 6, h),
                centrer ? XStringFormats.Center : XStringFormats.CenterLeft);
        }

        // Rend un PlotModel OxyPlot en image et l'insère dans la page PDF
        private void PlacerGraphique(XGraphics gfx, PlotModel model,
            double x, double y, double largeur, double hauteur)
        {
            if (largeur <= 0 || hauteur <= 0) return;

            int pixW = (int)(largeur * 2.0);
            int pixH = (int)(hauteur * 2.0);
            if (pixW < 10 || pixH < 10) return;

            string tmpPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
            try
            {
                using (var ms = new MemoryStream())
                {
                    var exporter = new PngExporter { Width = pixW, Height = pixH };
                    exporter.Export(model, ms);
                    File.WriteAllBytes(tmpPath, ms.ToArray());
                }
                using (XImage img = XImage.FromFile(tmpPath))
                    gfx.DrawImage(img, x, y, largeur, hauteur);
            }
            finally
            {
                if (File.Exists(tmpPath)) File.Delete(tmpPath);
            }
        }

        // ===================================================================
        // PAGE DE GARDE
        // ===================================================================

        private void PageGarde(PdfDocument doc,
            List<PresseProduction> presses, List<AssAutoProduction> assAutos,
            List<JointProduction> joints, List<TriProduction> tris,
            List<AssManuelProduction> assManuels)
        {
            var page = NouvellePageA3(doc);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);

                // Bande bleue en haut
                gfx.DrawRectangle(new XSolidBrush(ColNavy), 0, 0, W, 180);

                // Titre
                var fTitre = new XFont("Arial", 34, XFontStyleEx.Bold);
                gfx.DrawString("RAPPORT DE SUIVI DE PRODUCTION", fTitre, XBrushes.White,
                    new XRect(0, 40, W, 75), XStringFormats.Center);

                // Date
                var fDate = new XFont("Arial", 14, XFontStyleEx.Regular);
                gfx.DrawString("Genere le " + DateTime.Now.ToString("dd/MM/yyyy a HH:mm"),
                    fDate, XBrushes.White, new XRect(0, 125, W, 38), XStringFormats.Center);

                // Titre "Sections"
                var fSec = new XFont("Arial", 14, XFontStyleEx.Bold);
                gfx.DrawString("SECTIONS DU RAPPORT", fSec,
                    new XSolidBrush(XColor.FromArgb(50, 50, 50)),
                    new XRect(0, 218, W, 28), XStringFormats.Center);

                // Cartes sections — basées sur les listes filtrées
                var sections = new (string Nom, XColor Couleur, bool Active, int NbRefs)[]
                {
                    ("Suivi Presse",           ColPresse,    presses.Count > 0,    presses.Select(p => p.Reference).Distinct().Count()),
                    ("Assemblage Automatique", ColAssAuto,   assAutos.Count > 0,   assAutos.Select(a => a.Reference).Distinct().Count()),
                    ("Joints",                 ColJoints,    joints.Count > 0,     joints.Count),
                    ("Tri",                    ColTri,       tris.Count > 0,       tris.Count),
                    ("Assemblage Manuel",      ColAssManuel, assManuels.Count > 0, assManuels.Select(m => m.Reference).Distinct().Count()),
                };

                double cardW = 185, cardH = 78, gap = 16;
                double totalW = sections.Length * cardW + (sections.Length - 1) * gap;
                double startX = (W - totalW) / 2;
                double cardY = 258;

                var fCard    = new XFont("Arial", 13, XFontStyleEx.Bold);
                var fCardSub = new XFont("Arial",  9, XFontStyleEx.Regular);

                foreach (var (nom, coul, active, nbRefs) in sections)
                {
                    XColor bg = active ? coul : XColor.FromArgb(215, 215, 215);
                    gfx.DrawRoundedRectangle(new XSolidBrush(bg), startX, cardY, cardW, cardH, 10, 10);
                    gfx.DrawString(nom, fCard,
                        new XSolidBrush(active ? XColor.FromArgb(30, 30, 30) : XColor.FromArgb(150, 150, 150)),
                        new XRect(startX, cardY + 8, cardW, 24), XStringFormats.Center);
                    string statut = active ? nbRefs + " ref(s) en production" : "Aucune donnee";
                    gfx.DrawString(statut, fCardSub,
                        new XSolidBrush(active ? XColor.FromArgb(30, 120, 30) : XColor.FromArgb(150, 0, 0)),
                        new XRect(startX, cardY + cardH - 28, cardW, 16), XStringFormats.Center);
                    startX += cardW + gap;
                }

                // Pied de page
                gfx.DrawString("Document à usage interne — Impression recommandée en A3",
                    FSmall, XBrushes.Gray, new XRect(0, H - 28, W, 18), XStringFormats.Center);
            }
        }

        // ===================================================================
        // SÉPARATEUR DE SECTION
        // ===================================================================

        private void PageSeparateur(PdfDocument doc, string titre, XColor couleur)
        {
            var page = NouvellePageA3(doc);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(new XSolidBrush(couleur), 0, 0, W, H);

                var fTitre = new XFont("Arial", 50, XFontStyleEx.Bold);
                var pen = new XPen(XColor.FromArgb(80, 80, 80), 1.5);

                gfx.DrawLine(pen, 80, H / 2 - 62, W - 80, H / 2 - 62);
                gfx.DrawString(titre, fTitre,
                    new XSolidBrush(XColor.FromArgb(30, 30, 30)),
                    new XRect(0, 0, W, H), XStringFormats.Center);
                gfx.DrawLine(pen, 80, H / 2 + 62, W - 80, H / 2 + 62);
            }
        }

        // ===================================================================
        // PRESSE — BARRES JOURNALIÈRES
        // ===================================================================

        private void PagePresseBarres(PdfDocument doc, List<PresseProduction> presses)
        {
            // Découpe en groupes de 6 max pour que les barres restent lisibles
            int taille = Math.Min(6, Math.Max(1, presses.Count <= 6 ? presses.Count : (presses.Count + 1) / 2));
            var g1 = presses.Take(taille).ToList();
            var g2 = presses.Skip(taille).Take(taille).ToList();

            var m1 = BuildBarresPresse(g1, "Production presse — Groupe 1");
            var m2 = g2.Count > 0 ? BuildBarresPresse(g2, "Production presse — Groupe 2") : null;

            var page = NouvellePageA3(doc);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                DessinerEnTete(gfx, "Suivi Presse — Production journaliere VS Objectif", ColPresse);
                DessinerLegendeCouleurs(gfx, H - 18);

                double y0 = HeaderH + Marge;
                double cW = W - 2 * Marge;

                if (m2 != null)
                {
                    double cH = (H - y0 - Marge - 22) / 2;
                    PlacerGraphique(gfx, m1, Marge, y0, cW, cH);
                    PlacerGraphique(gfx, m2, Marge, y0 + cH + 4, cW, cH);
                }
                else
                {
                    PlacerGraphique(gfx, m1, Marge, y0, cW, H - y0 - Marge - 22);
                }
                DessinerNumeroPage(gfx, doc.Pages.Count);
            }
        }

        private PlotModel BuildBarresPresse(List<PresseProduction> groupe, string titre)
        {
            var model = new PlotModel { Title = titre, Background = OxyColors.White };
            if (groupe.Count == 0) return model;

            var axeX = new CategoryAxis { Position = AxisPosition.Bottom };
            var p0 = groupe[0];
            foreach (var l in new[] { p0.LabelLundi, p0.LabelMardi, p0.LabelMercredi,
                                      p0.LabelJeudi, p0.LabelVendredi, p0.LabelSamedi, p0.LabelDimanche })
                axeX.Labels.Add(l);

            var axeY = new LinearAxis { Position = AxisPosition.Left, Minimum = 0, Title = "Production" };
            var serie = new RectangleBarSeries { Title = "Références" };
            double lB = 0.18, eG = 0.8;

            for (int j = 0; j < 7; j++)
                for (int r = 0; r < groupe.Count; r++)
                {
                    var p = groupe[r];
                    double prod = ProdJourPresse(p, j);
                    double obj  = p.ObjectifSemaine / 7.0;
                    double x0   = j - eG / 2.0 + r * lB;
                    var item    = new RectangleBarItem(x0, 0, x0 + lB, prod);
                    item.Color  = prod >= obj ? OxyColors.SeaGreen : OxyColors.IndianRed;
                    serie.Items.Add(item);
                }

            model.Axes.Add(axeX);
            model.Axes.Add(axeY);
            model.Series.Add(serie);
            return model;
        }

        // ===================================================================
        // PRESSE — CAMEMBERTS OBJECTIFS SEMAINE
        // ===================================================================

        private void PagePresseCamemberts(PdfDocument doc, List<PresseProduction> presses)
        {
            // Agrégation par référence (une même ref peut tourner sur plusieurs machines)
            var refs = presses
                .GroupBy(p => p.Reference)
                .Select(g => new {
                    Titre        = g.Key.ToString("000000") + " - " + string.Join("/", g.Select(x => x.Machine).Distinct()),
                    TotalProd    = g.Sum(x => x.TotalProduction),
                    TotalObj     = g.Max(x => x.ObjectifSemaine)
                }).ToList();

            int cols = refs.Count <= 3 ? refs.Count : refs.Count <= 6 ? 3 : 4;
            int rows = (int)Math.Ceiling(refs.Count / (double)cols);

            var page = NouvellePageA3(doc);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                DessinerEnTete(gfx, "Suivi Presse — Taux d'atteinte des objectifs semaine", ColPresse);

                double y0 = HeaderH + Marge;
                double cH = (H - y0 - Marge) / rows;
                double cW = (W - 2 * Marge) / cols;

                for (int i = 0; i < refs.Count; i++)
                {
                    var r = refs[i];
                    var model = BuildCamembert(r.Titre, r.TotalProd, r.TotalObj);
                    int col = i % cols, row = i / cols;
                    PlacerGraphique(gfx, model, Marge + col * cW, y0 + row * cH, cW - 6, cH - 6);
                }
                DessinerNumeroPage(gfx, doc.Pages.Count);
            }
        }

        // ===================================================================
        // ASS AUTO — BARRES JOURNALIÈRES
        // ===================================================================

        private void PageAssAutoBarres(PdfDocument doc, List<AssAutoProduction> assAutos)
        {
            int taille = Math.Min(6, Math.Max(1, assAutos.Count <= 6 ? assAutos.Count : (assAutos.Count + 1) / 2));
            var g1 = assAutos.Take(taille).ToList();
            var g2 = assAutos.Skip(taille).Take(taille).ToList();

            var m1 = BuildBarresAssAuto(g1, "Assemblage Auto — Groupe 1");
            var m2 = g2.Count > 0 ? BuildBarresAssAuto(g2, "Assemblage Auto — Groupe 2") : null;

            var page = NouvellePageA3(doc);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                DessinerEnTete(gfx, "Assemblage Automatique — Production journaliere VS Objectif", ColAssAuto);
                DessinerLegendeCouleurs(gfx, H - 18);

                double y0 = HeaderH + Marge;
                double cW  = W - 2 * Marge;

                if (m2 != null)
                {
                    double cH = (H - y0 - Marge - 22) / 2;
                    PlacerGraphique(gfx, m1, Marge, y0, cW, cH);
                    PlacerGraphique(gfx, m2, Marge, y0 + cH + 4, cW, cH);
                }
                else
                {
                    PlacerGraphique(gfx, m1, Marge, y0, cW, H - y0 - Marge - 22);
                }
                DessinerNumeroPage(gfx, doc.Pages.Count);
            }
        }

        private PlotModel BuildBarresAssAuto(List<AssAutoProduction> groupe, string titre)
        {
            var model = new PlotModel { Title = titre, Background = OxyColors.White };
            if (groupe.Count == 0) return model;

            var axeX = new CategoryAxis { Position = AxisPosition.Bottom };
            var a0 = groupe[0];
            foreach (var l in new[] { a0.LabelLundi, a0.LabelMardi, a0.LabelMercredi,
                                      a0.LabelJeudi, a0.LabelVendredi, a0.LabelSamedi, a0.LabelDimanche })
                axeX.Labels.Add(l);

            var axeY = new LinearAxis { Position = AxisPosition.Left, Minimum = 0, Title = "Production" };
            var serie = new RectangleBarSeries { Title = "Références" };
            double lB = 0.18, eG = 0.8;

            for (int j = 0; j < 7; j++)
                for (int r = 0; r < groupe.Count; r++)
                {
                    var a    = groupe[r];
                    double prod = ProdJourAssAuto(a, j);
                    double obj  = a.ObjectifJournalierCalcule;
                    double x0   = j - eG / 2.0 + r * lB;
                    var item    = new RectangleBarItem(x0, 0, x0 + lB, prod);
                    item.Color  = prod >= obj ? OxyColors.SeaGreen : OxyColors.IndianRed;
                    serie.Items.Add(item);
                }

            model.Axes.Add(axeX);
            model.Axes.Add(axeY);
            model.Series.Add(serie);
            return model;
        }

        // ===================================================================
        // ASS AUTO — CAMEMBERTS OBJECTIFS SEMAINE
        // ===================================================================

        private void PageAssAutoCamemberts(PdfDocument doc, List<AssAutoProduction> assAutos)
        {
            var refs = assAutos
                .Where(a => a.ObjectifSemaine > 0)
                .GroupBy(a => a.Reference)
                .Select(g => new {
                    Titre     = g.Key + " - " + string.Join("/", g.Select(x => x.Machine).Distinct()),
                    TotalProd = g.Sum(x => x.TotalProduction),
                    TotalObj  = g.Sum(x => x.ObjectifSemaine)
                }).ToList();

            int cols = refs.Count <= 3 ? refs.Count : refs.Count <= 6 ? 3 : 4;
            int rows = (int)Math.Ceiling(refs.Count / (double)cols);

            var page = NouvellePageA3(doc);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                DessinerEnTete(gfx, "Assemblage Automatique — Taux d'atteinte des objectifs semaine", ColAssAuto);

                double y0 = HeaderH + Marge;
                double cH = (H - y0 - Marge) / rows;
                double cW = (W - 2 * Marge) / cols;

                for (int i = 0; i < refs.Count; i++)
                {
                    var r = refs[i];
                    var model = BuildCamembert(r.Titre, r.TotalProd, r.TotalObj);
                    int col = i % cols, row = i / cols;
                    PlacerGraphique(gfx, model, Marge + col * cW, y0 + row * cH, cW - 6, cH - 6);
                }
                DessinerNumeroPage(gfx, doc.Pages.Count);
            }
        }

        // ===================================================================
        // JOINTS — TABLEAU COMPACT
        // ===================================================================

        private void PageJoints(PdfDocument doc, List<JointProduction> joints)
        {
            // Camembert hebdo par référence (Equ1/Equ2/Equ3 + Reste vs objectif)
            int cols = joints.Count <= 3 ? joints.Count : joints.Count <= 6 ? 3 : 4;
            int rows = (int)Math.Ceiling(joints.Count / (double)cols);

            var page = NouvellePageA3(doc);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                DessinerEnTete(gfx, "Joints — Taux d'atteinte objectif semaine par reference (EQU1/EQU2/EQU3)", ColJoints);

                double y0 = HeaderH + Marge;
                double cH = (H - y0 - Marge) / rows;
                double cW = (W - 2 * Marge) / cols;

                for (int i = 0; i < joints.Count; i++)
                {
                    var jt = joints[i];
                    double equ1 = jt.LundiEqu1 + jt.MardiEqu1 + jt.MercrediEqu1 + jt.JeudiEqu1 + jt.VendrediEqu1;
                    double equ2 = jt.LundiEqu2 + jt.MardiEqu2 + jt.MercrediEqu2 + jt.JeudiEqu2 + jt.VendrediEqu2;
                    double equ3 = jt.LundiEqu3 + jt.MardiEqu3 + jt.MercrediEqu3 + jt.JeudiEqu3 + jt.VendrediEqu3;
                    var model = BuildCamembertEquipes(jt.Reference + " - " + jt.AncienCode, equ1, equ2, equ3, jt.ObjectifSemaine);
                    int col = i % cols, row = i / cols;
                    PlacerGraphique(gfx, model, Marge + col * cW, y0 + row * cH, cW - 6, cH - 6);
                }
                DessinerNumeroPage(gfx, doc.Pages.Count);
            }
        }

        // ===================================================================
        // TRI — TABLEAU COMPACT
        // ===================================================================

        private void PageTri(PdfDocument doc, List<TriProduction> tris)
        {
            int cols = tris.Count <= 3 ? tris.Count : tris.Count <= 6 ? 3 : 4;
            int rows = (int)Math.Ceiling(tris.Count / (double)cols);

            var page = NouvellePageA3(doc);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                DessinerEnTete(gfx, "Tri — Taux d'atteinte objectif semaine par reference (EQU1/EQU2/EQU3)", ColTri);

                double y0 = HeaderH + Marge;
                double cH = (H - y0 - Marge) / rows;
                double cW = (W - 2 * Marge) / cols;

                for (int i = 0; i < tris.Count; i++)
                {
                    var tr = tris[i];
                    double equ1 = tr.LundiEqu1 + tr.MardiEqu1 + tr.MercrediEqu1 + tr.JeudiEqu1 + tr.VendrediEqu1;
                    double equ2 = tr.LundiEqu2 + tr.MardiEqu2 + tr.MercrediEqu2 + tr.JeudiEqu2 + tr.VendrediEqu2;
                    double equ3 = tr.LundiEqu3 + tr.MardiEqu3 + tr.MercrediEqu3 + tr.JeudiEqu3 + tr.VendrediEqu3;
                    var model = BuildCamembertEquipes(tr.Reference + " - " + tr.AncienCode, equ1, equ2, equ3, tr.ObjectifSemaine);
                    int col = i % cols, row = i / cols;
                    PlacerGraphique(gfx, model, Marge + col * cW, y0 + row * cH, cW - 6, cH - 6);
                }
                DessinerNumeroPage(gfx, doc.Pages.Count);
            }
        }


        // ===================================================================
        // ASSEMBLAGE MANUEL — COURBES PAR RÉFÉRENCE
        // ===================================================================

        private void PageAssManuel(PdfDocument doc, List<AssManuelProduction> assManuels)
        {
            var references = assManuels
                .Select(x => x.Reference)
                .Distinct()
                .ToList();

            int cols = references.Count <= 2 ? references.Count :
                       references.Count <= 4 ? 2 : 3;
            int refsParPage = cols * 2;

            for (int debut = 0; debut < references.Count; debut += refsParPage)
            {
                var refPage = references.Skip(debut).Take(refsParPage).ToList();
                int lignes  = (int)Math.Ceiling(refPage.Count / (double)cols);

                var page = NouvellePageA3(doc);
                using (var gfx = XGraphics.FromPdfPage(page))
                {
                    gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                    DessinerEnTete(gfx, "Assemblage Manuel — Production par reference (Capuchon / Insert / Objectif)", ColAssManuel);

                    double y0 = HeaderH + Marge;
                    double cH = (H - y0 - Marge) / lignes;
                    double cW = (W - 2 * Marge) / cols;

                    for (int i = 0; i < refPage.Count; i++)
                    {
                        string reference = refPage[i];
                        int col = i % cols, row = i / cols;

                        var capuchon = assManuels.FirstOrDefault(x =>
                            x.Reference == reference && x.Operation.ToLower().Contains("capuchon"));
                        var insert = assManuels.FirstOrDefault(x =>
                            x.Reference == reference && x.Operation.ToLower().Contains("insert"));

                        if (capuchon == null && insert == null) continue;

                        double objSemaine = capuchon?.ObjectifSemaine ?? insert.ObjectifSemaine;
                        double[] prodCap  = ProdJoursAssManu(capuchon);
                        double[] prodIns  = ProdJoursAssManu(insert);

                        var model = BuildGraphiqueAssManuel(reference, prodCap, prodIns, objSemaine / 5.0);
                        PlacerGraphique(gfx, model,
                            Marge + col * cW, y0 + row * cH, cW - 8, cH - 8);
                    }
                    DessinerNumeroPage(gfx, doc.Pages.Count);
                }
            }
        }

        private PlotModel BuildGraphiqueAssManuel(string reference, double[] prodCap, double[] prodIns, double objJour)
        {
            var model = new PlotModel { Title = reference, Background = OxyColors.White };

            var axeX = new CategoryAxis { Position = AxisPosition.Bottom };
            foreach (var j in new[] { "Lun", "Mar", "Mer", "Jeu", "Ven", "Sam", "Dim" })
                axeX.Labels.Add(j);

            var axeY = new LinearAxis { Position = AxisPosition.Left, Minimum = 0, Title = "Production" };

            var serCap = new LineSeries
            {
                Title           = "Capuchon",
                StrokeThickness = 2,
                MarkerType      = MarkerType.Circle,
                Color           = OxyColor.FromRgb(40, 120, 200)
            };
            var serIns = new LineSeries
            {
                Title           = "Insert",
                StrokeThickness = 2,
                MarkerType      = MarkerType.Square,
                Color           = OxyColor.FromRgb(200, 100, 40)
            };
            var serObj = new LineSeries
            {
                Title           = "Objectif jour",
                StrokeThickness = 2,
                MarkerType      = MarkerType.Diamond,
                Color           = OxyColors.Orange,
                LineStyle       = LineStyle.Dash
            };

            for (int i = 0; i < 7; i++)
            {
                serCap.Points.Add(new DataPoint(i, prodCap[i]));
                serIns.Points.Add(new DataPoint(i, prodIns[i]));
                // Objectif uniquement Lun-Ven (pas de cible weekend)
                if (i <= 4)
                    serObj.Points.Add(new DataPoint(i, objJour));
            }

            model.Axes.Add(axeX);
            model.Axes.Add(axeY);
            model.Series.Add(serCap);
            model.Series.Add(serIns);
            model.Series.Add(serObj);
            return model;
        }

        // ===================================================================
        // CAMEMBERT GÉNÉRIQUE (Presse / AssAuto)
        // ===================================================================

        private PlotModel BuildCamembert(string titre, double prod, double obj)
        {
            double reste  = Math.Max(0, obj - prod);
            bool   atteint = prod >= obj;

            var model = new PlotModel { Title = titre, Background = OxyColors.White };
            var serie = new PieSeries
            {
                StrokeThickness     = 0,
                InsideLabelPosition = 0.68,
                InsideLabelFormat   = "{2:0}%",
                OutsideLabelFormat  = ""
            };

            serie.Slices.Add(new PieSlice(prod.ToString("0") + " pcs", prod)
            {
                Fill = atteint ? OxyColor.FromRgb(40, 160, 80) : OxyColor.FromRgb(210, 60, 60)
            });
            if (reste > 0)
                serie.Slices.Add(new PieSlice("Reste", reste) { Fill = OxyColor.FromRgb(210, 210, 210) });

            model.Series.Add(serie);
            return model;
        }

        // Camembert avec répartition 3 équipes + reste vs objectif
        private PlotModel BuildCamembertEquipes(string titre, double equ1, double equ2, double equ3, double objectifSemaine)
        {
            double totalProd = equ1 + equ2 + equ3;
            double reste     = Math.Max(0, objectifSemaine - totalProd);
            double pct       = objectifSemaine > 0 ? totalProd / objectifSemaine * 100 : 0;

            var model = new PlotModel
            {
                Title      = titre + "\n" + pct.ToString("0") + "% — " + totalProd.ToString("0") + " / " + objectifSemaine.ToString("0"),
                Background = OxyColors.White
            };
            var serie = new PieSeries
            {
                StrokeThickness     = 0.5,
                InsideLabelPosition = 0.65,
                InsideLabelFormat   = "{2:0}%",
                OutsideLabelFormat  = ""
            };

            if (equ1 > 0) serie.Slices.Add(new PieSlice("EQU1", equ1) { Fill = OxyColor.FromRgb(0, 100, 0) });
            if (equ2 > 0) serie.Slices.Add(new PieSlice("EQU2", equ2) { Fill = OxyColor.FromRgb(0, 160, 0) });
            if (equ3 > 0) serie.Slices.Add(new PieSlice("EQU3", equ3) { Fill = OxyColor.FromRgb(100, 210, 100) });
            if (reste > 0) serie.Slices.Add(new PieSlice("Reste", reste) { Fill = OxyColor.FromRgb(210, 60, 60) });
            if (totalProd == 0 && reste == 0)
                serie.Slices.Add(new PieSlice("Aucune prod", 1) { Fill = OxyColor.FromRgb(200, 200, 200) });

            model.Series.Add(serie);
            return model;
        }

        // ===================================================================
        // LÉGENDE COULEURS (partagée barres et tableaux)
        // ===================================================================

        private void DessinerLegendeCouleurs(XGraphics gfx, double y)
        {
            double x = Marge;
            gfx.DrawRectangle(new XSolidBrush(ColVertClair), x, y, 12, 10);
            gfx.DrawString("Objectif atteint", FSmall, XBrushes.Black,
                new XRect(x + 15, y, 110, 12), XStringFormats.CenterLeft);
            x += 130;
            gfx.DrawRectangle(new XSolidBrush(ColRougeClair), x, y, 12, 10);
            gfx.DrawString("Objectif non atteint", FSmall, XBrushes.Black,
                new XRect(x + 15, y, 130, 12), XStringFormats.CenterLeft);
            x += 150;
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(230, 230, 230)), x, y, 12, 10);
            gfx.DrawString("Weekend", FSmall, XBrushes.Black,
                new XRect(x + 15, y, 70, 12), XStringFormats.CenterLeft);
        }

        // ===================================================================
        // GETTERS PRODUCTION
        // ===================================================================

        private double ProdJourPresse(PresseProduction p, int j) =>
            j == 0 ? p.ProdLundi   : j == 1 ? p.ProdMardi    : j == 2 ? p.ProdMercredi :
            j == 3 ? p.ProdJeudi   : j == 4 ? p.ProdVendredi : j == 5 ? p.ProdSamedi   : p.ProdDimanche;

        private double ProdJourAssAuto(AssAutoProduction a, int j) =>
            j == 0 ? a.ProdLundi   : j == 1 ? a.ProdMardi    : j == 2 ? a.ProdMercredi :
            j == 3 ? a.ProdJeudi   : j == 4 ? a.ProdVendredi : j == 5 ? a.ProdSamedi   : a.ProdDimanche;

        private double[] ProdJoursAssManu(AssManuelProduction op)
        {
            if (op == null) return new double[7];
            return new double[]
            {
                op.LundiEqu1    + op.LundiEqu2    + op.LundiEqu3,
                op.MardiEqu1    + op.MardiEqu2    + op.MardiEqu3,
                op.MercrediEqu1 + op.MercrediEqu2 + op.MercrediEqu3,
                op.JeudiEqu1    + op.JeudiEqu2    + op.JeudiEqu3,
                op.VendrediEqu1 + op.VendrediEqu2 + op.VendrediEqu3,
                op.ProdSamedi,
                op.ProdDimanche
            };
        }
    }
}
