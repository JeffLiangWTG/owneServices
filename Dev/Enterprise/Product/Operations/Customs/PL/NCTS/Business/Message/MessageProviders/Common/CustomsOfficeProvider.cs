using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CustomsOfficeProvider : ICustomsOffice
{
	protected CustomsOfficeProvider(NctsPLOfficeCode customsOffice)
	{
		this.customsOffice = Argument.NotNull(customsOffice, nameof(customsOffice));
	}
	protected readonly NctsPLOfficeCode customsOffice;

	public static CustomsOfficeProvider NewOrNull(NctsPLOfficeCode customsOffice) => customsOffice != null
		? new CustomsOfficeProvider(customsOffice)
		: null;

	public string ReferenceNumber => customsOffice.CY_Data;
}
