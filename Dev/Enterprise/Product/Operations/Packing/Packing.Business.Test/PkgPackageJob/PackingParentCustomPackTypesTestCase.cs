using System.Linq;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestsSubclassesOf(typeof(IPackingParentCustomPackTypes))]
	public abstract class PackingParentCustomPackTypesTestCase<T> : PackingParentTestCase<T>
		where T : IPackingParentCustomPackTypes, IPackingParent
	{
		public void TestPackTypesToExcludeHasAtLeastOneElement()
		{
			var parent = GetNewParent();
			AssertEquals(true, parent.PackTypesToExclude.Any() && parent.PackTypesToExclude.All(p => !p.IsEmpty));
		}
	}
}
