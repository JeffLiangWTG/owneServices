using System;

namespace Enterprise.MasterFiles.Business
{
	class OutstandingAmountValidation : IOutstandingAmountValidation
	{
		public virtual string Validate(IOutstandingAmountValidationWrapper wrapper)
		{
			if (wrapper.ShouldCheckTransactionHeaderOutstandingAmount)
			{
				if (wrapper.OutstandingAmount != 0 && Math.Sign(wrapper.OutstandingAmount) != Math.Sign(wrapper.TotalAmount))
				{
					return wrapper.GetTransactionHeaderErrorMessage();
				}

				if (Math.Abs(wrapper.OutstandingAmount) > Math.Abs(wrapper.TotalAmount))
				{
					return wrapper.GetTransactionHeaderErrorMessage();
				}
			}

			return string.Empty;
		}
	}
}
