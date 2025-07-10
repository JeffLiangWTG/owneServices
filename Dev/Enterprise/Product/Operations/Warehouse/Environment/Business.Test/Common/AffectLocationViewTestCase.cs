using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public abstract class AffectLocationStringTestCase : AffectLocationViewTestCase
	{
		#region TestReloadLocationForAffectorsCore

		protected override void TestReloadLocationForAffectorsCore()
		{
			var parent = GetNewParent();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var parentInNewFactory = (IAffectLocationView)newFactory.Load(TestedTypeHelper.GetTestedType(GetType()), parent.PK);
			var locations = GetLocations(parentInNewFactory);
			var locationStrings = locations.Select(l => l.WLV_LocationString).ToArray();

			ModifyColumnsThatAffectionLocationView(parentInNewFactory);
			newFactory.Save();

			var newLocationStrings = locations.Select(l => l.WLV_LocationString).ToArray();

			AssertEquals("Location Strings should be modified.", locationStrings.Length, newLocationStrings.Except(locationStrings).Count());
		}

		protected abstract void ModifyColumnsThatAffectionLocationView(IAffectLocationView parent);

		#endregion
	}

	[TestsSubclassesOf(typeof(IAffectLocationView))]
	public abstract class AffectLocationViewTestCase : TestCaseWithFactory
	{
		#region TestReloadLocationForAffectors

		public void TestReloadLocationForAffectors()
		{
			TestReloadLocationForAffectorsCore();
		}

		#endregion

		#region TestGetColumnsThatAffectLocationView

		public void TestGetColumnsThatAffectLocationView()
		{
			AssertContainsExactElementsInAnyOrder(ExpectedColumnsThatAffectLocationView, GetNewParent().GetColumnsThatAffectLocationView());
		}

		#endregion

		#region Implementation

		protected abstract IAffectLocationView GetNewParent();

		protected abstract IEnumerable<WhsLocation> GetLocations(IAffectLocationView parent);

		protected abstract void TestReloadLocationForAffectorsCore();

		protected abstract IEnumerable<SchemaColumn> ExpectedColumnsThatAffectLocationView { get; }

		#endregion
	}
}
