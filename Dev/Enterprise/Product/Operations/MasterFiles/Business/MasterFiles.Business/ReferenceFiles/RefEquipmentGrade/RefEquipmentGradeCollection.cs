using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class RefEquipmentGradeCollection : ActiveBusinessObjectCollection<RefEquipmentGrade>, IRefEquipmentGradeCollection
	{
		public RefEquipmentGradeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefEquipmentGradeCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public RefEquipmentGradeCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
