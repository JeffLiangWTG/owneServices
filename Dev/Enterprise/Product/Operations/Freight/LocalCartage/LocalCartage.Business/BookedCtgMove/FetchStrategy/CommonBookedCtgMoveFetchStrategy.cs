using CargoWise.Application;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonBookedCtgMoveFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CommonBookedCtgMoveFetchStrategy(CommonBookedCtgMove commonBookedCtgMove)
			: base(commonBookedCtgMove)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(CommonCartageLeg), JobContainerLegsSchema.JU_EW, CommonBookedCtgMove.PK);
			Factory.AddFetchHint(JobContainerSchema.Constants.TableName, CommonBookedCtgMove.EW_JC_Container);
			Factory.AddFetchHint(typeof(CommonCartage), CommonBookedCtgMove.EW_JJ);
			Factory.AddFetchHint(typeof(UNDGDataItem), UNDGDataItemSchema.DI_ParentID, CommonBookedCtgMove.PK);
			Factory.AddFetchHint(typeof(JobDocAddress), CommonBookedCtgMove.EW_E2PickupAddressID);
			Factory.AddFetchHint(typeof(JobDocAddress), CommonBookedCtgMove.EW_E2WaitPointAddressID);
			Factory.AddFetchHint(typeof(JobDocAddress), CommonBookedCtgMove.EW_E2DeliveryAddressID);
			Factory.AddFetchHint(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, CommonBookedCtgMove.EW_JJ);

			var cusContainerType = ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			Factory.AddFetchHint(cusContainerType, CusContainerSchema.CO_JC, CommonBookedCtgMove.EW_JC_Container);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			//Test in CartageLegClannerController
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(typeof(JobService), JobServiceSchema.ES_ParentID, CommonBookedCtgMove.PK);
			Factory.AddFetchHint(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, CommonBookedCtgMove.EW_JC_Container);
		}

		CommonBookedCtgMove CommonBookedCtgMove
		{
			get { return (CommonBookedCtgMove)BusinessObject; }
		}
	}
}
