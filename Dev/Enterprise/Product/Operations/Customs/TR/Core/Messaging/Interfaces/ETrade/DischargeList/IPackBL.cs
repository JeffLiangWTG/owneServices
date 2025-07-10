using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface IPackBL
	{
		ZString LineNo { get; }
		ZString GoodDescription { get; }
		ZString Tariff { get; }
		ZString Unit { get; }
		ZDecimal GrossWeight { get; }
		ZDecimal NetWeight { get; }
	}
}
