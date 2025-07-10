using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

#if DEBUG
using Enterprise.MasterFiles.Business.Testing;
#endif

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(NettingSystemPeriod.Schema.NSP_Period), DescriptionProperty(NettingSystemPeriod.Schema.NSP_Description)]
	public class NettingSystemPeriod : AutoNettingSystemPeriod
	{
		public NettingSystemPeriod(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			NSP_EarliestInvoiceDateUtc = ZDateTime.UtcToday;
			NSP_LatestInvoiceDateUtc = ZDateTime.UtcToday.AddDays(30);
			NSP_Period = MasterFilesTestHelper.GetRandomString(10);
			NSP_ValueDate = ZDate.Today.AddDays(30);
			NSP_OfferPrepaymentDate = ZDate.Today.AddDays(29);
		}
#endif
	}
}
