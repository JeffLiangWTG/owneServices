using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business
{
	public sealed class DtbBookingInstructionPkgDivotCollection : ActiveBusinessObjectCollection<DtbBookingInstructionPkgDivot>
	{
		public DtbBookingInstructionPkgDivotCollection(DtbBookingInstruction instruction)
			: base(instruction.Factory)
		{
			Instruction = instruction;
			SetRelationshipFromInstruction();
		}

		public DtbBookingInstructionPkgDivotCollection(DtbBooking booking)
			: base(booking.Factory, new DtbBookingPackageDivotRelationship(booking))
		{
			Booking = booking;
			CollectionCountChange += CollectionCountChanged;
		}

		public DtbBookingInstructionPkgDivotCollection(DtbBookingPackage_PackageView package_PackageView)
			: base(package_PackageView.Factory, package_PackageView.Package, null, DtbBookingInstructionPkgDivotSchema.KD_KP_Package)
		{
			Package_PackageView = package_PackageView;
		}

		void SetRelationshipFromInstruction()
		{
			if (Instruction.IsSub)
			{
				var additionalFilter = new ZDBOnlyQuery(typeof(DtbBookingInstructionPkgDivot));
				var additionalFilterSQL = @"
KD_KP_Package IN
(
	SELECT KP_PK FROM dbo.PkgPackage
	WHERE KP_KJ_ParentPackageJob = @SubBookingPackageJobPK
)";
				var parameterCollection = new ZSqlParameterCollection();
				parameterCollection.Add("@SubBookingPackageJobPK", Instruction.Booking.PackageJob.PK, PkgPackageSchema.KP_KJ_ParentPackageJob);
				additionalFilter.AddFilterAndZSQLParameterCollection(additionalFilterSQL, parameterCollection);

				Relationship = new DependentRelationship(Instruction.MasterBookingInstruction, typeof(DtbBookingInstructionPkgDivot), additionalFilter, DtbBookingInstructionPkgDivotSchema.KD_KN_BookingInstruction);
			}
			else
			{
				Relationship = new InstructionDivotRelationship(Instruction);
			}
		}

		DtbBookingPackage_PackageView Package_PackageView { get; }
		DtbBookingInstruction Instruction { get; }
		DtbBooking Booking { get; }

		bool IsSubBookingOrInstruction
		{
			get
			{
				return
					(Instruction != null && !Instruction.IsDeleted && Instruction.IsSub) ||
					(Booking != null && !Booking.IsDeleted && Booking.IsSub) ||
					(IsBookingFilterRequired && Package_PackageView.Booking.IsSub);
			}
		}

		protected override void OnLoadedIntoCollectionCore(DtbBookingInstructionPkgDivot loadedObject)
		{
			UnhookEvents(loadedObject);
			HookEvents(loadedObject);
		}

		void HookEvents(DtbBookingInstructionPkgDivot loadedObject)
		{
			var package = loadedObject.Package;
			if (package != null && !package.IsDeleted && !package.IsDeleting)
			{
				package.KP_WeightInfo.ValueChanged += Package_WeightChanged;
				package.KP_WeightUQInfo.ValueChanged += Package_WeightChanged;
				package.KP_PackageQtyInfo.ValueChanged += Package_WeightChanged;
			}
		}

		void UnhookEvents(DtbBookingInstructionPkgDivot loadedObject)
		{
			var package = loadedObject.Package;
			if (package != null && !package.IsDeleted && !package.IsDeleting)
			{
				package.KP_WeightInfo.ValueChanged -= Package_WeightChanged;
				package.KP_WeightUQInfo.ValueChanged -= Package_WeightChanged;
				package.KP_PackageQtyInfo.ValueChanged -= Package_WeightChanged;
			}
		}

		void Package_WeightChanged(object sender, EventArgs e)
		{
			Booking?.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(e as ValueChangedEventArgs));
		}

		public override void Delete(DtbBookingInstructionPkgDivot businessObject)
		{
			if (IsSubBookingOrInstruction)
			{
				throw new InvalidOperationException("Removing of divots not allowed for a sub booking or instruction");
			}

			UnhookEvents(businessObject);
			base.Delete(businessObject);
		}

		void CollectionCountChanged(object sender, CollectionCountChangedEventArgs args)
		{
			Booking?.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(args, (NoResString)"Instruction package divot"));

			if (!(args.BizObject is DtbBookingInstructionPkgDivot packageDivot) || packageDivot.IsDeleting || packageDivot.IsDeleted)
			{
				return;
			}

			if (args.ItemAdded)
			{
				HookEvents(packageDivot);
			}
			else if (args.ItemRemoved)
			{
				UnhookEvents(packageDivot);
			}
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		public bool IsDeliveryComplete
		{
			get { return Count > 0 && this.All(d => d.Confirmations.IsDeliveryComplete); }
		}

		public bool IsPickUpComplete
		{
			get { return Count > 0 && this.All(d => d.Confirmations.IsPickUpComplete); }
		}

		// Used in the Cached Key for the Index
		protected override object[] GetCollectionState()
		{
			object[] result;

			if (IsBookingFilterRequired)
			{
				result = new object[] { BookingToFilterBy };
			}
			else
			{
				result = base.GetCollectionState();
			}

			return result;
		}

		protected override void SetRelationshipDefaultsForElementCore(DtbBookingInstructionPkgDivot newElement, bool throwIfRelationshipNotSupported)
		{
			if (IsSubBookingOrInstruction)
			{
				throw new InvalidOperationException("Adding of new divots not allowed for a sub booking or instruction");
			}
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
		}

		protected override bool MatchesFilterCore(DtbBookingInstructionPkgDivot element, bool fetchOnlyFromLocalCache)
		{
			bool matchesCollectionFilter = !IsBookingFilterRequired || element.Instruction == null || BookingToFilterBy.PK == element.Instruction.KN_KM_BookingMovement;
			return base.MatchesFilterCore(element, fetchOnlyFromLocalCache) && matchesCollectionFilter;
		}

		bool IsBookingFilterRequired
		{
			get { return Package_PackageView?.Booking != null && !Package_PackageView.Booking.IsDeleted; }
		}

		DtbBooking BookingToFilterBy
		{
			get { return IsBookingFilterRequired ?
					(Package_PackageView.Booking.IsSub ? Package_PackageView.Booking.MasterBooking : Package_PackageView.Booking) :
					null; }
		}

		public ZInt TotalQuantity()
		{
			return this.Sum(c => c.KD_Quantity);
		}

		class InstructionDivotRelationship : DependentRelationship
		{
			public InstructionDivotRelationship(DtbBookingInstruction instruction)
				: base(instruction, typeof(DtbBookingInstructionPkgDivot), null, DtbBookingInstructionPkgDivotSchema.KD_KN_BookingInstruction)
			{
			}

			protected override void AddToRelationship(BusinessObject businessObject)
			{
				base.AddToRelationship(businessObject);

				var divot = (DtbBookingInstructionPkgDivot)businessObject;
				foreach (DtbBookingConfirmation confirmation in divot.ConfirmationsDivotOnly)
				{
					confirmation.KK_KN_BookingInstruction = Master.PK;
				}
			}
		}
	}
}
