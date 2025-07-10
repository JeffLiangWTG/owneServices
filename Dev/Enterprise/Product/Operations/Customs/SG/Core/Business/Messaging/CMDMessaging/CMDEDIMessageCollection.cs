
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging
{
	public class CMDEDIMessageCollection : BusinessObjectCollection<CMDEDIMessage>
	{
		public CMDEDIMessageCollection(CMDShipmentWrapper parent)
			: base(parent.Factory)
		{
			this.Parent = parent;

			SetReadOnlyIncludingChildren(true);
			IsManagedForDataRefresh = true;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = new ZQuery();

			filter.AddToFilter(EDIMessageSchema.EM_LinkTable, JobShipmentSchema.Constants.TableName);
			filter.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, Parent.Shipment.PK);
			filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.SingaporeCMD);
			filter.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessage.ApplicationCodes.SingaporeCMD);

			return filter;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			CMDEDIMessage message = (CMDEDIMessage)child;
			message.EM_LinkedObject = Parent.Shipment;
		}

		public readonly CMDShipmentWrapper Parent;
	}
}
