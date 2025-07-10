using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;

namespace Enterprise.Customs.Business
{
	public class CusPackageLookups : PkgPackageLookups
	{
		public CusPackageLookups(CusPackage parent)
			: base(parent)
		{
		}

		new CusPackage Parent => (CusPackage)base.Parent;

		#region PackTypes

		public override RefPackTypeCollection PackTypes => GetPackTypes(Factory, Parent.PackageJob);

		#endregion
	}
}
