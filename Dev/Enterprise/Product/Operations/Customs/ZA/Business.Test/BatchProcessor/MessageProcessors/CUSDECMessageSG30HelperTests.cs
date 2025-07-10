using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Customs.ZA.Business.DocumentWrappers;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.Edifact.D96B.Messages.CUSDEC;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using D96BMessageFactory = Enterprise.Edifact.D96B.EdifactD96BMessageFactory;

namespace Enterprise.Customs.ZA.Business.MessageProcessor.Testing
{
	[TestedType(typeof(CUSDECMessageSG30Helper))]
	sealed class CUSDECMessageSG30HelperTests : NonPersistentBusinessObjectTestCase
	{
		public void TestILineLevelInformation()
		{
			var messageBody = @"UNH+123+CUSDEC:D:96B:UN:ZZZ01'
BGM+929+LRN::00001+9'
CST++A:117:ZZZ'
LOC+14+LOCGO::ZZZ'
LOC+18+EXWHS::ZZZ'
LOC+45+POEXI::ZZZ'
LOC+35+AU::5'
LOC+36+ZA::5'
LOC+96+DOC::ZZZ'
LOC+9+POLOA::5'
DTM+141:20160219:102'
DTM+132:20160220:102'
DTM+178:20160218:102'
GIS+N1:127:ZZZ'
GIS+1:134:ZZZ'
MEA+AAE+AAD+KGM:5000.46'
EQD+CN+CONT1+:::SEAL1+++4'
EQD+CN+CONT2+:::SEAL1          SEAL2+++5'
EQD+CN+CONT3+:::               SEAL2+++7'
EQD+CN+CONT4+:::SEAL1SEALASEALB+++8'
EQD+CN+CONT5+:::SEAL1SEALASEALB'
FTX+LIN+++3:1:2:TRANBKCODE:1'
RFF+BH:HBOL01'
DTM+137:20150101:102'
RFF+ABT:ORGMRN'
RFF+AAS:MBOL01'
DTM+137:20150102:102'
RFF+ABI:FINACCNUM'
RFF+UCN:UCR'
RFF+ACD:JOBNUM'
RFF+AAV:CASENUM'
PAC+200'
PCI++MARK1'
PCI++MARK2'
PCI++MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3'
TDT+20+JQ841+4:1+++++:::VESSEL'
DOC+380+INVN1'
DTM+3:20140101:102'
DOC+380+INVN2'
DTM+3:20140102:102'
NAD+AG+00626166'
NAD+AF+RTC'
NAD+MS+TST'
NAD+BY+BOC'
UNS+D'
CST+1+020110:108:ZZZ+100'
FTX+AAA+++THIS IS A LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG :LONG LONG LONG LONG LONG LONG LONG LONG LONG DESCRIPTION'
FTX+ACB+++NUIN:VDN141516:VTE:BND00000000000000000000000000000500:TS5VALUETS5'
FTX+AAI+++TS6VALUETS6:TS7VALUETS77777777777777777'
FTX+CCI+++11:00:promea::1'
LOC+27+AU'
MEA+AAR++KG:101.00'
MEA+AAS++M3:202.00'
MEA+AAT++CQ:303.00'
MEA+AAF++KG:404'
NAD+WH+WPC'
MOA+38:200'
MOA+40:88'
RFF+WE:PTA201604198464646:0001'
TAX+1+1P1:107:ZZZ'
MOA+161:11.00'
TAX+1+12A:107:ZZZ'
MOA+161:12.10'
TAX+1+12B:107:ZZZ'
MOA+161:12.20'
TAX+1+13A:107:ZZZ'
MOA+161:13.10'
TAX+1+13B:107:ZZZ'
MOA+161:13.20'
TAX+1+13C:107:ZZZ'
MOA+161:13.30'
TAX+1+13D:107:ZZZ'
MOA+161:13.40'
TAX+1+15A:107:ZZZ'
MOA+161:15.10'
TAX+1+15B:107:ZZZ'
MOA+161:15.20'
TAX+1+1P8:107:ZZZ'
MOA+161:18.00'
TAX+1+2P1:107:ZZZ'
MOA+161:21.00'
TAX+1+2P2:107:ZZZ'
MOA+161:22.00'
TAX+1+2P3:107:ZZZ'
MOA+161:23.00'
TAX+1+VAT:107:ZZZ'
MOA+161:44.44'
TAX+1+SUR:107:ZZZ'
MOA+161:55.55'
TAX+1+PEN:107:ZZZ'
MOA+161:66.66'
TAX+1+FOR:107:ZZZ'
MOA+161:99.99'
TAX+1+DLA:107:ZZZ'
MOA+161:77.77'
TAX+1+PPA:107:ZZZ'
MOA+161:88.88'
TAX+1+PPE:107:ZZZ'
MOA+161:10.00'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:200'
TAX+3+TRN:107:ZZZ'
MOA+161:700.70'
TAX+3+TDD:107:ZZZ'
MOA+161:300.30'
TAX+3+TVD:107:ZZZ'
MOA+161:400.40'
TAX+3+AOP:107:ZZZ'
MOA+161:500.50'
TAX+3+AUP:107:ZZZ'
MOA+161:600.60'
TAX+3+CUS:107:ZZZ'
MOA+161:800'
UNT+111+123'
";
			var helper = GetHelper(messageBody.Replace("\r", "").Replace("\n", "")) as ILineLevelInformation;
			CombineAssertions(() =>
			{
				AssertEquals("promea", helper.ProcedureMeasure);
				AssertEquals("1", helper.TradeStatisticsIndicator);
				AssertEquals("0001", helper.WarehousingMRNLineNumber);
				AssertEquals("100", helper.PreferenceCode);
				AssertEquals(200m, helper.ActualPrice);
				AssertEquals("AU", helper.CountryOfOrigin);
				AssertEquals("11", helper.CustomsProcedureCode);
				AssertEquals(101m, helper.CustomsQuantity);
				AssertEquals("KG", helper.CustomsUnitQty);
				AssertEquals(202m, helper.AdditionalQuantity);
				AssertEquals("M3", helper.AdditionalUnitQty);
				AssertEquals(303m, helper.ClassificationQuantity);
				AssertEquals("CQ", helper.ClassificationUnitQty);
				AssertEquals(404m, helper.WarehouseCountableQuantity);
				AssertEquals("KG", helper.WarehouseCountableUnitQty);
				AssertEquals(88m, helper.CustomsValue);
				AssertEquals("THIS IS A LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG DESCRIPTION", helper.GoodsDescription);
				AssertEquals("1", helper.LineNumber);
				AssertEquals("00", helper.PreviousProcedureCode);
				AssertEquals("PTA201604198464646", helper.PreviousProcedureMRN);
				AssertEquals("020110", helper.TariffCode);
				AssertEquals("WPC", helper.RebateUserCode);
				AssertEquals(16, helper.DutiesAndFees.Count());
				AssertEquals(4, helper.ProvisionalPayments.Count());
				var sg30Helper = (CUSDECMessageSG30Helper)helper;
				AssertEquals("141516", sg30Helper.ValueDeterminationNumber);
				AssertEquals(14, sg30Helper.FeeValues.CustomsDutiesExcluding12B.Count());
				AssertEquals(323.72m, sg30Helper.FeeValues.CustomsDutiesExcluding12B.Sum(x => x.Value));
				AssertEquals(323.72m, sg30Helper.FeeValues.CustomsDutyExcluding12B);
				AssertEquals(12.2m, sg30Helper.FeeValues.S1P2BDuty);
				AssertEquals(44.44m, sg30Helper.FeeValues.ValueAddedTax);
				AssertEquals(166.65m, sg30Helper.FeeValues.Penalty);
				AssertEquals(166.65m, sg30Helper.FeeValues.Penalties.Sum(x => x.Value));
				AssertEquals(98.88m, sg30Helper.FeeValues.ProvisionalPayment);
				AssertEquals(98.88m, sg30Helper.FeeValues.ProvisionalPayments.Sum(x => x.Value));
				AssertEquals(77m, sg30Helper.FeeValues.CustomsDutiesSchedule1P1andSchedule2);
				AssertEquals(7, helper.AdditionalInformations.Count());
				var addInfos = helper.AdditionalInformations.ToList();
				CheckAdditionalInformationDetails(addInfos, "BND", "500");
				CheckAdditionalInformationDetails(addInfos, "NUI", "N");
				CheckAdditionalInformationDetails(addInfos, "VDN", "141516");
				CheckAdditionalInformationDetails(addInfos, "VTE", "");
				CheckAdditionalInformationDetails(addInfos, "TS5", "VALUETS5");
				CheckAdditionalInformationDetails(addInfos, "TS6", "VALUETS6");
				CheckAdditionalInformationDetails(addInfos, "TS7", "VALUETS77777777777777777");
			});
		}

		protected override BusinessObject GetNewBusinessObject() => new CUSDECMessageSG30Helper(null, ZDateTime.Today, new BusinessObjectFactory());

		protected override void SetUp()
		{
			ZAUniversalReferenceTestDataHelper.SetupBasicTariffTypesForZATesting(Factory);
			base.SetUp();
		}

		void CheckAdditionalInformationDetails(List<IAdditionalInformation> addInfos, string codeToTest, string expectedValue)
		{
			var assertMsg = "Checking AdditionalInformationDocWrapper where Code = " + codeToTest;
			var wrapper = (AdditionalInformationDocWrapper)addInfos.Find(x => x.Code == codeToTest);
			Assert(assertMsg, wrapper != null);
			AssertEquals(assertMsg, codeToTest, wrapper.Code);
			AssertEquals(assertMsg, expectedValue, wrapper.Value);
		}

		CUSDECMessageSG30Helper GetHelper(string messageBody)
		{
			var testInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			var testMessage = testInterchange.ContainedMessages.AddNew();
			testMessage.EM_MessageText = messageBody;
			var d96bMessageFactory = new D96BMessageFactory();
			var zaCharSet = new ZACharacterSet();
			CUSDECMessage cusdecMessage = testMessage.GetAutoEdifactMessageUsingNamedFactory(d96bMessageFactory, zaCharSet) as CUSDECMessage;
			return new CUSDECMessageSG30Helper(cusdecMessage.Group30[0], ZDateTime.Today, new BusinessObjectFactory());
		}
	}
}
