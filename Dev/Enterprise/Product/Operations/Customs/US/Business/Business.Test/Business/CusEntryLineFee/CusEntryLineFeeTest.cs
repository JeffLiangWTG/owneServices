using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusEntryLineFee))]
	sealed class CusEntryLineFeeTest : Customs.Business.Testing.CusEntryLineFeeTest
	{
		public void TestIFee()
		{
			CusEntryLineFee lineFee = Factory.New<CusEntryLineFee>();
			lineFee.CF_ChargeType = "016";
			lineFee.CF_ChargeAmount = 234.3m;
			IFee fee = lineFee;
			AssertEquals(234.3m, fee.Amount);
			AssertEquals("016", fee.Code);
			AssertEquals(false, fee.IsOverridden);
		}

		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.CusEntryLineFee to include a decider for this class", Factory.New(typeof(Customs.Business.CusEntryLineFee)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestIsLineFee()
		{
			CusEntryLineFee lineFee = Factory.New<CusEntryLineFee>();
			lineFee.CF_ChargeType = "016";
			AssertEquals(false, lineFee.IsLineFee);
			lineFee.CF_ChargeType = "017";
			AssertEquals(false, lineFee.IsLineFee);
			lineFee.CF_ChargeType = "ABC";
			AssertEquals(false, lineFee.IsLineFee);
			lineFee.CF_ChargeType = Core.Constants.USCustoms.FeeCodes.Watermelon;
			AssertEquals(true, lineFee.IsLineFee);
		}

		public void TestIFeeAmount()
		{
			CusEntryLineFee lineFee = Factory.New<CusEntryLineFee>();
			lineFee.CF_ChargeAmount = 234.3m;
			AssertEquals("Amount", 234.3m, ((IFee)lineFee).Amount);
			lineFee.CF_ChargeAmount = ZDecimal.Zero;
			AssertEquals("Amount", 0m, ((IFee)lineFee).Amount);
		}

		public void TestInformalFeeDeletedWhenTheSecondImportChangesToFormalEntry()
		{
			//Duty Amount:326.00
			string messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10R888891-01319900091-013199000                 8         XJ5 0005505901891  CO 20     APL EMERALD         102809      93                   V123W               22            OBL93                               00000001PCS        OTT1       30                                                  1                   OTT1    40001FR00000034060000009000                    000000005060267                  50 9802004040                                                       FR071607N   51                                                                              60                                        XYBEREQU6LON                          709102111010 0000032600000000100000NO                               0000005258  809802004040                                                        0000002619  819102111020           000000100000NO                                           819802004040                                                        0000001344  819102111030           000000100000NO                                           819802004040                                                        0000000204  819102111040           000000100000NO                                           9000000032600                                              00000012831          ZZ7501000000010                                                                 ";
			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			NotificationCollection notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			AssertEquals("Total Duty Amount", 326m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalDutyAmount);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			//Still there
			AssertEquals("Total Duty Amount", 326m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalDutyAmount);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
