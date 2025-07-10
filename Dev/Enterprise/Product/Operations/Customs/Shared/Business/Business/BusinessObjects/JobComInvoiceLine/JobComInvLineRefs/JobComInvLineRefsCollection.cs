using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class JobComInvLineRefsCollection : DependentBusinessObjectCollection<JobComInvLineRefs, BaseJobComInvoiceLine>
	{
		public JobComInvLineRefsCollection(BaseJobComInvoiceLine parent)
			: base(parent)
		{
		}

		public JobComInvLineRefs AddNew(ZString type, ZString number)
		{
			var result = AddNew();
			result.JG_ReferenceType = type;
			result.JG_ReferenceNumber = number;
			return result;
		}

		public JobComInvLineRefs SetValueForJobComInvLineRefsAndDeleteDuplicateWhenEmpty(ZString referenceType, JobComInvLineRefs existingRefNo, ZString value, ZPropertyInfo info, Action validate = null)
		{
			if (existingRefNo == null || existingRefNo.IsDeleted)
			{
				existingRefNo = GetFirstJobComInvLineRefs(referenceType);
				if (existingRefNo == null || existingRefNo.IsDeleted)
				{
					existingRefNo = AddNew();
					existingRefNo.JG_ReferenceType = referenceType;
				}
			}
			else if (existingRefNo.JG_ReferenceType != referenceType)
			{
				existingRefNo.JG_ReferenceType = referenceType;
			}
			if (value.IsEmpty)
			{
				var currentRefNo = existingRefNo;
				this.Cast<JobComInvLineRefs>().Where(x => x != currentRefNo && x.JG_ReferenceType == referenceType).ToList().ForEach(x => x.Delete());
				if (!existingRefNo.JG_ReferenceNumber.IsEmpty)
				{
					existingRefNo.JG_ReferenceNumber = ZString.Empty;
				}
			}
			else
			{
				existingRefNo.JG_ReferenceNumber = value;
			}
			if (validate != null && !info.BizObj.IsValidationSuspended)
			{
				validate();
			}
			info.RefreshBinding();
			return existingRefNo;
		}

		public JobComInvLineRefs GetFirstJobComInvLineRefs(ZString referenceType)
		{
			return this.Cast<JobComInvLineRefs>().Where(x => x.JG_ReferenceType == referenceType).OrderBy(x => x.PK).FirstOrDefault();
		}

		protected override bool AllowNewCore
		{
			get
			{
				var master = Master;
				return master.SupportInvoiceLineRefs;
			}
		}
	}
}
