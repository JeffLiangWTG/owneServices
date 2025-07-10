using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(STATACREQDOCSendingObject))]
	sealed class STATACREQDOCSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOrganization()
		{
			AssertEquals(organization.OH_Code, sendingObject.OrganizationCode);
		}

		public void TestCustomsOffice()
		{
			mapping.CustomsOfficeCode = "DBN";
			AssertEquals(mapping.CustomsOfficeCode, sendingObject.CustomsOfficeCode);
		}

		public void TestFinancialAccountNumber()
		{
			mapping.FinancialAccountNumber = "1234567890";
			AssertEquals(mapping.FinancialAccountNumber, sendingObject.FinancialAccountNumber);
		}

		public void TestAgentCodeLinkedToFAN()
		{
			AssertEquals("AGT", sendingObject.AgentCodeLinkedToFAN);
		}

		public void TestAgentDualProfileCodeLinkedToFAN()
		{
			AssertEquals("1234", sendingObject.AgentDualProfileCodeLinkedToFAN);
		}

		[TestDate(2016, 08, 01)]
		public void TestEndDate()
		{
			AssertEquals(new ZDate(2016, 08, 01), sendingObject.EndDate);
		}

		[TestDate(2016, 08, 01)]
		public void TestStartDate1()
		{
			mapping.AccountStartDay = 1;
			var sendingObject = new STATACREQDOCSendingObject(parent, mapping);
			AssertEquals(new ZDate(2016, 08, 01), sendingObject.StartDate);
			mapping.AccountStartDay = 15;
			sendingObject = new STATACREQDOCSendingObject(parent, mapping);
			AssertEquals(new ZDate(2016, 07, 15), sendingObject.StartDate);
			mapping.AccountStartDay = 31;
			sendingObject = new STATACREQDOCSendingObject(parent, mapping);
			AssertEquals(new ZDate(2016, 07, 31), sendingObject.StartDate);
		}

		[TestDate(2016, 03, 15)]
		public void TestStartDate15()
		{
			mapping.AccountStartDay = 1;
			var sendingObject = new STATACREQDOCSendingObject(parent, mapping);
			AssertEquals(new ZDate(2016, 03, 01), sendingObject.StartDate);
			mapping.AccountStartDay = 15;
			sendingObject = new STATACREQDOCSendingObject(parent, mapping);
			AssertEquals(new ZDate(2016, 03, 15), sendingObject.StartDate);
			mapping.AccountStartDay = 31;
			sendingObject = new STATACREQDOCSendingObject(parent, mapping);
			AssertEquals(new ZDate(2016, 02, 29), sendingObject.StartDate);
		}

		[TestDate(2016, 03, 31)]
		public void TestStartDate31()
		{
			mapping.AccountStartDay = 1;
			var sendingObject = new STATACREQDOCSendingObject(parent, mapping);
			AssertEquals(new ZDate(2016, 03, 01), sendingObject.StartDate);
			mapping.AccountStartDay = 15;
			sendingObject = new STATACREQDOCSendingObject(parent, mapping);
			AssertEquals(new ZDate(2016, 03, 15), sendingObject.StartDate);
			mapping.AccountStartDay = 31;
			sendingObject = new STATACREQDOCSendingObject(parent, mapping);
			AssertEquals(new ZDate(2016, 03, 31), sendingObject.StartDate);
		}

		[TestDate(2016, 01, 01)]
		public void TestStartDate1Jan()
		{
			mapping.AccountStartDay = 1;
			var sendingObject = new STATACREQDOCSendingObject(parent, mapping);
			AssertEquals(new ZDate(2016, 01, 01), sendingObject.StartDate);
			mapping.AccountStartDay = 15;
			sendingObject = new STATACREQDOCSendingObject(parent, mapping);
			AssertEquals(new ZDate(2015, 12, 15), sendingObject.StartDate);
			mapping.AccountStartDay = 31;
			sendingObject = new STATACREQDOCSendingObject(parent, mapping);
			AssertEquals(new ZDate(2015, 12, 31), sendingObject.StartDate);
		}

		public void TestMessages()
		{
			AssertType(typeof(Messaging.Business.EDIMessageCollection), sendingObject.Messages);
		}

		public void TestMessageStatus()
		{
			AssertEquals(ZString.Empty, sendingObject.MessageStatus);
		}

		public void TestJobStatus()
		{
			AssertEquals(ZString.Empty, sendingObject.JobStatus);
		}

		public void TestJobIdentification()
		{
			AssertEquals(ZString.Empty, sendingObject.JobIdentification);
		}

		public void TestTopLevelBusinessObject()
		{
			AssertType(typeof(STATACREQDOCSendingObject), sendingObject.TopLevelBusinessObject);
			AssertSame(sendingObject, sendingObject.TopLevelBusinessObject);
		}

		public void TestAddMessage()
		{
			sendingObject.AddMessage(Factory.New<REQDOCEDIMessage>());
			AssertEquals("Nothing happens on add", 0, sendingObject.Messages.Count);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var mapping = new FinancialAccountNumberPortMap();
			return new STATACREQDOCSendingObject(new STATACREQDOCSendingObjectParent(Factory), mapping);
		}

		protected override void SetUp()
		{
			base.SetUp();
			organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_Code = "ABCD";
			organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "AGT", Core.Constants.CountryCodes.SouthAfrica);
			organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, "1234", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			mapping = new FinancialAccountNumberPortMap();
			mapping.OrganizationPK = organization.PK;
			parent = new STATACREQDOCSendingObjectParent(Factory);
			sendingObject = new STATACREQDOCSendingObject(parent, mapping);
		}

		OrgHeader organization;
		FinancialAccountNumberPortMap mapping;
		STATACREQDOCSendingObjectParent parent;
		STATACREQDOCSendingObject sendingObject;
	}
}
