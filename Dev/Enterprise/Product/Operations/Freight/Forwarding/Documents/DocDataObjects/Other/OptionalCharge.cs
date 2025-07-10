using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public class OptionalCharge : DocDataObject, IOptionalCharge
	{
		#region IsPrepaid

		public ZBool IsPrepaid
		{
			get => isPrepaid;
			set
			{
				if (SetNonPersistentPropertyValue(IsPrepaidInfo, ref isPrepaid, value) && isPrepaid)
				{
					IsCollect = false;
					IsPayableElsewhere = false;
				}

				ValidateAll();
			}
		}

		ZBool isPrepaid;

		public ZPropertyInfo IsPrepaidInfo => GetZPropertyInfo(nameof(IsPrepaid));

		#endregion

		#region IsCollect

		public ZBool IsCollect
		{
			get => isCollect;
			set
			{
				if (SetNonPersistentPropertyValue(IsCollectInfo, ref isCollect, value) && IsCollect)
				{
					IsPrepaid = false;
					IsPayableElsewhere = false;
				}

				ValidateAll();
			}
		}

		ZBool isCollect;

		public ZPropertyInfo IsCollectInfo => GetZPropertyInfo(nameof(IsCollect));

		#endregion

		#region IsPayableElsewhere

		public ZBool IsPayableElsewhere
		{
			get => isPayableElsewhere;
			set
			{
				if (SetNonPersistentPropertyValue(IsPayableElsewhereInfo, ref isPayableElsewhere, value) && IsPayableElsewhere)
				{
					IsPrepaid = false;
					IsCollect = false;
				}

				ValidateAll();
			}
		}

		ZBool isPayableElsewhere;

		public ZPropertyInfo IsPayableElsewhereInfo => GetZPropertyInfo(nameof(IsPayableElsewhere));

		#endregion
	}
}
