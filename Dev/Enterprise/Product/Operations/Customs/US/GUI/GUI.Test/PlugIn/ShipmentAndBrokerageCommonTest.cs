using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using SharedCustoms = Enterprise.Customs.Business;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class ShipmentAndBrokerageCommonTest : Customs.GUI.PlugIn.Testing.BaseShipmentAndBrokerageCommonTest
	{
		public override void TestIsSupervisorApproved()
		{
			bool oldAllowed = Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed;
			try
			{
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
				var common = new ShipmentAndBrokerageCommon(null);
				Assert(common.IsSupervisorApproved() == ContinueWithSave.Yes);
				var decl = Factory.New<JobDeclaration>();
				decl.JE_MessageType = JobMessageTypeList.Codes.Export;
				var invoice1 = decl.Invoices.AddNew();
				invoice1.US_HazardousCargo = "Y";
				invoice1.InvoiceLines.AddNew();
				var invoice2 = decl.Invoices.AddNew();
				invoice2.US_HazardousCargo = "N";
				invoice2.InvoiceLines.AddNew();
				decl.DoMerge(new SharedCustoms.SendsMessagesToCustomsShutterUpperer());
				Factory.Save();
				var entry1 = decl.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(entry => entry.InvoiceHeaders.Cast<JobComInvoiceHeader>().FirstOrDefault(invoice => invoice == invoice1) != null);
				entry1.CH_Status = AESDirectCustomsEntryStatus.Codes.Error;
				invoice1.Delete();
				decl.DoMerge(new SharedCustoms.SendsMessagesToCustomsShutterUpperer());
				Assert(decl.HasDeactivatedAESEntryOriginalRejected());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				common = new ShipmentAndBrokerageCommon(decl);
				Assert(common.IsSupervisorApproved() == ContinueWithSave.Yes);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var userNotification = "The changes you have made caused a rejected AES to be deactivated. The deactivated entries could be viewed on the grid under Customs Declarations > Messages > Shipper’s Export Declarations by right-clicking in the grid and selecting an option ‘Show Deactivated Entries’  Are you sure you wish to continue to save?";
				Assert(common.IsSupervisorApproved() == ContinueWithSave.No);
				AssertEquals("Last text", userNotification, UnitTestUserNotification.Instance.LastMessage.Text);

				var decl2 = Factory.New<JobDeclaration>();
				decl2.JE_MessageType = JobMessageTypeList.Codes.Export;
				var collection = decl2.CustomsEntryHeaders;
				var entry2Mock = Factory.NewMoq<CusEntryHeader>();
				entry2Mock.Setup(m => m.HasBeenLodgedAtCustoms).Returns(true);
				var entry2 = entry2Mock.Object;
				collection.Add(entry2);
				entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
				entry2.US_IsDeactivated = false;
				AssertEquals(false, decl2.HasAESTIRMessageThatNeedsToBeWithdrawn);
				entry2.US_IsDeactivated = true;
				AssertEquals(true, decl2.HasAESTIRMessageThatNeedsToBeWithdrawn);
				AssertEquals(false, decl2.HasDeactivatedAESEntryOriginalRejected());
				AssertEquals(false, decl2.IsImport);
				AssertEquals(false, decl2.IsExWarehouse);
				UnitTestUserNotification.Instance.ClearMessages();
				var shipmentAndBrokerageCommon = new ShipmentAndBrokerageCommon(decl2);
				shipmentAndBrokerageCommon.IsSupervisorApproved();
				var userNotification2 = "Some changes were made which has caused an entry, which has already been lodged, to be deactivated.\r\nThis entry can be viewed under Messages > Shipper's Export Declarations Entries.\r\nWould you like to withdraw such entries now?";
				AssertEquals("Last text", userNotification2, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = oldAllowed;
			}
		}
	}
}
