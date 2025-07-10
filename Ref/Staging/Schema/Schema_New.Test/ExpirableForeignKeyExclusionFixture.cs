using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Common.TypeProvider;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test
{
	[TestFixture]
	class ExpirableForeignKeyExclusionFixture
	{
		[Test]
		public void Test_Forbidden_ParentTableIsNonExpirable_ChildTableIsExpirable()
		{
			var types = Assembly.LoadFrom("CargoWise.RefDbRepo.Staging.Schema_New.dll").GetTypes();
			var childTypes = types.Where(type => type.IsExpirableType());
			Assert.Multiple(() =>
			{
				foreach (var childType in childTypes)
				{
					var parentTypes = childType.GetProperties().Where(p => p.IsDefined(typeof(ForeignKeyAttribute)))
						.Select(p => p.PropertyType);
					foreach (var parentType in parentTypes)
					{
						if (ExpirableForeignKeyExclusion.ContainsPair(parentType, childType))
						{
							continue;
						}

						Assert.That(parentType.IsExpirableType(), Is.True, $@"
Parent table {parentType.Name} is non-expirable, but child table {childType.Name} is expirable.
This behaviour is normally forbidden. If you're sure right, add them to {nameof(ExpirableForeignKeyExclusion)}");
					}
				}
			});
		}
	}
}
