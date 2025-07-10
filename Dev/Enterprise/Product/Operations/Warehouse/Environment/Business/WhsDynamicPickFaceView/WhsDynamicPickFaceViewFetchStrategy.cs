using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	class WhsDynamicPickFaceViewFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WhsDynamicPickFaceViewFetchStrategy(WhsDynamicPickFaceView pickFace)
			: base(pickFace)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			// We are using Fetch For Load as this view is created for use only in the module.
			// The module grid requires sorting by location and sorting occurs before any available fetch hooks.
			Factory.AddFetchHint(WhsLocationViewSchema.PK, ((WhsDynamicPickFaceView)BusinessObject).WDP_WL_Location);
		}
	}
}
