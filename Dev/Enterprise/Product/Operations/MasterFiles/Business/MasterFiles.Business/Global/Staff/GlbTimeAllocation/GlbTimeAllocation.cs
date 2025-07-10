using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbTimeAllocation : GlbStaffResourceTime
	{
		public GlbTimeAllocation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Default Values

		protected override string LeaveOrTimeAllocation
		{
			get { return GlbStaffHolidayLookups.RecordTypes.TimeAllocation; }
		}

		#endregion

		#region Properties

		public ZString RelatedItemTypeDescription
		{
			get
			{
				if (!GA_ParentTableCode.IsEmpty)
				{
					ITableSchema schema = EnterpriseSchema.GetTableSchemaFromColumnNamePrefix(GA_ParentTableCode);
					return DataBoundResourceStrings.GetTableDescriptiveName(schema.TableName);
				}
				return ZString.Empty;
			}
		}

		public ZPropertyInfo RelatedItemTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(RelatedItemTypeDescription)); }
		}

		#endregion

		#region Lookups

		protected override GlbStaffHolidayLookups GetNewLookups()
		{
			return GlbTimeAllocationLookups.New(this);
		}

		#endregion
	}
}
