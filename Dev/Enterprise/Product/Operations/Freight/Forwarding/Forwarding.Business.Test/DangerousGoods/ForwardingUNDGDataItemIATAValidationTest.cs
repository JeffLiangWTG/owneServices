using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingUNDGDataItemIATAValidationTest : TestCaseWithFactory
	{
		public void TestUNDGContactDetailsValidations()
		{
			const string requiresNameAndTelephoneNumber = "This substance requires the name and telephone number of a responsible person to be included on the Shipper's Declaration.";
			const string doesNotHaveContactNumber = "This substance requires the name and telephone number of a responsible person to be included on the Shipper's Declaration. The DG Contact entered does not have a contact number. Press F3 on the DG Contact to save a Work, Mobile or Fax Number against the Contact.";

			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_Code = "1001";
			substance.DG_UNNO = "1001";
			undgDataItem.DI_DG = substance.PK;
			undgDataItem.Validation.ValidateDI_OC_DGContact();

			AssertNoError(undgDataItem.DI_OC_DGContactInfo, requiresNameAndTelephoneNumber);
			AssertNoError(undgDataItem.DI_OC_DGContactInfo, doesNotHaveContactNumber);

			substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_Code = "2814";
			substance.DG_UNNO = "2814";
			undgDataItem.DI_DG = substance.PK;
			undgDataItem.Validation.ValidateDI_OC_DGContact();

			AssertHasError(undgDataItem.DI_OC_DGContactInfo, requiresNameAndTelephoneNumber);
			AssertNoError(undgDataItem.DI_OC_DGContactInfo, doesNotHaveContactNumber);

			substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_Code = "2900";
			substance.DG_UNNO = "2900";
			undgDataItem.DI_DG = substance.PK;
			undgDataItem.Validation.ValidateDI_OC_DGContact();

			AssertHasError(undgDataItem.DI_OC_DGContactInfo, requiresNameAndTelephoneNumber);
			AssertNoError(undgDataItem.DI_OC_DGContactInfo, doesNotHaveContactNumber);

			substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_Code = "2900A";
			substance.DG_UNNO = "2900";
			undgDataItem.DI_DG = substance.PK;
			undgDataItem.Validation.ValidateDI_OC_DGContact();

			AssertHasError(undgDataItem.DI_OC_DGContactInfo, requiresNameAndTelephoneNumber);
			AssertNoError(undgDataItem.DI_OC_DGContactInfo, doesNotHaveContactNumber);

			var contact = Factory.New<OrgContact>();
			undgDataItem.DI_OC_DGContact = contact.PK;
			contact.OC_ContactName = "Cornelius";
			undgDataItem.Validation.ValidateDI_OC_DGContact();

			AssertNoError(undgDataItem.DI_OC_DGContactInfo, requiresNameAndTelephoneNumber);
			AssertHasError(undgDataItem.DI_OC_DGContactInfo, doesNotHaveContactNumber);

			contact.OC_Phone = "+1 201-555-5555";
			undgDataItem.Validation.ValidateDI_OC_DGContact();

			AssertNoError(undgDataItem.DI_OC_DGContactInfo, requiresNameAndTelephoneNumber);
			AssertNoError(undgDataItem.DI_OC_DGContactInfo, doesNotHaveContactNumber);

			contact.OC_Phone = "";
			contact.OC_Mobile = "+1 201-555-5555";
			undgDataItem.Validation.ValidateDI_OC_DGContact();

			AssertNoError(undgDataItem.DI_OC_DGContactInfo, requiresNameAndTelephoneNumber);
			AssertNoError(undgDataItem.DI_OC_DGContactInfo, doesNotHaveContactNumber);

			contact.OC_Mobile = null;
			contact.OC_HomePhone = "+1 201-555-5555";
			undgDataItem.Validation.ValidateDI_OC_DGContact();

			AssertNoError(undgDataItem.DI_OC_DGContactInfo, requiresNameAndTelephoneNumber);
			AssertNoError(undgDataItem.DI_OC_DGContactInfo, doesNotHaveContactNumber);
		}

		#region DI_PackingInstructionSection

		public void TestDI_PackingInstructionSectionIsEmptyWhenSubstanceChanges()
		{
			var lithiumSubstanceCode = LithiumBatteryConstants.UNNOCodes.CodesList.First();
			var substance = CreateNewIATASubstance(lithiumSubstanceCode, "a");
			substance.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965;
			substance.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965;
			substance.DG_LQ2OrPaxMaxAmt = 5;
			substance.DG_LQ2OrPaxMaxAmtUQ = Constants.Weight.Kilograms;
			substance.DG_CargoMaxAmt = 5;
			substance.DG_CargoMaxAmtUQ = Constants.Weight.Kilograms;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.Transports.RemoveAndDeleteAll();

			var packline = shipment.OuterPackLines.AddNew();
			var container = packline.Containers.AddNew();
			container.JC_ContainerMode = Constants.ContainerModes.ULD;

			var shipmentDGItem = packline.UNDGs.AddNew();
			shipmentDGItem.DI_DG = substance.PK;
			shipmentDGItem.DI_DGWeight = 1;
			shipmentDGItem.DI_F3_NKPackType = "BOX";
			shipmentDGItem.DI_UnitOfWeight = Constants.Weight.Kilograms;

			shipmentDGItem.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionII;

			var lithiumIonSubstance = Factory.New<UNDGSubstance>();
			lithiumIonSubstance.DG_Code = LithiumBatteryConstants.UNNOCodes.LithiumIonBatteries;
			lithiumIonSubstance.DG_UNNO = LithiumBatteryConstants.UNNOCodes.LithiumIonBatteries;
			lithiumIonSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			shipmentDGItem.DI_DG = lithiumIonSubstance.PK;
			AssertEquals("When substance changes, PackingInstructionSection becomes an empty string", true, shipmentDGItem.DI_PackingInstructionSection.IsEmpty);

			shipmentDGItem.DI_DG = ZGuid.Empty;
			AssertEquals("When substance changes, PackingInstructionSection becomes an empty string", true, shipmentDGItem.DI_PackingInstructionSection.IsEmpty);
		}

		#endregion

		#region DI_DG

		const string ForbiddenMessage = "This substance is forbidden for uplift by Air.";
		const string ForbiddenMessageWithScaaNotes = "This substance is forbidden for uplift by Air. \r\nIf you have Competent Authority Approval Certificate for this substance, please attach it to eDocs (with document type SCAA) before adding the substance to the Shipment.";
		const string OkCargoMessage = "This substance is forbidden for uplift by Air on a passenger flight but OK for cargo aircraft.";
		const string specialProvisionMessage = "This substance is forbidden for uplift by air, but it is allowed under the Competent Authority Approval. \r\nPlease correct/remove this DG and Competent Authority Approval Certificate if this is an impossible condition.";

		public void TestDI_DGValidationsIfAttachedToConsolWithAllAirCargoButNoAirPassengerTransports()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_Code = "1001";
			substance.DG_UNNO = "1001";
			substance.DG_Class = "2.1";
			substance.DG_LQ2OrPaxMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
			undgDataItem.DI_DG = substance.PK;

			AssertNoError(undgDataItem.DI_DGInfo, ForbiddenMessage);

			substance.DG_CargoPackAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
			consol.Shipments.Add(shipment);

			AssertHasError(undgDataItem.DI_DGInfo, ForbiddenMessage);
		}

		public void TestDI_DGValidationsIfAttachedToConsolWithAirPassengerTransports()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var transport = consol.MostInterestingTransportForBinding[0];
			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Air;
			transport2.JW_IsCargoOnly = true;
			transport.JW_IsCargoOnly = false;

			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_Code = "1001";
			substance.DG_UNNO = "1001";
			substance.DG_Class = "2.1";
			undgDataItem.DI_DG = substance.PK;
			undgDataItem.Validation.ValidateDI_DG();

			AssertNoError(undgDataItem.DI_DGInfo, ForbiddenMessage);

			substance.DG_LQ2OrPaxMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
			undgDataItem.Validation.ValidateDI_DG();

			AssertNoError(undgDataItem.DI_DGInfo, ForbiddenMessage);

			shipment.Consols.Add(consol);

			AssertHasError(undgDataItem.DI_DGInfo, ForbiddenMessage);

			substance.DG_CargoPackAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
			undgDataItem.Validation.ValidateDI_DG();

			AssertHasError(undgDataItem.DI_DGInfo, ForbiddenMessage);
		}

		#endregion

		public void TestForbiddenAirSubstanceValidationsIfNoConsol()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_Code = "1001";
			substance.DG_UNNO = "1001";
			substance.DG_Class = "2.1";
			undgDataItem.DI_DG = substance.PK;
			undgDataItem.Validation.ValidateDI_DG();

			AssertNoError(undgDataItem.DI_DGInfo, ForbiddenMessage);
			AssertNoWarning(undgDataItem.DI_DGInfo, OkCargoMessage);

			substance.DG_LQ2OrPaxMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
			undgDataItem.Validation.ValidateDI_DG();

			AssertNoError(undgDataItem.DI_DGInfo, ForbiddenMessage);
			AssertHasWarning(undgDataItem.DI_DGInfo, OkCargoMessage);

			substance.DG_CargoPackAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
			undgDataItem.Validation.ValidateDI_DG();

			AssertHasError(undgDataItem.DI_DGInfo, ForbiddenMessage);
			AssertNoWarning(undgDataItem.DI_DGInfo, OkCargoMessage);
		}

		public void TestForbiddenAirSubstanceValidationsWithCompetentAuthorityExemption()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.DocFactory.RetrieveExistingOrCreateStorageMainForPK(shipment.PK, "SHP");
			var contents = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 };
			var storageDoc = shipment.StorageMain.AddFileOrDocument(contents, "document.pdf", "SCAA");
			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_Code = "1001";
			substance.DG_UNNO = "1001";
			substance.DG_Class = "2.1";
			undgDataItem.DI_DG = substance.PK;
			substance.DG_LQ2OrPaxMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;

			var specialProvisionAttribute = Factory.New<ViewUNDGAttribute>();
			specialProvisionAttribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.SpecialProvisions;
			specialProvisionAttribute.DA_Descriptor = "A1";
			specialProvisionAttribute.DA_DG = substance.PK;
			undgDataItem.Validation.ValidateDI_DG();

			AssertHasWarning(undgDataItem.DI_DGInfo, specialProvisionMessage);
			AssertNoError(undgDataItem.DI_DGInfo, ForbiddenMessage);

			specialProvisionAttribute.DA_Descriptor = "A2";
			undgDataItem.Validation.ValidateDI_DG();

			AssertHasWarning(undgDataItem.DI_DGInfo, OkCargoMessage);
			AssertNoError(undgDataItem.DI_DGInfo, ForbiddenMessage);

			specialProvisionAttribute.DA_Descriptor = "";
			substance.DG_CargoPackAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
			undgDataItem.Validation.ValidateDI_DG();

			AssertHasError(undgDataItem.DI_DGInfo, ForbiddenMessage);
		}

		public void TestForbiddenAirSubstanceValidationsErrorChangedToWarning_WhenCompetentAuthorityExemptionAdded()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_Code = "1001";
			substance.DG_UNNO = "1001";
			substance.DG_Class = "2.1";
			undgDataItem.DI_DG = substance.PK;
			substance.DG_LQ2OrPaxMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
			substance.DG_CargoPackAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;

			var specialProvisionAttribute = Factory.New<ViewUNDGAttribute>();
			specialProvisionAttribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.SpecialProvisions;
			specialProvisionAttribute.DA_Descriptor = "A1";
			specialProvisionAttribute.DA_DG = substance.PK;

			undgDataItem.Validation.ValidateDI_DG();
			AssertHasError(undgDataItem.DI_DGInfo, ForbiddenMessageWithScaaNotes);

			var docFactory = shipment.DocManagerInfo.MasterFactory;
			docFactory.RetrieveExistingOrCreateStorageMainForPK(shipment.PK, "SHP");
			var storageMain = docFactory.GetStorageMainForPK(shipment.PK);
			var contents = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 };
			storageMain.AddFileOrDocument(contents, "document.pdf", "SCAA");

			undgDataItem.Validation.ValidateDI_DG();

			AssertHasWarning(undgDataItem.DI_DGInfo, specialProvisionMessage);
			AssertNoError(undgDataItem.DI_DGInfo, ForbiddenMessageWithScaaNotes);
		}

		#region Limited Quantity

		const string LimitedQuantitiesMessage = "This substance is forbidden for uplift by Air in Limited Quantities.";

		public void TestLimitedQuantityValidationsIfNoConsol()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_Code = "1001";
			substance.DG_UNNO = "1001";
			undgDataItem.DI_DG = substance.PK;
			undgDataItem.DI_IsLimitedQuantity = false;

			AssertNoWarning(undgDataItem.DI_IsLimitedQuantityInfo, LimitedQuantitiesMessage);

			undgDataItem.DI_IsLimitedQuantity = true;

			AssertNoWarning(undgDataItem.DI_IsLimitedQuantityInfo, LimitedQuantitiesMessage);

			substance.DG_LQMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
			undgDataItem.DI_IsLimitedQuantity = false;

			AssertNoWarning(undgDataItem.DI_IsLimitedQuantityInfo, LimitedQuantitiesMessage);

			undgDataItem.DI_IsLimitedQuantity = true;

			AssertHasWarning(undgDataItem.DI_IsLimitedQuantityInfo, LimitedQuantitiesMessage);
		}

		public void TestLimitedQuantityValidationsIfAttachedToConsol()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_Code = "1001";
			substance.DG_UNNO = "1001";
			undgDataItem.DI_DG = substance.PK;
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			shipment.Consols.Add(consol);

			undgDataItem.DI_IsLimitedQuantity = false;

			AssertNoWarning(undgDataItem.DI_IsLimitedQuantityInfo, LimitedQuantitiesMessage);

			undgDataItem.DI_IsLimitedQuantity = true;

			AssertNoWarning(undgDataItem.DI_IsLimitedQuantityInfo, LimitedQuantitiesMessage);

			substance.DG_LQMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
			undgDataItem.DI_IsLimitedQuantity = false;

			AssertNoWarning(undgDataItem.DI_IsLimitedQuantityInfo, LimitedQuantitiesMessage);

			undgDataItem.DI_IsLimitedQuantity = true;

			AssertHasWarning(undgDataItem.DI_IsLimitedQuantityInfo, LimitedQuantitiesMessage);
		}

		#endregion

		#region Maximum Quantity Allowed Tests

		const string PassengerMaximumMessage = "This quantity exceeds the maximum allowed for a Passenger Aircraft.";
		const string CargoMaximumMessage = "This quantity exceeds the maximum allowed for a Cargo Aircraft.";
		const string PackCountNotSetMessage = "Please enter a pack count/type so the system can check if this quantity exceeds the maximum allowed for a Cargo Only/Passenger Aircraft.";

		public void TestConsolDangerousGoodsExceedsMaximumWeightOnLimitedQuantities()
		{
			var undgDataItem = PrepareUNDG(isCargoOnlyConsol: false, setupWeight: true, setupVolume: false);
			var substance = undgDataItem.Substance;

			substance.DG_LQMaxAmt = 2;
			substance.DG_LQMaxAmtType = "NLM";
			substance.DG_LQMaxAmtUQ = "KG";

			substance.DG_CargoMaxAmt = 50;
			substance.DG_LQ2OrPaxMaxAmt = 5;

			undgDataItem.DI_DGWeight = 1;
			undgDataItem.DI_PackageCount = 1;
			undgDataItem.DI_IsLimitedQuantity = true;
			undgDataItem.RunPreSaveValidation();

			AssertNoError(undgDataItem.DI_DGWeightInfo, PassengerMaximumMessage);

			undgDataItem.DI_DGWeight = 3;
			undgDataItem.DI_IsLimitedQuantity = true;
			undgDataItem.DI_PackageCount = 1;
			undgDataItem.RunPreSaveValidation();

			AssertHasError(undgDataItem.DI_DGWeightInfo, PassengerMaximumMessage);
		}

		public void TestConsolDangerousGoodsExceedsMaximumWeightOnCargoAircraft()
		{
			var undgDataItem = PrepareUNDG(isCargoOnlyConsol: true, setupWeight: true, setupVolume: false);
			var substance = undgDataItem.Substance;

			substance.DG_CargoMaxAmt = 50;
			substance.DG_LQ2OrPaxMaxAmt = 5;

			undgDataItem.DI_DGWeight = 10;
			undgDataItem.DI_PackageCount = 1;
			undgDataItem.RunPreSaveValidation();

			AssertNoError(undgDataItem.DI_DGWeightInfo, CargoMaximumMessage);

			undgDataItem.DI_DGWeight = 100;
			undgDataItem.DI_PackageCount = 1;

			AssertHasError(undgDataItem.DI_DGWeightInfo, CargoMaximumMessage);
		}

		public void TestConsolDangerousGoodsExceedsMaximumWeightOnPassengerAircraft()
		{
			var undgDataItem = PrepareUNDG(isCargoOnlyConsol: false, setupWeight: true, setupVolume: false);
			var substance = undgDataItem.Substance;

			substance.DG_CargoMaxAmt = 20;
			substance.DG_LQ2OrPaxMaxAmt = 5;

			undgDataItem.DI_DGWeight = 1;
			undgDataItem.DI_PackageCount = 1;
			undgDataItem.RunPreSaveValidation();

			AssertNoError(undgDataItem.DI_DGWeightInfo, PassengerMaximumMessage);

			undgDataItem.DI_DGWeight = 10;
			undgDataItem.DI_PackageCount = 1;

			AssertHasError(undgDataItem.DI_DGWeightInfo, PassengerMaximumMessage);
		}

		public void TestConsolDangerousGoodsExceedsMaximumWeightOnCargoAircraftAndPackageCountIsZero()
		{
			var undgDataItem = PrepareUNDG(isCargoOnlyConsol: true, setupWeight: true, setupVolume: false);
			var substance = undgDataItem.Substance;

			substance.DG_CargoMaxAmt = 50;
			substance.DG_LQ2OrPaxMaxAmt = 5;

			undgDataItem.DI_DGWeight = 100;
			undgDataItem.DI_PackageCount = 0;
			undgDataItem.RunPreSaveValidation();

			AssertHasError(undgDataItem.DI_DGWeightInfo, PackCountNotSetMessage);

			undgDataItem.DI_PackageCount = 1;
			undgDataItem.RunPreSaveValidation();

			AssertHasError(undgDataItem.DI_DGWeightInfo, CargoMaximumMessage);
		}

		public void TestConsolDangerousGoodsExceedsMaximumWeightOnPassengerAircraftAndPackageCountIsZero()
		{
			var undgDataItem = PrepareUNDG(isCargoOnlyConsol: false, setupWeight: true, setupVolume: false);
			var substance = undgDataItem.Substance;

			substance.DG_CargoMaxAmt = 20;
			substance.DG_LQ2OrPaxMaxAmt = 5;

			undgDataItem.DI_DGWeight = 10;
			undgDataItem.DI_PackageCount = 0;
			undgDataItem.RunPreSaveValidation();

			AssertHasError(undgDataItem.DI_DGWeightInfo, PackCountNotSetMessage);

			undgDataItem.DI_PackageCount = 1;
			undgDataItem.RunPreSaveValidation();

			AssertHasError(undgDataItem.DI_DGWeightInfo, PassengerMaximumMessage);
		}

		public void TestConsolDangerousGoodsExceedsMaximumVolumeOnLimitedQuantities()
		{
			var undgDataItem = PrepareUNDG(isCargoOnlyConsol: false, setupWeight: false, setupVolume: true);
			var substance = undgDataItem.Substance;

			substance.DG_LQMaxAmt = 2;
			substance.DG_LQMaxAmtType = "NLM";
			substance.DG_LQMaxAmtUQ = "L";

			substance.DG_CargoMaxAmt = 50;
			substance.DG_LQ2OrPaxMaxAmt = 5;

			undgDataItem.DI_DGVolume = 1;
			undgDataItem.DI_PackageCount = 1;
			undgDataItem.DI_IsLimitedQuantity = true;
			undgDataItem.RunPreSaveValidation();

			AssertNoError(undgDataItem.DI_DGVolumeInfo, PassengerMaximumMessage);

			undgDataItem.DI_DGVolume = 3;
			undgDataItem.DI_PackageCount = 1;
			undgDataItem.DI_IsLimitedQuantity = true;
			undgDataItem.RunPreSaveValidation();

			AssertHasError(undgDataItem.DI_DGVolumeInfo, PassengerMaximumMessage);
		}

		public void TestConsolDangerousGoodsExceedsMaximumVolumeOnCargoAircraft()
		{
			var undgDataItem = PrepareUNDG(isCargoOnlyConsol: true, setupWeight: false, setupVolume: true);
			var substance = undgDataItem.Substance;

			substance.DG_CargoMaxAmt = 50;
			substance.DG_LQ2OrPaxMaxAmt = 5;

			undgDataItem.DI_DGVolume = 10;
			undgDataItem.DI_PackageCount = 1;
			undgDataItem.RunPreSaveValidation();

			AssertNoError(undgDataItem.DI_DGVolumeInfo, CargoMaximumMessage);

			undgDataItem.DI_DGVolume = 100;
			undgDataItem.DI_PackageCount = 1;

			AssertHasError(undgDataItem.DI_DGVolumeInfo, CargoMaximumMessage);
		}

		public void TestDangerousGoodsExceedsMaximumVolumeOnCargoAircraftWithoutConsol()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			var undgDataItem = PrepareUNDG(shipment: shipment, setupWeight: false, setupVolume: true);
			var substance = undgDataItem.Substance;

			substance.DG_CargoMaxAmt = 50;
			substance.DG_LQ2OrPaxMaxAmt = 5;

			undgDataItem.DI_DGVolume = 10;
			undgDataItem.DI_PackageCount = 1;
			undgDataItem.RunPreSaveValidation();

			AssertNoError(undgDataItem.DI_DGVolumeInfo, CargoMaximumMessage);

			undgDataItem.DI_DGVolume = 100;
			undgDataItem.DI_PackageCount = 1;
			undgDataItem.RunPreSaveValidation();

			AssertHasError(undgDataItem.DI_DGVolumeInfo, CargoMaximumMessage);
		}

		public void TestConsolDangerousGoodsExceedsMaximumVolumeOnPassengerAircraft()
		{
			var undgDataItem = PrepareUNDG(isCargoOnlyConsol: false, setupWeight: false, setupVolume: true);
			var substance = undgDataItem.Substance;

			substance.DG_CargoMaxAmt = 20;
			substance.DG_LQ2OrPaxMaxAmt = 5;

			undgDataItem.DI_DGVolume = 1;
			undgDataItem.DI_PackageCount = 1;
			undgDataItem.RunPreSaveValidation();

			AssertNoError(undgDataItem.DI_DGVolumeInfo, PassengerMaximumMessage);

			undgDataItem.DI_DGVolume = 10;
			undgDataItem.DI_PackageCount = 1;

			AssertHasError(undgDataItem.DI_DGVolumeInfo, PassengerMaximumMessage);
		}

		public void TestConsolDangerousGoodsExceedsMaximumVolumeOnCargoAircraftAndPackageCountIsZero()
		{
			var undgDataItem = PrepareUNDG(isCargoOnlyConsol: true, setupWeight: false, setupVolume: true);
			var substance = undgDataItem.Substance;

			substance.DG_CargoMaxAmt = 50;
			substance.DG_LQ2OrPaxMaxAmt = 5;

			undgDataItem.DI_DGVolume = 100;
			undgDataItem.DI_PackageCount = 0;
			undgDataItem.RunPreSaveValidation();

			AssertHasError(undgDataItem.DI_DGVolumeInfo, PackCountNotSetMessage);

			undgDataItem.DI_PackageCount = 1;
			undgDataItem.RunPreSaveValidation();

			AssertHasError(undgDataItem.DI_DGVolumeInfo, CargoMaximumMessage);
		}

		public void TestConsolDangerousGoodsExceedsMaximumVolumeOnPassengerAircraftAndPackageCountIsZero()
		{
			var undgDataItem = PrepareUNDG(isCargoOnlyConsol: false, setupWeight: false, setupVolume: true);
			var substance = undgDataItem.Substance;

			substance.DG_CargoMaxAmt = 20;
			substance.DG_LQ2OrPaxMaxAmt = 5;

			undgDataItem.DI_DGVolume = 10;
			undgDataItem.DI_PackageCount = 0;
			undgDataItem.RunPreSaveValidation();

			AssertHasError(undgDataItem.DI_DGVolumeInfo, PackCountNotSetMessage);

			undgDataItem.DI_PackageCount = 1;
			undgDataItem.RunPreSaveValidation();

			AssertHasError(undgDataItem.DI_DGVolumeInfo, PassengerMaximumMessage);
		}

		public void TestWeightValidationWithoutPackLine_ShouldNotThrowException()
		{
			var undgDataItem = PrepareUNDG(isCargoOnlyConsol: false, setupWeight: true, setupVolume: false);
			var substance = undgDataItem.Substance;

			substance.DG_CargoMaxAmt = 20;
			substance.DG_LQ2OrPaxMaxAmt = 5;
			substance.DG_ExceptedQuantityCode = "E1";

			undgDataItem.DI_DGWeight = 1;
			undgDataItem.DI_ParentID = ZGuid.Empty;

			AssertNoExceptionThrown(() => undgDataItem.Validation.ValidateDI_DGWeight());
		}

		UNDGDataItem PrepareUNDG(bool isCargoOnlyConsol, bool setupWeight, bool setupVolume)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.MostInterestingTransportForBinding[0].JW_IsCargoOnly = isCargoOnlyConsol;

			var shipment = consol.Shipments.AddNew();
			return PrepareUNDG(shipment, setupWeight, setupVolume);
		}

		UNDGDataItem PrepareUNDG(ForwardingShipment shipment, bool setupWeight, bool setupVolume)
		{
			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();
			var substance = CreateNewIATASubstance("1234", "a");

			substance.DG_CargoPackAmtType = "NLM";
			substance.DG_LQ2OrPaxMaxAmtType = "NLM";

			if (setupVolume)
			{
				substance.DG_CargoMaxAmtUQ = "L";
				substance.DG_LQ2OrPaxMaxAmtUQ = "L";
				undgDataItem.DI_UnitOfVolume = "L";
			}

			if (setupWeight)
			{
				substance.DG_CargoMaxAmtUQ = "KG";
				substance.DG_LQ2OrPaxMaxAmtUQ = "KG";
				undgDataItem.DI_UnitOfWeight = "KG";
			}

			undgDataItem.DI_DG = substance.PK;

			return undgDataItem;
		}

		#endregion

		#region ExceptedQuantityCodes

		#region OuterPack

		#region Weight

		const string ExceedMaximumQuantitiesMessage = "This value exceeds the maximum quantity per pack so cannot be transported in Excepted Quantities, per IATA DGR.";

		public void TestPacklineDangerousGoodsExceedsExceptedQuantity_OuterPack_Solid_E1() =>
			OuterPacklineDangerousSolidGoodsExceedsExceptedQuantity("E1", 0.5, 1.2);

		public void TestPacklineDangerousGoodsExceedsExceptedQuantity_OuterPack_Solid_E2() =>
			OuterPacklineDangerousSolidGoodsExceedsExceptedQuantity("E2", 0.4, 0.6);

		public void TestPacklineDangerousGoodsExceedsExceptedQuantity_OuterPack_Solid_E3() =>
			OuterPacklineDangerousSolidGoodsExceedsExceptedQuantity("E3", 0.2, 0.4);

		public void TestPacklineDangerousGoodsExceedsExceptedQuantity_OuterPack_Solid_E4() =>
			OuterPacklineDangerousSolidGoodsExceedsExceptedQuantity("E4", 0.4, 0.6);

		public void TestPacklineDangerousGoodsExceedsExceptedQuantity_OuterPack_Solid_E5() =>
			OuterPacklineDangerousSolidGoodsExceedsExceptedQuantity("E5", 0.2, 0.4);

		void OuterPacklineDangerousSolidGoodsExceedsExceptedQuantity(string exceptedQuantity, double amountBefore, double amountAfter)
		{
			var packline = SetupAndGetForwardingPackline();
			var substance = CreateSolidSubstanceOfExceptedQuantityCode(exceptedQuantity, "1234", "a");

			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.DI_DG = substance.PK;
			undgDataItem.DI_DGWeight = amountBefore;
			undgDataItem.DI_UnitOfWeight = Constants.Weight.Kilograms;

			AssertNoWarning(undgDataItem.DI_DGWeightInfo, ExceedMaximumQuantitiesMessage);

			undgDataItem.DI_DGWeight = amountAfter;

			AssertHasWarning(undgDataItem.DI_DGWeightInfo, ExceedMaximumQuantitiesMessage);
		}

		#endregion

		#region Volume

		public void TestPacklineDangerousGoodsExceedsExceptedQuantity_OuterPack_Liquid_E1() =>
			OuterPacklineDangerousLiquidGoodsExceedsExceptedQuantity("E1", 0.5, 1.2);

		public void TestPacklineDangerousGoodsExceedsExceptedQuantity_OuterPack_Liquid_E2() =>
			OuterPacklineDangerousLiquidGoodsExceedsExceptedQuantity("E2", 0.4, 0.6);

		public void TestPacklineDangerousGoodsExceedsExceptedQuantity_OuterPack_Liquid_E3() =>
			OuterPacklineDangerousLiquidGoodsExceedsExceptedQuantity("E3", 0.2, 0.4);

		public void TestPacklineDangerousGoodsExceedsExceptedQuantity_OuterPack_Liquid_E4() =>
			OuterPacklineDangerousLiquidGoodsExceedsExceptedQuantity("E4", 0.4, 0.6);

		public void TestPacklineDangerousGoodsExceedsExceptedQuantity_OuterPack_Liquid_E5() =>
			OuterPacklineDangerousLiquidGoodsExceedsExceptedQuantity("E5", 0.2, 0.4);

		void OuterPacklineDangerousLiquidGoodsExceedsExceptedQuantity(string exceptedQuantity, double amountBefore, double amountAfter)
		{
			var packline = SetupAndGetForwardingPackline();
			var substance = CreateVolumeSubstanceOfExceptedQuantityCode(exceptedQuantity, "1234", "a");

			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.DI_DG = substance.PK;
			undgDataItem.DI_DGVolume = amountBefore;
			undgDataItem.DI_UnitOfVolume = Constants.Volume.Litre;

			AssertNoWarning(undgDataItem.DI_DGVolumeInfo, ExceedMaximumQuantitiesMessage);

			undgDataItem.DI_DGVolume = amountAfter;

			AssertHasWarning(undgDataItem.DI_DGVolumeInfo, ExceedMaximumQuantitiesMessage);
		}

		#endregion

		#endregion

		#region InnerPack

		#region Weight

		public void TestPacklineDangerousGoodsExceedsExceptedQuantity_InnerPack_Solid_E1() =>
			InnerPacklineDangerousSolidGoodsExceedsExceptedQuantity("E1", 20, 10, 40, 60);

		public void TestPacklineDangerousGoodsExceedsExceptedQuantity_InnerPack_Solid_E2() =>
			InnerPacklineDangerousSolidGoodsExceedsExceptedQuantity("E2", 20, 10, 40, 60);

		public void TestPacklineDangerousGoodsExceedsExceptedQuantity_InnerPack_Solid_E3() =>
			InnerPacklineDangerousSolidGoodsExceedsExceptedQuantity("E3", 20, 10, 40, 60);

		public void TestPacklineDangerousGoodsExceedsExceptedQuantity_InnerPack_Solid_E4() =>
			InnerPacklineDangerousSolidGoodsExceedsExceptedQuantity("E4", 0.5, 0.7, 1.2, 1.6);

		public void TestPacklineDangerousGoodsExceedsExceptedQuantity_InnerPack_Solid_E5() =>
			InnerPacklineDangerousSolidGoodsExceedsExceptedQuantity("E5", 0.5, 0.7, 1.2, 1.6);

		void InnerPacklineDangerousSolidGoodsExceedsExceptedQuantity(string exceptedQuantity, double amountBefore1, double amountBefore2, double amountAfter1, double amountAfter2)
		{
			var packline = SetupAndGetForwardingPackline();
			var substance1 = CreateSolidSubstanceOfExceptedQuantityCode(exceptedQuantity, "1234", "a");
			var substance2 = CreateSolidSubstanceOfExceptedQuantityCode(exceptedQuantity, "1234", "b");

			var undgDataItem1 = packline.UNDGs.AddNew();
			undgDataItem1.DI_DG = substance1.PK;
			undgDataItem1.DI_DGWeight = amountBefore1;
			undgDataItem1.DI_UnitOfWeight = Constants.Weight.Grams;

			var undgDataItem2 = packline.UNDGs.AddNew();
			undgDataItem2.DI_DG = substance2.PK;
			undgDataItem2.DI_DGWeight = amountBefore2;
			undgDataItem2.DI_UnitOfWeight = Constants.Weight.Grams;

			AssertNoWarning(undgDataItem1.DI_DGWeightInfo, ExceedMaximumQuantitiesMessage);
			AssertNoWarning(undgDataItem2.DI_DGWeightInfo, ExceedMaximumQuantitiesMessage);

			undgDataItem1.DI_DGWeight = amountAfter1;
			undgDataItem2.DI_DGWeight = amountAfter2;

			AssertHasWarning(undgDataItem1.DI_DGWeightInfo, ExceedMaximumQuantitiesMessage);
			AssertHasWarning(undgDataItem2.DI_DGWeightInfo, ExceedMaximumQuantitiesMessage);
		}

		#region Lithium Battery Permissible Package Weights

		const string LithiumPackageLimitMessage =
			"Lithium ion batteries packed in accordance with Section II of Packing instructions 965 and 968 have a limit of one package per consignment.";

		public void TestPacklineEmptyValuesDoesNotCauseError()
		{
			var lithiumSubstanceCode = LithiumBatteryConstants.UNNOCodes.CodesList.First();
			var substance = CreateNewIATASubstance(lithiumSubstanceCode, "a");
			substance.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965;
			substance.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965;
			substance.DG_LQ2OrPaxMaxAmt = 5;
			substance.DG_LQ2OrPaxMaxAmtUQ = "KG";
			substance.DG_CargoMaxAmt = 5;
			substance.DG_CargoMaxAmtUQ = "KG";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.Transports.RemoveAndDeleteAll();

			var packline = shipment.OuterPackLines.AddNew();
			var container = packline.Containers.AddNew();
			container.JC_ContainerMode = Constants.ContainerModes.ULD;

			var shipmentDGItem = packline.UNDGs.AddNew();
			shipmentDGItem.DI_DG = substance.PK;
			shipmentDGItem.DI_DGWeight = 1;
			shipmentDGItem.DI_F3_NKPackType = "BOX";
			shipmentDGItem.DI_UnitOfWeight = Constants.Weight.Kilograms;

			AssertNoWarnings(shipmentDGItem.DI_DGWeightInfo);
			AssertNoErrors(shipmentDGItem.DI_DGWeightInfo);

			shipmentDGItem.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionII;

			AssertNoWarnings(shipmentDGItem.DI_DGWeightInfo);
			AssertNoErrors(shipmentDGItem.DI_DGWeightInfo);
		}

		public void TestPacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroup_PI965_IA() =>
			PacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroupHelper_PerPackage(LithiumBatteryConstants.RefPackingInstructions.PI965, PackingInstructionSectionTypeList.Codes.SectionIA, 0, 35, 1);

		public void TestPacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroup_PI965_IB() =>
			PacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroupHelper_PerPackage(LithiumBatteryConstants.RefPackingInstructions.PI965, PackingInstructionSectionTypeList.Codes.SectionIB, 0, 10, 1);

		public void TestPacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroup_PI966_I() =>
			PacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroupHelper_PerPackage(LithiumBatteryConstants.RefPackingInstructions.PI966, PackingInstructionSectionTypeList.Codes.SectionI, 5, 35, 1);

		public void TestPacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroup_PI966_II() =>
			PacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroupHelper_PerPackage(LithiumBatteryConstants.RefPackingInstructions.PI966, PackingInstructionSectionTypeList.Codes.SectionII, 5, 5, 1);

		public void TestPacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroup_PI967_I() =>
			PacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroupHelper_PerPackage(LithiumBatteryConstants.RefPackingInstructions.PI967, PackingInstructionSectionTypeList.Codes.SectionI, 5, 35, 1);

		public void TestPacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroup_PI967_II() =>
			PacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroupHelper_PerPackage(LithiumBatteryConstants.RefPackingInstructions.PI967, PackingInstructionSectionTypeList.Codes.SectionII, 5, 5, 1);

		public void TestPacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroup_PI968_IA() =>
			PacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroupHelper_PerPackage(LithiumBatteryConstants.RefPackingInstructions.PI968, PackingInstructionSectionTypeList.Codes.SectionIA, 0, 35, 1);

		public void TestPacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroup_PI968_IB() =>
			PacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroupHelper_PerPackage(LithiumBatteryConstants.RefPackingInstructions.PI968, PackingInstructionSectionTypeList.Codes.SectionIB, 0, 2.5, 1);

		public void TestPacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroup_PI969_I() =>
			PacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroupHelper_PerPackage(LithiumBatteryConstants.RefPackingInstructions.PI969, PackingInstructionSectionTypeList.Codes.SectionI, 5, 35, 1);

		public void TestPacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroup_PI969_II() =>
			PacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroupHelper_PerPackage(LithiumBatteryConstants.RefPackingInstructions.PI969, PackingInstructionSectionTypeList.Codes.SectionII, 5, 5, 1);

		public void TestPacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroup_PI970_I() =>
			PacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroupHelper_PerPackage(LithiumBatteryConstants.RefPackingInstructions.PI970, PackingInstructionSectionTypeList.Codes.SectionI, 5, 35, 1);

		public void TestPacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroup_PI970_II() =>
			PacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroupHelper_PerPackage(LithiumBatteryConstants.RefPackingInstructions.PI970, PackingInstructionSectionTypeList.Codes.SectionII, 5, 5, 1);

		public void TestPacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroup_PI970_I_WhenPackageCountIsGreaterThanOne() =>
			PacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroupHelper_PerPackage(LithiumBatteryConstants.RefPackingInstructions.PI970, PackingInstructionSectionTypeList.Codes.SectionI, 25, 175, 5);

		public void PacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroupHelper_PerPackage(string packingInstruction, string packingInstructionSectionType, ZDecimal passengerFlightLimit, ZDecimal cargoAircraftOnlyLimit, ZInt packageCount)
		{
			var lithiumSubstanceCode = LithiumBatteryConstants.UNNOCodes.CodesList.First();
			var substance1 = CreateNewIATASubstance(lithiumSubstanceCode, "a");
			substance1.DG_CargoPackIns = packingInstruction;
			substance1.DG_PaxPackIns = packingInstruction;
			substance1.DG_LQ2OrPaxMaxAmt = passengerFlightLimit;
			substance1.DG_LQ2OrPaxMaxAmtUQ = "KG";
			substance1.DG_CargoMaxAmt = cargoAircraftOnlyLimit;
			substance1.DG_CargoMaxAmtUQ = "KG";

			var lithiumSubstanceCode2 = LithiumBatteryConstants.UNNOCodes.CodesList.Last();
			var substance2 = CreateNewIATASubstance(lithiumSubstanceCode2, "a");
			substance2.DG_CargoPackIns = packingInstruction;
			substance2.DG_PaxPackIns = packingInstruction;
			substance2.DG_LQ2OrPaxMaxAmt = passengerFlightLimit;
			substance2.DG_LQ2OrPaxMaxAmtUQ = "KG";
			substance2.DG_CargoMaxAmt = cargoAircraftOnlyLimit;
			substance2.DG_CargoMaxAmtUQ = "KG";

			var caoShipment = Factory.New<ForwardingShipment>();
			var caoTransport = caoShipment.Transports.AddNew();
			caoTransport.JW_IsCargoOnly = true;

			var caoPackline1 = caoShipment.OuterPackLines.AddNew();

			var caoShipmentDG1 = caoPackline1.UNDGs.AddNew();
			caoShipmentDG1.DI_DG = substance1.PK;
			caoShipmentDG1.DI_DGWeight = cargoAircraftOnlyLimit;
			caoShipmentDG1.DI_F3_NKPackType = "BOX";
			caoShipmentDG1.DI_UnitOfWeight = Constants.Weight.Kilograms;
			caoShipmentDG1.DI_PackingInstructionSection = packingInstructionSectionType;
			caoShipmentDG1.DI_PackageCount = packageCount;

			var caoPackline2 = caoShipment.OuterPackLines.AddNew();

			var caoShipmentDG2 = caoPackline2.UNDGs.AddNew();
			caoShipmentDG2.DI_DG = substance2.PK;
			caoShipmentDG2.DI_DGWeight = cargoAircraftOnlyLimit;
			caoShipmentDG2.DI_F3_NKPackType = "PLT";
			caoShipmentDG2.DI_UnitOfWeight = Constants.Weight.Kilograms;
			caoShipmentDG2.DI_PackingInstructionSection = packingInstructionSectionType;
			caoShipmentDG2.DI_PackageCount = packageCount;

			AssertPacklineDangerousGoodsPermissableQuantityHasNoErrorsAndWarnings(caoShipmentDG1.DI_DGWeightInfo, packingInstruction, packingInstructionSectionType);

			caoShipmentDG1.DI_DGWeight = cargoAircraftOnlyLimit + 1;

			AssertPacklineDangerousGoodsPermissableQuantityHasErrorsAndWarnings(caoShipmentDG1.DI_DGWeightInfo, packingInstruction, packingInstructionSectionType);

			var paxShipment = Factory.New<ForwardingShipment>();
			var paxTransport = paxShipment.Transports.AddNew();
			paxTransport.JW_IsCargoOnly = false;

			var paxPackline1 = paxShipment.OuterPackLines.AddNew();

			var paxShipmentDG1 = paxPackline1.UNDGs.AddNew();
			paxShipmentDG1.DI_DG = substance1.PK;
			paxShipmentDG1.DI_DGWeight = passengerFlightLimit;
			paxShipmentDG1.DI_F3_NKPackType = "BOX";
			paxShipmentDG1.DI_UnitOfWeight = Constants.Weight.Kilograms;
			paxShipmentDG1.DI_PackingInstructionSection = packingInstructionSectionType;
			paxShipmentDG1.DI_PackageCount = packageCount;

			var paxPackline2 = paxShipment.OuterPackLines.AddNew();

			var paxShipmentDG2 = paxPackline2.UNDGs.AddNew();
			paxShipmentDG2.DI_DG = substance2.PK;
			paxShipmentDG2.DI_DGWeight = passengerFlightLimit;
			paxShipmentDG2.DI_F3_NKPackType = "PLT";
			paxShipmentDG2.DI_UnitOfWeight = Constants.Weight.Kilograms;
			paxShipmentDG2.DI_PackingInstructionSection = packingInstructionSectionType;
			paxShipmentDG2.DI_PackageCount = packageCount;

			AssertPacklineDangerousGoodsPermissableQuantityHasNoErrorsAndWarnings(paxShipmentDG1.DI_DGWeightInfo, packingInstruction, packingInstructionSectionType);

			paxShipmentDG1.DI_DGWeight = passengerFlightLimit + 1;

			AssertPacklineDangerousGoodsPermissableQuantityHasErrorsAndWarnings(paxShipmentDG1.DI_DGWeightInfo, packingInstruction, packingInstructionSectionType);
		}

		void AssertPacklineDangerousGoodsPermissableQuantityHasErrorsAndWarnings(ZPropertyInfo propertyInfo, string packingInstruction, string sectionNumber)
		{
			var errorMessage = "Lithium ion batteries packed in accordance with Section " + sectionNumber + " of Packing Instruction " + packingInstruction + " exceed the quantity allowed.";

			AssertHasError(propertyInfo, errorMessage);
		}

		void AssertPacklineDangerousGoodsPermissableQuantityHasNoErrorsAndWarnings(ZPropertyInfo propertyInfo, string packingInstruction, string sectionNumber)
		{
			var errorMessage = "Lithium ion batteries packed in accordance with Section " + sectionNumber + " of Packing Instruction " + packingInstruction + " exceed the quantity allowed.";

			AssertNoError(propertyInfo, errorMessage);
		}

		public void TestUNDGPermissableQuantityULDCheck()
		{
			var lithiumSubstanceCode = LithiumBatteryConstants.UNNOCodes.CodesList.First();
			var substance = CreateNewIATASubstance(lithiumSubstanceCode, "a");
			substance.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965;
			substance.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965;
			substance.DG_LQ2OrPaxMaxAmt = 5;
			substance.DG_LQ2OrPaxMaxAmtUQ = "KG";
			substance.DG_CargoMaxAmt = 5;
			substance.DG_CargoMaxAmtUQ = "KG";

			var shipment = Factory.New<ForwardingShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_IsCargoOnly = true;

			var packline = shipment.OuterPackLines.AddNew();
			var container = packline.Containers.AddNew();
			container.JC_ContainerMode = Constants.ContainerModes.ULD;

			var shipmentDGItem = packline.UNDGs.AddNew();
			shipmentDGItem.DI_DG = substance.PK;
			shipmentDGItem.DI_DGWeight = 1;
			shipmentDGItem.DI_F3_NKPackType = "BOX";
			shipmentDGItem.DI_UnitOfWeight = Constants.Weight.Kilograms;
			shipmentDGItem.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionII;

			AssertHasWarning(shipmentDGItem.DI_PackingInstructionSectionInfo,
				"Lithium ion batteries packed in accordance with Section II of Packing instructions 965 and 968 must not be loaded into a unit load device(ULD) before being tendered to the airline.");
		}

		public void TestUNDGPermissableQuantityConsigmentCheck_PackagesOnSingleShipment()
		{
			var lithiumSubstanceCode = LithiumBatteryConstants.UNNOCodes.CodesList.First();
			var substance = CreateNewIATASubstance(lithiumSubstanceCode, "a");
			substance.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965;
			substance.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965;
			substance.DG_LQ2OrPaxMaxAmt = 5;
			substance.DG_LQ2OrPaxMaxAmtUQ = "KG";
			substance.DG_CargoMaxAmt = 5;
			substance.DG_CargoMaxAmtUQ = "KG";

			var shipment = Factory.New<ForwardingShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_IsCargoOnly = true;

			var packline = shipment.OuterPackLines.AddNew();
			var shipmentDGItem1 = packline.UNDGs.AddNew();
			shipmentDGItem1.DI_DG = substance.PK;
			shipmentDGItem1.DI_DGWeight = 1;
			shipmentDGItem1.DI_PackageCount = 1;
			shipmentDGItem1.DI_F3_NKPackType = "BOX";
			shipmentDGItem1.DI_UnitOfWeight = Constants.Weight.Kilograms;
			shipmentDGItem1.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionII;

			AssertNoError(shipmentDGItem1.DI_DGInfo,
				LithiumPackageLimitMessage);

			var shipmentDGItem2 = packline.UNDGs.AddNew();
			shipmentDGItem2.DI_DG = substance.PK;
			shipmentDGItem2.DI_DGWeight = 1;
			shipmentDGItem2.DI_PackageCount = 1;
			shipmentDGItem2.DI_F3_NKPackType = "BOX";
			shipmentDGItem2.DI_UnitOfWeight = Constants.Weight.Kilograms;
			shipmentDGItem2.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionII;

			AssertHasError(shipmentDGItem2.DI_DGInfo,
				LithiumPackageLimitMessage);
		}

		public void TestUNDGPermissableQuantityConsigmentCheck_PackagesOnMultipleShipments()
		{
			var lithiumSubstanceCode = LithiumBatteryConstants.UNNOCodes.CodesList.First();
			var substance = CreateNewIATASubstance(lithiumSubstanceCode, "a");
			substance.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965;
			substance.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965;
			substance.DG_LQ2OrPaxMaxAmt = 5;
			substance.DG_LQ2OrPaxMaxAmtUQ = "KG";
			substance.DG_CargoMaxAmt = 5;
			substance.DG_CargoMaxAmtUQ = "KG";

			var shipment = Factory.New<ForwardingShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_IsCargoOnly = true;

			var packline = shipment.OuterPackLines.AddNew();

			var shipmentDGItem1 = packline.UNDGs.AddNew();
			shipmentDGItem1.DI_DG = substance.PK;
			shipmentDGItem1.DI_DGWeight = 1;
			shipmentDGItem1.DI_PackageCount = 1;
			shipmentDGItem1.DI_F3_NKPackType = "BOX";
			shipmentDGItem1.DI_UnitOfWeight = Constants.Weight.Kilograms;
			shipmentDGItem1.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionII;

			AssertNoError(shipmentDGItem1.DI_DGInfo,
				LithiumPackageLimitMessage);

			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.Add(shipment);

			var shipment2 = consol.Shipments.AddNew();
			var packline2 = shipment2.OuterPackLines.AddNew();

			var shipmentDGItem2 = packline2.UNDGs.AddNew();
			shipmentDGItem2.DI_DG = substance.PK;
			shipmentDGItem2.DI_DGWeight = 1;
			shipmentDGItem2.DI_PackageCount = 1;
			shipmentDGItem2.DI_F3_NKPackType = "BOX";
			shipmentDGItem2.DI_UnitOfWeight = Constants.Weight.Kilograms;
			shipmentDGItem2.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionII;

			AssertHasError(shipmentDGItem2.DI_DGInfo,
				LithiumPackageLimitMessage);

			shipmentDGItem2.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionI;

			AssertNoError(shipmentDGItem2.DI_DGInfo,
				LithiumPackageLimitMessage);
		}

		public void TestUNDGPermissableQuantityConsigmentCheck_Forbidden()
		{
			const string errorMessage = "Lithium ion batteries for this Section and Packing Instruction are forbidden.";

			var lithiumSubstanceCode = LithiumBatteryConstants.UNNOCodes.CodesList.First();
			var substance = CreateNewIATASubstance(lithiumSubstanceCode, "a");
			substance.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.Forbidden;
			substance.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructions.Forbidden;
			substance.DG_LQ2OrPaxMaxAmt = 5;
			substance.DG_LQ2OrPaxMaxAmtUQ = "KG";
			substance.DG_CargoMaxAmt = 5;
			substance.DG_CargoMaxAmtUQ = "KG";

			var shipment = Factory.New<ForwardingShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_IsCargoOnly = true;

			var packline = shipment.OuterPackLines.AddNew();

			var shipmentDGItem1 = packline.UNDGs.AddNew();
			shipmentDGItem1.DI_DG = substance.PK;
			shipmentDGItem1.DI_DGWeight = 0;
			shipmentDGItem1.DI_PackageCount = 1;
			shipmentDGItem1.DI_F3_NKPackType = "BOX";
			shipmentDGItem1.DI_UnitOfWeight = Constants.Weight.Kilograms;
			shipmentDGItem1.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionII;

			AssertNoError(shipmentDGItem1.DI_DGInfo, errorMessage);

			shipmentDGItem1.DI_DGWeight = 1;

			AssertHasError(shipmentDGItem1.DI_DGInfo, errorMessage);
		}

		[ExpectNoExceptions]
		public void TestBypassUNDGPermissableQuantitiesCheck_WhenUowIsInvalid()
		{
			const string errorMessage = "Lithium ion batteries packed in accordance with Section I of Packing Instruction 966 exceed the quantity allowed.";

			var lithiumSubstanceCode = LithiumBatteryConstants.UNNOCodes.CodesList.First();
			var substance = CreateNewIATASubstance(lithiumSubstanceCode, "a");
			substance.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.PI966;
			substance.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructions.PI966;
			substance.DG_LQ2OrPaxMaxAmt = 5;
			substance.DG_LQ2OrPaxMaxAmtUQ = "KG";
			substance.DG_CargoMaxAmt = 5;
			substance.DG_CargoMaxAmtUQ = "KG";

			var shipment = Factory.New<ForwardingShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_IsCargoOnly = true;

			var packline = shipment.OuterPackLines.AddNew();

			var shipmentDGItem1 = packline.UNDGs.AddNew();
			shipmentDGItem1.DI_DG = substance.PK;
			shipmentDGItem1.DI_DGWeight = 50;
			shipmentDGItem1.DI_PackageCount = 1;
			shipmentDGItem1.DI_F3_NKPackType = "BOX";
			shipmentDGItem1.DI_UnitOfWeight = "C";

			AssertHasError("Precondition", shipmentDGItem1.DI_UnitOfWeightInfo, "Enter a valid Weight Unit.");
			shipmentDGItem1.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionI;
			AssertNoError(shipmentDGItem1.DI_DGWeightInfo, errorMessage);

			shipmentDGItem1.DI_UnitOfWeight = Constants.Weight.Kilograms;
			AssertNoErrors(shipmentDGItem1.DI_UnitOfWeightInfo);
			AssertHasError(shipmentDGItem1.DI_DGWeightInfo, errorMessage);
		}

		[ExpectNoExceptions]
		public void TestUNDGPermissableQuantitiesShouldNotPassWhenThereExitsInvalidDataOfUnitOfWeightInItems()
		{
			var lithiumSubstanceCode = LithiumBatteryConstants.UNNOCodes.CodesList.First();
			var substance = CreateNewIATASubstance(lithiumSubstanceCode, "a");
			substance.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.PI966;
			substance.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructions.PI966;
			substance.DG_LQ2OrPaxMaxAmt = 5;
			substance.DG_LQ2OrPaxMaxAmtUQ = "KG";
			substance.DG_CargoMaxAmt = 5;
			substance.DG_CargoMaxAmtUQ = "KG";

			var shipment = Factory.New<ForwardingShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_IsCargoOnly = true;

			var packline = shipment.OuterPackLines.AddNew();

			var shipmentDGItem1 = packline.UNDGs.AddNew();
			shipmentDGItem1.DI_DG = substance.PK;
			shipmentDGItem1.DI_DGWeight = 50;
			shipmentDGItem1.DI_PackageCount = 1;
			shipmentDGItem1.DI_F3_NKPackType = "BOX";
			shipmentDGItem1.DI_UnitOfWeight = Constants.Weight.Grams;
			AssertNoErrors(shipmentDGItem1.DI_UnitOfWeightInfo);

			var shipmentDGItem2 = packline.UNDGs.AddNew();
			shipmentDGItem2.DI_DG = substance.PK;
			shipmentDGItem2.DI_DGWeight = 25;
			shipmentDGItem2.DI_PackageCount = 1;
			shipmentDGItem2.DI_F3_NKPackType = "BOX";
			shipmentDGItem2.DI_UnitOfWeight = "2";
			AssertHasError("Precondition", shipmentDGItem2.DI_UnitOfWeightInfo, "Enter a valid Weight Unit.");

			AssertNoExceptionThrown(() => shipmentDGItem1.Validation.ValidateDI_DG());
		}

		[ExpectNoExceptions]
		public void TestUNDGPermissableQuantitiesShouldNotPassWhenOneOfUNDGPermissableQuantittHasInvalidDataOfUnitOfWeightInItem()
		{
			var substance = CreateNewIATASubstance(LithiumBatteryConstants.UNNOCodes.PackedLithiumIonBatteries, "a");
			substance.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.PI966;
			substance.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructions.PI966;
			substance.DG_LQ2OrPaxMaxAmt = 5;
			substance.DG_LQ2OrPaxMaxAmtUQ = "KG";
			substance.DG_CargoMaxAmt = 5;
			substance.DG_CargoMaxAmtUQ = "KG";

			var shipment = Factory.New<ForwardingShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_IsCargoOnly = true;

			var packline = shipment.OuterPackLines.AddNew();

			var shipmentDGItem1 = packline.UNDGs.AddNew();
			shipmentDGItem1.DI_DG = substance.PK;
			shipmentDGItem1.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionII;
			shipmentDGItem1.DI_DGWeight = 50;
			shipmentDGItem1.DI_PackageCount = 1;
			shipmentDGItem1.DI_F3_NKPackType = "BOX";
			shipmentDGItem1.DI_UnitOfWeight = Constants.Weight.Grams;
			AssertNoErrors(shipmentDGItem1.DI_UnitOfWeightInfo);

			var shipmentDGItem2 = packline.UNDGs.AddNew();
			shipmentDGItem2.DI_DG = substance.PK;
			shipmentDGItem2.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionII;
			shipmentDGItem2.DI_DGWeight = 25;
			shipmentDGItem2.DI_PackageCount = 1;
			shipmentDGItem2.DI_F3_NKPackType = "BOX";
			shipmentDGItem2.DI_UnitOfWeight = "2";
			AssertHasError("Precondition", shipmentDGItem2.DI_UnitOfWeightInfo, "Enter a valid Weight Unit.");

			AssertNoExceptionThrown(() => shipmentDGItem1.Validation.ValidateDI_DG());
		}

		#endregion

		#endregion

		#region Volume

		public void TestPacklineDangerousGoodsExceedsExceptedQuantity_InnerPack_Liquid_E1() =>
			InnerPacklineDangerousLiquidGoodsExceedsExceptedQuantity("E1", 0.02, 0.01, 0.04, 0.05);

		public void TestPacklineDangerousGoodsExceedsExceptedQuantity_InnerPack_Liquid_E2() =>
			InnerPacklineDangerousLiquidGoodsExceedsExceptedQuantity("E2", 0.02, 0.01, 0.04, 0.05);

		public void TestPacklineDangerousGoodsExceedsExceptedQuantity_InnerPack_Liquid_E3() =>
			InnerPacklineDangerousLiquidGoodsExceedsExceptedQuantity("E3", 0.02, 0.01, 0.04, 0.05);

		public void TestPacklineDangerousGoodsExceedsExceptedQuantity_InnerPack_Liquid_E4() =>
			InnerPacklineDangerousLiquidGoodsExceedsExceptedQuantity("E4", 0.0008, 0.0006, 0.002, 0.003);

		public void TestPacklineDangerousGoodsExceedsExceptedQuantity_InnerPack_Liquid_E5() =>
			InnerPacklineDangerousLiquidGoodsExceedsExceptedQuantity("E5", 0.0008, 0.0006, 0.002, 0.003);

		void InnerPacklineDangerousLiquidGoodsExceedsExceptedQuantity(string exceptedQuantity, double amountBefore1, double amountBefore2, double amountAfter1, double amountAfter2)
		{
			var packline = SetupAndGetForwardingPackline();
			var substance1 = CreateVolumeSubstanceOfExceptedQuantityCode(exceptedQuantity, "1234", "a");
			var substance2 = CreateVolumeSubstanceOfExceptedQuantityCode(exceptedQuantity, "1234", "b");

			var undgDataItem1 = packline.UNDGs.AddNew();
			undgDataItem1.DI_DG = substance1.PK;
			undgDataItem1.DI_DGVolume = amountBefore1;
			undgDataItem1.DI_UnitOfVolume = Constants.Volume.Litre;

			var undgDataItem2 = packline.UNDGs.AddNew();
			undgDataItem2.DI_DG = substance2.PK;
			undgDataItem2.DI_DGVolume = amountBefore2;
			undgDataItem2.DI_UnitOfVolume = Constants.Volume.Litre;

			AssertNoWarning(undgDataItem1.DI_DGVolumeInfo, ExceedMaximumQuantitiesMessage);
			AssertNoWarning(undgDataItem2.DI_DGVolumeInfo, ExceedMaximumQuantitiesMessage);

			undgDataItem1.DI_DGVolume = amountAfter1;
			undgDataItem2.DI_DGVolume = amountAfter2;

			AssertHasWarning(undgDataItem1.DI_DGVolumeInfo, ExceedMaximumQuantitiesMessage);
			AssertHasWarning(undgDataItem2.DI_DGVolumeInfo, ExceedMaximumQuantitiesMessage);
		}

		#endregion

		#endregion

		#region PackingInstructionSection

		public void TestPackingInstructionSection()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneePK = Guid.NewGuid();
			shipment.JS_ActualWeight = 50m;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_TransportMode = string.Empty;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";

			var packline = shipment.OuterPackLines.AddNew();

			var undgDataItem = packline.UNDGs.AddNew();

			var lithiumSubstance = Factory.New<UNDGSubstance>();
			lithiumSubstance.DG_Code = LithiumBatteryConstants.UNNOCodes.LithiumMetalBatteries;
			lithiumSubstance.DG_UNNO = LithiumBatteryConstants.UNNOCodes.LithiumMetalBatteries;
			undgDataItem.DI_DG = lithiumSubstance.PK;

			lithiumSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.ADN;
			undgDataItem.Validation.ValidateDI_PackingInstructionSection();

			AssertNoErrors(undgDataItem.DI_PackingInstructionSectionInfo);
			Assert(undgDataItem.DI_PackingInstructionSectionInfo.ReadOnly);

			lithiumSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgDataItem.UNDGSubstancePivotCollection.UpdateDefaultPivot(lithiumSubstance);
			undgDataItem.Validation.ValidateDI_PackingInstructionSection();

			AssertHasError(undgDataItem.DI_PackingInstructionSectionInfo, "Please enter a value.");
			AssertEquals(false, undgDataItem.DI_PackingInstructionSectionInfo.ReadOnly);

			undgDataItem.DI_PackingInstructionSection = "XX";
			undgDataItem.Validation.ValidateDI_PackingInstructionSection();

			AssertHasError(undgDataItem.DI_PackingInstructionSectionInfo, "Enter a valid selection.");
			AssertEquals(false, undgDataItem.DI_PackingInstructionSectionInfo.ReadOnly);

			undgDataItem.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionII;
			undgDataItem.Validation.ValidateDI_PackingInstructionSection();

			AssertEquals(false, undgDataItem.DI_PackingInstructionSectionInfo.ReadOnly);

			undgDataItem.Validation.ValidateDI_PackingInstructionSection();
			AssertHasError(undgDataItem.DI_PackingInstructionSectionInfo, "Section II of PI 965/968 is not available for use as per DGR 63rd Edition.");
			AssertHasError(undgDataItem.DI_PackingInstructionSectionInfo, "Enter a valid selection.");

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_TransportMode = Constants.TransportModes.Air;

			var seaTransportLeg = departureConsol.Transports[0];
			seaTransportLeg.JW_TransportMode = Constants.TransportModes.Sea;
			seaTransportLeg.JW_ETD = new ZDateTime(2021, 12, 29, 0, 0, 0);
			seaTransportLeg.JW_LegOrder = 1;
			seaTransportLeg.JW_RL_NKLoadPort = "AUSYD";
			seaTransportLeg.JW_RL_NKDiscPort = "USJFK";

			var firstAirTransportLeg = departureConsol.Transports.AddNew();
			firstAirTransportLeg.JW_TransportMode = Constants.TransportModes.Air;
			firstAirTransportLeg.JW_ETD = new ZDateTime(2021, 12, 25, 0, 0, 0);
			firstAirTransportLeg.JW_LegOrder = 2;
			firstAirTransportLeg.JW_RL_NKLoadPort = "USJFK";
			firstAirTransportLeg.JW_RL_NKDiscPort = "USLAX";

			var secondAirTransportLeg = departureConsol.Transports.AddNew();
			secondAirTransportLeg.JW_TransportMode = Constants.TransportModes.Air;
			secondAirTransportLeg.JW_ETD = new ZDateTime(2022, 1, 1, 0, 0, 0);
			secondAirTransportLeg.JW_LegOrder = 3;
			secondAirTransportLeg.JW_RL_NKLoadPort = "USJFK";
			secondAirTransportLeg.JW_RL_NKDiscPort = "USLAX";

			var seaConsol = shipment.Consols.AddNew();
			seaConsol.JK_TransportMode = Constants.TransportModes.Sea;
			seaConsol.JK_RL_NKLoadPort = "USJFL";

			var seaConsolTransportLeg = seaConsol.Transports[0];
			seaConsolTransportLeg.JW_TransportMode = Constants.TransportModes.Sea;
			seaConsolTransportLeg.JW_ETD = new ZDateTime(2022, 1, 8, 0, 0, 0);
			seaConsolTransportLeg.JW_LegOrder = 1;
			seaConsolTransportLeg.JW_RL_NKLoadPort = "USJFL";
			seaConsolTransportLeg.JW_RL_NKDiscPort = "USLAX";

			undgDataItem.Validation.ValidateDI_PackingInstructionSection();
			AssertHasError(undgDataItem.DI_PackingInstructionSectionInfo, "Section II of PI 965/968 is not available for use as per DGR 63rd Edition.");
			AssertHasError(undgDataItem.DI_PackingInstructionSectionInfo, "Enter a valid selection.");

			undgDataItem.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionII;
			firstAirTransportLeg.JW_ETD = new ZDateTime(2021, 12, 25, 0, 0, 0);
			undgDataItem.Validation.ValidateDI_PackingInstructionSection();
			AssertHasError(undgDataItem.DI_PackingInstructionSectionInfo, "Section II of PI 965/968 is not available for use as per DGR 63rd Edition.");
			Factory.Save();

			var lithiumIonSubstance = Factory.New<UNDGSubstance>();
			lithiumIonSubstance.DG_Code = LithiumBatteryConstants.UNNOCodes.LithiumIonBatteries;
			lithiumIonSubstance.DG_UNNO = LithiumBatteryConstants.UNNOCodes.LithiumIonBatteries;
			lithiumIonSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgDataItem.DI_DG = lithiumIonSubstance.PK;
			undgDataItem.Validation.ValidateDI_PackingInstructionSection();
			AssertHasError(undgDataItem.DI_PackingInstructionSectionInfo, "Please enter a value.");

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_Code = "1001";
			substance.DG_UNNO = "1001";
			undgDataItem.DI_DG = substance.PK;

			undgDataItem.Validation.ValidateDI_PackingInstructionSection();
			AssertEquals(true, undgDataItem.DI_PackingInstructionSection.IsEmpty);
			AssertEquals(true, undgDataItem.DI_PackingInstructionSectionInfo.ReadOnly);
			AssertNoErrors(undgDataItem.DI_PackingInstructionSectionInfo);
		}

		public void TestWeightLimitTypeAsGrossWeightLimit()
		{
			var undgDataItem = PrepareUNDG(isCargoOnlyConsol: false, setupWeight: true, setupVolume: false);
			var substance = undgDataItem.Substance;

			undgDataItem.RunPreSaveValidation();
			string weightLimitAsGrossLimitMessage = "Gross weight is required for this dangerous good substance or article. There is no validation for the maximum gross weight limit.";

			AssertNoWarning(undgDataItem.DI_DGWeightInfo, weightLimitAsGrossLimitMessage);

			substance.DG_LQMaxAmtType = "GLM";
			substance.DG_LQ2OrPaxMaxAmtType = "NLM";
			substance.DG_CargoPackAmtType = "NLM";
			undgDataItem.RunPreSaveValidation();
			AssertNoWarning(undgDataItem.DI_DGWeightInfo, weightLimitAsGrossLimitMessage);

			substance.DG_LQMaxAmtType = "NLM";
			substance.DG_LQ2OrPaxMaxAmtType = "GLM";
			substance.DG_CargoPackAmtType = "NLM";
			undgDataItem.RunPreSaveValidation();
			AssertHasWarning(undgDataItem.DI_DGWeightInfo, weightLimitAsGrossLimitMessage);

			substance.DG_LQMaxAmtType = "NLM";
			substance.DG_LQ2OrPaxMaxAmtType = "NLM";
			substance.DG_CargoPackAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.GLMCode;
			undgDataItem.RunPreSaveValidation();
			AssertHasWarning(undgDataItem.DI_DGWeightInfo, weightLimitAsGrossLimitMessage);
		}

		#region Lithium Battery Permissible Package Packing Instruction

		public void
			TestPacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroupHelper_PackingInstruction_PI965() =>
			PacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroupHelper_PackingInstruction(
				LithiumBatteryConstants.RefPackingInstructions.PI965, PackingInstructionSectionTypeList.Codes.SectionIA,
				PackingInstructionSectionTypeList.Codes.SectionIB, 35);

		public void
			TestPacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroupHelper_PackingInstruction_PI968() =>
			PacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroupHelper_PackingInstruction(
				LithiumBatteryConstants.RefPackingInstructions.PI968, PackingInstructionSectionTypeList.Codes.SectionIA,
				PackingInstructionSectionTypeList.Codes.SectionIB, 35);

		public void PacklineDangerousGoodsExceedsPermissableQuantityAllowedPerPackingGroupHelper_PackingInstruction(string packingInstruction, string beforePackingInstructionSectionType, string afterPackingInstructionSectionType, ZDecimal cargoAircraftOnlyLimit)
		{
			var lithiumSubstanceCode = LithiumBatteryConstants.UNNOCodes.CodesList.First();
			var substance = CreateNewIATASubstance(lithiumSubstanceCode, "a");
			substance.DG_CargoPackIns = packingInstruction;
			substance.DG_PaxPackIns = packingInstruction;
			substance.DG_LQ2OrPaxMaxAmt = 5;
			substance.DG_LQ2OrPaxMaxAmtUQ = "KG";
			substance.DG_CargoMaxAmt = 5;
			substance.DG_CargoMaxAmtUQ = "KG";

			var shipment = Factory.New<ForwardingShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_IsCargoOnly = true;

			var packline = shipment.OuterPackLines.AddNew();

			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.DI_DG = substance.PK;
			undgDataItem.DI_DGWeight = cargoAircraftOnlyLimit;
			undgDataItem.DI_UnitOfWeight = Constants.Weight.Kilograms;
			undgDataItem.DI_PackingInstructionSection = beforePackingInstructionSectionType;
			undgDataItem.DI_PackageCount = 0;

			var warningMessage1 = $"Lithium ion batteries packed in accordance with Section {beforePackingInstructionSectionType} of Packing Instruction {packingInstruction} exceed the quantity allowed.";
			var warningMessage2 = $"Lithium ion batteries packed in accordance with Section {afterPackingInstructionSectionType} of Packing Instruction {packingInstruction} exceed the quantity allowed.";
			AssertHasError(undgDataItem.DI_DGWeightInfo, warningMessage1);

			undgDataItem.DI_PackingInstructionSection = afterPackingInstructionSectionType;
			AssertHasError(undgDataItem.DI_DGWeightInfo, warningMessage2);

			undgDataItem.DI_PackingInstructionSection = beforePackingInstructionSectionType;
			undgDataItem.DI_PackageCount = 1;
			AssertNoError(undgDataItem.DI_DGWeightInfo, warningMessage1);

			undgDataItem.DI_PackingInstructionSection = afterPackingInstructionSectionType;
			AssertHasError(undgDataItem.DI_DGWeightInfo, warningMessage2);
		}

		#endregion

		#endregion

		#region Implementation

		ForwardingPackLine SetupAndGetForwardingPackline()
		{
			var shipment = Factory.New<ForwardingShipment>();
			return shipment.OuterPackLines.AddNew();
		}

		UNDGSubstance CreateSolidSubstanceOfExceptedQuantityCode(string exceptedQuantityCode, string unno, string variant)
		{
			var substance = CreateNewIATASubstance(unno, variant);
			substance.DG_ExceptedQuantityCode = exceptedQuantityCode;
			substance.DG_CargoMaxAmtUQ = "KG";

			return substance;
		}

		UNDGSubstance CreateVolumeSubstanceOfExceptedQuantityCode(string exceptedQuantityCode, string unno, string variant)
		{
			var substance = CreateNewIATASubstance(unno, variant);
			substance.DG_ExceptedQuantityCode = exceptedQuantityCode;
			substance.DG_CargoMaxAmtUQ = "L";

			return substance;
		}

		UNDGSubstance CreateNewIATASubstance(string unno, string variant)
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_UNNO = unno;
			substance.DG_Variant = variant;

			return substance;
		}

		#endregion

		#endregion

		#region Qvalue

		public void TestQValueExceedsWhenDIDGWeightGreaterThanMaxAllowed()
		{
			var message = "Each outer pack of this pack line contains multiple dangerous goods whose 'Q' value is calculated as more than 1 which is not allowed per IATA DGR 5.0.2.11. DGs in this pack line must be split into separate pack lines until 'Q' value is equal to or less than 1.";
			AssertQValueExceeds(message, 5000m, 10m, true);
		}

		public void TestQValueExceedsWhenDIDGVolumeGreaterThanMaxAllowed()
		{
			var message = "Each outer pack of this pack line contains multiple dangerous goods whose 'Q' value is calculated as more than 1 which is not allowed per IATA DGR 5.0.2.11. DGs in this pack line must be split into separate pack lines until 'Q' value is equal to or less than 1.";
			AssertQValueExceeds(message, 500m, 5000m, true);
		}

		public void TestQValueExceedsWhenDIDGWeightNotGreaterThanMaxAllowedAndGreaterThan1()
		{
			var message = "The DG Q-value for this packline is greater than 1, ensure every outer pack has the same DG quantity or the Q-value for each pack is less than or equal to 1";
			AssertQValueExceeds(message, 20m, 10m, false);
		}

		public void TestQValueExceedsWhenDIDGVolumeNotGreaterThanMaxAllowedAndGreaterThan1()
		{
			var message = "The DG Q-value for this packline is greater than 1, ensure every outer pack has the same DG quantity or the Q-value for each pack is less than or equal to 1";
			AssertQValueExceeds(message, 20m, 20m, false);
		}

		void AssertQValueExceeds(string message, ZDecimal weight, ZDecimal volume, bool errorExpected)
		{
			var shipment = CreateShipment();

			var packline = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline.JL_PackageCount = 2;

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_UNNO = "A01";
			substance.DG_LQMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.GLMCode;
			substance.DG_LQMaxAmt = 10;
			substance.DG_LQMaxAmtUQ = Core.Constants.Weight.Kilograms;

			var undg = packline.UNDGs.AddNew();
			undg.DI_PackageCount = 1;
			undg.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			undg.DI_DG = substance.PK;
			undg.DI_DGWeight = weight;
			undg.DI_UnitOfWeight = Constants.Weight.Kilograms;
			undg.DI_DGVolume = volume;
			undg.DI_UnitOfVolume = Constants.Volume.Litre;
			undg.DI_IsLimitedQuantity = true;

			undg.Validation.ValidateAll();

			AssertHasWarning(undg.DI_DGWeightInfo, message);
			AssertHasWarning(undg.DI_DGVolumeInfo, message);
			AssertHasWarning(undg.DI_UnitOfWeightInfo, message);
			AssertHasWarning(undg.DI_UnitOfVolumeInfo, message);
		}

		ForwardingShipment CreateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = "HOUSEBILL001";

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "MAERSK";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit 13";
			shipper.MainAddress.Address2 = "4 Lost Lane";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2000";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "MR Consignee";
			consignee.OH_RL_NKClosestPort = "SGSIN";
			consignee.MainAddress.Address1 = "Unit 1";
			consignee.MainAddress.Address2 = "4 What Lane";
			consignee.MainAddress.City = "Auckland";
			consignee.MainAddress.Postcode = "5022";
			consignee.MainAddress.OA_RN_NKCountryCode = "SG";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_UniqueConsignRef = "CONSOL0001";
			departureConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			departureConsol.JK_RL_NKLoadPort = "AUSYD";
			departureConsol.JK_RL_NKDischargePort = "MYKUL";
			departureConsol.JK_BookingReference = "BKG001";
			departureConsol.JK_MasterBillNum = "081-0000001";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_UniqueConsignRef = "CONSOL0002";
			arrivalConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			arrivalConsol.JK_RL_NKLoadPort = "MYKUL";
			arrivalConsol.JK_RL_NKDischargePort = "SGSIN";
			arrivalConsol.JK_BookingReference = "BKG002";
			arrivalConsol.JK_MasterBillNum = "001-0000001";

			var container = departureConsol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA";

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 1;
			packline1.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packline1.JL_ActualWeight = 2000;
			packline1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packline1.JL_ActualVolume = 1.3;
			packline1.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;

			container.PackLines.Add(packline1);

			var contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "Pumpernickel";
			contact.OC_Phone = "8000 1234";
			contact.OC_OH = shipper.PK;

			var undg = packline1.UNDGs.AddNew();
			undg.DI_DG = CreateUNDGSubstance("0143b", "0143", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, "b").PK;
			undg.DI_OC_DGContact = contact.PK;
			undg.DI_DGVolume = 1;
			undg.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			undg.DI_PackageCount = 1;

			Factory.Save();

			return shipment;
		}

		UNDGSubstance CreateUNDGSubstance(string code, string unno = "", string standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, string variant = "")
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = string.IsNullOrEmpty(unno) ? code.Substring(0, 4) : unno;
			substance.DG_Code = code;
			substance.DG_Standard = standard;
			substance.DG_Variant = variant;

			return substance;
		}

		#endregion
	}
}
