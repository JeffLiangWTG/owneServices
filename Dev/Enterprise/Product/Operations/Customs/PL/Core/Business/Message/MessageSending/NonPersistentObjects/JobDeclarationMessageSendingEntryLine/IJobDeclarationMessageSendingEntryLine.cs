using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public interface IJobDeclarationMessageSendingEntryLine
{
	ZShort LineNumber { get; }
	ZString TariffCode { get; }
	ZString Description { get; }
	ZString QuotaOrdNo { get; }
	ZString SupUq { get; }
	ZDecimal QuotaQuantity { get; }
	CusEntryLine Line { get; }
}
