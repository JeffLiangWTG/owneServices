using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class CusDV1DetailProviderTest : TestCaseWithFactory
	{
		public void TestCusDeclarationOfValueLinesMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				var declarationOfValue = declaration.DeclarationOfValue.ToArray();
				var declarationOfValueLines = declarationOfValue[0].DeclarationOfValueLines.ToArray();

				CombineAssertions("Declaration Of Value Lines 1 Test", () =>
				{
					AssertEquals("DeclarationOfValueLineNo", 1, declarationOfValueLines[0].DeclarationOfValueLineNo);
					AssertEquals("DeclarationLineNo", 1, declarationOfValueLines[0].DeclarationLineNo);
					AssertEquals("IndirectPayment", ZDecimal.Zero, declarationOfValueLines[0].IndirectPayment);
					AssertEquals("Commission", 520m, declarationOfValueLines[0].Commission);
					AssertEquals("BrokerageCommission", ZDecimal.Zero, declarationOfValueLines[0].BrokerageCommission);
					AssertEquals("PackageValue", ZDecimal.Zero, declarationOfValueLines[0].PackageValue);
					AssertEquals("ImportMergedGoods", ZDecimal.Zero, declarationOfValueLines[0].ImportMergedGoods);
					AssertEquals("VehiclesManufacturedForImport", ZDecimal.Zero, declarationOfValueLines[0].VehiclesManufacturedForImport);
					AssertEquals("ProductionAndConsumptionGoodsForImport", ZDecimal.Zero, declarationOfValueLines[0].ProductionAndConsumptionGoodsForImport);
					AssertEquals("PlanDraft", ZDecimal.Zero, declarationOfValueLines[0].PlanDraft);
					AssertEquals("RoyaltyLicence", 1560m, declarationOfValueLines[0].RoyaltyLicence);
					AssertEquals("IndirectTransition", 2080m, declarationOfValueLines[0].IndirectTransition);
					AssertEquals("Transportation", 5200m, declarationOfValueLines[0].Transportation);
					AssertEquals("Insurance", 1560m, declarationOfValueLines[0].Insurance);
					AssertEquals("AfterEntryTransportation", ZDecimal.Zero, declarationOfValueLines[0].AfterEntryTransportation);
					AssertEquals("TechnicalAssistance", ZDecimal.Zero, declarationOfValueLines[0].TechnicalAssistance);
					AssertEquals("OtherPayments", 2600m, declarationOfValueLines[0].OtherPayments);
					AssertEquals("OtherPaymentQualification", ZString.Empty, declarationOfValueLines[0].OtherPaymentQualification);
					AssertEquals("TaxFeesFund", ZDecimal.Zero, declarationOfValueLines[0].TaxFeesFund);
				});

				CombineAssertions("Declaration Of Value Lines 2 Test", () =>
				{
					AssertEquals("DeclarationOfValueLineNo", 2, declarationOfValueLines[1].DeclarationOfValueLineNo);
					AssertEquals("DeclarationLineNo", 2, declarationOfValueLines[1].DeclarationLineNo);
				});
			}
		}
	}
}
