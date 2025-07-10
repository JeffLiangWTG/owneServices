using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AddressLookupTest : TestCaseWithFactory
	{
		public void TestStateList()
		{
			var country = Factory.New<RefCountry>();
			country.RN_Code = "X7";
			var state1 = Factory.New<RefCountryStates>();
			state1.RW_RN_NKCountryCode = country.RN_Code;
			state1.RW_Code = "XK1";
			state1.RW_Description = "XK1 DESC";
			var state2 = Factory.New<RefCountryStates>();
			state2.RW_RN_NKCountryCode = country.RN_Code;
			state2.RW_Code = "XK2";
			state2.RW_Description = "XK2 DESC";

			var list = Factory.GetStateList("", false);
			AssertEquals("State List should have no elements.", 0, list.Count);

			list = Factory.GetStateList("X7", true);
			AssertEquals("State List should have no elements.", 0, list.Count);

			BusinessObjectFactory nullFactory = null;
			list = nullFactory.GetStateList("X7", false);
			AssertEquals("State List should have no elements.", 0, list.Count);

			list = Factory.GetStateList("AA", false);
			AssertEquals("State List should have no elements.", 0, list.Count);

			list = Factory.GetStateList("X7", false);
			AssertEquals("State List should have 2 elements.", 2, list.Count);
			AssertEquals("XK1 DESC", list.GetDescriptionFromCode("XK1"));
			AssertEquals("XK2 DESC", list.GetDescriptionFromCode("XK2"));
			var list2 = Factory.GetStateList("X7", false);
			AssertEquals(true, object.ReferenceEquals(list, list2));

			Factory.Save();
			list2 = Factory.GetStateList("X7", false);
			AssertEquals("List should be refresh whenever RefCountryStates is saved/deleted", false, object.ReferenceEquals(list, list2));
			AssertEquals("State List should have 2 elements.", 2, list2.Count);
			AssertEquals("XK1 DESC", list2.GetDescriptionFromCode("XK1"));
			AssertEquals("XK2 DESC", list2.GetDescriptionFromCode("XK2"));

			var newFactory = new BusinessObjectFactory();
			var state3 = newFactory.New<RefCountryStates>();
			state3.RW_RN_NKCountryCode = country.RN_Code;
			state3.RW_Code = "XK3";
			state3.RW_Description = "XK3 DESC";
			newFactory.Save();

			AssertEquals("List should not be refresh as add doesn't get updated by data refresh", true, object.ReferenceEquals(Factory.GetStateList("X7", false), list2));
			AssertEquals("State List should have 2 elements.", 2, list2.Count);
			AssertEquals("XK1 DESC", list2.GetDescriptionFromCode("XK1"));
			AssertEquals("XK2 DESC", list2.GetDescriptionFromCode("XK2"));

			var list3 = newFactory.GetStateList("X7", false);
			AssertEquals("State List should have 3 elements.", 3, list3.Count);
			AssertEquals("XK1 DESC", list3.GetDescriptionFromCode("XK1"));
			AssertEquals("XK2 DESC", list3.GetDescriptionFromCode("XK2"));
			AssertEquals("XK3 DESC", list3.GetDescriptionFromCode("XK3"));

			state1.Delete();
			var list4 = Factory.GetStateList("X7", false);
			AssertEquals("List should be refresh whenever RefCountryStates is saved/deleted", false, object.ReferenceEquals(list4, list2));
			AssertEquals("State List should have 1 element.", 1, list4.Count);
			AssertEquals("XK2 DESC", list4.GetDescriptionFromCode("XK2"));

			AssertEquals("List should not be refresh as delete data hasn't been saved", true, object.ReferenceEquals(newFactory.GetStateList("X7", false), list3));
			AssertEquals("State List should have 3 elements.", 3, list3.Count);
			AssertEquals("XK1 DESC", list3.GetDescriptionFromCode("XK1"));
			AssertEquals("XK2 DESC", list3.GetDescriptionFromCode("XK2"));
			AssertEquals("XK3 DESC", list3.GetDescriptionFromCode("XK3"));

			Factory.Save();
			AssertEquals("List should not be refresh as saving deleted should not cause refresh", true, object.ReferenceEquals(Factory.GetStateList("X7", false), list4));
			AssertEquals("State List should have 1 element.", 1, list4.Count);
			AssertEquals("XK2 DESC", list4.GetDescriptionFromCode("XK2"));

			var list5 = newFactory.GetStateList("X7", false);
			AssertEquals("List should be refresh as data was updated by data refresh", false, object.ReferenceEquals(list3, list5));
			AssertEquals("State List should have 2 elements.", 2, list5.Count);
			AssertEquals("XK2 DESC", list5.GetDescriptionFromCode("XK2"));
			AssertEquals("XK3 DESC", list5.GetDescriptionFromCode("XK3"));

			state2.RW_Description = "XK2 DESC 2";
			AssertEquals("List should not be refresh as data has not been saved", true, object.ReferenceEquals(Factory.GetStateList("X7", false), list4));
			AssertEquals("State List should have 1 element.", 1, list4.Count);
			AssertEquals("XK2 DESC", list4.GetDescriptionFromCode("XK2"));

			Factory.Save();
			list2 = Factory.GetStateList("X7", false);
			AssertEquals("List should be refresh as data has not been saved", false, object.ReferenceEquals(list2, list4));
			AssertEquals("State List should have 1 element.", 1, list2.Count);
			AssertEquals("XK2 DESC 2", list2.GetDescriptionFromCode("XK2"));

			list3 = newFactory.GetStateList("X7", false);
			AssertEquals("List should be refresh as data has been saved", false, object.ReferenceEquals(list3, list5));
			AssertEquals("State List should have 2 elements.", 2, list3.Count);
			AssertEquals("XK2 DESC 2", list3.GetDescriptionFromCode("XK2"));
			AssertEquals("XK3 DESC", list3.GetDescriptionFromCode("XK3"));
		}

		public void TestStateList_ExcludeInactiveState()
		{
			// Arrange.

			var country = Factory.New<RefCountry>();
			country.RN_Code = "W8";

			var activeState = Factory.New<RefCountryStates>();
			activeState.RW_RN_NKCountryCode = country.RN_Code;
			activeState.RW_Code = "MAS";
			activeState.RW_Description = "[_MOCK_ACTIVE_STATE_]";
			activeState.RW_IsActive = true;

			var inactiveState = Factory.New<RefCountryStates>();
			inactiveState.RW_RN_NKCountryCode = country.RN_Code;
			inactiveState.RW_Code = "MIS";
			inactiveState.RW_Description = "[_MOCK_INACTIVE_STATE_]";
			inactiveState.RW_IsActive = false;

			// Act.

			var states = Factory
				.GetStateList(country.RN_Code, false)
				.ToArray();

			// Assert.

			AssertEquals(1, states.Length);

			var state = states.Single();
			AssertEquals("MAS", state.Code);
			AssertEquals("[_MOCK_ACTIVE_STATE_]", state.Description);
		}
	}
}
