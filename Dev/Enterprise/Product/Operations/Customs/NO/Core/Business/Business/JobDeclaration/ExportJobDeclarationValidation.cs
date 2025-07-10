using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Constants = Enterprise.Customs.NO.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.NO.Business;

sealed class ExportJobDeclarationValidation : JobDeclarationValidation
{
	public ExportJobDeclarationValidation(JobDeclaration parent) : base(parent)
	{
	}

	protected override void CheckJE_OH_Supplier()
	{
		base.CheckJE_OH_Supplier();

		var parent = Parent;
		var targetInfo = parent.JE_OH_SupplierInfo;
		var supplier = parent?.Supplier;
		var orgType = (string)supplier?.OH_Category;
		if (orgType == Constants.OrgHeaderType.NaturalPerson)
		{
			if (!supplier.CustomsCodes.Any(cCode => cCode.OK_CodeType == Constants.OrgCodeType.SocialSecurityNumber && !cCode.OK_CustomsRegNo.IsEmpty))
			{
				targetInfo.AddMessageError(Res.GetString("9312E39C-A21A-4F2B-B961-B3AF906A0DB8", "The exporter is a natural person but has no social security number."));
			}
		}
		if (orgType == Constants.OrgHeaderType.Organization)
		{
			if (!supplier.CustomsCodes.Any(cCode => (cCode.OK_CodeType == Constants.OrgCodeType.OrganizationNumber || cCode.OK_CodeType == Constants.OrgCodeType.MVARegistrationNumber) && !cCode.OK_CustomsRegNo.IsEmpty))
			{
				targetInfo.AddMessageError(Res.GetString("A424F69C-0BE2-4E45-ACDF-C5307340543F", "The exporter is an organization but has no ORG number or MVA registered number."));
			}
		}

		CheckValidPowerOfAttorney(targetInfo, supplier);
	}

	protected override void CheckJE_CustomsOffice()
	{
		base.CheckJE_CustomsOffice();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_CustomsOfficeInfo);
	}

	protected override void CheckJE_GoodsDestination()
	{
		base.CheckJE_GoodsDestination();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_GoodsDestinationInfo);
	}

	protected override void CheckJE_GoodsNumber()
	{
		base.CheckJE_GoodsNumber();
		if (Parent.JE_LocationOfGoods.IsEmpty)
		{
			if (!Parent.JE_GoodsNumber.IsEmpty)
			{
				Parent.JE_GoodsNumberInfo.AddWarning(Res.GetString("41F9CE62-84B4-4C34-A58F-3EA50D7332B7", "Goods number is not required when goods location is blank."));
			}
		}
		else
		{
			if (Parent.JE_GoodsNumber.IsEmpty)
			{
				Parent.JE_GoodsNumberInfo.AddMessageError(Res.GetString("C08C99F9-E4A8-4184-A9FC-DB19A7610A48", "Goods number is required when goods location is set."));
			}
		}
	}

	protected override void CheckJE_GoodsOrigin()
	{
		base.CheckJE_GoodsOrigin();
		ListValidation.MessageErrorIfInvalidCode(Parent.JE_GoodsOriginInfo);
	}

	protected override void CheckJE_LocationOfGoods()
	{
		base.CheckJE_LocationOfGoods();
		ListValidation.MessageErrorIfInvalidCode(Parent.JE_LocationOfGoodsInfo);
		if (Parent.JE_LocationOfGoods.IsEmpty && Parent.CustomsEntryInstructions.Any(x => x.CEI_Procedure.EndsWith("70") || x.CEI_Procedure.EndsWith("71")))
		{
			Parent.JE_LocationOfGoodsInfo.AddMessageError(Res.GetString("5C088286-0BDD-480E-B9EC-D8A45335A8CE", "Please enter Goods location for goods released from customs warehouse."));
		}
	}

	protected override ZString CusAuthorizationType => CusAuthorizationHeaderTypeList.Codes.ExportCustomsDeclaration;
}
