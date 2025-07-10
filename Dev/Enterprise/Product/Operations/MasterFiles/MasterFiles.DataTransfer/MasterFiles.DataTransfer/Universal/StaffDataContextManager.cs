using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class StaffDataContextManager : EventDataContextManager<GlbStaff>
	{
		public override DataContextType DataContextType => DataContextType.Staff;
		public override ZString DataContextKey => ParentBO.GS_Code;
		public override string DefaultOutputDirectory => null;
		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger) => new ZQuery(GlbStaffSchema.GS_Code, matchingValues.Key);
		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => new StaffEventParentFinder(factory, this, logger);

		class StaffEventParentFinder : EventParentFinder
		{
			internal StaffEventParentFinder(BusinessObjectFactory factory, StaffDataContextManager manager, IXmlImportLogger logger)
				: base(factory, manager, logger)
			{
			}

			protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent) => null;
		}
	}
}
