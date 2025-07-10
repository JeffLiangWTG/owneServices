using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsDynamicPickFaceView : AutoWhsDynamicPickFaceView
	{
		public WhsDynamicPickFaceView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool CanDelete => false;

		public override void Delete()
		{
			throw new NotSupportedException("You cannot delete from dynamic pick face view.");
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new WhsDynamicPickFaceViewFetchStrategy(this);

		public WhsLocation Location => Factory.Load<WhsLocation>(WDP_WL_Location);

		protected override ZString HumanReadableNameCore => Res.GetString("A8B2A1E6-BD33-45AC-AEF7-2FFE4D98DD0D", "Dynamic Pick Face");
	}
}
