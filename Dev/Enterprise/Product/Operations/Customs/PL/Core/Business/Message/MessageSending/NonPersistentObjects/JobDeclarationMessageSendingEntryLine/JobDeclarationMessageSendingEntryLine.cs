using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class JobDeclarationMessageSendingEntryLine : AutoJobDeclarationMessageSendingEntryLine, IJobDeclarationMessageSendingEntryLine
{
	public JobDeclarationMessageSendingEntryLine(CusEntryLine line, JobDeclarationMessageSendingEntryLineCollection parentCollection) : base(line.Factory)
	{
		ParentCollection = Argument.NotNull(parentCollection, nameof(parentCollection));
		using (GetValidationSuspender())
		using (SuspendSettingHasChanges())
		{
			Line = Argument.NotNull(line, nameof(line));
			SetJobDeclarationMessageSendingEntryLineDefaultValues();
		}
	}

	public override ZBool Send
	{
		get => base.Send;
		set
		{
			var oldValue = base.Send;
			base.Send = value;
			if (Send != oldValue)
			{
				if (!IsValidationSuspended)
				{
					ParentCollection.Cast<JobDeclarationMessageSendingEntryLine>().ForEach(x => x.Validation.ValidateSend());
				}
			}
		}
	}

	[ReadOnly(true)]
	public override ZShort LineNumber
	{
		get => base.LineNumber;
		set => base.LineNumber = value;
	}

	[ReadOnly(true)]
	public override ZString TariffCode
	{
		get => base.TariffCode;
		set => base.TariffCode = value;
	}

	[ReadOnly(true)]
	public override ZString Description
	{
		get => base.Description;
		set => base.Description = value;
	}

	[ReadOnly(true)]
	public override ZString QuotaOrdNo
	{
		get => base.QuotaOrdNo;
		set => base.QuotaOrdNo = value;
	}

	[ReadOnly(true)]
	public override ZString SupUq
	{
		get => base.SupUq;
		set => base.SupUq = value;
	}

	[ReadOnly(true)]
	public override ZDecimal QuotaQuantity
	{
		get => base.QuotaQuantity;
		set => base.QuotaQuantity = value;
	}

	public CusEntryLine Line { get; }

	public JobDeclarationMessageSendingEntryLineCollection ParentCollection { get; }

	void SetJobDeclarationMessageSendingEntryLineDefaultValues()
	{
		LineNumber = Line.CL_LineNumber;
		TariffCode = Line.FormattedTariff;
		Description = Line.EffectiveDescription;
		QuotaOrdNo = Line.RandomLine.JI_ConcessionOrder;
		SupUq = new ZString(Line.RandomLine.GetRefCusQuota()?.ZXQ_UnitOfMeasure);
		QuotaQuantity = SupUq.IsEmpty ? ZDecimal.Zero : (ZDecimal)Line.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => GetQuotaCustomsQuantity(x, SupUq));
	}

	static ZDecimal GetQuotaCustomsQuantity(JobComInvoiceLine line, ZString quotaUoM)
	{
		if (line.JI_CustomsUnitQty == quotaUoM)
		{
			return line.JI_CustomsQuantity;
		}

		if (line.JI_CustomsSecondUnitQty == quotaUoM)
		{
			return line.JI_CustomsSecondQuantity;
		}

		if (line.JI_CustomsThirdUnitQty == quotaUoM)
		{
			return line.JI_CustomsThirdQuantity;
		}

		if (line.JI_CustomsFourthUnitQty == quotaUoM)
		{
			return line.JI_CustomsFourthQuantity;
		}

		return ZDecimal.Zero;
	}
}
