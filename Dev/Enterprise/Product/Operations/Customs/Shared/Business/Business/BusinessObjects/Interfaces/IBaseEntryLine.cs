using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface IBaseEntryLine
	{
		ZGuid PK { get; }
		ZShort CL_LineNumber { get; }
		ZString CL_AdValoremTariff { get; }
		ZDecimal CL_CustomsValue { get; }
		ZDecimal DutyAmount { get; }
	}
}
