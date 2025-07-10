using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	abstract class OrgCodeMappingBaseModuleFilterTest : ModuleTextFilterTest
	{
		#region Empty

		public abstract void TestIsEmpty();

		#endregion

		#region Clear

		public abstract void TestClear();

		#endregion

		#region Serialization

		public abstract void TestSerialization();

		public abstract void TestDeserialization();

		#endregion
	}
}
