using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsPackageValidation : NctsPackagePhase4Validation
	{
		public NctsPackageValidation(NctsPackage parent)
			: base(parent)
		{
		}

		public new NctsPackage Parent => (NctsPackage)base.Parent;

		protected override void CheckB5_UnitCount()
		{
			base.CheckB5_UnitCount();

			var parent = Parent;
			var nctsHeader = (NctsHeader)(parent.Parent?.Header);

			ZLong quantity = 0;
			foreach (var item in nctsHeader.MovementHeader.GoodsItems)
			{
				quantity += item.Packages.Cast<NctsPackage>().Aggregate(0, (ZLong current, NctsPackage package) => current + ((!package.IsBulk) ? package.B5_UnitCount : ((package.B5_UnitCount == 0) ? ((ZLong)1) : package.B5_UnitCount)));
			}

			if (quantity > int.MaxValue || quantity < int.MinValue)
			{
				Parent.B5_UnitCountInfo.AddError(ZString.Format((NoResString)"The maximum quantity should be {0}.", int.MaxValue));
			}
		}
	}
}
