namespace Enterprise.MasterFiles.Business
{
	class ARAPOutstandingAmountValidation : OutstandingAmountValidation
	{
		public override string Validate(IOutstandingAmountValidationWrapper wrapper)
		{
			var result = base.Validate(wrapper);
			if (!string.IsNullOrEmpty(result))
			{
				return result;
			}

			if (wrapper.ShouldCheckMatchLinkOutstandingAmount)
			{
				if (wrapper.OutstandingAmount != wrapper.TotalAmount - wrapper.MatchLinkAmountSum)
				{
					return wrapper.GetMatchLinkErrorMessage();
				}
			}
			return string.Empty;
		}
	}
}
