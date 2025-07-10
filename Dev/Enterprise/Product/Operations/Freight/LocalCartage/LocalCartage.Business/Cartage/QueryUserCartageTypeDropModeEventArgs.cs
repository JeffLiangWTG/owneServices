using Enterprise.ZArchitecture;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class QueryUserCartageTypeDropModeEventArgs : QueryUserEventArgs
	{
		public QueryUserCartageTypeDropModeEventArgs(string message, string addressButtonText, string jobTypeButtonText)
		{
			this.Message = message;
			this.AddressButtonText = addressButtonText;
			this.JobTypeButtonText = jobTypeButtonText;
		}

		public readonly string Message;
		public readonly string AddressButtonText;
		public readonly string JobTypeButtonText;
		public DropMode Response;
	}
}
