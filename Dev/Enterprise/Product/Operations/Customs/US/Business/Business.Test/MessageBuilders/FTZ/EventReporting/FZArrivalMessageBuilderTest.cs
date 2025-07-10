using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.Testing;
using NUnit.Framework;
using Constants = Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class FZArrivalMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2012, 03, 09)]
		public void TestGoodsArrivalForBillsAndInBonds()
		{
			#region Test Case example

			/*B013910SV9FZ                                               117231               
			102XXXAJSOCT0113                      I                         W004            
			11               15300011200000055                                              
			103VNY00000121                        I                         W004            
			Y  3910SV9FZ00003

			B013910SV9NF                                               117231               
			90        48           0                                                        
			91B6 2  XXXAJSOCT0113                      1203082219                           
			9502119 PHYSICAL ARRIVAL DATA ACCEPTED                                          
			90A153000112000000552501Y                                                       
			91B6 3  VNY00000121                        1203082219                           
			9502119 PHYSICAL ARRIVAL DATA ACCEPTED                                          
			Y  3910SV9NF00002                                                               

			B013910SV9FZ                                               HYEDUSCMT_117362     
			10277739100036HAWB002                 I                                         
			10277739100036HAWB001                 I                                         
			11               15300011200000060                                              
			Y  3910SV9FZ00003

			B013910SV9NF                                               HYEDUSCMT_117362     
			90Z153000112000000602501N                                                       
			91B6 2  7773910003600000HAWB002            1203271735                           
			9502119 PHYSICAL ARRIVAL DATA ACCEPTED                                          
			90Z153000112000000602501N                                                       
			91B6 2  7773910003600000HAWB001            1203271735                           
			9502119 PHYSICAL ARRIVAL DATA ACCEPTED                                          
			Y  3910SV9NF00002*/

			#endregion

			var declaration = GetDeclaration();
			declaration.FTZAdmissionNumber = "1530001|12|00000001";

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.US_UI_NKBillIssuerSCAC = "XXXA";
			bill.CU_BillNum = "JSOCT0113";
			bill.CU_NoOfPacks = 12m;

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "TESTBILL2";
			bill2.CU_NoOfPacks = 10m;

			var bill3 = declaration.Bills.AddNew();
			bill3.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill3.CU_BillNum = "TESTBILL3";
			bill3.CU_NoOfPacks = 11m;

			var itNo = bill2.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "VNY00000121";

			var builder = new FZArrivalMessageBuilder(declaration);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				var message = builder.PopulateMessage();
				AssertEquals(
	@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102TESTBILL2                          I                         W004            
11               15300011200000001                                              
102TESTBILL3                          I                         W004            
11               15300011200000001                                              
102XXXAJSOCT0113                      I                         W004            
11               15300011200000001                                              
Y  8888XJ5FZ", message.EM_FormattedMessageText);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				var message = builder.PopulateMessage();
				AssertEquals(
	@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102XXXAJSOCT0113                      I                         W004            
11               15300011200000001                                              
102TESTBILL2                          I                         W004            
11               15300011200000001                                              
Y  8888XJ5FZ", message.EM_FormattedMessageText);
			}
		}

		[TestDate(2021, 01, 01)]
		public void TestGoodsArrivalForBillsWithSplitDetails()
		{
			var declaration = GetDeclaration();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.FTZAdmissionNumber = "1530001|12|00000001";
			declaration.JE_MasterBill = "JSOCT0113";
			declaration.JE_HouseBill = "TESTBILL2";

			var bill = declaration.PrimaryHouseBill;
			bill.US_SESplitShip = true;
			var splitDetail1 = bill.ITAndSplitDetails.AddNew();
			splitDetail1.US_CarrierCode = "ABC";
			splitDetail1.US_FlightNumber = "001";
			splitDetail1.US_ArrivalDate = ZDateTime.Today;
			var splitDetail2 = bill.ITAndSplitDetails.AddNew();
			splitDetail2.US_CarrierCode = "DEF";
			splitDetail2.US_FlightNumber = "002";
			splitDetail2.US_ArrivalDate = ZDateTime.Today.AddDays(1);

			var builder = new FZArrivalMessageBuilder(declaration);
			var message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102JSOCT0113TESTBILL2                 I                         W004            
11               15300011200000001   ABC 001            20210101                
102JSOCT0113TESTBILL2                 I                         W004            
11               15300011200000001   DEF 002            20210102                
Y  8888XJ5FZ", message.EM_FormattedMessageText);
		}

		JobDeclaration GetDeclaration()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_US_NKLocationOfGoods = "W004";
			declaration.JE_DateOfArrival = new ZDateTime(2011, 10, 11);
			return declaration;
		}
	}
}
