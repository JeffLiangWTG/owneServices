using CargoWise.EntityFramework;

namespace Enterprise.Customs.NO.Business;

public class CusTempStorageRegHeaderValidation(CusTempStorageRegHeader parent) : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderValidation(parent)
{
	protected override void CheckSRH_PresentationDate()
	{
		base.CheckSRH_PresentationDate();
		MandatoryValidation.MessageErrorIfNotEntered(Header.SRH_PresentationDateInfo);
	}

	protected override void CheckSRH_Reference()
	{
		base.CheckSRH_Reference();
		MandatoryValidation.CheckEntered(Header.SRH_ReferenceInfo);
	}

	protected override void CheckSRH_CustomsOffice()
	{
		base.CheckSRH_CustomsOffice();
		MandatoryValidation.MessageErrorIfNotEntered(Header.SRH_CustomsOfficeInfo);
	}

	protected override void CheckSRH_PreviousReferenceType()
	{
		base.CheckSRH_PreviousReferenceType();
		ListValidation.MessageErrorIfInvalidCode(Header.SRH_PreviousReferenceTypeInfo);
	}

	CusTempStorageRegHeader Header => Parent as CusTempStorageRegHeader;
}
