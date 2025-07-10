using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.TransportCommon.Integration;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;
using WTG.RTUS.Interface;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(OrganisationRTUSOption))]
	class OrganisationRTUSOptionTest : RegistryBusinessObjectTemplateTestCase<OrganisationRTUSOption>
	{
		#region Properties and Validation

		public void TestOrganisation()
		{
			var bizO = new OrganisationRTUSOption();
			bizO.ValidateOrganisationPK();
			AssertEquals(ZGuid.Empty, bizO.OrganisationPK);
			AssertHasError(bizO.OrganisationPKInfo, "Please enter a value.");

			bizO.OrganisationPK = ZGuid.Invalid;
			AssertHasError(bizO.OrganisationPKInfo, "Please select a valid organization.");

			var organisation = Factory.New<OrgHeader>();
			bizO.OrganisationPK = organisation.PK;

			AssertEquals(organisation.PK, bizO.OrganisationPK);
			AssertNoErrors(bizO.OrganisationPKInfo);
		}

		public void TestCBACode()
		{
			var bizO = new OrganisationRTUSOption();

			bizO.ValidateCBACode();

			AssertEquals(string.Empty, bizO.CBACode);
			AssertHasError(bizO.CBACodeInfo, "Please enter a value.");

			bizO.CBACode = "XXX";
			AssertHasError(bizO.CBACodeInfo, "Enter a valid selection.");

			bizO.CBACode = CBAList.Codes.SaaS;
			AssertNoErrors(bizO.CBACodeInfo);
		}

		public void TestRTUSCBA()
		{
			var bizO = new OrganisationRTUSOption();
			AssertEquals("Precondition:", "", bizO.CBACode);

			RTUSCBA poke;
			AssertExceptionThrown(typeof(InvalidOperationException), "Invalid RTUSCBA ''.", () => poke = bizO.RTUSCBA);

			bizO.CBACode = CBAList.Codes.SaaS;
			AssertEquals(RTUSCBA.SaaSTransportation, bizO.RTUSCBA);

			bizO.CBACode = CBAList.Codes.SmartFreight;
			AssertEquals(RTUSCBA.SmartFreight, bizO.RTUSCBA);

			bizO.CBACode = CBAList.Codes.Teknowlogi;
			AssertEquals(RTUSCBA.Teknowlogi, bizO.RTUSCBA);

			bizO.CBACode = CBAList.Codes.TMS3G;
			AssertEquals(RTUSCBA.TMS3G, bizO.RTUSCBA);

			bizO.CBACode = CBAList.Codes.Transtream;
			AssertEquals(RTUSCBA.Pierbridge, bizO.RTUSCBA);

			bizO.CBACode = CBAList.Codes.Trinium;
			AssertEquals(RTUSCBA.Trinium, bizO.RTUSCBA);
		}

		#region TestUrl

		public void TestUrl_DefaultUrlForCBA()
		{
			var bizO = new OrganisationRTUSOption();
			AssertEquals("Precondition:", "", bizO.CBACode);
			AssertEquals("Precondition:", "", bizO.Url);

			bizO.CBACode = CBAList.Codes.SaaS;
			AssertEquals("This CBA is not yet supported", bizO.Url);

			bizO.CBACode = CBAList.Codes.Teknowlogi;
			AssertEquals("This CBA is not yet supported", bizO.Url);

			bizO.CBACode = CBAList.Codes.TMS3G;
			AssertEquals("This CBA is not yet supported", bizO.Url);

			bizO.CBACode = CBAList.Codes.Transtream;
			AssertEquals("", bizO.Url);

			bizO.CBACode = CBAList.Codes.SmartFreight;
			AssertEquals("", bizO.Url);

			bizO.CBACode = CBAList.Codes.Trinium;
			AssertEquals("This CBA is not yet supported", bizO.Url);
		}

		public void TestUrl_OnlyOverrideUrlIfDefault()
		{
			var bizO = new OrganisationRTUSOption();
			AssertEquals("Precondition:", "", bizO.CBACode);
			AssertEquals("Precondition:", "", bizO.Url);

			bizO.CBACode = CBAList.Codes.SaaS;
			AssertEquals("This CBA is not yet supported", bizO.Url);

			var userEnteredUrl = "https://testwtg1.p1.app.testing.com/DontOverride";
			bizO.Url = userEnteredUrl;
			bizO.CBACode = CBAList.Codes.SmartFreight;
			AssertEquals(userEnteredUrl, bizO.Url);

			var smartFreightUrl = "";
			bizO.Url = smartFreightUrl;
			bizO.CBACode = CBAList.Codes.SaaS;
			AssertEquals("This CBA is not yet supported", bizO.Url);

			bizO.CBACode = CBAList.Codes.SmartFreight;
			AssertEquals(smartFreightUrl, bizO.Url);
		}

		public void TestUrl_ValidateUrlIsWellFormedUri()
		{
			var urlError = "Please enter a valid URL.";
			var bizO = new OrganisationRTUSOption();
			AssertEquals("Precondition:", "", bizO.Url);
			AssertNoError(bizO.UrlInfo, urlError);

			bizO.Url = "sdjhfsdkfhsdfheio)(3";
			AssertHasError(bizO.UrlInfo, urlError);

			bizO.Url = "https://testwtg1.p1.app.testing.com/DontOverride";
			AssertNoError(bizO.UrlInfo, urlError);

			bizO.Url = string.Empty;
			AssertHasError(bizO.UrlInfo, urlError);

			bizO.Url = "https://wtg1.p1.app.smartfreight.com/smartfreight/api/ucp/sems/execute/N1ZVOmJ3VjNWcHJQNnVlMTlVUWhqXy1hQ05mdmZIT1ZYVXc3ZUE";
			AssertNoError(bizO.UrlInfo, urlError);

			bizO.Url = "This CBA is not yet supported";
			AssertHasError(bizO.UrlInfo, urlError);

			bizO.Url = "I really need RTUS to work.";
			AssertHasError(bizO.UrlInfo, urlError);
		}

		public void TestUrl_WrappedUrl()
		{
			var bizO = new OrganisationRTUSOption();
			AssertEquals("Precondition:", "", bizO.Url);

			Uri poke;
			AssertExceptionThrown<UriFormatException>(() => poke = bizO.WrappedUrl);

			bizO.Url = "https://wtg1.p1.app.smartfreight.com/smartfreight/api/ucp/sems/execute/N1ZVOmJ3VjNWcHJQNnVlMTlVUWhqXy1hQ05mdmZIT1ZYVXc3ZUE";
			AssertEquals("https://wtg1.p1.app.smartfreight.com/smartfreight/api/ucp/sems/execute/N1ZVOmJ3VjNWcHJQNnVlMTlVUWhqXy1hQ05mdmZIT1ZYVXc3ZUE", bizO.WrappedUrl);

			bizO.Url = "https://testwtg1.p1.app.testing.com/DontOverride";
			AssertEquals("https://testwtg1.p1.app.testing.com/DontOverride", bizO.WrappedUrl);

			bizO.Url = "This CBA is not yet supported";
			AssertExceptionThrown<UriFormatException>(() => poke = bizO.WrappedUrl);
		}

		#endregion

		#endregion

		#region NoDuplicates

		public void TestDuplicateOrganisationPKIsNotAllowed()
		{
			var org1Pk = "1EEBF4DF-FB6A-4696-A457-80C6E465B699";
			var org2Pk = "2EEBF4DF-FB6A-4696-A457-80C6E465B699";
			var org3Pk = "3EEBF4DF-FB6A-4696-A457-80C6E465B699";
			var org4Pk = "4EEBF4DF-FB6A-4696-A457-80C6E465B699";
			var collection = new OrganisationRTUSCollection();
			AddNew(collection, org1Pk, CBAList.Codes.SaaS);
			AddNew(collection, org2Pk, CBAList.Codes.TMS3G);
			AddNew(collection, org3Pk, CBAList.Codes.Teknowlogi);
			var optionDupOrg = AddNew(collection, org3Pk, CBAList.Codes.SmartFreight);

			AssertHasError(optionDupOrg.OrganisationPKInfo, "This organization has already been assigned to another CBA.");

			optionDupOrg.OrganisationPK = new ZGuid(org4Pk);
			AssertNoErrors(optionDupOrg.OrganisationPKInfo);
		}

		public void TestDuplicateRTUSIsAllowed()
		{
			var org1Pk = "1EEBF4DF-FB6A-4696-A457-80C6E465B699";
			var org2Pk = "2EEBF4DF-FB6A-4696-A457-80C6E465B699";
			var org3Pk = "3EEBF4DF-FB6A-4696-A457-80C6E465B699";
			var collection = new OrganisationRTUSCollection();
			AddNew(collection, org1Pk, CBAList.Codes.SaaS);
			AddNew(collection, org2Pk, CBAList.Codes.TMS3G);
			var optionDupRTUS = AddNew(collection, org3Pk, CBAList.Codes.SaaS);

			AssertNoErrors(optionDupRTUS.CBACodeInfo);
		}

		#endregion

		#region TestGetCBAList

		public void TestGetCBAList()
		{
			var bizO = new OrganisationRTUSOption();

			AssertNotNull(bizO.CBAList);
			AssertContainsExactElementsInAnyOrder(new CBAList(), bizO.CBAList);
		}

		#endregion

		#region TestOrganisations

		public void TestGetOrganisations()
		{
			var bizO = new OrganisationRTUSOption();

			AssertNotNull(bizO.Organisations);
			AssertType<OrgHeaderCollection>(bizO.Organisations);
		}

		#endregion

		#region TestGetOrganisationName

		public void TestGetOrganisationName()
		{
			var name = "Test Organisation RTUS company";
			var code = "ZXCZXC";
			var randomPK = "0B73F78C-1AA5-4EDA-893B-735B9D37D97C";

			var option = new OrganisationRTUSOption();
			var newOrg = Factory.New<OrgHeader>();
			newOrg.OH_FullName = name;
			newOrg.OH_Code = code;

			Factory.Save();

			option.OrganisationPK = newOrg.PK;
			AssertEquals(name, option.OrganisationName);

			option.OrganisationPK = ZGuid.Invalid;
			AssertEquals("", option.OrganisationName);

			option.OrganisationPK = ZGuid.Empty;
			AssertEquals("", option.OrganisationName);

			option.OrganisationPK = new ZGuid(randomPK);
			AssertEquals("", option.OrganisationName);
		}

		#endregion

		#region TestGetCBADescription

		public void TestGetCBADescription()
		{
			var bizO = new OrganisationRTUSOption();
			var codes = new CBAList().ToArray().Select(o => o.Code).ToArray();
			var descriptions = new CBAList().ToArray().Select(o => o.Description).ToArray();

			for (var i = 0; i < codes.Length; i++)
			{
				bizO.CBACode = codes[i];
				AssertEquals(descriptions[i], bizO.CBADescription);
			}
		}

		#endregion

		#region TestCanDelete

		public void TestCanDelete()
		{
			var bizO = new OrganisationRTUSOption();

			AssertEquals(true, bizO.CanDelete);
		}

		#endregion

		#region TestIOrganisationRTUSOption

		public void TestIOrganisationRTUSOption()
		{
			var rtusOption = new OrganisationRTUSOption
			{
				CBACode = CBAList.Codes.SmartFreight
			};

			IOrganisationRTUSOption iRTUSOption = rtusOption;
			AssertEquals(RTUSCBA.SmartFreight, iRTUSOption.RTUSCBA);

			rtusOption.Url = "https://Test";
			AssertEquals(new Uri("https://Test"), iRTUSOption.WrappedUrl);
		}

		#endregion

		public void TestRunPreSaveValidation_InvalidCbaCode()
		{
			var bizO = new OrganisationRTUSOption();
			AssertEquals("Precondition:", "", bizO.CBACode);
			AssertNoErrors(bizO.CBACodeInfo);

			bizO.RunPreSaveValidation();
			AssertHasError(bizO.CBACodeInfo, "Please enter a value.");
		}

		public void TestRunPreSaveValidation_InvalidOrganization()
		{
			var bizO = new OrganisationRTUSOption();
			AssertEquals(ZGuid.Empty, bizO.OrganisationPK);
			AssertNoErrors(bizO.OrganisationPKInfo);

			bizO.RunPreSaveValidation();
			AssertHasError(bizO.OrganisationPKInfo, "Please enter a value.");
		}

		public void TestRunPreSaveValidation_InvalidUrl()
		{
			var bizO = new OrganisationRTUSOption();
			AssertEquals("Precondition:", "", bizO.Url);
			AssertNoErrors(bizO.UrlInfo);

			bizO.RunPreSaveValidation();
			AssertHasError(bizO.UrlInfo, "Please enter a valid URL.");
		}

		#region Implementation

		OrganisationRTUSOption AddNew(OrganisationRTUSCollection collection, string organisationPK, string cbaCode)
		{
			var option = collection.AddNew();
			option.OrganisationPK = new ZGuid(organisationPK);
			option.CBACode = cbaCode;

			return option;
		}

		protected override OrganisationRTUSOption GetBusinessObjectToClone() => new OrganisationRTUSOption();

		protected override OrganisationRTUSOption GetBusinessObjectToSerialise() => new OrganisationRTUSOption();

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		#endregion
	}
}
