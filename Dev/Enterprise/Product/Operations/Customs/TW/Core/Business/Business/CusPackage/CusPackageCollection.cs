namespace Enterprise.Customs.TW.Business
{
	public class CusPackageCollection : Customs.Business.CusPackageCollection
	{
		public CusPackageCollection(CusPackageJob master)
			: base(master)
		{
		}

		public CusPackageCollection(CusPackage master)
			: base(master)
		{
		}

		public new CusPackage AddNew() => (CusPackage)base.AddNew();

		public new CusPackage this[int index] => (CusPackage)base[index];
	}
}
