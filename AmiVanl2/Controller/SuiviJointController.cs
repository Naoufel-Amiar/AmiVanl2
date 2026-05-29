using AmiVanl2.Model;
using AmiVanl2.Service;
using System;
using System.Threading.Tasks;

namespace AmiVanl2.Controller
{
    public class JointController
    {
        private SuiviJointService jointService;

        public JointController()
        {
            jointService = new SuiviJointService();
        }

        public async Task<int> ChargerJointsAsync()
        {
            if (string.IsNullOrWhiteSpace(AppData.ExcelFilePath))
            {
                throw new Exception("Aucun fichier Excel importé.");
            }

            AppData.Joints.Clear();

            AppData.Joints =
                await jointService.LireJointsAsync(AppData.ExcelFilePath);

            return AppData.Joints.Count;
        }


    }
}