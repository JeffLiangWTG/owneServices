using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCapabilityManyToManyCollection : ManyToManyBusinessObjectCollection<GlbCapability, GlbStaff>
	{
		public GlbCapabilityManyToManyCollection(GlbStaff parent)
			: base(parent)
		{
		}

		protected override Type TypeOfRelationshipBusinessObject
		{
			get { return typeof(GlbResourceCapabilityPivot); }
		}

		protected override SchemaGuidColumn PivotTableFKToAssociatedBusinessObject
		{
			get { return GlbResourceCapabilityPivotSchema.G5_GS_Resource; }
		}

		protected override SchemaGuidColumn PivotTableFKToCollectionBusinessObjects
		{
			get { return GlbResourceCapabilityPivotSchema.G5_G4_Capability; }
		}

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);
			var capability = (GlbCapability)businessObject;

			capability.ResourcePivot = (GlbResourceCapabilityPivot)GetRelationshipBusinessObject(capability);
			capability.MarkAsNeedingValidation();
		}

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		protected override bool AllowRemoveCore
		{
			get { return true; }
		}
	}
}
