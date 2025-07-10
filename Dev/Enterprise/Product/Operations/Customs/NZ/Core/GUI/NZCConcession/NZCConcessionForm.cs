using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.NZ.GUI
{
	public partial class NZCConcessionForm : ZTemplateForm
	{
		private ZArchitecture.ZGrid dutyRatesGrid;
		private ZArchitecture.ZLabel zLabel5;
		internal ZArchitecture.ZTextBox u2_Calc_TariffsApplicableTextBox;
		internal ZArchitecture.ZTextBox u2_DescriptionTextBox;
		private ZDateEdit u0_DateActiveFromDateEdit;
		internal ZArchitecture.ZTextBox u2_CodeTextBox;
		private ZArchitecture.ZLabel zLabel4;
		private ZArchitecture.ZLabel zLabel3;
		private ZArchitecture.ZLabel zLabel1;
		internal ZDateEdit u0_DateActiveToDateEdit;
		private ZArchitecture.ZLabel zLabel6;
		private ZArchitecture.ZLabel zLabel2;
		private readonly System.ComponentModel.Container components;

		public NZCConcessionForm(NZCConcession concession)
			: base(concession)
		{
		}

		public override string FormCaption
		{
			get { return Enterprise.Customs.NZ.GUI.Res.GetString("D8E50AD2-06A9-4E7A-AA3A-61C61DF67A6E", "Concession"); }
		}

		#region Implementation

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		#endregion
	}
}


