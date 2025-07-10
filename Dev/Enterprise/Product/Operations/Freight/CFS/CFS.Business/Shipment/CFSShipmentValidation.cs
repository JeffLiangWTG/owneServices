using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSShipmentValidation : CommonShipmentValidation
	{
		public CFSShipmentValidation(CFSShipment parent)
			: base(parent)
		{
		}

		#region ValidateJS_PackingMode

		protected override void CheckJS_PackingMode()
		{
		}

		#endregion

		#region ValidateJS_TranshipToOtherCFS

		protected override void CheckJS_TranshipToOtherCFS()
		{
			base.CheckJS_TranshipToOtherCFS();

			if (!Parent.JS_TranshipToOtherCFSInfo.HasErrors() && Parent.Consols.Count > 0 && Parent.IsExport() && Parent.JS_TranshipToOtherCFS)
			{
				Parent.JS_TranshipToOtherCFSInfo.AddError(Res.GetString("aa26307d-2f21-43bd-afbc-4680edd828cc", "Export shipments cannot be both gate passed and attached to a load list."));
			}
			else if (Parent.JS_TranshipToOtherCFS && Parent.IsCrossTrade())
			{
				foreach (CFSLoadListConsol consol in Parent.Consols)
				{
					if (consol.IsExport())
					{
						Parent.JS_TranshipToOtherCFSInfo.AddError(Res.GetString("b852aac1-a9fe-44e5-af1d-fa878eedd50f", "Shipments cannot be both gate passed and attached to export load lists."));
						break;
					}
				}
			}
		}

		#endregion

		#region ValidateJS_OH_HandledOnBehalfOfForwarder

		protected override void CheckJS_OH_HandledOnBehalfOfForwarder()
		{
			base.CheckJS_OH_HandledOnBehalfOfForwarder();

			if (!Parent.JS_OH_HandledOnBehalfOfForwarderInfo.OriginalValue.IsEmpty && Parent.JS_OH_HandledOnBehalfOfForwarder.IsEmpty)
			{
				Parent.JS_OH_HandledOnBehalfOfForwarderInfo.AddError(Res.GetString("ac94287f-99c2-48a1-b6a3-a58b8eaf3528", "Please enter the Client."));
			}
			else
			{
				MandatoryValidation.WarnIfNotEntered(Parent.JS_OH_HandledOnBehalfOfForwarderInfo);
			}
		}

		#endregion

		#region ValidateJS_JX

		protected override void CheckJS_JX()
		{
			base.CheckJS_JX();
			MandatoryValidation.WarnIfNotEntered(Parent.JS_JXInfo);
			if (Parent.JS_JXInfo.HasWarnings() && !Parent.JS_Calc_CurrentVesselInfo.HasWarnings())
			{
				ValidateJS_Calc_CurrentDischargePort();
				ValidateJS_Calc_CurrentVessel();
			}
			else if (!Parent.JS_JXInfo.HasWarnings())
			{
				ValidateJS_Calc_CurrentVessel();
			}
		}

		#endregion

		#region ValidateJS_Calc_CurrentDischargePort

		public void ValidateJS_Calc_CurrentDischargePort()
		{
			ValidateCalculatedProperty(Parent.JS_Calc_CurrentDischargePortInfo);
		}

		protected virtual void CheckJS_Calc_CurrentDischargePort()
		{
		}

		#endregion

		#region ValidateJS_Calc_CurrentVessel

		public void ValidateJS_Calc_CurrentVessel()
		{
			ValidateCalculatedProperty(Parent.JS_Calc_CurrentVesselInfo);
		}

		protected virtual void CheckJS_Calc_CurrentVessel()
		{
			if (Parent.JS_JXInfo.HasWarnings() && !Parent.JS_Calc_CurrentVesselInfo.HasWarnings())
			{
				Parent.JS_Calc_CurrentVesselInfo.AddWarning(Res.GetString("332fbd6f-d944-43f8-985d-0e093f3249d4", "Please select a sailing."));
			}
		}

		#endregion

		#region ValidateJS_OuterPacks

		protected override void CheckJS_OuterPacks()
		{
			base.CheckJS_OuterPacks();

			if (!Parent.JS_OuterPacksInfo.HasNotifications())
			{
				if (Parent.JS_OuterPacks != Parent.TotalOuterPacks)
				{
					Parent.JS_OuterPacksInfo.AddWarning(Res.GetString("217defcf-d037-4b67-bc5a-9e0964ee2646", "Entered number of packs does not equal the total number in the Pack Lines."));
				}
				else if (Parent.IsInDatabase && Parent.IsPacked && Parent.JS_OuterPacksInfo.HasChanges && Parent.JS_OuterPacks != (ZInt)Parent.JS_OuterPacksInfo.OriginalValue)
				{
					Parent.JS_OuterPacksInfo.AddWarning(Res.GetString("0e9e582c-6ac8-42b7-a9e2-8304cbbac7df", "Quantities on a packed shipment should not be changed"));
				}
			}
		}

		#endregion

		#region ValidateJS_F3_NKPackType

		protected override void CheckJS_F3_NKPackType()
		{
			base.CheckJS_F3_NKPackType();
			if (!Parent.JS_F3_NKPackTypeInfo.HasNotifications())
			{
				if (Parent.IsInDatabase && Parent.IsPacked && Parent.JS_F3_NKPackTypeInfo.HasChanges && Parent.JS_F3_NKPackType != (ZString)Parent.JS_F3_NKPackTypeInfo.OriginalValue)
				{
					Parent.JS_F3_NKPackTypeInfo.AddWarning(Res.GetString("0e9e582c-6ac8-42b7-a9e2-8304cbbac7df", "Quantities on a packed shipment should not be changed"));
				}
			}
		}

		#endregion

		#region ValidateJS_ActualWeight

		protected override void CheckJS_ActualWeight()
		{
			base.CheckJS_ActualWeight();

			if (!Parent.JS_ActualWeightInfo.HasNotifications())
			{
				if (Parent.JS_ActualWeight != Parent.TotalOuterPacksWeight)
				{
					Parent.JS_ActualWeightInfo.AddWarning(Res.GetString("0413d334-f369-496b-a700-b02507b6985c", "Entered weight does not match total weight of the packlines."));
				}
				else if (Parent.IsInDatabase && Parent.IsPacked && Parent.JS_ActualWeightInfo.HasChanges && Parent.JS_ActualWeight != (ZDecimal)Parent.JS_ActualWeightInfo.OriginalValue)
				{
					Parent.JS_ActualWeightInfo.AddWarning(Res.GetString("0e9e582c-6ac8-42b7-a9e2-8304cbbac7df", "Quantities on a packed shipment should not be changed"));
				}
			}
		}

		#endregion

		#region ValidateJS_UnitOfWeight

		protected override void CheckJS_UnitOfWeight()
		{
			base.CheckJS_UnitOfWeight();
			if (!Parent.JS_UnitOfWeightInfo.HasNotifications())
			{
				if (Parent.IsInDatabase && Parent.IsPacked && Parent.JS_UnitOfWeightInfo.HasChanges && Parent.JS_UnitOfWeight != (ZString)Parent.JS_UnitOfWeightInfo.OriginalValue)
				{
					Parent.JS_UnitOfWeightInfo.AddWarning(Res.GetString("0e9e582c-6ac8-42b7-a9e2-8304cbbac7df", "Quantities on a packed shipment should not be changed"));
				}
			}
		}

		#endregion

		#region ValidateJS_ActualVolume

		protected override void CheckJS_ActualVolume()
		{
			base.CheckJS_ActualVolume();
			if (!Parent.JS_ActualVolumeInfo.HasNotifications())
			{
				if (Parent.JS_ActualVolume != Enterprise.ZArchitecture.Core.Utilities.Round(Parent.TotalOuterPacksVolume, JobPackLinesSchema.JL_ActualVolume.Scale))
				{
					Parent.JS_ActualVolumeInfo.AddWarning(Res.GetString("29bff843-0db2-4a6c-a8de-17e264bba31d", "Entered volume does not match total volume of the packlines."));
				}
				else if (Parent.IsInDatabase && Parent.IsPacked && Parent.JS_ActualVolumeInfo.HasChanges && Parent.JS_ActualVolume != (ZDecimal)Parent.JS_ActualVolumeInfo.OriginalValue)
				{
					Parent.JS_ActualVolumeInfo.AddWarning(Res.GetString("0e9e582c-6ac8-42b7-a9e2-8304cbbac7df", "Quantities on a packed shipment should not be changed"));
				}
			}
		}

		#endregion

		#region ValidateJS_UnitOfVolume

		protected override void CheckJS_UnitOfVolume()
		{
			base.CheckJS_UnitOfVolume();
			if (!Parent.JS_UnitOfVolumeInfo.HasNotifications())
			{
				if (Parent.IsInDatabase && Parent.IsPacked && Parent.JS_UnitOfVolumeInfo.HasChanges && Parent.JS_UnitOfVolume != (ZString)Parent.JS_UnitOfVolumeInfo.OriginalValue)
				{
					Parent.JS_UnitOfVolumeInfo.AddWarning(Res.GetString("0e9e582c-6ac8-42b7-a9e2-8304cbbac7df", "Quantities on a packed shipment should not be changed"));
				}
			}
		}

		#endregion

		#region ValidateJS_A_RCV

		protected override void CheckJS_A_RCV()
		{
			TypeValidation.CheckValidZDateTimeAndRange(Parent.JS_A_RCVInfo);
			if (!Parent.JS_A_RCVInfo.HasNotifications())
			{
				foreach (CFSLoadListConsol loadList in Parent.Consols)
				{
					if (loadList.IsPackLoadList && !loadList.JK_JX_JA_E_DEP.IsEmpty && Parent.JS_A_RCV > loadList.JK_JX_JA_E_DEP)
					{
						Parent.JS_A_RCVInfo.AddWarning(Res.GetString("4cbabe95-905e-4449-976b-5dbb58adfb65", "Warehouse receival date is later than the ETD of the export Load List."));
						break;
					}
				}
			}
		}

		#endregion

		#region ValidateJS_OA_CartageCoAddr

		public void ValidateJS_OA_CartageCoAddr()
		{
			ValidateCalculatedProperty(Parent.JS_OA_CartageCoAddrInfo);
		}

		protected virtual void CheckJS_OA_CartageCoAddr()
		{
			TypeValidation.CheckValidGuid(Parent.JS_OA_CartageCoAddrInfo);
			//ListValidation.ErrorIfInvalidPK(JS_OA_CartageCoAddrInfo, LocalTransport_List);
		}

		#endregion

		#region Validate JS_RL_NKOrigin JS_RL_NKDestination

		protected override void CheckJS_RL_NKOrigin()
		{
			base.CheckJS_RL_NKOrigin();

			if (ShouldRunAdditionalValidationOnOrigin && ShouldAddErrorToOriginAndDestination)
			{
				string errorMessage = Res.GetString("6497338a-dc9b-475b-8426-a48c66b72817", "Either or Origin or Destination must be set");
				if (!Parent.JS_RL_NKOriginInfo.HasError(errorMessage))
				{
					Parent.JS_RL_NKOriginInfo.AddError(errorMessage);
				}

				ValidateJS_RL_NKDestination();
			}
		}

		protected override void CheckJS_RL_NKDestination()
		{
			base.CheckJS_RL_NKDestination();

			if (ShouldRunAdditionalValidationOnOrigin && ShouldAddErrorToOriginAndDestination)
			{
				string errorMessage = Res.GetString("6497338a-dc9b-475b-8426-a48c66b72817", "Either or Origin or Destination must be set");
				if (!Parent.JS_RL_NKDestinationInfo.HasError(errorMessage))
				{
					Parent.JS_RL_NKDestinationInfo.AddError(errorMessage);
				}

				ValidateJS_RL_NKOrigin();
			}
		}

		bool ShouldAddErrorToOriginAndDestination
		{
			get
			{
				if (Parent.JS_RL_NKDestination.IsEmpty && Parent.JS_RL_NKOrigin.IsEmpty)
				{
					return true;
				}
				return false;
			}
		}

		#endregion

		#region CheckNoOriginalBillsCount

		protected override void CheckNoOriginalBillsCount()
		{
		}

		#endregion

		#region CheckNoCopyBillsCount

		protected override void CheckNoCopyBillsCount()
		{
		}

		#endregion

		protected override ZQuery GetHouseBillDuplicateCheckAdditionalConditions()
		{
			return new ZQuery(JobShipmentSchema.JS_IsCFSRegistered, true);
		}

		#region Implementation

		public new CFSShipment Parent
		{
			get { return (CFSShipment)base.Parent; }
		}

		#endregion
	}
}
