namespace Enterprise.Customs.US.Business
{
	partial class ReleaseOriginCodeList
	{
		public static bool ShouldRemoveReleaseDate(string releaseOrigin)
		{
			return releaseOrigin == Codes.ReleaseDateRemoved;
		}
	}
}
