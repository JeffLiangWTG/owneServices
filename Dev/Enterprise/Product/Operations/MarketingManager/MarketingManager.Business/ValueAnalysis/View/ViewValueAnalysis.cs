using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	[CodeProperty("VVA_Code")]
	public class ViewValueAnalysis : AutoViewValueAnalysis
	{
		public ViewValueAnalysis(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool CanDelete => false;

		#region Properties
		public ZString VVA_Code { get; set; }

		public ViewLocation Origin => VVA_Origin.IsValid ? Factory.Load<ViewLocation>(VVA_Origin) : null;

		public ViewLocation Destination => VVA_Destination.IsValid ? Factory.Load<ViewLocation>(VVA_Destination) : null;

		public OrgHeader Buyer => VVA_OH_Buyer.IsValid ? Factory.Load<OrgHeader>(VVA_OH_Buyer) : null;

		public OrgHeader Supplier => VVA_OH_Supplier.IsValid ? Factory.Load<OrgHeader>(VVA_OH_Supplier) : null;

		public OrgHeader Primary => VVA_OH_Primary.IsValid ? Factory.Load<OrgHeader>(VVA_OH_Primary) : null;

		public virtual ZString Location
		{
			get
			{
				var location = Origin ?? Destination;

				if (location != null)
				{
					return location.IsUNLOCO ? location.UnlocoCode : location.VLO_CountryCode;
				}
				return VVA_WarehouseLocation;
			}
		}

		#endregion
	}
}
