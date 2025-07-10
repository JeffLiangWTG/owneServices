using System;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService
{
#if DEBUG
	[Serializable]
#endif
	public class ErrorInfo : DataObjectInfo
	{
		#region Constructors

		public ErrorInfo()
			: this(0, "", "")
		{
		}

		public ErrorInfo(int sequence, string errorMessage, string errorType)
		{
			Sequence = sequence;
			ErrorMessage = errorMessage;
			ErrorType = errorType;
		}

		#endregion

		public int Sequence { get; set; }
		public string ErrorMessage { get; set; }
		public string ErrorType { get; set; }
	}
}
