using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentHeaderGenPivotCollection : GenPivotCollection
	{
		public HVLVConsignmentHeaderGenPivotCollection(HVLVConsignmentHeader consignmentHeader)
			: base(consignmentHeader, GenPivotTypes.HighVolumeLowValue, true, false)
		{
		}

		public IEnumerable<BusinessObject> CustomsJobs
		{
			get
			{
				foreach (var pivot in this)
				{
					yield return pivot.Relation2Object;
				}
			}
		}
	}
}
