using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefTimeZoneSetFilterBusinessObject))]
	sealed class RefTimeZoneSetFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public ModuleTextFilter GetModuleTextFilter(ZString description)
		{
			return (ModuleTextFilter)(RefTimeZoneSetFilterBusinessObject[description]);
		}

		public ModuleTextFilter GetBaseFilter(ZString description)
		{
			return (ModuleTextFilter)(refTimeZoneSetFilterBusinessObject.ModuleFilters[description]);
		}

		public RefTimeZoneSetFilterBusinessObject RefTimeZoneSetFilterBusinessObject
		{
			get
			{
				if (refTimeZoneSetFilterBusinessObject == null)
				{
					refTimeZoneSetFilterBusinessObject = (RefTimeZoneSetFilterBusinessObject)GetNewFilterStripBusinessObject();
				}
				return refTimeZoneSetFilterBusinessObject;
			}
		}
		RefTimeZoneSetFilterBusinessObject refTimeZoneSetFilterBusinessObject;

		public void TestStandardZoneCode()
		{
			ModuleTextFilter filter = GetModuleTextFilter("Standard Zone Code");
			filter.Property = "1234";
			filter.IsActive = true;

			RefTimeZoneSetCollection collection = new RefTimeZoneSetCollection(Factory, RefTimeZoneSetFilterBusinessObject.Filter);

			AssertCollectionContains(timeZoneSet1, collection);
			AssertCollectionNotContains(timeZoneSet2, collection);
			AssertCollectionNotContains(timeZoneSet3, collection);

			filter = GetModuleTextFilter("Standard Zone Code");
			filter.Property = "1";
			filter.IsActive = true;

			collection = new RefTimeZoneSetCollection(Factory, RefTimeZoneSetFilterBusinessObject.Filter);

			AssertCollectionContains(timeZoneSet1, collection);
			AssertCollectionNotContains(timeZoneSet2, collection);
			AssertCollectionContains(timeZoneSet3, collection);
			filter.IsActive = false;

			filter = GetModuleTextFilter("Standard Zone Code");
			filter.Property = "44";
			filter.IsActive = true;

			collection = new RefTimeZoneSetCollection(Factory, RefTimeZoneSetFilterBusinessObject.Filter);

			AssertCollectionNotContains(timeZoneSet1, collection);
			AssertCollectionContains(timeZoneSet2, collection);
			AssertCollectionNotContains(timeZoneSet3, collection);
		}

		public void TestStandardZoneName()
		{
			ModuleTextFilter filter = GetModuleTextFilter("Standard Zone Name");
			filter.Property = "Zone1234";
			filter.IsActive = true;

			RefTimeZoneSetCollection collection = new RefTimeZoneSetCollection(Factory, RefTimeZoneSetFilterBusinessObject.Filter);

			AssertCollectionContains(timeZoneSet1, collection);
			AssertCollectionNotContains(timeZoneSet2, collection);
			AssertCollectionNotContains(timeZoneSet3, collection);
			filter.IsActive = false;

			filter = GetModuleTextFilter("Standard Zone Name");
			filter.Property = "ZZZ";
			filter.IsActive = true;

			collection = new RefTimeZoneSetCollection(Factory, RefTimeZoneSetFilterBusinessObject.Filter);

			AssertCollectionNotContains(timeZoneSet1, collection);
			AssertCollectionNotContains(timeZoneSet2, collection);
			AssertCollectionNotContains(timeZoneSet3, collection);
			filter.IsActive = false;

			filter = GetModuleTextFilter("Standard Zone Name");
			filter.Property = "Zone";
			filter.IsActive = true;

			collection = new RefTimeZoneSetCollection(Factory, RefTimeZoneSetFilterBusinessObject.Filter);

			AssertCollectionContains(timeZoneSet1, collection);
			AssertCollectionContains(timeZoneSet2, collection);
			AssertCollectionNotContains(timeZoneSet3, collection);
		}

		public void TestDaylightSavingZoneCode()
		{
			ModuleTextFilter filter = GetModuleTextFilter("Daylight Saving Zone Code");
			filter.Property = "5691";
			filter.IsActive = true;

			RefTimeZoneSetCollection collection = new RefTimeZoneSetCollection(Factory, RefTimeZoneSetFilterBusinessObject.Filter);

			AssertCollectionNotContains(timeZoneSet1, collection);
			AssertCollectionContains(timeZoneSet2, collection);
			AssertCollectionNotContains(timeZoneSet3, collection);
			filter.IsActive = false;

			filter = GetModuleTextFilter("Daylight Saving Zone Code");
			filter.Property = "5";
			filter.IsActive = true;

			collection = new RefTimeZoneSetCollection(Factory, RefTimeZoneSetFilterBusinessObject.Filter);

			AssertCollectionContains(timeZoneSet1, collection);
			AssertCollectionContains(timeZoneSet2, collection);
			AssertCollectionNotContains(timeZoneSet3, collection);
			filter.IsActive = false;

			filter = GetModuleTextFilter("Daylight Saving Zone Code");
			filter.Property = "99";
			filter.IsActive = true;

			collection = new RefTimeZoneSetCollection(Factory, RefTimeZoneSetFilterBusinessObject.Filter);

			AssertCollectionNotContains(timeZoneSet1, collection);
			AssertCollectionNotContains(timeZoneSet2, collection);
			AssertCollectionNotContains(timeZoneSet3, collection);
		}

		public void TestDaylightSavingZoneName()
		{
			ModuleTextFilter filter = GetModuleTextFilter("Daylight Saving Zone Name");
			filter.Property = "Z";
			filter.IsActive = true;

			RefTimeZoneSetCollection collection = new RefTimeZoneSetCollection(Factory, RefTimeZoneSetFilterBusinessObject.Filter);

			AssertCollectionContains(timeZoneSet1, collection);
			AssertCollectionContains(timeZoneSet2, collection);
			AssertCollectionNotContains(timeZoneSet3, collection);
			filter.IsActive = false;

			filter = GetModuleTextFilter("Daylight Saving Zone Name");
			filter.Property = "The";
			filter.IsActive = true;

			collection = new RefTimeZoneSetCollection(Factory, RefTimeZoneSetFilterBusinessObject.Filter);

			AssertCollectionNotContains(timeZoneSet1, collection);
			AssertCollectionNotContains(timeZoneSet2, collection);
			AssertCollectionContains(timeZoneSet3, collection);
			filter.IsActive = false;

			filter = GetModuleTextFilter("Daylight Saving Zone Name");
			filter.Property = "NN";
			filter.IsActive = true;

			collection = new RefTimeZoneSetCollection(Factory, RefTimeZoneSetFilterBusinessObject.Filter);

			AssertCollectionNotContains(timeZoneSet1, collection);
			AssertCollectionNotContains(timeZoneSet2, collection);
			AssertCollectionNotContains(timeZoneSet3, collection);
		}

		#region Implementation

		RefTimeZone timeZone1;
		RefTimeZone timeZone2;
		RefTimeZone timeZone3;
		RefTimeZone timeZone4;
		RefTimeZone timeZone5;
		RefTimeZone timeZone6;
		RefTimeZoneSet timeZoneSet1;
		RefTimeZoneSet timeZoneSet2;
		RefTimeZoneSet timeZoneSet3;

		protected override void SetUp()
		{
			base.SetUp();

			timeZone1 = Factory.NewWithValidTestData<RefTimeZone>();
			timeZone1.R2_CivilianTimeZoneCode = "1234";
			timeZone1.R2_CivilianTimeZoneFullName = "Zone1234";
			timeZone4 = Factory.NewWithValidTestData<RefTimeZone>();
			timeZone4.R2_CivilianTimeZoneCode = "5432";
			timeZone4.R2_CivilianTimeZoneFullName = "Zone5432";
			timeZoneSet1 = Factory.NewWithValidTestData<RefTimeZoneSet>();
			timeZoneSet1.R3_R2_StandardZone = timeZone1.PK;
			timeZoneSet1.R3_R2_DaylightSavingZone = timeZone4.PK;
			timeZone2 = Factory.NewWithValidTestData<RefTimeZone>();
			timeZone2.R2_CivilianTimeZoneCode = "4456";
			timeZone2.R2_CivilianTimeZoneFullName = "Zone4456";
			timeZone5 = Factory.NewWithValidTestData<RefTimeZone>();
			timeZone5.R2_CivilianTimeZoneCode = "5691";
			timeZone5.R2_CivilianTimeZoneFullName = "ZNN5691";
			timeZoneSet2 = Factory.NewWithValidTestData<RefTimeZoneSet>();
			timeZoneSet2.R3_R2_StandardZone = timeZone2.PK;
			timeZoneSet2.R3_R2_DaylightSavingZone = timeZone5.PK;
			timeZone3 = Factory.NewWithValidTestData<RefTimeZone>();
			timeZone3.R2_CivilianTimeZoneCode = "1712";
			timeZone3.R2_CivilianTimeZoneFullName = "ZZ1712";
			timeZone6 = Factory.NewWithValidTestData<RefTimeZone>();
			timeZone6.R2_CivilianTimeZoneCode = "8890";
			timeZone6.R2_CivilianTimeZoneFullName = "TheZone8890";
			timeZoneSet3 = Factory.NewWithValidTestData<RefTimeZoneSet>();
			timeZoneSet3.R3_R2_StandardZone = timeZone3.PK;
			timeZoneSet3.R3_R2_DaylightSavingZone = timeZone6.PK;

			Factory.Save();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefTimeZoneSetFilterBusinessObject();
		}

		#endregion
	}
}
