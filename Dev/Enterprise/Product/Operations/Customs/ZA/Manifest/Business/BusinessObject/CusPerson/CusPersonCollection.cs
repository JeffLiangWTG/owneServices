namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class CusPersonCollection : ASYCUDA.Business.CusPersonCollection<CusPerson, AsycudaManifestHeader>
	{
		public CusPersonCollection(AsycudaManifestHeader master)
			: base(master)
		{
		}
	}
}
