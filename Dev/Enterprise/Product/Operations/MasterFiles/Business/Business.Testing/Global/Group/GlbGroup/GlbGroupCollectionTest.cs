using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbGroupCollection))]
	sealed class GlbGroupCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlbGroupCollection(Factory);
		}

		public void TestGlbGroupCollection()
		{
			var group1 = Factory.New<GlbGroup>();
			group1.GG_Code = "ZZZ1";
			var group2 = Factory.New<GlbGroup>();
			group2.GG_Code = "YYY1";
			var group3 = Factory.New<GlbGroup>();
			group3.GG_ExternalId = "123";
			group3.GG_Code = "XXX1";

			var dbd = Factory.Load<GlbGroup>(GlbGroup.DbDeveloperGroupPK);
			var dbr = Factory.Load<GlbGroup>(GlbGroup.DbReaderGroupPK);
			var bko = Factory.Load<GlbGroup>(GlbGroup.BackupOperatorGroupPK);

			Factory.Save();

			var query = new ZQuery();
			query.AddToFilter(GlbGroupSchema.GG_Code, SQLComparisonOperator.StartsWith, "ZZZ");

			GlbGroupCollection groupCollection = new GlbGroupCollection(Factory, new ZQuery(), query);
			groupCollection.Load();
			Assert("It contains ZZZ1", groupCollection.Contains(group1.PK));
			Assert("It doesn't contains YYY1", !groupCollection.Contains(group2.PK));
			Assert("It doesn't contain database developer group", !groupCollection.Contains(dbd.PK));
			Assert("It doesn't contain database reader group", !groupCollection.Contains(dbr.PK));
			Assert("It doesn't contain backup operator group", !groupCollection.Contains(bko.PK));
			Assert("It doesn't contain scim group", !groupCollection.Contains(group3.PK));

			AssertEquals(true, Factory.ExistsInDatabase(GlbGroupSchema.Constants.TableName, new ZQuery(GlbGroupSchema.GG_Type, GlbGroupTypeList.Codes.Organisation)));
			var group4 = new GlbGroupCollection(Factory, true);
			group4.Load();
			var group5 = new GlbGroupCollection(Factory, false);
			group5.Load();
			AssertEquals(false, group4.OfType<GlbGroup>().Any(x => x.GG_Type == GlbGroupTypeList.Codes.Organisation));
			AssertEquals(true, group5.OfType<GlbGroup>().Any(x => x.GG_Type == GlbGroupTypeList.Codes.Organisation));
		}

		public void TestAddNotificationWhenAdditionalFilterNotMet()
		{
			var groupExt = Factory.NewWithValidTestData<GlbGroup>();
			groupExt.GG_Code = "~EXT";
			groupExt.GG_ExternalId = "123";
			var groupReg = Factory.NewWithValidTestData<GlbGroup>();
			groupReg.GG_Code = "~REG";
			Factory.Save();

			var collection = new GlbGroupCollection(Factory);
			collection.Add(groupExt);
			collection.Add(groupReg);

			AssertEquals("Cannot select an externally controlled group.", collection.GetAllNotificationsWhenAdditionalFilterNotMet(groupExt));
			AssertEquals("This Group (~REG) cannot be chosen here. Please choose another Group (~REG).", collection.GetAllNotificationsWhenAdditionalFilterNotMet(groupReg));
		}

		public void TestAddScimGroup_EditingNotAllowed()
		{
			SystemDataRegistry.Instance.ScimAllowLocalEditing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var groupExt = Factory.NewWithValidTestData<GlbGroup>();
			groupExt.GG_Code = "~EXT";
			groupExt.GG_ExternalId = "123";
			Factory.Save();

			var collection = new GlbGroupCollection(Factory);
			collection.Add(groupExt);
			AssertEquals("Cannot select an externally controlled group.", collection.GetAllNotificationsWhenAdditionalFilterNotMet(groupExt));
		}

		public void TestAddScimGroup_EditingAllowed_RegularUser()
		{
			SystemDataRegistry.Instance.ScimAllowLocalEditing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "~UU";

			var groupExt = Factory.NewWithValidTestData<GlbGroup>();
			groupExt.GG_Code = "~EXT";
			groupExt.GG_ExternalId = "123";
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var collection = new GlbGroupCollection(Factory);
				collection.Add(groupExt);
				AssertEquals(ZBool.True, groupExt.MatchesFilter(collection.CompleteFilter));
			}
		}

		public void TestAddScimGroup_EditingAllowed_IsController()
		{
			SystemDataRegistry.Instance.ScimAllowLocalEditing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "~UU";
			staff.GS_IsController = true;

			var groupExt = Factory.NewWithValidTestData<GlbGroup>();
			groupExt.GG_Code = "~EXT";
			groupExt.GG_ExternalId = "123";
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var collection = new GlbGroupCollection(Factory);
				collection.Add(groupExt);
				AssertEquals(ZBool.True, groupExt.MatchesFilter(collection.CompleteFilter));
			}
		}

		public void TestAddScimGroup_EditingAllowed_NonOperational()
		{
			SystemDataRegistry.Instance.ScimAllowLocalEditing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "~UU";
			staff.GS_IsOperational = false;

			var groupExt = Factory.NewWithValidTestData<GlbGroup>();
			groupExt.GG_Code = "~EXT";
			groupExt.GG_ExternalId = "123";
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var collection = new GlbGroupCollection(Factory);
				collection.Add(groupExt);
				AssertEquals(ZBool.True, groupExt.MatchesFilter(collection.CompleteFilter));
			}
		}
	}
}
