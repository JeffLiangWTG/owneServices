using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.DataTransfer.Document
{
	public class CIN750CorNotificationWriter : CIN750NotificationWriter<CIN750CorNotification>
	{
		public CIN750CorNotificationWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override IEnumerable<(string AddressType, Address Address, UniversalDataBuss.DataObjects.Universal.RegistrationNumber RegistrationNumber)> GetAddressSources(CIN750CorNotification notification)
		{
			yield return (nameof(DocAddressType.LocalCartageCFS), notification.DeclaredInWarehouse, notification.DeclaredInWarehouseCINNumber.ToUXmlRegistrationNumber());
		}

		protected override ZString GetMessageCode() => CIN750MessageTypes.Codes.CIN750CorNotification;

		protected override IEnumerable<AddInfo> GetPackingLineAddInfo(DocPackingLine docPackingLine)
		{
			yield return new AddInfo
			{
				Key = TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration,
				Value = docPackingLine.TemporaryStorageDeclaration
			};
		}
	}
}
