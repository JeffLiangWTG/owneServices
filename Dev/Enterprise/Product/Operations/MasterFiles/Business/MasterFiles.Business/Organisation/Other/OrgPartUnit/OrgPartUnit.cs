using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DeferTriggerAndRunBeforeCommit("TG_OrgPartUnit_UpdateOrgPartRelationUnitsPerClientUQ", "UpdateOrgPartRelationUnitsPerClientUQByOrgSupplierPart", OrgPartUnitSchema.Constants.OF_OP, typeof(IDeferTriggerOnDeleteConditionStrategy), ValueToRunStoredProcWith = ValueVersion.Original)]
	[DeferTriggerAndRunBeforeCommit("TG_OrgPartUnit_UpdateOrgPartRelationUnitsPerClientUQ", "UpdateOrgPartRelationUnitsPerClientUQByOrgSupplierPart", OrgPartUnitSchema.Constants.OF_OP, typeof(IUpdateOrgPartRelationStrategy_OrgPartUnitInsertUpdate), ValueToRunStoredProcWith = ValueVersion.Both)]
	public class OrgPartUnit : AutoOrgPartUnit, IUnitConverter, IPartProvider
	{
		public OrgPartUnit(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region CanDelete

		public override bool CanDelete
		{
			get
			{
				return
					base.CanDelete &&
					(
						!IsInDatabase // if not saved to DB - always can delete
						|| (!IsPackConversionWithPendingUOMPicks() // if not a pack type participating in UOM picking - it is ok to delete...
							&& !OrgPartUnitValidation.HasPicksOnSalesOrder(SupplierPart)) // ... if product haven't been picked as component for Pick On Sales Order
					);
			}
		}

		bool IsPackConversionWithPendingUOMPicks()
		{
			return !OrgPartUnitValidation.IsOriginallyWeightOrVolumeConversion(this)
				   && OrgPartUnitValidation.HasPendingPicksByUOM(this);
		}

		// run unit test to ensure this works
		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var result = base.ReasonForNotAbleToDelete;
				return result.IsEmpty
					? IsPackConversionWithPendingUOMPicks() ? OrgPartUnitValidation.PendingUOMPicksDetected : OrgPartUnitValidation.PickOnSalesOrderDetected
					: result;
			}
		}

		#endregion

		#region Properties

		#region OF_PackType

		[List("OF_PackType_List")]
		public override ZString OF_PackType
		{
			get { return base.OF_PackType; }
			set
			{
				base.OF_PackType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateOF_ParentPackType();
				}

				MarkSupplierPartAsNeedingValidation();
			}
		}

		#endregion

		#region OF_ParentPackType

		[List("OF_PackType_List")]
		public override ZString OF_ParentPackType
		{
			get { return base.OF_ParentPackType; }
			set
			{
				base.OF_ParentPackType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateOF_PackType();
				}

				MarkSupplierPartAsNeedingValidation();
			}
		}

		#endregion

		#region OF_OP

		public override ZGuid OF_OP
		{
			get { return base.OF_OP; }
			set
			{
				MarkSupplierPartAsNeedingValidation();
				base.OF_OP = value;
			}
		}

		#endregion

		#endregion

		#region Load / Saved / Delete

		public override void Delete()
		{
			MarkSupplierPartAsNeedingValidation();
			base.Delete();
		}

		#endregion

		#region MarkSupplierPartAsNeedingValidation

		void MarkSupplierPartAsNeedingValidation()
		{
			var supplierPart = SupplierPart;
			if (supplierPart != null)
			{
				supplierPart.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region Lookups

		public virtual CodeDescriptionPairList OF_PackType_List
		{
			get { return new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits(); }
		}

		#endregion

		#region IUnitConverter Members

		public ZString ParentUnit
		{
			get { return OF_ParentPackType; }
		}

		public ZString ChildUnit
		{
			get { return OF_PackType; }
		}

		public ZDecimal ConversionFactor
		{
			get { return OF_QuantityInParent; }
		}

		#endregion

		#region IPartProvider Members

		OrgSupplierPart IPartProvider.Part
		{
			get { return SupplierPart; }
		}

		#endregion
	}
}
