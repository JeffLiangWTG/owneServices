using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.PL;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(ExitSummaryJobDeclarationValidation))]
sealed class ExitSummaryJobDeclarationValidationTest : BaseExportJobDeclarationValidationTest
{
	public void TestCheckJE_MessageType() => CombineAssertions(() =>
	{
		const string messageError = "Itinerary Countries are required for EXS - Exit Summary Declaration.";

		AssertHasMessageError("Exit Summary Declaration, 0 Itinerary Countries", jobDeclaration.JE_MessageTypeInfo, messageError);

		jobDeclaration.ItineraryCountries.AddNew();
		jobDeclaration.Validation.ValidateJE_MessageType();
		AssertNoMessageError("Exit Summary Declaration, 1 Itinerary Country", jobDeclaration.JE_MessageTypeInfo, messageError);
	});

	public void TestCheckJE_TransportModeMandatory()
	{
		jobDeclaration.JE_TransportMode = ZString.Empty;
		jobDeclaration.Validation.ValidateJE_TransportMode();
		AssertNoNotifications(jobDeclaration.JE_TransportModeInfo);
	}

	public void TestCheckJE_OH_ShippingLine() => CombineAssertions(() =>
	{
		jobDeclaration.JE_OH_ShippingLine = ZGuid.Empty;
		jobDeclaration.Validation.ValidateJE_OH_ShippingLine();
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_OH_ShippingLineInfo);

		jobDeclaration.JE_OH_ShippingLine = ZGuid.BrettsGuid;
		AssertNoNotifications("Carrier selected", jobDeclaration.JE_OH_ShippingLineInfo);
	});

	protected override void SetUp()
	{
		base.SetUp();
		jobDeclaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
	}
}
