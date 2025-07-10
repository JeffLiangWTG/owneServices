using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class OfficeCodeValidation : EuOfficeCodeValidation
{
	public OfficeCodeValidation(OfficeCode parent) : base(parent)
	{
	}

	protected override void CheckIfOfficeTypeEmptyOrInvalid()
	{
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CY_CodeInfo, Parent.Lookups.CY_CodeList);
	}

	protected override CargoWise.ComponentModel.INotificationType OfficeTypeRepeatedMoreThanMaxNotificationType => NotificationType.Error;

	protected override bool IsTypeRepeatedMoreThanMax(int max) => OfficeCodeProvider.CustomsOfficesForBinding.Cast<EuOfficeCode>().Count(x => x.CY_Code == Parent.CY_Code) > max;

	protected new OfficeCode Parent => (OfficeCode)base.Parent;

	protected new JobDeclaration OfficeCodeProvider => Parent.Parent as JobDeclaration;
}
