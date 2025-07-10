//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoPkgPackageValidation
//
//    This class should be used for overriding validation in AutoPkgPackageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using System;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.Packing.Business
{
	public class PkgPackageValidation : AutoPkgPackageValidation
	{
		public PkgPackageValidation(AutoPkgPackage parent)
			: base(parent)
		{
		}

		protected new PkgPackage Parent
		{
			get { return (PkgPackage)base.Parent; }
		}

		public void ValidateKP_PackageID()
		{
			ValidateCalculatedProperty(Parent.KP_PackageIDInfo);
		}

		public static string EmptyContainerNumberMessage => Res.GetString("9b7d12c7-c4ff-4ff5-97d7-55ee68ccb6c8", "Container must have a container number.");

		protected virtual void CheckKP_PackageID()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.KP_PackageIDInfo);
			if (Parent.PackageJob?.ParentJob is IDtbBookingConsolidation dtbBookingConsolidation && Parent.IsContainer)
			{
				var instructions = dtbBookingConsolidation.Bookings
					.Where(b => b.IsSendingXUSToCTO)
					.SelectMany(b => b.Instructions)
					.Where(i => i.Packages.Contains(Parent) && i.KN_Sequence == 1);

				foreach (var instruction in instructions)
				{
					var isDLVConsolidationDirectionPICInstructionTypeAndCTOOrgType = dtbBookingConsolidation.KB_JobDirection == nameof(DtbBookingDirection.DLV) && instruction.KN_InstructionType == InstructionTypes.Codes.PickUp && instruction.OrganisationType == OrganisationTypesList.Codes.CTO;
					var isEmptyPackageIDContainer = string.IsNullOrEmpty(Parent.KP_PackageID);
					if (isDLVConsolidationDirectionPICInstructionTypeAndCTOOrgType && isEmptyPackageIDContainer)
					{
						Parent.KP_PackageIDInfo.AddMessageError(EmptyContainerNumberMessage);
					}
				}
			}
		}

		protected override void CheckKP_PackageQty()
		{
			base.CheckKP_PackageQty();
			if (Parent.KP_PackageQty < 1)
			{
				Parent.KP_PackageQtyInfo.AddError(Res.GetString("d48bfd58-b44e-489e-917f-34ce9fc8a966", "Package quantity cannot be less than 1"));
			}
		}

		protected static void ValidatePackageQtyInfo(PkgPackage package, Action<PkgPackage> packageQtyValidation)
		{
			((IValidationInternals)package.Validation).Validate(package.KP_PackageQtyInfo, () => packageQtyValidation(package));
		}

		protected override void CheckKP_ReleasedTimeUtc()
		{
			base.CheckKP_ReleasedTimeUtc();
			if (Parent.KP_IsHeld && Parent.IsReleased)
			{
				Parent.KP_ReleasedTimeUtcInfo.AddError(Res.GetString("FF94C57F-C008-4AB0-A9AC-3676E0D9C083", "A package that is held cannot be released"));
			}
		}

		public static string PackageIsNotContainerMessage => Res.GetString("2A0427C9-A332-49B9-96C1-98CD7F0E5058", "This package must be a container.");

		protected override void CheckKP_F3_NKPackType()
		{
			base.CheckKP_F3_NKPackType();
			if (Parent.PackageJob?.ParentJob is IDtbBookingConsolidation dtbBookingConsolidation && !Parent.IsContainer && dtbBookingConsolidation.Bookings.Where(b => b.IsSendingXUSToCTO).SelectMany(b => b.Instructions).SelectMany(p => p.Packages).Any(p => p == Parent))
			{
				Parent.KP_F3_NKPackTypeInfo.AddMessageError(PackageIsNotContainerMessage);
			}
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			return !PropertiesToIgnoreCancelValidation.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);
		}

		static ImmutableArray<string> PropertiesToIgnoreCancelValidation { get; } = ImmutableArray
			.Create(nameof(PkgPackage.KP_F3_NKPackType), nameof(PkgPackage.KP_KJ_ParentPackageJob), nameof(PkgPackage.KP_KP_ParentPackage), nameof(PkgPackage.KP_GS_NKClosedBy), nameof(PkgPackage.KP_GS_NKReleasedBy));

		#region ValidateRTUSPrinterPK

		public void ValidateRTUSLabelPrinterPK()
		{
			ValidateCalculatedProperty(Parent.RTUSLabelPrinterPKInfo);
		}

		protected void CheckRTUSLabelPrinterPK()
		{
			TypeValidation.CheckValidGuid(Parent.RTUSLabelPrinterPKInfo);
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateRTUSLabelPrinterPK();
			ValidateGoodsWeight();
			ValidateKP_PackageID();
		}

		#endregion

		#region ValidateGoodsWeight

		public void ValidateGoodsWeight()
		{
			ValidateCalculatedProperty(Parent.GoodsWeightInfo);
		}

		protected virtual void CheckGoodsWeight()
		{
		}

		#endregion
	}
}
