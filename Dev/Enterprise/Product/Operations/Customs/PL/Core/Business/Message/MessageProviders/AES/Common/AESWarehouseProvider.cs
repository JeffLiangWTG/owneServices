using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class AESWarehouseProvider : IWarehouse
{
	public AESWarehouseProvider(OrgCusCode cusCode)
	{
		this.cusCode = Argument.NotNull(cusCode, nameof(cusCode));
	}

	readonly OrgCusCode cusCode;

	public string Type => cusCode.OK_CustomsRegNo.Left(1);

	public string Identifier => cusCode.OK_CustomsRegNo;
}
