using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	[TestedType(typeof(TransitWarehouseCusEntryNumReferenceCollectionReader))]
	public class TransitWarehouseCusEntryNumReferenceCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectTransitWarehouseAdditionalReferences>();
			var houseBillReference = CreateTransitAdditionalReferenceInfo(WarehouseAdditionalReferenceTypes.Codes.HouseBill, TransitWarehouseReferenceCategories.Codes.AdditionalReference, "V1");
			var forwardingShipmentReference = CreateTransitAdditionalReferenceInfo(WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference, "V2");

			var logger = new TestErrorLogger();
			var reader = new TransitWarehouseCusEntryNumReferenceCollectionReader(new[] { houseBillReference, forwardingShipmentReference }.ToArray(), parent, logger, Factory);

			reader.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Two Additional References must be populated.", 2, parent.CusEntryNumReferences.Count);
				var additionalReferenceForHSB = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.HouseBill);
				var additionalReferenceForFSH = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber);

				AssertMultilineASCIIEquals("logs",
@"Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...".Trim(), logger.Logs);
			});
		}

		public void TestReadIntoColection_ExistingReferences_DifferentEntryTypes()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectTransitWarehouseAdditionalReferences>();
			var additionalreferenceWithHSB = Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.HouseBill, "HSBReference", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			var additionalreferenceWithFSH = Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, "FSHReference", TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var referenceForHSB = CreateTransitAdditionalReferenceInfo(WarehouseAdditionalReferenceTypes.Codes.HouseBill, TransitWarehouseReferenceCategories.Codes.AdditionalReference, "HSBReference");
			var referenceForFSH = CreateTransitAdditionalReferenceInfo(WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference, "FSHRef");
			var newReference = CreateTransitAdditionalReferenceInfo("ABC", TransitWarehouseReferenceCategories.Codes.AdditionalReference, "Ref");

			var logger = new TestErrorLogger();
			var reader = new TransitWarehouseCusEntryNumReferenceCollectionReader(new[] { referenceForHSB, referenceForFSH, newReference }.ToArray(),
				parent, logger, Factory);

			reader.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Parent must have 3 Additional References.", 3, parent.CusEntryNumReferences.Count);
				var additionalReferenceForHSB = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.HouseBill);
				var additionalReferenceForFSH = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber);
				var additionalReferenceForABC = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == "ABC");
				Helper.AssertAdditionalReference(additionalReferenceForHSB, WarehouseAdditionalReferenceTypes.Codes.HouseBill, "HSBReference", "", "", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
				Helper.AssertAdditionalReference(additionalReferenceForFSH, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, "FSHRef", "", "", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
				Helper.AssertAdditionalReference(additionalReferenceForABC, "ABC", "Ref", "", "", TransitWarehouseReferenceCategories.Codes.AdditionalReference);

				AssertMultilineASCIIEquals("logs",
@"Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...".Trim(), logger.Logs);
			});
		}

		public void TestReadIntoColection_CustomsReference()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectTransitWarehouseAdditionalReferences>();
			var additionalreferenceWithCEN = Helper.CreateCustomsAdditionalReference(parent, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CENReferenceOld", TransitWarehouseReferenceCategories.Codes.CustomsReference);
			Factory.SaveForTesting();

			var referenceForCEN = CreateTransitAdditionalReferenceInfo(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, "CENReferenceNew");
			var referenceForCRN = CreateTransitAdditionalReferenceInfo(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, "CRNReferenceNew");

			var logger = new TestErrorLogger();
			var reader = new TransitWarehouseCusEntryNumReferenceCollectionReader(new[] { referenceForCEN, referenceForCRN }.ToArray(), parent, logger, Factory);

			reader.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Parent must have 3 Custom References.", 3, parent.CusEntryNumReferences.Count);
				var additionalReferenceForCEN = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber && c.CE_EntryNum == "CENReferenceNew");
				var additionalReferenceForCRN = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber && c.CE_EntryNum == "CRNReferenceNew");
				var additionalReferenceForExistingCRN = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber && c.CE_EntryNum == "CENReferenceOld");
				Helper.AssertAdditionalReference(additionalReferenceForCEN, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CENReferenceNew", "", "", TransitWarehouseReferenceCategories.Codes.CustomsReference);
				Helper.AssertAdditionalReference(additionalReferenceForCRN, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRNReferenceNew", "", "", TransitWarehouseReferenceCategories.Codes.CustomsReference);
				Helper.AssertAdditionalReference(additionalReferenceForExistingCRN, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CENReferenceOld", "", "", TransitWarehouseReferenceCategories.Codes.CustomsReference);

				AssertMultilineASCIIEquals("logs",
@"Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...".Trim(), logger.Logs);
			});
		}

		public void TestReadIntoColectionTwice_CustomsReference()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectTransitWarehouseAdditionalReferences>();
			var additionalreferenceWithCEN = Helper.CreateCustomsAdditionalReference(parent, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CENReferenceOld", TransitWarehouseReferenceCategories.Codes.CustomsReference);
			Factory.SaveForTesting();

			var referenceForCEN1 = CreateTransitAdditionalReferenceInfo(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, "CENReferenceNew1");
			var referenceForCRN1 = CreateTransitAdditionalReferenceInfo(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, "CRNReferenceNew1");
			var reader1 = new TransitWarehouseCusEntryNumReferenceCollectionReader(new[] { referenceForCEN1, referenceForCRN1 }.ToArray(), parent, new TestErrorLogger(), Factory);
			reader1.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();

			var referenceForCEN2 = CreateTransitAdditionalReferenceInfo(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, "CENReferenceNew2");
			var referenceForCRN2 = CreateTransitAdditionalReferenceInfo(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, "CRNReferenceNew2");
			var logger = new TestErrorLogger();
			var reader2 = new TransitWarehouseCusEntryNumReferenceCollectionReader(new[] { referenceForCEN2, referenceForCRN2 }.ToArray(), parent, logger, Factory);
			reader2.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Parent must have 5 Custom References.", 5, parent.CusEntryNumReferences.Count);
				var additionalReferenceForCEN1 = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber && c.CE_EntryNum == "CENReferenceNew1");
				var additionalReferenceForCRN1 = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber && c.CE_EntryNum == "CRNReferenceNew1");
				var additionalReferenceForCEN2 = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber && c.CE_EntryNum == "CENReferenceNew2");
				var additionalReferenceForCRN2 = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber && c.CE_EntryNum == "CRNReferenceNew2");
				var additionalReferenceForExistingCRN = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber && c.CE_EntryNum == "CENReferenceOld");
				Helper.AssertAdditionalReference(additionalReferenceForCEN1, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CENReferenceNew1", "", "", TransitWarehouseReferenceCategories.Codes.CustomsReference);
				Helper.AssertAdditionalReference(additionalReferenceForCRN1, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRNReferenceNew1", "", "", TransitWarehouseReferenceCategories.Codes.CustomsReference);
				Helper.AssertAdditionalReference(additionalReferenceForCEN2, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CENReferenceNew2", "", "", TransitWarehouseReferenceCategories.Codes.CustomsReference);
				Helper.AssertAdditionalReference(additionalReferenceForCRN2, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRNReferenceNew2", "", "", TransitWarehouseReferenceCategories.Codes.CustomsReference);
				Helper.AssertAdditionalReference(additionalReferenceForExistingCRN, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CENReferenceOld", "", "", TransitWarehouseReferenceCategories.Codes.CustomsReference);

				AssertMultilineASCIIEquals("logs",
@"Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...".Trim(), logger.Logs);
			});
		}

		public void TestReadIntoColection_ExistingReferences_DuplicatedMatchingEntryTypes()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectTransitWarehouseAdditionalReferences>();
			var additionalreferenceWithHSB1 = Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.HouseBill, "HSBReference1", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			var additionalreferenceWithHSB2 = Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.HouseBill, "HSBReference2", TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var referenceForHSB = CreateTransitAdditionalReferenceInfo(WarehouseAdditionalReferenceTypes.Codes.HouseBill, TransitWarehouseReferenceCategories.Codes.AdditionalReference, "HSBReferenceNew");

			var logger = new TestErrorLogger();
			var reader = new TransitWarehouseCusEntryNumReferenceCollectionReader(new[] { referenceForHSB }.ToArray(),
				parent, logger, Factory);

			reader.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Parent must have 2 Additional Reference.", 2, parent.CusEntryNumReferences.Count);
				var additionalReferenceForHSBUpdated = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.HouseBill && c.CE_EntryNum == "HSBReferenceNew");
				var additionalReferenceNotChanged = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.HouseBill && c.CE_EntryNum == "HSBReference2");
				Helper.AssertAdditionalReference(additionalReferenceForHSBUpdated, WarehouseAdditionalReferenceTypes.Codes.HouseBill, "HSBReferenceNew", "", "", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
				Helper.AssertAdditionalReference(additionalReferenceNotChanged, WarehouseAdditionalReferenceTypes.Codes.HouseBill, "HSBReference2", "", "", TransitWarehouseReferenceCategories.Codes.AdditionalReference);

				AssertMultilineASCIIEquals("logs",
@"Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...".Trim(), logger.Logs);
			});
		}

		public void TestReadIntoCollection_SourceType()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectTransitWarehouseAdditionalReferences>();

			var referenceWithSourceType = CreateTransitAdditionalReferenceInfo(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, "Test1");
			referenceWithSourceType.SourceType = "T1";
			var referenceWithoutSourceType = CreateTransitAdditionalReferenceInfo(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, TransitWarehouseReferenceCategories.Codes.CustomsReference, "Test2");
			var logger = new TestErrorLogger();
			var reader = new TransitWarehouseCusEntryNumReferenceCollectionReader(new[] { referenceWithSourceType, referenceWithoutSourceType }.ToArray(),
				parent, logger, Factory);

			reader.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				var cusEntryNums = Factory.Load<CusEntryNumber>(new ZQuery()).ToList();
				AssertEquals("Additional References Count must be 2.", 2, cusEntryNums.Count);
				var numberWithSourceType = cusEntryNums.First(c => c.CE_EntryNum == "Test1");
				var numberWithoutSourceType = cusEntryNums.First(c => c.CE_EntryNum == "Test2");
				var addOnValues = Factory.Load<GenCustomAddOnValue>(new ZQuery()).ToList();
				AssertEquals("Should Create 1 AddOn value.", 1, addOnValues.Count);
				AssertEquals("numberWithSourceType Should have 1 AddOn value.", 1, addOnValues.Where(v => v.XV_ParentID == numberWithSourceType.PK).ToList().Count);
				AssertEquals("numberWithoutSourceType should have no AddOn value.", 0, addOnValues.Where(v => v.XV_ParentID == numberWithoutSourceType.PK).ToList().Count);
			});
		}

		public void TestReadIntoCollection_KeepUnMatchingExistingReference()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectTransitWarehouseAdditionalReferences>();
			var additionalReferenceWithHSB1 = Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.HouseBill, "HSBReference1", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			var additionalReferenceWithHSB2 = Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.HouseBill, "HSBReference2", TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var unMatchingReference = CreateTransitAdditionalReferenceInfo(WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference, "Unmatching");

			var logger = new TestErrorLogger();
			var reader = new TransitWarehouseCusEntryNumReferenceCollectionReader(new[] { unMatchingReference }.ToArray(),
				parent, logger, Factory);

			reader.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Additional References Count must be 3.", 3, parent.CusEntryNumReferences.Count);
				var unMatchingImportedReference = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber);
				var hsbRef1 = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.HouseBill && c.CE_EntryNum == "HSBReference1");
				var hsbRef2 = parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.HouseBill && c.CE_EntryNum == "HSBReference2");
				Helper.AssertAdditionalReference(unMatchingImportedReference, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, "Unmatching", "", "", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
				Helper.AssertAdditionalReference(hsbRef1, WarehouseAdditionalReferenceTypes.Codes.HouseBill, "HSBReference1", "", "", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
				Helper.AssertAdditionalReference(hsbRef2, WarehouseAdditionalReferenceTypes.Codes.HouseBill, "HSBReference2", "", "", TransitWarehouseReferenceCategories.Codes.AdditionalReference);

				AssertMultilineASCIIEquals("logs",
@"Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...".Trim(), logger.Logs);
			});
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;

		TransitAdditionalReferenceInfo CreateTransitAdditionalReferenceInfo(string type, string category, string value, string countryCode = "")
		{
			return new TransitAdditionalReferenceInfo
			{
				Type = type,
				Category = category,
				Value = value,
				CountryCode = countryCode
			};
		}
	}

	class DummyEnterpriseBusinessObjectTransitWarehouseAdditionalReferences : DummyEnterpriseBusinessObject, IHaveCusEntryNumReferences
	{
		public DummyEnterpriseBusinessObjectTransitWarehouseAdditionalReferences(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override string TableName => "CusEntryHeader";

		#region CusEntryNumReferences

		public ICusEntryNumReferenceCollection CusEntryNumReferences
		{
			get
			{
				if (cusEntryNumReferences == null)
				{
					var provider = ObjectFactory.New<ICusEntryNumReferenceCollectionProvider>();
					cusEntryNumReferences = provider.GetCollection(this);
				}

				return cusEntryNumReferences;
			}
		}

		ICusEntryNumReferenceCollection cusEntryNumReferences;

		#endregion

	}
}
