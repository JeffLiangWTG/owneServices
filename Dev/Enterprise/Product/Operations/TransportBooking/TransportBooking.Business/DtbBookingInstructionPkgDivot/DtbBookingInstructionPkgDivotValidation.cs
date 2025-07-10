//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTransportBookingInstructionPackageDivotValidation
//
//    This class should be used for overriding validation in AutoTransportBookingInstructionPackageDivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using Common = Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportBookings.Business
{
	public sealed class DtbBookingInstructionPkgDivotValidation : Common.DtbBookingInstructionPkgDivotValidation
	{
		public DtbBookingInstructionPkgDivotValidation(DtbBookingInstructionPkgDivot parent)
			: base(parent)
		{
		}

		public new DtbBookingInstructionPkgDivot Parent
		{
			get { return (DtbBookingInstructionPkgDivot)base.Parent; }
		}

		protected override void CheckKD_KN_BookingInstruction()
		{
			base.CheckKD_KN_BookingInstruction();

			if (!Parent.KD_KN_BookingInstructionInfo.HasErrors())
			{
				if (IsDuplicate)
				{
					Parent.KD_KN_BookingInstructionInfo.AddError(Res.GetString("320b6925-befc-49c0-846e-3e6441c5fcba", "The same Package cannot be assigned to the same Instruction."));
				}

				if (IsSubInstruction)
				{
					Parent.KD_KN_BookingInstructionInfo.AddError(Res.GetString("ddc8741b-ada7-47f4-beb4-5cafa933d602", "Direct assignment of a package to a sub Instruction is not allowed."));
				}
			}
		}

		protected override void CheckKD_KP_Package()
		{
			base.CheckKD_KP_Package();

			if (!Parent.KD_KP_PackageInfo.HasErrors() && IsDuplicate)
			{
				Parent.KD_KP_PackageInfo.AddError(Res.GetString("320b6925-befc-49c0-846e-3e6441c5fcba", "The same Package cannot be assigned to the same Instruction."));
			}
		}

		protected override void CheckKD_Quantity()
		{
			base.CheckKD_Quantity();

			if (Parent.KD_Quantity < 1)
			{
				var package = Parent.Package;
				if (package != null && package.KP_PackageQty == 0)
				{
					if (Parent.KD_Quantity < 0)
					{
						Parent.KD_QuantityInfo.AddError(Res.GetString("e1d77cf1 -67fe-4b17-b187-008fa37aa168", "Quantity cannot be less than 0."));
					}
				}
				else
				{
					Parent.KD_QuantityInfo.AddError(Res.GetString("4728c0e5-69fb-4ab7-b6e4-d959e1c11a49", "Quantity cannot be less than 1."));
				}
			}
			else
			{
				var package = Parent.Package;
				if (package != null && Parent.KD_Quantity > package.KP_PackageQty)
				{
					if (package.KP_PackageQty == 0)
					{
						Parent.KD_QuantityInfo.AddError(Res.GetString("009f8bf7-187a-4dbd-b12a-9ac3205ced92",
							"You cannot assign any units to this instruction because the quantity on the Package is unknown."));
					}
					else
					{
						Parent.KD_QuantityInfo.AddError(Res.GetString("fc252f39-a63c-4176-966b-96cd964947a8",
							"You cannot assign {0} {1} to this instruction because the quantity on the Package is only {2} {1}.",
							Parent.KD_Quantity, package.Description, package.KP_PackageQty));
					}
				}
			}
		}

		bool IsDuplicate
		{
			get
			{
				var uniqueQuery = new ZQuery(DtbBookingInstructionPkgDivotSchema.KD_KP_Package, Parent.KD_KP_Package);
				uniqueQuery.AddToFilter(DtbBookingInstructionPkgDivotSchema.KD_KN_BookingInstruction, Parent.KD_KN_BookingInstruction);

				return Parent.Factory.Load<DtbBookingInstructionPkgDivot>(uniqueQuery).Length > 1;
			}
		}

		bool IsSubInstruction => Parent.Instruction != null && Parent.Instruction.IsSub;
	}
}
