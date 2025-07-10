using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.TransportCommon.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StaffDriverCollection))]
	sealed class StaffDriverCollectionTest : ActiveBusinessObjectCollectionTestCase<StaffDriverCollection>
	{
		public void TestFilterBusinessObjectDefaults()
		{
			StaffDriverCollection col = new StaffDriverCollection(Factory);
			AssertEquals(Env.CurrentBranch.Code, col.FilterBusinessObjectDefaults["Driver Branch:Property"].Value);
		}

		public void TestIsDriverRelationshipFilter()
		{
			GlbGroup driverGroup = Factory.NewWithValidTestData<GlbGroup>();

			GlbStaff driverStaff = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff nonDriverStaff = Factory.NewWithValidTestData<GlbStaff>();

			driverStaff.Groups.Add(driverGroup);

			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, driverGroup.PK.ToGuid());

			Factory.Save();

			StaffDriverCollection drivers = new StaffDriverCollection(Factory);
			AssertCollectionContains(driverStaff, drivers);
			AssertCollectionNotContains(nonDriverStaff, drivers);
		}

		protected override StaffDriverCollection GetCollectionToTest()
		{
			return new StaffDriverCollection(Factory);
		}

		public override void TestAdd()
		{
			AssertEquals(1, new StaffDriverCollection(Factory).Count);
		}

		public override void TestAddNew()
		{
			StaffDriverCollection drivers = new StaffDriverCollection(Factory);
			drivers.AddNew();
			AssertEquals(1, drivers.Count);
		}

		public override void TestDelete()
		{
			StaffDriverCollection drivers = new StaffDriverCollection(Factory);
			AssertEquals(1, drivers.Count);
			drivers.Delete(driverStaff);
			AssertEquals(0, drivers.Count);
		}

		public override void TestRemoveFromRelationship()
		{
			TestDelete();
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(StaffDriverCollection);
		}

		protected override void SetUp()
		{
			base.SetUp();

			driverGroup = Factory.NewWithValidTestData<GlbGroup>();
			driverStaff = Factory.NewWithValidTestData<GlbStaff>();
			driverStaff.GS_LoginName = "fcuk";
			driverStaff.GS_Code = "cod";

			driverStaff.Groups.Add(driverGroup);

			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, driverGroup.PK.ToGuid());

			Factory.Save();
		}

		GlbGroup driverGroup;
		GlbStaff driverStaff;
	}
}
