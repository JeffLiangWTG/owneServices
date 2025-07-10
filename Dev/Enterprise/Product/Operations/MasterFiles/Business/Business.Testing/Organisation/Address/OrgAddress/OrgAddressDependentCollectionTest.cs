using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgAddressDependentCollection))]
	sealed class OrgAddressDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgHeader testHeader = Factory.New<OrgHeader>();
			return new OrgAddressDependentCollection(testHeader);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(OrgAddress));
		}

		public void TestGetGlobalDefaultAddressOfTypeAndLanguageDoesNotThrowExceptionIfCompanyIsNull()
		{
			var userContextMock = new Mock<IUserContext>();
			var userContext = Env.CurrentUserContext;
			userContextMock
				.SetupGet(context => context.Branch)
				.Returns(userContext.Branch);

			var header = Factory.New<OrgHeader>();
			header.OH_RL_NKClosestPort = "NZAKL";
			header.OH_IsGlobalAccount = ZBool.True;

			var address = Factory.New<OrgAddress>();
			address.OA_RL_NKRelatedPortCode = "AUBNE";
			address.OA_Language = Constants.Languages.Albanian;
			header.Addresses.Add(address);
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

			using (Env.SetTemporaryUserContext(userContextMock.Object))
			{
				AssertNull("Precondition: ", GlbCompany.CurrentCompany);
				AssertNoExceptionThrown(() => { header.Addresses.DefaultAddressOfType(OrgAddressType.Office); });
			}
		}

		public void TestDefaultAddressOfTypeInCommonLanguage()
		{
			OrgHeader organization = Factory.New<OrgHeader>();
			organization.OH_Language = Constants.Languages.ChineseTraditional;
			OrgAddress mainAddress = organization.MainAddress;
			mainAddress.OA_Language = Constants.Languages.English;

			AssertEquals("organization.Addresses.DefaultAddressOfType(OrgAddressType.Office, true)", mainAddress,
				organization.Addresses.DefaultAddressOfType(OrgAddressType.Office, true));

			OrgAddress englishAddress = organization.Addresses.AddNew();
			englishAddress.OA_Language = Constants.Languages.English;
			englishAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Receivables.Code);
			englishAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Receivables.Code);
			AssertEquals("organization.Addresses.DefaultAddressOfType(OrgAddressType.Receivables, true)", englishAddress,
				organization.Addresses.DefaultAddressOfType(OrgAddressType.Receivables, true));

			OrgAddress chineseAddress = organization.Addresses.AddNew();
			chineseAddress.OA_Language = Constants.Languages.ChineseTraditional;
			chineseAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Receivables.Code);
			chineseAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Receivables.Code);
			AssertEquals("organization.Addresses.DefaultAddressOfType(OrgAddressType.Receivables, true)", englishAddress,
				organization.Addresses.DefaultAddressOfType(OrgAddressType.Receivables, true));

			GlbCompany.CurrentCompany.OrgProxy.OH_Language = Constants.Languages.ChineseTraditional;
			AssertEquals("organization.Addresses.DefaultAddressOfType(OrgAddressType.Receivables, true)", chineseAddress,
				organization.Addresses.DefaultAddressOfType(OrgAddressType.Receivables, true));

			organization.Addresses.Remove(chineseAddress);
			AssertEquals("organization.Addresses.DefaultAddressOfType(OrgAddressType.Receivables, true)", englishAddress,
				organization.Addresses.DefaultAddressOfType(OrgAddressType.Receivables, true));
		}

		#region Remove and delete addresses

		public void TestRemoveItem()
		{
			OrgAddress nonMainAddress = AddAddressToCollection(OrgConstants.AddressType.Pickup);
			AssertEquals("AddressesTestCollection.Count", 2, TestCollection.Count);

			TestCollection.Remove(TestCollection.MainAddress);
			AssertEquals("Cannot remove MainAddress", 2, TestCollection.Count);

			TestCollection.Remove(nonMainAddress);
			AssertEquals("NonMainAddress can be removed", 1, TestCollection.Count);
		}

		public void TestRemoveAndDeleteItem()
		{
			OrgAddress nonMainAddress = AddAddressToCollection(OrgConstants.AddressType.Pickup);
			AssertEquals("AddressesTestCollection.Count", 2, TestCollection.Count);

			TestCollection.RemoveAndDelete(TestCollection.MainAddress);
			AssertEquals("Cannot delete MainAddress", 2, TestCollection.Count);

			TestCollection.RemoveAndDelete(nonMainAddress);
			AssertEquals("NonMainAddress can be delete", 1, TestCollection.Count);
		}

		public void TestRemoveAndDeleteAll()
		{
			OrgAddress nonMainAddress = AddAddressToCollection(OrgConstants.AddressType.Pickup);
			AssertEquals("AddressesTestCollection.Count", 2, TestCollection.Count);

			TestCollection.RemoveAndDeleteAll();
			AssertEquals("Cannot delete MainAddress", 1, TestCollection.Count);

			nonMainAddress = AddAddressToCollection(OrgConstants.AddressType.Sales);
			AssertEquals("AddressesTestCollection.Count", 2, TestCollection.Count);

			TestCollection.RemoveAndDeleteAllIncludingMainAddress();
			AssertEquals("Cannot delete MainAddress", 0, TestCollection.Count);
		}

		#endregion

		public void TestContainsAddressType()
		{
			AddAddressToCollection(OrgConstants.AddressType.Sales);
			AddAddressToCollection(OrgConstants.AddressType.Delivery);
			AddAddressToCollection(OrgConstants.AddressType.Office);

			Assert("Collection contains AddressType", TestCollection.ContainsAddressType(OrgConstants.AddressType.Sales));
			Assert("Collection does not containt AddressType 'XXX'", !TestCollection.ContainsAddressType("XXX"));
		}

		public void TestAddressesOfType()
		{
			AddAddressToCollection(OrgConstants.AddressType.Payables);
			AddAddressToCollection(OrgConstants.AddressType.PickupAndDelivery);
			AddAddressToCollection(OrgConstants.AddressType.Receivables);
			AddAddressToCollection(OrgConstants.AddressType.Receivables);
			var inactiveAddress = AddAddressToCollection(OrgConstants.AddressType.Receivables);
			inactiveAddress.OA_IsActive = false;

			AssertEquals("Collection count", 0, TestCollection.AddressesOfType(OrgConstants.AddressType.Sales).Count);
			AssertEquals("Collection count", 1, TestCollection.AddressesOfType(OrgConstants.AddressType.PickupAndDelivery).Count);
			AssertEquals("Collection count", 2, TestCollection.AddressesOfType(OrgConstants.AddressType.Receivables).Count);
			AssertEquals("Collection count, inactive too", 3, TestCollection.AddressesOfType(OrgConstants.AddressType.Receivables, false).Count);
		}

		public void TestAddressesOfType_CurrentCompanyOnly()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);

			var header = Factory.New<OrgHeader>();
			var address = header.Addresses.AddNew();
			address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);
			address.OA_RN_NKCountryCode = "US";

			header.OH_IsGlobalAccount = false;
			AssertEquals("Collection count", 1, header.Addresses.AddressesOfType(OrgConstants.AddressType.Payables, currentCompanyOnly: true).Count);

			header.OH_IsGlobalAccount = true;
			AssertEquals("Collection count", 0, header.Addresses.AddressesOfType(OrgConstants.AddressType.Payables, currentCompanyOnly: true).Count);
			AssertEquals("Collection count", 1, header.Addresses.AddressesOfType(OrgConstants.AddressType.Payables, currentCompanyOnly: false).Count);
		}

		public void TestAddressesOfType_NullInRelatedCountry()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_IsGlobalAccount = true;

			OrgAddress oneMoreAddress = header.Addresses.AddNew();
			oneMoreAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup);
			OrgAddress secondMoreAddress = header.Addresses.AddNew();
			secondMoreAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal);

			AssertNoExceptionThrown("Should not throw NullReferenceExpcetion when RelatedCountry is null",
				() => header.Addresses.AddressesOfType(OrgAddressType.PickupAndDelivery));
			AssertNoExceptionThrown("Should not throw NullReferenceExpcetion when RelatedCountry is null",
				() => header.Addresses.AddressesOfType(OrgAddressType.Pickup));
			AssertNoExceptionThrown("Should not throw NullReferenceExpcetion when RelatedCountry is null",
				() => header.Addresses.AddressesOfType(OrgAddressType.Postal));
		}

		public void TestAddressesOfCategory()
		{
			AddAddressToCollection(OrgAddressType.Pickup);
			var inactiveAddress = AddAddressToCollection(OrgAddressType.PickupAndDelivery);
			inactiveAddress.OA_IsActive = false;
			AddAddressToCollection(OrgAddressType.Postal);
			AddAddressToCollection(OrgAddressType.Receivables);
			AddAddressToCollection(OrgAddressType.Miscellaneous);

			// Note: There is another address in the collection - the original ALL address that has been changed to OFC automatically
			AssertEquals("Mailing addresses count", 2, TestCollection.AddressesOfCategory(AddressCategory.Mailing).Count);
			AssertEquals("Physical addresses count", 2, TestCollection.AddressesOfCategory(AddressCategory.Physical).Count);
			AssertEquals("Physical addresses count, in inactive", 3, TestCollection.AddressesOfCategory(AddressCategory.Physical, false).Count);
			AssertEquals("Other addresses count", 1, TestCollection.AddressesOfCategory(AddressCategory.Other).Count);
		}

		[ExpectNoExceptions]
		public void TestUnknownAddressTypeDoesntThrowException()
		{
			AddAddressToCollection("ZZZ");
			TestCollection.AddressesOfCategory(AddressCategory.Other);
		}

		public void TestCanBeSetAsDefaultAddress()
		{
			OrgAddress address1 = AddAddressToCollection(OrgConstants.AddressType.Sales);
			Assert("'SAL' can be set as default", TestCollection.CanBeSetAsDefaultAddress(OrgConstants.AddressType.Sales));

			OrgAddress address2 = AddAddressToCollection(OrgConstants.AddressType.Miscellaneous);
			Assert("'MSC' cannot be set as default", !TestCollection.CanBeSetAsDefaultAddress(OrgConstants.AddressType.Miscellaneous));

			address2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Sales);
			Assert("'SAL' cannot be set as default (one already exists)", !TestCollection.CanBeSetAsDefaultAddress(OrgConstants.AddressType.Sales));

			address1.AddressCapability.DisableAllCapabilities();
			address2.AddressCapability.DisableAllCapabilities();
			address1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			address2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			Assert("'PAD' can be set as default", TestCollection.CanBeSetAsDefaultAddress(OrgConstants.AddressType.PickupAndDelivery));
			Assert("'PKU' cannot be set as default", !TestCollection.CanBeSetAsDefaultAddress(OrgConstants.AddressType.Pickup));

			address1.AddressCapability.SetIsNotMainAddress(OrgConstants.AddressType.PickupAndDelivery);
			Assert("'PKU' can be set as default", TestCollection.CanBeSetAsDefaultAddress(OrgConstants.AddressType.Pickup));
		}

		public void TestCustomsAddress()
		{
			OrgAddress address = TestCollection.AddNew();
			address.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			AssertEquals("Address1 is CustomsAddress", address.PK, TestCollection.CustomsAddress.PK);
		}

		public void TestMainAddress()
		{
			OrgAddress address1 = TestCollection[0];
			OrgAddress address2 = AddAddressToNewCollection(TestCollection, OrgConstants.AddressType.Postal);
			OrgAddress address3 = AddAddressToNewCollection(TestCollection, OrgConstants.AddressType.Payables);
			AssertEquals("Address1 is MainAddress", address1.PK, TestCollection.MainAddress.PK);
			AssertEquals("MainAddress is OFC", ZBool.True, TestCollection.MainAddress.AddressCapability.GetCapabilityEnabled(OrgAddressType.Office.Code));

			OrgAddress address4 = AddAddressToNewCollection(TestCollection, OrgAddressType.Office.Code);

			address4.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			Assert("Address1 should not be default", !address1.AddressCapability.GetIsMainAddress(OrgAddressType.Office.Code));
			Assert("Address4 should be default", address4.AddressCapability.GetIsMainAddress(OrgAddressType.Office.Code));
			AssertEquals("Address4 is MainAddress", address4.PK, TestCollection.MainAddress.PK);
			AssertEquals("MainAddress is OFC", ZBool.True, TestCollection.MainAddress.AddressCapability.GetCapabilityEnabled(OrgAddressType.Office.Code));
		}

		public void TestMainAddressNotNull()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddressDependentCollection addresses = new OrgAddressDependentCollection(org);
			AssertEquals("Addresses.Count", 0, addresses.Count);

			OrgAddress mainAddress = addresses.MainAddress;
			AssertNotNull("MainAddress", mainAddress);
			AssertEquals("Addresses.Count", 1, addresses.Count);
		}

		public void TestAddNewMainAddress()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddressDependentCollection addresses = new OrgAddressDependentCollection(org);
			AssertEquals("Addresses.Count", 0, addresses.Count);

			OrgAddress newMainAddress = addresses.AddNewMainAddress();
			AssertNotNull("MainAddress", newMainAddress);
			AssertEquals("Addresses.Count", 1, addresses.Count);

			OrgAddress mainAddress = addresses.MainAddress;
			AssertEquals("MainAddress is same as NewMainAddress", newMainAddress, mainAddress);
		}

		public void TestOrgHasMailingAddress()
		{
			OrgAddress address1 = Org.Addresses[0];
			OrgAddress address2 = Org.Addresses.AddNew();
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.PickupAndDelivery.Code);

			Assert("No mailing address", !Org.Addresses.HasMailingAddress);

			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Payables.Code);
			Assert("Mailing address exists", Org.Addresses.HasMailingAddress);
		}

		public void TestGetAddressWithMainAddressFallback()
		{
			var orgAddress1 = Factory.New<OrgAddress>();
			orgAddress1.OA_RL_NKRelatedPortCode = "AUSYD";
			orgAddress1.OA_Address1 = "ADDRESS1";
			orgAddress1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_RL_NKRelatedPortCode = "AUMEL";
			orgAddress2.OA_Address1 = "ADDRESS2";
			orgAddress2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);
			orgAddress2.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.AWB);

			var orgAddress3 = Factory.New<OrgAddress>();
			orgAddress3.OA_RL_NKRelatedPortCode = "CNSHA";
			orgAddress3.OA_Address1 = "ADDRESS3";
			orgAddress3.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);
			orgAddress3.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
			orgAddress2.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);

			TestCollection.RemoveAll();
			TestCollection.AddRange(orgAddress1, orgAddress2, orgAddress3);

			var resultAddress = TestCollection.GetAddressWithMainAddressFallback("AUSYD", OrgAddressType.AWB);
			AssertEquals("Matched with the port and address type", orgAddress1, resultAddress);

			resultAddress = TestCollection.GetAddressWithMainAddressFallback("AUBNE", OrgAddressType.AWB);
			AssertEquals("Fall back the address with right address capability and marked as main", orgAddress2, resultAddress);

			resultAddress = TestCollection.GetAddressWithMainAddressFallback("AUSYD", OrgAddressType.Pickup);
			AssertNull(resultAddress);
		}

		public void TestGetAddressWithMainAddressFallback_UseActiveAddress()
		{
			var orgAddress1 = Factory.New<OrgAddress>();
			orgAddress1.OA_RL_NKRelatedPortCode = "AUSYD";
			orgAddress1.OA_Address1 = "ADDRESS1";
			orgAddress1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);
			orgAddress1.OA_IsActive = false;

			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_RL_NKRelatedPortCode = "AUMEL";
			orgAddress2.OA_Address1 = "ADDRESS2";
			orgAddress2.OA_IsActive = false;
			orgAddress2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);
			orgAddress2.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.AWB);

			TestCollection.RemoveAll();
			TestCollection.AddRange(orgAddress1, orgAddress2);

			var resultAddress = TestCollection.GetAddressWithMainAddressFallback("AUSYD", OrgAddressType.AWB);
			AssertNull("Only matched active address", resultAddress);

			resultAddress = TestCollection.GetAddressWithMainAddressFallback("AUBNE", OrgAddressType.AWB);
			AssertNull("Only matched active address", resultAddress);
		}

		public void TestOrgHasPhysicalAddress()
		{
			// should always have physical address
			OrgAddress address1 = Org.Addresses[0];
			Assert("OFC is valid Physical Address", Org.Addresses.HasPhysicalAddress);

			OrgAddress address2 = Org.Addresses.AddNew();
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Receivables.Code);
			Assert("OFC is valid Physical Address", Org.Addresses.HasPhysicalAddress);
		}

		public void TestSwapMainAddress()
		{
			OrgAddress address1 = TestCollection[0];
			OrgAddress address2 = AddAddressToCollection(OrgAddressType.Office);
			address1.OA_RL_NKRelatedPortCode = "UAIEV";
			address2.OA_RL_NKRelatedPortCode = "GLBON";
			Assert("Address1 should be default", address1.AddressCapability.GetIsMainAddress(OrgAddressType.Office));
			Assert("Address2 should not be default", !address2.AddressCapability.GetIsMainAddress(OrgAddressType.Office));
			AssertEquals("Address 1 is MainAddress", address1.PK, Org.Addresses.MainAddress.PK);
			AssertEquals("Address 1 is MainAddress", address1.PK, Org.MainAddressCollection[0].PK);
			AssertEquals("UAIEV", Org.OH_RL_NKClosestPort);
			AssertEquals("UAIEV", address1.OA_RL_NKRelatedPortCode);
			AssertEquals("GLBON", address2.OA_RL_NKRelatedPortCode);

			// Swap to new Main Address
			address2.AddressCapability.SetIsMainAddress(OrgAddressType.Office);
			Assert("Address1 should not be default", !address1.AddressCapability.GetIsMainAddress(OrgAddressType.Office));
			Assert("Address2 should be default", address2.AddressCapability.GetIsMainAddress(OrgAddressType.Office));
			AssertEquals("Address2 is MainAddress", address2.PK, TestCollection.MainAddress.PK);
			AssertEquals("Address 2 is MainAddress", address2.PK, Org.Addresses.MainAddress.PK);
			AssertEquals("Address 2 is MainAddress", address2.PK, Org.MainAddressCollection[0].PK);
			AssertEquals("GLBON", Org.OH_RL_NKClosestPort);
			AssertEquals("UAIEV", address1.OA_RL_NKRelatedPortCode);
			AssertEquals("GLBON", address2.OA_RL_NKRelatedPortCode);

			// Swap back again
			address1.AddressCapability.SetIsMainAddress(OrgAddressType.Office);
			Assert("Address1 should be default", address1.AddressCapability.GetIsMainAddress(OrgAddressType.Office));
			Assert("Address2 should not be default", !address2.AddressCapability.GetIsMainAddress(OrgAddressType.Office));
			AssertEquals("Address 1 is MainAddress", address1.PK, Org.Addresses.MainAddress.PK);
			AssertEquals("Address 1 is MainAddress", address1.PK, Org.MainAddressCollection[0].PK);
			AssertEquals("UAIEV", Org.OH_RL_NKClosestPort);
			AssertEquals("UAIEV", address1.OA_RL_NKRelatedPortCode);
			AssertEquals("GLBON", address2.OA_RL_NKRelatedPortCode);
		}

		public void TestAddNewWithParameters()
		{
			Assert("PRE: Only main address", Org.Addresses.Count == 1);

			OrgAddress test1 = Org.Addresses.AddNew(OrgAddressType.Office, ZBool.True);
			AssertEquals("test1 should be the main address", test1, Org.MainAddress);
			Assert("Only one address in system", Org.Addresses.Count == 1);

			OrgAddress test2 = Org.Addresses.AddNew(OrgAddressType.Delivery, ZBool.False);
			Assert("test2 should be in the system", Org.Addresses.Contains(test2.PK));
			Assert("two addresses in system", Org.Addresses.Count == 2);

			OrgAddress test3 = Org.Addresses.AddNew(OrgAddressType.Delivery, ZBool.True);
			Assert("test3 should be in the system", Org.Addresses.Contains(test3.PK));
			Assert("three addresses in system", Org.Addresses.Count == 3);

			OrgAddress test4 = Org.Addresses.AddNew(OrgAddressType.Delivery, ZBool.False);
			Assert("test4 should be in the system", Org.Addresses.Contains(test4.PK));
			Assert("four addresses in system", Org.Addresses.Count == 4);

			OrgAddress test5 = Org.Addresses.AddNew(OrgAddressType.Delivery, ZBool.True);
			AssertEquals("test4 should be the same as test3", test1, Org.MainAddress);
			Assert("four addresses in system", Org.Addresses.Count == 4);
		}

		public void TestBestAddressForPort()
		{
			Org.OH_IsGlobalAccount = true;

			OrgAddress melAddress = TestCollection[0];
			melAddress.OA_RL_NKRelatedPortCode = "AUMEL";
			melAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery.Code);

			OrgAddress sydAddress = TestCollection.AddNew();
			sydAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			sydAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery.Code);

			AssertEquals(sydAddress, TestCollection.BestAddressForPort("AUSYD", OrgAddressType.Delivery.Code));
			AssertEquals(melAddress, TestCollection.BestAddressForPort("AUMEL", OrgAddressType.Delivery.Code));
		}

		#region Global organisations

		public void TestDefaultAddressOfTypeForGlobal()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress mainAddress = header.MainAddress;
			header.OH_RL_NKClosestPort = "NZAKL";
			OrgAddress oneMoreAddress = header.Addresses.AddNew();
			OrgAddress testAddress = header.Addresses.DefaultAddressOfType(OrgAddressType.Office);
			AssertEquals("Org is not global first address is default", testAddress, mainAddress);

			header.OH_IsGlobalAccount = ZBool.True;

			testAddress = header.Addresses.DefaultAddressOfType(OrgAddressType.Office);
			AssertEquals("Org is global, but no address with same as logged in org coutry - MainAddress is default", testAddress, mainAddress);

			OrgAddress secondMain = Factory.New<OrgAddress>();
			secondMain.OA_RL_NKRelatedPortCode = "AUBNE";
			secondMain.OA_Language = Core.Constants.Languages.Albanian;
			header.Addresses.Add(secondMain);
			secondMain.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			testAddress = header.Addresses.DefaultAddressOfType(OrgAddressType.Office);
			AssertEquals("second main is related to AU - should be default", testAddress, secondMain);

			OrgAddress thirdMain = Factory.New<OrgAddress>();
			thirdMain.OA_RL_NKRelatedPortCode = "AUSYD";
			thirdMain.OA_Language = Core.Constants.Languages.English;
			header.Addresses.Add(thirdMain);
			thirdMain.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

			OrgAddress fourthMain = Factory.New<OrgAddress>();
			fourthMain.OA_RL_NKRelatedPortCode = "INBOM";
			fourthMain.OA_Language = Core.Constants.Languages.English;
			header.Addresses.Add(fourthMain);
			fourthMain.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

			testAddress = header.Addresses.DefaultAddressOfType(OrgAddressType.Office);
			AssertEquals("ThirdMain is related to AU and english - should be default", testAddress, thirdMain);
		}

		public void TestAddressesOfTypeForGlobal()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress mainAddress = header.MainAddress;
			header.OH_RL_NKClosestPort = "NZAKL";

			OrgAddress nZAddress = header.Addresses.AddNew();
			nZAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			nZAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Delivery.Code);
			OrgAddress aUAddress = header.Addresses.AddNew();
			aUAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			aUAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Delivery.Code);
			OrgAddress oneMoreAUAddress = header.Addresses.AddNew();
			oneMoreAUAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			oneMoreAUAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Delivery.Code);

			OrgAddressList addressList = header.Addresses.AddressesOfType(OrgAddressType.Delivery);
			AssertEquals("All delivery addresses will be in the list for non-global accounts", 3, addressList.Count);

			header.OH_IsGlobalAccount = ZBool.True;
			addressList = header.Addresses.AddressesOfType(OrgAddressType.Delivery);
			AssertEquals("only delivery addresses based on logged in country will be in the list", 2, addressList.Count);
		}

		#endregion

		#region Tests for Fallback according to Document Address Category

		public void TestAddressFallbackForCommercialDocs()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Consignee.Code;
			menuItem.SU_AddressCategory = OrgAddressCategory.Codes.Commercial;

			OrgAddress address1 = TestCollection[0];
			OrgAddress address2 = AddAddressToCollection(OrgAddressType.Postal);
			OrgAddress address3 = AddAddressToCollection(OrgAddressType.Delivery);
			OrgAddress address4 = AddAddressToCollection(OrgAddressType.Pickup);

			AssertEquals("DLV address returned as fallback address", address3.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Email, true).PK);

			menuItem.SU_ContactType = ContactType.Consignor.Code;
			AssertEquals("PIC address returned as fallback address", address4.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Email, true).PK);

			address4.AddressCapability.SetCapabilityEnabled(OrgAddressType.Miscellaneous.Code);
			address4.AddressCapability.SetCapabilityDisabled(OrgAddressType.Pickup.Code);
			address3.AddressCapability.SetCapabilityEnabled(OrgAddressType.PickupAndDelivery.Code);
			address3.AddressCapability.SetIsMainAddress(OrgAddressType.PickupAndDelivery.Code);
			AssertEquals("PAD address returned as fallback address", address3.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Email, true).PK);

			address3.AddressCapability.DisableAllCapabilities();
			address3.AddressCapability.SetCapabilityEnabled(OrgAddressType.Miscellaneous.Code);
			AssertEquals("OFC address returned as fallback address", address1.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Email, true).PK);
		}

		public void TestAddressFallbackForReceivablesDocs()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Receivables.Code;
			menuItem.SU_AddressCategory = OrgAddressCategory.Codes.Receivables;

			OrgAddress address1 = TestCollection[0];
			OrgAddress address2 = AddAddressToCollection(OrgAddressType.Postal);
			OrgAddress address3 = AddAddressToCollection(OrgAddressType.Receivables);
			OrgAddress address4 = AddAddressToCollection(OrgAddressType.Payables);

			AssertEquals("ARM address returned as first address in fallback", address3.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Email, true).PK);

			address3.AddressCapability.DisableAllCapabilities();
			address3.AddressCapability.SetCapabilityEnabled(OrgAddressType.Miscellaneous.Code);
			AssertEquals("PST address returned as fallback address for print", address2.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Print, true).PK);
			AssertEquals("OFC address returned as fallback address for email/fax", address1.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Email, true).PK);

			address2.AddressCapability.DisableAllCapabilities();
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Miscellaneous.Code);
			AssertEquals("OFC address returned as fallback address for print", address1.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Print, true).PK);
		}

		public void TestAddressFallbackForPayablesDocs()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Payables.Code;
			menuItem.SU_AddressCategory = OrgAddressCategory.Codes.Payables;

			OrgAddress address1 = TestCollection[0];
			OrgAddress address2 = AddAddressToCollection(OrgAddressType.Postal);
			OrgAddress address3 = AddAddressToCollection(OrgAddressType.Receivables);
			OrgAddress address4 = AddAddressToCollection(OrgAddressType.Payables);

			AssertEquals("APM address returned as first address in fallback", address4.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Email, true).PK);

			address4.AddressCapability.DisableAllCapabilities();
			address4.AddressCapability.SetCapabilityEnabled(OrgAddressType.Miscellaneous.Code);
			AssertEquals("PST address returned as fallback address for print", address2.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Print, true).PK);
			AssertEquals("OFC address returned as fallback address for email/fax", address1.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Email, true).PK);

			address2.AddressCapability.DisableAllCapabilities();
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Miscellaneous.Code);
			AssertEquals("OFC address returned as fallback address for print", address1.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Print, true).PK);
		}

		public void TestAddressFallbackForSalesDocs()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Sales.Code;
			menuItem.SU_AddressCategory = OrgAddressCategory.Codes.Sales;

			OrgAddress address1 = TestCollection[0];
			OrgAddress address2 = AddAddressToCollection(OrgAddressType.Postal);
			OrgAddress address3 = AddAddressToCollection(OrgAddressType.Sales);

			AssertEquals("SQM address returned as first address in fallback", address3.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Email, true).PK);

			address3.AddressCapability.DisableAllCapabilities();
			address3.AddressCapability.SetCapabilityEnabled(OrgAddressType.Miscellaneous.Code);
			AssertEquals("PST address returned as fallback address for print", address2.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Print, true).PK);
			AssertEquals("OFC address returned as fallback address for email/fax", address1.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Email, true).PK);

			address2.AddressCapability.DisableAllCapabilities();
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Miscellaneous.Code);
			AssertEquals("OFC address returned as fallback address for print", address1.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Print, true).PK);
		}

		public void TestAddressFallbackForClientFacingDocs()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.ExportFreightAgent.Code;
			menuItem.SU_AddressCategory = OrgAddressCategory.Codes.Office;

			OrgAddress address1 = TestCollection[0];
			OrgAddress address2 = AddAddressToCollection(OrgAddressType.Postal);
			OrgAddress address3 = AddAddressToCollection(OrgAddressType.Miscellaneous);

			AssertEquals("OFC address returned as fallback address", address1.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Fax, true).PK);
		}

		public void TestAddressFallbackForWarehousingDocs()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.ExportFreightAgent.Code;
			menuItem.SU_AddressCategory = OrgAddressCategory.Codes.Transport;

			OrgAddress address1 = TestCollection[0];
			OrgAddress address2 = AddAddressToCollection(OrgAddressType.Postal);
			OrgAddress address3 = AddAddressToCollection(OrgAddressType.PickupAndDelivery);
			OrgAddress address4 = AddAddressToCollection(OrgAddressType.Miscellaneous);

			AssertEquals("PAD address returned as fallback address", address3.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Fax, true).PK);

			address3.AddressCapability.DisableAllCapabilities();
			address4.AddressCapability.DisableAllCapabilities();
			address3.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery.Code);
			address4.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup.Code);
			AssertEquals("PIC address returned as fallback address", address4.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Fax, true).PK);

			address3.AddressCapability.DisableAllCapabilities();
			address4.AddressCapability.DisableAllCapabilities();
			address4.AddressCapability.SetCapabilityEnabled(OrgAddressType.Miscellaneous.Code);
			address3.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery.Code);
			AssertEquals("DLV address returned as fallback address", address3.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Fax, true).PK);

			address3.AddressCapability.DisableAllCapabilities();
			address2.AddressCapability.DisableAllCapabilities();
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Miscellaneous.Code);
			address3.AddressCapability.SetCapabilityEnabled(OrgAddressType.Miscellaneous.Code);
			AssertEquals("OFC address returned as fallback address", address1.PK, TestCollection.AddressForDocument(menuItem, Constants.ContactNotifyModes.Fax, true).PK);
		}

		#endregion

		public void TestErrorReportWhenCreateDuplicateAddressNotOnFile()
		{
			Org.OH_Code = "TEST001";
			Org.OH_FullName = "Test Organization";
			var newAddress1 = TestCollection.AddNew();
			newAddress1.Address1 = "NewAddressWithCode003";
			Factory.Save();
			var newAddress2 = TestCollection.AddNew();
			newAddress2.Address1 = "NewAddressWithCode002";

			using (OrganisationRegistry.Instance.EnableAddressNotOnFileLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestCollection.AddNewMainAddress();
			}

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("DuplicateMainAddress", ErrorReporter.LastKeyReported);
			AssertEquals(@"Duplicate main address should not be created again when it already exists.
-- Additional Information --
LastMainAddress:
[A/N/I/13/M/E/E/N/1/M/E/N] ***Address Not On File***
LastMainAddressDontCreate:
[A/N/I/13/M/E/E/N/1/M/E/N] ***Address Not On File***
LastMainAddressCached:
False
LastMainAddressCachedValue:
NullForLog
mainAddressFromDb:
[A/N/I/13/M/E/E/N/1/M/E/N] ***Address Not On File***
OrgAddressDependentCollection:
[0]: [A/N/I/13/M/E/E/N/1/M/E/N] ***Address Not On File***
[1]: [A/N/N/13/N/N/E/N/0/N/N/N] NewAddressWithCode002
[2]: [A/N/I/13/N/N/E/N/0/N/N/N] NewAddressWithCode003
OrgAddressesFromDb:
[0]: [A/N/I/13/M/E/E/N/1/M/E/N] ***Address Not On File***
[1]: [A/N/I/13/N/N/E/N/0/N/N/N] NewAddressWithCode003
IsParentOrgDeleted:
False
", ErrorReporter.LastMessageReported);
			AssertEquals("Duplicate Main Address Created", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Instance.Clear();
		}

		public void TestNoDuplicateMainAddressCreatedIfLockTimeoutExpired()
		{
			var boFactory = new BusinessObjectFactory();
			var orgHeader = boFactory.NewWithValidTestData<OrgHeader>();
			var address1 = orgHeader.Addresses.AddNew(OrgAddressType.Office, ZBool.True);
			address1.Address1 = "Test address1";
			var address2 = orgHeader.Addresses.AddNew(OrgAddressType.Delivery, ZBool.False);
			address2.Address1 = "Test address2";
			boFactory.Save();

			using (OrganisationRegistry.Instance.EnableAddressNotOnFileLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var curOrg = Factory.Load<OrgHeader>(orgHeader.PK);

				((IDbConnected)Factory).Connection.OnBeforeExecute += ThrowLockRequestTimeOutPeriodExceeded;

				AssertExceptionThrown<Exception>(() =>
				{
					var test = curOrg.Addresses[0].AddressCapability;
				});

				((IDbConnected)Factory).Connection.OnBeforeExecute -= ThrowLockRequestTimeOutPeriodExceeded;

				var mainAddress = curOrg.MainAddress;

				AssertEquals("No duplicate mainAddress created", 0, ErrorReporter.TotalErrorCount);
				AssertEquals("MainAddress is address1", address1.PK, mainAddress.PK);
			}

			void ThrowLockRequestTimeOutPeriodExceeded(DbCommand command)
			{
				if (command.CommandText.Contains("OrgAddressCapability"))
				{
					throw CargoWise.Data.Testing.SqlExceptionBuilder.CreateSqlException(1222, "Lock request time out period exceeded.");
				}
			}
		}

		public void TestErrorReportWhenCreateDuplicateAddressNotOnFile_OnSaving()
		{
			TestErrorReportWhenCreateDuplicateAddressNotOnFile();

			using (OrganisationRegistry.Instance.EnableAddressNotOnFileLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Factory.Save();

				AssertEquals(2, ErrorReporter.TotalErrorCount);
				AssertEquals("DuplicateAddressNotOnFile", ErrorReporter.LastKeyReported);
				Assert(ErrorReporter.LastMessageReported.StartsWith("Duplicate AddressNotOnFile should not be saved."));
				AssertEquals("Saving Duplicate AddressNotOnFile", ErrorReporter.LastExceptionReported.Message);
			}

			ErrorReporter.Instance.Clear();
		}

		public void TestNoErrorReportWhenCreateDuplicateAddress_RegistryDisabled()
		{
			Org.OH_Code = "TEST001";
			Org.OH_FullName = "Test Organization";
			var newAddress1 = TestCollection.AddNew();
			newAddress1.Address1 = "NewAddressWithCode003";
			Factory.Save();
			var newAddress2 = TestCollection.AddNew();
			newAddress2.Address1 = "NewAddressWithCode002";

			using (OrganisationRegistry.Instance.EnableAddressNotOnFileLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TestCollection.AddNewMainAddress();
			}

			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		#region Implementation

		OrgHeader Org;
		OrgAddressDependentCollection TestCollection;

		protected override void SetUp()
		{
			base.SetUp();
			Org = Factory.New<OrgHeader>();
			TestCollection = Org.Addresses;
		}

		OrgAddress AddAddressToCollection(OrgAddressType type)
		{
			return AddAddressToCollection(type.Code);
		}

		OrgAddress AddAddressToCollection(ZString addressType)
		{
			OrgAddress address = TestCollection.AddNew();
			address.AddressCapability.SetCapabilityEnabled(addressType);
			return address;
		}

		OrgAddress AddAddressToNewCollection(OrgAddressDependentCollection newCollection, ZString addressType)
		{
			OrgAddress address = newCollection.AddNew();
			address.AddressCapability.SetCapabilityEnabled(addressType);
			return address;
		}

		#endregion
	}
}
