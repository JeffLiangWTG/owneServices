using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC055CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var interpreter = new CC055CMessageInterpreter();
		var mockCC055C = Mock.Of<ICC055CDataProvider>(provider =>
			provider.GuaranteeReferences == new[] {
				Mock.Of<INCTSGuaranteeReferenceProvider>(reference =>
					reference.GRN == "TestGRN1" &&
					reference.InvalidGuaranteeReasons == new[] {
						Mock.Of<INCTSInvalidGuaranteeReasonProvider>(reason =>
							reason.Code == "G01" &&
							reason.Text == "TestReasonText1"
						),
						Mock.Of<INCTSInvalidGuaranteeReasonProvider>(reason =>
							reason.Code == "G02" &&
							reason.Text == "TestReasonText2"
						),
					}
				),
				Mock.Of<INCTSGuaranteeReferenceProvider>(reference =>
					reference.GRN == "TestGRN2" &&
					reference.InvalidGuaranteeReasons == new[] {
						Mock.Of<INCTSInvalidGuaranteeReasonProvider>(reason =>
							reason.Code == "G03" &&
							reason.Text == "TestReasonText3"
						),
						Mock.Of<INCTSInvalidGuaranteeReasonProvider>(reason =>
							reason.Code == "G04" &&
							reason.Text == "TestReasonText4"
						),
					}
				),
			}
		);
		var result = interpreter.Interpret(mockCC055C);
		AssertEquals("Guarantee invalid.</br>GRN: TestGRN1</br>Reason: Code: G01 Guarantee does not exist</br>        Text: TestReasonText1</br>Reason: Code: G02 Guarantee exists, but not valid</br>        Text: TestReasonText2</br>GRN: TestGRN2</br>Reason: Code: G03 Access code not valid</br>        Text: TestReasonText3</br>Reason: Code: G04 Holder of Guarantee is not equal to Holder of Transit procedure in declaration</br>        Text: TestReasonText4", result);
	}
}
