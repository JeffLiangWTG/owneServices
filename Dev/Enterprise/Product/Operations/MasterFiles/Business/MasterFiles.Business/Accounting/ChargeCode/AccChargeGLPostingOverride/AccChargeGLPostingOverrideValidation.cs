//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccChargeGLPostingOverrideValidation
//
//    This class should be used for overriding validation in AutoAccChargeGLPostingOverrideValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeGLPostingOverrideValidation : AutoAccChargeGLPostingOverrideValidation
	{
		public AccChargeGLPostingOverrideValidation(AutoAccChargeGLPostingOverride parent)
			: base(parent)
		{
		}

		#region Y1_AG_ACR

		protected override void CheckY1_AG_ACR()
		{
			if (Parent.ChargeCode.RequiredProperties(Parent.ChargeCode.HighestChargeType).AccrualAccount)
			{
				base.CheckY1_AG_ACR();
				MandatoryValidation.CheckEntered(Parent.Y1_AG_ACRInfo);
				ListValidation.ErrorIfInvalidPK(Parent.Y1_AG_ACRInfo, Parent.Lookups.GLAccrualAccountCollection, messageWhenWrongAccountSelected);
			}
		}

		#endregion

		#region Y1_AG_REV

		protected override void CheckY1_AG_REV()
		{
			if (Parent.ChargeCode.RequiredProperties(Parent.ChargeCode.HighestChargeType).RevenueAccount)
			{
				base.CheckY1_AG_REV();
				MandatoryValidation.CheckEntered(Parent.Y1_AG_REVInfo);
				ListValidation.ErrorIfInvalidPK(Parent.Y1_AG_REVInfo, Parent.Lookups.GLRevenueAccountCollection, messageWhenWrongAccountSelected);
			}
		}

		#endregion

		#region Y1_AG_CST

		protected override void CheckY1_AG_CST()
		{
			if (Parent.ChargeCode.RequiredProperties(Parent.ChargeCode.HighestChargeType).CostAccount)
			{
				base.CheckY1_AG_CST();
				MandatoryValidation.CheckEntered(Parent.Y1_AG_CSTInfo);
				ListValidation.ErrorIfInvalidPK(Parent.Y1_AG_CSTInfo, Parent.Lookups.GLCostAccountCollection, messageWhenWrongAccountSelected);
			}
		}

		#endregion

		#region Y1_AG_WIP

		protected override void CheckY1_AG_WIP()
		{
			if (Parent.ChargeCode.RequiredProperties(Parent.ChargeCode.HighestChargeType).WIPAccount)
			{
				base.CheckY1_AG_WIP();
				MandatoryValidation.CheckEntered(Parent.Y1_AG_WIPInfo);
				ListValidation.ErrorIfInvalidPK(Parent.Y1_AG_WIPInfo, Parent.Lookups.GLWIPAccountCollection, messageWhenWrongAccountSelected);
			}
		}

		#endregion

		#region Y1_GE

		protected override void CheckY1_GE()
		{
			base.CheckY1_GE();

			CheckY1_GEForGLPostingOverride();
		}

		void CheckY1_GEForGLPostingOverride()
		{
			var glPostingOverridesCollection = Parent.ChargeCode.GLPostingOverrides.Cast<AccChargeGLPostingOverride>();
			var glPostingOverride = this.Parent;

			if (glPostingOverride.Y1_ConsolidationAccountingCategoryClass == ConsolidatedAccountingCategoryClassList.Codes.Intercompany)
			{
				var collectionWithINTClass = glPostingOverridesCollection.Where(x => x.Y1_ConsolidationAccountingCategoryClass == ConsolidatedAccountingCategoryClassList.Codes.Intercompany);
				if (collectionWithINTClass.Any() && collectionWithINTClass.All(x => !x.Y1_GE.IsEmpty))
				{
					glPostingOverride.Y1_GEInfo.AddError(ResString.GetMultilingualString("4ef43075-652f-40cb-8792-a8d77fab2fd9", "There must be at least one row of GL Posting Override without department specified for 'INT' class."));
				}
			}
			else
			{
				if (glPostingOverride.Y1_GE.IsEmpty)
				{
					glPostingOverride.Y1_GEInfo.AddError(ResString.GetMultilingualString("81829355-2c35-413f-a3a9-2e8ac3cfa90a", "A department must be specified if the 'Class' value is 'ALL' or 'TPY."));
				}
			}

			CheckIsUnique(glPostingOverride.Y1_GEInfo);
		}

		#endregion

		#region Y1_AG_REV_Clearing

		protected override void CheckY1_AG_REV_Clearing()
		{
			base.CheckY1_AG_REV_Clearing();
			ListValidation.ErrorIfInvalidPK(Parent.Y1_AG_REV_ClearingInfo, clrAccErrMsg);
		}

		#endregion

		#region Y1_AG_CST_Clearing

		protected override void CheckY1_AG_CST_Clearing()
		{
			base.CheckY1_AG_CST_Clearing();
			ListValidation.ErrorIfInvalidPK(Parent.Y1_AG_CST_ClearingInfo, clrAccErrMsg);
		}

		#endregion

		#region Y1_ConsolidationAccountingCategoryClass

		protected override void CheckY1_ConsolidationAccountingCategoryClass()
		{
			base.CheckY1_ConsolidationAccountingCategoryClass();
			MandatoryValidation.CheckEntered(Parent.Y1_ConsolidationAccountingCategoryClassInfo);
			ListValidation.ErrorIfInvalidCode(Parent.Y1_ConsolidationAccountingCategoryClassInfo, new ConsolidatedAccountingCategoryClassList());

			CheckIsUnique(Parent.Y1_ConsolidationAccountingCategoryClassInfo);
		}

		#endregion

		#region Y1_JobType

		protected override void CheckY1_JobType()
		{
			base.CheckY1_JobType();

			MandatoryValidation.CheckEntered(Parent.Y1_JobTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.Y1_JobTypeInfo, Parent.Lookups.JobTypeList);

			if (!Parent.Y1_JobTypeInfo.HasErrors())
			{
				CheckIsUnique(Parent.Y1_JobTypeInfo);
			}

			if (!Parent.Y1_JobTypeInfo.HasErrors())
			{
				ValidateY1_TransportMode();
			}
		}

		#endregion

		#region Y1_TransportMode

		protected override void CheckY1_TransportMode()
		{
			base.CheckY1_TransportMode();

			if (!Parent.Y1_TransportModeInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.Y1_TransportModeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.Y1_TransportModeInfo, Parent.Lookups.TransportModeList);

				if (!Parent.Y1_TransportModeInfo.HasErrors())
				{
					if ((Parent.Y1_JobType == JobInvoicingConsumerTypes.CFSShipmentCode || Parent.Y1_JobType == JobInvoicingConsumerTypes.CFSLoadListCode) &&
						Parent.Y1_TransportMode != Core.Constants.TransportModes.Air &&
						Parent.Y1_TransportMode != Core.Constants.TransportModes.Sea &&
						Parent.Y1_TransportMode != Core.Constants.TransportModes.Road &&
						Parent.Y1_TransportMode != Core.Constants.TransportModes.Rail &&
						Parent.Y1_TransportMode != AccChargeGLPostingOverrideLookups.TransportModeAdditionalCodes.All)
					{
						Parent.Y1_TransportModeInfo.AddError(Res.GetString("3c662b7a-2956-4d6b-b5e9-9272210d3e0a", "Only 'Air', 'Sea', 'Road', 'Rail' and 'All' values are relevant for this Job Type."));
					}
				}

				if (!Parent.Y1_TransportModeInfo.HasErrors())
				{
					CheckIsUnique(Parent.Y1_TransportModeInfo);
				}
			}
		}

		#endregion

		#region Y1_Direction

		protected override void CheckY1_Direction()
		{
			base.CheckY1_Direction();

			if (!Parent.Y1_DirectionInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.Y1_DirectionInfo);
				ListValidation.ErrorIfInvalidCode(Parent.Y1_DirectionInfo, Parent.Lookups.DirectionList);

				if (!Parent.Y1_DirectionInfo.HasErrors())
				{
					CheckIsUnique(Parent.Y1_DirectionInfo);
				}
			}
		}

		#endregion

		#region Y1_ConsolContainerMode

		protected override void CheckY1_ConsolContainerMode()
		{
			base.CheckY1_ConsolContainerMode();

			if (!Parent.Y1_ConsolContainerModeInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.Y1_ConsolContainerModeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.Y1_ConsolContainerModeInfo, Parent.Lookups.ConsolContainerModeList);

				if (!Parent.Y1_ConsolContainerModeInfo.HasErrors())
				{
					CheckIsUnique(Parent.Y1_ConsolContainerModeInfo);
				}
			}
		}

		#endregion

		#region Y1_MasterPaymentType

		protected override void CheckY1_MasterPaymentType()
		{
			base.CheckY1_MasterPaymentType();

			if (!Parent.Y1_MasterPaymentTypeInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.Y1_MasterPaymentTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.Y1_MasterPaymentTypeInfo, Parent.Lookups.MasterPaymentTypeList);

				if (!Parent.Y1_MasterPaymentTypeInfo.HasErrors())
				{
					CheckIsUnique(Parent.Y1_MasterPaymentTypeInfo);
				}
			}
		}

		#endregion

		#region Y1_HousePaymentType

		protected override void CheckY1_HousePaymentType()
		{
			base.CheckY1_HousePaymentType();

			if (!Parent.Y1_HousePaymentTypeInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.Y1_HousePaymentTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.Y1_HousePaymentTypeInfo, Parent.Lookups.HousePaymentTypeList);

				if (!Parent.Y1_HousePaymentTypeInfo.HasErrors())
				{
					CheckIsUnique(Parent.Y1_HousePaymentTypeInfo);
				}
			}
		}

		#endregion

		void CheckIsUnique(ZPropertyInfo propertyInfo)
		{
			if (Parent.ChargeCode != null)
			{
				var hasDuplicateSetting = Parent.ChargeCode.GLPostingOverrides.Cast<AccChargeGLPostingOverride>()
													.Any(x => x != Parent &&
														x.Y1_JobType == Parent.Y1_JobType &&
														x.Y1_Direction == Parent.Y1_Direction &&
														x.Y1_TransportMode == Parent.Y1_TransportMode &&
														x.Y1_ConsolContainerMode == Parent.Y1_ConsolContainerMode &&
														x.Y1_MasterPaymentType == Parent.Y1_MasterPaymentType &&
														x.Y1_HousePaymentType == Parent.Y1_HousePaymentType &&
														x.Y1_ConsolidationAccountingCategoryClass == Parent.Y1_ConsolidationAccountingCategoryClass &&
														x.Y1_GE == Parent.Y1_GE);
				if (hasDuplicateSetting)
				{
					Parent.FindPropertyInfo(propertyInfo.Name).AddError(Res.GetString("574b2ae8-50f8-4b5c-a20a-5f4596201901", "At least one more record already sets a behavior for the same Job parameters."));
				}
			}
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			if (info.Name == AccChargeGLPostingOverrideSchema.Y1_AC.Name)
			{
				return false;
			}

			return base.ShouldValidateFKToCancelledRecord(info);
		}

		readonly MultilingualString messageWhenWrongAccountSelected = ResString.GetMultilingualString("defbae7f-c5c3-4835-aa0a-182fe686ac52", @"Please choose a different account.
Only P&L or non-control BSH accounts can be selected.");

		readonly IMultilingualString clrAccErrMsg = ResString.GetMultilingualString("7F7D4B2A-34BE-473A-B64D-D9205631BD32", "Clearing Account must be a Balance Sheet account and must allow Direct Posting.");
	}
}
