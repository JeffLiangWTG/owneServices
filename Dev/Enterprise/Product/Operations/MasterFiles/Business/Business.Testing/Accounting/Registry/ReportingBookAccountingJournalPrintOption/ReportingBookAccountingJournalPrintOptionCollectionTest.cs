using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ReportingBookAccountingJournalPrintOptionCollection))]
	class ReportingBookAccountingJournalPrintOptionCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ReportingBookAccountingJournalPrintOptionCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ReportingBookAccountingJournalPrintOptionCollection GetCollectionToTest()
		{
			return new ReportingBookAccountingJournalPrintOptionCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ReportingBookAccountingJournalPrintOption();
		}

		public void TestCreateNonPersistentBusinessObject()
		{
			var collection = GetCollectionToTest();
			collection.CurrentFallbackLevel = NewFallbackLevel();
			var control = collection.AddNew();
			AssertEquals(control.CurrentFallbackLevel, collection.CurrentFallbackLevel);
			AssertNotNull(control.CurrentFallbackLevel);
		}

		#endregion
	}
}
