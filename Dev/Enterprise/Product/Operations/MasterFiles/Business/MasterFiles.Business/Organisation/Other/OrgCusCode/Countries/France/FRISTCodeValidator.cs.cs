namespace Enterprise.MasterFiles.Business
{
	sealed class FRISTCodeValidator
	{
		internal void Validate(OrgCusCode orgCusCode)
		{
			var istValue = orgCusCode.OK_CustomsRegNo;
			if (istValue.Length != 8 || !istValue.IsLettersAndNumbersOnlyOrEmpty)
			{
				orgCusCode.OK_CustomsRegNoInfo.AddMessageError(InvalidFormatMessage);
			}
		}

		static string InvalidFormatMessage => Res.GetString("8D41664D-84E6-43A2-A5E4-EA340FDEA347", "The registration number for IST must be exactly 8 alphanumeric characters.");
	}
}
