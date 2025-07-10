using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class QueryCarrierForm : ZChildForm
	{
		public QueryCarrierForm()
		{
		}

		public QueryCarrierForm(QueryCarrierOption option)
			: base(option)
		{
		}

		QueryCarrierOption Option
		{
			get { return (QueryCarrierOption)BusinessEntity; }
		}

		public override string FormCaption
		{
			get { return "Carrier"; }
		}

		public override string FormVerb
		{
			get { return "Query"; }
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			ContinueWithSave continueWithSend = base.ValidateAndSave();

			if (continueWithSend == ContinueWithSave.Yes)
			{
				Option.SendQuery();
				Close();
			}
		}
	}
}
