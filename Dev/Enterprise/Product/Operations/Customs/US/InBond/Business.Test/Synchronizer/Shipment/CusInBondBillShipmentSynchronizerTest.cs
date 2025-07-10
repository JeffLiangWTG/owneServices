using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondBillShipmentSynchronizerTest : SynchroniserTestCase
	{
		public void TestSynchroniseHouseBill()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				var carrier0 = Factory.New<USCarrierCombined>();
				carrier0.UI_Code = "SCAC";
				carrier0.UI_ModeOfTransportation = TransportModeCodes.Codes.VesselContainer;
				shipment.JS_HouseBill = "SCACHB1";
				shipment.JS_TransportMode = "SEA";
				var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				var orgProxyCarrierCode = orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SCAC", Core.Constants.CountryCodes.UnitedStates);
				var header = Factory.New<CusInBondHeader>();
				header.BH_ParentID = shipment.PK;
				header.BH_ParentTableCode = shipment.TablePrefix;
				header.Synchroniser.Synchronise(true);
				AssertEquals(1, header.Bills.Count);
				var bill = header.Bills[0];
				AssertEquals("HB1", bill.B0_HouseBillNumber);
				AssertEquals("SCAC", bill.B0_HouseBillIssuerCode);
				shipment.JS_HouseBill = "SCA-CHB1";
				AssertEquals("HB1", bill.B0_HouseBillNumber);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				declaration.JE_JS = shipment.PK;
				declaration.JE_OverrideFreightDefaults = ZBool.True;
				declaration.JE_HouseBill = "HB2";
				declaration.JE_HouseBillIssuerSCAC = "ABCD";
				shipment.RaiseJobDeclarationCreated(declaration);
				AssertEquals("HB2", bill.B0_HouseBillNumber);
				AssertEquals("ABCD", bill.B0_HouseBillIssuerCode);
				declaration.JE_HouseBill = "HB2-123";
				AssertEquals("HB2123", bill.B0_HouseBillNumber);
			}
		}

		public void TestRecalculateDataWhenDeclarationIsCreated()
		{
			var carrier0 = Factory.New<USCarrierCombined>();
			carrier0.UI_Code = "OT12";
			carrier0.UI_ModeOfTransportation = TransportModeCodes.Codes.VesselContainer;
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDestination = "USNYC";
			shipment.JS_ActualWeight = 23.76m;
			shipment.JS_UnitOfWeight = "OT";
			shipment.JS_ActualVolume = 89.76m;
			shipment.JS_UnitOfVolume = "M3";
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			header.Synchroniser.Synchronise(true);
			AssertEquals(1, header.Bills.Count);
			var bill = header.Bills[0];
			AssertEquals("Issuer Code", "OT12", bill.B0_IssuerCode);
			AssertEquals("Bill Number", "MA1TERCONSOL", bill.B0_MasterBillNumber);
			AssertEquals("Qty", 0, bill.B0_ManifestQty);
			AssertEquals("UQ", "", bill.B0_ManifestUQ);
			AssertEquals("Weight", 23.76m, bill.B0_Weight);
			AssertEquals("Weight UQ", "OT", bill.B0_WeightUQ);
			AssertEquals("Volume", 89.76m, bill.B0_Volume);
			AssertEquals("Volume UQ", "M3", bill.B0_VolumeUQ);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = ZBool.True;
			declaration.JE_MasterBill = "DECLMASTER";
			declaration.JE_MasterBillIssuerSCAC = "OT23";
			declaration.JE_TotalNoOfPacks = 11;
			declaration.JE_TotalNoOfPacksPackType = "NO";
			declaration.JE_TotalWeight = 46.5m;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalVolume = 89.3m;
			declaration.JE_TotalVolumeUnit = "L";
			AssertEquals("Issuer Code", "OT12", bill.B0_IssuerCode);
			AssertEquals("Bill Number", "MA1TERCONSOL", bill.B0_MasterBillNumber);
			AssertEquals("Qty", 0, bill.B0_ManifestQty);
			AssertEquals("UQ", "", bill.B0_ManifestUQ);
			AssertEquals("Weight", 23.76m, bill.B0_Weight);
			AssertEquals("Weight UQ", "OT", bill.B0_WeightUQ);
			AssertEquals("Volume", 89.76m, bill.B0_Volume);
			AssertEquals("Volume UQ", "M3", bill.B0_VolumeUQ);
			shipment.RaiseJobDeclarationCreated(declaration);
			AssertEquals("Issuer Code", "OT23", bill.B0_IssuerCode);
			AssertEquals("Bill Number", "DECLMASTER", bill.B0_MasterBillNumber);
			AssertEquals("Qty", 11, bill.B0_ManifestQty);
			AssertEquals("UQ", "NO", bill.B0_ManifestUQ);
			AssertEquals("Weight", 46.5m, bill.B0_Weight);
			AssertEquals("Weight UQ", "KG", bill.B0_WeightUQ);
			AssertEquals("Volume", 89.3m, bill.B0_Volume);
			AssertEquals("Volume UQ", "L", bill.B0_VolumeUQ);
		}

		public void TestSynchronizeFromDeclaration()
		{
			shipment.JS_ActualWeight = 23.76m;
			shipment.JS_UnitOfWeight = "OT";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_TransportMode = "AIR";
			declaration.US_UI_NKCarrierSCAC = "QF";
			declaration.JE_VoyageFlightNo = "QF119";
			declaration.US_SchDLoading = "64234";
			declaration.US_SchDArrival = "2704";
			declaration.US_UC_NKCountryOfExport = "AU";
			declaration.JE_MasterBill = "DECLMASTER";
			declaration.JE_MasterBillIssuerSCAC = "OT23";
			declaration.JE_TotalNoOfPacks = 11;
			declaration.JE_TotalNoOfPacksPackType = "KG";
			declaration.JE_TotalWeight = 46;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalVolume = 89;
			declaration.JE_TotalVolumeUnit = "M3";
			Factory.Save();
			var inbondHeader = Factory.New<CusInBondHeader>();
			inbondHeader.BH_ParentID = shipment.PK;
			inbondHeader.BH_ParentTableCode = shipment.TablePrefix;
			inbondHeader.Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("Should be one bill", 1, inbondHeader.Bills.Count);
			var bill = inbondHeader.Bills[0];
			AssertEquals("Bill Number", "DECLMASTER", bill.B0_MasterBillNumber);
			AssertEquals("Issuer Code", "OT23", bill.B0_IssuerCode);
			AssertEquals("Qty should be from declaration", 11, bill.B0_ManifestQty);
			AssertEquals("UQ should be from declaration", "KG", bill.B0_ManifestUQ);
			AssertEquals("Weight from declaration", 46m, bill.B0_Weight);
			AssertEquals("Weight UQ from declaration", "KG", bill.B0_WeightUQ);
			AssertEquals("Volume from declaration", 89m, bill.B0_Volume);
			AssertEquals("Volume UQ from declaration", "M3", bill.B0_VolumeUQ);
		}

		public void TestSynchronizeFromConsolShipment()
		{
			var carrier0 = Factory.New<USCarrierCombined>();
			carrier0.UI_Code = "OT12";
			carrier0.UI_ModeOfTransportation = TransportModeCodes.Codes.VesselContainer;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = importer.PK;
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_E_DEP = new ZDateTime(2011, 04, 03);
			shipment.JS_E_ARV = new ZDateTime(2011, 04, 15);
			shipment.JS_ActualWeight = 23.76m;
			shipment.JS_UnitOfWeight = "OT";
			shipment.JS_ActualVolume = 89.76m;
			shipment.JS_UnitOfVolume = "M3";
			Factory.Save();
			var inbondHeader = Factory.New<CusInBondHeader>();
			inbondHeader.BH_ParentID = shipment.PK;
			inbondHeader.BH_ParentTableCode = shipment.TablePrefix;
			inbondHeader.Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("Should be one bill", 1, inbondHeader.Bills.Count);
			var bill = inbondHeader.Bills[0];
			AssertEquals("Bill Number", "MA1TERCONSOL", bill.B0_MasterBillNumber);
			AssertEquals("Issuer Code", "OT12", bill.B0_IssuerCode);
			AssertEquals("Qty should be 0, because declaration doesn't exist", 0, bill.B0_ManifestQty);
			AssertEquals("UQ should be empty, becuase declaration doesn't exist", "", bill.B0_ManifestUQ);
			AssertEquals("Weight", 23.76m, bill.B0_Weight);
			AssertEquals("Weight UQ", "OT", bill.B0_WeightUQ);
			AssertEquals("Volume", 89.76m, bill.B0_Volume);
			AssertEquals("Volume UQ", "M3", bill.B0_VolumeUQ);
			consol.JK_MasterBillNum = "58648658";
			AssertEquals("Bill Number", "58648658", bill.B0_MasterBillNumber);
			AssertEquals("Issuer Code", "OT12", bill.B0_IssuerCode);
		}

		public void TestDeletingMoveDetailsIfNeeded()
		{
			var carrier0 = Factory.New<USCarrierCombined>();
			carrier0.UI_Code = "OT12";
			carrier0.UI_ModeOfTransportation = TransportModeCodes.Codes.VesselContainer;
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
			var bill = header.Bills.AddNew();
			bill.B0_IssuerCode = "OT12";
			bill.B0_MasterBillNumber = "MA1TERCONSOL";
			var moveHeader = header.MovementHeader;
			var moveDetail1 = moveHeader.MovementDetails.AddNew(bill.PK);
			var moveDetail2 = moveHeader.MovementDetails.AddNew(bill.PK);
			moveDetail2.B9_SeqNo = "1";
			moveDetail1.B9_SeqNo = "2";
			header.Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(true, moveDetail1.IsDeleted);
			AssertEquals(false, moveDetail2.IsDeleted);
		}

		public void TestSynchronizeOrganizations()
		{
			var carrier0 = Factory.New<USCarrierCombined>();
			carrier0.UI_Code = "OT12";
			carrier0.UI_ModeOfTransportation = TransportModeCodes.Codes.VesselContainer;
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = new ZDateTime(2011, 04, 03);
			shipment.JS_E_ARV = new ZDateTime(2011, 04, 15);
			shipment.JS_ActualWeight = 23.76m;
			shipment.JS_UnitOfWeight = "OT";
			shipment.JS_ActualVolume = 89.76m;
			shipment.JS_UnitOfVolume = "M3";
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_RL_NKClosestPort = "USLAX";
			importer.OH_FullName = "First importer";
			var importerAddress = importer.Addresses.AddNew();
			importerAddress.OA_Address1 = "Test for synch";
			importerAddress.OA_City = "city";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = importerAddress.PK;
			var importerContact = importer.Contacts.AddNew();
			importerContact.OC_ContactName = "IMPORTER";
			importerContact.OC_Email = "importer@test.com";
			shipment.ConsigneeDocumentaryAddress.ContactPK = importerContact.PK;
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_RL_NKClosestPort = "AUMEL";
			supplier.OH_FullName = "FIRST SUPPLIER";
			var supplierAddress = supplier.Addresses.AddNew();
			supplierAddress.OA_Address1 = "Test for supplier synch";
			supplierAddress.OA_City = "test city";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = supplierAddress.PK;
			var supplierContact = importer.Contacts.AddNew();
			supplierContact.OC_ContactName = "SUPPLIER";
			supplierContact.OC_Email = "supplier@test.com";
			shipment.ConsignorDocumentaryAddress.ContactPK = supplierContact.PK;
			var notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty.OH_FullName = "NOTIFY PARTY 1";
			var notifyPartyAddress = notifyParty.Addresses.AddNew();
			notifyPartyAddress.OA_Address1 = "TEST 2";
			notifyPartyAddress.OA_City = "CITY2";
			shipment.DocAddresses.FindOrCreateWithDocAddressType(notifyPartyAddress.PK, MasterFiles.Integration.DocAddressType.NotifyParty);
			Factory.Save();
			var inbondHeader = Factory.New<CusInBondHeader>();
			inbondHeader.BH_ParentID = shipment.PK;
			inbondHeader.BH_ParentTableCode = shipment.TablePrefix;
			inbondHeader.Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("Should be one bill", 1, inbondHeader.Bills.Count);
			var bill = inbondHeader.Bills[0];
			AssertEquals("Bill Number", "MA1TERCONSOL", bill.B0_MasterBillNumber);
			AssertEquals("Issuer Code", "OT12", bill.B0_IssuerCode);
			AssertEquals(supplierAddress.PK, bill.ForeignShipper.E2_OA_Address);
			AssertEquals(importerAddress.PK, bill.Consignee.E2_OA_Address);
			AssertEquals(notifyPartyAddress.PK, bill.NotifyParty.E2_OA_Address);
			AssertEquals(importerContact.OC_ContactName, bill.Consignee.E2_Contact);
			AssertEquals(supplierContact.OC_ContactName, bill.ForeignShipper.E2_Contact);
			//address override
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
			inbondHeader.Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("First importer", bill.Consignee.E2_CompanyName);
			AssertEquals("Test for synch", bill.Consignee.E2_Address1);
			AssertEquals("city", bill.Consignee.E2_City);
			AssertEquals("US", bill.Consignee.E2_RN_NKCountryCode);
			AssertEquals("", bill.Consignee.E2_Postcode);
			AssertEquals("IMPORTER", bill.Consignee.E2_Contact);
			AssertEquals("importer@test.com", bill.Consignee.E2_Email);
			AssertEquals("", bill.Consignee.E2_Phone);
			AssertEquals("FIRST SUPPLIER", bill.ForeignShipper.E2_CompanyName);
			AssertEquals("Test for supplier synch", bill.ForeignShipper.E2_Address1);
			AssertEquals("test city", bill.ForeignShipper.E2_City);
			AssertEquals("US", bill.ForeignShipper.E2_RN_NKCountryCode);
			AssertEquals("", bill.ForeignShipper.E2_Postcode);
			AssertEquals("SUPPLIER", bill.ForeignShipper.E2_Contact);
			AssertEquals("supplier@test.com", bill.ForeignShipper.E2_Email);
			AssertEquals("", bill.ForeignShipper.E2_Phone);
			AssertEquals("NOTIFY PARTY 1", bill.NotifyParty.E2_CompanyName);
			AssertEquals("TEST 2", bill.NotifyParty.E2_Address1);
			AssertEquals("CITY2", bill.NotifyParty.E2_City);
			AssertEquals("US", bill.NotifyParty.E2_RN_NKCountryCode);
			AssertEquals("", bill.NotifyParty.E2_Postcode);
			AssertEquals("", bill.NotifyParty.E2_Contact);
			AssertEquals("", bill.NotifyParty.E2_Email);
			AssertEquals("", bill.NotifyParty.E2_Phone);
		}

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_MasterBillNum = "MA1TERCONSOL";
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "OT12";
			cusCode.OK_RN_NKCodeCountry = "US";
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
			shipment = consol.Shipments.AddNew();
			importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMP001";
			importer.OH_FullName = "Importer One Co. Ltd";
		}

		ForwardingConsol consol;
		ForwardingShipment shipment;
		OrgHeader importer;
	}
}
