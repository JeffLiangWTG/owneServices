using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Services.Scim.Models;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using SimpleIdServer.Scim.Exceptions;
#if NET48
using System;
using System.Linq;
#endif

namespace Enterprise.Services.Scim.Business.Test
{
	sealed class BizOExtensionsTest : TestCaseWithFactory
	{
		public void TestToScim_WrongType()
		{
			var org = Factory.New<OrgHeader>();
			AssertExceptionThrown<SCIMSchemaNotFoundException>(delegate { org.ToScim(); });
		}

		public void TestToScim_User_NonControllerContext()
		{
			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery());
			staff.GS_FullName = "first middle last";
			staff.GS_GivenName = "first";
			staff.GS_MiddleName = "middle";
			staff.GS_Surname = "last";
			staff.GS_NameSuffix = "suffix";
			staff.GS_NameTitle = "dr";
			staff.GS_ExternalId = "ext1";
			staff.GS_LoginName = "login";
			staff.GS_EmailAddress = "email@user.com";
			staff.GS_UserAddress1 = "address1";
			staff.GS_UserAddress2 = "address2";
			staff.GS_State = "QLD";
			staff.GS_City = "Maudsland";
			staff.GS_Postcode = "4210";
			staff.GS_RN_NKCountryCode = "AU";
			staff.GS_HomePhone = "111";
			staff.GS_WorkPhone = "222";
			staff.GS_MobilePhone = "333";
			staff.GS_FaxNum = "444";
			staff.GS_Title = "boss";
			staff.GS_FriendlyName = "mate";
			staff.GS_WorkingLanguage = "UA";

			Factory.Save();

			var userContext = new UserContext(User.WebUserName, EnvProxy.Instance.Registry.WebBranch, EnvProxy.Instance.Registry.WebDepartment);
			using (Env.SetTemporaryUserContext(userContext))
			{
				var scimBase = new BusinessObjectFactory().Load<GlbStaff>(staff.PK).ToScim();
				AssertNotNull(scimBase);

				var user = scimBase as ScimUser;
				AssertNotNull(user);

				if (user != null)
				{
					AssertEquals(staff.PK.ToGuid(), user.Id);
					AssertEquals("dr first middle last, suffix", user.NameFormatted);
					AssertEquals("first", user.GivenName);
					AssertEquals("middle", user.MiddleName);
					AssertEquals("last", user.FamilyName);
					AssertEquals("suffix", user.NameHonorificSuffix);
					AssertEquals("dr", user.NameHonorificPrefix);
					AssertEquals("ext1", user.ExternalId);
					AssertEquals("login", user.UserName);
					AssertEquals("email@user.com", user.Email);
					AssertEquals("address1", user.AddressesStreetAddress);
					AssertEquals("QLD", user.AddressesRegion);
					AssertEquals("Maudsland", user.AddressesLocality);
					AssertEquals("4210", user.AddressesPostalCode);
					AssertEquals("AU", user.AddressesCountry);
					AssertEquals("111", user.PhoneNumbersHome);
					AssertEquals("222", user.PhoneNumbersWork);
					AssertEquals("333", user.PhoneNumbersMobile);
					AssertEquals("444", user.PhoneNumbersFax);
					AssertEquals("boss", user.Title);
					AssertEquals("mate", user.NickName);
					AssertEquals("UA", user.PreferredLanguage);
				}
			}
		}

		public void TestToScim_User()
		{
			var group1 = Factory.New<GlbGroup>();
			group1.GG_Code = "111";
			group1.GG_Desc = "description 1";
			group1.GG_ExternalId = "gg1";

			var group2 = Factory.New<GlbGroup>();
			group2.GG_Code = "222";
			group2.GG_Desc = "description 2";
			group2.GG_IsSecurityEnabled = false;
			group2.GG_ExternalId = "gg2";

			var group3 = Factory.New<GlbGroup>();
			group3.GG_Code = "333";
			group3.GG_Desc = "description 3";
			group3.GG_ExternalId = "gg3";

			var group4 = Factory.New<GlbGroup>();
			group4.GG_Code = "4444";
			group4.GG_Desc = "description 4";
			group4.GG_ExternalId = "gg4";

			var branch = Factory.LoadTop1<GlbBranch>(new ZQuery());
			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery());

			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "first middle last";
			staff.GS_GivenName = "first";
			staff.GS_MiddleName = "middle";
			staff.GS_Surname = "last";
			staff.GS_NameSuffix = "suffix";
			staff.GS_NameTitle = "dr";
			staff.GS_ExternalId = "ext1";
			staff.GS_LoginName = "login";
			staff.GS_EmailAddress = "email@user.com";
			staff.GS_UserAddress1 = "address1";
			staff.GS_UserAddress2 = "address2";
			staff.GS_State = "QLD";
			staff.GS_City = "Maudsland";
			staff.GS_Postcode = "4210";
			staff.GS_RN_NKCountryCode = "AU";
			staff.GS_HomePhone = "111";
			staff.GS_WorkPhone = "222";
			staff.GS_MobilePhone = "333";
			staff.GS_FaxNum = "444";
			staff.GS_Title = "boss";
			staff.GS_FriendlyName = "mate";
			staff.GS_WorkingLanguage = "UA";
			staff.GS_GB_HomeBranch = branch.PK;
			staff.GS_GE_HomeDepartment = department.PK;
			staff.GS_Pager = "woot woot";

			group1.Staff.Add(staff);
			group2.Staff.Add(staff);
			group2.GG_GG_ParentGroup = group3.PK;

			Factory.Save();

			var scimBase = staff.ToScim();
			AssertNotNull(scimBase);

			var user = scimBase as ScimUser;
			AssertNotNull(user);
			if (user == null)
			{
				return;
			}

			AssertEquals(staff.PK.ToGuid(), user.Id);
			AssertEquals("dr first middle last, suffix", user.NameFormatted);
			AssertEquals("first", user.GivenName);
			AssertEquals("middle", user.MiddleName);
			AssertEquals("last", user.FamilyName);
			AssertEquals("suffix", user.NameHonorificSuffix);
			AssertEquals("dr", user.NameHonorificPrefix);
			AssertEquals("ext1", user.ExternalId);
			AssertEquals("login", user.UserName);
			AssertEquals("email@user.com", user.Email);
			AssertEquals("address1", user.AddressesStreetAddress);
			AssertEquals("QLD", user.AddressesRegion);
			AssertEquals("Maudsland", user.AddressesLocality);
			AssertEquals("4210", user.AddressesPostalCode);
			AssertEquals("AU", user.AddressesCountry);
			AssertEquals("111", user.PhoneNumbersHome);
			AssertEquals("222", user.PhoneNumbersWork);
			AssertEquals("333", user.PhoneNumbersMobile);
			AssertEquals("444", user.PhoneNumbersFax);
			AssertEquals("boss", user.Title);
			AssertEquals("mate", user.NickName);
			AssertEquals("UA", user.PreferredLanguage);
			AssertEquals("Should be a member of 3 test groups, an ALL group shouldn't show", 3, user.Groups.Count());
			AssertEquals(branch.GB_Code, user.HomeBranch);
			AssertEquals(department.GE_Code, user.HomeDepartment);
			AssertEquals("woot woot", user.OtherReferences);

			AssertContainsExactElementsInAnyOrder(new ZGuid[] { group1.PK, group2.PK, group3.PK }, user.Groups.Select(g => new ZGuid(g.Value)));

			var userGroup1 = user.Groups.SingleOrDefault(g => g.Value == group1.PK.ToString());
			var userGroup2 = user.Groups.SingleOrDefault(g => g.Value == group2.PK.ToString());
			var userGroup3 = user.Groups.SingleOrDefault(g => g.Value == group3.PK.ToString());
			AssertNotNull(userGroup1);
			AssertNotNull(userGroup2);
			AssertNotNull(userGroup3);
			if (userGroup1 == null || userGroup2 == null || userGroup3 == null)
			{
				return;
			}

			AssertEquals("../Groups/" + group1.PK, userGroup1.Ref);
			AssertEquals("Group", userGroup1.Type);
			AssertEquals(group1.GG_Desc, userGroup1.Display);

			AssertEquals("../Groups/" + group2.PK, userGroup2.Ref);
			AssertEquals("Group", userGroup2.Type);
			AssertEquals(group2.GG_Desc, userGroup2.Display);

			AssertEquals("../Groups/" + group3.PK, userGroup3.Ref);
			AssertEquals("Group", userGroup3.Type);
			AssertEquals(group3.GG_Desc, userGroup3.Display);
		}

		public void TestToScim_NonScimGroup_RegistryEnabled()
		{
			using (SystemDataRegistry.Instance.ScimReturnAllGroupMembershipsForStaff.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var group1 = Factory.New<GlbGroup>();
				group1.GG_Code = "AAA";
				group1.GG_Desc = "description 1";
				group1.GG_ExternalId = "g1";

				var group2 = Factory.New<GlbGroup>();
				group2.GG_Code = "BBB";
				group2.GG_Desc = "description 2";

				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "login1";
				staff.GS_FullName = "full name 1";

				group1.Staff.Add(staff);
				group2.Staff.Add(staff);

				var scimUser = staff.ToScim() as ScimUser;
				AssertNotNull(scimUser);
				if (scimUser == null)
				{
					return;
				}

				AssertEquals(3, scimUser.Groups.Count());
				AssertContainsExactElementsInAnyOrder(new[] { "description 1", "description 2", "ALL STAFF" }, scimUser.Groups.Select(g => g.Display));
			}
		}

		public void TestToScim_NonScimGroup_RegistryDisabled()
		{
			using (SystemDataRegistry.Instance.ScimReturnAllGroupMembershipsForStaff.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var group1 = Factory.New<GlbGroup>();
				group1.GG_Code = "AAA";
				group1.GG_Desc = "description 1";
				group1.GG_ExternalId = "g1";

				var group2 = Factory.New<GlbGroup>();
				group2.GG_Code = "BBB";
				group2.GG_Desc = "description 2";

				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "login1";
				staff.GS_FullName = "full name 1";

				group1.Staff.Add(staff);
				group2.Staff.Add(staff);

				var scimUser = staff.ToScim() as ScimUser;
				AssertNotNull(scimUser);
				if (scimUser == null)
				{
					return;
				}

				AssertEquals(1, scimUser.Groups.Count());
				AssertContainsExactElementsInAnyOrder(new[] { "description 1" }, scimUser.Groups.Select(g => g.Display));
			}
		}

		public void TestToScim_SystemGroups_RegistryEnabled()
		{
			using (SystemDataRegistry.Instance.ScimReturnAllGroupMembershipsForStaff.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "login1";
				staff.GS_FullName = "full name 1";
				staff.IsDatabaseDeveloper = true;
				staff.IsReadOnlyDBUser = true;
				staff.IsBackupOperator = true;

				Factory.Save();

				var scimUser = staff.ToScim() as ScimUser;
				AssertNotNull(scimUser);
				if (scimUser == null)
				{
					return;
				}

				AssertEquals(1, scimUser.Groups.Count());
				AssertContainsExactElementsInAnyOrder(new[] { "ALL STAFF" }, scimUser.Groups.Select(g => g.Display));
			}
		}

		public void TestToScim_SystemGroups_RegistryDisabled()
		{
			using (SystemDataRegistry.Instance.ScimReturnAllGroupMembershipsForStaff.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "login1";
				staff.GS_FullName = "full name 1";
				staff.IsDatabaseDeveloper = true;
				staff.IsReadOnlyDBUser = true;
				staff.IsBackupOperator = true;

				Factory.Save();

				var scimUser = staff.ToScim() as ScimUser;
				AssertNotNull(scimUser);
				if (scimUser == null)
				{
					return;
				}

				AssertEquals(0, scimUser.Groups.Count());
			}
		}

		public void TestToScim_Group()
		{
			var validCategories = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("C1", "Category 1"),
				new CodeDescriptionPair("C2", "Category 2"),
			};
			SystemDataRegistry.Instance.GroupCategoryList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, validCategories);

			var group1 = Factory.New<GlbGroup>();
			group1.GG_Code = "AAA";
			group1.GG_Desc = "description 1";
			group1.GG_ExternalId = "g1";
			group1.GG_Category = "C1";

			var group2 = Factory.New<GlbGroup>();
			group2.GG_Code = "BBB";
			group2.GG_Desc = "description 2";
			group2.GG_ExternalId = "g2";
			group2.GG_IsSecurityEnabled = false;

			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_LoginName = "login1";
			staff1.GS_FullName = "full name 1";

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_LoginName = "login2";
			staff2.GS_FullName = "full name 2";

			group1.Staff.Add(staff1);
			group1.Staff.Add(staff2);
			group2.GG_GG_ParentGroup = group1.PK;

			Factory.Save();

			var scimBase = group1.ToScim();
			AssertNotNull(scimBase);

			var scimGroup = scimBase as ScimGroup;
			AssertNotNull(scimGroup);
			if (scimGroup == null)
			{
				return;
			}

			AssertEquals(group1.PK.ToGuid(), scimGroup.Id);
			AssertEquals("description 1", scimGroup.DisplayName);
			AssertEquals("g1", scimGroup.ExternalId);
			AssertEquals("C1", scimGroup.Category);
			AssertEquals(3, scimGroup.Members.Count());

			var members = scimGroup.Members.ToArray();
			var memberUser1 = scimGroup.Members.SingleOrDefault(m => m.Value == staff1.PK.ToString());
			var memberUser2 = scimGroup.Members.SingleOrDefault(m => m.Value == staff2.PK.ToString());
			var memberGroup1 = scimGroup.Members.SingleOrDefault(m => m.Value == group2.PK.ToString());

			AssertNotNull(memberUser1);
			AssertNotNull(memberUser2);
			AssertNotNull(memberGroup1);
			if (memberUser1 == null || memberUser2 == null || memberGroup1 == null)
			{
				return;
			}

			AssertEquals("../Users/" + staff1.PK, memberUser1.Ref);
			AssertEquals("User", memberUser1.Type);

			AssertEquals("../Users/" + staff2.PK, memberUser2.Ref);
			AssertEquals("User", memberUser2.Type);

			AssertEquals("../Groups/" + group2.PK, memberGroup1.Ref);
			AssertEquals("Group", memberGroup1.Type);
		}

		public void TestSaveExceptions_User()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "123";
			staff.GS_FullName = "123";

			Factory.Save();

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "123";
			staff2.GS_FullName = "456";

			AssertExceptionThrown<SCIMUniquenessAttributeException>(staff2.TrySave);
		}

		public void TestUpdateFromScim_Mismatch()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "123";
			staff.GS_FullName = "123";

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "AAA";
			group.GG_Desc = "description";

			Factory.Save();

			var scimUser = new ScimUser();
			var scimGroup = new ScimGroup();

			AssertExceptionThrown<ArgumentException>(() => { staff.UpdateFromScim(scimGroup); });
			AssertExceptionThrown<ArgumentException>(() => { group.UpdateFromScim(scimUser); });
		}

		public void TestUpdateFromScim_User()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "123";
			staff.GS_FullName = "123";

			Factory.Save();

			var user = new ScimUser();
			user.NameFormatted = "first middle last";
			user.GivenName = "first";
			user.MiddleName = "middle";
			user.FamilyName = "last";
			user.NameHonorificSuffix = "suffix";
			user.NameHonorificPrefix = "dr";
			user.ExternalId = "ext1";
			user.UserName = "login";
			user.Email = "email@user.com";
			user.AddressesStreetAddress = "address1";
			user.AddressesRegion = "QLD";
			user.AddressesLocality = "Maudsland";
			user.AddressesPostalCode = "4210";
			user.AddressesCountry = "AU";
			user.PhoneNumbersHome = "111";
			user.PhoneNumbersWork = "tel:222";
			user.PhoneNumbersMobile = "333";
			user.PhoneNumbersFax = "444";
			user.Title = "boss";
			user.NickName = "mate";
			user.PreferredLanguage = "UA";

			staff.UpdateFromScim(user);
			AssertEquals("first middle last", staff.GS_FullName);
			AssertEquals("first", staff.GS_GivenName);
			AssertEquals("middle", staff.GS_MiddleName);
			AssertEquals("last", staff.GS_Surname);
			AssertEquals("suffix", staff.GS_NameSuffix);
			AssertEquals("dr", staff.GS_NameTitle);
			AssertEquals("ext1", staff.GS_ExternalId);
			AssertEquals("login", staff.GS_LoginName);
			AssertEquals("email@user.com", staff.GS_EmailAddress);
			AssertEquals("address1", staff.GS_UserAddress1);
			AssertEquals("QLD", staff.GS_State);
			AssertEquals("Maudsland", staff.GS_City);
			AssertEquals("4210", staff.GS_Postcode);
			AssertEquals("AU", staff.GS_RN_NKCountryCode);
			AssertEquals("111", staff.GS_HomePhone);
			AssertEquals("222", staff.GS_WorkPhone);
			AssertEquals("333", staff.GS_MobilePhone);
			AssertEquals("444", staff.GS_FaxNum);
			AssertEquals("boss", staff.GS_Title);
			AssertEquals("mate", staff.GS_FriendlyName);
			AssertEquals("EN", staff.GS_WorkingLanguage);
			AssertEquals("123", staff.GS_Code);
		}

		public void TestUpdateFromScim_SystemAccount()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "123";
			staff.GS_FullName = "123";
			staff.GS_IsSystemAccount = true;

			Factory.Save();

			var user = new ScimUser();
			user.NameFormatted = "first middle last";

			AssertExceptionThrown<SCIMNoTargetException>(delegate { staff.UpdateFromScim(user); });
		}

		public void TestUpdateFromScim_SystemGroup()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "123";
			group.GG_Desc = "123";
			group.GG_IsSystemDefined = true;

			Factory.Save();

			var scimGroup = new ScimGroup();
			scimGroup.DisplayName = "some group";

			AssertExceptionThrown<SCIMNoTargetException>(delegate { group.UpdateFromScim(scimGroup); });
		}

		public void TestUpdateFromScim_User_Language()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "123";
			staff.GS_FullName = "123";

			Factory.Save();

			var user = new ScimUser();
			user.NameFormatted = "first middle last";
			user.GivenName = "first";
			user.FamilyName = "last";
			user.ExternalId = "ext1";
			user.UserName = "login";
			user.Email = "email@user.com";
			user.PreferredLanguage = "UA";

			staff.UpdateFromScim(user);
			AssertEquals("first middle last", staff.GS_FullName);
			AssertEquals("first", staff.GS_GivenName);
			AssertEquals("last", staff.GS_Surname);
			AssertEquals("ext1", staff.GS_ExternalId);
			AssertEquals("login", staff.GS_LoginName);
			AssertEquals("email@user.com", staff.GS_EmailAddress);
			AssertEquals("EN", staff.GS_WorkingLanguage);
			AssertEquals("123", staff.GS_Code);

			user.PreferredLanguage = "";
			staff.UpdateFromScim(user);
			AssertEquals("EN", staff.GS_WorkingLanguage);

			user.AddressesCountry = "12";
			staff.UpdateFromScim(user);
			AssertEquals("EN", staff.GS_WorkingLanguage);

			user.AddressesCountry = "AF-ZA";
			staff.UpdateFromScim(user);
			AssertEquals("EN", staff.GS_WorkingLanguage);
		}

		public void TestUpdateFromScim_Group()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "AAA";
			group.GG_Desc = "description";

			Factory.Save();

			var scimGroup = new ScimGroup();
			scimGroup.DisplayName = "other desc";
			scimGroup.ExternalId = "123";

			group.UpdateFromScim(scimGroup);

			AssertEquals("other desc", group.GG_Desc);
			AssertEquals("123", group.GG_ExternalId);
			AssertEquals("AAA", group.GG_Code);

			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "111";
			staff1.GS_FullName = "111";
			staff1.GS_LoginName = "111";

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "222";
			staff2.GS_FullName = "222";
			staff2.GS_LoginName = "222";

			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "333";
			staff3.GS_FullName = "333";
			staff3.GS_LoginName = "333";

			var staff4 = Factory.New<GlbStaff>();
			staff4.GS_Code = "444";
			staff4.GS_FullName = "444";
			staff4.GS_LoginName = "444";

			Factory.Save();

			var user1 = new ScimUser();
			user1.Id = staff1.PK.ToGuid();

			var user2 = new ScimUser();
			user2.Id = staff2.PK.ToGuid();

			var user3 = new ScimUser();
			user3.Id = staff3.PK.ToGuid();

			var user4 = new ScimUser();
			user4.Id = staff4.PK.ToGuid();

			scimGroup.Members = new[] { new GroupMember() { Type = "User", Value = user1.Id.ToString() }, new GroupMember() { Type = "User", Value = user2.Id.ToString() } };

			group.UpdateFromScim(scimGroup);

			Factory.Save();

			AssertEquals(2, group.Staff.Count);
			AssertContainsExactElementsInAnyOrder(new[] { staff1.PK, staff2.PK }, group.Staff.Cast<GlbStaff>().Select(s => s.PK));

			scimGroup.Members = new[] { new GroupMember() { Type = "User", Value = user3.Id.ToString() }, new GroupMember() { Type = "User", Value = user4.Id.ToString() } };

			group.UpdateFromScim(scimGroup);

			Factory.Save();

			AssertEquals(2, group.Staff.Count);
			AssertContainsExactElementsInAnyOrder(new[] { staff3.PK, staff4.PK }, group.Staff.Cast<GlbStaff>().Select(s => s.PK));
		}

		public void TestUpdateFromScim_Group_NoType()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "AAA";
			group.GG_Desc = "description1";

			var subGroup = Factory.New<GlbGroup>();
			subGroup.GG_Code = "BBB";
			subGroup.GG_Desc = "description2";

			Factory.Save();

			var scimGroup = new ScimGroup();
			scimGroup.DisplayName = "groupName";
			scimGroup.ExternalId = "123";

			var scimSubGroup = new ScimGroup();
			scimSubGroup.DisplayName = "groupName2";
			scimSubGroup.ExternalId = "456";

			group.UpdateFromScim(scimGroup);
			subGroup.UpdateFromScim(scimSubGroup);

			AssertEquals("groupName", group.GG_Desc);
			AssertEquals("123", group.GG_ExternalId);
			AssertEquals("AAA", group.GG_Code);

			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "111";
			staff1.GS_FullName = "111";
			staff1.GS_LoginName = "111";
			staff1.GS_CanLogin = false;
			staff1.GS_IsController = false;
			staff1.GS_IsRobot = false;

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "222";
			staff2.GS_FullName = "222";
			staff2.GS_LoginName = "222";
			staff2.GS_CanLogin = false;
			staff2.GS_IsController = false;
			staff2.GS_IsRobot = false;

			Factory.Save();

			var user1 = new ScimUser();
			user1.Id = staff1.PK.ToGuid();

			var user2 = new ScimUser();
			user2.Id = staff2.PK.ToGuid();

			scimGroup.Members = new[] { new GroupMember() { Type = "User", Value = user1.Id.ToString() } };

			group.UpdateFromScim(scimGroup);

			Factory.Save();

			AssertEquals(1, group.Staff.Count);
			AssertEquals(ZGuid.Empty, subGroup.GG_GG_ParentGroup);
			AssertContainsExactElementsInAnyOrder(new[] { staff1.PK }, group.Staff.Cast<GlbStaff>().Select(s => s.PK));

			scimGroup.Members = new[] { new GroupMember() { Type = "User", Value = user1.Id.ToString() }, new GroupMember() { Type = "", Value = user2.Id.ToString() } };

			group.UpdateFromScim(scimGroup);

			Factory.Save();

			AssertEquals(2, group.Staff.Count);
			AssertEquals(ZGuid.Empty, subGroup.GG_GG_ParentGroup);
			AssertContainsExactElementsInAnyOrder(new[] { staff1.PK, staff2.PK }, group.Staff.Cast<GlbStaff>().Select(s => s.PK));

			scimGroup.Members = new[] { new GroupMember() { Type = "User", Value = user1.Id.ToString() }, new GroupMember() { Type = "", Value = subGroup.PK.ToString() } };

			group.UpdateFromScim(scimGroup);

			Factory.Save();

			AssertEquals(1, group.Staff.Count);
			AssertEquals(group.PK, subGroup.GG_GG_ParentGroup);
			AssertContainsExactElementsInAnyOrder(new[] { staff1.PK }, group.Staff.Cast<GlbStaff>().Select(s => s.PK));
		}

		public void TestUpdateFromScim_Group_Duplicate()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "AAA";
			group.GG_Desc = "description";

			Factory.Save();

			var newGroup = Factory.New<GlbGroup>();
			var scimGroup = new ScimGroup();
			scimGroup.DisplayName = "description";
			scimGroup.ExternalId = "123";

			AssertExceptionThrown<SCIMUniquenessAttributeException>(delegate { newGroup.UpdateFromScim(scimGroup); });
		}

		public void TestStaffSubClass()
		{
			var staff = Factory.New<GlbStaffForTest>();
			staff.GS_LoginName = "somelogin";
			staff.GS_Code = "~C1";

			AssertNoExceptionThrown(delegate { staff.ToScim(); });
		}

		public void TestGroupSubClass()
		{
			var group = Factory.New<GlbGroupForTest>();
			group.GG_Desc = "somedesc";
			group.GG_Code = "~C1";

			AssertNoExceptionThrown(delegate { group.ToScim(); });
		}

		public void TestSetStaffColumnValue_False_GS_CanLogin()
		{
			AssertSetStaffColumnValue_False("GS_CanLogin");
		}

		public void TestSetStaffColumnValue_False_IsDatabaseDeveloper()
		{
			AssertSetStaffColumnValue_False("IsDatabaseDeveloper");
		}

		public void TestSetStaffColumnValue_False_IsReadOnlyDBUser()
		{
			AssertSetStaffColumnValue_False("IsReadOnlyDBUser");
		}

		public void TestSetStaffColumnValue_False_IsBackupOperator()
		{
			AssertSetStaffColumnValue_False("IsBackupOperator");
		}

		public void AssertSetStaffColumnValue_False(string column)
		{
			var codesCollection = new StaffColumnToGroupDescriptionScimMappingCollection();
			var staffColumnToGroupDescriptionScimMapping = codesCollection.AddNew();
			staffColumnToGroupDescriptionScimMapping.StaffColumnName = column;
			staffColumnToGroupDescriptionScimMapping.GroupDescriptionMapping = "Test01";

			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, codesCollection))
			{
				var group = Factory.New<GlbGroup>();
				group.GG_Code = "AAA";
				group.GG_Desc = "Test01";

				var staff1 = Factory.New<GlbStaff>();
				staff1.GS_Code = "111";
				staff1.GS_FullName = "111";
				staff1.GS_LoginName = "111";
				staff1[column] = true;

				var staff2 = Factory.New<GlbStaff>();
				staff2.GS_Code = "222";
				staff2.GS_FullName = "222";
				staff2.GS_LoginName = "222";
				staff2[column] = true;

				var staff3 = Factory.New<GlbStaff>();
				staff3.GS_Code = "333";
				staff3.GS_FullName = "333";
				staff3.GS_LoginName = "333";
				staff3[column] = true;

				Factory.Save();

				AssertEquals(3, group.Staff.Count);

				group.RemoveStaffFlagsIfRequired();

				AssertEquals(false, staff1[column]);
				AssertEquals(false, staff2[column]);
				AssertEquals(false, staff3[column]);

				Factory.Save();
				group.Staff.Reload(true, true);

				AssertEquals(0, group.Staff.Count);
			}
		}

		public void TestSetStaffColumnValue_True_GS_CanLogin()
		{
			AssertSetStaffColumnValue_True("GS_CanLogin");
		}

		public void TestSetStaffColumnValue_True_IsDatabaseDeveloper()
		{
			AssertSetStaffColumnValue_True("IsDatabaseDeveloper");
		}

		public void TestSetStaffColumnValue_True_IsReadOnlyDBUser()
		{
			AssertSetStaffColumnValue_True("IsReadOnlyDBUser");
		}

		public void TestSetStaffColumnValue_True_IsBackupOperator()
		{
			AssertSetStaffColumnValue_True("IsBackupOperator");
		}

		public void AssertSetStaffColumnValue_True(string column)
		{
			var codesCollection = new StaffColumnToGroupDescriptionScimMappingCollection();
			var staffColumnToGroupDescriptionScimMapping = codesCollection.AddNew();
			staffColumnToGroupDescriptionScimMapping.StaffColumnName = column;
			staffColumnToGroupDescriptionScimMapping.GroupDescriptionMapping = "Test01";

			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, codesCollection))
			{
				var group = Factory.New<GlbGroup>();
				group.GG_Code = "AAA";
				group.GG_Desc = "Test01";

				Factory.Save();

				var scimGroup = new ScimGroup();
				scimGroup.DisplayName = "Test01";
				scimGroup.ExternalId = "123";

				var staff1 = Factory.New<GlbStaff>();
				staff1.GS_Code = "111";
				staff1.GS_FullName = "111";
				staff1.GS_LoginName = "111";
				staff1[column] = false;

				var staff2 = Factory.New<GlbStaff>();
				staff2.GS_Code = "222";
				staff2.GS_FullName = "222";
				staff2.GS_LoginName = "222";
				staff2[column] = false;

				Factory.Save();

				var user1 = new ScimUser();
				user1.Id = staff1.PK.ToGuid();

				var user2 = new ScimUser();
				user2.Id = staff2.PK.ToGuid();

				scimGroup.Members = new[] { new GroupMember() { Type = "User", Value = user1.Id.ToString() }, new GroupMember() { Type = "User", Value = user2.Id.ToString() } };

				group.UpdateFromScim(scimGroup);

				Factory.Save();

				AssertEquals(2, group.Staff.Count);
				AssertContainsExactElementsInAnyOrder(new[] { staff1.PK, staff2.PK }, group.Staff.Cast<GlbStaff>().Select(s => s.PK));
				Assert((ZBool)staff1[column]);
				Assert((ZBool)staff2[column]);
			}
		}

		public void TestSaveLogsIssueOnError()
		{
			var staff = Factory.New<GlbStaffForTest>();
			staff.ThrowOnFactorySaving = true;
			AssertExceptionThrown(typeof(SCIMNoTargetException), delegate { staff.TrySave(); } );

			Assert(ErrorReporter.LastMessageReported + " does not start with \"SCIM Error Saving Record\"", ErrorReporter.LastMessageReported.StartsWith("SCIM Error Saving Record"));
			ErrorReporter.Clear();
		}

		class GlbStaffForTest : GlbStaff
		{
			public GlbStaffForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override AutologState AutoLoggingState => AutologState.NotLogged;

			public bool ThrowOnFactorySaving;
			protected override void OnFactorySaving()
			{
				if (ThrowOnFactorySaving)
				{
					throw new ZSaveConcurrencyException(new ZDataConcurrencyException(null, null, null), Factory);
				}

				base.OnFactorySaving();
			}
		}

		class GlbGroupForTest : GlbGroup
		{
			public GlbGroupForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override AutologState AutoLoggingState => AutologState.NotLogged;
		}
	}
}
