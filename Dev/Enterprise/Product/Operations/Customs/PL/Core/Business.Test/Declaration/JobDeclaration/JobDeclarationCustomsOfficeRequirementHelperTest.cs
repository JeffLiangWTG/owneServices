using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.PL;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(JobDeclarationCustomsOfficeRequirementHelper))]
class JobDeclarationCustomsOfficeRequirementHelperTest : EU.Business.Declaration.Testing.JobDeclarationCustomsOfficeRequirementHelperAbstractTest<JobDeclarationCustomsOfficeRequirementHelper>
{
	public void TestAdditionalOffice_Import()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertCustomsOfficeRequirementEquals("Import AdditionalOffice", new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, isMandatory: true, isLocalCountryOnly: false, "Customs Office of Entry"), officeHelper.AdditionalOffice);
	}

	public void TestAdditionalOffice_Export()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertCustomsOfficeRequirementEquals("Export AdditionalOffice", new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.ActualExitOffice, isMandatory: true, isLocalCountryOnly: false, "Office of Exit")
		{
			OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Codes.OfficeOfExitInland }
		}, officeHelper.AdditionalOffice);
	}

	public void TestAdditionalOffice_ExitSummary()
	{
		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		AssertCustomsOfficeRequirementEquals("Exit Summary AdditionalOffice", new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.ActualExitOffice, isMandatory: false, isLocalCountryOnly: false, "Office of Exit")
		{
			OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Codes.OfficeOfExitInland }
		}, officeHelper.AdditionalOffice);
	}

	public void TestGetOfficeCode()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		declaration.CustomsOffices.RemoveAndDeleteAll();
		Factory.Save();
		declaration.JE_CustomsOffice = "PL000001";

		var cusOffice1 = declaration.CustomsOffices.AddNew();
		cusOffice1.CY_Code = EuOfficeCodesTypes.Codes.SupervisingCustomsOffice;
		cusOffice1.CY_Data = "PL000002";

		CombineAssertions(() =>
		{
			AssertEquals("Office with role EXP", "PL000001", officeHelper.GetOfficeCode(EuOfficeCodesTypes.Codes.OfficeOfExport));
			AssertEquals("Office with role SCO", "PL000002", officeHelper.GetOfficeCode(EuOfficeCodesTypes.Codes.SupervisingCustomsOffice));
			AssertEquals("Office with role AEC is not added", ZString.Empty, officeHelper.GetOfficeCode(EuOfficeCodesTypes.Codes.ActualExitOffice));
			AssertEquals("Office with role PLA is not added", ZString.Empty, officeHelper.GetOfficeCode(EuOfficeCodesTypes.Codes.OfficeOfPresentation));
			AssertEquals("Office with role DES is empty", ZString.Empty, officeHelper.GetOfficeCode(EuOfficeCodesTypes.Codes.OfficeOfDestination));
		});
	}

	public override void TestMainOffice_Import()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertCustomsOfficeRequirementEquals("Import MainOffice", new CustomsOfficeRequirement(
			EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent
			, isMandatory: true
			, isLocalCountryOnly: true
			, "Decl.Customs Office")
		{
			OfficeRolesForLookup = new ZString[]
			{
				EuOfficeCodesTypes.Codes.AeoCompetentCustomsAuthorities,
				EuOfficeCodesTypes.Codes.AeoMainCustomsAuthorities, EuOfficeCodesTypes.Codes.AuthorityControlCode,
				EuOfficeCodesTypes.Codes.AuthorizingCustomsAuthorityForRegularShippingServices, EuOfficeCodesTypes.Codes.CentralOfficeCommonDomain,
				EuOfficeCodesTypes.Codes.CentralOfficeNationalDomain, EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented,
				EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep, EuOfficeCodesTypes.Codes.CompetentAuthorityOfDispatch,
				EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry, EuOfficeCodesTypes.Codes.CompetentAuthorityOfExport,
				EuOfficeCodesTypes.Codes.CompetentAuthorityOfRecovery, EuOfficeCodesTypes.Codes.CustomsOfficeForControllingEconomicalProcedure,
				EuOfficeCodesTypes.Codes.EoriRegistrationAuthorities, EuOfficeCodesTypes.Codes.Excise,
				EuOfficeCodesTypes.Codes.HigherAuthority, EuOfficeCodesTypes.Codes.OfficeForCentralizedClearance,
				EuOfficeCodesTypes.Codes.OfficeOfDelivery, EuOfficeCodesTypes.Codes.OfficeOfDeparture,
				EuOfficeCodesTypes.Codes.OfficeOfDestination, EuOfficeCodesTypes.Codes.OfficeOfDispatch,
				EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, EuOfficeCodesTypes.Codes.OfficeOfExit,
				EuOfficeCodesTypes.Codes.OfficeOfExitInland, EuOfficeCodesTypes.Codes.OfficeOfExport,
				EuOfficeCodesTypes.Codes.OfficeOfGuarantee, EuOfficeCodesTypes.Codes.OfficeOfLodgement,
				EuOfficeCodesTypes.Codes.OfficeOfLodgementEntry, EuOfficeCodesTypes.Codes.OfficeOfLodgementExit,
				EuOfficeCodesTypes.Codes.OfficeOfTransit, EuOfficeCodesTypes.Codes.ReleaseForFreeCirculation, EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice
			}
		}, officeHelper.MainOffice);
	}

	public override void TestMainOffice_Export()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertCustomsOfficeRequirementEquals("Export MainOffice", new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfExport, isMandatory: true, isLocalCountryOnly: true, "Office of Export"), officeHelper.MainOffice);
	}

	public void TestMainOffice_ExitSummary()
	{
		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		AssertCustomsOfficeRequirementEquals("Exit Summary MainOffice", new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfExport, isMandatory: false, isLocalCountryOnly: true, "Office of Lodgement"), officeHelper.MainOffice);
	}

	public override void TestMainOffice_Miscellaneous()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertCustomsOfficeRequirementEquals("MiscellaneousCustoms MainOffice", new CustomsOfficeRequirement(ZString.Empty, isMandatory: true, isLocalCountryOnly: false), officeHelper.MainOffice);
	}

	public override void TestOtherRequirements_Import()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertCustomsOfficeRequirementEquals("Other Requirements Import AuthorityControlCode", new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.AuthorityControlCode, isMandatory: false, isLocalCountryOnly: false), officeHelper.OtherRequirements.Single(x => x.OfficeRole == EuOfficeCodesTypes.Codes.AuthorityControlCode));
		AssertCustomsOfficeRequirementEquals("Other Requirements Import OfficeOfPresentation", new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfPresentation, isMandatory: false, isLocalCountryOnly: false)
		{
			OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented }
		}, officeHelper.OtherRequirements.Single(x => x.OfficeRole == EuOfficeCodesTypes.Codes.OfficeOfPresentation));
	}

	public override void TestOtherRequirements_Export()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertCustomsOfficeRequirementEquals("Other Requirements Export OfficeOfPresentation", new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfPresentation, isMandatory: false, isLocalCountryOnly: false)
		{
			OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented }
		}, officeHelper.OtherRequirements.Single(x => x.OfficeRole == EuOfficeCodesTypes.Codes.OfficeOfPresentation));
		AssertCustomsOfficeRequirementEquals("Other Requirements Export SupervisingCustomsOffice", new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.SupervisingCustomsOffice, isMandatory: false, isLocalCountryOnly: false, EuOfficeCodesTypes.Descriptions.SupervisingCustomsOffice)
		{
			OfficeRolesForLookup = new ZString[]
			{
				EuOfficeCodesTypes.Codes.AeoCompetentCustomsAuthorities,
				EuOfficeCodesTypes.Codes.AeoMainCustomsAuthorities, EuOfficeCodesTypes.Codes.AuthorityControlCode,
				EuOfficeCodesTypes.Codes.AuthorizingCustomsAuthorityForRegularShippingServices, EuOfficeCodesTypes.Codes.CentralOfficeCommonDomain,
				EuOfficeCodesTypes.Codes.CentralOfficeNationalDomain, EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented,
				EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep, EuOfficeCodesTypes.Codes.CompetentAuthorityOfDispatch,
				EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry, EuOfficeCodesTypes.Codes.CompetentAuthorityOfExport,
				EuOfficeCodesTypes.Codes.CompetentAuthorityOfRecovery, EuOfficeCodesTypes.Codes.CustomsOfficeForControllingEconomicalProcedure,
				EuOfficeCodesTypes.Codes.EoriRegistrationAuthorities, EuOfficeCodesTypes.Codes.Excise,
				EuOfficeCodesTypes.Codes.HigherAuthority, EuOfficeCodesTypes.Codes.OfficeForCentralizedClearance,
				EuOfficeCodesTypes.Codes.OfficeOfDelivery, EuOfficeCodesTypes.Codes.OfficeOfDeparture,
				EuOfficeCodesTypes.Codes.OfficeOfDestination, EuOfficeCodesTypes.Codes.OfficeOfDispatch,
				EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, EuOfficeCodesTypes.Codes.OfficeOfExit,
				EuOfficeCodesTypes.Codes.OfficeOfExitInland, EuOfficeCodesTypes.Codes.OfficeOfExport,
				EuOfficeCodesTypes.Codes.OfficeOfGuarantee, EuOfficeCodesTypes.Codes.OfficeOfLodgement,
				EuOfficeCodesTypes.Codes.OfficeOfLodgementEntry, EuOfficeCodesTypes.Codes.OfficeOfLodgementExit,
				EuOfficeCodesTypes.Codes.OfficeOfTransit, EuOfficeCodesTypes.Codes.ReleaseForFreeCirculation, EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice
			}
		}, officeHelper.OtherRequirements.Single(x => x.OfficeRole == EuOfficeCodesTypes.Codes.SupervisingCustomsOffice));
	}

	public void TestOtherRequirements_ExitSummary()
	{
		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		AssertEquals("Other Requirements Exit Summary", false, officeHelper.OtherRequirements.Any());
	}

	public override void TestOtherRequirements_Miscellaneous()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertEquals("Other Requirements MiscellaneousCustoms", false, officeHelper.OtherRequirements.Any());
	}

	public void TestIsPresentationStartDateEditVisible_Export()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertEquals("Export IsPresentationStartDateEditVisible", true, officeHelper.IsPresentationStartDateEditVisible);
	}

	public void TestIsPresentationStartDateEditVisible_Import()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertEquals("Import IsPresentationStartDateEditVisible", true, officeHelper.IsPresentationStartDateEditVisible);
	}

	public void TestIsPresentationStartDateEditVisible_ExitSummary()
	{
		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		AssertEquals("Exit Summary IsPresentationStartDateEditVisible", false, officeHelper.IsPresentationStartDateEditVisible);
	}

	public void TestIsPresentationStartDateEditVisible_Miscellaneous()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertEquals("Miscellaneous IsPresentationStartDateEditVisible", true, officeHelper.IsPresentationStartDateEditVisible);
	}

	protected override string SetupDeclarationForCacheKey()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		return "JobDeclarationCustomsOfficeRequirementHelper,PL,EXP";
	}

	protected override EU.Business.Declaration.JobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();
}
