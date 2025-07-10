using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbGroup))]
	sealed class GlbGroupCustomFieldsTest : TestICustomFieldProvider
	{
		protected override BusinessObject GetBizo() => Factory.New<GlbGroup>();
	}
}
