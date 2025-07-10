using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Freight.Integration.CFS;
using TransportModeCodesMessaging = Enterprise.Customs.US.Messaging.Business.TransportModeCodes;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(ISFFromShipmentCreator))]
	public class ISFFromShipmentCreatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestReset()
		{
			var creator = new ISFFromShipmentCreator(Factory);
			creator.ShipmentPK = ZGuid.Invalid;
			AssertHasError(creator.ShipmentPKInfo, ISFFromShipmentCreator.InvalidShipmentNumber);
			creator.Reset();
			AssertEquals("Should reset to the default status.", ZGuid.Empty, creator.ShipmentPK);
			AssertNoErrors("Should reset to the default status.", creator.ShipmentPKInfo);
		}

		public void TestShipments()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment5 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment2.JS_TransportMode = Core.Constants.TransportModes.SeaAir;
			shipment3.JS_TransportMode = Core.Constants.TransportModes.AirSea;
			shipment4.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment5.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKOrigin = "SGSIN";
			shipment3.JS_RL_NKOrigin = "ZA2BY";
			shipment4.JS_RL_NKOrigin = "USCHI";
			shipment5.JS_RL_NKOrigin = "CNSHA";
			Factory.Save();
			var creator = new ISFFromShipmentCreator(Factory);
			var collection = creator.Shipments;
			collection.Load();
			CombineAssertions(() =>
			{
				AssertEquals("Should use ForwardingModuleShipment for finding data.", typeof(ForwardingModuleShipment), collection.TypeOfElements);
				var actualPks = collection.GetPKs();
				AssertCollectionContains("Should contains shipment1 as the transport mode is Sea.", shipment1.PK, actualPks);
				AssertCollectionContains("Should contains shipment2 as the transport mode is SeaAir.", shipment2.PK, actualPks);
				AssertCollectionContains("Should contains shipment3 as the transport mode is AirSea.", shipment3.PK, actualPks);
				AssertCollectionNotContains("Should not contains shipment4 as the origin is in US.", shipment4.PK, actualPks);
				AssertCollectionNotContains("Should not contains shipment5 as the transport mode is not Sea.", shipment5.PK, actualPks);
			});
		}

		public void TestCreateManufacturersAndSellersFromBuyerConsol()
		{
			var manufacturer1 = Factory.New<OrgHeader>();
			var manufacturer2 = Factory.New<OrgHeader>();
			var manufacturer3 = Factory.New<OrgHeader>();
			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			var shipment = CreateShipment();
			consol.Shipments.Add(shipment);
			shipment.ConsignorDocumentaryAddress.OrganisationPK = manufacturer1.PK;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OA_SellerAddress = Factory.New<OrgHeader>().MainAddress.PK;
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment.CoLoadShipments.Add(shipment1);
			shipment1.ConsignorDocumentaryAddress.OrganisationPK = manufacturer2.PK;
			var shipment2 = Factory.New<ForwardingShipment>();
			shipment.CoLoadShipments.Add(shipment2);
			shipment2.ConsignorDocumentaryAddress.OrganisationPK = manufacturer3.PK;
			Creator.ShipmentPK = shipment.PK;
			var isf = Creator.Create(Factory);
			var manufacturerPKs = isf.ManufacturerAddresses.Select(x => x.OrganisationPK);
			var sellers = isf.DocAddresses.FindDocAddressesByType(MasterFiles.Integration.DocAddressType.SellingParty).Select(x => x.OrganisationPK);
			Assert(manufacturerPKs.Contains(manufacturer2.PK));
			Assert(manufacturerPKs.Contains(manufacturer3.PK));
			Assert(!manufacturerPKs.Contains(manufacturer1.PK));
			Assert(sellers.Contains(manufacturer2.PK));
			Assert(sellers.Contains(manufacturer3.PK));
			Assert(!sellers.Contains(manufacturer1.PK));
			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = manufacturer1.PK;
			isf = Creator.Create(Factory);
			manufacturerPKs = isf.ManufacturerAddresses.Select(x => x.OrganisationPK);
			sellers = isf.DocAddresses.FindDocAddressesByType(MasterFiles.Integration.DocAddressType.SellingParty).Select(x => x.OrganisationPK);
			Assert(manufacturerPKs.Contains(manufacturer2.PK));
			Assert(manufacturerPKs.Contains(manufacturer3.PK));
			Assert(manufacturerPKs.Contains(manufacturer1.PK));
			Assert(sellers.Contains(manufacturer2.PK));
			Assert(sellers.Contains(manufacturer3.PK));
		}

		public void TestWorkfowTriggerOnTRFWhenISFIsCreatedFromShipment_CS00258351()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "CS1";
				org.OH_FullName = "ORG CS00258351";
				var cusCode = org.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
				cusCode.OK_CustomsRegNo = "APLU";
				cusCode.OK_RN_NKCodeCountry = "US";
				var org2 = Factory.New<OrgHeader>();
				org2.OH_Code = "CS2";
				org2.OH_FullName = "ORG2 CS00258351";
				var cusCode2 = org2.CustomsCodes.AddNew();
				cusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
				cusCode2.OK_CustomsRegNo = "SHCR";
				cusCode2.OK_RN_NKCodeCountry = "US";
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "USLAX";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_MasterBillNum = "APLUMST053012B";
				consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment.JS_HouseBill = "SHCRHSE0530123C";
				shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org2.PK;
				var task = shipment.WorkflowItems.Triggers.AddNew();
				task.P9_Description = "CLEAR ISF ADD";
				task.TriggerConditions.TriggerEventCode = Events.Transferred.Code;
				task.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
				task.TriggerConditions.TriggerConditionValue = "*ISF*";
				var notification = task.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
				notification.PQ_EmailAddr = "dummy1@where.com";
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_JS = shipment.PK;
				TestHelper.MakeConsolRelevantToDeclaration(consol, declaration);
				declaration.ShipmentSynchroniser.SetEnabled(false, false);
				Factory.Save();
				var carrier0 = Factory.New<USCarrierCombined>();
				carrier0.UI_Code = "APLU";
				// carrier0.UI_ModeOfTransportation = TransportModeCodes.Codes.VesselContainer;
				carrier0.UI_ModeOfTransportation = TransportModeCodesMessaging.Codes.VesselContainer;
				var carrier1 = Factory.New<USCarrierCombined>();
				carrier1.UI_Code = "SHCR";
				carrier1.UI_ModeOfTransportation = TransportModeCodesMessaging.Codes.VesselContainer;
				Creator.ShipmentPK = shipment.PK;
				var factory = new BusinessObjectFactory();
				var isf = Creator.Create(factory);
				AssertEquals(ZString.Empty, isf.BF_JobReference);
				AssertEquals(ZDateTime.Empty, shipment.WorkflowItems[0].P9_ActualDate.ToZDateTime());
				shipment = factory.Load<ForwardingShipment>(shipment.PK);
				AssertEquals("APLUMST053012B", isf.BF_MasterBill);
				AssertEquals("SHCRHSE0530123C", isf.BF_HouseBill);
				factory.Save();
				AssertNotEquals(ZString.Empty, isf.BF_JobReference);
				AssertNotEquals(ZDateTime.Empty, shipment.WorkflowItems[0].P9_ActualDate.ToZDateTime());
			}
		}

		public void TestSCACPulledFromShipmentWithDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_MasterBillNum = "MST053012B";
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "aaa";
			org1.OH_FullName = "bbb";
			var cusCode1 = org1.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode1.OK_CustomsRegNo = "APLU";
			cusCode1.OK_RN_NKCodeCountry = "US";
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org1.PK;
			var container = consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "HSE0530123C";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "aaa";
			org2.OH_FullName = "bbb";
			var cusCode2 = org2.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode2.OK_CustomsRegNo = "SHCR";
			cusCode2.OK_RN_NKCodeCountry = "US";
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org2.PK;
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container.PK;
			declaration.JE_JS = shipment.PK;
			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration);
			declaration.ShipmentSynchroniser.SetEnabled(false, false);
			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("Pre-condition", "MST053012B", declaration.JE_MasterBill);
			AssertEquals("Pre-condition", "HSE0530123C", declaration.JE_HouseBill);
			AssertEquals("Pre-condition", "APLU", declaration.JE_MasterBillIssuerSCAC);
			AssertEquals("Pre-condition", "SHCR", declaration.JE_HouseBillIssuerSCAC);
			Creator.ShipmentPK = shipment.PK;
			var isf = Creator.Create(Factory);
			AssertEquals("APLUMST053012B", isf.BF_MasterBill);
			AssertEquals("SHCRHSE0530123C", isf.BF_HouseBill);
			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "HS000001";
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill.US_UI_NKBillIssuerSCAC = "APLU";
			isf = Creator.Create(Factory);
			Assert(isf.ReferenceDatas.Find(new ZQuery(CusISFBillSchema.BB_BillNum, "APLUHS000001")).Any());
		}

		public void TestShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			AssertEquals("Shipment", null, Creator.Shipment);
			Creator.ShipmentPK = shipment.PK;
			AssertEquals("Shipment", shipment, Creator.Shipment);
		}

		public void TestValidateShipmentPK()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Factory.Save();
			Creator.ShipmentPK = ZGuid.NewZGuid();
			AssertNoErrorContaining(Creator.ShipmentPKInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(Creator.ShipmentPKInfo, ISFFromShipmentCreator.InvalidShipmentNumber);
			Creator.ShipmentPK = shipment.PK;
			AssertNoErrorContaining(Creator.ShipmentPKInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(Creator.ShipmentPKInfo, ISFFromShipmentCreator.InvalidShipmentNumber);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();
			Creator.ShipmentPK = ZGuid.Empty;
			AssertHasErrorContaining(Creator.ShipmentPKInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(Creator.ShipmentPKInfo, ISFFromShipmentCreator.InvalidShipmentNumber);
			Creator.ShipmentPK = shipment.PK;
			AssertNoErrorContaining(Creator.ShipmentPKInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(Creator.ShipmentPKInfo, ISFFromShipmentCreator.InvalidShipmentNumber);
		}

		public void TestAMSIsPulledFromConsolInsteadOfMasterBill()
		{
			var consol = CreateConsol();
			consol.JK_MasterBillNum = "MB001";
			consol.JK_AgentType = Constants.AgentType.Direct;
			var container1 = AddContainer(consol.Containers, "TURE2343232", "SL324", helper.Container40US.PK, Constants.ContainerModes.FCL);
			var container2 = AddContainer(consol.Containers, "TURE4569789", "SL589", helper.Container20US.PK, Constants.ContainerModes.FCL);
			var shipment = CreateShipment();
			consol.Shipments.Add(shipment);
			AddOuterPackLine(shipment.OuterPackLines, 10, Constants.PkgUnit.Carton, container1.PK, "1010102010", Core.Constants.CountryCodes.Australia);
			AddOuterPackLine(shipment.OuterPackLines, 20, Constants.PkgUnit.Bag, container2.PK, "2020203020", Core.Constants.CountryCodes.NewZealand);
			Factory.Save();
			var carrier0 = Factory.New<USCarrierCombined>();
			carrier0.UI_Code = "VOCC";
			carrier0.UI_ModeOfTransportation = TransportModeCodesMessaging.Codes.VesselContainer;
			Creator.ShipmentPK = shipment.PK;
			var header = Creator.Create(Factory);
			AssertEquals("Master bill from consol is pulled", "VOCCMB001", header.BF_OceanBill);
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "APLU";
			cusCode.OK_RN_NKCodeCountry = "US";
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
			Factory.Save();
			header = Creator.Create(Factory);
			AssertEquals("ams from consol is pulled", "APLUMB001", header.BF_OceanBill);
			var ams = consol.Numbers.AddNew();
			ams.CE_EntryType = Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			ams.CE_EntryNum = "AMS0001";
			header = Creator.Create(Factory);
			AssertEquals("ams from consol is pulled", "AMS0001", header.BF_OceanBill);
			consol.Numbers.RemoveAndDeleteAll();
			Factory.Save();
			header = Creator.Create(Factory);
			AssertEquals("ams from consol is pulled", "APLUMB001", header.BF_OceanBill);
		}

		public void TestAMSIsPulledFromShipmentInsteadOfHouseBill()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HB0001";
			Creator.ShipmentPK = shipment.PK;
			var header = Creator.Create(Factory);
			AssertEquals("house bill from shipment is pulled", "HB0001", header.BF_HouseBill);
			var ams = shipment.Numbers.AddNew();
			ams.CE_EntryType = Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			ams.CE_EntryNum = "AMS0001";
			header = Creator.Create(Factory);
			AssertEquals("ams from shipment is pulled", "AMS0001", header.BF_HouseBill);
			var subShipment1 = shipment.CoLoadShipments.AddNew();
			subShipment1.JS_HouseBill = "HB0002";
			var subShipment2 = shipment.CoLoadShipments.AddNew();
			subShipment2.JS_HouseBill = "HB0003";
			ams = subShipment2.Numbers.AddNew();
			ams.CE_EntryType = Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			ams.CE_EntryNum = "AMS0002";
			header = Creator.Create(Factory);
			AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.HouseBillOfLading, new ZString[] { "AMS0001", "HB0002", "AMS0002" });
		}

		public void TestCreateFromShipmentWithDirectConsol()
		{
			helper.UpdateOrAddCustomsRegNo(helper.Consignee, "45-4567835", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, helper.UnitedStates);
			ForwardingConsol consol = CreateConsol();
			AssertNotEquals(ZString.Empty, consol.JK_MasterBillNum);
			consol.JK_AgentType = Constants.AgentType.Direct;
			ForwardingContainer container1 = AddContainer(consol.Containers, "TURE2343232", "SL324", helper.Container40US.PK, Constants.ContainerModes.FCL);
			ForwardingContainer container2 = AddContainer(consol.Containers, "TURE4569789", "SL589", helper.Container20US.PK, Constants.ContainerModes.FCL);
			ForwardingShipment shipment = CreateShipment();
			AssertNotEquals(ZString.Empty, shipment.JS_HouseBill);
			AssertEquals(ZBool.False, shipment.BuyerDocAddress.IsEmpty);
			consol.Shipments.Add(shipment);
			AddOuterPackLine(shipment.OuterPackLines, 10, Constants.PkgUnit.Carton, container1.PK, "1010102010", Core.Constants.CountryCodes.Australia);
			AddOuterPackLine(shipment.OuterPackLines, 20, Constants.PkgUnit.Bag, container2.PK, "2020203020", Core.Constants.CountryCodes.NewZealand);
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			AssertEquals("Result", null, Creator.Create(secondFactory));
			Creator.ShipmentPK = shipment.PK;
			AssertEquals("Result", null, Creator.Create(secondFactory));
			Factory.Save();
			var carrier0 = secondFactory.New<USCarrierCombined>();
			carrier0.UI_Code = "VOCC";
			carrier0.UI_ModeOfTransportation = TransportModeCodesMessaging.Codes.VesselContainer;
			CusISFHeader header = Creator.Create(secondFactory);
			AssertEquals("Headers Factory", secondFactory, header.Factory);
			AssertCusISFHeader(header, header.BF_SCAC, helper.Consignee.PK, ZString.Empty, "45-4567835", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, ZString.Empty, ZString.Empty, "VOCC" + MasterBillNumber, ZString.Empty, ZString.Empty, "USLAX", "USNYK");
			AssertEquals(7, header.DocAddresses.Count);
			AssertDocAddress(header.MainShipToParty, helper.ImportForwarder.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.ShipToParty);
			AssertDocAddress(shipment.BuyerDocAddress, header.BuyingParty, DocAddressTypes.Codes.BuyingParty);
			AssertDocAddress(shipment.ConsignorDocumentaryAddress, header.SellingParty, DocAddressTypes.Codes.SellingParty);
			AssertDocAddress(header.Consolidator, helper.ExportForwarder.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.Consolidator);
			AssertDocAddress(header.StuffingLocation, helper.ExportForwarder.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.ScheduledContainerStuffingLocation);
			AssertEquals(1, header.ManufacturerAddresses.Count);
			AssertDocAddress(header.ManufacturerAddresses[0], helper.Consignor.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.Manufacturer);
			AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.OceanBillOfLading, new ZString[] { "VOCC" + MasterBillNumber });
			AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.HouseBillOfLading, System.Array.Empty<ZString>());
			AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.MasterBillOfLading, System.Array.Empty<ZString>());
			AssertEquals(2, header.Equipments.Count);
			AssertEquipment(header.Equipments["TURE2343232"], helper.Container40US.RC_ISOType, helper.Container40US.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates));
			AssertEquipment(header.Equipments["TURE4569789"], helper.Container20US.RC_ISOType, helper.Container20US.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates));
			AssertEquals(2, header.Lines.Count);
			AssertCusISFLine(header.Lines, ZGuid.Empty, ZString.Empty, "1010102010", Constants.CountryCodes.Australia);
			AssertCusISFLine(header.Lines, ZGuid.Empty, ZString.Empty, "2020203020", Constants.CountryCodes.NewZealand);
			AssertNotEquals(0, header.Transports.Count);
			AssertEquals(consol.Transports.Count, header.Transports.Count);
			for (int i = 0; i < header.Transports.Count; i++)
			{
				AssertTransport(consol.Transports[i], header.Transports[i]);
			}

			AssertTransferredEvent(header, shipment.PK);
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "APLU";
			cusCode.OK_RN_NKCodeCountry = "US";
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
			Factory.Save();
			header = Creator.Create(Factory);
			AssertCusISFHeader(header, header.BF_SCAC, helper.Consignee.PK, ZString.Empty, "45-4567835", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, ZString.Empty, ZString.Empty, "APLU" + MasterBillNumber, ZString.Empty, ZString.Empty, "USLAX", "USNYK");
			AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.OceanBillOfLading, new ZString[] { "APLU" + MasterBillNumber });
		}

		public void TestImportFromConsol()
		{
			ForwardingConsol consol = CreateConsol();
			consol.JK_AgentType = Constants.AgentType.Agent;
			ForwardingContainer container1 = AddContainer(consol.Containers, "TURE2343232", "SL324", helper.Container40US.PK, Constants.ContainerModes.FCL);
			ForwardingContainer container2 = AddContainer(consol.Containers, "TURE4569789", "SL589", helper.Container20US.PK, Constants.ContainerModes.FCL);
			ForwardingShipment shipment = CreateShipment();
			shipment.JS_UniqueConsignRef = "TEST100000";
			consol.Shipments.Add(shipment);
			AddOuterPackLine(shipment.OuterPackLines, 10, Constants.PkgUnit.Carton, container1.PK, "1010102010", Core.Constants.CountryCodes.Australia);
			AddOuterPackLine(shipment.OuterPackLines, 20, Constants.PkgUnit.Bag, container2.PK, "2020203020", Core.Constants.CountryCodes.NewZealand);
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CREDITOR";
			org.OH_FullName = "Creditor";
			consol.JK_OA_CreditorAddress = org.MainAddress.PK;
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "PACKDEPOT";
			org2.OH_FullName = "PackDepot";
			consol.JK_OA_PackDepotAddress = org2.MainAddress.PK;
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "SENDING";
			org3.OH_FullName = "Sending";
			consol.JK_OA_SendingForwarderAddress = org3.MainAddress.PK;
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			Creator.ShipmentPK = shipment.PK;
			Factory.Save();
			CusISFHeader header = Creator.Create(secondFactory);
			AssertEquals("Consol Type is Agent", org2.MainAddress.PK, header.StuffingLocation.E2_OA_Address);
			AssertEquals("Consol Type is Agent", org3.MainAddress.PK, header.Consolidator.E2_OA_Address);
		}

		public void TestImportFromConsolWithCoLoadType()
		{
			ForwardingConsol consol = CreateConsol();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			ForwardingContainer container1 = AddContainer(consol.Containers, "TURE2343232", "SL324", helper.Container40US.PK, Constants.ContainerModes.FCL);
			ForwardingContainer container2 = AddContainer(consol.Containers, "TURE4569789", "SL589", helper.Container20US.PK, Constants.ContainerModes.FCL);
			ForwardingShipment shipment = CreateShipment();
			shipment.JS_UniqueConsignRef = "TEST100000";
			consol.Shipments.Add(shipment);
			AddOuterPackLine(shipment.OuterPackLines, 10, Constants.PkgUnit.Carton, container1.PK, "1010102010", Core.Constants.CountryCodes.Australia);
			AddOuterPackLine(shipment.OuterPackLines, 20, Constants.PkgUnit.Bag, container2.PK, "2020203020", Core.Constants.CountryCodes.NewZealand);
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CREDITOR";
			org.OH_FullName = "Creditor";
			consol.JK_OA_CreditorAddress = org.MainAddress.PK;
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "PACKDEPOT";
			org2.OH_FullName = "PackDepot";
			consol.JK_OA_PackDepotAddress = org2.MainAddress.PK;
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "CONSIGNOR";
			org3.OH_FullName = "Consignor";
			shipment.ConsignorPK = org3.PK;
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			Creator.ShipmentPK = shipment.PK;
			Factory.Save();
			CusISFHeader header = Creator.Create(secondFactory);
			AssertEquals("Consol Type is CoLoad", org2.MainAddress.PK, header.StuffingLocation.E2_OA_Address);
			AssertEquals("Consol Type is CoLoad", org.MainAddress.PK, header.Consolidator.E2_OA_Address);
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.Shipments.Add(shipment);
			shipment.ConsignorPK = org2.PK;
			Factory.Save();
			CusISFHeader header2 = Creator.Create(secondFactory);
			AssertEquals("Shipper org and departure CFS are the same", org2.MainAddress.PK, header2.StuffingLocation.E2_OA_Address);
			AssertEquals("Shipper org and departure CFS are the same", org.MainAddress.PK, header2.Consolidator.E2_OA_Address);
		}

		public void TestImportFromConsolWithSameShipperAndCFS()
		{
			ForwardingConsol consol = CreateConsol();
			consol.JK_AgentType = Constants.AgentType.Agent;
			ForwardingContainer container1 = AddContainer(consol.Containers, "TURE2343232", "SL324", helper.Container40US.PK, Constants.ContainerModes.FCL);
			ForwardingContainer container2 = AddContainer(consol.Containers, "TURE4569789", "SL589", helper.Container20US.PK, Constants.ContainerModes.FCL);
			ForwardingShipment shipment = CreateShipment();
			consol.Shipments.Add(shipment);
			shipment.JS_UniqueConsignRef = "TEST100000";
			AddOuterPackLine(shipment.OuterPackLines, 10, Constants.PkgUnit.Carton, container1.PK, "1010102010", Core.Constants.CountryCodes.Australia);
			AddOuterPackLine(shipment.OuterPackLines, 20, Constants.PkgUnit.Bag, container2.PK, "2020203020", Core.Constants.CountryCodes.NewZealand);
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CREDITOR";
			org.OH_FullName = "Creditor";
			consol.JK_OA_CreditorAddress = org.MainAddress.PK;
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "PACKDEPOT";
			org2.OH_FullName = "PackDepot";
			consol.JK_OA_PackDepotAddress = org2.MainAddress.PK;
			shipment.ConsignorPK = org2.PK;
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			Creator.ShipmentPK = shipment.PK;
			Factory.Save();
			CusISFHeader header = Creator.Create(secondFactory);
			AssertEquals("Shipper org and departure CFS are the same", org2.MainAddress.PK, header.StuffingLocation.E2_OA_Address);
			AssertEquals("Shipper org and departure CFS are the same", org2.MainAddress.PK, header.Consolidator.E2_OA_Address);
		}

		public void TestCreateISFFromShipmentWithoutAppendingSCACCode()
		{
			ForwardingConsol consol = CreateConsol();
			consol.JK_MasterBillNum = "XXXXMB001";
			ForwardingShipment shipment = CreateShipment();
			shipment.JS_HouseBill = "XXXXHB001";
			consol.Shipments.Add(shipment);
			ForwardingShipment shipment2 = CreateShipment();
			shipment2.JS_HouseBill = "XXXXHB002";
			consol.Shipments.Add(shipment2);
			var ams = shipment2.Numbers.AddNew();
			ams.CE_EntryType = Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			ams.CE_EntryNum = "AMS0002";
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			AssertEquals("Result", null, Creator.Create(secondFactory));
			Creator.ShipmentPK = shipment.PK;
			AssertEquals("Result", null, Creator.Create(secondFactory));
			Factory.Save();
			AssertNull("Not link to a declaration", shipment.DeclarationForDocuments);
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var cusCodeForOrgProxy = orgProxy.CustomsCodes.AddNew();
			cusCodeForOrgProxy.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCodeForOrgProxy.OK_CustomsRegNo = "ULPA";
			cusCodeForOrgProxy.OK_RN_NKCodeCountry = "US";
			Factory.Save();
			var carrier = secondFactory.New<USCarrierCombined>();
			carrier.UI_Code = "XXXX";
			carrier.UI_ModeOfTransportation = "10";
			CusISFHeader header = Creator.Create(secondFactory);
			AssertEquals("Headers Factory", secondFactory, header.Factory);
			AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.OceanBillOfLading, System.Array.Empty<ZString>());
			AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.HouseBillOfLading, new ZString[] { "XXXXHB001" });
			AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.MasterBillOfLading, new ZString[] { "XXXXMB001" });
			//var creator2 = new ISFFromShipmentCreator(Factory);
			Creator.ShipmentPK = shipment2.PK;
			Factory.Save();
			CusISFHeader header2 = Creator.Create(secondFactory);
			AssertHasBill(header2.ReferenceDatas, BillTypeList.Codes.OceanBillOfLading, System.Array.Empty<ZString>());
			AssertHasBill(header2.ReferenceDatas, BillTypeList.Codes.HouseBillOfLading, new ZString[] { "AMS0002" });
			AssertHasBill(header2.ReferenceDatas, BillTypeList.Codes.MasterBillOfLading, new ZString[] { "XXXXMB001" });
		}

		public void TestCreateFromShipmentNotLinkToADeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				OrgHeader buyer = helper.CreateOrganisation("BUYER", "USLAX");
				helper.UpdateOrAddCustomsRegNo(helper.Consignee, "45-4567835", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, helper.UnitedStates);
				ForwardingConsol consol = CreateConsol();
				AssertNotEquals(ZString.Empty, consol.JK_MasterBillNum);
				ForwardingContainer container1 = AddContainer(consol.Containers, "TURE2343232", "SL324", helper.Container40US.PK, Constants.ContainerModes.FCL);
				ForwardingContainer container2 = AddContainer(consol.Containers, "TURE4569789", "SL589", helper.Container20US.PK, Constants.ContainerModes.FCL);
				ForwardingShipment shipment = CreateShipment();
				shipment.BuyerDocAddress.E2_OA_Address = buyer.MainAddress.PK;
				AssertNotEquals(ZString.Empty, shipment.JS_HouseBill);
				consol.Shipments.Add(shipment);
				AddOuterPackLine(shipment.OuterPackLines, 10, Constants.PkgUnit.Carton, container1.PK, "1010102010", Core.Constants.CountryCodes.Australia);
				AddOuterPackLine(shipment.OuterPackLines, 20, Constants.PkgUnit.Bag, container2.PK, "2020203020", Core.Constants.CountryCodes.NewZealand);
				BusinessObjectFactory secondFactory = new BusinessObjectFactory();
				AssertEquals("Result", null, Creator.Create(secondFactory));
				Creator.ShipmentPK = shipment.PK;
				AssertEquals("Result", null, Creator.Create(secondFactory));
				Factory.Save();
				AssertNull("Not link to a declaration", shipment.DeclarationForDocuments);
				var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				var cusCodeForOrgProxy = orgProxy.CustomsCodes.AddNew();
				cusCodeForOrgProxy.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
				cusCodeForOrgProxy.OK_CustomsRegNo = "ULPA";
				cusCodeForOrgProxy.OK_RN_NKCodeCountry = "US";
				Factory.Save();
				var carrier0 = secondFactory.New<USCarrierCombined>();
				carrier0.UI_Code = "ULPA";
				carrier0.UI_ModeOfTransportation = TransportModeCodesMessaging.Codes.VesselContainer;
				var carrier1 = secondFactory.New<USCarrierCombined>();
				carrier1.UI_Code = "VOCC";
				carrier1.UI_ModeOfTransportation = TransportModeCodesMessaging.Codes.VesselContainer;
				CusISFHeader header = Creator.Create(secondFactory);
				AssertEquals("Headers Factory", secondFactory, header.Factory);
				AssertCusISFHeader(header, header.BF_SCAC, helper.Consignee.PK, ZString.Empty, "45-4567835", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, ZString.Empty, ZString.Empty, ZString.Empty, "ULPA" + HouseBillNumber, "VOCC" + MasterBillNumber, "USLAX", "USNYK");
				AssertCusISFHeaderLowValueDetails(header, 150, ZString.Empty, 2000m, 2, Core.Constants.Weight.Tonnes);
				AssertEquals(7, header.DocAddresses.Count);
				AssertDocAddress(header.MainShipToParty, helper.ImportForwarder.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.ShipToParty);
				AssertDocAddress(shipment.BuyerDocAddress, header.BuyingParty, DocAddressTypes.Codes.BuyingParty);
				AssertDocAddress(shipment.ConsignorDocumentaryAddress, header.SellingParty, DocAddressTypes.Codes.SellingParty);
				AssertEquals(true, header.BookingParty.IsEmpty);
				AssertDocAddress(header.Consolidator, helper.ExportForwarder.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.Consolidator);
				AssertDocAddress(header.StuffingLocation, helper.ExportForwarder.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.ScheduledContainerStuffingLocation);
				AssertEquals(1, header.ManufacturerAddresses.Count);
				AssertDocAddress(header.ManufacturerAddresses[0], helper.Consignor.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.Manufacturer);
				AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.OceanBillOfLading, System.Array.Empty<ZString>());
				AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.HouseBillOfLading, new ZString[] { "ULPA" + HouseBillNumber });
				AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.MasterBillOfLading, new ZString[] { "VOCC" + MasterBillNumber });
				AssertEquals(2, header.Equipments.Count);
				AssertEquipment(header.Equipments["TURE2343232"], helper.Container40US.RC_ISOType, helper.Container40US.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates));
				AssertEquipment(header.Equipments["TURE4569789"], helper.Container20US.RC_ISOType, helper.Container20US.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates));
				AssertEquals(2, header.Lines.Count);
				AssertCusISFLine(header.Lines, ZGuid.Empty, ZString.Empty, "1010102010", Constants.CountryCodes.Australia);
				AssertCusISFLine(header.Lines, ZGuid.Empty, ZString.Empty, "2020203020", Constants.CountryCodes.NewZealand);
				AssertNotEquals(0, header.Transports.Count);
				AssertEquals(consol.Transports.Count, header.Transports.Count);
				for (int i = 0; i < header.Transports.Count; i++)
				{
					AssertTransport(consol.Transports[i], header.Transports[i]);
				}

				AssertTransferredEvent(header, shipment.PK);
				consol.Shipments.Remove(shipment);
				Factory.Save();
				header = Creator.Create(secondFactory);
				AssertEquals("Headers Factory", secondFactory, header.Factory);
				AssertEquals(ZGuid.Empty, header.BF_JS_Shipment);
				AssertCusISFHeader(header, header.BF_SCAC, helper.Consignee.PK, ZString.Empty, "45-4567835", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, ZString.Empty, ZString.Empty, ZString.Empty, "ULPA" + HouseBillNumber, ZString.Empty, "", "USNYK");
				AssertEquals(7, header.DocAddresses.Count);
				AssertDocAddress(shipment.ConsigneeDeliveryAddress, header.MainShipToParty, DocAddressTypes.Codes.ShipToParty);
				AssertEquals(false, header.MainShipToParty.IsEmpty);
				AssertDocAddress(shipment.BuyerDocAddress, header.BuyingParty, DocAddressTypes.Codes.BuyingParty);
				AssertDocAddress(shipment.ConsignorDocumentaryAddress, header.SellingParty, DocAddressTypes.Codes.SellingParty);
				AssertEquals(true, header.BookingParty.IsEmpty);
				AssertEquals(true, header.Consolidator.IsEmpty);
				AssertEquals(true, header.StuffingLocation.IsEmpty);
				AssertEquals(1, header.ManufacturerAddresses.Count);
				AssertDocAddress(header.ManufacturerAddresses[0], helper.Consignor.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.Manufacturer);
				AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.OceanBillOfLading, System.Array.Empty<ZString>());
				AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.HouseBillOfLading, new ZString[] { "ULPA" + HouseBillNumber });
				AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.MasterBillOfLading, System.Array.Empty<ZString>());
				AssertEquals(0, header.Equipments.Count);
				AssertEquals(2, header.Lines.Count);
				AssertCusISFLine(header.Lines, ZGuid.Empty, ZString.Empty, "1010102010", Constants.CountryCodes.Australia);
				AssertCusISFLine(header.Lines, ZGuid.Empty, ZString.Empty, "2020203020", Constants.CountryCodes.NewZealand);
				AssertNotEquals(0, header.Transports.Count);
				AssertEquals(shipment.Transports.Count, header.Transports.Count);
				for (int i = 0; i < header.Transports.Count; i++)
				{
					AssertTransport(shipment.Transports[i], header.Transports[i]);
				}

				var carrier = Factory.New<USCarrierCombined>();
				carrier.UI_Code = "HPTS";
				carrier.UI_ModeOfTransportation = "10";
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "aaa";
				org.OH_FullName = "bbb";
				var cusCode = org.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
				cusCode.OK_CustomsRegNo = "HPTS";
				cusCode.OK_RN_NKCodeCountry = "US";
				shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
				Factory.Save();
				header = Creator.Create(secondFactory);
				AssertCusISFHeader(header, header.BF_SCAC, helper.Consignee.PK, ZString.Empty, "45-4567835", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, ZString.Empty, ZString.Empty, ZString.Empty, "HPTS" + HouseBillNumber, ZString.Empty, "", "USNYK");
				AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.HouseBillOfLading, new ZString[] { "HPTS" + HouseBillNumber });
			}
		}

		public void TestCreateFromShipmentLinkToADeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				helper.UpdateOrAddCustomsRegNo(helper.Consignee, "45-4567835", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, helper.UnitedStates);
				ForwardingConsol consol = CreateConsol();
				AssertNotEquals(ZString.Empty, consol.JK_MasterBillNum);
				ForwardingContainer container1 = AddContainer(consol.Containers, "TURE2343232", "SL324", helper.Container40US.PK, Constants.ContainerModes.FCL);
				ForwardingContainer container2 = AddContainer(consol.Containers, "TURE4569789", "SL589", helper.Container20US.PK, Constants.ContainerModes.FCL);
				ForwardingShipment shipment = CreateShipment();
				AssertNotEquals(ZString.Empty, shipment.JS_HouseBill);
				shipment.BuyerDocAddress.E2_AddressOverride = ZBool.False;
				AssertEquals(ZBool.True, shipment.BuyerDocAddress.IsEmpty);
				consol.Shipments.Add(shipment);
				AddOuterPackLine(shipment.OuterPackLines, 10, Constants.PkgUnit.Carton, container1.PK, "1010102010", Core.Constants.CountryCodes.Australia);
				AddOuterPackLine(shipment.OuterPackLines, 20, Constants.PkgUnit.Bag, container2.PK, "2020203020", Core.Constants.CountryCodes.NewZealand);
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_OverrideFreightDefaults = true;
				declaration.JE_OH_Importer = helper.IntermediateConsignee.PK;
				helper.UpdateOrAddCustomsRegNo(helper.ImportForwarder, "56-98468845", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, helper.UnitedStates);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = Constants.TransportModes.Sea;
				declaration.US_EnableAII = true;
				declaration.US_EntryFilerCode = "XJ3";
				declaration.JE_RL_NKOrigin = "AUBNE";
				declaration.JE_RL_NKPortOfLoading = "AUSYD";
				declaration.JE_RL_NKPortOfArrival = "USMXC";
				declaration.JE_RL_NKFinalDestination = "USADD";
				declaration.JE_ExportDate = ZDateTime.Today.AddDays(-2);
				declaration.US_EntryDate = ZDateTime.Today.AddDays(20);
				declaration.JE_VesselName = helper.VesselWithoutCountry.RV_Code;
				declaration.JE_VoyageFlightNo = "526";
				declaration.JE_OA_ConsigneeAddress = helper.ImportForwarder.MainAddress.PK;
				declaration.JE_OA_SellerAddress = helper.ExportForwarder.MainAddress.PK;
				declaration.JE_OH_Buyer = helper.Consignee.PK;
				declaration.US_SuretyCode = "791";
				var aud = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia);
				aud.ExchangeRates.AddNew();
				var audCustomsRate = aud.ExchangeRates.AddNew();
				audCustomsRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.CustomsRate;
				audCustomsRate.RE_StartDate = ZDateTime.Today.AddDays(-364);
				audCustomsRate.RE_ExpiryDate = ZDateTime.Today.AddYears(1);
				audCustomsRate.RE_SellRate = 1.176471m;
				RefExchangeRate audNoneCustomsRate = aud.ExchangeRates.AddNew();
				audNoneCustomsRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.IATARate;
				audNoneCustomsRate.RE_StartDate = ZDateTime.Today.AddMonths(-1);
				audNoneCustomsRate.RE_ExpiryDate = ZDateTime.Today.AddYears(1);
				audNoneCustomsRate.RE_SellRate = 1.052632m;
				declaration.JE_TotalNoOfPacks = 200;
				declaration.JE_TotalNoOfPacksPackType = ShippingOrPackingingUnitList.Codes.Plate;
				declaration.JE_TotalWeight = 2030.51m;
				declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
				declaration.US_UI_NKCarrierSCAC = "SC43";
				declaration.JE_MasterBill = "MB32342323";
				declaration.JE_HouseBill = "HB32904934";
				Bill subHouseBill1 = declaration.PrimaryHouseBill.ChildBills.AddNew();
				subHouseBill1.CU_BillNum = "SB23439397";
				Bill masterBill2 = declaration.Bills.AddNew();
				masterBill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
				masterBill2.CU_BillNum = "MB669586445";
				Bill houseBill2 = masterBill2.ChildBills.AddNew();
				houseBill2.CU_BillNum = "HB968565545";
				Bill subHouseBill2 = houseBill2.ChildBills.AddNew();
				subHouseBill2.CU_BillNum = "SB352322394";
				helper.CreateCusContainer(declaration, "TURD4865644", "SL987", helper.Container20NO, Core.Constants.ContainerModes.FCL);
				helper.CreateCusContainer(declaration, "KJDK9868755", "SL354", helper.Container40NO, Core.Constants.ContainerModes.LCL);
				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "INVE2212";
				invoice.JZ_InvoiceAmount = 850m;
				invoice.JZ_RX_NKInvoice_Currency = aud.RX_Code;
				JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_PartNo = "PARTNO2";
				invoiceLine1.JI_Tariff = "2020301010";
				invoiceLine1.JI_OA_ManufacturerAddress = helper.Consignor.MainAddress.PK;
				invoiceLine1.US_UC_NKCountryOfOrigin = Constants.CountryCodes.SolomonIslands;
				JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "3040301010";
				invoiceLine2.JI_OA_ManufacturerAddress = helper.ExportForwarder.MainAddress.PK;
				invoiceLine2.US_UC_NKCountryOfOrigin = Constants.CountryCodes.PapuaNewGuinea;
				declaration.AllocateEntryNumber("52323435");
				Factory.Save();
				BusinessObjectFactory secondFactory = new BusinessObjectFactory();
				AssertEquals(true, shipment.IsInDatabase);
				AssertEquals("Result", null, Creator.Create(secondFactory));
				Creator.ShipmentPK = shipment.PK;
				CusISFHeader header = Creator.Create(secondFactory);
				AssertEquals("Headers Factory", secondFactory, header.Factory);
				AssertCusISFHeader(header, "SC43", helper.IntermediateConsignee.PK, ZString.Empty, "56-98468845", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, ZString.Empty, "XJ352323435", ZString.Empty, "HB32904934", "MB32342323", "USMXC", "USADD");
				AssertCusISFHeaderLowValueDetails(header, 200, ShippingOrPackingingUnitList.Codes.Plate, 1000m, 2031, Core.Constants.Weight.Kilograms);
				AssertEquals(8, header.DocAddresses.Count);
				AssertDocAddress(header.MainShipToParty, helper.ImportForwarder.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.ShipToParty);
				AssertDocAddress(header.BuyingParty, helper.Consignee.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.BuyingParty);
				AssertDocAddress(header.SellingParty, helper.ExportForwarder.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.SellingParty);
				AssertEquals(true, header.BookingParty.IsEmpty);
				AssertDocAddress(header.Consolidator, helper.ExportForwarder.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.Consolidator);
				AssertDocAddress(header.StuffingLocation, helper.ExportForwarder.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.ScheduledContainerStuffingLocation);
				AssertEquals(2, header.ManufacturerAddresses.Count);
				JobDocAddress manufacturerAddress1 = header.ManufacturerAddresses[0];
				JobDocAddress manufacturerAddress2 = header.ManufacturerAddresses[1];
				if (manufacturerAddress1.E2_OA_Address == helper.ExportForwarder.MainAddress.PK)
				{
					manufacturerAddress1 = header.ManufacturerAddresses[1];
					manufacturerAddress2 = header.ManufacturerAddresses[0];
				}

				AssertDocAddress(manufacturerAddress1, helper.Consignor.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.Manufacturer);
				AssertDocAddress(manufacturerAddress2, helper.ExportForwarder.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.Manufacturer);
				AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.OceanBillOfLading, System.Array.Empty<ZString>());
				AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.HouseBillOfLading, new ZString[] { "HB32904934", "HB968565545" });
				AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.MasterBillOfLading, new ZString[] { "MB32342323", "MB669586445" });
				AssertEquals(2, header.Equipments.Count);
				AssertEquipment(header.Equipments["TURD4865644"], helper.Container20NO.RC_ISOType, helper.Container20NO.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates));
				AssertEquipment(header.Equipments["KJDK9868755"], helper.Container40NO.RC_ISOType, helper.Container40NO.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates));
				AssertEquals(2, header.Lines.Count);
				AssertCusISFLine(header.Lines, manufacturerAddress1.PK, "PARTNO2", "2020301010", Constants.CountryCodes.SolomonIslands);
				AssertCusISFLine(header.Lines, manufacturerAddress2.PK, ZString.Empty, "3040301010", Constants.CountryCodes.PapuaNewGuinea);
				AssertTransferredEvent(header, shipment.PK);
				AssertEquals(ZString.Empty, header.BF_SuretyCode);
				OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(helper.IntermediateConsignee);
				US.Business.CusBondDetailCollection bondDetails = wrapper.BondDetails;
				US.Business.CusBondDetail bondData = bondDetails.AddNew();
				bondData.PW_ActivityCode = ISFBondActivityCodeList.Codes.ISFBond16;
				bondData.PW_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
				bondData.PW_SuretyCode = "968";
				bondData.PW_BondEffectiveDate = new ZDateTime(2007, 1, 1);
				bondData.PW_BondExpiryDate = ZDateTime.Today.AddDays(2);
				Factory.Save();
				header = Creator.Create(secondFactory);
				AssertEquals("Surety code should come from declaration", "791", header.BF_SuretyCode);
				declaration.US_SuretyCode = ZString.Empty;
				Factory.Save();
				header = Creator.Create(secondFactory);
				AssertEquals("Surety code should come from bond details in org", "968", header.BF_SuretyCode);
				AssertEquals(ZGuid.Empty, header.BF_JS_Shipment);
			}
		}

		public void TestCreateFromOverrideData()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				OrgHeader consingee = helper.CreateOrganisation("CONSINGEE", "USLAX", "CONSINGEE ADDRESS 1", "LOS ANGELES", "+1 801 120 456");
				OrgHeader consingor = helper.CreateOrganisation("CONSIGNOR", "AUSYD", "CONSIGNOR ADDRESS 1", "SYDNEY", "+61 2 9156 4568");
				ForwardingShipment shipment = CreateShipment();
				shipment.ConsigneePK = consingee.PK;
				shipment.ConsignorPK = consingor.PK;
				shipment.BuyerDocAddress.E2_AddressOverride = ZBool.True;
				shipment.BuyerDocAddress.E2_AddressOverride = ZBool.False;
				shipment.BuyerDocAddress.OrganisationPK = ZGuid.Empty;
				Factory.Save();
				BusinessObjectFactory secondFactory = new BusinessObjectFactory();
				Creator.ShipmentPK = shipment.PK;
				CusISFHeader header = Creator.Create(secondFactory);
				AssertDocAddress(header.BuyingParty, consingee.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.BuyingParty);
				AssertDocAddress(header.SellingParty, consingor.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.SellingParty);
				shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
				shipment.ConsigneeDocumentaryAddress.E2_CompanyName = "CONSIGNEE NAME";
				shipment.ConsigneeDocumentaryAddress.E2_Address1 = "CONSIGNEE ADDRESS 1";
				shipment.ConsigneeDocumentaryAddress.E2_Address2 = "CONSIGNEE ADDRESS 2";
				shipment.ConsigneeDocumentaryAddress.E2_Postcode = "65432";
				shipment.ConsigneeDocumentaryAddress.E2_City = "LOS ANGELES";
				shipment.ConsigneeDocumentaryAddress.E2_State = "CA";
				shipment.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "US";
				shipment.ConsigneeDocumentaryAddress.E2_Contact = "BROWN SMITH";
				shipment.ConsigneeDocumentaryAddress.E2_Phone = "+61 2 6958 6543";
				shipment.ConsigneeDocumentaryAddress.E2_Fax = "+61 2 6958 6544";
				shipment.ConsigneeDocumentaryAddress.E2_Email = "consingee@where.com";
				shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
				shipment.ConsignorDocumentaryAddress.E2_CompanyName = "CONSIGNOR NAME";
				shipment.ConsignorDocumentaryAddress.E2_Address1 = "CONSIGNOR ADDRESS 1";
				shipment.ConsignorDocumentaryAddress.E2_Address2 = "CONSIGNOR ADDRESS 2";
				shipment.ConsignorDocumentaryAddress.E2_Postcode = "2215";
				shipment.ConsignorDocumentaryAddress.E2_City = "SYDNEY";
				shipment.ConsignorDocumentaryAddress.E2_State = "NSW";
				shipment.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = "AU";
				shipment.ConsignorDocumentaryAddress.E2_Contact = "BOB SMITH";
				shipment.ConsignorDocumentaryAddress.E2_Phone = "+61 2 5684 6543";
				shipment.ConsignorDocumentaryAddress.E2_Fax = "+61 2 5684 6544";
				shipment.ConsignorDocumentaryAddress.E2_Email = "consingor@where.com";
				Factory.Save();
				header = Creator.Create(secondFactory);
				AssertDocAddress(header.BuyingParty, ZGuid.Empty, "CONSIGNEE NAME", "CONSIGNEE ADDRESS 1", "CONSIGNEE ADDRESS 2", "65432", "LOS ANGELES", "CA", "US", "BROWN SMITH", "+61 2 6958 6543", "+61 2 6958 6544", "consingee@where.com", DocAddressTypes.Codes.BuyingParty);
				AssertDocAddress(header.SellingParty, ZGuid.Empty, "CONSIGNOR NAME", "CONSIGNOR ADDRESS 1", "CONSIGNOR ADDRESS 2", "2215", "SYDNEY", "NSW", "AU", "BOB SMITH", "+61 2 5684 6543", "+61 2 5684 6544", "consingor@where.com", DocAddressTypes.Codes.SellingParty);
				AssertEquals(1, header.ManufacturerAddresses.Count);
				AssertDocAddress(header.ManufacturerAddresses[0], ZGuid.Empty, "CONSIGNOR NAME", "CONSIGNOR ADDRESS 1", "CONSIGNOR ADDRESS 2", "2215", "SYDNEY", "NSW", "AU", "BOB SMITH", "+61 2 5684 6543", "+61 2 5684 6544", "consingor@where.com", DocAddressTypes.Codes.Manufacturer);
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				Factory.Save();
				header = Creator.Create(secondFactory);
				AssertDocAddress(header.BuyingParty, ZGuid.Empty, "CONSIGNEE NAME", "CONSIGNEE ADDRESS 1", "CONSIGNEE ADDRESS 2", "65432", "LOS ANGELES", "CA", "US", "BROWN SMITH", "+61 2 6958 6543", "+61 2 6958 6544", "consingee@where.com", DocAddressTypes.Codes.BuyingParty);
				AssertDocAddress(header.SellingParty, ZGuid.Empty, "CONSIGNOR NAME", "CONSIGNOR ADDRESS 1", "CONSIGNOR ADDRESS 2", "2215", "SYDNEY", "NSW", "AU", "BOB SMITH", "+61 2 5684 6543", "+61 2 5684 6544", "consingor@where.com", DocAddressTypes.Codes.SellingParty);
				AssertEquals(1, header.ManufacturerAddresses.Count);
				AssertDocAddress(header.ManufacturerAddresses[0], ZGuid.Empty, "CONSIGNOR NAME", "CONSIGNOR ADDRESS 1", "CONSIGNOR ADDRESS 2", "2215", "SYDNEY", "NSW", "AU", "BOB SMITH", "+61 2 5684 6543", "+61 2 5684 6544", "consingor@where.com", DocAddressTypes.Codes.Manufacturer);
				declaration.JE_OH_Buyer = consingee.PK;
				declaration.JE_OA_SellerAddress = consingor.MainAddress.PK;
				Factory.Save();
			}
		}

		public void TestCreateFromShipmentCoLoadShipments()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				var org1 = Factory.New<OrgHeader>();
				org1.OH_Code = "aaa";
				var cusCode = org1.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
				cusCode.OK_CustomsRegNo = "COFI";
				cusCode.OK_RN_NKCodeCountry = "US";
				var org2 = Factory.New<OrgHeader>();
				org2.OH_Code = "bbb";
				var cusCode2 = org2.CustomsCodes.AddNew();
				cusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
				cusCode2.OK_CustomsRegNo = "COSE";
				cusCode2.OK_RN_NKCodeCountry = "US";
				var org3 = Factory.New<OrgHeader>();
				org3.OH_Code = "ccc";
				var cusCode3 = org3.CustomsCodes.AddNew();
				cusCode3.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
				cusCode3.OK_CustomsRegNo = "COTH";
				cusCode3.OK_RN_NKCodeCountry = "US";
				Factory.Save();
				var carrier = Factory.New<USCarrierCombined>();
				carrier.UI_Code = "ULPA";
				carrier.UI_ModeOfTransportation = "10";
				var carrier2 = Factory.New<USCarrierCombined>();
				carrier2.UI_Code = "COFI";
				carrier2.UI_ModeOfTransportation = "10";
				var carrier3 = Factory.New<USCarrierCombined>();
				carrier3.UI_Code = "COTH";
				carrier3.UI_ModeOfTransportation = "10";
				var buyer = helper.CreateOrganisation("BUYER", "USLAX");
				helper.UpdateOrAddCustomsRegNo(helper.Consignee, "45-4567835", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, helper.UnitedStates);
				var consol = CreateConsol();
				AssertNotEquals(ZString.Empty, consol.JK_MasterBillNum);
				var container1 = AddContainer(consol.Containers, "TURE2343232", "SL324", helper.Container40US.PK, Constants.ContainerModes.FCL);
				var container2 = AddContainer(consol.Containers, "TURE4569789", "SL589", helper.Container20US.PK, Constants.ContainerModes.FCL);
				var shipment = CreateShipment();
				shipment.BuyerDocAddress.E2_OA_Address = buyer.MainAddress.PK;
				AssertNotEquals(ZString.Empty, shipment.JS_HouseBill);
				consol.Shipments.Add(shipment);
				AddOuterPackLine(shipment.OuterPackLines, 10, Constants.PkgUnit.Carton, container1.PK, "1010102010", Core.Constants.CountryCodes.Australia);
				AddOuterPackLine(shipment.OuterPackLines, 20, Constants.PkgUnit.Bag, container2.PK, "2020203020", Core.Constants.CountryCodes.NewZealand);
				var coloadShipment1 = shipment.CoLoadShipments.AddNew();
				coloadShipment1.JS_HouseBill = "CLHW1234";
				AddOuterPackLine(coloadShipment1.OuterPackLines, 2, Constants.PkgUnit.Carton, container1.PK, "1010102010", Core.Constants.CountryCodes.Australia);
				AddOuterPackLine(coloadShipment1.OuterPackLines, 4, Constants.PkgUnit.Bag, container2.PK, "2020203020", Core.Constants.CountryCodes.NewZealand);
				coloadShipment1.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org1.PK;
				var coloadShipment2 = shipment.CoLoadShipments.AddNew();
				coloadShipment2.JS_HouseBill = ZString.Empty;
				AddOuterPackLine(coloadShipment2.OuterPackLines, 3, Constants.PkgUnit.Carton, container1.PK, "1010102010", Core.Constants.CountryCodes.Australia);
				AddOuterPackLine(coloadShipment2.OuterPackLines, 6, Constants.PkgUnit.Bag, container2.PK, "2020203020", Core.Constants.CountryCodes.NewZealand);
				coloadShipment2.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org2.PK;
				var coloadShipment3 = shipment.CoLoadShipments.AddNew();
				coloadShipment3.JS_HouseBill = "CLHW5678";
				AddOuterPackLine(coloadShipment3.OuterPackLines, 5, Constants.PkgUnit.Carton, container1.PK, "1010102010", Core.Constants.CountryCodes.Australia);
				AddOuterPackLine(coloadShipment3.OuterPackLines, 10, Constants.PkgUnit.Bag, container2.PK, "2020203020", Core.Constants.CountryCodes.NewZealand);
				coloadShipment3.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org3.PK;
				BusinessObjectFactory secondFactory = new BusinessObjectFactory();
				AssertEquals("Result", null, Creator.Create(secondFactory));
				Creator.ShipmentPK = shipment.PK;
				AssertEquals("Result", null, Creator.Create(secondFactory));
				Factory.Save();
				AssertNull("Not link to a declaration", shipment.DeclarationForDocuments);
				var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				var cusCodeForOrgProxy = orgProxy.CustomsCodes.AddNew();
				cusCodeForOrgProxy.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
				cusCodeForOrgProxy.OK_CustomsRegNo = "ULPA";
				cusCodeForOrgProxy.OK_RN_NKCodeCountry = "US";
				Factory.Save();
				var carrier0 = secondFactory.New<USCarrierCombined>();
				carrier0.UI_Code = "VOCC";
				carrier0.UI_ModeOfTransportation = TransportModeCodesMessaging.Codes.VesselContainer;
				CusISFHeader header = Creator.Create(secondFactory);
				AssertEquals("Headers Factory", secondFactory, header.Factory);
				AssertCusISFHeader(header, header.BF_SCAC, helper.Consignee.PK, ZString.Empty, "45-4567835", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, ZString.Empty, ZString.Empty, ZString.Empty, "ULPA" + HouseBillNumber, "VOCC" + MasterBillNumber, "USLAX", "USNYK");
				AssertEquals(7, header.DocAddresses.Count);
				AssertDocAddress(header.MainShipToParty, helper.ImportForwarder.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.ShipToParty);
				AssertDocAddress(shipment.BuyerDocAddress, header.BuyingParty, DocAddressTypes.Codes.BuyingParty);
				AssertDocAddress(shipment.ConsignorDocumentaryAddress, header.SellingParty, DocAddressTypes.Codes.SellingParty);
				AssertEquals(true, header.BookingParty.IsEmpty);
				AssertDocAddress(header.Consolidator, helper.ExportForwarder.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.Consolidator);
				AssertDocAddress(header.StuffingLocation, helper.ExportForwarder.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.ScheduledContainerStuffingLocation);
				AssertEquals(1, header.ManufacturerAddresses.Count);
				AssertDocAddress(header.ManufacturerAddresses[0], helper.Consignor.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.Manufacturer);
				AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.OceanBillOfLading, System.Array.Empty<ZString>());
				AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.HouseBillOfLading, new ZString[] { "ULPA" + HouseBillNumber, "COFICLHW1234", "COTHCLHW5678" });
				AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.MasterBillOfLading, new ZString[] { "VOCC" + MasterBillNumber });
				AssertEquals(2, header.Equipments.Count);
				AssertEquipment(header.Equipments["TURE2343232"], helper.Container40US.RC_ISOType, helper.Container40US.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates));
				AssertEquipment(header.Equipments["TURE4569789"], helper.Container20US.RC_ISOType, helper.Container20US.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates));
				AssertEquals(2, header.Lines.Count);
				AssertCusISFLine(header.Lines, ZGuid.Empty, ZString.Empty, "1010102010", Constants.CountryCodes.Australia);
				AssertCusISFLine(header.Lines, ZGuid.Empty, ZString.Empty, "2020203020", Constants.CountryCodes.NewZealand);
				AssertNotEquals(0, header.Transports.Count);
				AssertEquals(consol.Transports.Count, header.Transports.Count);
				for (int i = 0; i < header.Transports.Count; i++)
				{
					AssertTransport(consol.Transports[i], header.Transports[i]);
				}

				AssertTransferredEvent(header, shipment.PK);
				shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
				Factory.Save();
				secondFactory = new BusinessObjectFactory();
				header = Creator.Create(secondFactory);
				AssertEquals("Headers Factory", secondFactory, header.Factory);
				ZString expectedHouseBillNumber = header.BF_HouseBill == "COFICLHW1234" ? "COFICLHW1234" : "COTHCLHW5678";
				AssertCusISFHeader(header, header.BF_SCAC, ZGuid.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, expectedHouseBillNumber, "VOCC" + MasterBillNumber, "USLAX", "USNYK");
				AssertEquals(6, header.DocAddresses.Count);
				AssertDocAddress(header.MainShipToParty, helper.ImportForwarder.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.ShipToParty);
				AssertDocAddress(shipment.BuyerDocAddress, header.BuyingParty, DocAddressTypes.Codes.BuyingParty);
				AssertEquals(true, header.SellingParty.IsEmpty);
				AssertEquals(true, header.BookingParty.IsEmpty);
				AssertDocAddress(header.Consolidator, helper.ExportForwarder.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.Consolidator);
				AssertDocAddress(header.StuffingLocation, helper.ExportForwarder.MainAddress.PK, "", "", "", "", "", "", "", "", "", "", "", DocAddressTypes.Codes.ScheduledContainerStuffingLocation);
				AssertEquals(0, header.ManufacturerAddresses.Count);
				AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.OceanBillOfLading, System.Array.Empty<ZString>());
				AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.HouseBillOfLading, new ZString[] { "COFICLHW1234", "COTHCLHW5678" });
				AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.MasterBillOfLading, new ZString[] { "VOCC" + MasterBillNumber });
				AssertEquals(2, header.Equipments.Count);
				AssertEquipment(header.Equipments["TURE2343232"], helper.Container40US.RC_ISOType, helper.Container40US.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates));
				AssertEquipment(header.Equipments["TURE4569789"], helper.Container20US.RC_ISOType, helper.Container20US.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates));
				AssertEquals(2, header.Lines.Count);
				AssertCusISFLine(header.Lines, ZGuid.Empty, ZString.Empty, "1010102010", Constants.CountryCodes.Australia);
				AssertCusISFLine(header.Lines, ZGuid.Empty, ZString.Empty, "2020203020", Constants.CountryCodes.NewZealand);
				AssertNotEquals(0, header.Transports.Count);
				AssertEquals(consol.Transports.Count, header.Transports.Count);
				for (int i = 0; i < header.Transports.Count; i++)
				{
					AssertTransport(consol.Transports[i], header.Transports[i]);
				}

				consol.Shipments.Remove(shipment);
				Factory.Save();
				secondFactory = new BusinessObjectFactory();
				header = Creator.Create(secondFactory);
				expectedHouseBillNumber = header.BF_HouseBill == "COFICLHW1234" ? "COFICLHW1234" : "COTHCLHW5678";
				AssertEquals("Headers Factory", secondFactory, header.Factory);
				AssertCusISFHeader(header, header.BF_SCAC, ZGuid.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, expectedHouseBillNumber, ZString.Empty, "", "USNYK");
				AssertEquals(6, header.DocAddresses.Count);
				AssertEquals(false, header.MainShipToParty.IsEmpty);
				AssertDocAddress(shipment.BuyerDocAddress, header.BuyingParty, DocAddressTypes.Codes.BuyingParty);
				AssertEquals(true, header.SellingParty.IsEmpty);
				AssertEquals(true, header.BookingParty.IsEmpty);
				AssertEquals(true, header.Consolidator.IsEmpty);
				AssertEquals(true, header.StuffingLocation.IsEmpty);
				AssertEquals(0, header.ManufacturerAddresses.Count);
				AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.OceanBillOfLading, System.Array.Empty<ZString>());
				AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.HouseBillOfLading, new ZString[] { "COFICLHW1234", "COTHCLHW5678" });
				AssertHasBill(header.ReferenceDatas, BillTypeList.Codes.MasterBillOfLading, System.Array.Empty<ZString>());
				AssertEquals(0, header.Equipments.Count);
				AssertEquals(2, header.Lines.Count);
				AssertCusISFLine(header.Lines, ZGuid.Empty, ZString.Empty, "1010102010", Constants.CountryCodes.Australia);
				AssertCusISFLine(header.Lines, ZGuid.Empty, ZString.Empty, "2020203020", Constants.CountryCodes.NewZealand);
				AssertNotEquals(0, header.Transports.Count);
				AssertEquals(shipment.Transports.Count, header.Transports.Count);
				for (int i = 0; i < header.Transports.Count; i++)
				{
					AssertTransport(shipment.Transports[i], header.Transports[i]);
				}
			}
		}

		public void TestUseManufAddressWhenCreatingFromShipment()
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = CreateShipment();
			shipment.ManufacturerDocAddress.OrganisationPK = manufacturer.PK;
			Creator.ShipmentPK = shipment.PK;
			var isf = Creator.Create(Factory);
			var addresses = isf.ManufacturerAddresses.Select(a => a.Address1).ToList();
			Assert(addresses.Contains(shipment.ManufacturerDocAddress.Address1));
		}

		public void TestUseConsignorAddressWhenCreatingFromShipment()
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = CreateShipment();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = manufacturer.PK;
			Creator.ShipmentPK = shipment.PK;
			var isf = Creator.Create(Factory);
			var addresses = isf.ManufacturerAddresses.Select(a => a.Address1).ToList();
			Assert(addresses.Contains(shipment.ConsignorDocumentaryAddress.Address1));
			AssertEquals(ZGuid.Empty, isf.BF_JS_Shipment);
		}

		public void TestUseManufAddressWhenCreatingFromBuyerConsol()
		{
			var manufacturer1 = Factory.New<OrgHeader>();
			var manufacturer2 = Factory.New<OrgHeader>();
			var manufacturer3 = Factory.New<OrgHeader>();
			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			var shipment = CreateShipment();
			consol.Shipments.Add(shipment);
			shipment.ManufacturerDocAddress.OrganisationPK = manufacturer1.PK;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OA_SellerAddress = Factory.New<OrgHeader>().MainAddress.PK;
			// Multiple manufactures
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment.CoLoadShipments.Add(shipment1);
			shipment1.ManufacturerDocAddress.OrganisationPK = manufacturer2.PK;
			var shipment2 = Factory.New<ForwardingShipment>();
			shipment.CoLoadShipments.Add(shipment2);
			shipment2.ManufacturerDocAddress.OrganisationPK = manufacturer3.PK;
			Creator.ShipmentPK = shipment.PK;
			var isf = Creator.Create(Factory);
			var manufacturerPKs = isf.ManufacturerAddresses.Select(x => x.OrganisationPK);
			AssertEquals(3, manufacturerPKs.Count());
			Assert(manufacturerPKs.Contains(manufacturer2.PK));
			Assert(manufacturerPKs.Contains(manufacturer3.PK));
			Assert(manufacturerPKs.Contains(manufacturer1.PK));
			// Duplicate manufactures
			shipment1.ManufacturerDocAddress.OrganisationPK = manufacturer2.PK;
			shipment2.ManufacturerDocAddress.OrganisationPK = manufacturer2.PK;
			isf = Creator.Create(Factory);
			manufacturerPKs = isf.ManufacturerAddresses.Select(x => x.OrganisationPK);
			AssertEquals(2, manufacturerPKs.Count());
			Assert(manufacturerPKs.Contains(manufacturer2.PK));
			Assert(!manufacturerPKs.Contains(manufacturer3.PK));
			Assert(manufacturerPKs.Contains(manufacturer1.PK));
		}

		public void TestCreateISF_FromNonForwardingShipmentShipment_DoesNotHaveInvalidShipmentError()
		{
			var nonForwardingShipment = (CommonShipment)Factory.New<ICFSShipment>();
			nonForwardingShipment.FillWithValidTestData();
			nonForwardingShipment.JS_RL_NKOrigin = "AUSYD";
			nonForwardingShipment.JS_TransportMode = "SEA";
			Assert("precondition", !nonForwardingShipment.JS_IsForwardRegistered);
			Creator.ShipmentPK = nonForwardingShipment.PK;
			AssertNoNotifications(Creator.ShipmentPKInfo);
			var isf = Creator.Create(Factory);
			AssertNotNull(isf);
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new DeclarationTestHelper(Factory);
		}

		protected override BusinessObject GetNewBusinessObject() => new ISFFromShipmentCreator(Factory);

		protected ForwardingShipment CreateShipment()
		{
			var nzd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.NewZealand);
			nzd.ExchangeRates.AddNew();
			var nzdCustomsRate = nzd.ExchangeRates.AddNew();
			nzdCustomsRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.CustomsRate;
			nzdCustomsRate.RE_StartDate = ZDateTime.Today.AddDays(-364);
			nzdCustomsRate.RE_ExpiryDate = ZDateTime.Today.AddYears(1);
			nzdCustomsRate.RE_SellRate = 0.7m;
			RefExchangeRate nzdNoneCustomsRate = nzd.ExchangeRates.AddNew();
			nzdNoneCustomsRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.IATARate;
			nzdNoneCustomsRate.RE_StartDate = ZDateTime.Today.AddMonths(-1);
			nzdNoneCustomsRate.RE_ExpiryDate = ZDateTime.Today.AddYears(1);
			nzdNoneCustomsRate.RE_SellRate = 0.8m;
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.ConsignorPK = helper.Consignor.PK;
			shipment.ConsigneePK = helper.Consignee.PK;
			shipment.JS_HouseBill = HouseBillNumber;
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_E_DEP = ZDateTime.Today;
			shipment.JS_RL_NKDestination = "USNYK";
			shipment.JS_E_ARV = ZDateTime.Today.AddDays(19);
			shipment.JS_GoodsValue = 1400m;
			shipment.JS_RX_NKGoodsValueCurr = nzd.RX_Code;
			shipment.JS_TotalPackageCount = 150;
			shipment.JS_F3_NKTotalCountPackType = ShippingOrPackingingUnitList.Codes.Roll;
			shipment.JS_ActualWeight = 1.51m;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Tonnes;
			AddTransport(shipment.Transports, Constants.TransportModes.Road, Constants.TransportPlanningType.PreCarriage, "", "REG897", "AUMEL", "AUSYD", ZDateTime.Today.AddDays(-1), ZDateTime.Today);
			AddTransport(shipment.Transports, Constants.TransportModes.Road, Constants.TransportPlanningType.OnBoardCourier, "", "COUR598", "USLAX", "USNYK", ZDateTime.Today.AddDays(14), ZDateTime.Today.AddDays(19));
			PopulateDocAddress(shipment.BuyerDocAddress, ZGuid.Empty, "BUYER", "325234", "LOS ANGELES", "LA", "US", "2641658", "2641659");
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = helper.ImportForwarder.MainAddress.PK;
			return shipment;
		}

		protected ForwardingConsol CreateConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_MasterBillNum = MasterBillNumber;
			consol.SetDefaultSendingForwarderAddress(helper.ExportForwarder);
			consol.SetDefaultShippingLineAddress(helper.ShippingLine);
			consol.JK_OA_PackDepotAddress = helper.ExportForwarder.MainAddress.PK;
			AddTransport(consol.Transports, Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel, helper.VesselWithCountry.RV_Code, "434", "AUSYD", "USCHI", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(10));
			AddTransport(consol.Transports, Constants.TransportModes.Rail, Constants.TransportPlanningType.OnForwarding, "", "TRAIN323", "USCHI", "USLAX", ZDateTime.Today.AddDays(11), ZDateTime.Today.AddDays(13));
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "OIF";
			org.OH_FullName = "ISFORG";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "VOCC";
			cusCode.OK_RN_NKCodeCountry = "US";
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
			return consol;
		}

		void AssertCusISFHeaderLowValueDetails(CusISFHeader header, ZInt estimatedQuantity, ZString estimatedQuantityUQ, ZDecimal estimatedValue, ZInt estimatedWeight, ZString estimatedWeightUQ)
		{
			AssertEquals(estimatedQuantity, header.BF_EstimatedQuantity);
			AssertEquals(estimatedQuantityUQ, header.BF_EstimatedQuantityUQ);
			AssertEquals(estimatedValue, header.BF_EstimatedValue);
			AssertEquals(estimatedWeight, header.BF_EstimatedWeight);
			AssertEquals(estimatedWeightUQ, header.BF_EstimatedWeightUQ);
		}

		void AssertTransferredEvent(CusISFHeader header, ZGuid shipmentPK)
		{
			BusinessObjectFactory headerFactory = header.Factory;
			ForwardingShipment shipmentInHeaderFactory = headerFactory.Load<ForwardingShipment>(shipmentPK);
			ZQuery transferredQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Transferred.Code);
			AssertEquals(0, shipmentInHeaderFactory.Logs.Find(transferredQuery).Length);
			AssertEquals(ZString.Empty, header.BF_JobReference);
			headerFactory.Save();
			transferredQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Transferred.Code);
			AssertEquals(1, shipmentInHeaderFactory.Logs.Find(transferredQuery).Length);
			AssertNotEquals(ZString.Empty, header.BF_JobReference);
			AssertNotNull(shipmentInHeaderFactory.Logs.MostRecentLogByEventTime(Events.Transferred, "To " + header.BF_JobReference));
		}

		void AssertTransport(Transport soureceTransport, Transport transport)
		{
			AssertNotEquals(soureceTransport.JW_ParentGUID, transport.JW_ParentGUID);
			AssertNotEquals(soureceTransport.JW_ParentType, transport.JW_ParentType);
			AssertEquals(soureceTransport.JW_TransportType, transport.JW_TransportType);
			AssertEquals(soureceTransport.JW_Vessel, transport.JW_Vessel);
			AssertEquals(soureceTransport.JW_VoyageFlight, transport.JW_VoyageFlight);
			AssertEquals(soureceTransport.JW_RL_NKLoadPort, transport.JW_RL_NKLoadPort);
			AssertEquals(soureceTransport.JW_RL_NKDiscPort, transport.JW_RL_NKDiscPort);
			AssertEquals(soureceTransport.JW_ETD, transport.JW_ETD);
			AssertEquals(soureceTransport.JW_ATD, transport.JW_ATD);
			AssertEquals(soureceTransport.JW_ETA, transport.JW_ETA);
			AssertEquals(soureceTransport.JW_ATA, transport.JW_ATA);
			AssertEquals(soureceTransport.JW_OA_CarrierAddress, transport.JW_OA_CarrierAddress);
		}

		void AssertCusISFLine(CusISFLineCollection lines, ZGuid manufacturerDocAddressPK, ZString partNo, ZString tariff, ZString countryOfOrigin)
		{
			ZQuery query = new ZQuery(CusISFLineSchema.BL_TextProductCode, partNo);
			if (manufacturerDocAddressPK.IsValid)
			{
				query.AddToFilter(CusISFLineSchema.BL_ManufacturerDocAddressPK, manufacturerDocAddressPK);
			}
			else
			{
				query.AddToFilter(CusISFLineSchema.BL_ManufacturerDocAddressPK, null);
			}

			query.AddToFilter(CusISFLineSchema.BL_HarmonisedNum, tariff);
			query.AddToFilter(CusISFLineSchema.BL_RN_NKGoodsOrigin, countryOfOrigin);
			List<CusISFLine> list = new List<CusISFLine>(lines.Find(query));
			AssertEquals(1, list.Count);
		}

		void AssertEquipment(CusISFEquip equipment, ZString containerISO, ZString equipCode)
		{
			AssertEquals(containerISO, equipment.BE_ContainerISO);
			AssertEquals(equipCode, equipment.BE_EquipCode);
		}

		void AssertDocAddress(JobDocAddress docAddress, ZGuid addressPK, ZString companyName, ZString address1, ZString address2, ZString postCode, ZString city, ZString state, ZString countryCode, ZString contactName, ZString phone, ZString fax, ZString email, ZString addressType)
		{
			if (addressPK.IsEmpty)
			{
				AssertEquals(true, docAddress.E2_AddressOverride);
				AssertEquals(companyName, docAddress.E2_CompanyName);
				AssertEquals(address1, docAddress.E2_Address1);
				AssertEquals(address2, docAddress.E2_Address2);
				AssertEquals(postCode, docAddress.E2_Postcode);
				AssertEquals(city, docAddress.E2_City);
				AssertEquals(state, docAddress.E2_State);
				AssertEquals(countryCode, docAddress.E2_RN_NKCountryCode);
				AssertEquals(contactName, docAddress.E2_Contact);
				AssertEquals(phone, docAddress.E2_Phone);
				AssertEquals(fax, docAddress.E2_Fax);
				AssertEquals(email, docAddress.E2_Email);
			}
			else
			{
				AssertEquals(addressPK, docAddress.E2_OA_Address);
				AssertEquals(false, docAddress.E2_AddressOverride);
			}

			AssertEquals(addressType, docAddress.E2_AddressType);
		}

		void AssertDocAddress(JobDocAddress sourceDocAddress, JobDocAddress docAddress, ZString addressType)
		{
			AssertNotEquals(sourceDocAddress.E2_ParentID, docAddress.E2_ParentID);
			AssertDocAddress(docAddress, sourceDocAddress.E2_AddressOverride ? ZGuid.Empty : sourceDocAddress.E2_OA_Address, sourceDocAddress.E2_CompanyName, sourceDocAddress.E2_Address1, sourceDocAddress.E2_Address2, sourceDocAddress.E2_Postcode, sourceDocAddress.E2_City, sourceDocAddress.E2_State, sourceDocAddress.E2_RN_NKCountryCode, sourceDocAddress.E2_Contact, sourceDocAddress.E2_Phone, sourceDocAddress.E2_Fax, sourceDocAddress.E2_Email, addressType);
		}

		void AssertHasBill(CusISFBillCollection bills, ZString billType, ZString[] expectedNumbers)
		{
			List<string> actualNumbers = new List<string>();
			foreach (CusISFBill element in bills.Find(new ZQuery(CusISFBillSchema.BB_BillType, billType)))
			{
				actualNumbers.Add(element.BB_BillNum);
			}

			AssertContainsExactElementsInAnyOrder(expectedNumbers, actualNumbers);
		}

		void AssertCusISFHeader(CusISFHeader header, ZString scac, ZGuid importerPK, ZString ownerReference, ZString consigneeCode, ZString consigneeCodeType, ZString suretyCode, ZString entryNumber, ZString oceanBill, ZString houseBill, ZString masterBill, ZString portOfUnload, ZString placeOfDelivery)
		{
			AssertEquals(SubmissionTypeList.Codes.ISF10, header.BF_EntryType);
			AssertEquals(ShipmentTypeList.Codes.StandardOrRegularFilings, header.BF_ShipmentType);
			AssertEquals(scac, header.BF_SCAC);
			AssertEquals(importerPK, header.BF_OH_Importer);
			AssertEquals(ownerReference, header.BF_OwnerReference);
			AssertEquals(consigneeCode, header.BF_ConsigneeCode);
			AssertEquals(consigneeCodeType, header.BF_ConsigneeCodeType);
			AssertEquals(suretyCode, header.BF_SuretyCode);
			AssertEquals(entryNumber, header.BF_EntryNumber);
			AssertEquals(oceanBill, header.BF_OceanBill);
			AssertEquals(houseBill, header.BF_HouseBill);
			AssertEquals(masterBill, header.BF_MasterBill);
			AssertEquals(portOfUnload, header.BF_RL_NKPortOfUnload);
			AssertEquals(placeOfDelivery, header.BF_RL_NKPlaceOfDelivery);
		}

		PackLine AddOuterPackLine(OuterPackLineCollection outerPackLines, ZInt packageCount, ZString packageType, ZGuid containerPK, ZString tariff, ZString origin)
		{
			PackLine packLine = outerPackLines.AddNew();
			packLine.JL_PackageCount = packageCount;
			packLine.JL_F3_NKPackType = packageType;
			packLine.JL_JC = containerPK;
			packLine.JL_HarmonisedCode = tariff;
			packLine.JL_RN_NKOrigin = origin;
			return packLine;
		}

		void PopulateDocAddress(JobDocAddress docAddress, ZGuid addressPK, ZString name, ZString postCode, ZString city, ZString state, ZString countryCode, ZString phone, ZString fax)
		{
			if (addressPK.IsEmpty)
			{
				docAddress.E2_AddressOverride = true;
				docAddress.E2_CompanyName = name + "COMPANY";
				docAddress.E2_Address1 = name + "ADDRESS 1";
				docAddress.E2_Address2 = name + "ADDRESS 2";
				docAddress.E2_Postcode = postCode;
				docAddress.E2_City = city;
				docAddress.E2_State = state;
				docAddress.E2_RN_NKCountryCode = countryCode;
				docAddress.E2_Contact = "MR " + name;
				docAddress.E2_Phone = phone;
				docAddress.E2_Fax = fax;
				docAddress.E2_Email = name.ToLower() + "@where.com";
			}
			else
			{
				docAddress.E2_OA_Address = addressPK;
			}
		}

		ForwardingContainer AddContainer(ForwardingContainerCollection containers, ZString containerNumber, ZString sealNumber, ZGuid containerTypePK, ZString mode)
		{
			ForwardingContainer container = containers.AddNew();
			container.JC_ContainerNum = containerNumber;
			container.JC_SealNum = sealNumber;
			container.JC_ContainerMode = mode;
			container.JC_RC = containerTypePK;
			return container;
		}

		Transport AddTransport(TransportCollection transports, ZString transportMode, ZString transportType, ZString vessel, ZString voyageFlight, ZString loadPort, ZString dischargePort, ZDateTime etd, ZDateTime eta)
		{
			Transport transport = transports.AddNew();
			transport.JW_TransportMode = transportMode;
			transport.JW_TransportType = transportType;
			transport.JW_Vessel = vessel;
			transport.JW_VoyageFlight = voyageFlight;
			transport.JW_RL_NKLoadPort = loadPort;
			transport.JW_RL_NKDiscPort = dischargePort;
			transport.JW_ETD = etd;
			transport.JW_ETA = eta;
			return transport;
		}

		ISFFromShipmentCreator creator;
		ISFFromShipmentCreator Creator => creator ?? (creator = new ISFFromShipmentCreator(Factory));

		DeclarationTestHelper helper;
		const string MasterBillNumber = "MB12322333";
		const string HouseBillNumber = "HB323400832";
	}
}
