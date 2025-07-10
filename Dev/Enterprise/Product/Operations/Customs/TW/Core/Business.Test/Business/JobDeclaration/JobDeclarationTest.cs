using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobDeclaration))]
	sealed class JobDeclarationTest : Customs.Business.Testing.BaseJobDeclarationTest<JobDeclaration>
	{
		[ExpectNoExceptions]
		public void TestAdditionalBizoCaptionsAndDescriptions()
		{
			var decl = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(decl.JE_Calc_TWTransportCodeInfo, "Transport Code", "The code of the cargo's transport mode.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(decl.JE_VesselArrivalRegInfo, "Vessel Reg.", "The vessel registration number of Customs.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(decl.JE_CustomsOfficeInfo, "Transport Code", "The code of the cargo's transport mode.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(decl.JE_LocationOfGoodsInfo, "Goods Location", "Goods Location of Export/Office of Lading/Unlading.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(decl.JE_TotalNoOfPacksInfo, "Total Packages No.", "Total number of packages on declaration or of the shipment delivered.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(decl.JE_PaymentMethodInfo, "Payment Method", "The payment method of the declaration. The default can be set against the Importer's/Exporter's Organization > Details > Configuration > Taiwan.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(decl.JE_DateAtOriginInfo, "ETD", "The export date on the bills.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(decl.JE_DateAtFinalDestinationInfo, "ETA", "The import date of the shipment.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(decl.JE_TotalWeightInfo, "Total Weight", "Total gross weight on declaration. The unit of measurement is KGM. When the unit of measurement entered is not KGM, it will be converted into KGM for declaration.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(decl.JE_EntryStatusInfo, "Entry Status", "The entry status of the declaration.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(decl.JE_MessageStatusInfo, "Message Status", "The message status of the declaration.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(decl.DeclarationNumberDisplayInfo, "Entry Number", "The entry number of the customs declaration. The number range for entry number allocation can be set up against the login company profile.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(decl.ClearanceStatusInfo, "Clearance Status", "The clearance status of the declaration.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(decl.JE_GS_NKCusAgentInfo, "Broker Staff", "The broker staff assigned on declaration.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(decl.CusAgentCertificateNumberInfo, "Broker License", "The Broker License issued by customs to the qualified customs broker.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(decl.JE_CustomsProfileInfo, "Mail Box", "The Mail Box code issued by the third party network company and the sub box number issued by Customs to the customs broker.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(decl.JE_DefermentAccountNumberInfo, "Guarantee", "The number issued by customs for post-release duty payment type of declaration.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(decl.JE_OtherBankAccountInfo, "Bank Account", "The bank account for direct debit. When payment method is set to \"2\", this field must be entered.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(decl.JE_SplitMarkInfo, "Split Shipment", "Tick the box for split shipment declaration.");
			});
		}

		public void TestDonotDefaultConsignorPickupDeliveryAddress()
		{
			var supplierOrg = Factory.New<OrgHeader>();
			var consignorPickupOrg = Factory.New<OrgHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			declaration.JE_OH_Supplier = supplierOrg.PK;
			shipment.ConsignorPickupAddress.OrganisationPK = consignorPickupOrg.PK;
			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals(consignorPickupOrg.PK, shipment.ConsignorPickupAddress.OrganisationPK);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplierOrg.PK;
			declaration.SupplierPickupAddress.OrganisationPK = consignorPickupOrg.PK;
			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals(ZGuid.Empty, declaration.SupplierPickupAddress.OrganisationPK);
		}

		public void TestItineraries()
		{
			var decl = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				AssertType<ItineraryDataCollection>("Collection type should be ItineraryDataCollection", decl.Itineraries);
				Assert("IsLoaded is true", decl.Itineraries.IsLoaded);
			});
		}

		public void TestHasDaysOfDelayedDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DateAtFinalDestination = ZDateTime.Empty;
			AssertEquals(false, declaration.HasDaysOfDelayedDeclaration);

			declaration.JE_DateAtFinalDestination = ZDateTime.Now;
			AssertEquals(true, declaration.HasDaysOfDelayedDeclaration);
		}

		public override void TestSupportsCusPackingList()
		{
			using (TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals(false, declaration.SupportsCusPackingList);
			}

			using (TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals(true, declaration.SupportsCusPackingList);
			}
		}

		public override void TestEntryStatusChangedLogged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Factory.Save();

			declaration.JE_EntryStatus = Events.CustomsCommenced.Code;
			Factory.Save();

			ZQuery filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
			filter.AddToFilter(StmALogSchema.SL_Parent, declaration.PK);
			AssertEquals("Event Created", 0, Factory.GetDatabaseCount(typeof(StmALog), filter));
		}

		public void TestImporterName()
		{
			var testOrg = Factory.New<OrgHeader>();
			var testAddress = testOrg.MainAddress;
			testAddress.OA_Address1 = "Address 1";
			testOrg.OH_Code = "TESTORG1";
			testOrg.OH_FullName = "Test Name";
			Factory.Save();
			var testDecl = Factory.New<JobDeclaration>();
			testDecl.JE_OH_Importer = testOrg.PK;
			AssertEquals("Test Name", testDecl.ImporterName);
			testAddress.OA_CompanyNameOverride = "override company";
			AssertEquals("override company", testDecl.ImporterName);
			var importerDocumentaryAddress = testDecl.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.E2_CompanyName = "E2 Company Name(IMD)";
			AssertEquals("E2 Company Name(IMD)", testDecl.ImporterName);
		}

		public void TestImporterChineseName()
		{
			var testOrg = Factory.New<OrgHeader>();
			var mainAddress = testOrg.MainAddress;
			testOrg.OH_Code = "TESTORG1";
			testOrg.OH_FullName = "Test Name";
			mainAddress.OA_CompanyNameOverride = "HAPPY CO., LTD.";
			mainAddress.OA_Language = Core.SharedConstants.Languages.English;
			var zhTWtranslatedAddress1 = mainAddress.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			zhTWtranslatedAddress1.OTA_CompanyName = "綠晃科技股份有限公司";

			var testDecl = Factory.New<JobDeclaration>();
			testDecl.JE_OH_Importer = testOrg.PK;
			AssertEquals("綠晃科技股份有限公司", testDecl.ImporterChineseName);
		}

		public void TestSupplierName()
		{
			var testOrg = Factory.New<OrgHeader>();
			var testAddress = testOrg.MainAddress;
			testAddress.OA_Address1 = "Address 1";
			testOrg.OH_Code = "TESTORG1";
			testOrg.OH_FullName = "Test Name";
			Factory.Save();
			var testDecl = Factory.New<JobDeclaration>();
			testDecl.JE_OH_Supplier = testOrg.PK;
			AssertEquals("Test Name", testDecl.SupplierName);
			testAddress.OA_CompanyNameOverride = "override company";
			AssertEquals("override company", testDecl.SupplierName);
			var supplierDocumentaryAddress = testDecl.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_CompanyName = "E2 Company Name(SUD)";
			AssertEquals("E2 Company Name(SUD)", testDecl.SupplierName);
		}

		public void TestSupplierChineseName()
		{
			var testOrg = Factory.New<OrgHeader>();
			var mainAddress = testOrg.MainAddress;
			testOrg.OH_Code = "TESTORG1";
			testOrg.OH_FullName = "Test Name";
			mainAddress.OA_CompanyNameOverride = "HAPPY CO., LTD.";
			mainAddress.OA_Language = Core.SharedConstants.Languages.English;
			var zhTWtranslatedAddress1 = mainAddress.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			zhTWtranslatedAddress1.OTA_CompanyName = "綠晃科技股份有限公司";

			var testDecl = Factory.New<JobDeclaration>();
			testDecl.JE_OH_Supplier = testOrg.PK;
			AssertEquals("綠晃科技股份有限公司", testDecl.SupplierChineseName);
		}

		public void TestIsPackingInformationRelevant_PopulateValueFromRegistry()
		{
			using (TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				Assert("Should be false", !declaration.IsPackingInformationRelevant);
			}

			using (TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				Assert("Should be true", declaration.IsPackingInformationRelevant);
			}
		}

		[TestDate(2021, 3, 18)]
		public void TestEntryNumberShouldBeRemovedWhenFactorySaveFailed_AllocateEntryNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclarationForTesting>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_BoxNumber = "123";
			Factory.Save();
			declaration.AllocateEntryNumber("");
			Factory.Save();
			AssertEquals("BB  1012300001", declaration.EntryNumber);
			declaration.AllocateEntryNumber("");
			declaration.ShouldThrowException = true;
			try
			{
				Factory.Save();
			}
			catch (ApplicationException)
			{
			}

			AssertEquals(ZString.Empty, declaration.EntryNumber);
		}

		[TestDate(2021, 3, 18)]
		public void TestEntryNumberShouldBeRemovedWhenFactorySaveFailed_AllocateEntryNumberToEntry()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclarationForTesting>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_BoxNumber = "123";
			Factory.Save();
			declaration.AllocateEntryNumber("");
			Factory.Save();
			AssertEquals("BB  1012300001", declaration.EntryNumber);
			entryHeader.EntryNumber = ZString.Empty;
			declaration.AllocateEntryNumberToEntry();
			declaration.ShouldThrowException = true;
			try
			{
				Factory.Save();
			}
			catch (ApplicationException)
			{
			}

			AssertEquals(ZString.Empty, declaration.EntryNumber);
			AssertEquals(ZString.Empty, entryHeader.EntryNumber);
		}

		public void TestClearanceStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "CABF0945600030";
			entryHeader.CusEntryNumber.CE_EntryStatus = "C1";
			Factory.Save();
			AssertEquals("C1", declaration.ClearanceStatus);
		}

		protected override void DoTestLogCustomsCommencedIfNeeded(Event customsCommencedEvent, BaseJobDeclaration testDec)
		{
			var saveCurrentUserIsSystemAccount = GlbStaff.CurrentUser.GS_IsSystemAccount;
			var saveCurrentUserCode = GlbStaff.CurrentUser.GS_Code;
			try
			{
				GlbStaff.CurrentUser.GS_IsSystemAccount = false;
				GlbStaff.CurrentUser.GS_Code = "XZX";
				testDec.LogCustomsCommencedIfNeeded();
				var secondCommencedLog = testDec.Logs.MostRecentLogByEventTime(customsCommencedEvent, GetBranchQuery());
				AssertEquals("TestDec.JE_GS_NKCusAgent", "XZX", testDec.JE_GS_NKCusAgent);
				AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, secondCommencedLog.SL_Reference.Left(2));
				GlbStaff.CurrentUser.GS_Code = "ZXZ";
				testDec.CancelCustomsEvents();
				testDec.LogCustomsCommencedIfNeeded();
				var thirdCommencedLog = testDec.Logs.MostRecentLogByEventTime(customsCommencedEvent, GetBranchQuery());
				AssertEquals("The Broker Staff should not be re-default as Login User after sending Customs message", "XZX", testDec.JE_GS_NKCusAgent);
				AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, thirdCommencedLog.SL_Reference.Left(2));
				using (thirdCommencedLog.LockForUpdatingKeyFieldsForTesting())
				{
					thirdCommencedLog.SL_IsEstimate = true;
				}

				testDec.LogCustomsCommencedIfNeeded();
				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, customsCommencedEvent.Code);
				query.AddToFilter(StmALogSchema.SL_Parent, testDec.PK);
				AssertEquals("A new log should have been added", 3, Factory.Load<StmALog>(query).Length);
				testDec.JE_GS_NKCusAgent = "";
				testDec.LogCustomsCommencedIfNeeded();
				AssertEquals("Under no situations should the current user be defaulted as the job's broker staff, not even when the job's broker staff is empty.", ZString.Empty, testDec.JE_GS_NKCusAgent);
				GenRegCertAccredMaintList brokerLicence = GlbStaff.CurrentUser.Certificates.AddNew();
				brokerLicence.XZ_RefNumber = "54321";
				brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;
				testDec.LogCustomsCommencedIfNeeded();
				var commencedLog = testDec.Logs.MostRecentLogByEventTime(customsCommencedEvent, GetBranchQuery());
				AssertNotNull(commencedLog);
				AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, commencedLog.SL_Reference.Left(2));
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsSystemAccount = saveCurrentUserIsSystemAccount;
				GlbStaff.CurrentUser.GS_Code = saveCurrentUserCode;
			}
		}

		public void TestSetInvoiceHeadersIncoTermPlace()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			CombineAssertions("JE_RL_NKOrigin", () =>
			{
				invoice.JZ_IncoTermPlace = ZString.Empty;
				declaration.JE_RL_NKOrigin = "CNSHA";
				AssertEquals("China", invoice.JZ_IncoTermPlace);
				invoice.JZ_IncoTermPlace = ZString.Empty;
				declaration.JE_RL_NKOrigin = "TWKEL";
				AssertEquals("Taiwan", invoice.JZ_IncoTermPlace);
				invoice.JZ_IncoTermPlace = ZString.Empty;
				declaration.JE_RL_NKOrigin = "TWZ99";
				AssertEquals("Taiwan", invoice.JZ_IncoTermPlace);
			}

			);
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			CombineAssertions("JE_RL_NKFinalDestination", () =>
			{
				invoice.JZ_IncoTermPlace = ZString.Empty;
				declaration.JE_RL_NKFinalDestination = "CNSHA";
				AssertEquals("China Shanghai Hongqiao Internation", invoice.JZ_IncoTermPlace);
				invoice.JZ_IncoTermPlace = ZString.Empty;
				declaration.JE_RL_NKFinalDestination = "TWKEL";
				AssertEquals("Taiwan Keelung (Chilung)", invoice.JZ_IncoTermPlace);
				invoice.JZ_IncoTermPlace = ZString.Empty;
				declaration.JE_RL_NKFinalDestination = "TWZ99";
				AssertEquals("Taiwan", invoice.JZ_IncoTermPlace);
			}

			);
			CombineAssertions("Z99FinalDestination", () =>
			{
				invoice.JZ_IncoTermPlace = ZString.Empty;
				declaration.JE_RL_NKFinalDestination = "TWZ99";
				declaration.JE_Z99FinalDestination = "3/11/2020";
				AssertEquals("Taiwan 3/11/2020", invoice.JZ_IncoTermPlace);
				invoice.JZ_IncoTermPlace = ZString.Empty;
				declaration.JE_RL_NKFinalDestination = "CNZ99";
				declaration.JE_Z99FinalDestination = "3/12/2020";
				AssertEquals("China 3/12/2020", invoice.JZ_IncoTermPlace);
				declaration.JE_Z99FinalDestination = "23/11/2020";
				AssertEquals("China 23/11/2020", invoice.JZ_IncoTermPlace);
			}

			);
		}

		public void TestSupplierDocAddressRequirementLinkedRequirement()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_CompanyName = "AA";
			var supplierPickupAddress = declaration.SupplierPickupAddress;
			Assert(supplierPickupAddress.E2_AddressOverride);
			AssertEquals("AA", supplierPickupAddress.E2_CompanyName);
			supplierDocumentaryAddress.E2_CompanyName = "";
			AssertEquals("", supplierPickupAddress.E2_CompanyName);
			AssertNoErrors(supplierPickupAddress.E2_CompanyNameInfo);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertNoErrors(supplierPickupAddress.E2_CompanyNameInfo);
			AssertEquals("", supplierPickupAddress.E2_CompanyName);
			supplierPickupAddress.E2_AddressOverride = false;
			supplierDocumentaryAddress.E2_CompanyName = "AA";
			AssertEquals("", supplierPickupAddress.E2_CompanyName);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			supplierDocumentaryAddress.E2_AddressOverride = false;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_CompanyName = "AA";
			AssertEquals("AA", supplierPickupAddress.E2_CompanyName);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			supplierDocumentaryAddress.E2_AddressOverride = false;
			Factory.Save();
			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			supplierPickupAddress = declaration.SupplierPickupAddress;
			AssertEquals("", supplierPickupAddress.E2_CompanyName);
			supplierDocumentaryAddress.E2_CompanyName = "CC";
			AssertEquals("", supplierPickupAddress.E2_CompanyName);
			supplierDocumentaryAddress.E2_CompanyName = "BB";
			AssertEquals("", supplierPickupAddress.E2_CompanyName);
		}

		public void TestSupplierDocAddressRequirementType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			CombineAssertions(() =>
			{
				AssertType<SupplierAddressRequirement>(supplierDocumentaryAddress.Requirement);
				AssertType<SupplierLocalAddressRequirement>(supplierDocumentaryAddress.LocalAddress.Requirement);
				AssertType<SupplierPicDlvAddressRequirement>(declaration.SupplierPickupAddress.Requirement);
			});
		}

		public void TestSupplierDocumentaryAddressChanged()
		{
			var header1 = Factory.New<OrgHeader>();
			var header2 = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			AssertEquals(ZGuid.Empty, declaration.JE_OH_Supplier);
			supplierDocumentaryAddress.OrganisationPK = header1.PK;
			AssertEquals(header1.PK, declaration.JE_OH_Supplier);
			supplierDocumentaryAddress.OrganisationPK = header2.PK;
			AssertEquals(header2.PK, declaration.JE_OH_Supplier);
			supplierDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals(ZGuid.Empty, declaration.JE_OH_Supplier);
		}

		public void TestFlushSupplierDocumentaryAddressIfBlank()
		{
			var org = Factory.New<OrgHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			declaration.JE_OH_Supplier = org.PK;
			AssertNoExceptionThrown(() => declaration.SupplierDocumentaryAddress.E2_AddressOverride = true);
		}

		public void TestImporterDocAddressRequirementType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = true;
			CombineAssertions(() =>
			{
				AssertType<ImporterAddressRequirement>(importerDocumentaryAddress.Requirement);
				AssertType<ImporterLocalAddressRequirement>(importerDocumentaryAddress.LocalAddress.Requirement);
				AssertType<ImporterPicDlvAddressRequirement>(declaration.ImporterDeliveryAddress.Requirement);
			});
		}

		public void TestImporterDocumentaryAddressChanged()
		{
			var header1 = Factory.New<OrgHeader>();
			var header2 = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			AssertEquals(ZGuid.Empty, declaration.JE_OH_Importer);
			importerDocumentaryAddress.OrganisationPK = header1.PK;
			AssertEquals(header1.PK, declaration.JE_OH_Importer);
			importerDocumentaryAddress.OrganisationPK = header2.PK;
			AssertEquals(header2.PK, declaration.JE_OH_Importer);
			importerDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals(ZGuid.Empty, declaration.JE_OH_Importer);
		}

		public void TestFlushImporterDocumentaryAddressIfBlank()
		{
			var org = Factory.New<OrgHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			declaration.JE_OH_Importer = org.PK;
			AssertNoExceptionThrown(() => declaration.ImporterDocumentaryAddress.E2_AddressOverride = true);
		}

		public void TestJobDocAddressValidationWhenImporterChanged()
		{
			var newFactory = new BusinessObjectFactory();
			var org1 = newFactory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "X3";
			org1.OH_RL_NKClosestPort = "SGSIN";
			var address1 = org1.MainAddress;
			address1.OA_RN_NKCountryCode = "SG";
			var aeoNumber1 = org1.CustomsCodes.AddNew("AEO", "1111111111111111", "SG");
			var org2 = newFactory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "X4";
			org2.OH_RL_NKClosestPort = "CNSHA";
			var address2 = org2.MainAddress;
			address2.OA_RN_NKCountryCode = "CN";
			var aeoNumber2 = org2.CustomsCodes.AddNew("AEO", "2222222222222222", "CN");
			var org3 = newFactory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "X5";
			org3.OH_RL_NKClosestPort = "CNSHA";
			var address3 = org3.MainAddress;
			address3.OA_RN_NKCountryCode = "CN";
			var aeoNumber3 = org3.CustomsCodes.AddNew("AEO", "888888888888888", "CN");
			newFactory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_OH_Importer = org1.PK;
			var warnningMessage = "Only the first 15 characters will be sent to the customs.";
			declaration.RunPreSaveValidation();
			AssertHasWarningContaining(declaration.ImporterDocumentaryAddress.AEOCodeInfo, warnningMessage);
			Factory.Save();
			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			declaration.JE_OH_Importer = org2.PK;
			declaration.RunPreSaveValidation();
			AssertHasWarningContaining(declaration.ImporterDocumentaryAddress.AEOCodeInfo, warnningMessage);
			declaration.JE_OH_Importer = org3.PK;
			declaration.RunPreSaveValidation();
			AssertNoWarningContaining(declaration.ImporterDocumentaryAddress.AEOCodeInfo, warnningMessage);
		}

		public void TestJobDocAddressValidationWhenSupplierChanged()
		{
			var newFactory = new BusinessObjectFactory();
			var org1 = newFactory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "X3";
			org1.OH_RL_NKClosestPort = "SGSIN";
			var address1 = org1.MainAddress;
			address1.OA_RN_NKCountryCode = "SG";
			var aeoNumber1 = org1.CustomsCodes.AddNew("AEO", "1111111111111111", "SG");
			var org2 = newFactory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "X4";
			org2.OH_RL_NKClosestPort = "CNSHA";
			var address2 = org2.MainAddress;
			address2.OA_RN_NKCountryCode = "CN";
			var aeoNumber2 = org2.CustomsCodes.AddNew("AEO", "2222222222222222", "CN");
			var org3 = newFactory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "X5";
			org3.OH_RL_NKClosestPort = "CNSHA";
			var address3 = org3.MainAddress;
			address3.OA_RN_NKCountryCode = "CN";
			var aeoNumber3 = org3.CustomsCodes.AddNew("AEO", "888888888888888", "CN");
			newFactory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = org1.PK;
			var warnningMessage = "Only the first 15 characters will be sent to the customs.";
			declaration.RunPreSaveValidation();
			AssertHasWarningContaining(declaration.SupplierDocumentaryAddress.AEOCodeInfo, warnningMessage);
			Factory.Save();
			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			declaration.JE_OH_Supplier = org2.PK;
			declaration.RunPreSaveValidation();
			AssertHasWarningContaining(declaration.SupplierDocumentaryAddress.AEOCodeInfo, warnningMessage);
			declaration.JE_OH_Supplier = org3.PK;
			declaration.RunPreSaveValidation();
			AssertNoWarningContaining(declaration.SupplierDocumentaryAddress.AEOCodeInfo, warnningMessage);
		}

		public void TestDeclarationDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var testDate = new ZDateTime(2020, 09, 15);
			entryInstruction.CEI_DateForDuty = testDate;
			AssertEquals(testDate, declaration.DeclarationDate);
		}

		public void TestDeclarationNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			AssertEquals(ZString.Empty, declaration.DeclarationNumber);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "CABB0999900001";
			AssertEquals("", declaration.DeclarationNumberDisplay);
			AssertEquals("CABB0999900001", declaration.DeclarationNumber);
			entry.CH_Status = "AWO";
			AssertEquals("CA/BB/09/999/00001", declaration.DeclarationNumberDisplay);
			AssertEquals("CABB0999900001", declaration.DeclarationNumber);
			declaration.EntryNumber = "CABB0999900026";
			AssertEquals("CA/BB/09/999/00026", declaration.DeclarationNumberDisplay);
			AssertEquals("CABB0999900026", declaration.DeclarationNumber);
		}

		public void TestDeclDocTypeCodeAndDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("", declaration.DeclDocTypeCodeAndDescription);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_DeclDocType = ExportDeclDocTypeList.Codes.CustomsProcessingRecords;
			AssertEquals("1-海關處理紀錄聯", declaration.DeclDocTypeCodeAndDescription);
			declaration.JE_DeclDocType = ExportDeclDocTypeList.Codes.TaxRefundOfMaterials;
			AssertEquals("3-沖退原料稅用聯", declaration.DeclDocTypeCodeAndDescription);
			declaration.JE_DeclDocType = ExportDeclDocTypeList.Codes.TaxRefundOfMainland;
			AssertEquals("4-退內地稅用聯", declaration.DeclDocTypeCodeAndDescription);
			declaration.JE_DeclDocType = ExportDeclDocTypeList.Codes.ExportCustoms;
			AssertEquals("5-出口證明用聯", declaration.DeclDocTypeCodeAndDescription);
			declaration.JE_DeclDocType = ExportDeclDocTypeList.Codes.Backup;
			AssertEquals("6-留底聯", declaration.DeclDocTypeCodeAndDescription);
			declaration.JE_DeclDocType = ExportDeclDocTypeList.Codes.Other;
			AssertEquals("7-其他聯", declaration.DeclDocTypeCodeAndDescription);
			declaration.JE_DeclDocType = "8";
			AssertEquals("8-", declaration.DeclDocTypeCodeAndDescription);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("", declaration.DeclDocTypeCodeAndDescription);
			AssertEquals("", declaration.JE_DeclDocType);
			declaration.JE_DeclDocType = ImportDeclDocTypeList.Codes.CustomsProcessingRecords;
			AssertEquals("1-海關處理紀錄聯", declaration.DeclDocTypeCodeAndDescription);
			declaration.JE_DeclDocType = ImportDeclDocTypeList.Codes.ImportCustoms;
			AssertEquals("2-進口證明用聯", declaration.DeclDocTypeCodeAndDescription);
			declaration.JE_DeclDocType = ImportDeclDocTypeList.Codes.TaxRefundOfMaterials;
			AssertEquals("3-沖退原料稅用聯", declaration.DeclDocTypeCodeAndDescription);
			declaration.JE_DeclDocType = ImportDeclDocTypeList.Codes.Backup;
			AssertEquals("4-留底聯", declaration.DeclDocTypeCodeAndDescription);
			declaration.JE_DeclDocType = ImportDeclDocTypeList.Codes.Other;
			AssertEquals("5-其他聯", declaration.DeclDocTypeCodeAndDescription);
			declaration.JE_DeclDocType = "8";
			AssertEquals("8-", declaration.DeclDocTypeCodeAndDescription);
		}

		[TestDate(2020, 08, 05)]
		public void TestCusEntryInstructionDefaultValues()
		{
			new TestTWCreator(Factory).CreateRegistryItemCusCustomsOffice();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("FAC", "Facilities");
			var facility = helper.CreateCusCodeList("TW", "FAC", "ANP0060D", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "Desc.", "FAC", "TW");
			facility.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "CE");
			Factory.Save();
			var registryTemplates = new CusGoodsLocationCollection();
			var registryTemplate = registryTemplates.AddNew();
			registryTemplate.MessageType = "EXP";
			registryTemplate.CustomsOffice = "CE";
			registryTemplate.GoodsLocation = "ANP0060D";
			TWCustomsDataRegistry.Instance.CusGoodsLocation.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryTemplates);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CusEntryInstruction;
			AssertEquals("CE", entryInstruction.CEI_CustomsOffice);
			AssertEquals("ANP0060D", entryInstruction.CEI_GoodsLocation);
			AssertEquals(new ZDateTime(2020, 08, 05), entryInstruction.CEI_DateForDuty);
		}

		[TestDate(2020, 07, 02)]
		public void TestAllocateEntryNumber()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "Buyer TW";
			orgHeader.OH_FullName = "Buyer TW";
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			orgCusCode.OK_OH = orgHeader.PK;
			orgCusCode.OK_CustomsRegNo = "1245";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			var company = GlbCompany.CurrentCompany;
			var extPassword1 = Factory.New<GlbExternalPassword>();
			extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword1.GP_GC = company.PK;
			extPassword1.GP_MailBoxID = "123-3";
			extPassword1.GP_UserID = "001";
			extPassword1.GP_GS = staff.PK;
			extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclarationForTesting>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "TT 1234567890";
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_BoxNumber = "123";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 08, 06);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = orgHeader.PK;
			AssertEquals("TT 1234567890", entryHeader.CusEntryNumber.CE_EntryNum);
			declaration.AllocateEntryNumber("BB  0912300005");
			AssertEquals("BB  0912300005", declaration.EntryNumber);
			AssertNull(entryHeader.CusEntryNumber);
			Factory.Save();
			var cusEntryNumber = CusEntryNumber.Load(Factory, declaration.EntryNumberType, declaration.EntryNumber, Core.Constants.CountryCodes.Taiwan).FirstOrDefault();
			AssertEquals(true, cusEntryNumber.CE_EntryIsSystemGenerated);
			declaration.AllocateEntryNumber("");
			AssertEquals("BB  0912300005", declaration.EntryNumber);
			Factory.Save();
			AssertEquals("BB  0912300001", declaration.EntryNumber);
			cusEntryNumber = CusEntryNumber.Load(Factory, declaration.EntryNumberType, declaration.EntryNumber, Core.Constants.CountryCodes.Taiwan).FirstOrDefault();
			AssertEquals(true, cusEntryNumber.CE_EntryIsSystemGenerated);
		}

		[TestDate(2020, 07, 02)]
		public void TestAllocateNextEntryNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclarationForTesting>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "TT 1234567890";
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_BoxNumber = "123";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 08, 06);
			declaration.Invoices.AddNew();
			AssertEquals("TT 1234567890", entryHeader.CusEntryNumber.CE_EntryNum);
			declaration.AllocateNextEntryNumberForTest();
			AssertNull(entryHeader.CusEntryNumber);
		}

		public void TestAllocateEntryNumberToEntry()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "Buyer TW";
			orgHeader.OH_FullName = "Buyer TW";
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			orgCusCode.OK_OH = orgHeader.PK;
			orgCusCode.OK_CustomsRegNo = "1245";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			var company = GlbCompany.CurrentCompany;
			var extPassword1 = Factory.New<GlbExternalPassword>();
			extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword1.GP_GC = company.PK;
			extPassword1.GP_MailBoxID = "123-3";
			extPassword1.GP_UserID = "001";
			extPassword1.GP_GS = staff.PK;
			extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_BoxNumber = "123";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 08, 06);
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = orgHeader.PK;
			declaration.EntryNumber = "BB  0912300005";
			Assert(declaration.AllocateEntryNumberToEntry());
			AssertEquals("BB  0912300005", declaration.EntryNumber);
			Factory.Save();
			AssertEquals("", declaration.EntryNumber);
			AssertEquals("BB  0912300005", entry.EntryNumber);
			entry.EntryNumber = "";
			Assert(declaration.AllocateEntryNumberToEntry());
			Factory.Save();
			AssertEquals("", declaration.EntryNumber);
			AssertEquals("BB  0912300001", entry.EntryNumber);
			Assert(!declaration.AllocateEntryNumberToEntry());
		}

		public void TestEntryHeader()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			AssertNull(declaration.EntryHeader);
			var cusHead1 = declaration.ActiveEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusHead1.CH_CEI_Instruction = entryInstruction.PK;
			AssertEquals(cusHead1, declaration.EntryHeader);
		}

		public void TestEntryNumberMutex()
		{
			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var declaration = factory1.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			factory1.Save();
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var decInFactory2 = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals(true, decInFactory2.LockEntryNumberAllocationMutex);
			AssertEquals(false, declaration.LockEntryNumberAllocationMutex);
			AssertEquals("Still locked", true, decInFactory2.LockEntryNumberAllocationMutex);
			AssertEquals("Still locked by another session", false, declaration.LockEntryNumberAllocationMutex);
			declaration.UnlockEntryNumberAllocationMutex();
			AssertEquals("Still locked", true, decInFactory2.LockEntryNumberAllocationMutex);
			decInFactory2.UnlockEntryNumberAllocationMutex();
			AssertEquals(true, declaration.LockEntryNumberAllocationMutex);
			AssertEquals("another session took lock", false, decInFactory2.LockEntryNumberAllocationMutex);
			using (var importEntryNumberAllocationMutex = new ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, "CUS" + declaration.PK.ToString()))
			{
				AssertEquals(true, importEntryNumberAllocationMutex.IsLocked);
				AssertEquals(false, importEntryNumberAllocationMutex.HasLock);
				factory1.Save();
				AssertEquals("Lock should be released when factory is saved", false, importEntryNumberAllocationMutex.IsLocked);
				AssertEquals(false, importEntryNumberAllocationMutex.HasLock);
				AssertEquals(true, declaration.LockEntryNumberAllocationMutex);
				AssertEquals(true, importEntryNumberAllocationMutex.IsLocked);
				AssertEquals(false, importEntryNumberAllocationMutex.HasLock);
				declaration.Delete();
				AssertEquals("Lock should be released when dec is deleted", false, importEntryNumberAllocationMutex.IsLocked);
				AssertEquals(false, importEntryNumberAllocationMutex.HasLock);
			}
		}

		public void TestEntryNumberAllocationMutexLockInfo_NullUser()
		{
			var declaration = Factory.New<JobDeclaration>();
			NeedUnlockAfterMergeForTestHelper.NeedUnlockAfterMergeForTest = false;
			using (var mutex = new ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, "CUS" + declaration.PK.ToString()))
			{
				Assert(mutex.Lock());
				var lockInfo = "Mutex:" + MutexIDs.CustomsTransactionIDAllocation.Name + ":CUS" + declaration.PK.ToString();
				var emptyGuid = Guid.Empty;
				var sql = $@"UPDATE TOP(1) StmServiceHeartBeat
									SET SV_ParentId = '{emptyGuid}',
										SV_SystemLastEditTimeUtc = GetUtcDate(),
										SV_SystemLastEditUser = 'USR'
									FROM dbo.StmServiceSemaphore
									INNER JOIN dbo.StmServiceHeartBeat ON SS_SV = SV_PK
									WHERE SS_LockInfo LIKE '%{lockInfo}%';";
				TestConnection.Command(sql).ExecuteNonQuery();
				AssertEquals(false, declaration.LockEntryNumberAllocationMutex);
				AssertNoExceptionThrown(() => declaration.GetEntryNumberAllocationMutexLockInfo());
			}

			NeedUnlockAfterMergeForTestHelper.NeedUnlockAfterMergeForTest = true;
		}

		public void TestJE_VesselArrivalReg_IsClearedWhenTransportModeChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = "SEA";
			declaration.JE_VesselArrivalReg = "123";
			declaration.JE_TransportMode = "AIR";
			AssertEquals("", declaration.JE_VesselArrivalReg);
			declaration.JE_VesselArrivalReg = "123";
			declaration.JE_TransportMode = "";
			AssertEquals("", declaration.JE_VesselArrivalReg);
		}

		public void TestShouldContainerLinkToOneInvoiceLineCore()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(false, dec.IsContainerInvoiceLinkRelevant);
		}

		public void TestShouldContainerHasPackagesCore()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(false, dec.IsContainerPackingRequired);
		}

		public override void TestAreMultipleEntryInstructionsAllowed()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(false, dec.AreMultipleEntryInstructionsAllowed);
		}

		public override void TestLocalCurrencyCoreOverride()
		{
			AssertEquals("Replace this with the correct currency code when implemented in a real country", Core.Constants.CurrencyCodes.Taiwan, GetJobDeclarationForTesting().LocalCurrencyCodeCoreExposed);
		}

		public void TestHouseBillsCollectionIsOfRightType()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			AssertEquals(typeof(BillCollection), declaration.Bills.GetType());
		}

		public void TestCustomsMessageType()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("ECD", declaration.CustomsMessageType);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("ICD", declaration.CustomsMessageType);
			declaration.JE_MessageType = ZString.Empty;
			AssertEquals("", declaration.CustomsMessageType);
		}

		public void TestLookupObjectIsCached()
		{
			var bizO = (JobDeclaration)GetNewBusinessObject();
			JobDeclarationLookups firstLookup = bizO.Lookups;
			JobDeclarationLookups secondLookup = bizO.Lookups;
			AssertEquals(secondLookup, firstLookup);
		}

		public void TestTransportCodeByTransportModeAndContainerMode()
		{
			var bizO = (JobDeclaration)GetNewBusinessObject();
			bizO.JE_TransportMode = TransportTypeList.Codes.Sea;
			CombineAssertions(() =>
			{
				AssertEquals(ContainerModeList.Codes.Containerized, bizO.JE_ContainerMode);
				bizO.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals(ContainerModeList.Codes.Loose, bizO.JE_ContainerMode);
				foreach (CodeDescriptionPair transportType in bizO.Lookups.TransportTypeList)
				{
					bizO.JE_TransportMode = transportType.Code;
					foreach (CodeDescriptionPair cargoId in bizO.Lookups.CargoIdTypeList)
					{
						bizO.JE_ContainerMode = cargoId.Code;
						var transportMode = bizO.JE_TransportMode;
						var containerMode = bizO.JE_ContainerMode;
						var expectCode = CommonHelper.GetTWTransportCode(transportMode, containerMode);
						AssertEquals($"JE_Calc_TWTransportCode should be {expectCode} when JE_TransportMode is {transportMode} and JE_ContainerMode is {containerMode}", expectCode, bizO.JE_Calc_TWTransportCode);
					}
				}
			});
		}

		public void TestClearJE_SLDOnFactorySaving()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_SLD = "X2";
			Factory.Save();
			AssertEquals("X2", declaration.JE_SLD);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Factory.Save();
			AssertEquals(ZString.Empty, declaration.JE_SLD);
		}

		public void TestTypeDecider()
		{
			Assert("Update dbo.JobDeclaration to include a decider for this class", Factory.New<JobDeclaration>().GetType() == GetExpectedBusinessObjectType());
		}

		public void TestJE_CustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ZL", "BABA THE BUILDER", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(codeList.PK, TransportTypeList.Codes.Sea);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_CustomsOffice = "ZL";
			AssertEquals("BABA THE BUILDER", declaration.Lookups.CustomsOfficeList.GetDescriptionFromCode(declaration.JE_CustomsOffice));
		}

		[TestDate(2017, 12, 26)]
		public void TestJE_LocationOfGoods()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("FAC", "Location of Goods");
			helper.CreateCusCodeList("TW", "FAC", "49", "49 DESC", new ZDateTime(2017, 12, 25), new ZDateTime(2017, 12, 30));
			helper.CreateCusCodeList("TW", "FAC", "6", "6 DESC", new ZDateTime(2017, 12, 25), new ZDateTime(2017, 12, 25));
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_LocationOfGoods = "49";
			AssertEquals("49 DESC", ((IFindBoxListProvider)declaration.Lookups.LocationOfGoodsCollection).DescriptionFromCode(declaration.JE_LocationOfGoods));
			declaration.JE_LocationOfGoods = "6";
			AssertNull(((IFindBoxListProvider)declaration.Lookups.LocationOfGoodsCollection).DescriptionFromCode(declaration.JE_LocationOfGoods));
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_LocationOfGoods = "49";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(declaration.JE_LocationOfGoods.IsEmpty);
			var declaration2 = Factory.New<JobDeclaration>();
			using (declaration2.SetterSuspender.SuspendSetting(JobDeclaration.Schema.JE_LocationOfGoods))
			{
				declaration2.JE_LocationOfGoods = "49";
				var instruction2 = declaration2.CusEntryInstruction;
				AssertEquals("", instruction2.CEI_GoodsLocation);
			}

			new TestTWCreator(Factory).CreateRegistryItemCusGoodsLocation();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_CustomsOffice = "CC";
			AssertEquals("ANP0060D", declaration.JE_LocationOfGoods);
			declaration.JE_CustomsOffice = "DD";
			AssertEquals("ANP0062D", declaration.JE_LocationOfGoods);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("", declaration.JE_LocationOfGoods);
			declaration.JE_CustomsOffice = "CC";
			AssertEquals("", declaration.JE_LocationOfGoods);
		}

		public void TestGovernmentUniformInvoices()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(GovernmentUniformInvoiceCollection), declaration.GovernmentUniformInvoices.GetType());
			var gui = declaration.GovernmentUniformInvoices.AddNew();
			gui.CY_Code = "XXXX1";
			gui.Amount = 123456;
			AssertEquals(declaration.PK, gui.CY_ParentID);
			AssertEquals(JobDeclarationSchema.Constants.Prefix, gui.CY_ParentTableCode);
			AssertEquals("XXXX1", gui.CY_Code);
			AssertEquals("123456", gui.CY_Data);
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			ICusCodeDataTypeSupporter supporter = declaration;
			supporter.AssertType(typeof(GovernmentUniformInvoiceData), CusCodeDataTypeList.Codes.GOVUniformInvoice);
			supporter.AssertType(null, "XXX");
			var governmentUniformInvoiceData = declaration.GovernmentUniformInvoices.AddNew();
			governmentUniformInvoiceData.CY_Code = "A11111";
			governmentUniformInvoiceData.Amount = 1234m;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<CusCodeData>(governmentUniformInvoiceData.PK);
			AssertEquals(typeof(GovernmentUniformInvoiceData), codeData.GetType());
		}

		public void TestJE_MessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			invoiceLine.JI_BondedGoodsCode = BondedGoodsCodeList.Codes.CN;
			AssertEquals(BondedGoodsCodeList.Codes.CN, invoiceLine.JI_BondedGoodsCode);
			invoiceLine.JI_Procedure = "90";
			AssertEquals("90", invoiceLine.JI_Procedure);
			entryInstruction.CEI_DaysOfDelayedDeclaration = 2;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(ZString.Empty, invoiceLine.JI_Procedure);
			AssertEquals(BondedGoodsCodeList.Codes.CN, invoiceLine.JI_BondedGoodsCode);
			AssertEquals(0, entryInstruction.CEI_DaysOfDelayedDeclaration);
			invoiceLine.JI_Procedure = "5E";
			AssertEquals("5E", invoiceLine.JI_Procedure);
			entryInstruction.CEI_GoodsLocation = "CNSHA";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(ZString.Empty, invoiceLine.JI_Procedure);
			AssertEquals(ZString.Empty, invoiceLine.JI_BondedGoodsCode);
			AssertEquals(ZString.Empty, entryInstruction.CEI_GoodsLocation);
			entryInstruction.CEI_GoodsLocation = "CNSHA";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("CNSHA", entryInstruction.CEI_GoodsLocation);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("", entryInstruction.CEI_GoodsLocation);
		}

		public override void TestNewOwner()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "X1$";
			org1.OH_FullName = org1.OH_Code;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "X2$";
			org2.OH_FullName = org2.OH_Code;
			var instruction1 = declaration.CusEntryInstruction;
			instruction1.CEI_Style = "G2";
			instruction1.CEI_Description = "A.C";
			instruction1.CEI_OH_Owner = org1.PK;
			AssertEquals(instruction1.Owner, declaration.NewOwner);
			instruction1.CEI_OH_Owner = ZGuid.Empty;
			AssertNull(declaration.NewOwner);
		}

		public void TestShouldResetSplitMarkToFalse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_SplitMark = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert("Shoule reset SplitMark to false", !declaration.JE_SplitMark);
			declaration.JE_SplitMark = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Assert("Shoule reset SplitMark to false", !declaration.JE_SplitMark);
		}

		public void TestGetBillTypeListCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_HouseBill = "HHH11";
			declaration.JE_MasterBill = "MMM11";
			var list = declaration.GetBillTypeList();
			AssertEquals(2, list.Count);
			Assert(list.ContainsCode(Customs.Business.BillTypeList.Codes.HouseBill));
			Assert(list.ContainsCode(Customs.Business.BillTypeList.Codes.MasterBill));
			declaration.JE_HouseBill = ZString.Empty;
			declaration.JE_MasterBill = ZString.Empty;
			declaration.Bills.RemoveAndDeleteAll();
			list = declaration.GetBillTypeList();
			AssertEquals(3, list.Count);
			Assert(list.ContainsCode(Customs.Business.BillTypeList.Codes.HouseBill));
			Assert(list.ContainsCode(Customs.Business.BillTypeList.Codes.MasterBill));
			Assert(list.ContainsCode(BillTypeList.Codes.ContainerNote));
			declaration.JE_HouseBill = "HHH11";
			declaration.JE_MasterBill = ZString.Empty;
			list = declaration.GetBillTypeList();
			AssertEquals(2, list.Count);
			Assert(list.ContainsCode(Customs.Business.BillTypeList.Codes.HouseBill));
			Assert(list.ContainsCode(Customs.Business.BillTypeList.Codes.MasterBill));
			declaration.JE_HouseBill = ZString.Empty;
			declaration.JE_MasterBill = "MMM11";
			list = declaration.GetBillTypeList();
			AssertEquals(2, list.Count);
			Assert(list.ContainsCode(Customs.Business.BillTypeList.Codes.HouseBill));
			Assert(list.ContainsCode(Customs.Business.BillTypeList.Codes.MasterBill));
		}

		public void TestJE_CustomsProfile()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "DEVELOPER COMPANY";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_OH_OrgProxy = org.PK;
			Factory.Save();
			GlbStaff.CurrentUser.GS_GB_HomeBranch = branch.PK;
			var newBroker = Factory.NewWithValidTestData<GlbStaff>();
			newBroker.GS_GB_HomeBranch = GlbStaff.CurrentUser.HomeBranch.PK;
			var anotherBroker = Factory.NewWithValidTestData<GlbStaff>();
			anotherBroker.GS_GB_HomeBranch = GlbStaff.CurrentUser.HomeBranch.PK;
			var brokerWithNoProfile = Factory.NewWithValidTestData<GlbStaff>();
			brokerWithNoProfile.GS_GB_HomeBranch = GlbStaff.CurrentUser.HomeBranch.PK;
			var brkCertificate = newBroker.Certificates.AddNew();
			brkCertificate.XZ_Type = Enterprise.Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			brkCertificate.XZ_RN_NKCountryOfIssuance = "TW";
			brkCertificate.XZ_RefNumber = "1234";
			Factory.Save();
			var currentCompanyPk = GlbCompany.CurrentCompany.PK;
			var extPswUvc = Factory.New<GlbExternalPassword>();
			extPswUvc.GP_GC = currentCompanyPk;
			extPswUvc.GP_PasswordType = PasswordTypesList.Codes.UVC;
			extPswUvc.GP_MailBoxID = "AAA-1";
			var extPswUva = Factory.New<GlbExternalPassword>();
			extPswUva.GP_GC = currentCompanyPk;
			extPswUva.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPswUva.GP_GS = newBroker.PK;
			extPswUva.GP_MailBoxID = "BBB-2";
			var extPswZzz = Factory.New<GlbExternalPassword>();
			extPswZzz.GP_GC = currentCompanyPk;
			extPswZzz.GP_GS = anotherBroker.PK;
			extPswZzz.GP_PasswordType = PasswordTypesList.Codes.UVC;
			extPswZzz.GP_MailBoxID = "CCC-3";
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("JE_CustomsProfile should be readonly", true, declaration.JE_CustomsProfileInfo.ReadOnly);
			declaration.JE_GS_NKCusAgent = newBroker.GS_Code;
			AssertEquals("JE_CustomsProfile should not be readonly", false, declaration.JE_CustomsProfileInfo.ReadOnly);
			AssertEquals("JE_CustomsProfile should be first External Password", "BBB-2", declaration.JE_CustomsProfile);
			declaration.JE_GS_NKCusAgent = ZString.Empty;
			AssertEquals("JE_CustomsProfile should be empty when JE_GS_NKCusAgent is empty", ZString.Empty, declaration.JE_CustomsProfile);
			declaration.JE_GS_NKCusAgent = brokerWithNoProfile.GS_Code;
			AssertEquals("E_CustomsProfile should be empty when selected broker doesn't have a profile setup", ZString.Empty, declaration.JE_CustomsProfile);
		}

		public void TestEntryNumberComponetsReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var heard = declaration.CustomsEntryHeaders.AddNew();
			heard.CH_CEI_Instruction = entryInstruction.PK;
			heard.CH_EntryStatus = EntryStatusCodeList.Codes.RFM;
			CombineAssertions("RFM", () =>
			{
				Assert("IsWaitingForResponseOrHasBeenLodgedAtCustoms", heard.IsWaitingForResponseOrHasBeenLodgedAtCustoms);
				Assert("JE_MessageTypeInfo ReadOnly", declaration.JE_MessageTypeInfo.ReadOnly);
				Assert("JE_CustomsOfficeInfo ReadOnly", !declaration.JE_CustomsOfficeInfo.ReadOnly);
			});

			heard.CH_EntryStatus = EntryStatusCodeList.Codes.ARM;
			var disposition = heard.CusDispositions.AddNew();
			disposition.CDI_StatusKey = EntryStatusCodeList.Codes.ARM;
			disposition.CDI_Type = Common.CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			disposition.CDI_Status = "A01";
			CombineAssertions("ARM", () =>
			{
				Assert("IsWaitingForResponseOrHasBeenLodgedAtCustoms", !heard.IsWaitingForResponseOrHasBeenLodgedAtCustoms);
				Assert("JE_MessageTypeInfo ReadOnly", !declaration.JE_MessageTypeInfo.ReadOnly);
				Assert("JE_CustomsOfficeInfo ReadOnly", !declaration.JE_CustomsOfficeInfo.ReadOnly);
			});
		}

		public void TestJE_CustomsProfileInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(35, declaration.JE_CustomsProfileInfo.MaxLength);
		}

		public void TestJE_GS_NKCusAgent()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "DEVELOPER COMPANY";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_OH_OrgProxy = org.PK;
			Factory.Save();
			GlbStaff.CurrentUser.GS_GB_HomeBranch = branch.PK;
			var newBroker = Factory.NewWithValidTestData<GlbStaff>();
			newBroker.GS_GB_HomeBranch = GlbStaff.CurrentUser.HomeBranch.PK;
			var brkCertificate = newBroker.Certificates.AddNew();
			brkCertificate.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			brkCertificate.XZ_RN_NKCountryOfIssuance = "TW";
			brkCertificate.XZ_RefNumber = "1234";
			brkCertificate.XZ_ExpiryOrDueDate = DateTime.Today.AddYears(1);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = newBroker.GS_Code;
			AssertNotNull(declaration.CusAgent);
			AssertEquals("1234", declaration.CusAgentCertificateNumber);
		}

		public void TestPackages()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(DeclarationLevelPackageCollection), declaration.Packages.GetType());
		}

		public void TestSupportsChcPivotBetweenInvoiceLineAndPackingCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("SupportsChcPivotBetweenInvoiceLineAndPacking is True", declaration.SupportsChcPivotBetweenInvoiceLineAndPacking);
		}

		public void TestDefaultValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("JE_PaymentMethod should be empty", ZString.Empty, declaration.JE_PaymentMethod);
			AssertEquals("JE_MergeBy should be ", OrgConstants.MergeInvoiceLines.NotMerge, declaration.JE_MergeBy);
		}

		public void TestMergeManagerType()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType(typeof(MergeManager), declaration.MergeManager);
		}

		public void TestGetCredential()
		{
			var company = GlbCompany.CurrentCompany;
			var newFactory = new BusinessObjectFactory();
			var staff = newFactory.New<GlbStaff>();
			staff.GS_Code = "TT";
			var extPassword1 = newFactory.New<GlbExternalPassword>();
			extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword1.GP_GC = company.PK;
			extPassword1.GP_MailBoxID = "123-3";
			extPassword1.GP_UserID = "001";
			extPassword1.GP_GS = staff.PK;
			extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			var extPassword2 = newFactory.New<GlbExternalPassword>();
			extPassword2.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword2.GP_GC = company.PK;
			extPassword2.GP_GS = staff.PK;
			extPassword2.GP_MailBoxID = "456-3";
			extPassword2.GP_UserID = "002";
			extPassword2.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			var extPassword3 = newFactory.New<GlbExternalPassword>();
			extPassword3.GP_PasswordType = PasswordTypesList.Codes.UVC;
			extPassword3.GP_GC = company.PK;
			extPassword3.GP_GS = staff.PK;
			extPassword3.GP_MailBoxID = "789-3";
			extPassword3.GP_UserID = "003";
			extPassword3.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			var extPassword4 = newFactory.New<GlbExternalPassword>();
			extPassword4.GP_PasswordType = PasswordTypesList.Codes.UVC;
			extPassword4.GP_GC = company.PK;
			extPassword4.GP_GS = staff.PK;
			extPassword4.GP_MailBoxID = "101-3";
			extPassword4.GP_UserID = "004";
			extPassword4.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			var extPassword5 = newFactory.New<GlbExternalPassword>();
			extPassword5.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword5.GP_GC = company.PK;
			extPassword5.GP_MailBoxID = "102-3";
			extPassword5.GP_UserID = "005";
			extPassword5.GP_GS = staff.PK;
			extPassword5.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			newFactory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			var credential = declaration.GetCredential();
			AssertNotNull(credential);
			AssertEquals(extPassword1.PK, credential.PK);
			declaration.JE_CustomsProfile = "456-3";
			credential = declaration.GetCredential();
			AssertNotNull(credential);
			AssertEquals(extPassword2.PK, credential.PK);
			AssertEquals("INV", credential.GP_PasswordStatus);
			declaration.JE_CustomsProfile = "789-3";
			credential = declaration.GetCredential();
			AssertNotNull(credential);
			AssertEquals(extPassword3.PK, credential.PK);
			declaration.JE_CustomsProfile = "101-3";
			credential = declaration.GetCredential();
			AssertNotNull(credential);
			AssertEquals(extPassword4.PK, credential.PK);
			declaration.JE_CustomsProfile = "102-3";
			credential = declaration.GetCredential();
			AssertNotNull(credential);
			AssertEquals(extPassword5.PK, credential.PK);
			declaration.JE_CustomsProfile = ZString.Empty;
			AssertNull(declaration.GetCredential());
		}

		public override void TestBondedWarehouseEditable()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("BondedWarehouseEditable alway false", false, declaration.BondedWarehouseEditable);
		}

		protected override string DefaultMergeType => OrgConstants.MergeInvoiceLines.NotMerge;
		public void TestJE_OH_Supplier()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = org1.PK;
			var invoice = declaration.Invoices.AddNew();
			AssertEquals(declaration.JE_OH_Supplier, invoice.JZ_OH_Supplier);
			declaration.JE_OH_Supplier = org2.PK;
			AssertEquals(declaration.JE_OH_Supplier, invoice.JZ_OH_Supplier);
		}

		public void TestDefaultJE_DefermentAccountNumbeFromOrgImpAddInfo()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PBR, "I1234567", Core.Constants.CountryCodes.Taiwan);
			var declarant = Factory.New<OrgHeader>();
			declarant.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PBR, "D1234567", Core.Constants.CountryCodes.Taiwan);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var orgImpAddInfo = TWOrgImpAddInfo.Get(importer);
			CombineAssertions(() =>
			{
				orgImpAddInfo.ZO_TWDefaultIMPPaymentMethod = "2";
				declaration.JE_OH_Importer = importer.PK;
				AssertEquals("ZO_TWDefaultIMPPaymentMethod is 2", "D1234567", declaration.JE_DefermentAccountNumber);

				declaration.JE_OH_Importer = ZGuid.Empty;
				declaration.JE_DefermentAccountNumberInfo.ClearValue();
				orgImpAddInfo.ZO_TWDefaultIMPPaymentMethod = "5";
				declaration.JE_OH_Importer = importer.PK;
				AssertEquals("ZO_TWDefaultIMPPaymentMethod is 5", "D1234567", declaration.JE_DefermentAccountNumber);

				declaration.JE_OH_Importer = ZGuid.Empty;
				declaration.JE_DefermentAccountNumberInfo.ClearValue();
				orgImpAddInfo.ZO_TWDefaultIMPPaymentMethod = "7";
				declaration.JE_OH_Importer = importer.PK;
				AssertEquals("ZO_TWDefaultIMPPaymentMethod is 7", "D1234567", declaration.JE_DefermentAccountNumber);

				declaration.JE_OH_Importer = ZGuid.Empty;
				declaration.JE_DefermentAccountNumberInfo.ClearValue();
				orgImpAddInfo.ZO_TWDefaultIMPPaymentMethod = "4";
				declaration.JE_OH_Importer = importer.PK;
				AssertEquals("ZO_TWDefaultIMPPaymentMethod is 4", "I1234567", declaration.JE_DefermentAccountNumber);

				declaration.JE_OH_Importer = ZGuid.Empty;
				declaration.JE_DefermentAccountNumberInfo.ClearValue();
				orgImpAddInfo.ZO_TWDefaultIMPPaymentMethod = "6";
				declaration.JE_OH_Importer = importer.PK;
				AssertEquals("ZO_TWDefaultIMPPaymentMethod is 6", "I1234567", declaration.JE_DefermentAccountNumber);

				declaration.JE_OH_Importer = ZGuid.Empty;
				declaration.JE_DefermentAccountNumberInfo.ClearValue();
				orgImpAddInfo.ZO_TWDefaultIMPPaymentMethod = "8";
				declaration.JE_OH_Importer = importer.PK;
				AssertEquals("ZO_TWDefaultIMPPaymentMethod is 8", "I1234567", declaration.JE_DefermentAccountNumber);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_OH_Importer = ZGuid.Empty;
				declaration.JE_DefermentAccountNumberInfo.ClearValue();

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Importer is null", "D1234567", declaration.JE_DefermentAccountNumber);
			});

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_DefermentAccountNumber = ZString.Empty;
			var supplier = Factory.New<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("Should be empty", ZString.Empty, declaration.JE_DefermentAccountNumber);
		}

		public void TestDefaultImportPaymentMethodFromOrgImpAddInfo()
		{
			var localOrganization = Factory.New<OrgHeader>();
			localOrganization.OH_RL_NKClosestPort = "TWTPE";
			var buyerLink = localOrganization.BuyerLinks.AddNew();
			buyerLink.OL_OH_Buyer = localOrganization.PK;
			var foreignOrganization = Factory.New<OrgHeader>();
			foreignOrganization.OH_RL_NKClosestPort = "CNSXG";
			buyerLink = foreignOrganization.BuyerLinks.AddNew();
			buyerLink.OL_OH_Buyer = localOrganization.PK;
			var noLinkOrganization = Factory.New<OrgHeader>();
			TWOrgImpAddInfo.Get(localOrganization).ZO_TWDefaultIMPPaymentMethod = "1";
			TWOrgImpAddInfo.Get(foreignOrganization).ZO_TWDefaultIMPPaymentMethod = "2";
			TWOrgImpAddInfo.Get(localOrganization).ZO_TWDefaultEXPPaymentMethod = "3";
			TWOrgImpAddInfo.Get(foreignOrganization).ZO_TWDefaultEXPPaymentMethod = "4";
			TWOrgImpAddInfo.Get(noLinkOrganization).ZO_TWDefaultIMPPaymentMethod = "5";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = ZString.Empty;
			declaration.JE_OH_Importer = localOrganization.PK;
			AssertEquals("1", declaration.JE_PaymentMethod);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = "6";
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Importer = localOrganization.PK;
			AssertEquals("6", declaration.JE_PaymentMethod);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = ZString.Empty;
			declaration.JE_OH_Importer = foreignOrganization.PK;
			Assert(declaration.IsExport);
			AssertEquals("3", declaration.JE_PaymentMethod);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = ZString.Empty;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Supplier = noLinkOrganization.PK;
			AssertNullOrEmpty(declaration.JE_PaymentMethod);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_PaymentMethod = ZString.Empty;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Supplier = foreignOrganization.PK;
			Assert(declaration.IsImport);
			AssertEquals(localOrganization.PK, declaration.JE_OH_Importer);
			AssertEquals("1", declaration.JE_PaymentMethod);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_PaymentMethod = ZString.Empty;
			declaration.JE_OH_Importer = noLinkOrganization.PK;
			AssertEquals("4", declaration.JE_PaymentMethod);
		}

		public void TestDefaultExportPaymentMethodFromOrgImpAddInfo()
		{
			var localOrganization = Factory.New<OrgHeader>();
			localOrganization.OH_RL_NKClosestPort = "TWTPE";
			var buyerLink = localOrganization.BuyerLinks.AddNew();
			buyerLink.OL_OH_Buyer = localOrganization.PK;
			var foreignOrganization = Factory.New<OrgHeader>();
			foreignOrganization.OH_RL_NKClosestPort = "CNSXG";
			var supplierLinks = foreignOrganization.SupplierLinks.AddNew();
			supplierLinks.OL_OH_Supplier = localOrganization.PK;
			var noLinkOrganization = Factory.New<OrgHeader>();
			TWOrgImpAddInfo.Get(localOrganization).ZO_TWDefaultEXPPaymentMethod = "1";
			TWOrgImpAddInfo.Get(foreignOrganization).ZO_TWDefaultEXPPaymentMethod = "2";
			TWOrgImpAddInfo.Get(localOrganization).ZO_TWDefaultIMPPaymentMethod = "3";
			TWOrgImpAddInfo.Get(foreignOrganization).ZO_TWDefaultIMPPaymentMethod = "4";
			TWOrgImpAddInfo.Get(noLinkOrganization).ZO_TWDefaultIMPPaymentMethod = "5";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_PaymentMethod = ZString.Empty;
			declaration.JE_OH_Supplier = localOrganization.PK;
			AssertEquals("1", declaration.JE_PaymentMethod);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_PaymentMethod = "6";
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Supplier = localOrganization.PK;
			AssertEquals("6", declaration.JE_PaymentMethod);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_PaymentMethod = ZString.Empty;
			declaration.JE_OH_Supplier = foreignOrganization.PK;
			Assert(declaration.IsImport);
			AssertEquals("3", declaration.JE_PaymentMethod);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_PaymentMethod = ZString.Empty;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Importer = noLinkOrganization.PK;
			AssertNullOrEmpty(declaration.JE_PaymentMethod);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = ZString.Empty;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Importer = foreignOrganization.PK;
			Assert(declaration.IsExport);
			AssertEquals(localOrganization.PK, declaration.JE_OH_Supplier);
			AssertEquals("1", declaration.JE_PaymentMethod);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = ZString.Empty;
			declaration.JE_OH_Supplier = noLinkOrganization.PK;
			AssertEquals("4", declaration.JE_PaymentMethod);
		}

		public void TestDeaultValueFromTWOrgImpAddInfoWhenJE_MessageTypeChanged()
		{
			var organization1 = Factory.New<OrgHeader>();
			var organization2 = Factory.New<OrgHeader>();
			TWOrgImpAddInfo.Get(organization1).ZO_TWDefaultEXPPaymentMethod = "1";
			TWOrgImpAddInfo.Get(organization2).ZO_TWDefaultIMPPaymentMethod = "2";
			TWOrgImpAddInfo.Get(organization1).ZO_TWDefaultExamMode = "3";
			TWOrgImpAddInfo.Get(organization2).ZO_TWDefaultExamMode = "4";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = organization1.PK;
			declaration.JE_OH_Importer = organization2.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_PaymentMethod = ZString.Empty;
			declaration.CusEntryInstruction.CEI_ExamMode = ZString.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("2", declaration.JE_PaymentMethod);
			AssertEquals("4", declaration.CusEntryInstruction.CEI_ExamMode);
			declaration.JE_PaymentMethod = ZString.Empty;
			declaration.CusEntryInstruction.CEI_ExamMode = ZString.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("1", declaration.JE_PaymentMethod);
			AssertEquals("3", declaration.CusEntryInstruction.CEI_ExamMode);
		}

		public void TestOverrideSupplierNotChangedWhenJE_MessageTypeChanged()
		{
			var testOrg = Factory.New<OrgHeader>();
			var testAddress = testOrg.MainAddress;
			testAddress.OA_Address1 = "Address 1";
			testOrg.OH_Code = "TESTORG1";
			testOrg.OH_FullName = "Test Name";
			Factory.Save();

			var testDecl = Factory.New<JobDeclaration>();
			testDecl.JE_MessageType = JobMessageTypeList.Codes.Export;
			testDecl.JE_OH_Supplier = testOrg.PK;

			var supplierDocumentaryAddress = testDecl.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_CompanyName = "E2 Company Name(SUD)";

			CombineAssertions(() =>
			{
				Assert("supplier overrided", supplierDocumentaryAddress.E2_AddressOverride);
				AssertEquals("E2 Company Name(SUD)", testDecl.SupplierName);

				testDecl.JE_MessageType = JobMessageTypeList.Codes.Import;
				Assert("supplier override after message type change", supplierDocumentaryAddress.E2_AddressOverride);
				AssertEquals("E2 Company Name(SUD)", testDecl.SupplierName);

				testDecl.JE_MessageType = JobMessageTypeList.Codes.Export;
				Assert("supplier override after message type change back", supplierDocumentaryAddress.E2_AddressOverride);
				AssertEquals("E2 Company Name(SUD)", testDecl.SupplierName);
			});
		}

		public void TestDefaultJE_ApplicationCode()
		{
			var customsInterface = new LocalCountryCustomsInterface { RecipientID = "RecipientID", SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Builtin };
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("JE_ApplicationCode", DeclarationApplicationCodeListForRegistry.Codes.Builtin, Factory.New<JobDeclaration>().JE_ApplicationCode);
			}

			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("JE_ApplicationCode", DeclarationApplicationCodeListForRegistry.Codes.Builtin, Factory.New<JobDeclaration>().JE_ApplicationCode);
			}

			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("JE_ApplicationCode", DeclarationApplicationCodeListForRegistry.Codes.Interfaced, Factory.New<JobDeclaration>().JE_ApplicationCode);
			}

			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("JE_ApplicationCode", DeclarationApplicationCodeListForRegistry.Codes.Interfaced, Factory.New<JobDeclaration>().JE_ApplicationCode);
			}
		}

		public void TestJE_ApplicationCode_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var customsInterface = new LocalCountryCustomsInterface { RecipientID = "RecipientID", SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted };
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				Factory.InvalidateCachedProperties();
				Assert("BothBuiltInDefaulted JE_ApplicationCode_ReadOnly", !declaration.JE_ApplicationCodeInfo.ReadOnly);
				declaration.CustomsEntryHeaders.AddNew();
				Assert("BothBuiltInDefaulted HasEntry JE_ApplicationCode_ReadOnly", declaration.JE_ApplicationCodeInfo.ReadOnly);
			}

			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Builtin;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				Factory.InvalidateCachedProperties();
				Assert("Builtin JE_ApplicationCode_ReadOnly", declaration.JE_ApplicationCodeInfo.ReadOnly);
			}

			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				Factory.InvalidateCachedProperties();
				Assert("BothInterfaceDefaulted JE_ApplicationCode_ReadOnly", !declaration.JE_ApplicationCodeInfo.ReadOnly);
				declaration.CustomsEntryHeaders.AddNew();
				Assert("BothInterfaceDefaulted HasEntry JE_ApplicationCode_ReadOnly", declaration.JE_ApplicationCodeInfo.ReadOnly);
			}

			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				Factory.InvalidateCachedProperties();
				Assert("Interfaced JE_ApplicationCode_ReadOnly", declaration.JE_ApplicationCodeInfo.ReadOnly);
			}
		}

		public void TestJI_PrimaryPreferenceWhenJE_MessageTypeChanged()
		{
			var universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var preferencePR1 = universalReferenceTestDataHelper.CreatePreferenceForCountry(Constants.PreferenceCodes.Preference1, "PR1", "TW");
			var preferenceSTD = universalReferenceTestDataHelper.CreatePreferenceForCountry(Constants.PreferenceCodes.Standard, "Standard", "TW");
			Factory.Save();
			var date1 = ZDateTime.MinSmallDateTimeValue;
			var date2 = ZDateTime.MaxSmallDateTimeValue;
			var tradeGroup = universalReferenceTestDataHelper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "TEST1", date1, date2);
			universalReferenceTestDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia, date1.Date, date2.Date);
			var hsnTariffType = universalReferenceTestDataHelper.CreateNewOrGetExistingTariffType("TW", "HSN");
			Factory.Save();
			var rateType = universalReferenceTestDataHelper.CreateNewOrGetExistingRateType("TW", Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = universalReferenceTestDataHelper.LoadOrCreateNewCusRateCode(Factory, "DTA", rateType.PK);
			Factory.Save();
			var cusTariff = universalReferenceTestDataHelper.CreateTariff("TW", hsnTariffType.PK, "123456789", date1, date2, "dummy Description 0");
			var testRate1 = universalReferenceTestDataHelper.CreateRate(cusTariff, rateCode1.PK, date1, date2, "0", preferencePk: preferenceSTD.PK);
			universalReferenceTestDataHelper.CreateCusApplicability(testRate1, tradeGroup, date1, date2, "add11", "ord11");
			var testRate2 = universalReferenceTestDataHelper.CreateRate(cusTariff, rateCode1.PK, date1, date2, "0", preferencePk: preferencePR1.PK);
			universalReferenceTestDataHelper.CreateCusApplicability(testRate2, tradeGroup, date1, date2, "add21", "ord21");
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "123456789";
			invoiceLine1.JI_CountryOfOrigin = "AU";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "123456789";
			invoiceLine2.JI_CountryOfOrigin = "AU";
			invoice = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "123456789";
			invoiceLine3.JI_CountryOfOrigin = "AU";
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "123456789";
			invoiceLine4.JI_CountryOfOrigin = "AU";
			AssertEquals(4, declaration.InvoiceLines.Cast<JobComInvoiceLine>().Count());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(4, declaration.InvoiceLines.Cast<JobComInvoiceLine>().Count(x => x.JI_PrimaryPreference == ZString.Empty));
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(4, declaration.InvoiceLines.Cast<JobComInvoiceLine>().Count(x => x.JI_PrimaryPreference == Constants.PreferenceCodes.Preference1));
			declaration.JE_MessageType = ZString.Empty;
			AssertEquals(4, declaration.InvoiceLines.Cast<JobComInvoiceLine>().Count(x => x.JI_PrimaryPreference == ZString.Empty));
		}

		public void TestCEI_StyleWhenJE_MessasgeTypeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("G1", entryInstruction.CEI_Style);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("G5", entryInstruction.CEI_Style);
		}

		public void TestSetLocationOfGoodsIfNeeded()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			var locationOfGoods640BG340 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "640BG340", "港龍航空有限公司台灣分公司保稅倉庫", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(locationOfGoods640BG340.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "B1");
			var locationOfGoods640B2140 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "640B2140", "高雄空廚股份有限公司保稅倉庫", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(locationOfGoods640B2140.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "B1");
			var locationOfGoods000AZZZZ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "000AZZZZ", "未經海關登記空貨櫃儲存處", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(locationOfGoods000AZZZZ.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "A1");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "B1";
			AssertEquals("Should be empty", ZString.Empty, declaration.JE_LocationOfGoods);
			declaration.JE_CustomsOffice = "A1";
			AssertEquals("Should be empty", "", declaration.JE_LocationOfGoods);
		}

		[TestDate(2019, 3, 1, 13, 13, 13)]
		public void TestDateOfValuation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var today = new ZDateTime(2019, 3, 1);
			AssertEquals("DateOfValuation is today when not EntryInstruction", today, declaration.DateOfValuation);
			var entryInst = declaration.CusEntryInstruction;
			entryInst.CEI_DateForDuty = ZDateTime.Empty;
			AssertEquals("DateOfValuation is today when CEI_DateForDuty of declaration.CustomsEntryInstructions is not Valid", today, declaration.DateOfValuation);
			entryInst.CEI_DateForDuty = new ZDateTime(2019, 4, 5);
			AssertEquals("Inv's EffectiveValuationDateCore is today when CEI_DateForDuty of declaration.CusEntryInstruction", new ZDateTime(2019, 4, 5), declaration.DateOfValuation);
		}

		public void TestShouldShowInvoiceLinesAreNotLinkedToTheContainerDialog()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("ShouldShowInvoiceLinesAreNotLinkedToTheContainerDialog should be", false, declaration.ShouldShowInvoiceLinesAreNotLinkedToTheContainerDialog);
			declaration.CusContainers.AddNew();
			var header = declaration.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			declaration.JE_ContainerMode = line.JI_ContainerMode = Core.Constants.ContainerModes.FCL;
			line.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = false;
			AssertEquals("ShouldShowInvoiceLinesAreNotLinkedToTheContainerDialog should be", false, declaration.ShouldShowInvoiceLinesAreNotLinkedToTheContainerDialog);
		}

		public void TestConcurrencyWhenFactorySave()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AIR";
			var mainAddress = org.MainAddress;
			mainAddress.OA_Language = Core.SharedConstants.Languages.English;
			mainAddress.OA_Address1 = "1500 HAPPY RD";
			mainAddress.OA_RN_NKCountryCode = "TW";
			mainAddress.OA_PostCode = "123";
			mainAddress.OA_City = "APPLE CITY";
			var orgCusCode = org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "VAT001", Core.Constants.CountryCodes.Taiwan);
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_OH_Supplier = org.PK;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			Factory.Save();
			AssertNotEquals(ZString.Empty, declaration.CusEntryInstruction.UCRNumber);
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var newDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			newFactory.Save();
			AssertNoExceptionThrown(() =>
			{
				Factory.Save();
			}

			);
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_OH_Supplier = org.PK;
			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			Factory.Save();
			AssertNotEquals(ZString.Empty, declaration.CusEntryInstruction.UCRNumber);
			newDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			newDeclaration.CusEntryInstruction.UCRNumber = "";
			newFactory.Save();
			AssertExceptionThrown<ZSaveConcurrencyException>(() =>
			{
				declaration.CusEntryInstruction.UCRNumber = "";
				Factory.Save();
			}

			);
		}

		public void TestJE_SLDCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_SLDInfo, JobDeclaration.ImportCaptionKey);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Manifest", captionResourceString.Caption);
				AssertEquals("FullDescription", "The manifest number.", captionResourceString.FullDescription);
			});

			captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_SLDInfo, JobDeclaration.ExportCaptionKey);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "SO No", captionResourceString.Caption);
				AssertEquals("FullDescription", "The shipping order number.", captionResourceString.FullDescription);
			});
		}

		public void TestZ99PortOfOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKOrigin = "TWTPE";
			Assert(!declaration.IsZ99PortOfOrigin);
			declaration.JE_RL_NKOrigin = "TWZ99";
			Assert(declaration.IsZ99PortOfOrigin);
			declaration.JE_Z99PortOfOrigin = "66";
			declaration.JE_RL_NKOrigin = "CNZ99";
			AssertEquals("", declaration.JE_Z99PortOfOrigin);
			declaration.JE_Z99PortOfOrigin = "高雄";
			AssertEquals("高雄", declaration.JE_Z99PortOfOrigin);
		}

		public void TestZ99FinalDestination()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKFinalDestination = "TWTPE";
			Assert(!declaration.IsZ99FinalDestination);
			declaration.JE_RL_NKFinalDestination = "TWZ99";
			Assert(declaration.IsZ99FinalDestination);
			declaration.JE_Z99PortOfOrigin = "66";
			declaration.JE_RL_NKFinalDestination = "CNZ99";
			AssertEquals("", declaration.JE_Z99FinalDestination);
			declaration.JE_Z99FinalDestination = "高雄";
			AssertEquals("高雄", declaration.JE_Z99FinalDestination);
		}

		public void TestResetValuesinInvoiceLinesForJE_MessageTypeChanged()
		{
			CombineAssertions("When non import then set TW_EPTDigit empty.", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
				invoiceLine.JI_EPTDigit1 = ContainerMaterialList.Codes.A;
				invoiceLine.JI_EPTDigit2 = ContainerCapacityList.Codes._1;
				invoiceLine.JI_EPTDigit3 = ContainerMaterialNumberList.Codes._1;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("A", invoiceLine.JI_EPTDigit1);
				AssertEquals("1", invoiceLine.JI_EPTDigit2);
				AssertEquals("1", invoiceLine.JI_EPTDigit3);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals(ZString.Empty, invoiceLine.JI_EPTDigit1);
				AssertEquals(ZString.Empty, invoiceLine.JI_EPTDigit2);
				AssertEquals(ZString.Empty, invoiceLine.JI_EPTDigit3);
			});

			CombineAssertions("When JE_MessageType changed then reset defalut RAP.", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
				invoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
				invoiceLine.JI_UseOneTenthCV = ZBool.True;
				invoiceLine.JI_InvoiceQuantity = 5m;
				invoiceLine.JI_RAPPrice = 20m;
				invoiceLine.JI_RAPCurr = "USD";
				AssertEquals(4m, invoiceLine.JI_Calc_RAPRORUnitPrice);
				AssertEquals("USD", invoiceLine.JI_Calc_RAPRORUnitCurr);
				AssertEquals(20m, invoiceLine.JI_RAPPrice);
				AssertEquals("USD", invoiceLine.JI_RAPCurr);
				AssertEquals(ZBool.True, invoiceLine.JI_UseOneTenthCV);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals(0m, invoiceLine.JI_Calc_RAPRORUnitPrice);
				AssertEquals(ZString.Empty, invoiceLine.JI_Calc_RAPRORUnitCurr);
				AssertEquals(0m, invoiceLine.JI_RAPPrice);
				AssertEquals(ZString.Empty, invoiceLine.JI_RAPCurr);
				AssertEquals(ZBool.False, invoiceLine.JI_UseOneTenthCV);
			}

			);
			CombineAssertions("Clear JobComInvoiceLineTax when Message type is Exp.", () =>
			{
				var universalTestHelper = new UniversalReferenceTestDataHelper(Factory);
				var tariffTypeHSN = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
				var tariffTypeSS = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "SS");
				Factory.Save();
				var tariff = universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "86044", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				universalTestHelper.CreateNewOrGetExistingTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "L*", tariff);
				var childTariff2 = universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeSS.PK, "AirplaneAndHelicopter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				universalTestHelper.CreateTariffRelationship(childTariff2.PK, tariffTypeHSN.PK, "86044");
				var childTariff3 = universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeSS.PK, "Yachts", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				universalTestHelper.CreateTariffRelationship(childTariff3.PK, tariffTypeHSN.PK, "86044");
				Factory.Save();
				var testDecl = Factory.New<JobDeclaration>();
				testDecl.JE_MessageType = JobMessageTypeList.Codes.Import;
				var testLine = testDecl.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
				testLine.JI_Tariff = "86044";
				AssertEquals(1, testLine.Taxes.Count);
				AssertEquals("SS", testLine.Taxes[0].JLT_Type);
				AssertEquals(ZString.Empty, testLine.Taxes[0].JLT_Tariff);
				testDecl.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals(0, testLine.Taxes.Count);
			}

			);
		}

		public void TestExchangeRateWhenShipmentTypeChanged()
		{
			var foreignCurrency = RefCurrency.New(Factory);
			foreignCurrency.RX_Code = "XYZ";
			var cusRate = foreignCurrency.ExchangeRates.AddNew();
			cusRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			cusRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			cusRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			cusRate.RE_SellRate = 0.8m;
			var cusSecRate = foreignCurrency.ExchangeRates.AddNew();
			cusSecRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;
			cusSecRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			cusSecRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			cusSecRate.RE_SellRate = 0.7m;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			AssertEquals(0.7m, invoice1.JZ_InvoiceCurrExRate);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(0.8m, invoice1.JZ_InvoiceCurrExRate);
		}

		public void TestReservedFields()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			AssertType<JobDeclarationReservedFieldCollection>(jobDeclaration.ReservedFields);
			var reservedField = jobDeclaration.ReservedFields.AddNew();
			AssertEquals(1, jobDeclaration.ReservedFields.Count);
			AssertType<JobDeclarationReservedField>(reservedField);
			var supporter = jobDeclaration as IReservedFieldSupporter;
			AssertNotNull(supporter);
			AssertEquals(1, supporter.GetReservedFields().Count());
		}

		public override void TestSetDefaultPackagesType()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("default value for JE_TotalNoOfPacksPackType", "CTN", declaration.JE_TotalNoOfPacksPackType);
		}

		public void TestJobDeclarationSynchroniser()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HBL1";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "KRBUS";
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.Shipments.Add(shipment);
			consol.JK_MasterBillNum = "M1";
			var forwardingContainer1 = consol.Containers.AddNew();
			forwardingContainer1.JC_ContainerNum = "C1";
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_JC = forwardingContainer1.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			AssertType<JobDeclarationSynchroniser>(declaration.ShipmentSynchroniser);
		}

		public override void TestSetSynchroniserFieldsReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert(!declaration.Packages.ReadOnly);
			declaration.SetSynchroniserFieldsReadOnly(true);
			Assert(!declaration.Packages.ReadOnly);
			declaration.SetSynchroniserFieldsReadOnly(false);
			Assert(!declaration.Packages.ReadOnly);
		}

		public void TestPortOfOriginCountry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKOrigin = "TWXXX";
			AssertEquals("TW", declaration.PortOfOriginCountry.RN_Code);
			AssertEquals("Taiwan", declaration.PortOfOriginCountry.RN_Desc);
			declaration.JE_RL_NKOrigin = "ADXXX";
			AssertEquals("AD", declaration.PortOfOriginCountry.RN_Code);
			AssertEquals("Andorra", declaration.PortOfOriginCountry.RN_Desc);
			declaration.JE_RL_NKOrigin = "USXXX";
			AssertEquals("US", declaration.PortOfOriginCountry.RN_Code);
			AssertEquals("United States", declaration.PortOfOriginCountry.RN_Desc);
		}

		public void TestFinalDestinationCountry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKFinalDestination = "TWXXX";
			AssertEquals("TW", declaration.FinalDestinationCountry.RN_Code);
			AssertEquals("Taiwan", declaration.FinalDestinationCountry.RN_Desc);
			declaration.JE_RL_NKFinalDestination = "ADXXX";
			AssertEquals("AD", declaration.FinalDestinationCountry.RN_Code);
			AssertEquals("Andorra", declaration.FinalDestinationCountry.RN_Desc);
			declaration.JE_RL_NKFinalDestination = "USXXX";
			AssertEquals("US", declaration.FinalDestinationCountry.RN_Code);
			AssertEquals("United States", declaration.FinalDestinationCountry.RN_Desc);
		}

		public void TestPortOfOriginName()
		{
			var uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_Code = "TWXXX";
			uNLOCO.RL_PortName = "TAIWANG";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKOrigin = "TWXXX";
			AssertEquals("TAIWANG", declaration.PortOfOriginName);
			declaration.JE_Z99PortOfOrigin = "台湾";
			AssertEquals("TAIWANG", declaration.PortOfOriginName);
			declaration.JE_RL_NKOrigin = "TWZ99";
			AssertEquals("", declaration.PortOfOriginName);
			declaration.JE_Z99PortOfOrigin = "台湾";
			AssertEquals("台湾", declaration.PortOfOriginName);
		}

		public void TestFinalDestinationName()
		{
			var uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_Code = "TWXXX";
			uNLOCO.RL_PortName = "TAIWANG";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKFinalDestination = "TWXXX";
			AssertEquals("TAIWANG", declaration.FinalDestinationName);
			declaration.JE_Z99FinalDestination = "台湾";
			AssertEquals("TAIWANG", declaration.FinalDestinationName);
			declaration.JE_RL_NKFinalDestination = "TWZ99";
			AssertEquals("", declaration.FinalDestinationName);
			declaration.JE_Z99FinalDestination = "台湾";
			AssertEquals("台湾", declaration.FinalDestinationName);
		}

		public void TestPortOfOriginProperName()
		{
			var uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_Code = "TWXXX";
			uNLOCO.RL_NameWithDiacriticals = "TAIWANG";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKOrigin = "TWXXX";
			AssertEquals("TAIWANG", declaration.PortOfOriginProperName);
			declaration.JE_Z99PortOfOrigin = "台湾";
			AssertEquals("TAIWANG", declaration.PortOfOriginProperName);
			declaration.JE_RL_NKOrigin = "TWZ99";
			AssertEquals("", declaration.PortOfOriginProperName);
			declaration.JE_Z99PortOfOrigin = "台湾";
			AssertEquals("台湾", declaration.PortOfOriginProperName);
		}

		public void TestFinalDestinationProperName()
		{
			var uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_Code = "TWXXX";
			uNLOCO.RL_PortName = "TAIWANG";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKFinalDestination = "TWXXX";
			AssertEquals("TAIWANG", declaration.FinalDestinationProperName);
			declaration.JE_Z99FinalDestination = "台湾";
			AssertEquals("TAIWANG", declaration.FinalDestinationProperName);
			declaration.JE_RL_NKFinalDestination = "TWZ99";
			AssertEquals("", declaration.FinalDestinationProperName);
			declaration.JE_Z99FinalDestination = "台湾";
			AssertEquals("台湾", declaration.FinalDestinationProperName);
		}

		public void TestTotalDeclarationPackingGrossWeightInKilograms()
		{
			var testDecl = Factory.New<JobDeclaration>();
			var testPackage1 = testDecl.Packages.AddNew();
			testPackage1.CW_GrossWeight = 1.4555m;
			testPackage1.CW_GrossWeightUQ = "KG";
			var testPackage2 = testDecl.Packages.AddNew();
			testPackage2.CW_GrossWeight = 2000m;
			testPackage2.CW_GrossWeightUQ = "G";
			AssertEquals(3.456m, testDecl.TotalDeclarationPackingGrossWeightInKilograms);
		}

		public void TestTotalDeclarationPackingNetWeightInKilograms()
		{
			var testDecl = Factory.New<JobDeclaration>();
			var testPackage1 = testDecl.Packages.AddNew();
			testPackage1.CW_NetWeight = 1m;
			testPackage1.CW_NetWeightUQ = "LB";
			var testPackage2 = testDecl.Packages.AddNew();
			testPackage2.CW_NetWeight = 2000m;
			testPackage2.CW_NetWeightUQ = "G";
			AssertEquals(2.454m, testDecl.TotalDeclarationPackingNetWeightInKilograms);
		}

		public void TestJE_MessageStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageStatus = "X1";
			AssertEquals("X1", declaration.JE_MessageStatus);
			var entryInstruction = declaration.CusEntryInstruction;
			var heard = declaration.CustomsEntryHeaders.AddNew();
			heard.CH_CEI_Instruction = entryInstruction.PK;
			heard.CH_Status = JobDeclarationMessageStatusList.Codes.AWG;
			AssertEquals("AWG", declaration.JE_MessageStatus);
			Assert(declaration.JE_MessageStatusInfo.ReadOnly);
		}

		public void TestJE_MessageStatusDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageStatus = "AWG";
			AssertEquals("\u7b49\u5f85\u56de\u61c9\u4e2d (\u9032\u53e3\u8ca8\u7269\u67e5\u9a57\u7533\u8acb\u66f8)", declaration.JE_MessageStatusDescription);
		}

		public void TestJE_EntryStatusDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_EntryStatus = ZString.Empty;
			AssertEquals(EntryStatusCodeList.Descriptions.NotReceive, declaration.JE_EntryStatusDescription);
		}

		public void TestJE_EntryStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert(declaration.JE_EntryStatusInfo.ReadOnly);
		}

		public void TestPackingLinesShouldOnlyBeCreatedAtTheHouseBillLevel()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(true, declaration.PackingLinesShouldOnlyBeCreatedAtTheHouseBillLevel);
			declaration.JE_MasterBill = "X1";
			var bill = declaration.Bills.OfType<Bill>().FirstOrDefault();
			AssertEquals("X1", bill.CU_MasterBill);
			AssertEquals(true, bill.IsMasterBill);
			AssertEquals(0, declaration.Packages.Count);
		}

		public void TestShouldDefaultPackingInfoFromDeclarationToBills()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(false, declaration.ShouldDefaultPackingInfoFromDeclarationToBills);
			declaration.JE_HouseBill = "X1";
			var bill = declaration.Bills.OfType<Bill>().FirstOrDefault();
			AssertEquals("X1", bill.CU_HouseBill);
			AssertEquals(true, bill.IsHouseBill);
			AssertEquals(0, declaration.Packages.Count);
			var newBill = declaration.Bills.AddNew();
			newBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			newBill.CU_HouseBill = "X2";
			AssertEquals(0, declaration.Packages.Count);
		}

		public void TestEntryNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.EntryNumber = "TT 1234567890";
			AssertEquals("TT 1234567890", entryHeader.CusEntryNumber.CE_EntryNum);
			declaration.EntryNumber = "AA 123456";
			AssertEquals("AA 123456", declaration.EntryNumber);
			AssertNull(entryHeader.CusEntryNumber);
			entryHeader.EntryNumber = "TT 1234567890";
			AssertEquals("TT 1234567890", entryHeader.CusEntryNumber.CE_EntryNum);
			declaration.EntryNumber = ZString.Empty;
			AssertNullOrEmpty(declaration.EntryNumber);
			AssertNull(entryHeader.CusEntryNumber);
			declaration.EntryNumber = "AA 123456";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var cusEntryNumber = CusEntryNumber.Load(newFactory, declaration.EntryNumberType, "AA 123456", Core.Constants.CountryCodes.Taiwan, true).FirstOrDefault();
			AssertNotNull(cusEntryNumber);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			Factory.Save();
			newFactory = new BusinessObjectFactory();
			cusEntryNumber = CusEntryNumber.Load(newFactory, "EXP", "AA 123456", Core.Constants.CountryCodes.Taiwan, true).FirstOrDefault();
			AssertNull(cusEntryNumber);
			cusEntryNumber = CusEntryNumber.Load(newFactory, "IMP", "AA 123456", Core.Constants.CountryCodes.Taiwan, true).FirstOrDefault();
			AssertNull(cusEntryNumber);
			declaration.EntryNumber = "AA 123456";
			Factory.Save();
			newFactory = new BusinessObjectFactory();
			cusEntryNumber = CusEntryNumber.Load(newFactory, declaration.EntryNumberType, "AA 123456", Core.Constants.CountryCodes.Taiwan, true).FirstOrDefault();
			AssertNotNull(cusEntryNumber);
			var entryNumberType = declaration.EntryNumberType;
			declaration.Delete();
			Factory.Save();
			newFactory = new BusinessObjectFactory();
			cusEntryNumber = CusEntryNumber.Load(newFactory, entryNumberType, "AA 123456", Core.Constants.CountryCodes.Taiwan, true).FirstOrDefault();
			AssertNull(cusEntryNumber);
		}

		public void TestSetDefaultValuesForDefaultBrokerStaff()
		{
			new TestTWCreator(Factory).CreateRegistryItemCusBrokerStaff();
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("CYO", declaration.JE_GS_NKCusAgent);
			AssertEquals("TBK0461-0", declaration.JE_CustomsProfile);
		}

		public void TestSetDefaultValuesForDeclarant()
		{
			var org = new TestTWCreator(Factory).CreateOrganization();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = org.PK;
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(org.MainAddress.PK, declaration.JE_OA_DeclarantAddress);
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			declaration = Factory.New<JobDeclaration>();
			AssertEquals(ZGuid.Empty, declaration.JE_OA_DeclarantAddress);
		}

		public void TestDocAddresses()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertType<TWJobDocAddressDependentCollection>(jobDeclaration.DocAddresses);
			var supplierDocumentaryAddress = jobDeclaration.SupplierDocumentaryAddress;
			AssertType<TWJobDocAddress>(supplierDocumentaryAddress);
			AssertEquals(DocAddressType.SupplierTranslatedDocumentaryAddress, supplierDocumentaryAddress.LocalAddressType);
			var importerrDocumentaryAddress = jobDeclaration.ImporterDocumentaryAddress;
			AssertType<TWJobDocAddress>(importerrDocumentaryAddress);
			AssertEquals(DocAddressType.ImporterTranslatedDocumentaryAddress, importerrDocumentaryAddress.LocalAddressType);
		}

		public void TestCountryContext()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceHolder = declaration as IApportionInvoiceHolder;
			AssertEquals("TWIMP", invoiceHolder.CountryContext);
			declaration.JE_MessageType = "EXP";
			AssertEquals("TW", invoiceHolder.CountryContext);
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = "FOB";
			AssertEquals("TW", invoiceHolder.CountryContext);
			invoiceHeader.JZ_IncoTerm = "EXW";
			AssertEquals("TWEXPEXW", invoiceHolder.CountryContext);
			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_IncoTerm = "EXW";
			AssertEquals("TWEXPEXW", invoiceHolder.CountryContext);
			invoiceHeader.JZ_IncoTerm = "FOB";
			AssertEquals("TWEXPEXW", invoiceHolder.CountryContext);
			invoiceHeader2.JZ_IncoTerm = "CIF";
			AssertEquals("TW", invoiceHolder.CountryContext);
			invoiceHeader.JZ_IncoTerm = "EXW";
			AssertEquals("TWEXPEXW", invoiceHolder.CountryContext);
		}

		public void TestLogEventWhenEntryNumberChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CusEntryInstruction.CEI_CustomsOffice = "AA";
			declaration.CusEntryInstruction.CEI_BoxNumber = "AAA";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryHeader.Declaration.CusEntryInstruction.PK;
			var filtered = declaration.Logs.Find(x => x.Event.SE_Code == Events.CustomsNumberEntered.Code);
			AssertEquals(0, filtered.Count());
			declaration.AllocateEntryNumber("E0001");
			Factory.Save();
			filtered = declaration.Logs.Find(x => x.Event.SE_Code == Events.CustomsNumberEntered.Code);
			AssertEquals(1, filtered.Count());
			AssertEquals(declaration.EntryNumber, filtered.First().SL_Reference);
			declaration.AllocateEntryNumber("");
			Factory.Save();
			filtered = declaration.Logs.Find(x => x.Event.SE_Code == Events.CustomsNumberEntered.Code);
			AssertEquals(2, filtered.Count());
			AssertEquals(declaration.EntryNumber, filtered.ElementAt(1).SL_Reference);
			declaration.CusEntryNumber.Delete();
			filtered = declaration.Logs.Find(x => x.Event.SE_Code == Events.CustomsNumberEntered.Code);
			AssertEquals(2, filtered.Count());
			declaration.AllocateEntryNumberToEntry();
			Factory.Save();
			filtered = entryHeader.Logs.Find(x => x.Event.SE_Code == Events.CustomsNumberEntered.Code);
			AssertEquals(1, filtered.Count());
			AssertEquals(entryHeader.EntryNumber, filtered.First().SL_Reference);
		}

		public void TestOrgConsignee()
		{
			var orgheader = Factory.NewWithValidTestData<OrgHeader>();
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobdeclaration.JE_OH_Consignee = orgheader.PK;
			AssertEquals(orgheader, jobdeclaration.OrgConsignee);
		}

		public void TestAnyFeeHasChangesSinceLastMark()
		{
			var declaration = CreatDeclarationForTestCalculateDuties();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];

			var entryLineFeesAfterReMerge = entryLine.Fees.Cast<CusEntryLineFee>();
			Assert(!declaration.DutyCalculationManagerAnyEntryLineHasChangesSinceLastMark);

			var fee = entryLineFeesAfterReMerge.FirstOrDefault(x => x.CF_ChargeType == "VAT");
			fee.TW_RateOverride = "ADD";
			Assert(declaration.DutyCalculationManagerAnyEntryLineHasChangesSinceLastMark);

			declaration.CalculateDuties();
			Assert(!declaration.DutyCalculationManagerAnyEntryLineHasChangesSinceLastMark);

			fee.TW_RateOverride = ZString.Empty;
			Assert(declaration.DutyCalculationManagerAnyEntryLineHasChangesSinceLastMark);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Assert(!declaration.DutyCalculationManagerAnyEntryLineHasChangesSinceLastMark);
		}

		public void TestDefaultJE_PaidBy()
		{
			var cliOrgheader = Factory.NewWithValidTestData<OrgHeader>();
			cliOrgheader.CompanyData.OB_CusPaidBy = PaidByCodeList.Codes.CLI;

			var brkOrgheader = Factory.NewWithValidTestData<OrgHeader>();
			brkOrgheader.CompanyData.OB_CusPaidBy = PaidByCodeList.Codes.BRK;

			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobdeclaration.JE_MessageType = "EXP";
			jobdeclaration.JE_PaidBy = ZString.Empty;

			jobdeclaration.JE_OH_Supplier = cliOrgheader.PK;
			AssertEquals("CLI", jobdeclaration.JE_PaidBy);

			jobdeclaration.JE_OH_Supplier = brkOrgheader.PK;
			AssertEquals("CLI", jobdeclaration.JE_PaidBy);

			jobdeclaration.JE_MessageType = "IMP";
			jobdeclaration.JE_OH_Importer = brkOrgheader.PK;
			AssertEquals("CLI", jobdeclaration.JE_PaidBy);

			jobdeclaration.JE_OH_Importer = ZGuid.Empty;
			jobdeclaration.JE_PaidBy = ZString.Empty;
			jobdeclaration.JE_OH_Importer = brkOrgheader.PK;
			AssertEquals("BRK", jobdeclaration.JE_PaidBy);

			jobdeclaration.JE_OH_Importer = cliOrgheader.PK;
			AssertEquals("BRK", jobdeclaration.JE_PaidBy);
		}

		public void TestDeclarationType()
		{
			var testDecl = Factory.New<JobDeclaration>();
			var entryInstruction = testDecl.CusEntryInstruction;

			entryInstruction.CEI_Style = "G1";
			AssertEquals("G1", testDecl.DeclarationType);

			entryInstruction.CEI_Style = "G2";
			AssertEquals("G2", testDecl.DeclarationType);
		}

		JobDeclaration CreatDeclarationForTestCalculateDuties()
		{
			TariffDataForTestHelper.NewData(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeListForRegistry.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoice.JZ_InvoiceAmount = 2948836m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "PR";
			invoiceLine.JI_Tariff = "87031000002";
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "UNT";
			invoiceLine.JI_EnteredUnitPrice = 2948836m;
			invoiceLine.JI_DtyPymntMthd = "DEF";
			invoiceLine.JI_VatPymntMthd = "DEF";
			invoiceLine.JI_TpfPymntMthdInfo.ClearValue();

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			return declaration;
		}

		public void TestIsFreeTradeZoneDocumentaryAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			var address = header.MainAddress;
			address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FTZ, "FTZ001", Core.Constants.CountryCodes.Taiwan);
			var documentaryAddress = declaration.SupplierDocumentaryAddress;
			documentaryAddress.OrganisationPK = header.PK;
			documentaryAddress.E2_OA_Address = address.PK;
			Assert("SupplierDocumentaryAddress.IsFreeTradeZone is true.", declaration.IsFreeTradeZoneDocumentaryAddress);
			address.CustomsCodes.DeleteAll();
			Assert("SupplierDocumentaryAddress.IsFreeTradeZone is false.", !declaration.IsFreeTradeZoneDocumentaryAddress);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FTZ, "FTZ001", Core.Constants.CountryCodes.Taiwan);
			Assert("ImporterDocumentaryAddress.IsFreeTradeZone is false.", !declaration.IsFreeTradeZoneDocumentaryAddress);
			documentaryAddress = declaration.ImporterDocumentaryAddress;
			documentaryAddress.OrganisationPK = header.PK;
			documentaryAddress.E2_OA_Address = address.PK;
			Assert("ImporterDocumentaryAddress.IsFreeTradeZone is true.", declaration.IsFreeTradeZoneDocumentaryAddress);
			address.CustomsCodes.DeleteAll();
			Assert("ImporterDocumentaryAddress.IsFreeTradeZone is false.", !declaration.IsFreeTradeZoneDocumentaryAddress);
		}

		public void TestFreeTradeZoneDeclarationTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Export.D5, true);
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Export.B8, true);
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Export.B9, true);
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Export.F4, true);
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Export.B1, false);
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Export.B2, false);
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Export.D1, false);
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Export.G3, false);
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Export.G5, false);
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Export.F5, false);
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Import.D8, true);
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Import.B6, true);
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Import.F2, true);
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Import.D2, false);
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Import.D7, false);
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Import.G1, false);
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Import.G2, false);
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Import.G7, false);
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Import.F1, false);
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Import.F3, false);
			AssertFreeTradeZoneDeclarationTypes(declaration, Constants.DeclarationTypes.Import.L1, false);
		}

		void AssertFreeTradeZoneDeclarationTypes(JobDeclaration declaration, ZString declartionType, ZBool expected)
		{
			declaration.CusEntryInstruction.CEI_Style = declartionType;
			AssertEquals(expected, declaration.FreeTradeZoneDeclarationTypes);
		}

		public void TestMasterBillIsRequiredForDeclarationType()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Export.D5, false);
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Export.B8, false);
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Export.B9, false);
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Export.F4, false);
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Export.B1, false);
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Export.B2, false);
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Export.D1, false);
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Export.G3, true);
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Export.G5, true);
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Export.F5, false);
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Import.D8, false);
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Import.B6, false);
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Import.F2, false);
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Import.D2, false);
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Import.D7, false);
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Import.G1, true);
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Import.G2, false);
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Import.G7, true);
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Import.F1, false);
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Import.F3, false);
			AssertMasterBillIsRequiredForDeclarationType(declaration, Constants.DeclarationTypes.Import.L1, false);
		}

		void AssertMasterBillIsRequiredForDeclarationType(JobDeclaration declaration, ZString declartionType, ZBool expected)
		{
			declaration.CusEntryInstruction.CEI_Style = declartionType;
			AssertEquals(expected, declaration.MasterBillIsRequiredForDeclarationType);
		}

		public void TestAutomaticallyDeclareNILForDeclarationType()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Export.D5, false);
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Export.B8, false);
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Export.B9, false);
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Export.F4, false);
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Export.B1, true);
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Export.B2, true);
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Export.D1, true);
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Export.G3, false);
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Export.G5, false);
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Export.F5, true);
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Import.D8, false);
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Import.B6, false);
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Import.F2, false);
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Import.D2, true);
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Import.D7, true);
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Import.G1, false);
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Import.G2, true);
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Import.G7, false);
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Import.F1, false);
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Import.F3, true);
			AssertAutomaticallyDeclareNILForDeclarationType(declaration, Constants.DeclarationTypes.Import.L1, false);
		}

		void AssertAutomaticallyDeclareNILForDeclarationType(JobDeclaration declaration, ZString declartionType, ZBool expected)
		{
			declaration.CusEntryInstruction.CEI_Style = declartionType;
			AssertEquals(expected, declaration.AutomaticallyDeclareNILForDeclarationType);
		}

		public void TestDefaultPackageDescriptionWhenCreatePackingList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CusEntryInstruction.CEI_PackageDescription = "package desc";
			var packingList = declaration.CreateCusPackingList(Factory);
			AssertEquals("package desc", packingList.CUL_PackageDescription);
		}

		public void TestAgencyResponseCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var disposition1 = entryHeader.CusDispositions.AddNew();
			disposition1.CDI_Type = "CUS";
			disposition1.CDI_StatusKey = CusDispositionStatusKeyList.Codes.ARM;
			disposition1.CDI_Status = "A01";
			disposition1.CDI_StatusDate = ZDateTime.Now;

			var disposition2 = entryHeader.CusDispositions.AddNew();
			disposition2.CDI_Type = "CUS";
			disposition2.CDI_StatusKey = CusDispositionStatusKeyList.Codes.ARM;
			disposition2.CDI_Status = "A02";
			disposition2.CDI_StatusDate = ZDateTime.Now;
			AssertEquals("A01,A02", declaration.AgencyResponseCode);

			disposition2.CDI_Type = "REF";
			AssertEquals("A01", declaration.AgencyResponseCode);
			AssertEquals("Agency Response Code", DataBoundResourceStrings.GetDataForProperty(declaration.AgencyResponseCodeInfo).Caption);
		}

		public void TestRequiredFormalitiesCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var disposition1 = entryHeader.CusDispositions.AddNew();
			disposition1.CDI_Type = "CUS";
			disposition1.CDI_StatusKey = CusDispositionStatusKeyList.Codes.RFM;
			disposition1.CDI_Status = "A03";
			disposition1.CDI_StatusDate = ZDateTime.Now;

			var disposition2 = entryHeader.CusDispositions.AddNew();
			disposition2.CDI_Type = "CUS";
			disposition2.CDI_StatusKey = CusDispositionStatusKeyList.Codes.RFM;
			disposition2.CDI_Status = "A04";
			disposition2.CDI_StatusDate = ZDateTime.Now;
			AssertEquals("A03,A04", declaration.RequiredFormalitiesCode);

			disposition2.CDI_Type = "REF";
			AssertEquals("A03", declaration.RequiredFormalitiesCode);
			AssertEquals("Required Formalities Code", DataBoundResourceStrings.GetDataForProperty(declaration.RequiredFormalitiesCodeInfo).Caption);
		}

		public void TestClearanceCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var disposition1 = entryHeader.CusDispositions.AddNew();
			disposition1.CDI_Type = "CUS";
			disposition1.CDI_StatusKey = CusDispositionStatusKeyList.Codes.CLR;
			disposition1.CDI_Status = "1";
			disposition1.CDI_StatusDate = ZDateTime.Now;

			var disposition2 = entryHeader.CusDispositions.AddNew();
			disposition2.CDI_Type = "CUS";
			disposition2.CDI_StatusKey = CusDispositionStatusKeyList.Codes.CLR;
			disposition2.CDI_Status = "2";
			disposition2.CDI_StatusDate = ZDateTime.Now;
			AssertEquals("1,2", declaration.ClearanceCode);

			disposition2.CDI_Type = "REF";
			AssertEquals("1", declaration.ClearanceCode);
			AssertEquals("Clearance Code", DataBoundResourceStrings.GetDataForProperty(declaration.ClearanceCodeInfo).Caption);
		}

		[ExpectNoExceptions]
		public void TestJE_DeclDocType_Caption()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(declaration.JE_DeclDocTypeInfo, "Decl. Doc. Type", "The printing document type of import/export declaration.");
		}

		public void TestIEntryNumberGeneratorProviderMembers()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_DateForDuty = ZDateTime.BrettsBirthday;
			entryInstruction.CEI_CustomsOffice = "A";
			entryInstruction.CEI_BoxNumber = "ABC";
			entryInstruction.CEI_Style = "G1";
			CombineAssertions(() =>
			{
				var provider = (IEntryNumberGeneratorProvider)declaration;
				AssertEquals("EntryNumberDate", entryInstruction.CEI_DateForDuty, provider.EntryNumberDate);
				AssertEquals("EntryNumberType", declaration.JE_MessageType, provider.EntryNumberType);
				AssertEquals("Company", declaration.Company, provider.Company);
				AssertEquals("ShipmentType", declaration.JE_MessageType, provider.ShipmentType);
				AssertEquals("EntryNumberPart1Info", entryInstruction.CEI_CustomsOffice, provider.EntryNumberPart1Info.Value);
				AssertEquals("CustomsBrokerageBoxNumberInfo", entryInstruction.CEI_BoxNumber, provider.CustomsBrokerageBoxNumberInfo.Value);
				AssertEquals("EntryNumberPart2Info", entryInstruction.CEI_Style, provider.EntryNumberPart2Info.Value);
				AssertEquals("GetEntryNumberGeneratorCategory", entryInstruction.GetEntryNumberGeneratorCategory(), provider.GetEntryNumberGeneratorCategory());
				AssertEquals("EntryNumberGeneratorProviderBusinessObject", declaration, provider.EntryNumberGeneratorProviderBusinessObject);
			});
		}

		public void TestLogCustomsClearedWhenCH_EntryReleaseDateIsInvalid()
		{
			var declaration = GetJobDeclarationForTesting();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123";
			entryHeader.CusEntryNumber.CE_EntryStatus = "C1";
			entryHeader.CH_EntryReleaseDate = ZDateTime.Empty;
			declaration.LogCustomsClearedIfNeeded();
			var log = declaration.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
			AssertNull(log);
		}

		#region TestLogCustomsClearedIfNeeded
		protected override BaseJobDeclaration GetTestDecForTestLogCustomsCleared(ZString messageType)
		{
			var declaration = GetJobDeclarationForTesting();
			declaration.JE_MessageType = messageType;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123";
			entryHeader.CusEntryNumber.CE_EntryStatus = "C1";
			entryHeader.CH_EntryReleaseDate = new ZDateTime(2024, 6, 25, 13, 30, 21);
			return declaration;
		}

		protected override ZString ExpectedReferenceForLogCustomsClearedIfNeeded => "C1";

		protected override ZDateTimeOffset GetExpectedEventTimeOffsetForLogCustomsClearedIfNeeded(BaseJobDeclaration declaration) => new ZDateTime(2024, 6, 25, 13, 30, 21).ToDateTimeOffset(declaration.Branch.HomePort);
		#endregion

		public void TestTWConsignorOrConsigneeAddress()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				AssertType<TWConsignorOrConsigneeAddressDependentCollection<TWConsignorAddress>>(jobDeclaration.ConsignorAddresses);
				AssertType<TWConsignorOrConsigneeAddressDependentCollection<TWConsigneeAddress>>(jobDeclaration.ConsigneeAddresses);
				AssertType<TWConsignorAddress>(jobDeclaration.ConsignorDocumentaryAddress);
				AssertType<TWConsigneeAddress>(jobDeclaration.ConsigneeDocumentaryAddress);
			});
		}

		protected override Type ExpectedLoadOrCreateCusPackingListType => typeof(CusPackingList);

		#region Implementation
		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return declaration;
		}

		protected override bool ExpectedSupportInvoiceLineRefs => true;

		JobDeclarationForTesting GetJobDeclarationForTesting()
		{
			var dec = Factory.New<JobDeclarationForTesting>();
			dec.DisableDefaultPackingInformation = true;
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return dec;
		}

		public override void TestIsDeclarationWithEntryInstruction()
		{
			Assert("Please override this test if EntryInstruction is supposed to be supported in the country-specific Declaration", !Factory.New<JobDeclaration>().CustomsEntryInstructionProvider.IsNoEntryInstruction);
		}
		#endregion
	}
}
