using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class AESCurrencyExchangeProvider : ICurrencyExchange
{
	public AESCurrencyExchangeProvider(CusEntryInstruction entryInstruction)
	{
		this.entryInstruction = entryInstruction;
	}

	readonly CusEntryInstruction entryInstruction;

	const string NullCurrencyUnitByRuleR0099E = null;

	public string InternalCurrencyUnit => NullCurrencyUnitByRuleR0099E;

	public decimal? ExchangeRate =>  RuleR2011() ? null : entryInstruction.Invoices.First().JZ_InvoiceCurrExRate;

	bool RuleR2011()
	{
		var subStyle = entryInstruction.CEI_SubStyle;
		var invoices = entryInstruction.Invoices;
		return subStyle == Constants.SubStyleCodes.A
			|| subStyle == Constants.SubStyleCodes.D
			|| invoices.IsNullOrEmpty()
			|| invoices.First().JZ_InvoiceCurrExRate.IsEmpty;
	}
}
