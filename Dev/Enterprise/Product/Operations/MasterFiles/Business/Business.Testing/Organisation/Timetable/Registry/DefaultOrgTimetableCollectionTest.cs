using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DefaultOrgTimetableCollection))]
	sealed class DefaultOrgTimetableCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DefaultOrgTimetableCollection>
	{
		protected override bool RequiresFactory
		{
			get
			{
				return true;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected override DefaultOrgTimetableCollection GetCollectionToTest()
		{
			return new DefaultOrgTimetableCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DefaultOrgTimetable(Factory);
		}

		public void TestClone_DefaultOrgTimetable()
		{
			FallbackLevel currentFallbackLevel = NewFallbackLevel();

			var item1 = new DefaultOrgTimetable();
			var item2 = new DefaultOrgTimetable();

			item1.Type = OrgTimetableType.Codes.Pickup;
			item1.From = new ZDateTime(2015, 1, 1, 9, 0, 0);
			item1.To = new ZDateTime(2015, 1, 1, 17, 0, 0);
			item1.Day = "MON";

			item2.Type = OrgTimetableType.Codes.Deliver;
			item2.From = new ZDateTime(2015, 1, 1, 9, 0, 0);
			item2.To = new ZDateTime(2015, 1, 1, 17, 0, 0);
			item2.Day = "MON";

			Collection.Add(item1);
			Collection.Add(item2);

			var clone = (DefaultOrgTimetableCollection)Collection.Clone(currentFallbackLevel, Factory);
			BusinessObjectFactory expectedFactory = (RequiresFactory) ? Factory : RegistryFactory.Instance;

			Assert("Clone should be a different instance.", Collection != clone);
			Assert("Clone's elements should be different instances from the original's.", clone[0] != item1);
			Assert("Clone's elements should be different instances from the original's.", clone[1] != item2);
			AssertEquals("Clone.CurrentFactory", expectedFactory, CurrentFactoryPropertyInfo(typeof(DefaultOrgTimetableCollection)).GetValue(clone, null));

			DefaultOrgTimetable cloneElement1 = clone[0];
			DefaultOrgTimetable cloneElement2 = clone[1];

			AssertEquals("Clone[0].Type", OrgTimetableType.Codes.Pickup, cloneElement1.Type);
			AssertEquals("Clone[1].Type", OrgTimetableType.Codes.Deliver, cloneElement2.Type);
		}

		public void TestDefaultOrgTimetableCollectionOrder()
		{
			var defaultOrgTimetableCollection = new DefaultOrgTimetableCollection();

			DefaultOrgTimetable t1 = new DefaultOrgTimetable();
			t1.Type = OrgTimetableType.Codes.Pickup;
			t1.Day = "FRI";
			t1.From = new DateTime(2015, 1, 1, 8, 0, 0);
			t1.To = new DateTime(2015, 1, 1, 9, 0, 0);
			defaultOrgTimetableCollection.Add(t1);

			DefaultOrgTimetable t2 = new DefaultOrgTimetable();
			t2.Type = OrgTimetableType.Codes.Pickup;
			t2.Day = "WED";
			t2.From = new DateTime(2015, 1, 1, 8, 0, 0);
			t2.To = new DateTime(2015, 1, 1, 9, 30, 0);
			defaultOrgTimetableCollection.Add(t2);

			DefaultOrgTimetable t3 = new DefaultOrgTimetable();
			t3.Type = OrgTimetableType.Codes.Pickup;
			t3.Day = "MON";
			t3.From = new DateTime(2015, 1, 1, 9, 0, 0);
			t3.To = new DateTime(2015, 1, 1, 9, 30, 0);
			defaultOrgTimetableCollection.Add(t3);

			DefaultOrgTimetable t4 = new DefaultOrgTimetable();
			t4.Type = OrgTimetableType.Codes.Pickup;
			t4.Day = "MON";
			t4.From = new DateTime(2015, 1, 1, 8, 0, 0);
			t4.To = new DateTime(2015, 1, 1, 8, 30, 0);
			defaultOrgTimetableCollection.Add(t4);

			DefaultOrgTimetable t5 = new DefaultOrgTimetable();
			t5.Type = OrgTimetableType.Codes.Deliver;
			t5.Day = "TUE";
			t5.From = new DateTime(2015, 1, 1, 14, 0, 0);
			t5.To = new DateTime(2015, 1, 1, 14, 30, 0);
			defaultOrgTimetableCollection.Add(t5);

			IComparer comparer = new DefaultOrgTimetableComparer();
			defaultOrgTimetableCollection.Sort(comparer);

			AssertEquals(defaultOrgTimetableCollection[0], t4);
			AssertEquals(defaultOrgTimetableCollection[1], t3);
			AssertEquals(defaultOrgTimetableCollection[2], t5);
			AssertEquals(defaultOrgTimetableCollection[3], t2);
			AssertEquals(defaultOrgTimetableCollection[4], t1);
		}
	}
}
