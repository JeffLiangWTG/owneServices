using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportConfirmationCollection<T> : ActiveBusinessObjectCollection<T>, IDtbTransportConfirmationCollection
		where T : DtbTransportConfirmation
	{
		protected DtbTransportConfirmationCollection(DtbTransportInstruction instruction)
			: base(instruction.Factory, instruction, null, DtbBookingConfirmationSchema.KK_KN_BookingInstruction)
		{
		}

		protected DtbTransportConfirmationCollection(Package_PackageView package_PackageView)
			: base(package_PackageView.Factory, new PackageConfirmationRelationship<T>(package_PackageView))
		{
		}

		// Divot Confirmations Only ... will not contain Divot.Instruction.Confirmations.Where(c => c.PackageDivot == null)
		protected DtbTransportConfirmationCollection(DtbTransportInstructionPkgDivot instructionPkgDivot)
			: base(instructionPkgDivot.Factory, new DivotConfirmationRelationship(instructionPkgDivot))
		{
		}

		protected DtbTransportConfirmationCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected DtbTransportConfirmationCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		protected DtbTransportConfirmationCollection(BusinessObjectFactory factory, BusinessObject master, ZQuery filter, SchemaGuidColumn relationshipColumn)
			: base(factory, master, filter, relationshipColumn)
		{
		}

		#region AddNew

		public T AddNew(ZString confirmationType)
		{
			var confirmation = AddNew();
			confirmation.KK_ConfirmationType = confirmationType;
			return confirmation;
		}

		#endregion

		#region HasDelivery

		public bool HasDelivery
		{
			get { return this.Any(c => c.IsDelivery); }
		}

		#endregion

		#region HasPickup

		public bool HasPickup
		{
			get { return this.Any(c => c.IsPickUp); }
		}

		#endregion

		#region DivotConfirmationRelationship

		class DivotConfirmationRelationship : DependentRelationship
		{
			public DivotConfirmationRelationship(DtbTransportInstructionPkgDivot instructionPkgDivot)
				: base(instructionPkgDivot, typeof(T), new ZQuery(), DtbBookingConfirmationSchema.KK_KD_BookingInstructionPkgDivot)
			{ }

			protected override void AddToRelationship(BusinessObject businessObject)
			{
				base.AddToRelationship(businessObject);

				var divot = (DtbTransportInstructionPkgDivot)Master;
				if (divot != null)
				{
					var confirmation = (DtbTransportConfirmation)businessObject;
					confirmation.KK_KN_BookingInstruction = divot.KD_KN_BookingInstruction;
				}
			}
		}

		#endregion

		#region IDtbTransportConfirmationCollection Members

		DtbTransportConfirmation IDtbTransportConfirmationCollection.this[int index] => this[index];

		#endregion
	}
}
