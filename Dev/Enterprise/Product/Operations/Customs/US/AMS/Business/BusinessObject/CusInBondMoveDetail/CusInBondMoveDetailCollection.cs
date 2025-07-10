using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondMoveDetailCollection : US.Business.CusInBondMoveDetailCollection<CusInBondMoveDetail>, IBusinessObjectState
	{
		public CusInBondMoveDetailCollection(CusInBondMoveHeader master)
			: base(master)
		{
		}

		public CusInBondMoveDetailCollection(CusInBondBill master)
			: base(master)
		{
		}

		public ZString GetStatus(ZString propertyName)
		{
			ZString? result = null;
			foreach (var moveDetail in this)
			{
				if (!result.HasValue)
				{
					result = moveDetail[propertyName].ToString();
				}
				else if (result.Value != moveDetail[propertyName].ToString())
				{
					result = AMSBillMessageStatusList.Codes.Multiple;
					break;
				}
			}
			return result ?? ZString.Empty;
		}

		bool IBusinessObjectState.HasChanges
		{
			get { return false; }
			set { throw new System.NotImplementedException(); }
		}
	}
}
