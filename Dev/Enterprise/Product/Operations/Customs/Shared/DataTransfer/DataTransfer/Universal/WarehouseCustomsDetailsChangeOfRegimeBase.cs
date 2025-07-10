using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public abstract class WarehouseCustomsDetailsChangeOfRegimeBase : IWarehouseCustomsDetailsChangeOfRegime
	{
		protected WarehouseCustomsDetailsChangeOfRegimeBase(Shipment shipment)
		{
			Shipment = Argument.NotNull(shipment, nameof(shipment));
			RandomInvoiceLine = shipment.CommercialInfo?.CommercialInvoiceCollection?.FirstOrDefault()?.CommercialInvoiceLineCollection?.FirstOrDefault();
		}
		protected readonly Shipment Shipment;
		protected readonly CommercialInvoiceLine RandomInvoiceLine;

		public abstract CustomsRegime IntoRegimeType { get; }

		public virtual CustomsRegime OutOfRegimeType => IsOutOfInwardProcessing ? CustomsRegime.InwardProcessing : CustomsRegime.BondedWarehouse;

		public virtual OrganizationAddress NewWarehouse => RelatedEntryInstruction?.OrganizationAddressCollection.FirstOrDefault(AddressTypes.Warehouse2);

		protected EntryInstruction RelatedEntryInstruction => CachedValueHelper.GetValue(ref relatedEntryInstructionCached, () =>
		{
			EntryInstruction result = null;
			if (RandomInvoiceLine != null)
			{
				var entryInstructionLink = RandomInvoiceLine.EntryInstructionLink.GetValueOrDefault();
				if (!entryInstructionLink.IsEmpty)
				{
					result = Shipment.EntryInstructionCollection?.FirstOrDefault(h => h.Link == entryInstructionLink);
				}
			}
			return result;
		});
		CachedValue<EntryInstruction> relatedEntryInstructionCached;

		ZBool IsOutOfInwardProcessing => CachedValueHelper.GetValue(ref isOutOfInwardProcessingCached, () => RelatedEntryInstruction is EntryInstruction entryInstruction
			? ProcedureLoader.GetProcedure(GetEntryInstructionProcedure(entryInstruction), RandomInvoiceLine?.Procedure.GetValueOrDefault() ?? ZString.Empty, Shipment.GetSourceCountryCode())?.IsOutOfInwardProcessing() ?? ZBool.False
			: ZBool.False);

		protected virtual ZString GetEntryInstructionProcedure(EntryInstruction entryInstruction) => entryInstruction.Procedure ?? ZString.Empty;

		CachedValue<ZBool> isOutOfInwardProcessingCached;

		RefCusProcedureLoader ProcedureLoader => procedureLoader ?? (procedureLoader = new RefCusProcedureLoader(new BusinessObjectFactory()));
		RefCusProcedureLoader procedureLoader;
	}
}
