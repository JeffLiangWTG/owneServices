using CargoWise.Definitions;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	public class WhsDocketReferenceDataObjectReaderTest : OrganizationAddressTestHelper
	{
		#region TestBasicReferenceLevelFieldMappings

		public void TestBasicReferenceLevelFieldMappings()
		{
			var additionalReference = new AdditionalReference { ReferenceNumber = "121", Type = new EntryType { Code = "HSB", Description = "House Bill" } };
			var reader = new WhsDocketReferenceDataObjectReader(additionalReference, Logger, Factory, Factory.NewWithValidTestData<WhsOrder>());
			var reference = reader.ReadIntoBusinessObject();

			AssertNotNull("reference", reference);

			CombineAssertions(delegate
			{
				AssertEquals("reference.WX_Reference", "121", reference.WX_Reference);
				AssertEquals("reference.WX_RefType", "HSB", reference.WX_RefType);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching WhsDocketReference found, creating new WhsDocketReference.
Information - Populating WhsDocketReference...
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestLoadReference

		public void TestLoadReference()
		{
			var additionalReference = new AdditionalReference { ReferenceNumber = "121", Type = new EntryType { Code = "HSB", Description = "House Bill" } };
			var whsOrder = Factory.NewWithValidTestData<WhsOrder>();
			whsOrder.WD_BOLNo = "121";

			var reader = new WhsDocketReferenceDataObjectReader(additionalReference, Logger, Factory, whsOrder);
			var reference = reader.ReadIntoBusinessObject();

			AssertNotNull("reference", reference);

			CombineAssertions(delegate
			{
				AssertEquals("reference.WX_Reference", "121", reference.WX_Reference);
				AssertEquals("reference.WX_RefType", "HSB", reference.WX_RefType);
				AssertEquals("reference.PK", whsOrder.References[0].PK, reference.PK);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching WhsDocketReference.
Information - Populating WhsDocketReference...
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestLoadTPCReference

		public void TestLoadTPCReference()
		{
			var additionalReference = new AdditionalReference
			{
				ReferenceNumber = "45678",
				Type = new EntryType { Code = WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber, Description = WarehouseAdditionalReferenceTypes.Descriptions.ThirdPartyCarrierAccountNumber }
			};
			var whsOrder = Factory.NewWithValidTestData<WhsOrder>();
			var existingReference = whsOrder.References.AddNew();
			existingReference.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.OrderTypeCode;
			existingReference.WX_Reference = "AD1234";

			var reader = new WhsDocketReferenceDataObjectReader(additionalReference, Logger, Factory, whsOrder);
			var reference = reader.ReadIntoBusinessObject();

			AssertNotNull("reference", reference);

			CombineAssertions(delegate
			{
				AssertEquals("reference.WX_Reference", "45678", reference.WX_Reference);
				AssertEquals("reference.WX_RefType", WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber, reference.WX_RefType);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching WhsDocketReference found, creating new WhsDocketReference.
Information - Populating WhsDocketReference...
".Trim(), Logger.Logs);
			});
		}

		public void TestLoadTPCReference_Duplicated()
		{
			var additionalReference = new AdditionalReference
			{
				ReferenceNumber = "45678",
				Type = new EntryType { Code = WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber, Description = WarehouseAdditionalReferenceTypes.Descriptions.ThirdPartyCarrierAccountNumber }
			};
			var whsOrder = Factory.NewWithValidTestData<WhsOrder>();
			var existingTPCReference = whsOrder.References.AddNew();
			existingTPCReference.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber;
			existingTPCReference.WX_Reference = "777888";

			var reader = new WhsDocketReferenceDataObjectReader(additionalReference, Logger, Factory, whsOrder);

			// Assert Error Message
			AssertExceptionThrown(typeof(DataObjectReadFailureException), string.Format("Unable to import duplicate TPC Reference: {0}.", additionalReference.ReferenceNumber), () => reader.ReadIntoBusinessObject());
		}

		#endregion
	}
}
