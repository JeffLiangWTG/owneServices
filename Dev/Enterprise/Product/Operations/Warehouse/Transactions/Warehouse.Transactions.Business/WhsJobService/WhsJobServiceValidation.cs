using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsJobServiceValidation : JobServiceValidation
	{
		public WhsJobServiceValidation(AutoJobService parent) : base(parent)
		{
		}

		protected override void CheckES_ServiceCode()
		{
			base.CheckES_ServiceCode();
			if (Parent.ES_ServiceCode == ChargeCodeSubGroupList.Storage)
			{
				Parent.ES_ServiceCodeInfo.AddError(Res.GetString("b79bac7f-df3d-4911-9785-d570ccd0362c", "{0} code is reserved to calculate storage charges in periodic billing. Please choose another code.", ChargeCodeSubGroupList.Storage));
			}
		}

		protected override void CheckES_CompletedDateTimeOffset()
		{
			base.CheckES_CompletedDateTimeOffset();
			if (!Parent.ES_CompletedDateTimeOffset.IsValid)
			{
				var shouldCheckCompleted = false;
				if (Parent.ES_ParentTableCode == WhsVASOrderSchema.Constants.Prefix)
				{
					var vasOrder = Parent.Factory.Load<WhsVASOrder>(Parent.ES_ParentID);
					shouldCheckCompleted = vasOrder.IsFinalisingOrTransferringOut;
				}
				else if (Parent.ES_ParentTableCode == WhsAdHocServiceJobSchema.Constants.Prefix)
				{
					var adHocService = Parent.Factory.Load<WhsAdHocServiceJob>(Parent.ES_ParentID);
					shouldCheckCompleted = adHocService.IsFinalising;
				}

				if (shouldCheckCompleted)
				{
					Parent.ES_CompletedDateTimeOffsetInfo.AddError(Res.GetString("13646c27-e294-4675-9f35-59df8a84994b", "Service Job should be Completed before Finalizing."));
				}
			}
		}
	}
}
