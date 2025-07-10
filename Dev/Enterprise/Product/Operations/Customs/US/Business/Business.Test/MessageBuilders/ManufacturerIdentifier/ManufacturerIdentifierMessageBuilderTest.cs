using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ManufacturerIdentifierMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateWhenUpdateActionCodeIsAdd()
		{
			DeclarationTestHelper.SetupForSendMessage();
			var org = Factory.New<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(org);
			var messageData = new ManufacturerAddMessageData(wrapper);
			var message = ManufacturerIdentifierMessageBuilder.Generate(messageData, ManufacturerIdentifierManager.Constants.UpdateActionCode.Add);
			AssertEquals("B018888XJ5$I                                89             <<MSGNO PLACEHOLDER>>$A" + org.MainAddress.PK.ToString().PadRight(78).ToUpper() +
				@"$1A00001US                                                                      " +
				"$3                                                                              " +
				"$4                                                      US                      " +
				"Y  8888XJ5$I00004", message.EM_MessageText);
		}

		public void TestGenerateWhenUpdateActionCodeIsUpdate()
		{
			DeclarationTestHelper.SetupForSendMessage();
			var org = Factory.New<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(org);
			var messageData = new ManufacturerAddMessageData(wrapper);
			var message = ManufacturerIdentifierMessageBuilder.Generate(messageData, ManufacturerIdentifierManager.Constants.UpdateActionCode.Update);
			AssertEquals("B018888XJ5$I                                89             <<MSGNO PLACEHOLDER>>$A" + org.MainAddress.PK.ToString().PadRight(78).ToUpper() +
				@"$1U00001                                                                        " +
				"$3                                                                              " +
				"$4                                                      US                      " +
				"Y  8888XJ5$I00004", message.EM_MessageText);
		}

		public void TestCanadaOrganisationState()
		{
			var org = Factory.New<OrgHeader>();
			org.MainAddress.OA_RL_NKRelatedPortCode = "CAPRW";
			org.MainAddress.OA_City = "Prince William";
			org.MainAddress.OA_State = "NS";

			var wrapper = OrgHeaderWrapper.New(org);
			var messageData = new ManufacturerAddMessageData(wrapper);
			var message = ManufacturerIdentifierMessageBuilder.Generate(messageData, ManufacturerIdentifierManager.Constants.UpdateActionCode.Add);
			Assert("Nova Scotia state code should appear, but does not", message.EM_MessageText.Contains("XN"));
		}

		public void TestEndToEndWithGeneratedMIDWhenUpdateActionCodeIsAdd()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();

			organisation.OH_FullName = "ALPHANTRANS INTERNATIONAL555554444444444555555555566666666667777777777888888888899999999991111111111";
			organisation.MainAddress.OA_Address1 = "215-5000 MILLER9ROAD";
			organisation.MainAddress.OA_City = "VANCOUVER AIRPORT 901234";
			organisation.MainAddress.OA_PostCode = "V7A 4E9";//space should be removed for CA
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "CAYVR";
			organisation.MainAddress.OA_State = "AB";

			var wrapper = OrgHeaderWrapper.New(organisation);
			var messageData = new ManufacturerAddMessageData(wrapper);
			var message = ManufacturerIdentifierMessageBuilder.Generate(messageData, ManufacturerIdentifierManager.Constants.UpdateActionCode.Add);

			AssertEquals("message generated", (ZString)$@"B01       $I                                               <<MSGNO PLACEHOLDER>>$A{organisation.MainAddress.PK.ToString().PadRight(78).ToUpper()}$1A00001XAALPHANTRANS INTERNATIONAL555554444444444555555555566666666667777777777$2888888888899999999991111111111215-5000 MILLER9ROAD                            $3                                                   VANCOUVER AIRPORT 90123    $4                                            V7A4E9    XAALPINT2155VAN         Y         $I00005", message.EM_MessageText);
		}

		public void TestEndToEndWithGeneratedMIDWhenUpdateActionCodeIsUpdate()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();

			organisation.OH_FullName = "ALPHANTRANS INTERNATIONAL555554444444444555555555566666666667777777777888888888899999999991111111111";
			organisation.MainAddress.OA_Address1 = "215-5000 MILLER9ROAD";
			organisation.MainAddress.OA_City = "VANCOUVER AIRPORT 901234";
			organisation.MainAddress.OA_PostCode = "V7A 4E9";//space should be removed for CA
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "CAYVR";
			organisation.MainAddress.OA_State = "AB";

			var wrapper = OrgHeaderWrapper.New(organisation);
			var messageData = new ManufacturerAddMessageData(wrapper);
			var message = ManufacturerIdentifierMessageBuilder.Generate(messageData, ManufacturerIdentifierManager.Constants.UpdateActionCode.Update);

			AssertEquals("message generated", (ZString)$@"B01       $I                                               <<MSGNO PLACEHOLDER>>$A{organisation.MainAddress.PK.ToString().PadRight(78).ToUpper()}$1U00001                                                                        $2888888888899999999991111111111215-5000 MILLER9ROAD                            $3                                                   VANCOUVER AIRPORT 90123    $4                                            V7A4E9    XAALPINT2155VAN         Y         $I00005", message.EM_MessageText);
		}
	}
}
