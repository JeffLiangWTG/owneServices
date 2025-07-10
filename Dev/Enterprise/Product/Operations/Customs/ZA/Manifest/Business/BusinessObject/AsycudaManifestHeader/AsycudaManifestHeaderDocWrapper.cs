namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class AsycudaManifestHeaderDocWrapper : ASYCUDA.Business.AsycudaManifestHeaderDocWrapper
	{
		public AsycudaManifestHeaderDocWrapper(AsycudaManifestHeader manifestHeader)
			: base(manifestHeader)
		{
		}

		public new AsycudaManifestHeader Manifest => (AsycudaManifestHeader)base.Manifest;

		public ActiveABLEntryNumCollection ZAEntryNumbers
		{
			get
			{
				var result = new ActiveABLEntryNumCollection(Manifest, Core.Constants.CountryCodes.SouthAfrica);
				result.Load();
				return result;
			}
		}
	}
}
