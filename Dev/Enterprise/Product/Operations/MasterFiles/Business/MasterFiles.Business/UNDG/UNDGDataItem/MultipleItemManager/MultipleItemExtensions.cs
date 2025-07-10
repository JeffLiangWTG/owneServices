using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.MultipleItemExtensions
{
	public static class MultipleItemExtensions
	{
		public static bool ItemIsEmpty(this BusinessObject @this)
		{
			foreach (ZPropertyInfo info in @this.ZPropertyInfoHash)
			{
				if (!info.Name.Contains("PK")
					&& !info.Name.Contains("ParentID")
					&& !info.Name.Contains("ParentTableCode")
					&& !info.Value.IsDefault)
				{
					return false;
				}
			}

			return true;
		}
	}
}
