using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.LVS.Business
{
	class CusUSLVClearanceDocManagerInfo : DocManagerInfo
	{
		public CusUSLVClearanceDocManagerInfo(BusinessObject parent, string docManagerCode)
			: base(parent, docManagerCode)
		{
		}

		CusUSLVClearance Clearance => (CusUSLVClearance)BusinessEntity;

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = base.GetRelatedObjects().ToList();
			result.AddRange(Clearance.CusUSLVConsignments);
			return result.ToArray();
		}
	}
}
