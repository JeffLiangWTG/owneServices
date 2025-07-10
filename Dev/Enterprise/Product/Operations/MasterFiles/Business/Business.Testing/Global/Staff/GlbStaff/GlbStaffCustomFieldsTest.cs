using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaff))]
	sealed class GlbStaffCustomFieldsTest : TestICustomFieldProvider
	{
		protected override BusinessObject GetBizo() => Factory.New<GlbStaff>();
	}
}
