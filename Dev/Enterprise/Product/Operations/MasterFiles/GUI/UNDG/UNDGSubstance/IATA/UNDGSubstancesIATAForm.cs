using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class UNDGSubstanceIATAForm : UNDGSubstanceBaseForm
	{
		public UNDGSubstanceIATAForm(UNDGSubstance substance)
			: base(substance)
		{
			InitializeComponent();
		}

		UNDGSubstance Substance
		{
			get { return (UNDGSubstance)BusinessEntity; }
		}

		#region Load / Binding

		public override string FormCaption
		{
			get
			{
				var formName = Enterprise.MasterFiles.GUI.Res.GetString("UNDGSubstanceIATAForm|FormCaptionPrefix", "Dangerous Goods Substance - Air");
				return Substance.IsInDatabase ? formName + " - " + Substance.DG_UNNO + " - " + Substance.DG_PSN : formName;
			}
		}

		#endregion

		protected override ZUserControl GetNewUNDGSubstanceUserControl() => new UNDGSubstanceIATAControl(Substance);
	}
}
