namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusDV1DetailCollection : EU.Business.Declaration.CusDV1DetailCollection
	{
		public CusDV1DetailCollection(JobDeclaration parent) : base(parent)
		{
		}

		public new CusDV1Detail this[int index] => (CusDV1Detail)base[index];
		public new CusDV1Detail AddNew() => (CusDV1Detail)base.AddNew();
	}
}
