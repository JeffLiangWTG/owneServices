using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(IssuerAndBillNumber))]
	sealed class IssuerAndBillNumberTest : Customs.Business.Testing.CusCodeDataTest<IssuerAndBillNumber>
	{
		public void TestCY_TypeIsSet()
		{
			IssuerAndBillNumber billOfLading = Factory.New<IssuerAndBillNumber>();
			AssertEquals(CusCodeDataTypeList.Codes.IssuerAndBillNumber, billOfLading.CY_Type);
		}

		public void TestParent()
		{
			IssuerAndBillNumber billOfLading = Factory.New<IssuerAndBillNumber>();
			AssertNull(billOfLading.Parent);
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			billOfLading.CY_ParentID = message.PK;
			billOfLading.CY_ParentTableCode = EDIMessageSchema.Constants.Prefix;
			AssertEquals(message, billOfLading.Parent);
		}

		public void TestLoadIssuerCodeBillNumberFromCY_Data()
		{
			IssuerAndBillNumber billOfLading = Factory.New<IssuerAndBillNumber>();
			billOfLading.CY_ParentID = ZGuid.NewZGuid();
			billOfLading.CY_ParentTableCode = EDIMessageSchema.Constants.Prefix;
			billOfLading.CY_Data = "AAAA632409-3";
			AssertEquals("AAAA", billOfLading.CY_IssuerCode);
			AssertEquals("632409-3", billOfLading.CY_BillNumber);
			billOfLading.CY_IssuerCode = "";
			AssertEquals("", billOfLading.CY_IssuerCode);
			billOfLading.CY_BillNumber = "8734587";
			AssertEquals("8734587", billOfLading.CY_BillNumber);
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			IssuerAndBillNumber billOfLadingLoaded = factory2.Load<IssuerAndBillNumber>(billOfLading.PK);
			AssertEquals("", billOfLadingLoaded.CY_IssuerCode);
			AssertEquals("8734587", billOfLadingLoaded.CY_BillNumber);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var message = factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var result = factory.New<IssuerAndBillNumber>();
			result.CY_Data = "AAAA12584";
			result.Parent = message;
			return result;
		}
	}
}
