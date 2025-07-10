using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NO.Business;

public class JobDeclarationValidation : AutoNOJobDeclarationValidation
{
	public JobDeclarationValidation(JobDeclaration parent)
		: base(parent)
	{
	}

	protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateJE_GoodsNumber();
	}

	protected override void CheckJE_RN_NKTransportNationality()
	{
		base.CheckJE_RN_NKTransportNationality();
		var targetInfo = Parent.JE_RN_NKTransportNationalityInfo;

		if (Parent.TransportModeRequiresTransportNationality)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);
		}
	}

	#region JE_GoodsNumber

	public void ValidateJE_GoodsNumber()
	{
		ValidateCalculatedProperty(Parent.JE_GoodsNumberInfo);
	}

	protected virtual void CheckJE_GoodsNumber()
	{
		EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.JE_GoodsNumberInfo);
	}

	#endregion

	protected override void CheckJE_OH_Supplier()
	{
		base.CheckJE_OH_Supplier();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_SupplierInfo);
	}

	protected override void CheckJE_OH_Importer()
	{
		base.CheckJE_OH_Importer();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ImporterInfo);
	}

	protected override void CheckJE_TotalWeight()
	{
		base.CheckJE_TotalWeight();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_TotalWeightInfo);
	}

	protected override void CheckJE_TotalWeightUnit()
	{
		base.CheckJE_TotalWeightUnit();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_TotalWeightUnitInfo);
	}

	protected override void CheckJE_TotalNoOfPieces()
	{
		base.CheckJE_TotalNoOfPieces();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_TotalNoOfPiecesInfo);
	}

	protected override void CheckJE_OA_DeclarantAddress()
	{
		base.CheckJE_OA_DeclarantAddress();

		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OA_DeclarantAddressInfo);
		ValidateDeclarant_HasVATNo();
		ValidateDeclarant_HasAuthorization();
	}

	protected override void CheckJE_GS_NKCusAgent()
	{
		base.CheckJE_GS_NKCusAgent();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_GS_NKCusAgentInfo);
	}

	void ValidateDeclarant_HasVATNo()
	{
		var parent = Parent;
		if (!parent.JE_OA_DeclarantAddress.IsEmpty)
		{
			var declarantOrgHeader = parent.DeclarantAddress?.Header;
			if (declarantOrgHeader == null || declarantOrgHeader.CustomsCodes.GetCustomsRegNo(OrgCusCode.NorwayCodeTypes.MVA).IsEmpty)
			{
				parent.JE_OA_DeclarantAddressInfo.AddMessageError(Res.GetString("AB1AC377-5EDF-48FC-8873-C9076E67F5AF", "Declarant is missing MVA Number in Registration Numbers / Codes."));
			}
		}
	}

	void ValidateDeclarant_HasAuthorization()
	{
		var parent = Parent;
		if (parent is { JE_OA_DeclarantAddress.IsEmpty: false })
		{
			var dateOfValuation = parent.DateOfValuation;
			var header = parent.DeclarantAddress?.Header;

			if (header == null || CusAuthorisationHeader.Loader.GetAuthorisationNumber(header.Factory, Core.Constants.CountryCodes.Norway, CusAuthorizationType, dateOfValuation, header.PK).IsEmpty)
			{
				parent.JE_OA_DeclarantAddressInfo.AddMessageError(Res.GetString("082E80B0-B41D-40D2-A1C3-5EEDCB073A0E", "The Declarant must have an Authorization of Type '{0}'", CusAuthorizationType));
			}
		}
	}

	protected override void CheckJE_MessageSubType()
	{
		base.CheckJE_MessageSubType();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_MessageSubTypeInfo);
	}

	protected void CheckValidPowerOfAttorney(ZPropertyInfo targetInfo, OrgHeader organisation)
	{
		if (!targetInfo.Value.IsEmpty)
		{
			new AuthorityToActValidator().Validate(Parent, organisation, targetInfo, ZString.Empty, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms);
		}
	}

	protected override void CheckJE_LocationOfGoods()
	{
		base.CheckJE_LocationOfGoods();
		var parent = Parent;
		if (parent.JE_LocationOfGoods == ImportGoodsLocationCodeList.Codes._10DayWarehouseConsignee && AnyEntryInstructionWithPreliminaryDeclaration())
		{
			parent.JE_LocationOfGoodsInfo.AddMessageError(Res.GetString("9C43ADD9-8B96-4120-97A2-87043DC5A447", "Goods location A8 is not allowed on a Preliminary declaration entry (Customs Message Type FO)."));
		}
	}

	protected override void CheckJE_MergeBy()
	{
		base.CheckJE_MergeBy();
		var parent = Parent;
		if (parent.JE_MergeBy != MergeInvoiceLinesConstants.None && AnyEntryInstructionWithPreliminaryDeclaration())
		{
			parent.JE_MergeByInfo.AddWarning(Res.GetString("9E6BAAC5-47BB-4633-A7EC-B5325B9B9CA6", "The merge key should be NON on a Preliminary declaration entry (Customs Message Type FO)."));
		}
	}

	protected virtual ZString CusAuthorizationType => ZString.Empty;

	bool AnyEntryInstructionWithPreliminaryDeclaration()
	{
		return Parent.CustomsEntryInstructions
			.Any(x => x.CEI_SubStyle == ImportDeclarationSubTypes.Codes.P);
	}

	protected override void CheckJE_CustomsTransportMode()
	{
		base.CheckJE_CustomsTransportMode();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_CustomsTransportModeInfo);
	}
}
