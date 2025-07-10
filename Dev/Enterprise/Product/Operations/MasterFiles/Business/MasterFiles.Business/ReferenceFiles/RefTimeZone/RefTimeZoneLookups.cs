using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefTimeZoneLookups : AutoRefTimeZoneLookups
	{
		public RefTimeZoneLookups(AutoRefTimeZone parent) : base(parent)
		{
		}
		public RefTimeZoneLookups(BusinessObjectFactory factory)
			: this((AutoRefTimeZone)null)
		{
			this.factory = factory;
		}

		public new AutoRefTimeZone Parent
		{
			get { return (AutoRefTimeZone)base.Parent; }
		}

		public IEnumerable<ZShort> OffsetFromUtcList => Factory.GetCachedValue("RefTimeZoneLookups.OffsetFromUtcList", GetOffsetFromUtcList);

		IEnumerable<ZShort> GetOffsetFromUtcList()
		{
			var sqlStatement = $"SELECT DISTINCT {AutoRefTimeZone.Schema.R2_OffsetMinutesFromUTC} FROM {RefTimeZoneSchema.Constants.SqlSchemaName}.{RefTimeZoneSchema.Constants.TableName} ORDER BY {AutoRefTimeZone.Schema.R2_OffsetMinutesFromUTC}";
			var collection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			collection.Load(sqlStatement);
			var offsetFromUtcs = collection.Select(x => (ZShort)x[AutoRefTimeZone.Schema.R2_OffsetMinutesFromUTC]);
			return offsetFromUtcs;
		}

		protected override BusinessObjectFactory Factory
		{
			get { return (Parent != null) ? base.Factory : factory; }
		}

		readonly BusinessObjectFactory factory;
	}
}
