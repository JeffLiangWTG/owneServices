
namespace Enterprise.Freight.Agency.Business
{
	public abstract class ReleaseImportOrderActionMethodApplicator : AutoReleaseImportOrderActionMethodApplicator
	{
		protected ReleaseImportOrderActionMethodApplicator(string name)
			: base(name)
		{
		}

		protected static bool IsPendingResponse(BillOfLadingContainer container)
		{
			switch (container.JC_ImportReleaseOrderStatus)
			{
				case ReleaseImportOrderMessageStatusList.Codes.OriginalSent:
				case ReleaseImportOrderMessageStatusList.Codes.WithdrawSent:
					return true;

				default:
					return false;
			}
		}
	}
}
