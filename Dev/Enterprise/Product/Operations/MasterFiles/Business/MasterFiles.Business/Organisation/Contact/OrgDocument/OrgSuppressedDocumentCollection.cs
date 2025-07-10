using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSuppressedDocumentCollection : DependentBusinessObjectCollection<OrgDocument, OrgHeader>
	{
		public OrgSuppressedDocumentCollection(OrgHeader orgHeader) : base(orgHeader)
		{
		}

		public OrgSuppressedDocumentCollection(OrgHeader orgHeader, BusinessObjectFactory factory) : base(orgHeader, factory)
		{
		}

		public OrgSuppressedDocumentCollection(OrgHeader orgHeader, ZQuery filter) : base(orgHeader, filter)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var doc = (OrgDocument)child;
			doc.OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;
		}

		protected override string FkColumnName => OrgDocumentSchema.Constants.OD_OH_Suppressed;
	}
}
