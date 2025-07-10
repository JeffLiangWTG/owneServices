using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Testing
{
	sealed class ShipmentLocatorExtensionsTesting : TestCaseWithFactory
	{
		public void TestFind()
		{
			CommonShipment garbageShipment = Factory.NewWithValidTestData<CommonShipment>();

			CommonShipment housebillOnlyShipment1 = Factory.NewWithValidTestData<CommonShipment>();
			housebillOnlyShipment1.JS_HouseBill = "housebill";

			CommonShipment housebillOnlyShipment2 = Factory.NewWithValidTestData<CommonShipment>();
			housebillOnlyShipment2.JS_HouseBill = "housebill";
			housebillOnlyShipment2.JS_E_DEP = new ZDateTime(2011, 1, 2);

			CommonShipment housebillOnlyShipment3 = Factory.NewWithValidTestData<CommonShipment>();
			housebillOnlyShipment3.JS_HouseBill = "housebill";
			housebillOnlyShipment3.JS_RL_NKOrigin = "AUSYD";

			CommonShipment uniqueRefAndHousebillShipment = Factory.NewWithValidTestData<CommonShipment>();
			uniqueRefAndHousebillShipment.JS_UniqueConsignRef = "uniqueref";
			uniqueRefAndHousebillShipment.JS_HouseBill = "sometext";

			ShipmentLocator<CommonShipment> locator = new ShipmentLocator<CommonShipment>(Factory, new ZQuery(JobShipmentSchema.JS_IsForwardRegistered, true));

			var xsdShipment = new Shipment();
			xsdShipment.ShipmentDetails.AgentReference = "";
			xsdShipment.ShipmentIdentifier = new ShipmentIdentifierCollection();
			ShipmentIdentifier houseIdentifier = xsdShipment.ShipmentIdentifier.AddNew();
			houseIdentifier.ShipmentIdentifierType = ShipmentIdentifierType.Housebill;
			houseIdentifier.Value = "housebill";

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ShipmentNumberImportTypes.Code.ShipmentNumber);
			CommonShipment foundShipment = locator.Find(xsdShipment);
			AssertNull("ImportType SHP: If agents ref not specified, should not find shipment", foundShipment);

			xsdShipment.ShipmentDetails.AgentReference = "uniqueref";
			foundShipment = locator.Find(xsdShipment);
			AssertEquals("ImportType SHP: If agents ref is specified, should find the correct shipment", uniqueRefAndHousebillShipment.PK, foundShipment.PK);

			xsdShipment.ShipmentDetails.AgentReference = "blah";
			foundShipment = locator.Find(xsdShipment);
			AssertNull("ImportType SHP: If agents ref not found, should not fallback to the shipment with the housebill specified", foundShipment);

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ShipmentNumberImportTypes.Code.ShipmentThenHouseBill);
			foundShipment = locator.Find(xsdShipment);
			AssertEquals("ImportType SHB: If agents ref not specified, should find the correct shipment by housebill number", housebillOnlyShipment1.PK, foundShipment.PK);

			xsdShipment.ShipmentDetails.AgentReference = "uniqueref";
			foundShipment = locator.Find(xsdShipment);
			AssertEquals("ImportType SHB: If agents ref is specified, should find the correct shipment", uniqueRefAndHousebillShipment.PK, foundShipment.PK);

			xsdShipment.ShipmentDetails.AgentReference = "blah";
			foundShipment = locator.Find(xsdShipment);
			AssertEquals("ImportType SHB: If agents ref not found, should  should find the correct shipment by housebill number", housebillOnlyShipment1.PK, foundShipment.PK);

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ShipmentNumberImportTypes.Code.HouseBill);
			foundShipment = locator.Find(xsdShipment);
			AssertEquals("ImportType HBL: Should find the correct shipment by housebill number", housebillOnlyShipment1.PK, foundShipment.PK);

			houseIdentifier.Value = "blah";
			foundShipment = locator.Find(xsdShipment);
			AssertNull("ImportType HBL: no shipment was found by housebill number", foundShipment);

			houseIdentifier.Value = "housebill";
			xsdShipment.ShipmentDetails.PortOfOrigin.EstimatedDateTime = new ZDateTime(2011, 1, 2);
			foundShipment = locator.Find(xsdShipment);
			AssertEquals(housebillOnlyShipment2.PK, foundShipment.PK);

			xsdShipment.ShipmentDetails.PortOfOrigin.Port.Value = "AUSYD";
			foundShipment = locator.Find(xsdShipment);
			AssertEquals(housebillOnlyShipment3.PK, foundShipment.PK);
		}

		public void TestFindByOtherAgentReferences()
		{
			string otherAgentReference = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.OtherAgentReference;

			CommonShipment garbageShipment = Factory.NewWithValidTestData<CommonShipment>();

			CommonShipment shipment1 = Factory.NewWithValidTestData<CommonShipment>();
			shipment1.JS_UniqueConsignRef = "number1";
			shipment1.JS_HouseBill = "house1";
			shipment1.Numbers.AddNewIfNotExist(otherAgentReference, "otherRef1");

			CommonShipment shipment2 = Factory.NewWithValidTestData<CommonShipment>();
			shipment2.JS_UniqueConsignRef = "number2";
			shipment2.JS_HouseBill = "house2";
			shipment2.Numbers.AddNewIfNotExist(otherAgentReference, "otherRef2");

			Factory.Save();

			var xsdShipment = new Shipment();
			xsdShipment.ShipmentDetails.AgentReference = "number1";
			xsdShipment.ShipmentIdentifier = new ShipmentIdentifierCollection();

			var referenceNumber = xsdShipment.ShipmentDetails.ReferenceNumbers.AddNew();
			referenceNumber.Type = otherAgentReference;
			referenceNumber.Number = "number2";

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ShipmentNumberImportTypes.Code.HouseBill);

			var locator = new ShipmentLocator<CommonShipment>(Factory, null);
			AssertEquals("Matched by OAG number", shipment2, locator.Find(xsdShipment));

			referenceNumber.Number = "unmatched";
			xsdShipment.ShipmentDetails.AgentReference = "otherRef1";
			AssertEquals("Matched by OAG number", shipment1, locator.Find(xsdShipment));
		}
	}
}
