using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobComInvoiceHeaderScheduleRelatedJobTypeTest : ScheduleRelatedJobTypeTest
	{
		protected override ControllerID ExpectedControllerID => ControllerIDs.CommercialInvoice;

		protected override string ExpectedBizOType => "Enterprise.Customs.Business.BaseJobComInvoiceHeader";

		protected override ScheduleRelatedJobType GetScheduleRelatedJobType(string code, MultilingualString description) => new JobComInvoiceHeaderScheduleRelatedJobType(code, description);
	}
}
