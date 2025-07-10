namespace Enterprise.Customs.US.Business
{
	public interface IAutoQueryFilter
	{
		bool Filter(ICargoManifestStatusQueryData objectForQuery);

		bool RequestForRelatedBOL { get; }
	}
}
