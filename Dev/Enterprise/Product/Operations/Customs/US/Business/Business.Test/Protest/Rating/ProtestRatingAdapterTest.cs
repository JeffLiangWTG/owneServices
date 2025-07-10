using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Protest.Testing
{
	[TestedType(typeof(Protest))]
	sealed class ProtestRatingAdapterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestJobDatesProvider()
		{
			var protest = new Protest(Declaration);
			AssertType<JobDatesProvider<Protest>>(protest.GetFirstAdapter().JobDatesProvider);
		}

		public void TestImportBroker()
		{
			var protest = new Protest(Declaration);
			AssertNull(protest.GetFirstAdapter().ImportBroker);
		}

		public void TestExportBroker()
		{
			var protest = new Protest(Declaration);
			AssertNull(protest.GetFirstAdapter().ExportBroker);
		}

		public void TestAdapterTypeAndID()
		{
			var protest = new Protest(Declaration);
			AssertEquals(AdapterType.Protest, protest.GetFirstAdapter().AdapterType);
			AssertEquals(Declaration.JE_DeclarationReference, protest.GetFirstAdapter().OperationalJobCode);
		}

		protected override BusinessObject GetNewBusinessObject() => new Protest(Declaration);

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
	}
}
