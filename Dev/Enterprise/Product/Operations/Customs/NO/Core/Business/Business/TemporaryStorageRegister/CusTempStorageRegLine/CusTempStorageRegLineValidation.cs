using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.NO.Business;

public sealed class CusTempStorageRegLineValidation(CusTempStorageRegLine parent) : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineValidation(parent)
{
	CusTempStorageRegLine CusTempStorageRegLine => Parent as CusTempStorageRegLine;

	protected override void CheckSRL_OwnerReference()
	{
		base.CheckSRL_OwnerReference();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.SRL_OwnerReferenceInfo);
	}

	protected override void CheckSRL_GoodsDescription()
	{
		base.CheckSRL_GoodsDescription();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.SRL_GoodsDescriptionInfo);
	}

	protected override void CheckSRL_OwnerReferenceType()
	{
		base.CheckSRL_OwnerReferenceType();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.SRL_OwnerReferenceTypeInfo);
	}

	protected override void CheckSRL_PackageType()
	{
		base.CheckSRL_PackageType();
		ListValidation.MessageErrorIfInvalidCode(Parent.SRL_PackageTypeInfo);
	}

	protected override void CheckSRL_LineNumber()
	{
		base.CheckSRL_LineNumber();

		var regHeader = CusTempStorageRegLine?.RegHeader;
		if (regHeader != null)
		{
			var lineNumber = Parent.SRL_LineNumber;
			var currentPK = Parent.PK;
			if (regHeader.CusTempStorageRegLines.Any(x => x.SRL_LineNumber == lineNumber && x.PK != currentPK))
			{
				Parent.SRL_LineNumberInfo.AddMessageError(Res.GetString("1e54fb43-9a56-4c6d-883e-6060940f94eb", "Line Number should not be duplicated."));
			}
		}
	}

	protected override void CheckSRL_CustodianIdentifier()
	{
		base.CheckSRL_CustodianIdentifier();
		CheckEORIFormat(Parent.SRL_CustodianIdentifierInfo);
	}

	protected override void CheckSRL_CustodianIdentifierBranchNo()
	{
		base.CheckSRL_CustodianIdentifierBranchNo();
		if (!Parent.SRL_CustodianIdentifierBranchNo.IsEmpty)
		{
			GermanValidationHelper.ValidateEBS(Parent.SRL_CustodianIdentifierBranchNoInfo);
		}
	}

	protected override void CheckSRL_GoodsOwnerIdentifier()
	{
		base.CheckSRL_GoodsOwnerIdentifier();
		CheckEORIFormat(Parent.SRL_GoodsOwnerIdentifierInfo);
	}

	protected override void CheckSRL_GoodsOwnerIdentifierBranchNo()
	{
		base.CheckSRL_GoodsOwnerIdentifierBranchNo();
		if (!Parent.SRL_GoodsOwnerIdentifierBranchNo.IsEmpty)
		{
			GermanValidationHelper.ValidateEBS(Parent.SRL_GoodsOwnerIdentifierBranchNoInfo);
		}
	}

	void CheckEORIFormat(ZPropertyInfo propertyInfo)
	{
		var eori = (ZString)propertyInfo.Value;
		if (eori.Length >= 2)
		{
			var countryCode = eori.Left(2);
			if (Parent.Factory.LoadFromNaturalKey<IRefCountry>(ZArchitecture.Schema.RefCountrySchema.RN_Code, countryCode) == null)
			{
				propertyInfo.AddMessageError(Res.GetString("1b416efc-9743-4a52-a944-f011b92966cd", "The EORI code needs to start with a country/region code as a prefix."));
			}
		}
	}
}
