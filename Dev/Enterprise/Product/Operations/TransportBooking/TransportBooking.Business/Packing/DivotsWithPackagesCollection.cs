using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business
{
	public sealed class DivotsWithPackagesCollection : ActiveBusinessObjectCollection<DtbBookingInstructionPkgDivot>
	{
		public DivotsWithPackagesCollection(DtbBookingInstruction instruction)
			: base(GetFactoryFromInstructionWithNullCheck(instruction))
		{
			Instruction = instruction;
			SetRelationshipFromInstruction();
		}

		readonly DtbBookingInstruction Instruction;

		static BusinessObjectFactory GetFactoryFromInstructionWithNullCheck(DtbBookingInstruction instruction)
		{
			return Argument.NotNull(instruction, "DtbBookingInstruction instruction").Factory;
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
				Relationship = new DependentRelationship(Instruction, typeof(DtbBookingInstructionPkgDivot), null, DtbBookingInstructionPkgDivotSchema.KD_KN_BookingInstruction);
			}
		}

		public void AddPackage(PkgPackage package)
		{
			if (Instruction.IsSub)
			{
				throw new InvalidOperationException("Cannot call AddPackage() on divots for sub instruction");
			}

			if (package != null && !Contains(package))
			{
				var divot = Instruction.Factory.New<DtbBookingInstructionPkgDivot>();
				divot.KD_KP_Package = package.PK;
				divot.KD_KN_BookingInstruction = Instruction.PK;
				OnPackageAdded(Instruction, package);
			}
		}

		void OnPackageAdded(DtbBookingInstruction instruction, PkgPackage package)
		{
			if (instruction.Booking is DtbBooking booking && !booking.DelayInstructionUpdatesFromAddingDivots)
			{
				AfterPackagesAdded(instruction, new PkgPackage[] { package });
			}
		}

		public static void AfterPackagesAdded(DtbBookingInstruction instruction, IEnumerable<PkgPackage> packages)
		{
			Argument.NotNull(instruction, nameof(instruction));
			Argument.NotNull(packages, nameof(packages));

			if (instruction.Booking is DtbBooking booking)
			{
				if (instruction.Confirmations.Any())
				{
					booking.SetIsEmptyContainerOnAllRelatedConfirmations();
				}

				if (!packages.Any() || packages.Any(p => p.PackageJob != null && p.PackageJob == booking.PackageJob))
				{
					booking.UpdateIsHazardous();
					booking.UpdateRequiresRefrigeration();
				}
			}
		}

		public bool Contains(PkgPackage package)
		{
			var query = new ZQuery(DtbBookingInstructionPkgDivotSchema.KD_KP_Package, package.PK);
			return Find(query).Any();
		}

		public void RemovePackage(PkgPackage package)
		{
			if (Instruction.IsSub)
			{
				throw new InvalidOperationException("Cannot call RemovePackage() on divots for sub instruction");
			}

			if (package != null && (Contains(package) || ((IBusinessObjectInternals)package).IsUnCommittedRow))
			{
				var divotsToDelete = Instruction.PackageDivots.Cast<DtbBookingInstructionPkgDivot>().Where(d => d.KD_KP_Package == package.PK).ToArray();
				foreach (var divot in divotsToDelete)
				{
					divot.Delete();
				}
			}
		}

		public void AddPackages(IEnumerable<PkgPackage> packages)
		{
			if (Instruction.IsSub)
			{
				throw new InvalidOperationException("Cannot call AddPackages() on divots for sub instruction");
			}

			var existingDivots = this.ToDictionary(d => d.KD_KP_Package);

			using (Instruction.SuspendDefaultingDropMode())
			{
				foreach (var package in packages)
				{
					AddPackage(package);

					if (existingDivots.TryGetValue(package.PK, out var divot) && divot.KD_Quantity != package.KP_PackageQty)
					{
						divot.KD_Quantity = package.KP_PackageQty;
					}
				}
			}

			Instruction.DefaultDropMode();
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		public IEnumerable<PkgPackage> Packages
		{
			get
			{
				foreach (var divot in this)
				{
					var package = divot.Package;
					if (package != null)
					{
						yield return package;
					}
				}
			}
		}

		public IEnumerable<DtbBookingInstructionPkgDivot> Typed
		{
			get
			{
				foreach (var divot in this)
				{
					yield return divot;
				}
			}
		}
	}
}
