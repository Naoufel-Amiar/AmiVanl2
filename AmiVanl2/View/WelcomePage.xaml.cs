using System.Windows;
using System.Windows.Controls;

namespace AmiVanl2.View
{
    public partial class WelcomePage : UserControl
    {
        public WelcomePage()
        {
            InitializeComponent();
        }

        private void BtnCommencer_Click(object sender, RoutedEventArgs e)
        {
            MainWindow fenetre = Application.Current.MainWindow as MainWindow;
            if (fenetre == null) return;

            fenetre.MainContent.Children.Clear();
            fenetre.MainContent.Children.Add(new AccImport());
        }
    }
}
