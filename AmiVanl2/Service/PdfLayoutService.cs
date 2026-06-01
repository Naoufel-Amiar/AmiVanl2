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
        private const double W = 1190.55;
        private const double H = 841.89;
        private const double Marge = 28;
        private const double HeaderH = 52;

        private static readonly XColor ColPresse    = XColor.FromArgb(206, 195, 193);
        private static readonly XColor ColAssAuto   = XColor.FromArgb(246, 225, 207);
        private static readonly XColor ColJoints    = XColor.FromArgb(195, 228, 195);
        private static readonly XColor ColTri       = XColor.FromArgb(195, 215, 235);
        private static readonly XColor ColAssManuel = XColor.FromArgb(230, 210, 235);
        private static readonly XColor ColNavy      = XColor.FromArgb(47,  93,  140);
        private static readonly XColor ColVertClair = XColor.FromArgb(160, 220, 160);
        private static readonly XColor ColRougeClair= XColor.FromArgb(240, 180, 180);
        private static readonly XColor ColGris      = XColor.FromArgb(200, 200, 200);
        private static readonly XColor ColBlanc     = XColor.FromArgb(255, 255, 255);

        private readonly XFont FH1    = new XFont("Arial", 20, XFontStyleEx.Bold);
        private readonly XFont FNorm  = new XFont("Arial", 10, XFontStyleEx.Regular);
        private readonly XFont FBold  = new XFont("Arial", 10, XFontStyleEx.Bold);
        private readonly XFont FSmall = new XFont("Arial",  8, XFontStyleEx.Regular);
        private readonly XFont FTiny  = new XFont("Arial",  7, XFontStyleEx.Regular);

        // ===================================================================
        // CLASSES INTERNES
        // ===================================================================

        private class RefAgregee
        {
            public string Label;
            public string[] DayLabels;
            public double[] Prods;
            public double ObjSemaine;
        }

        private class LigneTable
        {
            public string Reference;
            public string AncienCode;
            public double ObjSemaine;
            public string[] DayLabels;    // 7 elements : Lun..Dim
            public double[][] DayEquipes; // [8][3] : 7 jours + index 7=Total, chacun [equ1,equ2,equ3]
        }

        // ===================================================================
        // POINT D'ENTREE
        // ===================================================================

        public void GenererRapport(string cheminPdf)
        {
            PdfDocument doc = new PdfDocument();
            doc.Info.Title = "Rapport de Suivi de Production";

            var presses    = AppData.Presses?.Where(p => ProdTotalPresse(p) > 0).ToList()
                             ?? new List<PresseProduction>();
            var assAutos   = AppData.AssAutos?.Where(a => ProdTotalAssAuto(a) > 0).ToList()
                             ?? new List<AssAutoProduction>();
            var joints     = AppData.Joints?.Where(j => j.TotalProduction > 0).ToList()
                             ?? new List<JointProduction>();
            var tris       = AppData.Tris?.Where(t => ProdTotalTri(t) > 0).ToList()
                             ?? new List<TriProduction>();
            var assManuels = AppData.AssManuels?.Where(m => ProdTotalAssManu(m) > 0).ToList()
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
                PageTableEquipes(doc,
                    "Joints — Suivi journalier par equipe et par reference",
                    ColJoints, BuildLignesJoints(joints));
            }

            if (tris.Count > 0)
            {
                PageSeparateur(doc, "TRI", ColTri);
                PageTableEquipes(doc,
                    "Tri — Suivi journalier par equipe et par reference",
                    ColTri, BuildLignesTri(tris));
            }

            if (assManuels.Count > 0)
            {
                PageSeparateur(doc, "ASSEMBLAGE MANUEL", ColAssManuel);
                PageAssManuelBarres(doc, assManuels);
                PageAssManuelCamemberts(doc, assManuels);
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
        // HELPERS GENERAUX
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
                gfx.DrawRectangle(new XSolidBrush(ColNavy), 0, 0, W, 180);

                var fTitre = new XFont("Arial", 34, XFontStyleEx.Bold);
                gfx.DrawString("RAPPORT DE SUIVI DE PRODUCTION", fTitre, XBrushes.White,
                    new XRect(0, 40, W, 75), XStringFormats.Center);

                var fDate = new XFont("Arial", 14, XFontStyleEx.Regular);
                gfx.DrawString("Genere le " + DateTime.Now.ToString("dd/MM/yyyy a HH:mm"),
                    fDate, XBrushes.White, new XRect(0, 125, W, 38), XStringFormats.Center);

                var fSec = new XFont("Arial", 14, XFontStyleEx.Bold);
                gfx.DrawString("SECTIONS DU RAPPORT", fSec,
                    new XSolidBrush(XColor.FromArgb(50, 50, 50)),
                    new XRect(0, 218, W, 28), XStringFormats.Center);

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

                gfx.DrawString("Document a usage interne — Impression recommandee en A3",
                    FSmall, XBrushes.Gray, new XRect(0, H - 28, W, 18), XStringFormats.Center);
            }
        }

        // ===================================================================
        // SEPARATEUR DE SECTION
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
        // PRESSE — BARRES PAR REFERENCE (une chart par ref, valeurs + objectif)
        // ===================================================================

        private void PagePresseBarres(PdfDocument doc, List<PresseProduction> presses)
        {
            var refs = presses.GroupBy(p => p.Reference).Select(g => new RefAgregee
            {
                Label = g.Key.ToString("000000") + " — " +
                        string.Join("/", g.Select(x => x.AncienCode).Distinct()) +
                        "  [" + string.Join("+", g.Select(x => x.Machine).Distinct()) + "]" +
                        "   Obj.sem: " + g.Sum(x => x.ObjectifSemaine).ToString("0"),
                DayLabels = new[] {
                    g.First().LabelLundi, g.First().LabelMardi, g.First().LabelMercredi,
                    g.First().LabelJeudi, g.First().LabelVendredi, g.First().LabelSamedi, g.First().LabelDimanche
                },
                Prods = new[] {
                    g.Sum(x => x.ProdLundi),    g.Sum(x => x.ProdMardi),    g.Sum(x => x.ProdMercredi),
                    g.Sum(x => x.ProdJeudi),    g.Sum(x => x.ProdVendredi), g.Sum(x => x.ProdSamedi),
                    g.Sum(x => x.ProdDimanche)
                },
                ObjSemaine = g.Sum(x => x.ObjectifSemaine)
            }).ToList();

            int cols = refs.Count <= 2 ? refs.Count : refs.Count <= 4 ? 2 : 3;
            int rows = (int)Math.Ceiling(refs.Count / (double)cols);

            var page = NouvellePageA3(doc);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                DessinerEnTete(gfx, "Suivi Presse — Production journaliere VS Objectif", ColPresse);
                DessinerLegendeCouleurs(gfx, H - 18);

                double y0 = HeaderH + Marge;
                double cH = (H - y0 - Marge - 22) / rows;
                double cW = (W - 2 * Marge) / cols;

                for (int i = 0; i < refs.Count; i++)
                {
                    int col = i % cols, row = i / cols;
                    var r = refs[i];
                    var model = BuildBarresJournalieres(r.Label, r.DayLabels, r.Prods, r.ObjSemaine);
                    PlacerGraphique(gfx, model, Marge + col * cW, y0 + row * cH, cW - 8, cH - 8);
                }
                DessinerNumeroPage(gfx, doc.Pages.Count);
            }
        }

        // ===================================================================
        // PRESSE — CAMEMBERTS OBJECTIFS SEMAINE
        // ===================================================================

        private void PagePresseCamemberts(PdfDocument doc, List<PresseProduction> presses)
        {
            var refs = presses
                .GroupBy(p => p.Reference)
                .Select(g => new {
                    Titre     = g.Key.ToString("000000") + " - " + string.Join("/", g.Select(x => x.Machine).Distinct()),
                    TotalProd = g.Sum(x => x.TotalProduction),
                    TotalObj  = g.Max(x => x.ObjectifSemaine)
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
        // ASS AUTO — BARRES PAR REFERENCE
        // ===================================================================

        private void PageAssAutoBarres(PdfDocument doc, List<AssAutoProduction> assAutos)
        {
            var refs = assAutos.GroupBy(a => a.Reference).Select(g => new RefAgregee
            {
                Label = g.Key + " — " +
                        string.Join("/", g.Select(x => x.AncienCode).Distinct()) +
                        "  [" + string.Join("+", g.Select(x => x.Machine).Distinct()) + "]" +
                        "   Obj.sem: " + g.Sum(x => x.ObjectifSemaine).ToString("0"),
                DayLabels = new[] {
                    g.First().LabelLundi, g.First().LabelMardi, g.First().LabelMercredi,
                    g.First().LabelJeudi, g.First().LabelVendredi, g.First().LabelSamedi, g.First().LabelDimanche
                },
                Prods = new[] {
                    g.Sum(x => x.ProdLundi),    g.Sum(x => x.ProdMardi),    g.Sum(x => x.ProdMercredi),
                    g.Sum(x => x.ProdJeudi),    g.Sum(x => x.ProdVendredi), g.Sum(x => x.ProdSamedi),
                    g.Sum(x => x.ProdDimanche)
                },
                ObjSemaine = g.Sum(x => x.ObjectifSemaine)
            }).ToList();

            int cols = refs.Count <= 2 ? refs.Count : refs.Count <= 4 ? 2 : 3;
            int rows = (int)Math.Ceiling(refs.Count / (double)cols);

            var page = NouvellePageA3(doc);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                DessinerEnTete(gfx, "Assemblage Automatique — Production journaliere VS Objectif", ColAssAuto);
                DessinerLegendeCouleurs(gfx, H - 18);

                double y0 = HeaderH + Marge;
                double cH = (H - y0 - Marge - 22) / rows;
                double cW = (W - 2 * Marge) / cols;

                for (int i = 0; i < refs.Count; i++)
                {
                    int col = i % cols, row = i / cols;
                    var r = refs[i];
                    var model = BuildBarresJournalieres(r.Label, r.DayLabels, r.Prods, r.ObjSemaine);
                    PlacerGraphique(gfx, model, Marge + col * cW, y0 + row * cH, cW - 8, cH - 8);
                }
                DessinerNumeroPage(gfx, doc.Pages.Count);
            }
        }

        // ===================================================================
        // ASS AUTO — CAMEMBERTS
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

            if (refs.Count == 0) return;

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
        // METHODE GENERIQUE BARRES JOURNALIERES (Presse + AssAuto)
        // ===================================================================

        private PlotModel BuildBarresJournalieres(string titre, string[] jourLabels,
            double[] prods, double objSemaine)
        {
            var model = new PlotModel { Title = titre, Background = OxyColors.White };

            var axeX = new CategoryAxis { Position = AxisPosition.Bottom };
            foreach (var l in jourLabels) axeX.Labels.Add(l);
            var axeY = new LinearAxis { Position = AxisPosition.Left, Minimum = 0, Title = "Production" };

            var serie = new RectangleBarSeries { StrokeThickness = 0 };
            double lB = 0.38;
            double objJour = objSemaine > 0 ? objSemaine / 7.0 : 0;

            for (int j = 0; j < 7; j++)
            {
                var item = new RectangleBarItem(j - lB / 2, 0, j + lB / 2, prods[j]);
                item.Color = objJour > 0
                    ? (prods[j] >= objJour ? OxyColors.SeaGreen : OxyColors.IndianRed)
                    : OxyColor.FromRgb(100, 150, 220);
                serie.Items.Add(item);

                if (prods[j] > 0)
                    model.Annotations.Add(new OxyPlot.Annotations.TextAnnotation
                    {
                        Text = prods[j].ToString("0"),
                        TextPosition = new DataPoint(j, prods[j]),
                        FontSize = 7,
                        TextColor = OxyColors.Black,
                        StrokeThickness = 0,
                        Background = OxyColors.Transparent
                    });
            }

            if (objJour > 0)
                model.Annotations.Add(new OxyPlot.Annotations.LineAnnotation
                {
                    Type = OxyPlot.Annotations.LineAnnotationType.Horizontal,
                    Y = objJour,
                    MinimumX = -0.5,
                    MaximumX = 4.5,
                    Color = OxyColors.DarkOrange,
                    LineStyle = LineStyle.Dash,
                    StrokeThickness = 1.5,
                    Text = "Obj/j: " + objJour.ToString("0"),
                    TextColor = OxyColors.DarkOrange
                });

            model.Axes.Add(axeX);
            model.Axes.Add(axeY);
            model.Series.Add(serie);
            return model;
        }

        // ===================================================================
        // JOINTS / TRI — CONSTRUCTEURS DE LIGNES TABLE
        // ===================================================================

        private List<LigneTable> BuildLignesJoints(List<JointProduction> joints)
        {
            return joints.Select(jt => new LigneTable
            {
                Reference  = jt.Reference,
                AncienCode = jt.AncienCode,
                ObjSemaine = jt.ObjectifSemaine,
                DayLabels  = new[] {
                    jt.LabelLundi, jt.LabelMardi, jt.LabelMercredi,
                    jt.LabelJeudi, jt.LabelVendredi, jt.LabelSamedi, jt.LabelDimanche
                },
                DayEquipes = new double[][] {
                    new[] { jt.LundiEqu1,    jt.LundiEqu2,    jt.LundiEqu3    },
                    new[] { jt.MardiEqu1,    jt.MardiEqu2,    jt.MardiEqu3    },
                    new[] { jt.MercrediEqu1, jt.MercrediEqu2, jt.MercrediEqu3 },
                    new[] { jt.JeudiEqu1,    jt.JeudiEqu2,    jt.JeudiEqu3    },
                    new[] { jt.VendrediEqu1, jt.VendrediEqu2, jt.VendrediEqu3 },
                    new[] { jt.ProdSamedi,   0.0,             0.0             },
                    new[] { jt.ProdDimanche, 0.0,             0.0             },
                    new[] {
                        jt.LundiEqu1+jt.MardiEqu1+jt.MercrediEqu1+jt.JeudiEqu1+jt.VendrediEqu1,
                        jt.LundiEqu2+jt.MardiEqu2+jt.MercrediEqu2+jt.JeudiEqu2+jt.VendrediEqu2,
                        jt.LundiEqu3+jt.MardiEqu3+jt.MercrediEqu3+jt.JeudiEqu3+jt.VendrediEqu3
                    }
                }
            }).ToList();
        }

        private List<LigneTable> BuildLignesTri(List<TriProduction> tris)
        {
            return tris.Select(tr => new LigneTable
            {
                Reference  = tr.Reference,
                AncienCode = tr.AncienCode,
                ObjSemaine = tr.ObjectifSemaine,
                DayLabels  = new[] {
                    tr.LabelLundi, tr.LabelMardi, tr.LabelMercredi,
                    tr.LabelJeudi, tr.LabelVendredi, tr.LabelSamedi, tr.LabelDimanche
                },
                DayEquipes = new double[][] {
                    new[] { tr.LundiEqu1,    tr.LundiEqu2,    tr.LundiEqu3    },
                    new[] { tr.MardiEqu1,    tr.MardiEqu2,    tr.MardiEqu3    },
                    new[] { tr.MercrediEqu1, tr.MercrediEqu2, tr.MercrediEqu3 },
                    new[] { tr.JeudiEqu1,    tr.JeudiEqu2,    tr.JeudiEqu3    },
                    new[] { tr.VendrediEqu1, tr.VendrediEqu2, tr.VendrediEqu3 },
                    new[] { tr.ProdSamedi,   0.0,             0.0             },
                    new[] { tr.ProdDimanche, 0.0,             0.0             },
                    new[] {
                        tr.LundiEqu1+tr.MardiEqu1+tr.MercrediEqu1+tr.JeudiEqu1+tr.VendrediEqu1,
                        tr.LundiEqu2+tr.MardiEqu2+tr.MercrediEqu2+tr.JeudiEqu2+tr.VendrediEqu2,
                        tr.LundiEqu3+tr.MardiEqu3+tr.MercrediEqu3+tr.JeudiEqu3+tr.VendrediEqu3
                    }
                }
            }).ToList();
        }

        // ===================================================================
        // JOINTS / TRI — TABLEAU VISUEL JOURNALIER PAR EQUIPE
        // ===================================================================

        private void PageTableEquipes(PdfDocument doc, string titre, XColor couleur,
            List<LigneTable> lignes)
        {
            if (lignes.Count == 0) return;

            var page = NouvellePageA3(doc);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                DessinerEnTete(gfx, titre, couleur);

                double x0        = Marge;
                double y0        = HeaderH + 4;
                double tableW    = W - 2 * Marge;
                double colRefW   = 108;
                double colDataW  = (tableW - colRefW) / 8.0;
                double headerRowH = 26;
                double dataRowH  = (H - y0 - Marge - headerRowH) / lignes.Count;

                // En-tete colonnes
                XColor hBg = XColor.FromArgb(
                    (int)(couleur.R * 0.78), (int)(couleur.G * 0.78), (int)(couleur.B * 0.78));
                XColor hBgTotal = XColor.FromArgb(
                    (int)(couleur.R * 0.65), (int)(couleur.G * 0.65), (int)(couleur.B * 0.65));

                DessinerCellule(gfx, x0, y0, colRefW, headerRowH,
                    "Ref / Ancien code / Objectif", FTiny, new XSolidBrush(hBg), true);

                string[] dayHeaders = lignes[0].DayLabels.Concat(new[] { "TOTAL" }).ToArray();
                for (int d = 0; d < 8; d++)
                {
                    XColor bg = d == 7 ? hBgTotal : hBg;
                    DessinerCellule(gfx, x0 + colRefW + d * colDataW, y0, colDataW, headerRowH,
                        dayHeaders[d], FTiny, new XSolidBrush(bg), true);
                }

                // Max valeur journaliere pour echelle barres
                double maxVal = 1;
                foreach (var l in lignes)
                    for (int d = 0; d < 7; d++)
                        foreach (var v in l.DayEquipes[d])
                            if (v > maxVal) maxVal = v;

                // Lignes de donnees
                for (int r = 0; r < lignes.Count; r++)
                {
                    var ligne = lignes[r];
                    double ry = y0 + headerRowH + r * dataRowH;

                    // Cellule reference
                    XColor refBg = XColor.FromArgb(
                        Math.Min(255, (int)(couleur.R * 0.94) + 8),
                        Math.Min(255, (int)(couleur.G * 0.94) + 8),
                        Math.Min(255, (int)(couleur.B * 0.94) + 8));
                    gfx.DrawRectangle(new XSolidBrush(refBg), x0, ry, colRefW, dataRowH);
                    gfx.DrawRectangle(new XPen(XColor.FromArgb(155, 155, 155), 0.5), x0, ry, colRefW, dataRowH);

                    double refLineH = dataRowH / 3.0;
                    string[] refLines = {
                        ligne.Reference,
                        ligne.AncienCode,
                        "Obj: " + ligne.ObjSemaine.ToString("0")
                    };
                    for (int rl = 0; rl < 3; rl++)
                        gfx.DrawString(refLines[rl], rl == 0 ? FBold : FTiny, XBrushes.Black,
                            new XRect(x0 + 3, ry + rl * refLineH, colRefW - 6, refLineH),
                            XStringFormats.CenterLeft);

                    // Cellules journalieres
                    for (int d = 0; d < 8; d++)
                    {
                        double cx = x0 + colRefW + d * colDataW;
                        bool isTotal   = d == 7;
                        bool isWeekend = d == 5 || d == 6;
                        DessinerCelluleEquipes(gfx, cx, ry, colDataW, dataRowH,
                            ligne.DayEquipes[d], maxVal, isTotal, isWeekend);
                    }
                }

                DessinerNumeroPage(gfx, doc.Pages.Count);
            }
        }

        private void DessinerCelluleEquipes(XGraphics gfx, double x, double y, double w, double h,
            double[] equipes, double maxVal, bool isTotal, bool isWeekend)
        {
            XColor bg = isTotal  ? XColor.FromArgb(210, 230, 210)
                      : isWeekend ? XColor.FromArgb(238, 238, 238)
                      : ColBlanc;
            gfx.DrawRectangle(new XSolidBrush(bg), x, y, w, h);
            gfx.DrawRectangle(new XPen(XColor.FromArgb(175, 175, 175), 0.4), x, y, w, h);

            double total = equipes[0] + equipes[1] + equipes[2];
            if (total <= 0)
            {
                gfx.DrawString("—", FTiny, XBrushes.LightGray,
                    new XRect(x, y, w, h), XStringFormats.Center);
                return;
            }

            XColor[] equColors = {
                XColor.FromArgb(0, 105, 0),
                XColor.FromArgb(45, 170, 45),
                XColor.FromArgb(135, 210, 135)
            };
            string[] equLabels = { "EQ1", "EQ2", "EQ3" };

            double labelW  = 20;
            double valW    = 36;
            double barMaxW = w - labelW - valW - 6;
            if (barMaxW < 4) barMaxW = 4;

            // Nombre de lignes a afficher
            int shown = 0;
            bool[] show = new bool[3];
            if (isWeekend)
            {
                show[0] = equipes[0] > 0;
                shown = show[0] ? 1 : 0;
            }
            else
            {
                for (int e = 0; e < 3; e++) { show[e] = equipes[e] > 0; if (show[e]) shown++; }
            }
            if (shown == 0) return;

            double subH = h / shown;
            int rowIdx = 0;

            for (int e = 0; e < 3; e++)
            {
                if (!show[e]) continue;
                double subY = y + rowIdx * subH;
                rowIdx++;

                // Label equipe
                string lbl = isWeekend ? "Prod" : equLabels[e];
                gfx.DrawString(lbl, FTiny, new XSolidBrush(XColor.FromArgb(70, 70, 70)),
                    new XRect(x + 2, subY, labelW, subH), XStringFormats.CenterLeft);

                // Mini barre coloree
                double barW = Math.Min(barMaxW, Math.Max(2, equipes[e] / maxVal * barMaxW));
                gfx.DrawRectangle(new XSolidBrush(equColors[e]),
                    x + labelW + 2, subY + 2, barW, subH - 4);

                // Valeur numerique
                gfx.DrawString(equipes[e].ToString("0"), FTiny, XBrushes.Black,
                    new XRect(x + labelW + barMaxW + 4, subY, valW, subH),
                    XStringFormats.CenterLeft);
            }
        }

        // ===================================================================
        // ASSEMBLAGE MANUEL — BARRES + CAMEMBERTS
        // ===================================================================

        private void PageAssManuelBarres(PdfDocument doc, List<AssManuelProduction> assManuels)
        {
            var references = assManuels.Select(x => x.Reference).Distinct().ToList();
            int cols = references.Count == 1 ? 1 : references.Count <= 4 ? 2 : 3;
            int rows = (int)Math.Ceiling(references.Count / (double)cols);

            var page = NouvellePageA3(doc);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                DessinerEnTete(gfx, "Assemblage Manuel — Production journaliere par operation (Capuchon / Insert)", ColAssManuel);

                double y0 = HeaderH + Marge;
                double cH = (H - y0 - Marge) / rows;
                double cW = (W - 2 * Marge) / cols;

                for (int i = 0; i < references.Count; i++)
                {
                    string reference = references[i];
                    int col = i % cols, row = i / cols;

                    var capuchon = assManuels.FirstOrDefault(x =>
                        x.Reference == reference && x.Operation.ToLower().Contains("capuchon"));
                    var insert = assManuels.FirstOrDefault(x =>
                        x.Reference == reference && x.Operation.ToLower().Contains("insert"));

                    if (capuchon == null && insert == null) continue;

                    AssManuelProduction refObj = capuchon ?? insert;
                    string[] jourLabels = {
                        refObj.LabelLundi, refObj.LabelMardi, refObj.LabelMercredi,
                        refObj.LabelJeudi, refObj.LabelVendredi, refObj.LabelSamedi, refObj.LabelDimanche
                    };

                    double objJour = (capuchon?.ObjectifSemaine ?? insert.ObjectifSemaine) / 5.0;
                    double[] prodCap = ProdJoursAssManu(capuchon);
                    double[] prodIns = ProdJoursAssManu(insert);

                    var model = BuildBarresAssManuel(reference, jourLabels, prodCap, prodIns, objJour);
                    PlacerGraphique(gfx, model, Marge + col * cW, y0 + row * cH, cW - 8, cH - 8);
                }
                DessinerNumeroPage(gfx, doc.Pages.Count);
            }
        }

        private void PageAssManuelCamemberts(PdfDocument doc, List<AssManuelProduction> assManuels)
        {
            var operations = assManuels.OrderBy(x => x.Reference).ThenBy(x => x.Operation).ToList();
            if (operations.Count == 0) return;

            int cols = operations.Count <= 3 ? operations.Count : operations.Count <= 6 ? 3 : 4;
            int rows = (int)Math.Ceiling(operations.Count / (double)cols);

            var page = NouvellePageA3(doc);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                DessinerEnTete(gfx, "Assemblage Manuel — Repartition par equipe (EQU1/EQU2/EQU3)", ColAssManuel);

                double y0 = HeaderH + Marge;
                double cH = (H - y0 - Marge) / rows;
                double cW = (W - 2 * Marge) / cols;

                for (int i = 0; i < operations.Count; i++)
                {
                    var op = operations[i];
                    int col = i % cols, row = i / cols;

                    double equ1 = op.LundiEqu1 + op.MardiEqu1 + op.MercrediEqu1 + op.JeudiEqu1 + op.VendrediEqu1;
                    double equ2 = op.LundiEqu2 + op.MardiEqu2 + op.MercrediEqu2 + op.JeudiEqu2 + op.VendrediEqu2;
                    double equ3 = op.LundiEqu3 + op.MardiEqu3 + op.MercrediEqu3 + op.JeudiEqu3 + op.VendrediEqu3;
                    string titre = op.Reference + " - " + op.Operation;

                    var model = BuildCamembertEquipes(titre, equ1, equ2, equ3, op.ObjectifSemaine);
                    PlacerGraphique(gfx, model, Marge + col * cW, y0 + row * cH, cW - 8, cH - 8);
                }
                DessinerNumeroPage(gfx, doc.Pages.Count);
            }
        }

        private PlotModel BuildBarresAssManuel(string reference, string[] jourLabels,
            double[] prodCap, double[] prodIns, double objJour)
        {
            var model = new PlotModel { Title = reference, Background = OxyColors.White };

            var axeX = new CategoryAxis { Position = AxisPosition.Bottom };
            foreach (var l in jourLabels) axeX.Labels.Add(l);
            var axeY = new LinearAxis { Position = AxisPosition.Left, Minimum = 0, Title = "Production" };

            var serieCap = new RectangleBarSeries
            {
                Title = "Capuchon", FillColor = OxyColor.FromRgb(40, 120, 200), StrokeThickness = 0
            };
            var serieIns = new RectangleBarSeries
            {
                Title = "Insert", FillColor = OxyColor.FromRgb(200, 100, 40), StrokeThickness = 0
            };

            double lB = 0.23, gap = 0.03;
            for (int j = 0; j < 7; j++)
            {
                serieCap.Items.Add(new RectangleBarItem(j - lB - gap / 2, 0, j - gap / 2, prodCap[j]));
                serieIns.Items.Add(new RectangleBarItem(j + gap / 2, 0, j + lB + gap / 2, prodIns[j]));
            }

            if (objJour > 0)
                model.Annotations.Add(new OxyPlot.Annotations.LineAnnotation
                {
                    Type = OxyPlot.Annotations.LineAnnotationType.Horizontal,
                    Y = objJour, MinimumX = -0.5, MaximumX = 4.5,
                    Color = OxyColors.DarkOrange, LineStyle = LineStyle.Dash, StrokeThickness = 2,
                    Text = "Obj/j: " + objJour.ToString("0"), TextColor = OxyColors.DarkOrange
                });

            model.Axes.Add(axeX);
            model.Axes.Add(axeY);
            model.Series.Add(serieCap);
            model.Series.Add(serieIns);
            return model;
        }

        // ===================================================================
        // CAMEMBERT GENERIQUE (Presse / AssAuto)
        // ===================================================================

        private PlotModel BuildCamembert(string titre, double prod, double obj)
        {
            double reste  = Math.Max(0, obj - prod);
            bool   atteint = prod >= obj;

            var model = new PlotModel { Title = titre, Background = OxyColors.White };
            var serie = new PieSeries
            {
                StrokeThickness = 0, InsideLabelPosition = 0.68,
                InsideLabelFormat = "{2:0}%", OutsideLabelFormat = ""
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

        private PlotModel BuildCamembertEquipes(string titre,
            double equ1, double equ2, double equ3, double objectifSemaine)
        {
            double totalProd = equ1 + equ2 + equ3;
            double reste     = Math.Max(0, objectifSemaine - totalProd);
            double pct       = objectifSemaine > 0 ? totalProd / objectifSemaine * 100 : 0;

            var model = new PlotModel
            {
                Title = titre + "\n" + pct.ToString("0") + "% — " +
                        totalProd.ToString("0") + " / " + objectifSemaine.ToString("0"),
                Background = OxyColors.White
            };
            var serie = new PieSeries
            {
                StrokeThickness = 0.5, InsideLabelPosition = 0.65,
                InsideLabelFormat = "{2:0}%", OutsideLabelFormat = ""
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
        // LEGENDE + GETTERS PRODUCTION
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

        private double[] ProdJoursAssManu(AssManuelProduction op)
        {
            if (op == null) return new double[7];
            return new double[] {
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
