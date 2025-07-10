using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class QueryFIRMSForm : ZChildForm
	{
		public QueryFIRMSForm()
		{
		}

		public QueryFIRMSForm(QueryFIRMSOption option)
			: base(option)
		{
		}

		QueryFIRMSOption Option
		{
			get { return (QueryFIRMSOption)BusinessEntity; }
		}

		public override string FormCaption
		{
			get { return "FIRMS"; }
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
