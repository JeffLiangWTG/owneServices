using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondHeader))]
	sealed class CusInBondHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUniversalCopy()
		{
			var ignoreElementAttributes = (UniversalCopyIgnoreElementAttribute[])typeof(CusInBondHeader).GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), false);
			AssertEquals(1, ignoreElementAttributes.Length);
			AssertEquals(9, ignoreElementAttributes[0].ElementNames.Count);
			AssertCollectionContains(Universal.Constants.Header.UniversalCopyIgnoreElement.AMSMoveHeaders, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(Universal.Constants.Header.UniversalCopyIgnoreElement.CusAddInfos, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(Universal.Constants.Header.UniversalCopyIgnoreElement.CusInBondManifestBills, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(Universal.Constants.Header.UniversalCopyIgnoreElement.CusInBondOceanBills, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(Universal.Constants.Header.UniversalCopyIgnoreElement.CusInBondRegularBills, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(Universal.Constants.Header.UniversalCopyIgnoreElement.PTTMoveHeaders, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(CusInBondHeader.Schema.BH_MessageStatus, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(CusInBondHeader.Schema.BH_ReleaseStatus, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(CusInBondHeader.Schema.BH_JobReference, ignoreElementAttributes[0].ElementNames);

			var movementHeaders = typeof(CusInBondHeader).GetProperty(nameof(CusInBondHeader.MovementHeaders), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			Assert(Attribute.IsDefined(movementHeaders, typeof(UniversalCopyCollectionEntityAttribute)));

			var bills = typeof(CusInBondHeader).GetProperty(nameof(CusInBondHeader.Bills), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			Assert(Attribute.IsDefined(bills, typeof(UniversalCopyCollectionEntityAttribute)));
		}

		[UseSnapshotProtection]
		public void TestShouldPopulateUniqueJobReference()
		{
			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();
			var header1 = factory1.New<CusInBondHeader>();
			var dbConnection = ((CargoWise.Data.IDbConnected)factory1).Connection;
			header1.OnSaving();
			var reference1 = header1.BH_JobReference;
			dbConnection.RollbackTransaction();
			var header2 = factory2.New<CusInBondHeader>();
			factory2.Save();
			var reference2 = header2.BH_JobReference;
			AssertEquals(false, reference1.IsEmpty);
			AssertEquals(false, reference2.IsEmpty);
			AssertEquals(reference1, reference2);
			dbConnection.BeginTransaction();
			AssertEquals(reference1, header1.BH_JobReference);
			factory1.Save();
			AssertNotEquals(reference1, header1.BH_JobReference);
		}

		public void TestForeignDestinationPortName()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "12345", "port1", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort1.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
			Factory.Save();

			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_ForeignDestPortKCode = "12345";

			var foreignDestinationPortName = header.ForeignDestinationPortName;

			AssertEquals("port1", foreignDestinationPortName);
			AssertEquals("Cached", Factory.GetCachedValue("CusInBondHeader|ForeignDestinationPortName|12345", () => ZString.Empty), foreignDestinationPortName);
		}

		public void TestImportLoadPortKCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "12345", "port1", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort1.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
			Factory.Save();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportLoadPortKCode = "12345";

			var importLoadPortKCode = header.ImportLoadPortKCode;
			CombineAssertions(() =>
			{
				AssertNotNull(importLoadPortKCode);
				AssertEquals("port1", importLoadPortKCode.ZZD_Description);
				AssertSame("Cached", Factory.GetCachedValue<ZZRefCusCodeListCombined>("CusInBondHeader|ImportLoadPortKCode|12345", () => null), importLoadPortKCode);
			});
		}

		public void TestValidationModes()
		{
			header.ValidationModes = ValidationModes.InBondLevelArrival;
			Assert(header.IsInBondLevelArrivalValidationMode);
			Assert(!header.IsBillOfLadingArrivalValidationMode);
			Assert(!header.IsContainerArrivalValidationMode);
			Assert(!header.IsInBondLevelExportationValidationMode);
			Assert(!header.IsBillOfLadingExportationValidationMode);
			Assert(!header.IsContainerExportationValidationMode);
			Assert(header.IsArrivalValidationMode);
			Assert(!header.IsExportationValidationMode);
			header.ValidationModes = ValidationModes.BillOfLadingLevelArrival;
			Assert(!header.IsInBondLevelArrivalValidationMode);
			Assert(header.IsBillOfLadingArrivalValidationMode);
			Assert(!header.IsContainerArrivalValidationMode);
			Assert(!header.IsInBondLevelExportationValidationMode);
			Assert(!header.IsBillOfLadingExportationValidationMode);
			Assert(!header.IsContainerExportationValidationMode);
			Assert(header.IsArrivalValidationMode);
			Assert(!header.IsExportationValidationMode);
			header.ValidationModes = ValidationModes.ContainerLevelArrival;
			Assert(!header.IsInBondLevelArrivalValidationMode);
			Assert(!header.IsBillOfLadingArrivalValidationMode);
			Assert(header.IsContainerArrivalValidationMode);
			Assert(!header.IsInBondLevelExportationValidationMode);
			Assert(!header.IsBillOfLadingExportationValidationMode);
			Assert(!header.IsContainerExportationValidationMode);
			Assert(header.IsArrivalValidationMode);
			Assert(!header.IsExportationValidationMode);
			header.ValidationModes = ValidationModes.InBondLevelExportation;
			Assert(!header.IsInBondLevelArrivalValidationMode);
			Assert(!header.IsBillOfLadingArrivalValidationMode);
			Assert(!header.IsContainerArrivalValidationMode);
			Assert(header.IsInBondLevelExportationValidationMode);
			Assert(!header.IsBillOfLadingExportationValidationMode);
			Assert(!header.IsContainerExportationValidationMode);
			Assert(!header.IsArrivalValidationMode);
			Assert(header.IsExportationValidationMode);
			header.ValidationModes = ValidationModes.BillOfLadingLevelExportation;
			Assert(!header.IsInBondLevelArrivalValidationMode);
			Assert(!header.IsBillOfLadingArrivalValidationMode);
			Assert(!header.IsContainerArrivalValidationMode);
			Assert(!header.IsInBondLevelExportationValidationMode);
			Assert(header.IsBillOfLadingExportationValidationMode);
			Assert(!header.IsContainerExportationValidationMode);
			Assert(!header.IsArrivalValidationMode);
			Assert(header.IsExportationValidationMode);
			header.ValidationModes = ValidationModes.ContainerLevelExportation;
			Assert(!header.IsInBondLevelArrivalValidationMode);
			Assert(!header.IsBillOfLadingArrivalValidationMode);
			Assert(!header.IsContainerArrivalValidationMode);
			Assert(!header.IsInBondLevelExportationValidationMode);
			Assert(!header.IsBillOfLadingExportationValidationMode);
			Assert(header.IsContainerExportationValidationMode);
			Assert(!header.IsArrivalValidationMode);
			Assert(header.IsExportationValidationMode);
		}

		public void TestSelectedMovementDetails()
		{
			for (int i = 1; i < 10; i++)
			{
				var moveHeader = header.MovementHeaders.AddNew();
				for (int j = 1; j < 10; j++)
				{
					moveHeader.MovementDetails.AddNew();
				}
			}

			header.SelectedMovementDetail = header.MovementHeaders[6].MovementDetails[5].PK;
			AssertEquals(1, header.SelectedMovementDetails.Count);
			AssertEquals(header.SelectedMovementDetail, header.SelectedMovementDetails[0].PK);
		}

		public void TestIsDiversionRequestMode()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "MB12334";
			header.RecalculateValidationModesOnHeader(InBondMessageType.DiversionRequest);
			Assert(header.IsDiversionRequestMode);
		}

		public void TestMovementHeaderMembers()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "COR";
			org.OH_FullName = "BOB THE BUILDER";
			org.MainAddress.OA_Address1 = "ADDRESS 1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "COR2";
			org2.OH_FullName = "BOB THE BUILDER2";
			org2.MainAddress.OA_Address1 = "ADDRESS 2";
			var carrierCombined = Factory.New<USCarrierCombined>();
			carrierCombined.UI_Code = "ABCD";
			carrierCombined.UI_Name = "KNZ1";

			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "A1A1", "BEACHFRONT AVENUE", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var port1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "5860", "PORT", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(port1.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.InBond);
			Factory.Save();
			var header = Factory.New<CusInBondHeader>();
			var movementHeader1 = header.MovementHeaders.AddNew();
			var detail1 = movementHeader1.MovementDetails.AddNew();
			movementHeader1.BM_InBondClosedDate = ZDateTime.BrettsBirthday;
			movementHeader1.BM_OA_InBondCarrier = org.MainAddress.PK;
			movementHeader1.BM_InBondCarrierSCAC = "ABCD";
			movementHeader1.BM_DestinationPortCode = "A1A1";
			movementHeader1.BM_ForeignDestPortKCode = "5860";
			detail1.B9_InBoundQty = 200;
			AssertEquals(ZDateTime.BrettsBirthday.ToLongTimeString(), header.InBondClosedDate);
			AssertEquals("COR", header.InBondCarrier);
			AssertEquals("ABCD", header.InBondCarrierCode);
			AssertEquals("KNZ1", header.InBondCarrierName);
			AssertEquals("A1A1", header.USDestinationPortCode);
			AssertEquals("BEACHFRONT AVENUE", header.USDestinationPortName);
			AssertEquals("5860", header.ForeignDestinationPortCode);
			AssertEquals("PORT", header.ForeignDestinationPortName);
			AssertEquals(200, header.InBondQTY);
			var movementHeader2 = header.MovementHeaders.AddNew();
			var detail2 = movementHeader2.MovementDetails.AddNew();
			movementHeader2.BM_InBondClosedDate = ZDateTime.BrettsBirthday.AddDays(2);
			movementHeader2.BM_OA_InBondCarrier = org2.MainAddress.PK;
			movementHeader2.BM_InBondCarrierSCAC = "1";
			movementHeader2.BM_DestinationPortCode = "1";
			movementHeader2.BM_ForeignDestPortKCode = "1";
			detail2.B9_InBoundQty = 100;
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(2).ToLongTimeString(), header.InBondClosedDate);
			AssertEquals("MUL", header.InBondCarrier);
			AssertEquals("MUL", header.InBondCarrierCode);
			AssertEquals("MUL", header.InBondCarrierName);
			AssertEquals("MUL", header.USDestinationPortCode);
			AssertEquals("MUL", header.USDestinationPortName);
			AssertEquals("MUL", header.ForeignDestinationPortCode);
			AssertEquals("MUL", header.ForeignDestinationPortName);
			AssertEquals(300, header.InBondQTY);
		}

		public void TestCurrentUserHasBondedWarehouseSecurityAccess()
		{
			Env.Security.USInBondEditBondedWarehouse.IsAllowed = true;
			var header = Factory.New<CusInBondHeader>();
			AssertEquals(true, header.CurrentUserHasBondedWarehouseSecurityAccess);
			Env.Security.USInBondEditBondedWarehouse.IsAllowed = false;
			AssertEquals(false, header.CurrentUserHasBondedWarehouseSecurityAccess);
		}

		public void TestHumanReadableShortcutNameInInBond()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "WISETECHGLOBAL in Alexandria";
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			AssertNotNull("ImporterOrg", header.ImporterOrg);
			AssertEquals("Shortcut Name", header.BH_JobReference + " - " + header.ImporterOrg.OH_FullName, header.HumanReadableShortcutName);
		}

		public void TestSetDefaultValueToAirCarrierCode()
		{
			var refAirline = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, "A2"));
			if (refAirline == null)
			{
				refAirline = Factory.NewWithValidTestData<RefAirline>();
				refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "A8";
				refAirline.RM_TwoCharacterCode = "A2";
			}

			refAirline.RM_ThreeLetterCode = "A00";
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			header.BH_CarrierSCAC = "A2";
			AssertEquals(header.ThreeLetterAirCarrierCode, refAirline.RM_ThreeLetterCode);
			AssertEquals(false, header.ThreeLetterAirCarrierCode_ReadOnly);
			Factory.Save();
			var addOnCarrierCodeQuery = new ZQuery(GenAddOnColumnSchema.XA_ParentID, header.PK);
			addOnCarrierCodeQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, header.TablePrefix);
			addOnCarrierCodeQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, "ThreeLetterAirCarrierCode");
			var addOn = Factory.LoadTop1<GenAddOnColumn>(addOnCarrierCodeQuery);
			AssertNull("Do not create GenAddOnColumn", addOn);
		}

		public void TestRefreshDocAddressesWhenChangeToAIRTransportMode()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();

			AssertEquals(false, bill.ForeignShipper.IgnoreValidationStatusError);
			AssertEquals(false, bill.Consignee.IgnoreValidationStatusError);
			AssertEquals(false, bill.NotifyParty.IgnoreValidationStatusError);

			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			AssertEquals(true, bill.ForeignShipper.IgnoreValidationStatusError);
			AssertEquals(true, bill.Consignee.IgnoreValidationStatusError);
			AssertEquals(true, bill.NotifyParty.IgnoreValidationStatusError);

			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselNonContainer;
			AssertEquals(false, bill.ForeignShipper.IgnoreValidationStatusError);
			AssertEquals(false, bill.Consignee.IgnoreValidationStatusError);
			AssertEquals(false, bill.NotifyParty.IgnoreValidationStatusError);

			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			var bill2 = header.Bills.AddNew();
			var foreignShipperAddress = bill2.DocAddresses.AddNew(DocAddressType.ForeignShipperDocumentaryAddress);
			var consigneeAddress = bill2.DocAddresses.AddNew(DocAddressType.ConsigneeAddress);
			var notifyPartyAddress = bill2.DocAddresses.AddNew(DocAddressType.NotifyParty);
			AssertEquals(false, foreignShipperAddress.IgnoreValidationStatusError);
			AssertEquals(false, consigneeAddress.IgnoreValidationStatusError);
			AssertEquals(false, notifyPartyAddress.IgnoreValidationStatusError);

			bill2.RunPreSaveValidation();
			foreignShipperAddress = bill2.DocAddresses.FindByDocAddressType(DocAddressType.ForeignShipperDocumentaryAddress);
			consigneeAddress = bill2.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeAddress);
			notifyPartyAddress = bill2.DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty);
			AssertEquals(true, foreignShipperAddress.IgnoreValidationStatusError);
			AssertEquals(true, consigneeAddress.IgnoreValidationStatusError);
			AssertEquals(true, notifyPartyAddress.IgnoreValidationStatusError);

			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselNonContainer;
			AssertEquals(false, foreignShipperAddress.IgnoreValidationStatusError);
			AssertEquals(false, consigneeAddress.IgnoreValidationStatusError);
			AssertEquals(false, notifyPartyAddress.IgnoreValidationStatusError);
		}

		public void TestReadOnlySetWhenInactive()
		{
			header.BH_IsActive = false;
			foreach (ZPropertyInfo property in header.ZPropertyInfoHash)
			{
				AssertEquals("All properties should be read-only", true, property.ReadOnly);
			}

			header.BH_IsActive = true;
			AssertEquals(false, header.BH_ImportTransportModeInfo.ReadOnly);
		}

		public void TestBH_PostDepartureOnly_ReadOnly()
		{
			var moveheader1 = header.MovementHeaders.AddNew();
			moveheader1.BM_CustomsStatus = MessageStatusListIT.Codes.AwaitingDepartureOriginal;
			var moveheader2 = header.MovementHeaders.AddNew();
			moveheader2.BM_CustomsStatus = MessageStatusListIT.Codes.ClearDepartureOriginal;
			var moveheader3 = header.MovementHeaders.AddNew();
			moveheader3.BM_CustomsStatus = MessageStatusListIT.Codes.ClearDepartureAmendment;
			var moveheader4 = header.MovementHeaders.AddNew();
			moveheader4.BM_CustomsStatus = MessageStatusListIT.Codes.NotSent;
			AssertEquals(true, header.BH_PostDepartureOnly_ReadOnly);
			moveheader1.BM_CustomsStatus = MessageStatusListIT.Codes.NotSent;
			moveheader2.BM_CustomsStatus = MessageStatusListIT.Codes.NotSent;
			moveheader3.BM_CustomsStatus = MessageStatusListIT.Codes.NotSent;
			AssertEquals(false, header.BH_PostDepartureOnly_ReadOnly);
		}

		public void TestCanCancelInBond()
		{
			Assert(string.IsNullOrEmpty(header.CanCancel()));
			var moveheader1 = header.MovementHeaders.AddNew();
			moveheader1.BM_CustomsStatus = MessageStatusListIT.Codes.AwaitingDepartureOriginal;
			var moveheader2 = header.MovementHeaders.AddNew();
			moveheader2.BM_CustomsStatus = MessageStatusListIT.Codes.ClearDepartureOriginal;
			var moveheader3 = header.MovementHeaders.AddNew();
			moveheader3.BM_CustomsStatus = MessageStatusListIT.Codes.ErrorDepartureOriginal;
			var moveheader4 = header.MovementHeaders.AddNew();
			moveheader4.BM_CustomsStatus = MessageStatusListIT.Codes.NotSent;
			AssertContains("This record cannot be deactivated as there are Movements that are still waiting for a response from Customs.", header.CanCancel());
			moveheader1.Delete();
			AssertContains("This record cannot be deactivated as there are Movements that have been accepted by Customs.", header.CanCancel());
			moveheader2.BM_CustomsStatus = MessageStatusListIT.Codes.ClearDepartureWithdraw;
			Assert(string.IsNullOrEmpty(header.CanCancel()));
			moveheader2.Delete();
			Assert(string.IsNullOrEmpty(header.CanCancel()));
		}

		public void TestICustomLabelsConfigOrgProviderMembers()
		{
			var header = Factory.New<CusInBondHeader>();
			var changedCount = 0;
			ICustomLabelsConfigOrgProvider provider = header;
			provider.ConfigOrgChanged += (object sender, EventArgs e) =>
			{
				changedCount++;
			};
			AssertNull("Without a config org should return null", provider.ConfigOrg);
			AssertEquals(0, changedCount);
			var importer = OrgHeader.New(Factory);
			header.BH_OA_Importer = importer.MainAddress.PK;
			AssertEquals("ConfigOrg is Importer", importer.PK, provider.ConfigOrg.PK);
			AssertEquals(1, changedCount);
		}

		public void TestCaclulatedProperties()
		{
			var loadPort = Factory.New<RefUNLOCO>();
			loadPort.RL_Code = "AU!23";
			var loadPortMap = loadPort.RefLocoMaps.AddNew();
			loadPortMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			loadPortMap.RY_LocalPortCode = "12!AU";
			loadPortMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			var discPort = Factory.New<RefUNLOCO>();
			discPort.RL_Code = "US!23";
			var discPortMap = discPort.RefLocoMaps.AddNew();
			discPortMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			discPortMap.RY_LocalPortCode = "1!US";
			discPortMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCD;
			var header = Factory.New<CusInBondHeader>();
			AssertEquals("header.BH_Calc_FreightTransportMode", ZString.Empty, header.BH_Calc_FreightTransportMode);
			var inBondTransportModeCodes = new[]
			{
				InBondTransportModeCodes.Codes.AirNonContainer,
				InBondTransportModeCodes.Codes.RailNonContainer,
				InBondTransportModeCodes.Codes.TruckNonContainer,
				InBondTransportModeCodes.Codes.VesselContainer,
				InBondTransportModeCodes.Codes.VesselNonContainer,
				InBondTransportModeCodes.Codes.FixedTransportInstallations,
				"#@"
			};
			var expectedTransportTypeList = new[]
			{
				US.Business.TransportTypeList.Codes.Air,
				US.Business.TransportTypeList.Codes.Rail,
				US.Business.TransportTypeList.Codes.Truck,
				US.Business.TransportTypeList.Codes.Sea,
				US.Business.TransportTypeList.Codes.Sea,
				US.Business.TransportTypeList.Codes.FixedTransportInstallations,
				""
			};
			for (var i = 0; i < expectedTransportTypeList.Length; i++)
			{
				header.BH_ImportTransportMode = inBondTransportModeCodes[i];
				AssertEquals(inBondTransportModeCodes[i], expectedTransportTypeList[i], header.BH_Calc_FreightTransportMode);
			}

			AssertEquals("header.BH_Calc_ImportLoadPortUNLOCO", ZString.Empty, header.BH_Calc_ImportLoadPortUNLOCO);
			header.BH_ImportLoadPortKCode = "12!AU";
			AssertEquals("header.BH_Calc_ImportLoadPortUNLOCO", "AU!23", header.BH_Calc_ImportLoadPortUNLOCO);
			AssertEquals("header.BH_Calc_PortUnladingUNLOCO", ZString.Empty, header.BH_Calc_PortUnladingUNLOCO);
			header.BH_PortUnladingDCode = "1!US";
			AssertEquals("header.BH_Calc_PortUnladingUNLOCO", "US!23", header.BH_Calc_PortUnladingUNLOCO);
			header.BH_ImportLoadPortKCode = ZString.Empty;
			var bill = header.Bills.AddNew();
			header.BH_CarrierSCAC = "XXXX";
			header.MovementHeaders.AddNew().BM_InBondCarrierSCAC = "XXXX";
			header.BH_FTZMove = true;
			AssertEquals(ZString.Empty, header.BH_CarrierSCAC);
			AssertEquals("XXXX", header.MovementHeaders[0].BM_InBondCarrierSCAC);
			AssertEquals(CusInBondBill.FTZForeignPortOfLading, header.Bills[0].B0_PortOfLadingKCode);
		}

		public void TestHasAtLeastOneMovementMarkedForBondedWarhousing()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var helper = new WhsDataTestHelper(Factory);
			helper.GetNewWhsWarehouse(importer.MainAddress.PK, true, "HO2");
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = true;
			AssertEquals(false, header.HasAtLeastOneMovementMarkedForBondedWarhousing);
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCanceled;
			AssertEquals(false, header.HasAtLeastOneMovementMarkedForBondedWarhousing);
			var moveHeader2 = header.MovementHeaders.AddNew();
			AssertEquals(false, header.HasAtLeastOneMovementMarkedForBondedWarhousing);
			moveHeader2.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			AssertEquals(true, header.HasAtLeastOneMovementMarkedForBondedWarhousing);
			moveHeader2.BM_WarehouseTransactionStatus = "";
			AssertEquals(false, header.HasAtLeastOneMovementMarkedForBondedWarhousing);
			moveHeader1.BM_OA_WarehouseAddress = importer.MainAddress.PK;
			AssertEquals(true, header.HasAtLeastOneMovementMarkedForBondedWarhousing);
			header.BH_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-2);
			AssertEquals(true, header.HasAtLeastOneMovementMarkedForBondedWarhousing);
		}

		public void TestHasAtLeastOneMovementWithWHSTransaction()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = true;
			AssertEquals(false, header.HasAtLeastOneMovementWithWHSTransaction);
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCanceled;
			AssertEquals(false, header.HasAtLeastOneMovementWithWHSTransaction);
			var moveHeader2 = header.MovementHeaders.AddNew();
			AssertEquals(false, header.HasAtLeastOneMovementWithWHSTransaction);
			moveHeader2.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			AssertEquals(true, header.HasAtLeastOneMovementWithWHSTransaction);
			header.BH_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-2);
			AssertEquals(true, header.HasAtLeastOneMovementWithWHSTransaction);
		}

		public void TestBH_FTZMoveChanged()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = false;
			header.BH_CarrierSCAC = "SCAC";
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			bill1.B0_IssuerCode = "";
			bill2.B0_IssuerCode = "XXXX";
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.BM_InBondCarrierSCAC = "ABCD";
			AssertEquals("", bill1.B0_IssuerCode);
			AssertEquals("XXXX", bill2.B0_IssuerCode);
			header.BH_FTZMove = true;
			AssertEquals("ABCD", moveHeader1.BM_InBondCarrierSCAC);
			AssertEquals("ABCD", bill1.B0_IssuerCode);
			AssertEquals("ABCD", bill2.B0_IssuerCode);
		}

		public void TestBH_FTZMoveAndBH_USeFirmsCodeForBillIssuer()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = false;
			header.BH_CarrierSCAC = "ABCD";
			var bill1 = header.Bills.AddNew();
			bill1.B0_IssuerCode = "";
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.BM_InBondCarrierSCAC = "EFGH";
			header.BH_FTZMove = false;
			bill1.B0_IssuerCode = ZString.Empty;
			AssertEquals("", bill1.B0_IssuerCode);
			header.BH_FIRMS = "W256";
			header.BH_FTZMove = true;
			header.BH_FTZMoveInfo.RefreshBinding();
			AssertEquals("should be same as FIRMS", "EFGH", bill1.B0_IssuerCode);
			AssertEquals("EFGH", moveHeader1.BM_InBondCarrierSCAC);
			AssertNoMessageError(bill1.B0_IssuerCodeInfo, Business.ValidationConstants.Bill.IssuerCodeShouldBeTheSameAsFirms.ToString());
			bill1.B0_IssuerCode = "W256";
			AssertNoMessageError(bill1.B0_IssuerCodeInfo, Business.ValidationConstants.Bill.IssuerCodeShouldBeTheSameAsFirms.ToString());
			bill1.B0_IssuerCode = "TSDP";
			AssertNoMessageError(bill1.B0_IssuerCodeInfo, Business.ValidationConstants.Bill.IssuerCodeShouldBeTheSameAsFirms.ToString());
			bill1.B0_IssuerCode = "W200";
			AssertHasMessageError(bill1.B0_IssuerCodeInfo, Business.ValidationConstants.Bill.IssuerCodeShouldBeTheSameAsFirms.ToString());
		}

		public void TestParentBusinessObjectsWithRelatedEvents()
		{
			var dec = Factory.New<JobDeclaration>();
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_ParentID = dec.PK;
			header1.BH_ParentTableCode = dec.TablePrefix;
			var moveHeader1 = header1.MovementHeaders.AddNew();
			Assert(dec.BusinessObjectsWithRelatedEvents.Contains(header1));
			Assert(dec.BusinessObjectsWithRelatedEvents.Contains(moveHeader1));
			var shipment = Factory.New<ForwardingShipment>();
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_ParentID = shipment.PK;
			header2.BH_ParentTableCode = shipment.TablePrefix;
			var moveHeader2 = header2.MovementHeaders.AddNew();
			Assert(shipment.BusinessObjectsWithRelatedEvents.Contains(header2));
			Assert(shipment.BusinessObjectsWithRelatedEvents.Contains(moveHeader2));
		}

		public void TestIControllerIDProviderMembers()
		{
			var header = Factory.New<CusInBondHeader>();
			IControllerIDProvider provider = header;
			AssertEquals("ControllerID", ControllerIDs.Customs.US.InBond, provider.ControllerID);
			AssertEquals("BusinessObjectPK", header.PK.ToGuid(), provider.BusinessObjectPK);
			var shipment = Factory.New<ForwardingShipment>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			AssertEquals("ControllerID", ControllerIDs.JobShipment, provider.ControllerID);
			AssertEquals("BusinessObjectPK", shipment.PK.ToGuid(), provider.BusinessObjectPK);
		}

		public void TestDefault()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			AssertEquals(CusInBondApplicationCodeList.Codes.InBond, header.BH_ApplicationCode);
			AssertEquals(InBondHeaderTypeList.Codes.AMS, header.BH_HeaderType);
			AssertEquals(GlbBranch.CurrentBranch.PK, header.BH_GB);
		}

		public void TestHeaderType()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = ZString.Empty;
			AssertEquals(true, header.IsDetailedInBond);
			AssertEquals(false, header.IsDocumentOnly);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			AssertEquals(false, header.IsDetailedInBond);
			AssertEquals(false, header.IsDocumentOnly);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			AssertEquals(false, header.IsDetailedInBond);
			AssertEquals(true, header.IsDocumentOnly);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			AssertEquals(true, header.IsDetailedInBond);
			AssertEquals(false, header.IsDocumentOnly);
		}

		public void TestIsStandAlone()
		{
			var testInBondHeader = Factory.New<CusInBondHeader>();
			AssertEquals("Stand alone In-bond", true, testInBondHeader.IsStandAlone);
			var shipment = Factory.New<ForwardingShipment>();
			testInBondHeader.BH_ParentID = shipment.PK;
			testInBondHeader.BH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			AssertEquals("Pluged-in In-Bond", false, testInBondHeader.IsStandAlone);
		}

		public void TestHumanReadableName()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			AssertEquals("", header.HumanReadableName);
			header.BH_JobReference = "BF234322";
			AssertEquals("BF234322", header.HumanReadableName);
		}

		public void TestIHaveRequiredDocumentsMembers()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_JobReference = "BH234232";
			IHaveRequiredDocuments haveRequiredDocuments = header;
			AssertNull(haveRequiredDocuments.AdditionalRefTypes);
			AssertNull(haveRequiredDocuments.ExportBroker);
			AssertEquals(ZString.Empty, haveRequiredDocuments.HouseBill);
			AssertEquals(header.Logs, haveRequiredDocuments.Logs);
			AssertEquals(ZString.Empty, haveRequiredDocuments.MasterBill);
			AssertEquals(header.PK, haveRequiredDocuments.PK);
			AssertEquals(header.RequiredDocuments, haveRequiredDocuments.RequiredDocuments);
			AssertEquals(header.TablePrefix, haveRequiredDocuments.TableCode);
			AssertEquals(header, haveRequiredDocuments.UltimateDocumentParent);
			AssertEquals("BH234232", haveRequiredDocuments.UniqueConsignRef);
		}

		public void TestBH_OverrideFreightDefaults()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_MasterBillNum = "302034322";
			consol.Transports[0].JW_Vessel = "APL VESSEL";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			header.Synchroniser.Synchronise(true);
			AssertEquals("APL VESSEL", header.BH_ImportConveyanceName);
			shouldCancel = true;
			header.OnOverrideFreightDefaultsChanging += header_OnOverrideFreightDefaultsChanging;
			header.BH_OverrideFreightDefaults = ZBool.True;
			AssertEquals("APL VESSEL", header.BH_ImportConveyanceName);
			AssertEquals(ZBool.True, header.BH_OverrideFreightDefaults);
			header.BH_ImportConveyanceName = "HELLO WORLD";
			AssertEquals("HELLO WORLD", header.BH_ImportConveyanceName);
			AssertEquals(ZBool.True, header.BH_OverrideFreightDefaults);
			header.BH_OverrideFreightDefaults = ZBool.False;
			AssertEquals("HELLO WORLD", header.BH_ImportConveyanceName);
			AssertEquals(ZBool.True, header.BH_OverrideFreightDefaults);
			shouldCancel = false;
			header.BH_OverrideFreightDefaults = ZBool.False;
			AssertEquals("APL VESSEL", header.BH_ImportConveyanceName);
			AssertEquals(ZBool.False, header.BH_OverrideFreightDefaults);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var header = Factory.New<CusInBondHeader>();
			var list = header.BusinessObjectsWithRelatedEvents;
			AssertEquals(0, list.Length);
			var moveHeader1 = header.MovementHeaders.AddNew();
			var moveHeader2 = header.MovementHeaders.AddNew();
			list = header.BusinessObjectsWithRelatedEvents;
			AssertEquals(2, list.Length);
			AssertEquals(moveHeader1, list[0]);
			AssertEquals(moveHeader2, list[1]);
		}

		public void TestBranch()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			AssertEquals(GlbBranch.CurrentBranch.PK, header.Branch.PK);
		}

		public void TestIsTruck()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			AssertEquals(false, header.IsTruck);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
			AssertEquals(false, header.IsTruck);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.TruckNonContainer;
			AssertEquals(true, header.IsTruck);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.TruckContainer;
			AssertEquals(true, header.IsTruck);
		}

		public void TestIsRail()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			AssertEquals(false, header.IsRail);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
			AssertEquals(false, header.IsRail);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.RailNonContainer;
			AssertEquals(true, header.IsRail);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.RailContainer;
			AssertEquals(true, header.IsRail);
		}

		public void TestInBondNumbers()
		{
			AssertEquals(ZString.Empty, header.InBondNumbers);
			var moveHeader1 = header.MovementHeaders.AddNew();
			var moveHeader2 = header.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "0501334985";
			AssertEquals("0501334985", header.InBondNumbers);
			moveHeader2.InBondNumber = "0501334992";
			AssertEquals("Multiple InBond Nos.", header.InBondNumbers);
		}

		public void TestInBondStatus()
		{
			AssertEquals(ZString.Empty, header.InBondQPStatus);
			var moveHeader1 = header.MovementHeaders.AddNew();
			var moveHeader2 = header.MovementHeaders.AddNew();
			moveHeader1.BM_CustomsStatus = MessageStatusListIT.Codes.ClearDepartureOriginal;
			AssertEquals("CDO", header.InBondQPStatus);
			moveHeader2.BM_CustomsStatus = MessageStatusListIT.Codes.ClearDepartureOriginal;
			AssertEquals("multiple movements with the same QP status should show that status code", "CDO", header.InBondQPStatus);
			moveHeader2.BM_CustomsStatus = MessageStatusListIT.Codes.AwaitingDepartureWithdraw;
			AssertEquals("multiple movements with varying QP status should show the multiple status value", "Multiple Status", header.InBondQPStatus);

			AssertEquals("", header.InBondWPStatus);
			moveHeader1.BM_MessageStatus = MessageStatusListIT.Codes.AwaitingArrival;
			AssertEquals("AAV", header.InBondWPStatus);
			moveHeader2.BM_MessageStatus = MessageStatusListIT.Codes.AwaitingArrival;
			AssertEquals("multiple movements with the same WP status should show that status code", "AAV", header.InBondWPStatus);
			moveHeader2.BM_MessageStatus = MessageStatusListIT.Codes.AwaitingExportation;
			AssertEquals("multiple movements with varying WP status should show the multiple status value", "Multiple Status", header.InBondWPStatus);
		}

		public void TestInBondStatusDescription()
		{
			AssertEquals("Not Sent", header.InBondQPStatusDescription);
			var moveHeader1 = header.MovementHeaders.AddNew();
			var moveHeader2 = header.MovementHeaders.AddNew();
			moveHeader1.BM_CustomsStatus = MessageStatusListIT.Codes.ClearDepartureOriginal;
			AssertEquals("Clear In-Bond Departure Add", header.InBondQPStatusDescription);
			moveHeader2.BM_CustomsStatus = MessageStatusListIT.Codes.ClearDepartureOriginal;
			AssertEquals("multiple movements with the same status should show that status description", "Clear In-Bond Departure Add", header.InBondQPStatusDescription);
			moveHeader1.BM_CustomsStatus = MessageStatusListIT.Codes.AwaitingDepartureWithdraw;
			AssertEquals("multiple movements with varying status should description as blank", "Multiple Status", header.InBondQPStatusDescription);
		}

		public void TestImporterName()
		{
			AssertEquals(ZString.Empty, header.ImporterName);
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "A&S FURNISHING CO LTD";
			org.MainAddress.OA_Address1 = "UNIT A1, 4TH FLOOR, PIONEER IND BLDG";
			org.MainAddress.OA_City = "HONG KONG";
			header.ImporterOrgPK = org.PK;
			AssertEquals("A&S FURNISHING CO LTD", header.ImporterName);
		}

		public void TestInBondEntryTypes()
		{
			AssertEquals(ZString.Empty, header.InBondEntryTypes);
			var moveHeader1 = header.MovementHeaders.AddNew();
			var moveHeader2 = header.MovementHeaders.AddNew();
			moveHeader1.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertEquals("61", header.InBondEntryTypes);
			moveHeader2.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertEquals("Movements with same type should show that type.", "61", header.InBondEntryTypes);
			var moveHeader3 = header.MovementHeaders.AddNew();
			moveHeader3.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			AssertEquals("Multiple Entry Types", header.InBondEntryTypes);
		}

		public void TestJobReferenceVisible()
		{
			CusInBondHeader testInBondHeader = Factory.New<CusInBondHeader>();
			AssertEquals("Job Reference Number should not show until it has a value", false, testInBondHeader.JobReferenceVisible);
			testInBondHeader.BH_JobReference = "INB0000010";
			AssertEquals("Stand-alone In-Bond show the Job Reference Number.", true, testInBondHeader.JobReferenceVisible);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			testInBondHeader.BH_ParentID = shipment.PK;
			testInBondHeader.BH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			AssertEquals("Plugged in In-Bond should not show the Job Reference Number as Shipment No is already shown on the Parent control.", false, testInBondHeader.JobReferenceVisible);
		}

		public void TestSavingSetsJobReference()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			CusInBondHeader header = factory.New<CusInBondHeader>();
			ZString jobReference = Env.NumberFountains.USInBondJobReference.PeekPreliminaryFormatted(factory);
			AssertNotEquals(ZString.Empty, jobReference);
			AssertEquals("", header.BH_JobReference);
			factory.Save();
			AssertEquals(jobReference, header.BH_JobReference);
		}

		public void TestSavingInbondBeforeShipmentStillSetsJobReference()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var shipment = factory.New<ForwardingShipment>();
			AssertEquals("Pre-condition: Shipment Reference should be blank before saving", ZString.Empty, shipment.JobNumber);
			var inBondHeader = factory.New<CusInBondHeader>();
			inBondHeader.BH_ParentID = shipment.PK;
			inBondHeader.BH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			AssertEquals("Pre-condition: In-Bond Reference should be blank before saving", ZString.Empty, inBondHeader.BH_JobReference);
			inBondHeader.Factory.Save();
			AssertEquals("Shipment Reference should have been created", false, shipment.JobNumber.Length == 0);
			AssertEquals("In-Bond Reference should have been created", false, inBondHeader.BH_JobReference.IsEmpty);
			AssertEquals("In-Bond Reference should be the Shipment Reference", shipment.JobNumber, inBondHeader.BH_JobReference);
		}

		public void TestShipmentReferenceNumberIsAssignedOnSaving()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			CusInBondHeader testInBondHeader = Factory.New<CusInBondHeader>();
			testInBondHeader.BH_ParentID = shipment.PK;
			testInBondHeader.BH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Factory.Save();
			AssertEquals("Shipment Number is assigned as In-Bond reference no.", shipment.JS_UniqueConsignRef, testInBondHeader.BH_JobReference);
		}

		public void TestDeclaration()
		{
			var testForeignCompany = Factory.NewWithValidTestData<GlbCompany>();
			var localBranch = Factory.NewWithValidTestData<GlbBranch>();
			localBranch.GB_Code = "LAX";
			localBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			var sisterBranch = Factory.NewWithValidTestData<GlbBranch>();
			sisterBranch.GB_Code = "SFO";
			sisterBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			var anotherCompanyBranch = Factory.NewWithValidTestData<GlbBranch>();
			anotherCompanyBranch.GB_GC = testForeignCompany.PK;
			anotherCompanyBranch.GB_Code = "LA&";
			Factory.Save();
			var testInBondHeader = Factory.New<CusInBondHeader>();
			AssertEquals("Stand alone inbond", null, testInBondHeader.Declaration);
			var shipment = Factory.New(typeof(ForwardingShipment));
			testInBondHeader.BH_ParentID = shipment.PK;
			testInBondHeader.BH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			AssertEquals("Shipment plugged-in inbond, but without declaration created", null, testInBondHeader.Declaration);
			var testDec1 = Factory.New<JobDeclaration>();
			testDec1.JE_GB = anotherCompanyBranch.PK;
			Factory.Save();
			AssertEquals("Shipment plugged-in inbond, but without company/branch declaration created", null, testInBondHeader.Declaration);
			var testDec2 = Factory.New<JobDeclaration>();
			testDec2.JE_GB = localBranch.PK;
			testDec2.JE_JS = shipment.PK;
			Factory.Save();
			AssertEquals("Shipment plugged-in inbond, with local declaration", testDec2, testInBondHeader.Declaration);
			testDec1.JE_GB = sisterBranch.PK;
			Factory.Save();
			AssertEquals("Shipment plugged-in inbond, with another branch declaration for same company should still find declaration", testDec2, testInBondHeader.Declaration);
		}

		public void TestShipment()
		{
			CusInBondHeader testInBondHeader = Factory.New<CusInBondHeader>();
			AssertEquals("Stand alone inbond", null, testInBondHeader.Parent);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			testInBondHeader.BH_ParentID = shipment.PK;
			testInBondHeader.BH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			AssertEquals("Shipment plugged in inbond", shipment, testInBondHeader.Parent);
		}

		public void TestConsol()
		{
			var testInBondHeader = Factory.New<CusInBondHeader>();
			AssertEquals("Stand alone inbond", null, testInBondHeader.Parent);
			var consol = Factory.New<ForwardingConsol>();
			testInBondHeader.BH_ParentID = consol.PK;
			testInBondHeader.BH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			AssertEquals("Consol plugged in inbond", consol, testInBondHeader.Parent);
		}

		public void TestRelevantConsol()
		{
			const string port1 = "ITAAC";
			const string port2 = "DEAAC";
			const string port3 = "DEAAA";
			const string port4 = "USNYC";
			const string port5 = "USCHI";
			var shipment = Factory.New<ForwardingShipment>();
			var inbond = Factory.New<CusInBondHeader>();
			inbond.BH_ParentID = shipment.PK;
			inbond.BH_ParentTableCode = shipment.TablePrefix;
			AddConsol(shipment, port4, port5); //domestic
			AddConsol(shipment, port2, port3); //OS domestic
			var consol1 = AddConsol(shipment, port3, port4); //US import
			var consol2 = AddConsol(shipment, port1, port2); //OS import
			shipment.JS_RL_NKOrigin = port2;
			shipment.JS_RL_NKDestination = port5;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Italy);
			AssertNull("US In-Bond Relevant Consol", inbond.Consol);
			var consol3 = AddConsol(shipment, port4, port2); //US export
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("US In-Bond should find Relevant Consol", consol1, inbond.Consol);
			shipment.JS_RL_NKOrigin = port4;
			shipment.JS_RL_NKDestination = port1;
			AssertEquals("US Export Relevant Consol: USNYC - DEAAA", consol3, inbond.Consol);
		}

		public void TestImportConveyance()
		{
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "BRENDON'S DUMMY VESSEL";
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
			AssertNull(header.ImportConveyance);
			header.BH_ImportConveyanceName = "BRENDON'S DUMMY VESSEL";
			AssertEquals(vessel, header.ImportConveyance);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.TruckContainer;
			AssertEquals(vessel, header.ImportConveyance);
		}

		public void TestBH_ImportConveyanceNameReadOnly()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
			AssertEquals(false, header.BH_ImportConveyanceNameInfo.ReadOnly);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.TruckContainer;
			AssertEquals(false, header.BH_ImportConveyanceNameInfo.ReadOnly);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirContainer;
			AssertEquals(false, header.BH_ImportConveyanceNameInfo.ReadOnly);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			AssertEquals(true, header.BH_ImportConveyanceNameInfo.ReadOnly);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.PassengerHandCarried;
			AssertEquals(false, header.BH_ImportConveyanceNameInfo.ReadOnly);
		}

		public void TestDefaultFromImportConveyanceIfPossible()
		{
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "BRENDON'S DUMMY VESSEL";
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.NewZealand;
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.TruckContainer;
			AssertEquals("", header.BH_ImportConveyanceCountry);
			header.BH_ImportConveyanceCountry = Core.Constants.CountryCodes.Australia;
			header.BH_ImportConveyanceName = "BRENDON'S DUMMY VESSEL";
			AssertEquals(Core.Constants.CountryCodes.NewZealand, header.BH_ImportConveyanceCountry);
		}

		public void TestLookups()
		{
			AssertEquals(typeof(CusInBondHeaderLookups), header.Lookups.GetType());
		}

		public void TestValidation()
		{
			AssertEquals(typeof(CusInBondHeaderValidation), header.Validation.GetType());
		}

		public void TestSettingValueToSelectedOriginalEntryDoesNotCauseHasChanges()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader1 = header.MovementHeaders.AddNew();
			CusInBondMoveHeader moveHeader2 = header.MovementHeaders.AddNew();
			AssertEquals(2, header.FilteredMovementHeaders.Count);
			Factory.Save();
			AssertEquals("No HasChanges", false, header.HasChanges);
			header.SelectedMovementHeader = moveHeader1.PK;
			AssertEquals("This should not cause HasChanges", false, header.HasChanges);
		}

		public void TestDefaultValidationMode()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.TruckNonContainer;
			AssertEquals("Default for non air is Department mode", ValidationModes.Departure, header.ValidationModes);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			AssertEquals("Default for non air is Department mode", ValidationModes.AirInitiationAndDeletion, header.ValidationModes);
		}

		public void TestAirValidationModes()
		{
			var header = Factory.New<CusInBondHeader>();
			header.ValidationModes = ValidationModes.AirInitiationAndDeletion;
			AssertEquals("Validation mode is Initiation and Deletion", true, header.IsAirInBondInitiationAndDeletionMode);
			header.ValidationModes = ValidationModes.AirEntireInBondArrival;
			AssertEquals("Validation mode is Initiation and Deletion", true, header.IsAirEntireInBondArrivalMode);
			AssertEquals("Validation mode is Initiation and Deletion", true, header.IsAirInBondLevelMode);
			header.ValidationModes = ValidationModes.AirEntireInBondExportation;
			AssertEquals("Validation mode is Initiation and Deletion", true, header.IsAirEntireInBondExportationMode);
			AssertEquals("Validation mode is Initiation and Deletion", true, header.IsAirInBondLevelMode);
		}

		public void TestDepartureValidationModes()
		{
			var header = Factory.New<CusInBondHeader>();
			header.ValidationModes = ValidationModes.Departure;
			AssertEquals("Validation mode is Departure", true, header.IsInBondLevelDepartureValidationMode);
			header.ValidationModes = ValidationModes.DepartureDelete;
			AssertEquals("Validation mode is Departure Delete", false, header.IsInBondLevelDepartureValidationMode);
			AssertEquals("Validation mode is Departure Delete", true, header.IsDepartureDeleteValidationMode);
		}

		public void TestDepartureBillDeleteValidationModes()
		{
			var header = Factory.New<CusInBondHeader>();
			header.ValidationModes = ValidationModes.BillOfLadingDelete;
			AssertEquals("Validation mode is Departure Bill Delete", true, header.IsBillOfLadingDeleteMode);
		}

		public void TestFilteredMovementDetails()
		{
			for (int i = 1; i < 10; i++)
			{
				var moveHeader = header.MovementHeaders.AddNew();
				for (int j = 1; j < 10; j++)
				{
					moveHeader.MovementDetails.AddNew();
				}
			}

			header.SelectedMovementHeader = header.MovementHeaders[6].PK;
			foreach (var moveDetails in header.FilteredMovementDetails)
			{
				Assert(header.MovementHeaders[6].MovementDetails.Contains(moveDetails));
			}
		}

		public void TestSendCargoManifestStatusQueryForInBond()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
			AssertEquals(Enterprise.Customs.US.Business.TransportTypeList.Codes.Sea, ((ICargoManifestStatusQueryHeader)header).TransportMode);
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "XJ5-ENS32423";
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "NC";
			var commodity = container.Commodities.AddNew();
			commodity.BY_InvoiceQuantity = 10m;
			Factory.Save();
			var sendingObject = new CargoManifestStatusQueryHeaderObject(header);
			sendingObject.SendingObjects[0].ShouldSendMessage = true;
			AssertEquals("1 message was created.", 1, sendingObject.SendQueryMessage());
		}

		public void TestSendCargoManifestStatusQueryForSEAInBond()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "001234578";
			bill.B0_HouseBillNumber = "HouseBill1";
			var bill2 = header.Bills.AddNew();
			bill2.B0_MasterBillNumber = "001234578";
			bill2.B0_HouseBillNumber = "HouseBill2";
			Factory.Save();
			var sendingObject = new CargoManifestStatusQueryHeaderObject(header);
			AssertEquals("There should be two sending objects", 2, sendingObject.SendingObjects.Count);
		}

		public void TestSendCargoManifestStatusQueryForAIRInBond()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			AssertEquals(Enterprise.Customs.US.Business.TransportTypeList.Codes.Air, ((ICargoManifestStatusQueryHeader)header).TransportMode);
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "001234578";
			bill.B0_HouseBillNumber = "HouseBill1";
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "NC";
			var commodity = container.Commodities.AddNew();
			commodity.BY_InvoiceQuantity = 10m;
			Factory.Save();
			var sendingObject = new CargoManifestStatusQueryHeaderObject(header);
			sendingObject.ActionCode = CargoManifestStatusQueryActionList.Codes.HAWB;
			sendingObject.SendingObjects[0].ShouldSendMessage = true;
			AssertEquals("1 message was created.", 1, sendingObject.SendQueryMessage());
			AssertEquals(@"B         CQ                                               <<MSGNO PLACEHOLDER>>
WR1                                            001234578  HOUSEBILL1    2       
Y         CQ", moveHeader.Messages[0].EM_FormattedMessageText);
		}

		public void TestNoteTypes()
		{
			var header = Factory.New<CusInBondHeader>();
			Assert("PredefinedNoteTypes.Instance.AutoRatingAuditLog should be in BO.NoteTypes", PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.AutoRatingAuditLog, header.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.ClientVisibleJobNotes should be in BO.NoteTypes", PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.ClientVisibleJobNotes, header.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.InternalWorkNotes should be in BO.NoteTypes", PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.InternalWorkNotes, header.NoteTypes));
		}

		public void TestValidationModesCalculator()
		{
			var header = Factory.New<CusInBondHeader>();
			var calculator1 = header.ValidationModesCalculator;
			var calculator2 = header.ValidationModesCalculator;
			AssertSame(calculator1, calculator2);
		}

		protected override Type ExpectedMetadataType => typeof(Metadata.Business.USCusInBondHeader);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusInBondHeader>();
		}
		CusInBondHeader header;

		bool shouldCancel;
		void header_OnOverrideFreightDefaultsChanging(object sender, CancelEventArgs e)
		{
			e.Cancel = shouldCancel;
		}

		static ForwardingConsol AddConsol(ForwardingShipment shipment, string port1, string port2)
		{
			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = port1;
			consol.JK_RL_NKDischargePort = port2;
			return consol;
		}

		bool PredefinedNoteTypeExist(PredefinedNoteType noteTypeToCheck, NoteTypeCollection noteTypes)
		{
			bool result = false;
			foreach (PredefinedNoteType noteType in noteTypes)
			{
				if (noteTypeToCheck == noteType)
				{
					result = true;
					break;
				}
			}

			return result;
		}
	}
}
