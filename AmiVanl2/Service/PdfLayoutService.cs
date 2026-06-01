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

            PageGarde(doc);

            if (AppData.Presses?.Count > 0)
            {
                PageSeparateur(doc, "SUIVI PRESSE", ColPresse);
                PagePresseBarres(doc);
                PagePresseCamemberts(doc);
            }

            if (AppData.AssAutos?.Count > 0)
            {
                PageSeparateur(doc, "ASSEMBLAGE AUTOMATIQUE", ColAssAuto);
                PageAssAutoBarres(doc);
                PageAssAutoCamemberts(doc);
            }

            if (AppData.Joints?.Count > 0)
            {
                PageSeparateur(doc, "JOINTS", ColJoints);
                PageJoints(doc);
            }

            if (AppData.Tris?.Count > 0)
            {
                PageSeparateur(doc, "TRI", ColTri);
                PageTri(doc);
            }

            if (AppData.AssManuels?.Count > 0)
            {
                PageSeparateur(doc, "ASSEMBLAGE MANUEL", ColAssManuel);
                PageAssManuel(doc);
            }

            doc.Save(cheminPdf);
            doc.Close();
        }

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

        private void PageGarde(PdfDocument doc)
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
                gfx.DrawString("Généré le " + DateTime.Now.ToString("dd/MM/yyyy à HH:mm"),
                    fDate, XBrushes.White, new XRect(0, 125, W, 38), XStringFormats.Center);

                // Titre "Sections"
                var fSec = new XFont("Arial", 14, XFontStyleEx.Bold);
                gfx.DrawString("SECTIONS DU RAPPORT", fSec,
                    new XSolidBrush(XColor.FromArgb(50, 50, 50)),
                    new XRect(0, 218, W, 28), XStringFormats.Center);

                // Cartes sections
                var sections = new (string Nom, XColor Couleur, bool Active)[]
                {
                    ("Suivi Presse",           ColPresse,    AppData.Presses?.Count > 0),
                    ("Assemblage Automatique", ColAssAuto,   AppData.AssAutos?.Count > 0),
                    ("Joints",                 ColJoints,    AppData.Joints?.Count > 0),
                    ("Tri",                    ColTri,       AppData.Tris?.Count > 0),
                    ("Assemblage Manuel",      ColAssManuel, AppData.AssManuels?.Count > 0),
                };

                double cardW = 185, cardH = 78, gap = 16;
                double totalW = sections.Length * cardW + (sections.Length - 1) * gap;
                double startX = (W - totalW) / 2;
                double cardY = 258;

                var fCard    = new XFont("Arial", 13, XFontStyleEx.Bold);
                var fCardSub = new XFont("Arial",  9, XFontStyleEx.Regular);

                foreach (var (nom, coul, active) in sections)
                {
                    XColor bg = active ? coul : XColor.FromArgb(215, 215, 215);
                    gfx.DrawRoundedRectangle(new XSolidBrush(bg), startX, cardY, cardW, cardH, 10, 10);
                    gfx.DrawString(nom, fCard,
                        new XSolidBrush(active ? XColor.FromArgb(30, 30, 30) : XColor.FromArgb(150, 150, 150)),
                        new XRect(startX, cardY + 10, cardW, cardH - 30), XStringFormats.Center);
                    string statut = active ? "✓ Données chargées" : "Aucune donnée";
                    gfx.DrawString(statut, fCardSub,
                        new XSolidBrush(active ? XColor.FromArgb(30, 120, 30) : XColor.FromArgb(150, 0, 0)),
                        new XRect(startX, cardY + cardH - 24, cardW, 20), XStringFormats.Center);
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

        private void PagePresseBarres(PdfDocument doc)
        {
            var g1 = AppData.Presses.Take(4).ToList();
            var g2 = AppData.Presses.Skip(4).Take(4).ToList();

            var m1 = BuildBarresPresse(g1, "Production presse — Groupe 1");
            var m2 = BuildBarresPresse(g2, "Production presse — Groupe 2");

            var page = NouvellePageA3(doc);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                DessinerEnTete(gfx, "Suivi Presse — Production journalière VS Objectif", ColPresse);
                DessinerLegendeCouleurs(gfx, H - 18);

                double y0 = HeaderH + Marge;
                double cH  = (H - y0 - Marge - 22) / 2;
                double cW  = W - 2 * Marge;

                PlacerGraphique(gfx, m1, Marge, y0, cW, cH);
                PlacerGraphique(gfx, m2, Marge, y0 + cH + 4, cW, cH);
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

        private void PagePresseCamemberts(PdfDocument doc)
        {
            var presses = AppData.Presses
                .GroupBy(p => p.Reference)
                .Select(g => g.First())
                .Take(8).ToList();

            var page = NouvellePageA3(doc);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                DessinerEnTete(gfx, "Suivi Presse — Taux d'atteinte des objectifs semaine", ColPresse);

                double y0 = HeaderH + Marge;
                double cH = (H - y0 - Marge) / 2;
                double cW = (W - 2 * Marge) / 4;

                for (int i = 0; i < Math.Min(presses.Count, 8); i++)
                {
                    var p = presses[i];
                    string titre = p.Reference.ToString("000000") + "\n" + p.Machine;
                    var model = BuildCamembert(titre, p.TotalProduction, p.ObjectifSemaine);
                    int col = i % 4, row = i / 4;
                    PlacerGraphique(gfx, model,
                        Marge + col * cW, y0 + row * cH, cW - 6, cH - 6);
                }
                DessinerNumeroPage(gfx, doc.Pages.Count);
            }
        }

        // ===================================================================
        // ASS AUTO — BARRES JOURNALIÈRES
        // ===================================================================

        private void PageAssAutoBarres(PdfDocument doc)
        {
            var g1 = AppData.AssAutos.Take(4).ToList();
            var g2 = AppData.AssAutos.Skip(4).Take(4).ToList();

            var m1 = BuildBarresAssAuto(g1, "Assemblage Auto — Groupe 1");
            var m2 = BuildBarresAssAuto(g2, "Assemblage Auto — Groupe 2");

            var page = NouvellePageA3(doc);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                DessinerEnTete(gfx, "Assemblage Automatique — Production journalière VS Objectif", ColAssAuto);
                DessinerLegendeCouleurs(gfx, H - 18);

                double y0 = HeaderH + Marge;
                double cH  = (H - y0 - Marge - 22) / 2;
                double cW  = W - 2 * Marge;

                PlacerGraphique(gfx, m1, Marge, y0, cW, cH);
                PlacerGraphique(gfx, m2, Marge, y0 + cH + 4, cW, cH);
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

        private void PageAssAutoCamemberts(PdfDocument doc)
        {
            var assAutos = AppData.AssAutos
                .Where(a => a.ObjectifSemaine > 0)
                .GroupBy(a => a.Reference)
                .Select(g => new AssAutoProduction
                {
                    Reference    = g.Key,
                    Machine      = string.Join("/", g.Select(x => x.Machine).Distinct()),
                    ObjectifSemaine = g.Sum(x => x.ObjectifSemaine),
                    ProdLundi    = g.Sum(x => x.ProdLundi),
                    ProdMardi    = g.Sum(x => x.ProdMardi),
                    ProdMercredi = g.Sum(x => x.ProdMercredi),
                    ProdJeudi    = g.Sum(x => x.ProdJeudi),
                    ProdVendredi = g.Sum(x => x.ProdVendredi),
                    ProdSamedi   = g.Sum(x => x.ProdSamedi),
                    ProdDimanche = g.Sum(x => x.ProdDimanche)
                }).Take(8).ToList();

            var page = NouvellePageA3(doc);
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                DessinerEnTete(gfx, "Assemblage Automatique — Taux d'atteinte des objectifs semaine", ColAssAuto);

                double y0 = HeaderH + Marge;
                double cH = (H - y0 - Marge) / 2;
                double cW = (W - 2 * Marge) / 4;

                for (int i = 0; i < Math.Min(assAutos.Count, 8); i++)
                {
                    var a     = assAutos[i];
                    var model = BuildCamembert(a.Reference + "\n" + a.Machine, a.TotalProduction, a.ObjectifSemaine);
                    int col = i % 4, row = i / 4;
                    PlacerGraphique(gfx, model,
                        Marge + col * cW, y0 + row * cH, cW - 6, cH - 6);
                }
                DessinerNumeroPage(gfx, doc.Pages.Count);
            }
        }

        // ===================================================================
        // JOINTS — TABLEAU COMPACT
        // ===================================================================

        private void PageJoints(PdfDocument doc)
        {
            var joints = AppData.Joints;
            // Pagination automatique si trop de références
            int debut = 0;
            while (debut < joints.Count)
            {
                var page = NouvellePageA3(doc);
                using (var gfx = XGraphics.FromPdfPage(page))
                {
                    gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                    DessinerEnTete(gfx, "Joints — Production par référence et journée (total équipes)", ColJoints);
                    debut = DessinerTableauJoints(gfx, joints, debut, HeaderH + Marge);
                    DessinerLegendeCouleurs(gfx, H - 18);
                    DessinerNumeroPage(gfx, doc.Pages.Count);
                }
            }
        }

        // Retourne l'index de la prochaine ligne non dessinée (pour pagination)
        private int DessinerTableauJoints(XGraphics gfx, List<JointProduction> joints,
            int startIndex, double startY)
        {
            if (joints.Count == 0) return joints.Count;

            double dispH  = H - startY - Marge - 22;
            double hdrH   = 26;
            double rowH   = 20;
            int    maxLig = (int)((dispH - hdrH) / rowH);

            double wRef  = 80, wCode = 68, wTot = 58, wObj = 58, wPct = 52;
            double wJour = (W - 2 * Marge - wRef - wCode - wTot - wObj - wPct) / 7;

            string[] labels = joints.Count > 0
                ? new[] { joints[0].LabelLundi, joints[0].LabelMardi, joints[0].LabelMercredi,
                          joints[0].LabelJeudi, joints[0].LabelVendredi, joints[0].LabelSamedi, joints[0].LabelDimanche }
                : new[] { "Lun", "Mar", "Mer", "Jeu", "Ven", "Sam", "Dim" };

            // En-tête colonnes
            double x = Marge, y = startY;
            DessinerCellule(gfx, x, y, wRef,  hdrH, "Référence",  FBold, new XSolidBrush(ColGris), true); x += wRef;
            DessinerCellule(gfx, x, y, wCode, hdrH, "Anc. Code",  FBold, new XSolidBrush(ColGris), true); x += wCode;
            for (int d = 0; d < 7; d++)
            {
                DessinerCellule(gfx, x, y, wJour, hdrH, labels[d], FBold, new XSolidBrush(ColGris), true);
                x += wJour;
            }
            DessinerCellule(gfx, x, y, wTot, hdrH, "Total",    FBold, new XSolidBrush(ColGris), true); x += wTot;
            DessinerCellule(gfx, x, y, wObj, hdrH, "Objectif", FBold, new XSolidBrush(ColGris), true); x += wObj;
            DessinerCellule(gfx, x, y, wPct, hdrH, "Taux %",   FBold, new XSolidBrush(ColGris), true);
            y += hdrH;

            // Lignes données
            int ri = startIndex;
            int lignesAffichees = 0;
            while (ri < joints.Count && lignesAffichees < maxLig)
            {
                var jt = joints[ri];
                double[] prods = {
                    jt.TotalLundi, jt.TotalMardi, jt.TotalMercredi,
                    jt.TotalJeudi, jt.TotalVendredi, jt.ProdSamedi, jt.ProdDimanche
                };

                XBrush bgAlt = ri % 2 == 0 ? XBrushes.White : new XSolidBrush(XColor.FromArgb(248, 248, 248));
                x = Marge;

                DessinerCellule(gfx, x, y, wRef,  rowH, jt.Reference,   FNorm, bgAlt); x += wRef;
                DessinerCellule(gfx, x, y, wCode, rowH, jt.AncienCode,  FNorm, bgAlt); x += wCode;

                for (int d = 0; d < 7; d++)
                {
                    double prod = prods[d];
                    bool   wknd = d >= 5;
                    XBrush bg;
                    if (wknd)
                        bg = new XSolidBrush(XColor.FromArgb(230, 230, 230));
                    else if (prod <= 0)
                        bg = bgAlt;
                    else
                        bg = prod >= jt.ObjectifEquipe
                            ? new XSolidBrush(ColVertClair)
                            : new XSolidBrush(ColRougeClair);

                    DessinerCellule(gfx, x, y, wJour, rowH, prod > 0 ? prod.ToString("0") : "—", FNorm, bg, true);
                    x += wJour;
                }

                XBrush bgTaux = jt.TauxAtteinte >= 100
                    ? new XSolidBrush(XColor.FromArgb(130, 205, 130))
                    : new XSolidBrush(XColor.FromArgb(230, 155, 155));

                DessinerCellule(gfx, x, y, wTot, rowH, jt.TotalProduction.ToString("0"), FBold, bgTaux, true); x += wTot;
                DessinerCellule(gfx, x, y, wObj, rowH, jt.ObjectifSemaine.ToString("0"), FNorm, bgAlt,  true); x += wObj;
                DessinerCellule(gfx, x, y, wPct, rowH, jt.TauxAtteinte.ToString("0") + " %", FBold, bgTaux, true);

                y += rowH;
                ri++;
                lignesAffichees++;
            }

            return ri;
        }

        // ===================================================================
        // TRI — TABLEAU COMPACT
        // ===================================================================

        private void PageTri(PdfDocument doc)
        {
            var tris = AppData.Tris;
            int debut = 0;
            while (debut < tris.Count)
            {
                var page = NouvellePageA3(doc);
                using (var gfx = XGraphics.FromPdfPage(page))
                {
                    gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
                    DessinerEnTete(gfx, "Tri — Production par référence et journée (total équipes)", ColTri);
                    debut = DessinerTableauTri(gfx, tris, debut, HeaderH + Marge);
                    DessinerLegendeCouleurs(gfx, H - 18);
                    DessinerNumeroPage(gfx, doc.Pages.Count);
                }
            }
        }

        private int DessinerTableauTri(XGraphics gfx, List<TriProduction> tris,
            int startIndex, double startY)
        {
            if (tris.Count == 0) return tris.Count;

            double dispH  = H - startY - Marge - 22;
            double hdrH   = 26;
            double rowH   = 20;
            int    maxLig = (int)((dispH - hdrH) / rowH);

            double wRef  = 80, wCode = 68, wTot = 58, wObj = 58, wPct = 52;
            double wJour = (W - 2 * Marge - wRef - wCode - wTot - wObj - wPct) / 7;

            string[] labels = tris.Count > 0
                ? new[] { tris[0].LabelLundi, tris[0].LabelMardi, tris[0].LabelMercredi,
                          tris[0].LabelJeudi, tris[0].LabelVendredi, tris[0].LabelSamedi, tris[0].LabelDimanche }
                : new[] { "Lun", "Mar", "Mer", "Jeu", "Ven", "Sam", "Dim" };

            double x = Marge, y = startY;
            DessinerCellule(gfx, x, y, wRef,  hdrH, "Référence",  FBold, new XSolidBrush(ColGris), true); x += wRef;
            DessinerCellule(gfx, x, y, wCode, hdrH, "Anc. Code",  FBold, new XSolidBrush(ColGris), true); x += wCode;
            for (int d = 0; d < 7; d++)
            {
                DessinerCellule(gfx, x, y, wJour, hdrH, labels[d], FBold, new XSolidBrush(ColGris), true);
                x += wJour;
            }
            DessinerCellule(gfx, x, y, wTot, hdrH, "Total",    FBold, new XSolidBrush(ColGris), true); x += wTot;
            DessinerCellule(gfx, x, y, wObj, hdrH, "Objectif", FBold, new XSolidBrush(ColGris), true); x += wObj;
            DessinerCellule(gfx, x, y, wPct, hdrH, "Taux %",   FBold, new XSolidBrush(ColGris), true);
            y += hdrH;

            int ri = startIndex;
            int lignes = 0;
            while (ri < tris.Count && lignes < maxLig)
            {
                var tr = tris[ri];
                double[] prods = {
                    tr.LundiEqu1   + tr.LundiEqu2   + tr.LundiEqu3,
                    tr.MardiEqu1   + tr.MardiEqu2   + tr.MardiEqu3,
                    tr.MercrediEqu1 + tr.MercrediEqu2 + tr.MercrediEqu3,
                    tr.JeudiEqu1   + tr.JeudiEqu2   + tr.JeudiEqu3,
                    tr.VendrediEqu1 + tr.VendrediEqu2 + tr.VendrediEqu3,
                    tr.ProdSamedi,
                    tr.ProdDimanche
                };
                double totalProd = prods.Sum();
                double pct = tr.ObjectifSemaine > 0 ? totalProd / tr.ObjectifSemaine * 100 : 0;

                XBrush bgAlt = ri % 2 == 0 ? XBrushes.White : new XSolidBrush(XColor.FromArgb(248, 248, 248));
                x = Marge;

                DessinerCellule(gfx, x, y, wRef,  rowH, tr.Reference,  FNorm, bgAlt); x += wRef;
                DessinerCellule(gfx, x, y, wCode, rowH, tr.AncienCode, FNorm, bgAlt); x += wCode;

                for (int d = 0; d < 7; d++)
                {
                    double prod = prods[d];
                    bool   wknd = d >= 5;
                    XBrush bg;
                    if (wknd)
                        bg = new XSolidBrush(XColor.FromArgb(230, 230, 230));
                    else if (prod <= 0)
                        bg = bgAlt;
                    else
                        bg = prod >= tr.ObjectifEquipe
                            ? new XSolidBrush(ColVertClair)
                            : new XSolidBrush(ColRougeClair);

                    DessinerCellule(gfx, x, y, wJour, rowH, prod > 0 ? prod.ToString("0") : "—", FNorm, bg, true);
                    x += wJour;
                }

                XBrush bgTaux = pct >= 100
                    ? new XSolidBrush(XColor.FromArgb(130, 205, 130))
                    : new XSolidBrush(XColor.FromArgb(230, 155, 155));

                DessinerCellule(gfx, x, y, wTot, rowH, totalProd.ToString("0"),       FBold, bgTaux, true); x += wTot;
                DessinerCellule(gfx, x, y, wObj, rowH, tr.ObjectifSemaine.ToString("0"), FNorm, bgAlt,  true); x += wObj;
                DessinerCellule(gfx, x, y, wPct, rowH, pct.ToString("0") + " %",      FBold, bgTaux, true);

                y += rowH;
                ri++;
                lignes++;
            }

            return ri;
        }

        // ===================================================================
        // ASSEMBLAGE MANUEL — COURBES PAR RÉFÉRENCE
        // ===================================================================

        private void PageAssManuel(PdfDocument doc)
        {
            var references = AppData.AssManuels
                .Select(x => x.Reference)
                .Distinct()
                .ToList();

            // Grille : 3 colonnes max, 2 lignes par page = 6 refs max par page
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
                    DessinerEnTete(gfx, "Assemblage Manuel — Production par référence (Capuchon / Insert / Objectif)", ColAssManuel);

                    double y0 = HeaderH + Marge;
                    double cH = (H - y0 - Marge) / lignes;
                    double cW = (W - 2 * Marge) / cols;

                    for (int i = 0; i < refPage.Count; i++)
                    {
                        string reference = refPage[i];
                        int col = i % cols, row = i / cols;

                        var capuchon = AppData.AssManuels.FirstOrDefault(x =>
                            x.Reference == reference && x.Operation.ToLower().Contains("capuchon"));
                        var insert = AppData.AssManuels.FirstOrDefault(x =>
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
                serObj.Points.Add(new DataPoint(i, i <= 4 ? objJour : 0));
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
