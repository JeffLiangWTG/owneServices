using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobDeclarationNonTransactionTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestDeclarationDocManagerInfo()
		{
			var factory = new BusinessObjectFactory();
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.AllocateEntryNumber(string.Empty);
			var trigger = declaration.WorkflowItems.AddNew();
			trigger.P9_Description = "Data Export";
			trigger.P9_Type = Enterprise.Core.Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.DataExportCode;

			var triggerReload = declaration.DocManagerInfo.MasterFactory.FactoryForEverythingExceptEDocs.Load<ProcessTask>(trigger.PK);
			AssertNotNull(triggerReload);
		}

		[UseSnapshotProtection]
		public void TestDuplicateCusEntryNumber()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;

			var dec = factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			dec.US_EnableENS = true;
			dec.AllocateEntryNumber(string.Empty);

			var entry = dec.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;

			var throwException = true;
			dec.OnFactorySavingHandlerForTest = () =>
			{
				if (throwException)
				{
					throw new InvalidOperationException();
				}
			};

			using (((IDbConnected)factory).Connection.BeginTransactionWithManager())
			{
				try
				{
					factory.Save();
					Fail("Expected exception not thrown.");
				}
				catch (InvalidOperationException)
				{
				}
			}

			var anotherFac = new BusinessObjectFactory();
			anotherFac.RefreshEnabled = false;
			var dec1 = anotherFac.NewWithValidTestData<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			dec1.US_EnableENS = true;
			dec1.AllocateEntryNumber(string.Empty);
			anotherFac.Save();

			throwException = false;
			factory.Save();

			Assert(dec.IsInDatabase);
			Assert(dec1.IsInDatabase);
			Assert(!dec1.ImportEntryNumber.IsEmpty);
			Assert(!dec.ImportEntryNumber.IsEmpty);
			Assert(dec.ImportEntryNumber != dec1.ImportEntryNumber);
		}
	}
}
