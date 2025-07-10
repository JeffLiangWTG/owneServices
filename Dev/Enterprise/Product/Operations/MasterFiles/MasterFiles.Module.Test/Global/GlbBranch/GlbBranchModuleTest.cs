using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbBranchModule))]
	internal class GlbBranchModuleTest : ZArchitecture.Modules.Testing.ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlbBranch;
		}

		[RequiresSTA]
		public void TestFilterControl()
		{
			branch = new GlbBranchModuleForTest();
			IFilterControl controlForTest = branch.GetNewFilterControlForTest();
			Assert(controlForTest is GlbBranchFilterControl);
			controlForTest.Dispose();
			branch.Dispose();
		}

		public void TestGridCollection()
		{
			branch = new GlbBranchModuleForTest();
			Assert(branch.GetNewGridCollectionForTest() is BusinessObjectCollection);
			branch.Dispose();
		}

		public void TestFilterBusinessObject()
		{
			branch = new GlbBranchModuleForTest();
			Assert(branch.GetNewFilterBusinessObjectForTest() is FilterBusinessObject);
			branch.Dispose();
		}

		[RequiresSTA]
		public void TestControlResourceString()
		{
			using (branch = new GlbBranchModuleForTest())
			using (var filterControl = branch.GetNewFilterControlForTest() as GlbBranchFilterControl)
			{
				AssertNotNull(filterControl);

				StringBuilder result = new StringBuilder();
				foreach (var columnStyle in filterControl.FilteredGrid.ColumnStyles)
				{
					var resCaptionedControl = columnStyle as IResCaptionedControl;
					if (resCaptionedControl != null)
					{
						var zGridColumn = resCaptionedControl as ZGridColumnInfo;
						if (zGridColumn != null)
						{
							var resourceStringData = new ResourceStringKeyCalculator(filterControl.FilteredGrid, zGridColumn.ColumnName).DataString;
							if (resourceStringData != null && !resourceStringData.IsEmpty())
							{
								var captions = resourceStringData.GetCaptions();
								var controlResoureString = resCaptionedControl.CaptionResourceString.Caption;
								var definedResourceString = string.Join(",", captions);
								if (!(definedResourceString + ",").Contains(controlResoureString + ","))
								{
									result.AppendLine(string.Format(@"Control has the resource string set to '{0}', but it has different caption(s) '{1}' defined in the resource xml.", controlResoureString, definedResourceString));
								}
							}
						}
					}
				}
				AssertEquals("", result.ToString());
			}
		}

		#region Implementation

		GlbBranchModuleForTest branch;

		#endregion
	}
}
