using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public class CommercialInvoiceFilterBusinessObject : Customs.Module.CommercialInvoiceFilterBusinessObject
	{
		protected override Customs.Module.CommercialInvoiceFilterLookups GetNewLookups() => new CommercialInvoiceFilterLookups(this);

		protected override ZQuery GetAdditionalDeclarationFilter()
		{
			var filter = base.GetAdditionalDeclarationFilter();
			filter.AddToFilter(JobDeclarationSchema.JE_MessageType, SQLComparisonOperator.NotEqual, JobMessageTypeList.Codes.Drawback);
			return filter;
		}
	}
}
