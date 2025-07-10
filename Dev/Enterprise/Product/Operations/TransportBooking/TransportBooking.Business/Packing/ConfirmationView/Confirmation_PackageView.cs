using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.Business
{
	public class Confirmation_PackageView : NonPersistentBusinessObject
	{
		public Confirmation_PackageView(DtbBookingConfirmation confirmation, DtbBookingPackage_PackageView packageView)
			: base(confirmation.Factory)
		{
			this.confirmation = confirmation;
			this.packageView = packageView;
		}

		public static class Schema
		{
			public const string ConfirmationDescription = "ConfirmationDescription";
			public const string ConfirmationType = "ConfirmationType";
			public const string Actual = "Actual";
			public const string Estimated = "Estimated";
			public const string Quantity = "Quantity";
			public const string ReceivedBy = "ReceivedBy";
			public const string ReferenceNum = "ReferenceNum";
			public const string RequiredFrom = "RequiredFrom";
			public const string RequiredTo = "RequiredTo";

			public const string InstructionDivotPK = "InstructionDivotPK";
		}

		public PkgPackage Package
		{
			get { return packageView.Package; }
		}

		public DtbBookingPackage_PackageView PackageView
		{
			get { return packageView; }
		}

		readonly DtbBookingPackage_PackageView packageView;

		public DtbBookingConfirmation Confirmation
		{
			get { return confirmation; }
		}

		readonly DtbBookingConfirmation confirmation;

		public DtbBookingInstructionPkgDivot PackageDivot
			=> Confirmation.PackageDivot
				// assuming you can't have 2 divots between the same Instruction and Package!
				?? PackageView.InstructionDivots.FirstOrDefault(d => d.KD_KN_BookingInstruction == Confirmation.Instruction.PK);

		[BusinessObjectTestExclude]
		[ResourceStringData("ConfirmationFromPackageView|InstructionDivotPK", Caption = "Instruction")]
		[List("InstructionDivots")]
		public ZGuid InstructionDivotPK
		{
			get
			{
				RefreshInstructionDivotPK_IfNeeded();
				return instructionDivotPK;
			}
			set
			{
				if (instructionDivotPK != value)
				{
					SetNonPersistentPropertyValue(InstructionDivotPKInfo, ref instructionDivotPK, value);

					if (InstructionDivotsContainsPK(instructionDivotPK))
					{
						if (PackageDivot != null)
						{
							Confirmation.SplitFromInstructionToPackageDivots(PackageDivot);
						}

						Confirmation.KK_KD_BookingInstructionPkgDivot = instructionDivotPK;
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateInstructionDivotPK();
				}

				InstructionDivotPKInfo.RefreshBinding();
			}
		}

		ZGuid instructionDivotPK = ZGuid.Missing;

		public ZPropertyInfo InstructionDivotPKInfo
		{
			get { return GetZPropertyInfo(Schema.InstructionDivotPK); }
		}

		void RefreshInstructionDivotPK_IfNeeded()
		{
			var divot = PackageDivot;
			if (divot != null)
			{
				bool isMissing = instructionDivotPK == ZGuid.Missing;

				if (isMissing)
				{
					instructionDivotPK = divot.PK;
				}
			}
		}

		[List("Confirmation.Lookups.ConfirmationTypes")]
		public ZString ConfirmationType
		{
			get { return Confirmation.KK_ConfirmationType; }
			set
			{
				if (Confirmation.KK_ConfirmationType != value)
				{
					Split();
					Confirmation.KK_ConfirmationType = value;
				}
			}
		}

		public ZPropertyInfo ConfirmationTypeInfo
		{
			get { return Confirmation == null ? GetZPropertyInfo(Schema.ConfirmationType) : GetWrappedZPropertyInfo(Schema.ConfirmationType, x => Confirmation.KK_ConfirmationTypeInfo); }
		}

		[ResourceStringData("ConfirmationFromPackageView|ConfirmationDescription", ShortCaption = "Type", Caption = "Confirmation Type")]
		[List("Confirmation.Lookups.ConfirmationDescriptions")]
		public ZString ConfirmationDescription
		{
			get { return Confirmation.ConfirmationDescription; }
			set
			{
				if (Confirmation.ConfirmationDescription != value)
				{
					Split();
					Confirmation.ConfirmationDescription = value;
				}
			}
		}

		public ZPropertyInfo ConfirmationDescriptionInfo
		{
			get { return Confirmation == null ? GetZPropertyInfo(Schema.ConfirmationDescription) : GetWrappedZPropertyInfo(Schema.ConfirmationDescription, x => Confirmation.ConfirmationDescriptionInfo); }
		}

		[ResourceStringData("ConfirmationFromPackageView|Actual", Caption = "Actual")]
		public ZDateTime Actual
		{
			get { return Confirmation.KK_Actual; }
			set
			{
				if (Confirmation.KK_Actual != value)
				{
					Split();
					Confirmation.KK_Actual = value;
				}
			}
		}

		public ZPropertyInfo ActualInfo
		{
			get { return Confirmation == null ? GetZPropertyInfo(Schema.Actual) : GetWrappedZPropertyInfo(Schema.Actual, x => Confirmation.KK_ActualInfo); }
		}

		[ResourceStringData("ConfirmationFromPackageView|Estimated", Caption = "Estimated")]
		public ZDateTime Estimated
		{
			get { return Confirmation.KK_Estimated; }
			set
			{
				if (Confirmation.KK_Estimated != value)
				{
					Split();
					Confirmation.KK_Estimated = value;
				}
			}
		}

		public ZPropertyInfo EstimatedInfo
		{
			get { return Confirmation == null ? GetZPropertyInfo(Schema.Estimated) : GetWrappedZPropertyInfo(Schema.Estimated, x => Confirmation.KK_EstimatedInfo); }
		}

		[ResourceStringData("ConfirmationFromPackageView|Quantity", ShortCaption = "Qty", Caption = "Quantity")]
		public ZInt Quantity
		{
			get { return Confirmation.KK_Quantity; }
			set
			{
				if (Confirmation.KK_Quantity != value)
				{
					// Do not split! When the parent of the confirmation is an instruction, Quantity is readonly.
					Confirmation.KK_Quantity = value;
				}
			}
		}

		public ZPropertyInfo QuantityInfo
		{
			get { return Confirmation == null ? GetZPropertyInfo(Schema.Quantity) : GetWrappedZPropertyInfo(Schema.Quantity, x => Confirmation.KK_QuantityInfo); }
		}

		[ResourceStringData("ConfirmationFromPackageView|ReceivedBy", Caption = "Signed By")]
		public ZString ReceivedBy
		{
			get { return Confirmation.KK_ReceivedBy; }
			set
			{
				if (Confirmation.KK_ReceivedBy != value)
				{
					Split();
					Confirmation.KK_ReceivedBy = value;
				}
			}
		}

		public ZPropertyInfo ReceivedByInfo
		{
			get { return Confirmation == null ? GetZPropertyInfo(Schema.ReceivedBy) : GetWrappedZPropertyInfo(Schema.ReceivedBy, x => Confirmation.KK_ReceivedByInfo); }
		}

		[ResourceStringData("ConfirmationFromPackageView|ReferenceNum", ShortCaption = "Ref. Num.", MediumCaption = "Reference Num.", Caption = "Reference Number")]
		public ZString ReferenceNum
		{
			get { return Confirmation.KK_ReferenceNum; }
			set
			{
				if (Confirmation.KK_ReferenceNum != value)
				{
					Split();
					Confirmation.KK_ReferenceNum = value;
				}
			}
		}

		public ZPropertyInfo ReferenceNumInfo
		{
			get { return Confirmation == null ? GetZPropertyInfo(Schema.ReferenceNum) : GetWrappedZPropertyInfo(Schema.ReferenceNum, x => Confirmation.KK_ReferenceNumInfo); }
		}

		[ResourceStringData("ConfirmationFromPackageView|RequiredFrom", Caption = "Required From")]
		public ZDateTime RequiredFrom
		{
			get { return Confirmation.KK_RequiredFrom; }
			set
			{
				if (Confirmation.KK_RequiredFrom != value)
				{
					Split();
					Confirmation.KK_RequiredFrom = value;
				}
			}
		}

		public ZPropertyInfo RequiredFromInfo
		{
			get { return Confirmation == null ? GetZPropertyInfo(Schema.RequiredFrom) : GetWrappedZPropertyInfo(Schema.RequiredFrom, x => Confirmation.KK_RequiredFromInfo); }
		}

		[ResourceStringData("ConfirmationFromPackageView|RequiredTo", Caption = "Required To")]
		public ZDateTime RequiredTo
		{
			get { return Confirmation.KK_RequiredTo; }
			set
			{
				if (Confirmation.KK_RequiredTo != value)
				{
					Split();
					Confirmation.KK_RequiredTo = value;
				}
			}
		}

		public ZPropertyInfo RequiredToInfo
		{
			get { return Confirmation == null ? GetZPropertyInfo(Schema.RequiredTo) : GetWrappedZPropertyInfo(Schema.RequiredTo, x => Confirmation.KK_RequiredToInfo); }
		}

		void Split()
		{
			if (Confirmation.Instruction != null)
			{
				Confirmation.SplitFromInstructionToPackageDivots(PackageDivot);
			}
		}

		public CodeDescriptionPairList InstructionDivots
		{
			get
			{
				var result = new CodeDescriptionPairList();

				foreach (DtbBookingInstructionPkgDivot divot in packageView.InstructionDivots)
				{
					var instructionDescription = divot.Instruction.Description;
					result.AddPair(divot.PK, instructionDescription, Res.GetString("0cfa4efe-c915-4ad3-99eb-515ad49079e3", "Applies to '{0}' Only", instructionDescription));
				}

				return result;
			}
		}

		bool InstructionDivotsContainsPK(ZGuid pk)
		{
			return Array.Exists(InstructionDivots.ToArray(), c => (ZGuid)c.PK == pk);
		}

		public override void Delete()
		{
			Split();
			Confirmation.Delete();

			base.Delete();
		}

		public override bool IsDeleted
		{
			get { return base.IsDeleted || Confirmation.IsDeleted; }
		}

		public Confirmation_PackageViewValidation Validation
		{
			get { return new Confirmation_PackageViewValidation(this); }
		}
	}
}
