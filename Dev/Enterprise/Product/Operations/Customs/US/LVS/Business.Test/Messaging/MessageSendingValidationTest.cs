using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	public class MessageSendingValidationTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			AssertEquals(typeof(Customs.Business.MessageSendingValidation), MessageSendingValidation.New(clearance, null).GetType());
			AssertEquals(typeof(MessageSendingValidation), MessageSendingValidation.New(clearance).GetType());
		}

		public void TestMarkAsNeedingValidationIncludingChildren()
		{
			var messageError = "Agency Program declarations are not supported in this module and will need to be completed using a stand alone declaration.";
			string tariffNumber = "8542996328";

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffNumber;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDate.Today;
			tariff.UE_PGACodes = "FD1";

			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var cusUSLVItem1 = consignment.CusUSLVItems.AddNew();
			var cusUSLVItem2 = consignment.CusUSLVItems.AddNew();
			cusUSLVItem2.ULI_Tariff = tariffNumber;
			cusUSLVItem2.MarkLightValidationAsValidForTesting();
			AssertEquals(true, cusUSLVItem2.LightValidationIsValid);

			var baseMessageSendingValidation = MessageSendingValidation.New(clearance, null);
			baseMessageSendingValidation.CheckBusinessObjectLevelValidation();
			AssertEquals(false, cusUSLVItem2.GetMessageErrors().ContainsNotificationContaining(messageError));

			cusUSLVItem2.MarkLightValidationAsValidForTesting();
			AssertEquals(true, cusUSLVItem2.LightValidationIsValid);

			var messageSendingValidation = MessageSendingValidation.New(clearance);
			messageSendingValidation.CheckBusinessObjectLevelValidation();
			AssertEquals(true, cusUSLVItem2.GetMessageErrors().ContainsNotificationContaining(messageError));
		}
	}
}
