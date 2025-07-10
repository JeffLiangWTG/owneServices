using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeTypeOverrideValidation : AutoAccChargeTypeOverrideValidation
	{
		public AccChargeTypeOverrideValidation(AutoAccChargeTypeOverride parent) : base(parent)
		{
		}

		new AccChargeTypeOverride Parent
		{
			get { return (AccChargeTypeOverride)base.Parent; }
		}

		bool IsParentChargeTypeComment
		{
			get { return Parent.ChargeCode != null && Parent.ChargeCode.AC_ChargeType == Core.Constants.ChargeType.Comment; }
		}

		#region AN_JobDirection

		protected override void CheckAN_JobDirection()
		{
			if (!IsParentChargeTypeComment)
			{
				base.CheckAN_JobDirection();
				MandatoryValidation.CheckEntered(Parent.AN_JobDirectionInfo);
				ListValidation.ErrorIfInvalidCode(Parent.AN_JobDirectionInfo);
			}
			else if (!Parent.AN_JobDirection.IsEmpty)
			{
				Parent.AN_JobDirectionInfo.AddError(ErrorMessageForInvalidDataIfChargeTypeIsComment);
			}
		}

		#endregion

		#region AN_JobType

		protected override void CheckAN_JobType()
		{
			if (!IsParentChargeTypeComment)
			{
				base.CheckAN_JobType();
				MandatoryValidation.CheckEntered(Parent.AN_JobTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.AN_JobTypeInfo);
			}
			else if (!Parent.AN_JobType.IsEmpty)
			{
				Parent.AN_JobTypeInfo.AddError(ErrorMessageForInvalidDataIfChargeTypeIsComment);
			}
		}

		#endregion

		#region AN_ChargeType

		protected override void CheckAN_ChargeType()
		{
			if (ShouldDisableChargeTypeOverride)
			{
				Parent.AN_ChargeTypeInfo.AddError(ErrorMessageForInvalidChargeTypeOverrideWithDsbCharge);
			}

			if (!IsParentChargeTypeComment)
			{
				base.CheckAN_ChargeType();
				MandatoryValidation.CheckEntered(Parent.AN_ChargeTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.AN_ChargeTypeInfo);
			}
			else if (!Parent.AN_ChargeType.IsEmpty)
			{
				Parent.AN_ChargeTypeInfo.AddError(ErrorMessageForInvalidDataIfChargeTypeIsComment);
			}

			if (Parent.ChargeCode != null)
			{
				ZString chargeType = Parent.ChargeCode.AC_ChargeType;
				switch (Parent.AN_ChargeType)
				{
					case Core.Constants.ChargeType.Disbursement:
					case Core.Constants.ChargeType.Margin:
					case Core.Constants.ChargeType.ManualJobAccrual:
						if (chargeType != Core.Constants.ChargeType.Margin && chargeType != Core.Constants.ChargeType.Disbursement && chargeType != Core.Constants.ChargeType.ManualJobAccrual)
						{
							Parent.AN_ChargeTypeInfo.AddError(ErrorMessageForInvalidChargeType + Core.Constants.ChargeType.Margin + ", " + Core.Constants.ChargeType.Disbursement + ", " + Core.Constants.ChargeType.ManualJobAccrual);
						}
						break;
					case Core.Constants.ChargeType.Revenue:
						if (chargeType != Core.Constants.ChargeType.Margin && chargeType != Core.Constants.ChargeType.Disbursement && chargeType != Core.Constants.ChargeType.ManualJobAccrual && chargeType != Core.Constants.ChargeType.Revenue)
						{
							Parent.AN_ChargeTypeInfo.AddError(ErrorMessageForInvalidChargeType + Core.Constants.ChargeType.Margin + ", " + Core.Constants.ChargeType.Disbursement + ", " + Core.Constants.ChargeType.ManualJobAccrual + ", " + Core.Constants.ChargeType.Revenue);
						}
						break;
				}
			}
		}

		bool ShouldDisableChargeTypeOverride
		{
			get
			{
				var chargeCode = Parent.ChargeCode;
				if (chargeCode == null)
				{
					return false;
				}
				var isDsbChargeWithDsbAccountsFilled = chargeCode.AC_ChargeType == Core.Constants.ChargeType.Disbursement && !chargeCode.AC_AG_DisbursementShortfallAccount.IsEmpty && !chargeCode.AC_AG_DisbursementSurplusAccount.IsEmpty;
				var isDsbRegistryEnabled = ObjectFactory.Get<IAccounting>()?.EnableBulkDisbursementJobsClosure ?? false;
				return isDsbChargeWithDsbAccountsFilled && isDsbRegistryEnabled;
			}
		}

		#endregion

		#region AN_MarginPercentage

		protected override void CheckAN_MarginPercentage()
		{
			if (!IsParentChargeTypeComment)
			{
				base.CheckAN_MarginPercentage();

				if (Parent.IsMargin)
				{
					if (Parent.AN_MarginPercentage < 0.0M || Parent.AN_MarginPercentage > 100M)
					{
						Parent.AN_MarginPercentageInfo.AddError(Res.GetString("ce3f19c4-a29b-4c9b-a606-aa069e9b9c1b", "Margin Percentage must be a value between 0.01 and 100."));
					}
				}
				else if (!Parent.IsDisbursement && Parent.AN_MarginPercentage != 0M)
				{
					Parent.AN_MarginPercentageInfo.AddError(Res.GetString("11b480b1-0d57-4e52-bcf5-fb301656c91f", "Margin Percentage value must be 0."));
				}
			}
			else if (!Parent.AN_MarginPercentage.IsEmpty)
			{
				Parent.AN_MarginPercentageInfo.AddError(ErrorMessageForInvalidDataIfChargeTypeIsComment);
			}
		}

		#endregion

		#region AN_InvoiceType

		protected override void CheckAN_InvoiceType()
		{
			if (!IsParentChargeTypeComment)
			{
				base.CheckAN_InvoiceType();
				MandatoryValidation.CheckEntered(Parent.AN_InvoiceTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.AN_InvoiceTypeInfo);
			}
			else if (!Parent.AN_InvoiceType.IsEmpty)
			{
				Parent.AN_InvoiceTypeInfo.AddError(ErrorMessageForInvalidDataIfChargeTypeIsComment);
			}
		}

		#endregion

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			if (info.Name == AccChargeTypeOverrideSchema.AN_AC_ChargeCode.Name)
			{
				return false;
			}

			return base.ShouldValidateFKToCancelledRecord(info);
		}

		#region Implementation

		static string ErrorMessageForInvalidDataIfChargeTypeIsComment
		{
			get { return Res.GetString("3efdc819-f5e2-4c5a-9ccd-2a697a78eb73", "This data is invalid for default charge type 'CMT'"); }
		}

		static string ErrorMessageForInvalidChargeType
		{
			get { return Res.GetString("506544b7-f412-41df-bf56-cfb6363017c6", "The default charge type should be more comprehensive than the override please change your default to either:") + " "; }
		}

		static string ErrorMessageForInvalidChargeTypeOverrideWithDsbCharge
		{
			get { return Res.GetString("EDDFD793-F157-4E93-BAFA-BCD64AC64AF9", "Charge Type Override is not supported if the default charge type is 'DSB' and DSB Surplus/Shortfall Accounts have been specified."); }
		}

		#endregion
	}
}
