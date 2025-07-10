using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DataTransfer.Universal
{
	class PreviousClearedWriter : ITopLevelDataObjectWriter
	{
		public PreviousClearedWriter(RecipientRoleType recipientRoleType)
		{
			this.recipientRoleType = recipientRoleType;
		}
		readonly RecipientRoleType recipientRoleType;

		#region ITopLevelDataObjectWriter Members

		public ZString EDIMessageSubType
		{
			get { return EDIMessageSubTypeList.Codes.XmlUniversalShipment; }
		}

		public ITopLevelDataObject GetDataObject(BusinessObject sourceBO)
		{
			var declaration = (IWarehouseIntegrationSupporter)sourceBO;
			return declaration.GetLastClearedUniversalShipment(recipientRoleType);
		}

		public ZString RootElementName
		{
			get { return (NoResString)"Shipment"; }
		}

		public DataContextType TopLevelDataContextType
		{
			get { return DataContextType.CustomsDeclaration; }
		}

		#endregion
	}
}
