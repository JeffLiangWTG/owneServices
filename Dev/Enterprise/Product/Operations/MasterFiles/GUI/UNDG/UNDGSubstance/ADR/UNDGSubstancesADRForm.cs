using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class UNDGSubstanceADRForm : UNDGSubstanceBaseForm
	{
		public UNDGSubstanceADRForm(UNDGSubstanceADR substance)
			: base(substance)
		{
		}

		UNDGSubstanceADR Substance
		{
			get { return (UNDGSubstanceADR)BusinessEntity; }
		}

		public override string FormCaption
		{
			get
			{
				var formName = Res.GetString("UNDGSubstanceADRForm|FormCaptionPrefix", "Dangerous Goods Substance - Road");
				return Substance.IsInDatabase ? formName + " - " + Substance.ADR_UNNO + " - " + Substance.ADR_PSN : formName;
			}
		}

		protected override ZUserControl GetNewUNDGSubstanceUserControl() => new UNDGSubstanceADRControl();
	}
}
