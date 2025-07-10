using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.GUI.AddressesFilterBusinessObject;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AddressesFilterBusinessObject))]
	public class AddressesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Text Filters

		public void TestAddress1Filter()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "Address1";
			address1.OA_Address1 = "725 5th Avenue";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			var docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_Address1 = "Address1";
			docAddress.E2_AddressOverride = true;
			docAddress1.E2_Address1 = "725 5th Avenue";
			docAddress1.E2_AddressOverride = true;
			docAddress2.E2_Address1 = "Address1";
			docAddress2.E2_AddressOverride = false;
			docAddress1.E2_AddressOverride = false;

			Factory.Save();

			var filter = new AddressesFilterBusinessObject();
			((ModuleTextFilter)filter["Address1"]).Property = "Address1";
			((ModuleTextFilter)filter["Address1"]).IsActive = true;

			// Act.

			var collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
			collection.Load();

			// Assert.

			AssertCollectionContains(address.PK, collection.GetPKs());
			AssertCollectionContains(docAddress.PK, collection.GetPKs());
			AssertCollectionNotContains(address1.PK, collection.GetPKs());
			AssertCollectionNotContains(docAddress1.PK, collection.GetPKs());
			AssertCollectionNotContains(docAddress2.PK, collection.GetPKs());
		}

		public void TestAddress2Filter()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address2 = "Address2";
			address1.OA_Address2 = "725 5th Avenue";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			var docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_Address2 = "Address2";
			docAddress.E2_AddressOverride = true;
			docAddress1.E2_Address2 = "725 5th Avenue";
			docAddress1.E2_AddressOverride = true;
			docAddress2.E2_Address2 = "Address2";
			docAddress2.E2_AddressOverride = false;
			docAddress1.E2_AddressOverride = false;

			Factory.Save();

			var filter = new AddressesFilterBusinessObject();
			((ModuleTextFilter)filter["Address2"]).Property = "Address2";
			((ModuleTextFilter)filter["Address2"]).IsActive = true;

			// Act.

			var collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
			collection.Load();

			// Assert.

			AssertCollectionContains(address.PK, collection.GetPKs());
			AssertCollectionContains(docAddress.PK, collection.GetPKs());
			AssertCollectionNotContains(address1.PK, collection.GetPKs());
			AssertCollectionNotContains(docAddress1.PK, collection.GetPKs());
			AssertCollectionNotContains(docAddress2.PK, collection.GetPKs());
		}

		public void TestAdditionalAddressFilter()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address.PrimaryOrgAddressAdditionalInfoDetail = "AdditionalAddress";
			address1.PrimaryOrgAddressAdditionalInfoDetail = "AdditionalAddress1";

			Factory.Save();

			var filter = new AddressesFilterBusinessObject();
			((ModuleTextFilter)filter["Additional Address Info"]).Property = "AdditionalAddress1";
			((ModuleTextFilter)filter["Additional Address Info"]).IsActive = true;

			// Act.

			var collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
			collection.Load();

			// Assert.

			AssertCollectionNotContains(address.PK, collection.GetPKs());
			AssertCollectionContains(address1.PK, collection.GetPKs());
		}

		public void TestAddressCodeFilter()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "ABC";
			address1.OA_Code = "DEF";

			Factory.Save();

			var filter = new AddressesFilterBusinessObject();
			((ModuleTextFilter)filter["Address Code"]).Property = "ABC";
			((ModuleTextFilter)filter["Address Code"]).IsActive = true;

			// Act.

			var collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
			collection.Load();

			// Assert.

			AssertCollectionContains(address.PK, collection.GetPKs());
			AssertCollectionNotContains(address1.PK, collection.GetPKs());
		}

		public void TestCityFilter()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_City = "Sydney";
			address1.OA_City = "Melbourne";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			var docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_City = "Sydney";
			docAddress.E2_AddressOverride = true;
			docAddress1.E2_City = "Melbourne";
			docAddress1.E2_AddressOverride = true;
			docAddress2.E2_City = "Sydney";
			docAddress2.E2_AddressOverride = false;
			docAddress1.E2_AddressOverride = false;

			Factory.Save();

			var filter = new AddressesFilterBusinessObject();
			((ModuleTextFilter)filter["City"]).Property = "Sydney";
			((ModuleTextFilter)filter["City"]).IsActive = true;

			// Act.

			var collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
			collection.Load();

			// Assert.

			AssertCollectionContains(address.PK, collection.GetPKs());
			AssertCollectionContains(docAddress.PK, collection.GetPKs());
			AssertCollectionNotContains(address1.PK, collection.GetPKs());
			AssertCollectionNotContains(docAddress1.PK, collection.GetPKs());
			AssertCollectionNotContains(docAddress2.PK, collection.GetPKs());
		}

		public void TestCompanyNameFilter()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_CompanyNameOverride = "Express";
			address1.OA_CompanyNameOverride = "DHL";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			var docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_CompanyName = "Express";
			docAddress.E2_AddressOverride = true;
			docAddress1.E2_CompanyName = "DHL";
			docAddress1.E2_AddressOverride = true;
			docAddress2.E2_CompanyName = "Express";
			docAddress2.E2_AddressOverride = false;
			docAddress1.E2_AddressOverride = false;

			Factory.Save();

			var filter = new AddressesFilterBusinessObject();
			((ModuleTextFilter)filter["Company Name"]).Property = "Express";
			((ModuleTextFilter)filter["Company Name"]).IsActive = true;

			// Act.

			var collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
			collection.Load();

			// Assert.

			AssertCollectionContains(address.PK, collection.GetPKs());
			AssertCollectionContains(docAddress.PK, collection.GetPKs());
			AssertCollectionNotContains(address1.PK, collection.GetPKs());
			AssertCollectionNotContains(docAddress1.PK, collection.GetPKs());
			AssertCollectionNotContains(docAddress2.PK, collection.GetPKs());
		}

		public void TestLanguageFilter()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Language = Constants.Languages.French;

			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.OA_Language = Constants.Languages.German;

			Factory.Save();

			var filter = new AddressesFilterBusinessObject();
			((ModuleTextFilter)filter["Language"]).Property = Constants.Languages.French;
			((ModuleTextFilter)filter["Language"]).IsActive = true;

			// Act.

			var collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
			collection.Load();

			// Assert.

			AssertCollectionContains(address.PK, collection.GetPKs());
			AssertCollectionNotContains(address1.PK, collection.GetPKs());
		}

		public void TestPostCodeFilter()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_PostCode = "1234";
			address1.OA_PostCode = "4321";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			var docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_Postcode = "1234";
			docAddress.E2_AddressOverride = true;
			docAddress1.E2_Postcode = "4321";
			docAddress1.E2_AddressOverride = true;
			docAddress2.E2_Postcode = "1234";
			docAddress2.E2_AddressOverride = false;
			docAddress1.E2_AddressOverride = false;

			Factory.Save();

			var filter = new AddressesFilterBusinessObject();
			((ModuleTextFilter)filter["Post Code"]).Property = "1234";
			((ModuleTextFilter)filter["Post Code"]).IsActive = true;

			// Act.

			var collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
			collection.Load();

			// Assert.

			AssertCollectionContains(address.PK, collection.GetPKs());
			AssertCollectionContains(docAddress.PK, collection.GetPKs());
			AssertCollectionNotContains(address1.PK, collection.GetPKs());
			AssertCollectionNotContains(docAddress1.PK, collection.GetPKs());
			AssertCollectionNotContains(docAddress2.PK, collection.GetPKs());
		}

		public void TestStateFilter()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_State = "New South Wales";
			address1.OA_State = "Victoria";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			var docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_State = "New South Wales";
			docAddress.E2_AddressOverride = true;
			docAddress1.E2_State = "Victoria";
			docAddress1.E2_AddressOverride = true;
			docAddress2.E2_State = "New South Wales";
			docAddress2.E2_AddressOverride = false;
			docAddress1.E2_AddressOverride = false;

			Factory.Save();

			var filter = new AddressesFilterBusinessObject();
			((ModuleTextFilter)filter["State"]).Property = "New South Wales";
			((ModuleTextFilter)filter["State"]).IsActive = true;

			// Act.

			var collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
			collection.Load();

			// Assert.

			AssertCollectionContains(address.PK, collection.GetPKs());
			AssertCollectionContains(docAddress.PK, collection.GetPKs());
			AssertCollectionNotContains(address1.PK, collection.GetPKs());
			AssertCollectionNotContains(docAddress1.PK, collection.GetPKs());
			AssertCollectionNotContains(docAddress2.PK, collection.GetPKs());
		}

		public void TestCountryFilter()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "CN";
			address1.OA_RN_NKCountryCode = "US";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			var docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_RN_NKCountryCode = "CN";
			docAddress.E2_AddressOverride = true;
			docAddress1.E2_RN_NKCountryCode = "US";
			docAddress1.E2_AddressOverride = true;
			docAddress2.E2_RN_NKCountryCode = "CN";
			docAddress2.E2_AddressOverride = false;
			docAddress1.E2_AddressOverride = false;

			Factory.Save();

			var filter = new AddressesFilterBusinessObject();
			((ModuleNkFilter)filter["Country"]).Property = "CN";
			((ModuleNkFilter)filter["Country"]).IsActive = true;

			// Act.

			var collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
			collection.Load();

			// Assert.

			AssertCollectionContains(address.PK, collection.GetPKs());
			AssertCollectionContains(docAddress.PK, collection.GetPKs());
			AssertCollectionNotContains(address1.PK, collection.GetPKs());
			AssertCollectionNotContains(docAddress1.PK, collection.GetPKs());
			AssertCollectionNotContains(docAddress2.PK, collection.GetPKs());
		}

		public void TestValidationStatusFilter()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_ValidationStatus = AddressValidationStatus.Verified;
			string uniqStr = Guid.NewGuid().ToString();
			address.OA_Address2 = uniqStr;
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.OA_ValidationStatus = AddressValidationStatus.Invalid;
			address1.OA_Address2 = uniqStr;

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;
			docAddress.E2_Address2 = uniqStr;

			Factory.Save();

			var filter = new AddressesFilterBusinessObject()[AddressFilterConstants.ValidationStatus] as ModuleTextFilter;
			filter.Property = AddressValidationStatus.Verified;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;

			var query = filter.Query.AddToFilter(MDMAdminPanelAddressViewSchema.MDM_Address2, SQLComparisonOperator.Equal, uniqStr);

			// Act.

			var collection = new MDMAdminPanelAddressCollection(Factory, query);
			collection.Load();

			// Assert.

			var pKs = collection.GetPKs();
			collection.RemoveAll();
			AssertCollectionNotContains(address.PK, pKs);
			AssertCollectionContains(docAddress.PK, pKs);
			AssertCollectionContains(address1.PK, pKs);
		}

		public void TestAddressTypeFilter()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			string uniqStr = Guid.NewGuid().ToString();
			address.OA_Address2 = uniqStr;

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_AddressType = DocAddressTypes.Codes.Carrier;
			docAddress.E2_Address2 = uniqStr;
			docAddress.E2_AddressOverride = true;

			var docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress1.E2_AddressType = DocAddressTypes.Codes.BuyingParty;
			docAddress1.E2_AddressOverride = true;
			docAddress1.E2_Address2 = uniqStr;

			Factory.Save();

			var filter = new AddressesFilterBusinessObject()[AddressFilterConstants.AddressType] as ModuleTextFilter;
			filter.Property = DocAddressTypes.Codes.Carrier;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var query = filter.Query.AddToFilter(MDMAdminPanelAddressViewSchema.MDM_Address2, SQLComparisonOperator.Equal, uniqStr);

			// Act.

			var collection = new MDMAdminPanelAddressCollection(Factory, query);
			collection.Load();

			// Assert.

			var pKs = collection.GetPKs();
			collection.RemoveAll();
			AssertCollectionNotContains(address.PK, pKs);
			AssertCollectionContains(docAddress.PK, pKs);
			AssertCollectionNotContains(docAddress1.PK, pKs);
		}

		public void TestOrgMainCountryFilter()
		{
			// Arrange.
			var uniqStr = Guid.NewGuid().ToString();
			var org = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var address = org.Addresses.AddNew();
			var mainAddress = org.Addresses.MainAddress;
			mainAddress.OA_RN_NKCountryCode = "US";

			var usAddress = org.Addresses.AddNew();
			usAddress.OA_RN_NKCountryCode = "US";

			address.OA_Address1 = mainAddress.OA_Address1 = usAddress.OA_Address1 = "Somewhere over the rainbow";
			address.OA_Address2 = mainAddress.OA_Address2 = usAddress.OA_Address2 = uniqStr;

			Factory.Save();

			var filter = new AddressesFilterBusinessObject()[AddressFilterConstants.OrganisationMainCountry] as ModuleNkFilter;
			filter.Property = "US";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var query = filter.Query.AddToFilter(MDMAdminPanelAddressViewSchema.MDM_Address2, SQLComparisonOperator.Equal, uniqStr);

			// Act.

			var collection = new MDMAdminPanelAddressCollection(Factory, query);
			collection.Load();

			// Assert.

			var pKs = collection.GetPKs();
			collection.RemoveAll();
			AssertCollectionContains(mainAddress.PK, pKs);
			AssertCollectionNotContains(address.PK, pKs);
			AssertCollectionNotContains(usAddress.PK, pKs);
		}
		public void TestOrgCodeFilter()
		{
			// Arrange.
			var uniqStr = Guid.NewGuid().ToString();
			var org1 = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			org1.OH_Code = "K39UVL8";
			org1.MainAddress.OA_Address1 = "Street";

			var org2 = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			org2.MainAddress.OA_Address1 = "Street";
			org2.OH_Code = "K39UVL9";
			org1.MainAddress.OA_Address2 = org2.MainAddress.OA_Address2 = uniqStr;

			Factory.Save();

			var filter = new AddressesFilterBusinessObject()[AddressFilterConstants.OrganisationCode] as ModuleTextFilter;
			filter.Property = "K39UVL8";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var query = filter.Query.AddToFilter(MDMAdminPanelAddressViewSchema.MDM_Address2, SQLComparisonOperator.Equal, uniqStr);

			// Act.

			var collection = new MDMAdminPanelAddressCollection(Factory, query);
			collection.Load();

			// Assert.

			var pKs = collection.GetPKs();
			collection.RemoveAll();
			AssertCollectionContains(org1.MainAddress.PK, pKs);
			AssertCollectionNotContains(org2.MainAddress.PK, pKs);
		}

		public void TestOrgNameFilter()
		{
			// Arrange.
			var uniqStr = Guid.NewGuid().ToString();
			var org1 = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			org1.OH_FullName = "K39UVL8";
			org1.MainAddress.OA_Address1 = "Street";

			var org2 = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			org2.MainAddress.OA_Address1 = "Street";
			org2.OH_FullName = "K39UVL9";
			org1.MainAddress.OA_Address2 = org2.MainAddress.OA_Address2 = uniqStr;

			Factory.Save();

			var filter = new AddressesFilterBusinessObject()[AddressFilterConstants.OrganisationName] as ModuleTextFilter;
			filter.Property = "K39UVL8";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var query = filter.Query.AddToFilter(MDMAdminPanelAddressViewSchema.MDM_Address2, SQLComparisonOperator.Equal, uniqStr);

			// Act.

			var collection = new MDMAdminPanelAddressCollection(Factory, query);
			collection.Load();

			// Assert.

			var pKs = collection.GetPKs();
			collection.RemoveAll();
			AssertCollectionContains(org1.MainAddress.PK, pKs);
			AssertCollectionNotContains(org2.MainAddress.PK, pKs);
		}

		public void TestAddressActiveStatusFilter()
		{
			// Arrange.
			var uniqStr = Guid.NewGuid().ToString();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_IsActive = false;
			address.OA_Address1 = uniqStr;
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.OA_IsActive = true;
			address1.OA_Address1 = uniqStr;
			Factory.Save();

			var filter = new AddressesFilterBusinessObject();
			((ModuleTextFilter)filter[AddressFilterConstants.AddressActiveStatus]).IsActive = true;

			((ModuleTextFilter)filter[AddressFilterConstants.Address1]).Property = uniqStr;
			((ModuleTextFilter)filter[AddressFilterConstants.Address1]).IsActive = true;

			AllLanguages.ForEach(lan =>
			{
				using (Res.TemporarilySwitchLanguage(lan))
				{
					((ModuleTextFilter)filter[AddressFilterConstants.AddressActiveStatus]).Property = FilterStripBusinessObject.StatusActive;

					// Act.

					var collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
					collection.Load();

					// Assert.

					AssertCollectionNotContains(address.PK, collection.GetPKs());
					AssertCollectionContains(address1.PK, collection.GetPKs());

					// Arrange.

					((ModuleTextFilter)filter[AddressFilterConstants.AddressActiveStatus]).Property = FilterStripBusinessObject.StatusInactive;

					// Act.

					collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
					collection.Load();

					// Assert.

					AssertCollectionContains(address.PK, collection.GetPKs());
					AssertCollectionNotContains(address1.PK, collection.GetPKs());

					// Arrange.

					((ModuleTextFilter)filter[AddressFilterConstants.AddressActiveStatus]).Property = FilterStripBusinessObject.StatusAll;

					// Act.

					collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
					collection.Load();

					// Assert.

					AssertCollectionContains(address.PK, collection.GetPKs());
					AssertCollectionContains(address1.PK, collection.GetPKs());
				}
			});
		}

		public void TestOrganisationActiveStatusFilter()
		{
			var languages = typeof(SharedConstants.Languages).GetAllPublicConstantValues();
			languages.ForEach(TestOrganisationActiveStatusFilterWithMultilingualActiveStatus);
		}

		void TestOrganisationActiveStatusFilterWithMultilingualActiveStatus(string language)
		{
			using (Res.TemporarilySwitchLanguage(language))
			{
				// Arrange.

				var uniqStr = Guid.NewGuid().ToString();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
				orgHeader.OH_IsActive = false;
				orgHeader.MainAddress.OA_Address1 = uniqStr;
				orgHeader.MainAddress.OA_IsActive = true;
				var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
				orgHeader1.OH_IsActive = true;
				orgHeader1.MainAddress.OA_Address1 = uniqStr;
				orgHeader1.MainAddress.OA_IsActive = true;
				Factory.Save();

				var filter = new AddressesFilterBusinessObject();
				((ModuleTextFilter)filter[AddressFilterConstants.OrganisationActiveStatus]).Property = FilterStripBusinessObject.StatusActive;
				((ModuleTextFilter)filter[AddressFilterConstants.OrganisationActiveStatus]).IsActive = true;

				((ModuleTextFilter)filter[AddressFilterConstants.Address1]).Property = uniqStr;
				((ModuleTextFilter)filter[AddressFilterConstants.Address1]).IsActive = true;

				// Act.

				var collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
				collection.Load();

				// Assert.

				AssertCollectionNotContains(orgHeader.MainAddress.PK, collection.GetPKs());
				AssertCollectionContains(orgHeader1.MainAddress.PK, collection.GetPKs());

				// Arrange.

				((ModuleTextFilter)filter[AddressFilterConstants.OrganisationActiveStatus]).Property = FilterStripBusinessObject.StatusInactive;
				((ModuleTextFilter)filter[AddressFilterConstants.OrganisationActiveStatus]).IsActive = true;

				// Act.

				collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
				collection.Load();

				// Assert.

				AssertCollectionContains(orgHeader.MainAddress.PK, collection.GetPKs());
				AssertCollectionNotContains(orgHeader1.MainAddress.PK, collection.GetPKs());

				// Arrange.

				((ModuleTextFilter)filter[AddressFilterConstants.OrganisationActiveStatus]).Property = FilterStripBusinessObject.StatusAll;
				((ModuleTextFilter)filter[AddressFilterConstants.OrganisationActiveStatus]).IsActive = true;

				// Act.

				collection = new MDMAdminPanelAddressCollection(Factory, filter.Filter);
				collection.Load();

				// Assert.

				AssertCollectionContains(orgHeader.MainAddress.PK, collection.GetPKs());
				AssertCollectionContains(orgHeader1.MainAddress.PK, collection.GetPKs());
			}
		}

		#endregion

			#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AddressesFilterBusinessObject();
		}

		#endregion
	}
}
