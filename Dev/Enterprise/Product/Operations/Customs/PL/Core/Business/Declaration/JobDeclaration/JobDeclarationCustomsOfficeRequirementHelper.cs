using System.Collections.Generic;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class JobDeclarationCustomsOfficeRequirementHelper(JobDeclaration declaration) : EU.Business.JobDeclarationCustomsOfficeRequirementHelper(declaration)
{
	protected override CustomsOfficeRequirement GetMainOffice() => Factory.GetCachedValue("PL.JobDeclarationCustomsOfficeRequirementHelper.MainOffice" + Declaration.JE_MessageType, () => Declaration switch
	{
		{ } job when job.IsImport => new(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, isMandatory: true, isLocalCountryOnly: true, Res.GetString("D9DF59F5-1371-41D4-84D4-33FB7AE93B12", "Decl.Customs Office"))
		{
			OfficeRolesForLookup =
			[
				EuOfficeCodesTypes.Codes.AeoCompetentCustomsAuthorities,
				EuOfficeCodesTypes.Codes.AeoMainCustomsAuthorities,
				EuOfficeCodesTypes.Codes.AuthorityControlCode,
				EuOfficeCodesTypes.Codes.AuthorizingCustomsAuthorityForRegularShippingServices,
				EuOfficeCodesTypes.Codes.CentralOfficeCommonDomain,
				EuOfficeCodesTypes.Codes.CentralOfficeNationalDomain,
				EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented,
				EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep,
				EuOfficeCodesTypes.Codes.CompetentAuthorityOfDispatch,
				EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry,
				EuOfficeCodesTypes.Codes.CompetentAuthorityOfExport,
				EuOfficeCodesTypes.Codes.CompetentAuthorityOfRecovery,
				EuOfficeCodesTypes.Codes.CustomsOfficeForControllingEconomicalProcedure,
				EuOfficeCodesTypes.Codes.EoriRegistrationAuthorities,
				EuOfficeCodesTypes.Codes.Excise,
				EuOfficeCodesTypes.Codes.HigherAuthority,
				EuOfficeCodesTypes.Codes.OfficeForCentralizedClearance,
				EuOfficeCodesTypes.Codes.OfficeOfDelivery,
				EuOfficeCodesTypes.Codes.OfficeOfDeparture,
				EuOfficeCodesTypes.Codes.OfficeOfDestination,
				EuOfficeCodesTypes.Codes.OfficeOfDispatch,
				EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent,
				EuOfficeCodesTypes.Codes.OfficeOfExit,
				EuOfficeCodesTypes.Codes.OfficeOfExitInland,
				EuOfficeCodesTypes.Codes.OfficeOfExport,
				EuOfficeCodesTypes.Codes.OfficeOfGuarantee,
				EuOfficeCodesTypes.Codes.OfficeOfLodgement,
				EuOfficeCodesTypes.Codes.OfficeOfLodgementEntry,
				EuOfficeCodesTypes.Codes.OfficeOfLodgementExit,
				EuOfficeCodesTypes.Codes.OfficeOfTransit,
				EuOfficeCodesTypes.Codes.ReleaseForFreeCirculation,
				EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice
			]
		},
		{ } job when job.IsExport => new(EuOfficeCodesTypes.Codes.OfficeOfExport, isMandatory: true, isLocalCountryOnly: true, Res.GetString("BD43DE07-BE5B-463D-84C5-0B3F4E87A81C", "Office of Export")),
		{ } job when job.IsExitSummary => new(EuOfficeCodesTypes.Codes.OfficeOfExport, isMandatory: false, isLocalCountryOnly: true, Res.GetString("C57A4313-47E7-4E05-B22F-2103B46FB391", "Office of Lodgement")),
		_ => base.GetMainOffice()
	});

	public virtual CustomsOfficeRequirement AdditionalOffice => Factory.GetCachedValue("PL.JobDeclarationCustomsOfficeRequirementHelper.AdditionalOffice" + Declaration.JE_MessageType, () => Declaration switch
	{
		{ } job when job.IsImport => new(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, isMandatory: true, isLocalCountryOnly: false, Res.GetString("FC232324-634B-4CC2-9743-8FB88DDDAAD5", "Customs Office of Entry")),
		{ } job when job.IsExport => new(EuOfficeCodesTypes.Codes.ActualExitOffice, isMandatory: true, isLocalCountryOnly: false, Res.GetString("F8A8C19E-7821-4EE4-834A-0013389F0B19", "Office of Exit")) { OfficeRolesForLookup = [EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Codes.OfficeOfExitInland] },
		{ } job when job.IsExitSummary => new(EuOfficeCodesTypes.Codes.ActualExitOffice, isMandatory: false, isLocalCountryOnly: false, Res.GetString("F8A8C19E-7821-4EE4-834A-0013389F0B19", "Office of Exit")) { OfficeRolesForLookup = [EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Codes.OfficeOfExitInland] },
		_ => (CustomsOfficeRequirement)null
	});

	protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements() => Factory.GetCachedValue("PL.JobDeclarationCustomsOfficeRequirementHelper.OtherRequirements" + Declaration.JE_MessageType, () => Declaration switch
	{
		{ } job when job.IsImport => new List<CustomsOfficeRequirement>
		{
			new(EuOfficeCodesTypes.Codes.AuthorityControlCode, isMandatory: false, isLocalCountryOnly: false),
			new(EuOfficeCodesTypes.Codes.OfficeOfPresentation, isMandatory: false, isLocalCountryOnly: false)
			{
				OfficeRolesForLookup = [EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented]
			}
		},
		{ } job when job.IsExport => new List<CustomsOfficeRequirement>
		{
			new(EuOfficeCodesTypes.Codes.OfficeOfPresentation, isMandatory: false, isLocalCountryOnly: false)
			{
				OfficeRolesForLookup = [EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented]
			},
			new(EuOfficeCodesTypes.Codes.SupervisingCustomsOffice, isMandatory: false, isLocalCountryOnly: false, EuOfficeCodesTypes.Descriptions.SupervisingCustomsOffice)
			{
				OfficeRolesForLookup =
				[
					EuOfficeCodesTypes.Codes.AeoCompetentCustomsAuthorities,
					EuOfficeCodesTypes.Codes.AeoMainCustomsAuthorities,
					EuOfficeCodesTypes.Codes.AuthorityControlCode,
					EuOfficeCodesTypes.Codes.AuthorizingCustomsAuthorityForRegularShippingServices,
					EuOfficeCodesTypes.Codes.CentralOfficeCommonDomain,
					EuOfficeCodesTypes.Codes.CentralOfficeNationalDomain,
					EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented,
					EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep,
					EuOfficeCodesTypes.Codes.CompetentAuthorityOfDispatch,
					EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry,
					EuOfficeCodesTypes.Codes.CompetentAuthorityOfExport,
					EuOfficeCodesTypes.Codes.CompetentAuthorityOfRecovery,
					EuOfficeCodesTypes.Codes.CustomsOfficeForControllingEconomicalProcedure,
					EuOfficeCodesTypes.Codes.EoriRegistrationAuthorities,
					EuOfficeCodesTypes.Codes.Excise,
					EuOfficeCodesTypes.Codes.HigherAuthority,
					EuOfficeCodesTypes.Codes.OfficeForCentralizedClearance,
					EuOfficeCodesTypes.Codes.OfficeOfDelivery,
					EuOfficeCodesTypes.Codes.OfficeOfDeparture,
					EuOfficeCodesTypes.Codes.OfficeOfDestination,
					EuOfficeCodesTypes.Codes.OfficeOfDispatch,
					EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent,
					EuOfficeCodesTypes.Codes.OfficeOfExit,
					EuOfficeCodesTypes.Codes.OfficeOfExitInland,
					EuOfficeCodesTypes.Codes.OfficeOfExport,
					EuOfficeCodesTypes.Codes.OfficeOfGuarantee,
					EuOfficeCodesTypes.Codes.OfficeOfLodgement,
					EuOfficeCodesTypes.Codes.OfficeOfLodgementEntry,
					EuOfficeCodesTypes.Codes.OfficeOfLodgementExit,
					EuOfficeCodesTypes.Codes.OfficeOfTransit,
					EuOfficeCodesTypes.Codes.ReleaseForFreeCirculation,
					EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice
				]
			},
		},
		_ => base.GetOtherRequirements()
	});

	public virtual bool IsPresentationStartDateEditVisible =>
			Factory.GetCachedValue("PL.JobDeclarationCustomsOfficeRequirementHelper.PresentationStartDateEditVisible" + Declaration.JE_MessageType, () => !Declaration.IsExitSummary);

	new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
}
