using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

public class AddInfoJobDeclarationValidationTest : TestCaseWithFactory
{
	public void TestCheckZG_SpecificCircumstanceIndicator()
	{
		jobDeclaration.ZG_SpecificCircumstanceIndicator = "Z";
		AssertHasMessageError(jobDeclaration.ZG_SpecificCircumstanceIndicatorInfo, ListValidation.InvalidCodeMessageError);

		jobDeclaration.ZG_SpecificCircumstanceIndicator = "A20";
		AssertNoNotifications(jobDeclaration.ZG_SpecificCircumstanceIndicatorInfo);
	}

	public void TestCheckZG_CTStatusID()
	{
		jobDeclaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
		jobDeclaration.ZG_CTStatusID = "T2L";
		AssertHasMessageError(jobDeclaration.ZG_CTStatusIDInfo, ListValidation.InvalidCodeMessageError);

		jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
		jobDeclaration.ZG_CTStatusID = "BAH";
		AssertHasMessageError(jobDeclaration.ZG_CTStatusIDInfo, ListValidation.InvalidCodeMessageError);
		jobDeclaration.ZG_CTStatusID = "T2L";
		AssertNoNotifications(jobDeclaration.ZG_CTStatusIDInfo);

		jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
		jobDeclaration.ZG_CTStatusID = "BAH";
		AssertHasMessageError(jobDeclaration.ZG_CTStatusIDInfo, ListValidation.InvalidCodeMessageError);
		jobDeclaration.ZG_CTStatusID = "T2L";
		AssertNoNotifications(jobDeclaration.ZG_CTStatusIDInfo);
	}

	protected override void SetUp()
	{
		base.SetUp();
		jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageType;
	}

	protected JobDeclaration jobDeclaration;

	protected virtual string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;
}
