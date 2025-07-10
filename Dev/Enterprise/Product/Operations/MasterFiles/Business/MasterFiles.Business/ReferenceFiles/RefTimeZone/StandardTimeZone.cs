using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnly", MetaDataTypes.ReadOnly)]
	public class StandardTimeZone : RefTimeZone
	{
		public StandardTimeZone(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		protected bool GetReadOnly(PropertyDescriptor property)
		{
			return RefTimeZoneSet.R3_IsSystem || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		public RefTimeZoneSet RefTimeZoneSet
		{
			get
			{
				if (refTimeZoneSet == null)
				{
					var query = new ZQuery(RefTimeZoneSetSchema.R3_R2_StandardZone, SQLComparisonOperator.Equal, PK);
					refTimeZoneSet = Factory.LoadTop1<RefTimeZoneSet>(query);
				}

				return refTimeZoneSet;
			}
		}
		RefTimeZoneSet refTimeZoneSet;
	}
}
