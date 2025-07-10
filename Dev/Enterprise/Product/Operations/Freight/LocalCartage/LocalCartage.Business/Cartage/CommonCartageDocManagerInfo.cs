using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonCartageDocManagerInfo : DocManagerInfo
	{
		public CommonCartageDocManagerInfo(CommonCartage cartage, ZString docManagerCode)
			: base(cartage, docManagerCode)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = new List<BusinessObject>();
			var cartage = (CommonCartage)BusinessEntity;

			result.AddRange(cartage.Containers.ToArray());
			result.AddRange(cartage.CartageLegs.ToArray());
			result.AddRange(Transactions(cartage));

			foreach (JobDocAddress address in cartage.DocAddresses)
			{
				var organisation = address.Organisation;
				if (organisation != null)
				{
					result.Add(organisation);
				}
			}

			var parent = cartage.CartageParent; // tested in ICartageParentTestCase.TestRelatedLocalTransportObjectsWithEDocs
			if (parent != null)
			{
				var parentWithEdocs = parent.BusinessObjectForRelatedEDocs as BusinessObject;
				if (parentWithEdocs != null)
				{
					result.Add(parentWithEdocs);
				}
			}

			var parentBooking = cartage.ParentBooking;
			if (parentBooking != null)
			{
				var parentAsBizO = parentBooking as BusinessObject;
				if (parentAsBizO != null)
				{
					result.Add(parentAsBizO);
					var bookingRelatedJobBOs = parentBooking.RelatedJobs.OfType<BusinessObject>().Where(bo => bo.PK != cartage.PK);
					result.AddRange(bookingRelatedJobBOs);
				}
			}

			return result.ToArray();
		}

		AccTransactionHeaderCollection Transactions(CommonCartage cartage)
		{
			return new InvoiceLoader(BusinessEntity.Factory).GetInvoicesForUniqueRef(cartage.JJ_ConsignmentID);
		}
	}
}
