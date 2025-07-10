using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance.Portugal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class ManyToOneMergerTest : TestCaseWithFactory
	{
		public void TestNotificationIncludesOrgCode()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Name = "1234";

			CreateOrgHeader("~111~", "DOUBLE AXE", "Backpack, on the left", "DWARF", true, false, "111@yandex.ru");
			var notMerge1 = CreateOrgHeader("~333~", "FLAMBERGE", "Backpack, on the left", "KNGHT", true, true, "222@ukr.net"); //should not be merged as consignee=true

			var data1 = Factory.New<OrgCompanyData>();
			data1.OB_OH = notMerge1;
			data1.OB_GC = company.PK;
			data1.OB_IsCreditor = true;
			data1.OB_IsDebtor = false;

			Factory.Save();

			using (OrganisationRegistry.Instance.AllowMergeIgnoringARAP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var merger = new ManyToOneMerger(new[] { "~333~" }, "~111~");
				var message = merger.Process();
				AssertNotNull(message);
				AssertMultilineASCIIEquals("Should include org code", @"~333~: Error - NewOrganisationPk: The old organization has been setup with the following Organization Types. It can only be merged into another organization that is also setup with these types below:
Payable (1234)", message);
			}
		}

		public void TestConstructor()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new ManyToOneMerger(null, "a"); });
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new ManyToOneMerger(null, ""); });
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new ManyToOneMerger(null, null); });
			AssertExceptionThrown(typeof(ArgumentException), delegate
			{ new ManyToOneMerger(Array.Empty<string>(), ""); });
			AssertExceptionThrown(typeof(ArgumentException), delegate
			{ new ManyToOneMerger(Array.Empty<string>(), null); });
			AssertNoExceptionThrown(delegate
			{ new ManyToOneMerger(Array.Empty<string>(), "a"); });
		}

		[ExpectNoExceptions]
		public void TestMerge()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Name = "1234";

			var newOrg = CreateOrgHeader("~111~", "DOUBLE AXE", "Backpack, on the left", "DWARF", true, false, "111@yandex.ru");
			var merged1 = CreateOrgHeader("~222~", "FLAMBERGE", "Backpack, on the left", "KNGHT", true, false, null); //should be merged, address merged
			var notMerged1 = CreateOrgHeader("~333~", "FLAMBERGE", "Backpack, on the left", "KNGHT", true, true, "222@ukr.net"); //should not be merged as consignee=true
			var notMerged2 = CreateOrgHeader("~444~", "FLAMBERGE", "Backpack, on the left", "KNGHT", false, true, "111@yandex.ru"); //should not be merged as consignee=true
			var merged2 = CreateOrgHeader("~555~", "BATTLE HAMMER", "In hands", "SMITH", true, false, "111@yandex.ru"); //should be merged, address added, contact merged
			var merged3 = CreateOrgHeader("~666~", "WHITE GLOVES", "Backpack, on the left", "DWARF", false, false, "111@mail.ru"); //should be merged, address merged, contact added

			var data1 = Factory.New<OrgCompanyData>();
			data1.OB_OH = notMerged1;
			data1.OB_GC = company.PK;
			data1.OB_IsCreditor = true;
			data1.OB_IsDebtor = false;

			var data2 = Factory.New<OrgCompanyData>();
			data2.OB_OH = notMerged2;
			data2.OB_GC = company.PK;
			data2.OB_IsCreditor = false;
			data2.OB_IsDebtor = true;

			Factory.Save();

			using (OrganisationRegistry.Instance.AllowMergeIgnoringARAP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var merger = new ManyToOneMerger(new string[] { "~222~", "~333~", "~444~", "~555~", "~666~", "~777~", "~111~" }, "~111~");
				var message = merger.Process();
				AssertNotNull(message);
				Assert("Creditor should not be merged", message.Contains("Payable (1234)"));
				Assert("Debtor should not be merged", message.Contains("Receivable (1234)"));
				Assert(message.Contains("Organization with code '~777~' does not exist. No merging is possible."));
				Assert(message.Contains("Cannot merge Organization with itself."));
			}

			var newFactory = new BusinessObjectFactory();
			AssertNull(newFactory.Load<OrgHeader>(merged1));
			AssertNull(newFactory.Load<OrgHeader>(merged2));
			AssertNull(newFactory.Load<OrgHeader>(merged3));
			AssertNotNull(newFactory.Load<OrgHeader>(notMerged1));
			AssertNotNull(newFactory.Load<OrgHeader>(notMerged2));

			var newOrgHeader = newFactory.Load<OrgHeader>(newOrg);
			AssertEquals("One original, one added, other merged into original", 2, newOrgHeader.AddressesNoAutoCreate.Count);
			AssertEquals("One original, one added, one merged into original", 2, newOrgHeader.Contacts.Count);
		}

		[ExpectNoExceptions]
		public void TestMergeWithoutError()
		{
			CreateOrgHeader("~111~", "DOUBLE AXE", "Backpack, on the left", "DWARF", true, false, "111@yandex.ru");
			CreateOrgHeader("~222~", "FLAMBERGE", "Backpack, on the left", "KNGHT", true, false, null); //should be merged, address merged
			CreateOrgHeader("~333~", "BATTLE HAMMER", "In hands", "SMITH", true, false, "111@yandex.ru"); //should be merged, address added, contact merged
			CreateOrgHeader("~444~", "WHITE GLOVES", "Backpack, on the left", "DWARF", false, false, "111@mail.ru"); //should be merged, address merged, contact added
			Factory.Save();

			using (OrganisationRegistry.Instance.AllowMergeIgnoringARAP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var merger = new ManyToOneMerger(new[] { "~222~", "~333~", "~444~" }, "~111~");
				AssertEquals(string.Empty, merger.Process());
			}
		}

		[ExpectNoExceptions]
		public void TestMergeIntoNotExistingOrg()
		{
			CreateOrgHeader("~111~", "DOUBLE AXE", "Backpack, on the left", "DWARF", true, false, "111@yandex.ru");
			var merger = new ManyToOneMerger(new[] { "~111~" }, "~222~");
			var message = merger.Process();
			Assert(message.Contains("Organization with code '~222~' does not exist. No merging is possible."));
		}

		[ExpectNoExceptions]
		public void TestMergeWhenOrgHeaderMergingCheckerIsNotAllowed()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;

			var retainedOrg = Factory.NewWithValidTestData<OrgHeader>();
			retainedOrg.OH_Code = "RETAINEDORG";
			retainedOrg.OH_FullName = "RETAINED ORG PTY";
			retainedOrg.MainAddress.OA_Address1 = "Test Address 1";
			retainedOrg.MainAddress.OA_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var dissolvedOrg = Factory.NewWithValidTestData<OrgHeader>();
			dissolvedOrg.OH_Code = "DISSOLVEDORG";
			dissolvedOrg.OH_FullName = "DISSOLVED ORG PTY";
			dissolvedOrg.MainAddress.OA_Address1 = "Test Address 2";

			var dissolvedOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			dissolvedOrg1.OH_Code = "TESTORG";
			dissolvedOrg1.OH_FullName = "DISSOLVED ORG PTY1";
			dissolvedOrg1.MainAddress.OA_Address1 = "Test Address 3";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var retainedOrgCusCode = AddIVACustomsCode(retainedOrg, "123");
				var dissolvedOrgCusCode = AddIVACustomsCode(dissolvedOrg, "456");
				CreatePostedTransaction(retainedOrg, company);
				CreatePostedTransaction(dissolvedOrg, company);
				Factory.Save();

				AssertEquals(true, retainedOrgCusCode.IsThereAnyTransactionForOrgWithRestrictedCusCode());
				AssertEquals(true, dissolvedOrgCusCode.IsThereAnyTransactionForOrgWithRestrictedCusCode());

				var merger = new ManyToOneMerger(new string[] { dissolvedOrg.OH_Code, dissolvedOrg1.OH_Code }, retainedOrg.OH_Code);
				var message = merger.Process();

				var newFactory = new BusinessObjectFactory();
				AssertNotNull(newFactory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "RETAINEDORG"));
				AssertNotNull(newFactory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DISSOLVEDORG"));
				AssertNull(newFactory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "TESTORG"));
				AssertEquals("DISSOLVED ORG PTY: The organizations have different PT IVA registration codes and posted transactions linked to these codes. Please mark the organization to dissolve as inactive.", message);
			}
		}

		#region Implementation

		ZGuid CreateOrgHeader(ZString code, ZString orgName, ZString orgAddress, ZString unloco, bool isConsignor, bool isConsignee, string contactEmail)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_IsConsignee = true;
			org.OH_FullName = orgName;
			org.OH_RL_NKClosestPort = unloco;
			org.MainAddress.OA_Code = GetRandomString(OrgAddressSchema.OA_Code.MaxLength);
			org.MainAddress.OA_Address1 = orgAddress;
			org.OH_Code = code;
			org.OH_IsConsignee = isConsignee;
			org.OH_IsConsignor = isConsignor;
			if (!string.IsNullOrEmpty(contactEmail))
			{
				var cnt = org.Contacts.AddNew();
				cnt.OC_ContactName = GetRandomString(OrgContactSchema.OC_ContactName.MaxLength);
				cnt.OC_Email = contactEmail;
			}

			return org.PK;
		}

		ZString GetRandomString(int length)
		{
			return new ZString(ZGuid.NewZGuid().ToString()).Replace("-", "").Left(length);
		}

		OrgCusCode AddIVACustomsCode(OrgHeader header, string customsRegNo)
		{
			var retainedOrgCusCode = header.CustomsCodes.AddNew();
			retainedOrgCusCode.OK_OH = header.PK;
			retainedOrgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			retainedOrgCusCode.OK_CodeType = OrgCusCode.CodeTypes.IVA;
			retainedOrgCusCode.OK_CustomsRegNo = customsRegNo;
			var newAddress = header.Addresses.AddNew();
			newAddress.OA_Address1 = "newAddress";
			retainedOrgCusCode.OK_OA_PremisesAddress = newAddress.PK;

			return retainedOrgCusCode;
		}

		void CreatePostedTransaction(OrgHeader header, GlbCompany company)
		{
			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = TransactionTypes.Invoice;
			transactionHeader.AH_OH = header.PK;
			transactionHeader.AH_GC = company.PK;

			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AH = transactionHeader.PK;
			transactionLine.AL_OH = header.PK;
			transactionLine.AL_GC = company.PK;
			transactionLine.AL_LineType = TransactionLineTypes.Revenue;
			transactionLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
		}

		#endregion
	}
}
