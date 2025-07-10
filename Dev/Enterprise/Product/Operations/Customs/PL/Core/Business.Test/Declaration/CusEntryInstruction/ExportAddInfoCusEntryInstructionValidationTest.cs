namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class ExportAddInfoCusEntryInstructionValidationTest : AddInfoCusEntryInstructionValidationTest
{
	#region ZG_EADPrintOut

	public void TestCheckRuleR523()
	{
		var dec = Factory.New<JobDeclaration>();
		var entry = dec.CustomsEntryInstructions.AddNew();
		var messageError = "(R523) Invalid value for EAD generation (printout) type. Code 1 is allowed only when Exit Customs office code starts with PL.";

		entry.ZG_EADPrintOut = EadPrintOutList.Codes._1;
		AssertHasMessageError(entry.ZG_EADPrintOutInfo, messageError);

		dec.JE_OfficeOfEntryExit = "DE1031";
		entry.AddInfoValidation.ValidateZG_EADPrintOut();
		AssertHasMessageError(entry.ZG_EADPrintOutInfo, messageError);

		entry.ZG_EADPrintOut = EadPrintOutList.Codes._0;
		AssertNoMessageError(entry.ZG_EADPrintOutInfo, messageError);

		entry.ZG_EADPrintOut = EadPrintOutList.Codes._2;
		AssertNoMessageError(entry.ZG_EADPrintOutInfo, messageError);

		dec.JE_OfficeOfEntryExit = "PL1031";
		entry.AddInfoValidation.ValidateZG_EADPrintOut();
		AssertNoMessageError(entry.ZG_EADPrintOutInfo, messageError);
	}

	#endregion
}
