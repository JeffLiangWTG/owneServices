using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class UNDGSubstanceADNForm : UNDGSubstanceBaseForm
	{
		public UNDGSubstanceADNForm(UNDGSubstanceADN substance)
			: base(substance)
		{
		}

		UNDGSubstanceADN Substance
		{
			get { return (UNDGSubstanceADN)BusinessEntity; }
		}

		#region Load / Binding

		public override string FormCaption
		{
			get
			{
				var formName = Res.GetString("UNDGSubstanceADNForm|FormCaptionPrefix", "Dangerous Goods Substance - Inland Water (ADN)");
				return Substance.IsInDatabase ? formName + " - " + Substance.ADN_UNNO + " - " + Substance.ADN_PSN : formName;
			}
		}

		#endregion

		protected override ZUserControl GetNewUNDGSubstanceUserControl() => new UNDGSubstanceADNControl();
	}
}
