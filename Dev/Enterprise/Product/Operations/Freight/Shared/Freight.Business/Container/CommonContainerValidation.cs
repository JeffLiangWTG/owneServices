using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static CargoWise.EventReference.Constants.EventReferenceParameters;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Business
{
	public class CommonContainerValidation : JobContainerValidation
	{
		public CommonContainerValidation(CommonContainer container)
			: base(container)
		{
		}

		protected static string NotRefridgerationError
		{
			get { return Res.GetString("0ea0a938-15ca-4f01-8b00-032c342a62fe", "This type of container cannot have a controlled atmosphere."); }
		}

		#region JC_ContainerMode

		protected override void CheckJC_ContainerMode()
		{
			base.CheckJC_ContainerMode();
			if (Parent.IsConsolContainer)
			{
				MandatoryValidation.CheckEntered(Parent.JC_ContainerModeInfo);
			}

			if (!Parent.JC_ContainerModeInfo.HasErrors() && Parent.IsConsolContainer)
			{
				if ((!IsFcxContainterModeAllowed || Parent.JC_ContainerMode != "FCX") && !SkipContainerModeListCheck)
				{
					ListValidation.ErrorIfInvalidCode(Parent.JC_ContainerModeInfo, Parent.JC_ContainerMode_List);
				}

				if (!Parent.JC_ContainerModeInfo.HasErrors() && Parent.Consol != null)
				{
					if (!ContainerModeMatchesConsolContainerMode(Parent.Consol.JK_TransportMode, Parent.Consol.JK_ConsolMode, Parent.JC_ContainerMode))
					{
						Parent.JC_ContainerModeInfo.AddWarning(Res.GetString("1593a6ec-8242-4b5d-994e-6ec70972c1dd", "Container Mode does not match Container Mode of Consol"));
					}
				}
			}
		}

		protected virtual bool IsFcxContainterModeAllowed
		{
			get { return true; }
		}

		protected bool SkipContainerModeListCheck
		{
			get { return Parent.IsInDatabase && Parent.JC_ContainerMode == Constants.ContainerModes.AIR && !Parent.JC_ContainerModeInfo.HasChanges; }
		}

		protected bool ContainerModeMatchesConsolContainerMode(ZString transportMode, ZString consolMode, ZString containerMode)
		{
			bool result = true;
			if (transportMode == Constants.TransportModes.Air)
			{
				if (consolMode == Constants.ContainerModes.ULD)
				{
					result = containerMode == consolMode;
				}
			}
			else
			{
				if (consolMode != Constants.ContainerModes.Bulk &&
					consolMode != Constants.ContainerModes.Liquid &&
					consolMode != Constants.ContainerModes.BreakBulk &&
					consolMode != Constants.ContainerModes.Other)
				{
					result = consolMode == containerMode;
				}
			}

			return result;
		}

		#endregion

		#region CheckJC_ContainerNum

		protected override void CheckJC_ContainerNum()
		{
			if (!FactoryCacheHelper.GetIsViewingFromPortTransportLegPlanner(Parent.Factory))   //Container # is readonly in LegPlanner
			{
				base.CheckJC_ContainerNum();

				if (Parent.JC_ContainerNum.Length > 12)
				{
					Parent.JC_ContainerNumInfo.AddError(Res.GetString("c01fb76c-9878-4fb5-b031-e43197fefe1f", "The container number can be no more than 12 characters long"));
				}

				if (!Parent.JC_ContainerNumInfo.OriginalValue.IsEmpty
					&& Parent.JC_ContainerNum.IsEmpty
					&& !Parent.JC_JSB_SupplierBookingInfo.Value.IsEmpty
					&& Parent.IsAttachedToContainerLoadList())
				{
					Parent.JC_ContainerNumInfo.AddError(Res.GetString("e0b1adfc-83dc-42c8-9766-bffc0efa239b", "The container number cannot be set to empty if a container number exists and the container is associated with a Supplier Booking and the container has 'Container Load List Lines'."));
				}

				if (!Parent.JC_ContainerNumInfo.HasErrors())
				{
					if (Parent.JC_ContainerCount == 1)
					{
						if (!Parent.JC_ContainerNumInfo.HasErrors())
						{
							CheckContainerNumberAgainstRelatedContainers();

							if (!Parent.JC_ContainerNumInfo.HasErrors())
							{
								if (Parent.JC_ContainerMode != Constants.ContainerModes.AIR && Parent.JC_ContainerMode != Constants.ContainerModes.ULD)
								{
									MandatoryValidation.WarnIfNotEntered(Parent.JC_ContainerNumInfo);
									if (Parent.JC_ContainerMode != Constants.ContainerModes.RollOnRollOff)
									{
										ContainerNumberValidation.WarnIfInvalid(Parent.JC_ContainerNumInfo);
									}
								}
								if (Parent.JC_ContainerMode == Constants.ContainerModes.ULD && !ULDRegex.IsMatch(Parent.JC_ContainerNum))
								{
									Parent.JC_ContainerNumInfo.AddWarning(Res.GetString("8884af44-93c2-4316-aa21-054d1be9f751", "This is not a valid ULD number."));
								}
							}
						}
					}
					else if (!Parent.JC_ContainerNum.IsEmpty)
					{
						Parent.JC_ContainerNumInfo.AddError(Res.GetString("57775e0a-2cdf-49df-b7dd-4dc9ea0e80ad", "Container Count should be equal 1 if you have a container number."));
					}
				}
				if (Parent.JC_ContainerCountInfo.HasErrors())
				{
					ValidateJC_ContainerCount();
				}

				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia &&
					Parent.JC_ContainerNumInfo.OriginalValue.ToString() != Parent.JC_ContainerNumInfo.Value.ToString())
				{
					EDIMessage lastPRAMessage = Parent.GetLastPRAMessage();

					if (lastPRAMessage != null)
					{
						EDIMessage lastPRAMessageSent = Parent.GetLastPRAMessageSent();

						if (lastPRAMessage.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
						{
							if (lastPRAMessage.EM_MessageSubType == "SSM")
							{
								Parent.JC_ContainerNumInfo.AddError(CannotChangeContainerWhileWaitingForAReplyMustCancelToo);
							}
							else
							{
								Parent.JC_ContainerNumInfo.AddError(CannotChangeContainerWhileWaitingForAReplyToCancellation);
							}
						}
						else
						{
							if (lastPRAMessageSent.EM_MessageSubType == "SSM" && lastPRAMessage.EM_MessageSubType == "ACK")
							{
								Parent.JC_ContainerNumInfo.AddError(CannotChangeContainerWithoutCancellingPRAFirst);
							}
						}
					}
				}

				if (Parent.JC_ContainerNum.IsEmpty && Parent.IsGrossWeightVerified)
				{
					Parent.JC_ContainerNumInfo.AddError(Res.GetString("c80fd3ef-0914-4be5-96bb-3ba84ba1de6b", "Container number is blank. Container number is required for gross weight verification. Enter the container number or change VGM Verification Method to NON."));
				}

				ValidateIsRemovedByCarrier();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regex")]
		public static Regex ULDRegex
			=> new (@"^(?:[a-zA-Z]){1}(?:[a-zA-Z0-9]){3}\d{3,4}(?:(?!\d{2})([a-zA-Z0-9]){2})$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		#endregion

		#region ValidateIsRemovedByCarrier

		public void ValidateIsRemovedByCarrier()
		{
			if (Parent.IsInDatabase)
			{
				foreach (var parent in new[] { Parent.Consol, Parent.Declaration }.Cast<EnterpriseBusinessObject>().Where(x => x != null && !x.IsDeleted))
				{
					var removalLogsExist = parent.Logs.GetAllLogs().Cast<StmALog>().Any(
						log => log.SL_SE_NKEvent == Events.StatusUpdatedCode
							&& log.Parameters.TryGetValue(Codes.EquipmentReferenceNumber, out var referenceNumber)
							&& log.Parameters.TryGetValue(Codes.Department, out var department)
							&& log.Parameters.TryGetValue(Codes.Type, out var eventType)
							&& referenceNumber.Equals(Parent.JC_ContainerNum)
							&& department.Equals((NoResString)"Carrier")
							&& eventType.Equals((NoResString)"Container Number removed from the Booking"));

					if (removalLogsExist)
					{
						Parent.AddRowWarning(Res.GetString("1a778f27-89fe-4e0e-8795-239b31192bdf", "Carrier removed this container from the booking."));
					}
				}
			}
		}

		#endregion

		#region CheckContainerNumberAgainstRelatedContainers

		protected virtual void CheckContainerNumberAgainstRelatedContainers()
		{
			if (Parent.Consol != null && !Parent.Consol.IsDeleted)
			{
				foreach (CommonContainer container in Parent.Consol.Containers)
				{
					if (!container.JC_ContainerNum.IsEmpty && container.PK != Parent.PK && Parent.JC_ContainerNum == container.JC_ContainerNum)
					{
						Parent.JC_ContainerNumInfo.AddError(Res.GetString("5ce9289e-7c54-4d23-924a-a2ff306e18d6", "Duplicate Container Number is entered."));
						break;
					}
				}
			}

			if (Parent.Sailing != null && !Parent.Sailing.IsDeleted)
			{
				foreach (CommonContainer container in Parent.Sailing.Containers)
				{
					if (!container.JC_ContainerNum.IsEmpty && container != Parent && Parent.JC_ContainerNum == container.JC_ContainerNum)
					{
						Parent.JC_ContainerNumInfo.AddError(Res.GetString("5ce9289e-7c54-4d23-924a-a2ff306e18d6", "Duplicate Container Number is entered."));
						break;
					}
				}
			}
		}

		#endregion

		#region JC_RC

		protected override void CheckJC_RC()
		{
			base.CheckJC_RC();

			if (Parent.StandAloneCustomsContainer && Parent.JC_RC.IsEmpty)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.JC_RCInfo);
			}
			else if (!Parent.JC_RCInfo.HasErrors())
			{
				if (Parent.JC_RCInfo.HasChanges && HasNonEditableSupplierBooking())
				{
					Parent.JC_RCInfo.AddError(ContainerAlreadyAllocatedErrorMessage);
				}
				else if (Parent.IsMandatoryContainerType)
				{
					MandatoryValidation.CheckEntered(Parent.JC_RCInfo);
				}
			}

			if (Parent.Container != null)
			{
				if (Parent.IsAirContainer && !Parent.Container.IsAirContainer)
				{
					Parent.JC_RCInfo.AddError(Res.GetString("58fcee40-e1a0-4ba8-af1c-0d9437f2fd43", "Your container mode is Air, so you must choose an Air ULD container."));
				}
				if (Parent.IsSeaContainer && !Parent.Container.IsSeaContainer && !Parent.Container.IsRoadTruckContainer)
				{
					Parent.JC_RCInfo.AddError(Res.GetString("374f613b-22c9-4a7e-821d-8671b0aa4502", "Your container mode is Sea, so you must choose a Sea FCL container."));
				}
			}

			ValidateIsChiller();
			ValidateIsFreezer();
			ValidateJC_HumidityPercent();
			ValidateJC_AirVentFlow();
			ValidateJC_TempRecorderSerialNo();
			ValidateJC_SetPointTemp();
		}

		#endregion

		#region JC_ContainerCount

		protected override void CheckJC_ContainerCount()
		{
			base.CheckJC_ContainerCount();
			ValidateContainerCount();
		}

		#region ValidateContainerCount

		protected void ValidateContainerCount()
		{
			base.ValidateJC_ContainerCount();
			CompareValidation.CheckNumberNotNegative(Parent.JC_ContainerCountInfo);
			if (!Parent.JC_ContainerCountInfo.HasErrors())
			{
				if (Parent.JC_ContainerCount == 0 && Parent.JC_ContainerNum.IsEmpty)
				{
					Parent.JC_ContainerCountInfo.AddError(Res.GetString("a06eb8ec-144f-4614-affe-1e2d7dd1c3b7", "Please enter a container count or a container number."));
				}
				else if (Parent.JC_ContainerCount != 1 && !Parent.JC_ContainerNum.IsEmpty)
				{
					Parent.JC_ContainerCountInfo.AddError(Res.GetString("eb9095bc-c899-45ae-a6ef-ad52505dd8dd", "You cannot have a container number if the container count is not 1."));
				}
				else if (Parent.JC_ContainerCount < 0 && Parent.JC_ContainerNum.IsEmpty)
				{
					Parent.JC_ContainerCountInfo.AddError(Res.GetString("21420c6e-acc8-4575-b2aa-6f14c72dbe7b", "Please enter a container count > 0"));
				}
				else if (Parent.JC_ContainerCountInfo.HasChanges && HasNonEditableSupplierBooking())
				{
					Parent.JC_ContainerCountInfo.AddError(ContainerAlreadyAllocatedErrorMessage);
				}
			}
			if (Parent.JC_ContainerNumInfo.HasErrors())
			{
				ValidateJC_ContainerNum();
			}
		}

		#endregion

		#endregion

		#region JC_HumidityPercent

		protected override void CheckJC_HumidityPercent()
		{
			base.CheckJC_HumidityPercent();

			if (Parent.JC_HumidityPercent < 0 || Parent.JC_HumidityPercent > 100)
			{
				Parent.JC_HumidityPercentInfo.AddError(Res.GetString("d45068b8-e790-4b5c-9a1f-62795af8d0cf", "Please enter a valid percent."));
			}

			if (!Parent.JC_IsRefrigerated && Parent.JC_HumidityPercent != 0m)
			{
				Parent.JC_HumidityPercentInfo.AddError(NotRefridgerationError);
			}
		}

		#endregion

		#region JC_AirVentFlow

		protected override void CheckJC_AirVentFlow()
		{
			base.CheckJC_AirVentFlow();
			if (!Parent.JC_IsRefrigerated && Parent.JC_AirVentFlow != 0m)
			{
				Parent.JC_AirVentFlowInfo.AddError(NotRefridgerationError);
			}
		}

		#endregion

		#region JC_AirVentFlowUnit

		protected override void CheckJC_AirVentFlowRateUnit()
		{
			base.CheckJC_AirVentFlowRateUnit();
			if (Parent.JC_AirVentFlow != 0m)
			{
				MandatoryValidation.CheckEntered(Parent.JC_AirVentFlowRateUnitInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.JC_AirVentFlowRateUnitInfo, Parent.BindToLists.AirVentFlowRateUnits);
		}

		#endregion

		#region JC_TempRecorderSerialNo

		protected override void CheckJC_TempRecorderSerialNo()
		{
			base.CheckJC_TempRecorderSerialNo();
			if (!Parent.JC_IsRefrigerated && !Parent.JC_TempRecorderSerialNo.IsEmpty)
			{
				Parent.JC_TempRecorderSerialNoInfo.AddError(NotRefridgerationError);
			}
		}

		#endregion

		#region JC_SetPointTemp

		protected override void CheckJC_SetPointTemp()
		{
			base.CheckJC_SetPointTemp();

			if (!Parent.JC_IsRefrigerated && Parent.JC_SetPointTemp != 0m)
			{
				Parent.JC_SetPointTempInfo.AddError(NotRefridgerationError);
			}

			if (!Parent.JC_IsNonOperativeReefer && Parent.JC_IsRefrigerated
				&& (Parent.JC_SetPointTempUnit == Constants.Temperature.Centigrade
				|| Parent.JC_SetPointTempUnit == Constants.Temperature.Fahrenheit))
			{
				if (ConsolRequiresTempControl()
					&& (Parent.Consol.JK_RequiredTemperatureUnit == Constants.Temperature.Centigrade
					|| Parent.Consol.JK_RequiredTemperatureUnit == Constants.Temperature.Fahrenheit))
				{
					var consolMinTemp = decimal.Round(Constants.Temperature.Convert(Parent.Consol.JK_RequiredTemperatureMinimum, Parent.Consol.JK_RequiredTemperatureUnit, Parent.JC_SetPointTempUnit), 1);
					var consolMaxTemp = decimal.Round(Constants.Temperature.Convert(Parent.Consol.JK_RequiredTemperatureMaximum, Parent.Consol.JK_RequiredTemperatureUnit, Parent.JC_SetPointTempUnit), 1);

					if (Parent.JC_SetPointTemp < consolMinTemp || Parent.JC_SetPointTemp > consolMaxTemp)
					{
						Parent.JC_SetPointTempInfo.AddError(Res.GetString("2a6810df-da6c-4539-b758-b1de7288899a",
							"This container is not within the Consolidation Pre-Allocated temperature range of {0}°{2} to {1}°{2}", consolMinTemp, consolMaxTemp, Parent.JC_SetPointTempUnit));
					}
				}

				if (Parent.PackLines.OfType<PackLine>().Any(IsPackLineOutsideContainerTemp))
				{
					Parent.JC_SetPointTempInfo.AddWarning(Res.GetString("a2a7a89f-eefb-43f1-a718-b5938ee72ffa", "This container is not within the range of all Allocated Packlines temperature range"));
				}
			}
		}

		bool IsPackLineOutsideContainerTemp(PackLine packLine)
		{
			if (packLine.JL_RequiresTemperatureControl)
			{
				var containerTemp = decimal.Round(Constants.Temperature.Convert(Parent.JC_SetPointTemp, Parent.JC_SetPointTempUnit, packLine.JL_RequiredTemperatureUnit), 1);

				return containerTemp < packLine.JL_RequiredTemperatureMinimum || containerTemp > packLine.JL_RequiredTemperatureMaximum;
			}

			return false;
		}

		#endregion

		#region JC_SetPointTempUnit

		protected override void CheckJC_SetPointTempUnit()
		{
			base.CheckJC_SetPointTempUnit();

			if (!Parent.JC_IsNonOperativeReefer)
			{
				if (Parent.Container != null && Parent.Container.RC_ContainerType == Constants.ContainerTypes.Refrigerated
					&& Parent.JC_SetPointTemp == 0m && Parent.JC_SetPointTempUnit.IsEmpty)
				{
					Parent.JC_SetPointTempUnitInfo.AddWarning(Res.GetString("e3eb6a367-ea5c-4180-aefc-5ff3f3cc0306",
						"The default temperature has not yet been verified by the user. Please check the temperature and the unit."));
				}

				if (!Parent.JC_SetPointTemp.IsEmpty && Parent.JC_SetPointTempUnit.IsEmpty)
				{
					Parent.JC_SetPointTempUnitInfo.AddError(Res.GetString("e7ade78d-b9c8-4e73-a64a-82b87b1bb71b",
						"Unit must be entered when temperature set point is entered."));
				}
			}
			ListValidation.ErrorIfInvalidCode(Parent.JC_SetPointTempUnitInfo);
		}

		#endregion

		#region IsControlledAtmosphere

		protected override void CheckJC_IsControlledAtmosphere()
		{
			base.CheckJC_IsControlledAtmosphere();
			if (!Parent.JC_IsRefrigerated && Parent.JC_IsControlledAtmosphere)
			{
				Parent.JC_IsControlledAtmosphereInfo.AddError(NotRefridgerationError);
			}

			if (ConsolRequiresTempControl() && !Parent.JC_IsControlledAtmosphere)
			{
				Parent.JC_IsControlledAtmosphereInfo.AddWarning(Res.GetString("e861c600-53f5-42ec-8191-a5103d10daea",
					"Container must have a Controlled Atmosphere as Consolidation is Pre-Allocated as Temperature Controlled"));
			}
		}

		bool ConsolRequiresTempControl() => Parent.Consol?.JK_RequiresTemperatureControl ?? false;

		#endregion

		#region DateTime Range Checking Off

		#region JC_ArrivalCartageAdvised

		protected override void CheckJC_ArrivalCartageAdvisedIsValidZDateTimeRange()
		{
		}

		#endregion

		#region JC_ArrivalCartageComplete

		protected override void CheckJC_ArrivalCartageCompleteIsValidZDateTimeRange()
		{
		}

		#endregion

		#region JC_ArrivalEstimatedDelivery

		protected override void CheckJC_ArrivalEstimatedDeliveryIsValidZDateTimeRange()
		{
		}

		#endregion

		#region JC_ArrivalSlotDateTime

		protected override void CheckJC_ArrivalSlotDateTimeIsValidZDateTimeRange()
		{
		}

		#endregion

		#region JC_DepartureCartageAdvised

		protected override void CheckJC_DepartureCartageAdvisedIsValidZDateTimeRange()
		{
		}

		#endregion

		#region JC_DepartureCartageComplete

		protected override void CheckJC_DepartureCartageCompleteIsValidZDateTimeRange()
		{
		}

		#endregion

		#region JC_DepartureEstimatedPickup

		protected override void CheckJC_DepartureEstimatedPickupIsValidZDateTimeRange()
		{
		}

		#endregion

		#region JC_DepartureSlotDateTime

		protected override void CheckJC_DepartureSlotDateTimeIsValidZDateTimeRange()
		{
		}

		#endregion

		#region JC_ContainerYardEmptyReturnGateIn

		protected override void CheckJC_ContainerYardEmptyReturnGateInIsValidZDateTimeRange()
		{
		}

		#endregion

		#region JC_EmptyReadyForReturn

		protected override void CheckJC_EmptyReadyForReturnIsValidZDateTimeRange()
		{
		}

		#endregion

		#region JC_EmptyRequired

		protected override void CheckJC_EmptyRequiredIsValidZDateTimeRange()
		{
		}

		#endregion

		#region JC_EmptyReturnedBy

		protected override void CheckJC_EmptyReturnedByIsValidZDateTimeRange()
		{
		}

		protected override void CheckJC_EmptyReturnedBy()
		{
			base.CheckJC_EmptyReturnedBy();

			if (!Parent.JC_EmptyReturnedBy.IsEmpty)
			{
				IContainerDefaultingStrategy strategy = Parent.NewContainerDefaultingStrategy();
				var (requiredBy, _, _) = strategy == null ? (ZDateTime.Empty, ZDateTime.Empty, ZString.Empty) : strategy.CalculateRequiredBy();

				if (!requiredBy.IsEmpty && requiredBy.Date != Parent.JC_EmptyReturnedBy.Date)
				{
					Parent.JC_EmptyReturnedByInfo.AddWarning(Res.GetString("68301722-250c-4ff5-a5d2-2d46b768aa35", "The availability date and applicable detention free days indicate that this should be '{0}'.", requiredBy.ToShortDateString()));
				}
			}
		}

		#endregion

		#region JC_LCLAvailable

		protected override void CheckJC_LCLAvailableIsValidZDateTimeRange()
		{
		}

		#endregion

		#region JC_LCLStorageCommences

		protected override void CheckJC_LCLStorageCommencesIsValidZDateTimeRange()
		{
		}

		#endregion

		#region JC_LCLUnpack

		protected override void CheckJC_LCLUnpackIsValidZDateTimeRange()
		{
		}

		#endregion

		#endregion

		#region JC_GrossWeight

		protected override void CheckJC_GrossWeight()
		{
			base.CheckJC_GrossWeight();

			if (Parent.JC_GrossWeight > Parent.JC_Calc_MaxGrossWeight)
			{
				Parent.JC_GrossWeightInfo.AddWarning(Res.GetString("f4865aed-ccb9-4655-b187-c55c8241807c", "The gross weight of this container exceeds the maximum weight recorded for this container."));
			}

			if (Parent.IsGrossWeightVerified && Parent.JC_GrossWeight <= 0)
			{
				Parent.JC_GrossWeightInfo.AddError(Res.GetString("bca8092e-14bc-4225-8370-be4b4f774725", "The Verified Gross Container Weight has to be greater than 0."));
			}
		}

		#endregion

		#region JC_GrossWeightUQ

		protected override void CheckJC_GrossWeightUQ()
		{
			base.CheckJC_GrossWeightUQ();
			if (Parent.JC_GrossWeightUQ.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.JC_GrossWeightUQInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.JC_GrossWeightUQInfo, Parent.TotalWeightUnit_List);
		}

		#endregion

		#region JC_RH_NKContainerCommodityCode

		protected override void CheckJC_RH_NKContainerCommodityCode()
		{
			base.CheckJC_RH_NKContainerCommodityCode();
			ListValidation.ErrorIfInvalidCode(Parent.JC_RH_NKContainerCommodityCodeInfo, Parent.ContainerCommodityCode_List);
		}

		#endregion

		#region JC_ArrivalSlotDateTime

		protected override void CheckJC_ArrivalSlotDateTime()
		{
			base.CheckJC_ArrivalSlotDateTime();
			if (Parent.Consol != null && Parent.Consol.IsImport())
			{
				if (Parent.Sailing != null && Parent.JC_ArrivalSlotDateTime < Parent.Sailing.JX_JB_E_ARV)
				{
					Parent.JC_ArrivalSlotDateTimeInfo.AddWarning(Res.GetString("908dbe12-c3e6-4072-9998-4f7de94b3985", "Arrival slot date time is before estimated arrival date."));
				}
			}
		}

		#endregion

		#region JC_ArrivalSlotReference

		protected override void CheckJC_ArrivalSlotReference()
		{
			base.CheckJC_ArrivalSlotReference();
			if (Parent.Consol != null && Parent.Consol.IsImport()
				&& Parent.JC_ArrivalSlotDateTime.IsValid)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.JC_ArrivalSlotReferenceInfo);
			}
		}

		#endregion

		#region JC_DepartureSlotDatetime

		protected override void CheckJC_DepartureSlotDateTime()
		{
			base.CheckJC_DepartureSlotDateTime();
			if (Parent.Consol != null && Parent.Consol.IsImport())
			{
				if (Parent.Sailing != null && Parent.JC_DepartureSlotDateTime > Parent.Sailing.JX_JA_E_DEP)
				{
					Parent.JC_DepartureSlotDateTimeInfo.AddWarning(Res.GetString("42b5d126-f0e6-4572-a12c-f646546bb338", "Departure slot date time is after estimated departure date."));
				}
			}
		}

		#endregion

		#region JC_DepartureSlotReference

		protected override void CheckJC_DepartureSlotReference()
		{
			base.CheckJC_DepartureSlotReference();
			if (Parent.Consol != null && Parent.Consol.IsExport() && Parent.JC_DepartureSlotDateTime.IsValid)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.JC_DepartureSlotReferenceInfo);
			}
		}

		#endregion

		#region JC_ContainerStatus

		protected override void CheckJC_ContainerStatus()
		{
			base.CheckJC_ContainerStatus();
			ListValidation.ErrorIfInvalidCode(Parent.JC_ContainerStatusInfo);
		}

		#endregion

		#region JC_ContainerQuality

		protected override void CheckJC_ContainerQuality()
		{
			base.CheckJC_ContainerQuality();
			ListValidation.ErrorIfInvalidCode(Parent.JC_ContainerQualityInfo);
		}

		#endregion

		protected override void CheckJC_GrossWeightVerificationType()
		{
			base.CheckJC_GrossWeightVerificationType();

			MandatoryValidation.CheckEntered(Parent.JC_GrossWeightVerificationTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JC_GrossWeightVerificationTypeInfo);

			if (Parent.JC_IsEmptyContainer && Parent.JC_GrossWeightVerificationType == Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages)
			{
				Parent.JC_GrossWeightVerificationTypeInfo.AddError(CannotAllowMethod2PackagesForEmptyContainer);
			}

			if (Parent.JC_GrossWeightVerificationType == Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal)
			{
				var previousFirstSeaPort = Parent.JC_GrossWeightVerificationLoadPort;
				var currentFirstSeaPort = Parent.GetCurrentFirstSeaLoadPort();

				if (!currentFirstSeaPort.IsEmpty && previousFirstSeaPort != currentFirstSeaPort)
				{
					Parent.JC_GrossWeightVerificationTypeInfo.AddError(Res.GetString("9eca3ad5-d469-4ae3-b59e-8871560bf79e", "The Load Port has been changed. Please reselect the Verified Method."));
				}
			}
		}

		public static string CannotAllowMethod2PackagesForEmptyContainer
		{
			get { return Res.GetString("9572A2A0-8E6C-481D-BB2A-AFA424004F79", "Method 2 is not allowed for empty containers."); }
		}

		public static string CannotChangeContainerWhileWaitingForAReplyMustCancelToo
		{
			get { return Res.GetString("ef3f13b7-61d9-43a6-98a8-49ef44a8ca76", "Cannot change container number - You need to wait for the PRA Submission Response Message and send a Cancellation before you can change the container number."); }
		}
		public static string CannotChangeContainerWhileWaitingForAReplyToCancellation
		{
			get { return Res.GetString("617715b9-22e1-413e-b7b7-59d5b6fb9bc4", "Cannot change container number - You need to wait for the PRA Cancellation Response Message before you can change the container number."); }
		}
		public static string CannotChangeContainerWithoutCancellingPRAFirst
		{
			get { return Res.GetString("2f78fe51-abbd-40c7-a7c3-888c77de0c50", "Cannot change container number - You need to send a PRA Cancellation Message and await its acceptance before you can change the container number."); }
		}

		public static string CannotBeNonOperatingReefer
		{
			get { return Res.GetString("34C665A4-17E1-49F3-AEA2-8BC565B17CD2", "The Consolidation is Pre-Allocated as Temperature Controlled; container cannot be Non-Operating Reefer."); }
		}

		#region

		protected override void CheckJC_IsNonOperativeReefer()
		{
			base.CheckJC_IsNonOperativeReefer();
			if (Parent.JC_IsNonOperativeReefer && Parent.Consol != null && Parent.Consol.JK_RequiresTemperatureControl)
			{
				Parent.JC_IsNonOperativeReeferInfo.AddError(CannotBeNonOperatingReefer);
			}
		}

		#endregion

		#region JC_DeliveryMode

		protected override void CheckJC_DeliveryMode()
		{
			base.CheckJC_DeliveryMode();
			ListValidation.ErrorIfInvalidCode(Parent.JC_DeliveryModeInfo, Parent.JC_DeliveryMode_List);
		}

		#endregion

		protected override void CheckJC_VehicleTransmission()
		{
			base.CheckJC_VehicleTransmission();
			ListValidation.ErrorIfInvalidCode(Parent.JC_VehicleTransmissionInfo);
		}

		protected override void CheckJC_RX_NKGoodsCurrency()
		{
			base.CheckJC_RX_NKGoodsCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.JC_RX_NKGoodsCurrencyInfo);
		}

		#region JC_GrossWeightVerificationDateTime

		protected override void CheckJC_GrossWeightVerificationDateTime()
		{
			base.CheckJC_GrossWeightVerificationDateTime();

			if (Parent.IsGrossWeightVerified || Parent.GrossWeightVerificationNotRequired)
			{
				MandatoryValidation.CheckEntered(Parent.JC_GrossWeightVerificationDateTimeInfo);
				if (!Parent.JC_GrossWeightVerificationDateTimeInfo.HasErrors())
				{
					var unlocoDateTime = ZDateTime.Empty;
					var vgmVerifiedByPartyUNLOCO = Parent.GrossWeightVerifiedByAddress?.Organisation?.UNLOCO;

					if (vgmVerifiedByPartyUNLOCO != null)
					{
						var timeZoneSet = vgmVerifiedByPartyUNLOCO.TimeZoneSet;
						if (timeZoneSet != null)
						{
							var calculationTimeZone = timeZoneSet.GetCalculationTimeZone();
							unlocoDateTime = calculationTimeZone.ToLocalTime(ZDateTime.UtcNow.ToDateTime());
						}
					}

					if (!unlocoDateTime.IsEmpty && Parent.JC_GrossWeightVerificationDateTime.ToDateTime() > unlocoDateTime.ToDateTime())
					{
						Parent.JC_GrossWeightVerificationDateTimeInfo.AddError((NoResString)"The Verified Date cannot be a future date.");
					}
				}
			}
			else
			{
				MandatoryValidation.CheckNotEntered(Parent.JC_GrossWeightVerificationDateTimeInfo);
			}
		}

		#endregion

		#region SealParty

		protected override void CheckJC_SealParty()
		{
			base.CheckJC_SealParty();
			ListValidation.ErrorIfInvalidCode(Parent.JC_SealPartyInfo, Parent.Lookups.SealParty_List);
		}

		protected override void CheckJC_AdditionalSealParty()
		{
			base.CheckJC_AdditionalSealParty();
			ListValidation.ErrorIfInvalidCode(Parent.JC_AdditionalSealPartyInfo, Parent.Lookups.SealParty_List);
		}

		protected override void CheckJC_Additional2SealParty()
		{
			base.CheckJC_Additional2SealParty();
			ListValidation.ErrorIfInvalidCode(Parent.JC_Additional2SealPartyInfo, Parent.Lookups.SealParty_List);
		}

		#endregion

		#region SealNum

		protected override void CheckJC_SealNum()
		{
			base.CheckJC_SealNum();
			if (!Parent.JC_SealParty.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.JC_SealNumInfo);
			}
		}

		protected override void CheckJC_AdditionalSealNum()
		{
			base.CheckJC_AdditionalSealNum();
			if (!Parent.JC_AdditionalSealParty.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.JC_AdditionalSealNumInfo);
			}
		}

		protected override void CheckJC_Additional2SealNum()
		{
			base.CheckJC_Additional2SealNum();
			if (!Parent.JC_Additional2SealParty.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.JC_Additional2SealNumInfo);
			}
		}

		#endregion

		#region Calculated

		#region IsChiller

		public void ValidateIsChiller()
		{
			ValidateCalculatedProperty(Parent.IsChillerInfo);
		}

		protected void CheckIsChiller()
		{
			if (!Parent.JC_IsRefrigerated && Parent.IsChiller)
			{
				Parent.IsChillerInfo.AddError(NotRefridgerationError);
			}
		}

		#endregion

		#region IsFreezer

		public void ValidateIsFreezer()
		{
			ValidateCalculatedProperty(Parent.IsFreezerInfo);
		}

		protected void CheckIsFreezer()
		{
			if (!Parent.JC_IsRefrigerated && Parent.IsFreezer)
			{
				Parent.IsFreezerInfo.AddError(NotRefridgerationError);
			}
		}

		#endregion

		#endregion

		#region Spot Rates

		protected override void CheckJC_SellSpotRateMode()
		{
			base.CheckJC_SellSpotRateMode();
			ListValidation.ErrorIfInvalidCode(Parent.JC_SellSpotRateModeInfo);

			if (!IsInPreSaveValidation) //Showing these warnings makes extra DB Hits which is unnecessary in bulk operations
			{
				if (ShipmentHasSpotRate(AutoratedValueType.SpotRate))
				{
					Parent.JC_SellSpotRateModeInfo.AddWarning(Res.GetString("c5b342f9-97d3-4be4-aa5f-a188558896fa", "The One Off Freight Rate cannot be applied to both Shipment and Container"));
				}

				if (ConsolHasOtherContainersWithDifferentSpotRateMode(AutoratedValueType.SpotRate))
				{
					Parent.JC_SellSpotRateModeInfo.AddWarning(Res.GetString("c3696069-231a-47e8-99c9-300125428faa", "The One Off Freight Rate will be ignored as there are other Containers with different One Off Freight Rate Mode on this Consolidation"));
				}
			}
		}

		protected override void CheckJC_RX_NKSellSpotRateCurrency()
		{
			base.CheckJC_RX_NKSellSpotRateCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.JC_RX_NKSellSpotRateCurrencyInfo);
		}

		protected override void CheckJC_CostSpotRateMode()
		{
			base.CheckJC_CostSpotRateMode();
			ListValidation.ErrorIfInvalidCode(Parent.JC_CostSpotRateModeInfo);

			if (!IsInPreSaveValidation) //Showing these warnings makes extra DB Hits which is unnecessary in bulk operations
			{
				if (ShipmentHasSpotRate(AutoratedValueType.NegotiatedCost))
				{
					Parent.JC_CostSpotRateModeInfo.AddWarning(Res.GetString("edaaa408-5397-4e2e-9dc6-c465766777f9", "The Negotiated Cost cannot be applied to both Shipment and Container"));
				}

				if (ConsolHasOtherContainersWithDifferentSpotRateMode(AutoratedValueType.NegotiatedCost))
				{
					Parent.JC_CostSpotRateModeInfo.AddWarning(Res.GetString("1d3800a5-785d-4bbf-b3bb-f0fd761dfa90", "The Negotiated Cost will be ignored as there are other Containers with different Negotiated Cost Mode on this Consolidation"));
				}
			}
		}

		protected override void CheckJC_RX_NKCostSpotRateCurrency()
		{
			base.CheckJC_RX_NKCostSpotRateCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.JC_RX_NKCostSpotRateCurrencyInfo);
		}

		protected override void CheckJC_GatewaySellSpotRateMode()
		{
			base.CheckJC_GatewaySellSpotRateMode();
			ListValidation.ErrorIfInvalidCode(Parent.JC_GatewaySellSpotRateModeInfo);

			if (!IsInPreSaveValidation) //Showing these warnings makes extra DB Hits which is unnecessary in bulk operations
			{
				if (ShipmentHasSpotRate(AutoratedValueType.GatewaySell))
				{
					Parent.JC_GatewaySellSpotRateModeInfo.AddWarning(Res.GetString("bf999fc1-63b6-4243-b247-961e5e7dc71f", "The Gateway Sell Rate cannot be applied to both Shipment and Container"));
				}

				if (!ConsolIsValidGatewayConsol())
				{
					string gatewayAgentIsNotIdentifiedWarning = Res.GetString("1ead78ac-4876-441d-bcbf-c99d75dfe50b",
						@"There is a Gateway Sell amount entered for this container, but no Sending Agent identified as Gateway Agent on related consols.
Please setup the Sending Agent as the Gateway Agent for the transhipment port in Maintain>Reference Files>Organizations>Forwarder>Gateway Agent");

					Parent.JC_GatewaySellSpotRateModeInfo.AddWarning(gatewayAgentIsNotIdentifiedWarning);
				}

				if (ConsolHasOtherContainersWithDifferentSpotRateMode(AutoratedValueType.GatewaySell))
				{
					Parent.JC_GatewaySellSpotRateModeInfo.AddWarning(Res.GetString("59120e66-49f4-4d35-938c-2dfb0bc26fde", "The Gateway Sell Rate will be ignored as there are other Containers with different Gateway Sell Mode on this Consolidation"));
				}
			}
		}

		protected override void CheckJC_RX_NKGatewaySellSpotRateCurrency()
		{
			base.CheckJC_RX_NKGatewaySellSpotRateCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.JC_RX_NKGatewaySellSpotRateCurrencyInfo);
		}

		bool ShipmentHasSpotRate(AutoratedValueType valueType)
		{
			var parentShipments = Parent.GetParentShipments();
			if (!parentShipments.Any())
			{
				return false;
			}

			switch (valueType)
			{
				case AutoratedValueType.SpotRate:
					return Parent.JC_SellSpotRateMode != Constants.FreightRateAutoratingModes.Code.StandardRate && parentShipments.Any(x => x.JS_FreightSpotRateAutoratingMode != Constants.FreightRateAutoratingModes.Code.StandardRate);

				case AutoratedValueType.NegotiatedCost:
					return Parent.JC_CostSpotRateMode != Constants.FreightRateAutoratingModes.Code.StandardRate && parentShipments.Any(x => x.JS_FreightCostRateAutoratingMode != Constants.FreightRateAutoratingModes.Code.StandardRate);

				case AutoratedValueType.GatewaySell:
					return Parent.JC_GatewaySellSpotRateMode != Constants.FreightRateAutoratingModes.Code.StandardRate && parentShipments.Any(x => x.JS_FreightGatewaySellRateAutoratingMode != Constants.FreightRateAutoratingModes.Code.StandardRate);

				default:
					return false;
			}
		}

		bool ConsolHasOtherContainersWithDifferentSpotRateMode(AutoratedValueType valueType)
		{
			switch (valueType)
			{
				case AutoratedValueType.SpotRate:
					return Parent.Consol != null && Parent.Consol.Containers.Cast<CommonContainer>().Any(x => x.PK != Parent.PK && x.JC_SellSpotRateMode != Parent.JC_SellSpotRateMode);

				case AutoratedValueType.NegotiatedCost:
					return Parent.Consol != null && Parent.Consol.Containers.Cast<CommonContainer>().Any(x => x.PK != Parent.PK && x.JC_CostSpotRateMode != Parent.JC_CostSpotRateMode);

				case AutoratedValueType.GatewaySell:
					return Parent.Consol != null && Parent.Consol.Containers.Cast<CommonContainer>().Any(x => x.PK != Parent.PK && x.JC_GatewaySellSpotRateMode != Parent.JC_GatewaySellSpotRateMode);

				default:
					return false;
			}
		}

		bool ConsolIsValidGatewayConsol()
		{
			return Parent.JC_GatewaySellSpotRateMode == Constants.FreightRateAutoratingModes.Code.StandardRate || FreightRatingHelper.IsConsolSendingAgentActingAsGatewayInAnyCompany(Parent.Consol);
		}

		public bool ContainerSpotRateIsValid(AutoratedValueType valueType)
		{
			return !ShipmentHasSpotRate(valueType) && !ConsolHasOtherContainersWithDifferentSpotRateMode(valueType) && !(valueType == AutoratedValueType.GatewaySell && !ConsolIsValidGatewayConsol());
		}

		bool IsInPreSaveValidation
		{
			get { return ((IBusinessObjectInternals)Parent).IsInPreSaveValidation; }
		}

		#endregion

		#region JC_Calc_DepartureContainerYardAddressOrg

		public void ValidateJC_Calc_DepartureContainerYardAddressOrg()
		{
			ValidateCalculatedProperty(Parent.JC_Calc_DepartureContainerYardAddressOrgInfo);
		}

		protected void CheckJC_Calc_DepartureContainerYardAddressOrg()
		{
			TypeValidation.CheckValidGuid(Parent.JC_Calc_DepartureContainerYardAddressOrgInfo);
		}

		#endregion

		#region JC_Calc_ArrivalContainerYardAddressOrg

		public void ValidateJC_Calc_ArrivalContainerYardAddressOrg()
		{
			ValidateCalculatedProperty(Parent.JC_Calc_ArrivalContainerYardAddressOrgInfo);
		}

		protected void CheckJC_Calc_ArrivalContainerYardAddressOrg()
		{
			TypeValidation.CheckValidGuid(Parent.JC_Calc_ArrivalContainerYardAddressOrgInfo);
		}

		#endregion

		#region JC_Calc_ArrivalUnpackAddressOrg

		public void ValidateJC_Calc_ArrivalUnpackAddressOrg()
		{
			ValidateCalculatedProperty(Parent.JC_Calc_ArrivalUnpackAddressOrgInfo);
		}

		protected void CheckJC_Calc_ArrivalUnpackAddressOrg()
		{
			TypeValidation.CheckValidGuid(Parent.JC_Calc_ArrivalUnpackAddressOrgInfo);
		}

		#endregion

		#region JC_Calc_DeparturePackAddressOrg

		public void ValidateJC_Calc_DeparturePackAddressOrg()
		{
			ValidateCalculatedProperty(Parent.JC_Calc_DeparturePackAddressOrgInfo);
		}

		protected void CheckJC_Calc_DeparturePackAddressOrg()
		{
			TypeValidation.CheckValidGuid(Parent.JC_Calc_DeparturePackAddressOrgInfo);
		}

		#endregion

		#region JC_Calc_DepartureCTOAddressOrg

		public void ValidateJC_Calc_DepartureCTOAddressOrg()
		{
			ValidateCalculatedProperty(Parent.JC_Calc_DepartureCTOAddressOrgInfo);
		}

		protected void CheckJC_Calc_DepartureCTOAddressOrg()
		{
			TypeValidation.CheckValidGuid(Parent.JC_Calc_DepartureCTOAddressOrgInfo);
		}

		#endregion

		#region JC_Calc_ArrivalCTOAddressOrg

		public void ValidateJC_Calc_ArrivalCTOAddressOrg()
		{
			ValidateCalculatedProperty(Parent.JC_Calc_ArrivalCTOAddressOrgInfo);
		}

		protected void CheckJC_Calc_ArrivalCTOAddressOrg()
		{
			TypeValidation.CheckValidGuid(Parent.JC_Calc_ArrivalCTOAddressOrgInfo);
		}

		#endregion

		#region JC_JSB_SupplierBooking

		protected override void CheckJC_JSB_SupplierBooking()
		{
			base.CheckJC_JSB_SupplierBooking();

			if (AdvOrmFeatureHelper.IsEnabled && !Parent.JC_JSB_SupplierBookingInfo.HasErrors())
			{
				if ((!Parent.IsInDatabase || Parent.JC_JSB_SupplierBookingInfo.HasChanges) &&
					Parent.Factory.Load<IJobSupplierBooking>(Parent.JC_JSB_SupplierBooking) is IJobSupplierBooking supplierBooking &&
					!(supplierBooking.JSB_Status == SupplierBookingStatus.Approved || supplierBooking.JSB_Status == SupplierBookingStatus.Planned))
				{
					Parent.JC_JSB_SupplierBookingInfo.AddError(Res.GetString(
						"5337c35e-ef65-454e-af84-0104854acf81",
						"Only approved or planned Supplier Bookings can be linked to containers.")
					);
				}
				else if (SupplierBookingChangedFromNonEditableState())
				{
					Parent.JC_JSB_SupplierBookingInfo.AddError(ContainerHasLoadListLineMessage);
				}
			}
		}

		public readonly MultilingualString ContainerAlreadyAllocatedErrorMessage = ResString.GetMultilingualString(
			"3399cef2-4870-4161-99bf-87a82766fae9",
			"This container row is already allocated to an active Supplier Booking in status PLN or CNV. The Supplier Booking must be canceled in order to edit the container count or type on this row."
		);

		public readonly MultilingualString ContainerHasLoadListLineMessage = ResString.GetMultilingualString(
			"df908d7f-58cd-44db-98db-ac6feb9ae348",
			"Cannot remove or replace supplier booking reference if the linked supplier booking has been progressed to a planned state. Supplier booking must be canceled to deallocate this container from the booking."
		);

		public readonly MultilingualString CanNotDeleteContainerWhenItHasLoadListLineMessage = ResString.GetMultilingualString(
			"0da8e82c-afc4-42c8-a7a1-0264f9328da2",
			"Cannot delete container when it has 'Container Load List Lines'. Please de-allocate related 'Container Load List Lines' before deleting this Container."
		);

		public bool HasNonEditableSupplierBooking()
		{
			return AdvOrmFeatureHelper.IsEnabled
				&& !Parent.JC_JSB_SupplierBooking.IsEmpty
				&& NonEditableSupplierBookingStatus.Contains(Parent.SupplierBooking.JSB_Status);
		}

		public bool SupplierBookingChangedFromNonEditableState()
		{
			return Parent.JC_JSB_SupplierBookingInfo.HasChanges
				&& Parent.JC_JSB_SupplierBookingInfo.OriginalValue is ZGuid originalValue
				&& originalValue != ZGuid.Empty
				&& Parent.Factory.Load<IJobSupplierBooking>(originalValue) is IJobSupplierBooking originalSupplierBooking
				&& NonEditableSupplierBookingStatus.Contains(originalSupplierBooking.JSB_Status)
				&& (SupplierBookingStatus.Planned == originalSupplierBooking.JSB_Status || Parent.IsAttachedToContainerLoadList());
		}

		readonly ZString[] NonEditableSupplierBookingStatus = new ZString[]
		{
			SupplierBookingStatus.Planned,
			SupplierBookingStatus.Converted,
		};

		#endregion

		protected override void CheckJC_TareWeight()
		{
			base.CheckJC_TareWeight();
			if (Parent.JC_TareWeight < 0)
			{
				Parent.JC_TareWeightInfo.AddError(Res.GetString("B981631F-83B1-4C67-B9C7-69958CD5894D", "Tare Weight cannot be less than 0."));
			}
		}

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateIsChiller();
			ValidateIsFreezer();

			ValidateJC_Calc_DepartureContainerYardAddressOrg();
			ValidateJC_Calc_ArrivalContainerYardAddressOrg();
			ValidateJC_Calc_ArrivalUnpackAddressOrg();
			ValidateJC_Calc_DeparturePackAddressOrg();
			ValidateJC_Calc_DepartureCTOAddressOrg();
			ValidateJC_Calc_ArrivalCTOAddressOrg();
		}

		#endregion

		#region Parent

		public new CommonContainer Parent
		{
			get { return (CommonContainer)base.Parent; }
		}

		#endregion
	}
}
