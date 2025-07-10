using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business
{
	public sealed class DtbBookingConfirmationCollection : ActiveBusinessObjectCollection<DtbBookingConfirmation>
	{
		public DtbBookingConfirmationCollection(DtbBookingInstruction instruction)
			: base(instruction.Factory, instruction, null, DtbBookingConfirmationSchema.KK_KN_BookingInstruction)
		{
		}

		// Divot Confirmations Only ... will not contain Divot.Instruction.Confirmations.Where(c => c.PackageDivot == null)
		public DtbBookingConfirmationCollection(DtbBookingInstructionPkgDivot instructionPkgDivot)
			: base(instructionPkgDivot.Factory, new DivotConfirmationRelationship(instructionPkgDivot))
		{
		}

		public DtbBookingConfirmationCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public DtbBookingConfirmationCollection(DtbBookingPackage_PackageView package_PackageView)
			: base(package_PackageView.Factory, new PackageConfirmationRelationship(package_PackageView))
		{
		}

		public bool IsDeliveryComplete
		{
			get { return Deliveries.IsConfirmationsComplete(); }
		}

		public IEnumerable<DtbBookingConfirmation> Deliveries
		{
			get { return this.Where(c => c.IsDelivery); }
		}

		public bool IsPickUpComplete
		{
			get { return PickUps.IsConfirmationsComplete(); }
		}

		public IEnumerable<DtbBookingConfirmation> PickUps
		{
			get { return this.Where(c => c.IsPickUp); }
		}

		public DtbBookingConfirmation AddNew(ZString confirmationType)
		{
			var confirmation = AddNew();
			confirmation.KK_ConfirmationType = confirmationType;
			return confirmation;
		}

		public bool HasDelivery
		{
			get { return this.Any(c => c.IsDelivery); }
		}

		public bool HasPickup
		{
			get { return this.Any(c => c.IsPickUp); }
		}

		class DivotConfirmationRelationship : DependentRelationship
		{
			public DivotConfirmationRelationship(DtbBookingInstructionPkgDivot instructionPkgDivot)
				: base(instructionPkgDivot, typeof(DtbBookingConfirmation), new ZQuery(), DtbBookingConfirmationSchema.KK_KD_BookingInstructionPkgDivot)
			{ }

			protected override void AddToRelationship(BusinessObject businessObject)
			{
				base.AddToRelationship(businessObject);

				var divot = (DtbBookingInstructionPkgDivot)Master;
				if (divot != null)
				{
					var confirmation = (DtbBookingConfirmation)businessObject;
					confirmation.KK_KN_BookingInstruction = divot.KD_KN_BookingInstruction;
				}
			}
		}
	}

	public static class IEnumerable_DtbBookingConfirmation_Extensions
	{
		public static ZBool IsConfirmationsComplete(this IEnumerable<DtbBookingConfirmation> confirmations)
		{
			var result = confirmations.Any();
			if (result)
			{
				var confirmedQuantityByPackageDivot = new Dictionary<ZGuid, ConfirmedQuantityForPackageDivot>();

				// Sum the quantity for each Package Divot
				foreach (var confirm in confirmations)
				{
					if (!confirm.KK_Actual.IsValid)
					{
						result = false;
						break;
					}

					if (confirm.PackageDivot != null) // only the Package Divot Confirmation are checked because otherwise the quantity is readonly
					{
						var packageDivotPK = confirm.PackageDivot.PK;
						ConfirmedQuantityForPackageDivot confirmedQuantityForPackageDivot = null;
						if (!confirmedQuantityByPackageDivot.TryGetValue(packageDivotPK, out confirmedQuantityForPackageDivot))
						{
							confirmedQuantityForPackageDivot = new ConfirmedQuantityForPackageDivot(confirm.PackageDivot);
							confirmedQuantityByPackageDivot.Add(packageDivotPK, confirmedQuantityForPackageDivot);
						}

						confirmedQuantityForPackageDivot.Quantity += confirm.KK_Quantity;
					}
				}

				// Check each quanity against the package divot
				if (result)
				{
					result = confirmedQuantityByPackageDivot.Values.All(p => p.IsValid());
				}
			}

			return result;
		}

		class ConfirmedQuantityForPackageDivot
		{
			public ConfirmedQuantityForPackageDivot(DtbBookingInstructionPkgDivot packageDivot)
			{
				PackageDivot = packageDivot;
			}
			readonly DtbBookingInstructionPkgDivot PackageDivot;

			public ZInt Quantity { get; set; }

			public ZBool IsValid()
			{
				return Quantity >= PackageDivot.KD_Quantity;
			}
		}
	}
}
