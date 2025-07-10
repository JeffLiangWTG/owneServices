namespace Enterprise.Freight.Business
{
	using CargoWise.Types;
	using Enterprise.ZArchitecture;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Schema;

	internal sealed class VoyageDependentBizObjFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public VoyageDependentBizObjFetchStrategy(EnterpriseBusinessObject businessObject, ZGuid parentVoyageZGuid)
			: base(businessObject)
		{
			this.parentVoyageZGuid = parentVoyageZGuid;
		}

		readonly ZGuid parentVoyageZGuid;

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			Factory.AddFetchHint(JobVoyageSchema.Constants.TableName, parentVoyageZGuid);
		}
	}
}
