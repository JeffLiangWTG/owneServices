using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccComplianceSequence)]
	public class AccComplianceSequenceCollection : ActiveBusinessObjectCollection<AccComplianceSequence>
	{
		public AccComplianceSequenceCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public AccComplianceSequenceCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccComplianceSequenceCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
