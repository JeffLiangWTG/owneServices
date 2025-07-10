using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class AsycudaBillCollection : ManifestBase.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>
	{
		public AsycudaBillCollection(AsycudaManifestHeader master)
			: base(master, GetChildOnlyFilter())
		{ }

		static ZQuery GetChildOnlyFilter()
		{
			return new ZQuery(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, AsycudaBill.ChildBolCode);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			using (child.SuspendSettingHasChanges())
			{
				if (child is AsycudaBill bill)
				{
					bill.ABL_BolType = AsycudaBill.HouseBillCode;
				}
			}
		}
	}
}
