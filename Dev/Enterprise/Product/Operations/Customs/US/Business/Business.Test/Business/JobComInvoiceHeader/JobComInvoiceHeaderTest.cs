using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;
using static CargoWise.EventReference.Constants;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	sealed class JobComInvoiceHeaderTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		public void TestSetJZ_OA_FDAShipperAddress()
		{
			var supplier1 = Factory.New<OrgHeader>();
			supplier1.OH_Code = "SUPPLIER1";
			supplier1.OH_FullName = "SUPPLIER 1";
			var addressPK = supplier1.MainAddress.PK;

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_OA_InvoicerDocAddress = addressPK;
			invoice.JZ_OA_FDAShipperAddress = addressPK;
			AssertEquals(addressPK, invoice.JZ_OA_FDAShipperAddress);
			AssertEquals(ZGuid.Empty, invoice.FDAShipperDocumentaryAddress.E2_OA_Address);

			invoice.JZ_OA_InvoicerDocAddress = ZGuid.Empty;
			invoice.JZ_OA_SupplierAddress = addressPK;
			invoice.JZ_OA_FDAShipperAddress = addressPK;
			AssertEquals(addressPK, invoice.JZ_OA_FDAShipperAddress);
			AssertEquals(ZGuid.Empty, invoice.FDAShipperDocumentaryAddress.E2_OA_Address);
		}

		public void TestUS_USPPIEIN()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			orgHeader.OH_FullName = "ABC INC.";
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "33-8888888AA", Core.Constants.CountryCodes.UnitedStates);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			invoice.USPPIDocAddress.OrganisationPK = orgHeader.PK;
			AssertEquals("EIN: 33-8888888AA", invoice.US_USPPIEIN);

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			invoice.USPPIDocAddress.OrganisationPK = ZGuid.Empty;
			invoice.USPPIDocAddress.OrganisationPK = orgHeader.PK;
			AssertEquals(ZString.Empty, invoice.US_USPPIEIN);
		}

		protected override (BaseJobDeclaration, BaseJobDeclaration) GetDeclarationsForAttach()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var result = base.GetDeclarationsForAttach();
			var declaration = result.Item1 as JobDeclaration;
			declaration.US_EntryFilerCode = "XJ5";
			declaration = result.Item2 as JobDeclaration;
			declaration.US_EntryFilerCode = "XJ5";
			return result;
		}

		[ExpectNoExceptions]
		public void TestAllAddInfoColumnsAreInModelView()
		{
			var jobComInvoiceHeader = Factory.New<JobComInvoiceHeader>();
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(jobComInvoiceHeader,
				"USJobComInvoiceHeader",
				schemaTypeName: nameof(AutoJobComInvoiceHeader.Schema));
		}

		public void TestUS_LicenseNoReadOnly()
		{
			var factory = new BusinessObjectFactory();
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(factory, new ZString[] { USAESLicenseCode.Codes.C33 });
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_LicenseType = ZString.Empty;
			var invoice = declaration.Invoices.AddNew();
			invoice.US_LicenseType = ZString.Empty;
			Assert("should not read-only", !invoice.US_LicenseNoInfo.ReadOnly);
			invoice.US_LicenseType = USAESLicenseCode.Codes.C33;
			Assert("should read-only", invoice.US_LicenseNoInfo.ReadOnly);
			invoice.US_LicenseNo = "ABC";
			Assert("should not read-only", !invoice.US_LicenseNoInfo.ReadOnly);
		}

		public void TestGetFromJobBranchCurrentTime()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "AAA";

			var branch1 = Factory.New<GlbBranch>();
			var branch2 = Factory.New<GlbBranch>();
			branch1.GB_Code = "CCC";
			branch1.GB_GC = company1.PK;
			branch1.GB_RL_NKHomePort = "USPHL";
			branch2.GB_Code = "DDD";
			branch2.GB_RL_NKHomePort = "USLAX";
			branch2.GB_GC = company1.PK;

			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "TTT";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();

			using (Env.SetTemporaryUserContext(company1.PK.ToGuid(), branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				declaration.JE_GB = branch1.PK;

				var localTimeBranch1 = ZDateTime.Now;
				var utcTimeNow = DateTime.UtcNow;
				var centralimeInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
				var centralTimeZone = TimeZoneInfo.ConvertTimeFromUtc(utcTimeNow, centralimeInfo);
				AssertEquals("Branch1 Time Zone", centralTimeZone.TimeOfDay.Hours, localTimeBranch1.TimeOfDay.Hours);

				using (Env.SetTemporaryUserContext(company1.PK.ToGuid(), branch2.PK.ToGuid(), department.PK.ToGuid()))
				{
					var localTimeBranch2 = ZDateTime.Now;
					var utcTimeNow2 = DateTime.UtcNow;

					var log = invoice.LogManager.AddALogIfNecessary("", ImportMessageStatusList.Codes.ClearDepartureOriginal, new ImportMessageStatusList());
					AssertEquals(ImportMessageStatusList.Codes.ClearDepartureOriginal, log.SL_Reference);

					var pacificTimeInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
					var pacificTimeZone = TimeZoneInfo.ConvertTimeFromUtc(utcTimeNow2, pacificTimeInfo);
					AssertEquals("Branch2 Time Zone", pacificTimeZone.TimeOfDay.Hours, localTimeBranch2.TimeOfDay.Hours);

					AssertEquals("Wrote by Job Branch Time", localTimeBranch1.TimeOfDay.Hours, log.SL_EventTime.TimeOfDay.Hours);
				}
			}
		}

		public void TestDoNotLoadPGARelatedDataIfIsUncommitted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = Factory.New<JobComInvoiceHeaderForTest>();
			invoice.JZ_JE = declaration.PK;
			var properties = new (ZPropertyInfo info, IZType value)[]
			{
				(invoice.US_FDAContactNameInfo, (ZString)"$"),
				(invoice.US_FDAContactPhoneNoInfo, (ZString)"$"),
				(invoice.US_FDAContactEmailInfo, (ZString)"$"),
				(invoice.JZ_CU_RelatedHouseBillInfo, ZGuid.BrettsGuid),
				(invoice.JZ_OA_ExporterAddressInfo, ZGuid.BrettsGuid),
				(invoice.US_ZoneStatusInfo, (ZString)"$"),
				(invoice.JZ_OA_ManufacturerAddressInfo, ZGuid.BrettsGuid),
				(invoice.JZ_OA_SupplierAddressInfo, ZGuid.BrettsGuid),
				(invoice.JZ_OA_ConsigneeAddressInfo, ZGuid.BrettsGuid),
				(invoice.US_UC_NKCountryOfOriginInfo, (ZString)"$"),
				(invoice.US_UC_NKCountryOfExportInfo, (ZString)"$"),
				(invoice.JZ_OH_BuyerInfo, ZGuid.BrettsGuid),
			};
			CombineAssertions(() =>
			{
				invoice.IsUnCommittedRow_Mock = true;
				foreach (var property in properties)
				{
					var info = property.info;
					info.Value = property.value;
					AssertEquals($"Setting {info.Name} should not trigger PGA Tracker", 0, invoice.TrackerCount);
					info.Value = info.DefaultValue;
					AssertEquals($"Clearing {info.Name} should not trigger PGA Tracker", 0, invoice.TrackerCount);
				}

				invoice.IsUnCommittedRow_Mock = false;
				foreach (var property in properties)
				{
					invoice.TrackerCount = 0;
					var info = property.info;
					info.Value = property.value;
					AssertEquals($"Setting {info.Name} should trigger PGA Tracker", 1, invoice.TrackerCount);
					info.Value = info.DefaultValue;
					AssertEquals($"Clearing {info.Name} should trigger PGA Tracker", 2, invoice.TrackerCount);
				}
			});
		}

		public void TestAddCommercialInvoiceHeaderRefsToDeclaration()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "LMAG";
			carrier.UI_ModeOfTransportation = "10";
			var carrier2 = Factory.New<USCarrierCombined>();
			carrier2.UI_Code = "DDKK";
			carrier2.UI_ModeOfTransportation = "10";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.CN;
			ref1.J2_ReferenceNumber = "CNTTT002";

			var ref2 = invoice.InvoiceHeaderRefs.AddNew();
			ref2.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.MB;
			ref2.J2_ReferenceNumber = "DDKK800450";

			var ref3 = invoice.InvoiceHeaderRefs.AddNew();
			ref3.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.HB;
			ref3.J2_ReferenceNumber = "LMAG55667788";

			invoice.JZ_JE = declaration.PK;

			AssertEquals(1, declaration.CusContainers.Count);
			AssertNotNull(declaration.CusContainers.Find("CNTTT002"));

			AssertEquals(2, declaration.Bills.Count);
			var mBill = declaration.Bills.FindByBillNumberAndType("800450", BillTypeList.Codes.MasterBill);
			AssertNotNull(mBill);
			AssertEquals("DDKK", mBill.US_UI_NKBillIssuerSCAC);

			var hBill = declaration.Bills.FindByBillNumberAndType("55667788", BillTypeList.Codes.HouseBill);
			AssertNotNull(hBill);
			AssertEquals("LMAG", hBill.US_UI_NKBillIssuerSCAC);

			AssertEquals("DDKK", declaration.JE_MasterBillIssuerSCAC);
			AssertEquals("800450", declaration.JE_MasterBill);

			AssertEquals("LMAG", declaration.JE_HouseBillIssuerSCAC);
			AssertEquals("55667788", declaration.JE_HouseBill);
		}

		public void TestUS_DeductADDCVDDuty_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			AssertEquals(true, invoice.IsDeductADD_CVDDutyRequired);
			AssertEquals(false, invoice.US_DeductADDCVDDutyInfo.ReadOnly);

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			AssertEquals(false, invoice.IsDeductADD_CVDDutyRequired);
			AssertEquals(true, invoice.US_DeductADDCVDDutyInfo.ReadOnly);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			AssertEquals(false, invoice.IsDeductADD_CVDDutyRequired);
			AssertEquals(true, invoice.US_DeductADDCVDDutyInfo.ReadOnly);
		}

		public void TestExportDateForLicenseType()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			AssertEquals(ZDateTime.Today, invoice.ExportDateForLicenseType);
			invoice.US_DateOfExport = ZDateTime.Today.AddDays(-1);
			AssertEquals(ZDateTime.Today.AddDays(-1), invoice.ExportDateForLicenseType);

			invoice.US_DateOfExport = ZDateTime.Invalid;
			AssertEquals("Invalidat Date should now allow on US_DateOfExport in InvoiceLine", ZDateTime.Today, invoice.ExportDateForLicenseType);
			dec.US_DateOfExport = ZDateTime.Today.AddDays(-5);
			AssertEquals("fallback to US_DateOfExport on JobDeclaration", dec.US_DateOfExport, invoice.ExportDateForLicenseType);
		}

		public void TestGetEffectiveConsigeeAddress()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG1";
			org1.MainAddress.OA_Address1 = "ORG1 MAIN ADDRESS";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG2";
			org2.MainAddress.OA_Address1 = "ORG2 MAIN ADDRESS";
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "ORG3";
			org3.MainAddress.OA_Address1 = "ORG3 MAIN ADDRESS";
			var org3SelectedAddress = org3.Addresses.AddNew();
			org3SelectedAddress.OA_Address1 = "ORG3 SELECTED ADDRESS";

			Declaration.JE_OH_Importer = org1.PK;
			var inv1 = Declaration.Invoices.AddNew();
			inv1.JZ_OH_Buyer = org2.PK;
			inv1.JZ_OA_ConsigneeAddress = org3SelectedAddress.PK;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("EffectiveConsigeeAddress", inv1.UltimateConsigneeDocAddress.RealAddress, inv1.EffectiveConsigeeAddress);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("EffectiveConsigeeAddress", org3SelectedAddress, inv1.EffectiveConsigeeAddress);
		}

		public void TestGetEffectiveSupplierAddress()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG1";
			org1.MainAddress.OA_Address1 = "ORG1 MAIN ADDRESS";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG2";
			org2.MainAddress.OA_Address1 = "ORG2 MAIN ADDRESS";
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "ORG3";
			org3.MainAddress.OA_Address1 = "ORG3 MAIN ADDRESS";
			var org3SelectedAddress = org3.Addresses.AddNew();
			org3SelectedAddress.OA_Address1 = "ORG3 SELECTED ADDRESS";

			Declaration.JE_OH_Supplier = org1.PK;
			var inv1 = Declaration.Invoices.AddNew();
			inv1.JZ_OH_Supplier = org2.PK;
			inv1.JZ_OA_SupplierAddress = org3SelectedAddress.PK;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("EffectiveSupplierAddress", inv1.USPPIDocAddress.RealAddress, inv1.EffectiveSupplierAddress);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("EffectiveSupplierAddress", org3SelectedAddress, inv1.EffectiveSupplierAddress);
		}

		public void TestUSPPIDocAddressValidationSuppressed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "OH" + new Random().Next(1000000).ToString();
			org.OH_FullName = "DUMMY ORG FOR TESTING";
			org.MainAddress.OA_Address1 = "ADDRESS 1";
			org.OH_RL_NKClosestPort = "USCHI";
			var usppiDocAddress = invoice.USPPIDocAddress;

			usppiDocAddress.E2_AddressOverride = true;
			usppiDocAddress.E2_Address1 = ZString.Empty;
			usppiDocAddress.E2_Address2 = ZString.Empty;
			usppiDocAddress.E2_RN_NKCountryCode = ZString.Empty;
			usppiDocAddress.E2_City = ZString.Empty;
			usppiDocAddress.E2_Postcode = ZString.Empty;
			usppiDocAddress.E2_State = ZString.Empty;
			usppiDocAddress.E2_CompanyName = ZString.Empty;

			usppiDocAddress.Validation.ValidateAll();
			AssertNoErrors(usppiDocAddress.E2_Address1Info);
			AssertNoErrors(usppiDocAddress.E2_Address2Info);
			AssertNoErrors(usppiDocAddress.E2_RN_NKCountryCodeInfo);
			AssertNoErrors(usppiDocAddress.E2_CityInfo);
			AssertNoErrors(usppiDocAddress.E2_PostcodeInfo);
			AssertNoErrors(usppiDocAddress.E2_StateInfo);

			AssertHasErrorContaining(usppiDocAddress.E2_CompanyNameInfo, "Please enter a Company Name, or remove the override for this Address.");

			usppiDocAddress.E2_CompanyName = "TEST COMPANY";
			usppiDocAddress.Validation.ValidateAll();
			AssertNoErrorContaining(usppiDocAddress.E2_CompanyNameInfo, "Please enter a Company Name, or remove the override for this Address.");

			usppiDocAddress.E2_AddressOverride = true;
			usppiDocAddress.E2_RN_NKCountryCode = "AU";

			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			using (OrganisationsDataRegistry.Instance.AddressValidationServiceSuppression.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressListCollection()))
			{
				usppiDocAddress.E2_ValidationStatus = AddressValidationStatus.Invalid;
				AssertNoErrorContaining(usppiDocAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

				usppiDocAddress.IgnoreValidationStatusError = false;
				usppiDocAddress.Validation.ValidateE2_ValidationStatus();
				AssertHasErrorContaining(usppiDocAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
			}
		}

		public void TestUltimateConsigneeDocAddressDefault()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var addressPK = importer.MainAddress.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();
			AssertEquals(addressPK, invoice.UltimateConsigneeDocAddress.E2_OA_Address);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newInvoice = newFactory.Load<JobComInvoiceHeader>(invoice.PK);
			AssertEquals(addressPK, newInvoice.UltimateConsigneeDocAddress.E2_OA_Address);
		}

		public void TestIntermediateConsigneeDocAddressDefault()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var addressPK = consignee.MainAddress.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Consignee = consignee.PK;
			var invoice = declaration.Invoices.AddNew();
			AssertEquals(addressPK, invoice.IntermediateConsigneeDocAddress.E2_OA_Address);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newInvoice = newFactory.Load<JobComInvoiceHeader>(invoice.PK);
			AssertEquals(addressPK, newInvoice.IntermediateConsigneeDocAddress.E2_OA_Address);
		}

		public void TestFetchForLoadChildEditableObjectsCoreOfEDIMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			message.EM_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			message.EM_Status = EDIMessageStatusList.Codes.Sent;
			message.EM_LinkTable = JobComInvoiceHeader.Schema.TableName;
			message.EM_LinkUniqueID = invoice.PK;
			Factory.AllowMultipleBusinessObjectsAroundOneRow = false;
			AssertNoExceptionThrown(() =>
			{
				invoice.FetchStrategy.FetchForLoadChildEditableObjects();
				Factory.Load(typeof(Enterprise.Messaging.Business.EDIMessage), new ZQuery(EDIMessageSchema.EM_LinkUniqueID, invoice.PK));
			});
		}

		public void TestUniversalCopyWithExtendedEntitiesAttribute()
		{
			AssertNotNull(typeof(JobComInvoiceHeader).GetCustomAttribute<UniversalCopyWithExtendedEntitiesAttribute>());
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			ICommonInvoice commonInvoice = dec.Invoices.AddNew();
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			ZArchitecture.Core.CodeDescriptionPairList customsChargeTypeList = new USCustomsChargeTypeList();
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			customsChargeTypeList = new CustomsChargeTypeList();
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}

		public void TestHumanReadableShortcutNameInCommercialInvoice()
		{
			org1.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "ABC123456");
			org1.OH_FullName = "FULLNAME";

			InvoiceHeader.JZ_InvoiceNumber = "INV23423";
			InvoiceHeader.JZ_OH_Buyer = org1.PK;

			AssertEquals("Invoice - invoice# - ImporterFullName", string.Format("Invoice - {0} - {1}", InvoiceHeader.JZ_InvoiceNumber, InvoiceHeader.Importer.OH_FullName), InvoiceHeader.HumanReadableShortcutName);
		}

		public void TestManufacturerFallBackToSupplierNumber()
		{
			var supplier = Factory.New<OrgHeader>();
			var supplierAddress = supplier.Addresses.AddNew(OrgAddressType.Miscellaneous, false);
			supplierAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "SUP12345678");

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MAN12345678");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_OA_SupplierAddress = supplierAddress.PK;
			invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			AssertEquals("ManufacturerFallBackToSupplierNumber", "MAN12345678", invoice.ManufacturerFallBackToSupplierNumber);

			invoice.JZ_OA_ManufacturerAddress = ZGuid.Empty;
			AssertEquals("ManufacturerFallBackToSupplierNumber", "SUP12345678", invoice.ManufacturerFallBackToSupplierNumber);

			invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			AssertEquals("ManufacturerFallBackToSupplierNumber", "MAN12345678", invoice.ManufacturerFallBackToSupplierNumber);
		}

		public void TestSupportsRelatedBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			AssertEquals(true, invoice.SupportsRelatedBill);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(false, invoice.SupportsRelatedBill);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(false, invoice.SupportsRelatedBill);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableAII = true;
			AssertEquals(true, invoice.SupportsRelatedBill);
		}

		public void TestSupplierDefaultToInvoiceOnImport()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "BOB THE BUILDER";
			org1.MainAddress.OA_Address1 = "BOB ADDRESS 1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "WENDY THE DESTROYER";
			org2.MainAddress.OA_Address1 = "WENDY ADDRESS 1";

			var dec = Factory.New<JobDeclaration>();
			dec.JE_OH_Supplier = org1.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = dec.Invoices.AddNew();
			AssertEquals("invoice.JZ_OH_Supplier", org1.PK, invoice.JZ_OH_Supplier);
			AssertEquals("invoice.JZ_OA_SupplierAddress", org1.MainAddress.PK, invoice.JZ_OA_SupplierAddress);
			dec.JE_OH_Supplier = org2.PK;
			AssertEquals("invoice.JZ_OH_Supplier", org2.PK, invoice.JZ_OH_Supplier);
			AssertEquals("invoice.JZ_OA_SupplierAddress", org2.MainAddress.PK, invoice.JZ_OA_SupplierAddress);
			dec.JE_OH_Supplier = ZGuid.Empty;
			AssertEquals("invoice.JZ_OH_Supplier", ZGuid.Empty, invoice.JZ_OH_Supplier);
			AssertEquals("invoice.JZ_OA_SupplierAddress", ZGuid.Empty, invoice.JZ_OA_SupplierAddress);
			dec.JE_OH_Supplier = ZGuid.Invalid;
			AssertEquals("invoice.JZ_OH_Supplier", ZGuid.Invalid, invoice.JZ_OH_Supplier);
			AssertEquals("invoice.JZ_OA_SupplierAddress", ZGuid.Empty, invoice.JZ_OA_SupplierAddress);
			dec.JE_OH_Supplier = ZGuid.Missing;
			AssertEquals("invoice.JZ_OH_Supplier", ZGuid.Missing, invoice.JZ_OH_Supplier);
			AssertEquals("invoice.JZ_OA_SupplierAddress", ZGuid.Empty, invoice.JZ_OA_SupplierAddress);
			dec.JE_OH_Supplier = org1.PK;
			AssertEquals("invoice.JZ_OH_Supplier", org1.PK, invoice.JZ_OH_Supplier);
			AssertEquals("invoice.JZ_OA_SupplierAddress", org1.MainAddress.PK, invoice.JZ_OA_SupplierAddress);
		}

		public void TestHasInvoiceLinesWithSection301Or232()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceline1 = invoice.JobComInvoiceLines.AddNew();
			invoiceline1.US_SupTariff = "99011001";
			var invoiceline2 = invoice.JobComInvoiceLines.AddNew();
			invoiceline2.US_SupTariff = "99011002";
			AssertEquals("No invoice line with section 301 or 232 exists", false, invoice.HasInvoiceLinesWithSection301Or232);

			invoiceline1.US_SupTariff = "99031001";
			AssertEquals("One invoice line with section 301 or 232 exists", true, invoice.HasInvoiceLinesWithSection301Or232);
		}

		public void TestHasInvoiceLinesWithODSOrTSCAARequireCB()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = true;
			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();

			invoiceLine1.US_ODSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine1.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine1.US_TSCAODSCertIndividual = PartyTypeList.Codes.CustomsBroker;
			Assert(invoice.HasInvoiceLinesWithODSOrTSCAARequireCB);

			invoiceLine1.US_ODSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine1.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine1.US_TSCAODSCertIndividual = PartyTypeList.Codes.CustomsBroker;
			Assert(invoice.HasInvoiceLinesWithODSOrTSCAARequireCB);

			invoiceLine1.US_TSCAODSCertIndividual = PartyTypeList.Codes.Importer;
			Assert(!invoice.HasInvoiceLinesWithODSOrTSCAARequireCB);
		}

		public void TestHasInvoiceLinesWith()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = true;
			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.US_APHISInd = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals("RequiresCustomsBrokerReporting", false, invoice.RequiresCustomsBrokerReporting);
			AssertEquals("HasInvoiceLinesWithAPHIS", false, invoice.HasInvoiceLinesWithAPHIS);
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_APHISInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("RequiresCustomsBrokerReporting", true, invoice.RequiresCustomsBrokerReporting);
			AssertEquals("HasInvoiceLinesWithAPHIS", true, invoice.HasInvoiceLinesWithAPHIS);
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.US_APHISInd = "!";
			AssertEquals("RequiresCustomsBrokerReporting", true, invoice.RequiresCustomsBrokerReporting);
			AssertEquals("HasInvoiceLinesWithAPHIS", true, invoice.HasInvoiceLinesWithAPHIS);
			invoiceLine1.US_APHISInd = ZString.Empty;
			invoiceLine2.US_APHISInd = ZString.Empty;
			invoiceLine3.US_APHISInd = ZString.Empty;
			AssertEquals("RequiresCustomsBrokerReporting", false, invoice.RequiresCustomsBrokerReporting);
			AssertEquals("HasInvoiceLinesWithAPHIS", false, invoice.HasInvoiceLinesWithAPHIS);
			invoiceLine2.US_FWSInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("RequiresCustomsBrokerReporting", false, invoice.RequiresCustomsBrokerReporting);
			AssertEquals("HasInvoiceLinesWithFWS", true, invoice.HasInvoiceLinesWithFWS);
			invoiceLine3.US_FWSInd = "!";
			AssertEquals("RequiresCustomsBrokerReporting", false, invoice.RequiresCustomsBrokerReporting);
			AssertEquals("HasInvoiceLinesWithFWS", true, invoice.HasInvoiceLinesWithFWS);
			invoiceLine1.US_FWSInd = ZString.Empty;
			invoiceLine2.US_FWSInd = ZString.Empty;
			invoiceLine3.US_FWSInd = ZString.Empty;
			AssertEquals("RequiresCustomsBrokerReporting", false, invoice.RequiresCustomsBrokerReporting);
			AssertEquals("HasInvoiceLinesWithFWS", false, invoice.HasInvoiceLinesWithFWS);
			invoiceLine2.US_ODSInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("RequiresCustomsBrokerReporting", false, invoice.RequiresCustomsBrokerReporting);
			AssertEquals("HasInvoiceLinesWithODS", true, invoice.HasInvoiceLinesWithODS);
			invoiceLine2.US_ODSInd = ZString.Empty;
			invoiceLine2.US_VNEInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("RequiresCustomsBrokerReporting", false, invoice.RequiresCustomsBrokerReporting);
			AssertEquals("HasInvoiceLinesWithODS", false, invoice.HasInvoiceLinesWithODS);
			AssertEquals("HasInvoiceLinesWithVNE", true, invoice.HasInvoiceLinesWithVNE);
			invoiceLine2.US_VNEInd = ZString.Empty;
			invoiceLine2.US_FSISInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("RequiresCustomsBrokerReporting", true, invoice.RequiresCustomsBrokerReporting);
			AssertEquals("HasInvoiceLinesWithVNE", false, invoice.HasInvoiceLinesWithVNE);
			AssertEquals("HasInvoiceLinesWithFSIS", true, invoice.HasInvoiceLinesWithFSIS);
			invoiceLine2.US_FSISInd = ZString.Empty;
			invoiceLine2.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals("RequiresCustomsBrokerReporting", true, invoice.RequiresCustomsBrokerReporting);
			AssertEquals("HasInvoiceLinesWithFSIS", false, invoice.HasInvoiceLinesWithFSIS);
			AssertEquals("HasInvoiceLinesWithPST", true, invoice.HasInvoiceLinesWithPST);
			invoiceLine2.US_HFCInd = ZString.Empty;
			AssertEquals("HasInvoiceLinesWithHFC", false, invoice.HasInvoiceLinesWithHFC);
			invoiceLine2.US_HFCInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("HasInvoiceLinesWithHFC", true, invoice.HasInvoiceLinesWithHFC);

			invoice.InvoiceLines.RemoveAndDeleteAll();
			invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			AssertHasInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_FDAIndicator, () => invoice.HasInvoiceLinesWithOGAFDA && dec.PGAFlags.HasInvoiceLinesWithOGAFDA);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_FDAIndicator, () => invoice.HasInvoiceLinesWithOGAFDA);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_FDAIndicator, () => dec.PGAFlags.HasInvoiceLinesWithOGAFDA);

			AssertHasInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_DOTIndicator, () => invoice.HasInvoiceLinesWithDOT && dec.PGAFlags.HasInvoiceLinesWithDOT);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_DOTIndicator, () => invoice.HasInvoiceLinesWithDOT);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_DOTIndicator, () => dec.PGAFlags.HasInvoiceLinesWithDOT);

			AssertHasInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_NHTSAIndicator, () => invoice.HasInvoiceLinesWithNHTSA && dec.PGAFlags.HasInvoiceLinesWithNHTSA);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_NHTSAIndicator, () => invoice.HasInvoiceLinesWithNHTSA);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_NHTSAIndicator, () => dec.PGAFlags.HasInvoiceLinesWithNHTSA);

			AssertHasInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_NMFS370Ind, () => invoice.HasInvoiceLinesWithNMFS && dec.PGAFlags.HasInvoiceLinesWithNMFS);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_NMFS370Ind, () => invoice.HasInvoiceLinesWithNMFS);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_NMFS370Ind, () => dec.PGAFlags.HasInvoiceLinesWithNMFS);

			AssertHasInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_NMFSAMRInd, () => invoice.HasInvoiceLinesWithNMFS && dec.PGAFlags.HasInvoiceLinesWithNMFS);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_NMFSAMRInd, () => invoice.HasInvoiceLinesWithNMFS);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_NMFSAMRInd, () => dec.PGAFlags.HasInvoiceLinesWithNMFS);

			AssertHasInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_NMFSHMSInd, () => invoice.HasInvoiceLinesWithNMFS && dec.PGAFlags.HasInvoiceLinesWithNMFS);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_NMFSHMSInd, () => invoice.HasInvoiceLinesWithNMFS);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_NMFSHMSInd, () => dec.PGAFlags.HasInvoiceLinesWithNMFS);

			AssertHasInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_NMFSSIMPInd, () => invoice.HasInvoiceLinesWithNMFS && dec.PGAFlags.HasInvoiceLinesWithNMFS);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_NMFSSIMPInd, () => invoice.HasInvoiceLinesWithNMFS);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_NMFSSIMPInd, () => dec.PGAFlags.HasInvoiceLinesWithNMFS);

			AssertHasInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_APHISInd, () => invoice.HasInvoiceLinesWithAPHIS && dec.PGAFlags.HasInvoiceLinesWithAPHIS);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_APHISInd, () => invoice.HasInvoiceLinesWithAPHIS);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_APHISInd, () => dec.PGAFlags.HasInvoiceLinesWithAPHIS);

			AssertHasInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_FWSInd, () => invoice.HasInvoiceLinesWithFWS && dec.PGAFlags.HasInvoiceLinesWithFWS);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_FWSInd, () => invoice.HasInvoiceLinesWithFWS);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_FWSInd, () => dec.PGAFlags.HasInvoiceLinesWithFWS);

			AssertHasInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_OMCInd, () => invoice.HasInvoiceLinesWithOMC && dec.PGAFlags.HasInvoiceLinesWithOMC);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_OMCInd, () => invoice.HasInvoiceLinesWithOMC);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_OMCInd, () => dec.PGAFlags.HasInvoiceLinesWithOMC);

			AssertHasInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_TSCAInd, () => invoice.HasInvoiceLinesWithTSCA && dec.PGAFlags.HasInvoiceLinesWithTSCA);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_TSCAInd, () => invoice.HasInvoiceLinesWithTSCA);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_TSCAInd, () => dec.PGAFlags.HasInvoiceLinesWithTSCA);

			AssertHasInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_AMSInd, () => invoice.HasInvoiceLinesWithAMS && dec.PGAFlags.HasInvoiceLinesWithAMS);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_AMSInd, () => invoice.HasInvoiceLinesWithAMS);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_AMSInd, () => dec.PGAFlags.HasInvoiceLinesWithAMS);

			AssertHasInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_NOPInd, () => invoice.HasInvoiceLinesWithNOP && dec.PGAFlags.HasInvoiceLinesWithNOP);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_NOPInd, () => invoice.HasInvoiceLinesWithNOP);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_NOPInd, () => dec.PGAFlags.HasInvoiceLinesWithNOP);

			AssertHasInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_CPSCInd, () => invoice.HasInvoiceLinesWithCPSC && dec.PGAFlags.HasInvoiceLinesWithCPSC);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_CPSCInd, () => invoice.HasInvoiceLinesWithCPSC);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_CPSCInd, () => dec.PGAFlags.HasInvoiceLinesWithCPSC);

			AssertHasInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_LaceyIndicator, () => invoice.HasInvoiceLinesWithACELacey && dec.PGAFlags.HasInvoiceLinesWithACELacey);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_LaceyIndicator, () => invoice.HasInvoiceLinesWithACELacey);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_LaceyIndicator, () => dec.PGAFlags.HasInvoiceLinesWithACELacey);

			AssertHasInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_FDAIndicator, () => invoice.HasInvoiceLinesWithOGAFDA && dec.PGAFlags.HasInvoiceLinesWithOGAFDA);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_FDAIndicator, () => invoice.HasInvoiceLinesWithOGAFDA);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_FDAIndicator, () => dec.PGAFlags.HasInvoiceLinesWithOGAFDA);

			AssertHasInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_DEAInd, () => invoice.HasInvoiceLinesWithDEA && dec.PGAFlags.HasInvoiceLinesWithDEA);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_DEAInd, () => invoice.HasInvoiceLinesWithDEA);
			AssertNoInvoiceLinesWith(invoiceLine2, USAddInfoSchema.Constants.US_DEAInd, () => dec.PGAFlags.HasInvoiceLinesWithDEA);

			invoiceLine2.US_FDAIndicator = ZString.Empty;
			invoiceLine2.FDAs.AddNew();
			invoiceLine2.ACE_FDALines.AddNew();
			AssertEquals(false, invoice.HasInvoiceLinesWithOGAFDA);
			AssertEquals(false, invoice.HasInvoiceLinesWithPGAFDA);

			invoiceLine2.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(false, invoice.HasInvoiceLinesWithOGAFDA);
			AssertEquals(false, invoice.HasInvoiceLinesWithPGAFDA);

			invoiceLine2.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(true, invoice.HasInvoiceLinesWithOGAFDA);
			AssertEquals(true, invoice.HasInvoiceLinesWithPGAFDA);
			invoiceLine2.FDAs.RemoveAndDeleteAll();
			AssertEquals(true, invoice.HasInvoiceLinesWithPGAFDA);

			var fwsheader = invoiceLine2.FWSHeaders.AddNew();
			fwsheader.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			AssertEquals(true, invoiceLine2.InvoiceHeader.HasInvoiceLinesWithFWSProcessingCodeWithEDS);
		}

		public void TestRequiresCustomsBrokerReporting()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = true;
			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("RequiresCustomsBrokerReporting", true, invoice.RequiresCustomsBrokerReporting);
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("RequiresCustomsBrokerReporting", false, invoice.RequiresCustomsBrokerReporting);
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("RequiresCustomsBrokerReporting", true, invoice.RequiresCustomsBrokerReporting);

			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("RequiresCustomsBrokerReporting", false, invoice.RequiresCustomsBrokerReporting);
			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("RequiresCustomsBrokerReporting", true, invoice.RequiresCustomsBrokerReporting);

			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals("RequiresCustomsBrokerReporting", false, invoice.RequiresCustomsBrokerReporting);
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals("RequiresCustomsBrokerReporting", true, invoice.RequiresCustomsBrokerReporting);

			invoiceLine.US_PSTIndicator = ZString.Empty;
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("RequiresCustomsBrokerReporting", true, invoice.RequiresCustomsBrokerReporting);
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals("RequiresCustomsBrokerReporting", false, invoice.RequiresCustomsBrokerReporting);
		}

		public void TestRequiresCustomsBrokerReportingForFDA()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = true;
			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.ACE_FDALines.AddNew();
			invoiceLine.RefreshInvoiceLinesWithPGAIndicators();
			AssertEquals("RequiresCustomsBrokerReporting", true, invoice.RequiresCustomsBrokerReporting);

			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			AssertEquals("RequiresCustomsBrokerReporting", false, invoice.RequiresCustomsBrokerReporting);

			dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableSPN = true;
			invoice = dec.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.RefreshInvoiceLinesWithPGAIndicators();
			invoiceLine.ACE_FDALines.AddNew();
			AssertEquals("RequiresCustomsBrokerReporting", true, invoice.RequiresCustomsBrokerReporting);

			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("RequiresCustomsBrokerReporting", false, invoice.RequiresCustomsBrokerReporting);

			dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			dec.US_EnableSPN = true;
			dec.US_F_PNMode = PriorNoticeModeCodeList.Codes.P;
			invoice = dec.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.RefreshInvoiceLinesWithPGAIndicators();
			invoiceLine.ACE_FDALines.AddNew();
			AssertEquals("RequiresCustomsBrokerReporting", true, invoice.RequiresCustomsBrokerReporting);

			dec.US_F_PNMode = PriorNoticeModeCodeList.Codes.O;
			AssertEquals("RequiresCustomsBrokerReporting", false, invoice.RequiresCustomsBrokerReporting);
		}

		public void TestSetImporterAndSupplierWhenDetachFromDeclaration()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();

			AssertEquals(declaration.PK, invoice.JZ_JE);
			AssertEquals(importer.PK, invoice.JZ_OH_Buyer);
			AssertEquals(supplier.PK, invoice.JZ_OH_Supplier);
			AssertNotEquals(ZGuid.Empty, invoice.JZ_OA_SupplierAddress);

			invoice.JZ_JE = ZGuid.Empty;

			AssertEquals(importer.PK, invoice.JZ_OH_Buyer);
			AssertEquals(supplier.PK, invoice.JZ_OH_Supplier);
			AssertNotEquals(ZGuid.Empty, invoice.JZ_OA_SupplierAddress);
		}

		public void TestDefaultUltimateConsigneeType()
		{
			var importer1 = Factory.New<OrgHeader>();
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			var link1 = supplier1.BuyerLinks.AddNew(importer1);
			link1.OL_RelatedParty = "N";
			link1.OL_RN_NKImporterCountry = "US";
			link1.GetAddInfo().ZO_AESUltConsigneeType = UltimateConsigneeTypeList.Codes.DirectConsumer;

			var importer2 = Factory.New<OrgHeader>();
			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			var link2 = supplier2.BuyerLinks.AddNew(importer2);
			link2.OL_RelatedParty = "N";
			link2.OL_RN_NKImporterCountry = "US";
			link2.GetAddInfo().ZO_AESUltConsigneeType = UltimateConsigneeTypeList.Codes.Reseller;

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.JE_RL_NKFinalDestination = "NZAKL";
			dec.JE_OH_Importer = importer1.PK;
			var invoice = dec.Invoices.AddNew();

			AssertEquals(UltimateConsigneeTypeList.Codes.DirectConsumer, invoice.US_UltimateConsigneeType);
			invoice.JZ_OH_Supplier = supplier2.PK;
			invoice.JZ_OH_Buyer = importer2.PK;
			AssertEquals(UltimateConsigneeTypeList.Codes.Reseller, invoice.US_UltimateConsigneeType);
			invoice.US_UltimateConsigneeType = UltimateConsigneeTypeList.Codes.GovernmentEntity;
			dec.JE_OH_Importer = importer2.PK;
			AssertEquals(UltimateConsigneeTypeList.Codes.GovernmentEntity, invoice.US_UltimateConsigneeType);

			invoice.US_UltimateConsigneeType = ZString.Empty;
			link2.GetAddInfo().ZO_AESUltConsigneeType = ZString.Empty;
			invoice.JZ_OH_Supplier = supplier2.PK;
			invoice.JZ_OH_Buyer = importer2.PK;
			AssertEquals("Should be empty by default", ZString.Empty, invoice.US_UltimateConsigneeType);
		}

		public void TestIControllerIDProviderMembers()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			IControllerIDProvider provider = invoice;
			AssertEquals("ControllerID", ControllerIDs.Customs.JobDeclaration, provider.ControllerID);
			AssertEquals("BusinessObjectPK", dec.PK.ToGuid(), provider.BusinessObjectPK);

			var shipment = Factory.New<ForwardingShipment>();
			dec.JE_JS = shipment.PK;
			AssertEquals("ControllerID", ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, provider.ControllerID);
			AssertEquals("BusinessObjectPK", dec.PK.ToGuid(), provider.BusinessObjectPK);
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			ICusCodeDataTypeSupporter supporter = invoice;
			supporter.AssertType(typeof(RelatedDocument), CusCodeDataTypeList.Codes.RelatedDocument);
			supporter.AssertType(null, "ZZ!");

			var relatedDocument = invoice.RelatedDocuments.AddNew();
			relatedDocument.CY_Code = "A";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<CusCodeData>(relatedDocument.PK);
			AssertEquals(typeof(RelatedDocument), codeData.GetType());
		}

		public void TestDeleteInvoiceChangesInBondRelatedRecords()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = ZBool.True;
			declaration.US_EnableAII = ZBool.False;
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_JE = declaration.PK;
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_Tariff = "1010101010";
			var message = Factory.New<EDIMessage>();
			invoice.Messages.Add(message);

			var inBondRelatedRecords = declaration.InBondRelatedRecords;
			AssertEquals(1, inBondRelatedRecords.Count);
			AssertNotNull(inBondRelatedRecords.GetElementWrapping(invoice));
			invoice.Delete();
			AssertEquals(0, inBondRelatedRecords.Count);
		}

		public void TestFTZBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "M43289";

			var houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1.CU_BillNum = "H43289";

			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_BillNum = "H33289";

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_CU_RelatedHouseBill = houseBill1.PK;
			invoice1.InvoiceLines.AddNew();

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_CU_RelatedHouseBill = houseBill2.PK;
			invoice2.InvoiceLines.AddNew();

			AssertEquals(masterBill, invoice1.FTZBill);
			AssertEquals(masterBill, invoice2.FTZBill);
		}

		public void TestGetEffectiveCountryOfExportFromBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Germany;
			var bill = declaration.Bills.AddNew();
			bill.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Australia;
			var invoice = declaration.Invoices.AddNew();
			invoice.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.France;
			invoice.JZ_CU_RelatedHouseBill = bill.PK;
			AssertEquals("invoice level", Core.Constants.CountryCodes.France, invoice.US_UC_NKCountryOfExport);
			invoice.US_UC_NKCountryOfExport = ZString.Empty;
			AssertEquals("Bill level", Core.Constants.CountryCodes.Australia, invoice.US_UC_NKCountryOfExport);
			invoice.JZ_CU_RelatedHouseBill = ZGuid.Empty;
			AssertEquals("Dec level", Core.Constants.CountryCodes.Germany, invoice.US_UC_NKCountryOfExport);
		}

		public void TestInvoiceNumberFilledInIfRequired()
		{
			AssertInvoiceNumberFilledIn(JobMessageTypeList.Codes.Import, false);
			AssertInvoiceNumberFilledIn(JobMessageTypeList.Codes.Export, true);
			AssertInvoiceNumberFilledIn(JobMessageTypeList.Codes.Miscellaneous, false);
			AssertInvoiceNumberFilledIn(JobMessageTypeList.Codes.Recon, false);
			AssertInvoiceNumberFilledIn(JobMessageTypeList.Codes.Drawback, true);
		}

		public void TestAreChargesBalancedForInvoicesWhenADDGroupChargeIsUsedAndAMMVOnLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasFreight);

			invoiceLine.JI_LinePrice = 1000m;

			declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasFreight, 100m, JobDeclaration.LocalCurrencyConstantCode);
			declaration.ResumeApportionment();
			AssertEquals("Line and Invoice should have a charge apportioned", 100m, invoice.GroupCharges[0].J7_Amount);
			AssertEquals("Line and invoice should have a charge apportioned", 100m, invoiceLine.ApportionedCharges[0].J7_Amount);
			string message;
			AssertEquals("Apportionment is balanced", true, declaration.Invoices.AreChargesBalancedForInvoices(out message));

			declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew(USCustomsChargeTypeList.Codes.AdditionCharge, 250m, JobDeclaration.LocalCurrencyConstantCode);
			declaration.ResumeApportionment();
			AssertEquals("Line and Invoice should have ADD charge apportioned", 250m, invoice.GroupCharges[1].J7_Amount);
			AssertEquals("Line and invoice should have a charge apportioned", 250m, invoiceLine.ApportionedCharges[1].J7_Amount);
			AssertEquals("Apportionment is balanced", true, declaration.Invoices.AreChargesBalancedForInvoices(out message));

			invoiceLine.JI_InvoiceQuantity = 100m;
			invoiceLine.JI_InvoiceUQ = "NMB";
			invoiceLine.US_AMMVPerUnit = 3.7m;
			declaration.ResumeApportionment();
			AssertEquals("Line should have AMMV line level charge apportioned", 370m, invoiceLine.ApportionedCharges[2].J7_Amount);
			AssertEquals("Apportionment should still be balanced - AMMV at line level is system charge & should not affect balance checking", true, declaration.Invoices.AreChargesBalancedForInvoices(out message));
		}

		public void TestSoldToParty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			var party1 = Factory.New<OrgHeader>();
			var party2 = Factory.New<OrgHeader>();

			declaration.JE_OA_SoldToPartyAddress = party1.MainAddress.PK;
			AssertEquals(party1.MainAddress.PK, invoice.JZ_OA_SoldToPartyAddress);
			AssertEquals(party1.MainAddress.PK, invoiceLine.JI_OA_SoldToPartyAddress);

			invoice.JZ_OA_SoldToPartyAddress = party2.MainAddress.PK;
			AssertEquals(party2.MainAddress.PK, invoice.JZ_OA_SoldToPartyAddress);
			AssertEquals(party2.MainAddress.PK, invoiceLine.JI_OA_SoldToPartyAddress);

			invoiceLine.JI_OA_SoldToPartyAddress = party1.MainAddress.PK;
			AssertEquals(party2.MainAddress.PK, invoice.JZ_OA_SoldToPartyAddress);
			AssertEquals(party1.MainAddress.PK, invoiceLine.JI_OA_SoldToPartyAddress);

			invoice.JZ_OA_SoldToPartyAddress = party1.MainAddress.PK;
			AssertEquals(party1.MainAddress.PK, invoice.JZ_OA_SoldToPartyAddress);
			AssertEquals(party1.MainAddress.PK, invoiceLine.JI_OA_SoldToPartyAddress);

			declaration.JE_OA_SoldToPartyAddress = party2.MainAddress.PK;
			AssertEquals(party2.MainAddress.PK, invoice.JZ_OA_SoldToPartyAddress);
			AssertEquals(party2.MainAddress.PK, invoiceLine.JI_OA_SoldToPartyAddress);
		}

		public void TestTotalNonSecondaryEntryLinesCount()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = false;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine.AddSecondaryInvoiceLine();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0, invoice.TotalNonSecondaryEntrySummaryLinesCount);

			declaration.US_EnableENS = true;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, invoice.TotalNonSecondaryEntrySummaryLinesCount);
		}

		public void TestHasInvoiceLineWithPerishableCommodity()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			var commodity1 = Factory.New<RefCommodityCode>();
			commodity1.RH_Code = "AMYT";

			var commodity2 = Factory.New<RefCommodityCode>();
			commodity2.RH_Code = "AMYX";
			commodity2.RH_IsPerishable = false;

			invoiceLine1.JI_RH_NKCommodity_Code = commodity1.RH_Code;
			invoiceLine2.JI_RH_NKCommodity_Code = commodity1.RH_Code;

			AssertEquals(false, invoice.HasInvoiceLinesWithPerishableCommodity);

			commodity1.RH_IsPerishable = true;
			invoiceLine1.JI_RH_NKCommodity_Code = commodity1.RH_Code;
			AssertEquals(true, invoice.HasInvoiceLinesWithPerishableCommodity);
		}

		public void TestIsLineGroupingEnabled()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableAII = false;
			declaration.US_IsInvoiceByRequest = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			AssertEquals(false, invoice.IsLineGroupingEnabled);
			invoice.US_IsLineGrouping = true;
			AssertEquals(true, invoice.IsLineGroupingEnabled);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(false, invoice.IsLineGroupingEnabled);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(false, invoice.IsLineGroupingEnabled);
			declaration.US_IsInvoiceByRequest = true;
			AssertEquals(true, invoice.IsLineGroupingEnabled);
			declaration.US_IsInvoiceByRequest = false;
			declaration.US_EnableAII = true;
			AssertEquals(true, invoice.IsLineGroupingEnabled);
		}

		public void TestIRegistryAccessingSupporterMembers()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			AssertEquals(true, invoice.IsAttachedToPersistentDeclaration);
			declaration.JE_GB = ZGuid.Empty;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), invoice.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), invoice.RegistryBranchPK);

			declaration.JE_GB = branch.PK;
			AssertEquals(company.PK.ToGuid(), invoice.RegistryCompanyPK);
			AssertEquals(branch.PK.ToGuid(), invoice.RegistryBranchPK);

			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), invoice.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), invoice.RegistryBranchPK);

			invoice.JZ_JE = ZGuid.Empty;
			AssertEquals(false, invoice.IsAttachedToPersistentDeclaration);
			invoice.JZ_GB = ZGuid.Empty;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), invoice.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), invoice.RegistryBranchPK);

			invoice.JZ_GB = branch.PK;
			AssertEquals(company.PK.ToGuid(), invoice.RegistryCompanyPK);
			AssertEquals(branch.PK.ToGuid(), invoice.RegistryBranchPK);

			invoice.JZ_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), invoice.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), invoice.RegistryBranchPK);
		}

		public void TestHasFDAToDeclare()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(false, invoice.HasFDAToDeclare);

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(false, invoice.HasFDAToDeclare);
			AssertEquals(true, invoice2.HasFDAToDeclare);
		}

		public void TestIncotermWorksCorrectlyForExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			AssertEquals("PreCondition:CFR is valid", true, invoice.Lookups.JZ_IncoTerm_List.ContainsCode(invoice.JZ_IncoTerm));

			InvoiceCharge charge = invoice.Charges.AddNew("OFT", 10m, "AUD");
			AssertEquals("IsIncludedInLines should be read/write", false, charge.J7_IsIncludedInITOTInfo.ReadOnly);
			AssertEquals(true, charge.J7_Calc_IsIncludedInInvoiceAmount);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.CAF;
			AssertEquals("IsIncludedInLines should be read/write", false, charge.J7_IsIncludedInITOTInfo.ReadOnly);
			AssertEquals(true, charge.J7_Calc_IsIncludedInInvoiceAmount);
		}

		public void TestUS_ZoneStatus()
		{
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			AssertEquals(ZoneStatusList.Codes.Domestic, invoiceLine.US_ZoneStatus);

			bool invoiceLineUS_ZoneStatusChangeCalled = false;
			invoiceLine.US_ZoneStatusInfo.ValueChanged += delegate
			{ invoiceLineUS_ZoneStatusChangeCalled = true; };

			invoiceHeader.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			Assert("invoiceLine.US_ZoneStatus should be refreshed when invoiceHeader.US_ZoneStatus the same as invoiceLine.US_ZoneStatus", invoiceLineUS_ZoneStatusChangeCalled);

			invoiceHeader.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			AssertEquals(ZoneStatusList.Codes.PrivilegedForeign, invoiceLine.US_ZoneStatus);
		}

		public void TestDefaultDataOnAttachingToADeclaration()
		{
			org1.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = Core.Constants.CountryCodes.Singapore;
			org1.MiscServ.OM_EXDefaultIncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			org2.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = Core.Constants.CountryCodes.Malaysia;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = org1.PK;
			declaration.JE_OA_ManufacturerAddress = org2.MainAddress.PK;

			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			AssertEquals(ZString.Empty, invoice.US_UC_NKCountryOfExport);
			AssertEquals(ZString.Empty, invoice.US_UC_NKCountryOfOrigin);

			invoice.JZ_JE = declaration.PK;
			AssertEquals(Core.Constants.IncoTerms.CostInsuranceAndFreight, invoice.JZ_IncoTerm);
			AssertEquals(ZString.Empty, invoice.US_UC_NKCountryOfExport);
			AssertEquals(Core.Constants.CountryCodes.Malaysia, invoice.US_UC_NKCountryOfOrigin);
		}

		public void TestDeletingPGADataOnAttachingToADeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZString.Empty;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals(false, declaration.IsACECargoCertificationMode);
			var invoice = Factory.New<JobComInvoiceHeader>();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var dot = invoiceLine.DOTs.AddNew();
			var atf = invoiceLine.ATFLines.AddNew();

			invoice.JZ_JE = declaration.PK;
			AssertEquals(1, invoiceLine.DOTs.Count);
			AssertEquals(0, invoiceLine.ATFLines.Count);

			invoice.JZ_JE = ZGuid.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			AssertEquals(false, declaration.IsACECargoCertificationMode);
			AssertEquals(false, dot.IsDeleted);
			AssertEquals(true, atf.IsDeleted);

			invoice.JZ_JE = declaration.PK;
			AssertEquals(1, invoiceLine.DOTs.Count);
			AssertEquals(false, dot.IsDeleted);
			AssertEquals(0, invoiceLine.ATFLines.Count);
			AssertEquals(true, atf.IsDeleted);

			invoice.JZ_JE = ZGuid.Empty;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			AssertEquals(true, declaration.IsACECargoCertificationMode);
			atf = invoiceLine.ATFLines.AddNew();
			AssertEquals(false, dot.IsDeleted);
			AssertEquals(false, atf.IsDeleted);
			invoice.JZ_JE = declaration.PK;
			AssertEquals(0, invoiceLine.DOTs.Count);
			AssertEquals(true, dot.IsDeleted);
			AssertEquals(1, invoiceLine.ATFLines.Count);
			AssertEquals(false, atf.IsDeleted);
		}

		public void TestDefaultDataFromSupplier()
		{
			org1.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = Core.Constants.CountryCodes.Singapore;
			org1.MiscServ.OM_EXDefaultIncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;

			org2.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = Core.Constants.CountryCodes.Malaysia;
			org2.MiscServ.OM_EXDefaultIncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;

			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice.US_UC_NKCountryOfExport = ZString.Empty;
			invoice.JZ_OH_Supplier = org1.PK;
			AssertEquals(Core.Constants.IncoTerms.CostInsuranceAndFreight, invoice.JZ_IncoTerm);
			AssertEquals(ZString.Empty, invoice.US_UC_NKCountryOfExport);
			AssertEquals(Core.Constants.CountryCodes.Singapore, invoice.US_UC_NKCountryOfOrigin);

			invoice.JZ_OH_Supplier = org2.PK;
			AssertEquals(Core.Constants.IncoTerms.DeliveredDutyPaid, invoice.JZ_IncoTerm);
			AssertEquals(ZString.Empty, invoice.US_UC_NKCountryOfExport);
			AssertEquals(Core.Constants.CountryCodes.Malaysia, invoice.US_UC_NKCountryOfOrigin);

			invoice.JZ_OA_ManufacturerAddress = org1.MainAddress.PK;
			invoice.JZ_OH_Supplier = org1.PK;
			AssertEquals(Core.Constants.IncoTerms.CostInsuranceAndFreight, invoice.JZ_IncoTerm);
			AssertEquals(ZString.Empty, invoice.US_UC_NKCountryOfExport);
			AssertEquals(Core.Constants.CountryCodes.Singapore, invoice.US_UC_NKCountryOfOrigin);
		}

		public void TestDefaultDataFromManufacturer()
		{
			org1.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = Core.Constants.CountryCodes.Singapore;
			org2.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = Core.Constants.CountryCodes.Malaysia;

			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			invoice.ManufacturerOrgPK = org1.PK;
			AssertEquals(Core.Constants.CountryCodes.Singapore, invoice.US_UC_NKCountryOfOrigin);

			invoice.ManufacturerOrgPK = org2.PK;
			AssertEquals(Core.Constants.CountryCodes.Malaysia, invoice.US_UC_NKCountryOfOrigin);
		}

		public void TestUSPPIIsDefaultedFromDeclarationPickupDetails()
		{
			org1 = CreateNewOrg("ORGANISATION 1", "ORG1 ADDRESS", "BOB SMITH", "+61 (2) 9840 4564");
			org2 = CreateNewOrg("ORGANISATION 2", "ORG2 ADDRESS", "JOE BROWN", "+61 (3) 3659 5874");
			var org2Address2 = org2.Addresses.AddNew();
			org2Address2.OA_Address1 = "ORG2 ADDRESS 2";
			var org2Contact2 = org2.Contacts.AddNew();
			org2Contact2.OC_ContactName = "JANE DO";
			org2Contact2.OC_Phone = "+61 (4) 4569 3287";

			var invoice = this.Declaration.Invoices.AddNew();
			this.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var uS_USPPI = invoice.US_USPPI;
			uS_USPPI.ZO_OH_Organisation = org1.PK;
			AssertEquals("US_USPPI.ZO_OH_Organisation", org1.PK, uS_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OA_Address", org1.MainAddress.PK, uS_USPPI.ZO_OA_Address);
			AssertEquals("US_USPPI.ZO_Contact", "BOB SMITH", uS_USPPI.ZO_Contact);
			AssertEquals("US_USPPI.ZO_Phone", "61298404564", uS_USPPI.ZO_Phone);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = org2.PK;

			org2.OH_RL_NKClosestPort = "US2CW";
			uS_USPPI.ZO_OA_Address = ZGuid.Empty;
			invoice.JZ_JE = declaration.PK;
			uS_USPPI = invoice.US_USPPI;
			AssertEquals("US_USPPI.ZO_OH_Organisation", org2.PK, uS_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OA_Address", org2.MainAddress.PK, uS_USPPI.ZO_OA_Address);
			AssertEquals("US_USPPI.ZO_Contact", "JOE BROWN", uS_USPPI.ZO_Contact);
			AssertEquals("US_USPPI.ZO_Contact", "336595874", uS_USPPI.ZO_Phone);

			declaration.SupplierPickupAddress.E2_OA_Address = org2Address2.PK;
			declaration.SupplierPickupAddress.ContactPK = org2Contact2.PK;
			invoice.USPPIDocAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals(org2Address2.PK, invoice.USPPIDocAddress.E2_OA_Address);
			AssertEquals("US_USPPI.ZO_OH_Organisation", org2.PK, uS_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OA_Address", org2Address2.PK, uS_USPPI.ZO_OA_Address);
			AssertEquals("US_USPPI.ZO_Contact", "JANE DO", uS_USPPI.ZO_Contact);
			AssertEquals("US_USPPI.ZO_Contact", "445693287", uS_USPPI.ZO_Phone);

			uS_USPPI.ZO_OH_Organisation = org1.PK;
			AssertEquals("US_USPPI.ZO_OH_Organisation", org1.PK, uS_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OA_Address", org1.MainAddress.PK, uS_USPPI.ZO_OA_Address);
			AssertEquals("US_USPPI.ZO_Contact", "BOB SMITH", uS_USPPI.ZO_Contact);
			AssertEquals("US_USPPI.ZO_Contact", "61298404564", uS_USPPI.ZO_Phone);

			uS_USPPI.ZO_OH_Organisation = org2.PK;
			AssertEquals("US_USPPI.ZO_OH_Organisation", org2.PK, uS_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OA_Address", org2.MainAddress.PK, uS_USPPI.ZO_OA_Address);
			AssertEquals("US_USPPI.ZO_Contact", "JANE DO", uS_USPPI.ZO_Contact);
			AssertEquals("US_USPPI.ZO_Contact", "445693287", uS_USPPI.ZO_Phone);

			uS_USPPI.ZO_OA_Address = ZGuid.Empty;
			AssertEquals("US_USPPI.ZO_OH_Organisation", org2.PK, uS_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OA_Address", org2Address2.PK, uS_USPPI.ZO_OA_Address);
			AssertEquals("US_USPPI.ZO_Contact", "JANE DO", uS_USPPI.ZO_Contact);
			AssertEquals("US_USPPI.ZO_Contact", "445693287", uS_USPPI.ZO_Phone);

			declaration.JE_OH_Supplier = org1.PK;
			AssertEquals("US_USPPI.ZO_OH_Organisation", org1.PK, uS_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OA_Address", org1.MainAddress.PK, uS_USPPI.ZO_OA_Address);
			AssertEquals("US_USPPI.ZO_Contact", "BOB SMITH", uS_USPPI.ZO_Contact);
			AssertEquals("US_USPPI.ZO_Contact", "61298404564", uS_USPPI.ZO_Phone);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsignorPK = org2.PK;
			shipment.ConsignorPickupAddress.E2_OA_Address = org2Address2.PK;

			declaration.SupplierPickupAddress.E2_OA_Address = org1.MainAddress.PK;
			declaration.JE_JS = shipment.PK;
			ErrorReporter.Clear();
			uS_USPPI = invoice.US_USPPI;
			AssertEquals("US_USPPI.ZO_OH_Organisation", org1.PK, uS_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OA_Address", org1.MainAddress.PK, uS_USPPI.ZO_OA_Address);
			AssertEquals("US_USPPI.ZO_Contact", "BOB SMITH", uS_USPPI.ZO_Contact);
			AssertEquals("US_USPPI.ZO_Contact", "61298404564", uS_USPPI.ZO_Phone);

			uS_USPPI.ZO_OH_Organisation = org2.PK;
			AssertEquals("US_USPPI.ZO_OH_Organisation", org2.PK, uS_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OA_Address", org2.MainAddress.PK, uS_USPPI.ZO_OA_Address);
			AssertEquals("US_USPPI.ZO_Contact", "JOE BROWN", uS_USPPI.ZO_Contact);
			AssertEquals("US_USPPI.ZO_Contact", "336595874", uS_USPPI.ZO_Phone);

			uS_USPPI.ZO_OH_Organisation = ZGuid.Empty;
			shipment.ConsignorPickupAddress.ContactPK = org2Contact2.PK;
			uS_USPPI.ZO_OH_Organisation = org2.PK;
			AssertEquals("US_USPPI.ZO_OH_Organisation", org2.PK, uS_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OA_Address", org2.MainAddress.PK, uS_USPPI.ZO_OA_Address);
			AssertEquals("US_USPPI.ZO_Contact", "JANE DO", uS_USPPI.ZO_Contact);
			AssertEquals("US_USPPI.ZO_Contact", "445693287", uS_USPPI.ZO_Phone);
		}

		public void TestNoErrorWhenCopyWithUSPPIZO_PhoneExist()
		{
			var org1 = CreateNewOrg("ORGANISATION 1", "ORG1 ADDRESS", "BOB SMITH", "+1 215-555-12");
			var org2 = CreateNewOrg("ORGANISATION 2", "ORG2 ADDRESS", "JOE BROWN", "+61 (3) 3659 5874");
			org1.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			org2.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = org1.PK;
			declaration.JE_OH_Importer = org2.PK;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			var invoice = declaration.Invoices.AddNew();
			var usppiDocAddress = invoice.USPPIDocAddress;
			string errorMessage = "The USPPI Contact Phone Number must be ten numeric digits in length.";

			usppiDocAddress.Validation.ValidateAll();
			AssertHasMessageErrorContaining(usppiDocAddress.E2_ContactInfo, errorMessage);
			((OrgContact)org1.Contacts.FirstOrDefault()).OC_Phone = "+1 215-555-1212";
			invoice.US_USPPI.ZO_Phone = "+1 215-555-1212";
			usppiDocAddress.Validation.ValidateAll();
			AssertNoMessageErrorContaining(usppiDocAddress.E2_ContactInfo, errorMessage);

			var clonedDec = (JobDeclaration)declaration.TemplateCopy();
			var clonedInvoice = (JobComInvoiceHeader)clonedDec.Invoices.FirstOrDefault();
			usppiDocAddress = clonedInvoice.USPPIDocAddress;

			usppiDocAddress.Validation.ValidateAll();
			AssertNoMessageErrorContaining(usppiDocAddress.E2_ContactInfo, errorMessage);
		}

		public void TestUS_PrivilegedStatusDate()
		{
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.US_PrivilegedStatusDate = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, invoiceLine.US_PrivilegedStatusDate);

			bool invoiceLineUS_PrivilegedStatusDateChangeCalled = false;
			invoiceLine.US_PrivilegedStatusDateInfo.ValueChanged += delegate
			{ invoiceLineUS_PrivilegedStatusDateChangeCalled = true; };

			invoiceHeader.US_PrivilegedStatusDate = ZDateTime.Today;
			Assert("invoiceLine.US_PrivilegedStatusDate should be refreshed when invoiceHeader.US_PrivilegedStatusDate the same as invoiceLine.US_PrivilegedStatusDate", invoiceLineUS_PrivilegedStatusDateChangeCalled);

			invoiceHeader.US_PrivilegedStatusDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(ZDateTime.Today.AddDays(-1), invoiceLine.US_PrivilegedStatusDate);
		}

		public void TestPrivilegedStatusDateVisible()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			for (int i = 0; i < invoice.AddInfoLookups.US_ZoneStatusList.Count; i++)
			{
				invoice.US_ZoneStatus = invoice.AddInfoLookups.US_ZoneStatusList[i].Code;
				if (invoice.US_ZoneStatus == ZoneStatusList.Codes.PrivilegedForeign)
				{
					Assert("PrivilegedStatusDateVisible should be true when invoiceLine.US_ZoneStatus == ZoneStatusList.Codes.PrivilegedForeign", invoice.PrivilegedStatusDateVisible);
				}
				else
				{
					Assert("PrivilegedStatusDateVisible should NOT be true when invoiceLine.US_ZoneStatus != ZoneStatusList.Codes.PrivilegedForeign", !invoice.PrivilegedStatusDateVisible);
				}
			}
		}

		public void TestJZ_OH_SupplierIsSavedForReportForImportDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "OH" + new Random().Next(1000000).ToString();
			org.OH_FullName = "DUMMY ORG FOR TESTING";
			org.MainAddress.OA_Address1 = "ADDRESS 1";
			invoice.JZ_OA_SupplierAddress_ZAddress.OrgPK = org.PK;
			AssertEquals("Supplier", org.PK, invoice.JZ_OH_Supplier);
			invoice.JZ_OH_Supplier = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, invoice.JZ_OA_SupplierAddress_ZAddress.OrgPK);
			invoice.JZ_OA_SupplierAddress_ZAddress.OrgPK = org.PK;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var invoices = new DynamicBusinessObjectCollection(newFactory);
			string dynamicSQL = string.Format("SELECT {0} FROM {1} WHERE {2} = '{3}'", JobComInvoiceHeader.Schema.JZ_OH_Supplier, JobComInvoiceHeader.Schema.TableName, JobComInvoiceHeader.Schema.PK, invoice.PK);
			invoices.Load(dynamicSQL);
			AssertEquals(1, invoices.Count);
			AssertEquals(org.PK, invoices[0][JobComInvoiceHeader.Schema.JZ_OH_Supplier]);

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "OH1" + new Random().Next(1000000).ToString();
			org1.OH_FullName = "DUMMY ORG FOR TESTING";
			org1.MainAddress.OA_Address1 = "ADDRESS 1";
			invoice.JZ_OA_SupplierAddress = org1.MainAddress.PK;
			Factory.Save();
			invoices.Load(dynamicSQL);
			AssertEquals(1, invoices.Count);
			AssertEquals(org1.PK, invoices[0][JobComInvoiceHeader.Schema.JZ_OH_Supplier]);

			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.SupplierPickupAddress.E2_OA_Address = ZGuid.Empty;
			invoice.JZ_OH_Supplier = ZGuid.Empty;
			invoice.JZ_OA_SupplierAddress = ZGuid.Empty;
			var row = ((IBusinessObjectInternals)invoice).Row;
			row[JobComInvoiceHeader.Schema.JZ_OH_Supplier] = DBNull.Value;
			Factory.Save();
			row = ((IBusinessObjectInternals)declaration).Row;
			row[JobDeclaration.Schema.JE_OH_Supplier] = org.PK.ToGuid();
			declaration.HasChanges = true;
			Factory.Save();
			invoices.Load(dynamicSQL);
			AssertEquals(1, invoices.Count);
			AssertEquals(ZGuid.Empty, invoices[0][JobComInvoiceHeader.Schema.JZ_OH_Supplier]);

			newFactory = new BusinessObjectFactory();
			var invoiceInDiffFactory = newFactory.Load<JobComInvoiceHeader>(invoice.PK);
			row = ((IBusinessObjectInternals)invoiceInDiffFactory).Row;
			AssertEquals(DBNull.Value, row[JobComInvoiceHeader.Schema.JZ_OH_Supplier]);
			AssertEquals(org.PK, invoiceInDiffFactory.JZ_OH_Supplier);
			AssertEquals(org.MainAddress.PK, invoiceInDiffFactory.JZ_OA_SupplierAddress);
			AssertEquals(DBNull.Value, row[JobComInvoiceHeader.Schema.JZ_OH_Supplier]);
			AssertEquals("Haschanges", false, invoiceInDiffFactory.HasChanges);
		}

		public void TestJZ_OH_SupplierIsSavedForReportForExportDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "OH" + new Random().Next(1000000).ToString();
			org.OH_FullName = "DUMMY ORG FOR TESTING";
			org.MainAddress.OA_Address1 = "ADDRESS 1";
			org.OH_RL_NKClosestPort = "USCHI";
			invoice.USPPIDocAddress.OrganisationPK = org.PK;
			AssertEquals("Supplier", org.PK, invoice.JZ_OH_Supplier);
			invoice.JZ_OH_Supplier = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, invoice.USPPIDocAddress.OrganisationPK);
			invoice.USPPIDocAddress.OrganisationPK = org.PK;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var invoices = new DynamicBusinessObjectCollection(newFactory);
			string dynamicSQL = string.Format("SELECT {0} FROM {1} WHERE {2} = '{3}'", JobComInvoiceHeader.Schema.JZ_OH_Supplier, JobComInvoiceHeader.Schema.TableName, JobComInvoiceHeader.Schema.PK, invoice.PK);
			invoices.Load(dynamicSQL);
			AssertEquals(1, invoices.Count);
			AssertEquals(org.PK, invoices[0][JobComInvoiceHeader.Schema.JZ_OH_Supplier]);

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "OH1" + new Random().Next(1000000).ToString();
			org1.OH_FullName = "DUMMY ORG FOR TESTING";
			org1.MainAddress.OA_Address1 = "ADDRESS 1";
			invoice.USPPIDocAddress.E2_OA_Address = org1.MainAddress.PK;
			Factory.Save();
			invoices.Load(dynamicSQL);
			AssertEquals(1, invoices.Count);
			AssertEquals(org1.PK, invoices[0][JobComInvoiceHeader.Schema.JZ_OH_Supplier]);

			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.SupplierPickupAddress.E2_OA_Address = ZGuid.Empty;
			invoice.JZ_OH_Supplier = ZGuid.Empty;
			invoice.USPPIDocAddress.E2_OA_Address = ZGuid.Empty;
			var row = ((IBusinessObjectInternals)invoice).Row;
			row[JobComInvoiceHeader.Schema.JZ_OH_Supplier] = DBNull.Value;
			Factory.Save();
			row = ((IBusinessObjectInternals)declaration).Row;
			row[JobDeclaration.Schema.JE_OH_Supplier] = org.PK.ToGuid();
			declaration.HasChanges = true;
			Factory.Save();
			invoices.Load(dynamicSQL);
			AssertEquals(1, invoices.Count);
			AssertEquals(ZGuid.Empty, invoices[0][JobComInvoiceHeader.Schema.JZ_OH_Supplier]);

			newFactory = new BusinessObjectFactory();
			var invoiceInDiffFactory = newFactory.Load<JobComInvoiceHeader>(invoice.PK);
			row = ((IBusinessObjectInternals)invoiceInDiffFactory).Row;
			AssertEquals(DBNull.Value, row[JobComInvoiceHeader.Schema.JZ_OH_Supplier]);
			AssertEquals(org.PK, invoiceInDiffFactory.JZ_OH_Supplier);
			AssertEquals(org.MainAddress.PK, invoiceInDiffFactory.USPPIDocAddress.E2_OA_Address);
			AssertEquals(DBNull.Value, row[JobComInvoiceHeader.Schema.JZ_OH_Supplier]);
		}

		public void TestUS_TransactionsRelated()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKFinalDestination = "USLAX";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = ZGuid.Empty;
			AssertEquals("Related Transaction", "", invoice.US_TransactionsRelated);

			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_RL_NKClosestPort = "CATOR";
			OrgHeader supplier = Factory.New<OrgHeader>();
			OrgSupplierBuyerLink link1 = supplier.BuyerLinks.AddNew(importer);
			link1.OL_RelatedParty = "Y";
			link1.OL_RN_NKImporterCountry = "CA";
			OrgSupplierBuyerLink link2 = supplier.BuyerLinks.AddNew(importer);
			link2.OL_RelatedParty = "N";
			link2.OL_RN_NKImporterCountry = "US";

			invoice.JZ_OH_Buyer = importer.PK;
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertEquals("Related Transaction should be defaulted from the link2 - where OL_RN_NKImporterCountry is US",
				"N", invoice.US_TransactionsRelated);

			OrgHeader supplier2 = Factory.New<OrgHeader>();
			invoice.JZ_OH_Supplier = supplier2.PK;
			AssertEquals("Related Transaction", "", invoice.US_TransactionsRelated);

			invoice.JZ_OA_SupplierAddress = supplier.MainAddress.PK;
			AssertEquals("Related Transaction should be defaulted from the link2 - where OL_RN_NKImporterCountry is US", "", invoice.US_TransactionsRelated);

			invoice.JZ_OH_Buyer = ZGuid.Empty;
			AssertEquals("Related Transaction", "", invoice.US_TransactionsRelated);

			invoice.JZ_OH_Buyer = importer.PK;
			AssertEquals("Related Transaction should be defaulted from the link2 - where OL_RN_NKImporterCountry is US",
				"N", invoice.US_TransactionsRelated);
		}

		public void TestExportFieldsAreReadonly()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = ZGuid.Empty;
			invoice.JZ_OH_Buyer = ZGuid.Empty;
			invoice.JZ_OH_Consignee = ZGuid.Empty;

			AssertEquals(false, invoice.JZ_OH_SupplierInfo.ReadOnly);
			AssertEquals(true, invoice.USPPIDocAddress.E2_ContactInfo.ReadOnly);
			AssertEquals(true, invoice.USPPIDocAddress.E2_Phone_FormattedInfo.ReadOnly);
			invoice.USPPIDocAddress.E2_AddressOverride = true;
			AssertEquals(false, invoice.USPPIDocAddress.E2_ContactInfo.ReadOnly);
			AssertEquals(false, invoice.USPPIDocAddress.E2_Phone_FormattedInfo.ReadOnly);

			AssertEquals(false, invoice.JZ_OH_BuyerInfo.ReadOnly);
			AssertEquals(true, invoice.UltimateConsigneeDocAddress.E2_ContactInfo.ReadOnly);
			AssertEquals(true, invoice.UltimateConsigneeDocAddress.E2_Phone_FormattedInfo.ReadOnly);
			invoice.UltimateConsigneeDocAddress.E2_AddressOverride = true;
			AssertEquals(false, invoice.UltimateConsigneeDocAddress.E2_ContactInfo.ReadOnly);
			AssertEquals(false, invoice.UltimateConsigneeDocAddress.E2_Phone_FormattedInfo.ReadOnly);

			AssertEquals(false, invoice.JZ_OH_ConsigneeInfo.ReadOnly);
			AssertEquals(true, invoice.IntermediateConsigneeDocAddress.E2_ContactInfo.ReadOnly);
			AssertEquals(true, invoice.IntermediateConsigneeDocAddress.E2_Phone_FormattedInfo.ReadOnly);
			invoice.IntermediateConsigneeDocAddress.E2_AddressOverride = true;
			AssertEquals(false, invoice.IntermediateConsigneeDocAddress.E2_ContactInfo.ReadOnly);
			AssertEquals(false, invoice.IntermediateConsigneeDocAddress.E2_Phone_FormattedInfo.ReadOnly);
		}

		public void TestSupplierAddressFuction()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "OH1" + new Random().Next(1000000).ToString();
			org1.OH_FullName = "DUMMY ORG FOR TESTING";
			org1.MainAddress.OA_Address1 = "ADDRESS 1";
			OrgAddress org1Address1 = org1.Addresses.AddNew();
			org1Address1.OA_Address1 = "Address 1";

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OH2" + new Random().Next(1000000).ToString();
			org2.OH_FullName = "DUMMY ORG FOR TESTING";
			org2.MainAddress.OA_Address1 = "ADDRESS 1";
			OrgAddress org2Address1 = org2.Addresses.AddNew();
			org2Address1.OA_Address1 = "Address 1";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = org1.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = ZString.Empty;
			invoice.JZ_IncoTermInfo.ValueChanged += new EventHandler((object sender, EventArgs e) => { ZGuid oldAddress = invoice.JZ_OA_SupplierAddress; }); // simulate same behaviour as on the form... changing the supplier, will cause the IncoTerm to be changed and hence the JZ_OA_SupplierAddress is called
			AssertEquals(org1.PK, invoice.JZ_OA_SupplierAddress_ZAddress.OrgPK);
			AssertEquals(org1.PK, invoice.JZ_OH_Supplier);

			invoice.JZ_OH_Supplier = org2.PK;
			AssertEquals(org2.MainAddress.PK, invoice.JZ_OA_SupplierAddress);
			AssertEquals(org2.PK, invoice.JZ_OA_SupplierAddress_ZAddress.OrgPK);
			AssertEquals(org2.PK, invoice.JZ_OH_Supplier);

			invoice.JZ_OA_SupplierAddress = org1Address1.PK;
			AssertEquals(org1.PK, invoice.JZ_OA_SupplierAddress_ZAddress.OrgPK);
			AssertEquals(org1.PK, invoice.JZ_OH_Supplier);

			invoice.JZ_OA_SupplierAddress = org2Address1.PK;
			AssertEquals(org2.PK, invoice.JZ_OA_SupplierAddress_ZAddress.OrgPK);
			AssertEquals(org2.PK, invoice.JZ_OH_Supplier);

			invoice.JZ_OH_Supplier = ZGuid.Empty;
			AssertEquals(org1.PK, invoice.JZ_OA_SupplierAddress_ZAddress.OrgPK);
			AssertEquals(org1.PK, invoice.JZ_OH_Supplier);
		}

		public void TestReconOriginalEntry()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			ReconOriginalEntryHeader originalEntry1 = reconDeclaration.OriginalEntries.AddNew();

			ReconOriginalEntryHeader originalEntry2 = reconDeclaration.OriginalEntries.AddNew();

			JobComInvoiceHeader invoice = reconDeclaration.Invoices.AddNew();
			AssertNull(invoice.ReconOriginalEntry);

			invoice.US_CH_ReconEntry = originalEntry2.CH_PK;
			AssertEquals("ReconOriginalEntry", originalEntry2, invoice.ReconOriginalEntry);
		}

		public void TestUS_OA_UltimateConsignee()
		{
			var ultimateConsignee = Factory.New<OrgHeader>();
			var ultiOrgAddressPK = ultimateConsignee.MainAddress.PK;

			var ultimateConsignee2 = Factory.New<OrgHeader>();
			var ultiOrgAddress2PK = ultimateConsignee2.MainAddress.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.JE_OA_ConsigneeAddress = ultiOrgAddressPK;

			var invoice = declaration.Invoices.AddNew();
			AssertEquals("should return an effective value", ultiOrgAddressPK, invoice.JZ_OA_ConsigneeAddress);

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_OA_ConsigneeAddress = ultiOrgAddress2PK;
			AssertEquals(ultiOrgAddress2PK, invoiceLine.JI_OA_ConsigneeAddress);

			invoice.JZ_OA_ConsigneeAddress = ultiOrgAddress2PK;
			AssertEquals("invoice line still retains the value", ultiOrgAddress2PK, invoiceLine.JI_OA_ConsigneeAddress);

			invoice.JZ_OA_ConsigneeAddress = ultiOrgAddressPK;
			AssertEquals("invoice line returns a new value", ultiOrgAddressPK, invoiceLine.JI_OA_ConsigneeAddress);
		}

		public void TestJZ_OA_ShipToPartyAddress()
		{
			var shipToParty = Factory.New<OrgHeader>();
			var shipToPartyPK = shipToParty.MainAddress.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.JE_OA_ShipToPartyAddress = shipToPartyPK;
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("should return an effective value", shipToPartyPK, invoice.JZ_OA_ShipToPartyAddress);

			DataRegistry.Business.USCustomsDataRegistry.Instance.DoDefaultShipTo.SetValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, true);
			declaration.JE_OA_ShipToPartyAddress = ZGuid.Empty;
			var ultimateConsignee = Factory.New<OrgHeader>();
			var ultiOrgAddressPK = ultimateConsignee.MainAddress.PK;
			invoice.JZ_OA_ConsigneeAddress = ultiOrgAddressPK;
			AssertEquals("should default from Ultimate Consignee", ultiOrgAddressPK, invoice.JZ_OA_ShipToPartyAddress);
			declaration.JE_OA_ShipToPartyAddress = shipToPartyPK;
			invoice.JZ_OA_ConsigneeAddress = ZGuid.Empty;
			invoice.JZ_OA_ConsigneeAddress = ultiOrgAddressPK;
			AssertEquals("should override from Ultimate Consignee", ultiOrgAddressPK, invoice.JZ_OA_ShipToPartyAddress);

			DataRegistry.Business.USCustomsDataRegistry.Instance.DoDefaultShipTo.SetValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, false);
			declaration.JE_OA_ShipToPartyAddress = ZGuid.Empty;
			invoice.JZ_OA_ShipToPartyAddress = ZGuid.Empty;
			invoice.JZ_OA_ConsigneeAddress = ultiOrgAddressPK;
			Assert("should not default from Ultimate Consignee", !invoice.JZ_OA_ShipToPartyAddress.IsValid);
		}

		public void TestEffectiveValudationDate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ExportDate = new ZDateTime(2008, 1, 1);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			AssertEquals("EffectiveValuationDate for import", new ZDateTime(2008, 1, 1), invoice.EffectiveValuationDate);

			invoice.US_DateOfExport = new ZDateTime(2008, 1, 2);
			AssertEquals("EffectiveValuationDate for import", new ZDateTime(2008, 1, 2), invoice.EffectiveValuationDate);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("EffectiveValuationDate for export", new ZDateTime(2008, 1, 1), invoice.EffectiveValuationDate);
		}

		public void TestHasAnEntryWithACertifiedCargoRelease()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			CusEntryHeader entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry1.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.CertificationSentAckPending;
			CusEntryLine entryLine1 = entry1.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			CusEntryHeader entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry2.US_CRLCertStatus = "";
			CusEntryLine entryLine2 = entry2.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			AssertEquals(false, invoice.HasAnEntryWithACertifiedCargoRelease);
			entry2.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;
			AssertEquals(true, invoice.HasAnEntryWithACertifiedCargoRelease);
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			AssertEquals(false, invoice.HasAnEntryWithACertifiedCargoRelease);
		}

		public void TestDefaultValueForFakeDeclaration()
		{
			using (USCustomsDataRegistry.Instance.DoDefaultImporterOfRecord.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var helper = new DeclarationTestHelper(Factory);
				var invoice = Factory.New<JobComInvoiceHeader>();
				var declaration = (JobDeclaration)new FakeDeclarationCreatorForInvoice(invoice).HeaderData;
				invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
				invoice.JZ_OH_Supplier =  helper.Consignee.PK;
				invoice.JZ_OH_Buyer = helper.Consignor.PK;
				AssertEquals(helper.Consignor.PK, declaration.IOROrgPK);
				Factory.Save();

				var invoice1 = Factory.Load<JobComInvoiceHeader>(invoice.PK);
				var declaration1 = (JobDeclaration)new FakeDeclarationCreatorForInvoice(invoice1).HeaderData;
				AssertEquals(helper.Consignor.PK, declaration1.IOROrgPK);
			}
		}

		public void TestOGAAndPGADataIsNotDeletedOnSavingStandaloneInvoice()
		{
			var helper = new DeclarationTestHelper(Factory);
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";

			product.RelatedOrganisations.AddOwner(helper.Consignee);
			product.RelatedOrganisations.AddSupplier(helper.Consignor);

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;

			pivot.CD_ACEFDAIndicator = OGAIndicatorList.Codes.Declared;
			var aceFDA = pivot.ACEFDAs.AddNew();
			aceFDA.US_Description = "PGA FDA DESC";

			var invoice = Factory.New<JobComInvoiceHeader>();
			var declaration = (JobDeclaration)new FakeDeclarationCreatorForInvoice(invoice).HeaderData;
			invoice.JZ_OH_Buyer = helper.Consignee.PK;
			invoice.JZ_OH_Supplier = helper.Consignor.PK;
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "Test";
			invoiceLine.JI_InvoiceQuantity = 1000m;
			invoiceLine.JI_InvoiceUQ = "KG";
			AssertEquals("invoiceLine.FDAs.Count", 0, invoiceLine.FDAs.Count);
			AssertEquals("invoiceLine.ACE_FDALines.Count", 1, invoiceLine.ACE_FDALines.Count);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			AssertEquals("invoiceLine.FDAs.Count", 0, invoiceLine.FDAs.Count);
			AssertEquals("invoiceLine.ACE_FDALines.Count", 1, invoiceLine.ACE_FDALines.Count);

			Factory.Save();
			AssertEquals("invoiceLine.FDAs.Count", 0, invoiceLine.FDAs.Count);
			AssertEquals("invoiceLine.ACE_FDALines.Count", 1, invoiceLine.ACE_FDALines.Count);
		}

		// CS00058759
		public void TestStandaloneCommercialInvoiceFields()
		{
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			var declaration = (JobDeclaration)new FakeDeclarationCreatorForInvoice(invoice).HeaderData;
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory);
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Export;
			invoice.JZ_OH_Buyer = helper.Consignee.PK;
			invoice.JZ_OH_Supplier = helper.Consignor.PK;
			invoice.US_TariffType = TariffTypeList.Codes.HTS;
			invoice.JZ_InvoiceNumber = "INV2#$34$#23";
			invoice.JZ_GB = GlbBranch.CurrentBranch.PK;
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = helper.AUD.RX_Code;
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.FOA;
			invoice.IsJZ_InvoiceCurrExRateUserEnterable = true;
			invoice.JZ_InvoiceCurrExRate = 0.95m;
			invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 110m);
			invoice.JZ_Weight = 150m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Kilograms;

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobComInvoiceHeader reloadInvoice = newFactory.Load<JobComInvoiceHeader>(invoice.PK);
			AssertEquals("MessageType", JobMessageTypeList.Codes.Export, reloadInvoice.JZ_MessageType);
			AssertEquals("Buyer", helper.Consignee.PK, reloadInvoice.JZ_OH_Buyer);
			AssertEquals("Supplier", helper.Consignor.PK, reloadInvoice.JZ_OH_Supplier);
			AssertEquals("TariffType", TariffTypeList.Codes.HTS, reloadInvoice.US_TariffType);
			AssertEquals("InvoiceNumber", "INV2#$34$#23", reloadInvoice.JZ_InvoiceNumber);
			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, reloadInvoice.JZ_GB);
			AssertEquals("InvoiceAmount", 1000m, reloadInvoice.JZ_InvoiceAmount);
			AssertEquals("Invoice_Currency", helper.AUD.RX_Code, reloadInvoice.JZ_RX_NKInvoice_Currency);
			AssertEquals("InvoiceCurrExRate", 0.95m, reloadInvoice.JZ_InvoiceCurrExRate);
			AssertEquals("IncoTerm", TermsOfDeliveryList.Codes.FOA, reloadInvoice.JZ_IncoTerm);
			AssertEquals("Weight", 150m, reloadInvoice.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, reloadInvoice.JZ_WeightUQ);
			AssertEquals("Charges", 1, reloadInvoice.Charges.Count);
			AssertEquals("Charge Amount", 110m, reloadInvoice.Charges[0].J7_Amount);
			AssertEquals("Charge Type", Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, reloadInvoice.Charges[0].J7_ChargeType);
		}

		public void TestCountryOfOrigin()
		{
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, invoiceLine.US_UC_NKCountryOfOrigin);

			bool invoiceLineCountryOfOriginChangeCalled = false;

			invoiceLine.US_UC_NKCountryOfOriginInfo.ValueChanged += delegate
			{ invoiceLineCountryOfOriginChangeCalled = true; };

			invoiceHeader.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			invoiceHeader.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Cambodia;
			Assert("Country of Origin refreshed", invoiceLineCountryOfOriginChangeCalled);

			invoiceHeader.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Cambodia;
			AssertEquals(Core.Constants.CountryCodes.Cambodia, invoiceLine.US_UC_NKCountryOfOrigin);
		}

		public void TestENSReleaseEntry()
		{
			SetupData();
			var invoice = testDeclaration.Invoices[0];
			invoice.US_ReleaseEntryNumber = "SV912345678";
			AssertNull(invoice.ENSReleaseEntry);

			testReleaseDeclaration.CustomsEntryHeaders[0].CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			testReleaseDeclaration.CustomsEntryHeaders[0].CusEntryNumber.CE_EntryType = "ENS";
			Factory.Save();
			AssertNotNull(invoice.ENSReleaseEntry);
		}

		public void TestUS_ReleaseEntryNumber()
		{
			SetupData();
			AssertEquals("PreCondition: US_ConsolidatedJobNumber is empty.", ZString.Empty, testReleaseDeclaration.US_ConsolidatedJobNumber);

			var invoice = testDeclaration.Invoices[0];
			invoice.US_ReleaseEntryNumber = "SV912345678";
			Factory.Save();

			AssertEquals("US_ConsolidatedJobNumber has a value", "B00110010", testReleaseDeclaration.US_ConsolidatedJobNumber);
		}

		[TestDate(2020, 1, 1)]
		public void TestUS_ReleaseEntryNumberUpdatesPSD()
		{
			SetupData();
			var statementData = new DefaultStatementPrintDate();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = 0;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, statementData);

			testDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			testDeclaration.JE_EntryAuthorisationDate = new ZDateTime(2020, 1, 1);
			testReleaseDeclaration.JE_EntryAuthorisationDate = new ZDateTime(2020, 4, 1);

			AssertEquals(new ZDateTime(2020, 1, 1), testDeclaration.US_PreliminaryStatementPrintDate);
			var invoice = testDeclaration.Invoices[0];
			invoice.US_ReleaseEntryNumber = "SV912345678";
			AssertEquals(new ZDateTime(2020, 4, 1), testDeclaration.US_PreliminaryStatementPrintDate);
		}

		public void TestDefaultUS_SupTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = new ZDate(2016, 01, 01);
			var endDate = new ZDate(2079, 01, 01);

			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();

			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "7301100000";
			tariff1.UE_DateFrom = startDate;
			tariff1.UE_DateTo = endDate;
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "8450110010";
			tariff2.UE_DateFrom = startDate;
			tariff2.UE_DateTo = endDate;
			Factory.Save();

			var progTariff1 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038001", startDate, endDate);
			var progTariff2 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99034501", startDate, endDate);
			var progTariff3 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99034502", startDate, endDate);
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			Factory.Save();
			var rate1 = helper.CreateRate(progTariff1, rateCode.PK, startDate, endDate, "0");
			var rate2 = helper.CreateRate(progTariff2, rateCode.PK, startDate, endDate, "0");
			var rate3 = helper.CreateRate(progTariff3, rateCode.PK, startDate, endDate, "0");
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.HongKong, "HK", startDate, endDate);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.HongKong, startDate, endDate);
			var applicability1 = helper.CreateCusApplicability(rate1, tradeGroup, startDate, endDate);
			var applicability2 = helper.CreateCusApplicability(rate2, tradeGroup, startDate, endDate);
			var applicability3 = helper.CreateCusApplicability(rate3, tradeGroup, startDate, endDate);
			var relationship1 = helper.CreateTariffRelationship(progTariff1.PK, hsnTariffType.PK, "73");
			var relationship2 = helper.CreateTariffRelationship(progTariff2.PK, hsnTariffType.PK, "84501100");
			var relationship3 = helper.CreateTariffRelationship(progTariff3.PK, hsnTariffType.PK, "84501100");
			var tariffAttribute1 = helper.CreateTariffAttribute("RULE", "A99", progTariff1);
			var tariffAttribute2 = helper.CreateTariffAttribute("RULE", "A99", progTariff2);
			var tariffAttribute3 = helper.CreateTariffAttribute("RULE", "A99", progTariff3);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLines = invoice.JobComInvoiceLines;
			var invoiceLine1 = invoiceLines.AddNew();
			AssertEquals(ZString.Empty, invoiceLine1.US_SupTariff);

			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLine1.JI_Tariff = "7301100000";
			AssertEquals("US_SupTariff has been set", "99038001", invoiceLine1.US_SupTariff);
			AssertEquals(1, invoiceLine1.ApplicableSupTariffList.Count);

			var invoiceLine2 = invoiceLines.AddNew();
			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLine2.JI_Tariff = "8450110010";
			AssertEquals(ZString.Empty, invoiceLine2.US_SupTariff);
			AssertEquals(2, invoiceLine2.ApplicableSupTariffList.Count);
		}

		public void TestIsNoCharge()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			Assert("IsNoCharge", !invoice.IsNoCharge);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Assert("IsNoCharge", invoice.IsNoCharge);
		}

		public void TestImporterOfRecord()
		{
			AssertNull(InvoiceHeader.ImporterOfRecord);

			OrgHeader importer = Factory.New<OrgHeader>();
			Declaration.IOROrgPK = importer.PK;
			AssertEquals(importer, InvoiceHeader.ImporterOfRecord);
		}

		public void TestCountryOfExport()
		{
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.SouthAfrica;
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, invoiceLine.US_UC_NKCountryOfExport);

			bool invoiceLineCountryOfExportChangeCalled = false;

			invoiceLine.US_UC_NKCountryOfExportInfo.ValueChanged += delegate
			{ invoiceLineCountryOfExportChangeCalled = true; };

			invoiceHeader.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.SouthAfrica;
			invoiceHeader.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Cambodia;
			Assert("Country of Export refreshed", invoiceLineCountryOfExportChangeCalled);
			AssertEquals(Core.Constants.CountryCodes.Cambodia, invoiceLine.US_UC_NKCountryOfExport);
		}

		public void TestDateOfExport()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ExportDate = ZDateTime.BrettsBirthday;
			AssertEquals("DateOfExport", ZDateTime.BrettsBirthday, InvoiceHeader.US_DateOfExport);

			InvoiceHeader.US_DateOfExport = ZDateTime.Today;
			AssertEquals("DateOfExport", ZDateTime.Today, InvoiceHeader.US_DateOfExport);

			InvoiceHeader.US_DateOfExport = ZDateTime.BrettsBirthday;
			AssertEquals("DateOfExport", ZDateTime.BrettsBirthday, InvoiceHeader.US_DateOfExport);
		}

		public void TestUS_DateOfExport_ValuationDatesDirty()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ExportDate = ZDateTime.BrettsBirthday;
			AssertEquals("ValuationDatesChanged shoudl be true", true, Declaration.ValuationDatesChanged);
			Factory.Save();

			Declaration.JE_ExportDate = ZDateTime.BrettsBirthday;
			AssertEquals("ValuationDatesChanged shoudl be false", false, Declaration.ValuationDatesChanged);
		}

		public void TestFallBackOfManufacturerDetails()
		{
			OrgHeader manufacturer1 = Factory.New<OrgHeader>();
			manufacturer1.OH_FullName = "MANUFACTURER 1";
			OrgHeader manufacturer2 = Factory.New<OrgHeader>();
			manufacturer2.OH_FullName = "MANUFACTURER 2";

			Declaration.JE_OA_ManufacturerAddress = manufacturer1.MainAddress.PK;
			InvoiceHeader.JZ_OA_ManufacturerAddress = manufacturer2.MainAddress.PK;
			AssertEquals("ManufacturerDetails", "MANUFACTURER 2", InvoiceHeader.ManufacturerDetails.CompanyName);

			InvoiceHeader.JZ_OA_ManufacturerAddress = ZGuid.Empty;
			AssertEquals("ManufacturerDetails", "MANUFACTURER 1", InvoiceHeader.ManufacturerDetails.CompanyName);
		}

		public void TestMessageStatusIsClearedAfterCloned()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_MessageStatus = "AAA";

			JobDeclaration clonedDec = (JobDeclaration)declaration.TemplateCopy();
			AssertEquals("Status is cleared", ZString.Empty, clonedDec.Invoices[0].JZ_MessageStatus);
		}

		public void TestCanSendOriginalCanSendWithdrawal()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_MessageStatus = ImportMessageStatusList.Codes.AwaitingElectronicInvoiceOriginal;
			AssertEquals("CanSendWithdrawal", false, invoice.CanSendWithdrawal);
			AssertEquals("CanSendOriginal", true, invoice.CanSendOriginal);

			invoice.JZ_MessageStatus = ImportMessageStatusList.Codes.ErrorElectronicInvoiceOriginal;
			AssertEquals("CanSendWithdrawal", false, invoice.CanSendWithdrawal);
			AssertEquals("CanSendOriginal", true, invoice.CanSendOriginal);

			invoice.JZ_MessageStatus = ImportMessageStatusList.Codes.ClearElectronicInvoiceOriginal;
			AssertEquals("CanSendWithdrawal", true, invoice.CanSendWithdrawal);
			AssertEquals("CanSendOriginal", false, invoice.CanSendOriginal);

			invoice.JZ_MessageStatus = ImportMessageStatusList.Codes.AwaitingElectronicInvoiceReplace;
			AssertEquals("CanSendWithdrawal", true, invoice.CanSendWithdrawal);
			AssertEquals("CanSendOriginal", false, invoice.CanSendOriginal);

			invoice.JZ_MessageStatus = ImportMessageStatusList.Codes.ErrorElectronicInvoiceReplace;
			AssertEquals("CanSendWithdrawal", true, invoice.CanSendWithdrawal);
			AssertEquals("CanSendOriginal", false, invoice.CanSendOriginal);
		}

		public void TestUS_TariffType()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.ExportDefaultTariffType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TariffTypeList.Codes.ScheduleB);
			JobComInvoiceHeader invoiceToBeSaved = Factory.New<JobComInvoiceHeader>();
			invoiceToBeSaved.JZ_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Default from registry invoiceToBeSaved.US_TariffType", TariffTypeList.Codes.ScheduleB, invoiceToBeSaved.US_TariffType);
			invoiceToBeSaved.US_TariffType = TariffTypeList.Codes.HTS;
			AssertEquals("invoiceToBeSaved.US_TariffType", TariffTypeList.Codes.HTS, invoiceToBeSaved.US_TariffType);
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobComInvoiceHeader invoiceLoaded = newFactory.Load<JobComInvoiceHeader>(invoiceToBeSaved.PK);
			AssertEquals("invoiceLoaded.US_TariffType", TariffTypeList.Codes.HTS, invoiceLoaded.US_TariffType);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;
			invoiceToBeSaved.JZ_JE = declaration.PK;
			AssertEquals("invoiceToBeSaved.US_TariffType", TariffTypeList.Codes.HTS, invoiceToBeSaved.US_TariffType);
			Factory.Save();

			invoiceLoaded = newFactory.Load<JobComInvoiceHeader>(invoiceToBeSaved.PK);
			AssertEquals("invoiceLoaded.US_TariffType", TariffTypeList.Codes.HTS, invoiceLoaded.US_TariffType);

			DataRegistry.Business.USCustomsDataRegistry.Instance.ExportDefaultTariffType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TariffTypeList.Codes.HTS);
			invoiceToBeSaved = Factory.New<JobComInvoiceHeader>();
			invoiceToBeSaved.JZ_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Default from registry invoiceToBeSaved.US_TariffType", TariffTypeList.Codes.HTS, invoiceToBeSaved.US_TariffType);
		}

		public void TestHumanFriendlyReference()
		{
			org1.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "ABC123456");
			Declaration.JE_OH_Supplier = org1.PK;
			InvoiceHeader.JZ_InvoiceNumber = "INV23423";
			AssertEquals("HumanFriendlyReference", "INV23423", MessageAttacheeInDeclaration.HumanFriendlyReference);
		}

		public void TestTransportMode()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Transport Mode", "SEA", MessageAttacheeInDeclaration.TransportMode);
		}

		public void TestAutoPopulateChargesOnIncotermChange()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			AssertEquals("FOB defaulted", Core.Constants.IncoTerms.FreeOnBoard, invoice.JZ_IncoTerm);

			AssertEquals("Overseas Freight is defaulted", 1, invoice.GroupHeader.Charges.Count);
			AssertEquals("OFT", invoice.GroupHeader.Charges[0].J7_ChargeType);
		}

		public void TestIsStandAlonePriorNoticeMode()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.ValidationModes = ValidationModes.None;
			AssertEquals(false, InvoiceHeader.IsStandAlonePriorNoticeMode);
			Declaration.ValidationModes = ValidationModes.StandAlonePriorNotice;
			AssertEquals(true, InvoiceHeader.IsStandAlonePriorNoticeMode);
		}

		public void TestBuyerOrgPKEffective()
		{
			Declaration[JobDeclarationSchema.JE_OH_Buyer] = org1.PK;
			InvoiceHeader[JobComInvoiceHeader.Schema.BuyerOrgPK] = org2.PK;
			AssertEquals(org2.PK, InvoiceHeader[JobComInvoiceHeader.Schema.BuyerOrgPK]);
			InvoiceHeader[JobComInvoiceHeader.Schema.BuyerOrgPK] = ZGuid.Empty;
			AssertEquals(org1.PK, InvoiceHeader[JobComInvoiceHeader.Schema.BuyerOrgPK]);
		}

		public void TestJZ_OH_BuyingAgent_Effective()
		{
			AssertInvoiceEffectiveOrganisation(JobDeclarationSchema.JE_OH_BuyingAgent, JobComInvoiceHeaderSchema.JZ_OH_BuyerAgent);
		}

		public void TestJZ_OA_Seller_Effective()
		{
			AssertInvoiceEffectiveOrgAddress(JobDeclarationSchema.JE_OA_SellerAddress, JobComInvoiceHeaderSchema.JZ_OA_SellerAddress);
		}

		public void TestJZ_OH_SellingAgent_Effective()
		{
			AssertInvoiceEffectiveOrganisation(JobDeclarationSchema.JE_OH_SellingAgent, JobComInvoiceHeaderSchema.JZ_OH_SellingAgent);
		}

		public void TestJZ_OA_ConsigneeAddress_Effective()
		{
			AssertInvoiceEffectiveOrgAddress(JobDeclarationSchema.JE_OA_ConsigneeAddress, JobComInvoiceHeaderSchema.JZ_OA_ConsigneeAddress);
		}

		public void TestJZ_OA_ShipToPartyAddress_Effective()
		{
			AssertInvoiceEffectiveOrgAddress(JobDeclarationSchema.JE_OA_ShipToPartyAddress, JobComInvoiceHeaderSchema.JZ_OA_ShipToPartyAddress);
		}

		public void TestIsJZ_InvoiceCurrExRateUserEnterable()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.IsJZ_InvoiceCurrExRateUserEnterable = true;
			AssertEquals(true, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			invoice.JZ_InvoiceCurrExRate = 0.004m;
			AssertEquals(0.004m, invoice.JZ_InvoiceCurrExRate);
			invoice.IsJZ_InvoiceCurrExRateUserEnterable = false;
			AssertEquals(false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertNotEquals(0.004m, invoice.JZ_InvoiceCurrExRate);
		}

		public void TestRequireInvoiceLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			AssertEquals("RequireInvoiceLine for export", true, invoice.RequireInvoiceLines);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = false;
			AssertEquals("RequireInvoiceLine for import", true, invoice.RequireInvoiceLines);

			declaration.US_EnableINB = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			AssertEquals("ENS is enabled", true, invoice.RequireInvoiceLines);
		}

		public void TestUS_AESOriginIndicatorIsDefaultedBasedOnSupplier()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.AllowExportDefaultOriginIndicatorAtInvoice.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory);
			helper.Consignor.OH_RL_NKClosestPort = helper.USLAX.Code;
			helper.Consignee.OH_RL_NKClosestPort = helper.USLAX.Code;
			Factory.Save();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			header.US_AESOriginIndicator = "";
			AssertEquals("", header.US_AESOriginIndicator);
			header.JZ_OH_Supplier = helper.Consignor.PK;
			AssertEquals("", header.US_AESOriginIndicator);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			header.JZ_OH_Supplier = helper.Consignor.PK;
			AssertEquals(helper.Consignor.PK, header.JZ_OH_Supplier);
			AssertEquals(AESOriginIndicatorList.Codes.Domestic, header.US_AESOriginIndicator);
			header.JZ_OH_Supplier = ZGuid.Empty;
			header.US_AESOriginIndicator = "";
			AssertEquals("", header.US_AESOriginIndicator);
			header.JZ_OH_Supplier = helper.Consignor.PK;
			AssertEquals(AESOriginIndicatorList.Codes.Domestic, header.US_AESOriginIndicator);
			header.Supplier.OH_RL_NKClosestPort = helper.AUSYD.Code;
			header.JZ_OH_Supplier = helper.Consignee.PK;
			AssertEquals(AESOriginIndicatorList.Codes.Domestic, header.US_AESOriginIndicator);

			header.US_AESOriginIndicator = "";
			header.JZ_OH_Supplier = helper.Consignor.PK;
			AssertEquals(AESOriginIndicatorList.Codes.Foreign, header.US_AESOriginIndicator);

			DataRegistry.Business.USCustomsDataRegistry.Instance.AllowExportDefaultOriginIndicatorAtInvoice.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			header.JZ_OH_Supplier = ZGuid.Empty;
			header.US_AESOriginIndicator = ZString.Empty;
			header.JZ_OH_Supplier = helper.Consignor.PK;
			AssertEquals("Origin Indicator should be empty as registry is set to false", ZString.Empty, header.US_AESOriginIndicator);
		}

		public void TestHasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var amsHeader = invoiceLine.AMSLines.AddNew();
			amsHeader.US_Program = AMSProgramList.Codes.MO1;
			Assert(invoiceHeader.HasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2);

			amsHeader.US_Program = AMSProgramList.Codes.MO2;
			Assert(invoiceHeader.HasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2);

			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.US_AMSInd = "C";
			invoiceLine2.US_AMSDisclaimProgram = AMSProgramList.Codes.MO7;
			Assert(invoiceHeader2.HasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2);
		}

		public void TestEntryHeaderData()
		{
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory);
			JobDeclaration declaration = helper.CreateSeaExportDeclaration();

			JobComInvoiceHeader invoice = declaration.Invoices[0];
			AssertEquals("US_EntryNumber", "", invoice.US_EntryNumber);
			AssertEquals("US_XTN", "", invoice.US_XTN);

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals("declaration.CustomsEntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.EntryNumber = "ITN1234";
			entryHeader.US_XTN = "XTN1234";

			AssertEquals("US_EntryNumber", entryHeader.EntryNumber, invoice.US_EntryNumber);
			AssertEquals("US_XTN", entryHeader.US_XTN, invoice.US_XTN);
		}

		public void TestUS_TermsOfDeliveryLocationScheduleD()
		{
			AssertEquals(4, InvoiceHeader.US_TermsOfDeliveryLocationScheduleDInfo.MaxLength);
			InvoiceHeader.US_TermsOfDeliveryLocationScheduleD = "2342";
			AssertEquals("2342", InvoiceHeader.US_TermsOfDeliveryLocation);
			InvoiceHeader.US_TermsOfDeliveryLocation = "4323";
			AssertEquals("4323", InvoiceHeader.US_TermsOfDeliveryLocationScheduleD);
			ZString maxLengthMessage = ZString.Empty;
			try
			{
				InvoiceHeader.US_TermsOfDeliveryLocationScheduleD = "23932423";
			}
			catch (MaxLengthExceededException e)
			{
				maxLengthMessage = e.Message;
				ErrorReporter.Clear();
			}
			AssertEquals("The maximum length of 'US_TermsOfDeliveryLocationScheduleD' has been exceeded.\n The maximum length of this property is 4 characters, but 8 were entered. New value: 23932423. Old value: 4323", maxLengthMessage);
		}

		public void TestUS_TermsOfDeliveryLocationScheduleK()
		{
			AssertEquals(5, InvoiceHeader.US_TermsOfDeliveryLocationScheduleKInfo.MaxLength);
			InvoiceHeader.US_TermsOfDeliveryLocationScheduleK = "23422";
			AssertEquals("23422", InvoiceHeader.US_TermsOfDeliveryLocation);
			InvoiceHeader.US_TermsOfDeliveryLocation = "43232";
			AssertEquals("43232", InvoiceHeader.US_TermsOfDeliveryLocationScheduleK);
			ZString maxLengthMessage = ZString.Empty;
			try
			{
				InvoiceHeader.US_TermsOfDeliveryLocationScheduleK = "23932423";
			}
			catch (MaxLengthExceededException e)
			{
				maxLengthMessage = e.Message;
				ErrorReporter.Clear();
			}
			AssertEquals("The maximum length of 'US_TermsOfDeliveryLocationScheduleK' has been exceeded.\n The maximum length of this property is 5 characters, but 8 were entered. New value: 23932423. Old value: 43232", maxLengthMessage);
		}

		public void TestUS_TermsOfDeliveryLocationCountry()
		{
			InvoiceHeader.US_TermsOfDeliveryLocationIndicator = TermsOfDeliveryLocationCodeIndicators.Codes.Other;
			AssertEquals(USAddInfoSchema.US_TermsOfDeliveryLocation.MaxLength, InvoiceHeader.US_TermsOfDeliveryLocationCountryInfo.MaxLength);
			InvoiceHeader.US_TermsOfDeliveryLocationIndicator = TermsOfDeliveryLocationCodeIndicators.Codes.ISOCountryCode;
			AssertEquals(2, InvoiceHeader.US_TermsOfDeliveryLocationCountryInfo.MaxLength);
			InvoiceHeader.US_TermsOfDeliveryLocationCountry = "AU";
			AssertEquals("AU", InvoiceHeader.US_TermsOfDeliveryLocation);
			InvoiceHeader.US_TermsOfDeliveryLocation = "NZ";
			AssertEquals("NZ", InvoiceHeader.US_TermsOfDeliveryLocationCountry);
			ZString maxLengthMessage = ZString.Empty;
			try
			{
				InvoiceHeader.US_TermsOfDeliveryLocationCountry = "ABC";
			}
			catch (MaxLengthExceededException e)
			{
				maxLengthMessage = e.Message;
				ErrorReporter.Clear();
			}
			AssertEquals("The maximum length of 'US_TermsOfDeliveryLocationCountry' has been exceeded.\n The maximum length of this property is 2 characters, but 3 were entered. New value: ABC. Old value: NZ", maxLengthMessage);
		}

		public void TestUS_USPPI()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoice.JZ_OH_Supplier = org1.PK;
			USOrganisation uS_USPPI = invoice.US_USPPI;
			AssertNotNull("US_USPPI", uS_USPPI);
			AssertEquals("US_USPPI.ZO_OH_Organisation", invoice.JZ_OH_Supplier, uS_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OH_Organisation", org1.PK, uS_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OA_Address", invoice.USPPIDocAddress.E2_OA_Address, uS_USPPI.ZO_OA_Address);
			AssertEquals("US_USPPI.ZO_OA_Address", org1.MainAddress.PK, uS_USPPI.ZO_OA_Address);

			uS_USPPI.ZO_OA_Address = org2.MainAddress.PK;
			AssertEquals("US_USPPI.ZO_OH_Organisation", invoice.JZ_OH_Supplier, uS_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OH_Organisation", org2.PK, uS_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OA_Address", invoice.USPPIDocAddress.E2_OA_Address, uS_USPPI.ZO_OA_Address);
			AssertEquals("US_USPPI.ZO_OA_Address", org2.MainAddress.PK, uS_USPPI.ZO_OA_Address);

			invoice.USPPIDocAddress.E2_OA_Address = org1.MainAddress.PK;
			AssertEquals("US_USPPI.ZO_OH_Organisation", invoice.JZ_OH_Supplier, uS_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OH_Organisation", org1.PK, uS_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OA_Address", invoice.USPPIDocAddress.E2_OA_Address, uS_USPPI.ZO_OA_Address);
			AssertEquals("US_USPPI.ZO_OA_Address", org1.MainAddress.PK, uS_USPPI.ZO_OA_Address);

			invoice.USPPIDocAddress.E2_AddressOverride = true;
			uS_USPPI.ZO_Contact = "BOB SMITH";
			AssertEquals("US_USPPI.ZO_Contact", invoice.USPPIDocAddress.E2_Contact, uS_USPPI.ZO_Contact);
			AssertEquals("US_USPPI.ZO_Contact", "BOB SMITH", uS_USPPI.ZO_Contact);

			invoice.USPPIDocAddress.E2_Contact = "JOE BROWN";
			AssertEquals("US_USPPI.ZO_Contact", invoice.USPPIDocAddress.E2_Contact, uS_USPPI.ZO_Contact);
			AssertEquals("US_USPPI.ZO_Contact", "JOE BROWN", uS_USPPI.ZO_Contact);

			uS_USPPI.ZO_Phone = "11111111";
			AssertEquals("US_USPPI.ZO_Phone", invoice.USPPIDocAddress.E2_Phone_Formatted, uS_USPPI.ZO_Phone);
			AssertEquals("US_USPPI.ZO_Phone", "11111111", uS_USPPI.ZO_Phone);

			uS_USPPI.ZO_Phone = "22222222";
			AssertEquals("US_USPPI.ZO_Phone", invoice.USPPIDocAddress.E2_Phone_Formatted, uS_USPPI.ZO_Phone);
			AssertEquals("US_USPPI.ZO_Phone", "22222222", uS_USPPI.ZO_Phone);

			AssertEquals("US_USPPI.DocGroup", ContactType.Consignor.ToString(), uS_USPPI.DocGroup);
		}

		public void TestUS_USPPI_ContactsActive()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST";
			org.MainAddress.OA_Address1 = "Address 1";
			org.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact 1";
			contact.OC_Phone = "Phone 1";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Contact 2";
			contact2.OC_Phone = "Phone 2";
			contact2.OC_IsActive = false;

			var invoice = Declaration.Invoices.AddNew();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoice.JZ_OH_Supplier = org.PK;
			var uS_USPPI = invoice.US_USPPI;
			AssertEquals(2, uS_USPPI.Organisation.Contacts.Count);
			AssertEquals(1, uS_USPPI.Organisation.ContactsActive.Count);
			AssertEquals("Contact 1", uS_USPPI.Organisation.ContactsActive[0].OC_ContactName);
		}

		public void TestContactPhoneDefaultsFromChosenContact()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = "John Smith";
			contact1.OC_Phone = "+1 (378) 9512000";

			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_ContactName = "Adam Smith";
			contact2.OC_Phone = "(378) 9512005";

			var contact3 = orgHeader.Contacts.AddNew();
			contact3.OC_ContactName = "Wendy Jones";
			contact3.OC_Phone = "3789512010";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = orgHeader.PK;
			invoice.USPPIDocAddress.E2_Contact = contact1.OC_ContactName;
			AssertEquals("Phone no should default from contact", invoice.USPPIDocAddress.E2_Phone_Formatted, contact1.OC_Phone);

			invoice.USPPIDocAddress.E2_Contact = contact2.OC_ContactName;
			AssertEquals("Phone no should default from contact", invoice.USPPIDocAddress.E2_Phone_Formatted, contact2.OC_Phone);

			invoice.USPPIDocAddress.E2_Contact = contact3.OC_ContactName;
			AssertEquals("Phone no should default from contact", invoice.USPPIDocAddress.E2_Phone_Formatted, contact3.OC_Phone);

			invoice.JZ_OH_Buyer = orgHeader.PK;
			invoice.UltimateConsigneeDocAddress.E2_Contact = contact1.OC_ContactName;
			AssertEquals("Phone no should default from contact", invoice.UltimateConsigneeDocAddress.E2_Phone_Formatted, contact1.OC_Phone);

			invoice.UltimateConsigneeDocAddress.E2_Contact = contact2.OC_ContactName;
			AssertEquals("Phone no should default from contact", invoice.UltimateConsigneeDocAddress.E2_Phone_Formatted, contact2.OC_Phone);

			invoice.UltimateConsigneeDocAddress.E2_Contact = contact3.OC_ContactName;
			AssertEquals("Phone no should default from contact", invoice.UltimateConsigneeDocAddress.E2_Phone_Formatted, contact3.OC_Phone);

			invoice.JZ_OH_Consignee = orgHeader.PK;
			invoice.IntermediateConsigneeDocAddress.E2_Contact = contact1.OC_ContactName;
			AssertEquals("Phone no should default from contact", invoice.IntermediateConsigneeDocAddress.E2_Phone_Formatted, contact1.OC_Phone);

			invoice.IntermediateConsigneeDocAddress.E2_Contact = contact2.OC_ContactName;
			AssertEquals("Phone no should default from contact", invoice.IntermediateConsigneeDocAddress.E2_Phone_Formatted, contact2.OC_Phone);

			invoice.IntermediateConsigneeDocAddress.E2_Contact = contact3.OC_ContactName;
			AssertEquals("Phone no should default from contact", invoice.IntermediateConsigneeDocAddress.E2_Phone_Formatted, contact3.OC_Phone);
		}

		public void TestUS_ExportUltimateConsignee()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var dec = (JobDeclaration)new FakeDeclarationCreatorForInvoice(invoice).HeaderData;
			var uS_ExportUltimateConsignee = invoice.US_ExportUltimateConsignee;
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Export;
			invoice.JZ_OH_Buyer = org1.PK;
			AssertEquals("US_ExportUltimateConsignee.ZO_OH_Organisation", invoice.JZ_OH_Buyer, uS_ExportUltimateConsignee.ZO_OH_Organisation);
			AssertEquals("US_ExportUltimateConsignee.ZO_OH_Organisation", org1.PK, uS_ExportUltimateConsignee.ZO_OH_Organisation);
			AssertEquals("US_ExportUltimateConsignee.ZO_OA_Address", invoice.UltimateConsigneeDocAddress.E2_OA_Address, uS_ExportUltimateConsignee.ZO_OA_Address);
			AssertEquals("US_ExportUltimateConsignee.ZO_OA_Address", org1.MainAddress.PK, uS_ExportUltimateConsignee.ZO_OA_Address);

			uS_ExportUltimateConsignee.ZO_OA_Address = org2.MainAddress.PK;
			AssertEquals("US_ExportUltimateConsignee.ZO_OH_Organisation", invoice.JZ_OH_Buyer, uS_ExportUltimateConsignee.ZO_OH_Organisation);
			AssertEquals("US_ExportUltimateConsignee.ZO_OH_Organisation", org2.PK, uS_ExportUltimateConsignee.ZO_OH_Organisation);
			AssertEquals("US_ExportUltimateConsignee.ZO_OA_Address", invoice.UltimateConsigneeDocAddress.E2_OA_Address, uS_ExportUltimateConsignee.ZO_OA_Address);
			AssertEquals("US_ExportUltimateConsignee.ZO_OA_Address", org2.MainAddress.PK, uS_ExportUltimateConsignee.ZO_OA_Address);

			invoice.UltimateConsigneeDocAddress.E2_OA_Address = org1.MainAddress.PK;
			AssertEquals("US_ExportUltimateConsignee.ZO_OH_Organisation", invoice.JZ_OH_Buyer, uS_ExportUltimateConsignee.ZO_OH_Organisation);
			AssertEquals("US_ExportUltimateConsignee.ZO_OH_Organisation", org1.PK, uS_ExportUltimateConsignee.ZO_OH_Organisation);
			AssertEquals("US_ExportUltimateConsignee.ZO_OA_Address", invoice.UltimateConsigneeDocAddress.E2_OA_Address, uS_ExportUltimateConsignee.ZO_OA_Address);
			AssertEquals("US_ExportUltimateConsignee.ZO_OA_Address", org1.MainAddress.PK, uS_ExportUltimateConsignee.ZO_OA_Address);

			uS_ExportUltimateConsignee.ZO_Contact = "BOB SMITH";
			AssertEquals("US_ExportUltimateConsignee.ZO_Contact", invoice.UltimateConsigneeDocAddress.E2_Contact, uS_ExportUltimateConsignee.ZO_Contact);
			AssertEquals("US_ExportUltimateConsignee.ZO_Contact", "BOB SMITH", uS_ExportUltimateConsignee.ZO_Contact);

			invoice.UltimateConsigneeDocAddress.E2_Contact = "JOE BROWN";
			AssertEquals("US_ExportUltimateConsignee.ZO_Contact", invoice.UltimateConsigneeDocAddress.E2_Contact, uS_ExportUltimateConsignee.ZO_Contact);
			AssertEquals("US_ExportUltimateConsignee.ZO_Contact", "JOE BROWN", uS_ExportUltimateConsignee.ZO_Contact);

			invoice.UltimateConsigneeDocAddress.E2_AddressOverride = true;
			uS_ExportUltimateConsignee.ZO_Phone = "11111111";
			AssertEquals("US_ExportUltimateConsignee.ZO_Phone", invoice.UltimateConsigneeDocAddress.E2_Phone_Formatted, uS_ExportUltimateConsignee.ZO_Phone);
			AssertEquals("US_ExportUltimateConsignee.ZO_Phone", "11111111", uS_ExportUltimateConsignee.ZO_Phone);

			uS_ExportUltimateConsignee.ZO_Phone = "22222222";
			AssertEquals("US_ExportUltimateConsignee.ZO_Phone", invoice.UltimateConsigneeDocAddress.E2_Phone_Formatted, uS_ExportUltimateConsignee.ZO_Phone);
			AssertEquals("US_ExportUltimateConsignee.ZO_Phone", "22222222", uS_ExportUltimateConsignee.ZO_Phone);

			AssertEquals("US_ExportUltimateConsignee.DocGroup", ContactType.Consignee.ToString(), uS_ExportUltimateConsignee.DocGroup);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Importer = org1.PK;
			declaration.Invoices.Add(invoice);
			declaration.US_SoldEnRouteIndicator = YesNoDefaultList.Codes.Yes;
			invoice.JZ_OH_Buyer = ZGuid.Empty;
			AssertEquals("No effective value for Sold En Route", ZGuid.Empty, invoice.JZ_OH_Buyer);
		}

		public void TestSupplierPickupAddress()
		{
			org1.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var decPickupAddress = Declaration.SupplierPickupAddress;
			decPickupAddress.E2_OA_Address = org1.MainAddress.PK;
			var invoice = Declaration.Invoices.AddNew();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var supplierPickupAddress = invoice.SupplierPickupAddress;

			AssertEquals("SupplierPickupAddress.ZO_OH_Organisation", org1.PK, supplierPickupAddress.OrganisationPK);
			AssertEquals("SupplierPickupAddress.ZO_OA_Address", decPickupAddress.E2_OA_Address, supplierPickupAddress.E2_OA_Address);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var invoiceReload = newFactory.Load<JobComInvoiceHeader>(invoice.PK);
			var supplierPickupAddressReload = invoiceReload.SupplierPickupAddress;
			AssertEquals("HasChanges", false, supplierPickupAddressReload.HasChanges);
			AssertEquals("PK", supplierPickupAddress.PK, supplierPickupAddressReload.PK);
			AssertEquals("E2_AddressType", DocAddressTypes.Codes.SupplierPickupDeliveryAddress, supplierPickupAddressReload.E2_AddressType);
			AssertEquals("E2_ParentID", supplierPickupAddress.E2_ParentID, supplierPickupAddressReload.E2_ParentID);
			AssertEquals("E2_OA_Address", org1.MainAddress.PK, supplierPickupAddressReload.E2_OA_Address);
			supplierPickupAddressReload.E2_AddressOverride = true;
			AssertNoExceptionThrown(newFactory.Save);
		}

		public void TestSupplierPickupAddressOverridden()
		{
			var decPickupAddress = Declaration.SupplierPickupAddress;
			decPickupAddress.E2_AddressOverride = true;
			decPickupAddress.E2_Address1 = "ADDRESS1";
			decPickupAddress.E2_Address2 = "ADDRESS2";
			decPickupAddress.E2_Postcode = "2017";
			decPickupAddress.E2_City = "WATERLOO";
			decPickupAddress.E2_State = "NS";
			decPickupAddress.E2_RN_NKCountryCode = "US";
			var invoice = Declaration.Invoices.AddNew();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var supplierPickupAddress = invoice.SupplierPickupAddress;

			AssertEquals("ADDRESS1", supplierPickupAddress.E2_Address1);
			AssertEquals("ADDRESS2", supplierPickupAddress.E2_Address2);
			AssertEquals("2017", supplierPickupAddress.E2_Postcode);
			AssertEquals("WATERLOO", supplierPickupAddress.E2_City);
			AssertEquals("NS", supplierPickupAddress.E2_State);
			AssertEquals("US", supplierPickupAddress.E2_RN_NKCountryCode);
		}

		public void TestUS_UltimateDestinationCountry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.UnitedStates;

			var org01 = Factory.New<OrgHeader>();
			org01.FillWithValidTestData();
			org01.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			org01.OH_RL_NKClosestPort = "USTES";

			var org02 = Factory.New<OrgHeader>();
			org02.FillWithValidTestData();
			org02.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			org02.OH_RL_NKClosestPort = "CATES";

			var invoice01 = declaration.Invoices.AddNew();
			var invoice02 = declaration.Invoices.AddNew();
			var line01 = invoice01.InvoiceLines.AddNew();
			var line02 = invoice02.InvoiceLines.AddNew();
			invoice01.JZ_OH_Buyer = org01.PK;
			invoice02.JZ_OH_Buyer = org02.PK;

			AssertEquals("US", org01.CountryCode, invoice01.US_UltimateDestinationCountry);
			AssertEquals("CA", org02.CountryCode, invoice02.US_UltimateDestinationCountry);
		}

		public void TestDefaultUltCneeCountryWhenSetUltimateConsigneeDocAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.UnitedStates;

			var org01 = Factory.New<OrgHeader>();
			org01.FillWithValidTestData();
			org01.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			org01.OH_RL_NKClosestPort = "USTES";

			var org02 = Factory.New<OrgHeader>();
			org02.FillWithValidTestData();
			org02.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			org02.OH_RL_NKClosestPort = "CATES";

			var invoice01 = declaration.Invoices.AddNew();
			var invoice02 = declaration.Invoices.AddNew();
			var line01 = invoice01.InvoiceLines.AddNew();
			var line02 = invoice02.InvoiceLines.AddNew();
			invoice01.UltimateConsigneeDocAddress.OrganisationPK = org01.PK;
			invoice02.UltimateConsigneeDocAddress.OrganisationPK = org02.PK;

			AssertEquals("US", org01.CountryCode, invoice01.US_UltimateDestinationCountry);
			AssertEquals("CA", org02.CountryCode, invoice02.US_UltimateDestinationCountry);
		}

		public void TestUS_IntermediateConsignee()
		{
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			USOrganisation us_IntermConsignee = invoice.US_IntermediateConsignee;
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Export;
			invoice.JZ_OH_Consignee = org1.PK;
			AssertEquals("US_IntermConsignee.ZO_OH_Organisation", invoice.JZ_OH_Consignee, us_IntermConsignee.ZO_OH_Organisation);
			AssertEquals("US_IntermConsignee.ZO_OH_Organisation", org1.PK, us_IntermConsignee.ZO_OH_Organisation);
			AssertEquals("US_IntermConsignee.ZO_OA_Address", invoice.IntermediateConsigneeDocAddress.E2_OA_Address, us_IntermConsignee.ZO_OA_Address);
			AssertEquals("US_IntermConsignee.ZO_OA_Address", org1.MainAddress.PK, us_IntermConsignee.ZO_OA_Address);

			us_IntermConsignee.ZO_OA_Address = org2.MainAddress.PK;
			AssertEquals("US_IntermConsignee.ZO_OH_Organisation", invoice.JZ_OH_Consignee, us_IntermConsignee.ZO_OH_Organisation);
			AssertEquals("US_IntermConsignee.ZO_OA_Address", invoice.IntermediateConsigneeDocAddress.E2_OA_Address, us_IntermConsignee.ZO_OA_Address);
			AssertEquals("US_IntermConsignee.ZO_OA_Address", org2.MainAddress.PK, us_IntermConsignee.ZO_OA_Address);

			invoice.IntermediateConsigneeDocAddress.E2_AddressOverride = true;
			us_IntermConsignee.ZO_Contact = "BOB SMITH";
			AssertEquals("US_IntermConsignee.ZO_Contact", invoice.IntermediateConsigneeDocAddress.E2_Contact, us_IntermConsignee.ZO_Contact);
			AssertEquals("US_IntermConsignee.ZO_Contact", "BOB SMITH", us_IntermConsignee.ZO_Contact);

			invoice.IntermediateConsigneeDocAddress.E2_Contact = "JOE BROWN";
			AssertEquals("US_IntermConsignee.ZO_Contact", invoice.IntermediateConsigneeDocAddress.E2_Contact, us_IntermConsignee.ZO_Contact);
			AssertEquals("US_IntermConsignee.ZO_Contact", "JOE BROWN", us_IntermConsignee.ZO_Contact);

			us_IntermConsignee.ZO_Phone = "11111111";
			AssertEquals("US_IntermConsignee.ZO_Phone", invoice.IntermediateConsigneeDocAddress.E2_Phone_Formatted, us_IntermConsignee.ZO_Phone);
			AssertEquals("US_IntermConsignee.ZO_Phone", "11111111", us_IntermConsignee.ZO_Phone);

			us_IntermConsignee.ZO_Phone = "22222222";
			AssertEquals("US_IntermConsignee.ZO_Phone", invoice.IntermediateConsigneeDocAddress.E2_Phone_Formatted, us_IntermConsignee.ZO_Phone);
			AssertEquals("US_IntermConsignee.ZO_Phone", "22222222", us_IntermConsignee.ZO_Phone);

			AssertEquals("US_IntermConsignee.DocGroup", ContactType.Consignee.ToString(), us_IntermConsignee.DocGroup);
		}

		public void TestSetDefaultAESRegistrationNumberIfEmpty()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU });

			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_IsConsignor = true;

			var customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_CodeType = OrgCusCode.USACodeTypes.DDTCRegistrationNumber;
			customsCode.OK_CustomsRegNo = "123456";
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			org.MainAddress.OA_Address1 = "ORG MainAddress";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_LicenseType = "C33";
			var header = declaration.Invoices.AddNew();

			var invoice1 = header.InvoiceLines.AddNew();
			var invoice2 = header.InvoiceLines.AddNew();
			var invoice3 = header.InvoiceLines.AddNew();

			AssertEquals(header.US_LicenseType, declaration.US_LicenseType);
			AssertEquals(invoice1.US_LicenseType, declaration.US_LicenseType);
			AssertEquals(invoice2.US_LicenseType, declaration.US_LicenseType);
			AssertEquals(invoice3.US_LicenseType, declaration.US_LicenseType);

			header.US_USPPI.ZO_OH_Organisation = org.PK;

			invoice1.US_LicenseType = USAESLicenseCode.Codes.C33;
			AssertEquals(ZString.Empty, invoice1.US_DDTCRegistrationNo);
			invoice2.US_LicenseType = USAESLicenseCode.Codes.SAG;
			AssertEquals("123456", invoice2.US_DDTCRegistrationNo);
			invoice3.US_LicenseType = USAESLicenseCode.Codes.C33;
			AssertEquals(ZString.Empty, invoice3.US_DDTCRegistrationNo);

			invoice1.US_LicenseType = USAESLicenseCode.Codes.SAU;
			invoice2.US_LicenseType = USAESLicenseCode.Codes.SAU;
			AssertEquals("123456", invoice1.US_DDTCRegistrationNo);
			AssertEquals("123456", invoice2.US_DDTCRegistrationNo);
			AssertEquals(ZString.Empty, invoice3.US_DDTCRegistrationNo);
			AssertEquals(header.US_LicenseType, declaration.US_LicenseType);

			header.US_LicenseType = USAESLicenseCode.Codes.SAG;
			AssertEquals("123456", header.US_DDTCRegistrationNo);

			header.AddInfoValidation.ValidateUS_LicenseType();
			AssertHasWarning(header.US_LicenseTypeInfo, ExportAddInfoJobComInvoiceHeaderValidation.LicenseTypeSyncError);
		}

		public void TestCopyValueFrom()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.AllowExportDefaultOriginIndicatorAtInvoice.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			JobComInvoiceHeader sourceInvoice = Factory.New<JobComInvoiceHeader>();
			sourceInvoice.US_USPPI.ZO_OH_Organisation = org1.PK;
			sourceInvoice.US_USPPI.ZO_OA_Address = org1.MainAddress.PK;
			sourceInvoice.US_USPPI.ZO_Contact = org1.Contacts[0].OC_ContactName;
			sourceInvoice.US_USPPI.ZO_Phone = org1.Contacts[0].OC_Phone;

			sourceInvoice.US_ExportUltimateConsignee.ZO_OH_Organisation = org2.PK;
			sourceInvoice.US_ExportUltimateConsignee.ZO_OA_Address = org2.MainAddress.PK;
			sourceInvoice.US_ExportUltimateConsignee.ZO_Contact = org2.Contacts[0].OC_ContactName;
			sourceInvoice.US_ExportUltimateConsignee.ZO_Phone = org2.Contacts[0].OC_Phone;

			OrgHeader org3 = CreateNewOrg("ORGANISATION 3", "ORG3 ADDRESS", "JULIE BROWN");
			sourceInvoice.US_IntermediateConsignee.ZO_OH_Organisation = org3.PK;
			sourceInvoice.US_IntermediateConsignee.ZO_OA_Address = org3.MainAddress.PK;
			sourceInvoice.US_IntermediateConsignee.ZO_Contact = org3.Contacts[0].OC_ContactName;
			sourceInvoice.US_IntermediateConsignee.ZO_Phone = org3.Contacts[0].OC_Phone;

			sourceInvoice.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Foreign;
			sourceInvoice.US_StateOfOrigin = "US";
			sourceInvoice.US_ForeignTradeZone = "FTZ";
			sourceInvoice.US_ECCN = "ECCN";
			sourceInvoice.US_RoutedTransaction = YesNoDefaultList.Codes.No;
			sourceInvoice.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			sourceInvoice.US_LicenseType = USAESLicenseCode.Codes.C50;
			sourceInvoice.US_LicenseNo = "LicenseNo";
			sourceInvoice.US_InbondType = InbondTypeList.Codes.TAndEForeignTradeZoneWithdrawal;
			sourceInvoice.US_ImportEntryNo = "ImportEntryNo";
			sourceInvoice.US_HazardousCargo = "";
			sourceInvoice.US_ExportCode = ExportInformationCodeList.Codes.TE;
			sourceInvoice.US_TermsOfDeliveryLocationQualifier = TermsOfDeliveryLocQualifierList.Codes.LoadingPort;
			sourceInvoice.US_TermsOfDeliveryLocationIndicator = TermsOfDeliveryLocationCodeIndicators.Codes.ISOCountryCode;
			sourceInvoice.US_TermsOfDeliveryLocation = "AU";

			JobComInvoiceHeader newInvoice = Factory.New<JobComInvoiceHeader>();
			newInvoice.CopyValueFromForExport(sourceInvoice);

			AssertEquals("US_USPPI.ZO_OH_Organisation", sourceInvoice.US_USPPI.ZO_OH_Organisation, newInvoice.US_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OA_Address", sourceInvoice.US_USPPI.ZO_OA_Address, newInvoice.US_USPPI.ZO_OA_Address);
			AssertEquals("US_USPPI.ZO_Contact", sourceInvoice.US_USPPI.ZO_Contact, newInvoice.US_USPPI.ZO_Contact);
			AssertEquals("US_USPPI.ZO_Phone", sourceInvoice.US_USPPI.ZO_Phone, newInvoice.US_USPPI.ZO_Phone);

			AssertEquals("US_ExportUltimateConsignee.ZO_OH_Organisation", sourceInvoice.US_ExportUltimateConsignee.ZO_OH_Organisation, newInvoice.US_ExportUltimateConsignee.ZO_OH_Organisation);
			AssertEquals("US_ExportUltimateConsignee.ZO_OA_Address", sourceInvoice.US_ExportUltimateConsignee.ZO_OA_Address, newInvoice.US_ExportUltimateConsignee.ZO_OA_Address);
			AssertEquals("US_ExportUltimateConsignee.ZO_Contact", sourceInvoice.US_ExportUltimateConsignee.ZO_Contact, newInvoice.US_ExportUltimateConsignee.ZO_Contact);
			AssertEquals("US_ExportUltimateConsignee.ZO_Phone", sourceInvoice.US_ExportUltimateConsignee.ZO_Phone, newInvoice.US_ExportUltimateConsignee.ZO_Phone);

			AssertEquals("US_IntermediateConsignee.ZO_OH_Organisation", sourceInvoice.US_IntermediateConsignee.ZO_OH_Organisation, newInvoice.US_IntermediateConsignee.ZO_OH_Organisation);
			AssertEquals("US_IntermediateConsignee.ZO_OA_Address", sourceInvoice.US_IntermediateConsignee.ZO_OA_Address, newInvoice.US_IntermediateConsignee.ZO_OA_Address);
			AssertEquals("US_IntermediateConsignee.ZO_Contact", sourceInvoice.US_IntermediateConsignee.ZO_Contact, newInvoice.US_IntermediateConsignee.ZO_Contact);
			AssertEquals("US_IntermediateConsignee.ZO_Phone", sourceInvoice.US_IntermediateConsignee.ZO_Phone, newInvoice.US_IntermediateConsignee.ZO_Phone);

			AssertEquals("US_AESOriginIndicator", sourceInvoice.US_AESOriginIndicator, newInvoice.US_AESOriginIndicator);
			AssertEquals("US_StateOfOrigin", sourceInvoice.US_StateOfOrigin, newInvoice.US_StateOfOrigin);
			AssertEquals("US_ForeignTradeZone", sourceInvoice.US_ForeignTradeZone, newInvoice.US_ForeignTradeZone);
			AssertEquals("US_ECCN", sourceInvoice.US_ECCN, newInvoice.US_ECCN);
			AssertEquals("US_RoutedTransaction", sourceInvoice.US_RoutedTransaction, newInvoice.US_RoutedTransaction);
			AssertEquals("US_TransactionsRelated", sourceInvoice.US_TransactionsRelated, newInvoice.US_TransactionsRelated);
			AssertEquals("US_LicenseType", sourceInvoice.US_LicenseType, newInvoice.US_LicenseType);
			AssertEquals("US_LicenseNo", sourceInvoice.US_LicenseNo, newInvoice.US_LicenseNo);
			AssertEquals("US_InbondType", sourceInvoice.US_InbondType, newInvoice.US_InbondType);
			AssertEquals("US_ImportEntryNo", sourceInvoice.US_ImportEntryNo, newInvoice.US_ImportEntryNo);
			AssertEquals("US_HazardousCargo", sourceInvoice.US_HazardousCargo, newInvoice.US_HazardousCargo);
			AssertEquals("US_ExportCode", sourceInvoice.US_ExportCode, newInvoice.US_ExportCode);
			AssertEquals("US_TermsOfDeliveryLocationQualifier", sourceInvoice.US_TermsOfDeliveryLocationQualifier, newInvoice.US_TermsOfDeliveryLocationQualifier);
			AssertEquals("US_TermsOfDeliveryLocationIndicator", sourceInvoice.US_TermsOfDeliveryLocationIndicator, newInvoice.US_TermsOfDeliveryLocationIndicator);
			AssertEquals("US_TermsOfDeliveryLocation", sourceInvoice.US_TermsOfDeliveryLocation, newInvoice.US_TermsOfDeliveryLocation);

			sourceInvoice.US_TermsOfDeliveryLocationQualifier = TermsOfDeliveryLocQualifierList.Codes.LoadingPort;
			sourceInvoice.US_TermsOfDeliveryLocation = "AU";
			sourceInvoice.US_TermsOfDeliveryLocationIndicator = TermsOfDeliveryLocationCodeIndicators.Codes.ISOCountryCode;

			DataRegistry.Business.USCustomsDataRegistry.Instance.AllowExportDefaultOriginIndicatorAtInvoice.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			newInvoice.US_AESOriginIndicator = ZString.Empty;
			newInvoice.CopyValueFromForExport(sourceInvoice);
			AssertEquals("Origin Indicator remains blank as registry is override to false", ZString.Empty, newInvoice.US_AESOriginIndicator);
		}

		public void TestLazyCreateUSOrganisation()
		{
			var org3 = CreateNewOrg("ORGANISATION 3", "ORG3 ADDRESS", "JULIE BROWN");
			var org4 = CreateNewOrg("ORGANISATION 4", "ORG4 ADDRESS", "BOB BROWN");
			var org5 = CreateNewOrg("ORGANISATION 5", "ORG5 ADDRESS", "JACK BROWN");
			var org6 = CreateNewOrg("ORGANISATION 6", "ORG6 ADDRESS", "MARY BROWN");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = org1.PK;
			declaration.JE_OH_Importer = org2.PK;
			declaration.JE_OH_Consignee = org3.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_OH_Supplier = org1.PK;
			invoice1.JZ_OH_Consignee = org3.PK;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_OH_Supplier = org4.PK;

			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_OH_Buyer = org5.PK;

			var invoice4 = declaration.Invoices.AddNew();
			invoice4.JZ_OH_Consignee = org6.PK;
			AssertEquals("US_USPPI.ZO_OH_Organisation", org1.PK, invoice1.US_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OA_Address", org1.MainAddress.PK, invoice1.US_USPPI.ZO_OA_Address);
			AssertEquals("US_USPPI.ZO_Contact", "BOB SMITH", invoice1.US_USPPI.ZO_Contact);
			invoice1.USPPIDocAddress.E2_Contact = "SUPCONT1";
			AssertEquals("US_USPPI.ZO_Contact", "SUPCONT1", invoice1.US_USPPI.ZO_Contact);

			AssertEquals("US_ExportUltimateConsignee.ZO_OH_Organisation", org2.PK, invoice1.US_ExportUltimateConsignee.ZO_OH_Organisation);
			AssertEquals("US_ExportUltimateConsignee.ZO_OA_Address", org2.MainAddress.PK, invoice1.US_ExportUltimateConsignee.ZO_OA_Address);
			AssertEquals("US_ExportUltimateConsignee.ZO_Contact", "JOE BROWN", invoice1.US_ExportUltimateConsignee.ZO_Contact);
			invoice1.UltimateConsigneeDocAddress.E2_Contact = "IMPCONT1";
			AssertEquals("US_ExportUltimateConsignee.ZO_Contact", "IMPCONT1", invoice1.US_ExportUltimateConsignee.ZO_Contact);

			AssertEquals("US_IntermediateConsignee.ZO_OH_Organisation", org3.PK, invoice1.US_IntermediateConsignee.ZO_OH_Organisation);
			AssertEquals("US_IntermediateConsignee.ZO_OA_Address", org3.MainAddress.PK, invoice1.US_IntermediateConsignee.ZO_OA_Address);
			AssertEquals("US_IntermediateConsignee.ZO_Contact", "JULIE BROWN", invoice1.US_IntermediateConsignee.ZO_Contact);
			invoice1.IntermediateConsigneeDocAddress.E2_Contact = "INTCONT1";
			AssertEquals("US_IntermediateConsignee.ZO_Contact", "INTCONT1", invoice1.US_IntermediateConsignee.ZO_Contact);

			AssertEquals("US_USPPI.ZO_OH_Organisation", org4.PK, invoice2.US_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OA_Address", org4.MainAddress.PK, invoice2.US_USPPI.ZO_OA_Address);
			AssertEquals("US_USPPI.ZO_Contact", "BOB BROWN", invoice2.US_USPPI.ZO_Contact);

			AssertEquals("US_ExportUltimateConsignee.ZO_OH_Organisation", org2.PK, invoice2.US_ExportUltimateConsignee.ZO_OH_Organisation);
			AssertEquals("US_ExportUltimateConsignee.ZO_OA_Address", org2.MainAddress.PK, invoice2.US_ExportUltimateConsignee.ZO_OA_Address);
			AssertEquals("US_ExportUltimateConsignee.ZO_Contact", "JOE BROWN", invoice2.US_ExportUltimateConsignee.ZO_Contact);

			AssertEquals("US_IntermediateConsignee.ZO_OH_Organisation", org3.PK, invoice2.US_IntermediateConsignee.ZO_OH_Organisation);
			AssertEquals("US_IntermediateConsignee.ZO_OA_Address", org3.MainAddress.PK, invoice2.US_IntermediateConsignee.ZO_OA_Address);
			AssertEquals("US_IntermediateConsignee.ZO_Contact", "JULIE BROWN", invoice2.US_IntermediateConsignee.ZO_Contact);

			AssertEquals("US_USPPI.ZO_OH_Organisation", org4.PK, invoice3.US_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OA_Address", org4.MainAddress.PK, invoice3.US_USPPI.ZO_OA_Address);
			AssertEquals("US_USPPI.ZO_Contact", "BOB BROWN", invoice3.US_USPPI.ZO_Contact);

			invoice3.USPPIDocAddress.E2_Contact = "SUPCONT1";
			AssertEquals("US_USPPI.ZO_Contact", "SUPCONT1", invoice3.US_USPPI.ZO_Contact);

			AssertEquals("US_ExportUltimateConsignee.ZO_OH_Organisation", org5.PK, invoice3.US_ExportUltimateConsignee.ZO_OH_Organisation);
			AssertEquals("US_ExportUltimateConsignee.ZO_OA_Address", org5.MainAddress.PK, invoice3.US_ExportUltimateConsignee.ZO_OA_Address);
			AssertEquals("US_ExportUltimateConsignee.ZO_Contact", "JACK BROWN", invoice3.US_ExportUltimateConsignee.ZO_Contact);

			AssertEquals("US_IntermediateConsignee.ZO_OH_Organisation", org3.PK, invoice3.US_IntermediateConsignee.ZO_OH_Organisation);
			AssertEquals("US_IntermediateConsignee.ZO_OA_Address", org3.MainAddress.PK, invoice3.US_IntermediateConsignee.ZO_OA_Address);
			AssertEquals("US_IntermediateConsignee.ZO_Contact", "JULIE BROWN", invoice3.US_IntermediateConsignee.ZO_Contact);

			AssertEquals("US_USPPI.ZO_OH_Organisation", org4.PK, invoice4.US_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OA_Address", org4.MainAddress.PK, invoice4.US_USPPI.ZO_OA_Address);
			AssertEquals("US_USPPI.ZO_Contact", "BOB BROWN", invoice4.US_USPPI.ZO_Contact);

			invoice4.UltimateConsigneeDocAddress.E2_Contact = "IMPCONT4";
			AssertEquals("US_ExportUltimateConsignee.ZO_Contact", "IMPCONT4", invoice4.US_ExportUltimateConsignee.ZO_Contact);

			AssertEquals("US_ExportUltimateConsignee.ZO_OH_Organisation", org5.PK, invoice4.US_ExportUltimateConsignee.ZO_OH_Organisation);
			AssertEquals("US_ExportUltimateConsignee.ZO_OA_Address", org5.MainAddress.PK, invoice4.US_ExportUltimateConsignee.ZO_OA_Address);
			AssertEquals("US_ExportUltimateConsignee.ZO_Contact", "IMPCONT4", invoice4.US_ExportUltimateConsignee.ZO_Contact);

			invoice4.UltimateConsigneeDocAddress.E2_Contact = ZString.Empty;
			invoice4.UltimateConsigneeDocAddress.E2_Phone_Formatted = ZString.Empty;
			AssertEquals("US_IntermediateConsignee.ZO_OH_Organisation", org6.PK, invoice4.US_IntermediateConsignee.ZO_OH_Organisation);
			AssertEquals("US_IntermediateConsignee.ZO_OA_Address", org6.MainAddress.PK, invoice4.US_IntermediateConsignee.ZO_OA_Address);
			AssertEquals("US_IntermediateConsignee.ZO_Contact", "MARY BROWN", invoice4.US_IntermediateConsignee.ZO_Contact);

			var invoice5 = declaration.Invoices.AddNew();
			invoice5.JZ_OH_Supplier = org4.PK;
			invoice5.JZ_OH_Buyer = org5.PK;
			invoice5.JZ_OH_Consignee = org6.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedDec = newFactory.Load<JobDeclaration>(declaration.PK);
			var reloadedInvoice5 = (JobComInvoiceHeader)reloadedDec.Invoices.FindByPK(invoice5.PK);
			AssertEquals("US_USPPI.ZO_OH_Organisation", org4.PK, reloadedInvoice5.US_USPPI.ZO_OH_Organisation);
			AssertEquals("US_USPPI.ZO_OA_Address", org4.MainAddress.PK, reloadedInvoice5.US_USPPI.ZO_OA_Address);
			AssertEquals("US_USPPI.ZO_Contact", "BOB BROWN", reloadedInvoice5.US_USPPI.ZO_Contact);

			AssertEquals("US_ExportUltimateConsignee.ZO_OH_Organisation", org5.PK, reloadedInvoice5.US_ExportUltimateConsignee.ZO_OH_Organisation);
			AssertEquals("US_ExportUltimateConsignee.ZO_OA_Address", org5.MainAddress.PK, reloadedInvoice5.US_ExportUltimateConsignee.ZO_OA_Address);
			AssertEquals("US_ExportUltimateConsignee.ZO_Contact", "JACK BROWN", reloadedInvoice5.US_ExportUltimateConsignee.ZO_Contact);

			AssertEquals("US_IntermediateConsignee.ZO_OH_Organisation", org6.PK, reloadedInvoice5.US_IntermediateConsignee.ZO_OH_Organisation);
			AssertEquals("US_IntermediateConsignee.ZO_OA_Address", org6.MainAddress.PK, reloadedInvoice5.US_IntermediateConsignee.ZO_OA_Address);
			AssertEquals("US_IntermediateConsignee.ZO_Contact", "MARY BROWN", reloadedInvoice5.US_IntermediateConsignee.ZO_Contact);
		}

		public void TestSetDefaultValues()
		{
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			AssertEquals("Default: US_InvoiceType", InvoiceTypeList.Codes.CommercialInvoice, invoice.US_InvoiceType);
			AssertEquals(ZDateTime.Empty, invoice.JZ_InvoiceDate);
		}

		public void TestJZ_OA_InvoicerDocAddress()
		{
			var invoicer1 = Factory.New<OrgHeader>();
			invoicer1.OH_Code = "INVOICER1234";
			invoicer1.OH_FullName = "INVOICER 1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OA_InvoicerAddress = invoicer1.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			AssertEquals(invoicer1.MainAddress.PK, invoice.JZ_OA_InvoicerDocAddress);
			AssertEquals(invoicer1.PK, invoice.JZ_OA_InvoicerDocAddress_ZAddress.OrgPK);
			AssertEquals(ZGuid.Empty, invoice.InvoicerDocumentaryAddress.E2_OA_Address);

			invoice.InvoicerOrgPK = ZGuid.Invalid;
			AssertEquals(ZGuid.Empty, invoice.JZ_OA_InvoicerDocAddress);
			AssertEquals(ZGuid.Invalid, invoice.JZ_OA_InvoicerDocAddress_ZAddress.OrgPK);
			AssertEquals(ZGuid.Empty, invoice.InvoicerDocumentaryAddress.E2_OA_Address);
		}

		public void TestJZ_OA_DistributorAddress()
		{
			var distributor = Factory.New<OrgHeader>();
			distributor.OH_Code = "DISTRIBUTOR";
			distributor.OH_FullName = "DISTRIBUTOR 1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OA_DistributorAddress = distributor.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			AssertEquals(distributor.MainAddress.PK, invoice.JZ_OA_DistributorAddress);
			AssertEquals(distributor.PK, invoice.JZ_OA_DistributorAddress_ZAddress.OrgPK);

			invoice.DistributorOrgPK = ZGuid.Invalid;
			AssertEquals(ZGuid.Empty, invoice.JZ_OA_DistributorAddress);
			AssertEquals(ZGuid.Invalid, invoice.JZ_OA_DistributorAddress_ZAddress.OrgPK);
		}

		public void TestJZ_OA_PackagerAddress()
		{
			var packager = Factory.New<OrgHeader>();
			packager.OH_Code = "PACKAGER1234";
			packager.OH_FullName = "PACKAGER 1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OA_PackagerAddress = packager.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			AssertEquals(packager.MainAddress.PK, invoice.JZ_OA_PackagerAddress);
			AssertEquals(packager.PK, invoice.JZ_OA_PackagerAddress_ZAddress.OrgPK);

			invoice.PackagerOrgPK = ZGuid.Invalid;
			AssertEquals(ZGuid.Empty, invoice.JZ_OA_PackagerAddress);
			AssertEquals(ZGuid.Invalid, invoice.JZ_OA_PackagerAddress_ZAddress.OrgPK);
		}

		public void TestJZ_OA_ShipperAddress()
		{
			var shipper = Factory.New<OrgHeader>();
			shipper.OH_Code = "SHIPPER1234";
			shipper.OH_FullName = "SHIPPER 1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OA_ShipperAddress = shipper.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			AssertEquals(shipper.MainAddress.PK, invoice.JZ_OA_ShipperAddress);
			AssertEquals(shipper.PK, invoice.JZ_OA_ShipperAddress_ZAddress.OrgPK);

			invoice.ShipperOrgPK = ZGuid.Invalid;
			AssertEquals(ZGuid.Empty, invoice.JZ_OA_ShipperAddress);
			AssertEquals(ZGuid.Invalid, invoice.JZ_OA_ShipperAddress_ZAddress.OrgPK);
		}

		public void TestUS_FirstSale()
		{
			var importer = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));

			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			var link = importer.SupplierLinks.AddNew(supplier);
			link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;
			var addInfo = link.GetAddInfo();
			addInfo.ZO_FirstSale = YesNoDefaultList.Codes.No;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKFinalDestination = "USCHI";

			var invoice = declaration.Invoices.AddNew();
			AssertEquals("Should be defaulted from Supplier/Buyer link", YesNoDefaultList.Codes.No, invoice.US_FirstSale);

			invoice.US_FirstSale = YesNoDefaultList.Codes.Yes;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(YesNoDefaultList.Codes.Yes, invoiceLine.US_FirstSale);

			invoiceLine.US_FirstSale = YesNoDefaultList.Codes.No;
			AssertEquals(YesNoDefaultList.Codes.No, invoiceLine.US_FirstSale);

			invoice.US_FirstSale = YesNoDefaultList.Codes.No;
			AssertEquals(YesNoDefaultList.Codes.No, invoiceLine.US_FirstSale);

			invoice.US_FirstSale = YesNoDefaultList.Codes.Yes;
			AssertEquals(YesNoDefaultList.Codes.Yes, invoiceLine.US_FirstSale);
		}

		public void TestJZ_OA_ManufacturerAddress()
		{
			OrgHeader manufacturer1 = Factory.New<OrgHeader>();
			manufacturer1.OH_Code = "MANU1234";
			manufacturer1.OH_FullName = "MANUFACTURER 1";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OA_ManufacturerAddress = manufacturer1.MainAddress.PK;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			AssertEquals(manufacturer1.MainAddress.PK, invoice.JZ_OA_ManufacturerAddress);
			AssertEquals(manufacturer1.PK, invoice.JZ_OA_ManufacturerAddress_ZAddress.OrgPK);

			invoice.ManufacturerOrgPK = ZGuid.Invalid;
			AssertEquals(ZGuid.Empty, invoice.JZ_OA_ManufacturerAddress);
			AssertEquals(ZGuid.Invalid, invoice.JZ_OA_ManufacturerAddress_ZAddress.OrgPK);
			declaration.JE_OA_ManufacturerAddress = ZGuid.Empty;

			JobComInvoiceLine line = invoice.InvoiceLines.AddNew();
			FDA fdaLine = line.FDAs.AddNew();
			AssertEquals(ZGuid.Empty, fdaLine.US_FDAManufacturerAddress_ZAddress.OrgPK);

			invoice.JZ_OA_ManufacturerAddress = manufacturer1.MainAddress.PK;
			AssertEquals(manufacturer1.PK, fdaLine.US_FDAManufacturerAddress_ZAddress.OrgPK);
		}

		public void TestJZ_OA_SupplierAddress()
		{
			OrgHeader supplier1 = Factory.New<OrgHeader>();
			supplier1.OH_Code = "SUPPLIER1234";
			supplier1.OH_FullName = "SUPPLIER 1";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = supplier1.PK;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			AssertEquals(supplier1.MainAddress.PK, invoice.JZ_OA_SupplierAddress);
			AssertEquals(supplier1.PK, invoice.JZ_OA_SupplierAddress_ZAddress.OrgPK);

			invoice.JZ_OH_Supplier = ZGuid.Invalid;
			AssertEquals(ZGuid.Empty, invoice.JZ_OA_SupplierAddress);
			AssertEquals(ZGuid.Invalid, invoice.JZ_OA_SupplierAddress_ZAddress.OrgPK);
			AssertEquals(ZGuid.Invalid, invoice.JZ_OH_Supplier);

			invoice.JZ_OH_Supplier = supplier1.PK;
			AssertEquals(supplier1.MainAddress.PK, invoice.JZ_OA_SupplierAddress);
			AssertEquals(supplier1.PK, invoice.JZ_OA_SupplierAddress_ZAddress.OrgPK);
			AssertEquals(supplier1.PK, invoice.JZ_OH_Supplier);

			invoice.JZ_AddInfo = ZString.Empty;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobComInvoiceHeader reloadInvoice = newFactory.Load<JobComInvoiceHeader>(invoice.PK);
			AssertEquals(supplier1.MainAddress.PK, reloadInvoice.JZ_OA_SupplierAddress);
			AssertEquals(supplier1.PK, reloadInvoice.JZ_OA_SupplierAddress_ZAddress.OrgPK);
			AssertEquals(supplier1.PK, reloadInvoice.JZ_OH_Supplier);

			reloadInvoice.JZ_OH_Supplier = ZGuid.Empty;
			reloadInvoice.JZ_OA_SupplierAddress = supplier1.MainAddress.PK;
			newFactory.Save();

			BusinessObjectFactory newFactory1 = new BusinessObjectFactory();
			JobComInvoiceHeader reloadInvoice1 = newFactory1.Load<JobComInvoiceHeader>(invoice.PK);
			AssertEquals(supplier1.MainAddress.PK, reloadInvoice1.JZ_OA_SupplierAddress);
			AssertEquals(supplier1.PK, reloadInvoice1.JZ_OA_SupplierAddress_ZAddress.OrgPK);
			AssertEquals(supplier1.PK, reloadInvoice1.JZ_OH_Supplier);
		}

		public void TestJZ_OA_ExporterAddress()
		{
			OrgHeader exporter = Factory.New<OrgHeader>();
			exporter.OH_Code = "SUPPLIER1234";
			exporter.OH_FullName = "SUPPLIER 1";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Exporter = exporter.PK;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			AssertEquals(exporter.MainAddress.PK, invoice.JZ_OA_ExporterAddress);
			AssertEquals(exporter.PK, invoice.JZ_OA_ExporterAddress_ZAddress.OrgPK);

			invoice.ExporterOrgPK = ZGuid.Invalid;
			AssertEquals(ZGuid.Empty, invoice.JZ_OA_ExporterAddress);
			AssertEquals(ZGuid.Invalid, invoice.JZ_OA_ExporterAddress_ZAddress.OrgPK);
			AssertEquals(ZGuid.Invalid, invoice.ExporterOrgPK);

			invoice.ExporterOrgPK = exporter.PK;
			AssertEquals(exporter.MainAddress.PK, invoice.JZ_OA_ExporterAddress);
			AssertEquals(exporter.PK, invoice.JZ_OA_ExporterAddress_ZAddress.OrgPK);
			AssertEquals(exporter.PK, invoice.ExporterOrgPK);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobComInvoiceHeader reloadInvoice = newFactory.Load<JobComInvoiceHeader>(invoice.PK);
			AssertEquals(exporter.MainAddress.PK, reloadInvoice.JZ_OA_ExporterAddress);
			AssertEquals(exporter.PK, reloadInvoice.JZ_OA_ExporterAddress_ZAddress.OrgPK);
			AssertEquals(exporter.PK, reloadInvoice.ExporterOrgPK);
		}

		public void TestFDAShipper()
		{
			var supplier1 = Factory.New<OrgHeader>();
			supplier1.OH_Code = "SUPPLIER1234";
			supplier1.OH_FullName = "SUPPLIER 1";

			var invoicer1 = Factory.New<OrgHeader>();
			invoicer1.OH_Code = "INVOICER1234";
			invoicer1.OH_FullName = "INVOICER 1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier1.PK;
			AssertEquals("FDAShipper should fall back to supplier", supplier1.MainAddress.PK, invoice.JZ_OA_FDAShipperAddress);

			declaration.JE_OA_InvoicerAddress = invoicer1.MainAddress.PK;
			AssertEquals("FDAShipper should default to invoicer org in preference", invoicer1.MainAddress.PK, invoice.JZ_OA_FDAShipperAddress);
		}

		public void TestFDAShipperIsEffective()
		{
			var supplier1 = Factory.New<OrgHeader>();
			supplier1.OH_Code = "SUPPLIER1";
			supplier1.OH_FullName = "SUPPLIER 1";

			var supplier2 = Factory.New<OrgHeader>();
			supplier2.OH_Code = "SUPPLIER2";
			supplier2.OH_FullName = "SUPPLIER 2";

			var supplier3 = Factory.New<OrgHeader>();
			supplier3.OH_Code = "SUPPLIER3";
			supplier3.OH_FullName = "SUPPLIER 3";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier1.PK;
			AssertEquals("FDAShipper should be supplier 1 effectively", supplier1.MainAddress.PK, invoice.JZ_OA_FDAShipperAddress);

			declaration.JE_OA_InvoicerAddress = supplier2.MainAddress.PK;
			AssertEquals("FDAShipper should now be supplier 2 effectively", supplier2.MainAddress.PK, invoice.JZ_OA_FDAShipperAddress);

			invoice.JZ_OH_Supplier = supplier1.PK;
			AssertEquals("FDAShipper should remain as Invoicer (supplier 2) effectively", supplier2.MainAddress.PK, invoice.JZ_OA_FDAShipperAddress);

			invoice.JZ_OA_FDAShipperAddress = supplier3.MainAddress.PK;
			AssertEquals("FDAShipper should now be supplier 3 actually", supplier3.MainAddress.PK, invoice.JZ_OA_FDAShipperAddress);

			declaration.JE_OA_InvoicerAddress = supplier3.MainAddress.PK;
			AssertEquals("FDAShipper should now be supplier 3 effectively as value is same as entered in FDAShipper field", supplier3.MainAddress.PK, invoice.JZ_OA_FDAShipperAddress);

			invoice.JZ_OA_FDAShipperAddress = ZGuid.Empty;
			declaration.JE_OA_InvoicerAddress = ZGuid.Empty;
			AssertEquals("FDAShipper should revert to Supplier (supplier 1) effectively again", supplier1.MainAddress.PK, invoice.JZ_OA_FDAShipperAddress);
		}

		public void TestAIIStatusCustomsNarrative()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			invoice.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.ElectronicInvoice));
			Factory.Save();

			MQEDIMessage message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse);
			invoice.Messages.Add(message);
			message.EM_MessageText =
"B018888XJ5CR                                               25077                " +
"E00XOJOHMAT130BRA 24432                EJT  ERROR-FREE INVOICE ACKNOWLEDGED     " +
"Y  8888XJ5CR00001";
			Factory.Save();

			AssertEquals("EJT - ERROR-FREE INVOICE ACKNOWLEDGED", invoice.AIIStatusCustomsNarrative);
		}

		public void TestAIIStatus()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.ElectronicInvoice));
			invoice.JZ_MessageStatus = MessageStatusListEI.Codes.AwaitingElectronicInvoiceOriginal;
			Factory.Save();

			AssertEquals("AII Msg Status Code - AIO", MessageStatusListEI.Codes.AwaitingElectronicInvoiceOriginal, invoice.AIIStatus);
		}

		public void TestAIIStatusDescription()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.ElectronicInvoice));
			invoice.JZ_MessageStatus = MessageStatusListEI.Codes.AwaitingElectronicInvoiceOriginal;
			MQEDIMessage message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse);
			invoice.Messages.Add(message);
			invoice.JZ_MessageStatus = MessageStatusListEI.Codes.ErrorElectronicInvoiceOriginal;
			Factory.Save();

			AssertEquals("AII Status Description", MessageStatusListEI.Descriptions.ErrorElectronicInvoiceOriginal, invoice.AIIStatusDescription);
		}

		public void TestAIIERecords()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			invoice.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.ElectronicInvoice));
			Factory.Save();

			MQEDIMessage message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse);
			invoice.Messages.Add(message);
			message.EM_MessageText =
"B018888XJ5CR                                               11417                " +
"C01ADEROLFEN15HAN  345              061508INUSD 0000000                         " +
"C60                                                                             " +
"E60DEROLFEN15HAN  345              000120T  ADDRESS REQUIRED                    " +
"C60                                                                             " +
"E60DEROLFEN15HAN  345              000220T  ADDRESS REQUIRED                    " +
"C60                                                                             " +
"E60DEROLFEN15HAN  345              000320T  ADDRESS REQUIRED                    " +
"C60                                                                             " +
"E60DEROLFEN15HAN  345              000420T  ADDRESS REQUIRED                    " +
"E95DEROLFEN15HAN  345                  524  TRANSACTION DATA REJECTED           " +
"Y  8888XJ5CR00010";
			Factory.Save();

			AssertEquals(5, invoice.AIIERecords.Count);
			AssertEquals("DEROLFEN15HAN", invoice.AIIERecords[4].InvoicingPartyID);
			AssertEquals("345", invoice.AIIERecords[4].InvoiceNumber);
			AssertEquals("0", invoice.AIIERecords[4].InvoiceLine);
			AssertEquals("524", invoice.AIIERecords[4].ErrorMessageIdentifier);
			AssertEquals(USConstants.NarrativeRejectedMessage, invoice.AIIERecords[4].ErrorMessage);
		}

		public void TestIsUSUltimateConsignee()
		{
			OrgHeader consignee = Factory.New<OrgHeader>();
			RefUNLOCO unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "BEMTR";
			consignee.OH_RL_NKClosestPort = unloco.RL_Code;
			InvoiceHeader.JZ_OA_ConsigneeAddress = consignee.MainAddress.PK;
			AssertEquals(false, InvoiceHeader.IsUSUltimateConsignee);

			unloco.RL_Code = "USLAX";
			consignee.OH_RL_NKClosestPort = unloco.RL_Code;
			AssertEquals(true, InvoiceHeader.IsUSUltimateConsignee);
		}

		public void TestDefaultRelatedPartyIfPossible()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_RL_NKClosestPort = "USLAX";
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_OH_Importer = importer.PK;

			OrgHeader supplier = Factory.New<OrgHeader>();
			OrgSupplierBuyerLink link = supplier.BuyerLinks.AddNew();
			link.OL_OH_Buyer = importer.PK;
			link.OL_RelatedParty = "";
			InvoiceHeader.JZ_OH_Supplier = supplier.PK;

			AssertEquals("Related Party", "", InvoiceHeader.US_TransactionsRelated);

			OrgHeader supplier2 = Factory.New<OrgHeader>();

			link.OL_RelatedParty = "Y";
			InvoiceHeader.JZ_OH_Supplier = supplier2.PK;
			InvoiceHeader.JZ_OH_Supplier = supplier.PK;
			AssertEquals("Related Party", "Y", InvoiceHeader.US_TransactionsRelated);

			InvoiceHeader.JZ_OH_Supplier = supplier2.PK;
			AssertEquals("Related Party", "", InvoiceHeader.US_TransactionsRelated);
		}

		public void TestEffectiveContact()
		{
			Declaration.US_FDAContactName = "John";
			AssertEquals("John", InvoiceHeader.US_FDAContactName);

			InvoiceHeader.US_FDAContactName = "Bill";
			AssertEquals("Bill", InvoiceHeader.US_FDAContactName);

			Declaration.US_FDAContactPhoneNo = "7184255000";
			AssertEquals("7184255000", InvoiceHeader.US_FDAContactPhoneNo);

			InvoiceHeader.US_FDAContactPhoneNo = "3489200316";
			AssertEquals("3489200316", InvoiceHeader.US_FDAContactPhoneNo);

			Declaration.US_FDAContactEmail = "john@somewhere.com";
			AssertEquals("john@somewhere.com", InvoiceHeader.US_FDAContactEmail);

			InvoiceHeader.US_FDAContactEmail = "bill@somewhere.com";
			AssertEquals("bill@somewhere.com", InvoiceHeader.US_FDAContactEmail);
		}

		public void TestInvCurrFDAValueRefreshed()
		{
			using (DeclarationTestHelper.SetReciprocalFlagForCurrentCompany(true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				declaration.US_EnableENS = true;
				declaration.US_EntryFilerCode = "XJ5";

				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				invoice.JZ_InvoiceAmount = 1000m;
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
				invoiceLine.JI_LinePrice = 1000m;

				var fdaLine1 = invoiceLine.FDAs.AddNew();
				fdaLine1.US_InvCurrFDAValue = 700m;

				var fdaLine2 = invoiceLine.FDAs.AddNew();
				fdaLine2.US_InvCurrFDAValue = 300m;
				declaration.ResumeApportionment();
				AssertEquals("FDA value displayed", "700", fdaLine1.FDAValue);
				AssertEquals("FDA value displayed", "300", fdaLine2.FDAValue);

				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("FDA value calcualted", 700m, fdaLine1.US_FDAValue);
				AssertEquals("FDA value calcualted", 300m, fdaLine2.US_FDAValue);

				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Singapore;
				invoice.JZ_InvoiceCurrExRate = 0.5m;
				invoice.IsJZ_InvoiceCurrExRateUserEnterable = true;
				declaration.ResumeApportionment();
				AssertEquals("FDA value displayed", "...", fdaLine1.FDAValue);
				AssertEquals("FDA value displayed", "...", fdaLine2.FDAValue);

				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("value should be re-calculated as currency has changed", 350m, fdaLine1.US_FDAValue);
				AssertEquals("value should be re-calculated as currency has changed", 150m, fdaLine2.US_FDAValue);
			}
		}

		public void TestUS_DES()
		{
			Declaration.US_DES = "A001";
			AssertEquals("A001", InvoiceHeader.US_DES);

			InvoiceHeader.US_DES = "A002";
			AssertEquals("A002", InvoiceHeader.US_DES);
		}

		public void TestDefaultAdditionalReconDetailsFromSupplierImporterLink()
		{
			var declarationSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			var declarationImporter = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = declarationSupplier.PK;
			declaration.JE_OH_Importer = declarationImporter.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKFinalDestination = "USCHI";

			var link1 = declarationImporter.SupplierLinks.AddNew(declarationSupplier);
			link1.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;
			var addInfo = link1.GetAddInfo();
			addInfo.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.ClassRecon;
			addInfo.ZO_FirstSale = YesNoDefaultList.Codes.Yes;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Buyer = declarationImporter.PK;
			Factory.Save();
			AssertEquals("First Sale from supplier link", YesNoDefaultList.Codes.Yes, invoice.US_FirstSale);
			AssertEquals("Other Recon Indicator", ReconIssueCodeList.Codes.ClassRecon, declaration.US_OtherReconIndicator);

			var invoiceSupplier = Factory.NewWithValidTestData<OrgHeader>();
			var invoiceImporter = Factory.NewWithValidTestData<OrgHeader>();

			var link2 = invoiceImporter.SupplierLinks.AddNew(invoiceSupplier);
			link2.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;
			var addInfo2 = link2.GetAddInfo();
			addInfo2.ZO_FirstSale = YesNoDefaultList.Codes.No;
			addInfo2.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.ValueRecon;

			invoice.JZ_OH_Supplier = invoiceSupplier.PK;
			invoice.JZ_OH_Buyer = invoiceImporter.PK;
			Factory.Save();
			AssertEquals("Other Recon Indicator", ReconIssueCodeList.Codes.ValueClassRecon, declaration.US_OtherReconIndicator);
		}

		public void TestHasInvoiceLinesWithPGARequireIOR()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			Assert(!invoice.HasInvoiceLinesWithNHTSARequireIOR);

			var nhtsa = invoiceLine.NHTSALines.AddNew();
			Assert(!invoice.HasInvoiceLinesWithNHTSARequireIOR);

			var document = nhtsa.NHTSADocuments.AddNew();
			document.US_NHTDocumentOwner = NHTSAOrganizationTypeList.Codes.Importer;
			Assert(invoice.HasInvoiceLinesWithNHTSARequireIOR);
			document.US_NHTDocumentOwner = NHTSAOrganizationTypeList.Codes.FabricatingManufacturer;
			Assert(!invoice.HasInvoiceLinesWithNHTSARequireIOR);
		}

		[ExpectNoExceptions]
		public void TestAdvanceShippingNotice_PartWithMultipleClassification()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.FillWithValidTestData();
			consignor.OH_Code = "ORGAUSYD";
			consignor.OH_IsConsignor = ZBool.True;
			consignor.OH_RL_NKClosestPort = "AUSYD";
			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_Code = "ORGUSCHI";
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_RL_NKClosestPort = "USCHI";

			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART1";
			part.OP_Weight = 100m;
			part.OP_WeightUQ = "HG";
			part.RelatedOrganisations.AddSupplier(consignor);
			part.RelatedOrganisations.AddOwner(consignee);
			CusClassPartPivot pivot = part.PivotsForBinding.AddNew();
			pivot.CI_UsageComment = "U1";
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1010101010";
			pivot.CI_SupplementalTariff = "2020202020";
			pivot.CI_CC = classification.PK;
			pivot.CI_OP = part.PK;
			CusClassPartPivot pivotChild1 = pivot.Children.AddNew();
			pivotChild1.CI_UsageComment = "CU1";
			pivotChild1.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			pivotChild1.CI_TariffNum = "1010101011";
			pivotChild1.CI_SupplementalTariff = "2020202021";
			pivotChild1.CI_CC = classification.PK;
			pivotChild1.CI_OP = part.PK;
			CusClassPartPivot pivotChild2 = pivot.Children.AddNew();
			pivotChild2.CI_UsageComment = "CU2";
			pivotChild2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivotChild2.CI_TariffNum = "1010101012";
			pivotChild2.CI_SupplementalTariff = "2020202022";
			pivotChild2.CI_CC = classification.PK;
			pivotChild2.CI_OP = part.PK;

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_StandAloneInvoiceDirection = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_OH_Buyer = consignee.PK;
			invoice.JZ_OH_Supplier = consignor.PK;

			new FakeDeclarationCreatorForInvoice(invoice);
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_PartNo = "PART1";
			AssertEquals("JI_OP should be null for ASN invoice", ZGuid.Empty, invoiceLine1.JI_OP);
			AssertEquals("JI_Weight has NOT been defaulted", 0m, invoiceLine1.JI_Weight);
			AssertNotEquals("JI_WeightUQ has NOT been defaulted", "HG", invoiceLine1.JI_WeightUQ);
			JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(invoiceLine1.Factory, invoiceLine1.PartSyncManagerActiveDeciderPK);

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = "";
			var childInvoiceLine = invoice.InvoiceLines.AddNew();
			childInvoiceLine.JI_ParentID = invoiceLine2.PK;

			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("JI_OP should be set for IMP invoice", part.PK, invoiceLine1.JI_OP);
			AssertEquals("JI_Weight has been defaulted", 100m, invoiceLine1.JI_Weight);
			AssertEquals("JI_WeightUQ has been defaulted", "HG", invoiceLine1.JI_WeightUQ);
			AssertEquals("Child Invoice Lines with product details should be created", 5, invoice.InvoiceLines.Count);
			Assert("Child InvoiceLine without product detail should not be deleted", !childInvoiceLine.IsDeleted);

			invoice.JZ_MessageType = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			AssertEquals("JI_OP should be null for ASN invoice", ZGuid.Empty, invoiceLine1.JI_OP);
			invoiceLine1.JI_Weight = 0m;
			invoiceLine1.JI_WeightUQ = ZString.Empty;
			AssertEquals("Child Invoice Lines with product details should be deleted", 3, invoice.InvoiceLines.Count);
			Assert("Child InvoiceLine without product detail should not be deleted", !childInvoiceLine.IsDeleted);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			declaration.Invoices.Add(invoice);
			AssertEquals("Product data defaulted to the invoice line", part.PK, invoiceLine1.JI_OP);
			AssertEquals("invoiceLine.JI_Weight", 100m, invoiceLine1.JI_Weight);
			AssertEquals("invoiceLine.JI_WeightUQ", "HG", invoiceLine1.JI_WeightUQ);
			AssertEquals("Child Invoice Lines with product details should be created", 5, declaration.InvoiceLines.Count);
			Assert("Child InvoiceLine without product detail should not be deleted", !childInvoiceLine.IsDeleted);

			new FakeDeclarationCreatorForInvoice(invoice);
			invoice.JZ_MessageType = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			AssertEquals("JZ_StandAloneInvoiceDirection", "", invoice.JZ_StandAloneInvoiceDirection);
			AssertEquals("JZ_MessageType", "ASN", invoice.JZ_MessageType);
			AssertEquals("JI_OP should be null for ASN invoice", ZGuid.Empty, invoiceLine1.JI_OP);
			Factory.Save();
			AssertEquals("JZ_StandAloneInvoiceDirection", "ASN", invoice.JZ_StandAloneInvoiceDirection);
			AssertEquals("Child Invoice Lines with product details should be deleted", 3, invoice.InvoiceLines.Count);
			Assert("Child InvoiceLine without product detail should not be deleted", !childInvoiceLine.IsDeleted);

			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine2.JI_PartNo = "PART1";
			AssertEquals("Child Invoice Lines should be created for invoiceLine2", 6, invoice.InvoiceLines.Count);
		}

		[TestDate(2016, 11, 20)]
		public void TestAdditionalInformation()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "Importer Company";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_RN_NKCountryOfDestination = "HK";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_OH_Buyer = orgHeader.PK;
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.US_LicenseType = USAESLicenseCode.Codes.C30;
			invoiceLine1.US_ECCN = "9A610";
			invoiceLine1.US_LicenseNo = "AAA";

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.US_LicenseType = USAESLicenseCode.Codes.C30;
			invoiceLine2.US_ECCN = "9A515";
			invoiceLine2.US_LicenseNo = "NLR";
			invoiceLine2.US_DDTCITARExemptionNo = "123.9E";

			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.US_LicenseType = USAESLicenseCode.Codes.C30;
			invoiceLine3.US_ECCN = "9A444";

			var expectResult = DataRegistry.Business.USCustomsDataRegistry.Instance.DestinationControlStatement.Value + " ECCNs: 9A610, 9A515";
			AssertEquals(expectResult, invoiceHeader.AdditionalInformation);

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_OH_Buyer = orgHeader.PK;
			var header2InvoiceLine1 = invoiceHeader2.InvoiceLines.AddNew();
			header2InvoiceLine1.US_LicenseType = USAESLicenseCode.Codes.C45;
			header2InvoiceLine1.US_ECCN = "9A610";
			header2InvoiceLine1.US_LicenseNo = "AAA";

			var header2InvoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			header2InvoiceLine2.US_LicenseType = USAESLicenseCode.Codes.C43;
			header2InvoiceLine2.US_ECCN = "9A515";
			header2InvoiceLine2.US_LicenseNo = "BBB";

			AssertEquals("", invoiceHeader2.AdditionalInformation);

			var invoiceHeader3 = declaration.Invoices.AddNew();
			invoiceHeader3.JZ_OH_Buyer = orgHeader.PK;
			var header3InvoiceLine1 = invoiceHeader3.InvoiceLines.AddNew();
			header3InvoiceLine1.US_LicenseType = USAESLicenseCode.Codes.N01;
			header3InvoiceLine1.US_ECCN = "9A610";
			header3InvoiceLine1.US_LicenseNo = "AAA";

			var header3InvoiceLine2 = invoiceHeader3.InvoiceLines.AddNew();
			header3InvoiceLine2.US_LicenseType = USAESLicenseCode.Codes.S00;
			header3InvoiceLine2.US_ECCN = "9A515";
			header3InvoiceLine2.US_LicenseNo = "NLR";
			header3InvoiceLine2.US_DDTCITARExemptionNo = "123.9E";

			expectResult = DataRegistry.Business.USCustomsDataRegistry.Instance.DestinationControlStatement.Value + " ECCNs: 9A610, 9A515 Country of Destination: HK End-User: Importer Company License/Approval/Exemption: 123.9E";
			AssertEquals(expectResult, invoiceHeader3.AdditionalInformation);
		}

		[TestDate(2016, 11, 20)]
		public void TestAdditionInformationShouldBeEmptyIfDeclarationIsNull()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			AssertNull("Precondition", invoiceHeader.JobDeclaration);
			AssertEquals("AdditionalInfomation should be empty as the JobDeclaration is null.", ZString.Empty, invoiceHeader.AdditionalInformation);
		}

		public void TestSettingSupplierDefaultsDependOnIsInitialised()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.DefaultManufacturerFromSupplier.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			DataRegistry.Business.USCustomsDataRegistry.Instance.DefaultSellerFromSupplier.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			OrgHeader oh = Factory.New<OrgHeader>();
			oh.FillWithValidTestData();
			OrgAddress oa = oh.MainAddress;

			OrgHeader oh1 = Factory.New<OrgHeader>();
			oh1.FillWithValidTestData();
			OrgAddress oa1 = oh1.MainAddress;

			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = ZGuid.Empty;
			AssertEquals(false, invoice.IsAttachedToPersistentDeclaration);
			invoice.JZ_OA_SupplierAddress = oa.PK;

			AssertEquals("Manufacturer should default", oa.PK, invoice.JZ_OA_ManufacturerAddress);
			AssertEquals("Seller should default", oa.PK, invoice.JZ_OA_SellerAddress);

			invoice.JZ_OA_ManufacturerAddress = oa1.PK;
			invoice.JZ_OA_SellerAddress = oa1.PK;
			Factory.Save();

			var addinfoheader = new AddInfoJobComInvoiceHeader(invoice.JZ_AddInfoInfo);

			AssertEquals("Manufacturer should not default to Supplier", oa1.PK, invoice.JZ_OA_ManufacturerAddress);
			AssertEquals("Seller should not default to Supplier", oa1.PK, invoice.JZ_OA_SellerAddress);

			JobDeclaration declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice1 = declaration1.Invoices.AddNew();
			AssertEquals(true, invoice1.IsAttachedToPersistentDeclaration);
			invoice1.JZ_OA_SupplierAddress = oa.PK;

			AssertEquals("Manufacturer should not default to Supplier", oa.PK, invoice1.JZ_OA_ManufacturerAddress);
			AssertEquals("Seller should not default to Supplier", ZGuid.Empty, invoice1.JZ_OA_SellerAddress);
		}

		public void TestSettingSupplierDefaultsManufacturer()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_OA_SupplierAddress = PartyInForeignCountry.MainAddress.PK;
			AssertEquals("Manufacturer should not default", ZGuid.Empty, invoice.JZ_OA_ManufacturerAddress);

			DataRegistry.Business.USCustomsDataRegistry.Instance.DefaultManufacturerFromSupplier.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			invoice.JZ_OA_SupplierAddress = ZGuid.Empty;
			invoice.JZ_OA_SupplierAddress = PartyInForeignCountry.MainAddress.PK;
			AssertEquals("Manufacturer should not default to Supplier if it's not standalone Invoice", PartyInForeignCountry.MainAddress.PK, invoice.JZ_OA_ManufacturerAddress);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoice.JZ_OA_SupplierAddress = ZGuid.Empty;
			invoice.JZ_OA_ManufacturerAddress = ZGuid.Empty;
			invoice.JZ_OA_SupplierAddress = PartyInCurrentCountry.PK;
			AssertEquals("Manufacturer should not default for Non Import declarations", ZGuid.Empty, invoice.JZ_OA_ManufacturerAddress);
		}

		public void TestSettingSupplierDefaultsSeller()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_OA_SupplierAddress = PartyInForeignCountry.MainAddress.PK;
			AssertEquals("Seller should not default", ZGuid.Empty, invoice.JZ_OA_SellerAddress);

			DataRegistry.Business.USCustomsDataRegistry.Instance.DefaultSellerFromSupplier.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			invoice.JZ_OA_SupplierAddress = ZGuid.Empty;
			invoice.JZ_OA_SupplierAddress = PartyInForeignCountry.MainAddress.PK;
			AssertEquals("Seller should not default to Supplier if it's not standalone Invoice", ZGuid.Empty, invoice.JZ_OA_SellerAddress);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoice.JZ_OA_SupplierAddress = ZGuid.Empty;
			invoice.JZ_OA_SellerAddress = ZGuid.Empty;
			invoice.JZ_OA_SupplierAddress = PartyInCurrentCountry.MainAddress.PK;
			AssertEquals("Seller should not default for Non Import declarations", ZGuid.Empty, invoice.JZ_OA_SellerAddress);
		}

		public void TestSettingSupplierOnInvoiceOverrisesDeclarationDefaultedManufacturer()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.DefaultManufacturerFromSupplier.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			declaration.JE_OH_Supplier = PartyInForeignCountry.PK;
			AssertEquals("Manufacturer should default to Supplier at Declaration level", PartyInForeignCountry.MainAddress.PK, declaration.JE_OA_ManufacturerAddress);
			AssertEquals("and flow through to default to Supplier at Invoice Header level", PartyInForeignCountry.MainAddress.PK, invoice.JZ_OA_ManufacturerAddress);

			invoice.JZ_OA_SupplierAddress = PartyInCurrentCountry.MainAddress.PK;
			AssertEquals("Manufacturer should default to Supplier at Invoice Level if it's not standalone Invoice", PartyInCurrentCountry.MainAddress.PK, invoice.JZ_OA_ManufacturerAddress);
			AssertNotEquals("invoice manufacturer should now default to declaration manufacturer when Invoice Supplier not same as Declaration Supplier", declaration.JE_OA_ManufacturerAddress, invoice.JZ_OA_ManufacturerAddress);

			invoice.JZ_OA_ManufacturerAddress = PartyInCurrentCountry.MainAddress.PK;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OA_ManufacturerAddress = PartyInForeignCountry.PK;
			AssertEquals("declaration manufacturer if changed should not re-default over invoice value", PartyInCurrentCountry.MainAddress.PK, invoice.JZ_OA_ManufacturerAddress);
		}

		public void TestSettingSupplierOnInvoiceOverrisesDeclarationDefaultedSeller()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.DefaultSellerFromSupplier.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			declaration.JE_OH_Supplier = PartyInForeignCountry.PK;
			AssertEquals("Seller should default to Supplier at Declaration level", PartyInForeignCountry.MainAddress.PK, declaration.JE_OA_SellerAddress);
			AssertEquals("and flow through to default to Supplier at Invoice Header level", PartyInForeignCountry.MainAddress.PK, invoice.JZ_OA_SellerAddress);

			invoice.JZ_OA_SupplierAddress = PartyInCurrentCountry.MainAddress.PK;
			AssertNotEquals("Seller should not default to Supplier at Invoice Level if it's not standalone Invoice", PartyInCurrentCountry.MainAddress.PK, invoice.JZ_OA_SellerAddress);
			AssertEquals("invoice seller should now default to declaration seller", declaration.JE_OA_SellerAddress, invoice.JZ_OA_SellerAddress);

			invoice.JZ_OA_SellerAddress = PartyInCurrentCountry.MainAddress.PK;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OA_SellerAddress = PartyInForeignCountry.PK;
			AssertNotEquals("declaration seller if changed should not re-default over invoice value", declaration.JE_OA_SellerAddress, invoice.JZ_OA_SellerAddress);
		}

		public void TestUS_SplitShipmentDetail_RelatedHuseBillHasChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = "HB";
			houseBill1.CU_BillNum = "HB1";
			houseBill1.US_SESplitShip = true;
			var billSplitDetail = houseBill1.ITAndSplitDetails.AddNew();
			billSplitDetail.US_ArrivalDate = new ZDateTime(2021, 04, 28);
			billSplitDetail.US_CarrierCode = "A2";
			billSplitDetail.US_FlightNumber = "04A";
			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = "HB";
			houseBill2.CU_BillNum = "HB2";
			houseBill2.US_SESplitShip = false;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = houseBill1.PK;
			invoice.US_SplitShipmentDetail = invoice.AddInfoLookups.SplitShipmentDetailsList[0].Code;
			AssertNotEquals(ZString.Empty, invoice.US_SplitShipmentDetail);
			invoice.JZ_CU_RelatedHouseBill = houseBill2.PK;
			AssertEquals(ZString.Empty, invoice.US_SplitShipmentDetail);
		}

		public void TestIShouldUpdateScreeningStatus()
		{
			var org = Factory.New<OrgHeader>();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Consignee = org.PK;
			Assert("ShouldUpdateScreeningStatus", ((IShouldUpdateScreeningStatus)invoice).ShouldUpdateScreeningStatus);
		}

		public void TestDeleteInvoice()
		{
			var invoice = this.Declaration.Invoices.AddNew();
			AssertEquals("Invoice can be deleted.", true, invoice.CanDelete);

			invoice.JZ_MessageStatus = ImportMessageStatusList.Codes.AwaitingElectronicInvoiceOriginal;
			AssertEquals("Invoice can't be deleted as it's waiting for response.", false, invoice.CanDelete);
			AssertEquals("Electronic invoice message(s) exist that are pending response(s) from Customs. Please wait for the response(s) from Customs before delete the invoice(s).", invoice.ReasonForNotAbleToDelete);

			invoice.JZ_MessageStatus = ImportMessageStatusList.Codes.ClearElectronicInvoiceOriginal;
			AssertEquals("Invoice can be deleted.", true, invoice.CanDelete);
			AssertEquals("You are about to delete Invoices that have been sent to Customs electronically.\r\n", invoice.GetWarningBeforeBeingDeleted());

			var invoiceLine = invoice.InvoiceLines.AddNew();
			var lineRagne = invoiceLine.LineGroupingRanges.AddNew();
			var aiiLine = invoiceLine.AIILines.AddNew();

			var message = invoice.Messages.AddNew(typeof(MQEDIMessage));

			var entry = this.Declaration.CustomsEntryHeaders.AddNew();
			entry.US7501DocPrintingData.AddNew();

			invoice.CleanUpInvoiceAfterDetachedOrDeleted("Deleted");
			AssertEquals(0, invoice.Messages.Count);
			AssertEquals(0, invoiceLine.LineGroupingRanges.Count);
			AssertEquals(0, invoiceLine.AIILines.Count);
			AssertEquals(0, entry.US7501DocPrintingData.Count);
			AssertEquals(this.Declaration.PK, message.EM_LinkUniqueID);

			var declaration = Factory.Load<JobDeclaration>(this.Declaration.PK);
			AssertEquals(1, declaration.Messages.Count);
			AssertEquals("Message is discarded.", EDIMessageStatusList.Codes.Discarded, declaration.Messages[0].EM_Status);
			AssertEquals(1, declaration.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Invoice Deleted")).Length);
		}

		public void TestDetachInvoice()
		{
			var invoice = this.Declaration.Invoices.AddNew();
			AssertEquals("Invoice can be detached.", true, invoice.CanDetach);

			invoice.JZ_MessageStatus = ImportMessageStatusList.Codes.AwaitingElectronicInvoiceOriginal;
			AssertEquals("Invoice can't be detached as it's waiting for response.", false, invoice.CanDetach);
			AssertEquals("Electronic invoice message(s) exist that are pending response(s) from Customs. Please wait for the response(s) from Customs before detach the invoice(s).", invoice.ReasonNotToBeAbleToDetach);

			invoice.JZ_MessageStatus = ImportMessageStatusList.Codes.ClearElectronicInvoiceOriginal;
			AssertEquals("Invoice can be detached.", true, invoice.CanDetach);
			AssertEquals("You are about to detach Invoices that have been sent to Customs electronically.\r\n", invoice.GetWarningBeforeBeingDetached());

			var invoiceLine = invoice.InvoiceLines.AddNew();
			var lineRagne = invoiceLine.LineGroupingRanges.AddNew();
			var aiiLine = invoiceLine.AIILines.AddNew();

			var message = invoice.Messages.AddNew(typeof(MQEDIMessage));

			var entry = this.Declaration.CustomsEntryHeaders.AddNew();
			entry.US7501DocPrintingData.AddNew();

			invoice.CleanUpInvoiceAfterDetachedOrDeleted("Detached");
			AssertEquals(0, invoice.Messages.Count);
			AssertEquals(0, invoiceLine.LineGroupingRanges.Count);
			AssertEquals(0, invoiceLine.AIILines.Count);
			AssertEquals(0, entry.US7501DocPrintingData.Count);
			AssertEquals(this.Declaration.PK, message.EM_LinkUniqueID);

			var declaration = Factory.Load<JobDeclaration>(this.Declaration.PK);
			AssertEquals(1, declaration.Messages.Count);
			AssertEquals("Message is discarded.", EDIMessageStatusList.Codes.Discarded, declaration.Messages[0].EM_Status);
			AssertEquals(1, declaration.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Invoice Detached")).Length);
		}

		public void TestJZ_OH_SupplierConcurrency()
		{
			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			var jobDeclaration1 = factory1.New<JobDeclaration>();
			var supplier1 = factory1.New<OrgHeader>();
			supplier1.OH_Code = "supplier1";
			supplier1.MainAddress.OA_Address1 = "supplier1 MAIN ADDRESS";
			var invoiceHeader1 = jobDeclaration1.Invoices.AddNew();
			invoiceHeader1.JZ_OH_Supplier = supplier1.PK;
			var temp = invoiceHeader1.JZ_OH_Supplier;
			factory1.RefreshEnabled = false;
			factory1.Save();

			var jobDeclaration2 = factory2.Load<JobDeclaration>(jobDeclaration1.PK);
			var invoiceHeader2 = jobDeclaration2.Invoices.FirstOrDefault();
			var supplier2 = factory2.New<OrgHeader>();
			supplier2.OH_Code = "supplier2";
			supplier2.MainAddress.OA_Address1 = "supplier2 MAIN ADDRESS";

			invoiceHeader2.JZ_OH_Supplier = supplier2.PK;
			factory2.RefreshEnabled = false;
			factory2.Save();

			invoiceHeader1.Delete();

			var ex = AssertExceptionThrown<ZSaveConcurrencyException>(() => factory1.Save());
			ZExceptionReporting.HandleSaveException(ex);
		}

		public void TestJZ_OA_ShipperAddress_WhenSetToEmpty()
		{
			AssertAddressField_WhenSetToEmpty(
				(declaration, orgHeader) => declaration.JE_OA_ShipperAddress = orgHeader.MainAddress.PK,
				(invoice, addressPk) => invoice.JZ_OA_ShipperAddress = addressPk,
				invoice => invoice.JZ_OA_ShipperAddress
			);
		}

		public void TestJZ_OA_ConsigneeAddress_WhenSetToEmpty()
		{
			AssertAddressField_WhenSetToEmpty(
				(declaration, orgHeader) => declaration.JE_OA_ConsigneeAddress = orgHeader.MainAddress.PK,
				(invoice, addressPk) => invoice.JZ_OA_ConsigneeAddress = addressPk,
				invoice => invoice.JZ_OA_ConsigneeAddress
			);
		}

		public void TestJZ_OA_ShipToPartyAddress_WhenSetToEmpty()
		{
			AssertAddressField_WhenSetToEmpty(
				(declaration, orgHeader) => declaration.JE_OA_ShipToPartyAddress = orgHeader.MainAddress.PK,
				(invoice, addressPk) => invoice.JZ_OA_ShipToPartyAddress = addressPk,
				invoice => invoice.JZ_OA_ShipToPartyAddress
			);
		}

		public void TestJZ_OA_SoldToPartyAddress_WhenSetToEmpty()
		{
			AssertAddressField_WhenSetToEmpty(
				(declaration, orgHeader) => declaration.JE_OA_SoldToPartyAddress = orgHeader.MainAddress.PK,
				(invoice, addressPk) => invoice.JZ_OA_SoldToPartyAddress = addressPk,
				invoice => invoice.JZ_OA_SoldToPartyAddress
			);
		}

		public void TestJZ_OA_ManufacturerAddress_WhenSetToEmpty()
		{
			AssertAddressField_WhenSetToEmpty(
				(declaration, orgHeader) => declaration.JE_OA_ManufacturerAddress = orgHeader.MainAddress.PK,
				(invoice, addressPk) => invoice.JZ_OA_ManufacturerAddress = addressPk,
				invoice => invoice.JZ_OA_ManufacturerAddress
			);
		}

		public void TestJZ_OA_SellerAddress_WhenSetToEmpty()
		{
			AssertAddressField_WhenSetToEmpty(
				(declaration, orgHeader) => declaration.JE_OA_SellerAddress = orgHeader.MainAddress.PK,
				(invoice, addressPk) => invoice.JZ_OA_SellerAddress = addressPk,
				invoice => invoice.JZ_OA_SellerAddress
			);
		}

		public void TestJZ_OA_ExporterAddress_WhenSetToEmpty()
		{
			AssertAddressField_WhenSetToEmpty(
				(declaration, orgHeader) => declaration.JE_OH_Exporter = orgHeader.PK,
				(invoice, addressPk) => invoice.JZ_OA_ExporterAddress = addressPk,
				invoice => invoice.JZ_OA_ExporterAddress
			);
		}

		public void TestJZ_OA_SupplierAddress_WhenSetToEmpty()
		{
			AssertAddressField_WhenSetToEmpty(
				(declaration, orgHeader) => declaration.JE_OH_Supplier = orgHeader.PK,
				(invoice, addressPk) => invoice.JZ_OA_SupplierAddress = addressPk,
				invoice => invoice.JZ_OA_SupplierAddress
			);
		}

		void AssertAddressField_WhenSetToEmpty(Action<JobDeclaration, OrgHeader> setDeclarationOrgHeaderOrAddress, Action<JobComInvoiceHeader, ZGuid> setInvoiceAddress, Func<JobComInvoiceHeader, ZGuid> getInvoiceAddress)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "orgCode";
			orgHeader.OH_FullName = "orgName";
			orgHeader.MainAddress.OA_Address1 = "orgAddress";

			var declaration = Factory.New<JobDeclaration>();
			setDeclarationOrgHeaderOrAddress(declaration, orgHeader);

			var invoice = declaration.Invoices.AddNew();
			setInvoiceAddress(invoice, orgHeader.MainAddress.PK);
			AssertEquals(orgHeader.MainAddress.PK, getInvoiceAddress(invoice));

			setInvoiceAddress(invoice, Guid.Empty);
			AssertEquals("When the declaration is not created from low value", orgHeader.MainAddress.PK, getInvoiceAddress(invoice));

			MarkDeclarationAsFromLowValue(declaration);
			AssertEquals("When the declaration is created from low value", Guid.Empty, getInvoiceAddress(invoice));

			void MarkDeclarationAsFromLowValue(JobDeclaration declaration)
			{
				declaration.Logs.AddNew(AutoEvents.Transferred,
							[
								new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, "LV001"),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "USLV")
							]);
			}
		}

		public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.UnitedStates;

		protected override Hashtable ExpectedDocAddressTypes
		{
			get
			{
				if (expectedDocAddressTypes == null)
				{
					expectedDocAddressTypes = base.ExpectedDocAddressTypes;
					expectedDocAddressTypes.Add(DocAddressTypes.Codes.UltimateConsignee, DocAddressType.UltimateConsignee);
					expectedDocAddressTypes.Add(DocAddressTypes.Codes.IntermediateConsignee, DocAddressType.IntermediateConsignee);
					expectedDocAddressTypes.Add(DocAddressTypes.Codes.USPrincipalPartyInInterest, DocAddressType.USPrincipalPartyInInterest);
					expectedDocAddressTypes.Add(DocAddressTypes.Codes.SupplierPickupDeliveryAddress, DocAddressType.SupplierPickupDeliveryAddress);
					expectedDocAddressTypes.Add(DocAddressTypes.Codes.InvoicerAddress, DocAddressType.InvoicerAddress);
					expectedDocAddressTypes.Add(DocAddressTypes.Codes.FDAShipperAddress, DocAddressType.FDAShipperAddress);
				}
				return expectedDocAddressTypes;
			}
		}
		Hashtable expectedDocAddressTypes;

		protected override bool RatesAreReciprocal => true;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var invoice = (JobComInvoiceHeader)base.GetNewBusinessObjectForDeleteTest(factory);
			invoice.IsTestingBusinessObjectTest = true;
			return invoice;
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override void SetUp()
		{
			base.SetUp();
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			org1 = CreateNewOrg("ORGANISATION 1", "ORG1 ADDRESS", "BOB SMITH");
			org2 = CreateNewOrg("ORGANISATION 2", "ORG2 ADDRESS", "JOE BROWN");
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest() => Factory.New<JobComInvoiceHeader>();

		protected override Type ExpectedTypeOfGroupCharges => typeof(InvoiceApportionChargeCollection);

		protected override Type ExpectedTypeOfCharges => typeof(InvoiceChargeCollection);

		protected override IEnumerable<Func<BaseJobDeclaration, JobComInvoiceHeader, BaseJobComInvoiceLine, (ZPropertyInfo, IZType)>> GetJZ_Calc_LinesEnteredRelatedProperties()
		{
			foreach (var item in base.GetJZ_Calc_LinesEnteredRelatedProperties())
			{
				yield return item;
			}
			yield return (declaration, invoiceHeader, invoiceLine) => (((JobDeclaration)declaration).JE_ApplicationCodeInfo, new ZString(JobApplicationCodeList.Codes.ACE));
			yield return (declaration, invoiceHeader, invoiceLine) => (((JobDeclaration)declaration).JE_MessageTypeInfo, new ZString(JobMessageTypeList.Codes.Import));
			yield return (declaration, invoiceHeader, invoiceLine) => (((JobComInvoiceLine)invoiceLine).US_SetIndInfo, new ZString(SecondarySpecProgIndicatorList.Codes.V));
			yield return (declaration, invoiceHeader, invoiceLine) => (((JobComInvoiceLine)invoiceLine).US_SecondarySPIInfo, new ZString(SecondarySpecProgIndicatorList.Codes.V));
			yield return (declaration, invoiceHeader, invoiceLine) => (((JobComInvoiceLine)invoiceLine).US_98ValueInvCurrInfo, new ZDecimal(1));
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new USBusinessLightValidationTester(bizObjToTest);
		}

		JobDeclaration testDeclaration;
		JobDeclaration testReleaseDeclaration;

		void SetupData()
		{
			testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDeclaration.JE_DeclarationReference = "B00110010";
			testDeclaration.US_ConsolACE = true;
			var invoice = testDeclaration.Invoices.AddNew();

			testReleaseDeclaration = Factory.New<JobDeclaration>();
			testReleaseDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testReleaseDeclaration.US_EntryFilerCode = "SV9";
			var releaseEntry1 = testReleaseDeclaration.CustomsEntryHeaders.AddNew();
			releaseEntry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			releaseEntry1.EntryNumber = "12345678";
			Factory.Save();
		}

		IMessageAttacheeInDeclaration MessageAttacheeInDeclaration => InvoiceHeader;

		OrgHeader CreateNewOrg(ZString companyName, ZString address, ZString contactName) => CreateNewOrg(companyName, address, contactName, "");

		OrgHeader CreateNewOrg(ZString companyName, ZString address, ZString contactName, ZString contactPhone)
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			result.OH_FullName = companyName;
			result.MainAddress.OA_Address1 = address;
			result.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			var contact = result.Contacts.AddNew();
			contact.OC_ContactName = contactName;
			contact.OC_Phone = contactPhone;
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.All.ToString();
			document.OD_DefaultContact = true;
			return result;
		}

		JobDeclaration Declaration => (JobDeclaration)declaration;

		JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)invoiceHeader;

		OrgHeader org1;
		OrgHeader org2;

		void AssertHasInvoiceLinesWith(JobComInvoiceLine invoiceLine, string indicatorName, Func<bool> hasInvoiceLinesWith)
		{
			invoiceLine[indicatorName] = OGAIndicatorList.Codes.Declared;
			invoiceLine.RefreshInvoiceLinesWithPGAIndicators();

			if (indicatorName == USAddInfoSchema.US_FDAIndicator.Name)
			{
				invoiceLine.FDAs.AddNew();
				invoiceLine.ACE_FDALines.AddNew();
			}

			Assert(indicatorName, hasInvoiceLinesWith());
		}

		void AssertNoInvoiceLinesWith(JobComInvoiceLine invoiceLine, string indicatorNameToClear, Func<bool> hasInvoiceLinesWith)
		{
			invoiceLine[indicatorNameToClear] = ZString.Empty;
			Assert(indicatorNameToClear, !hasInvoiceLinesWith());
		}

		void AssertInvoiceNumberFilledIn(ZString messageType, bool expectedResult)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			var invoice = declaration.Invoices.AddNew();
			Assert("Pre-condition", invoice.JZ_InvoiceNumber.IsEmpty);
			Factory.Save();
			AssertEquals("Invoice Number result", expectedResult, !invoice.JZ_InvoiceNumber.IsEmpty);
		}

		void AssertInvoiceEffectiveOrganisation(SchemaGuidColumn declaration_OH_PK, SchemaGuidColumn invoice_OH_PK)
		{
			Declaration[declaration_OH_PK] = org1.PK;
			InvoiceHeader[invoice_OH_PK] = org2.PK;
			AssertEquals(invoice_OH_PK.Name, org2.PK, InvoiceHeader[invoice_OH_PK.Name]);
			InvoiceHeader[invoice_OH_PK] = ZGuid.Empty;
			AssertEquals(invoice_OH_PK.Name, org1.PK, InvoiceHeader[invoice_OH_PK.Name]);
		}

		void AssertInvoiceEffectiveOrgAddress(SchemaGuidColumn declaration_OA_PK, SchemaGuidColumn invoice_OA_PK)
		{
			Declaration[declaration_OA_PK] = org1.MainAddress.PK;
			InvoiceHeader[invoice_OA_PK] = org2.MainAddress.PK;
			AssertEquals(invoice_OA_PK.Name, org2.MainAddress.PK, InvoiceHeader[invoice_OA_PK.Name]);
			InvoiceHeader[invoice_OA_PK] = ZGuid.Empty;
			AssertEquals(invoice_OA_PK.Name, org1.MainAddress.PK, InvoiceHeader[invoice_OA_PK.Name]);
		}

		MQEDIMessage CreateMessage(string receiveTransmit, string applicationIdentifier)
		{
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			MQEDIMessage result = mock.Object;
			result.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			result.EM_ReceiveTransmit = receiveTransmit;
			result.EM_MessageType = applicationIdentifier;
			result.EM_MessageText = @"B018888XJ5CI                                               ~15000               Y  8888XJ5CI00054";

			return result;
		}

		OrgHeader partyInCurrentCountry;
		OrgHeader PartyInCurrentCountry
		{
			get
			{
				if (partyInCurrentCountry == null)
				{
					partyInCurrentCountry = OrgHeader.New(Factory);
					partyInCurrentCountry.OH_RL_NKClosestPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RL_Code;
				}
				return partyInCurrentCountry;
			}
		}

		OrgHeader partyInForeignCountry;
		OrgHeader PartyInForeignCountry
		{
			get
			{
				if (partyInForeignCountry == null)
				{
					partyInForeignCountry = OrgHeader.New(Factory);
					partyInForeignCountry.OH_RL_NKClosestPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RL_Code;
				}
				return partyInForeignCountry;
			}
		}

		sealed class JobComInvoiceHeaderForTest : JobComInvoiceHeader, IPGADataChangeTrackerSupporter
		{
			public JobComInvoiceHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{ }

			public bool IsUnCommittedRow => IsUnCommittedRow_Mock;
			public bool IsUnCommittedRow_Mock;
			public int TrackerCount;
			public PGADataChangeTracker Tracker
			{
				get
				{
					TrackerCount++;
					return JobDeclaration?.PGATrackerHelper;
				}
			}
		}
	}
}
