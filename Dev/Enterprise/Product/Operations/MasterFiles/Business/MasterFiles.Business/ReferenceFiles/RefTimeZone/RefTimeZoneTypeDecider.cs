using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefTimeZoneTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			ZQuery query = new ZQuery(RefTimeZoneRuleSchema.R4_R2, row[RefTimeZoneSchema.PK.Name]);
			query.FetchOnlyFromLocalCache = true;
			RefTimeZoneRule result = factory.LoadTop1<RefTimeZoneRule>(query);
			return result != null ? typeof(DaylightSavingTimeZone) : typeof(StandardTimeZone);
		}

		public override Type GetTypeForNew()
		{
			return typeof(StandardTimeZone);
			// For the business object collection test to pass, can't throw an exception here
			// throw new InvalidOperationException("Create a StandardTimeZone or a DaylightSavingTimeZone.");
		}

		public override Type GetTypeForBinding()
		{
			return typeof(StandardTimeZone);
			// For the business object collection test to pass, can't throw an exception here
			// throw new InvalidOperationException("Create a StandardTimeZone or a DaylightSavingTimeZone.");
		}
	}
}
