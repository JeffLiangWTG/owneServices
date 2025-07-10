using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgHeaderLink))]
	sealed class OrgHeaderLinkTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var mockUserNotification = new Mock<IUserNotification>();

			return new OrgHeaderLink(Factory, mockUserNotification.Object, org1, org2);
		}

		public void TestSimpleCaseMakingOrg1AsHeadOffice() // Test a simpe case making the first organisation parent and adding to database
		{
			var orgParent = Factory.NewWithValidTestData<OrgHeader>();
			orgParent.OH_Code = "TESTPARENT";
			var orgChild = Factory.NewWithValidTestData<OrgHeader>();
			orgChild.OH_Code = "TESTCHILD";
			Factory.Save();

			var mockUserNotification = new Mock<IUserNotification>();

			var link = new OrgHeaderLink(Factory, mockUserNotification.Object, orgParent, orgChild);
			var result = link.TryAddNewOrgRelatedParty(orgParent, orgChild);
			link.Save();

			Assert("Does not expect any error", result);

			var query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, orgParent.PK);
			query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, orgChild.PK);
			var relatedParties = Factory.Load<OrgRelatedParty>(query);
			AssertEquals("Adding related party to table", 1, relatedParties.Length);

			AssertEquals("Expecting a correct Party Type", "MNG", relatedParties.Single().PR_PartyType);
		}

		public void TestCaseUsingOther() // Test a case using other, should add 2 new rows to database with correct PKs
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TESTCHILD1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TESTCHILD2";
			Factory.Save();

			var mockUserNotification = new Mock<IUserNotification>();

			var orgOther = Factory.NewWithValidTestData<OrgHeader>();
			orgOther.OH_Code = "TESTPARENT";
			Factory.Save();

			var link = new OrgHeaderLink(Factory, mockUserNotification.Object, org1, org2);
			var result = link.TryAddNewOrgRelatedParty(orgOther, org1, org2);
			link.Save();

			Assert("Does not expect any error", result);

			result = link.TryAddNewOrgRelatedParty(null, org1, org2);
			Assert("Expect an error", !result);
			mockUserNotification.Verify(mock => mock.ShowError(It.IsAny<string>()), Times.Once);
			mockUserNotification.Verify(mock => mock.ShowError("Please choose an organization from the search box."), Times.Once);

			var query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, orgOther.PK);
			var relatedParties = Factory
				.Load<OrgRelatedParty>(query)
				.ToArray();

			var relatedPartyPKs = relatedParties
				.Select(party => party.PR_OH_Parent)
				.ToArray();

			AssertEquals("Adding related party to table", 2, relatedPartyPKs.Length);
			AssertCollectionContains(org1.PK, relatedPartyPKs);
			AssertCollectionContains(org2.PK, relatedPartyPKs);

			foreach (var relatedParty in relatedParties)
			{
				AssertEquals("Expecting a correct Party Type", "MNG", relatedParty.PR_PartyType);
			}
		}

		public void TestAttemptToLinkSameOrg()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();

			var mockUserNotification = new Mock<IUserNotification>();

			var link = new OrgHeaderLink(Factory, mockUserNotification.Object, org1, org1);
			var result = link.TryAddNewOrgRelatedParty(org1, org1);
			link.Save();

			Assert("Expect an error", !result);

			mockUserNotification.Verify(mock => mock.ShowError(It.IsAny<string>()), Times.Once);
			mockUserNotification.Verify(mock => mock.ShowError("You cannot link an organization with itself."), Times.Once);

			var query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, org1.PK);
			query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, org1.PK);
			var relatedParties = Factory.Load<OrgRelatedParty>(query);
			AssertEquals("Relationship should not exist", 0, relatedParties.Length);
		}

		public void TestSettingOrg1OrOrg2AsOther()
		{
			var orgOtherSameAsOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			orgOtherSameAsOrg1.OH_Code = "TESTOTHER";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TESTCHILD2";
			Factory.Save();

			var mockUserNotification = new Mock<IUserNotification>();

			var link = new OrgHeaderLink(Factory, mockUserNotification.Object, orgOtherSameAsOrg1, org2);
			var result = link.TryAddNewOrgRelatedParty(orgOtherSameAsOrg1, orgOtherSameAsOrg1, org2);
			link.Save();

			Assert("Expect an error", !result);

			mockUserNotification.Verify(mock => mock.ShowError(It.IsAny<string>()), Times.Once);
			mockUserNotification.Verify(mock => mock.ShowError("Your selected organization is already shown."), Times.Once);

			var query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, orgOtherSameAsOrg1.PK);
			query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, orgOtherSameAsOrg1.PK);
			var relatedParties = Factory.Load<OrgRelatedParty>(query);
			AssertEquals("Relationship should not exist", 0, relatedParties.Length);

			query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, orgOtherSameAsOrg1.PK);
			query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, org2.PK);
			relatedParties = Factory.Load<OrgRelatedParty>(query);
			AssertEquals("Relationship should not exist", 0, relatedParties.Length);
		}

		public void TestAttemptToLinkRelationshipThatExist()
		{
			var orgParent = Factory.NewWithValidTestData<OrgHeader>();
			orgParent.OH_Code = "TESTPARENT";
			var orgChild = Factory.NewWithValidTestData<OrgHeader>();
			orgChild.OH_Code = "TESTCHILD";
			Factory.Save();

			var mockUserNotification = new Mock<IUserNotification>();

			var link = new OrgHeaderLink(Factory, mockUserNotification.Object, orgParent, orgChild);
			var result = link.TryAddNewOrgRelatedParty(orgParent, orgChild);
			link.Save();
			Assert("Expect no error", result);

			result = link.TryAddNewOrgRelatedParty(orgChild, orgParent);
			link.Save();
			Assert("Expect an error", !result);

			mockUserNotification.Verify(mock => mock.ShowError(It.IsAny<string>()), Times.Once);
			mockUserNotification.Verify(mock => mock.ShowError("TESTCHILD cannot be the head office of TESTPARENT as TESTPARENT is already the head office of TESTCHILD."), Times.Once);

			var query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, orgParent.PK);
			query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, orgChild.PK);
			var relatedParties = Factory.Load<OrgRelatedParty>(query);
			AssertEquals("Relationship should exist exactly once", 1, relatedParties.Length);

			var query2 = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, orgChild.PK);
			query2.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, orgParent.PK);
			var relatedPartyInDb = Factory.ExistsInDatabase(OrgRelatedParty.Schema.TableName, query2);
			Assert("Relationship should not exist", !relatedPartyInDb);
		}

		public void TestAttemptToLinkMoreThanOneParent()
		{
			var orgParent = Factory.NewWithValidTestData<OrgHeader>();
			orgParent.OH_Code = "TESTPARENT";
			var orgChild = Factory.NewWithValidTestData<OrgHeader>();
			orgChild.OH_Code = "TESTCHILD";

			var thirdOrg = Factory.NewWithValidTestData<OrgHeader>();
			thirdOrg.OH_Code = "2NDPARENT";
			Factory.Save();

			var mockUserNotification = new Mock<IUserNotification>();

			var link = new OrgHeaderLink(Factory, mockUserNotification.Object, orgParent, orgChild);
			var result = link.TryAddNewOrgRelatedParty(orgParent, orgChild);
			link.Save();
			Assert("Expect no error", result);

			result = link.TryAddNewOrgRelatedParty(thirdOrg, orgChild);
			link.Save();
			Assert("Expect an error", !result);

			mockUserNotification.Verify(mock => mock.ShowError(It.IsAny<string>()), Times.Once);
			mockUserNotification.Verify(mock => mock.ShowError("It is not possible for 2NDPARENT to be head office of TESTCHILD as TESTCHILD already has TESTPARENT as its head office.\r\nRelationships can be managed under the Related Party tab."), Times.Once);

			var query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, orgParent.PK);
			query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, orgChild.PK);
			query.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ManagementGrouping);
			var relatedParties = Factory.Load<OrgRelatedParty>(query);
			AssertEquals("Relationship should exist exactly once", 1, relatedParties.Length);

			var query2 = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, thirdOrg.PK);
			query2.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, orgChild.PK);
			var relatedPartyInDb = Factory.ExistsInDatabase(OrgRelatedParty.Schema.TableName, query2);
			Assert("Relationship should not exist", !relatedPartyInDb);
		}

		public void TestNewOrganisationNotInDatabase()
		{
			var orgChild = Factory.NewWithValidTestData<OrgHeader>();

			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var mockUserNotification = new Mock<IUserNotification>();

			var link = new OrgHeaderLink(Factory, mockUserNotification.Object, newOrg, orgChild);
			var result = link.TryAddNewOrgRelatedParty(newOrg, orgChild);

			Assert("Does not expect any error", result);

			var query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, newOrg.PK);
			query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, orgChild.PK);
			var relatedPartyInDb = Factory.ExistsInDatabase(OrgRelatedParty.Schema.TableName, query);
			Assert("Relationship should not exist", !relatedPartyInDb);

			link.Save();

			relatedPartyInDb = Factory.ExistsInDatabase(OrgRelatedParty.Schema.TableName, query);
			Assert("Relationship should exist", relatedPartyInDb);
		}

		public void TestShowCorrectErrorMessage()
		{
			var orgParent = Factory.NewWithValidTestData<OrgHeader>();
			orgParent.OH_Code = "TESTPARENT";
			var orgChild = Factory.NewWithValidTestData<OrgHeader>();
			orgChild.OH_Code = "TESTCHILD";
			var orgOther = Factory.NewWithValidTestData<OrgHeader>();
			orgOther.OH_Code = "TESTOTHER";
			Factory.Save();

			var mockUserNotification = new Mock<IUserNotification>();
			var link = new OrgHeaderLink(Factory, mockUserNotification.Object, orgParent, orgChild);
			var result = link.TryAddNewOrgRelatedParty(orgParent, orgChild);
			link.Save();
			Assert("Expect no error", result);

			result = link.TryAddNewOrgRelatedParty(orgChild, orgParent);
			Assert("Expect an error", !result);
			mockUserNotification.Verify(mock => mock.ShowError(It.IsAny<string>()), Times.Once);
			mockUserNotification.Verify(mock => mock.ShowError("TESTCHILD cannot be the head office of TESTPARENT as TESTPARENT is already the head office of TESTCHILD."), Times.Once);

			result = link.TryAddNewOrgRelatedParty(orgParent, orgChild);
			Assert("Expect an error", !result);
			mockUserNotification.Verify(mock => mock.ShowError(It.IsAny<string>()), Times.Exactly(2));
			mockUserNotification.Verify(mock => mock.ShowError("It is not possible for TESTPARENT to be head office of TESTCHILD as TESTCHILD already has TESTPARENT as its head office.\r\nRelationships can be managed under the Related Party tab."), Times.Once);

			var query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, orgParent.PK);
			query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, orgChild.PK);
			var relatedPartie = Factory.LoadTop1<OrgRelatedParty>(query);
			relatedPartie.Delete();
			Factory.Save();

			result = link.TryAddNewOrgRelatedParty(orgChild, orgParent);
			link.Save();
			Assert("Expect no error", result);

			result = link.TryAddNewOrgRelatedParty(orgChild, orgParent);
			Assert("Expect an error", !result);
			mockUserNotification.Verify(mock => mock.ShowError(It.IsAny<string>()), Times.Exactly(3));
			mockUserNotification.Verify(mock => mock.ShowError("It is not possible for TESTCHILD to be head office of TESTPARENT as TESTPARENT already has TESTCHILD as its head office.\r\nRelationships can be managed under the Related Party tab."), Times.Once);

			result = link.TryAddNewOrgRelatedParty(orgParent, orgChild);
			Assert("Expect an error", !result);
			mockUserNotification.Verify(mock => mock.ShowError(It.IsAny<string>()), Times.Exactly(4));
			mockUserNotification.Verify(mock => mock.ShowError("TESTPARENT cannot be the head office of TESTCHILD as TESTCHILD is already the head office of TESTPARENT."), Times.Once);

			query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, orgChild.PK);
			query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, orgParent.PK);
			relatedPartie = Factory.LoadTop1<OrgRelatedParty>(query);
			relatedPartie.Delete();
			Factory.Save();

			result = link.TryAddNewOrgRelatedParty(orgOther, orgChild, orgParent);
			link.Save();
			Assert("Expect no error", result);

			result = link.TryAddNewOrgRelatedParty(orgOther, orgChild, orgParent);
			Assert("Expect an error", !result);
			mockUserNotification.Verify(mock => mock.ShowError(It.IsAny<string>()), Times.Exactly(6));
			mockUserNotification.Verify(mock => mock.ShowError("It is not possible for TESTOTHER to be head office of TESTCHILD as TESTCHILD already has TESTOTHER as its head office.\r\nRelationships can be managed under the Related Party tab."), Times.Once);
			mockUserNotification.Verify(mock => mock.ShowError("It is not possible for TESTOTHER to be head office of TESTPARENT as TESTPARENT already has TESTOTHER as its head office.\r\nRelationships can be managed under the Related Party tab."), Times.Once);

			result = link.TryAddNewOrgRelatedParty(orgParent, orgOther, orgChild);
			Assert("Expect an error", !result);
			mockUserNotification.Verify(mock => mock.ShowError(It.IsAny<string>()), Times.Exactly(8));
			mockUserNotification.Verify(mock => mock.ShowError("TESTPARENT cannot be the head office of TESTOTHER as TESTOTHER is already the head office of TESTPARENT."), Times.Once);
			mockUserNotification.Verify(mock => mock.ShowError("It is not possible for TESTPARENT to be head office of TESTCHILD as TESTCHILD already has TESTOTHER as its head office.\r\nRelationships can be managed under the Related Party tab."), Times.Once);

			result = link.TryAddNewOrgRelatedParty(orgChild, orgOther, orgParent);
			Assert("Expect an error", !result);
			mockUserNotification.Verify(mock => mock.ShowError(It.IsAny<string>()), Times.Exactly(10));
			mockUserNotification.Verify(mock => mock.ShowError("TESTCHILD cannot be the head office of TESTOTHER as TESTOTHER is already the head office of TESTCHILD."), Times.Once);
			mockUserNotification.Verify(mock => mock.ShowError("It is not possible for TESTCHILD to be head office of TESTPARENT as TESTPARENT already has TESTOTHER as its head office.\r\nRelationships can be managed under the Related Party tab."), Times.Once);
		}

		public void TestNotSaveAnyRelatedPartyWhenAnyVerificationFailed()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TESTORG1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TESTORG2";
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "TESTORG3";
			var orgOther = Factory.NewWithValidTestData<OrgHeader>();
			orgOther.OH_Code = "TESTOTHER";

			Factory.Save();

			var mockUserNotification = new Mock<IUserNotification>();
			var link = new OrgHeaderLink(Factory, mockUserNotification.Object, org1, org2);
			var org1RelatedParty = Factory.New<OrgRelatedParty>();
			org1RelatedParty.PR_OH_Parent = org1.PK;
			org1RelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			org1RelatedParty.PR_OH_RelatedParty = org3.PK;
			var result = link.TryAddNewOrgRelatedParty(orgOther, org1, org2);
			Assert("Should not related successed", !result);

			link.Save();
			var query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, orgOther.PK);
			var relatedParties = Factory.Load<OrgRelatedParty>(query);
			Assert("Should not contain related party", !relatedParties.Any());

			org1RelatedParty.Delete();
			var org2RelatedParty = Factory.New<OrgRelatedParty>();
			org2RelatedParty.PR_OH_Parent = org2.PK;
			org2RelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			org2RelatedParty.PR_OH_RelatedParty = org3.PK;
			result = link.TryAddNewOrgRelatedParty(orgOther, org1, org2);
			Assert("Should not related successed", !result);

			link.Save();
			relatedParties = Factory.Load<OrgRelatedParty>(query);
			Assert("Should not contain related party", !relatedParties.Any());

			org2RelatedParty.Delete();
			result = link.TryAddNewOrgRelatedParty(orgOther, org1, org2);
			Assert("Should related successed", result);

			link.Save();
			relatedParties = Factory.Load<OrgRelatedParty>(query);
			AssertEquals(2, relatedParties.Length);
		}

		public void TestConcurrencyErrorWhenSaving()
		{
			var orgParent = Factory.NewWithValidTestData<OrgHeader>();
			orgParent.OH_Code = "TESTPARENT";
			orgParent.MainAddress.ValidationStatus = AddressValidationStatus.ToBeVerified;

			var orgChild = Factory.NewWithValidTestData<OrgHeader>();
			orgChild.OH_Code = "TESTCHILD";
			orgChild.MainAddress.ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			var query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, orgParent.PK);
			query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, orgChild.PK);

			var mockUserNotification = new Mock<IUserNotification>();
			var link = new OrgHeaderLink(Factory, mockUserNotification.Object, orgParent, orgChild);
			link.TryAddNewOrgRelatedParty(orgParent, orgChild);

			var relatedParties = Factory.Load<OrgRelatedParty>(query);
			AssertEquals(1, relatedParties.Length);
			Assert(!relatedParties[0].IsInDatabase);

			var anohterFactory = new BusinessObjectFactory();
			anohterFactory.RefreshEnabled = false;
			var org1MainAddress = anohterFactory.Load<OrgAddress>(orgParent.MainAddress.PK);
			org1MainAddress.ValidationStatus = AddressValidationStatus.Invalid;
			var org2MainAddress = anohterFactory.Load<OrgAddress>(orgChild.MainAddress.PK);
			org2MainAddress.ValidationStatus = AddressValidationStatus.Invalid;
			anohterFactory.Save();

			UnitTestUserNotification.Instance.ClearMessages();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
			orgParent.MainAddress.ValidationStatus = AddressValidationStatus.ManuallyVerified;
			orgChild.MainAddress.ValidationStatus = AddressValidationStatus.ManuallyVerified;
			var result = link.Save();

			Assert(!result);
			AssertContains("While you have been working with this form", UnitTestUserNotification.Instance.LastMessage.Text);

			link.DeleteRelatedParties();
			relatedParties = Factory.Load<OrgRelatedParty>(query);
			AssertEquals(0, relatedParties.Length);

			link.TryAddNewOrgRelatedParty(orgParent, orgChild);
			result = link.Save();

			relatedParties = Factory.Load<OrgRelatedParty>(query);
			Assert(result);
			Assert(relatedParties[0].IsInDatabase);
			AssertEquals(1, relatedParties.Length);
			AssertEquals("Expecting a correct Party Type", "MNG", relatedParties[0].PR_PartyType);
		}
	}
}
