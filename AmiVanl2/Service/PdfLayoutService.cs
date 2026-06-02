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
        private readonly XFont FSmall = new XFont("Arial",  9, XFontStyleEx.Regular);
        private readonly XFont FTiny  = new XFont("Arial",  8, XFontStyleEx.Regular);

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
                PagePresseComplet(doc, presses);
                var commentsPresse = presses
                    .GroupBy(p => p.Reference)
                    .Where(g => g.Any(p => !string.IsNullOrWhiteSpace(p.Commentaire)))
                    .Select(g => (g.Key, g.First(p => !string.IsNullOrWhiteSpace(p.Commentaire)).Commentaire))
                    .ToList();
                if (commentsPresse.Count > 0)
                    PageCommentaires(doc, "Presse — Commentaires de la semaine", ColPresse, commentsPresse);
            }

            if (assAutos.Count > 0)
            {
                PageSeparateur(doc, "ASSEMBLAGE AUTOMATIQUE", ColAssAuto);
                PageAssAutoComplet(doc, assAutos);
                var commentsAssAuto = assAutos
                    .GroupBy(a => a.Reference)
                    .Where(g => g.Any(a => !string.IsNullOrWhiteSpace(a.Commentaire)))
                    .Select(g => (g.Key, g.First(a => !string.IsNullOrWhiteSpace(a.Commentaire)).Commentaire))
                    .ToList();
                if (commentsAssAuto.Count > 0)
                    PageCommentaires(doc, "Assemblage Automatique — Commentaires de la semaine", ColAssAuto, commentsAssAuto);
            }

            if (joints.Count > 0)
            {
                PageSeparateur(doc, "JOINTS", ColJoints);
                PageTableEquipes(doc,
                    "Joints — Suivi journalier par equipe et par reference",
                    ColJoints, BuildLignesJoints(joints));
                var commentsJoints = joints
                    .Where(j => !string.IsNullOrWhiteSpace(j.Commentaire))
                    .Select(j => (j.Reference, j.Commentaire))
                    .ToList();
                if (commentsJoints.Count > 0)
                    PageCommentaires(doc, "Joints — Commentaires de la semaine", ColJoints, commentsJoints);
            }

            if (tris.Count > 0)
            {
                PageSeparateur(doc, "TRI", ColTri);
                PageTableEquipes(doc,
                    "Tri — Suivi journalier par equipe et par reference",
                    ColTri, BuildLignesTri(tris));
                var commentsTri = tris
                    .Where(t => !string.IsNullOrWhiteSpace(t.Commentaire))
                    .Select(t => (t.Reference, t.Commentaire))
                    .ToList();
                if (commentsTri.Count > 0)
                    PageCommentaires(doc, "Tri — Commentaires de la semaine", ColTri, commentsTri);
            }

            if (assManuels.Count > 0)
            {
                PageSeparateur(doc, "ASSEMBLAGE MANUEL", ColAssManuel);
                PageAssManuelTable(doc, assManuels);
                var commentsAssManu = assManuels
                    .GroupBy(m => m.Reference)
                    .Where(g => g.Any(m => !string.IsNullOrWhiteSpace(m.Commentaire)))
                    .Select(g => (g.Key, g.First(m => !string.IsNullOrWhiteSpace(m.Commentaire)).Commentaire))
                    .ToList();
                if (commentsAssManu.Count > 0)
                    PageCommentaires(doc, "Assemblage Manuel — Commentaires de la semaine", ColAssManuel, commentsAssManu);
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
        // PRESSE — PAGE COMPLETE (barres + camembert par ref sur 1 page)
        // ===================================================================

        private void PagePresseComplet(PdfDocument doc, List<PresseProduction> presses)
        {
            var refs = presses.GroupBy(p => p.Reference).Select(g => new {
                BarLabel = g.Key.ToString("000000") + " — " +
                           string.Join("/", g.Select(x => x.AncienCode).Distinct()) +
                           "  [" + string.Join("+", g.Select(x => x.Machine).Distinct()) + "]" +
                           "   Obj.sem: " + g.Sum(x => x.ObjectifSemaine).ToString("0"),
                CamTitre = g.Key.ToString("000000") + " - " +
                           string.Join("/", g.Select(x => x.AncienCode).Distinct()),
                DayLabels = new[] {
                    g.First().LabelLundi, g.First().LabelMardi, g.First().LabelMercredi,
                    g.First().LabelJeudi, g.First().LabelVendredi, g.First().LabelSamedi, g.First().LabelDimanche
                },
                Prods = new[] {
                    g.Sum(x => x.ProdLundi),    g.Sum(x => x.ProdMardi),    g.Sum(x => x.ProdMercredi),
                    g.Sum(x => x.ProdJeudi),    g.Sum(x => x.ProdVendredi), g.Sum(x => x.ProdSamedi),
                    g.Sum(x => x.ProdDimanche)
                },
                ObjSemaine = g.Sum(x => x.ObjectifSemaine),
                TotalProd  = g.Sum(x => x.TotalProduction)
            }).ToList();

            var page = NouvellePageA3(doc);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                DessinerEnTete(gfx, "Suivi Presse — Production journaliere VS Objectif semaine", ColPresse);
                DessinerLegendeCouleurs(gfx, H - 18);

                double y0   = HeaderH + Marge;
                double rowH = (H - y0 - Marge - 22) / refs.Count;
                double barW = (W - 2 * Marge) * 0.72;
                double camW = (W - 2 * Marge) * 0.28;

                for (int i = 0; i < refs.Count; i++)
                {
                    var r  = refs[i];
                    double ry = y0 + i * rowH;

                    var barModel = BuildBarresJournalieres(r.BarLabel, r.DayLabels, r.Prods, r.ObjSemaine);
                    PlacerGraphique(gfx, barModel, Marge, ry, barW - 6, rowH - 8);

                    var camModel = BuildCamembert(r.CamTitre, r.TotalProd, r.ObjSemaine);
                    PlacerGraphique(gfx, camModel, Marge + barW, ry, camW - 4, rowH - 8);
                }
                DessinerNumeroPage(gfx, doc.Pages.Count);
            }
        }

        // ===================================================================
        // ASS AUTO — PAGE COMPLETE (barres + camembert par ref sur 1 page)
        // ===================================================================

        private void PageAssAutoComplet(PdfDocument doc, List<AssAutoProduction> assAutos)
        {
            var refs = assAutos.GroupBy(a => a.Reference).Select(g => new {
                BarLabel = g.Key + " — " +
                           string.Join("/", g.Select(x => x.AncienCode).Distinct()) +
                           "  [" + string.Join("+", g.Select(x => x.Machine).Distinct()) + "]" +
                           "   Obj.sem: " + g.Sum(x => x.ObjectifSemaine).ToString("0"),
                CamTitre = g.Key + " - " +
                           string.Join("/", g.Select(x => x.AncienCode).Distinct()),
                DayLabels = new[] {
                    g.First().LabelLundi, g.First().LabelMardi, g.First().LabelMercredi,
                    g.First().LabelJeudi, g.First().LabelVendredi, g.First().LabelSamedi, g.First().LabelDimanche
                },
                Prods = new[] {
                    g.Sum(x => x.ProdLundi),    g.Sum(x => x.ProdMardi),    g.Sum(x => x.ProdMercredi),
                    g.Sum(x => x.ProdJeudi),    g.Sum(x => x.ProdVendredi), g.Sum(x => x.ProdSamedi),
                    g.Sum(x => x.ProdDimanche)
                },
                ObjSemaine = g.Sum(x => x.ObjectifSemaine),
                TotalProd  = g.Sum(x => x.TotalProduction)
            }).ToList();

            var page = NouvellePageA3(doc);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                DessinerEnTete(gfx, "Assemblage Automatique — Production journaliere VS Objectif semaine", ColAssAuto);
                DessinerLegendeCouleurs(gfx, H - 18);

                double y0   = HeaderH + Marge;
                double rowH = (H - y0 - Marge - 22) / refs.Count;
                double barW = (W - 2 * Marge) * 0.72;
                double camW = (W - 2 * Marge) * 0.28;

                for (int i = 0; i < refs.Count; i++)
                {
                    var r  = refs[i];
                    double ry = y0 + i * rowH;

                    var barModel = BuildBarresJournalieres(r.BarLabel, r.DayLabels, r.Prods, r.ObjSemaine);
                    PlacerGraphique(gfx, barModel, Marge, ry, barW - 6, rowH - 8);

                    var camModel = BuildCamembert(r.CamTitre, r.TotalProd, r.ObjSemaine);
                    PlacerGraphique(gfx, camModel, Marge + barW, ry, camW - 4, rowH - 8);
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

            double objJour   = objSemaine > 0 ? objSemaine / 7.0 : 0;
            double maxProd   = prods.Length > 0 ? prods.Max() : 0;
            double yMax      = objJour > 0 ? Math.Max(maxProd, objJour) * 1.12 : (maxProd > 0 ? maxProd * 1.1 : 1);
            var axeY = new LinearAxis { Position = AxisPosition.Left, Minimum = 0, Maximum = yMax, Title = "Production" };

            var serie = new RectangleBarSeries { StrokeThickness = 0 };
            double lB = 0.38;

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
                        FontSize = 10,
                        FontWeight = OxyPlot.FontWeights.Bold,
                        TextColor = OxyColors.Black,
                        StrokeThickness = 0,
                        Background = OxyColors.Transparent
                    });
            }

            if (objJour > 0)
            {
                // Ligne pleine epaisse pour bien voir l'objectif
                model.Annotations.Add(new OxyPlot.Annotations.LineAnnotation
                {
                    Type = OxyPlot.Annotations.LineAnnotationType.Horizontal,
                    Y = objJour,
                    MinimumX = -0.5,
                    MaximumX = 6.5,
                    Color = OxyColor.FromRgb(220, 80, 0),
                    LineStyle = LineStyle.Solid,
                    StrokeThickness = 3.0,
                    Text = "Obj/j: " + objJour.ToString("0"),
                    TextColor = OxyColor.FromRgb(180, 50, 0),
                    FontSize = 11,
                    FontWeight = OxyPlot.FontWeights.Bold
                });
            }

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

                double x0         = Marge;
                double y0         = HeaderH + 4;
                double tableW     = W - 2 * Marge;
                double colRefW    = 108;
                // Colonne TOTAL plus large pour les labels camembert
                double colTotalW  = (tableW - colRefW) * 0.20;
                double colDayW    = (tableW - colRefW - colTotalW) / 7.0;
                double headerRowH = 26;
                double dataRowH   = (H - y0 - Marge - headerRowH) / lignes.Count;

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
                    double cw  = d == 7 ? colTotalW : colDayW;
                    double cx0 = x0 + colRefW + (d < 7 ? d * colDayW : 7 * colDayW);
                    XColor bg  = d == 7 ? hBgTotal : hBg;
                    DessinerCellule(gfx, cx0, y0, cw, headerRowH,
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
                        double cw = d == 7 ? colTotalW : colDayW;
                        double cx = x0 + colRefW + (d < 7 ? d * colDayW : 7 * colDayW);
                        DessinerCelluleEquipes(gfx, cx, ry, cw, dataRowH,
                            ligne.DayEquipes[d], maxVal, d == 7, d == 5 || d == 6, ligne.ObjSemaine);
                    }
                }

                DessinerNumeroPage(gfx, doc.Pages.Count);
            }
        }

        private void DessinerCelluleEquipes(XGraphics gfx, double x, double y, double w, double h,
            double[] equipes, double maxVal, bool isTotal, bool isWeekend, double objSemaine)
        {
            // Colonne TOTAL : camembert production globale vs objectif semaine
            if (isTotal)
            {
                double totalProd = equipes[0] + equipes[1] + equipes[2];
                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(240, 240, 240)), x, y, w, h);
                gfx.DrawRectangle(new XPen(XColor.FromArgb(155, 155, 155), 0.6), x, y, w, h);
                if (objSemaine > 0)
                {
                    var camModel = BuildCamembert("", totalProd, objSemaine);
                    PlacerGraphique(gfx, camModel, x + 2, y + 2, w - 4, h - 4);
                }
                else if (totalProd > 0)
                {
                    gfx.DrawString(totalProd.ToString("0"), FSmall, XBrushes.Black,
                        new XRect(x, y, w, h), XStringFormats.Center);
                }
                return;
            }

            XColor bg = isWeekend ? XColor.FromArgb(238, 238, 238) : ColBlanc;
            gfx.DrawRectangle(new XSolidBrush(bg), x, y, w, h);
            gfx.DrawRectangle(new XPen(XColor.FromArgb(175, 175, 175), 0.4), x, y, w, h);

            double total = equipes[0] + equipes[1] + equipes[2];
            if (total <= 0)
            {
                gfx.DrawString("—", FTiny, XBrushes.LightGray,
                    new XRect(x, y, w, h), XStringFormats.Center);
                return;
            }

            // Obj journalier par equipe = ObjSemaine / (5 jours * 3 equipes)
            double objEquipeJour = (objSemaine > 0 && !isWeekend) ? objSemaine / 15.0 : 0;

            // Couleur vert/rouge selon objectif atteint ou non (gris si weekend sans obj)
            XColor[] equColors = new XColor[3];
            for (int e = 0; e < 3; e++)
            {
                equColors[e] = isWeekend
                    ? XColor.FromArgb(100, 150, 220)
                    : objEquipeJour > 0
                        ? (equipes[e] >= objEquipeJour ? XColor.FromArgb(40, 160, 80) : XColor.FromArgb(210, 60, 60))
                        : XColor.FromArgb(100, 150, 220);
            }

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
        // ASSEMBLAGE MANUEL — TABLEAU PAR EQUIPE ET PAR OPERATION
        // ===================================================================

        private void PageAssManuelTable(PdfDocument doc, List<AssManuelProduction> assManuels)
        {
            if (assManuels.Count == 0) return;

            // Groupe par reference, puis tri par operation dans chaque groupe
            var refs = assManuels
                .GroupBy(m => m.Reference)
                .Select(g => g.OrderBy(m => m.Operation ?? "").ToList())
                .ToList();

            int totalRows = refs.Sum(g => g.Count);

            var page = NouvellePageA3(doc);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                DessinerEnTete(gfx, "Assemblage Manuel — Suivi journalier par equipe et par operation", ColAssManuel);

                double x0         = Marge;
                double y0         = HeaderH + 4;
                double tableW     = W - 2 * Marge;
                double colRefW    = 105;
                // Colonne TOTAL plus large pour que les labels exterieurs soient visibles
                double colTotalW  = (tableW - colRefW) * 0.22;
                double colDayW    = (tableW - colRefW - colTotalW) / 7.0;
                double headerRowH = 26;
                double dataRowH   = (H - y0 - Marge - headerRowH) / totalRows;

                // En-tete colonnes
                XColor hBg = XColor.FromArgb(
                    (int)(ColAssManuel.R * 0.78), (int)(ColAssManuel.G * 0.78), (int)(ColAssManuel.B * 0.78));
                XColor hBgTotal = XColor.FromArgb(
                    (int)(ColAssManuel.R * 0.65), (int)(ColAssManuel.G * 0.65), (int)(ColAssManuel.B * 0.65));

                DessinerCellule(gfx, x0, y0, colRefW, headerRowH,
                    "Ref / Op / Objectif", FTiny, new XSolidBrush(hBg), true);

                var firstOp = assManuels.First();
                string[] dayHeaders = new[] {
                    firstOp.LabelLundi, firstOp.LabelMardi, firstOp.LabelMercredi,
                    firstOp.LabelJeudi, firstOp.LabelVendredi, firstOp.LabelSamedi,
                    firstOp.LabelDimanche, "TOTAL"
                };
                for (int d = 0; d < 8; d++)
                {
                    double cw  = d == 7 ? colTotalW : colDayW;
                    double cx0 = x0 + colRefW + (d < 7 ? d * colDayW : 7 * colDayW);
                    XColor bg  = d == 7 ? hBgTotal : hBg;
                    DessinerCellule(gfx, cx0, y0, cw, headerRowH,
                        dayHeaders[d], FTiny, new XSolidBrush(bg), true);
                }

                // Echelle barres : max equipe PAR REFERENCE (evite que les petites refs soient ecrasees)
                // On calcule un maxVal par groupe, stocke dans un dictionnaire indexe sur refs
                var maxValParRef = refs.Select(grp => {
                    double m = 1;
                    foreach (var op in grp)
                    {
                        double[] vals = {
                            op.LundiEqu1, op.LundiEqu2, op.LundiEqu3,
                            op.MardiEqu1, op.MardiEqu2, op.MardiEqu3,
                            op.MercrediEqu1, op.MercrediEqu2, op.MercrediEqu3,
                            op.JeudiEqu1, op.JeudiEqu2, op.JeudiEqu3,
                            op.VendrediEqu1, op.VendrediEqu2, op.VendrediEqu3
                        };
                        foreach (var v in vals) if (v > m) m = v;
                    }
                    return m;
                }).ToList();

                XColor refBg = XColor.FromArgb(
                    Math.Min(255, (int)(ColAssManuel.R * 0.94) + 8),
                    Math.Min(255, (int)(ColAssManuel.G * 0.94) + 8),
                    Math.Min(255, (int)(ColAssManuel.B * 0.94) + 8));

                int rowIdx = 0;
                int refIdx = 0;
                foreach (var ops in refs)
                {
                    double maxVal = maxValParRef[refIdx++];
                    int opCount = ops.Count;
                    double refH = opCount * dataRowH;
                    double ry   = y0 + headerRowH + rowIdx * dataRowH;

                    // Cellule reference (fusion des lignes operations)
                    gfx.DrawRectangle(new XSolidBrush(refBg), x0, ry, colRefW, refH);
                    gfx.DrawRectangle(new XPen(XColor.FromArgb(140, 140, 140), 0.6), x0, ry, colRefW, refH);

                    // Ref + AncienCode en haut
                    var op0 = ops[0];
                    gfx.DrawString(op0.Reference, FBold, XBrushes.Black,
                        new XRect(x0 + 3, ry + 2, colRefW - 6, dataRowH * 0.45), XStringFormats.CenterLeft);

                    // Lignes par operation
                    for (int opIdx = 0; opIdx < opCount; opIdx++)
                    {
                        var op  = ops[opIdx];
                        double opY = ry + opIdx * dataRowH;

                        // Nom operation dans la partie basse de la cellule ref
                        string opLabel = string.IsNullOrWhiteSpace(op.Operation)
                            ? "Op " + (opIdx + 1)
                            : op.Operation;
                        gfx.DrawString(opLabel + "  Obj: " + op.ObjectifSemaine.ToString("0"),
                            FTiny, new XSolidBrush(XColor.FromArgb(100, 60, 120)),
                            new XRect(x0 + 3, opY + dataRowH * 0.72, colRefW - 6, dataRowH * 0.28),
                            XStringFormats.CenterLeft);

                        // Separateur entre operations
                        if (opIdx > 0)
                            gfx.DrawLine(new XPen(XColor.FromArgb(130, 130, 130), 0.4),
                                x0 + colRefW, opY, x0 + colRefW + 7 * colDayW + colTotalW, opY);

                        // Construire DayEquipes[8][3]
                        double[][] dayEquipes = {
                            new[] { op.LundiEqu1,    op.LundiEqu2,    op.LundiEqu3    },
                            new[] { op.MardiEqu1,    op.MardiEqu2,    op.MardiEqu3    },
                            new[] { op.MercrediEqu1, op.MercrediEqu2, op.MercrediEqu3 },
                            new[] { op.JeudiEqu1,    op.JeudiEqu2,    op.JeudiEqu3    },
                            new[] { op.VendrediEqu1, op.VendrediEqu2, op.VendrediEqu3 },
                            new[] { op.ProdSamedi,   0.0,             0.0             },
                            new[] { op.ProdDimanche, 0.0,             0.0             },
                            new[] {
                                op.LundiEqu1+op.MardiEqu1+op.MercrediEqu1+op.JeudiEqu1+op.VendrediEqu1,
                                op.LundiEqu2+op.MardiEqu2+op.MercrediEqu2+op.JeudiEqu2+op.VendrediEqu2,
                                op.LundiEqu3+op.MardiEqu3+op.MercrediEqu3+op.JeudiEqu3+op.VendrediEqu3
                            }
                        };

                        // Si cellule Excel fusionnee, l'Insert n'a pas d'objectif propre : emprunter celui du groupe
                        double opObj = op.ObjectifSemaine > 0 ? op.ObjectifSemaine : ops[0].ObjectifSemaine;

                        // Corriger le label si objectif emprunte
                        if (op.ObjectifSemaine == 0 && opObj > 0)
                        {
                            // Redessiner le label avec le bon objectif (par dessus l'ancien)
                            gfx.DrawRectangle(new XSolidBrush(refBg),
                                x0, opY + dataRowH * 0.72, colRefW, dataRowH * 0.28);
                            gfx.DrawString(opLabel + "  Obj: " + opObj.ToString("0"),
                                FTiny, new XSolidBrush(XColor.FromArgb(100, 60, 120)),
                                new XRect(x0 + 3, opY + dataRowH * 0.72, colRefW - 6, dataRowH * 0.28),
                                XStringFormats.CenterLeft);
                        }

                        for (int d = 0; d < 8; d++)
                        {
                            double cw = d == 7 ? colTotalW : colDayW;
                            double cx = x0 + colRefW + (d < 7 ? d * colDayW : 7 * colDayW);
                            DessinerCelluleEquipes(gfx, cx, opY, cw, dataRowH,
                                dayEquipes[d], maxVal,
                                isTotal: d == 7,
                                isWeekend: d == 5 || d == 6,
                                objSemaine: opObj);
                        }
                    }

                    // Bordure exterieure du groupe reference
                    gfx.DrawRectangle(new XPen(XColor.FromArgb(110, 80, 130), 1.0), x0, ry, tableW, refH);

                    rowIdx += opCount;
                }

                DessinerNumeroPage(gfx, doc.Pages.Count);
            }
        }

        // ===================================================================
        // CAMEMBERT GENERIQUE (Presse / AssAuto)
        // ===================================================================

        private PlotModel BuildCamembert(string titre, double prod, double obj)
        {
            double reste = Math.Max(0, obj - prod);

            var model = new PlotModel { Title = titre, Background = OxyColors.White };
            var serie = new PieSeries
            {
                StrokeThickness     = 0,
                InsideLabelPosition = 0.6,
                InsideLabelFormat   = "{2:0}%",  // % a l'interieur
                OutsideLabelFormat  = "{1}",      // {1} = label string controle par nous
                FontSize            = 11
            };

            // Vert : label = valeur entiere formatee
            if (prod > 0)
                serie.Slices.Add(new PieSlice(prod.ToString("0") + " pcs", prod)
                    { Fill = OxyColor.FromRgb(40, 160, 80) });

            // Rouge : toujours afficher le manque si > 0 (meme 3% = potentiellement 2000 pieces)
            if (reste > 0)
                serie.Slices.Add(new PieSlice("manque: " + reste.ToString("0"), reste)
                    { Fill = OxyColor.FromRgb(210, 60, 60) });

            if (prod <= 0 && reste <= 0)
                serie.Slices.Add(new PieSlice("Aucune donnee", 1)
                    { Fill = OxyColor.FromRgb(200, 200, 200) });

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

    }
}
