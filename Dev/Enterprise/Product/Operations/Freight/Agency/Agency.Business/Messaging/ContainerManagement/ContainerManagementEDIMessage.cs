using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class ContainerManagementEDIMessage : EDIMessage
	{
		public ContainerManagementEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		public const string ContainerManagementMessageType = "CMM";

		public override void OnSaving()
		{
			base.OnSaving();

			if (EM_MessageNum.IsEmpty)
			{
				PopulateMessageNumber();
			}
		}

		protected override string GetMessageReferenceNumber()
		{
			return MessageNumberStrategy != null ? base.GetMessageReferenceNumber() : string.Empty;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.ContainerManagement;
			EM_MessageType = ContainerManagementMessageType;
		}
	}
}
