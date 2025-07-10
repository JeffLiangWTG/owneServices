using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.Business
{
	public class WarehouseCustomsAddInfo : CusAddInfo, IWarehouseCustomsAddInfo
	{
		public WarehouseCustomsAddInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[BusinessObjectTestExclude]
		public override ZString B7_Type
		{
			get { return base.B7_Type; }
			set { base.B7_Type = value; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			B7_Type = CusAddInfoTypeAttribute.Codes.WarehouseCustomsAddInfo;
		}
	}
}
