using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.eTail.DataTransfer
{
	public abstract class AsycudaManifestConverter : CustomsRelatedBusinessObjectConverter
	{
		public AsycudaManifestConverter(BaseHVLVRelatedJobCommand relatedJobCommand)
			: base(relatedJobCommand)
		{
		}

		public override DataContextType MasterBillDataContextType => DataContextType.AsycudaManifest;

		public virtual CodeDescriptionPair ManifestApplicationTypeCode => new CodeDescriptionPair();

		protected override void CreateManifestStyleEntryInstruction(Shipment dataObject, ForwardingShipment forwardingShipment)
		{
			var headerEntry = dataObject.EntryHeaderCollection?.Count == 1 ? dataObject.EntryHeaderCollection[0] : default;
			if (headerEntry != null)
			{
				var entryInstruction = new EntryInstruction(DefaultDataObjectWriterStrategy.Instance) { Link = 1, Style = ManifestType.Code };
				headerEntry.EntryInstructionLink = 1;
				dataObject.SetEntryInstructionCollection(() => new List<EntryInstruction> { entryInstruction });
			}
		}

		public override void OnDataObjectExported(Shipment result)
		{
			base.OnDataObjectExported(result);

			result.MessagingApplicationCode = ManifestApplicationTypeCode;
			PopulateContainerMode(result);
		}

		void PopulateContainerMode(Shipment dataObject)
		{
			var containerisedContainerModeList = new List<string>() { Core.Constants.ContainerModes.FCL, Core.Constants.ContainerModes.LCL, Core.Constants.ContainerModes.FCLMixedShipper };

			if (!AsycudaManifestHeaderLookups.ContainerModeList.ContainsCode(dataObject.ContainerMode?.Code))
			{
				if (containerisedContainerModeList.Contains(dataObject.ContainerMode?.Code))
				{
					dataObject.ContainerMode = ListHelper.GetWithDescription<ContainerMode>(Core.Constants.ContainerModes.Containerised, AsycudaManifestHeaderLookups.ContainerModeList);
				}
				else
				{
					dataObject.ContainerMode = new ContainerMode() { Code = string.Empty };
				}
			}
		}
	}
}
