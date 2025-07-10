using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using Address = Enterprise.DocumentVisualizer.DocDataObjects.Address;

namespace Enterprise.Warehouse.Transit.DataTransfer.Document
{
	sealed public class CIN750OutNotificationWriter : CIN750NotificationWriter<CIN750OutNotification>
	{
		public CIN750OutNotificationWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override IEnumerable<(string AddressType, Address Address, RegistrationNumber RegistrationNumber)> GetAddressSources(CIN750OutNotification notification)
		{
			yield return (nameof(DocAddressType.LocalCartageCFS), notification.DeclaredInWarehouse, notification.DeclaredInWarehouseCINNumber.ToUXmlRegistrationNumber());
			yield return (nameof(DocAddressType.DepartureCTOAddress), notification.ToCTO, notification.ToCTOCINNumber.ToUXmlRegistrationNumber());
		}

		protected override ZString GetMessageCode() => CIN750MessageTypes.Codes.CIN750OutNotification;

		protected override IEnumerable<AddInfo> GetPackingLineAddInfo(DocPackingLine docPackingLine)
		{
			yield return new AddInfo
			{
				Key = TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration,
				Value = docPackingLine.TemporaryStorageDeclaration
			};
		}

		protected override void PopulateAddInfo(CIN750OutNotification notification, Shipment shipment)
		{
			if (notification.CustomsStatus != null)
			{
				shipment.MessageStatus = new CodeDescriptionPair
				{
					Code = notification.CustomsStatus.Code,
					Description = notification.CustomsStatus.Description
				};
			}

			var addInfoCollection = new List<AddInfo>() {
				new AddInfo() { Key = CIN750AddInfoConstants.Keys.JobID, Value = notification.JobID },
				new AddInfo() { Key = CIN750AddInfoConstants.Keys.MessageID, Value = notification.MessageID },
				new AddInfo() { Key = CIN750AddInfoConstants.Keys.ServerType, Value = GetServerType() }
			};

			if (notification.CustomsDocuments != null && notification.CustomsDocuments.Count > 0)
			{
				addInfoCollection = addInfoCollection.Concat(notification.CustomsDocuments.Select(p => new AddInfo
				{
					Key = p.RefType,
					Value = p.RefCode
				})).ToList();
			}
			shipment.SetAddInfoCollection(() => addInfoCollection);
		}
	}
}
