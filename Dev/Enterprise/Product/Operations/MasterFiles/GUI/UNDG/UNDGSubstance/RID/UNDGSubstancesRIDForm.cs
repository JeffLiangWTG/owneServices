using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class UNDGSubstanceRIDForm : UNDGSubstanceBaseForm
	{
		public UNDGSubstanceRIDForm(UNDGSubstanceRID substance)
			: base(substance)
		{
		}

		UNDGSubstanceRID Substance
		{
			get { return (UNDGSubstanceRID)BusinessEntity; }
		}

		#region Load / Binding

		public override string FormCaption
		{
			get
			{
				var formName = Res.GetString("UNDGSubstanceRIDForm|FormCaptionPrefix", "Dangerous Goods Substance - Rail");
				return Substance.IsInDatabase ? formName + " - " + Substance.RID_UNNO + " - " + Substance.RID_PSN : formName;
			}
		}

		#endregion

		protected override ZUserControl GetNewUNDGSubstanceUserControl() => new UNDGSubstanceRIDControl();
	}
}
