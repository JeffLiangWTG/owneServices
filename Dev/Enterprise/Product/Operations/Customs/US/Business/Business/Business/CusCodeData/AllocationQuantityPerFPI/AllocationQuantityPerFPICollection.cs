using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class AllocationQuantityPerFPICollection : Customs.Business.CusCodeDataCollection<AllocationQuantityPerFPI>
	{
		public AllocationQuantityPerFPICollection(OrgCountryData master)
			: base(master, CusCodeDataTypeList.Codes.AllocationQuantityPerFPI)
		{
		}

		new OrgCountryData Master => (OrgCountryData)base.Master;

		protected override bool AllowNewCore => base.AllowNewCore && Master.OrgHeader.OH_IsConsignee;
	}
}
