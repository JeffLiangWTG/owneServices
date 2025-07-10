using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	partial class PackageCollection : Customs.Business.BasePackageCollection
	{
		public new PackingGroup PackingGroup
		{
			get { return (PackingGroup)base.PackingGroup; }
		}

		public new Package this[int index]
		{
			get { return (Package)base[index]; }
		}

		public new Package AddNew()
		{
			return (Package)base.AddNew();
		}

		public new Package GetPackageWithPackTypeAndCount(ZString packType, ZInt packCount)
		{
			return (Package)base.GetPackageWithPackTypeAndCount(packType, packCount);
		}

		public new Package GetPackageWithPackTypeAndCount(ZString packType, ZInt packCount, bool shouldGetEmptyPackageIfNullResult)
		{
			return (Package)base.GetPackageWithPackTypeAndCount(packType, packCount, shouldGetEmptyPackageIfNullResult);
		}
	}
}
