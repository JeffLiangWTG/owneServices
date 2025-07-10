namespace Enterprise.Customs.Business.Testing
{
	static class BasePackageExtensions
	{
		public static BasePackage AddChild(this BasePackage package)
		{
			var childPackage = package.Declaration.Packages.AddNew();
			childPackage.CW_CW_Parent = package.PK;
			childPackage.CW_CR_HouseContainer = package.CW_CR_HouseContainer;
			childPackage.CW_PackQty = 1;
			return childPackage;
		}
	}
}
