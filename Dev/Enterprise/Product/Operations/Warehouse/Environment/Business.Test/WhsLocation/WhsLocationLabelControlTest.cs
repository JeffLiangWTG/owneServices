using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsLocationLabelList))]
	public class WhsLocationLabelControlTest : NonPersistentBusinessObjectTestCase
	{
		#region Test Cases

		public void TestConstructors()
		{
			LocationLabelControl = new WhsLocationLabelList(Factory);
			AssertNotNull(LocationLabelControl.Locations);
			AssertEquals(0, LocationLabelControl.Locations.Count);

			WhsLocation location = Factory.New<WhsLocation>();
			LocationLabelControl = new WhsLocationLabelList(new List<WhsLocation>(), Factory);
			LocationLabelControl.Locations.Add(location);
			AssertNotNull(LocationLabelControl.Locations);
			AssertEquals(1, LocationLabelControl.Locations.Count);
			AssertEquals(location, LocationLabelControl.Locations[0]);
		}

		public void TestLocations()
		{
			WhsLocation location1 = Factory.New<WhsLocation>();
			LocationLabelControl = new WhsLocationLabelList(new List<WhsLocation>(), Factory);
			LocationLabelControl.Locations.Add(location1);
			AssertEquals(1, LocationLabelControl.Locations.Count);
			AssertEquals(location1, LocationLabelControl.Locations[0]);

			WhsLocation location2 = Factory.New<WhsLocation>();
			LocationLabelControl.Locations.Add(location2);
			AssertEquals(2, LocationLabelControl.Locations.Count);
			AssertEquals(location1, LocationLabelControl.Locations[0]);
			AssertEquals(location2, LocationLabelControl.Locations[1]);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WhsLocationLabelList(new List<WhsLocation>(), Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			LocationLabelControl = new WhsLocationLabelList(Factory);
		}

		WhsLocationLabelList LocationLabelControl;

		#endregion
	}
}
