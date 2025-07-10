using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ExternalValidationResultWrapper : NonPersistentBusinessObject
	{
		public ExternalValidationResultWrapper(ExternalValidationResult result)
		{
			this.result = result;
		}

		readonly ExternalValidationResult result;
		public static MultilingualString ErrorStatus = ResString.GetMultilingualString("a09e3fb6-ced3-48f5-a5eb-2b93ac769da3", "Error");
		public static MultilingualString WarningStatus = ResString.GetMultilingualString("1cba4dcb-5f79-4402-a104-7659156c34ad", "Warning");

		public ExternalValidationResultMessageCollection Messages
		{
			get
			{
				var messages = new ExternalValidationResultMessageCollection();
				if (result.Errors != null)
				{
					foreach (string errorMessage in result.Errors)
					{
						messages.Add(new ExternalValidationResultMessage(ErrorStatus, errorMessage));
					}
				}
				if (result.Warnings != null)
				{
					foreach (string warningMessage in result.Warnings)
					{
						messages.Add(new ExternalValidationResultMessage(WarningStatus, warningMessage));
					}
				}
				return messages;
			}
		}

		public bool ResultIsValid
		{
			get { return result.IsValid; }
		}
	}
}
