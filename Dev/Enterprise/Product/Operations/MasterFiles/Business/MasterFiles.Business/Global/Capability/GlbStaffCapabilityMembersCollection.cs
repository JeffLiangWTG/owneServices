using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffCapabilityMembersCollection : ManyToManyBusinessObjectCollection<GlbStaff, GlbCapability>
	{
		public GlbStaffCapabilityMembersCollection(GlbCapability parent)
			: base(parent)
		{
		}

		protected override Type TypeOfRelationshipBusinessObject
		{
			get { return typeof(GlbResourceCapabilityPivot); }
		}

		protected override SchemaGuidColumn PivotTableFKToAssociatedBusinessObject
		{
			get { return GlbResourceCapabilityPivotSchema.G5_G4_Capability; }
		}

		protected override SchemaGuidColumn PivotTableFKToCollectionBusinessObjects
		{
			get { return GlbResourceCapabilityPivotSchema.G5_GS_Resource; }
		}

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);
			var resource = (GlbStaff)businessObject;

			resource.CapabilityPivot = (GlbResourceCapabilityPivot)GetRelationshipBusinessObject(resource);
			resource.MarkAsNeedingValidation();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return true; }
		}
	}
}
