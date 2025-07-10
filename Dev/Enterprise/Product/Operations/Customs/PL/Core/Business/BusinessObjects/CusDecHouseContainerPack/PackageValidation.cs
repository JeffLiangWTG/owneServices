using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business;

public class PackageValidation : EU.Business.Declaration.PackageValidation
{
	public PackageValidation(Package parent) : base(parent)
	{
	}

	public new Package Package => (Package)base.Package;

	protected override void CheckCW_PackType()
	{
		base.CheckCW_PackType();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CW_PackTypeInfo);
	}
}
