using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.DocumentEngine.GUI;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	internal class DetentionAdviceDocumentCustomisationDevTool : DocumentCustomisationDevTool
	{
		public override string Name
		{
			get { return Res.GetString("e1bd3ad1-2a30-49ff-9915-0929e8e1bff7", "Customize Detention Advice Document"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Diagnostic Tool")]
		protected override void ShowCore(Form form)
		{
			ZForm zForm;
			BillOfLading bill;
			JobHeader job;
			OrgHeader client;

			if ((zForm = form as ZForm) == null)
			{
				Globals.Message.Show((NoResString)"Not a ZForm");
			}
			else if ((bill = zForm.BusinessEntity as BillOfLading) == null)
			{
				Globals.Message.Show((NoResString)"Could not find the BillOfLading");
			}
			else if ((job = bill.Job) == null)
			{
				Globals.Message.Show("No Job Header");
			}
			else if ((client = job.Factory.Load<OrgHeader>(job.LocalChargesPK)) == null)
			{
				Globals.Message.Show("No Client");
			}
			else
			{
				DetentionAdviceHeader advice = new DetentionAdviceHeader(client);
				advice.AsAt = ZDateTime.Today;
				advice.Containers.AddRange(bill.RealContainers);
				ShowCustomisationForm(advice);
			}
		}
	}
}
