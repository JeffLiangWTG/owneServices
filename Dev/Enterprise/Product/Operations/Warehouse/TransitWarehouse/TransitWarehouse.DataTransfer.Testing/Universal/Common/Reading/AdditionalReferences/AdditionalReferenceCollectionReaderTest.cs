using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using AdditionalReference = Enterprise.UniversalDataBuss.DataObjects.Universal.AdditionalReference;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	[TestedType(typeof(AdditionalReferenceCollectionReader))]
	public class AdditionalReferenceCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectWithCusEntryReferences>();

			var reference = GetAdditionalReference();
			var logger = new TestErrorLogger();
			var reader = new AdditionalReferenceCollectionReader(new[] { reference }.ToArray(),
				parent, logger, Factory);

			reader.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();
			CombineAssertions(() =>
			{
				AssertEquals("Additional References Count", 1, parent.CusEntryNumReferences.Count);
				var additionalReference = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == "TES");
				Helper.AssertAdditionalReference(additionalReference, "TES", "Test Reference", "AU", category: TransitWarehouseReferenceCategories.Codes.AdditionalReference);

				AssertMultilineASCIIEquals("logs",
@"Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...".Trim(), logger.Logs);
			});
		}

		public void TestReadIntoCollection_KeepUnMatchingExistingReferences()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectWithCusEntryReferences>();
			Helper.CreateAdditionalReference(parent, "ABC Reference", "ABC");

			var unMatchingReference = GetAdditionalReference();

			var logger = new TestErrorLogger();
			var reader = new AdditionalReferenceCollectionReader(new[] { unMatchingReference }.ToArray(),
				parent, logger, Factory);

			reader.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Additional References Count", 2, parent.CusEntryNumReferences.Count);
				var additionalReferenceABC = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(p => p.CE_EntryType == "ABC");
				var additionalReferenceTES = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(p => p.CE_EntryType == "TES");

				Helper.AssertAdditionalReference(additionalReferenceABC, "ABC", "ABC Reference", category: TransitWarehouseReferenceCategories.Codes.AdditionalReference);
				Helper.AssertAdditionalReference(additionalReferenceTES, "TES", "Test Reference", "AU", category: TransitWarehouseReferenceCategories.Codes.AdditionalReference);

				AssertMultilineASCIIEquals("logs",
@"Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...".Trim(), logger.Logs);
			});
		}

		AdditionalReference GetAdditionalReference()
		{
			return Helper.CreateAdditionalReference("TES", "Test Type TES", "Test Reference", string.Empty, ZDateTime.Empty);
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;
	}

	class DummyEnterpriseBusinessObjectWithCusEntryReferences : DummyEnterpriseBusinessObject, IHaveCusEntryNumReferences
	{
		public DummyEnterpriseBusinessObjectWithCusEntryReferences(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override string TableName => "CusEntryHeader";

		#region CusEntryReferences

		public ICusEntryNumReferenceCollection CusEntryNumReferences
		{
			get
			{
				if (cusEntryReferenceNumbers == null)
				{
					var provider = ObjectFactory.New<ICusEntryNumReferenceCollectionProvider>();
					cusEntryReferenceNumbers = provider.GetCollection(this);
				}

				return cusEntryReferenceNumbers;
			}
		}

		ICusEntryNumReferenceCollection cusEntryReferenceNumbers;

		#endregion
	}
}
