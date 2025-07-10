using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class InvoiceHeaderRefsCollection : ActiveBusinessObjectCollection<JobComInvoiceHeaderRefs>
	{
		public InvoiceHeaderRefsCollection(BaseJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		public JobComInvoiceHeaderRefs AddNew(ZString type, ZString number)
		{
			var result = AddNew();
			result.J2_ReferenceType = type;
			number = number.SubstringSafe(0, result.J2_ReferenceNumberInfo.MaxLength);
			result.J2_ReferenceNumber = number;
			return result;
		}

		public JobComInvoiceHeaderRefs SetValueForJobComInvoiceHeaderRefsAndDeleteDuplicateWhenEmpty(ZString referenceType, JobComInvoiceHeaderRefs existingRefNo, ZString value, ZPropertyInfo info, Action validate = null)
		{
			if (existingRefNo == null || existingRefNo.IsDeleted)
			{
				existingRefNo = GetFirstJobComInvoiceHeaderRefs(referenceType);
				if (existingRefNo == null || existingRefNo.IsDeleted)
				{
					existingRefNo = AddNew();
					existingRefNo.J2_ReferenceType = referenceType;
				}
			}
			else if (existingRefNo.J2_ReferenceType != referenceType)
			{
				existingRefNo.J2_ReferenceType = referenceType;
			}
			if (value.IsEmpty)
			{
				var currentRefNo = existingRefNo;
				this.Where(x => x != currentRefNo && x.J2_ReferenceType == referenceType).ToList().ForEach(x => x.Delete());
				if (!existingRefNo.J2_ReferenceNumber.IsEmpty)
				{
					existingRefNo.J2_ReferenceNumber = ZString.Empty;
				}
			}
			else
			{
				existingRefNo.J2_ReferenceNumber = value;
			}
			if (validate != null && !info.BizObj.IsValidationSuspended)
			{
				validate();
			}
			info.RefreshBinding();
			return existingRefNo;
		}

		public JobComInvoiceHeaderRefs GetFirstJobComInvoiceHeaderRefs(ZString referenceType)
		{
			return this.Where(x => x.J2_ReferenceType == referenceType).OrderBy(x => x.PK).FirstOrDefault();
		}
	}
}
