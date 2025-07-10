using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgBuyerSupplierLinkPackPivotCollection : ActiveBusinessObjectCollection<OrgBuyerSupplierLinkPackPivot>
	{
		internal readonly OrgSupplierBuyerLink orgSupplierBuyerLink;

		public OrgBuyerSupplierLinkPackPivotCollection(OrgSupplierBuyerLink orgSupplierBuyerLink)
			: base(orgSupplierBuyerLink)
		{
			this.orgSupplierBuyerLink = orgSupplierBuyerLink;
		}

		public OrgBuyerSupplierLinkPackPivot GetDetailsForPackType(string packTypeCode)
		{
			if (String.IsNullOrEmpty(packTypeCode))
			{
				return null;
			}

			RefPackType packType = Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, packTypeCode));

			if (packType == null)
			{
				return null;
			}

			var query = new ZQuery();
			query.AddToFilter(OrgBuyerSupplierLinkPackPivotSchema.Q0_OL, orgSupplierBuyerLink.PK);
			query.AddToFilter(OrgBuyerSupplierLinkPackPivotSchema.Q0_F3, packType.PK);

			return Factory.LoadTop1<OrgBuyerSupplierLinkPackPivot>(query);
		}
	}
}
