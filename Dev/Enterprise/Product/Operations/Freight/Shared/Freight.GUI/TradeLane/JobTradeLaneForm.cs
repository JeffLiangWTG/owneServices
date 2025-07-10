using System;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public partial class JobTradeLaneForm : ZTemplateForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public JobTradeLaneForm() { }

		public JobTradeLaneForm(JobTradeLane tradeLane)
			: base(tradeLane) { }

		#region ZForm Overrides

		public override string FormCaption
		{
			get { return Res.GetString("Freight|JobTradeLaneForm|FormCaptionPrefix", "Trade Lane") + " " + TradeLane.EJ_Code; }
		}

		protected override bool SupportsEDocs
		{
			get { return false; }
		}

		#endregion

		#region Implementation

		JobTradeLane TradeLane
		{
			get { return (JobTradeLane)BusinessEntity; }
		}

		#endregion
	}
}
