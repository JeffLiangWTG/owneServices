using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public partial class JobDeclarationValidation(JobDeclaration parent) : AutoPLJobDeclarationValidation(parent)
{
	public JobDeclaration Declaration => Parent;

	protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateJE_OfficeOfEntryExit();
	}

	protected override void CheckJE_OwnerRef() { } // do nothing we are not using it.

	protected override void CheckJE_TotalNoOfPacksPackType() { } // do nothing we are not using it.

	protected override void CheckJE_TotalNoOfPacks()
	{
		base.CheckJE_TotalNoOfPacks();

		var declaration = Declaration;
		if (declaration.JE_TotalNoOfPacks <= 0)
		{
			declaration.JE_TotalNoOfPacksInfo.AddMessageError(Res.GetString("5EAAD744-439D-4416-AC20-26A89E6B1FDE", "Amount should be bigger than 0"));
		}
	}

	protected override void CheckJE_CustomsOffice()
	{
		base.CheckJE_CustomsOffice();
		ListValidation.MessageErrorIfInvalidCode(Parent.JE_CustomsOfficeInfo);
	}

	protected override void CheckJE_GoodsDestination()
	{
		base.CheckJE_GoodsDestination();

		var parent = Parent;
		if (parent.IsExport && parent.FilteredInvoiceLines.Any())
		{
			var lineZGCountryOfDestination = parent.FilteredInvoiceLines.First()?.ZG_CountryOfDestination ?? ZString.Empty;
			if (!lineZGCountryOfDestination.IsEmpty && lineZGCountryOfDestination != parent.JE_GoodsDestination && parent.FilteredInvoiceLines.All(line => line.ZG_CountryOfDestination == lineZGCountryOfDestination))
			{
				parent.JE_GoodsDestinationInfo.AddMessageError(Res.GetString("229937eb-aa1b-414b-88db-24d08b4ce149", "The Countries of Destination for Inv.Lines are different from the Declaration destination country."));
			}
		}
	}

	public void ValidateJE_OfficeOfEntryExit()
	{
		ValidateCalculatedProperty(Parent.JE_OfficeOfEntryExitInfo);
	}

	protected virtual void CheckJE_OfficeOfEntryExit()
	{
		var additionalOffice = Parent.CustomsOfficeRequirementHelper?.AdditionalOffice;
		if (additionalOffice != null && additionalOffice.IsMandatory)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_OfficeOfEntryExitInfo);
		}
		else
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_OfficeOfEntryExitInfo);
		}
	}

	protected override void CheckJE_UCR()
	{
		var parent = Parent;
		if (!Regex.IsMatch(parent.JE_UCR, @"^[a-zA-Z0-9\-_\.\@\#\/\\\=]*$"))
		{
			parent.JE_UCRInfo.AddMessageError(Res.GetString("3C52AC9A-6FB8-446F-8A43-5808BFFE4577", "Can contain only digits 0 to 9, letters from a to Z and signs '-', '_', '.', '@', '#', '/', '\\', '='."));
		}
	}

	protected override void CheckJE_DeclarantType()
	{
		var parent = Parent;
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.JE_DeclarantTypeInfo, parent.Lookups.PLRepresentationTypeList);
	}

	protected override void CheckJE_EntryStyle()
	{
		base.CheckJE_EntryStyle();

		var parent = Parent;
		if (parent.IsExitSummary && parent.JE_EntryStyle.IsEmpty)
		{
			parent.JE_EntryStyleInfo.AddMessageError(Res.GetString("8434A8A6-FBB8-4D69-9985-0DFC7A324E9B", "Entry Style is required for EXS - Exit Summary Declaration"));
		}
	}

	protected override void CheckJE_ContainerMode()
	{
		var parent = Parent;
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.JE_ContainerModeInfo, parent.Lookups.CargoIdTypeList);
	}

	protected override void CheckJE_ApplicationCode()
	{
		var parent = Parent;
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.JE_ApplicationCodeInfo, parent.Lookups.ApplicationCodeList);
	}

	protected override void CheckJE_OA_DeclarantAddress()
	{
		base.CheckJE_OA_DeclarantAddress();

		var parent = Parent;
		if (parent.DeclarantAddress?.Header is OrgHeader orgHeader)
		{
			PLOrgHeaderValidationHelper.ValidateHasCustomsRegNo(orgHeader.CustomsCodes, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, ZString.Empty, parent.JE_OA_DeclarantAddressInfo);
		}
	}

	protected override void CheckJE_OA_DeclarantAddressIsNotEmpty()
	{
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OA_DeclarantAddressInfo, messagePrefix: ValidationRuleMessagePrefixes.R293);
	}

	protected override void CheckJE_LloydsIMO()
	{
		base.CheckJE_LloydsIMO();

		var parent = Parent;
		if (parent.ZG_BorderTransportMeans == MeansOfTransportList.Codes.ImoShipIdentificationNumber)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JE_LloydsIMOInfo, Res.GetString("5AC0FAB5-24DC-462E-82CF-66B17943FDA7", "IMO number"));
		}
	}

	public void ValidateGoodsLocationCustomsOffice()
	{
		ValidateCalculatedProperty(Parent.GoodsLocationCustomsOfficeInfo);
	}

	protected virtual void CheckGoodsLocationCustomsOffice()
	{
		var parent = Parent;
		if (parent.IsLocationOfGoodsFromOfficeList)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.GoodsLocationCustomsOfficeInfo);
		}
	}

	public void ValidateGoodsLocationUNLocode()
	{
		ValidateCalculatedProperty(Parent.GoodsLocationUNLocodeInfo);
	}

	protected virtual void CheckGoodsLocationUNLocode()
	{
		var parent = Parent;
		if (parent.IsLocationOfGoodsFromUNLocode)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.GoodsLocationUNLocodeInfo);
		}
	}

	protected override void CheckJE_RN_NKTransportNationalityInland()
	{
		base.CheckJE_RN_NKTransportNationalityInland();

		var declaration = Parent;
		if (declaration.JE_TransportModeInland == TransportModes.Sea)
		{
			CheckSeaVesselNationality(declaration.JE_TransportMeans, declaration.VesselInland, declaration.JE_RN_NKTransportNationalityInland, declaration.JE_RN_NKTransportNationalityInlandInfo);
		}
		CheckRuleR0013E();
	}

	void CheckRuleR0013E()
	{
		var parent = Parent;
		var transportMeansType = parent.JE_TransportMeans.SubstringSafe(0, 1);
		if (!transportMeansType.IsEmpty
			&& !IsRailOrPostalOrFixedTransport())
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JE_RN_NKTransportNationalityInlandInfo, messagePrefix: "[R0013E] ");
		}

		bool IsRailOrPostalOrFixedTransport() => transportMeansType == ModeOfTransportList.Codes._2_RailTransport
												|| transportMeansType == ModeOfTransportList.Codes._5_PostalConsignment
												|| transportMeansType == ModeOfTransportList.Codes._7_FixedTransportInstallations;
	}

	protected override void CheckJE_RN_NKTrailer1Nationality()
	{
		base.CheckJE_RN_NKTrailer1Nationality();

		var parent = Parent;
		if (parent.IsRoadInland
			&& !parent.JE_Trailer1RegNo.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JE_RN_NKTrailer1NationalityInfo);
		}
	}

	protected override void CheckJE_RN_NKTrailer2Nationality()
	{
		base.CheckJE_RN_NKTrailer2Nationality();

		var parent = Parent;
		if (parent.IsRoadInland
			&& !parent.JE_Trailer2RegNo.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JE_RN_NKTrailer2NationalityInfo);
		}
	}

	void CheckSeaVesselNationality(string transportMeans, RefVessel vessel, ZString countryCode, ZPropertyInfo propertyInfo)
	{
		if (!countryCode.IsEmpty
			&& (transportMeans == ExportBorderTransportMeansList.Codes._10
				|| transportMeans == ExportBorderTransportMeansList.Codes._11)
			&& vessel is RefVessel
			&& !vessel.RV_RN_NKCountryOfReg.IsEmpty
			&& vessel.RV_RN_NKCountryOfReg != countryCode)
		{
			propertyInfo.AddMessageError(Res.GetString("E8FBDFD6-4CC1-4A6E-9CE1-7739077C0797", "The Vessel Registration Country is different from the Nationality code entered."));
		}
	}

	protected override void CheckJE_RN_NKTransportNationality()
	{
		base.CheckJE_RN_NKTransportNationality();

		var declaration = Parent;
		if (declaration.JE_TransportMode == TransportModes.Sea)
		{
			CheckSeaVesselNationality(declaration.ZG_BorderTransportMeans, declaration.Vessel, declaration.JE_RN_NKTransportNationality, declaration.JE_RN_NKTransportNationalityInfo);
		}
	}
}
