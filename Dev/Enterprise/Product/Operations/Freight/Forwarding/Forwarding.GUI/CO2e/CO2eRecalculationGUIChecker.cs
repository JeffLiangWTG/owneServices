using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class CO2eRecalculationGUIChecker : ICO2eRecalculationChecker
	{
		public static void Register(BusinessObjectFactory factory)
		{
			factory?.SetValue<ICO2eRecalculationChecker, CO2eRecalculationGUIChecker>();
		}

		bool ICO2eRecalculationChecker.ShouldRecalculate(ICO2eCalculationSupporter supporter)
		{
			var dialogResult = Globals.Message.Show(
				Res.GetString("cacb9620-cf89-4bef-87fc-f37896faa7f3","The CO2e value is current. Do you wish to recalculate greenhouse gas emissions?"),
				Res.GetString("031b9872-af84-4e9c-8b10-01e22bcce596", "Confirmation"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				DialogResult.No
			);

			return dialogResult == DialogResult.Yes;
		}
	}
}
