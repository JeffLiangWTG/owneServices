using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ZCodeFindBoxWithCodeTypeTest : ZControlBaseTestCase<ZDropEditCodeFindBox>
	{
		#region IDataBoundControl Members

		public void TestDataSourceType()
		{
			AssertEquals("DataSourceType required so that BindingSource.GetBindingMembersForCompileTimeCheck serialized", typeof(object), Control.DataSourceType);
		}

		#endregion

		#region Implementation

		protected override bool UsesControlDataBindings
		{
			get { return false; }
		}

		#endregion
	}
}
