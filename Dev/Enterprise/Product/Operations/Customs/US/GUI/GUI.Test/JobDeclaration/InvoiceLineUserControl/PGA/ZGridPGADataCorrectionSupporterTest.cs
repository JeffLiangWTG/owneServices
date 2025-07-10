using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	abstract class ZGridPGADataCorrectionSupporterTest<T> : TestCaseWithFactory where T : ZUserControl, new()
	{
		public void TestPGAMenuItems()
		{
			using (var control = new T())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.SetTrackingID();
				var grid = GetGrid(control);
				AssertNotNull("Context menu contains Update PGA Line", grid.ContextMenu.MenuItems.FindByText("Update PGA Line"));
				AssertNotNull("Context menu contains Delete PGA Line", grid.ContextMenu.MenuItems.FindByText("Delete PGA Line"));
				AssertNotNull("Context menu contains Add More PGA Lines)", grid.ContextMenu.MenuItems.FindByText("Add More PGA Lines"));
			}
		}

		public void TestUpdatePGALine()
		{
			using (var control = new T())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.US_EntryFilerCode = "XJ5";
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EnableCRL = true;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				var pgaCollection = GetPGACollection(invoiceLine);
				var grid = GetGrid(control);
				var pga = AddNewItemToCollection(pgaCollection);
				pga.TrackingStatusInfo.Value = (ZString)PGATrackingStatusList.Codes.Added;
				invoiceLine.SetTrackingID();
				control.SetDataBinding(pgaCollection, "");
				grid.DataSource = pgaCollection;
				grid.Select(0);
				declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
				var updatePGALine = grid.ContextMenu.MenuItems.FindByText("Update PGA Line");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				updatePGALine.PerformClick();
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ZGridPGADataCorrectionSupporter.UpdateStatusChangeWarning));
				AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, pga.TrackingStatusInfo.Value);
				AssertEquals(YesNoDefaultList.Codes.Yes, declaration.US_PGAReplaceUpdateNeeded);
				pga.TrackingStatusInfo.Value = (ZString)PGATrackingStatusList.Codes.Added;
				declaration.JE_EntryAuthorisationDate = ZDateTime.BrettsBirthday;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				updatePGALine.PerformClick();
				if (UpdateFDA)
				{
					Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ZGridPGADataCorrectionSupporter.GetReleasedUpdateStatusChangeWarning(UpdateFDACode)));
				}
				else
				{
					Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ZGridPGADataCorrectionSupporter.PGAReleasedUpdateStatusChangeWarning));
				}
			}
		}

		public void TestDeletePGALine()
		{
			using (var control = new T())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.US_EntryFilerCode = "XJ5";
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EnableCRL = true;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				var pgaCollection = GetPGACollection(invoiceLine);
				var grid = GetGrid(control);
				var pga = AddNewItemToCollection(pgaCollection);
				invoiceLine.SetTrackingID();
				pga.TrackingStatusInfo.Value = (ZString)PGATrackingStatusList.Codes.Added;
				control.SetDataBinding(pgaCollection, "");
				grid.DataSource = pgaCollection;
				grid.Select(0);
				declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
				var deletePGALine = grid.ContextMenu.MenuItems.FindByText("Delete PGA Line");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				deletePGALine.PerformClick();
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ZGridPGADataCorrectionSupporter.DeleteStatusChangeWarning));
				AssertEquals(PGATrackingStatusList.Codes.ToBeDeleted, pga.TrackingStatusInfo.Value);
				AssertEquals(YesNoDefaultList.Codes.Yes, declaration.US_PGAReplaceUpdateNeeded);
				pga.TrackingStatusInfo.Value = (ZString)PGATrackingStatusList.Codes.Added;
				declaration.JE_EntryAuthorisationDate = ZDateTime.BrettsBirthday;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				deletePGALine.PerformClick();
				if (UpdateFDA)
				{
					Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ZGridPGADataCorrectionSupporter.GetReleasedDeleteStatusChangeWarning(UpdateFDACode)));
				}
				else
				{
					Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ZGridPGADataCorrectionSupporter.PGAReleasedDeleteStatusChangeWarning));
				}
			}
		}

		public void TestAddMorePGALines()
		{
			using (var control = new T())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.US_EntryFilerCode = "XJ5";
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EnableCRL = true;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				var pgaCollection = GetPGACollection(invoiceLine);
				var grid = GetGrid(control);
				var pga = AddNewItemToCollection(pgaCollection);
				pga.TrackingStatusInfo.Value = (ZString)PGATrackingStatusList.Codes.Added;
				invoiceLine.SetTrackingID();
				control.SetDataBinding(pgaCollection, "");
				grid.DataSource = pgaCollection;
				var addMorePGALines = grid.ContextMenu.MenuItems.FindByText("Add More PGA Lines");
				declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
				addMorePGALines.PerformClick();
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ZGridPGADataCorrectionSupporter.AddStatusChangeWarning));
				declaration.JE_EntryAuthorisationDate = ZDateTime.BrettsBirthday;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				addMorePGALines.PerformClick();
				if (UpdateFDA)
				{
					Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ZGridPGADataCorrectionSupporter.GetReleasedAddStatusChangeWarning(UpdateFDACode)));
				}
				else
				{
					Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ZGridPGADataCorrectionSupporter.PGAReleasedAddStatusChangeWarning));
				}
			}
		}

		protected abstract IPGADataCorrectionCollection GetPGACollection(JobComInvoiceLine invoiceLine);

		protected abstract ZGrid GetGrid(T control);

		protected abstract IPGADataCorrection AddNewItemToCollection(IPGADataCorrectionCollection collection);

		protected virtual bool UpdateFDA => false;

		protected virtual ZString UpdateFDACode => ZString.Empty;
	}
}
