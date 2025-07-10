namespace Enterprise.Customs.SG.Access.Business
{
	public class CusPersonCollection : ASYCUDA.Business.CusPersonCollection<CusPerson, AsycudaManifestHeader>
	{
		public CusPersonCollection(AsycudaManifestHeader master)
			: base(master)
		{
		}
	}
}
