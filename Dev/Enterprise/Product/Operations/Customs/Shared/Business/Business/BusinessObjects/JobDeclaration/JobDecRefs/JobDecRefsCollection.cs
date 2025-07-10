using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class JobDecRefsCollection : ActiveBusinessObjectCollection<JobDecRefs>
	{
		public JobDecRefsCollection(BaseJobDeclaration parent)
			: base(parent)
		{
		}

		public JobDecRefs AddNew(ZString type, ZString number)
		{
			var result = AddNew();
			result.J3_ReferenceType = type;
			result.J3_ReferenceNumber = number;
			return result;
		}

		public JobDecRefs SetValueForJobDecRefsAndDeleteDuplicateWhenEmpty(ZString referenceType, JobDecRefs existingRefNo, ZString value, ZPropertyInfo info, Action validate = null)
		{
			if (existingRefNo == null || existingRefNo.IsDeleted)
			{
				existingRefNo = GetFirstJobDecRefs(referenceType);
				if (existingRefNo == null || existingRefNo.IsDeleted)
				{
					existingRefNo = AddNew();
					existingRefNo.J3_ReferenceType = referenceType;
				}
			}
			else if (existingRefNo.J3_ReferenceType != referenceType)
			{
				existingRefNo.J3_ReferenceType = referenceType;
			}
			if (value.IsEmpty)
			{
				var currentRefNo = existingRefNo;
				this.Where(x => x != currentRefNo && x.J3_ReferenceType == referenceType).ToList().ForEach(x => x.Delete());
				if (!existingRefNo.J3_ReferenceNumber.IsEmpty)
				{
					existingRefNo.J3_ReferenceNumber = ZString.Empty;
				}
			}
			else
			{
				existingRefNo.J3_ReferenceNumber = value;
			}
			if (validate != null && !info.BizObj.IsValidationSuspended)
			{
				validate();
			}
			info.RefreshBinding();
			return existingRefNo;
		}

		public JobDecRefs GetFirstJobDecRefs(ZString referenceType)
		{
			return this.Where(x => x.J3_ReferenceType == referenceType).OrderBy(x => x.PK).FirstOrDefault();
		}
	}
}
