using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AmiVanl2.Service
{
    internal class PdfExportService
    {
        public void ExporterPages(List<FrameworkElement> pages, string cheminPdf)
        {
            if (pages == null || pages.Count == 0)
                throw new Exception("Aucune page à exporter.");

            PdfDocument document = new PdfDocument();

            foreach (FrameworkElement pageWpf in pages)
            {
                PdfPage pagePdf = document.AddPage();
                pagePdf.Size = PageSize.A3;
                pagePdf.Orientation = PageOrientation.Landscape;

                double marge = 20;
                double largeurPdf = pagePdf.Width - marge * 2;
                double hauteurPdf = pagePdf.Height - marge * 2;

                BitmapSource image = CapturerElement(pageWpf, largeurPdf, hauteurPdf);
                string imageTemp = SauverImageTemporaire(image);

                using (XGraphics gfx = XGraphics.FromPdfPage(pagePdf))
                using (XImage img = XImage.FromFile(imageTemp))
                {
                    gfx.DrawImage(img, marge, marge, largeurPdf, hauteurPdf);
                }

                File.Delete(imageTemp);
            }

            document.Save(cheminPdf);
            document.Close();
        }

        private BitmapSource CapturerElement(
            FrameworkElement element,
            double largeurPdf,
            double hauteurPdf)
        {
            double facteurQualite = 2.5;

            double largeur = largeurPdf * facteurQualite;
            double hauteur = hauteurPdf * facteurQualite;

            element.Width = largeur;
            element.Height = hauteur;

            element.Measure(new Size(largeur, hauteur));
            element.Arrange(new Rect(0, 0, largeur, hauteur));
            element.UpdateLayout();

            RenderTargetBitmap bitmap = new RenderTargetBitmap(
                (int)largeur,
                (int)hauteur,
                150,
                150,
                PixelFormats.Pbgra32);

            bitmap.Render(element);

            return bitmap;
        }

        private string SauverImageTemporaire(BitmapSource image)
        {
            string cheminTemp = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString() + ".png");

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (FileStream stream = new FileStream(cheminTemp, FileMode.Create))
            {
                encoder.Save(stream);
            }

            return cheminTemp;
        }
    }
}