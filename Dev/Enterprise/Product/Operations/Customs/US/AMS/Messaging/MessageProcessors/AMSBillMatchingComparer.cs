using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	class AMSBillMatchingComparer : IComparer<IBaseBillOfLading>
	{
		#region IComparer<IBaseBillOfLading> Members

		public int Compare(IBaseBillOfLading x, IBaseBillOfLading y)
		{
			var result = 0;
			if (x == null && y != null)
			{
				result = -1;
			}
			else if (x != null && y == null)
			{
				result = 1;
			}
			else if (x != null && y != null)
			{
				var xAttachee = x.MessageAttachee;
				var yAttachee = y.MessageAttachee;
				if (xAttachee == null && yAttachee != null)
				{
					result = -1;
				}
				else if (xAttachee != null && yAttachee == null)
				{
					result = 1;
				}
				else if (xAttachee != null && yAttachee != null)
				{
					result = GetValue(xAttachee.ApplicationCode).CompareTo(GetValue(yAttachee.ApplicationCode));
					if (result == 0)
					{
						result = GetValue(xAttachee.SupApplicationCode).CompareTo(GetValue(yAttachee.SupApplicationCode));
					}
				}
			}
			return result;
		}

		int GetValue(ZString code)
		{
			var result = 10;
			switch (code)
			{
				case SubApplicationCodeList.Codes.AMS:
					result = 1;
					break;
				case SubApplicationCodeList.Codes.MasterInBond:
					result = 2;
					break;
				case SubApplicationCodeList.Codes.SubsequentInBond:
					result = 3;
					break;
				case SubApplicationCodeList.Codes.PermitToTransfer:
					result = 4;
					break;
			}
			return result;
		}

		#endregion
	}
}
