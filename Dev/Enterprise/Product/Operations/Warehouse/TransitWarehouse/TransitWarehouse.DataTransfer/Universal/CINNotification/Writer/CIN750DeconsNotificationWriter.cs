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
	public sealed class CIN750DeconsNotificationWriter : CIN750NotificationWriter<CIN750DeconsNotification>
	{
		public CIN750DeconsNotificationWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override IEnumerable<(string AddressType, Address Address, RegistrationNumber RegistrationNumber)> GetAddressSources(CIN750DeconsNotification notification)
		{
			yield return (nameof(DocAddressType.LocalCartageCFS), notification.DeclaredInWarehouse, notification.DeclaredInWarehouseCINNumber.ToUXmlRegistrationNumber());
		}

		protected override ZString GetMessageCode() => CIN750MessageTypes.Codes.CIN750DeconNotification;

		protected override IEnumerable<AddInfo> GetPackingLineAddInfo(DocPackingLine docPackingLine)
		{
			yield return new AddInfo
			{
				Key = TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration,
				Value = docPackingLine.TemporaryStorageDeclaration
			};
		}

		protected override void PopulatePackingLines(CIN750DeconsNotification notification, UniversalShipment uxmlShipment)
		{
			var packingLineCollection = new DataObjectList<PackingLine>();
			var subShipmentCollection = new DataObjectList<UniversalShipment>();
			var link = 0;
			foreach (var fromToGoods in notification.GoodsPairs)
			{
				link += 1;

				var toPackingLine = base.GetPackingLine(fromToGoods.Item2);
				toPackingLine.Link = link;
				packingLineCollection.Add(toPackingLine);

				var fromGoods = fromToGoods.Item1;
				var subShipment = new UniversalShipment(writeManager.WriterStrategy);
				subShipment.DataContext = DocDataObjectWriterExtensions.CreateUXmlDataContext(fromGoods.SourceType, fromGoods.SourceID);
				var additionalReferences = new DataObjectList<AdditionalReference>()
				{
					new AdditionalReference()
					{
						Type = new EntryType { Code = fromGoods.RefType.Code,Description = fromGoods.RefType.Description },
						ReferenceNumber = fromGoods.RefCode
					}
				};
				subShipment.SetAdditionalReferenceCollection(() => additionalReferences);

				var subShipmentPackingLineCollection = new DataObjectList<PackingLine>();
				var fromPackingLine = base.GetPackingLine(fromGoods);
				fromPackingLine.Link = link;
				subShipmentPackingLineCollection.Add(fromPackingLine);
				subShipment.SetPackingLineCollection(() => subShipmentPackingLineCollection);
				subShipment.SetAddInfoCollection(() => new List<AddInfo>() {
					new AddInfo()
					{
						Key = CIN750AddInfoConstants.Keys.JobID,
						Value = notification.JobID
					},
					new AddInfo()
					{
						Key = CIN750AddInfoConstants.Keys.MessageID,
						Value = fromGoods.DeconsMessageID
					}
				});

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

		protected override void PopulateAddInfo(CIN750DeconsNotification notification, UniversalShipment shipment)
		{
			var addInfoCollection = new List<AddInfo>() {
				new AddInfo() { Key = CIN750AddInfoConstants.Keys.ServerType, Value = GetServerType() }
			};
			shipment.SetAddInfoCollection(() => addInfoCollection);
		}
	}
}
