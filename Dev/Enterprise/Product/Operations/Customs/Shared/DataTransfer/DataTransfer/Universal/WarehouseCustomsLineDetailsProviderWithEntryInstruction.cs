using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public abstract class WarehouseCustomsLineDetailsProviderWithEntryInstruction<TDeclaration, TInvoice> : WarehouseCustomsLineDetailsProvider<TDeclaration, TInvoice>
		where TDeclaration : BaseJobDeclaration
		where TInvoice : BaseJobComInvoiceHeader
	{
		protected WarehouseCustomsLineDetailsProviderWithEntryInstruction(Shipment shipment)
			: base(shipment)
		{
		}

		protected sealed override IWarehouseCustomsLineDetails GetNewLineDetail(CommercialInvoiceLine invoiceLine, CommercialInvoiceHeader invoice)
		{
			return GetNewLineDetail(Factory, invoiceLine, (WarehouseCustomsFallbackDetailWithEntryInstruction)GetFallbackDetail(invoice));
		}

		protected abstract WarehouseCustomsLineDetailsWithEntryInstruction GetNewLineDetail(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, WarehouseCustomsFallbackDetailWithEntryInstruction fallbackDetail);

		protected Dictionary<ZInt, ZString> EntryInstructionProcedureMap
		{
			get
			{
				return entryInstructionProcedureMap = entryInstructionProcedureMap ?? GetNewEntryInstructionProcedureMap();
			}
		}

		protected virtual Dictionary<ZInt, ZString> GetNewEntryInstructionProcedureMap()
		{
			var result = new Dictionary<ZInt, ZString>();
			if (shipment.EntryInstructionCollection != null)
			{
				foreach (var entryInstruction in shipment.EntryInstructionCollection)
				{
					var link = entryInstruction.Link.GetValueOrDefault();
					if (link > ZInt.Zero && !result.ContainsKey(link))
					{
						result.Add(link, entryInstruction.Style.GetValueOrDefault());
					}
				}
			}

			return result;
		}

		Dictionary<ZInt, ZString> entryInstructionProcedureMap;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected Dictionary<ZInt, List<EntryHeader>> EntryInstructionEntryHeaderMap
		{
			get
			{
				if (entryInstructionEntryHeaderMap == null)
				{
					entryInstructionEntryHeaderMap = new Dictionary<ZInt, List<EntryHeader>>();
					if (shipment.EntryHeaderCollection != null)
					{
						foreach (var entryHeader in shipment.EntryHeaderCollection)
						{
							var link = entryHeader.EntryInstructionLink.GetValueOrDefault();
							if (link > ZInt.Zero)
							{
								if (!entryInstructionEntryHeaderMap.TryGetValue(link, out var list))
								{
									list = new List<EntryHeader>();
									entryInstructionEntryHeaderMap.Add(link, list);
								}
								list.Add(entryHeader);
							}
						}
					}
				}
				return entryInstructionEntryHeaderMap;
			}
		}
		Dictionary<ZInt, List<EntryHeader>> entryInstructionEntryHeaderMap;

		protected sealed override WarehouseCustomsFallbackDetail GetNewDeclarationDetail()
		{
			var messageType = shipment.MessageType.GetCodeAsUpperCase();
			return new WarehouseCustomsFallbackDetailWithEntryInstruction()
			{
				IsExport = messageType == JobMessageTypeList.Codes.Export,
				EntryInstructionProcedureMap = this.EntryInstructionProcedureMap,
				EntryInstructionEntryHeaderMap = this.EntryInstructionEntryHeaderMap,
				SupplierAddress = shipment.OrganizationAddressCollection.FindBestSupplierMatch(),
				InvoiceLineAddInfosApplicableForInwardWarehousing = InvoiceLineAddInfosApplicableForInwardWarehousing
			};
		}
	}
}
