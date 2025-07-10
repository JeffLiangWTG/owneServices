namespace Enterprise.Customs.US.Business
{
	class OceanRailTruckBillQueryFilter : IAutoQueryFilter
	{
		public bool RequestForRelatedBOL => true;

		public bool Filter(ICargoManifestStatusQueryData objectForQuery)
		{
			return (objectForQuery as Bill)?.IsMasterBill ?? false;
		}
	}
}
