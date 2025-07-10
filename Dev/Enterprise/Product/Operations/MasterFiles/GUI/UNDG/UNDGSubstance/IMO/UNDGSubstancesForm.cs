using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class UNDGSubstanceForm : UNDGSubstanceBaseForm
	{
		public UNDGSubstanceForm(UNDGSubstance substance)
			: base(substance)
		{
		}

		UNDGSubstance Substance
		{
			get { return (UNDGSubstance)BusinessEntity; }
		}

		#region Load / Binding

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			Substance.CalcSegregation();
		}

		public override string FormCaption
		{
			get
			{
				var formName = Enterprise.MasterFiles.GUI.Res.GetString("UNDGSubstanceForm|FormCaptionPrefix", "Dangerous Goods Substance - Sea");
				return Substance.IsInDatabase ? formName + " - " + Substance.DG_UNNO + " - " + Substance.DG_PSN : formName;
			}
		}

		protected override ZUserControl GetNewUNDGSubstanceUserControl() => new UNDGSubstanceControl();

		#endregion
	}
}
