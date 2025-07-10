using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.NL
{
	sealed class CGNExportNotificationDataObjectWriter : DataObjectWriter<CGNExportNotification, UniversalShipment>
	{
		public CGNExportNotificationDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(CGNExportNotification exportNotification)
		{
			var universalShipment = new UniversalShipment(writeManager.WriterStrategy);
			universalShipment.DataContext = exportNotification.CreateUXmlDataContext();

			PopulateSubShipmentCollection(exportNotification, universalShipment);
			PopulateOrganizationAddressCollection(exportNotification, universalShipment);
			PopulateAddInfos(exportNotification, universalShipment);

			return universalShipment;
		}

		#region PopulateSubShipmentCollection

		void PopulateSubShipmentCollection(CGNExportNotification exportNotification, UniversalShipment uxmlShipment)
		{
			var shipments = new DataObjectList<UniversalShipment>();
			foreach (var cctShipment in exportNotification.Shipments)
			{
				var shipment = new UniversalShipment(writeManager.WriterStrategy);
				shipment.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
				{
					DataSource = new UniversalDataBuss.DataObjects.Universal._2012_11.DataSource
					{
						Type = DocDataConstants.DataSources.ForwardingShipment,
						Key = cctShipment.ShipmentID
					}
				};

				shipment.WayBillNumber = cctShipment.HouseBillNumber;
				shipment.WayBillType = new WayBillType
				{
					Code = "HBL"
				};

				shipment.TotalWeight = cctShipment.GrossWeight?.Value;
				shipment.TotalWeightUnit = new UnitOfWeight
				{
					Code = cctShipment.GrossWeight?.Unit?.Code,
					Description = cctShipment.GrossWeight?.Unit?.Description
				};

				shipment.TotalNoOfPacks = cctShipment.PackCount;
				shipment.TotalNoOfPacksPackageType = new PackageType()
				{
					Code = cctShipment.PackType?.Code,
					Description = cctShipment.PackType?.Description
				};

				if (cctShipment.MRNNumbers != null)
				{
					var additionalReferences = shipment.AdditionalReferenceCollection ?? new DataObjectList<AdditionalReference>();

					foreach (var mrnNumber in cctShipment.MRNNumbers)
					{
						var additionalReference = new AdditionalReference
						{
							Type = new EntryType
							{
								Code = mrnNumber.Type?.Code,
								Description = mrnNumber.Type?.Description
							},
							ReferenceNumber = mrnNumber?.Value
						};
						additionalReferences.Add(additionalReference);
					}

					shipment.SetAdditionalReferenceCollection(() => additionalReferences);
				}

				shipments.Add(shipment);
			}

			uxmlShipment.SetSubShipmentCollection(() => shipments.Any() ? shipments : null);
		}

		#endregion

		#region PopulateAddInfos

		void PopulateAddInfos(CGNExportNotification exportNotification, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();

				AddAddInfo(addInfos, "MAWB", exportNotification.MasterAirWaybill);
				AddAddInfo(addInfos, "OperationalPort_Code", exportNotification.OperationalPort?.Code ?? ZString.Empty);
				AddAddInfo(addInfos, "OperationalPort_Name", exportNotification.OperationalPort?.Name ?? ZString.Empty);

				return addInfos.Any() ? addInfos : null;
			});
		}

		void AddAddInfo(List<AddInfo> addinfos, ZString key, ZString value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				addinfos.Add(new AddInfo
				{
					Key = key,
					Value = value
				});
			}
		}

		#endregion

		#region PopulateOrganizationAddressCollection

		void PopulateOrganizationAddressCollection(CGNExportNotification exportNotification, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = CreateAddresses(exportNotification).ToList();
				return addresses.Count > 0 ? addresses : null;
			});
		}

		IEnumerable<OrganizationAddress> CreateAddresses(CGNExportNotification exportNotification)
		{
			if (!exportNotification.SendingParty.IsEmpty())
			{
				yield return exportNotification.SendingParty.ToUXmlOrganizationAddress("CurrentUser", writeManager.WriterStrategy, CreateRegistrationNumbers(exportNotification.SendingPartyCGNNumber));
			}

			if (!exportNotification.Carrier.IsEmpty())
			{
				yield return exportNotification.Carrier.ToUXmlOrganizationAddress(nameof(DocAddressType.Carrier), writeManager.WriterStrategy, CreateRegistrationNumbers(exportNotification.CarrierCGNNumber));
			}
		}

		IEnumerable<RegistrationNumber> CreateRegistrationNumbers(params DocumentVisualizer.DocDataObjects.RegistrationNumber[] registrationNumbers)
		{
			foreach (var number in registrationNumbers.Where(x => !x.Value.IsEmpty))
			{
				yield return number.ToUXmlRegistrationNumber();
			}
		}
		#endregion
	}
}
