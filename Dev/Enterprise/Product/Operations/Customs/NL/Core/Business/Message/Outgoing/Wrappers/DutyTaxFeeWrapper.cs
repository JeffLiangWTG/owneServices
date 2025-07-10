using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.Business;

public class DutyTaxFeeWrapper : IDutyTaxFee
{
	public DutyTaxFeeWrapper(EU.Business.Declaration.CusEntryLineFee cusEntryLineFee, int sequenceNumeric)
	{
		this.cusEntryLineFee = Argument.NotNull(cusEntryLineFee, nameof(cusEntryLineFee));
		SequenceNumeric = sequenceNumeric;
	}
	readonly EU.Business.Declaration.CusEntryLineFee cusEntryLineFee;

	public int SequenceNumeric { get; }

	public string TypeCode => cusEntryLineFee.CF_ChargeType;

	public string CCQualifierCode => null;

	public IPayment Payment
	{
		get
		{
			var ins = cusEntryLineFee.EntryLine.Header.EntryInstruction;
			if (ins.CEI_Style.In(new ZString[] { DeclarationTypeList.Codes.H1, DeclarationTypeList.Codes.H5, DeclarationTypeList.Codes.I1 })
				&& !ins.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Any(x => x.AGC_Code == CusAuthorizationHeaderTypeList.Codes.DeferredPayment))
			{
				return new PaymentWrapper(EU.Business.UniversalReferenceConstants.MethodOfPayments.Cash);
			}
			return new PaymentWrapper(cusEntryLineFee.G4_MethodOfPayment);
		}
	}

	public IReadOnlyCollection<ITaxBase> TaxBases => taxBases ??= GetTaxBases().ToList();
	IReadOnlyCollection<ITaxBase> taxBases;

	IEnumerable<ITaxBase> GetTaxBases()
	{
		yield return new TaxBaseWrapper(cusEntryLineFee, 1);
	}
}
