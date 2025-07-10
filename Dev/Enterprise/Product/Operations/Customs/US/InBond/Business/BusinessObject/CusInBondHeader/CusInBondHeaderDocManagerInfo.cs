using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	class CusInBondHeaderDocManagerInfo : DocManagerInfo
	{
		public CusInBondHeaderDocManagerInfo(BusinessObject parent)
			: base(parent, Core.Constants.DocManagerCodes.InBond)
		{
		}

		CusInBondHeader Header
		{
			get { return (CusInBondHeader)BusinessEntity; }
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			result.AddRange(new InvoiceLoader(BusinessEntity.Factory).GetInvoicesForUniqueRef(Header.BH_JobReference));

			return result.ToArray();
		}
	}
}
