using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business.Customs.EU;

namespace Enterprise.Customs.NL.Business;

public class CustomsValuationWrapper : ICustomsValuation
{
	public CustomsValuationWrapper(JobComInvoiceLine invLine)
	{
		this.invoiceLine = Argument.NotNull(invLine, nameof(invLine));
	}
	readonly JobComInvoiceLine invoiceLine;

	public string MethodCode => invoiceLine.JI_ValuationCode;

	public IReadOnlyCollection<IChargeDeduction> ChargeDeductions => chargeDeductions ??= ShouldChargesBeIncluded() ? invoiceLine.Charges.Select((x, index) => new ChargeDeductionWrapper(x, index + 1)).ToArray() : Array.Empty<IChargeDeduction>();
	IReadOnlyCollection<IChargeDeduction> chargeDeductions;

	bool ShouldChargesBeIncluded() => invoiceLine.EntryInstruction is Declaration.CusEntryInstruction entryInstruction && entryInstruction.CEI_Style.In(new ZString[] { DeclarationTypeList.Codes.H1, DeclarationTypeList.Codes.H5 }) && invoiceLine.EntryInstruction.ZG_IsHighValueOvrd && ValuationMethodList.Codes._1.Equals(invoiceLine.JI_ValuationCode);
}
