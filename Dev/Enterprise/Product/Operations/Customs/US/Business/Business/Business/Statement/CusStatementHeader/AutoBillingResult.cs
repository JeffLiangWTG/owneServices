using CargoWise.Types;
using Enterprise.Accounting.Integration;

namespace Enterprise.Customs.US.Business
{
	public class AutoBillingResult : IAutoBillingResult
	{
		public AutoBillingResult()
		{
			Message = "";
		}

		public AutoBillingResult(bool wasSuccessful)
		{
			WasSuccessful = wasSuccessful;
			Message = "";
		}

		public bool WasSuccessful
		{
			get { return wasSuccessful.HasValue && wasSuccessful.Value; }
			set { wasSuccessful = value; }
		}
		bool? wasSuccessful;

		public string Message { get; set; }

		public ZBool HasChanges { get; set; }

		public void SetResult(IAutoBillingResult result)
		{
			SetIfSuccessful(result.WasSuccessful);

			SetMessageResult(result.Message);

			HasChanges |= result.HasChanges;
		}

		public void SetResult(ICustomsPaymentCreationResult result)
		{
			SetIfSuccessful(result.WasSuccessful);

			SetMessageResult(result.ErrorMessage);

			HasChanges |= result.WasSuccessful;
		}

		void SetIfSuccessful(bool successful)
		{
			if (!wasSuccessful.HasValue)
			{
				WasSuccessful = successful;
			}
			else
			{
				WasSuccessful &= successful;
			}
		}

		void SetMessageResult(string messageToAdd)
		{
			if (!string.IsNullOrEmpty(messageToAdd))
			{
				if (!string.IsNullOrEmpty(Message))
				{
					Message += "\r\n";
				}

				Message += messageToAdd;
			}
		}

		#region IAutoBillingResult Members

		ZString IAutoBillingResult.Message
		{
			get { return Message; }
		}

		ZBool IAutoBillingResult.WasSuccessful
		{
			get { return WasSuccessful; }
		}

		#endregion
	}
}
