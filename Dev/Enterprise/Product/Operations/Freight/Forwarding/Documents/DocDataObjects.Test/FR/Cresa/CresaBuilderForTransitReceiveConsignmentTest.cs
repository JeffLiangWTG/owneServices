using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	sealed class CresaBuilderForTransitReceiveConsignmentTest : CresaBuilderForTransitWarehouseTest<WhsItemReceiveConsignment>
	{
		#region TestETAAtPortOfArrival

		[TestDate(2022, 04, 19, 10, 0, 0)]
		public void TestETAAtPortOfArrival()
		{
			var now = ZDateTime.Now;
			var outboundRoutingETA = now.AddDays(4);

			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();
			var consignment = Helper.CreateReceiveConsignment("RCN", warehouse.PK);

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);

			var cresa = builder.Build();
			cresa.ValidateAllIncludingChildren();
			AssertEquals("ETA should be the current date if no outbound routing.", now, cresa.ETA);

			var outboundRouting = CresaTestHelper.CreateTransportRouting(Factory, typeof(WhsItemReceiveConsignment), WhsItemReceiveConsignmentSchema.Constants.Prefix, consignment.PK, "V123", "AUBLA", GlbBranch.CurrentBranch.GB_RL_NKHomePort);
			outboundRouting.JW_ETA = outboundRoutingETA;
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var consignmentFromDB = newFactory.Load<WhsItemReceiveConsignment>(consignment.PK);

			builder = new CresaBuilderForTransitReceiveConsignment(consignmentFromDB);
			cresa = builder.Build();
			cresa.ValidateAllIncludingChildren();
			AssertEquals("ETA should be defaulted to outbound routing's ETA.", outboundRoutingETA, cresa.ETA);
		}

		#endregion

		#region TestAddressDetails

		public void TestAddressDetails()
		{
			var consignment = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();

			var consignee = CreateOrganisation("CNE Org", "FRPRS", "52 Florence", "St Clair CT", "Paris", "12121", "FR");
			var consignor = CreateOrganisation("CNR Org", "DKAAL", "Unit 13", "4 Lost Lane", "Aalborg", "2000", "DK");
			var bookingParty = CreateOrganisation("BKP Org", "AUSYD", "ABC Forwarder", "Unit 399", "Sydney", "2050", "AU");
			var warehouseOrg = CreateOrganisation("TW Org", "AUSYD", "ABC TW Org", "Unit 888", "Carlingford", "3002", "AU");

			consignment.Warehouse.WW_OA_WarehouseAddress = warehouseOrg.MainAddress.PK;
			consignment.BookingPartyDocAddress.OrganisationPK = bookingParty.PK;
			consignment.ConsignorDocAddress.OrganisationPK = consignor.PK;
			consignment.ConsigneeDocAddress.OrganisationPK = consignee.PK;

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();
			AssertNotNull(cresa);

			AssertionHelper.AssertAddressData(consignment.ConsigneeDocAddress, cresa.Buyer);
			AssertionHelper.AssertAddressData(consignment.ConsignorDocAddress, cresa.Supplier);
			AssertionHelper.AssertAddressData(consignment.Warehouse.WarehouseAddress, cresa.SendingParty);
			AssertionHelper.AssertAddressData(consignment.BookingPartyDocAddress, cresa.SendingForwarder);
			AssertionHelper.AssertAddressData(consignment.BookingPartyDocAddress, cresa.Agent);
		}

		#endregion

		#region TestPopulateAPPlusCodes

		public void TestPopulateAPPlusCodes()
		{
			var consignment = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();

			var consignee = CreateOrganisation("CNE Org", "FRPRS", "52 Florence", "St Clair CT", "Paris", "12121", "FR");
			var consignor = CreateOrganisation("CNR Org", "DKAAL", "Unit 13", "4 Lost Lane", "Aalborg", "2000", "DK");
			var bookingParty = CreateOrganisation("BKP Org", "AUSYD", "ABC Forwarder", "Unit 399", "Sydney", "2050", "AU");
			var warehouseOrg = CreateOrganisation("TW Org", "AUSYD", "ABC TW Org", "Unit 888", "Carlingford", "3002", "AU");

			consignment.Warehouse.WW_OA_WarehouseAddress = warehouseOrg.MainAddress.PK;
			consignment.BookingPartyDocAddress.OrganisationPK = bookingParty.PK;
			consignment.ConsignorDocAddress.OrganisationPK = consignor.PK;
			consignment.ConsigneeDocAddress.OrganisationPK = consignee.PK;

			AssertionHelper.AssertPopulateAPPlusCodes(() => new CresaBuilderForTransitReceiveConsignment(consignment).Build(), consignment.Warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.SOW, "SendingPartySOW", string.Empty);

			AssertionHelper.AssertPopulateAPPlusCodes(() => new CresaBuilderForTransitReceiveConsignment(consignment).Build(), consignment.ConsigneeDocAddress.Address, OrgCusCode.FranceCodeTypes.SON, "BuyerSON", "BuyerCI5");

			AssertionHelper.AssertPopulateAPPlusCodes(() => new CresaBuilderForTransitReceiveConsignment(consignment).Build(), consignment.ConsignorDocAddress.Address, OrgCusCode.FranceCodeTypes.SON, "SupplierSON", "SupplierCI5");

			AssertionHelper.AssertPopulateAPPlusCodes(() => new CresaBuilderForTransitReceiveConsignment(consignment).Build(), consignment.BookingPartyDocAddress.Address, OrgCusCode.FranceCodeTypes.SON, "SendingForwarderSON", "SendingForwarderCI5");
			AssertionHelper.AssertPopulateAPPlusCodes(() => new CresaBuilderForTransitReceiveConsignment(consignment).Build(), consignment.BookingPartyDocAddress.Address, OrgCusCode.FranceCodeTypes.SOA, "SendingForwarderSOA", "SendingForwarderCI5");
			AssertionHelper.AssertPopulateAPPlusCodes(() => new CresaBuilderForTransitReceiveConsignment(consignment).Build(), consignment.BookingPartyDocAddress.Address, OrgCusCode.FranceCodeTypes.SOW, "SendingForwarderSOW", "SendingForwarderCI5");

			AssertionHelper.AssertPopulateAPPlusCodes(() => new CresaBuilderForTransitReceiveConsignment(consignment).Build(), consignment.BookingPartyDocAddress.Address, OrgCusCode.FranceCodeTypes.SON, "AgentSON", "AgentCI5");
			AssertionHelper.AssertPopulateAPPlusCodes(() => new CresaBuilderForTransitReceiveConsignment(consignment).Build(), consignment.BookingPartyDocAddress.Address, OrgCusCode.FranceCodeTypes.SOA, "AgentSOA", "AgentCI5");
			AssertionHelper.AssertPopulateAPPlusCodes(() => new CresaBuilderForTransitReceiveConsignment(consignment).Build(), consignment.BookingPartyDocAddress.Address, OrgCusCode.FranceCodeTypes.SOW, "AgentSOW", "AgentCI5");
		}

		#endregion

		#region TestPortLocationAreaAndServiceReference

		public void TestPortLocationAreaAndServiceReference()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);

			var bookingParty = CreateOrganisation("BKP Org", "AUSYD", "ABC Forwarder", "Unit 399", "Sydney", "2050", "AU");
			consignment.BookingPartyDocAddress.OrganisationPK = bookingParty.PK;

			var psnCode = consignment.BookingPartyDocAddress.Address.CustomsCodes.AddNew();
			psnCode.OK_CodeType = OrgCusCode.CodeTypes.PortSystemNumber;
			psnCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Reunion;
			psnCode.OK_CustomsRegNo = "Area\\Location";

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();
			AssertNotNull(cresa);

			AssertEquals("PortServiceCodeReference", "", cresa.PortServiceCodeReference);
			AssertEquals("PortLocation", "Location", cresa.PortLocation);
			AssertEquals("PortArea", "Area", cresa.PortArea);
			AssertHasMessageError("Port Service Code mandatory", cresa.PortServiceCodeReferenceInfo, "Port Service Reference is missing from RCN > Booking Party > Organization > Config > Registration Numbers/Codes - type PSR.");

			var psrCode = consignment.BookingPartyDocAddress.Address.CustomsCodes.AddNew();
			psrCode.OK_CodeType = OrgCusCode.CodeTypes.PortServiceReference;
			psrCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Reunion;
			psrCode.OK_CustomsRegNo = "PortServRef";

			cresa = builder.Build();
			AssertEquals("PortServiceCodeReference", "PortServRef", cresa.PortServiceCodeReference);
			AssertNoMessageErrors("Port Service Code mandatory", cresa.PortServiceCodeReferenceInfo);
		}

		#endregion

		#region TestECVReferenceAndCRESAReference

		public void TestECVReferenceAndCRESAReference_NoPANReferences()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			Helper.CreatePortAuthorityReference(consignment.PK, consignment.TablePrefix, "XXX", "ECV REference", "CLR");

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();

			AssertEquals("ECVReference", "", cresa.ECVReference);
			AssertEquals("CRESAReference", "", cresa.CRESAReference);
		}

		public void TestECVReferenceAndCRESAReference_WithPANReference()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var panReference = Helper.CreatePortAuthorityReference(consignment.PK, consignment.TablePrefix, "PAN", "ECV REference", "CLR");
			panReference.CE_EntryLineReference = "CRESA Reference";
			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();

			AssertEquals("ECVReference must come from PAN Reference.", "ECV REference", cresa.ECVReference);
			AssertEquals("CRESAReference must come from PAN reference's line reference.", "CRESA Reference", cresa.CRESAReference);

			cresa.ECVReference = "";
			cresa.CRESAReference = "";
			AssertHasMessageError("ECV has message error when empty after the message was accepcted", cresa.ECVReferenceInfo, "ECV Reference (PAN Reference) is required when sending an amendment or withdrawal.");
			AssertHasMessageError("CRESA has message error when empty after the message was accepcted", cresa.CRESAReferenceInfo, "CRESA Reference (PAN Line Reference) is required when sending an amendment or withdrawal.");
		}

		#endregion

		#region TestTranshipmentAndPortOfArrival

		public void TestTranshipmentAndPortOfArrival()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			consignment.WRC_RL_NKNextDischargePort = "FRMRS";
			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();

			AssertEquals("PortOfTranshipment must be WRC_RL_NKNextDischargePort.", "FRMRS", cresa.PortOfTranshipment.Code);
			AssertEquals("PortOfArrival must be WRC_RL_NKNextDischargePort.", "FRMRS", cresa.PortOfArrival.Code);
			AssertNoMessageErrors("PortOfTranshipment is not empty therefore must not have any errors.", ((Unloco)cresa.PortOfTranshipment).CodeInfo);
			AssertNoMessageErrors("Port Of Arrival is not empty therefore must not have any errors.", ((Unloco)cresa.PortOfTranshipment).CodeInfo);
		}

		public void TestTranshipmentAndPortOfArrival_MandatoryCheck()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();
			AssertHasMessageError("Port Of Transhipment is required", ((Unloco)cresa.PortOfTranshipment).CodeInfo, "Port of Transhipment (Next Discharge Port) is required.");
			AssertHasMessageError("Port Of Arrival is required", ((Unloco)cresa.PortOfArrival).CodeInfo, "Port of Arrival (Next Discharge Port) is required.");
		}

		#endregion

		#region TestGoodsAreSealed

		public void TestGoodsAreSealed_PackagesWithMultipleRTUs()
		{
			var now = Now;
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit1 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			receiveTransportationUnit1.PackageJob.Containers.Single().Container.K0_Seal1 = "Test";
			receiveTransportationUnit2.PackageJob.Containers.Single().Container.K0_Seal1 = "";

			var packageState1 = Helper.CreatePackageState(consignment, 1, "PLT", "PKG1", "ARV", receiveTransportationUnit1);
			var packageState2 = Helper.CreatePackageState(consignment, 1, "PLT", "PKG2", "ARV", receiveTransportationUnit2);
			packageState1.WPS_UnloadedTime = now;
			packageState2.WPS_UnloadedTime = now.AddDays(-1);
			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);

			var cresa = builder.Build();
			cresa.ValidateAllIncludingChildren();
			AssertEquals("Goods sealed must be true.", false, cresa.GoodsSealed);

			receiveTransportationUnit2.PackageJob.Containers.Single().Container.K0_Seal1 = "T2";
			var builderAfterAllRTUsHaveSeal = new CresaBuilderForTransitReceiveConsignment(consignment);

			var cresaAfterAllRTUsHaveSeal = builderAfterAllRTUsHaveSeal.Build();
			AssertEquals("Goods sealed must be true.", true, cresaAfterAllRTUsHaveSeal.GoodsSealed);
		}

		#endregion

		#region TestGoodsDetails

		public void TestGoodsDetails()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);

			var packageState1 = Helper.CreatePackageState(consignment, 1, "PLT", "PKG1", "ARV", receiveTransportationUnit1, volume: 1, volumeUQ: "M3", weight: 2, weightUQ: "KG");
			var packageState2 = Helper.CreatePackageState(consignment, 1, "PLT", "PKG2", "ARV", receiveTransportationUnit2, volume: 3, volumeUQ: "M3", weight: 4, weightUQ: "KG");
			Helper.CreatePackageState(consignment, 1, "BOX", "", "BKD", volume: 2, volumeUQ: "CC", weight: 3, weightUQ: "G");
			packageState1.Package.KP_GoodsDescription = "PKG1 - Description";
			packageState2.Package.KP_GoodsDescription = "PKG2 - Description";
			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();

			AssertEquals("TotalPackCount", 2, cresa.TotalPackCount);
			AssertEquals("PackType", "PLT", cresa.PackType.Code);
			AssertEquals("TotalWeight", 6m, cresa.TotalWeight.Value);
			AssertEquals("TotalWeight Unit", "KG", cresa.TotalWeight.Unit.Code);
			AssertEquals("TotalVolume", 4m, cresa.TotalVolume.Value);
			AssertEquals("TotalVolume Unit", "M3", cresa.TotalVolume.Unit.Code);
			AssertEquals("Goods Description", "PKG1 - Description, PKG2 - Description", cresa.GoodsDescription);

			packageState1.Package.KP_GoodsDescription = "PKG1 - Description";
			packageState2.Package.KP_GoodsDescription = "PKG1 - Description";
			packageState2.Package.KP_F3_NKPackType = "BOX";
			packageState2.Package.KP_WeightUQ = "G";
			packageState2.Package.KP_VolumeUQ = "CC";

			var builderAfterChangingPackDetails = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresaAfterChangingPackDetails = builderAfterChangingPackDetails.Build();

			AssertEquals("TotalPackCount must be number of packs regardless of package type.", 2, cresaAfterChangingPackDetails.TotalPackCount);
			AssertEquals("PackType must be PKG since there are two different pack types in Packages.", "PKG", cresaAfterChangingPackDetails.PackType.Code);
			AssertEquals("TotalWeight must be in KG", 2m, cresaAfterChangingPackDetails.TotalWeight.Value);
			AssertEquals("TotalWeight Unit must be KG", "KG", cresaAfterChangingPackDetails.TotalWeight.Unit.Code);
			AssertEquals("TotalVolume must be in M3", 1.000003m, cresaAfterChangingPackDetails.TotalVolume.Value);
			AssertEquals("TotalVolume Unit must be M3", "M3", cresaAfterChangingPackDetails.TotalVolume.Unit.Code);
			AssertEquals("Goods Description must be unique", "PKG1 - Description", cresaAfterChangingPackDetails.GoodsDescription);
		}

		public void TestGoodsDetails_EmptyDescriptions()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);

			var packageState1 = Helper.CreatePackageState(consignment, 1, "PLT", "PKG1", "ARV", receiveTransportationUnit1, volume: 1, volumeUQ: "M3", weight: 2, weightUQ: "KG");
			var packageState2 = Helper.CreatePackageState(consignment, 1, "PLT", "PKG2", "ARV", receiveTransportationUnit2, volume: 3, volumeUQ: "M3", weight: 4, weightUQ: "KG");
			Helper.CreatePackageState(consignment, 1, "BOX", "", "BKD", volume: 2, volumeUQ: "CC", weight: 3, weightUQ: "G");
			packageState1.Package.KP_GoodsDescription = "";
			packageState2.Package.KP_GoodsDescription = "PKG2 - Description";
			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();
			AssertEquals("Goods Description", "PKG2 - Description", cresa.GoodsDescription);
		}

		public void TestGoodsDetailsValidationTotalVolume()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			Helper.CreatePackageState(consignment, 1, "BOX", "", "ARV", volume: 2, volumeUQ: "CC", weight: 3, weightUQ: "G", receiveUnit: receiveTransportationUnit);

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();

			var totalVolumeErrorMessage = "Total Volume is required.";
			cresa.TotalVolume.Value = 0m;
			cresa.ValidateAllIncludingChildren();
			AssertEquals("Precondition", 0m, cresa.TotalVolume.Value);
			AssertHasMessageError("Total volume should have error message", ((Measurement)cresa.TotalVolume).ValueInfo, totalVolumeErrorMessage);
		}

		public void TestGoodsDetailsValidationTotalWeight()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			Helper.CreatePackageState(consignment, 1, "BOX", "", "ARV", volume: 2, volumeUQ: "CC", weight: 3, weightUQ: "G", receiveUnit: receiveTransportationUnit);

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();

			var totalWeightErrorMessage = "The Ci5 and S)One systems supports only integer value of weight. The weight entered or rounded to nearest kilogram value is zero. Please review the pack line weight and input a valid weight.";
			AssertEquals("Precondition", 0m, cresa.TotalWeight.Value);
			AssertHasMessageError("Total weight should have error message, sum of actual weight is 0", ((Measurement)cresa.TotalWeight).ValueInfo, totalWeightErrorMessage);
		}

		public void TestNoArrivedPackages()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			Helper.CreatePackageState(consignment, 1, "BOX", "", "BKD", volume: 2, volumeUQ: "CC", weight: 3, weightUQ: "G");

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();
			AssertHasMessageError("Error because No Packages are arrived.", cresa.ErrorPlaceHolderInfo, "No Packages have been received.");
		}

		public void TestGoodsDetailsValidationTotalPack()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			Helper.CreatePackageState(consignment, 1, "BOX", "", "BKD", volume: 2, volumeUQ: "CC", weight: 3, weightUQ: "G");

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();

			var totalPackCountErrorMessage = "Packs are required.";
			AssertEquals("Precondition", 0, cresa.TotalPackCount);
			AssertHasMessageError("Total Pack count should have error message", cresa.TotalPackCountInfo, totalPackCountErrorMessage);
			AssertHasMessageError("Error because No Packages are arrived.", cresa.ErrorPlaceHolderInfo, "No Packages have been received.");
		}

		public void TestGoodsDetailsValidationPackType()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			Helper.CreatePackageState(consignment, 1, "BOX", "", "ARV", volume: 2, volumeUQ: "CC", weight: 3, weightUQ: "G", receiveUnit: receiveTransportationUnit);

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();

			var packTypeErrorMessage = "Package Type are required.";
			AssertNoMessageErrors("Pack Type should not have error message", ((CodeDescription)cresa.PackType).CodeInfo);
			AssertEquals("Precondition - By default Pack type will be BOX.", "BOX", cresa.PackType.Code);
			cresa.PackType.Code = "";
			cresa.ValidateAllIncludingChildren();
			AssertHasMessageError("Pack Type should have error message", ((CodeDescription)cresa.PackType).CodeInfo, packTypeErrorMessage);
		}

		public void TestGoodsDetailsValidationGoodsDescription()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();

			var goodsDescriptionErrorMessage = "Goods Description is required";
			AssertEquals("Precondition", "", cresa.GoodsDescription);
			AssertHasMessageError("Goods Description should have error message", cresa.GoodsDescriptionInfo, goodsDescriptionErrorMessage);
		}

		#endregion

		#region TestPackingLines

		public void TestPackingLines()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);

			var packageState1 = Helper.CreatePackageState(consignment, 1, "PLT", "PKG1", "ARV", receiveTransportationUnit1, volume: 1, volumeUQ: "M3", weight: 2, weightUQ: "KG");
			var packageState2 = Helper.CreatePackageState(consignment, 1, "BOX", "", "ARV", receiveTransportationUnit2, volume: 3, volumeUQ: "CC", weight: 4, weightUQ: "G");
			Helper.CreatePackageState(consignment, 1, "BOX", "", "BKD", volume: 2, volumeUQ: "CC", weight: 3, weightUQ: "G");
			packageState1.Package.KP_Length = 5;
			packageState1.Package.KP_Width = 6;
			packageState1.Package.KP_Height = 7;
			packageState1.Package.KP_GoodsDescription = "PKG1 Description ";
			packageState1.Package.KP_MarksAndNumbers = " PKG1 Marks and Numbers";

			packageState2.Package.KP_ExternalReference = "ER2";
			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();
			AssertNotNull(cresa);

			AssertEquals("PackingLines.Count", 2, cresa.PackingLines.Count);

			var packingLines = cresa.PackingLines;

			//Packing Line 1
			var packingLineForPKG1 = packingLines.Single(p => p.PackingLineID == "PKG1");
			AssertEquals("ID (Package 1)", "PKG1", packingLineForPKG1.PackingLineID);
			AssertEquals("Package ID (Package 1)", "", packingLineForPKG1.ReferenceNumber);
			AssertEquals("Packs (Package 1)", 1, packingLineForPKG1.Quantity);
			AssertEquals("Weight (Package 1)", 2m, packingLineForPKG1.Weight.Value);
			AssertEquals("Weight UM (Package 1)", "KG", packingLineForPKG1.Weight.Unit.Code);
			AssertEquals("Volume (Package 1)", 210m, packingLineForPKG1.Volume.Value);
			AssertEquals("Volume UM (Package 1)", "M3", packingLineForPKG1.Volume.Unit.Code);
			AssertEquals("Length (Package 1)", 500m, packingLineForPKG1.Length.Value);
			AssertEquals("Width (Package 1)", 600m, packingLineForPKG1.Width.Value);
			AssertEquals("Height (Package 1)", 700m, packingLineForPKG1.Height.Value);
			AssertEquals("Description (Package 1)", "PKG1 Description", packingLineForPKG1.GoodsDescription);
			AssertEquals("Marks And Numbers (Package 1)", "PKG1 Marks and Numbers", packingLineForPKG1.MarksAndNumbers);

			//Packing Line 2
			var packingLineForPKG2 = packingLines.Single(p => p.PackingLineID == "ER2");
			AssertEquals("ID (Package 2)", "ER2", packingLineForPKG2.PackingLineID);
			AssertEquals("Package ID (Package 2)", "", packingLineForPKG2.ReferenceNumber);
			AssertEquals("Packs (Package 2)", 1, packingLineForPKG2.Quantity);
			AssertEquals("Weight (Package 2)", 0.004m, packingLineForPKG2.Weight.Value);
			AssertEquals("Weight UM (Package 2)", "KG", packingLineForPKG2.Weight.Unit.Code);
			AssertEquals("Volume (Package 2)", 0.000003m, packingLineForPKG2.Volume.Value);
			AssertEquals("Volume UM (Package 2)", "M3", packingLineForPKG2.Volume.Unit.Code);
			AssertEquals("Length (Package 2)", 0m, packingLineForPKG2.Length.Value);
			AssertEquals("Width (Package 2)", 0m, packingLineForPKG2.Width.Value);
			AssertEquals("Height (Package 2)", 0m, packingLineForPKG2.Height.Value);
			AssertEquals("Description (Package 2)", "", packingLineForPKG2.GoodsDescription);
			AssertEquals("Marks And Numbers (Package 2)", "", packingLineForPKG2.MarksAndNumbers);

			packingLineForPKG1.PackingLineID = "";
			cresa.ValidateAllIncludingChildren();
			AssertHasMessageError("Packing line id is required.", packingLineForPKG1.PackingLineIDInfo, "Package ID is required.");

			packingLineForPKG2.PackingLineID = "123456789012345678";
			cresa.ValidateAllIncludingChildren();
			AssertHasMessageError("Packing line id is more than 17 characters", packingLineForPKG2.PackingLineIDInfo, "Package ID is more than 17 characters.");
		}

		public void TestPackingLines_NotInWarehouse()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);

			var packageState1 = Helper.CreatePackageState(consignment, 1, "PLT", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveTransportationUnit1, volume: 1, volumeUQ: "M3", weight: 2, weightUQ: "KG");
			var packageState2 = Helper.CreatePackageState(consignment, 1, "BOX", "", TransitWarehouseStatuses.Codes.Booked, receiveTransportationUnit2, volume: 3, volumeUQ: "CC", weight: 4, weightUQ: "G");
			var packageState3 = Helper.CreatePackageState(consignment, 1, "BOX", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveTransportationUnit2, volume: 3, volumeUQ: "CC", weight: 4, weightUQ: "G");
			Helper.CreatePackageState(consignment, 1, "BOX", "", "BKD", volume: 2, volumeUQ: "CC", weight: 3, weightUQ: "G");
			packageState1.Package.KP_Length = 5;
			packageState1.Package.KP_Width = 6;
			packageState1.Package.KP_Height = 7;
			packageState1.Package.KP_GoodsDescription = "PKG1 Description ";
			packageState1.Package.KP_MarksAndNumbers = " PKG1 Marks and Numbers";

			packageState2.Package.KP_ExternalReference = "ER2";
			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();
			AssertNotNull(cresa);

			AssertEquals("PackingLines.Count", 1, cresa.PackingLines.Count);
			AssertEquals(cresa.PackingLines.First().PackingLineID, "PKG1");
		}

		#region TestPackingLinesValidation

		public void TestPackingLinesValidation_PackageID_Empty()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var packageState = Helper.CreatePackageState(consignment, 1, "BOX", "PKG1", "ARV", volume: 2, volumeUQ: "CC", weight: 3, weightUQ: "G", receiveUnit: receiveTransportationUnit);

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();
			var packingLine = cresa.PackingLines.First();

			packingLine.PackingLineID = "";
			AssertHasMessageError("Packing line id is required.", packingLine.PackingLineIDInfo, "Package ID is required.");
		}

		public void TestPackingLinesValidation_PackageID_Length()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var packageState = Helper.CreatePackageState(consignment, 1, "BOX", "PKG1", "ARV", volume: 2, volumeUQ: "CC", weight: 3, weightUQ: "G", receiveUnit: receiveTransportationUnit);

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();
			var packingLine = cresa.PackingLines.First();

			packingLine.PackingLineID = "123456789012345678";
			AssertHasMessageError("Packing line id is more than 17 characters", packingLine.PackingLineIDInfo, "Package ID is more than 17 characters.");
		}

		public void TestPackingLinesValidation_Packs()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var packageState = Helper.CreatePackageState(consignment, 1, "BOX", "PKG1", "ARV", volume: 2, volumeUQ: "CC", weight: 3, weightUQ: "G", receiveUnit: receiveTransportationUnit);

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();
			var packingLine = cresa.PackingLines.First();

			packingLine.Quantity = 0;
			AssertHasMessageError("Packs are required", packingLine.QuantityInfo, "Packline ID: PKG1 Packs are required.");
		}

		public void TestPackingLinesValidation_PackageType()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var packageState = Helper.CreatePackageState(consignment, 1, "BOX", "PKG1", "ARV", volume: 2, volumeUQ: "CC", weight: 3, weightUQ: "G", receiveUnit: receiveTransportationUnit);

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();
			var packingLine = cresa.PackingLines.First();

			packingLine.PackageType.Code = null;
			AssertHasMessageError("Package Type is required", (packingLine.PackageType as CodeDescription).CodeInfo, "Packline ID: PKG1 Package Type is required.");
		}

		public void TestPackingLinesValidation_Weight()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var packageState = Helper.CreatePackageState(consignment, 1, "BOX", "PKG1", "ARV", volume: 2, volumeUQ: "CC", weight: 3, weightUQ: "G", receiveUnit: receiveTransportationUnit);

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();
			var packingLine = cresa.PackingLines.First();

			packingLine.Weight.Value = 0;
			AssertHasMessageError("Weight is required", packingLine.Weight.ValueInfo, "Packline ID: PKG1 Weight is required.");
		}

		public void TestPackingLinesValidation_Volume()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var packageState = Helper.CreatePackageState(consignment, 1, "BOX", "PKG1", "ARV", volume: 2, volumeUQ: "CC", weight: 3, weightUQ: "G", receiveUnit: receiveTransportationUnit);

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();
			var packingLine = cresa.PackingLines.First();

			packingLine.Volume.Value = 0;
			AssertHasMessageError("Volume is required", packingLine.Volume.ValueInfo, "Packline ID: PKG1 Volume is required.");
		}

		public void TestPackingLinesValidation_GoodsDescription()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var packageState = Helper.CreatePackageState(consignment, 1, "BOX", "PKG1", "ARV", volume: 2, volumeUQ: "CC", weight: 3, weightUQ: "G", receiveUnit: receiveTransportationUnit);

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();
			var packingLine = cresa.PackingLines.First();

			packingLine.GoodsDescription = null;
			AssertHasMessageError("Goods Description is required", packingLine.GoodsDescriptionInfo, "Packline ID: PKG1 Goods Description is required, OVP cannot auto full Goods Description as inner has different Goods Description.");
		}

		#endregion

		#region TestOVPGoodsDescription

		public void TestOVPGoodsDescription_NotEmpty()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);

			var ovpPackageState = Helper.CreateOverpackPackage("OVP1", consignment, receiveTransportationUnit1, rcn: consignment);
			ovpPackageState.Package.KP_Weight = 1;
			ovpPackageState.Package.KP_WeightUQ = "G";
			ovpPackageState.Package.KP_Volume = 1;
			ovpPackageState.Package.KP_VolumeUQ = "M3";

			var itemPackageState1 = Helper.CreatePackageState(consignment, 1, "BOX", "PKG1", "ARV", receiveTransportationUnit2, volume: 3, volumeUQ: "CC", weight: 4, weightUQ: "G");
			Helper.PackPackageIntoHandlingUnit(ovpPackageState, itemPackageState1, ZDateTimeOffset.Now, "XXX", ovpPackageState);
			var itemPackageState2 = Helper.CreatePackageState(consignment, 1, "BOX", "PKG2", "ARV", receiveTransportationUnit2, volume: 3, volumeUQ: "CC", weight: 4, weightUQ: "G");
			Helper.PackPackageIntoHandlingUnit(ovpPackageState, itemPackageState2, ZDateTimeOffset.Now, "XXX", ovpPackageState);

			ovpPackageState.Package.KP_GoodsDescription = "OVP Description";
			itemPackageState2.Package.KP_GoodsDescription = "Inner2 Description";
			itemPackageState1.Package.KP_GoodsDescription = "Inner1 Description";

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();
			var ovpPackingLine = cresa.PackingLines.First(p => p.PackingLineID == "OVP1");

			AssertEquals("OVP Description", ovpPackingLine.GoodsDescription);
		}

		public void TestOVPGoodsDescription_Empty_InnersHaveSameDescription()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);

			var ovpPackageState = Helper.CreateOverpackPackage("OVP1", consignment, receiveTransportationUnit1, rcn: consignment);
			ovpPackageState.Package.KP_Weight = 1;
			ovpPackageState.Package.KP_WeightUQ = "G";
			ovpPackageState.Package.KP_Volume = 1;
			ovpPackageState.Package.KP_VolumeUQ = "M3";

			var itemPackageState1 = Helper.CreatePackageState(consignment, 1, "BOX", "PKG1", "ARV", receiveTransportationUnit2, volume: 3, volumeUQ: "CC", weight: 4, weightUQ: "G");
			Helper.PackPackageIntoHandlingUnit(ovpPackageState, itemPackageState1, ZDateTimeOffset.Now, "XXX", ovpPackageState);
			var itemPackageState2 = Helper.CreatePackageState(consignment, 1, "BOX", "PKG2", "ARV", receiveTransportationUnit2, volume: 3, volumeUQ: "CC", weight: 4, weightUQ: "G");
			Helper.PackPackageIntoHandlingUnit(ovpPackageState, itemPackageState2, ZDateTimeOffset.Now, "XXX", ovpPackageState);

			itemPackageState2.Package.KP_GoodsDescription = "Inner Description";
			itemPackageState1.Package.KP_GoodsDescription = "Inner Description";

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();
			var ovpPackingLine = cresa.PackingLines.First(p => p.PackingLineID == "OVP1");

			AssertEquals("Inner Description", ovpPackingLine.GoodsDescription);
		}

		public void TestOVPGoodsDescription_Empty_InnersHaveDifferentDescription()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);

			var ovpPackageState = Helper.CreateOverpackPackage("OVP1", consignment, receiveTransportationUnit1, rcn: consignment);
			ovpPackageState.Package.KP_Weight = 1;
			ovpPackageState.Package.KP_WeightUQ = "G";
			ovpPackageState.Package.KP_Volume = 1;
			ovpPackageState.Package.KP_VolumeUQ = "M3";

			var itemPackageState1 = Helper.CreatePackageState(consignment, 1, "BOX", "PKG1", "ARV", receiveTransportationUnit2, volume: 3, volumeUQ: "CC", weight: 4, weightUQ: "G");
			Helper.PackPackageIntoHandlingUnit(ovpPackageState, itemPackageState1, ZDateTimeOffset.Now, "XXX", ovpPackageState);
			var itemPackageState2 = Helper.CreatePackageState(consignment, 1, "BOX", "PKG2", "ARV", receiveTransportationUnit2, volume: 3, volumeUQ: "CC", weight: 4, weightUQ: "G");
			Helper.PackPackageIntoHandlingUnit(ovpPackageState, itemPackageState2, ZDateTimeOffset.Now, "XXX", ovpPackageState);

			itemPackageState2.Package.KP_GoodsDescription = "Inner1 Description";
			itemPackageState1.Package.KP_GoodsDescription = "Inner2 Description";

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();
			var ovpPackingLine = cresa.PackingLines.First(p => p.PackingLineID == "OVP1");

			AssertNullOrEmpty(ovpPackingLine.GoodsDescription);
			AssertHasMessageError("Goods Description is required", ovpPackingLine.GoodsDescriptionInfo, "Packline ID: OVP1 Goods Description is required, OVP cannot auto full Goods Description as inner has different Goods Description.");
		}

		#endregion

		#endregion

		#region Implementation

		#region ImplementationForTestCargoReceiptDate

		protected override Cresa GetCresa_PackagesWithNoRTU()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			Helper.CreatePackageState(consignment, 1, "PLT", "PKG1", "BKD");
			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);

			return builder.Build();
		}

		protected override Cresa GetCresa_PackagesWithNonGateInRTU()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			Helper.CreatePackageState(consignment, 1, "PLT", "PKG1", "ARV", receiveTransportationUnit);
			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);

			return builder.Build();
		}

		protected override Cresa GetCresa_PackagesWithSingleGateInRTU()
		{
			var now = Now;
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			receiveTransportationUnit.WRH_GateInTime = now;
			Helper.CreatePackageState(consignment, 1, "PLT", "PKG1", "ARV", receiveTransportationUnit);
			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);

			return builder.Build();
		}

		protected override Cresa GetCresa_PackagesWithMultipleGateInRTU()
		{
			var now = Now;
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			receiveTransportationUnit1.WRH_GateInTime = now;
			var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, warehouse.DefaultLocation.PK);
			receiveTransportationUnit2.WRH_GateInTime = now.AddDays(-1);
			Helper.CreatePackageState(consignment, 1, "PLT", "PKG1", "ARV", receiveTransportationUnit1);
			Helper.CreatePackageState(consignment, 1, "PLT", "PKG2", "ARV", receiveTransportationUnit2);
			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);

			return builder.Build();
		}

		#endregion

		#region ImplementationForTestGoodsInDate

		protected override Cresa GetCresa_PackagesWithNonUnloadedPackage()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			Helper.CreatePackageState(consignment, 1, "PLT", "PKG1", "BKD");
			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);

			return builder.Build();
		}

		protected override Cresa GetCresa_PackagesWithSingleUnloadedPackage()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var packageState = Helper.CreatePackageState(consignment, 1, "PLT", "PKG1", "ARV", receiveTransportationUnit);
			packageState.WPS_UnloadedTime = Now;
			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);

			return builder.Build();
		}

		protected override Cresa GetCresa_PackagesWithMultipleUnloadedPackages()
		{
			var now = Now;
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			receiveTransportationUnit1.WRH_UnloadCompleteTime = now;
			var packageState1 = Helper.CreatePackageState(consignment, 1, "PLT", "PKG1", "ARV", receiveTransportationUnit1);
			var packageState2 = Helper.CreatePackageState(consignment, 1, "PLT", "PKG2", "ARV", receiveTransportationUnit1);
			packageState1.WPS_UnloadedTime = now;
			packageState2.WPS_UnloadedTime = now.AddDays(-1);
			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);

			return builder.Build();
		}

		#endregion

		#region ImplementationForTestOperationalPort

		protected override Cresa GetCresa_WithPortCode()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1");
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "FRMAR";
			consignment.WRC_WW_IntendedWarehouse = warehouse.PK;

			var builderWithWarehouse = new CresaBuilderForTransitReceiveConsignment(consignment);
			return builderWithWarehouse.Build();
		}

		#endregion

		#region ImplementationForTestGoodsReceiptNotes

		protected override Cresa GetCresa_GoodsReceiptNotes()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var deliveryOrderReceiptNote = consignment.Notes.AddNew();
			deliveryOrderReceiptNote.ST_Description = PredefinedNoteTypes.Instance.DeliveryOrderReceiptNotes.Description;
			deliveryOrderReceiptNote.ST_NoteText = "Delivery Order Receipt Notes";

			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			return builder.Build();
		}

		#endregion

		#region ImplementationForTestGoodsAreSealed

		protected override Cresa GetCresa_GoodsAreSealed_PackagesWithNonUnloadedPackage()
		{
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			receiveTransportationUnit.PackageJob.Containers.Single().Container.K0_Seal1 = "Test";
			Helper.CreatePackageState(consignment, 1, "PLT", "PKG1", "BKD");
			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);

			return builder.Build();
		}

		protected override Cresa GetCresa_GoodsAreSealed_PackagesWithSingleUnloadedPackage()
		{
			var now = Now;
			var warehouse = Helper.CreateTRWWarehouse("TWH");
			var consignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			receiveTransportationUnit.PackageJob.Containers.Single().Container.K0_Seal1 = "Test";
			var packageState = Helper.CreatePackageState(consignment, 1, "PLT", "PKG1", "ARV", receiveTransportationUnit);
			packageState.WPS_UnloadedTime = now;
			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);

			return builder.Build();
		}

		#endregion

		protected override ZString ConsignmentJobID => "RCN1";
		protected override ZString SourceType => "TransitReceive";
		protected override ZString ConsignmentType => "RCN";

		protected override Cresa GetCresa()
		{
			var consignment = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			consignment.WRC_JobID = ConsignmentJobID;
			consignment.WRC_RS_NKServiceLevel = "STD";
			var builder = new CresaBuilderForTransitReceiveConsignment(consignment);
			var cresa = builder.Build();

			return cresa;
		}

		#endregion
	}
}
