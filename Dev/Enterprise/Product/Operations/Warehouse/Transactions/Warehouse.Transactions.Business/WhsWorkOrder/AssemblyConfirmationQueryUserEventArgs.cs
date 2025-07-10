using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class AssemblyConfirmationQueryUserEventArgs : QueryUserMsgBoxEventArgs
	{
		public AssemblyConfirmationQueryUserEventArgs(bool defaultResponse)
			: base("", "", defaultResponse)
		{
		}
	}
}
