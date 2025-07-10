using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business.InterfaceImplementations;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Customs.Testing;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobDeclarationCustomsChargesTest : TestCaseWithFactory
	{
		public void TestIsActiveConsidersDeclarationCompany()
		{
			var currentDeclaration = Factory.New<BaseJobDeclaration>();

			ICustomsCharges chargesHost = new JobDeclarationCustomsCharges(currentDeclaration);
			Assert("Should be active", chargesHost.IsActive);

			var otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_RN_NKCountryCode = currentDeclaration.CountryCode;
			var branch = otherCompany.Branches.AddNew();
			branch.GB_RL_NKHomePort = currentDeclaration.Branch.GB_RL_NKHomePort;

			var otherDeclaration = Factory.New<BaseJobDeclaration>();
			otherDeclaration.JE_GB = branch.PK;
			chargesHost = new JobDeclarationCustomsCharges(otherDeclaration);
			Assert("Should noit be active", !chargesHost.IsActive);
		}

		public void TestCustomsCharges()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.FillWithValidTestData();
			var decAsCustomsCharges = ServiceLocator.GetService<ICustomsCharges>(dec);

			var feeCodes = new EntryChargeTypeListForTesting();
			feeCodes.Add("C1", "ChargeCode1", true, "");
			feeCodes.Add("C1", "ChargeCode1", true, "");
			feeCodes.Add("C2", "ChargeCode2", true, "");
			feeCodes.Add("C3", "ChargeCode3", false, "");

			var mock1 = Factory.NewMoq<CusEntryHeader>();
			mock1
				.Protected()
				.Setup<EntryChargeTypeList>("GetEntryChargeTypeList")
				.Returns(feeCodes);

			mock1.Setup(m => m.IsFeePaidByBroker("C1", It.IsAny<ZString>(), It.IsAny<Integration.ILogger>()))
				.Returns(true);
			mock1.Setup(m => m.IsFeePaidByBroker("C2", It.IsAny<ZString>(), It.IsAny<Integration.ILogger>()))
				.Returns(true);

			var mock2 = Factory.NewMoq<CusEntryHeader>();
			mock2
				.Protected()
				.Setup<EntryChargeTypeList>("GetEntryChargeTypeList")
				.Returns(feeCodes);

			mock2.Setup(m => m.IsFeePaidByBroker("C1", It.IsAny<ZString>(), It.IsAny<Integration.ILogger>()))
				.Returns(true);
			mock2.Setup(m => m.IsFeePaidByBroker("C2", It.IsAny<ZString>(), It.IsAny<Integration.ILogger>()))
				.Returns(false);
			mock2.Setup(m => m.IsFeePaidByBroker("C3", It.IsAny<ZString>(), It.IsAny<Integration.ILogger>()))
				.Returns(true);

			var entry1 = mock1.Object;
			var entry2 = mock2.Object;

			entry1.Charges.AddNew("C1", 50m);
			entry1.Charges.AddNew("C2", 120m);

			entry2.Charges.AddNew("C1", 60m);
			entry2.Charges.AddNew("C2", 130m);
			entry2.Charges.AddNew("C3", 300m);

			dec.CustomsEntryHeaders.Add(entry1);
			dec.CustomsEntryHeaders.Add(entry2);
			entry1.SetDeclarationForTesting(dec);
			entry2.SetDeclarationForTesting(dec);

			var charges = decAsCustomsCharges.GetCustomsCharges(null);
			Array.Sort(charges, new CustomsChargeComparerForTest());

			AssertEquals(3, charges.Length);
			AssertCustomsCharge(charges[0], 110m, "ChargeCode1");   // 50
			AssertCustomsCharge(charges[1], 120m, "ChargeCode2");   // 60
			AssertCustomsCharge(charges[2], 130m, "ChargeCode2");   // 250
		}

		public void TestGetCustomsCharges_IncludeEntryReference()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			RatingDataRegistry.Instance.IncludeEntryHeaderReferenceInCustomsDisbursementCharges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();

			CusEntryHeader header = dec.CustomsEntryHeaders.AddNew();
			header.CH_BGMReference = "EntryRef1";

			CusEntryHeaderCharges charge1 = header.Charges.AddNew();
			charge1.C1_ChargeAmount = 100m;
			charge1.C1_ChargeType = header.EntryChargeTypeList[0].Code;

			CusEntryHeaderCharges charge2 = header.Charges.AddNew();
			charge2.C1_ChargeAmount = 70m;
			charge2.C1_ChargeType = header.EntryChargeTypeList[1].Code;

			CusEntryHeader header2 = dec.CustomsEntryHeaders.AddNew();
			header2.CH_JE = dec.PK;
			header2.CH_BGMReference = "EntryRef2";

			CusEntryHeaderCharges charge3 = header2.Charges.AddNew();
			charge3.C1_ChargeAmount = 60m;
			charge3.C1_ChargeType = header.EntryChargeTypeList[0].Code;

			ICustomsCharges chargeProvider = new JobDeclarationCustomsCharges(dec);
			AssertEquals(3, chargeProvider.GetCustomsCharges(null).Length);

			AssertEquals(header.EntryChargeTypeList[0].Description, chargeProvider.GetCustomsCharges(null)[0].Description);
			AssertEquals(100m, chargeProvider.GetCustomsCharges(null)[0].Amount);
			AssertEquals("EntryRef1", chargeProvider.GetCustomsCharges(null)[0].EntryReference);

			AssertEquals(header.EntryChargeTypeList[1].Description, chargeProvider.GetCustomsCharges(null)[1].Description);
			AssertEquals(70m, chargeProvider.GetCustomsCharges(null)[1].Amount);
			AssertEquals("EntryRef1", chargeProvider.GetCustomsCharges(null)[1].EntryReference);

			AssertEquals(header.EntryChargeTypeList[0].Description, chargeProvider.GetCustomsCharges(null)[2].Description);
			AssertEquals(60m, chargeProvider.GetCustomsCharges(null)[2].Amount);
			AssertEquals("EntryRef2", chargeProvider.GetCustomsCharges(null)[2].EntryReference);
		}

		public void TestGetCustomsCharges_ExcludeEntryReference()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			RatingDataRegistry.Instance.IncludeEntryHeaderReferenceInCustomsDisbursementCharges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();

			CusEntryHeader header = dec.CustomsEntryHeaders.AddNew();
			header.CH_BGMReference = "EntryRef1";

			CusEntryHeaderCharges charge1 = header.Charges.AddNew();
			charge1.C1_ChargeAmount = 100m;
			charge1.C1_ChargeType = header.EntryChargeTypeList[0].Code;

			CusEntryHeaderCharges charge2 = header.Charges.AddNew();
			charge2.C1_ChargeAmount = 70m;
			charge2.C1_ChargeType = header.EntryChargeTypeList[1].Code;

			CusEntryHeader header2 = dec.CustomsEntryHeaders.AddNew();
			header2.CH_BGMReference = "EntryRef2";

			CusEntryHeaderCharges charge3 = header2.Charges.AddNew();
			charge3.C1_ChargeAmount = 60m;
			charge3.C1_ChargeType = header.EntryChargeTypeList[0].Code;

			ICustomsCharges chargeProvider = new JobDeclarationCustomsCharges(dec);
			AssertEquals(2, chargeProvider.GetCustomsCharges(null).Length);

			AssertEquals(header.EntryChargeTypeList[0].Description, chargeProvider.GetCustomsCharges(null)[0].Description);
			AssertEquals(160m, chargeProvider.GetCustomsCharges(null)[0].Amount);
			AssertEquals("", chargeProvider.GetCustomsCharges(null)[0].EntryReference);

			AssertEquals(header.EntryChargeTypeList[1].Description, chargeProvider.GetCustomsCharges(null)[1].Description);
			AssertEquals(70m, chargeProvider.GetCustomsCharges(null)[1].Amount);
			AssertEquals("", chargeProvider.GetCustomsCharges(null)[1].EntryReference);
		}

		void AssertCustomsCharge(CustomsCharge charge, ZDecimal expectedAmount, ZString expectedDescription)
		{
			AssertEquals("Invalid Amount", expectedAmount, charge.Amount);
			AssertEquals("Invalid Description", expectedDescription, charge.Description);
		}
	}
}
