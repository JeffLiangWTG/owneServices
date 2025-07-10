using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Packing.Business
{
	public class PkgPackageDocManagerInfo : DocManagerInfo
	{
		public PkgPackageDocManagerInfo(PkgPackage package) : base(package, Constants.DocManagerCodes.Package)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var package = (PkgPackage)BusinessEntity;
			var list = new List<BusinessObject>(base.GetRelatedObjects());
			if (package.GetRelatedBusinessObjects != null)
			{
				list.AddRange(package.GetRelatedBusinessObjects());
			}
			return list.ToArray();
		}
	}
}
