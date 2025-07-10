#if DEBUG

namespace Enterprise.MasterFiles.Business
{
	public partial class AccGLHeaderCollection
	{
		public AccTransactionLines TransactionLines_ForTestOnly => TransactionLines;
	}
}

#endif
