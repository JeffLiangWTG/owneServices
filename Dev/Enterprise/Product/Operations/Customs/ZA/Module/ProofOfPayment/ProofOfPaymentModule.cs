using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.GUI;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.Module
{
	public class ProofOfPaymentModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ZAModuleIDs.ZA404ProofOfPayment; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZA404ProofOfPayment); }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Broker; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ZAControllerIDs.ZA404ProofOfPayment);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ProofOfPaymentFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ProofOfPaymentFilterControl(GridCollection, (ProofOfPaymentFilterBusinessObject)FilterBusinessObject);
		}

		protected override bool ShouldLoadFilterBusinessObjectDefaults => true;

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			var collection = new ModuleCusEntryPayInfoCollection(Factory);
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ProofOfPaymentFilterBusinessObject.FilterConstants.VATAmount, "GreaterThanDefaultProperty", ZDecimal.Zero));
			return collection;
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowView
		{
			get { return false; }
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem(ResString.GetMultilingualString("E6096ED4-6172-4ECF-AE43-8D4E1E9BD2A4", "Bulk Update"), OnBulkUpdateAction_Click));
			result.Add(new ZMenuItem(ResString.GetMultilingualString("D418B1D0-3C79-417F-8C22-65EDF30EE242", "Bulk Update - Clear Receipt Numbers"), OnBulkRemoveAction_Click));
			result.Add(new ZMenuItem(ResString.GetMultilingualString("8B0BB7EA-8D99-4690-8528-6AC3C8561C74", "Send Proof Of Payments Document"), OnSendProofOfPaymentsDocumentAction_Click));
			return result.ToArray();
		}

		void OnSendProofOfPaymentsDocumentAction_Click(object sender, EventArgs e)
		{
			var documentSendingInstruction = new VAT404DocumentInstruction(new BusinessObjectFactory());
			new VAT404DocumentSendingForm(documentSendingInstruction).ShowDialog();
		}

		void OnBulkUpdateAction_Click(object sender, EventArgs e)
		{
			var bos = GetSelectedBusinessObjects();
			if (bos.Length > 0)
			{
				var bulkBo = new CusEntryPayInfoBulkUpdateBusinessObject(Factory);
				bulkBo.SelectedPayInfos.AddRange(bos);
				new ProofOfPaymentBulkUpdateForm(bulkBo).ShowDialog();
			}
			else
			{
				Globals.Message.ShowInformation("Please select Pay Infos before selecting the Bulk Update feature.", "Proof Of Payment Bulk Update");
			}
		}

		void OnBulkRemoveAction_Click(object sender, EventArgs e)
		{
			var bos = GetSelectedBusinessObjects();
			if (bos.Length > 0)
			{
				var bulkBo = new CusEntryPayInfoBulkUpdateBusinessObject(Factory);
				bulkBo.SelectedPayInfos.AddRange(bos);

				var text = string.Format(CultureInfo.InvariantCulture, @"You are about to clear the receipt number and date
of {0} record{1}. Are you sure you wish to continue?", bulkBo.SelectedPayInfos.Count, (bulkBo.SelectedPayInfos.Count != 1 ? "s" : ""));
				var res = Globals.Message.ShowConfirmation(text, "Proof Of Payment Bulk Update", "Yes", MessageBoxIcon.Warning);

				if (res == DialogResult.OK)
				{
					bulkBo.ReceiptNumber = ZString.Empty;
					bulkBo.ReceiptDate = ZDate.Empty;
					Factory.Save();
				}
			}
			else
			{
				Globals.Message.ShowInformation("Please select Pay Infos before selecting the Bulk Update feature.", "Proof Of Payment Bulk Update");
			}
		}
	}
}
