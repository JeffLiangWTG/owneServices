using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public partial class ExportJobDeclarationValidation(JobDeclaration parent) : BaseExportJobDeclarationValidation(parent)
{
	protected override void CheckJE_CustomsOffice()
	{
		base.CheckJE_CustomsOffice();

		CheckRuleR0007E();
		CheckRuleR0002E();
	}

	void CheckRuleR0007E()
	{
		var parent = Parent;

		var authorisationCodeC513Exists = parent.CustomsEntryInstructions.Any(x => x.HasAuthorisationUsageCode(Constants.CusAuthorizationUsageType.C513));
		if (authorisationCodeC513Exists && parent.CustomsOfficesForBinding.Cast<EuOfficeCode>().Any(Rule342CustomsOfficeForBindingMeetsRequirements))
		{
			parent.JE_CustomsOfficeInfo.AddMessageError(Res.GetString("PLExportJobDeclarationValidation|CheckRuleR0007E",
				"(R0007E) A Decl. Customs Office code cannot be the same as a Customs Office of Presentation"));
		}
	}
	bool Rule342CustomsOfficeForBindingMeetsRequirements(EuOfficeCode office) => (office.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfPresentation) && (office.CY_Data == Parent.JE_CustomsOffice);

	void CheckRuleR0002E()
	{
		var customsOffice = Parent.JE_CustomsOffice;
		if (!customsOffice.IsEmpty && customsOffice.Left(2) != Core.Constants.CountryCodes.Poland)
		{
			Parent.JE_CustomsOfficeInfo.AddMessageError(Res.GetString("PLExportJobDeclarationValidation|CheckRuleR0002E", "[R0002E] The Customs Office code must start with PL."));
		}
	}

	protected override void CheckJE_RL_NKFinalDestination()
	{
		base.CheckJE_RL_NKFinalDestination();

		var parent = Parent;

		var headerDestination = parent.JE_RL_NKFinalDestination.Left(2);
		var inlineDestinations = parent
								.Invoices
								.SelectMany(x => x.InvoiceLines)
								.Cast<JobComInvoiceLine>()
								.Select(x => x.ZG_CountryOfDestination);

		HashSet<ZString> uniqueLineDestinations = inlineDestinations.ToHashSet();
		if (headerDestination.IsEmpty)
		{
			if (uniqueLineDestinations.Count == 1 && uniqueLineDestinations.First().IsEmpty)
			{
				parent.JE_RL_NKFinalDestinationInfo.AddMessageError(Res.GetString("1bd57d73-08fe-4a09-9b39-48632d0da9d0", "A Country of Destination must be declared at header or line level."));
			}
		}
		else
		{
			if (uniqueLineDestinations.Count == 1
				&& !uniqueLineDestinations.Single().IsEmpty
				&& uniqueLineDestinations.Single() != headerDestination)
			{
				parent.JE_RL_NKFinalDestinationInfo.AddWarning(Res.GetString("f9a45de0-25b9-43fc-bdc3-6c348550ca7a", "The Country of Destination declared in the header will be ignored because all the lines have the same value, which differs from the value declared in the header."));
			}
			else
			{
				bool hasMatchingDestination = parent.FilteredInvoiceLines.Any(cod => cod.ZG_CountryOfDestination == headerDestination);
				bool hasAtLeastOneInlineDestination = parent
													.Invoices
													.SelectMany(x => x.InvoiceLines)
													.Cast<JobComInvoiceLine>()
													.Any(line => !line.ZG_CountryOfDestination.IsEmpty);
				if (hasAtLeastOneInlineDestination && !hasMatchingDestination)
				{
					parent.JE_RL_NKFinalDestinationInfo.AddWarning(Res.GetString("53e5b756-fecb-458a-8ad1-19cc59266a88", "\"Destination\" value in Shipment Details section is different than values indicated in \"Country of Destination\" on Invoice Lines."));
				}
			}
		}
	}

	protected override void CheckJE_OA_Representative()
	{
		base.CheckJE_OA_Representative();

		CheckRuleR0010E_Representative();
		CheckRuleR0893();
		CheckRuleR0940(Parent.RepresentativeOrgAddress, Parent.JE_OA_RepresentativeInfo);
	}

	void CheckRuleR0010E_Representative()
	{
		var parent = Parent;
		if (parent.JE_OA_Representative.IsValid
			&& AddressHelper.GetExporterAddressWithSupplierFallback(parent)?.Header is OrgHeader exporter
			&& !IsEUNHeader(exporter))
		{
			parent.JE_OA_RepresentativeInfo.AddMessageError(Res.GetString("PLExportJobDeclarationValidation|RepresentativeAddressShouldBeEmptyMessage",
				"[R0010E] Representative cannot be given."));
		}
	}

	void CheckRuleR0893()
	{
		var parent = Parent;
		if (parent.RepresentativeOrgAddress is OrgAddress representativeAddress
			&& parent.Declarant is OrgAddress declarantAddress)
		{
			var representativeEORI = AddressHelper.GetEORIForOrganisationAddress(representativeAddress);
			var declarantEORI = AddressHelper.GetEORIForOrganisationAddress(declarantAddress);

			if (!representativeEORI.IsEmpty
				&& !declarantEORI.IsEmpty
				&& representativeEORI.Equals(declarantEORI))
			{
				parent.JE_OA_RepresentativeInfo.AddMessageError(Res.GetString("PLExportJobDeclarationValidation|RepresentativeSameEoriMessage",
					"[R0893] EORI number for Representative must be different from Declarant EORI number."));
			}
		}
	}

	protected override void CheckJE_OA_DeclarantAddress()
	{
		base.CheckJE_OA_DeclarantAddress();

		CheckRuleR0010E_Declarant();
		CheckRuleR0940(Parent.Declarant, Parent.JE_OA_DeclarantAddressInfo);
	}

	void CheckRuleR0010E_Declarant()
	{
		var parent = Parent;
		if (parent.Declarant is OrgAddress declarantAddress
			&& AddressHelper.GetExporterAddressWithSupplierFallback(parent)?.Header is OrgHeader exporter
			&& !IsEUNHeader(exporter))
		{
			var cusCode = AddressHelper.GetEORICusCodeForOrganisationAddress(declarantAddress);

			if (!(cusCode is OrgCusCode)
				|| cusCode.OK_CustomsRegNo.IsEmpty
				|| !(cusCode.CodeCountry?.IsPartOfEuropeanUnion ?? false))
			{
				parent.JE_OA_DeclarantAddressInfo.AddMessageError(Res.GetString("PLExportJobDeclarationValidation|DeclarantAddressEoriMessage",
					"[R0010E] Declarant EORI number starting with EU country code is required"));
			}
		}
	}

	protected override void CheckJE_LocationOfGoods()
	{
		if (Parent.IsAuthorisationNumberQualifier)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_LocationOfGoodsInfo);
		}
	}

	protected override void CheckGoodsLocationUNLocode()
	{
		base.CheckGoodsLocationUNLocode();
		var latitude = Parent.GoodsLocationUNLocodeData?.RL_GeoLocation.Latitude ?? 0;
		var longitude = Parent.GoodsLocationUNLocodeData?.RL_GeoLocation.Longitude ?? 0;
		if (Parent.JE_LocationQualifier == QualifierOfTheIdentificationList.Codes.W && latitude == 0 && longitude == 0)
		{
			Parent.GoodsLocationUNLocodeInfo.AddMessageError(Res.GetString("PLExportJobDeclarationValidation|CheckGoodsLocationUNLocode", "GNSS does not exists for the UNLOCO selected"));
		}
	}

	protected override void CheckJE_VoyageFlightNo()
	{
		base.CheckJE_VoyageFlightNo();
		var parent = Parent;
		if (parent.IsAir)
		{
			if (!parent.ZG_BorderTransportMeans.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.JE_VoyageFlightNoInfo);
			}

			if (parent.JE_VoyageFlightNo.IsEmpty && ExportJobDeclarationValidationHelper.IsC0890TransportIdRequired(parent))
			{
				parent.JE_VoyageFlightNoInfo.AddMessageError(Res.GetString("PLExportJobDeclarationValidation|C0890", "[C0890] Transport ID/Flight number is required."));
			}
		}
	}

	protected override void CheckJE_RN_NKTransportNationality()
	{
		base.CheckJE_RN_NKTransportNationality();
		if (!Parent.ZG_BorderTransportMeans.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RN_NKTransportNationalityInfo);
		}
	}

	protected override void CheckJE_VesselName()
	{
		base.CheckJE_VesselName();
		var parent = Parent;
		if (parent.IsSea && !parent.ZG_BorderTransportMeans.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JE_VesselNameInfo);
		}
		if ((parent.IsSea || parent.IsWaterwayTransport || parent.IsOwnPropulsion || parent.IsRoad)
			&& parent.JE_VesselName.IsEmpty && ExportJobDeclarationValidationHelper.IsC0890TransportIdRequired(parent))
		{
			parent.JE_VesselNameInfo.AddMessageError(Res.GetString("PLExportJobDeclarationValidation|C0890", "[C0890] Transport ID/Flight number is required."));
		}
	}

	protected override void CheckJE_TransportIDInland()
	{
		base.CheckJE_TransportIDInland();
		EU.Business.Declaration.JobDeclarationUCC6ExportValidation.CheckJE_TransportIDInland(Parent);
	}

	protected override void CheckJE_Trailer1RegNo()
	{
		base.CheckJE_Trailer1RegNo();
		EU.Business.Declaration.JobDeclarationUCC6ExportValidation.CheckJE_Trailer1RegNo(Parent);
	}

	protected override void CheckJE_AircraftRegistrationInland()
	{
		base.CheckJE_AircraftRegistrationInland();
		EU.Business.Declaration.JobDeclarationUCC6ExportValidation.CheckJE_AircraftRegistrationInland(Parent);
	}

	static bool IsEUNHeader(OrgHeader header) => header?.Country?.IsPartOfEuropeanUnion ?? false;

	protected override void CheckJE_TransportModeInland()
	{
		base.CheckJE_TransportModeInland();

		if (Parent.JE_TransportModeInland.IsEmpty)
		{
			CheckC0843();
		}
	}

	void CheckC0843()
	{
		var parent = Parent;
		var customsOfficeOfPresentation = Declaration.CustomsOfficeOfPresentationReferenceNumber();
		var customsOfficeOfExitDeclared = parent.JE_OfficeOfEntryExit;
		var customsOfficeOfExport = parent.JE_CustomsOffice;

		if ((!customsOfficeOfPresentation.IsEmpty
			&& customsOfficeOfExitDeclared != customsOfficeOfPresentation)
			|| (customsOfficeOfPresentation.IsEmpty
				&& customsOfficeOfExitDeclared != customsOfficeOfExport))
		{
			if (!IsC0843EntrySubStyleOrMessageSubTypeAndProcedure_Case1()
				&& !IsC0843EntrySubStyleOrMessageSubTypeAndProcedure_Case2())
			{
				parent.JE_TransportModeInlandInfo.AddMessageError(Res.GetString("PLExportJobDeclarationValidation|C0843", "[C0843] Inland M.O.T. is required."));
			}
		}
	}

	bool IsC0843EntrySubStyleOrMessageSubTypeAndProcedure_Case1()
	{
		var parent = Parent;
		return parent.CustomsEntryInstructions.Any(x => IsSubStyleBOrCOrEOrF(x.CEI_SubStyle))
				|| (parent.JE_MessageSubType == EntryStyleListExport.Codes.ExportToSpecialTerritory
					&& parent.CustomsEntryInstructions.Any(x => x.CEI_Procedure == Constants.ProcedureCodes._10));
	}

	bool IsC0843EntrySubStyleOrMessageSubTypeAndProcedure_Case2()
	{
		var parent = Parent;
		return parent.CustomsEntryInstructions.Any(x => x.CEI_SubStyle == SubStyleCodes.D)
				|| (parent.JE_MessageSubType == EntryStyleListExport.Codes.ExportToSpecialTerritory
					&& parent.CustomsEntryInstructions.Any(x => IsProcedureCode76Or77(x.CEI_Procedure)));
	}

	bool IsSubStyleBOrCOrEOrF(ZString subStyle) => subStyle == SubStyleCodes.B
													|| subStyle == SubStyleCodes.C
													|| subStyle == SubStyleCodes.E
													|| subStyle == SubStyleCodes.F;

	bool IsProcedureCode76Or77(ZString procedureCode) => procedureCode == Constants.ProcedureCodes._76
														|| procedureCode == Constants.ProcedureCodes._77;

	protected override void CheckJE_OfficeOfEntryExit()
	{
		base.CheckJE_OfficeOfEntryExit();

		CheckRuleR0082E();
	}

	void CheckRuleR0082E()
	{
		var declaration = Parent;

		if (declaration.JE_OfficeOfEntryExit != declaration.JE_CustomsOffice
			&& declaration.CustomsEntryInstructions.Take(1).Count() == 1
			&& declaration.CustomsEntryInstructions.All(entry => entry.CEI_SubStyle == SubStyleCodes.R))
		{
			declaration.JE_OfficeOfEntryExitInfo.AddMessageError(Res.GetString("1140754F-6240-4D9C-8E7C-B686BD4FB811", "[R0082E] For Export Declaration with Sub Style ’R’ Customs Office of Export must be equal to Customs Office of Exit Declared"));
		}
	}

	protected override void CheckJE_OH_ShippingLine()
	{
		base.CheckJE_OH_ShippingLine();

		CheckRuleR0840();
	}

	void CheckRuleR0840()
	{
		var parent = Parent;
		var address = parent.ShippingLine?.Addresses.FirstOrDefault() as OrgAddress;
		CheckHasProperRegistrationNumber(address, parent.JE_OH_ShippingLineInfo, R0840Message, carrierRegistrationCodes);
	}

	void CheckRuleR0940(OrgAddress address, ZPropertyInfo addressPropertyInfo)
		=> CheckHasProperRegistrationNumber(address, addressPropertyInfo, R0940Message, registrationCodes);

	void CheckHasProperRegistrationNumber(OrgAddress address, ZPropertyInfo info, string errorMessage, ZString[] expectedCodes)
	{
		if (address is null)
		{
			return;
		}

		var cusCodes = address.Header.CustomsCodes.GetOrgCusCodesForMatchingCodesIgnoringCountry(expectedCodes);
		var addressCusCode = cusCodes.FirstOrDefault(x => x.OK_OA_PremisesAddress == address.PK) ?? cusCodes.FirstOrDefault(x => !x.OK_OA_PremisesAddress.IsValid);
		if (addressCusCode == null)
		{
			info.AddMessageError(errorMessage);
		}
	}

	static string R0840Message => Res.GetString("PLExportJobDeclarationValidation|CheckRuleR0840", "[R0840] In CC515C, CC513C, CC613C or CC615C message for Carrier, ‘EOR’ or ‘TCU’ Registration Number is required.");

	static string R0940Message => Res.GetString("PLExportJobDeclarationValidation|CheckRuleR0940", "[R0940] One of Registration Number: ‘EOR’, ‘TCU’, ‘PES’, ‘PAS’ or ‘DRV’ is required.");

	readonly ZString[] registrationCodes =
	[
		OrgCusCode.EuropeanUnionSharedCodeTypes.Eori,
		OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU,
		OrgCusCode.PolandCodeTypes.PES,
		OrgCusCode.CodeTypes.PassportID,
		OrgCusCode.CodeTypes.DriverLicenceID
	];

	readonly ZString[] carrierRegistrationCodes =
	[
		OrgCusCode.EuropeanUnionSharedCodeTypes.Eori,
		OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU
	];
}
