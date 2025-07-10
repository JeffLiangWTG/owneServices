using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class UNDGSubstanceJTTForm : UNDGSubstanceBaseForm
	{
		public UNDGSubstanceJTTForm(UNDGSubstanceJTT substance)
			: base(substance)
		{
		}

		UNDGSubstanceJTT Substance
		{
			get { return (UNDGSubstanceJTT)BusinessEntity; }
		}

		#region Load / Binding

		public override string FormCaption
		{
			get
			{
				var formName = Res.GetString("UNDGSubstanceJTTForm|FormCaptionPrefix", "Dangerous Goods Substance - Road");
				return Substance.IsInDatabase ? formName + " - " + Substance.JTT_UNNO + " - " + Substance.JTT_PSN : formName;
			}
		}

		#endregion

		protected override ZUserControl GetNewUNDGSubstanceUserControl() => new UNDGSubstanceJTTControl();
	}
}
