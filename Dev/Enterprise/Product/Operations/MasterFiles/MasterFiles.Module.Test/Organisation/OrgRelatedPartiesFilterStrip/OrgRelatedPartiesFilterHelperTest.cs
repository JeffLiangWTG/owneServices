using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ModuleTextFilter))]
	sealed class OrgRelatedPartiesFilterHelperTest : ModuleTextFilterTest
	{
		public void TestGetDocAddressFromOrgAddressFilter()
		{
			ZString expectedOrgAddressIn = "WHERE E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_Code = 'AddressCode')";
			ZQuery query = OrgRelatedPartiesFilterHelper.GetDocAddressFromOrgAddressFilter(OrgAddressFilter, false);
			AssertEquals("OrgAddress In", expectedOrgAddressIn, query.GetAsWhereClause(true).Trim());

			ZString expectedOrgAddressNotIn = "WHERE E2_OA_Address NOT IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_Code = 'AddressCode')";
			query = OrgRelatedPartiesFilterHelper.GetDocAddressFromOrgAddressFilter(OrgAddressFilter, true);
			AssertEquals("OrgAddress NOT In", expectedOrgAddressNotIn, query.GetAsWhereClause(true).Trim());
		}

		public void TestGetDocAddressFromOrgHeaderFilter()
		{
			ZString expectedOrgHeaderInOrgAddressIn = "WHERE E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH IN (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'CompanyCode') and OA_Code = 'AddressCode')";
			ZQuery query = OrgRelatedPartiesFilterHelper.GetDocAddressFromOrgHeaderFilter(OrgHeaderFilter, OrgAddressFilter, false, false);
			AssertEquals("OrgHeader In OrgAddress In", expectedOrgHeaderInOrgAddressIn, query.GetAsWhereClause(true).Trim());

			ZString expectedOrgHeaderInOrgAddressNotIn = "WHERE E2_OA_Address NOT IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH IN (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'CompanyCode') and OA_Code = 'AddressCode')";
			query = OrgRelatedPartiesFilterHelper.GetDocAddressFromOrgHeaderFilter(OrgHeaderFilter, OrgAddressFilter, false, true);
			AssertEquals("OrgHeader In OrgAddress NOT In", expectedOrgHeaderInOrgAddressNotIn, query.GetAsWhereClause(true).Trim());

			ZString expectedOrgHeaderNotInOrgAddressIn = "WHERE E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH NOT IN (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'CompanyCode') and OA_Code = 'AddressCode')";
			query = OrgRelatedPartiesFilterHelper.GetDocAddressFromOrgHeaderFilter(OrgHeaderFilter, OrgAddressFilter, true, false);
			AssertEquals("OrgHeader NOT In OrgAddress In", expectedOrgHeaderNotInOrgAddressIn, query.GetAsWhereClause(true).Trim());

			ZString expectedOrgHeaderNotInOrgAddressNotIn = "WHERE E2_OA_Address NOT IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH NOT IN (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'CompanyCode') and OA_Code = 'AddressCode')";
			query = OrgRelatedPartiesFilterHelper.GetDocAddressFromOrgHeaderFilter(OrgHeaderFilter, OrgAddressFilter, true, true);
			AssertEquals("OrgHeader NOT In OrgAddress NOT In", expectedOrgHeaderNotInOrgAddressNotIn, query.GetAsWhereClause(true).Trim());
		}

		public void TestGetJobHeaderFromOrgAddressFilter()
		{
			ZString expectedOrgAddressIn = "WHERE JH_OA_LocalChargesAddr IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_Code = 'AddressCode')";
			ZQuery query = OrgRelatedPartiesFilterHelper.GetJobHeaderFromOrgAddressFilter(OrgAddressFilter, false);
			AssertEquals("OrgAddress In", expectedOrgAddressIn, query.GetAsWhereClause(true).Trim());

			ZString expectedOrgAddressNotIn = "WHERE JH_OA_LocalChargesAddr NOT IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_Code = 'AddressCode')";
			query = OrgRelatedPartiesFilterHelper.GetJobHeaderFromOrgAddressFilter(OrgAddressFilter, true);
			AssertEquals("OrgAddress NOT In", expectedOrgAddressNotIn, query.GetAsWhereClause(true).Trim());
		}

		public void TestGetJobHeaderFromOrgHeaderFilter()
		{
			ZString expectedOrgHeaderInOrgAddressIn = "WHERE JH_OA_LocalChargesAddr IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH IN (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'CompanyCode') and OA_Code = 'AddressCode')";
			ZQuery query = OrgRelatedPartiesFilterHelper.GetJobHeaderFromOrgHeaderFilter(OrgHeaderFilter, OrgAddressFilter, false, false);
			AssertEquals("OrgHeader In OrgAddress In", expectedOrgHeaderInOrgAddressIn, query.GetAsWhereClause(true).Trim());

			ZString expectedOrgHeaderInOrgAddressNotIn = "WHERE JH_OA_LocalChargesAddr NOT IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH IN (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'CompanyCode') and OA_Code = 'AddressCode')";
			query = OrgRelatedPartiesFilterHelper.GetJobHeaderFromOrgHeaderFilter(OrgHeaderFilter, OrgAddressFilter, false, true);
			AssertEquals("OrgHeader In OrgAddress NOT In", expectedOrgHeaderInOrgAddressNotIn, query.GetAsWhereClause(true).Trim());

			ZString expectedOrgHeaderNotInOrgAddressIn = "WHERE JH_OA_LocalChargesAddr IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH NOT IN (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'CompanyCode') and OA_Code = 'AddressCode')";
			query = OrgRelatedPartiesFilterHelper.GetJobHeaderFromOrgHeaderFilter(OrgHeaderFilter, OrgAddressFilter, true, false);
			AssertEquals("OrgHeader NOT In OrgAddress In", expectedOrgHeaderNotInOrgAddressIn, query.GetAsWhereClause(true).Trim());

			ZString expectedOrgHeaderNotInOrgAddressNotIn = "WHERE JH_OA_LocalChargesAddr NOT IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH NOT IN (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'CompanyCode') and OA_Code = 'AddressCode')";
			query = OrgRelatedPartiesFilterHelper.GetJobHeaderFromOrgHeaderFilter(OrgHeaderFilter, OrgAddressFilter, true, true);
			AssertEquals("OrgHeader NOT In OrgAddress NOT In", expectedOrgHeaderNotInOrgAddressNotIn, query.GetAsWhereClause(true).Trim());
		}

		public void TestGetOrgAddressFromOrgHeaderFilter()
		{
			ZString expectedOrgHeaderIn = "WHERE OA_OH IN (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'CompanyCode')";
			ZQuery query = OrgRelatedPartiesFilterHelper.GetOrgAddressFromOrgHeaderFilter(OrgHeaderFilter, false);
			AssertEquals("OrgHeader In", expectedOrgHeaderIn, query.GetAsWhereClause(true).Trim());

			ZString expectedOrgHeaderNotIn = "WHERE OA_OH NOT IN (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'CompanyCode')";
			query = OrgRelatedPartiesFilterHelper.GetOrgAddressFromOrgHeaderFilter(OrgHeaderFilter, true);
			AssertEquals("OrgHeader NOT In", expectedOrgHeaderNotIn, query.GetAsWhereClause(true).Trim());
		}

		#region Implementation

		ZQuery OrgHeaderFilter
		{
			get { return new ZQuery(OrgHeaderSchema.OH_Code, "CompanyCode"); }
		}

		ZQuery OrgAddressFilter
		{
			get { return new ZQuery(OrgAddressSchema.OA_Code, "AddressCode"); }
		}

		#endregion
	}
}
