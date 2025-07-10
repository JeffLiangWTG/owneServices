using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Forwarding.Registry;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ConsolExportAWBHeaderValidation : ExportAWBHeaderValidation
	{
		public ConsolExportAWBHeaderValidation(ConsolExportAWBHeader parent)
			: base(parent)
		{
		}

		public new ConsolExportAWBHeader Parent
		{
			get { return (ConsolExportAWBHeader)base.Parent; }
		}

		#region EH_ShippersSignature

		protected override void CheckEH_ShippersSignature()
		{
			base.CheckEH_ShippersSignature();

			var awbHeader = Parent;
			if (awbHeader != null)
			{
				var maxLength = awbHeader.ShippersSignatureMaxLength;
				if (awbHeader.EH_ShippersSignature.Length > maxLength)
				{
					Parent.EH_ShippersSignatureInfo.AddWarning(Res.GetString("217c6ef0-f62b-4c7e-bed5-8eb7c1d9b2c8", "Signature in FWB message cannot exceed {0} characters and therefore will be truncated for messaging purposes!", maxLength));
				}
			}
		}

		#endregion

		#region EH_AsAgreed

		static string GeneralAsAgreedNotAllowedMessage => Res.GetString("4432a9e0-006c-4168-bf89-23df2e4d6650", "As Agreed cannot be selected for Master Bill");

		protected override void CheckEH_AsAgreed1st()
		{
			base.CheckEH_AsAgreed1st();
			var awbHeader = Parent;
			if (awbHeader != null && (!awbHeader.AsAgreedAllowed && awbHeader.EH_AsAgreed1st != Core.Constants.AWB.AsAgreedTypes.Codes.None))
			{
				Parent.EH_AsAgreed1stInfo.AddError(GeneralAsAgreedNotAllowedMessage);
			}
		}

		protected override void CheckEH_AsAgreed2nd()
		{
			base.CheckEH_AsAgreed2nd();
			var awbHeader = Parent;
			if (awbHeader != null && (!awbHeader.AsAgreedAllowed && awbHeader.EH_AsAgreed2nd != Core.Constants.AWB.AsAgreedTypes.Codes.None))
			{
				Parent.EH_AsAgreed2ndInfo.AddError(GeneralAsAgreedNotAllowedMessage);
			}
		}

		#endregion

		#region EH_AWBIssueDate

		protected override void CheckEH_AWBIssueDate()
		{
			base.CheckEH_AWBIssueDate();

			if (NotificationLevelForFieldsRequiredForElectronicTransmission == CargoWise.EntityFramework.NotificationType.MessageError)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.EH_AWBIssueDateInfo);
			}
			else
			{
				MandatoryValidation.WarnIfNotEntered(Parent.EH_AWBIssueDateInfo);
			}

			if (Parent.EH_AWBIssueDate > ZDateTime.Now)
			{
				Parent.EH_AWBIssueDateInfo.AddWarning(Res.GetString("68c97be9-b246-4060-b896-3fdff718b3de", "Be aware that some airlines reject eAWB that are future dated, which may have operational impacts."));
			}
		}

		#endregion

		#region EH_Currency

		protected override void CheckEH_Currency()
		{
			base.CheckEH_Currency();
			//TODO brendon check if this currency is part of our list - or put a findbox on the screen
			ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_CurrencyInfo);
			ValidateFieldLengthForElectronicTransmission(Parent.EH_CurrencyInfo, 3);
			ValidateFieldFormatForElectronicTransmission(Parent.EH_CurrencyInfo, CIMPFieldFormats.Alpha);
		}

		#endregion

		#region EH_ChargesCode

		protected override void CheckEH_ChargesCode()
		{
			base.CheckEH_ChargesCode();
			ValidateFieldLengthForElectronicTransmission(Parent.EH_ChargesCodeInfo, 2);
			ListValidation.ErrorIfInvalidCode(Parent.EH_ChargesCodeInfo, Parent.ChargeCodesList);
		}

		#endregion

		#region EH_AWBIssuePlace

		protected override void CheckEH_AWBIssuePlace()
		{
			base.CheckEH_AWBIssuePlace();
			ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_AWBIssuePlaceInfo, CIMPFieldFormats.Text);
		}

		#endregion

		#region EH_ECNCRNNumber

		protected override void CheckEH_ECNCRNNumber()
		{
			base.CheckEH_ECNCRNNumber();

			if (Parent.IsDirectMAWB && Parent.Consol.ShipmentCount > 0)
			{
				var shipment = Parent.Consol.Shipments[0];
				if (shipment.CusEntryNumbers.Count > 0)
				{
					if (shipment.CusEntryNumbers.Count > 4)
					{
						Parent.EH_ECNCRNNumberInfo.AddWarning(Res.GetString("f3dff7a6-7924-4059-aa3e-5520b60a0827", "If more than four entry numbers present on a shipment, they will not be shown on this document but will be included in the FHL and FWB airline messages."));
					}

					shipment.ShipmentCustomsEntryNumber.Validation.ValidateEntryNumber();
					if (shipment.CustomsEntryNumberForBindingInfo.HasMessageErrors() || shipment.CustomsEntryNumberForBindingInfo.HasWarnings())
					{
						shipment.CustomsEntryNumberForBindingInfo.GetMessageErrors().ForEach(e => Parent.EH_ECNCRNNumberInfo.AddWarning(e.Message));
						shipment.CustomsEntryNumberForBindingInfo.GetWarnings().ForEach(e => Parent.EH_ECNCRNNumberInfo.AddWarning(e.Message));
					}
				}
			}
		}

		#endregion

		#region Shipper

		protected override void CheckEH_ShipperName()
		{
			base.CheckEH_ShipperName();
			ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_ShipperNameInfo, Res.GetString("790d793b-5d8d-4168-bde8-7f53b51924e3", "Shipper Name"), CIMPFieldFormats.Text);
		}

		protected override void CheckEH_ShipperAddress()
		{
			base.CheckEH_ShipperAddress();
			ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_ShipperAddressInfo, Res.GetString("bc9aa3d7-dd37-4db2-aafe-b3fba87f7742", "Shipper Address"), CIMPFieldFormats.Text);
		}

		protected override void CheckEH_ShipperPlace()
		{
			base.CheckEH_ShipperPlace();
			ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_ShipperPlaceInfo, Res.GetString("7e8c0b6d-14ea-4cf9-8131-2854a901554a", "Shipper City"), CIMPFieldFormats.Text);
		}

		protected override void CheckEH_ShipperCountryCode()
		{
			base.CheckEH_ShipperCountryCode();
			ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_ShipperCountryCodeInfo, Res.GetString("137fc6d9-c9c8-4215-9700-e14b24030f94", "Shipper Country/Region Code"));
		}

		#endregion

		#region Consignee

		protected override void CheckEH_ConsigneeContactDetail()
		{
			base.CheckEH_ConsigneeContactDetail();

			var awbHeader = Parent;
			if (awbHeader != null && !awbHeader.IsToOrderConsignee && awbHeader.EH_ConsigneeContactDetail.IsEmpty)
			{
				Parent.EH_ConsigneeContactDetailInfo.AddWarning(Res.GetString("e4ae2c59-d1ef-4759-987b-d8cd4729de31", "Consignee communication number is empty."));
			}
		}

		protected override void CheckEH_ConsigneeName()
		{
			base.CheckEH_ConsigneeName();
			ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_ConsigneeNameInfo, Res.GetString("c7ef25ca-76d0-4731-9b23-807b8fefd63d", "Consignee Name"), CIMPFieldFormats.Text);
		}

		protected override void CheckEH_ConsigneeAddress()
		{
			base.CheckEH_ConsigneeAddress();
			ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_ConsigneeAddressInfo, Res.GetString("c41de3f7-298e-43bd-9789-624285948fdc", "Consignee Address"), CIMPFieldFormats.Text);
		}

		protected override void CheckEH_ConsigneePlace()
		{
			base.CheckEH_ConsigneePlace();
			ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_ConsigneePlaceInfo, Res.GetString("7777717f-aff1-4cd4-a5cc-458a4834628f", "Consignee City"), CIMPFieldFormats.Text);
		}

		protected override void CheckEH_ConsigneeCountryCode()
		{
			base.CheckEH_ConsigneeCountryCode();
			ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_ConsigneeCountryCodeInfo, Res.GetString("ab810471-97d8-43e4-8060-ed3090a77d56", "Consignee Country/Region Code"));
		}

		#endregion

		#region EH_By & EH_To

		protected override void CheckEH_By1st()
		{
			base.CheckEH_By1st();
			ValidateFieldLengthForElectronicTransmission(Parent.EH_By1stInfo, 2);
			ValidateFieldFormatForElectronicTransmission(Parent.EH_By1stInfo, CIMPFieldFormats.AlphaNumeric);
		}

		protected override void CheckEH_To1st()
		{
			base.CheckEH_To1st();

			if (!Parent.EH_To1st.IsEmpty)
			{
				ValidateFieldLengthForElectronicTransmission(Parent.EH_To1stInfo, 3);
				ValidateFieldFormatForElectronicTransmission(Parent.EH_To1stInfo, CIMPFieldFormats.Alpha);
			}
		}

		protected override void CheckEH_By2nd()
		{
			base.CheckEH_By2nd();

			if (!Parent.EH_By2nd.IsEmpty)
			{
				ValidateFieldLengthForElectronicTransmission(Parent.EH_By2ndInfo, 2);
				ValidateFieldFormatForElectronicTransmission(Parent.EH_By2ndInfo, CIMPFieldFormats.AlphaNumeric);
			}
		}

		protected override void CheckEH_To2nd()
		{
			base.CheckEH_To2nd();

			if (!Parent.EH_By2nd.IsEmpty || !Parent.EH_To2nd.IsEmpty)
			{
				ValidateFieldLengthForElectronicTransmission(Parent.EH_To2ndInfo, 3);
				ValidateFieldFormatForElectronicTransmission(Parent.EH_To2ndInfo, CIMPFieldFormats.Alpha);
			}
		}

		protected override void CheckEH_By3rd()
		{
			base.CheckEH_By3rd();

			if (!Parent.EH_By3rd.IsEmpty)
			{
				ValidateFieldLengthForElectronicTransmission(Parent.EH_By3rdInfo, 2);
				ValidateFieldFormatForElectronicTransmission(Parent.EH_By3rdInfo, CIMPFieldFormats.AlphaNumeric);
			}
		}

		protected override void CheckEH_To3rd()
		{
			base.CheckEH_To3rd();

			if (!Parent.EH_By3rd.IsEmpty || !Parent.EH_To3rd.IsEmpty)
			{
				ValidateFieldLengthForElectronicTransmission(Parent.EH_To3rdInfo, 3);
				ValidateFieldFormatForElectronicTransmission(Parent.EH_To3rdInfo, CIMPFieldFormats.Alpha);
			}
		}

		#endregion

		#region Prepaid Collect

		protected override void CheckEH_WeightPrepaidCollect()
		{
			base.CheckEH_WeightPrepaidCollect();
			if (!Parent.EH_OtherPrepaidCollect.IsEmpty && Parent.EH_WeightPrepaidCollect.IsEmpty)
			{
				Parent.EH_WeightPrepaidCollectInfo.AddNotification(NotificationLevelForFieldsRequiredForElectronicTransmission, ErrorIfChargesAreMandatory);
			}
			else if (Parent.EH_TotalLineTotals != 0 || Parent.AWBOtherCharges.Count != 0)
			{
				ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_WeightPrepaidCollectInfo, Res.GetString("cb4876d9-5aa3-46ed-b14a-ec660149fbef", "When there are charges on the AWB, Weight Prepaid/Collect"));
			}
		}

		protected override void CheckEH_OtherPrepaidCollect()
		{
			base.CheckEH_OtherPrepaidCollect();
			if (!Parent.EH_WeightPrepaidCollect.IsEmpty && Parent.EH_OtherPrepaidCollect.IsEmpty)
			{
				Parent.EH_OtherPrepaidCollectInfo.AddNotification(NotificationLevelForFieldsRequiredForElectronicTransmission, ErrorIfChargesAreMandatory);
			}
			else if (Parent.AWBOtherCharges.Count != 0 || Parent.EH_TotalLineTotals != 0)
			{
				ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_OtherPrepaidCollectInfo, Res.GetString("2f176f79-0ddd-4365-9f1f-11ff71494263", "When there are charges on the AWB, Other Prepaid/Collect"));
			}
		}

		static string ErrorIfChargesAreMandatory => Res.GetString("5fb87f92-8339-4309-8c23-d2b071ff1888", "If one of WT/VAL (P/C) or Other (P/C) are entered then both are required");

		#endregion

		#region Also Notify

		public void ValidateMandatoryAlsoNotifyFields()
		{
			ValidateEH_AlsoNotifyName();
			ValidateEH_AlsoNotifyAddress();
			ValidateEH_AlsoNotifyPlace();
			ValidateEH_AlsoNotifyCountryCode();
		}

		protected override void CheckEH_AlsoNotifyName()
		{
			base.CheckEH_AlsoNotifyName();

			if (Parent.EH_AWBType != AWBTypeList.Codes.House)
			{
				var propertyName = Res.GetString("87f17e3f-3045-497b-a2e4-51d759e58152", "Also Notify Name");
				AddInvalidNotificationIfNeed(propertyName, Parent.EH_AlsoNotifyNameInfo, Parent.EH_AlsoNotifyName);

				AddLengthWarningForNameAndAddressIfNeed(propertyName, Parent.EH_AlsoNotifyNameInfo, Parent.EH_AlsoNotifyName);
			}

			if (IsAnyAlsoNotifyDataEntered)
			{
				ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_AlsoNotifyNameInfo,
					Res.GetString("e3c3827d-185e-4565-a40d-1129ba8fd4c7", "When you have an 'Also Notify' party, Name"), CIMPFieldFormats.Text);
			}

			if (Parent.EH_HandlingInformation.Length +
				alsoNotifyBoxLength +
				Parent.EH_SpecialHandlingCode.Length +
				Parent.EH_KnownConsignorCode.Length > 216)
			{
				Parent.EH_AlsoNotifyNameInfo.AddWarning(Res.GetString("0107badc-67cd-4bbe-8814-bc67732ba974",
					"Although the FWB allows for 166 characters of Also Notify information to be sent in the NFY element, when the sum of characters in the Also Notify box, Handling Information box and SCI box values (as well as the Security Status box for SG login company) exceeds 216 characters, the Also Notify information will be omitted from the FWB message (NFY element)."));
			}

			ValidateEH_AlsoNotifyAddress();
			ValidateEH_AlsoNotifyPlace();
			ValidateEH_AlsoNotifyCountryCode();
		}

		int alsoNotifyBoxLength => Parent.EH_AlsoNotifyName.Length +
				Parent.EH_AlsoNotifyAddress.Length +
				Parent.EH_AlsoNotifyPlace.Length +
				Parent.EH_AlsoNotifyState.Length +
				Parent.EH_AlsoNotifyTraderNoCountryCode.Length +
				Parent.EH_AlsoNotifyPostCode.Length +
				Parent.EH_AlsoNotifyContactCode.Length +
				Parent.EH_AlsoNotifyContactDetail.Length;

		protected override void CheckEH_AlsoNotifyAddress()
		{
			base.CheckEH_AlsoNotifyAddress();

			if (Parent.EH_AWBType != AWBTypeList.Codes.House)
			{
				var propertyName = Res.GetString("27b6907a-a025-46ed-adba-cb66fe7acff7", "Also Notify Address");
				var value = string.Concat(Parent.EH_AlsoNotifyAddress, " ", Parent.EH_AlsoNotifyAddress2);

				AddInvalidNotificationIfNeed(propertyName, Parent.EH_AlsoNotifyAddressInfo, value);
				AddLengthWarningForNameAndAddressIfNeed(propertyName, Parent.EH_AlsoNotifyAddressInfo, value);
			}

			if (IsAnyAlsoNotifyDataEntered)
			{
				ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_AlsoNotifyAddressInfo,
					Res.GetString("65e0237d-ae33-474c-a279-8a3b552bffbc", "When you have an 'Also Notify' party, Address"), CIMPFieldFormats.Text);
			}

			ValidateEH_AlsoNotifyName();
			ValidateEH_AlsoNotifyPlace();
			ValidateEH_AlsoNotifyCountryCode();
		}

		protected override void CheckEH_AlsoNotifyAddress2()
		{
			base.CheckEH_AlsoNotifyAddress2();

			if (Parent.EH_AWBType != AWBTypeList.Codes.House)
			{
				var propertyName = Res.GetString("27b6907a-a025-46ed-adba-cb66fe7acff7", "Also Notify Address");
				var value = string.Concat(Parent.EH_AlsoNotifyAddress, " ", Parent.EH_AlsoNotifyAddress2);

				AddInvalidNotificationIfNeed(propertyName, Parent.EH_AlsoNotifyAddress2Info, value);
				AddLengthWarningForNameAndAddressIfNeed(propertyName, Parent.EH_AlsoNotifyAddress2Info, value);
			}
		}

		protected override void CheckEH_AlsoNotifyPlace()
		{
			base.CheckEH_AlsoNotifyPlace();

			if (!Parent.EH_AlsoNotifyPlace.IsEmpty && Parent.EH_AWBType != AWBTypeList.Codes.House && !IsValidAddress(Parent.EH_AlsoNotifyPlace))
			{
				AddInvalidNotification(Parent.EH_AlsoNotifyPlaceInfo, Res.GetString("5bd4ce58-4c4b-4e08-b063-7ad00ccdf501", "Also Notify City"));
			}

			if (IsAnyAlsoNotifyDataEntered)
			{
				ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_AlsoNotifyPlaceInfo,
					Res.GetString("9b64af1b-680e-4ca5-978a-f73da2c81c58", "When you have an 'Also Notify' party, City"), CIMPFieldFormats.Text);
			}

			ValidateEH_AlsoNotifyName();
			ValidateEH_AlsoNotifyAddress();
			ValidateEH_AlsoNotifyCountryCode();
		}

		protected override void CheckEH_AlsoNotifyState()
		{
			base.CheckEH_AlsoNotifyState();

			if (!Parent.EH_AlsoNotifyState.IsEmpty && Parent.EH_AWBType != AWBTypeList.Codes.House && !IsValidState(Parent.EH_AlsoNotifyState))
			{
				AddInvalidNotification(Parent.EH_AlsoNotifyStateInfo, Res.GetString("1733f109-58e4-42e8-b60f-773fcafcd812", "Also Notify State"));
			}
		}

		protected override void CheckEH_AlsoNotifyPostCode()
		{
			base.CheckEH_AlsoNotifyPostCode();

			if (!Parent.EH_AlsoNotifyPostCode.IsEmpty && Parent.EH_AWBType != AWBTypeList.Codes.House && !IsValidPostCode(Parent.EH_AlsoNotifyPostCode))
			{
				AddInvalidNotification(Parent.EH_AlsoNotifyPostCodeInfo, Res.GetString("7773d68d-9dc8-48c2-a567-54603370e18f", "Also Notify Post Code"));
			}
		}

		protected override void CheckEH_AlsoNotifyCountryCode()
		{
			base.CheckEH_AlsoNotifyCountryCode();

			ValidateFieldFormatForElectronicTransmission(Parent.EH_AlsoNotifyCountryCodeInfo, CIMPFieldFormats.Alpha);

			if (!Parent.EH_AlsoNotifyCountryCode.IsEmpty)
			{
				ValidateFieldLengthForElectronicTransmission(Parent.EH_AlsoNotifyCountryCodeInfo, 2);
			}

			if (IsAnyAlsoNotifyDataEntered)
			{
				ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_AlsoNotifyCountryCodeInfo,
					Res.GetString("60218d2b-f75d-44a7-9be1-ae7a922cca6e", "When you have an 'Also Notify' party, Country/Region Code"));
			}

			ValidateEH_AlsoNotifyName();
			ValidateEH_AlsoNotifyAddress();
			ValidateEH_AlsoNotifyPlace();
		}

		protected override void CheckEH_AlsoNotifyContactCode()
		{
			base.CheckEH_AlsoNotifyContactCode();
			if (!Parent.EH_AlsoNotifyContactCode.IsCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric))
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.EH_AlsoNotifyContactCodeInfo, Parent.ContactCodesList);
			}
			if (!Parent.EH_AlsoNotifyContactDetail.IsCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric))
			{
				ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_AlsoNotifyContactCodeInfo,
					Res.GetString("f6939e66-f497-4e1e-a609-843e414e045f", "If you enter an Also Notify Contact Number, the Contact Method"), CIMPFieldFormats.AlphaNumeric);
			}
		}

		protected override void CheckEH_AlsoNotifyContactDetail()
		{
			base.CheckEH_AlsoNotifyContactDetail();
			if (!Parent.EH_AlsoNotifyContactCode.IsCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric))
			{
				ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_AlsoNotifyContactDetailInfo,
					Res.GetString("b2b36b88-0963-4c3b-bfd5-1d878349e32a", "If you enter an Also Notify Contact Method, the Contact Number"), CIMPFieldFormats.AlphaNumeric);
			}
		}

		protected bool IsAnyAlsoNotifyDataEntered => !Parent.EH_AlsoNotifyName.IsCargoIMPEmpty()
					|| !Parent.EH_AlsoNotifyAddress.IsCargoIMPEmpty()
					|| !Parent.EH_AlsoNotifyPostCode.IsCargoIMPEmpty()
					|| !Parent.EH_AlsoNotifyState.IsCargoIMPEmpty()
					|| !Parent.EH_AlsoNotifyAddress2.IsCargoIMPEmpty()
					|| !Parent.EH_AlsoNotifyPlace.IsCargoIMPEmpty()
					|| !Parent.EH_AlsoNotifyCountryCode.IsEmpty
					|| !Parent.EH_AlsoNotifyContactCode.IsCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric)
					|| !Parent.EH_AlsoNotifyContactDetail.IsCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric);

		#endregion

		#region Agent

		protected override void CheckEH_AgentName()
		{
			base.CheckEH_AgentName();

			if (!Parent.EH_AgentName.IsEmpty && Parent.EH_AgentIATACodeFormatted.IsEmpty && !AgentDetailsAreAlwaysRequired)
			{
				Parent.EH_AgentNameInfo.AddNotification(NotificationLevelForFieldsRequiredForElectronicTransmission
					, Res.GetString("4595e7ee-d784-4793-860f-7513d578bc1a", "You cannot have an Agent Name without an Agent IATA Code."));
			}

			if (Parent.EH_AgentName.IsEmpty && (!Parent.EH_AgentIATACodeFormatted.IsEmpty || AgentDetailsAreAlwaysRequired))
			{
				Parent.EH_AgentNameInfo.AddNotification(NotificationLevelForFieldsRequiredForElectronicTransmission
					, Res.GetString("534db54f-8c33-4f59-89a0-ef4f1bb40248", "You must have an Agent Name to go with the Agent IATA Code.{0}"
					, "\r\n\r\n" + Res.GetString("5a42e7e0-0c37-4724-a419-395cce434061", "Agent Name can be entered as a default in the System Registry under Branch|Freight|AWB|MAWB|Issuing Carrier Agent|Name.")));
			}
		}

		protected override void CheckEH_AgentPlace()
		{
			base.CheckEH_AgentPlace();

			if (!Parent.EH_AgentPlace.IsEmpty && Parent.EH_AgentIATACodeFormatted.IsEmpty && !AgentDetailsAreAlwaysRequired)
			{
				Parent.EH_AgentPlaceInfo.AddNotification(NotificationLevelForFieldsRequiredForElectronicTransmission
					, Res.GetString("f28535e1-a230-4b4f-b6dc-b1245f9f7d51", "You cannot have an Agent City without an Agent IATA Code."));
			}

			if (Parent.EH_AgentPlace.IsEmpty && (!Parent.EH_AgentIATACodeFormatted.IsEmpty || AgentDetailsAreAlwaysRequired))
			{
				Parent.EH_AgentPlaceInfo.AddNotification(NotificationLevelForFieldsRequiredForElectronicTransmission
					, Res.GetString("b6e372b7-9439-4f33-be1d-611144fc3216", "You must have an Agent City to go with the Agent IATA Code.{0}"
					, "\r\n\r\n" + Res.GetString("60baa213-d0ee-44f5-9a55-a4890de2e8c1", "Agent City can be entered as a default in the System Registry under Branch|Freight|AWB|MAWB|Issuing Carrier Agent|City.")));
			}
		}

		protected override void CheckEH_AgentIATACodeFormatted()
		{
			base.CheckEH_AgentIATACodeFormatted();

			if (AgentDetailsAreAlwaysRequired)
			{
				string registrySetupInfoForEH_AgentIATACode = "\r\n\r\n" + Res.GetString("da6d7780-ce45-44f9-88f7-4c9450153705", @"The Agent's IATA Code can be entered as a default in two places:
1) For a borrowed Master; In the IATA Code Box on the Forwarder Tab of the Organization from which the Master bill is borrowed from.
2) For other Masters; In the System Registry under Branch|Freight|AWB|MAWB|Issuing Carrier Agent|IATA Code.");
				ZString warning = Res.GetString("09435bea-2778-4c28-9a93-9bab404576ac", @"The Agent IATA Code is mandatory for a Master Air Waybill.{0}", registrySetupInfoForEH_AgentIATACode);
				ValidateFieldRequiredForElectronicTransmission(Parent.EH_AgentIATACodeFormattedInfo, warning);
			}

			if (!Parent.EH_AgentIATACodeFormatted.IsEmpty)
			{
				var length = Parent.EH_AgentIATACodeFormatted.KeepNumericCharacters().Length;
				if (length != 7 && length != 11)
				{
					Parent.EH_AgentIATACodeFormattedInfo.AddNotification(NotificationLevelForFieldsRequiredForElectronicTransmission, MessageErrorInvalidIATACode);
				}
			}

			ValidateEH_AgentName();
			ValidateEH_AgentPlace();
			ValidateEH_AgentAccountNo();
		}

		internal static string MessageErrorInvalidIATACode => Res.GetString("c38fe44b-17ed-4fbd-9f46-536e5aac4371", "Invalid Agent IATA Code - Must contain either 7 digits, or 11 digits when a 4 digit CASS Account Code suffix is included.");

		protected override void CheckEH_AgentAccountNo()
		{
			base.CheckEH_AgentAccountNo();

			if (!Parent.EH_AgentAccountNo.IsEmpty && Parent.EH_AgentIATACodeFormatted.IsEmpty && !AgentDetailsAreAlwaysRequired)
			{
				Parent.EH_AgentAccountNoInfo.AddNotification(NotificationLevelForFieldsRequiredForElectronicTransmission, Res.GetString("6e151d13-13f4-4440-ad4d-8c5cc0979667", "You cannot have an Account No without an Agent IATA Code."));
			}
		}

		static bool AgentDetailsAreAlwaysRequired => !ForwardingConfigurationRegistry.Instance.AllowFWBWithoutAGTSegment.GetValueWithoutFallback(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty);

		#endregion

		#region Implementation

		public override INotificationType NotificationLevelForBookings
		{
			get { return NotificationLevelForFieldsRequiredForElectronicTransmission; }
		}

		readonly Lazy<INotificationType> notificationLevelForFieldsRequired = new Lazy<INotificationType>(
			() => ForwardingConfigurationRegistry.Instance.AllowUsersToSendFWBsWithErrors.Value ? CargoWise.EntityFramework.NotificationType.Warning : CargoWise.EntityFramework.NotificationType.MessageError);

		public override INotificationType NotificationLevelForFieldsRequiredForElectronicTransmission => notificationLevelForFieldsRequired.Value;

		#endregion
	}
}
