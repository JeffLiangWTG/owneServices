using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	class CusISFHeaderDocManagerInfo : DocManagerInfo
	{
		public CusISFHeaderDocManagerInfo(BusinessObject parent)
			: base(parent, Core.Constants.DocManagerCodes.ImporterSecurityFiling)
		{
		}

		CusISFHeader Header
		{
			get { return (CusISFHeader)BusinessEntity; }
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			if (Header.Importer != null)
			{
				result.Add(Header.Importer);
			}
			result.AddRange(new InvoiceLoader(BusinessEntity.Factory).GetInvoicesForUniqueRef(Header.BF_JobReference));

			return result.ToArray();
		}
	}
}
