using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class UTBEntryLineFeeWrapper : NonPersistentBusinessObject, IEntryLineFee
{
	public CusEntryLineFee LineFee { get; private set; }
	public ZBool IsSummary { get; private set; }
	public UTBEntryLineFeeWrapper(CusEntryLineFee fee, bool isSummary) : base((fee as BusinessObject)?.Factory ?? new BusinessObjectFactory())
	{
		LineFee = fee;
		IsSummary = isSummary;
	}

	public ZString Description => LineFee.ChargeTypeDescription;
	public ZDecimal BaseAmount => LineFee.CF_BaseValue;
	public ZDecimal TariffApplied => LineFee.CF_Rate;
	public ZString MethodOfCalculation => LineFee.CF_MethodOfCalculation;
	public ZString UnitOfTariff => LineFee.CF_ChargeAmountInfo.Description;
	public ZString TariffCode => LineFee.CF_ChargeType;
	public ZDecimal TariffAmount => LineFee.CF_ChargeAmount;

	public UTBEntryLineDetailWrapper EntryLineDetail => IsSummary ? null : new UTBEntryLineDetailWrapper(LineFee.EntryLine, LineFee.PK);
}
