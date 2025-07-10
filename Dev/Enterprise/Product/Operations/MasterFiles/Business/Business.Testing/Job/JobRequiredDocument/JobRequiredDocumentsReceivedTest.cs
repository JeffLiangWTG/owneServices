using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class JobRequiredDocumentsReceivedTest<ParentT> : TestCaseWithFactory
			where ParentT : BusinessObject
	{
		public void TestAIDandAEDeventSuccessForAllInOneSave()
		{
			CreateCountryRequiredDocuments();

			CreateNewParent(out ZGuid pk, out IHaveRequiredDocuments requiredDocumentsParent, out IStmALogParent logsParent);

			var requiredDocument = requiredDocumentsParent.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.CartageAdvice;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
			requiredDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = ZDateTimeOffset.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0001";

			requiredDocument = requiredDocumentsParent.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.ArrivalNotice;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
			requiredDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = ZDateTimeOffset.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0002";

			requiredDocument = requiredDocumentsParent.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.BillOfEntry;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
			requiredDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = ZDateTimeOffset.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0003";

			Factory.Save();

			AssertNotNull("AID event should be logged", logsParent.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived));
			AssertNotNull("AED event should be logged", logsParent.Logs.MostRecentLogByEventTime(Events.AllExportDocumentsReceived));

			var currentAIDEventTime = logsParent.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived).SL_EventTime;
			var currentAEDEventTime = logsParent.Logs.MostRecentLogByEventTime(Events.AllExportDocumentsReceived).SL_EventTime;

			ModifyParent(requiredDocumentsParent.UltimateDocumentParent as ParentT);

			Thread.Sleep(2000);

			Factory.Save();

			AssertEquals("AID Event Time stays the same", currentAIDEventTime, logsParent.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived).SL_EventTime);
			AssertEquals("AED Event Time stays the same", currentAEDEventTime, logsParent.Logs.MostRecentLogByEventTime(Events.AllExportDocumentsReceived).SL_EventTime);

			requiredDocument.EQ_DateReceived = ZDateTimeOffset.Empty;

			Factory.Save();

			AssertNull("AID event should be cancelled", logsParent.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived));
			AssertNull("AED event should be cancelled", logsParent.Logs.MostRecentLogByEventTime(Events.AllExportDocumentsReceived));
		}

		public void TestAIDandAEDeventSuccessForMultipleSavesSameDeclaration()
		{
			CreateCountryRequiredDocuments();

			CreateNewParent(out ZGuid pk, out IHaveRequiredDocuments requiredDocumentsParent, out IStmALogParent logsParent);

			var requiredDocument = requiredDocumentsParent.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.CartageAdvice;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
			requiredDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = ZDateTimeOffset.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0001";

			requiredDocument = requiredDocumentsParent.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.ArrivalNotice;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
			requiredDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = ZDateTimeOffset.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0002";

			AssertNull("AID event should not be logged", logsParent.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived));
			AssertNull("AED event should not be logged", logsParent.Logs.MostRecentLogByEventTime(Events.AllExportDocumentsReceived));

			Factory.Save();

			requiredDocument = requiredDocumentsParent.RequiredDocuments.Cast<JobRequiredDocument>().FirstOrDefault(d => d.EQ_DocType == Core.Constants.RefDocTypes.BillOfEntry);

			requiredDocument.EQ_DateReceived = ZDateTimeOffset.Today;

			Factory.Save();

			AssertNotNull("AID event should be logged", logsParent.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived));
			AssertNotNull("AED event should be logged", logsParent.Logs.MostRecentLogByEventTime(Events.AllExportDocumentsReceived));
		}

		public void TestAIDandAEDeventSuccessForDifferentDeclarationSaves()
		{
			CreateCountryRequiredDocuments();

			CreateNewParent(out ZGuid pk, out IHaveRequiredDocuments requiredDocumentsParent, out IStmALogParent logsParent);

			var requiredDocument = requiredDocumentsParent.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.CartageAdvice;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
			requiredDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = ZDateTimeOffset.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0001";

			requiredDocument = requiredDocumentsParent.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.ArrivalNotice;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
			requiredDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = ZDateTimeOffset.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0002";

			AssertNull("AID event should not be logged", logsParent.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived));
			AssertNull("AED event should not be logged", logsParent.Logs.MostRecentLogByEventTime(Events.AllExportDocumentsReceived));

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			ReloadParent(newFactory, pk, out IHaveRequiredDocuments requiredDocumentsParent2, out IStmALogParent logsParent2);

			requiredDocument = requiredDocumentsParent2.RequiredDocuments.Cast<JobRequiredDocument>().FirstOrDefault(d => d.EQ_DocType == Core.Constants.RefDocTypes.BillOfEntry);

			requiredDocument.EQ_DateReceived = ZDateTimeOffset.Today;

			newFactory.Save();

			AssertNotNull("AID event should be logged", logsParent2.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived));
			AssertNotNull("AED event should be logged", logsParent2.Logs.MostRecentLogByEventTime(Events.AllExportDocumentsReceived));
		}

		protected virtual void CreateCountryRequiredDocuments()
		{
			var testClasses = new RefCountryRequiredDocumentCollectionTest();
			testClasses.CreateRequiredDocumentsForAustraliaAndOriginSingapore();
		}

		protected abstract void CreateNewParent(out ZGuid pk, out IHaveRequiredDocuments requiredDocumentsParent, out IStmALogParent logsParent);
		protected abstract void ReloadParent(BusinessObjectFactory factory, ZGuid pk, out IHaveRequiredDocuments requiredDocumentsParent, out IStmALogParent logsParent);
		protected abstract void ModifyParent(ParentT parent);
	}
}
