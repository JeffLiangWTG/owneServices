using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class OrgContactStmALogFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestFindButtonIsDisabled()
		{
			using (var control = new OrgContactLogFilterControlForTest(new StmALogCollection(Factory), new OrgContactStmALogFilterBusinessObject()))
			{
				AssertEquals(false, control.FindButton_Exposed.Enabled);
				AssertEquals(false, control.ShouldPerformSearch_Exposed);
			}
		}

		class OrgContactLogFilterControlForTest : OrgContactStmALogFilterControl
		{
			public OrgContactLogFilterControlForTest(StmALogCollection collection, OrgContactStmALogFilterBusinessObject filterBusinessObject) : base(collection, filterBusinessObject)
			{
			}

			public ZToolStripSplitButton FindButton_Exposed
			{
				get
				{
					return base.ToolStripFindDropButton;
				}
			}

			public bool ShouldPerformSearch_Exposed
			{
				get
				{
					return ShouldPerformSearch();
				}
			}
		}
	}
}
