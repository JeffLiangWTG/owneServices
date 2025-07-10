using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class WarehouseCustomsAttributeAddInfo : MultiLineAddInfos.CusAddInfo, IWarehouseCustomsAttributeAddInfo
	{
		public WarehouseCustomsAttributeAddInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[BusinessObjectTestExclude]
		public override ZString B7_ParentTableCode
		{
			get { return base.B7_ParentTableCode; }
			set
			{
				base.B7_ParentTableCode = value;
				if (B7_ParentTableCode != WhsBondedWarehouseAttributeSchema.Constants.Prefix)
				{
					ErrorReporter.ReportOnce(System.FormattableString.Invariant($"Table Code must only be 'WB' but instead it is '{B7_ParentTableCode}'."));
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			B7_ParentTableCode = WhsBondedWarehouseAttributeSchema.Constants.Prefix;
		}
	}
}
