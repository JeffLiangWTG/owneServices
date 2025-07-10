using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class OrgHeaderWrapperMessageCollection : EDIMessageCollection
	{
		public OrgHeaderWrapperMessageCollection(OrgHeader master)
			: base(master)
		{
		}

		public new OrgHeader Master
		{
			get { return (OrgHeader)base.Master; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = null;
			if (Master == null)
			{
				result = ZQuery.NoResultQuery;
			}
			else
			{
				var pks = Master.Addresses.GetPKs();
				pks.Insert(0, Master.PK);
				result = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, pks.ToArray());
				result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.USCustomsImport);
				result.FetchOnlyFromLocalCache = !Master.IsInDatabase;
			}
			return result;
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			var message = child as EDIMessage;
			if (message != null)
			{
				var address = message.EM_LinkedObject as OrgAddress;
				if (address != null && address.OA_OH == Master.PK)
				{
					return;
				}
			}
			base.SetCollectionRelationships(child);
		}
	}
}
