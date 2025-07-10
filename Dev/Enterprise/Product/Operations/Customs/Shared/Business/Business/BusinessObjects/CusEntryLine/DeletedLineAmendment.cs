using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class DeletedLineAmendment : ICusEntryLine
	{
		public DeletedLineAmendment(ICusEntryLine originalEntryLine, CusEntryHeader currentEntryHeader)
		{
			this.originalEntryLine = originalEntryLine;
			this.currentEntryHeader = currentEntryHeader;
		}

		#region Implementation

		protected ICusEntryLine originalEntryLine;
		protected CusEntryHeader currentEntryHeader;

		#endregion

		#region ICusEntryLine Members

		ZDecimal ICusEntryLine.CL_CustomsValue
		{
			get { return originalEntryLine != null ? originalEntryLine.CL_CustomsValue : ZDecimal.Zero; }
		}

		ZShort ICusEntryLine.CL_LineNumber
		{
			get { return originalEntryLine != null ? originalEntryLine.CL_LineNumber : ZShort.Zero; }
		}

		ZString ICusEntryLine.FormattedTariff
		{
			get { return originalEntryLine != null ? originalEntryLine.FormattedTariff : ZString.Empty; }
		}

		ZString ICusEntryLine.Tariff
		{
			get { return originalEntryLine != null ? originalEntryLine.Tariff : ZString.Empty; }
		}

		ZDecimal ICusEntryLine.CustomsQuantity
		{
			get { return originalEntryLine != null ? originalEntryLine.CustomsQuantity : ZDecimal.Zero; }
		}

		ZString ICusEntryLine.CustomsUnitQty
		{
			get { return originalEntryLine != null ? originalEntryLine.CustomsUnitQty : ZString.Empty; }
		}

		ZString ICusEntryLine.Description
		{
			get { return originalEntryLine != null ? originalEntryLine.Description : ZString.Empty; }
		}

		ZString ICusEntryLine.ExtendedCommercialDescription
		{
			get { return originalEntryLine != null ? originalEntryLine.ExtendedCommercialDescription : ZString.Empty; }
		}

		ZDecimal ICusEntryLine.BondedWarehouseQuantity
		{
			get { return originalEntryLine != null ? originalEntryLine.BondedWarehouseQuantity : ZDecimal.Zero; }
		}

		ZString ICusEntryLine.BondedWarehouseUnitQuantity
		{
			get { return originalEntryLine != null ? originalEntryLine.BondedWarehouseUnitQuantity : ZString.Empty; }
		}

		ZDecimal ICusEntryLine.CL_DutyPercent
		{
			get { return originalEntryLine != null ? originalEntryLine.CL_DutyPercent : ZDecimal.Zero; }
		}

		ZString ICusEntryLine.DutyRateDescription
		{
			get { return originalEntryLine != null ? originalEntryLine.DutyRateDescription : ZString.Empty; }
		}

		ZString ICusEntryLine.CL_ParentTrailer
		{
			get { return originalEntryLine != null ? originalEntryLine.CL_ParentTrailer : ZString.Empty; }
		}

		ZDecimal ICusEntryLine.CL_WarehouseUnitValue
		{
			get { return originalEntryLine != null ? originalEntryLine.CL_WarehouseUnitValue : ZDecimal.Zero; }
		}

		ZDecimal ICusEntryLine.DutyAmount
		{
			get { return originalEntryLine != null ? originalEntryLine.DutyAmount : ZDecimal.Zero; }
		}

		ZDecimal ICusEntryLine.GSTVATAmount
		{
			get { return originalEntryLine != null ? originalEntryLine.GSTVATAmount : ZDecimal.Zero; }
		}

		ZDecimal ICusEntryLine.GSTVATDeferred
		{
			get { return originalEntryLine != null ? originalEntryLine.GSTVATDeferred : ZDecimal.Zero; }
		}

		InvoiceLinesForEntryLineCollection ICusEntryLine.InvoiceLines
		{
			get { return originalEntryLine != null ? originalEntryLine.InvoiceLines : null; }
		}

		BaseJobComInvoiceLine ICusEntryLine.RandomLine
		{
			get { return originalEntryLine != null ? originalEntryLine.RandomLine : null; }
		}

		Money ICusEntryLine.TotalLinePrice
		{
			get { return originalEntryLine != null ? originalEntryLine.TotalLinePrice : new Money(0, CusEntryHeader.GetLocalCurrencyFor(currentEntryHeader)); }
		}

		ZDecimal ICusEntryLine.TotalLinePriceInLocalCurrency
		{
			get { return originalEntryLine != null ? originalEntryLine.TotalLinePriceInLocalCurrency : ZDecimal.Zero; }
		}

		Money ICusEntryLine.CustomsValue
		{
			get { return CustomsValueCore; }
		}

		protected virtual Money CustomsValueCore
		{
			get { return originalEntryLine != null ? originalEntryLine.CustomsValue : null; }
		}
		#endregion
	}
}
