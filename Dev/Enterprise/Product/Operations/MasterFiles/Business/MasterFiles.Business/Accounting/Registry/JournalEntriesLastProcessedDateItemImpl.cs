using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JournalEntriesLastProcessedDateItemImpl : DateTimeRegistryItem
	{
		public JournalEntriesLastProcessedDateItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options) : base(name, category, caption, hint, storage, options)
		{
		}

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);

			var factory = new BusinessObjectFactory();
			var query = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, companyOrOwnerPK) { OrderBy = AccPeriodManagementSchema.Constants.AM_Period + OrderByClause.Ascending };
			var firstPeriod = factory.LoadTop1<AccPeriodManagement>(query);
			if (firstPeriod != null && (DateTime)newValue == firstPeriod.AM_StartDate)
			{
				var collectorProvider = ObjectFactory.Get<IAccUsageCollectorProvider>();
				collectorProvider.ReportGeneralLedgerProcess(companyOrOwnerPK);
			}
		}
	}
}
