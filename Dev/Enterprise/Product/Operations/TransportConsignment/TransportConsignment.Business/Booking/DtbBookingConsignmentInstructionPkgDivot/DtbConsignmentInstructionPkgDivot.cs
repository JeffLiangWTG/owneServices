using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentInstructionPkgDivot : DtbTransportInstructionPkgDivot
	{
		public DtbConsignmentInstructionPkgDivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		#region ConfirmationsDivotOnly

		public new DtbConsignmentConfirmationCollection ConfirmationsDivotOnly
		{
			get { return (DtbConsignmentConfirmationCollection)base.ConfirmationsDivotOnly; }
		}

		protected override IDtbTransportConfirmationCollection GetNewConfirmationsDivotOnlyCollection()
		{
			return new DtbConsignmentConfirmationCollection(this);
		}

		#endregion

		#region Confirmations

		public new DtbConsignmentConfirmationCollection Confirmations
		{
			get { return (DtbConsignmentConfirmationCollection)base.Confirmations; }
		}

		protected override IDtbTransportConfirmationCollection GetNewConfirmationsCollection()
		{
			return new DtbConsignmentConfirmationCollection(Factory, new PackageDivotInstructionConfirmationRelationship<DtbConsignmentConfirmation>(this));
		}

		#endregion

		#region Instruction

		public new DtbConsignmentInstruction Instruction
		{
			get { return (DtbConsignmentInstruction)base.Instruction; }
		}

		protected override Type InstructionType
		{
			get { return typeof(DtbConsignmentInstruction); }
		}

		#endregion

		#endregion

		#region Properties

		#region KD_KP_Package

		protected override void OnBeforePackageAssignedToDivot()
		{
			base.OnBeforePackageAssignedToDivot();

			var package = Package;
			if (package != null)
			{
				package.KP_PackageQtyInfo.ValueChanged -= PackageKP_PackageQty_ValueChanged;
			}
		}

		protected override void OnPackageAssignedOrUnassignedToDivot()
		{
			base.OnPackageAssignedOrUnassignedToDivot();
			HookPackage();
		}

		internal void HookPackage()
		{
			var package = Package;
			if (package != null)
			{
				package.KP_PackageQtyInfo.ValueChanged += PackageKP_PackageQty_ValueChanged;
			}
		}

		void PackageKP_PackageQty_ValueChanged(object sender, EventArgs e)
		{
			SetQuantityFromPackage();
		}

		#endregion

		#endregion

		#region Delete

		protected override void DeleteCore()
		{
			base.DeleteCore();
			KD_KP_Package = ZGuid.Empty; // Unhook Package
		}

		#endregion

		#region Lookups

		public new DtbConsignmentInstructionPkgDivotLookups Lookups
		{
			get { return (DtbConsignmentInstructionPkgDivotLookups)base.Lookups; }
		}

		protected override DtbTransportInstructionPkgDivotLookups GetNewLookupsCore()
		{
			return new DtbConsignmentInstructionPkgDivotLookups(this);
		}

		#endregion

		#region Validation

		public new DtbConsignmentInstructionPkgDivotValidation Validation
		{
			get { return (DtbConsignmentInstructionPkgDivotValidation)base.Validation; }
		}

		protected override DtbTransportInstructionPkgDivotValidation GetNewValidationCore()
		{
			return new DtbConsignmentInstructionPkgDivotValidation(this);
		}

		#endregion
	}
}
