using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ConsolidatedDelcarationJobNumberTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestAssignConsolidatedDeclarationJobNumber()
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var jobDeclaration = ConsolidatedDeclarationTestHelper.CreateJobDeclarationReadyForConsolidation<DummyJobDeclaration>(factory);
			factory.Save();

			var consolidatedDeclaration = factory.NewWithValidTestData<DummyConsolidatedDeclaration>();
			consolidatedDeclaration.JobDeclarations.Add(jobDeclaration);
			var expectedJobNumber = PeekConsolidatedDeclarationJobNumber();
			var validApplicationCode = consolidatedDeclaration.CRD_ApplicationCode;
			consolidatedDeclaration.CRD_ApplicationCode = "";
			AssertExceptionThrown<ZSaveException>(() => factory.Save());
			((IDbConnected)factory).Connection.RollbackTransaction();

			var newJobDeclaration = ConsolidatedDeclarationTestHelper.CreateJobDeclarationReadyForConsolidation<DummyJobDeclaration>(factory);
			consolidatedDeclaration.JobDeclarations.Add(newJobDeclaration);

			consolidatedDeclaration.CRD_ApplicationCode = validApplicationCode;
			factory.Save();
			AssertEquals("Consolidated Declaration Job number", expectedJobNumber, consolidatedDeclaration.CRD_JobReferenceNumber);
		}

		ZString PeekConsolidatedDeclarationJobNumber()
		{
			ZString declarationReference;
			var newFactory = new BusinessObjectFactory();
			var dbConnection = ((IDbConnected)newFactory).Connection;
			try
			{
				dbConnection.BeginTransaction();
				var declaration = newFactory.New<ConsolidatedDeclaration>();
				declaration.PopulateReferenceNumberIfNeeded();
				declarationReference = declaration.CRD_JobReferenceNumber;
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}
			return declarationReference;
		}
	}
}
