using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GUI;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.GUI
{
	class DA63PreSaveDialogStrategy : PreSaveDialogStrategy
	{
		public DA63PreSaveDialogStrategy(IDA63ValueRecalculationParent source)
		{
			this.source = source;
		}
		readonly IDA63ValueRecalculationParent source;

		internal static string ReCalculateConfimation => Res.GetString("E047D3BB-B013-493A-BD58-C00106D36F8B", "Some Invoice Lines have value changes that may affect the DA63 value, do you want to recalculate DA63 Values to suggested values?");

		#region Override

		protected override bool ShouldRunPreSaveAction()
		{
			return source != null
				&& source.DA63NeedsRecalculation
				&& Globals.Message.Show(ReCalculateConfimation, Res.GetString("D6E2DB4F-CD87-410B-9DAF-428B207E01E6", "Re-calculate DA63"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes) == DialogResult.Yes;
		}

		protected override ContinueWithSave RunPreSaveAction()
		{
			source.RecalculateDA63Values();
			return ContinueWithSave.Yes;
		}

		#endregion
	}
}
