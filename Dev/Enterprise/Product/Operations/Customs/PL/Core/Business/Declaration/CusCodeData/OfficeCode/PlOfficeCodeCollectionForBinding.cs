using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public sealed class PlOfficeCodeCollectionForBinding : BusinessObjectCollectionView<OfficeCode>
{
	public PlOfficeCodeCollectionForBinding(JobDeclaration declaration)
		: base(Argument.NotNull(declaration, nameof(declaration)).CustomsOffices)
	{ }

	protected override bool IsThisPartOfTheCollection(BusinessObject element)
		=> element is EuOfficeCode officeCode
			&& officeCode.CY_Code != EuOfficeCodesTypes.Codes.OfficeOfExit
			&& officeCode.CY_Code != EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent;
}
