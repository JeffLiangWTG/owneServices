using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrganisationsFindBoxListProviderTest : TestCaseWithFactory
	{
		OrgHeader testOrg;
		OrganisationsFindBoxCollection orgList { get; set; }
		OrganisationsFindBoxListProvider provider;

		public void TestGetBusinessObjectFromCodeCore()
		{
			AssertGetBusinessObjectFromCode(provider.GetBusinessObjectFromCode);
		}

		public void TestBizObjFromCodeWithoutFilter()
		{
			AssertGetBusinessObjectFromCode(provider.GetBusinessObjectFromCodeWithoutFilter);
		}

		public void TestGetBusinessObjectFromCodeCoreForPluginParent()
		{
			AssertDefaultBizoFromUnmatchedNote(provider.GetBusinessObjectFromCode);
		}

		public void TestBizObjFromCodeWithoutFilterForPluginParent()
		{
			AssertDefaultBizoFromUnmatchedNote(provider.GetBusinessObjectFromCodeWithoutFilter);
		}

		void AssertDefaultBizoFromUnmatchedNote(Func<string, BusinessObject> baseMethod)
		{
			var pluginParent = new JobDocAddressCollectionForPlugin(new JobDocAddressCollection(Factory));
			pluginParent.HostParentBizo = Factory.New<ICommonShipment>() as IBusiness;

			var stmNote = Factory.NewWithValidTestData<StmNote>();
			stmNote.ST_ParentID = (pluginParent.HostParentBizo as BusinessObject).PK;
			stmNote.ST_Description = PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Code;
			stmNote.ST_Table = pluginParent.HostParentBizo.TableName;
			stmNote.ST_NoteText = @"<UnmatchOrgRecords><UnmatchOrgRecord><OrganisationType>Consignor</OrganisationType><OrganisationSubType>Consignor</OrganisationSubType><OwnerCode /><EDICode>CONSALVADALV</EDICode>
<OrganisationName>CONSIGNOR</OrganisationName><AddressLine1>TIAN</AddressLine1><AddressLine2 /><City>NANJING</City><PostCode>21000</PostCode><StateOrProvince>32</StateOrProvince><Country>CN</Country><DocAddressType /></UnmatchOrgRecord></UnmatchOrgRecords>";
			Factory.Save();

			orgList.OrganisationType = OrganisationTypes.Consignor;
			orgList.OrganisationSubType = ZString.Empty;
			orgList.DocAddressType = ZString.Empty;

			((IBusinessObjectCollection)orgList).Parent = pluginParent;
			((IBusinessObjectCollection)orgList).ListPropertyDescriptor = KPropertyDescriptorCollection.FromType(GetType(), true).Find(nameof(orgList), false);

			AssertEquals("precondition: ShouldSetValuesFromConditionalDefaults = false", false, ((IOrganisationDefaultProvider)orgList).ShouldSetValuesFromConditionalDefaults);
			var organisationReturned = (OrgHeader)baseMethod(OrgHeader.UnmatchedOrganisationCode);
			AssertNull("organisation returned is null", organisationReturned);
			AssertEquals("ShouldSetValuesFromConditionalDefaults is set to true", true, ((IOrganisationDefaultProvider)orgList).ShouldSetValuesFromConditionalDefaults);

			var orgFromUnmatched = Factory.New<OrgHeader>();
			orgList.SetupNewElementButDoNotAddIt(orgFromUnmatched, true);
			var mainAddress = orgFromUnmatched.MainAddress;

			AssertEquals("TIAN", mainAddress.Address1);
			AssertEquals("NANJING", mainAddress.City);
			AssertEquals("CN", mainAddress.Country.Code);
			AssertEquals("21000", mainAddress.Postcode);
			AssertEquals("32", mainAddress.OA_State);
		}

		void AssertGetBusinessObjectFromCode(Func<string, BusinessObject> baseMethod)
		{
			AssertNotNull("search code = testorg.OH_Code", baseMethod(testOrg.OH_Code));
			AssertEquals(testOrg.PK, baseMethod(testOrg.OH_Code).PK);

			AssertEquals("precondition: ShouldSetValuesFromConditionalDefaults = false", false, ((IOrganisationDefaultProvider)orgList).ShouldSetValuesFromConditionalDefaults);
			OrgHeader organisationReturned = (OrgHeader)baseMethod(OrgHeader.UnmatchedOrganisationCode);
			AssertNotNull("search code = 'UnMatchedOrganisationCode'", organisationReturned);
			AssertEquals("it will return the unmatchedorganisation", Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation, organisationReturned.PK);

			orgList.DefaultsForNewChild.Add(new OrgFieldDefault() { IsConditional = true, FieldName = OrgAddress.Schema.OA_Address1, Value = new ZString("aaa") });
			organisationReturned = (OrgHeader)baseMethod(OrgHeader.UnmatchedOrganisationCode);
			AssertNull("organisation returned is null", organisationReturned);
			AssertEquals("ShouldSetValuesFromConditionalDefaults is set to true", true, ((IOrganisationDefaultProvider)orgList).ShouldSetValuesFromConditionalDefaults);
		}

		protected override void SetUp()
		{
			base.SetUp();

			testOrg = Factory.LoadTop1<OrgHeader>(new ZQuery());
			orgList = new OrganisationsFindBoxCollection(Factory);
			provider = new OrganisationsFindBoxListProvider(orgList);
		}
	}
}
