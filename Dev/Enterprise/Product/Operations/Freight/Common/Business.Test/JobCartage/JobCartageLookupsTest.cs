using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Common.Business.Testing
{
	sealed class JobCartageLookupsTest : BusinessObjectLookupsTestCase
	{
		#region Unmatched Notes

		#region TestUnmatchedOrgNotes_Consignee

		public void TestUnmatchedOrgNotes_Consignee()
		{
			Factory.New<OrgHeader>().OH_IsConsignee = true;
			AssertUnmatchedOrgNotesDefaults(OrganisationTypes.Consignee, "", "ConsigneeList", typeof(ConsigneeCollection), cartage => cartage.Lookups.ConsigneeList, cartage => cartage.Lookups.ConsignorList);
		}

		#endregion

		#region TestUnmatchedOrgNotes_Consignor

		public void TestUnmatchedOrgNotes_Consignor()
		{
			Factory.New<OrgHeader>().OH_IsConsignor = true;
			AssertUnmatchedOrgNotesDefaults(OrganisationTypes.Consignor, "", "ConsignorList", typeof(ConsignorCollection), cartage => cartage.Lookups.ConsignorList, cartage => cartage.Lookups.ConsigneeList);
		}

		#endregion

		#region TestUnmatchedOrgNotes_CFS

		public void TestUnmatchedOrgNotes_CFS()
		{
			Factory.New<OrgHeader>().OH_IsMiscFreightServices = true;
			AssertUnmatchedOrgNotesDefaults(OrganisationTypes.Services, OrganisationsSubTypeList.Codes.ContainerFreightStation, "DepotList", typeof(DepotCollection), cartage => cartage.Lookups.DepotList, cartage => cartage.Lookups.CTOList);
		}

		#endregion

		#region TestUnmatchedOrgNotes_CTO

		public void TestUnmatchedOrgNotes_CTO()
		{
			Factory.New<OrgHeader>().OH_IsMiscFreightServices = true;
			AssertUnmatchedOrgNotesDefaults(OrganisationTypes.Services, OrganisationsSubTypeList.Codes.ContainerTerminalOperator, "CTOList", typeof(CTOCollection), cartage => cartage.Lookups.CTOList, cartage => cartage.Lookups.ContainerYardList);
		}

		#endregion

		#region TestUnmatchedOrgNotes_ContainerYard

		public void TestUnmatchedOrgNotes_ContainerYard()
		{
			Factory.New<OrgHeader>().OH_IsMiscFreightServices = true;
			AssertUnmatchedOrgNotesDefaults(OrganisationTypes.Services, OrganisationsSubTypeList.Codes.ContainerYard, "ContainerYardList", typeof(ContainerYardCollection), cartage => cartage.Lookups.ContainerYardList, cartage => cartage.Lookups.CTOList);
		}

		#endregion

		#endregion

		#region Implementation

		delegate OrganisationsFindBoxCollection GetLookupDelegate(CartageForTest cartage);

		void AssertUnmatchedOrgNotesDefaults(OrganisationTypes organisationTypes, string organisationSubType, string collectionName, Type lookupType, GetLookupDelegate getLookupsCollection, GetLookupDelegate getOtherLookupsCollection)
		{
			var cartage = Factory.New<CartageForTest>();
			var unmatchedOrgRecord = GetUnmatchedRecord(organisationTypes, string.IsNullOrEmpty(organisationSubType) ? organisationTypes.ToString() : organisationSubType);
			var helper = new UnmatchOrgRecordTestHelper(cartage, unmatchedOrgRecord);

			var lookupsCollection = getLookupsCollection(cartage);
			AssertNotNull(lookupsCollection);
			AssertEquals(lookupType, lookupsCollection.GetType());
			AssertEquals("Collection should not be loaded.", 0, lookupsCollection.Count);

			helper.PopulateOrgDefaultsFromUnmatchedNote(cartage.Lookups.GetType(), collectionName, lookupsCollection);
			AssertEquals("Collection should have defaults from unmatchorgnotes", true, lookupsCollection.DefaultsForNewChild.Count > 0);
			var defaultfromNote = lookupsCollection.DefaultsForNewChild.GetDefaultByFieldName(OrgHeader.Schema.OH_FullName);
			AssertNotNull("name default shouldn't be null", defaultfromNote);
			AssertEquals("name", "Name", defaultfromNote.Value);

			defaultfromNote = lookupsCollection.DefaultsForNewChild.GetDefaultByFieldName(OrgAddress.Schema.OA_Address1);
			AssertNotNull("address line 1 default shouldn't be null", defaultfromNote);
			AssertEquals("address line 1", "addr 1", defaultfromNote.Value);

			defaultfromNote = lookupsCollection.DefaultsForNewChild.GetDefaultByFieldName(OrgAddress.Schema.OA_Address2);
			AssertNotNull("address line 2 default shouldn't be null", defaultfromNote);
			AssertEquals("address line 2", "addr 2", defaultfromNote.Value);

			defaultfromNote = lookupsCollection.DefaultsForNewChild.GetDefaultByFieldName(OrgAddress.Schema.OA_PostCode);
			AssertNotNull("post code default shouldn't be null", defaultfromNote);
			AssertEquals("post code", "2222", defaultfromNote.Value);

			defaultfromNote = lookupsCollection.DefaultsForNewChild.GetDefaultByFieldName(OrgAddress.Schema.OA_City);
			AssertNotNull("City default shouldn't be null", defaultfromNote);
			AssertEquals("city", "sydney", defaultfromNote.Value);

			defaultfromNote = lookupsCollection.DefaultsForNewChild.GetDefaultByFieldName(OrgAddress.Schema.OA_State);
			AssertNotNull("state default shouldn't be null", defaultfromNote);
			AssertEquals("state", "NSW", defaultfromNote.Value);

			var otherLookups = getOtherLookupsCollection(cartage);
			AssertNull(otherLookups.DefaultsForNewChild.GetDefaultByFieldName(OrgHeader.Schema.OH_FullName));
			AssertNull(otherLookups.DefaultsForNewChild.GetDefaultByFieldName(OrgAddress.Schema.OA_Address1));
			AssertNull(otherLookups.DefaultsForNewChild.GetDefaultByFieldName(OrgAddress.Schema.OA_Address2));
			AssertNull(otherLookups.DefaultsForNewChild.GetDefaultByFieldName(OrgAddress.Schema.OA_PostCode));
			AssertNull(otherLookups.DefaultsForNewChild.GetDefaultByFieldName(OrgAddress.Schema.OA_City));
			AssertNull(otherLookups.DefaultsForNewChild.GetDefaultByFieldName(OrgAddress.Schema.OA_State));
		}

		UnmatchOrgRecord GetUnmatchedRecord(OrganisationTypes organisationType, string organisationSubType)
		{
			var unmatchedRecord = new UnmatchOrgRecord()
			{
				OrganisationType = organisationType.ToString(),
				OrganisationSubType = organisationSubType,
				AddressLine1 = "addr 1",
				AddressLine2 = "addr 2",
				OrganisationName = "Name",
				PostCode = "2222",
				StateOrProvince = "NSW",
				City = "sydney",
				EDICode = "EDIcode",
				OwnerCode = "OwnerCode"
			};

			return unmatchedRecord;
		}

		class CartageForTest : AutoJobCartage
		{
			public CartageForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		#endregion
	}
}
