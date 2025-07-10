using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class FZDeliveryMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2012, 03, 09)]
		public void TestSendMessage()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_US_NKLocationOfGoods = "W004";
			declaration.JE_DateOfArrival = new ZDateTime(2011, 10, 11);
			declaration.FTZAdmissionNumber = "1530001|12|00000001";

			var carrier = Factory.New<OrgHeader>();
			carrier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123456789012");
			declaration.DeliveryOrPickupCartageCoPK = carrier.PK;

			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "123456789";
			bill.CU_NoOfPacks = 12m;
			bill.US_F_Remarks = "Some remarks";
			var container = bill.Containers.AddNew();
			container.CO_ContainerNumber = "ABC";
			var container1 = bill.Containers.AddNew();
			container1.CO_ContainerNumber = "CBA";

			var itNo = bill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "6005006";
			itNo = bill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "V1006003";

			var builder = new FZDeliveryMessageBuilder(new FZEventAction(declaration, FZEventType.Delivery));
			var message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
10115300011200000001                                                            
Y  8888XJ5FZ", message.EM_FormattedMessageText);
		}
	}
}
