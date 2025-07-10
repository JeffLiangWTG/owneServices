using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;

namespace Enterprise.Customs.NL.Business;

public class WarehouseWrapper : IWarehouse
{
	public WarehouseWrapper(Customs.Business.CusAuthorisationHeader authorisationHeader)
	{
		this.authorisationHeader = Argument.NotNull(authorisationHeader, nameof(authorisationHeader));
	}
	readonly Customs.Business.CusAuthorisationHeader authorisationHeader;

	public string Id => authorisationHeader.CPH_Number;

	public string TypeCode => authorisationHeader.Lookups.AuthorisationTypeList.GetDescriptionFromCode(authorisationHeader.CPH_Type);
}
