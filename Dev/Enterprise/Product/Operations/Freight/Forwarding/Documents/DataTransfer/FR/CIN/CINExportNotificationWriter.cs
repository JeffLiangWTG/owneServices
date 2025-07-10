using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.FR
{
	sealed class CINExportNotificationWriter : DataObjectWriter<CINExportNotification, UniversalShipment>
	{
		public CINExportNotificationWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(CINExportNotification exportNotification)
		{
			var universalShipment = new UniversalShipment(writeManager.WriterStrategy);

			universalShipment.DataContext = exportNotification.CreateUXmlDataContext();
			var notExistsDirectConsol = exportNotification.AgentTypes.All(pair => pair.Code != Core.Constants.AgentType.Direct);
			universalShipment.WayBillType = new WayBillType
			{
				Code = notExistsDirectConsol ? ShippingInstructionReleaseTypes.Codes.HouseBill : ShippingInstructionReleaseTypes.Codes.AirWaybill,
				Description = notExistsDirectConsol ? ShippingInstructionReleaseTypes.Descriptions.HouseBill : ShippingInstructionReleaseTypes.Descriptions.AirWaybill
			};

			universalShipment.WayBillNumber = notExistsDirectConsol ? exportNotification.HouseBill : exportNotification.MasterBill;
			universalShipment.TotalWeight = exportNotification.GrossWeight?.Value;
			universalShipment.TotalWeightUnit = new UnitOfWeight()
			{
				Code = exportNotification.GrossWeight?.Unit?.Code,
				Description = exportNotification.GrossWeight?.Unit?.Description
			};

			universalShipment.TotalNoOfPacks = exportNotification.PackCount;
			universalShipment.TotalNoOfPacksPackageType = new PackageType
			{
				Code = exportNotification.PackType.Code,
				Description = exportNotification.PackType.Description
			};

			PopulateAdditionalReferences(exportNotification, universalShipment);
			PopulateAddInfos(exportNotification, universalShipment);
			PopulateAddresses(exportNotification, universalShipment);

			return universalShipment;
		}

		#region PopulateAdditionalReferences

		void PopulateAdditionalReferences(CINExportNotification exportNotification, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = new DataObjectList<AdditionalReference>(CreateAdditionalReferences(exportNotification));
				return additionalReferences.Any() ? additionalReferences : null;
			});
		}

		IEnumerable<AdditionalReference> CreateAdditionalReferences(CINExportNotification exportNotification)
		{
			if (exportNotification.MRNNumbers.Any())
			{
				foreach (var number in exportNotification.MRNNumbers)
				{
					yield return new AdditionalReference
					{
						ReferenceNumber = number.Value,
						Type = new EntryType
						{
							Code = CusEntryNumberTypes.Standard.MovementReferenceNumber,
							Description = (NoResString)"Movement Reference Number" // Valid Code and should not be localized
						}
					};
				}
			}

			if (!exportNotification.CustomsOfficeCode.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = exportNotification.CustomsOfficeCode,
					Type = new EntryType
					{
						Code = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.COC,
						Description = (NoResString)"Customs Office Code" // Valid Code and should not be localized
					}
				};
			}
		}

		#endregion

		#region PopulateAddInfos

		void PopulateAddInfos(CINExportNotification exportNotification, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();

				AddAddInfo(addInfos, "MAWB", exportNotification.MasterBill);
				AddAddInfo(addInfos, "OperationalPort_Code", exportNotification.OperationalPort?.Code ?? ZString.Empty);
				AddAddInfo(addInfos, "OperationalPort_Name", exportNotification.OperationalPort?.Name ?? ZString.Empty);

				return addInfos.Any() ? addInfos : null;
			});
		}

		void AddAddInfo(List<AddInfo> addinfos, ZString key, ZString value)
		{
			if (!value.IsEmpty)
			{
				addinfos.Add(new AddInfo
				{
					Key = key,
					Value = value
				});
			}
		}

		#endregion

		#region PopulateAddresses

		void PopulateAddresses(CINExportNotification exportNotification, UniversalShipment shipment)
		{
			shipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = CreateAddresses(exportNotification).ToList();
				return addresses.Count > 0 ? addresses : null;
			});
		}

		IEnumerable<OrganizationAddress> CreateAddresses(CINExportNotification exportNotification)
		{
			if (!exportNotification.SendingParty.IsEmpty())
			{
				yield return exportNotification.SendingParty.ToUXmlOrganizationAddress("CurrentUser", writeManager.WriterStrategy, CreateRegistrationNumbers(exportNotification.SendingPartyCINNumber));
			}

			if (!exportNotification.Carrier.IsEmpty())
			{
				yield return exportNotification.Carrier.ToUXmlOrganizationAddress(nameof(DocAddressType.Carrier), writeManager.WriterStrategy, CreateRegistrationNumbers(exportNotification.CarrierCINNumber));
			}

			if (!exportNotification.Warehouse.IsEmpty())
			{
				yield return exportNotification.Warehouse.ToUXmlOrganizationAddress(nameof(DocAddressType.DepartureCFSAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(exportNotification.WarehouseCINNumber));
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
