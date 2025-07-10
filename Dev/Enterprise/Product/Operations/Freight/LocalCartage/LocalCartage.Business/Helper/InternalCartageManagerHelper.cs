using System;
using System.Collections.Generic;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using EventReferenceCodes = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.LocalCartage.Business
{
	public static class InternalCartageManagerHelper
	{
		public static void PopulateCartage(CommonCartage cartage, CartageType cartageTypeInOtherFactory)
		{
			using (cartage.SuspendBehavior())
			{
				PopulateCartageDetails(cartage, cartageTypeInOtherFactory);
				cartage.BehaviorStrategy.RefreshDocAddresses(cartage);
				cartage.SetCartageDropModeWithOutSettingMoves();
				PopulateLooseOrContainerisedCartage(cartage, cartageTypeInOtherFactory);
				PopulateCartageDates(cartage, cartageTypeInOtherFactory);
			}
		}

		static void PopulateCartageDetails(CommonCartage cartage, CartageType cartageType)
		{
			cartage.JJ_ParentID = cartageType.CartageParent.CartageParentID;
			cartage.JJ_ParentTableCode = cartageType.CartageParent.CartageParentTableCode;
			cartage.JJ_GB = cartageType.CartageParent.BranchPK.IsValid ? cartageType.CartageParent.BranchPK : GlbBranch.CurrentBranch.PK;

			cartage.JJ_E3_NKJobType = cartageType.CartageJobType;
			cartage.DefaultModesFromJobTypeTemplate();

			cartage.JJ_RS_NKServiceLevel = cartageType.CartageParent.ServiceLevel;
			cartage.JJ_OrderReferenceNumber = cartageType.CartageParent.OrderReferenceNumber.SubstringSafe(0, cartage.JJ_OrderReferenceNumberInfo.MaxLength);
			cartage.JJ_WaybillNumber = cartageType.CartageParent.WayBillNumber.SubstringSafe(0, JobCartageSchema.JJ_WaybillNumber.MaxLength);
			cartage.JJ_GoodsDescription = cartageType.CartageParent.GoodsDescription.SubstringSafe(0, JobCartageSchema.JJ_GoodsDescription.MaxLength);
		}

		static void PopulateLooseOrContainerisedCartage(CommonCartage cartage, CartageType cartageType)
		{
			if (cartageType != null)
			{
				if (cartage.IsContainerised)
				{
					foreach (ICartageContainer iContainer in cartageType.CartageContainers)
					{
						CommonContainer jContainer;

						if (!iContainer.JobContainerPK.IsEmpty)
						{
							jContainer = cartage.Factory.Load<CommonContainer>(iContainer.JobContainerPK);
							if (jContainer == null)
							{
								throw new InvalidOperationException("iContainer is a JobContainer, but can not load it.");
							}

							var move = cartage.Factory.New<CommonBookedCtgMove>();
							move.EW_JC_Container = jContainer.PK;
							cartage.ContainerBookedMoves.Add(move);
							move.EW_BookedPackCount = jContainer.JC_Calc_TotalPackages;
							move.EW_F3_NKPackType = jContainer.JC_Calc_TotalPackagesUnit;
						}
						else
						{
							var move = cartage.ContainerBookedMoves.AddNew();
							jContainer = move.Container;
							jContainer.JC_ContainerNum = iContainer.ContainerNumber;
							jContainer.JC_ContainerMode = iContainer.ContainerMode;
							jContainer.JC_RC = iContainer.ContainerRC;
							jContainer.JC_SealNum = iContainer.Seal;
							jContainer.JC_Calc_NetWeight = iContainer.NetWeight;
						}

						if (iContainer.LooseCargo.Count > 0)
						{
							foreach (ICartageLooseCargo looseCargo in iContainer.LooseCargo)
							{
								foreach (UNDGDataItem dgItem in looseCargo.DangerousGoods)
								{
									foreach (CommonBookedCtgMove move in cartage.GetBookedMoves(jContainer))
									{
										if (move.IsTransportingGoods)
										{
											UNDGDataItem moveDGItem = move.UNDGs.AddNew();
											moveDGItem.DI_DG = dgItem.DI_DG;
											moveDGItem.DI_DGFlashPoint = dgItem.DI_DGFlashPoint;
											moveDGItem.DI_OC_DGContact = dgItem.DI_OC_DGContact;
										}
									}
								}
							}
						}
					}
				}

				if (cartage.IsLoose)
				{
					foreach (ICartageLooseCargo iLoose in cartageType.CartageLooseCargo)
					{
						CommonBookedCtgMove looseMove = cartage.LooseBookedMoves.AddNew();
						looseMove.EW_BookedPackCount = iLoose.BookedPackages;
						looseMove.EW_F3_NKPackType = iLoose.BookedPackType;
						looseMove.EW_BookedWeight = iLoose.BookedWeight;
						looseMove.EW_WeightUQ = iLoose.BookedWeightUnit;
						looseMove.EW_BookedVolume = iLoose.BookedVolume;
						looseMove.EW_VolumeUQ = iLoose.BookedVolumeUnit;

						if (!iLoose.BookedDimensionUnit.IsEmpty)
						{
							looseMove.EW_BookedHeight = iLoose.BookedHeight;
							looseMove.EW_BookedLength = iLoose.BookedLength;
							looseMove.EW_BookedWidth = iLoose.BookedWidth;
							looseMove.EW_DimUnit = iLoose.BookedDimensionUnit;
						}

						foreach (UNDGDataItem dgItem in iLoose.DangerousGoods)
						{
							UNDGDataItem moveDGItem = looseMove.UNDGs.AddNew();
							moveDGItem.DI_DG = dgItem.DI_DG;
							moveDGItem.DI_DGFlashPoint = dgItem.DI_DGFlashPoint;
							moveDGItem.DI_OC_DGContact = dgItem.DI_OC_DGContact;
						}
					}
				}

				if (cartageType.CartageParent.UseJobTotals)
				{
					try
					{
						using (cartage.GetValidationSuspender())
						{
							cartage.JJ_OuterPacks = cartageType.CartageParent.TotalPackages;
							cartage.JJ_F3_NKPackType = cartageType.CartageParent.TotalPackType;
							cartage.JJ_Weight = cartageType.CartageParent.TotalWeight;
							cartage.JJ_WeightUQ = cartageType.CartageParent.TotalWeightUnit;
							cartage.JJ_Volume = cartageType.CartageParent.TotalVolume;
							cartage.JJ_VolumeUQ = cartageType.CartageParent.TotalVolumeUnit;
						}
					}
					finally
					{
						cartage.Validation.ValidateAll();
					}
				}
				else
				{
					cartage.UpdateCartageFromLooseBookedMovesOrContainers();
				}
			}
		}

		static void PopulateCartageDates(CommonCartage cartage, CartageType cartageType)
		{
			cartage.JJ_EstimatedPickup = cartageType.EstimatedCartagePickup;
			cartage.JJ_EstimatedDelivery = cartageType.EstimatedCartageDelivery;
		}

		public static void LogCartageCreationOnParent(CommonCartage cartage)
		{
			LogCartageDataInParent(Events.InternalCartageJobCreated, cartage);
		}

		public static void LogCartageDeactivationOnParent(CommonCartage cartage)
		{
			LogCartageDataInParent(Events.InternalCartageJobDeactivated, cartage);
		}

		static void LogCartageDataInParent(Event @event, CommonCartage cartage)
		{
			var parent = cartage.CartageParent;
			if (parent != null)
			{
				((IStmALogParent)parent).Logs.AddNew(
					@event,
					new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>(EventReferenceCodes.Type, cartage.JJ_Direction),
						new KeyValuePair<string, string>(EventReferenceCodes.JobNumber, cartage.JJ_ConsignmentID),
						new KeyValuePair<string, string>(EventReferenceCodes.Name, cartage.LocalTransportProviderAddress?.EffectiveCompanyName ?? string.Empty),
					});
			}
		}

		public static void Refresh(CommonCartage cartage, CartageType cartageType)
		{
			cartage.LooseBookedMoves.DeleteAll();
			cartage.ContainerBookedMoves.DeleteAll();

			PopulateLooseOrContainerisedCartage(cartage, cartageType);
		}
	}
}
