using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbTeam))]
	sealed class GlbTeamTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHasCodeAttribute()
		{
			var attibute = typeof(GlbTeam).GetCustomAttribute<CodePropertyAttribute>();
			AssertEquals(attibute != null, true);
			AssertEquals(attibute.PropertyName, nameof(GlbTeam.GST_Code));
		}

		public void TestHasDescriptionAttribute()
		{
			var attibute = typeof(GlbTeam).GetCustomAttribute<DescriptionPropertyAttribute>();
			AssertEquals(attibute != null, true);
			AssertEquals(attibute.PropertyName, nameof(GlbTeam.GST_Name));
		}
	}
}
