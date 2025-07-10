using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.N5301;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5301MessageSendingObject))]
	sealed class N5301MessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			return new N5301MessageSendingObject(header);
		}

		[ExpectNoExceptions]
		public void TestID()
		{
			header.EntryNumber = "XXX222";
			NUnit.Framework.Assert.That(messageSendingObject.ID, NUnit.Framework.Is.EqualTo("XXX222").Using(CustomComparers.TypeComparison));

			header.EntryNumberInfo.ClearValue();
			NUnit.Framework.Assert.That(messageSendingObject.ID, NUnit.Framework.Is.EqualTo(MessageConstants.EntryNumberPlaceHolder).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTotalGrossMassMeasure()
		{
			arrivalBill.B0_Weight = 123;
			NUnit.Framework.Assert.That(messageSendingObject.TotalGrossMassMeasure, NUnit.Framework.Is.EqualTo(123m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTotalPackageQuantity()
		{
			arrivalBill.B0_ManifestQty = 321;
			NUnit.Framework.Assert.That(messageSendingObject.TotalPackageQuantity, NUnit.Framework.Is.EqualTo(321).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			moveHeader.BM_InBondEntryType = "T2";
			NUnit.Framework.Assert.That(messageSendingObject.TypeCode, NUnit.Framework.Is.EqualTo("T2").Using(CustomComparers.TypeComparison));
		}

		public void AdditionalInformations()
		{
			NUnit.Framework.Assert.That(messageSendingObject.AdditionalInformations, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<Messaging.IAdditionalInformation>)));
		}

		[ExpectNoExceptions]
		public void TestAgent()
		{
			header.TW_BoxNumber = "123";
			header.BH_CustomsProfile = "CBK1123-Z";
			var agent = messageSendingObject.Agent;
			NUnit.Framework.Assert.That(agent.ID, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(agent.SubBoxID, NUnit.Framework.Is.EqualTo("Z").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(agent.RoleCode, NUnit.Framework.Is.EqualTo("CB").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBorderTransportMeans()
		{
			NUnit.Framework.Assert.That(messageSendingObject.BorderTransportMeans, NUnit.Framework.Is.TypeOf(typeof(BorderTransportMeans)));
		}

		[ExpectNoExceptions]
		public void TestCarrier()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "00612348", Core.Constants.CountryCodes.Taiwan);
			header.BH_OH_Carrier = carrier.PK;
			Factory.Save();
			NUnit.Framework.Assert.That(messageSendingObject.Carrier.ID, NUnit.Framework.Is.EqualTo("00612348").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestConsignment()
		{
			NUnit.Framework.Assert.That(messageSendingObject.Consignment, NUnit.Framework.Is.TypeOf(typeof(Consignment)));
		}

		[ExpectNoExceptions]
		public void TestDeconsolidator()
		{
			NUnit.Framework.Assert.That(messageSendingObject.Deconsolidator, NUnit.Framework.Is.EqualTo(default(Messaging.IPartyDetails)));
		}

		[ExpectNoExceptions]
		public void TestLoadingLocation()
		{
			header.MovementHeader.BM_PlaceOfLoading = "TWKEL";
			NUnit.Framework.Assert.That(messageSendingObject.LoadingLocation, NUnit.Framework.Is.EqualTo("TWKEL").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestRepresentativePersonName()
		{
			var borker = Factory.NewWithValidTestData<GlbStaff>();
			var brkCertificate = borker.Certificates.AddNew();
			brkCertificate.XZ_Type = CertificateTypePairList.Codes.BR1;
			brkCertificate.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Taiwan;
			brkCertificate.XZ_RefNumber = "888555";
			header.BH_GS_NKCusAgent = borker.GS_Code;
			Factory.Save();
			NUnit.Framework.Assert.That(messageSendingObject.RepresentativePersonName, NUnit.Framework.Is.EqualTo("888555").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestApplicant()
		{
			var importerOrg = Factory.NewWithValidTestData<OrgHeader>();
			importerOrg.OH_FullName = "importer01";
			var importerAddress = importerOrg.Addresses.AddNew();
			importerAddress.Address1 = "ad1222222";
			importerAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			importerAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			importerAddress.OA_IsActive = true;
			importerAddress.OA_Language = Core.SharedConstants.Languages.English;
			importerAddress.OA_CompanyNameOverride = "company 2";
			importerAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var importerTranslatedAddress = importerAddress.TranslatedAddresses.AddNew();
			importerTranslatedAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			importerTranslatedAddress.Address1 = "otaaddress 222";
			importerTranslatedAddress.CompanyName = "公司2";
			importerAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "GB98222221365", Core.Constants.CountryCodes.Taiwan);
			importerOrg.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "PID02125200", Core.Constants.CountryCodes.Taiwan);
			header.BH_OA_Importer = importerAddress.PK;
			Factory.Save();

			var party = messageSendingObject.Applicant;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(party.ID, NUnit.Framework.Is.EqualTo("PID02125200").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(party.CustomsControlID, NUnit.Framework.Is.EqualTo("GB98222221365").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(party.Name, NUnit.Framework.Is.EqualTo("company 2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(party.ChineseName, NUnit.Framework.Is.EqualTo("公司2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(party.TypeCode, NUnit.Framework.Is.EqualTo("174").Using(CustomComparers.TypeComparison));
			});

			importerOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "PAS02125201", Core.Constants.CountryCodes.Taiwan);
			party = messageSendingObject.Applicant;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(party.ID, NUnit.Framework.Is.EqualTo("NOPAS02125201").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(party.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison));
			});

			importerOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "VAT02125205", Core.Constants.CountryCodes.Taiwan);
			party = messageSendingObject.Applicant;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(party.ID, NUnit.Framework.Is.EqualTo("VAT02125205").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(party.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestTestApplicant_CustomsControlID()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "importer01";
			var address1 = org.Addresses.AddNew();
			address1.Address1 = "address 1";
			address1.OA_IsActive = true;
			address1.OA_Language = Core.SharedConstants.Languages.English;
			address1.OA_CompanyNameOverride = "company 1";
			address1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			address1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "GB98222221365", Core.Constants.CountryCodes.Taiwan);

			var address2 = org.Addresses.AddNew();
			address2.Address1 = "address 2";
			address2.OA_IsActive = true;
			address2.OA_Language = Core.SharedConstants.Languages.English;
			address2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			address2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CPW111", Core.Constants.CountryCodes.Taiwan);

			var address3 = org.Addresses.AddNew();
			address3.Address1 = "address 3";
			address3.OA_IsActive = true;
			address3.OA_Language = Core.SharedConstants.Languages.English;
			address3.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			address3.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "00612348", Core.Constants.CountryCodes.Taiwan);

			var address4 = org.Addresses.AddNew();
			address4.Address1 = "address 4";
			address4.OA_IsActive = true;
			address4.OA_Language = Core.SharedConstants.Languages.English;
			address4.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			address4.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "00987654", Core.Constants.CountryCodes.Taiwan);

			var address5 = org.Addresses.AddNew();
			address5.Address1 = "address 5";
			address5.OA_IsActive = true;
			address5.OA_Language = Core.SharedConstants.Languages.English;
			address5.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			address5.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.FTZ, "PAA01", Core.Constants.CountryCodes.Taiwan);

			org.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "PID02125200", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();

			header.BH_OA_Importer = address1.PK;
			var party = messageSendingObject.Applicant;
			NUnit.Framework.Assert.That(party.CustomsControlID, NUnit.Framework.Is.EqualTo("GB98222221365").Using(CustomComparers.TypeComparison));

			header.BH_OA_Importer = address2.PK;
			party = messageSendingObject.Applicant;
			NUnit.Framework.Assert.That(party.CustomsControlID, NUnit.Framework.Is.EqualTo("CPW111").Using(CustomComparers.TypeComparison));

			header.BH_OA_Importer = address3.PK;
			party = messageSendingObject.Applicant;
			NUnit.Framework.Assert.That(party.CustomsControlID, NUnit.Framework.Is.EqualTo("00612348").Using(CustomComparers.TypeComparison));

			header.BH_OA_Importer = address4.PK;
			party = messageSendingObject.Applicant;
			NUnit.Framework.Assert.That(party.CustomsControlID, NUnit.Framework.Is.EqualTo("00987654").Using(CustomComparers.TypeComparison));

			header.BH_OA_Importer = address5.PK;
			party = messageSendingObject.Applicant;
			NUnit.Framework.Assert.That(party.CustomsControlID, NUnit.Framework.Is.EqualTo("PAA01").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUnloadingLocation()
		{
			moveHeader.BM_RL_NKForeignDestPort = "USLAX";
			NUnit.Framework.Assert.That(messageSendingObject.UnloadingLocation, NUnit.Framework.Is.EqualTo("USLAX").Using(CustomComparers.TypeComparison));
			moveHeader.BM_ForeignDestPortKCode = "UULAX";
			NUnit.Framework.Assert.That(messageSendingObject.UnloadingLocation, NUnit.Framework.Is.EqualTo("UULAX").Using(CustomComparers.TypeComparison));
			moveHeader.BM_RL_NKForeignDestPort = "";
			NUnit.Framework.Assert.That(messageSendingObject.UnloadingLocation, NUnit.Framework.Is.EqualTo("UULAX").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<CusInBondHeader>();
			arrivalBill = header.ArrivalBill;
			moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.InBondMoveDetail;
			var moveLine = moveDetail.InBondMoveLineItem;
			messageSendingObject = new N5301MessageSendingObject(header);
		}

		CusInBondHeader header;
		CusInBondBill arrivalBill;
		CusInBondMoveHeader moveHeader;
		N5301MessageSendingObject messageSendingObject;
	}
}
