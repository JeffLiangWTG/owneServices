using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business
{
	public class AttachedHVLVOriginLoadListCollection : HVLVOriginLoadListCollection, IFilterModuleExtraNotificationProvider
	{
		public AttachedHVLVOriginLoadListCollection(BusinessObjectFactory factory, ZString currentConsolMasterBillNumber)
			: base(factory, new ZQuery(HVLVOriginLoadListSchema.HVL_Status, Core.Constants.ELoadListStatuses.Lodged))
		{
			currentConsolBillNumber = currentConsolMasterBillNumber;
		}

		protected override void SetDefaultsForNewElementCore(HVLVOriginLoadList loadList)
		{
			base.SetDefaultsForNewElementCore(loadList);
			loadList.HVL_Status = Core.Constants.ELoadListStatuses.Lodged;
		}

		readonly ZString currentConsolBillNumber;

		#region IFilterModuleExtraNotificationProvider Members

		INotification IFilterModuleExtraNotificationProvider.GetExtraNotification(BusinessObject businessObject)
		{
			Notification result = null;

			if (businessObject is HVLVOriginLoadList loadList)
			{
				if (!loadList.HVL_MasterBillNumber.IsEmpty && loadList.HVL_MasterBillNumber != currentConsolBillNumber)
				{
					var filter = new ZQuery(JobConsolSchema.JK_MasterBillNum, loadList.HVL_MasterBillNumber);
					filter.AddToFilter(JoinCondition.And, JobConsolSchema.JK_IsCancelled, ZBool.False);
					if (Factory.Exists(typeof(ForwardingConsol), filter))
					{
						var warningMessage = ZString.Format(
							   Res.GetString("b2b51d9c-865e-407b-aefb-9e2ca30c8cb1", "Origin Load List {0}: This Master Bill Number {1} already exists on another Consol",
							   loadList.HumanReadableName,
							   loadList.HVL_MasterBillNumber),
							   loadList.HVL_MasterBillNumber);
						result = new Notification(CargoWise.ComponentModel.NotificationType.Warning, warningMessage);
					}
					else
					{
						var warningMessage = ZString.Format(
						Res.GetString("71bd9cf6-2f62-49fc-8e96-2e7f9bea6d82", "Origin Load List {0}: This Master Bill Number {1} does not match the current Consol Master Bill Number {2}",
						loadList.HumanReadableName,
						loadList.HVL_MasterBillNumber,
						currentConsolBillNumber),
						loadList.HVL_MasterBillNumber);
						result = new Notification(CargoWise.ComponentModel.NotificationType.Warning, warningMessage);
					}
				}
			}

			return result;
		}

		#endregion
	}
}
