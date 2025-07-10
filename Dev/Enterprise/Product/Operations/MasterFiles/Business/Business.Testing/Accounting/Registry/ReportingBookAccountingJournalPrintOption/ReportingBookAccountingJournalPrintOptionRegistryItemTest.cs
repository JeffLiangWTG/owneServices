using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ReportingBookAccountingJournalPrintOptionRegistryItem))]
	sealed class ReportingBookAccountingJournalPrintOptionRegistryItemTest : StronglyTypedRegistryItemTestCase<ReportingBookAccountingJournalPrintOptionCollection>
	{
		protected override StronglyTypedRegistryItem<ReportingBookAccountingJournalPrintOptionCollection, ReportingBookAccountingJournalPrintOptionCollection> GetNewRegistryItem()
		{
			return new ReportingBookAccountingJournalPrintOptionRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System);
		}
	}

	[TestedType(typeof(ReportingBookAccountingJournalPrintOptionRegistryDataType))]
	sealed class ReportingBookAccountingJournalPrintOptionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ReportingBookAccountingJournalPrintOptionRegistryDataType>
	{
		#region Implementation

		protected override ReportingBookAccountingJournalPrintOptionRegistryDataType GetNewDataType()
		{
			return new ReportingBookAccountingJournalPrintOptionRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ReportingBookAccountingJournalPrintOptionRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factroy = new BusinessObjectFactory();
			var accountingTestObjectCreator = new AccountingTestObjectCreator(factroy);
			var globalChart = accountingTestObjectCreator.CreateAlternateChart("GLC", isGlobal: true);
			var reportingBook = accountingTestObjectCreator.CreateAccReportingBook("GLC", globalChart.PK, GlbCompany.CurrentCompany.PK, "EET", "Description");

			var nonGlobalChart = accountingTestObjectCreator.CreateAlternateChart("NGC", isGlobal: false);
			var nonGlobalReportingBook = accountingTestObjectCreator.CreateAccReportingBook("NGC", nonGlobalChart.PK, GlbCompany.CurrentCompany.PK, "EET", "Description");

			factroy.Save();

			var collection = new ReportingBookAccountingJournalPrintOptionCollection();
			var copy = collection.AddNew();

			copy.ReportingBook = reportingBook.PK;
			copy.Default = true;

			var collection2 = new ReportingBookAccountingJournalPrintOptionCollection();
			var copy2 = collection2.AddNew();

			copy2.ReportingBook = nonGlobalReportingBook.PK;
			copy2.Default = true;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new ReportingBookAccountingJournalPrintOptionRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new ReportingBookAccountingJournalPrintOptionRegistryDataType().Serialise(collection2))
			};
		}

		#endregion
	}
}
