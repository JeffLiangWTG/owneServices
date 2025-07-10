using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HVLShipmentWritingHelper : IHVLShipmentWritingHelper
	{
		public HVLShipmentWritingHelper(IDataWritingManager writeManager, ForwardingShipment parentShipmentBO, UniversalShipment parentShipmentData, bool includeAdditionalReferenceCollectionOverride)
		{
			this.writeManager = Argument.NotNull(writeManager, nameof(writeManager));
			this.parentShipmentBO = Argument.NotNull(parentShipmentBO, nameof(parentShipmentBO));
			this.parentShipmentData = Argument.NotNull(parentShipmentData, nameof(parentShipmentData));
			this.includeAdditionalReferenceCollectionOverride = includeAdditionalReferenceCollectionOverride;
		}

		readonly IDataWritingManager writeManager;
		readonly ForwardingShipment parentShipmentBO;
		readonly UniversalShipment parentShipmentData;
		readonly bool includeAdditionalReferenceCollectionOverride;

		protected DataObjectWriter<HVLVConsignment, UniversalShipment> ConsignmentDataObjectWriter => consignmentDataObjectWriter ?? (consignmentDataObjectWriter = GetWriter());

		DataObjectWriter<HVLVConsignment, UniversalShipment> consignmentDataObjectWriter;

		DataObjectWriter<HVLVConsignment, UniversalShipment> GetWriter()
		{
			var dataExportStrategy = DefaultHVLVConsignmentDataExportStrategy.Instance;

			if (writeManager.IsPublishingInternally)
			{
				if (includeAdditionalReferenceCollectionOverride)
				{
					dataExportStrategy = HVLVConsignmentToCargoReportWithAdditionalReferencesDataExportStrategy.Instance;
				}
				else
				{
					dataExportStrategy = HVLVConsignmentToCargoReportDataExportStrategy.Instance;
				}
			}

			return new HVLVConsignmentDataObjectWriter(writeManager, dataExportStrategy);
		}

		void IHVLShipmentWritingHelper.WriteCommercialInfo()
		{
			var consignmentWriter = ConsignmentDataObjectWriter as HVLVConsignmentDataObjectWriter;
			foreach (var consignment in parentShipmentBO.HVLVConsignments.OfType<HVLVConsignment>().OrderBy(consignment => consignment.HVC_WaybillNumber))
			{
				using (consignment.SetIsBeingExportedToUniversalXml())
				{
					consignmentWriter.WriteCommercialInvoiceAndLines(consignment, parentShipmentData);
				}
			}
		}

		void IHVLShipmentWritingHelper.WriteConsignmentsAsSubShipments()
		{
			PopulateShipmentGoodsLocation();

			if (!ObjectFactory.Get<IUniversalXmlContentFilterApplicator>().GetShouldUniversalShipmentExcludeCollection(parentShipmentData, writeManager.Action, nameof(HVLVConsignment)))
			{
				parentShipmentData.SetSubShipmentCollection(() =>
				{
					var subShipments = new DataObjectList<UniversalShipment>();

					foreach (var consignment in parentShipmentBO.HVLVConsignments.OfType<HVLVConsignment>().OrderBy(consignment => consignment.HVC_WaybillNumber))
					{
						using (consignment.SetIsBeingExportedToUniversalXml())
						{
							subShipments.Add(ConsignmentDataObjectWriter.GetDataObject(consignment));
						}
					}

					return subShipments;
				});
			}
		}

		void PopulateShipmentGoodsLocation()
		{
			var cfsAddress = parentShipmentBO.Factory.Load<OrgAddress>(parentShipmentBO.JS_OA_ImportReleaseDepot);
			if (cfsAddress == null && parentShipmentBO.ArrivalConsol != null)
			{
				cfsAddress = parentShipmentBO.Factory.Load<OrgAddress>(parentShipmentBO.ArrivalConsol.JK_OA_UnpackDepotAddress);
			}

			if (cfsAddress != null)
			{
				parentShipmentData.AddOrgAddress(writeManager, cfsAddress, DocAddressType.GoodsLocation);
			}
		}
	}
}
