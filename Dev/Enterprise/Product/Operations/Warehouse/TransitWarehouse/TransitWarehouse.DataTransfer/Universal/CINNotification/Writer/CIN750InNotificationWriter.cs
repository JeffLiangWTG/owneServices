using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using Address = Enterprise.DocumentVisualizer.DocDataObjects.Address;

namespace Enterprise.Warehouse.Transit.DataTransfer.Document
{
	public class CIN750InNotificationWriter : CIN750NotificationWriter<CIN750InNotification>
	{
		public CIN750InNotificationWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override IEnumerable<(string AddressType, Address Address, RegistrationNumber RegistrationNumber)> GetAddressSources(CIN750InNotification notification)
		{
			yield return (nameof(DocAddressType.LocalCartageCFS), notification.DeclaredInWarehouse, notification.DeclaredInWarehouseCINNumber.ToUXmlRegistrationNumber());
			yield return (nameof(DocAddressType.ArrivalCTOAddress), notification.FromCTO, notification.FromCTOCINNumber.ToUXmlRegistrationNumber());
		}

		protected override ZString GetMessageCode() => CIN750MessageTypes.Codes.CIN750InNotification;

		protected override IEnumerable<AddInfo> GetPackingLineAddInfo(DocPackingLine docPackingLine)
		{
			yield return new AddInfo
			{
				Key = TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration,
				Value = docPackingLine.TemporaryStorageDeclaration
			};
			yield return new AddInfo
			{
				Key = docPackingLine.AccompanyDocumentType,
				Value = docPackingLine.AccompanyDocumentRef
			};
		}

		protected override void PopulateAddInfo(CIN750InNotification notification, Shipment shipment)
		{
			base.PopulateAddInfo(notification, shipment);

			if (notification.CustomsStatus != null)
			{
				shipment.MessageStatus = new CodeDescriptionPair
				{
					Code = notification.CustomsStatus.Code,
					Description = notification.CustomsStatus.Description
				};
			}
		}
	}
}
