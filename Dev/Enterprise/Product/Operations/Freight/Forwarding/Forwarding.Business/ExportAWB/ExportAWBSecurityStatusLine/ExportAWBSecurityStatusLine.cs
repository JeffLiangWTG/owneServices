using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	[DependentBusinessObject(typeof(ExportAWBHeader), "ExportAWBSecurityStatusLines")]
	public class ExportAWBSecurityStatusLine : Forwarding.AWB.Business.ExportAWBSecurityStatusLine
	{
		public ExportAWBSecurityStatusLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Implementation

		public override ZString EAS_ApprovalCategory
		{
			get { return base.EAS_ApprovalCategory; }
			set
			{
				if (base.EAS_ApprovalCategory != value)
				{
					base.EAS_ApprovalCategory = value;

					ResetExpiryDateIfNotSupported();
					MarkMasterAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateEAS_ApprovalExpiryDate();
						Validation.ValidateEAS_RN_NKCountryCode();
					}
				}
			}
		}

		public override CodeDescriptionPairList ApprovalCategoryList
		{
			get
			{
				if (EAS_RN_NKCountryCode.IsEmpty)
				{
					return new AviationSecuritySchemeMembership();
				}

				var config = SupplyChainSecurityConfiguration.New(EAS_RN_NKCountryCode);

				var list = new CodeDescriptionPairList();

				foreach (ICodeDescription item in config.ApprovalCodesList)
				{
					if (item.Code != AviationSecuritySchemeMembershipEx.Codes.Yes && item.Code != AviationSecuritySchemeMembershipEx.Codes.No)
					{
						list.AddPair(item.Code, item.Description);
					}
				}
				return list;
			}
		}

		public override ZString EAS_RN_NKCountryCode
		{
			get { return base.EAS_RN_NKCountryCode; }
			set
			{
				if (base.EAS_RN_NKCountryCode != value)
				{
					base.EAS_RN_NKCountryCode = value;

					ResetExpiryDateIfNotSupported();
					MarkMasterAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateEAS_ApprovalExpiryDate();
						Validation.ValidateEAS_ApprovalCategory();
						Validation.ValidateEAS_ApprovalNumber();
					}
				}
			}
		}

		public override ZString EAS_ApprovalNumber
		{
			get { return base.EAS_ApprovalNumber; }
			set
			{
				if (base.EAS_ApprovalNumber != value)
				{
					base.EAS_ApprovalNumber = value;

					MarkMasterAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateEAS_ApprovalExpiryDate();
						Validation.ValidateEAS_ApprovalCategory();
					}
				}
			}
		}

		#region EAS_ApprovalExpiryDate

		public override ZDateTime EAS_ApprovalExpiryDate
		{
			get { return base.EAS_ApprovalExpiryDate; }
			set
			{
				if (base.EAS_ApprovalExpiryDate != value)
				{
					base.EAS_ApprovalExpiryDate = value;

					MarkMasterAsNeedingValidation();
				}
			}
		}

		protected override bool EAS_ApprovalExpiryDate_ReadOnly => Master == null ? !ExpiryDateIsSupported : (!ExpiryDateIsSupported || !Master.IsCSDOverridden);

		bool ExpiryDateIsSupported => SupplyChainSecurityConfiguration.New(EAS_RN_NKCountryCode).ApprovalCodeAllowsExpiryDate(EAS_ApprovalCategory);

		void ResetExpiryDateIfNotSupported()
		{
			if (!EAS_ApprovalExpiryDate.IsEmpty && !ExpiryDateIsSupported)
			{
				EAS_ApprovalExpiryDate = ZDate.Empty;
			}
		}

		#endregion

		[ReadOnly(true)]
		[ResourceStringData("896ecb08-89fe-46d7-a4a3-452c42bd3628", Caption = "Screening Method")]
		public override ZString EAS_ScreeningMethod
		{
			get { return base.EAS_ScreeningMethod; }
			set
			{
				if (EAS_ScreeningMethod != value)
				{
					base.EAS_ScreeningMethod = value;

					MarkMasterAsNeedingValidation();
				}
			}
		}

		[ReadOnly(true)]
		[ResourceStringData("e9ecbafa-db62-46aa-adf2-362030a0e0b5", Caption = "Grounds for Exemption")]
		public override ZString EAS_ExemptionGround
		{
			get { return base.EAS_ExemptionGround; }
			set
			{
				if (EAS_ExemptionGround != value)
				{
					base.EAS_ExemptionGround = value;

					MarkMasterAsNeedingValidation();
				}
			}
		}

		protected override Forwarding.AWB.Business.ExportAWBSecurityStatusLineValidation GetNewValidation()
		{
			return new ExportAWBSecurityStatusLineValidation(this);
		}

		public new ExportAWBHeader Master
		{
			get { return (ExportAWBHeader)base.Master; }
		}

		ExportAWBHeader OriginalMaster
		{
			get { return Factory.Load<ExportAWBHeader>(IsDeleted || EAS_EH.IsEmpty ? originalER_EH : EAS_EH); }
		}

		public override ZGuid EAS_EH
		{
			get { return base.EAS_EH; }
			set
			{
				if (base.EAS_EH != value)
				{
					base.EAS_EH = value;

					if (!value.IsEmpty)
					{
						originalER_EH = value;
					}

					MarkMasterAsNeedingValidation();
				}
			}
		}
		ZGuid originalER_EH;

		public override bool IsSavedByFactory
		{
			get
			{
				var saveMode = OriginalMaster != null
					? OriginalMaster.FactorySaveMode
					: ExportAWBHeader.SaveMode.Normal;

				switch (saveMode)
				{
					case ExportAWBHeader.SaveMode.Normal:
						return base.IsSavedByFactory;
					case ExportAWBHeader.SaveMode.Forced:
						return IsInDatabase || (!IsDeleted && !IsEmpty);
					default:
						return false;
				}
			}
		}

		void MarkMasterAsNeedingValidation()
		{
			var master = Master;

			if (master != null)
			{
				master.MarkAsNeedingValidation();
			}
		}

		public ICollection<ForwardingShipment> Shipments
		{
			get
			{
				if (shipments == null)
				{
					var comparer = new LambdaComparer<ForwardingShipment>((s1, s2) => s1.PK == s2.PK, s1 => s1.PK.GetHashCode());
					shipments = new HashSet<ForwardingShipment>(comparer);
				}

				return shipments;
			}
		}

		HashSet<ForwardingShipment> shipments;

		public OrgHeader Organization
		{
			get; set;
		}

		#endregion
	}
}
