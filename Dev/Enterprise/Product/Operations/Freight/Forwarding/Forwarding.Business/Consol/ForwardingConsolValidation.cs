using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolValidation : CommonConsolValidation
	{
		public ForwardingConsolValidation(ForwardingConsol parent)
			: base(parent)
		{
		}

		#region JK_AgentType

		protected override void CheckJK_AgentType()
		{
			base.CheckJK_AgentType();

			if ((!Parent.IsInDatabase || Parent.JK_AgentTypeInfo.HasChanges) && !GetConsolTypeSecurityCheckpoint().IsAllowed)
			{
				Parent.JK_AgentTypeInfo.AddError(Res.GetString("8e2b3a32-debb-4bc7-8c26-039f35def6b6", "You do not have security rights to create {0} Consol", Parent.JK_AgentType));
			}
		}

		#region Gateway Agent Type

		ForwardingConsolGatewayBillingSupporter GatewayBillingSupporter => gatewayBillingSupporter ?? (gatewayBillingSupporter = ((IGateway)Parent).GatewayBillingSupporter as ForwardingConsolGatewayBillingSupporter);
		ForwardingConsolGatewayBillingSupporter gatewayBillingSupporter;

		#endregion

		#endregion

		#region JK_AWBServiceLevel

		protected override void CheckJK_AWBServiceLevel()
		{
			base.CheckJK_AWBServiceLevel();
			ListValidation.ErrorIfInvalidCode(Parent.JK_AWBServiceLevelInfo, Parent.NeutralAirWaybillServiceLevelList);
		}

		#endregion

		#region JK_MasterBillNum

		protected override void CheckJK_MasterBillNum()
		{
			base.CheckJK_MasterBillNum();
			if (Parent.IsAir)
			{
				ValidateMasterBillMAWB();
			}
			else
			{
				if (!Parent.JK_MasterBillNum.IsEmpty && MasterBillValidator.IsDuplicate(Parent, ZDateTime.Empty, ZDateTime.Empty))
				{
					Parent.JK_MasterBillNumInfo.AddWarning(Res.GetString("bbc31594-036b-4724-bede-8e850ab352d4", "This Ocean Bill of Lading already exists on another Consol or on a Shipments that is not attached to this Consol."));
				}
				else if (!Parent.JK_MasterBillNum.IsEmpty && Parent.JK_MasterBillNum.StartsWith(" ", System.StringComparison.CurrentCultureIgnoreCase))
				{
					Parent.JK_MasterBillNumInfo.AddMessageError(Res.GetString("84AC3918-E430-4984-9728-B39745277C39", "Ocean Bill should not contain leading spaces."));
				}
				else if (Parent.ReferenceNumberShouldBeSplitIntoNumbers(Parent.JK_MasterBillNum))
				{
					Parent.JK_MasterBillNumInfo.AddError(Res.GetString("F7D66AC5-553F-417C-8CD9-0800D414D5E6", "Commas, spaces, colons, and semicolons are not valid characters on a Bill of Lading."));
				}
				else if (Parent.JK_MasterBillNum.IsEmpty)
				{
					Parent.JK_MasterBillNumInfo.AddWarning(Res.GetString("fc23c8f3-dd93-4d53-8809-8f32d31a1f8d", "You have not entered a Bill of Lading."));
				}
			}

			if (Parent.IsSuitableForForwardAirMessage() && Parent.IsForwardAirCarrierOnConsol())
			{
				var stmNums = Parent.ForwardAirBillStmNums;
				if (stmNums != null)
				{
					const int leftNumbers = 10;

					var fountain = stmNums.TryGetNumberFountain();
					if (fountain != null)
					{
						var preliminaryNumber = fountain.PeekPreliminary(Parent.Factory);

						var availableNumbers = preliminaryNumber > stmNums.SN_MaximumValue || preliminaryNumber < stmNums.SN_MinimumValue ? 0 : stmNums.SN_MaximumValue - preliminaryNumber + 1;
						if (availableNumbers <= leftNumbers)
						{
							var message = availableNumbers > 0
								? Res.GetString("fd77a2e9-08f0-430e-a69e-37b0b995373d",
									"The BOL number range allocated by Forward Air is about to expire. You have {0} numbers left. Please contact Forward Air to obtain a new set of numbers, which can be added to the Forward Air Carrier organization under Details > Config > Number Ranges",
									availableNumbers)
								: Res.GetString("dd6634f4-5ce4-45b6-9fbb-a34488db078a",
									"The BOL number range allocated by Forward Air is to expire. Please contact Forward Air to obtain a new set of numbers, which can be added to the Forward Air Carrier organization under Details > Config > Number Ranges");

							Parent.JK_MasterBillNumInfo.AddWarning(message);
						}
					}
				}
			}
		}

		#endregion

		#region JK_Phase

		protected override void CheckJK_Phase()
		{
			base.CheckJK_Phase();
			ListValidation.ErrorIfInvalidCode(Parent.JK_PhaseInfo, Parent.Phases);
		}

		#endregion

		#region JK_RequiresTemperatureControl

		protected override void CheckJK_RequiresTemperatureControl()
		{
			base.CheckJK_RequiresTemperatureControl();

			if (!Parent.JK_RequiresTemperatureControl
				&& (!Parent.JK_RequiredTemperatureMaximum.IsDefault
				|| !Parent.JK_RequiredTemperatureMinimum.IsDefault))
			{
				Parent.JK_RequiresTemperatureControlInfo.AddError(Res.GetString("0a898703-fb9e-1a91-4226-0bd2f795610d",
					"Is Temperature Control flag should be set when a temperature range is specified."));
				return;
			}

			var shipmentsWithInvalidTempRanges = Parent.Shipments.OfType<CommonShipment>()
				.Where(s => !Parent.ShipmentTemperatureRangeIsValid(s))
				.Select(s => s.HumanReadableName)
				.ToArray();

			if (shipmentsWithInvalidTempRanges.Length > 0)
			{
				var errorMessage = Res.GetString("cf786644-a1a1-ee80-4701-874bc90cb01e",
					"The following Shipment(s) attached to this consol have temperature controlled cargo that is not supported by this consol: {0}",
					string.Join(System.Environment.NewLine, shipmentsWithInvalidTempRanges));

				Parent.JK_RequiresTemperatureControlInfo.AddError(errorMessage);
			}

			if (Parent.JK_RequiresTemperatureControl)
			{
				var shipmentsWithPackLinesNotRequiringTemperatureControl = Parent.Shipments.OfType<CommonShipment>()
				.Where(s => s.OuterPackLines.OfType<PackLine>().Any(p => !p.JL_RequiresTemperatureControl))
				.Select(s => s.HumanReadableName)
				.ToArray();

				if (shipmentsWithPackLinesNotRequiringTemperatureControl.Length > 0)
				{
					var warningMessage = Res.GetString("ccb4306f-d207-4f8c-8d62-3775b32c4cbb",
						"The following Shipment(s) contain non temperature controlled cargo. Please verify the temperature range of this Consol is suitable for this cargo: {0}",
						string.Join(System.Environment.NewLine, shipmentsWithPackLinesNotRequiringTemperatureControl));

					Parent.JK_RequiresTemperatureControlInfo.AddWarning(warningMessage);
				}
			}
		}

		#endregion

		#region JK_RequiredTemperatureUnit

		protected override void CheckJK_RequiredTemperatureUnit()
		{
			base.CheckJK_RequiredTemperatureUnit();

			if (Parent.JK_RequiredTemperatureUnit != Constants.Temperature.Centigrade && Parent.JK_RequiredTemperatureUnit != Constants.Temperature.Fahrenheit)
			{
				Parent.JK_RequiredTemperatureUnitInfo.AddError(Res.GetString("dd68e6d1-9333-4568-bf61-53c3bb5aab53", "Temperature must be set to C (Celsius) or F (Fahrenheit)"));
			}
		}

		#endregion

		#region JK_RequiredTemperature MinAndMax

		protected override void CheckJK_RequiredTemperatureMinimum()
		{
			base.CheckJK_RequiredTemperatureMinimum();

			if (Parent.JK_RequiredTemperatureMinimum > Parent.JK_RequiredTemperatureMaximum)
			{
				Parent.JK_RequiredTemperatureMinimumInfo.AddError(Res.GetString("8b143e46-f586-4057-b3cf-4c93a2ed824e", "Minimum temperature cannot be higher than maximum temperature"));
			}

			if (Parent.JK_RequiredTemperatureUnit == Constants.Temperature.Centigrade || Parent.JK_RequiredTemperatureUnit == Constants.Temperature.Fahrenheit)
			{
				if (Parent.JK_RequiredTemperatureMinimum < GetAbsoluteZero(Parent.JK_RequiredTemperatureUnit))
				{
					Parent.JK_RequiredTemperatureMinimumInfo.AddError(Res.GetString("9d02f20a-c464-4f0e-a654-7cdc9365643e",
						"{0}°{2} is below the minimum possible temperature of absolute zero ({1}°{2})",
						Parent.JK_RequiredTemperatureMinimum, GetAbsoluteZero(Parent.JK_RequiredTemperatureUnit), Parent.JK_RequiredTemperatureUnit));
				}
			}
		}

		protected override void CheckJK_RequiredTemperatureMaximum()
		{
			base.CheckJK_RequiredTemperatureMaximum();

			if (Parent.JK_RequiredTemperatureMaximum < Parent.JK_RequiredTemperatureMinimum)
			{
				Parent.JK_RequiredTemperatureMaximumInfo.AddError(Res.GetString("1159f1ea-75d7-45e9-90cc-81d191bd17c8", "Maximum temperature cannot be lower than minimum temperature"));
			}

			if (Parent.JK_RequiredTemperatureUnit == Constants.Temperature.Centigrade || Parent.JK_RequiredTemperatureUnit == Constants.Temperature.Fahrenheit)
			{
				if (Parent.JK_RequiredTemperatureMaximum < GetAbsoluteZero(Parent.JK_RequiredTemperatureUnit))
				{
					Parent.JK_RequiredTemperatureMaximumInfo.AddError(Res.GetString("9d02f20a-c464-4f0e-a654-7cdc9365643e",
						"{0}°{2} is below the minimum possible temperature of absolute zero ({1}°{2})",
						Parent.JK_RequiredTemperatureMaximum, GetAbsoluteZero(Parent.JK_RequiredTemperatureUnit), Parent.JK_RequiredTemperatureUnit));
				}
			}
		}

		ZDecimal GetAbsoluteZero(ZString unit) => Constants.Temperature.Convert(0, Constants.Temperature.Kelvin, unit);

		#endregion

		#region JK_BookingReference

		protected override void CheckJK_BookingReference()
		{
			base.CheckJK_BookingReference();
			if (!Parent.JK_BookingReference.IsEmpty && Parent.JK_BookingReference.StartsWith(" ", System.StringComparison.CurrentCultureIgnoreCase))
			{
				Parent.JK_BookingReferenceInfo.AddMessageError(Res.GetString("DEE78D83-F64A-4CDF-B547-CCDB467AEE8E", "Booking Reference should not contain leading spaces."));
			}
			if (Parent.ReferenceNumberShouldBeSplitIntoNumbers(Parent.JK_BookingReference))
			{
				Parent.JK_BookingReferenceInfo.AddError(Res.GetString("BC73E58B-0BFC-4BC6-8CFE-CDCD0300FCA7", "Booking Reference cannot contain multiple numbers. Please split these numbers and add them to the Reference Numbers grid under the numbers tab."));
			}
		}

		#endregion

		#region JK_MBLAWBChargesDisplay

		protected override void CheckJK_MBLAWBChargesDisplay()
		{
			base.CheckJK_MBLAWBChargesDisplay();

			if (ChargesApplyHelper.ChargesApplyPairList.ContainsCode(Parent.JK_MBLAWBChargesDisplay)
				&& Parent.IsImportTo(Constants.CountryCodes.Brazil)
				&& Parent.IsAgentOrDirect
				&& Parent.JK_MBLAWBChargesDisplay != ChargesApplyHelper.ChargesApplyConstants.NON)
			{
				Parent.JK_MBLAWBChargesDisplayInfo.AddError(Res.GetString("3becbd65-3650-43a8-46c8-c15e2bcd5249", "\"As Agreed\" option cannot be used for imports to Brazil"));
			}

			ListValidation.ErrorIfInvalidCode(Parent.JK_MBLAWBChargesDisplayInfo);
		}

		#endregion

		#region JK_MaximumAllowablePackageDimensions

		protected override void CheckJK_MaximumAllowablePackageHeight()
		{
			base.CheckJK_MaximumAllowablePackageHeight();

			if (Parent.JK_MaximumAllowablePackageHeight == 0)
			{
				CheckPreallocationDimensionsAreAllPopulated(Parent.JK_MaximumAllowablePackageHeightInfo);
			}
			CheckPacklineDimensionsAreUnderMaximumAllowableDimensions(Parent.JK_MaximumAllowablePackageHeightInfo, result => result.ItemFitsHeight);
		}

		protected override void CheckJK_MaximumAllowablePackageWidth()
		{
			base.CheckJK_MaximumAllowablePackageWidth();

			if (Parent.JK_MaximumAllowablePackageWidth == 0)
			{
				CheckPreallocationDimensionsAreAllPopulated(Parent.JK_MaximumAllowablePackageWidthInfo);
			}
			CheckPacklineDimensionsAreUnderMaximumAllowableDimensions(Parent.JK_MaximumAllowablePackageWidthInfo, result => result.ItemFitsWidthAndLength);
		}

		protected override void CheckJK_MaximumAllowablePackageLength()
		{
			base.CheckJK_MaximumAllowablePackageLength();

			if (Parent.JK_MaximumAllowablePackageLength == 0)
			{
				CheckPreallocationDimensionsAreAllPopulated(Parent.JK_MaximumAllowablePackageLengthInfo);
			}
			CheckPacklineDimensionsAreUnderMaximumAllowableDimensions(Parent.JK_MaximumAllowablePackageLengthInfo, result => result.ItemFitsWidthAndLength);
		}

		protected override void CheckJK_MaximumAllowablePackageUnit()
		{
			base.CheckJK_MaximumAllowablePackageUnit();

			ListValidation.ErrorIfInvalidCode(Parent.JK_MaximumAllowablePackageUnitInfo, Parent.JK_PackageUnit_List);

			if (Parent.JK_MaximumAllowablePackageUnit.IsEmpty)
			{
				CheckPreallocationDimensionsAreAllPopulated(Parent.JK_MaximumAllowablePackageUnitInfo);
			}
		}

		void CheckPreallocationDimensionsAreAllPopulated(ZPropertyInfo propertyInfo)
		{
			bool anyDimensionsNotEmpty = Parent.JK_MaximumAllowablePackageLength != 0
				|| Parent.JK_MaximumAllowablePackageWidth != 0
				|| Parent.JK_MaximumAllowablePackageHeight != 0
				|| !Parent.JK_MaximumAllowablePackageUnit.IsEmpty;

			if (anyDimensionsNotEmpty)
			{
				var errorMessage = Res.GetString("2cafb337-7176-41be-49db-bde51c62e1d7", "Please enter the dimensions into all fields.");
				propertyInfo.AddError(errorMessage);
			}
		}

		void CheckPacklineDimensionsAreUnderMaximumAllowableDimensions(ZPropertyInfo propertyInfo, Func<CargoDimensionsHelpers.CheckFitsInConsolResult, bool> check)
		{
			var invalidPackLines = Parent
				.Shipments
				.Cast<ForwardingShipment>()
				.SelectMany(shipment => shipment.OuterPackLines)
				.Cast<ForwardingPackLine>()
				.Where(packLine => !check(CargoDimensionsHelpers.CheckCanFitInConsol(Parent, packLine)));

			if (invalidPackLines.Any())
			{
				var message = Res.GetString("2d3c3cb5-f033-e192-4dce-d213d1693e3f", "The attached packlines have dimensions that exceeds the maximum dimensions.");
				CargoDimensionsHelpers.AddPreAllocationCheckDimensionsError(propertyInfo, message);
			}
		}

		#endregion

		#region JK_PackageGrouping

		protected override void CheckJK_PackageGrouping()
		{
			base.CheckJK_PackageGrouping();

			MandatoryValidation.CheckEntered(Parent.JK_PackageGroupingInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JK_PackageGroupingInfo);
		}

		#endregion

		#region Load / Discharge

		protected override void CheckJK_RL_NKLoadPort()
		{
			base.CheckJK_RL_NKLoadPort();

			if (!Parent.IsValidationSuspended && !Parent.JK_RL_NKLoadPortInfo.HasErrors())
			{
				var warning = Parent.DestinationSupplyChainSecurityConfiguration.GetWarningForProhibitedRouting(Parent);
				if (!warning.IsEmpty)
				{
					Parent.JK_RL_NKLoadPortInfo.AddWarning(warning);
				}
				else
				{
					foreach (Transport transport in Parent.Transports.Cast<Transport>().Where(x => x.JW_RL_NKLoadPort == Parent.JK_RL_NKLoadPort))
					{
						var transportWarning = transport.DestinationSupplyChainSecurityConfiguration.GetWarningForProhibitedRouting(Parent);
						if (!transportWarning.IsEmpty)
						{
							Parent.JK_RL_NKLoadPortInfo.AddWarning(transportWarning);
						}
					}
				}

				if (IsCreatingFromTemplate())
				{
					Parent.Transports.Sort(MovementLegComparer.PortsAndDatesBased(Parent.Transports));

					var firstTransport = Parent.Transports.Cast<Transport>().FirstOrDefault();
					if (firstTransport != null
						&& !firstTransport.JW_RL_NKLoadPort.IsEmpty
						&& Parent.JK_RL_NKLoadPort != firstTransport.JW_RL_NKLoadPort)
					{
						Parent.JK_RL_NKLoadPortInfo.AddWarning(Res.GetString("74ba7fd5-a0e9-4d17-b82d-adaf89a131d9", "Consol templates must be the same origin as the selected flight schedule."));
					}
				}
			}
		}

		protected override void CheckJK_RL_NKDischargePort()
		{
			base.CheckJK_RL_NKDischargePort();

			if (!Parent.IsValidationSuspended
				&& !Parent.JK_RL_NKDischargePortInfo.HasErrors()
				&& IsCreatingFromTemplate())
			{
				Parent.Transports.Sort(MovementLegComparer.PortsAndDatesBased(Parent.Transports));

				var lastTransport = Parent.Transports.Cast<Transport>().LastOrDefault();
				if (lastTransport != null
					&& !lastTransport.JW_RL_NKDiscPort.IsEmpty
					&& Parent.JK_RL_NKDischargePort != lastTransport.JW_RL_NKDiscPort)
				{
					Parent.JK_RL_NKDischargePortInfo.AddWarning(Res.GetString("1a1767f9-c869-4f48-88d8-943ad6adb7d2", "Consol templates must be the same destination as the selected flight schedule."));
				}
			}
		}

		#endregion

		#region Gateway Handling Types

		#region JK_SendingForwarderHandlingType

		bool CanHaveGatewayAddress(CommonConsol parent) => parent.IsAgent || parent.IsCoLoad || parent.IsMultiAWBMaster || parent.IsAWBCoload || parent.IsDirect;

		string GetGatewayAddressRestrictionMessage(string addressType) => Res.GetString("d8e382c5-deb0-4e15-8b17-dca6576429e1", "{0} on this Consol is flagged as \"Gateway\", the Consol type must be either AGT, CLA, CLD, CLM, or DRT.", addressType);

		protected override void CheckJK_SendingForwarderHandlingType()
		{
			base.CheckJK_SendingForwarderHandlingType();

			if (ShouldAddErrorForAdditionRemovalOfTypeWhenNoSecurityGranted(Parent.JK_SendingForwarderHandlingTypeInfo, Parent.JK_SendingForwarderHandlingType_Defaulted))
			{
				Parent.JK_SendingForwarderHandlingTypeInfo.AddError(Res.GetString("b7f43463-cd76-f698-47b0-97835a9a9afa", "You do not have the security rights to add or remove the Gateway Handling Type."));
				return;
			}

			if (Parent.JK_SendingForwarderHandlingType.IsEmpty)
			{
				AddErrorWhenForwarderHandlingTypeIsBlankButGatewayJobExists(Parent.JK_SendingForwarderHandlingTypeInfo, GatewayBillingSupporter.IsReceivingAgentGateway);
				return;
			}

			if (!CanHaveGatewayAddress(Parent))
			{
				Parent.JK_SendingForwarderHandlingTypeInfo.AddError(GetGatewayAddressRestrictionMessage((NoResString)"Sending Agent")); // for error message
				return;
			}

			if (Parent.JK_OA_SendingForwarderAddress.IsEmpty)
			{
				Parent.JK_SendingForwarderHandlingTypeInfo.AddError(Res.GetString("88f4a063-b4dd-8590-4fca-73500cf6f663", "No Address has been selected to flag as \"Gateway\"."));
				return;
			}

			if (TryFindValidGatewayTypeForAddress(GatewayAddress.SendingForwarder, out var gatewayHandlingType))
			{
				var sendingForwarderHandlingType = Parent.JK_SendingForwarderHandlingType;
				if (sendingForwarderHandlingType != gatewayHandlingType)
				{
					Parent.JK_SendingForwarderHandlingTypeInfo.AddError(Res.GetString("d459c46b-f59c-b089-488d-4587e292307d", @"This Agent Organization is flagged as {0}. {1} type is not valid.
You can verify the organization's Gateway Agent Details on the Fwd/Agent tab.",
						string.Concat(gatewayHandlingType, " - ", AgentStatuses.GetDescriptionFromCode(gatewayHandlingType)),
						string.Concat(sendingForwarderHandlingType, " - ", AgentStatuses.GetDescriptionFromCode(sendingForwarderHandlingType))));
				}
			}
			else
			{
				Parent.JK_SendingForwarderHandlingTypeInfo.AddError(Res.GetString("924f6ab8-6ac8-3693-4df7-b2a4e026ccab",
					@"Sending Agent can be set as a {0} on a Consol if this Agent is:
- configured as a {0} for the Load Port with the corresponding Transport Mode in Organization>Forwarder/Agent>Gateway Agent
- an Organization Proxy",
					AgentStatuses.GetDescriptionFromCode(Parent.JK_SendingForwarderHandlingType)));
			}
		}

		#endregion

		#region JK_ReceivingForwarderHandlingType

		protected override void CheckJK_ReceivingForwarderHandlingType()
		{
			base.CheckJK_ReceivingForwarderHandlingType();

			if (ShouldAddErrorForAdditionRemovalOfTypeWhenNoSecurityGranted(Parent.JK_ReceivingForwarderHandlingTypeInfo, Parent.JK_ReceivingForwarderHandlingType_Defaulted))
			{
				Parent.JK_ReceivingForwarderHandlingTypeInfo.AddError(Res.GetString("b7f43463-cd76-f698-47b0-97835a9a9afa", "You do not have the security rights to add or remove the Gateway Handling Type."));
				return;
			}

			if (Parent.JK_ReceivingForwarderHandlingType.IsEmpty)
			{
				AddErrorWhenForwarderHandlingTypeIsBlankButGatewayJobExists(Parent.JK_ReceivingForwarderHandlingTypeInfo, GatewayBillingSupporter.IsSendingAgentGateway);
				return;
			}

			if (!CanHaveGatewayAddress(Parent))
			{
				Parent.JK_ReceivingForwarderHandlingTypeInfo.AddError(GetGatewayAddressRestrictionMessage((NoResString)"Receiving Agent")); // for error message
				return;
			}

			if (Parent.JK_OA_ReceivingForwarderAddress.IsEmpty)
			{
				Parent.JK_ReceivingForwarderHandlingTypeInfo.AddError(Res.GetString("cb395e27-df07-d292-4b58-06cc05c47178", "No Address has been selected to flag as \"Gateway\"."));
				return;
			}

			if (TryFindValidGatewayTypeForAddress(GatewayAddress.ReceivingForwarder, out var gatewayHandlingType))
			{
				var receivingForwarderHandlingType = Parent.JK_ReceivingForwarderHandlingType;
				if (receivingForwarderHandlingType != gatewayHandlingType)
				{
					Parent.JK_ReceivingForwarderHandlingTypeInfo.AddError(Res.GetString("fd78184d-30d9-3a9a-431a-cd0cfd95eb37", @"This Agent Organization is flagged as {0}. {1} type is not valid.
You can verify the organization's Gateway Agent Details on the Fwd/Agent tab.",
						string.Concat(gatewayHandlingType, " - ", AgentStatuses.GetDescriptionFromCode(gatewayHandlingType)),
						string.Concat(receivingForwarderHandlingType, " - ", AgentStatuses.GetDescriptionFromCode(receivingForwarderHandlingType))));
				}
			}
			else
			{
				Parent.JK_ReceivingForwarderHandlingTypeInfo.AddError(Res.GetString("ab323cc5-b72f-ae9c-4a3c-0926a313b2f2",
					@"Receiving Agent can be set as a {0} on a Consol if this Agent is:
- configured as a {0} for the Discharge Port with the corresponding Transport Mode in Organization>Forwarder/Agent>Gateway Agent
- an Organization Proxy",
					AgentStatuses.GetDescriptionFromCode(Parent.JK_ReceivingForwarderHandlingType)));
			}
		}

		bool ShouldAddErrorForAdditionRemovalOfTypeWhenNoSecurityGranted(ZPropertyInfo handlingTypeInfo, string defaultedValue)
		{
			if (Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed)
			{
				return false;
			}

			var handlingType = (ZString)handlingTypeInfo.Value;

			if (Parent.IsInDatabase &&
				handlingTypeInfo.HasChanges &&
				handlingType != defaultedValue)
			{
				return true;
			}

			if (!Parent.IsInDatabase
				&& ((defaultedValue != null && handlingType != defaultedValue) ||
					(defaultedValue == null && !handlingType.IsEmpty)))
			{
				return true;
			}

			return false;
		}

		void AddErrorWhenForwarderHandlingTypeIsBlankButGatewayJobExists(ZPropertyInfo handlingTypeInfoToCheck, Func<GlbCompany, string, bool> isGatewayForOtherAgent)
		{
			if (handlingTypeInfoToCheck.OriginalValue is ZString originalHandlingType &&
				originalHandlingType.IsValid && !originalHandlingType.IsEmpty)
			{
				foreach (var company in GatewayBillingSupporter.AllGlbCompanies)
				{
					if (GatewayBillingSupporter.GetConsolJob(company) != null
						&& !isGatewayForOtherAgent(company, null))
					{
						AddGatewayJobExistsErrorForHandlingType(handlingTypeInfoToCheck, company.GC_Code);
						break;
					}
				}
			}
		}

		void AddGatewayJobExistsErrorForHandlingType(ZPropertyInfo propertyInfo, ZString companyCode)
		{
			var message = Res.GetString("4212D505-8DE9-487F-AC62-7E0C3DB8385B",
				"Un-flagging the {0} is only permitted if no Gateway Billing Job exists in the Gateway Agent's login Company {1}.",
				propertyInfo.HumanReadableName, companyCode);
			propertyInfo.AddError(message);
		}

		bool TryFindValidGatewayTypeForAddress(GatewayAddress gatewayAddress, out ZString gatewayHandlingType)
		{
			gatewayHandlingType = ZString.Empty;
			if (GatewayBillingSupporter == null)
			{
				return false;
			}

			foreach (var company in GatewayBillingSupporter.AllGlbCompanies)
			{
				if (gatewayAddress == GatewayAddress.SendingForwarder)
				{
					gatewayHandlingType = GatewayBillingSupporter.GetDefaultSendingForwarderAddressGatewayType(company);
				}
				else
				{
					gatewayHandlingType = GatewayBillingSupporter.GetDefaultReceivingForwarderAddressGatewayType(company);
				}

				if (!gatewayHandlingType.IsEmpty)
				{
					return true;
				}
			}

			return false;
		}

		enum GatewayAddress { SendingForwarder, ReceivingForwarder }

		AgentStatusList AgentStatuses => agentStatuses ?? (agentStatuses = new AgentStatusList());

		AgentStatusList agentStatuses;

		#endregion

		#endregion

		#region Verification / Pre-Allocation Details

		public void ValidatePreAllocationValues()
		{
			if (!Parent.IsValidationSuspended)
			{
				ValidateJK_TotalShipmentActWeightCheck();
				ValidateJK_TotalShipmentActVolumeCheck();
				ValidateJK_TotalShipmentChargableCheck();
				ValidateJK_TotalShipmentCountCheck();
			}
		}

		protected override void CheckJK_TotalShipmentActWeightCheck()
		{
			base.CheckJK_TotalShipmentActWeightCheck();
			if (!PreAllocationChecks.Weight.IsNone && WeightExceedsPreAllocationPercentage)
			{
				Parent.JK_TotalShipmentActWeightCheckInfo.AddWarning(Res.GetString("e6f5c50a-0426-4822-8597-34b373d8bb79", "Total {0} ({2} {3}) exceeds the registry-specified {1}% of consol pre-allocation {0} ({4} {5}).",
					Res.GetString("11f25ce6-3de6-4553-87a4-b91d7393be2c", "weight"),
					PreAllocationChecks.Weight.Percentage,
					Parent.JK_TotalShipmentWeight,
					Parent.JK_TotalShipmentWeightUnit,
					Parent.JK_TotalShipmentActWeightCheck,
					Parent.WeightVerificationUnit));
			}
		}

		protected override void CheckJK_TotalShipmentActVolumeCheck()
		{
			base.CheckJK_TotalShipmentActVolumeCheck();
			if (!PreAllocationChecks.Volume.IsNone && VolumeExceedsPreAllocationPercentage)
			{
				Parent.JK_TotalShipmentActVolumeCheckInfo.AddWarning(Res.GetString("e6f5c50a-0426-4822-8597-34b373d8bb79", "Total {0} ({2} {3}) exceeds the registry-specified {1}% of consol pre-allocation {0} ({4} {5}).",
					Res.GetString("aa3f2c17-f0e8-42a6-94ec-b1e0205aac93", "volume"),
					PreAllocationChecks.Volume.Percentage,
					Parent.JK_TotalShipmentVolume,
					Parent.JK_TotalShipmentVolumeUnit,
					Parent.JK_TotalShipmentActVolumeCheck,
					Parent.VolumeVerificationUnit));
			}
		}

		protected override void CheckJK_TotalShipmentChargableCheck()
		{
			base.CheckJK_TotalShipmentChargableCheck();
			if (!PreAllocationChecks.Chargeable.IsNone && ChargeableExceedsPreAllocationPercentage)
			{
				Parent.JK_TotalShipmentChargableCheckInfo.AddWarning(Res.GetString("e6f5c50a-0426-4822-8597-34b373d8bb79", "Total {0} ({2} {3}) exceeds the registry-specified {1}% of consol pre-allocation {0} ({4} {5}).",
					Res.GetString("fe09382b-b4be-4514-8169-741218f47840", "chargeable"),
					PreAllocationChecks.Chargeable.Percentage,
					Parent.JK_TotalShipmentChargeable,
					Parent.JK_Calc_TotalShipmentChargeableUnit,
					Parent.JK_TotalShipmentChargableCheck,
					Parent.JK_TotalShipmentChargeableUnit));
			}
		}

		protected override void CheckJK_TotalShipmentCountCheck()
		{
			base.CheckJK_TotalShipmentCountCheck();
			if (!PreAllocationChecks.ShipmentCount.IsNone && ShipmentCountExceedsPreAllocationPercentage)
			{
				Parent.JK_TotalShipmentCountCheckInfo.AddWarning(Res.GetString("0ec4d1ec-154e-4290-b14c-5b79401acd55", "Total {0} ({1}) exceeds the registry-specified {2}% of consol pre-allocation {0} ({3}).",
					Res.GetString("95d3e10a-2c88-4e62-8041-1b22ced414ea", "number of shipments"),
					Parent.Shipments.Count,
					PreAllocationChecks.ShipmentCount.Percentage,
					Parent.JK_TotalShipmentCountCheck));
			}
		}

		internal bool WeightExceedsPreAllocationPercentage
		{
			get { return MeasureExceedsPreAllocation(Parent.JK_TotalShipmentWeight, Parent.JK_TotalShipmentWeightUnit, Parent.JK_TotalShipmentActWeightCheck, Parent.JK_TotalShipmentActWeightCheckInfo, Parent.WeightVerificationUnit, PreAllocationChecks.Weight.Percentage); }
		}

		internal bool VolumeExceedsPreAllocationPercentage
		{
			get { return MeasureExceedsPreAllocation(Parent.JK_TotalShipmentVolume, Parent.JK_TotalShipmentVolumeUnit, Parent.JK_TotalShipmentActVolumeCheck, Parent.JK_TotalShipmentActVolumeCheckInfo, Parent.VolumeVerificationUnit, PreAllocationChecks.Volume.Percentage); }
		}

		internal bool ChargeableExceedsPreAllocationPercentage
		{
			get { return MeasureExceedsPreAllocation(Parent.JK_TotalShipmentChargeable, Parent.JK_TotalShipmentChargeableUnit, Parent.JK_TotalShipmentChargableCheck, Parent.JK_TotalShipmentChargableCheckInfo, Parent.JK_TotalShipmentChargeableUnit, PreAllocationChecks.Chargeable.Percentage); }
		}

		internal bool ShipmentCountExceedsPreAllocationPercentage
		{
			get
			{
				ZDecimal warningPercentage = PreAllocationChecks.ShipmentCount.Percentage;
				if (warningPercentage > 0 && Parent.JK_TotalShipmentCountCheck > 0)
				{
					var shipmentCount = Parent.JK_AgentType == Constants.AgentType.AWBMaster ? Parent.ColoadConsols.Sum(c => c.Shipments.Count) : Parent.Shipments.Count;
					if (shipmentCount > Parent.JK_TotalShipmentCountCheck * warningPercentage / 100)
					{
						return true;
					}
				}

				return false;
			}
		}

		bool MeasureExceedsPreAllocation(ZDecimal totalMeasure, string totalMeasureUnit, ZDecimal preAllocationMeasure, ZPropertyInfo preAllocationMeasureInfo, string preAllocationMeasureUnit, ZDecimal warningPercentage)
		{
			if (preAllocationMeasure != 0 && warningPercentage > 0)
			{
				if (totalMeasureUnit != preAllocationMeasureUnit)
				{
					if (Constants.Weight.ContainsCode(totalMeasureUnit) && Constants.Weight.ContainsCode(preAllocationMeasureUnit))
					{
						totalMeasure = Constants.Weight.Convert(totalMeasure, totalMeasureUnit, preAllocationMeasureUnit);
						totalMeasure = Parent.GetRoundedValue(preAllocationMeasureInfo, totalMeasure);
					}
					else if (Constants.Volume.ContainsCode(totalMeasureUnit) && Constants.Volume.ContainsCode(preAllocationMeasureUnit))
					{
						totalMeasure = Constants.Volume.Convert(totalMeasure, totalMeasureUnit, preAllocationMeasureUnit);
						totalMeasure = Parent.GetRoundedValue(preAllocationMeasureInfo, totalMeasure);
					}
				}

				if (totalMeasure > preAllocationMeasure * warningPercentage / 100)
				{
					return true;
				}
			}

			return false;
		}

		protected PreAllocationCheckCollection PreAllocationChecks
		{
			get { return ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value; }
		}

		protected override void CheckJK_ConsolCutOffDate()
		{
			base.CheckJK_ConsolCutOffDate();

			if (IsLateThanCutOffDate)
			{
				var newShipmentDesc = Res.GetString("9dbd998e-1346-407a-bc68-e8a22b483f44", "New Shipment");
				var newConsolDesc = Res.GetString("18af47f4-04d5-4d17-93f0-847567b7bc80", "New Consol");
				var notificationType = Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed
					? CargoWise.ComponentModel.NotificationType.Warning
					: CargoWise.ComponentModel.NotificationType.Error;

				AddNotificationForLateConsolsOrShipments<ForwardingShipment>(notificationType, Res.GetString("ea9e037f-9388-4fca-bca4-db275a30451c", "The following Shipment(s) were removed from the Consol after the Cut Off Date:"), Parent.ShipmentsDetachedThisSession, newShipmentDesc);
				AddNotificationForLateConsolsOrShipments<ForwardingConsol>(notificationType, Res.GetString("0d2895d7-a794-45cf-b66b-006b321c9d95", "The following Consol(s) were removed from the Consol after the Cut Off Date:"), Parent.ConsolsDetachedThisSession, newConsolDesc);
				AddNotificationForLateConsolsOrShipments<ForwardingShipment>(notificationType, Res.GetString("84653afc-7108-440a-9f91-499fbd3929de", "The following Shipment(s) were added to the Consol after the Cut Off Date:"), Parent.ShipmentsAttachedThisSession, newShipmentDesc);
				AddNotificationForLateConsolsOrShipments<ForwardingConsol>(notificationType, Res.GetString("d8b67999-2678-4312-a842-dec3cf8eef13", "The following Consol(s) were added to the Consol after the Cut Off Date:"), Parent.ConsolsAttachedThisSession, newConsolDesc);
			}

			AddNotificationForShipmentsWithChangedWeightOrVolumeAfterConsolCutOffDate();
		}

		public bool IsLateThanCutOffDate => Parent.JK_ConsolCutOffDate.IsValid && Parent.JK_ConsolCutOffDate < ZDateTime.UtcNow;

		void AddNotificationForShipmentsWithChangedWeightOrVolumeAfterConsolCutOffDate()
		{
			if (Parent.IsInDatabase
				&& Parent.Shipments.Cast<ForwardingShipment>()
					.Any(x => (x.JS_ActualWeightModifiedDateUtc.IsValid && x.JS_ActualWeightModifiedDateUtc > Parent.JK_ConsolCutOffDate)
						|| (x.JS_UnitOfWeightModifiedDateUtc.IsValid && x.JS_UnitOfWeightModifiedDateUtc > Parent.JK_ConsolCutOffDate)
						|| (x.JS_ActualVolumeModifiedDateUtc.IsValid && x.JS_ActualVolumeModifiedDateUtc > Parent.JK_ConsolCutOffDate)
						|| (x.JS_UnitOfVolumeModifiedDateUtc.IsValid && x.JS_UnitOfVolumeModifiedDateUtc > Parent.JK_ConsolCutOffDate)))
			{
				if (Env.Security.ConsolChangeWeightOrVolumeAfterCutOffDate.IsAllowed)
				{
					Parent.JK_ConsolCutOffDateInfo.AddWarning(Res.GetString("03ef73e4-0e66-4448-a02f-6b3e742c22e1", "You have modified Shipment's weight or volume after the Consol Cut Off Date."));
				}
				else
				{
					Parent.JK_ConsolCutOffDateInfo.AddError(Res.GetString("3bab4c16-b72d-4e77-8217-c15fe0c8c635",
						"You have modified Shipment's weight or volume after the Consol Cut Off Date. Supervisor access is required to save the changes at this time.\r\n\r\n{0}",
						Env.Security.GetErrorMessageForNotAllowed(Env.Security.ConsolChangeWeightOrVolumeAfterCutOffDate)));
				}
			}
		}

		void AddNotificationForLateConsolsOrShipments<T>(INotificationType notificationType, string notificationMessage, Dictionary<ZGuid, ZDateTime> sessionObjects, string newBizoDesc)
			where T : class, IJobNumber, IConsolOrShipment
		{
			var message = new StringBuilder();

			foreach (var sessionObjectKeyValuePair in sessionObjects)
			{
				if (sessionObjectKeyValuePair.Value > Parent.JK_ConsolCutOffDate)
				{
					var bizo = Parent.Factory.Load<T>(sessionObjectKeyValuePair.Key) as IJobNumber;
					if (bizo != null)
					{
						message.AppendLine(bizo.JobNumber == ZString.Empty ? newBizoDesc : bizo.JobNumber);
					}
				}
			}

			if (message.Length > 0)
			{
				message.Insert(0, notificationMessage + "\r\n");
				Parent.JK_ConsolCutOffDateInfo.AddNotification(notificationType, message.ToString());
			}
		}

		#region Is Hazardous Flags

		protected override void CheckJK_IsHazardous()
		{
			base.CheckJK_IsHazardous();

			if (!FreightConfigurationRegistry.Instance.IsConsolDangerousGoodsValidationEnabled.Value)
			{
				return;
			}

			var shipmentsWithDangerousGoods = Parent
				.Shipments.OfType<ForwardingShipment>().Where(s => s.IsHazardous).ToList();

			if (!Parent.JK_IsHazardous)
			{
				if (Parent.ConsolDGRestrictionCollection.Any())
				{
					Parent.JK_IsHazardousInfo.AddError(Res.GetString("519c2583-0669-f298-4b31-6486c016d55c", "Is Hazardous Flag should be set when the Consol accepts DG Classes or DG Substances."));
				}
				else if (shipmentsWithDangerousGoods.Count > 0)
				{
					Parent.JK_IsHazardousInfo.AddError(Res.GetString("1014f0a9-f7fe-e893-4c14-b16450306d72", @"Is Hazardous Flag should be set when there are shipments with dangerous goods attached.
The following Shipment(s) attached to this consol have dangerous cargo: {0}", string.Join(System.Environment.NewLine, shipmentsWithDangerousGoods.Select(s => s.HumanReadableName).ToArray())));
				}

				return;
			}

			if (!Parent.ConsolDGRestrictionCollection.Any())
			{
				return;
			}

			var invalidShipments = new List<ForwardingShipment>();
			foreach (var shipment in shipmentsWithDangerousGoods)
			{
				if (!Parent.ShipmentDangerousGoodsIsCompatible(shipment))
				{
					invalidShipments.Add(shipment);
				}
			}

			if (invalidShipments.Count > 0)
			{
				Parent.JK_IsHazardousInfo.AddError(Res.GetString("146e2801-9f7f-d180-4b6c-ee08ba54ce86", "The following Shipment(s) attached to this consol have dangerous cargo that is not accepted: {0}", string.Join(System.Environment.NewLine, invalidShipments.Select(s => s.HumanReadableName).ToArray())));
			}
		}

		#endregion

		#endregion

		#region Custom Dates

		protected override void CheckJK_CustomDate1()
		{
			base.CheckJK_CustomDate1();
			CustomLabelPropertyValidation.Validate(new ForwardingConsol.CustomLabelsProvider(GlbCompany.CurrentCompany.OrgProxy), Parent.JK_CustomDate1Info);
		}

		protected override void CheckJK_CustomDate2()
		{
			base.CheckJK_CustomDate2();
			CustomLabelPropertyValidation.Validate(new ForwardingConsol.CustomLabelsProvider(GlbCompany.CurrentCompany.OrgProxy), Parent.JK_CustomDate2Info);
		}

		#endregion

		#region Forwarder Addresses

		protected override void CheckJK_OA_SendingForwarderAddress()
		{
			base.CheckJK_OA_SendingForwarderAddress();
			if (Parent.JK_OA_SendingForwarderAddressInfo.HasChanges)
			{
				AddErrorWhenForwarderGatewayUnflaggedButGatewayJobExists(Parent.JK_OA_SendingForwarderAddressInfo, Parent.JK_SendingForwarderHandlingTypeInfo, GatewayBillingSupporter.IsSendingAgentGateway, GatewayBillingSupporter.IsReceivingAgentGateway);
			}
		}

		protected override void CheckJK_OA_ReceivingForwarderAddress()
		{
			base.CheckJK_OA_ReceivingForwarderAddress();
			if (Parent.JK_OA_ReceivingForwarderAddressInfo.HasChanges)
			{
				AddErrorWhenForwarderGatewayUnflaggedButGatewayJobExists(Parent.JK_OA_ReceivingForwarderAddressInfo, Parent.JK_ReceivingForwarderHandlingTypeInfo, GatewayBillingSupporter.IsReceivingAgentGateway, GatewayBillingSupporter.IsSendingAgentGateway);
			}
		}

		void AddErrorWhenForwarderGatewayUnflaggedButGatewayJobExists(ZPropertyInfo forwarderAddressTypeInfoToCheck, ZPropertyInfo forwarderHandlingTypeInfoToCheck, Func<GlbCompany, ZGuid, ZString?, bool> isGatewayForCurrentAgent, Func<GlbCompany, ZGuid, ZString?, bool> isGatewayForOtherAgent)
		{
			if (forwarderAddressTypeInfoToCheck.OriginalValue is ZGuid originalForwarderAddress && !originalForwarderAddress.IsEmpty)
			{
				var originalForwarderHandlingType = (ZString)forwarderHandlingTypeInfoToCheck.OriginalValue;
				var newForwarderAddress = (ZGuid)forwarderAddressTypeInfoToCheck.Value;
				foreach (var company in GatewayBillingSupporter.AllGlbCompanies)
				{
					if (GatewayBillingSupporter.GetConsolJob(company) != null
						&& !isGatewayForCurrentAgent(company, newForwarderAddress, null)
						&& !isGatewayForOtherAgent(company, ZGuid.Empty, null)
						&& isGatewayForCurrentAgent(company, originalForwarderAddress, originalForwarderHandlingType))
					{
						AddGatewayJobExistsErrorForForwarderType(forwarderAddressTypeInfoToCheck, company.GC_Code);
						break;
					}
				}
			}
		}

		void AddGatewayJobExistsErrorForForwarderType(ZPropertyInfo propertyInfo, ZString companyCode)
		{
			var message = Res.GetString("92192E03-34F2-40F7-BC2D-B1505B1BB960",
				"Unable to change the {0}. Gateway Billing Job exists in the Gateway Agent's login Company {1}.",
				propertyInfo.HumanReadableName, companyCode);
			propertyInfo.AddError(message);
		}

		#endregion

		#region Co-Loader / Creditor

		protected override void CheckJK_OA_CreditorAddressIsValidZGuid()
		{
			if (!Parent.JK_OA_CreditorAddress.IsEmpty && !Parent.JK_OA_CreditorAddress.IsValid)
			{
				if (Parent.JK_AgentType == Core.Constants.AgentType.CoLoad)
				{
					if (!Parent.JK_SendingForwarderHandlingType.IsEmpty || !Parent.JK_ReceivingForwarderHandlingType.IsEmpty)
					{
						Parent.JK_OA_CreditorAddressInfo.AddError(Res.GetString("536e04f7-893b-4a9e-94c7-c02ba28516f7", "Enter a valid Gateway Co-Loader."));
					}
					else
					{
						Parent.JK_OA_CreditorAddressInfo.AddError(Res.GetString("b3da1207-ee9b-4747-a78f-026c2680facf", "Enter a valid Co-Loader."));
					}
				}
				else
				{
					Parent.JK_OA_CreditorAddressInfo.AddError(Res.GetString("2b52cc26-def5-413b-89a1-f85b805bcf27", "Enter a valid Creditor."));
				}
			}
		}

		#endregion

		#region JK_JX_JA_E_DEP

		public void ValidateJK_JX_JA_E_DEP()
		{
			ValidateCalculatedProperty(Parent.JK_JX_JA_E_DEPInfo);
		}

		protected virtual void CheckJK_JX_JA_E_DEP()
		{
			if (!Parent.IsValidationSuspended
				&& IsCreatingFromTemplate()
				&& Parent.JK_JX_JA_E_DEP.IsValid
				&& Parent.JK_JX_JA_E_DEP < ZDateTime.Today)
			{
				Parent.JK_JX_JA_E_DEPInfo.AddWarning(Res.GetString("6523ae5b-44ab-44bf-a433-68daf018c82f", "Flight dates must be today or later."));
			}
		}

		#endregion

		#region JK_JX_JB_E_LastARV

		public void ValidateJK_JX_JB_E_LastARV()
		{
			ValidateCalculatedProperty(Parent.JK_JX_JB_E_LastARVInfo);
		}

		protected virtual void CheckJK_JX_JB_E_LastARV()
		{
			if (!Parent.IsValidationSuspended
				&& IsCreatingFromTemplate()
				&& Parent.JK_JX_JB_E_LastARV.IsValid
				&& Parent.JK_JX_JB_E_LastARV < ZDateTime.Today)
			{
				Parent.JK_JX_JB_E_LastARVInfo.AddWarning(Res.GetString("5ebdf00d-8bee-47bc-9322-30fbaff59b5d", "Flight dates must be today or later."));
			}
		}

		#endregion

		#region JK_OA_ShippingLineAddress

		protected override void CheckJK_OA_ShippingLineAddress()
		{
			base.CheckJK_OA_ShippingLineAddress();

			if (!Parent.IsValidationSuspended
				&& IsCreatingFromTemplate())
			{
				var consolProvider = Parent.TemplateRecordProviderConsol;

				if (!consolProvider.JK_OA_ShippingLineAddress.IsEmpty
					&& !Parent.Transports.Cast<Transport>().Any(t => t.JW_OA_CarrierAddress == consolProvider.JK_OA_ShippingLineAddress))
				{
					Parent.JK_OA_ShippingLineAddressInfo.AddWarning(Res.GetString("7578cee4-475f-43b0-8e30-9da3cc8d82a7", "If the carrier is populated on the template, it may only be selected for that carrier."));
				}
			}
		}

		#endregion

		#region JK_CarrierContractNumber

		protected override void CheckJK_CarrierContractNumber()
		{
			base.CheckJK_CarrierContractNumber();

			if (Parent.IsValidationSuspended
				|| Parent.JK_CarrierContractNumber.IsEmpty
				|| !ObjectFactory.Get<IContractPermissions>().IsCarrierAndClientContractModulesEnabled())
			{
				return;
			}

			var validationData = Parent as ICCACommonAssignmentValidationData;

			if (!CCAContractValidationHelper.DoesContractNumberExistWithAnyCarrier(Parent.JK_CarrierContractNumber, Parent.Factory))
			{
				Parent.JK_CarrierContractNumberInfo.AddWarning(CCAValidationMessageProvider.MissingContractRecord());
			}
			else if (!CCAContractValidationHelper.DoesContractNumberExistWithCarrier(Parent.JK_CarrierContractNumber, Parent.ShippingLine, Parent.Factory))
			{
				var message = validationData.ContractServiceProvider?.PK.IsEmpty ?? true
					? CCAValidationMessageProvider.MissingCarrierOnJob(validationData)
					: CCAValidationMessageProvider.CarrierContractMismatch(validationData);

				var anyContainersAssignedToAllocationRoutes = validationData
					.Containers
					.Any(container => !container.JC_RCA_AllocationLine.IsEmpty);

				if (anyContainersAssignedToAllocationRoutes || validationData.AllocationRoute != null)
				{
					Parent.JK_CarrierContractNumberInfo.AddError(message);
				}
				else
				{
					Parent.JK_CarrierContractNumberInfo.AddWarning(message);
				}
			}

			if (Parent.CarrierContract is not RatingContract contract)
			{
				return;
			}

			if (!CCAContractValidationHelper.IsConsolValidForContractValidDateRanges(contract, Parent))
			{
				Parent.JK_CarrierContractNumberInfo.AddError(CCAValidationMessageProvider.InvalidConsolDatesForContract(contract));
			}

			if (CCAContractValidationHelper.AreAnyContainerTypesInvalidForContract(contract, validationData))
			{
				Parent.JK_CarrierContractNumberInfo.AddError(CCAValidationMessageProvider.ContainerTypesInvalid(contract, validationData));
			}

			if (CCAContractValidationHelper.AreAnyContainerCommoditiesInvalidForNonHazardousContract(contract, validationData))
			{
				Parent.JK_CarrierContractNumberInfo.AddError(CCAValidationMessageProvider.HazardousContainerCommoditiesInvalid(contract, validationData));
			}

			if (!CCAContractValidationHelper.DoesJobTransportModeMatchContract(contract, validationData))
			{
				Parent.JK_CarrierContractNumberInfo.AddError(CCAValidationMessageProvider.InvalidTransportMode(contract, validationData));
			}

			if (CCAContractValidationHelper.IsConsolInvalidForContractNamedAccounts(contract, Parent))
			{
				Parent.JK_CarrierContractNumberInfo.AddWarning(CCAValidationMessageProvider.InvalidConsolForContractNamedAccounts(contract));
			}

			if (CCAContractValidationHelper.IsConsolInvalidForNonHazardousContract(contract, Parent))
			{
				Parent.JK_CarrierContractNumberInfo.AddError(CCAValidationMessageProvider.InvalidConsolForNonHazardousContract(contract, Parent));
			}
		}

		#endregion

		#region JK_RCA_AllocationLine

		protected override void CheckJK_RCA_AllocationLine()
		{
			base.CheckJK_RCA_AllocationLine();

			if (Parent.IsValidationSuspended || Parent.JK_RCA_AllocationLine.IsEmpty)
			{
				return;
			}

			if (CCARouteValidationHelper.IsRouteAssignmentMissingCarrierContract(Parent))
			{
				Parent.JK_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.MissingContractWhenAllocatedToRoute(Parent));
				return;
			}

			if (CCARouteValidationHelper.IsInvalidAllocationRouteAssigned(Parent.JK_RCA_AllocationLine, Parent.Factory))
			{
				Parent.JK_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidAllocationRoute(Parent));
				return;
			}

			if (!CCARouteValidationHelper.IsRouteAssignedUnderParentContractAssigned(Parent.JK_RCA_AllocationLine, Parent.CarrierContract))
			{
				Parent.JK_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.AllocationRouteAndContractMismatch(Parent));
				return;
			}

			var allocationRoute = Parent.AllocationLine;

			if (!Parent.AllocationRouteContainerWeightLimitHelper.CheckContainerWeightLimit(allocationRoute))
			{
				Parent.JK_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.GrossContainerWeightExceedsAllocationLimit(allocationRoute));
			}

			if (allocationRoute.RCA_JX_SailingSchedule.IsEmpty)
			{
				ValidateConsolForUnlinkedAllocationRoute(allocationRoute);
			}
			else
			{
				ValidateConsolForLinkedAllocationRoute(allocationRoute);
			}

			if (allocationRoute.RCA_HasBookingLimit)
			{
				var outstandingUtilization = ContractAllocationHelper.CalculateOutstandingUtilisation(Parent.Factory, allocationRoute);
				if (outstandingUtilization < 0)
				{
					if (allocationRoute.RCA_AllocatedUQ.EqualsIgnoringCase(Constants.AllocationQuantityUnits.Containers))
					{
						Parent.JK_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.ContainerBookingLimitExceeded(allocationRoute, -outstandingUtilization));
					}
					else
					{
						Parent.JK_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.TEUBookingLimitExceeded(allocationRoute, -outstandingUtilization));
					}
				}
			}

			if (allocationRoute.RCA_AllowGatewayConsolOnly
				&& !CCARouteValidationHelper.IsGatewayAgentAssigned(Parent))
			{
				Parent.JK_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.MissingGatewayAgent(allocationRoute));
			}

			if (allocationRoute.RCA_AllowGroupageOnly
				&& !Parent.IsGroupage)
			{
				Parent.JK_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.GroupageContainerModeConsolsOnly(allocationRoute, Parent));
			}

			if (!CCARouteValidationHelper.IsValidForwarderForAllocationRouteAgents(Parent, allocationRoute))
			{
				if ((Parent.SendingForwarder != null && Parent.ReceivingForwarder != null)
					|| (Parent.SendingForwarder == null && Parent.ReceivingForwarder == null))
				{
					Parent.JK_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidConsolForAllocationRouteAgents(allocationRoute, Parent, Parent.SendingForwarder, Parent.ReceivingForwarder));
				}
				else
				{
					var forwarder = Parent.SendingForwarder is null
						? Res.GetString("6b31e0f0-12ef-eb8f-40e7-10554b737368", $"Receiving Agent {Parent.ReceivingForwarder.OH_Code}")
						: Res.GetString("68f654d1-9513-0e89-4170-e8773005212c", $"Sending Agent {Parent.SendingForwarder.OH_Code}");

					Parent.JK_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidConsolForAllocationRouteAgents(allocationRoute, Parent, forwarder));
				}
			}

			if (CCARouteValidationHelper.IsConsolInvalidForAllocationRouteNamedAccounts(allocationRoute, Parent))
			{
				Parent.JK_RCA_AllocationLineInfo.AddWarning(CCAValidationMessageProvider.InvalidConsolForAllocationRouteNamedAccounts(allocationRoute));
			}

			if (!allocationRoute.RCA_RC_ContainerType.IsEmpty
				&& CCARouteValidationHelper.JobHasContainersNotMatchingAllocationContainerType(allocationRoute, Parent))
			{
				Parent.JK_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidContainerCodeInCollection(allocationRoute, Parent));
			}
			else if (!allocationRoute.RCA_StorageOrFreightRateClass.IsEmpty
				&& CCARouteValidationHelper.JobHasContainersNotMatchingAllocationContainerType(allocationRoute, Parent))
			{
				Parent.JK_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidContainerClassInCollection(allocationRoute, Parent));
			}

			if (FreightConfigurationRegistry.Instance.EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes.Value)
			{
				if (!allocationRoute.RCA_PlaceOfReceipt.IsEmpty && !CCARouteValidationHelper.IsConsolFirstLoadValidForPlaceOfReceipt(allocationRoute, Parent))
				{
					Parent.JK_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidPlaceOfReceiptAndFirstLoadForConsol(allocationRoute, Parent));
				}

				if (!allocationRoute.RCA_PlaceOfDelivery.IsEmpty && !CCARouteValidationHelper.IsConsolDischargeValidForPlaceOfDelivery(allocationRoute, Parent))
				{
					Parent.JK_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidPlaceOfDeliveryAndLastDischargeForConsol(allocationRoute, Parent));
				}
			}
		}

		void ValidateConsolForLinkedAllocationRoute(IRatingContractAllocationLine allocationRoute)
		{
			if (CCARouteValidationHelper.DoAnyConsolTransportLegsMatchAllocationRouteSchedule(allocationRoute, Parent))
			{
				return;
			}

			Parent.JK_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidConsolForLinkedAllocationRoute(allocationRoute));
		}

		void ValidateConsolForUnlinkedAllocationRoute(IRatingContractAllocationLine allocationRoute)
		{
			if (!CCARouteValidationHelper.IsConsolValidForRouteLoadLocation(allocationRoute, Parent))
			{
				Parent.JK_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidLoadPortForConsol(allocationRoute, Parent));
			}

			if (!CCARouteValidationHelper.IsConsolValidForRouteDischargeLocation(allocationRoute, Parent))
			{
				Parent.JK_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidDischargePortForConsol(allocationRoute, Parent));
			}

			if (!CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, Parent))
			{
				if (CCARouteValidationHelper.BookingEmptyAndMBLEmptyOrAllocationChanged(Parent))
				{
					Parent.JK_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidConsolDatesForAllocationRoute(allocationRoute));
				}
				else
				{
					Parent.JK_RCA_AllocationLineInfo.AddWarning(CCAValidationMessageProvider.InvalidConsolETDForAllocationRoute(allocationRoute, Parent));
				}
			}

			if (!CCARouteValidationHelper.IsConsolValidForRouteScheduleDetails(allocationRoute, Parent))
			{
				Parent.JK_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidScheduleDetailsOnConsol(allocationRoute, Parent));
			}
		}

		#endregion

		#region JK_ElectronicBillOfLadingTerms

		protected override void CheckJK_ElectronicBillOfLadingTerms()
		{
			base.CheckJK_ElectronicBillOfLadingTerms();
			ListValidation.ErrorIfInvalidCode(Parent.JK_ElectronicBillOfLadingTermsInfo);
		}

		#endregion

		#region JK_ElectronicBillOfLadingType

		protected override void CheckJK_ElectronicBillOfLadingType()
		{
			base.CheckJK_ElectronicBillOfLadingType();
			ListValidation.ErrorIfInvalidCode(Parent.JK_ElectronicBillOfLadingTypeInfo);
		}

		#endregion

		#region CheckJK_OA_PackDepotAddress

		protected override void CheckJK_OA_PackDepotAddress()
		{
			base.CheckJK_OA_PackDepotAddress();

			if (AdvOrmFeatureHelper.IsEnabled && Parent.Containers.OfType<ForwardingContainer>().Any(container => container.ContainerLoadPlan != null && container.ContainerLoadPlan.CLH_OA_CFSAddress != Parent.JK_OA_PackDepotAddress))
			{
				Parent.JK_OA_PackDepotAddressInfo.AddWarning(Res.GetString("11bf4bd3-707b-4c79-838c-b90e6531d5ce", "The consolidation departure CFS address does not match the CFS address on the linked container load plan."));
			}
		}

		#endregion

		#region TotalCO2eForBinding

		public void ValidateTotalCO2eForBinding()
		{
			ValidateCalculatedProperty(Parent.TotalCO2eForBindingInfo);
		}

		protected void CheckTotalCO2eForBinding()
		{
			Parent.CheckCO2eStatus(Parent.TotalCO2eForBindingInfo);
		}

		#endregion

		#region TotalCO2eForSorting

		public void ValidateTotalCO2eForSorting()
		{
			ValidateCalculatedProperty(Parent.TotalCO2eForSortingInfo);
		}

		protected void CheckTotalCO2eForSorting()
		{
			Parent.CheckCO2eStatus(Parent.TotalCO2eForSortingInfo);
		}

		#endregion

		#region Calculated

		#region JK_CRN

		protected override void CheckJK_CRN()
		{
			base.CheckJK_CRN();

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Iceland)
			{
				Parent.SendingarnumerHelper.Validate();
			}
		}

		#endregion

		#region MasterBillMAWB

		public void ValidateMasterBillMAWB()
		{
			ValidateCalculatedProperty(Parent.MasterBillMAWBInfo);
		}

		public void ValidateMasterBillNeutralMAWB()
		{
			ValidateCalculatedProperty(Parent.MasterBillNeutralMAWBInfo);
		}

		bool IsDuplicateMAWB()
		{
			return MasterBillValidator.IsDuplicate(Parent,
				Parent.JK_SystemCreateTimeUtc.IsValid ? Parent.JK_SystemCreateTimeUtc.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value) : ZDateTime.UtcNow.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value),
				Parent.JK_SystemCreateTimeUtc.IsValid ? Parent.JK_SystemCreateTimeUtc.AddMonths(FreightDataRegistry.Instance.MAWBRecyclePeriod.Value) : ZDateTime.Empty);
		}

		protected virtual void CheckMasterBillMAWB()
		{
			if (Parent.IsAir)
			{
				MasterBillValidator.ValidateMAWB(Parent, IsDuplicateMAWB);
			}
		}

		protected virtual void CheckMasterBillNeutralMAWB()
		{
			if (Parent.IsAir)
			{
				MasterBillValidator.ValidateNeutralMAWB(Parent);
			}
		}

		#endregion

		public void ValidateWeightVerificationUnit()
		{
			ValidateCalculatedProperty(Parent.WeightVerificationUnitInfo);
		}

		protected virtual void CheckWeightVerificationUnit()
		{
			if (Parent.JK_TotalShipmentActWeightCheck != 0 && Parent.WeightVerificationUnit.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.WeightVerificationUnitInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.WeightVerificationUnitInfo, Parent.WeightUnits);
		}

		public void ValidateVolumeVerificationUnit()
		{
			ValidateCalculatedProperty(Parent.VolumeVerificationUnitInfo);
		}

		protected virtual void CheckVolumeVerificationUnit()
		{
			if (Parent.JK_TotalShipmentActVolumeCheck != 0 && Parent.VolumeVerificationUnit.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.VolumeVerificationUnitInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.VolumeVerificationUnitInfo, Parent.VolumeUnits);
		}

		// TODO: Is this used?
		public void ValidateMasterBillAirlinePrefix()
		{
			ValidateCalculatedProperty(Parent.MasterBillAirlinePrefixInfo);
		}

		protected virtual void CheckMasterBillAirlinePrefix()
		{
			if (Parent.IsAir && Parent.JK_JX_JV_VoyageFlight.Length >= 2)
			{
				RefAirline airline = RefAirline.LoadFromAirline2LetterCode(Parent.Factory, Parent.TwoLetterAirlineCode);
				if (airline != null && airline.RM_EagleAddedAirlinePrefixOrAccountingCode != Parent.MasterBillAirlinePrefix)
				{
					Parent.MasterBillAirlinePrefixInfo.AddWarning(Res.GetString("5ea415c7-f507-499d-9419-f4c4296c2456", "The Airline Prefix does not match the Airline 2 Letter Code in the Flight Number."));
				}
			}
		}

		public void ValidateAWBCurrentStatus()
		{
			ValidateCalculatedProperty(Parent.AWBCurrentStatusInfo);
		}

		protected virtual void CheckAWBCurrentStatus()
		{
			if (Parent.AWBCurrentStatus.Contains("ERROR", StringComparison.OrdinalIgnoreCase))
			{
				Parent.AWBCurrentStatusInfo.AddMessageError(Res.GetString("3930617a-618c-43f3-8d4a-75e76e2fc5e9", "{0}\r\nMessage Type FNA is an error message.", Parent.AWBCurrentStatus));
			}
		}

		public void ValidateJK_CustomString1()
		{
			ValidateCalculatedProperty(Parent.JK_CustomString1Info);
		}

		protected virtual void CheckJK_CustomString1()
		{
			CustomLabelPropertyValidation.Validate(new ForwardingConsol.CustomLabelsProvider(GlbCompany.CurrentCompany.OrgProxy), Parent.JK_CustomString1Info);
		}

		public void ValidateJK_CustomString2()
		{
			ValidateCalculatedProperty(Parent.JK_CustomString2Info);
		}

		protected virtual void CheckJK_CustomString2()
		{
			CustomLabelPropertyValidation.Validate(new ForwardingConsol.CustomLabelsProvider(GlbCompany.CurrentCompany.OrgProxy), Parent.JK_CustomString2Info);
		}

		public void ValidateJK_CustomNumber1()
		{
			ValidateCalculatedProperty(Parent.JK_CustomNumber1Info);
		}

		protected virtual void CheckJK_CustomNumber1()
		{
			CustomLabelPropertyValidation.Validate(new ForwardingConsol.CustomLabelsProvider(GlbCompany.CurrentCompany.OrgProxy), Parent.JK_CustomNumber1Info);
		}

		public void ValidateJK_CustomNumber2()
		{
			ValidateCalculatedProperty(Parent.JK_CustomNumber2Info);
		}

		protected virtual void CheckJK_CustomNumber2()
		{
			CustomLabelPropertyValidation.Validate(new ForwardingConsol.CustomLabelsProvider(GlbCompany.CurrentCompany.OrgProxy), Parent.JK_CustomNumber2Info);
		}

		public void ValidateSecurityStatusCode()
		{
			ValidateCalculatedProperty(Parent.SecurityStatusCodeInfo);
		}

		protected void CheckSecurityStatusCode()
		{
			ListValidation.ErrorIfInvalidCode(Parent.SecurityStatusCodeInfo);

			var consolSecurityStatusValidationHelper = new ConsolSecurityStatusValidationHelper(Parent);
			var securityStatusCode = Parent.SecurityStatusCode;
			consolSecurityStatusValidationHelper.CheckSecurityStatusCode(securityStatusCode, Parent.SecurityStatusCodeInfo);
			consolSecurityStatusValidationHelper.ValidateSpecialHandlingCodeForUncertifiedUser(Parent.SecurityStatusCode, Parent.SecurityStatusCodeInfo, Parent.SecurityStatusCodeHasChanges);

			var defaultCode = Parent.GetAviationSecurityCode();
			if (defaultCode != ZString.Empty && defaultCode != securityStatusCode)
			{
				Parent.SecurityStatusCodeInfo.AddWarning(Res.GetString("cfb10f29-f7d7-4187-b2e6-d45b7ce20fa9", "This status has been overridden and does not match attached Shipments' overall Inspection status of '{0}'.", defaultCode));
			}
		}

		/// <summary>
		/// Validate calculated field AutoratingDate on a consol
		/// </summary>
		public void ValidateAutoratingDate()
		{
			ValidateCalculatedProperty(Parent.AutoratingDateInfo);
		}

		/// <summary>
		/// CheckXXX method is called automatically by validation framework when ValidateXXX is called.
		/// </summary>
		protected void CheckAutoratingDate()
		{
			TypeValidation.CheckValidSmallDateTime(Parent.AutoratingDateInfo);
			TypeValidation.CheckValidZDateTimeRange(Parent.AutoratingDateInfo);
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateMasterBillMAWB();
			ValidateWeightVerificationUnit();
			ValidateVolumeVerificationUnit();
			ValidateMasterBillAirlinePrefix();
			ValidateAWBCurrentStatus();

			ValidateJK_CustomString1();
			ValidateJK_CustomString2();
			ValidateJK_CustomNumber1();
			ValidateJK_CustomNumber2();
			ValidateSecurityStatusCode();

			ValidateJK_JX_JA_E_DEP();
			ValidateJK_JX_JB_E_LastARV();

			ValidateTotalCO2eForBinding();
		}

		#endregion

		#region Implementation

		bool IsCreatingFromTemplate() => !Parent.IsTemplate && Parent.TemplateRecord != null;

		public new ForwardingConsol Parent
		{
			get { return (ForwardingConsol)base.Parent; }
		}

		protected CustomLabelPropertyValidation CustomLabelPropertyValidation
		{
			get { return customLabelPropertyValidation ?? (customLabelPropertyValidation = new CustomLabelPropertyValidation(Parent.Factory)); }
		}
		CustomLabelPropertyValidation customLabelPropertyValidation;

		#endregion
	}
}
