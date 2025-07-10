using System;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AddressCombiner))]
	sealed class AddressCombinerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = GetNewAddressForTesting(org);
			var address2 = GetNewAddressForTesting(org);
			address1.Language = Core.SharedConstants.Languages.EnglishAmerican;
			address2.Language = Core.SharedConstants.Languages.ChineseSimplified;
			return new AddressCombiner(Factory, address1, address2);
		}
		OrgAddress GetNewAddressForTesting(OrgHeader org)
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = org.PK;
			address.OA_Code = Guid.NewGuid().ToString().Substring(0, 25);
			return address;
		}

		public void TestSetCapabilities()
		{
			var addressCombiner = (AddressCombiner)GetNewBusinessObject();
			var address1 = addressCombiner.Address1;
			var address2 = addressCombiner.Address2;

			var capability1 = Factory.NewWithValidTestData<OrgAddressCapability>();
			capability1.PZ_AddressType = "OFC";
			capability1.PZ_IsMainAddress = true;
			capability1.PZ_OA = address1.PK;

			var capability2 = Factory.NewWithValidTestData<OrgAddressCapability>();
			capability2.PZ_AddressType = "PAD";
			capability2.PZ_IsMainAddress = false;
			capability2.PZ_OA = address2.PK;

			AssertEquals(1, address2.CapabilitiesCollection.Count);
			addressCombiner.SetCapabilities(address1, address2);
			AssertEquals(2, address2.CapabilitiesCollection.Count);

			var addressCombiner2 = (AddressCombiner)GetNewBusinessObject();
			address1 = addressCombiner2.Address1;
			address2 = addressCombiner2.Address2;

			capability1 = Factory.NewWithValidTestData<OrgAddressCapability>();
			capability1.PZ_AddressType = "OFC";
			capability1.PZ_IsMainAddress = true;
			capability1.PZ_OA = address1.PK;

			capability2 = Factory.NewWithValidTestData<OrgAddressCapability>();
			capability2.PZ_AddressType = "OFC";
			capability2.PZ_OA = address2.PK;
			addressCombiner.SetCapabilities(address1, address2);
			Assert(capability2.PZ_IsMainAddress);
		}

		public void TestOrgAddressToTranslatedAddress()
		{
			var addressCombiner = (AddressCombiner)GetNewBusinessObject();
			Factory.Save();
			var translatedAddress = addressCombiner.CreateTranslatedAddress(addressCombiner.Address1, addressCombiner.Address2);
			AssertEquals(translatedAddress.OTA_OA, addressCombiner.Address2.PK);
			AssertEquals(translatedAddress.Language, addressCombiner.Address1.Language);
			AssertEquals(translatedAddress.Address1, addressCombiner.Address1.Address1);
			AssertEquals(translatedAddress.Address2, addressCombiner.Address1.Address2);
			AssertEquals(translatedAddress.City, addressCombiner.Address1.City);
			AssertEquals(translatedAddress.CompanyName, addressCombiner.Address1.CompanyName);
			AssertEquals(translatedAddress.State, addressCombiner.Address1.State);
			AssertEquals(translatedAddress.AddressMap, addressCombiner.Address1.AddressMap);
		}

		public void TestCreateTranslatedAddressWithSameCapabilitiesAsMainAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = GetNewAddressForTesting(org);
			var address2 = GetNewAddressForTesting(org);
			org.Addresses.Reload(true);
			AssertEquals(3, org.Addresses.Count);
			var addressCombiner = new AddressCombiner(Factory, address1, address2);
			var capability1 = Factory.New<OrgAddressCapability>();
			capability1.PZ_AddressType = "OFC";
			var capability2 = Factory.New<OrgAddressCapability>();
			capability2.PZ_AddressType = "OFC";
			capability2.PZ_IsMainAddress = false;
			capability1.PZ_IsMainAddress = true;
			capability1.PZ_OA = addressCombiner.Address1.PK;
			capability2.PZ_OA = addressCombiner.Address2.PK;
			var result = addressCombiner.CombineAsTranslatedAddressInBulkTransaction(AddressCombiner.CombineOptions.TranslateAddress1);
			org.Addresses.Reload(true);
			AssertEquals(2, org.Addresses.Count);
			Assert("Address 2 is the main address", capability2.PZ_IsMainAddress);
			Assert(capability1.IsDeleted);
		}

		public void TestUpdatesAllForeignTableReferencesAndDeletesOriginalRelatedRecord()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var oldAddress = GetNewAddressForTesting(org);

			var addressCapability = Factory.NewWithValidTestData<OrgAddressCapability>();
			addressCapability.PZ_OA = oldAddress.PK;
			var orgPatternMatch = Factory.NewWithValidTestData<OrgPatternMatch>();
			orgPatternMatch.OS_OA = oldAddress.PK;
			var accTransactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			accTransactionHeader.AH_OA_InvoiceAddressOverride = oldAddress.PK;
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_OA_Address = oldAddress.PK;
			var orgSalesCall = Factory.NewWithValidTestData<OrgSalesCall>();
			orgSalesCall.OQ_OA_LocationAddress = oldAddress.PK;
			var orgOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			orgOpportunity.P8_OA = oldAddress.PK;
			orgOpportunity.P8_OA_AssignedOffice = oldAddress.PK;
			var orgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			orgCusCode.OK_OA_PremisesAddress = oldAddress.PK;

			var newAddress = GetNewAddressForTesting(org);
			Factory.Save();
			AssertEquals(oldAddress.PK, addressCapability.PZ_OA);
			AssertEquals(oldAddress.PK, accTransactionHeader.AH_OA_InvoiceAddressOverride);
			AssertEquals(oldAddress.PK, jobDocAddress.E2_OA_Address);
			AssertEquals(oldAddress.PK, orgSalesCall.OQ_OA_LocationAddress);
			AssertEquals(oldAddress.PK, orgOpportunity.P8_OA);
			AssertEquals(oldAddress.PK, orgCusCode.OK_OA_PremisesAddress);
			var addressCombiner = new AddressCombiner(Factory, oldAddress, newAddress);
			var result = addressCombiner.CombineAsTranslatedAddressInBulkTransaction(AddressCombiner.CombineOptions.TranslateAddress1);
			AssertEquals(AddressCombiner.CombineResult.Success, result);
			Factory.ReloadAll<AccTransactionHeader>();
			Factory.ReloadAll<JobDocAddress>();
			Factory.ReloadAll<OrgSalesCall>();
			Factory.ReloadAll<OrgOpportunity>();
			Factory.ReloadAll<OrgCusCode>();
			AssertEquals(newAddress.PK, accTransactionHeader.AH_OA_InvoiceAddressOverride);
			AssertEquals(newAddress.PK, jobDocAddress.E2_OA_Address);
			AssertEquals(newAddress.PK, orgSalesCall.OQ_OA_LocationAddress);
			AssertEquals(newAddress.PK, orgOpportunity.P8_OA);
			AssertEquals(newAddress.PK, orgCusCode.OK_OA_PremisesAddress);
			Assert(oldAddress.IsDeleted);
			Assert(newAddress.TranslatedAddresses.Any());
			AssertEquals(2, newAddress.AddressLanguagePack.Count);
		}

		public void TestCombinerFailsWhenFromAddressHasExistingTranslatedAddresses()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var oldAddress = GetNewAddressForTesting(org);

			var addressCombiner = new AddressCombiner(Factory, oldAddress, GetNewAddressForTesting(org));
			Factory.Save();
			var translatedAddress = addressCombiner.CreateTranslatedAddress(addressCombiner.Address1, addressCombiner.Address2);
			oldAddress.TranslatedAddresses.Add(translatedAddress);
			var result = addressCombiner.CombineAsTranslatedAddressInBulkTransaction(AddressCombiner.CombineOptions.TranslateAddress1);
			AssertEquals(AddressCombiner.CombineResult.FailedSourceAddressHasExistingTranslatedRecords, result);

			var newAddress = GetNewAddressForTesting(org);
			var addressCombiner2 = new AddressCombiner(Factory, GetNewAddressForTesting(org), newAddress);
			Factory.Save();
			var translatedAddress2 = newAddress.AddNewTranslatedAddress();
			translatedAddress2.Language = Core.SharedConstants.Languages.ChineseSimplified;
			addressCombiner2.Address1.Language = Core.SharedConstants.Languages.ChineseSimplified;
			addressCombiner2.Address1.TranslatedAddresses.DeleteAll();
			result = addressCombiner.CombineAsTranslatedAddressInBulkTransaction(AddressCombiner.CombineOptions.TranslateAddress2);
			AssertEquals(AddressCombiner.CombineResult.FailedTargetAddressHasSameLanguageTranslatedRecords, result);
		}

		public void TestCanCombineRules()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = GetNewAddressForTesting(org);
			var address2 = GetNewAddressForTesting(org);
			Factory.Save();
			address1.Language = Core.SharedConstants.Languages.English;
			address2.Language = Core.SharedConstants.Languages.English;
			var addressCombiner = new AddressCombiner(Factory, address1, address2);
			Assert("Addresses are the same language", !addressCombiner.CanCombine());
			address1.Language = Core.SharedConstants.Languages.English;
			address2.Language = Core.SharedConstants.Languages.ChineseSimplified;
			Assert("Addresses should not be the same language", addressCombiner.CanCombine());
			var capability1 = Factory.NewWithValidTestData<OrgAddressCapability>();
			capability1.PZ_AddressType = "OFC";
			capability1.PZ_IsMainAddress = true;
			var capability2 = Factory.NewWithValidTestData<OrgAddressCapability>();
			capability2.PZ_AddressType = "PAD";
			capability2.PZ_IsMainAddress = true;
			capability1.PZ_OA = address1.PK;
			capability2.PZ_OA = address2.PK;
			Assert("Different capabilities are set as main address", addressCombiner.CanCombine());
			capability2.PZ_IsMainAddress = true;
			Assert("Different capabilities are set with both as main addresses", addressCombiner.CanCombine());
			capability2.PZ_AddressType = "OFC";
			capability2.PZ_IsMainAddress = false;
			Assert("The same capabilities exist and one is set as the main address", addressCombiner.CanCombine());
			var translatedAddress = address1.AddNewTranslatedAddress();
			translatedAddress.Language = Core.SharedConstants.Languages.English;
			address2.Language = Core.SharedConstants.Languages.English;
			Assert("The same language is on existing translated address records", !addressCombiner.CanCombine());
		}

		public void TestCombineWorksWithNewDatabaseSchemaObject()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var oldAddress = GetNewAddressForTesting(org);
			var newAddress = GetNewAddressForTesting(org);
			Factory.Save();

			var sqlNewColumnText = "ALTER TABLE dbo.RefCarrierConsortium ADD RG_OA uniqueidentifier NULL";
			using (var cmd = Db.Connection.Command(sqlNewColumnText))
			{
				cmd.ExecuteNonQuery();
			}

			var testPK = Guid.NewGuid();
			var sqlInsertText = string.Format(CultureInfo.InvariantCulture, "INSERT INTO dbo.RefCarrierConsortium (RG_PK, RG_Code, RG_OH, RG_OA) VALUES ('{0}', 'test','{1}', '{2}')", testPK, org.PK, oldAddress.PK);
			using (var cmd = Db.Connection.Command(sqlInsertText))
			{
				cmd.ExecuteNonQuery();
			}

			var addressCombiner = new AddressCombiner(Factory, oldAddress, newAddress);
			addressCombiner.CombineAsTranslatedAddressInBulkTransaction(AddressCombiner.CombineOptions.TranslateAddress1);
			var sqlSavedRecord = string.Format(CultureInfo.InvariantCulture, "SELECT RG_OA FROM dbo.RefCarrierConsortium WHERE RG_PK = '{0}'", testPK);
			using (var result = Db.Connection.Command(sqlSavedRecord).ExecuteReader())
			{
				string returnValue = string.Empty;
				while (result.Read())
				{
					returnValue = result.GetValue(0).ToString();
				}
				AssertEquals(newAddress.PK.ToString(), returnValue);
			}
		}
	}
}
