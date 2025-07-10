using CargoWise.Common;

namespace Enterprise.Warehouse.Transactions.Business.Tools
{
	#region ReleaseManagerResult

	public static class ReleaseManagerResult
	{
		public static ReleaseManagerResult<T> Success<T>(T[] items)
		{
			Argument.NotNull(items, nameof(items));
			return new ReleaseManagerResult<T>(items, ActionResult.Success());
		}

		public static ReleaseManagerResult<T> Failure<T>(string errorMessage)
		{
			Argument.NotNullOrEmpty(errorMessage, nameof(errorMessage));
			return new ReleaseManagerResult<T>(null, ActionResult.Failure(errorMessage));
		}
	}

	#endregion

	#region ReleaseManagerResult<T>

	public class ReleaseManagerResult<T>
	{
		internal ReleaseManagerResult(T[] items, ActionResult result)
		{
			Items = items;
			Result = result;
		}

		public T[] Items { get; }
		public ActionResult Result { get; }
		public bool IsSuccess => Result.IsSuccess;
	}

	#endregion
}
