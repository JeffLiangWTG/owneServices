namespace Enterprise.MasterFiles.Business
{
	class OPYORCTransactionOutstandingAmountValidation : IOutstandingAmountValidation
	{
		public string Validate(IOutstandingAmountValidationWrapper wrapper)
		{
			if (wrapper.ShouldCheckTransactionHeaderOutstandingAmount)
			{
				if (wrapper.OutstandingAmount != wrapper.TotalAmount)
				{
					return wrapper.GetTransactionHeaderErrorMessage();
				}
			}

			return string.Empty;
		}
	}
}
