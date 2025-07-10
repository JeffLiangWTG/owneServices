using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer
{
	sealed class VerifiedGrossMassDataObjectWriter : DataObjectWriter<VerifiedGrossMass, UniversalShipment>
	{
		public VerifiedGrossMassDataObjectWriter(IDataWritingManager writeManager, IDocument document = null)
			: base(writeManager)
		{
			this.document = document;
		}

		readonly IDocument document;

		protected override UniversalShipment PopulateDataObject(VerifiedGrossMass vgm)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy)
			{
				DataContext = vgm.CreateUXmlDataContext(),
				BookingConfirmationReference = vgm.CarrierBookingReference,
				WayBillNumber = vgm.BillOfLadingNumber
			};
			shipment.SetContainerCollection(() => CreateContainers(vgm));

			if (vgm.ContainerMode != null)
			{
				shipment.ContainerMode = new ContainerMode
				{
					Code = vgm.ContainerMode.Code,
					Description = vgm.ContainerMode.Description
				};
			}

			if (vgm.ShipmentType != null)
			{
				shipment.ShipmentType = new CodeDescriptionPair
				{
					Code = vgm.ShipmentType.Code,
					Description = vgm.ShipmentType.Description
				};
			}

			shipment.VesselName = vgm.FirstSeaLegForChina?.Vessel?.Name;
			shipment.LloydsIMO = vgm.FirstSeaLegForChina?.Vessel?.LloydsIMO;
			shipment.VoyageFlightNo = vgm.FirstSeaLegForChina?.VoyageFlightNumber;

			var addresses = CreateAddresses(vgm).ToList();
			shipment.SetOrganizationAddressCollection(() => addresses.Any()
				? addresses
				: null);

			var addInfos = CreateAddInfos(vgm).ToList();
			shipment.SetAddInfoCollection(() => addInfos.Any()
				? addInfos
				: null);

			if (vgm.IsRequiredSendAttachment)
			{
				PopulateAttachedDocuments(shipment);
			}

			return shipment;
		}

		#region AttachedDocuments

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Attributes")]
		void PopulateAttachedDocuments(UniversalShipment uxmlShipment)
		{
			var fileAttributes = new DataObjectWriterHelper.FileAttributes()
			{
				Name = "Verified Gross Container Weight",
				Description = "Verified Gross Container Weight",
				Code = "VGM",
				IsPublished = false
			};

			DataObjectWriterHelper.AppendPopulatePDFAttachedDocument(uxmlShipment, document, fileAttributes);
		}

		#endregion

		#region Implementation

		DataObjectList<UniversalDataBuss.DataObjects.Universal.Container> CreateContainers(VerifiedGrossMass vgm)
		{
			var containers = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();

			foreach (var vgmContainer in vgm.Containers)
			{
				containers.Add(vgmContainer.ToUXmlContainer(writeManager.WriterStrategy));
			}

			return containers;
		}

		IEnumerable<OrganizationAddress> CreateAddresses(VerifiedGrossMass vgm)
		{
			if (!vgm.Shipper.IsEmpty())
			{
				yield return vgm.Shipper.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorDocumentaryAddress), writeManager.WriterStrategy);
			}

			if (!vgm.Carrier.IsEmpty())
			{
				yield return vgm.Carrier.ToUXmlOrganizationAddress(nameof(DocAddressType.ShippingLineAddress), writeManager.WriterStrategy);
			}

			if (!vgm.Consignee.IsEmpty())
			{
				yield return vgm.Consignee.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress), writeManager.WriterStrategy);
			}

			if (!vgm.FreightForwarder.IsEmpty())
			{
				yield return vgm.FreightForwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.BookingPartyDocumentaryAddress), writeManager.WriterStrategy);
			}

			if (!vgm.CarrierHandlingAgent.IsEmpty())
			{
				yield return vgm.CarrierHandlingAgent.ToUXmlOrganizationAddress(nameof(DocAddressType.CarrierHandlingAgent), writeManager.WriterStrategy);
			}

			if (!vgm.CarrierBookingAgent.IsEmpty())
			{
				yield return vgm.CarrierBookingAgent.ToUXmlOrganizationAddress(nameof(DocAddressType.CarrierBookingAgent), writeManager.WriterStrategy);
			}

			if (!vgm.CurrentUser.IsEmpty())
			{
				yield return vgm.CurrentUser.ToUXmlOrganizationAddress(nameof(vgm.CurrentUser), writeManager.WriterStrategy);
			}
		}

		IEnumerable<AddInfo> CreateAddInfos(VerifiedGrossMass vgm)
		{
			if (!vgm.OperationalPort.IsEmpty())
			{
				foreach (var addInfo in vgm.OperationalPort.ToUXmlAddInfos(nameof(vgm.OperationalPort)))
				{
					yield return addInfo;
				}
			}
		}

		#endregion
	}
}
