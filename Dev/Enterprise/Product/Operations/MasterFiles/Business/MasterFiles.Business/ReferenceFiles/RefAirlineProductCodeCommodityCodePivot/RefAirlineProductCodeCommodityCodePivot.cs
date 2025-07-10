using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class RefAirlineProductCodeCommodityCodePivot : AutoRefAirlineProductCodeCommodityCodePivot
	{
		public RefAirlineProductCodeCommodityCodePivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region related business objects
		public RefAirlineProductCode ProductCode
		{
			get { return Factory.Load<RefAirlineProductCode>(RPC_RAR); }
		}

		public RefAirlineCommodityCode CommodityCode
		{
			get { return Factory.Load<RefAirlineCommodityCode>(RPC_RAC); }
		}

		#endregion

		#region Properties

		[RelatedBusinessObject("ProductCode")]
		public override ZGuid RPC_RAR
		{
			get { return base.RPC_RAR; }
			set
			{
				if (value != base.RPC_RAR)
				{
					base.RPC_RAR = value;
				}
			}
		}

		[RelatedBusinessObject("CommodityCode")]
		public override ZGuid RPC_RAC
		{
			get { return base.RPC_RAC; }
			set
			{
				if (value != base.RPC_RAC)
				{
					base.RPC_RAC = value;
				}
			}
		}

		#endregion
	}
}
