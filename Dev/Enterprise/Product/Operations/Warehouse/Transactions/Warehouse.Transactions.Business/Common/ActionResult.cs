using CargoWise.Common;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Transactions.Business
{
	[Immutable]
	public struct ActionResult
	{
		ActionResult(bool isSuccess, string errorMessage)
		{
			IsSuccess = isSuccess;
			ErrorMessage = errorMessage;
		}

		public static ActionResult Success()
		{
			return new ActionResult(true, "");
		}

		public static ActionResult Failure(string errorMessage)
		{
			Argument.NotNullOrEmpty(errorMessage, nameof(errorMessage));
			return new ActionResult(false, errorMessage);
		}

		public override int GetHashCode()
		{
			return IsSuccess.GetHashCode()
				^ ErrorMessage.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return obj is ActionResult && this == (ActionResult)obj;
		}

		public static bool operator ==(ActionResult actionResult1, ActionResult actionResult2)
		{
			return actionResult1.IsSuccess == actionResult2.IsSuccess && actionResult1.ErrorMessage == actionResult2.ErrorMessage;
		}

		public static bool operator !=(ActionResult actionResult1, ActionResult actionResult2)
		{
			return !(actionResult1 == actionResult2);
		}

		public bool IsSuccess { get; }

		public string ErrorMessage { get; }
	}
}
