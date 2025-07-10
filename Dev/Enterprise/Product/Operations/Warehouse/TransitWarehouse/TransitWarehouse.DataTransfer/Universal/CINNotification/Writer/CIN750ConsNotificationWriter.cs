using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using Address = Enterprise.DocumentVisualizer.DocDataObjects.Address;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Document
{
	public sealed class CIN750ConsNotificationWriter : CIN750NotificationWriter<CIN750ConsNotification>
	{
		public CIN750ConsNotificationWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override IEnumerable<(string AddressType, Address Address, RegistrationNumber RegistrationNumber)> GetAddressSources(CIN750ConsNotification notification)
		{
			yield return (nameof(DocAddressType.LocalCartageCFS), notification.DeclaredInWarehouse, notification.DeclaredInWarehouseCINNumber.ToUXmlRegistrationNumber());
		}

		protected override ZString GetMessageCode() => CIN750MessageTypes.Codes.CIN750ConNotification;

		protected override IEnumerable<AddInfo> GetPackingLineAddInfo(DocPackingLine docPackingLine)
		{
			yield return new AddInfo
			{
				Key = TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration,
				Value = docPackingLine.TemporaryStorageDeclaration
			};
		}

		protected override void PopulatePackingLines(CIN750ConsNotification notification, UniversalShipment uxmlShipment)
		{
			var packingLineCollection = new DataObjectList<PackingLine>();
			var subShipmentCollection = new DataObjectList<UniversalShipment>();
			var fromGoods = notification.FromGoods;
			var toGoods = notification.ToGoods;
			packingLineCollection.Add(GetPackingLine(toGoods));

			foreach (var fromGood in fromGoods)
			{
				var subShipment = new UniversalShipment(writeManager.WriterStrategy);
				subShipment.DataContext = DocDataObjectWriterExtensions.CreateUXmlDataContext(fromGood.SourceType, fromGood.SourceID);
				var additionalReferences = new DataObjectList<AdditionalReference>()
				{
					new AdditionalReference()
					{
						Type = new EntryType { Code = fromGood.RefType.Code,Description = fromGood.RefType.Description },
						ReferenceNumber = fromGood.RefCode
					}
				};
				subShipment.SetAdditionalReferenceCollection(() => additionalReferences);

				var subShipmentPackingLineCollection = new DataObjectList<PackingLine>();
				var fromPackingLine = base.GetPackingLine(fromGood);
				subShipmentPackingLineCollection.Add(fromPackingLine);
				subShipment.SetPackingLineCollection(() => subShipmentPackingLineCollection);

				subShipmentCollection.Add(subShipment);
			}

			if (packingLineCollection.Count > 0)
			{
				uxmlShipment.SetPackingLineCollection(() => packingLineCollection);
			}
			if (subShipmentCollection.Count > 0)
			{
				uxmlShipment.SetSubShipmentCollection(() => subShipmentCollection);
			}
		}
	}
}
