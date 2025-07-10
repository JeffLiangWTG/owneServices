using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public abstract class WarehouseCustomsLineDetailsWithEntryInstruction : WarehouseCustomsLineDetails
	{
		protected WarehouseCustomsLineDetailsWithEntryInstruction(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, WarehouseCustomsFallbackDetailWithEntryInstruction fallbackDetail)
			: base(factory, invoiceLine, fallbackDetail)
		{
		}

		protected WarehouseCustomsLineDetailsWithEntryInstruction(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, WarehouseCustomsFallbackDetailWithEntryInstruction fallbackDetail, Shipment shipment)
			: base(factory, invoiceLine, fallbackDetail, shipment)
		{
		}

		public new WarehouseCustomsFallbackDetailWithEntryInstruction FallbackDetail => (WarehouseCustomsFallbackDetailWithEntryInstruction)base.FallbackDetail;

		public sealed override ZDateTime? CustomsDeadline => CachedValueHelper.GetValue(ref customsDeadline, GetCustomsDeadline);
		CachedValue<ZDateTime?> customsDeadline;

		protected virtual ZDateTime? GetCustomsDeadline()
		{
			var entryInstructionLink = InvoiceLine.EntryInstructionLink.GetValueOrDefault();
			return entryInstructionLink > ZInt.Zero && FallbackDetail.EntryInstructionEntryHeaderMap.TryGetValue(entryInstructionLink, out var list) ? list.FirstOrDefault()?.BondValidToDate : null;
		}

		public sealed override ZString? Style => CachedValueHelper.GetValue(ref inwardStyle, GetStyle);
		CachedValue<ZString?> inwardStyle;

		protected virtual ZString? GetStyle() => EntryInstructionProcedure;

		public override ZString? Procedure => InvoiceLine.Procedure;

		protected abstract string CountryCode { get; }

		protected override bool IsOutward
		{
			get
			{
				if (isOutward == null)
				{
					isOutward = ProcedureLoader.GetProcedure(CustomsProcedureCode, InvoiceLine.Procedure.GetValueOrDefault(), CountryCode)?.IsOutOfRegime() ?? ZBool.False;
				}
				return isOutward.Value;
			}
		}
		bool? isOutward;

		protected ZString CustomsProcedureCode
		{
			get
			{
				if (!customsProcedureCode.HasValue)
				{
					customsProcedureCode = EntryInstructionProcedure.GetValueOrDefault();
				}
				return customsProcedureCode.Value;
			}
		}
		ZString? customsProcedureCode;

		protected ZString? EntryInstructionProcedure => CachedValueHelper.GetValue(ref entryInstructionProcedure, () =>
		{
			var entryInstructionLink = InvoiceLine.EntryInstructionLink.GetValueOrDefault();
			return entryInstructionLink > ZInt.Zero && FallbackDetail.EntryInstructionProcedureMap.TryGetValue(entryInstructionLink, out var result) ? result : null;
		});
		CachedValue<ZString?> entryInstructionProcedure;

		protected override ZString? GetPreviousEntryNumber() => IsOutward ? InvoiceLine.PreviousEntryNumber : null;

		protected override ZShort? GetPreviousEntryLineNumber() => IsOutward ? InvoiceLine.PreviousEntryLineNumber : null;

		RefCusProcedureLoader ProcedureLoader => procedureLoader ?? (procedureLoader = new RefCusProcedureLoader(factory));
		RefCusProcedureLoader procedureLoader;
	}
}
