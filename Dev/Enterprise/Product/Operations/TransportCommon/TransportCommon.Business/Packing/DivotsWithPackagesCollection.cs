using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business
{
	public sealed class DivotsWithPackagesCollection<T> : ActiveBusinessObjectCollection<T>, IDivotsWithPackagesCollection
		where T : DtbTransportInstructionPkgDivot
	{
		public DivotsWithPackagesCollection(DtbTransportInstruction instruction)
			: base(GetFactoryFromInstructionWithNullCheck(instruction), instruction, null, DtbBookingInstructionPkgDivotSchema.KD_KN_BookingInstruction)
		{
			Instruction = instruction;
		}

		readonly DtbTransportInstruction Instruction;

		static BusinessObjectFactory GetFactoryFromInstructionWithNullCheck(DtbTransportInstruction instruction)
		{
			return Argument.NotNull(instruction, "DtbTransportInstruction instruction").Factory;
		}

		DtbTransportInstructionPkgDivot IDivotsWithPackagesCollection.this[int index] => this[index];

		#region AddPackage

		public void AddPackage(PkgPackage package)
		{
			if (package != null && !Contains(package))
			{
				var divot = Instruction.Factory.New<T>();
				divot.KD_KP_Package = package.PK;
				divot.KD_KN_BookingInstruction = Instruction.PK;
				divot.Confirmations.Cast<DtbTransportConfirmation>().ForEach(x => x.SetIsEmptyContainerOnThisAndAllRelatedConfirmations());
			}
		}

		#endregion

		#region Contains

		public bool Contains(PkgPackage package)
		{
			var query = new ZQuery(DtbBookingInstructionPkgDivotSchema.KD_KP_Package, package.PK);
			return Find(query).Any();
		}

		#endregion

		#region RemovePackage

		public void RemovePackage(PkgPackage package)
		{
			if (package != null && (Contains(package) || ((IBusinessObjectInternals)package).IsUnCommittedRow))
			{
				var divotsToDelete = Instruction.PackageDivots.Cast<T>().Where(d => d.KD_KP_Package == package.PK).ToArray();
				foreach (var divot in divotsToDelete)
				{
					divot.Delete();
				}
			}
		}

		public void AddPackages(IEnumerable<PkgPackage> packages)
		{
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

		#endregion

		#region AllowNew

		protected override bool AllowNew
		{
			get { return false; }
		}

		#endregion

		#region Packages

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

		#endregion

		#region Typed

		IEnumerable<DtbTransportInstructionPkgDivot> IDivotsWithPackagesCollection.Typed
		{
			get
			{
				foreach (var divot in this)
				{
					yield return divot;
				}
			}
		}

		#endregion
	}
}
