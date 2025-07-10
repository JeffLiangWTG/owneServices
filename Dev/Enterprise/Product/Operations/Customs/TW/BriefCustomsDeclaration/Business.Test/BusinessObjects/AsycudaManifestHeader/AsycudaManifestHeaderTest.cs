using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using AsycudaContainer = Enterprise.Customs.ASYCUDA.Business.AsycudaContainer;
using CusEntryNumber = Enterprise.Customs.TW.Business.CusEntryNumber;
using GlbExternalPassword = Enterprise.Customs.TW.Business.GlbExternalPassword;
using UniversalReferenceConstants = Enterprise.Customs.TW.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestDefaultAMA_CustomsOffice()
		{
			new TestTWCreator(Factory).CreateRegistryItemCusCustomsOffice();
			var header = Factory.New<AsycudaManifestHeader>();
			CombineAssertions(() =>
			{
				var customsOffice = header.AMA_CustomsOffice;
				AssertEquals("Should be equal to DefaultCustomsOfficeCode", RegistryHelper.DefaultCustomsOfficeCode, customsOffice);
				AssertEquals("AMA_CustomsOffice", "CE", customsOffice);
			});
		}

		public void TestDefaultGoodsLocationIfNeeded()
		{
			new TestTWCreator(Factory).CreateRegistryItemCusGoodsLocation();
			var header = Factory.New<AsycudaManifestHeader>();
			var masterBill = header.MasterBill;
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			masterBill.ABL_GoodsLocationInfo.ClearValue();
			header.AMA_CustomsOffice = "CC";
			AssertEquals("CC & IMP", "ANP0060D", masterBill.ABL_GoodsLocation);

			masterBill.ABL_GoodsLocationInfo.ClearValue();
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			AssertEquals("CC & EXP", "ANP0060D", masterBill.ABL_GoodsLocation);

			header.AMA_CustomsOffice = "DD";
			AssertEquals("Do not default value if GoodsLocaltion is not empty", "ANP0060D", masterBill.ABL_GoodsLocation);

			masterBill.ABL_GoodsLocationInfo.ClearValue();
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			AssertEquals("DD & IMP", "ANP0061D", masterBill.ABL_GoodsLocation);
		}

		public void TestDefaultImporterOrExporterFromBranchProxy()
		{
			var org = new TestTWCreator(Factory).CreateOrganization();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = org.PK;
			var header = Factory.New<AsycudaManifestHeader>();
			CombineAssertions(() =>
			{
				var masterBill = header.MasterBill;
				header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
				AssertEquals("Import, ConsigneeOrgPK", org.PK, masterBill.ConsigneeOrgPK);
				AssertEquals("Import, ShipperOrgPK", ZGuid.Empty, masterBill.ShipperOrgPK);

				masterBill.ConsigneeOrgPKInfo.ClearValue();
				masterBill.ShipperOrgPKInfo.ClearValue();
				header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
				AssertEquals("Export, ConsigneeOrgPK", ZGuid.Empty, masterBill.ConsigneeOrgPK);
				AssertEquals("Export, ShipperOrgPK", org.PK, masterBill.ShipperOrgPK);

				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				org2.OH_Code = "org2";
				masterBill.ConsigneeOrgPK = org2.PK;
				header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
				AssertEquals("Do not default ConsigneeOrgPK if it's not null", org2.PK, masterBill.ConsigneeOrgPK);
			});
		}

		public void TestDefaultCarrierFromBranchProxy()
		{
			var org = new TestTWCreator(Factory).CreateOrganization();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = org.PK;
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(org.MainAddress.PK, header.AMA_OA_Carrier);
		}

		public void TestPerson_CPN_PER_PersonPK()
		{
			var person = ManifestHeader.Person ?? ManifestHeader.Persons.AddNew();
			var targetInfo = ManifestHeader.Person_CPN_PER_PersonPKInfo;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(targetInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Person_CPN_PER_PersonPK", person.CPN_PER_Person, ManifestHeader.Person_CPN_PER_PersonPK);
				AssertEquals("Person_CPN_PER_PersonPK Caption", "Onboard Courier", resourceStringData.Caption);
				AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaManifestHeader), nameof(ManifestHeader.Person_CPN_PER_PersonPK), false, (a) => a.ListDataSourceMember == "Lookups.PersonsList");
			});
		}

		public void TestAMA_VehicleRegistration()
		{
			var targetInfo = ManifestHeader.AMA_VehicleRegistrationInfo;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(targetInfo);
			CombineAssertions(() =>
			{
				AssertEquals("AMA_VehicleRegistration Caption", "Vessel Registration Number", resourceStringData.Caption);
				AssertEquals("AMA_VehicleRegistration Short Caption", "Vessel Reg. No.", resourceStringData.ShortCaption);
				AssertEquals("AMA_VehicleRegistration MaxLength", 6, targetInfo.MaxLength);
			});
		}

		public void TestAMA_ManifestNumber()
		{
			var targetInfo = ManifestHeader.AMA_ManifestNumberInfo;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(targetInfo);
			CombineAssertions(() =>
			{
				AssertEquals("AMA_ManifestNumber Caption", "Manifest Number", resourceStringData.Caption);
				AssertEquals("AMA_ManifestNumber Short Caption", "Manifest No.", resourceStringData.ShortCaption);
				AssertEquals("AMA_ManifestNumber MaxLength", 4, targetInfo.MaxLength);
			});
		}

		public void TestIsImport()
		{
			ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			Assert("IsImport is true", ManifestHeader.IsImport);

			ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			Assert("IsImport is false", !ManifestHeader.IsImport);
		}

		public void TestIsExport()
		{
			ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			Assert("IsExport is true", ManifestHeader.IsExport);

			ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			Assert("IsExport is false", !ManifestHeader.IsExport);
		}

		public void TestContainerType()
		{
			var containers = ManifestHeader.Containers;
			CombineAssertions(() =>
			{
				AssertType<AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>>(containers);
			});
		}

		public void TestIAsycudaManifestHeader()
		{
			var bizObj = GetNewBusinessObject();
			bizObj.FillWithValidTestData();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<AsycudaManifestHeader>(bizObj.PK).GetType());
		}

		public void TestBills()
		{
			var bills = ManifestHeader.Bills;
			AssertType<AsycudaBillCollection>(bills);
			var bill = bills.AddNew();
			AssertType<AsycudaBill>(bill);
		}

		public void TestPackedItemRelationship()
		{
			AssertEquals("Should be RelationshipType.None", ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.None, ManifestHeader.PackedItemRelationship);
		}

		public void TestMasterBill()
		{
			AssertType<AsycudaBill>(ManifestHeader.MasterBill);
		}

		public void TestLookups()
		{
			AssertType<AsycudaManifestHeaderLookups>(ManifestHeader.Lookups);
		}

		public void TestValidation()
		{
			AssertType<AsycudaManifestHeaderValidation>(ManifestHeader.Validation);
		}

		[TestDate(2022, 11, 11)]
		public void TestSetDefaultValues()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			CombineAssertions(() =>
			{
				AssertEquals("AMA_ApplicationCode", ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration, header.AMA_ApplicationCode);
				AssertEquals("AMA_TransportMode", Core.Constants.TransportModes.Air, header.AMA_TransportMode);
				AssertEquals("AMA_ManifestType", TWManifestTypes.Codes.ImportLowValueDutyFreeGoods, header.AMA_ManifestType);
				AssertEquals("DeclarationDate", new ZDateTime(2022, 11, 11), header.DeclarationDate);
			});
		}

		public void TestAMA_NatureCaption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(ManifestHeader.AMA_NatureInfo);
			AssertEquals("AMA_Nature Caption", "Message Type", resourceStringData.Caption);
		}

		public void TestAMA_Nature()
		{
			ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			ManifestHeader.MasterBill.ABL_CarrierReference = "XXX";
			ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			CombineAssertions(() =>
			{
				AssertEquals("AMA_ManifestType default to X2", TWManifestTypes.Codes.ImportLowValueDutyFreeGoods, ManifestHeader.AMA_ManifestType);
				Assert("ABL_CarrierReference cleared", ManifestHeader.MasterBill.ABL_CarrierReference.IsEmpty);

				ManifestHeader.AMA_PaymentMethod = "XX";
				ManifestHeader.MasterBill.ABL_E_ARV = ZDateTime.BrettsBirthday;
				ManifestHeader.AMA_PaymentAccountNumber = "YY";
				ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
				AssertEquals("AMA_ManifestType default to X7", TWManifestTypes.Codes.ExportLowValueGoods, ManifestHeader.AMA_ManifestType);
				Assert("AMA_PaymentMethod cleared", ManifestHeader.AMA_PaymentMethod.IsEmpty);
				Assert("ABL_E_ARV cleared", ManifestHeader.MasterBill.ABL_E_ARV.IsEmpty);
				Assert("AMA_PaymentAccountNumber cleared", ManifestHeader.AMA_PaymentAccountNumber.IsEmpty);
			});
		}

		public void TestAMA_TransportModeCaption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(ManifestHeader.AMA_TransportModeInfo);
			AssertEquals("AMA_TransportMode Caption", "Transport Mode", resourceStringData.Caption);
		}

		public void TestAMA_TransportMode()
		{
			ManifestHeader.AMA_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			ManifestHeader.MasterBill.ABL_CarrierReference = "XXX";
			ManifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Air;
			Assert("ABL_CarrierReference cleared", ManifestHeader.MasterBill.ABL_CarrierReference.IsEmpty);
		}

		public void TestAMA_ManifestTypeCaption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(ManifestHeader.AMA_ManifestTypeInfo);
			AssertEquals("AMA_ManifestType Caption", "Declaration Type", resourceStringData.Caption);
		}

		public void TestAMA_CustomsOfficeCaption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(ManifestHeader.AMA_CustomsOfficeInfo);
			AssertEquals("AMA_CustomsOffice Caption", "Customs Office", resourceStringData.Caption);
		}

		public void TestAMA_PaymentAccountNumber()
		{
			var info = ManifestHeader.AMA_PaymentAccountNumberInfo;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(info);
			CombineAssertions(() =>
			{
				AssertEquals("AMA_PaymentAccountNumber Caption", "Guarantee", resourceStringData.Caption);
				AssertEquals("AMA_PaymentAccountNumber MaxLength", 12, info.MaxLength);
			});
		}

		public void TestAMA_PaymentMethod()
		{
			var info = ManifestHeader.AMA_PaymentMethodInfo;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals("AMA_PaymentMethod Caption", "Payment Method", resourceStringData.Caption);
		}

		public void TestAMA_RecipientReference()
		{
			var targetInfo = ManifestHeader.AMA_RecipientReferenceInfo;
			AssertEquals("List", "Lookups.BoxNumbers", targetInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("MaxLength", 3, targetInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
			AssertEquals("Caption", "Box Number", targetInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestAMA_CustomsOffice()
		{
			AssertEquals("Caption", "Customs Office", ManifestHeader.AMA_CustomsOfficeInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestAMA_CustomsOffice_SetAMA_RecipientReference()
		{
			var boxNumbers = new CusBrokerageBoxNumberCollection();
			var boxNumber = boxNumbers.AddNew();
			boxNumber.BoxNumber = "B01";
			boxNumber.CustomsOfficeArea = "B";
			boxNumber.IsDefaultBoxNumber = true;
			using (TWCustomsDataRegistry.Instance.CusBrokerageBoxNumber.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, boxNumbers))
			{
				ManifestHeader.AMA_CustomsOffice = "BF";
				AssertEquals("B01", ManifestHeader.AMA_RecipientReference);
			}
		}

		public void TestCusBrokerageBoxNumbers()
		{
			var boxNumbers = new CusBrokerageBoxNumberCollection();
			var boxNumber = boxNumbers.AddNew();
			boxNumber.BoxNumber = "B01";
			boxNumber.CustomsOfficeArea = "B";
			boxNumber.IsDefaultBoxNumber = true;
			var boxNumber2 = boxNumbers.AddNew();
			boxNumber2.BoxNumber = "C01";
			boxNumber2.CustomsOfficeArea = "C";
			boxNumber2.IsDefaultBoxNumber = false;
			var boxNumber3 = boxNumbers.AddNew();
			boxNumber3.BoxNumber = "C02";
			boxNumber3.CustomsOfficeArea = "C";
			boxNumber3.IsDefaultBoxNumber = true;
			using (TWCustomsDataRegistry.Instance.CusBrokerageBoxNumber.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, boxNumbers))
			{
				ManifestHeader.AMA_CustomsOffice = "BF";
				var cusBrokerageBoxNumbers = ManifestHeader.CusBrokerageBoxNumbers;
				AssertEquals("B01", cusBrokerageBoxNumbers.PairList.CodesAsString);
				AssertEquals("B01", cusBrokerageBoxNumbers.DefaultBoxNumber);

				ManifestHeader.AMA_CustomsOffice = "CA";
				cusBrokerageBoxNumbers = ManifestHeader.CusBrokerageBoxNumbers;
				AssertEquals("C01, C02", cusBrokerageBoxNumbers.PairList.CodesAsString);
				AssertEquals("C02", cusBrokerageBoxNumbers.DefaultBoxNumber);

				ManifestHeader.AMA_CustomsOffice = "CB";
				AssertEquals("Value cached", cusBrokerageBoxNumbers, ManifestHeader.CusBrokerageBoxNumbers);

				ManifestHeader.AMA_CustomsOffice = "XX";
				cusBrokerageBoxNumbers = ManifestHeader.CusBrokerageBoxNumbers;
				AssertEquals(ZString.Empty, cusBrokerageBoxNumbers.PairList.CodesAsString);
				AssertEquals(ZString.Empty, cusBrokerageBoxNumbers.DefaultBoxNumber);
			}
		}

		public void TestAMA_GS_NKCustomsAgent()
		{
			AssertEquals("Caption", "Customs Agent", ManifestHeader.AMA_GS_NKCustomsAgentInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestAMA_GS_NKCustomsAgentSetAMA_CustomsProfile()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "GS1";
			var password = TWGlbStaffWrapper.Get(staff).TWPasswordCollection.AddNew();
			password.GP_MailBoxID = "MB1";

			ManifestHeader.AMA_GS_NKCustomsAgent = "GS1";

			AssertEquals("MB1", ManifestHeader.AMA_CustomsProfile);
		}

		public void TestTWBrkCertificateAndCustomsAgentDescription()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "GS1";
			var cert = staff.Certificates.AddNew();
			cert.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Taiwan;
			cert.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			cert.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(1);
			cert.XZ_RefNumber = "CER1";

			ManifestHeader.AMA_GS_NKCustomsAgent = "GS1";
			var twBrkCertificate = ManifestHeader.TWBrkCertificate;
			AssertNotNull("TWBrkCertificate", twBrkCertificate);
			AssertSame("Should be cached", twBrkCertificate, ManifestHeader.TWBrkCertificate);
			AssertEquals("CustomsAgentDescription", "CER1", ManifestHeader.CustomsAgentDescription);

			ManifestHeader.AMA_GS_NKCustomsAgent = "XXX";
			AssertNull("TWBrkCertificate", ManifestHeader.TWBrkCertificate);
			AssertEquals("CustomsAgentDescription", ZString.Empty, ManifestHeader.CustomsAgentDescription);
		}

		public void TestAMA_CustomsProfile()
		{
			var targetInfo = ManifestHeader.AMA_CustomsProfileInfo;
			AssertEquals("List", "Lookups.MailboxList", targetInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("Caption", "Mailbox", targetInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestPasswords()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "GS1";
			var password = TWGlbStaffWrapper.Get(staff).TWPasswordCollection.AddNew();
			password.GP_MailBoxID = "MB1";

			ManifestHeader.AMA_GS_NKCustomsAgent = "GS1";
			var passwords = ManifestHeader.Passwords;
			AssertEquals("Length", 1, passwords.Count);
			AssertEquals("GP_MailBoxID", "MB1", passwords[0].GP_MailBoxID);
			AssertSame("Should be cached", passwords, ManifestHeader.Passwords);

			ManifestHeader.AMA_GS_NKCustomsAgent = "XXX";
			AssertEquals("Length", 0, ManifestHeader.Passwords.Count);
		}

		public void TestPerson()
		{
			AssertNull("Person is null", ManifestHeader.Person);

			var person = ManifestHeader.Persons.AddNew();
			AssertSame("First if exists", person, ManifestHeader.Person);
		}

		public void TestDeleteInvalidPersonOnSaving()
		{
			ManifestHeader.Person_CPN_PER_PersonPK = ZGuid.Empty;
			AssertNotNull("manifestHeader.Person is not null", ManifestHeader.Person);

			Factory.Save();
			AssertNull("Empty new added person should be deleted when save", ManifestHeader.Person);
		}

		public void TestCalculateCustomsValue()
		{
			var bills = ManifestHeader.Bills;
			var bill1 = bills.AddNew();
			var packedItem1 = bill1.PackedItems.AddNew();
			packedItem1.API_GoodsValue = 11m;
			var bill2 = bills.AddNew();
			var packedItem2 = bill2.PackedItems.AddNew();
			packedItem2.API_GoodsValue = 22m;
			ManifestHeader.CalculateCustomsValueAndTaxesAndDuties();
			CombineAssertions("should calculate all bills", () =>
			{
				AssertEquals(11m, bill1.ABL_GoodsValue);
				AssertEquals(22m, bill2.ABL_GoodsValue);
			});
		}

		public void TestCalculateCustomsValueAndTaxesAndDuties()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 0.33m, new ZDateTime(2023, 08, 04), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 0.32m, new ZDateTime(2023, 08, 04), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();

			var universalTestHelper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			var rateType = universalTestHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "DTY", "Duty");
			var rateCodeDTA = universalTestHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DTA, rateType.PK);
			var preference = universalTestHelper.CreatePreferenceForCountry("PR1", "PR1", Core.Constants.CountryCodes.Taiwan);
			var tradeGroup = universalTestHelper.CreateTradeGroup("TW", "JP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.AddCountry(tradeGroup, "JP", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var tariffDTA = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090200", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rateDTA = universalTestHelper.CreateRate(tariffDTA, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.15 * VFD", preference.PK, "0.15", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(rateDTA, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			var bills = ManifestHeader.Bills;
			var bill = bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			packedItem.API_Tariff = "21039090200";
			packedItem.API_RN_NKGoodsOrigin = "JP";
			packedItem.API_Preference = "PR1";
			packedItem.API_GoodsValue = 11m;
			ManifestHeader.CalculateCustomsValueAndTaxesAndDuties();
			CombineAssertions(() =>
			{
				AssertEquals("bill.AsycudaTaxes.Count", 0, bill.AsycudaTaxes.Count);
				AssertEquals("packedItem.AsycudaTaxes.Count", 0, packedItem.AsycudaTaxes.Count);
			});

			ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			ManifestHeader.CalculateCustomsValueAndTaxesAndDuties();
			CombineAssertions(() =>
			{
				AssertEquals("bill.AsycudaTaxes.Count", 0, bill.AsycudaTaxes.Count);
				AssertEquals("packedItem.AsycudaTaxes.Count", 0, packedItem.AsycudaTaxes.Count);
			});
			
			packedItem.API_GoodsValue = 20000m;
			bill.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			ManifestHeader.CalculateCustomsValueAndTaxesAndDuties();
			CombineAssertions(() =>
			{
				AssertEquals("bill.AsycudaTaxes.Count", 1, bill.AsycudaTaxes.Count);
				AssertEquals("packedItem.AsycudaTaxes.Count", 3, packedItem.AsycudaTaxes.Count);
			});
		}

		public void TestRequiresDefaultPackitemTaxes()
		{
			AsycudaPackedItemTaxHelperForTest.CreateTariffData(Factory);
			var header = ManifestHeader;
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;

			var bill1 = header.Bills.AddNew();
			var packedItem1 = bill1.PackedItems.AddNew();
			packedItem1.API_FormattedTariff = "87031000002";
			packedItem1.API_Preference = "PR2";
			packedItem1.API_RN_NKGoodsOrigin = "US";

			var bill2 = header.Bills.AddNew();
			var packedItem2 = bill2.PackedItems.AddNew();
			packedItem2.API_FormattedTariff = "87031000003";
			packedItem2.API_Preference = "PR2";
			packedItem2.API_RN_NKGoodsOrigin = "US";
			AssertEquals("EXP and Sum(ABL_CustomsValue) < 2000", false, header.RequiresDefaultPackitemTaxes);

			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			AssertEquals("IMP and Sum(ABL_CustomsValue) < 2000", false, header.RequiresDefaultPackitemTaxes);

			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			bill1.ABL_CustomsValue = 1000m;
			bill2.ABL_CustomsValue = 1000m;
			AssertEquals("EXP and Sum(ABL_CustomsValue) >= 2000", false, header.RequiresDefaultPackitemTaxes);

			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			AssertEquals("IMP and Sum(ABL_CustomsValue) >= 2000", true, header.RequiresDefaultPackitemTaxes);
		}
		public void TestDeclarationNumberDisplay()
		{
			var targetInfo = ManifestHeader.DeclarationNumberDisplayInfo;
			BusinessObjectCaptionTestHelper.AssertCaptions(targetInfo, "Entry Number");
			ManifestHeader.DeclarationNumber = "CA1245600096";
			AssertEquals("CA/12/45/600/096", ManifestHeader.DeclarationNumberDisplay);
		}

		public void TestDeclarationNumber()
		{
			CombineAssertions(() =>
			{
				ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
				ManifestHeader.DeclarationNumber = "CA1245600096";
				var entryNumberImp = CusEntryNumber.Load(ManifestHeader, CusEntryNumberTypes.Taiwan.ImportBriefCustomsDeclaration, "TW");
				AssertEquals("CA1245600096", entryNumberImp.CE_EntryNum);

				ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
				ManifestHeader.DeclarationNumber = "CA1245600097";
				var entryNumberExp = CusEntryNumber.Load(ManifestHeader, CusEntryNumberTypes.Taiwan.ExportBriefCustomsDeclaration, "TW");
				AssertEquals("CA1245600097", entryNumberExp.CE_EntryNum);

				Assert("should be deleted when message type change", entryNumberImp.IsDeleted);
			});
		}

		public void TestCalculateDutyOnSaving()
		{
			AsycudaPackedItemTaxHelperForTest.CreateTariffData(Factory);

			ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			var bills = ManifestHeader.Bills;
			var bill = bills.AddNew();
			bill.ABL_CustomsValue = 2000m;

			var packedItem = bill.PackedItems.AddNew();
			packedItem.API_FormattedTariff = "87031000002";
			packedItem.API_Preference = "PR1";
			packedItem.API_RN_NKGoodsOrigin = "TW";
			packedItem.API_CustomsValue = 500000m;
			packedItem.API_RX_NKGoodsValueCurrency = "TWD";
			packedItem.API_CustomsUQ2 = "LTR";
			packedItem.API_CustomsQty2 = 2m;
			packedItem.AsycudaTaxes.Cast<AsycudaPackedItemTax>().FirstOrDefault(c => c.AET_ChargeType == "SS").AET_Tariff = "SSTariff";
			Factory.Save();
			AssertEquals(3, bill.AsycudaTaxes.Count);

			ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			Factory.Save();
			AssertEquals("bill.AsycudaTaxes.Count", 0, bill.AsycudaTaxes.Count);
		}

		public void TestDeclarationDate()
		{
			var targetInfo = ManifestHeader.DeclarationDateInfo;
			BusinessObjectCaptionTestHelper.AssertCaptions(targetInfo, "Declaration Date");
			CombineAssertions(() =>
			{
				ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
				ManifestHeader.DeclarationDate = ZDateTime.BrettsBirthday;
				var entryNumberImp = CusEntryNumber.Load(ManifestHeader, CusEntryNumberTypes.Taiwan.ImportBriefCustomsDeclaration, "TW");
				AssertEquals(ZDateTime.BrettsBirthday, entryNumberImp.CE_IssueDate);

				ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
				ManifestHeader.DeclarationDate = new ZDateTime(2023, 11, 11);
				var entryNumberExp = CusEntryNumber.Load(ManifestHeader, CusEntryNumberTypes.Taiwan.ExportBriefCustomsDeclaration, "TW");
				AssertEquals(new ZDateTime(2023, 11, 11), entryNumberExp.CE_IssueDate);

				Assert("should be deleted when message type change", entryNumberImp.IsDeleted);
			});
		}

		public void TestDeclarationDateWhenAMA_NatureChanged()
		{
			ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			ManifestHeader.DeclarationDate = ZDateTime.BrettsBirthday;
			ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			AssertEquals("DeclarationDate should not clear up when AMA_Nature changed", ZDateTime.BrettsBirthday, ManifestHeader.DeclarationDate);
		}

		public void TestBagNumber()
		{
			var targetInfo = ManifestHeader.BagNumberInfo;
			BusinessObjectCaptionTestHelper.AssertCaptions(targetInfo, "Bag Number", "Bag No.");

			CombineAssertions(() =>
			{
				ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
				ManifestHeader.BagNumber = "123";
				var entryNumberImp = CusEntryNumber.Load(ManifestHeader, CusEntryNumberTypes.Taiwan.ImportBriefCustomsDeclaration, "TW");
				AssertEquals("123", entryNumberImp.CE_EntryLineReference);

				ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
				ManifestHeader.BagNumber = "456";
				var entryNumberExp = CusEntryNumber.Load(ManifestHeader, CusEntryNumberTypes.Taiwan.ExportBriefCustomsDeclaration, "TW");
				AssertEquals("456", entryNumberExp.CE_EntryLineReference);

				Assert("should be deleted when message type change", entryNumberImp.IsDeleted);
			});
		}

		public void TestCleanBagNumberOnSaving()
		{
			ManifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Air;
			ManifestHeader.BagNumber = "123";
			Factory.Save();
			AssertEquals("bagNumber should not be cleared when Air", "123", ManifestHeader.BagNumber);

			ManifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();
			AssertEquals("bagNumber should be cleared when Sea", ZString.Empty, ManifestHeader.BagNumber);
		}

		public void TestCusEntryNumberCorrectlySaved()
		{
			ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			ManifestHeader.BagNumber = "123";
			var entryNumberImp = CusEntryNumber.Load(ManifestHeader, CusEntryNumberTypes.Taiwan.ImportBriefCustomsDeclaration, "TW");

			ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			ManifestHeader.BagNumber = "456";
			var entryNumberExp = CusEntryNumber.Load(ManifestHeader, CusEntryNumberTypes.Taiwan.ExportBriefCustomsDeclaration, "TW");
			Factory.Save();
			CombineAssertions("Should save correct entry number record", () =>
			{
				Assert(entryNumberImp.IsDeleted);
				Assert(!entryNumberExp.IsDeleted);
			});
		}

		public void TestITWMessageInfoProvider()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";

			var extPassword1 = Factory.New<GlbExternalPassword>();
			extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword1.GP_MailBoxID = "00000000-1";
			extPassword1.GP_GS = staff.PK;

			var extPassword2 = Factory.New<GlbExternalPassword>();
			extPassword2.GP_PasswordType = PasswordTypesList.Codes.UVC;
			extPassword2.GP_MailBoxID = "00000000-2";
			extPassword2.GP_GS = staff.PK;

			ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			ManifestHeader.AMA_GS_NKCustomsAgent = "TT";
			ManifestHeader.AMA_CustomsProfile = "00000000-1";
			ManifestHeader.DeclarationNumber = "CA1245600096";
			var bill = ManifestHeader.Bills.AddNew();
			bill.CustomsEntryNumber = "ABC12345";

			var provider = (ITWMessageInfoProvider)ManifestHeader;
			CombineAssertions("ITWMessageInfoProvider members", () =>
			{
				AssertEquals("Entry Number", "CA1245600096", provider.EntryNumber);
				AssertEquals("Staff Code", "TT", provider.StaffCode);
				AssertEquals("Company ID", GlbCompany.CurrentCompany.GC_Code, provider.CompanyID);
				AssertEquals("Password Type", PasswordTypesList.Codes.TVA, provider.PasswordType);
			});

			ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			ManifestHeader.AMA_GS_NKCustomsAgent = "TT";
			ManifestHeader.AMA_CustomsProfile = "00000000-2";
			ManifestHeader.DeclarationNumber = "CA1245600096";
			CombineAssertions("ITWMessageInfoProvider members", () =>
			{
				AssertEquals("Entry Number", "CA1245600096", provider.EntryNumber);
				AssertEquals("Entry Number Type", MessageTypeList.Codes.EBC, provider.EntryNumberType);
				AssertEquals("Staff Code", "TT", provider.StaffCode);
				AssertEquals("Company ID", GlbCompany.CurrentCompany.GC_Code, provider.CompanyID);
				AssertEquals("Password Type", PasswordTypesList.Codes.UVC, provider.PasswordType);
			});
		}

		public void TestIEntryNumberGeneratorProviderMembers()
		{
			ManifestHeader.DeclarationDate = ZDateTime.BrettsBirthday;
			ManifestHeader.AMA_CustomsOffice = "A";
			ManifestHeader.AMA_RecipientReference = "A11";
			ManifestHeader.BagNumber = "ABCAA";
			ManifestHeader.AMA_Nature = "IMP";
			CombineAssertions(() =>
			{
				var provider = (IEntryNumberGeneratorProvider)ManifestHeader;
				AssertEquals("EntryNumberDate", ZDateTime.BrettsBirthday, provider.EntryNumberDate);
				AssertEquals("Company", ManifestHeader.Branch.Company, provider.Company);
				AssertEquals("ShipmentType", ManifestHeader.AMA_Nature, provider.ShipmentType);
				AssertEquals("EntryNumberPart1Info", ManifestHeader.AMA_CustomsOffice, provider.EntryNumberPart1Info.Value);
				AssertEquals("CustomsBrokerageBoxNumberInfo", ManifestHeader.AMA_RecipientReference, provider.CustomsBrokerageBoxNumberInfo.Value);
				AssertEquals("SequenceNumber", ManifestHeader.BagNumber, provider.SequenceNumber);
				AssertNull("EntryNumberPart2Info", provider.EntryNumberPart2Info);
				AssertEquals("GetEntryNumberGeneratorCategory", EntryNumberGeneratorCategory.D, provider.GetEntryNumberGeneratorCategory());
				AssertEquals("EntryNumberGeneratorProviderBusinessObject", ManifestHeader, provider.EntryNumberGeneratorProviderBusinessObject);
			});
		}

		public void TestEntryNumberType()
		{
			ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			CombineAssertions(() =>
			{
				AssertEquals("Entry Number Type for IMP", CusEntryNumberTypes.Taiwan.ImportBriefCustomsDeclaration, ManifestHeader.EntryNumberType);
				ManifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
				AssertEquals("Entry Number Type for EXP", CusEntryNumberTypes.Taiwan.ExportBriefCustomsDeclaration, ManifestHeader.EntryNumberType);
			});
		}

		public void TestAllocateEntryNumberWithUserEnteredEntryNumber()
		{
			ManifestHeader.AllocateEntryNumber("AB  1112300006");
			AssertEquals("AB  1112300006", ManifestHeader.DeclarationNumber);
		}

		public void TestEntryNumberOnFactorySaving()
		{
			ManifestHeader.AMA_CustomsOffice = "AA";
			ManifestHeader.AMA_RecipientReference = "123";
			ManifestHeader.DeclarationDate = new ZDateTime(2019, 12, 31);
			ManifestHeader.DeclarationNumber = "98767666";
			AssertEquals("98767666", ManifestHeader.DeclarationNumber);
			ManifestHeader.AllocateEntryNumber(ZString.Empty);
			Factory.Save();
			AssertEquals("AA  0812300001", ManifestHeader.DeclarationNumber);
			ManifestHeader.AllocateEntryNumber(ZString.Empty);
			Factory.Save();
			AssertEquals("AA  0812300002", ManifestHeader.DeclarationNumber);
		}

		public void TestEntryNumberShouldBeRemovedWhenFactorySaveFailed_AllocateEntryNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderForTesting>();
			header.SuspendCheckBusinessObjectType();
			header.AMA_JobReference = "C1234";
			header.AMA_CustomsOffice = "AA";
			header.AMA_RecipientReference = "123";
			header.DeclarationDate = new ZDateTime(2019, 12, 31);
			header.DeclarationNumber = "98767666";
			header.AllocateEntryNumber(ZString.Empty);
			header.ShouldThrowException = true;
			AssertExceptionThrown<ApplicationException>(() => Factory.Save());
			AssertEquals(ZString.Empty, header.DeclarationNumber);
		}

		public void TestEntryNumberMutex()
		{
			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var header = factory1.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			factory1.Save();
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var decInFactory2 = factory2.Load<AsycudaManifestHeader>(header.PK);
			AssertEquals(true, decInFactory2.LockEntryNumberAllocationMutex);
			AssertEquals(false, header.LockEntryNumberAllocationMutex);
			AssertEquals("Still locked", true, decInFactory2.LockEntryNumberAllocationMutex);
			AssertEquals("Still locked by another session", false, header.LockEntryNumberAllocationMutex);
			header.UnlockEntryNumberAllocationMutex();
			AssertEquals("Still locked", true, decInFactory2.LockEntryNumberAllocationMutex);
			decInFactory2.UnlockEntryNumberAllocationMutex();
			AssertEquals(true, header.LockEntryNumberAllocationMutex);
			AssertEquals("another session took lock", false, decInFactory2.LockEntryNumberAllocationMutex);
			using (var importEntryNumberAllocationMutex = new ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, "IBC" + header.PK.ToString()))
			{
				AssertEquals(true, importEntryNumberAllocationMutex.IsLocked);
				AssertEquals(false, importEntryNumberAllocationMutex.HasLock);
				factory1.Save();
				AssertEquals("Lock should be released when factory is saved", false, importEntryNumberAllocationMutex.IsLocked);
				AssertEquals(false, importEntryNumberAllocationMutex.HasLock);
				AssertEquals(true, header.LockEntryNumberAllocationMutex);
				AssertEquals(true, importEntryNumberAllocationMutex.IsLocked);
				AssertEquals(false, importEntryNumberAllocationMutex.HasLock);
				header.Delete();
				AssertEquals("Lock should be released when dec is deleted", false, importEntryNumberAllocationMutex.IsLocked);
				AssertEquals(false, importEntryNumberAllocationMutex.HasLock);
			}
		}

		public void TestEntryNumberAllocationMutexLockInfo_NullUser()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			NeedUnlockAfterMergeForTestHelper.NeedUnlockAfterMergeForTest = false;
			using (var mutex = new ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, "IBC" + header.PK.ToString()))
			{
				Assert(mutex.Lock());
				var lockInfo = "Mutex:" + MutexIDs.CustomsTransactionIDAllocation.Name + ":IBC" + header.PK.ToString();
				var emptyGuid = Guid.Empty;
				var sql = $@"UPDATE TOP(1) StmServiceHeartBeat
									SET SV_ParentId = '{emptyGuid}',
										SV_SystemLastEditTimeUtc = GetUtcDate(),
										SV_SystemLastEditUser = 'USR'
									FROM dbo.StmServiceSemaphore
									INNER JOIN dbo.StmServiceHeartBeat ON SS_SV = SV_PK
									WHERE SS_LockInfo LIKE '%{lockInfo}%';";
				TestConnection.Command(sql).ExecuteNonQuery();
				AssertEquals(false, header.LockEntryNumberAllocationMutex);
				AssertNoExceptionThrown(() => header.GetEntryNumberAllocationMutexLockInfo());
			}

			NeedUnlockAfterMergeForTestHelper.NeedUnlockAfterMergeForTest = true;
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			header.AMA_JobReference = "C1234";
			return header;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			ASYCUDA.Business.Testing.ZZDataTestHelper.SetupZZ(Factory, Core.Constants.CountryCodes.Taiwan);
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		AsycudaManifestHeader ManifestHeader
		{
			get
			{
				if (manifestHeader == null)
				{
					manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
					manifestHeader.AMA_JobReference = "C4567";
				}
				return manifestHeader;
			}
		}
		AsycudaManifestHeader manifestHeader;
	}

	class AsycudaManifestHeaderForTesting : AsycudaManifestHeader
	{
		public AsycudaManifestHeaderForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool ShouldThrowException;
		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (ShouldThrowException)
			{
				throw new ApplicationException("intended");
			}
		}
	}
}
