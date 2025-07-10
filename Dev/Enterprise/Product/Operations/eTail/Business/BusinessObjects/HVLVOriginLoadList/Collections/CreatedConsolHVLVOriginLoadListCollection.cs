using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business
{
	public class CreatedConsolHVLVOriginLoadListCollection : HVLVOriginLoadListCollection, IFilterModuleExtraNotificationProvider
	{
		public CreatedConsolHVLVOriginLoadListCollection(BusinessObjectFactory factory)
			: base(factory, new ZQuery(HVLVOriginLoadListSchema.HVL_Status, Core.Constants.ELoadListStatuses.Lodged))
		{
		}

		protected override void SetDefaultsForNewElementCore(HVLVOriginLoadList loadList)
		{
			base.SetDefaultsForNewElementCore(loadList);
			loadList.HVL_Status = Core.Constants.ELoadListStatuses.Lodged;
		}

		#region IFilterModuleExtraNotificationProvider Members

		INotification IFilterModuleExtraNotificationProvider.GetExtraNotification(BusinessObject businessObject)
		{
			Notification result = null;

			if (businessObject is HVLVOriginLoadList loadList)
			{
				if (loadList.HVL_MasterBillNumber.IsEmpty)
				{
					result = new Notification(CargoWise.ComponentModel.NotificationType.Error, Res.GetString("d7862e5e-32c2-4c61-b311-f21284155a91", "A load list should have a master bill number."));
				}

				var filter = new ZQuery(JobConsolSchema.JK_MasterBillNum, loadList.HVL_MasterBillNumber);
				filter.AddToFilter(JoinCondition.And, JobConsolSchema.JK_IsCancelled, ZBool.False);

				var consol = Factory.LoadTop1<ForwardingConsol>(filter);
				if (consol != null)
				{
					var errorMessage = ZString.Format(
						   Res.GetString("57abdbdb-e77d-4dd7-bca2-175848465f9f", "The Master Bill Number {0} already exists on Consol {1}, please attach Load List within the Consol via Actions > Attach HVLV Origin Load List"),
						   loadList.HVL_MasterBillNumber, consol.JK_UniqueConsignRef);
					result = new Notification(CargoWise.ComponentModel.NotificationType.Error, errorMessage);
				}
			}

			return result;
		}

		#endregion
	}
}
