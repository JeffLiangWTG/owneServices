using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(STATACREQDOCSendingObjectParent))]
	sealed class STATACREQDOCSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendingObjectsCollection()
		{
			var zaTestHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			zaTestHelper.CreateCustomsOfficeCusCodeEntry("JHB");
			zaTestHelper.CreateCustomsStatusCusCodeEntry("1");
			var collection = new FinancialAccountNumberPortMapCollection();
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			var testWrapper = new STATACREQDOCSendingObjectParent(Factory);
			AssertEquals(0, testWrapper.SendingObjectsCollection.Count);
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "buyer";
			buyer.OH_IsConsignee = true;
			buyer.OH_Code = "IMP#@$43";
			buyer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "ASBSD", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var creditor = collection.AddNew();
			creditor.OrganizationPK = buyer.PK;
			creditor.CustomsOfficeCode = "JHB";
			creditor.FinancialAccountNumber = "3234002346";
			creditor.ImporterPays = ZBool.True;
			creditor.AccountStartDay = 1;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			testWrapper = new STATACREQDOCSendingObjectParent(Factory);
			AssertEquals(1, testWrapper.SendingObjectsCollection.Count);
		}

		public void TestSecurityRightToSendWithMessageErrors()
		{
			var testWrapper = new STATACREQDOCSendingObjectParent(Factory);
			AssertEquals(Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZACustomsStatement), testWrapper.SecurityCheckpointToSendWithMessageError);
		}

		protected override BusinessObject GetNewBusinessObject() => new STATACREQDOCSendingObjectParent(Factory);
	}
}
