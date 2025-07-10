using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	[TestedType(typeof(PortReferenceCollectionReader))]
	public class PortReferenceCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectPortReferences>();
			var reference1 = Helper.CreatePortReference("PAN", "Port reference Desc", "AU", "PANRef", "CLR");
			var reference2 = Helper.CreatePortReference("ABC", "ABC reference Desc", "NZ", "ABCRef1", "HLD");
			var nullStatus = Helper.CreatePortReference("AAA", "Null reference Desc", "FR", "AAARef1", null);

			var emptyStatus = Helper.CreatePortReference("BBB", "BBB reference Desc", "NZ", "BBBRef1", "");

			var logger = new TestErrorLogger();
			var reader = new PortReferenceCollectionReader(new[] { reference1, reference2, nullStatus, emptyStatus }.ToArray(),
				parent, logger, Factory);

			reader.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();
			CombineAssertions(() =>
			{
				AssertEquals("Port References Count", 4, parent.PortReferences.Count);
				var portReferenceForPAN = parent.PortReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == "PAN");
				var portReferenceForABC = parent.PortReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == "ABC");
				var portReferenceForAAA = parent.PortReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == "AAA");
				var portReferenceForBBB = parent.PortReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == "BBB");
				Helper.AssertAdditionalReference(portReferenceForPAN, "PAN", "PANRef", "AU", "CLR", TransitWarehouseReferenceCategories.Codes.PortReference);
				Helper.AssertAdditionalReference(portReferenceForABC, "ABC", "ABCRef1", "NZ", "HLD", TransitWarehouseReferenceCategories.Codes.PortReference);
				Helper.AssertAdditionalReference(portReferenceForAAA, "AAA", "AAARef1", "FR", "", TransitWarehouseReferenceCategories.Codes.PortReference);
				Helper.AssertAdditionalReference(portReferenceForBBB, "BBB", "BBBRef1", "NZ", "", TransitWarehouseReferenceCategories.Codes.PortReference);

				AssertMultilineASCIIEquals("logs",
@"Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...".Trim(), logger.Logs);
			});
		}

		public void TestReadIntoCollection_ExistingReferences_DifferentCountriesAndEntryNumbers()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectPortReferences>();
			var portReferenceInAU = Helper.CreateCustomsAdditionalReference(parent, "PAN", "PANReference", TransitWarehouseReferenceCategories.Codes.PortReference, "CLS", "AU");
			var portReferenceInNZ = Helper.CreateCustomsAdditionalReference(parent, "PAN", "PANReference", TransitWarehouseReferenceCategories.Codes.PortReference, "CLS", "NZ");
			var referenceStatusToBeUnChanged = Helper.CreateCustomsAdditionalReference(parent, "AAA", "AAAReference", TransitWarehouseReferenceCategories.Codes.PortReference, "HLD", "NZ");
			var referenceStatusToBeErased = Helper.CreateCustomsAdditionalReference(parent, "BBB", "BBBReference", TransitWarehouseReferenceCategories.Codes.PortReference, "CLS", "FR");

			var referenceForAU = Helper.CreatePortReference("PAN", "ABC reference Desc", "AU", "NewRef", "CLS");
			var referenceForNZ = Helper.CreatePortReference("PAN", "ABC reference Desc", "NZ", "NewPANReference", "HLD");
			var differentReference = Helper.CreatePortReference("ABC", "ABC ref", "NZ", "ABC Reference", "CLS");
			var nullStatus = Helper.CreatePortReference("AAA", "ABC ref", "NZ", "AAA Reference", null);
			var emptyStatus = Helper.CreatePortReference("BBB", "ABC ref", "FR", "BBB Reference", "");

			var logger = new TestErrorLogger();
			var reader = new PortReferenceCollectionReader(new[] { referenceForAU, referenceForNZ, differentReference, nullStatus, emptyStatus }.ToArray(),
				parent, logger, Factory);

			reader.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Port References Count", 5, parent.PortReferences.Count);
				var portReferenceForAU = parent.PortReferences.Cast<CusEntryNumber>().Single(c => c.CE_RN_NKCountryCode == "AU");
				var portReferenceForNZAndPAN = parent.PortReferences.Cast<CusEntryNumber>().Single(c => c.CE_RN_NKCountryCode == "NZ" && c.CE_EntryType == "PAN");
				var portReferenceForNZAndNew = parent.PortReferences.Cast<CusEntryNumber>().Single(c => c.CE_RN_NKCountryCode == "NZ" && c.CE_EntryType == "ABC");
				var portReferenceForStatusUnChanged = parent.PortReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == "AAA");
				var portReferenceForStatusErased = parent.PortReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == "BBB");
				Helper.AssertAdditionalReference(portReferenceForAU, "PAN", "NewRef", "AU", "CLS", TransitWarehouseReferenceCategories.Codes.PortReference);
				Helper.AssertAdditionalReference(portReferenceForNZAndPAN, "PAN", "NewPANReference", "NZ", "HLD", TransitWarehouseReferenceCategories.Codes.PortReference);
				Helper.AssertAdditionalReference(portReferenceForNZAndNew, "ABC", "ABC Reference", "NZ", "CLS", TransitWarehouseReferenceCategories.Codes.PortReference);
				Helper.AssertAdditionalReference(portReferenceForStatusUnChanged, "AAA", "AAA Reference", "NZ", "HLD", TransitWarehouseReferenceCategories.Codes.PortReference);
				//Helper.AssertAdditionalReference(portReferenceForStatusErased, "BBB", "BBB Reference", "FR", "", TransitWarehouseReferenceCategories.Codes.PortReference);

				AssertMultilineASCIIEquals("logs",
@"Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...".Trim(), logger.Logs);
			});
		}

		public void TestReadIntoCollection_KeepUnMatchingExistingReferences()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectPortReferences>();
			var portReferenceInAU = Helper.CreateCustomsAdditionalReference(parent, "PAN", "PANReference", TransitWarehouseReferenceCategories.Codes.PortReference, "CLS", "AU");
			var portReferenceInNZ = Helper.CreateCustomsAdditionalReference(parent, "PAN", "PANReference", TransitWarehouseReferenceCategories.Codes.PortReference, "CLS", "NZ");

			var unMatchingReference = Helper.CreatePortReference("ABC", "ABC reference Desc", "NZ", "Unmatching", "HLD");

			var logger = new TestErrorLogger();
			var reader = new PortReferenceCollectionReader(new[] { unMatchingReference }.ToArray(),
				parent, logger, Factory);

			reader.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Port References Count", 3, parent.PortReferences.Count);
				var portReferenceABC = parent.PortReferences.Cast<CusEntryNumber>().Single(p => p.CE_EntryType == "ABC");
				var portReferencePANInAU = parent.PortReferences.Cast<CusEntryNumber>().Single(p => p.CE_EntryType == "PAN" && p.CE_RN_NKCountryCode == "AU");
				var portReferencePANInNZ = parent.PortReferences.Cast<CusEntryNumber>().Single(p => p.CE_EntryType == "PAN" && p.CE_RN_NKCountryCode == "NZ");
				Helper.AssertAdditionalReference(portReferenceABC, "ABC", "Unmatching", "NZ", "HLD", TransitWarehouseReferenceCategories.Codes.PortReference);
				Helper.AssertAdditionalReference(portReferencePANInAU, "PAN", "PANReference", "AU", "CLS", TransitWarehouseReferenceCategories.Codes.PortReference);
				Helper.AssertAdditionalReference(portReferencePANInNZ, "PAN", "PANReference", "NZ", "CLS", TransitWarehouseReferenceCategories.Codes.PortReference);

				AssertMultilineASCIIEquals("logs",
@"Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...".Trim(), logger.Logs);
			});
		}

		public void TestReadIntoCollection_MissingPortReferenceStatus_ShouldNotUpdateEntryStatus()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectPortReferences>();
			var emptyStatus = Helper.CreatePortReference("PAN", "Port reference 1", "AU", "PANRef1", "CLR");
			var nullStatus = Helper.CreatePortReference("PAN", "Port reference 2", "NZ", "PANRef2", "HLD");
			var noStatus = Helper.CreatePortReference("PAN", "Port reference 3", "FR", "PANRef3", "CLR");

			var logger = new TestErrorLogger();
			var reader = new PortReferenceCollectionReader(new[] { emptyStatus, nullStatus, noStatus }.ToArray(),
				parent, logger, Factory);

			reader.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();
			CombineAssertions(() =>
			{
				AssertEquals("Port References Count", 3, parent.PortReferences.Count);
				var portReferenceForPAN1 = parent.PortReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == "PAN" && c.CE_EntryNum == "PANRef1");
				var portReferenceForPAN2 = parent.PortReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == "PAN" && c.CE_EntryNum == "PANRef2");
				var portReferenceForPAN3 = parent.PortReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == "PAN" && c.CE_EntryNum == "PANRef3");
				Helper.AssertAdditionalReference(portReferenceForPAN1, "PAN", "PANRef1", "AU", "CLR", TransitWarehouseReferenceCategories.Codes.PortReference);
				Helper.AssertAdditionalReference(portReferenceForPAN2, "PAN", "PANRef2", "NZ", "HLD", TransitWarehouseReferenceCategories.Codes.PortReference);
				Helper.AssertAdditionalReference(portReferenceForPAN3, "PAN", "PANRef3", "FR", "CLR", TransitWarehouseReferenceCategories.Codes.PortReference);
			});

			// update port reference status
			emptyStatus.Status.Code = "";
			nullStatus.Status.Code = null;
			noStatus = Helper.CreatePortReference("PAN", "Port reference 3", "FR", "PANRef3", null);

			reader.ReadIntoCollection();
			Factory.SaveForTesting();
			CombineAssertions(() =>
			{
				AssertEquals("Port References Count", 3, parent.PortReferences.Count);
				var portReferenceForPAN1 = parent.PortReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == "PAN" && c.CE_EntryNum == "PANRef1");
				var portReferenceForPAN2 = parent.PortReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == "PAN" && c.CE_EntryNum == "PANRef2");
				var portReferenceForPAN3 = parent.PortReferences.Cast<CusEntryNumber>().Single(c => c.CE_EntryType == "PAN" && c.CE_EntryNum == "PANRef3");
				Helper.AssertAdditionalReference(portReferenceForPAN1, "PAN", "PANRef1", "AU", "CLR", TransitWarehouseReferenceCategories.Codes.PortReference);
				Helper.AssertAdditionalReference(portReferenceForPAN2, "PAN", "PANRef2", "NZ", "HLD", TransitWarehouseReferenceCategories.Codes.PortReference);
				Helper.AssertAdditionalReference(portReferenceForPAN3, "PAN", "PANRef3", "FR", "CLR", TransitWarehouseReferenceCategories.Codes.PortReference);
			});
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;
	}

	class DummyEnterpriseBusinessObjectPortReferences : DummyEnterpriseBusinessObject, IHavePortReferences
	{
		public DummyEnterpriseBusinessObjectPortReferences(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override string TableName => "CusEntryHeader";

		#region PortReferences

		public IPortReferenceCollection PortReferences
		{
			get
			{
				if (portReferenceNumbers == null)
				{
					var provider = ObjectFactory.New<IPortReferenceCollectionProvider>();
					portReferenceNumbers = provider.GetCollection(this);
				}

				return portReferenceNumbers;
			}
		}

		IPortReferenceCollection portReferenceNumbers;

		#endregion
	}
}
