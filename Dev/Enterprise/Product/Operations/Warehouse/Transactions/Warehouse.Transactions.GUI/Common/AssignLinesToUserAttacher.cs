using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class AssignLinesToUserAttacher<T> : ZRecordAttacher
		where T : ILineStaffAssigner
	{
		public AssignLinesToUserAttacher(IEnumerable<T> lines, BusinessObjectFactory factory)
			: base(null, new GlbStaffCollection(factory, new ZQuery(GlbStaffSchema.GS_IsActive, true)), ModuleIDs.GlbStaff)
		{
			Argument.NotNull(lines, "Line collection."); // Message for the developer.
			Lines = lines;
			Factory = factory;
		}

#if DEBUG
		public
#endif
		readonly IEnumerable<T> Lines;
		readonly BusinessObjectFactory Factory;

		protected override bool AttachCore(BusinessObject selectedStaff, List<BusinessObject> listToBulkAdd)
		{
			var staff = Factory.Load<GlbStaff>(selectedStaff.PK);
			foreach (var line in Lines.Where(l => l.CanAssignOrUnAssignLine()))
			{
				line.AssignLine(staff);
			}

			return true;
		}
	}
}
