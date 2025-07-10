using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgDocumentCollection : DependentBusinessObjectCollection<OrgDocument, OrgContact>
	{
		public OrgDocumentCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public OrgDocumentCollection(OrgContact parent, BusinessObjectFactory factory) : base(parent, factory)
		{
		}

		#region Contains Document Group / Document

		public ZBool ContainsDefault(ZString docGroup)
		{
			foreach (OrgDocument doc in this)
			{
				if (doc.OD_DocumentGroup == docGroup && doc.OD_DefaultContact)
				{
					return true;
				}
			}
			return false;
		}

		public ZBool ContainsDocumentWithDeliveryMode(ZString deliveryMode)
		{
			foreach (OrgDocument document in this)
			{
				if (document.OD_DeliverBy == deliveryMode)
				{
					return true;
				}
			}
			return false;
		}

		public ZBool ContainsDocGroup(ZString docGroup)
		{
			foreach (OrgDocument doc in this)
			{
				if (doc.OD_DocumentGroup == docGroup)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgDocument doc = (OrgDocument)child;
			doc.OD_DeliverBy = Master.OC_NotifyMode;
			doc.OD_AttachmentType = Master.OC_AttachmentType;
		}

		#endregion
	}
}
