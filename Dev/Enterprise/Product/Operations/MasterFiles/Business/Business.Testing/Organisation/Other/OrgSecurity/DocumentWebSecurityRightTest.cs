using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DocumentWebSecurityRightTest : TestCaseWithFactory
	{
		public void TestDeniedByDefaultRegistry()
		{
			var docType = CreateDocType("XZY", save: true);
			var code = DocumentWebSecurityRights.GetSecurityCodeFromDocType(docType);
			var list = new DocumentWebSecurityRights();
			WebSecurityRight securityRight;
			Assert(list.TryGetValue(code, out securityRight));
			AssertEquals(true, securityRight.IsGrantedByDefault);

			using (OrganisationRegistry.Instance.WebSecurityRightsDeniedByDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				list = new DocumentWebSecurityRights();
				Assert(list.TryGetValue(code, out securityRight));
				AssertEquals(false, securityRight.IsGrantedByDefault);
			}
		}

		public void TestTryGetValue_Caches()
		{
			var docType = CreateDocType("XZY", save: true);
			var code = DocumentWebSecurityRights.GetSecurityCodeFromDocType(docType);

			var securityRights = new DocumentWebSecurityRights();

			WebSecurityRight first, second;
			securityRights.TryGetValue(code, out first);
			securityRights.TryGetValue(code, out second);

			AssertNotNull("PRE: Should have received a right", first);
			AssertSame("Should have cached the value", first, second);
		}

		public void TestGetSecurityRight()
		{
			var docType = CreateDocType("XZY", "ABC");
			docType.RT_Desc = "Do Stuff";

			var securityRights = new DocumentWebSecurityRights();

			var newRight = securityRights.GetSecurityRight(docType);
			AssertContains("Should be related to the doc type", "XZY", newRight.Code);
			AssertContains("Should be related to the ref type", "ABC", newRight.Code);
			AssertEquals("Should have a meaningful name", "Document XZY for ABC (Do Stuff)", newRight.Description);
		}

		public void TestEnumerate_IncludesAllDocumentTypes()
		{
			CreateDocType("XZY", save: true);

			var numberOfDocumentTypes = Factory.GetDatabaseCount(typeof(RefDocType));

			var securityRights = new DocumentWebSecurityRights();
			AssertEquals("Iterating it should produce the same amount as there is types", numberOfDocumentTypes, securityRights.Count);
			Assert("Should contain out document code somewhere", securityRights.Any(right => right.Code.Contains("XZY")));
		}

		public void TestEnumerate_DocTypeCreatedAfterInstantiationIsIncluded()
		{
			var securityRights = new DocumentWebSecurityRights();

			CreateDocType("XZY", save: true);

			Assert("Should contain out document code somewhere", securityRights.Any(right => right.Code.Contains("XZY")));
		}

		public void TestCount()
		{
			var rights = new DocumentWebSecurityRights();

			AssertEquals("Should be one for each doc type", Factory.GetDatabaseCount(typeof(RefDocType)), rights.Count);

			CreateDocType("JCD");
			CreateDocType("BDC");
			CreateDocType("EGI");

			Factory.Save();

			AssertEquals("Should be one for each doc type", Factory.GetDatabaseCount(typeof(RefDocType)), rights.Count);
		}

		public void TestTryGetValue_KeyDoesExist()
		{
			var rights = new DocumentWebSecurityRights();

			var docType = CreateDocType("XZY", save: true);
			var securityRightCode = DocumentWebSecurityRights.GetSecurityCodeFromDocType(docType);

			WebSecurityRight right;
			Assert("Should get value", rights.TryGetValue(securityRightCode, out right));
			AssertEquals(securityRightCode, right.Code);
		}

		public void TestTryGetValue_VsMalformedDocType()
		{
			var rights = new DocumentWebSecurityRights();

			var docType1 = CreateDocType("1", save: true);
			var docType2 = CreateDocType("AAA", "2!4", true);
			var docType3 = CreateDocType("AAA", save: true);
			var securityRightCode1 = DocumentWebSecurityRights.GetSecurityCodeFromDocType(docType1);
			var securityRightCode2 = DocumentWebSecurityRights.GetSecurityCodeFromDocType(docType2);
			var securityRightCode3 = DocumentWebSecurityRights.GetSecurityCodeFromDocType(docType3);

			WebSecurityRight right;
			Assert("Should get value", rights.TryGetValue(securityRightCode1, out right));
			AssertEquals(securityRightCode1, right.Code);
			Assert("Should get value", rights.TryGetValue(securityRightCode2, out right));
			AssertEquals(securityRightCode2, right.Code);
			Assert("Should get value", rights.TryGetValue(securityRightCode3, out right));
			AssertEquals(securityRightCode3, right.Code);
		}

		public void TestTryGetValue_SecurityRightDoesntExist()
		{
			var docType = CreateDocType("XZY", save: false);
			var securityRightCode = DocumentWebSecurityRights.GetSecurityCodeFromDocType(docType);
			docType.Delete();

			var rights = new DocumentWebSecurityRights();

			WebSecurityRight right;
			Assert("Should return false", !rights.TryGetValue(securityRightCode, out right));
			AssertNull(right);
		}

		public void TestTryGetValue_KeyDoesntMatchPattern()
		{
			const string securityRightCode = "fnasn fjkdfn lkdsnf";

			var rights = new DocumentWebSecurityRights();

			WebSecurityRight right;
			Assert("Should return false", !rights.TryGetValue(securityRightCode, out right));
			AssertNull(right);
		}

		RefDocType CreateDocType(string docType, string refType = "ALL", bool save = false)
		{
			var newDocType = Factory.NewWithValidTestData<RefDocType>();
			newDocType.RT_DocType = docType;
			newDocType.RT_ReferenceType = refType;

			if (save)
			{
				Factory.Save();
			}

			return newDocType;
		}
	}
}
