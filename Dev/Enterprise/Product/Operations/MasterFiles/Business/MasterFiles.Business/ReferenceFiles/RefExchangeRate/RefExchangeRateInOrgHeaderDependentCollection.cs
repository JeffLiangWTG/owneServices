using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefExchangeRateInOrgHeaderDependentCollection : ActiveBusinessObjectCollection<RefExchangeRate>
	{
		public RefExchangeRateInOrgHeaderDependentCollection(OrgHeader parent)
			: base(parent.Factory, parent, null, RefExchangeRateSchema.RE_OH_Client)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(RefExchangeRateSchema.RE_GC, GlbCompany.CurrentCompany.PK);
			return query;
		}

		protected override void SetDefaultsForNewElementCore(RefExchangeRate newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.RE_StartDate = ZDateTime.Now;
			newElement.RE_ExpiryDate = ZDateTime.Now;
			newElement.RE_SellRate = 1m;
			newElement.RE_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;
		}
	}
}
