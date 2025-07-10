using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Services.Scim.Models;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SimpleIdServer.Scim.Domains;
using SimpleIdServer.Scim.Domains.Builders;
using SimpleIdServer.Scim.DTOs;
using SimpleIdServer.Scim.Exceptions;
using SimpleIdServer.Scim.Parser;
using SimpleIdServer.Scim.Parser.Expressions;
using SimpleIdServer.Scim.Persistence;
#if NET48
using System;
using System.Collections.Generic;
using System.Linq;
#endif

namespace Enterprise.Services.Scim.Business.Test
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Testing")]
	sealed class UserRepositoryTest : TestCaseWithFactory
	{
#pragma warning disable CW1021 // Static Fields Are Thread Static Rule
		static ISCIMSchemaQueryRepository? scimSchemaQueryRepository;
#pragma warning restore CW1021 // Static Fields Are Thread Static Rule

		#region Create user

		public void TestCreateSCIMUser()
		{
			var repo = new UserRepository(scimSchemaQueryRepository);

			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX", "AU", "ENG", "123", "456", "789", "", "SYD", "BRN", "abcd");

			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			var staff = Factory.Load<GlbStaff>(new ZGuid(id));
			AssertEquals("123", staff.GS_ExternalId);
			AssertEquals("scimusername", staff.GS_LoginName);
			AssertEquals("first middle last", staff.GS_FullName);
			AssertEquals("1 George St", staff.GS_UserAddress1);
			AssertEquals("Sydney", staff.GS_City);
			AssertEquals("first", staff.GS_GivenName);
			AssertEquals("middle", staff.GS_MiddleName);
			AssertEquals("last", staff.GS_Surname);
			AssertEquals("0499123456", staff.GS_MobilePhone);
			AssertEquals("test@email.com", staff.GS_EmailAddress);
			AssertEquals("title", staff.GS_Title);
			AssertEquals("III", staff.GS_NameSuffix);
			AssertEquals("XX", staff.GS_NameTitle);
			AssertEquals("AU", staff.GS_RN_NKCountryCode);
			AssertEquals("EN", staff.GS_WorkingLanguage);
			AssertEquals("123", staff.GS_HomePhone);
			AssertEquals("456", staff.GS_WorkPhone);
			AssertEquals("789", staff.GS_FaxNum);
			AssertEquals(true, staff.GS_CanLogin);
			AssertEquals("SYD", staff.HomeBranch.GB_Code);
			AssertEquals("BRN", staff.HomeDepartment.GE_Code);
			AssertEquals("abcd", staff.GS_Pager);
		}

		public void TestCreateUser_Branch_NotExists()
		{
			var repo = new UserRepository(scimSchemaQueryRepository);

			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX", "AU", "ENG", "123", "456", "789", "", "XXX", "BRN", "abcd");

			AssertExceptionThrown<SCIMNoTargetException>(delegate
			{
				repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult();
			});
		}

		public void TestCreateUser_Department_NotExists()
		{
			var repo = new UserRepository(scimSchemaQueryRepository);

			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX", "AU", "ENG", "123", "456", "789", "", "SYD", "XXX", "abcd");

			AssertExceptionThrown<SCIMNoTargetException>(delegate
			{
				repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult();
			});
		}

		public void TestCreateSCIMUser_CanLogin_False()
		{
			AssertCanLoginDefault(false);
		}

		public void TestCreateSCIMUser_CanLogin_True()
		{
			AssertCanLoginDefault(true);
		}

		void AssertCanLoginDefault(bool canLogin)
		{
			Env.Registry.ScimCanLogin = canLogin;
			var repo = new UserRepository(scimSchemaQueryRepository);

			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX", "AU", "ENG", "123", "456", "789");

			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			var staff = Factory.Load<GlbStaff>(new ZGuid(id));
			AssertEquals(canLogin, staff.GS_CanLogin);
		}

		public void TestCreateSCIMUser_FullName()
		{
			var repo = new UserRepository(scimSchemaQueryRepository);

			var scimUser1 = ScimProcessorTestHelper.CreateScimUser("1", "1", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "1@email.com", "title", "III", "XX");
			var scimUser2 = ScimProcessorTestHelper.CreateScimUser("2", "2", "XX first middle last, III", "XX first", "middle", "last III", "1 George St", "Sydney", "0499123456", "1@email.com", "title", "III", "XX");
			var scimUser3 = ScimProcessorTestHelper.CreateScimUser("3", "3", "XX first middle last", "XX first", "middle", "last", "1 George St", "Sydney", "0499123456", "1@email.com", "title", "", "XX");
			var scimUser4 = ScimProcessorTestHelper.CreateScimUser("4", "4", "XX first middle last, III,, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "1@email.com", "title", "III,, III", "XX");
			var scimUser5 = ScimProcessorTestHelper.CreateScimUser("5", "5", "XX YY  first middle last, ,,,", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "1@email.com", "title", ",,,", "XX YY ");

			string id1 = repo.CreateSCIMResource(scimUser1).GetAwaiter().GetResult().Id.ToString();
			string id2 = repo.CreateSCIMResource(scimUser2).GetAwaiter().GetResult().Id.ToString();
			string id3 = repo.CreateSCIMResource(scimUser3).GetAwaiter().GetResult().Id.ToString();
			string id4 = repo.CreateSCIMResource(scimUser4).GetAwaiter().GetResult().Id.ToString();
			string id5 = repo.CreateSCIMResource(scimUser5).GetAwaiter().GetResult().Id.ToString();

			var staff1 = Factory.Load<GlbStaff>(new ZGuid(id1));
			var staff2 = Factory.Load<GlbStaff>(new ZGuid(id2));
			var staff3 = Factory.Load<GlbStaff>(new ZGuid(id3));
			var staff4 = Factory.Load<GlbStaff>(new ZGuid(id4));
			var staff5 = Factory.Load<GlbStaff>(new ZGuid(id5));

			AssertEquals("first middle last", staff1.GS_FullName);
			AssertEquals("first middle last", staff2.GS_FullName);
			AssertEquals("first middle last", staff3.GS_FullName);
			AssertEquals("first middle last", staff4.GS_FullName);
			AssertEquals("first middle last", staff5.GS_FullName);
		}

		public void TestCreateSCIMResource_DuplicateExternalId()
		{
			var repo = new UserRepository(scimSchemaQueryRepository);
			string externalId = "123";
			string userName1 = "username1";

			string userName2 = "username2";

			var scimUser = new ScimUser();
			scimUser.UserName = userName1;
			scimUser.ExternalId = externalId;

			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			AssertNotEquals(string.Empty, id);

			scimUser = new ScimUser();
			scimUser.UserName = userName2;
			scimUser.ExternalId = externalId;

			AssertExceptionThrown<SCIMUniquenessAttributeException>(delegate
			{ repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult(); });
		}

		public void TestCreateSCIMResource_DuplicateLoginName()
		{
			var repo = new UserRepository(scimSchemaQueryRepository);
			string externalId1 = "123";
			string externalId2 = "456";

			string userName = "username1";

			var scimUser = new ScimUser();
			scimUser.UserName = userName;
			scimUser.ExternalId = externalId1;

			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();
			AssertNotEquals(string.Empty, id);

			scimUser = new ScimUser();
			scimUser.UserName = userName;
			scimUser.ExternalId = externalId2;

			AssertExceptionThrown<SCIMUniquenessAttributeException>(delegate
			{ repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult(); });
		}

		#endregion

		#region Delete user

		public void TestDeleteSCIMUserById()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_ExternalId = "123";
			staff.GS_IsActive = true;

			Factory.Save();

			var repo = new UserRepository(scimSchemaQueryRepository);
			repo.DeleteSCIMResourceById(staff.PK.ToString()).GetAwaiter().GetResult();

			staff = new BusinessObjectFactory() { RefreshEnabled = false }.Load<GlbStaff>(staff.PK);
			AssertNotNull(staff);
			AssertEquals(false, staff.GS_IsActive);
		}

		public void TestDeleteSCIMUserById_NotExists()
		{
			var repo = new UserRepository(scimSchemaQueryRepository);
			AssertNoExceptionThrown(delegate
			{ repo.DeleteSCIMResourceById(ZGuid.NewZGuid().ToString()).GetAwaiter().GetResult(); });
		}

		#endregion

		#region Find user

		public void TestFind_StaffPaging()
		{
			CreateSomeUsers();

			var repo = new UserRepository(scimSchemaQueryRepository);
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 3, 4, null);
			var users = repo.FindSCIMResource(param).GetAwaiter().GetResult();

			AssertEquals(4, users.Content.Count());
			AssertEquals(new Guid("33333333-F5B2-4186-A908-20C66BE7F77D"), users.Content.ToArray()[0].Id);
			AssertEquals(new Guid("44444444-F5B2-4186-A908-20C66BE7F77D"), users.Content.ToArray()[1].Id);
			AssertEquals(new Guid("55555555-F5B2-4186-A908-20C66BE7F77D"), users.Content.ToArray()[2].Id);
			AssertEquals(new Guid("66666666-F5B2-4186-A908-20C66BE7F77D"), users.Content.ToArray()[3].Id);
		}

		public void TestFind_ZeroCount()
		{
			CreateSomeUsers();

			var repo = new UserRepository(scimSchemaQueryRepository);
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 3, 0, null);
			var users = repo.FindSCIMResource(param).GetAwaiter().GetResult();

			AssertEquals(0, users.Content.Count());
			AssertNotEquals(0, users.TotalResults);
		}

		void CreateSomeUsers()
		{
			using (var cmd = Db.Connection.Command("update glbstaff set GS_IsActive = 0, GS_SystemLastEditTimeUtc = getdate(), GS_SystemLastEditUser = 'X'"))
			{
				cmd.ExecuteNonQuery();
			}

			var sql = @"
INSERT INTO GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser, GS_IsActive)
VALUES ('11111111-F5B2-4186-A908-20C66BE7F77D', '111', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser, GS_IsActive)
VALUES ('99999999-F5B2-4186-A908-20C66BE7F77D', '999', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser, GS_IsActive)
VALUES ('22222222-F5B2-4186-A908-20C66BE7F77D', '222', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser, GS_IsActive)
VALUES ('88888888-F5B2-4186-A908-20C66BE7F77D', '888', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser, GS_IsActive)
VALUES ('33333333-F5B2-4186-A908-20C66BE7F77D', '333', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser, GS_IsActive)
VALUES ('77777777-F5B2-4186-A908-20C66BE7F77D', '777', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser, GS_IsActive)
VALUES ('44444444-F5B2-4186-A908-20C66BE7F77D', '444', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser, GS_IsActive)
VALUES ('66666666-F5B2-4186-A908-20C66BE7F77D', '666', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser, GS_IsActive)
VALUES ('55555555-F5B2-4186-A908-20C66BE7F77D', '555', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser, GS_IsActive)
VALUES ('AAAAAAAA-F5B2-4186-A908-20C66BE7F77D', 'AAA', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser, GS_IsActive)
VALUES ('FFFFFFFF-F5B2-4186-A908-20C66BE7F77D', 'FFF', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser, GS_IsActive)
VALUES ('BBBBBBBB-F5B2-4186-A908-20C66BE7F77D', 'BBB', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser, GS_IsActive)
VALUES ('CCCCCCCC-F5B2-4186-A908-20C66BE7F77D', 'CCC', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser, GS_IsActive)
VALUES ('DDDDDDDD-F5B2-4186-A908-20C66BE7F77D', 'DDD', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);
";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public void TestFind_TotalCount()
		{
			CreateSomeUsers();

			var repo = new UserRepository(scimSchemaQueryRepository);
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 3, 4, null);
			var response = repo.FindSCIMResource(param).GetAwaiter().GetResult();

			AssertEquals(4, response.Content.Count());
			AssertEquals(14, response.TotalResults);
		}

		public void TestFindSCIMUser_Id()
		{
			var user = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");

			var repo = new UserRepository(scimSchemaQueryRepository);
			string id = repo.CreateSCIMResource(user).GetAwaiter().GetResult().Id.ToString();
			AssertNotEquals(string.Empty, id);

			var foundUser = repo.FindSCIMByResourceId(id).GetAwaiter().GetResult();
			AssertNotNull(foundUser);

			if (foundUser == null)
			{
				return;
			}

			AssertEquals("123", foundUser.ExternalId);
			AssertEquals("scimusername", foundUser.UserName);
			AssertEquals("XX first middle last, III", foundUser.NameFormatted);
			AssertEquals("1 George St", foundUser.AddressesStreetAddress);
			AssertEquals("Sydney", foundUser.AddressesLocality);
			AssertEquals("0499123456", foundUser.PhoneNumbersMobile);
			AssertEquals("test@email.com", foundUser.Email);
		}

		public void TestFindSCIMUser_Id_SystemAccount()
		{
			var cwSupport = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, ZArchitecture.Environment.User.SupportUserName));
			var repo = new UserRepository(scimSchemaQueryRepository);

			var foundUser = repo.FindSCIMByResourceId(cwSupport.PK.ToString()).GetAwaiter().GetResult();
			AssertNull(foundUser);
		}

		public void TestFindSCIMUser_ExternalId()
		{
			var userSchema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:User", "User", "User Account")
			  .AddStringAttribute("userName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
			  .Build();

			var user1 = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "first1", "middle1", "last1", "address1", "city1", "0499123456", "test1@user.com");
			var user2 = ScimProcessorTestHelper.CreateScimUser("ext2", "userName2", "first2", "middle2", "last2", "address2", "city2", "0499456789", "test2@user.com");

			var repo = new UserRepository(scimSchemaQueryRepository);
			string id1 = repo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id.ToString();
			string id2 = repo.CreateSCIMResource(user2).GetAwaiter().GetResult().Id.ToString();

			var filter = SCIMFilterParser.Parse("externalId eq \"ext2\"", new List<SCIMSchema> { userSchema });
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null, filter: filter);
			var foundUser = repo.FindSCIMResource(param).GetAwaiter().GetResult().Content.FirstOrDefault();

			AssertNotNull(foundUser);
			if (foundUser == null)
			{
				return;
			}

			AssertEquals("ext2", foundUser.ExternalId);
			AssertEquals(id2, foundUser.Id.ToString());
		}

		public void TestFindSCIMUser_MultipleFilters()
		{
			var userSchema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:User", "User", "User Account")
			  .AddStringAttribute("userName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
			  .Build();

			var user1 = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "first1", "middle1", "last1", "address1", "city1", "0499123456", "test1@user.com");
			var user2 = ScimProcessorTestHelper.CreateScimUser("ext2", "userName2", "first2", "middle2", "last2", "address2", "city2", "0499456789", "test2@user.com");

			var repo = new UserRepository(scimSchemaQueryRepository);
			string id1 = repo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id.ToString();
			string id2 = repo.CreateSCIMResource(user2).GetAwaiter().GetResult().Id.ToString();

			var filter = SCIMFilterParser.Parse($"externalId eq \"ext2\" and userName eq \"userName2\" and id eq \"{id2}\"", new List<SCIMSchema> { userSchema });
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null, filter: filter);
			var foundUser = repo.FindSCIMResource(param).GetAwaiter().GetResult().Content.FirstOrDefault();

			AssertNotNull(foundUser);
			if (foundUser == null)
			{
				return;
			}

			AssertEquals("ext2", foundUser.ExternalId);
			AssertEquals(id2, foundUser.Id.ToString());
		}

		public void TestFindSCIMUser_MultipleResults()
		{
			var userSchema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:User", "User", "User Account")
			  .AddStringAttribute("userName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
			  .Build();

			var user1 = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "full name", "firstname", "middle1", "last1", "address1", "city1", "0499123456", "test1@user.com");
			var user2 = ScimProcessorTestHelper.CreateScimUser("ext2", "userName2", "full name", "firstname", "middle2", "last2", "address2", "city2", "0499456789", "test2@user.com");
			var user3 = ScimProcessorTestHelper.CreateScimUser("ext3", "userName3", "full name", "other", "middle3", "last3", "address3", "city3", "0499456000", "test3@user.com");

			var repo = new UserRepository(scimSchemaQueryRepository);
			string id1 = repo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id.ToString();
			string id2 = repo.CreateSCIMResource(user2).GetAwaiter().GetResult().Id.ToString();
			string id3 = repo.CreateSCIMResource(user3).GetAwaiter().GetResult().Id.ToString();

			var filter = SCIMFilterParser.Parse("givenName eq \"firstname\"", new List<SCIMSchema> { userSchema });
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null, filter: filter);
			var result = repo.FindSCIMResource(param).GetAwaiter().GetResult();

			AssertNotNull(result);
			AssertEquals(2, result.TotalResults);
			Assert("Results should contain user1", result.Content.Cast<ScimUser>().Any(u => u.ExternalId == "ext1"));
			Assert("Results should contain user2", result.Content.Cast<ScimUser>().Any(u => u.ExternalId == "ext2"));
		}

		public void TestFindComplex()
		{
			var userSchema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:User", "User", "User Account")
			  .AddStringAttribute("userName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
			  .Build();

			var user1 = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "full name", "first", "middle1", "last1", "address1", "city1", "0499123456", "test1@user.com");
			var user2 = ScimProcessorTestHelper.CreateScimUser("ext2", "userName2", "full name", "first", "middle2", "last2", "address2", "city2", "0499456789", "test2@user.com");
			var user3 = ScimProcessorTestHelper.CreateScimUser("ext3", "userName3", "full name", "other", "middle3", "last3", "address3", "city3", "0499456000", "test3@user.com");

			var repo = new UserRepository(scimSchemaQueryRepository);
			string id1 = repo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id.ToString();
			string id2 = repo.CreateSCIMResource(user2).GetAwaiter().GetResult().Id.ToString();
			string id3 = repo.CreateSCIMResource(user3).GetAwaiter().GetResult().Id.ToString();

			var filter = SCIMFilterParser.Parse("givenName eq \"first\" and (middleName eq \"middle1\" or middleName eq \"middle3\")", new List<SCIMSchema> { userSchema });
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null, filter: filter);
			var result = repo.FindSCIMResource(param).GetAwaiter().GetResult();

			AssertNotNull(result);
			AssertEquals(1, result.TotalResults);
			Assert("Results should contain user1", result.Content.Cast<ScimUser>().Any(u => u.ExternalId == "ext1"));
		}

		public void TestFindEmails()
		{
			var userSchema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:User", "User", "User Account")
			  .AddStringAttribute("userName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
			  .Build();

			var user1 = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "full name", "first", "middle1", "last1", "address1", "city1", "0499123456", "test1@user.com");
			var user2 = ScimProcessorTestHelper.CreateScimUser("ext2", "userName2", "full name", "first", "middle2", "last2", "address2", "city2", "0499456789", "test2@user.com");
			var user3 = ScimProcessorTestHelper.CreateScimUser("ext3", "userName3", "full name", "other", "middle3", "last3", "address3", "city3", "0499456000", "test3@user.com");

			var repo = new UserRepository(scimSchemaQueryRepository);
			string id1 = repo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id.ToString();
			string id2 = repo.CreateSCIMResource(user2).GetAwaiter().GetResult().Id.ToString();
			string id3 = repo.CreateSCIMResource(user3).GetAwaiter().GetResult().Id.ToString();

			Factory.Save();

			var filter = SCIMFilterParser.Parse("emails[type eq \"work\" and value co \"test2\"]", new List<SCIMSchema> { userSchema });
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null, filter: filter);
			var result = repo.FindSCIMResource(param).GetAwaiter().GetResult();

			AssertNotNull(result);
			AssertEquals(1, result.TotalResults);
			Assert("Results should contain user2", result.Content.Cast<ScimUser>().Any(u => u.ExternalId == "ext2"));
		}

		public void TestFindAddresses()
		{
			var userSchema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:User", "User", "User Account")
			  .AddStringAttribute("userName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
			  .Build();

			var user1 = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "full name", "first", "middle1", "last1", "George Street", "Sydney", "0499123456", "test1@user.com", country: "AU", state: "NSW");
			var user2 = ScimProcessorTestHelper.CreateScimUser("ext2", "userName2", "full name", "first", "middle2", "last2", "Finnin Court", "Maudsland", "0499456789", "test2@user.com", country: "AU", state: "QLD");
			var user3 = ScimProcessorTestHelper.CreateScimUser("ext3", "userName3", "full name", "other", "middle3", "last3", "Roberts Street", "Maudsland", "0499456000", "test3@user.com", country: "AU", state: "QLD");

			var repo = new UserRepository(scimSchemaQueryRepository);
			string id1 = repo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id.ToString();
			string id2 = repo.CreateSCIMResource(user2).GetAwaiter().GetResult().Id.ToString();
			string id3 = repo.CreateSCIMResource(user3).GetAwaiter().GetResult().Id.ToString();

			Factory.Save();

			var filter = SCIMFilterParser.Parse("addresses[country eq \"AU\"] and externalId ne \"\"", new List<SCIMSchema> { userSchema });
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null, filter: filter);
			var result = repo.FindSCIMResource(param).GetAwaiter().GetResult();

			AssertNotNull(result);
			AssertEquals(3, result.TotalResults);

			filter = SCIMFilterParser.Parse("addresses[streetAddress co \"street\"] and externalId ne \"\"", new List<SCIMSchema> { userSchema });
			param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null, filter: filter);
			result = repo.FindSCIMResource(param).GetAwaiter().GetResult();

			AssertNotNull(result);
			AssertEquals(2, result.TotalResults);
			Assert("Results should contain user1", result.Content.Cast<ScimUser>().Any(u => u.ExternalId == "ext1"));
			Assert("Results should contain user3", result.Content.Cast<ScimUser>().Any(u => u.ExternalId == "ext3"));

			filter = SCIMFilterParser.Parse("addresses[locality eq \"Maudsland\"] and externalId ne \"\"", new List<SCIMSchema> { userSchema });
			param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null, filter: filter);
			result = repo.FindSCIMResource(param).GetAwaiter().GetResult();

			AssertNotNull(result);
			AssertEquals(2, result.TotalResults);
			Assert("Results should contain user2", result.Content.Cast<ScimUser>().Any(u => u.ExternalId == "ext2"));
			Assert("Results should contain user3", result.Content.Cast<ScimUser>().Any(u => u.ExternalId == "ext3"));

			filter = SCIMFilterParser.Parse("addresses[locality eq \"Maudsland\" and streetAddress co \"street\"] and externalId ne \"\"", new List<SCIMSchema> { userSchema });
			param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null, filter: filter);
			result = repo.FindSCIMResource(param).GetAwaiter().GetResult();

			AssertNotNull(result);
			AssertEquals(1, result.TotalResults);
			Assert("Results should contain user3", result.Content.Cast<ScimUser>().Any(u => u.ExternalId == "ext3"));

			filter = SCIMFilterParser.Parse("addresses[region eq \"NSW\" and streetAddress co \"street\"] and externalId ne \"\"", new List<SCIMSchema> { userSchema });
			param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null, filter: filter);
			result = repo.FindSCIMResource(param).GetAwaiter().GetResult();

			AssertNotNull(result);
			AssertEquals(1, result.TotalResults);
			Assert("Results should contain user1", result.Content.Cast<ScimUser>().Any(u => u.ExternalId == "ext1"));

			filter = SCIMFilterParser.Parse("addresses[region eq \"NSW\" or streetAddress eq \"Roberts Street\"] and externalId ne \"\"", new List<SCIMSchema> { userSchema });
			param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null, filter: filter);
			result = repo.FindSCIMResource(param).GetAwaiter().GetResult();

			AssertNotNull(result);
			AssertEquals(2, result.TotalResults);
			Assert("Results should contain user1", result.Content.Cast<ScimUser>().Any(u => u.ExternalId == "ext1"));
			Assert("Results should contain user3", result.Content.Cast<ScimUser>().Any(u => u.ExternalId == "ext3"));
		}

		public void TestFindLessThan()
		{
			var userSchema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:User", "User", "User Account")
			  .AddStringAttribute("userName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
			  .Build();

			var user1 = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "full name", "first", "middle1", "last1", "address1", "city1", "0499123456", "test1@user.com");
			var user2 = ScimProcessorTestHelper.CreateScimUser("ext2", "userName2", "full name", "first", "middle2", "last2", "address2", "city2", "0499456789", "test2@user.com");

			var repo = new UserRepository(scimSchemaQueryRepository);
			string id1 = repo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id.ToString();
			string id2 = repo.CreateSCIMResource(user2).GetAwaiter().GetResult().Id.ToString();

			var staff1 = Factory.Load<GlbStaff>(new ZGuid(id1));
			staff1.GS_SystemCreateTimeUtc = DateTime.UtcNow.AddDays(-5);

			Factory.Save();

			var filter = SCIMFilterParser.Parse($"externalId eq \"ext1\" and meta.created lt \"{DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-dd")}T04:42:34Z\"", new List<SCIMSchema> { userSchema });
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null, filter: filter);
			var result = repo.FindSCIMResource(param).GetAwaiter().GetResult();

			AssertNotNull(result);
			AssertEquals(1, result.TotalResults);
			Assert("Results should contain user1", result.Content.Cast<ScimUser>().Any(u => u.ExternalId == "ext1"));

			filter = SCIMFilterParser.Parse($"externalId eq \"ext2\" and meta.created gt \"{DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-dd")}T04:42:34Z\"", new List<SCIMSchema> { userSchema });
			param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null, filter: filter);
			result = repo.FindSCIMResource(param).GetAwaiter().GetResult();

			AssertNotNull(result);
			AssertEquals(1, result.TotalResults);
			Assert("Results should contain user2", result.Content.Cast<ScimUser>().Any(u => u.ExternalId == "ext2"));
		}

		public void TestFind_IncludedAttribute()
		{
			var userSchema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:User", "User", "User Account")
				 .AddStringAttribute("userName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
				 .Build();

			var filter = SCIMFilterParser.Parse("externalId ne \"\"", new List<SCIMSchema> { userSchema });

			var user1 = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "first1 middle1 last1", "first1", "middle1", "last1", "address1", "city1", "0499123456", "test1@user.com");
			var user2 = ScimProcessorTestHelper.CreateScimUser("ext2", "userName2", "first2 middle2 last2", "first2", "middle2", "last2", "address2", "city2", "0499456789", "test2@user.com");

			var repo = new UserRepository(scimSchemaQueryRepository);
			string id1 = repo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id.ToString();
			string id2 = repo.CreateSCIMResource(user2).GetAwaiter().GetResult().Id.ToString();

			var attribute = new SCIMAttributeExpression("userName");
			var list = new List<SCIMAttributeExpression>();
			list.Add(attribute);

			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null, filter: filter, includedAttributes: list);
			var foundUsers = repo.FindSCIMResource(param).GetAwaiter().GetResult().Content.ToList();

			AssertEquals(2, foundUsers.Count);

			user1 = foundUsers[0];
			user2 = foundUsers[1];

			if (user1 == null || user2 == null)
			{
				Fail();
				return;
			}

			if (user1.UserName == "userName1")
			{
				AssertEquals("userName1", user1.UserName);
				AssertEquals("userName2", user2.UserName);
			}
			else
			{
				AssertEquals("userName2", user1.UserName);
				AssertEquals("userName1", user2.UserName);
			}

			AssertEquals(null, user1.ExternalId);
			AssertEquals(string.Empty, user1.GivenName);
			AssertEquals(string.Empty, user1.MiddleName);
			AssertEquals(string.Empty, user1.FamilyName);
			AssertEquals(string.Empty, user1.AddressesStreetAddress);
			AssertEquals(string.Empty, user1.AddressesLocality);
			AssertEquals(string.Empty, user1.AddressesCountry);
			AssertEquals(string.Empty, user1.PhoneNumbersMobile);
			AssertEquals(string.Empty, user1.Email);

			AssertEquals(null, user2.ExternalId);
			AssertEquals(string.Empty, user2.GivenName);
			AssertEquals(string.Empty, user2.MiddleName);
			AssertEquals(string.Empty, user2.FamilyName);
			AssertEquals(string.Empty, user2.AddressesStreetAddress);
			AssertEquals(string.Empty, user2.AddressesLocality);
			AssertEquals(string.Empty, user2.AddressesCountry);
			AssertEquals(string.Empty, user2.PhoneNumbersMobile);
			AssertEquals(string.Empty, user2.Email);
		}

		public void TestFind_Included_Empty()
		{
			var userSchema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:User", "User", "User Account")
				 .AddStringAttribute("userName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
				 .Build();

			var filter = SCIMFilterParser.Parse("externalId ne \"\"", new List<SCIMSchema> { userSchema });

			var user1 = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "first1 middle1 last1", "first1", "middle1", "last1", "address1", "city1", "0499123456", "test1@user.com");
			var user2 = ScimProcessorTestHelper.CreateScimUser("ext2", "userName2", "first2 middle2 last2", "first2", "middle2", "last2", "address2", "city2", "0499456789", "test2@user.com");

			var repo = new UserRepository(scimSchemaQueryRepository);
			string id1 = repo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id.ToString();
			string id2 = repo.CreateSCIMResource(user2).GetAwaiter().GetResult().Id.ToString();

			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null, filter: filter, includedAttributes: new List<SCIMAttributeExpression>());
			var foundUsers = repo.FindSCIMResource(param).GetAwaiter().GetResult().Content.ToList();

			AssertEquals(2, foundUsers.Count);
			if (foundUsers[0] == null || foundUsers[1] == null)
			{
				Fail();
				return;
			}

			user1 = foundUsers[0].UserName == "userName1" ? foundUsers[0] : foundUsers[1];
			user2 = user1 == foundUsers[0] ? foundUsers[1] : foundUsers[0];

			AssertEquals("userName1", user1.UserName);
			AssertEquals("userName2", user2.UserName);

			AssertEquals("ext1", user1.ExternalId);
			AssertEquals("userName1", user1.UserName);
			AssertEquals("first1 middle1 last1", user1.NameFormatted);
			AssertEquals("first1", user1.GivenName);
			AssertEquals("middle1", user1.MiddleName);
			AssertEquals("last1", user1.FamilyName);
			AssertEquals("address1", user1.AddressesStreetAddress);
			AssertEquals("city1", user1.AddressesLocality);
			AssertEquals("0499123456", user1.PhoneNumbersMobile);
			AssertEquals("test1@user.com", user1.Email);

			AssertEquals("userName2", user2.UserName);
			AssertEquals("ext2", user2.ExternalId);
			AssertEquals("first2 middle2 last2", user2.NameFormatted);
			AssertEquals("first2", user2.GivenName);
			AssertEquals("middle2", user2.MiddleName);
			AssertEquals("last2", user2.FamilyName);
			AssertEquals("address2", user2.AddressesStreetAddress);
			AssertEquals("city2", user2.AddressesLocality);
			AssertEquals("0499456789", user2.PhoneNumbersMobile);
			AssertEquals("test2@user.com", user2.Email);
		}

		public void TestFind_ExcludedAttribute()
		{
			var userSchema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:User", "User", "User Account")
				 .AddStringAttribute("userName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
				 .Build();

			var filter = SCIMFilterParser.Parse("externalId ne \"\"", new List<SCIMSchema> { userSchema });

			var user1 = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "first1 middle1 last1", "first1", "middle1", "last1", "address1", "city1", "0499123456", "test1@user.com");
			var user2 = ScimProcessorTestHelper.CreateScimUser("ext2", "userName2", "first2 middle2 last2", "first2", "middle2", "last2", "address2", "city2", "0499456789", "test2@user.com");

			var repo = new UserRepository(scimSchemaQueryRepository);
			string id1 = repo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id.ToString();
			string id2 = repo.CreateSCIMResource(user2).GetAwaiter().GetResult().Id.ToString();

			var attribute = new SCIMAttributeExpression(AttributeNames.GivenName);
			var list = new List<SCIMAttributeExpression>();
			list.Add(attribute);

			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null, filter: filter, excludedAttributes: list);
			var foundUsers = repo.FindSCIMResource(param).GetAwaiter().GetResult().Content.ToList();

			AssertEquals(2, foundUsers.Count);
			if (foundUsers[0] == null || foundUsers[1] == null)
			{
				Fail();
				return;
			}

			user1 = foundUsers[0].UserName == "userName1" ? foundUsers[0] : foundUsers[1];
			user2 = user1 == foundUsers[0] ? foundUsers[1] : foundUsers[0];

			AssertEquals("userName1", user1.UserName);
			AssertEquals("userName2", user2.UserName);

			AssertEquals("ext1", user1.ExternalId);
			AssertEquals("userName1", user1.UserName);
			AssertEquals(string.Empty, user1.GivenName);
			AssertEquals("middle1", user1.MiddleName);
			AssertEquals("last1", user1.FamilyName);
			AssertEquals("address1", user1.AddressesStreetAddress);
			AssertEquals("city1", user1.AddressesLocality);
			AssertEquals("0499123456", user1.PhoneNumbersMobile);
			AssertEquals("test1@user.com", user1.Email);

			AssertEquals("userName2", user2.UserName);
			AssertEquals("ext2", user2.ExternalId);
			AssertEquals(string.Empty, user2.GivenName);
			AssertEquals("middle2", user2.MiddleName);
			AssertEquals("last2", user2.FamilyName);
			AssertEquals("address2", user2.AddressesStreetAddress);
			AssertEquals("city2", user2.AddressesLocality);
			AssertEquals("0499456789", user2.PhoneNumbersMobile);
			AssertEquals("test2@user.com", user2.Email);
		}

		public void TestFind_Excluded_Empty()
		{
			var userSchema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:User", "User", "User Account")
				 .AddStringAttribute("userName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
				 .Build();

			var filter = SCIMFilterParser.Parse("externalId ne \"\"", new List<SCIMSchema> { userSchema });

			var user1 = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "first1 middle1 last1", "first1", "middle1", "last1", "address1", "city1", "0499123456", "test1@user.com");
			var user2 = ScimProcessorTestHelper.CreateScimUser("ext2", "userName2", "first2 middle2 last2", "first2", "middle2", "last2", "address2", "city2", "0499456789", "test2@user.com");

			var repo = new UserRepository(scimSchemaQueryRepository);
			string id1 = repo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id.ToString();
			string id2 = repo.CreateSCIMResource(user2).GetAwaiter().GetResult().Id.ToString();

			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null, filter: filter, excludedAttributes: new List<SCIMAttributeExpression>());
			var foundUsers = repo.FindSCIMResource(param).GetAwaiter().GetResult().Content.ToList();

			AssertEquals(2, foundUsers.Count);
			if (foundUsers[0] == null || foundUsers[1] == null)
			{
				Fail();
				return;
			}

			user1 = foundUsers[0].UserName == "userName1" ? foundUsers[0] : foundUsers[1];
			user2 = user1 == foundUsers[0] ? foundUsers[1] : foundUsers[0];

			AssertEquals("userName1", user1.UserName);
			AssertEquals("userName2", user2.UserName);

			AssertEquals("ext1", user1.ExternalId);
			AssertEquals("userName1", user1.UserName);
			AssertEquals("first1 middle1 last1", user1.NameFormatted);
			AssertEquals("first1", user1.GivenName);
			AssertEquals("middle1", user1.MiddleName);
			AssertEquals("last1", user1.FamilyName);
			AssertEquals("address1", user1.AddressesStreetAddress);
			AssertEquals("city1", user1.AddressesLocality);
			AssertEquals("0499123456", user1.PhoneNumbersMobile);
			AssertEquals("test1@user.com", user1.Email);

			AssertEquals("userName2", user2.UserName);
			AssertEquals("ext2", user2.ExternalId);
			AssertEquals("first2 middle2 last2", user2.NameFormatted);
			AssertEquals("first2", user2.GivenName);
			AssertEquals("middle2", user2.MiddleName);
			AssertEquals("last2", user2.FamilyName);
			AssertEquals("address2", user2.AddressesStreetAddress);
			AssertEquals("city2", user2.AddressesLocality);
			AssertEquals("0499456789", user2.PhoneNumbersMobile);
			AssertEquals("test2@user.com", user2.Email);
		}

		[TestDate(2023, 1, 1)]
		public void TestFindSCIMUser_Deleted_ById()
		{
			var user = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "first1", "middle1", "last1", "address1", "city1", "0499123456", "test1@user.com");

			var repo = new UserRepository(scimSchemaQueryRepository);
			string id = repo.CreateSCIMResource(user).GetAwaiter().GetResult().Id.ToString();
			repo.DeleteSCIMResourceById(id);

			TestDateAttribute.AddDays(40);

			var userById = repo.FindSCIMByResourceId(id).GetAwaiter().GetResult();
			AssertNull(userById);
		}

		public void TestFindSCIMUser_Deleted_ById_Recent()
		{
			var user = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "first1", "middle1", "last1", "address1", "city1", "0499123456", "test1@user.com");

			var repo = new UserRepository(scimSchemaQueryRepository);
			string id = repo.CreateSCIMResource(user).GetAwaiter().GetResult().Id.ToString();
			repo.DeleteSCIMResourceById(id);

			var userById = repo.FindSCIMByResourceId(id).GetAwaiter().GetResult();
			AssertNotNull(userById);
		}

		public void TestFindSCIMUser_Deleted_Filter_Recent()
		{
			var userSchema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:User", "User", "User Account")
			  .AddStringAttribute("userName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
			  .Build();

			var user = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "first1", "middle1", "last1", "address1", "city1", "0499123456", "test1@user.com");

			var repo = new UserRepository(scimSchemaQueryRepository);
			string id = repo.CreateSCIMResource(user).GetAwaiter().GetResult().Id.ToString();
			repo.DeleteSCIMResourceById(id);

			var filter = SCIMFilterParser.Parse("externalId eq \"ext1\"", new List<SCIMSchema> { userSchema });
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null, filter: filter);
			var foundUser = repo.FindSCIMResource(param).GetAwaiter().GetResult().Content.FirstOrDefault();

			AssertNotNull(foundUser);
		}

		[TestDate(2023, 1, 1)]
		public void TestFindSCIMUser_Deleted_Filter()
		{
			var userSchema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:User", "User", "User Account")
			  .AddStringAttribute("userName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
			  .Build();

			var user = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "first1", "middle1", "last1", "address1", "city1", "0499123456", "test1@user.com");

			var repo = new UserRepository(scimSchemaQueryRepository);
			string id = repo.CreateSCIMResource(user).GetAwaiter().GetResult().Id.ToString();
			repo.DeleteSCIMResourceById(id);

			TestDateAttribute.AddDays(40);

			var filter = SCIMFilterParser.Parse("externalId eq \"ext1\"", new List<SCIMSchema> { userSchema });
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null, filter: filter);
			var foundUser = repo.FindSCIMResource(param).GetAwaiter().GetResult().Content.FirstOrDefault();

			AssertNull(foundUser);
		}

		public void TestFindSCIMUser_NotFound()
		{
			var userSchema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:User", "User", "User Account")
				 .AddStringAttribute("userName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
				 .Build();

			var repo = new UserRepository(scimSchemaQueryRepository);
			var filter = SCIMFilterParser.Parse("externalId eq \"ext1\"", new List<SCIMSchema> { userSchema });
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null, filter: filter);
			var result = repo.FindSCIMResource(param).GetAwaiter().GetResult();

			AssertEquals(0, result.TotalResults);
		}

		public void TestFindSCIMUser_NotFound_Id()
		{
			var repo = new UserRepository(scimSchemaQueryRepository);
			var result = repo.FindSCIMByResourceId(Guid.NewGuid().ToString()).GetAwaiter().GetResult();

			AssertNull(result);
		}

		public void TestFindSCIMUser_NonUserTypes()
		{
			var systemStaff = Factory.New<GlbStaff>();
			systemStaff.GS_LoginName = "login1";
			systemStaff.GS_IsSystemAccount = true;

			var resourceStaff = Factory.New<GlbStaff>();
			resourceStaff.GS_LoginName = "login2";
			resourceStaff.GS_IsResource = true;

			var robotJason = Factory.New<GlbStaff>();
			robotJason.GS_LoginName = "login3";
			robotJason.GS_IsRobot = true;

			var device = Factory.New<GlbStaff>();
			device.GS_LoginName = "login4";
			device.GS_IsDevice = true;

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "login5";

			Factory.Save();

			var repo = new UserRepository(scimSchemaQueryRepository);
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 1, 100, null);

			var foundUsers = repo.FindSCIMResource(param).GetAwaiter().GetResult();
			AssertNotNull(foundUsers);

			AssertNotEquals(0, foundUsers.TotalResults);

			bool found = false;
			foreach (var user in foundUsers.Content)
			{
				AssertNotEquals(systemStaff.PK.ToGuid(), user.Id);
				AssertNotEquals(resourceStaff.PK.ToGuid(), user.Id);
				AssertNotEquals(robotJason.PK.ToGuid(), user.Id);
				AssertNotEquals(device.PK.ToGuid(), user.Id);

				if (user.Id == staff.PK.ToGuid())
				{
					found = true;
				}
			}

			Assert(found);
		}

		#endregion

		#region Patch user

		public void TestPatchSystemAccount()
		{
			Clean_GS_ExternalId();
			var staffs = Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_IsSystemAccount, true));

			AssertNotEquals(0, staffs.Length);

			var repo = new UserRepository(scimSchemaQueryRepository);

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			PatchOperationParameter patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.ADD,
				Path = "externalId",
				Value = "newExternal"
			};

			param.Operations.Add(patch);

			foreach (var staff in staffs)
			{
				if (staff.GS_IsActive)
				{
					AssertExceptionThrown<SCIMNoTargetException>(delegate
					{
						var result = repo.PatchSCIMResourceById(staff.PK.ToString(), param).GetAwaiter().GetResult();
						AssertNull(result);
					});
				}
				else
				{
					AssertExceptionThrown<SCIMNotFoundException>(delegate
					{
						var result = repo.PatchSCIMResourceById(staff.PK.ToString(), param).GetAwaiter().GetResult();
						AssertNull(result);
					});
				}
			}

			var newFactory = new BusinessObjectFactory();
			foreach (var staff in staffs)
			{
				var staffReloaded = newFactory.Load<GlbStaff>(staff.PK);

				AssertEquals(string.Empty, staffReloaded.GS_ExternalId);
			}
		}

		public void TestPatchExternalId_LastController()
		{
			Clean_GS_ExternalId();
			string id = Env.CurrentUser.PK.ToString();

			using (var cmd = Db.Connection.Command($"update GlbStaff set GS_IsController = 0, GS_SystemLastEditTimeUtc = getdate(), GS_SystemLastEditUser = 'X' where GS_PK <> '{id}'"))
			{
				cmd.ExecuteNonQuery();
			}

			var staffs = Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_IsSystemAccount, true));

			foreach (var s in staffs)
			{
				s.GS_IsSystemAccount = false;
			}

			Factory.Save();

			var repo = new UserRepository(scimSchemaQueryRepository);

			var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			staff.GS_ExternalId = ZString.Empty;
			staff.IsBackupOperator = true;
			staff.IsDatabaseDeveloper = true;
			staff.IsReadOnlyDBUser = true;
			staff.GS_IsController = true;
			staff.GS_CanLogin = true;
			staff.GS_IsDeveloper = true;
			staff.GS_IsRobot = true;
			staff.GS_MobilePhone = "123";

			Factory.Save();

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			PatchOperationParameter patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.ADD,
				Path = "externalId",
				Value = "newExternal"
			};

			param.Operations.Add(patch);

			repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();
			staff = new BusinessObjectFactory().Load<GlbStaff>(new ZGuid(id));
			AssertEquals(false, staff.IsBackupOperator);
			AssertEquals(false, staff.IsDatabaseDeveloper);
			AssertEquals(false, staff.IsReadOnlyDBUser);
			AssertEquals("Must stay true because this is the last controller", true, staff.GS_IsController);
			AssertEquals(true, staff.GS_CanLogin);
			AssertEquals(true, staff.GS_IsDeveloper);
			AssertEquals(true, staff.GS_IsRobot);
			AssertEquals("newExternal", staff.GS_ExternalId);
		}

		public void TestPatchExternalId_NonLastController()
		{
			Clean_GS_ExternalId();
			var repo = new UserRepository(scimSchemaQueryRepository);
			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");
			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			var controller = Factory.NewWithValidTestData<GlbStaff>();
			controller.GS_IsController = true;

			var staff = Factory.Load<GlbStaff>(new ZGuid(id));
			staff.GS_ExternalId = ZString.Empty;
			staff.IsBackupOperator = true;
			staff.IsDatabaseDeveloper = true;
			staff.IsReadOnlyDBUser = true;
			staff.GS_IsController = true;
			staff.GS_IsOperational = true;
			staff.GS_CanLogin = true;
			staff.GS_IsDeveloper = true;
			staff.GS_IsRobot = true;
			Factory.Save();

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			PatchOperationParameter patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.ADD,
				Path = "externalId",
				Value = "newExternal"
			};

			param.Operations.Add(patch);

			repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();

			staff = new BusinessObjectFactory().Load<GlbStaff>(new ZGuid(id));
			AssertEquals(false, staff.IsBackupOperator);
			AssertEquals(false, staff.IsDatabaseDeveloper);
			AssertEquals(false, staff.IsReadOnlyDBUser);
			AssertEquals(false, staff.GS_IsController);
			AssertEquals(true, staff.GS_IsOperational);
			AssertEquals(true, staff.GS_CanLogin);
			AssertEquals(true, staff.GS_IsDeveloper);
			AssertEquals(true, staff.GS_IsRobot);
			AssertEquals("newExternal", staff.GS_ExternalId);
		}

		public void TestPatchExternalId_Exists()
		{
			Clean_GS_ExternalId();
			var repo = new UserRepository(scimSchemaQueryRepository);
			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");
			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			var staff = Factory.Load<GlbStaff>(new ZGuid(id));
			staff.IsBackupOperator = true;
			staff.IsDatabaseDeveloper = true;
			staff.IsReadOnlyDBUser = true;
			staff.GS_IsController = true;
			staff.GS_IsOperational = true;
			staff.GS_CanLogin = true;
			staff.GS_IsDeveloper = true;
			staff.GS_IsRobot = true;
			staff.GS_IsSalesRep = true;
			staff.GS_IsDriver = true;
			Factory.Save();

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			PatchOperationParameter patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.ADD,
				Path = "externalId",
				Value = "newExternal"
			};

			param.Operations.Add(patch);

			repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();

			staff = Factory.Load<GlbStaff>(new ZGuid(id));
			AssertEquals(true, staff.IsBackupOperator);
			AssertEquals(true, staff.IsDatabaseDeveloper);
			AssertEquals(true, staff.IsReadOnlyDBUser);
			AssertEquals(true, staff.GS_IsController);
			AssertEquals(true, staff.GS_IsOperational);
			AssertEquals(true, staff.GS_CanLogin);
			AssertEquals(true, staff.GS_IsDeveloper);
			AssertEquals(true, staff.GS_IsRobot);
			AssertEquals(true, staff.GS_IsSalesRep);
			AssertEquals(true, staff.GS_IsDriver);
			AssertEquals("newExternal", staff.GS_ExternalId);
		}

		public void TestPatchExternalId_Roles()
		{
			SystemDataRegistry.Instance.ScimClearRoleFlagsOnMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Clean_GS_ExternalId();
			var repo = new UserRepository(scimSchemaQueryRepository);
			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");
			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			var controller = Factory.NewWithValidTestData<GlbStaff>();
			controller.GS_IsController = true;

			var staff = Factory.Load<GlbStaff>(new ZGuid(id));
			staff.GS_ExternalId = ZString.Empty;
			staff.GS_IsOperational = true;
			staff.GS_CanLogin = true;
			staff.GS_IsDeveloper = true;
			staff.GS_IsDevice = true;
			staff.GS_IsRobot = true;
			staff.GS_IsSalesRep = true;
			staff.GS_IsDriver = true;
			Factory.Save();

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			PatchOperationParameter patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.ADD,
				Path = "externalId",
				Value = "newExternal"
			};

			param.Operations.Add(patch);

			repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();

			staff = new BusinessObjectFactory().Load<GlbStaff>(new ZGuid(id));
			AssertEquals(true, staff.GS_IsOperational);
			AssertEquals(true, staff.GS_CanLogin);
			AssertEquals(true, staff.GS_IsDeveloper);
			AssertEquals(true, staff.GS_IsDevice);
			AssertEquals(true, staff.GS_IsRobot);
			AssertEquals(false, staff.GS_IsSalesRep);
			AssertEquals(false, staff.GS_IsDriver);
			AssertEquals("newExternal", staff.GS_ExternalId);
		}

		public void TestPatchExternalId_Log()
		{
			Clean_GS_ExternalId();

			var repo = new UserRepository(scimSchemaQueryRepository);
			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");
			var id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			var controller = Factory.NewWithValidTestData<GlbStaff>();
			controller.GS_IsController = true;

			var staff = Factory.Load<GlbStaff>(new ZGuid(id));
			staff.GS_ExternalId = ZString.Empty;
			staff.IsBackupOperator = true;
			staff.IsDatabaseDeveloper = true;
			staff.IsReadOnlyDBUser = true;
			staff.GS_IsController = true;
			staff.GS_IsOperational = true;
			staff.GS_CanLogin = true;
			staff.GS_IsDeveloper = true;
			staff.GS_IsRobot = true;
			Factory.Save();

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			var patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.ADD,
				Path = "externalId",
				Value = "newExternal"
			};

			param.Operations.Add(patch);

			repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();

			Factory.Save();
			staff = new BusinessObjectFactory().Load<GlbStaff>(new ZGuid(id));

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "Matched to external id");
			var logs = staff.Logs.Find(query);
			AssertNotNull(logs);
			AssertEquals(1, logs.Length);
			AssertEquals("Matched to external id [newExternal].", logs[0].SL_Reference);

			param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "externalId",
				Value = "veryNewExternal"
			};

			param.Operations.Add(patch);

			repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();
			Factory.Save();

			staff = Factory.Load<GlbStaff>(new ZGuid(id));
			logs = staff.Logs.Find(query);
			AssertNotNull(logs);
			AssertEquals(2, logs.Length);
			if (logs[0].SL_Reference.Contains("[newExternal]"))
			{
				AssertEquals("Matched to external id [newExternal].", logs[0].SL_Reference);
				AssertEquals("Matched to external id [veryNewExternal].", logs[1].SL_Reference);
			}
			else
			{
				AssertEquals("Matched to external id [newExternal].", logs[1].SL_Reference);
				AssertEquals("Matched to external id [veryNewExternal].", logs[0].SL_Reference);
			}
		}

		public void TestPatchSCIMUserById()
		{
			Clean_GS_ExternalId();

			var repo = new UserRepository(scimSchemaQueryRepository);

			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");

			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			var staff = Factory.Load<GlbStaff>(new ZGuid(id));
			AssertEquals("middle", staff.GS_MiddleName);

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			PatchOperationParameter patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REMOVE,
				Path = "name.middleName"
			};

			param.Operations.Add(patch);

			repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();

			staff = Factory.Load<GlbStaff>(new ZGuid(id));
			AssertEquals("", staff.GS_MiddleName);

			param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.ADD,
				Path = "name.middleName",
				Value = "middle2"
			};

			param.Operations.Add(patch);

			repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();

			staff = Factory.Load<GlbStaff>(new ZGuid(id));
			AssertEquals("middle2", staff.GS_MiddleName);

			param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "name.middleName",
				Value = "middle3"
			};

			param.Operations.Add(patch);

			repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();

			staff = Factory.Load<GlbStaff>(new ZGuid(id));
			AssertEquals("middle3", staff.GS_MiddleName);
		}

		public void TestPatchSCIMUserById_NoChange_Replace()
		{
			Clean_GS_ExternalId();

			var repo = new UserRepository(scimSchemaQueryRepository);

			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");

			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			PatchOperationParameter patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "userName",
				Value = "otherusername"
			};

			param.Operations.Add(patch);

			AssertNoExceptionThrown(delegate
			{ repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult(); });
		}

		public void TestPatchSCIMUserById_NoChange_Remove()
		{
			Clean_GS_ExternalId();

			var repo = new UserRepository(scimSchemaQueryRepository);

			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");

			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			PatchOperationParameter patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REMOVE,
				Path = "middleName"
			};

			param.Operations.Add(patch);

			var result = repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();
			AssertNull(result);
		}

		public void TestPatchSCIMUserById_NotFound()
		{
			var repo = new UserRepository(scimSchemaQueryRepository);
			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			PatchOperationParameter patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.ADD,
				Path = "userName",
				Value = "username"
			};

			param.Operations.Add(patch);

			AssertExceptionThrown<SCIMNotFoundException>(delegate
			{ repo.PatchSCIMResourceById(Guid.NewGuid().ToString(), param).GetAwaiter().GetResult(); });
		}

		public void TestPatchSCIMUserById_NonUnique_ExternalId()
		{
			Clean_GS_ExternalId();

			var repo = new UserRepository(scimSchemaQueryRepository);
			var scimUser1 = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");
			var scimUser2 = ScimProcessorTestHelper.CreateScimUser("456", "scimusername2", "XX2 first2 middle2 last2, III2", "first2", "middle2", "last2", "2 George St", "Sydney2", "0499123452", "test2@email.com", "title2", "III2", "XX2");

			string id1 = repo.CreateSCIMResource(scimUser1).GetAwaiter().GetResult().Id.ToString();
			string id2 = repo.CreateSCIMResource(scimUser2).GetAwaiter().GetResult().Id.ToString();

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			PatchOperationParameter patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "externalId",
				Value = "123"
			};

			param.Operations.Add(patch);

			AssertExceptionThrown<SCIMUniquenessAttributeException>(delegate
			{ repo.PatchSCIMResourceById(id2, param).GetAwaiter().GetResult(); });
		}

		public void TestPatchSCIMUserById_NonUnique()
		{
			Clean_GS_ExternalId();

			var repo = new UserRepository(scimSchemaQueryRepository);
			var scimUser1 = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");
			var scimUser2 = ScimProcessorTestHelper.CreateScimUser("456", "scimusername2", "XX2 first2 middle2 last2, III2", "first2", "middle2", "last2", "2 George St", "Sydney2", "0499123452", "test2@email.com", "title2", "III2", "XX2");

			string id1 = repo.CreateSCIMResource(scimUser1).GetAwaiter().GetResult().Id.ToString();
			string id2 = repo.CreateSCIMResource(scimUser2).GetAwaiter().GetResult().Id.ToString();

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			var jObj = new JObject
			{
				new JProperty(AttributeNames.GivenName, "otherFirst"),
				new JProperty(AttributeNames.ExternalId, "123"),
			};

			PatchOperationParameter patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Value = jObj
			};

			param.Operations.Add(patch);

			AssertExceptionThrown<SCIMUniquenessAttributeException>(delegate
			{ repo.PatchSCIMResourceById(id2, param).GetAwaiter().GetResult(); });
		}

		public void TestPatchSCIMUser_WorkAddress()
		{
			Clean_GS_ExternalId();

			var repo = new UserRepository(scimSchemaQueryRepository);

			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");

			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			PatchOperationParameter patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "addresses[type eq \"work\"].streetAddress",
				Value = "72 O'Riordan St"
			};

			param.Operations.Add(patch);

			AssertNoExceptionThrown(delegate
			{
				repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();
				AssertEquals("1 George St", repo.FindSCIMByResourceId(id).GetAwaiter().GetResult().AddressesStreetAddress);
			});
		}

		public void TestPatchSCIMUser_HomeAddress()
		{
			Clean_GS_ExternalId();

			var repo = new UserRepository(scimSchemaQueryRepository);

			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");

			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			PatchOperationParameter patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "addresses[type eq \"home\"].streetAddress",
				Value = "72 O'Riordan St"
			};

			param.Operations.Add(patch);

			AssertNoExceptionThrown(delegate
			{
				repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();
				AssertEquals("72 O'Riordan St", repo.FindSCIMByResourceId(id).GetAwaiter().GetResult().AddressesStreetAddress);
			});
		}

		public void TestPatchSCIMUser_Address()
		{
			Clean_GS_ExternalId();

			var repo = new UserRepository(scimSchemaQueryRepository);

			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");

			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			PatchOperationParameter patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "addresses.streetAddress",
				Value = "72 O'Riordan St"
			};

			param.Operations.Add(patch);

			AssertNoExceptionThrown(delegate
			{
				repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();
				AssertEquals("72 O'Riordan St", repo.FindSCIMByResourceId(id).GetAwaiter().GetResult().AddressesStreetAddress);
			});
		}

		public void TestPatchSCIMUser_WorkAddress_Formatted()
		{
			Clean_GS_ExternalId();

			var repo = new UserRepository(scimSchemaQueryRepository);

			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");

			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			PatchOperationParameter patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "addresses[type eq \"work\"].formatted",
				Value = "72 O'Riordan St"
			};

			param.Operations.Add(patch);

			AssertNoExceptionThrown(delegate
			{
				repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();
				AssertEquals("1 George St", repo.FindSCIMByResourceId(id).GetAwaiter().GetResult().AddressesStreetAddress);
			});
		}

		public void TestPatchSCIMUser_HomeAddress_Formatted()
		{
			Clean_GS_ExternalId();

			var repo = new UserRepository(scimSchemaQueryRepository);

			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");

			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			PatchOperationParameter patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "addresses[type eq \"home\"].formatted",
				Value = "72 O'Riordan St"
			};

			param.Operations.Add(patch);

			AssertNoExceptionThrown(delegate
			{
				repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();
				AssertEquals("1 George St", repo.FindSCIMByResourceId(id).GetAwaiter().GetResult().AddressesStreetAddress);
			});
		}

		public void TestPatchSCIMUser_Address_Formatted()
		{
			Clean_GS_ExternalId();

			var repo = new UserRepository(scimSchemaQueryRepository);

			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");

			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			PatchOperationParameter patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "addresses.formatted",
				Value = "72 O'Riordan St"
			};

			param.Operations.Add(patch);

			AssertNoExceptionThrown(delegate
			{
				repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();
				AssertEquals("1 George St", repo.FindSCIMByResourceId(id).GetAwaiter().GetResult().AddressesStreetAddress);
			});
		}

		public void TestPatchAddSCIMUser_WorkAddress()
		{
			Clean_GS_ExternalId();

			var repo = new UserRepository(scimSchemaQueryRepository);

			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "", "", "", "test@email.com", "title", "III", "XX");

			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			PatchOperationParameter patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.ADD,
				Path = "addresses[type eq \"work\"].streetAddress",
				Value = "72 O'Riordan St"
			};

			param.Operations.Add(patch);

			AssertNoExceptionThrown(delegate
			{
				repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();
				AssertEquals("", repo.FindSCIMByResourceId(id).GetAwaiter().GetResult().AddressesStreetAddress);
			});
		}

		public void TestPatch_Multiple_WithInvalid()
		{
			Clean_GS_ExternalId();

			var repo = new UserRepository(scimSchemaQueryRepository);

			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");

			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			var patch1 = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "addresses.streetAddress",
				Value = "72 O'Riordan St"
			};

			var patch2 = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "addresses[type eq \"home\"].formatted",
				Value = "73 O'Riordan St"
			};

			var patch3 = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "addresses[type eq \"work\"].streetAddress",
				Value = "74 O'Riordan St"
			};

			param.Operations.Add(patch1);
			param.Operations.Add(patch2);
			param.Operations.Add(patch3);

			AssertNoExceptionThrown(delegate
			{
				repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();
				AssertEquals("72 O'Riordan St", repo.FindSCIMByResourceId(id).GetAwaiter().GetResult().AddressesStreetAddress);
			});
		}

		public void TestPatchSCIMUser_Branch()
		{
			Clean_GS_ExternalId();

			var repo = new UserRepository(scimSchemaQueryRepository);

			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");

			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			AssertEquals("", repo.FindSCIMByResourceId(id).GetAwaiter().GetResult().HomeBranch);

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			var patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "homeBranch",
				Value = "SYD"
			};

			param.Operations.Add(patch);

			AssertNoExceptionThrown(delegate
			{
				repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();
				AssertEquals("SYD", repo.FindSCIMByResourceId(id).GetAwaiter().GetResult().HomeBranch);
			});

			param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "homeBranch",
				Value = "BNE"
			};

			param.Operations.Add(patch);

			AssertNoExceptionThrown(delegate
			{
				repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();
				AssertEquals("BNE", repo.FindSCIMByResourceId(id).GetAwaiter().GetResult().HomeBranch);
			});
		}

		public void TestPatchSCIMUser_Branch_NonExisting()
		{
			Clean_GS_ExternalId();

			var repo = new UserRepository(scimSchemaQueryRepository);

			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");

			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			AssertEquals("", repo.FindSCIMByResourceId(id).GetAwaiter().GetResult().HomeBranch);

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			var patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "homeBranch",
				Value = "ZZZ"
			};

			param.Operations.Add(patch);

			AssertExceptionThrown<SCIMNoTargetException>(delegate
			{
				repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();
			});
		}

		public void TestPatchSCIMUser_Department()
		{
			Clean_GS_ExternalId();

			var repo = new UserRepository(scimSchemaQueryRepository);

			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");

			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			AssertEquals("", repo.FindSCIMByResourceId(id).GetAwaiter().GetResult().HomeDepartment);

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			var patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "homeDepartment",
				Value = "BRN"
			};

			param.Operations.Add(patch);

			AssertNoExceptionThrown(delegate
			{
				repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();
				AssertEquals("BRN", repo.FindSCIMByResourceId(id).GetAwaiter().GetResult().HomeDepartment);
			});

			param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "homeDepartment",
				Value = "TLA"
			};

			param.Operations.Add(patch);

			AssertNoExceptionThrown(delegate
			{
				repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();
				AssertEquals("TLA", repo.FindSCIMByResourceId(id).GetAwaiter().GetResult().HomeDepartment);
			});
		}

		public void TestPatchSCIMUser_Department_NotExisting()
		{
			Clean_GS_ExternalId();

			var repo = new UserRepository(scimSchemaQueryRepository);

			var scimUser = ScimProcessorTestHelper.CreateScimUser("123", "scimusername", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");

			string id = repo.CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id.ToString();

			AssertEquals("", repo.FindSCIMByResourceId(id).GetAwaiter().GetResult().HomeBranch);

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			var patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "homeDepartment",
				Value = "ZZZ"
			};

			param.Operations.Add(patch);

			AssertExceptionThrown<SCIMNoTargetException>(delegate
			{
				repo.PatchSCIMResourceById(id, param).GetAwaiter().GetResult();
			});
		}

		#endregion

		#region Update user

		public void TestUpdateSCIMUserById()
		{
			var repo = new UserRepository(scimSchemaQueryRepository);

			var user1 = ScimProcessorTestHelper.CreateScimUser("123", "username1", "XX first1 middle1 last1, III", "first1", "middle1", "last1", "1 George St", "Sydney1", "0499123456", "test1@email.com", "title1", "III", "XX", branch: "SYD", department: "BRN");
			var user2 = ScimProcessorTestHelper.CreateScimUser("456", "username2", "YY first2 middle2 last2, IV", "first2", "middle2", "last2", "2 George St", "Sydney2", "0499789987", "test2@email.com", "title2", "IV", "YY", branch: "BNE", department: "TLA");

			string id = repo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id.ToString();

			var staff = Factory.Load<GlbStaff>(new ZGuid(id));
			AssertEquals("123", staff.GS_ExternalId);
			AssertEquals("username1", staff.GS_LoginName);
			AssertEquals("first1 middle1 last1", staff.GS_FullName);
			AssertEquals("1 George St", staff.GS_UserAddress1);
			AssertEquals("Sydney1", staff.GS_City);
			AssertEquals("first1", staff.GS_GivenName);
			AssertEquals("middle1", staff.GS_MiddleName);
			AssertEquals("last1", staff.GS_Surname);
			AssertEquals("0499123456", staff.GS_MobilePhone);
			AssertEquals("test1@email.com", staff.GS_EmailAddress);
			AssertEquals("title1", staff.GS_Title);
			AssertEquals("III", staff.GS_NameSuffix);
			AssertEquals("XX", staff.GS_NameTitle);
			AssertEquals("SYD", staff.HomeBranch.GB_Code);
			AssertEquals("BRN", staff.HomeDepartment.GE_Code);

			repo.UpdateSCIMResourceById(id, user2).GetAwaiter().GetResult();
			staff = Factory.Load<GlbStaff>(new ZGuid(id));
			AssertEquals("456", staff.GS_ExternalId);
			AssertEquals("username2", staff.GS_LoginName);
			AssertEquals("first2 middle2 last2", staff.GS_FullName);
			AssertEquals("2 George St", staff.GS_UserAddress1);
			AssertEquals("Sydney2", staff.GS_City);
			AssertEquals("first2", staff.GS_GivenName);
			AssertEquals("middle2", staff.GS_MiddleName);
			AssertEquals("last2", staff.GS_Surname);
			AssertEquals("0499789987", staff.GS_MobilePhone);
			AssertEquals("test2@email.com", staff.GS_EmailAddress);
			AssertEquals("title2", staff.GS_Title);
			AssertEquals("IV", staff.GS_NameSuffix);
			AssertEquals("YY", staff.GS_NameTitle);
			AssertEquals("BNE", staff.HomeBranch.GB_Code);
			AssertEquals("TLA", staff.HomeDepartment.GE_Code);
		}

		public void TestUpdateShouldClearAllFields()
		{
			var repo = new UserRepository(scimSchemaQueryRepository);

			var user1 = ScimProcessorTestHelper.CreateScimUser("123", "username1", "XX first1 middle1 last1, III", "first1", "middle1", "last1", "1 George St", "Sydney1", "0499123456", "test1@email.com", "title1", "III", "XX", "AU", "EN", "0211111111", "0311111111", "0411111111", branch: "SYD", department: "BRN");
			var user2 = ScimProcessorTestHelper.CreateScimUser("456", "username2", "full name", "first2", "middle2", "last2");
			string id = repo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id.ToString();

			var staff = Factory.Load<GlbStaff>(new ZGuid(id));
			AssertEquals("123", staff.GS_ExternalId);
			AssertEquals("username1", staff.GS_LoginName);
			AssertEquals("first1 middle1 last1", staff.GS_FullName);
			AssertEquals("1 George St", staff.GS_UserAddress1);
			AssertEquals("Sydney1", staff.GS_City);
			AssertEquals("first1", staff.GS_GivenName);
			AssertEquals("middle1", staff.GS_MiddleName);
			AssertEquals("last1", staff.GS_Surname);
			AssertEquals("0499123456", staff.GS_MobilePhone);
			AssertEquals("test1@email.com", staff.GS_EmailAddress);
			AssertEquals("title1", staff.GS_Title);
			AssertEquals("III", staff.GS_NameSuffix);
			AssertEquals("XX", staff.GS_NameTitle);
			AssertEquals("EN", staff.GS_WorkingLanguage);
			AssertEquals("0211111111", staff.GS_HomePhone);
			AssertEquals("0311111111", staff.GS_WorkPhone);
			AssertEquals("0411111111", staff.GS_FaxNum);
			AssertEquals("SYD", staff.HomeBranch.GB_Code);
			AssertEquals("BRN", staff.HomeDepartment.GE_Code);

			repo.UpdateSCIMResourceById(id, user2).GetAwaiter().GetResult();
			staff = Factory.Load<GlbStaff>(new ZGuid(id));
			AssertEquals("456", staff.GS_ExternalId);
			AssertEquals("username2", staff.GS_LoginName);
			AssertEquals("full name", staff.GS_FullName);
			AssertEquals("first2", staff.GS_GivenName);
			AssertEquals("middle2", staff.GS_MiddleName);
			AssertEquals("last2", staff.GS_Surname);

			AssertEquals(ZString.Empty, staff.GS_UserAddress1);
			AssertEquals(ZString.Empty, staff.GS_City);
			AssertEquals(ZString.Empty, staff.GS_MobilePhone);
			AssertEquals(ZString.Empty, staff.GS_EmailAddress);
			AssertEquals(ZString.Empty, staff.GS_Title);
			AssertEquals(ZString.Empty, staff.GS_NameTitle);
			AssertEquals(ZString.Empty, staff.GS_NameSuffix);
			AssertEquals("EN", staff.GS_WorkingLanguage);
			AssertEquals(ZString.Empty, staff.GS_HomePhone);
			AssertEquals(ZString.Empty, staff.GS_WorkPhone);
			AssertEquals(ZString.Empty, staff.GS_FaxNum);

			AssertEquals(ZString.Empty, staff.Person.PER_EmailAddress);
			AssertEquals(ZString.Empty, staff.Person.PER_City);
			AssertEquals(ZString.Empty, staff.Person.PER_MobilePhone);
			AssertEquals(ZString.Empty, staff.Person.PER_EmailAddress);
			AssertEquals(ZString.Empty, staff.Person.PER_NameTitle);
			AssertEquals(ZString.Empty, staff.Person.PER_NameSuffix);
			AssertEquals("EN", staff.Person.PER_PreferredLanguage);
			AssertEquals(ZString.Empty, staff.Person.PER_HomePhone);
			AssertEquals(ZString.Empty, staff.Person.PER_FaxNumber);

			AssertEquals(ZGuid.Empty, staff.GS_GB_HomeBranch);
			AssertEquals(ZGuid.Empty, staff.GS_GE_HomeDepartment);
		}

		public void TestUpdateSCIMUserById_NotFound()
		{
			var repo = new UserRepository(scimSchemaQueryRepository);

			var user = ScimProcessorTestHelper.CreateScimUser("123", "username", "XX first middle last, III", "first", "middle", "last", "1 George St", "Sydney", "0499123456", "test@email.com", "title", "III", "XX");
			AssertExceptionThrown<SCIMNotFoundException>(delegate
			{ repo.UpdateSCIMResourceById(Guid.NewGuid().ToString(), user).GetAwaiter().GetResult(); });
		}

		public void TestUpdateSCIMUserById_NonUnique()
		{
			var repo = new UserRepository(scimSchemaQueryRepository);

			var user1 = ScimProcessorTestHelper.CreateScimUser("123", "username1", "XX first1 middle1 last1, II", "first1", "middle1", "last1", "1 George St", "Sydney1", "0499123456", "test1@email.com", "title1", "II", "XX");
			var user2 = ScimProcessorTestHelper.CreateScimUser("456", "username2", "YY first2 middle2 last2, IV", "first2", "middle2", "last2", "2 George St", "Sydney2", "0499789987", "test2@email.com", "title2", "IV", "YY");
			var user3 = ScimProcessorTestHelper.CreateScimUser("123", "username3", "ZZ first2 middle2 last2, ZZ", "first3", "middle3", "last3", "3 George St", "Sydney3", "0499789988", "test3@email.com", "title3", "ZZ", "ZZ");

			string id1 = repo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id.ToString();
			string id2 = repo.CreateSCIMResource(user2).GetAwaiter().GetResult().Id.ToString();

			AssertExceptionThrown<SCIMUniquenessAttributeException>(delegate
			{ repo.UpdateSCIMResourceById(id2, user3).GetAwaiter().GetResult(); });
		}

		#endregion

		void Clean_GS_ExternalId()
		{
			scimSchemaQueryRepository = ScimProcessorTestHelper.ReturnSchemaQueryRepository();
			var query = new ZQuery(GlbStaffSchema.GS_ExternalId, SQLComparisonOperator.IsNotBlank, string.Empty);
			var staffs = Factory.Load<GlbStaff>(query);

			foreach (var staff in staffs)
			{
				staff.GS_ExternalId = ZString.Empty;
			}
		}
	}
}
