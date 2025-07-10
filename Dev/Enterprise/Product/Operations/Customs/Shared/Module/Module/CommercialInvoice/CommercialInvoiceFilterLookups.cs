using CargoWise.EntityFramework;

using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module
{
	public class CommercialInvoiceFilterLookups
	{
		public CommercialInvoiceFilterLookups(CommercialInvoiceFilterBusinessObject filterBizObj)
		{
			this.filterBizObj = filterBizObj;
		}

		protected readonly CommercialInvoiceFilterBusinessObject filterBizObj;

		protected BusinessObjectFactory Factory
		{
			get { return filterBizObj.Factory; }
		}

		#region Findbox Lists

		public ConsignorCollection Suppliers
		{
			get { return new ConsignorCollection(Factory); }
		}

		public ConsigneeCollection Importers
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public GlbBranchCollection BranchList
		{
			get
			{
				if (fGlbBranchCollection == null)
				{
					fGlbBranchCollection = new GlbBranchCollection(Factory);
				}
				return fGlbBranchCollection;
			}
		}
		GlbBranchCollection fGlbBranchCollection;

		public OrgHeaderCollection ShippingLines
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public LocationCollection LocationList
		{
			get
			{
				if (fLocationList == null)
				{
					fLocationList = new LocationCollection(Factory);
				}
				return fLocationList;
			}
		}
		LocationCollection fLocationList;

		public RefVesselCollection VesselList
		{
			get
			{
				if (fVesselList == null)
				{
					fVesselList = new RefVesselCollection(Factory);
				}
				return fVesselList;
			}
		}
		RefVesselCollection fVesselList;

		#endregion

		public virtual CodeDescriptionPairList MessageTypes
		{
			get { return JobMessageTypeList.GetListWithAdvanceShippingNotice(Factory); }
		}
	}
}
