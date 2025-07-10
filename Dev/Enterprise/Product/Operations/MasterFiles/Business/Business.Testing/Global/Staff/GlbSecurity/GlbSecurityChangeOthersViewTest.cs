using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbSecurityChangeOthersView))]
	sealed class GlbSecurityChangeOthersViewTest : BusinessObjectCollectionViewTestCase<GlbSecurityChangeOthersView>
	{
		GlbStaff staff;

		GlbStaff Staff
		{
			get { return staff ?? (staff = Factory.New<GlbStaff>()); }
		}

		void AssertView(GlbSecurityChangeOthersView view, ZGuid groupPK, ZGuid staffPK, string securityRight, ZGuid itemGuid, int count = 1)
		{
			AssertEquals("Count", count, view.Count);
			if (count == 1)
			{
				AssertEquals("view[0].GU_GG", groupPK, view[0].GU_GG);
				AssertEquals("view[0].GU_GS", staffPK, view[0].GU_GS);
				AssertEquals("view[0].GU_SecurityRight", securityRight, view[0].GU_SecurityRight);
				AssertEquals("view[0].GU_SecurityItemIsAllowed", true, view[0].GU_SecurityItemIsAllowed);
				AssertEquals("view[0].GU_ItemGUID", itemGuid, view[0].GU_ItemGUID);
			}
			else
			{
				bool found = false;
				for (var i = 0; i < count; ++i)
				{
					if (view[i].GU_GG == groupPK && view[i].GU_GS == staffPK && view[i].GU_SecurityRight == securityRight && view[i].GU_SecurityItemIsAllowed && view[i].GU_ItemGUID == itemGuid)
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					Fail("No matching security item found");
				}
			}
		}

		protected override GlbSecurityChangeOthersView GetCollectionToTest()
		{
			GlbSecurityCollection securityCollection = new GlbSecurityCollection(Factory);
			GlbSecurityChangeOthersView view = new GlbSecurityChangeOthersView(securityCollection, Staff, GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator);
			view.Rebuild();
			return view;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			GlbSecurity security = Factory.New<GlbSecurity>();
			security.GU_SecurityRight = GlbSecurity.ChangeOtherGroupSecurityRightName;
			security.GU_ItemGUID = group.PK;
			security.GU_GS = Staff.PK;
			security.GU_SecurityItemIsAllowed = true;
			return security;
		}

		public void TestAllowNew()
		{
			AssertEquals("AllowNew", false, Collection.AllowNew);
		}

		public void TestGroupAddSecurityToChangeOtherGroup()
		{
			GlbGroup groupGettingTheRight = Factory.New<GlbGroup>();
			GlbGroup groupWhoseRightsCanBeChanged = Factory.NewWithValidTestData<GlbGroup>();

			GlbSecurityChangeOthersView view = new GlbSecurityChangeOthersView(new GlbSecurityCollection(Factory), groupGettingTheRight, GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator);
			AssertEquals("Count", 0, view.Count);

			view.AddSecurityToChangeOtherGroup(groupWhoseRightsCanBeChanged.GG_Code);
			AssertView(view, groupGettingTheRight.PK, ZGuid.Empty, GlbSecurity.ChangeOtherGroupSecurityRightName, groupWhoseRightsCanBeChanged.PK);

			view.AddSecurityToChangeOtherGroup(groupWhoseRightsCanBeChanged.GG_Code);
			AssertView(view, groupGettingTheRight.PK, ZGuid.Empty, GlbSecurity.ChangeOtherGroupSecurityRightName, groupWhoseRightsCanBeChanged.PK);
		}

		public void TestGroupAddSecurityToChangeOtherStaff()
		{
			GlbGroup groupGettingTheRight = Factory.New<GlbGroup>();
			GlbStaff staffWhoseRightsCanBeChanged = Factory.NewWithValidTestData<GlbStaff>();

			GlbSecurityChangeOthersView view = new GlbSecurityChangeOthersView(new GlbSecurityCollection(Factory), groupGettingTheRight, GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator);
			AssertEquals("view.Count", 0, view.Count);

			view.AddSecurityToChangeOtherStaff(staffWhoseRightsCanBeChanged.GS_Code);
			AssertView(view, groupGettingTheRight.PK, ZGuid.Empty, GlbSecurity.ChangeOtherStaffSecurityRightName, staffWhoseRightsCanBeChanged.PK);

			view.AddSecurityToChangeOtherStaff(staffWhoseRightsCanBeChanged.GS_Code);
			AssertView(view, groupGettingTheRight.PK, ZGuid.Empty, GlbSecurity.ChangeOtherStaffSecurityRightName, staffWhoseRightsCanBeChanged.PK);
		}

		public void TestGroupOwnerMode()
		{
			GlbGroup groupGettingTheRight = Factory.New<GlbGroup>();
			GlbGroup groupA = Factory.NewWithValidTestData<GlbGroup>();
			GlbGroup groupB = Factory.NewWithValidTestData<GlbGroup>();

			GlbSecurityChangeOthersView view = new GlbSecurityChangeOthersView(new GlbSecurityCollection(Factory), groupGettingTheRight, GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator);
			AssertEquals("view.Count", 0, view.Count);

			view.AddSecurityToChangeOtherGroup(groupA.GG_Code);
			AssertView(view, groupGettingTheRight.PK, ZGuid.Empty, GlbSecurity.ChangeOtherGroupSecurityRightName, groupA.PK);
			view.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner;
			AssertEquals("view.Count", 0, view.Count);
			view.AddSecurityToChangeOtherGroup(groupB.GG_Code);
			AssertView(view, groupGettingTheRight.PK, ZGuid.Empty, GlbSecurity.GroupOwnerSecurityRightName, groupB.PK);
			view.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator;
			AssertView(view, groupGettingTheRight.PK, ZGuid.Empty, GlbSecurity.ChangeOtherGroupSecurityRightName, groupA.PK);
			view.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner;
			AssertView(view, groupGettingTheRight.PK, ZGuid.Empty, GlbSecurity.GroupOwnerSecurityRightName, groupB.PK);
		}

		public void TestGroupOwnersForGroupMode()
		{
			GlbStaff staffA = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staffB = Factory.NewWithValidTestData<GlbStaff>();
			GlbGroup groupA = Factory.NewWithValidTestData<GlbGroup>();
			GlbGroup groupB = Factory.NewWithValidTestData<GlbGroup>();

			GlbSecurityChangeOthersView viewA = new GlbSecurityChangeOthersView(new GlbSecurityCollection(Factory), staffA, GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner);
			GlbSecurityChangeOthersView viewB = new GlbSecurityChangeOthersView(new GlbSecurityCollection(Factory), staffB, GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner);
			GlbSecurityChangeOthersView viewC = new GlbSecurityChangeOthersView(new GlbSecurityCollection(Factory), groupA, GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner);

			viewA.AddSecurityToChangeOtherGroup(groupA.GG_Code);
			viewA.AddSecurityToChangeOtherGroup(groupB.GG_Code);
			viewB.AddSecurityToChangeOtherGroup(groupA.GG_Code);
			viewC.AddSecurityToChangeOtherGroup(groupB.GG_Code);

			Factory.Save();

			var security = new GlbSecurityCollection(Factory);
			security.Load(new ZQuery());

			GlbSecurityChangeOthersView viewD = new GlbSecurityChangeOthersView(security, groupA, GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwnersForGroup);
			GlbSecurityChangeOthersView viewE = new GlbSecurityChangeOthersView(security, groupB, GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwnersForGroup);
			AssertEquals("view.Count", 2, viewD.Count);
			AssertView(viewD, ZGuid.Empty, staffA.PK, GlbSecurity.GroupOwnerSecurityRightName, groupA.PK, 2);
			AssertView(viewD, ZGuid.Empty, staffB.PK, GlbSecurity.GroupOwnerSecurityRightName, groupA.PK, 2);
			AssertEquals("view.Count", 2, viewE.Count);
			AssertView(viewE, ZGuid.Empty, staffA.PK, GlbSecurity.GroupOwnerSecurityRightName, groupB.PK, 2);
			AssertView(viewE, groupA.PK, ZGuid.Empty, GlbSecurity.GroupOwnerSecurityRightName, groupB.PK, 2);

			viewD.AddSecurityToChangeOtherGroup(groupB.GG_Code);
			viewE.AddSecurityToChangeOtherStaff(staffB.GS_Code);

			AssertEquals("view.Count", 3, viewD.Count);
			AssertView(viewD, groupB.PK, ZGuid.Empty, GlbSecurity.GroupOwnerSecurityRightName, groupA.PK, 3);
			AssertEquals("view.Count", 3, viewE.Count);
			AssertView(viewE, ZGuid.Empty, staffB.PK, GlbSecurity.GroupOwnerSecurityRightName, groupB.PK, 3);

			Factory.Save();

			security = new GlbSecurityCollection(Factory);
			security.Load(new ZQuery());

			GlbSecurityChangeOthersView viewA2 = new GlbSecurityChangeOthersView(security, staffA, GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner);
			GlbSecurityChangeOthersView viewB2 = new GlbSecurityChangeOthersView(security, staffB, GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner);
			GlbSecurityChangeOthersView viewC2 = new GlbSecurityChangeOthersView(security, groupA, GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner);
			GlbSecurityChangeOthersView viewD2 = new GlbSecurityChangeOthersView(security, groupB, GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner);
			AssertEquals("view.Count", 2, viewA2.Count);
			AssertView(viewA2, ZGuid.Empty, staffA.PK, GlbSecurity.GroupOwnerSecurityRightName, groupA.PK, 2);
			AssertView(viewA2, ZGuid.Empty, staffA.PK, GlbSecurity.GroupOwnerSecurityRightName, groupB.PK, 2);
			AssertEquals("view.Count", 2, viewB2.Count);
			AssertView(viewB2, ZGuid.Empty, staffB.PK, GlbSecurity.GroupOwnerSecurityRightName, groupA.PK, 2);
			AssertView(viewB2, ZGuid.Empty, staffB.PK, GlbSecurity.GroupOwnerSecurityRightName, groupB.PK, 2);
			AssertEquals("view.Count", 1, viewC2.Count);
			AssertView(viewC2, groupA.PK, ZGuid.Empty, GlbSecurity.GroupOwnerSecurityRightName, groupB.PK);
			AssertEquals("view.Count", 1, viewD2.Count);
			AssertView(viewD2, groupB.PK, ZGuid.Empty, GlbSecurity.GroupOwnerSecurityRightName, groupA.PK);
		}

		public void TestReadOnlyFactory()
		{
			AssertNotNull("ReadOnlyFactory", ((IReadOnlyFactoryForSecurityValidationProvider)Collection).ReadOnlyFactory);
		}

		public void TestStaffAddSecurityToChangeOtherGroup()
		{
			GlbStaff staffGettingTheRight = Factory.New<GlbStaff>();
			GlbGroup groupWhoseRightsCanBeChanged = Factory.NewWithValidTestData<GlbGroup>();

			GlbSecurityChangeOthersView view = new GlbSecurityChangeOthersView(new GlbSecurityCollection(Factory), staffGettingTheRight, GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator);
			AssertEquals("view.Count", 0, view.Count);

			view.AddSecurityToChangeOtherGroup(groupWhoseRightsCanBeChanged.GG_Code);
			AssertView(view, ZGuid.Empty, staffGettingTheRight.PK, GlbSecurity.ChangeOtherGroupSecurityRightName, groupWhoseRightsCanBeChanged.PK);

			view.AddSecurityToChangeOtherGroup(groupWhoseRightsCanBeChanged.GG_Code);
			AssertView(view, ZGuid.Empty, staffGettingTheRight.PK, GlbSecurity.ChangeOtherGroupSecurityRightName, groupWhoseRightsCanBeChanged.PK);
		}

		public void TestStaffAddSecurityToChangeOtherStaff()
		{
			GlbStaff staffGettingTheRight = Factory.New<GlbStaff>();
			GlbStaff staffWhoseRightsCanBeChanged = Factory.NewWithValidTestData<GlbStaff>();

			GlbSecurityChangeOthersView view = new GlbSecurityChangeOthersView(new GlbSecurityCollection(Factory), staffGettingTheRight, GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator);
			AssertEquals("view.Count", 0, view.Count);

			view.AddSecurityToChangeOtherStaff(staffWhoseRightsCanBeChanged.GS_Code);
			AssertView(view, ZGuid.Empty, staffGettingTheRight.PK, GlbSecurity.ChangeOtherStaffSecurityRightName, staffWhoseRightsCanBeChanged.PK);

			view.AddSecurityToChangeOtherStaff(staffWhoseRightsCanBeChanged.GS_Code);
			AssertView(view, ZGuid.Empty, staffGettingTheRight.PK, GlbSecurity.ChangeOtherStaffSecurityRightName, staffWhoseRightsCanBeChanged.PK);
		}

		public void TestStaffAddSecurityToChangeOtherStaff_NewViewShouldReloadElements()
		{
			var staffGettingTheRight = Factory.New<GlbStaff>();
			var staffWhoseRightsCanBeChanged = Factory.NewWithValidTestData<GlbStaff>();

			var collection = new GlbSecurityCollection(Factory);
			var view = new GlbSecurityChangeOthersView(collection, staffGettingTheRight, GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator);
			AssertEquals("view.Count", 0, view.Count);

			view.AddSecurityToChangeOtherStaff(staffWhoseRightsCanBeChanged.GS_Code);
			AssertView(view, ZGuid.Empty, staffGettingTheRight.PK, GlbSecurity.ChangeOtherStaffSecurityRightName, staffWhoseRightsCanBeChanged.PK);

			view = new GlbSecurityChangeOthersView(collection, staffGettingTheRight, GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator);
			AssertEquals("view.Count", 1, view.Count);
		}
	}
}
