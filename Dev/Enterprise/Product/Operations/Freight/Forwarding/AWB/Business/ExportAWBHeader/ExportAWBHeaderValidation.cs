using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class ExportAWBHeaderValidation : CommonExportAWBHeaderValidation
	{
		public ExportAWBHeaderValidation(AutoExportAWBHeader parent)
			: base(parent)
		{
		}

		public new ExportAWBHeader Parent
		{
			get { return base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateEH_TotalGrossWeight();
			ValidateEH_WeightPrepaidCollect();
			ValidateEH_OtherPrepaidCollect();
			ValidateEH_AirlinePrefix();
			ValidateEH_AWBSerialNo();
			ValidateEH_AgentIATACodeFormatted();
			ValidateEH_ECNCRNNumber();
		}

		public virtual INotificationType NotificationLevelForFieldsRequiredForElectronicTransmission
		{
			get { return CargoWise.EntityFramework.NotificationType.Warning; }
		}

		#region Consignment Details

		#region EH_AirlinePrefix Validation

		public void ValidateEH_AirlinePrefix()
		{
			ValidateCalculatedProperty(Parent.EH_AirlinePrefixInfo);
		}

		protected virtual void CheckEH_AirlinePrefix()
		{
			if (Parent.EH_AirlinePrefix.Length == 3)
			{
				RefAirline[] airlines = Parent.Factory.Load<RefAirline>(new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, Parent.EH_AirlinePrefix));
				if (airlines.Length == 0)
				{
					var error = Res.GetString("7e4db5be-4e95-402b-bf91-4424c290945e", "The Airline code number that is entered is not listed with IATA.");
					Parent.EH_AirlinePrefixInfo.AddNotification(NotificationLevelForFieldsRequiredForElectronicTransmission, error);
				}
				else if (airlines.Length > 1)
				{
					var warning = Res.GetString("6fa396f8-b244-4ace-95f2-48a5cf29bd2f", "There are {0} Airlines with this Airline code number. Please delete the duplicated Airline(s); Maintain > Reference Files > Airlines.", airlines.Length);
					Parent.EH_AirlinePrefixInfo.AddWarning(warning);
				}
			}
			else
			{
				ValidateFieldLengthForElectronicTransmission(Parent.EH_AirlinePrefixInfo, 3);
			}
		}

		#endregion

		#region EH_AWBSerialNo Validation

		public void ValidateEH_AWBSerialNo()
		{
			ValidateCalculatedProperty(Parent.EH_AWBSerialNoInfo);
		}

		protected virtual void CheckEH_AWBSerialNo()
		{
			WarnIfIncorrectLength(Parent.EH_AWBSerialNoInfo, Res.GetString("8272f87c-9f3b-4a52-8379-404716dab5db", "Serial Number"), 8);
		}

		#endregion

		protected override void CheckEH_AWBOriginCode()
		{
			base.CheckEH_AWBOriginCode();
			ValidateFieldLengthForElectronicTransmission(Parent.EH_AWBOriginCodeInfo, 3);
			ValidateFieldFormatForElectronicTransmission(Parent.EH_AWBOriginCodeInfo, CIMPFieldFormats.Alpha);
		}

		protected override void CheckEH_AirportOfDestinationCode()
		{
			base.CheckEH_AirportOfDestinationCode();
			ValidateFieldLengthForElectronicTransmission(Parent.EH_AirportOfDestinationCodeInfo, 3);
			ValidateFieldFormatForElectronicTransmission(Parent.EH_AirportOfDestinationCodeInfo, CIMPFieldFormats.Alpha);
		}

		#region EH_TotalGrossWeight Validation

		public void ValidateEH_TotalGrossWeight()
		{
			ValidateCalculatedProperty(Parent.EH_TotalGrossWeightInfo);
		}

		protected virtual void CheckEH_TotalGrossWeight()
		{
			if (Parent.EH_TotalGrossWeight <= 0)
			{
				Parent.EH_TotalGrossWeightInfo.AddNotification(NotificationLevelForFieldsRequiredForElectronicTransmission, Res.GetString("a597ab6d-29ce-4008-b47c-3db84d45210c", "Total weight must be greater than 0."));
			}
		}

		#endregion

		#endregion

		protected override void CheckEH_By1st()
		{
			base.CheckEH_By1st();
			ListValidation.MessageErrorIfInvalidCode(Parent.EH_By1stInfo, Parent.Lookups.Airlines);
		}

		#region As Agreed

		protected override void CheckEH_AsAgreed1st()
		{
			base.CheckEH_AsAgreed1st();
			MandatoryValidation.CheckEntered(Parent.EH_AsAgreed1stInfo);
			ListValidation.ErrorIfInvalidCode(Parent.EH_AsAgreed1stInfo, Parent.AsAgreed1stList);
		}

		protected override void CheckEH_AsAgreed2nd()
		{
			base.CheckEH_AsAgreed2nd();
			MandatoryValidation.CheckEntered(Parent.EH_AsAgreed2ndInfo);
			ListValidation.ErrorIfInvalidCode(Parent.EH_AsAgreed2ndInfo, Parent.AsAgreed2ndList);
		}

		#endregion

		#region Shipper

		protected override void CheckEH_ShipperCountryCode()
		{
			base.CheckEH_ShipperCountryCode();
			ValidateFieldFormatForElectronicTransmission(Parent.EH_ShipperCountryCodeInfo, CIMPFieldFormats.Alpha);
			if (!Parent.EH_ShipperCountryCode.IsEmpty)
			{
				ValidateFieldLengthForElectronicTransmission(Parent.EH_ShipperCountryCodeInfo, 2);
			}
		}

		protected override void CheckEH_ShipperContactCode()
		{
			base.CheckEH_ShipperContactCode();
			if (!Parent.EH_ShipperContactCode.IsCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric))
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.EH_ShipperContactCodeInfo, Parent.ContactCodesList);
			}

			if (!Parent.EH_ShipperContactDetail.IsCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric))
			{
				ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_ShipperContactCodeInfo,
					Res.GetString("ab9a8c07-d8c7-4a9c-8d01-d81fef54c53d", "If you enter a Shipper Contact Number, the Contact Method"), CIMPFieldFormats.AlphaNumeric);
			}
		}

		protected override void CheckEH_ShipperContactDetail()
		{
			base.CheckEH_ShipperContactDetail();
			if (!Parent.EH_ShipperContactCode.IsCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric))
			{
				ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_ShipperContactDetailInfo,
					Res.GetString("eaab40fe-adc0-4d14-af03-f508b61e9aed", "If you enter a Shipper Contact Method, the Contact Number"), CIMPFieldFormats.AlphaNumeric);
			}
		}

		protected override void CheckEH_ShipperName()
		{
			base.CheckEH_ShipperName();

			var propertyName = Res.GetString("ab74deea-40ac-4062-be73-021ca064f189", "Shipper Name");
			AddInvalidNotificationIfNeed(propertyName, Parent.EH_ShipperNameInfo, Parent.EH_ShipperName);
			AddLengthWarningForNameAndAddressIfNeed(propertyName, Parent.EH_ShipperNameInfo, Parent.EH_ShipperName);
		}

		protected override void CheckEH_ShipperAddress()
		{
			base.CheckEH_ShipperAddress();

			var propertyName = Res.GetString("d0e54a76-6404-4bbc-af2d-35b56b63bfc6", "Shipper Address");
			var value = string.Concat(Parent.EH_ShipperAddress, " ", Parent.EH_ShipperAddress2);

			AddInvalidNotificationIfNeed(propertyName, Parent.EH_ShipperAddressInfo, value);
			AddLengthWarningForNameAndAddressIfNeed(propertyName, Parent.EH_ShipperAddressInfo, value);
		}

		protected override void CheckEH_ShipperAddress2()
		{
			base.CheckEH_ShipperAddress2();

			var propertyName = Res.GetString("d0e54a76-6404-4bbc-af2d-35b56b63bfc6", "Shipper Address");
			var value = string.Concat(Parent.EH_ShipperAddress, " ", Parent.EH_ShipperAddress2);

			AddInvalidNotificationIfNeed(propertyName, Parent.EH_ShipperAddress2Info, value);
			AddLengthWarningForNameAndAddressIfNeed(propertyName, Parent.EH_ShipperAddress2Info, value);
		}

		protected override void CheckEH_ShipperPlace()
		{
			base.CheckEH_ShipperPlace();

			if (!Parent.EH_ShipperPlace.IsEmpty && !IsValidAddress(Parent.EH_ShipperPlace))
			{
				AddInvalidNotification(Parent.EH_ShipperPlaceInfo, Res.GetString("a08897e0-93e3-4a8d-aaab-dbfd9380557e", "Shipper City"));
			}
		}

		protected override void CheckEH_ShipperState()
		{
			base.CheckEH_ShipperState();

			if (!Parent.EH_ShipperState.IsEmpty && !IsValidState(Parent.EH_ShipperState))
			{
				AddInvalidNotification(Parent.EH_ShipperStateInfo, Res.GetString("0768def4-e245-43b1-bd0b-8c8cbed966b1", "Shipper State"));
			}
		}

		protected override void CheckEH_ShipperPostCode()
		{
			base.CheckEH_ShipperPostCode();

			if (!Parent.EH_ShipperPostCode.IsEmpty && !IsValidPostCode(Parent.EH_ShipperPostCode))
			{
				AddInvalidNotification(Parent.EH_ShipperPostCodeInfo, Res.GetString("e0611f8a-4dc8-443c-ae43-68c339bd0dba", "Shipper Post Code"));
			}
		}

		protected override void CheckEH_ShipperTraderNo()
		{
			base.CheckEH_ShipperTraderNo();

			var warning = GetTraderTypeAndNoLengthWarning(Parent.EH_ShipperTraderNoTypeInfo, Parent.EH_ShipperTraderNoInfo);
			if (!warning.IsEmpty)
			{
				Parent.EH_ShipperTraderNoInfo.AddWarning(warning);
			}

			CheckTraderNoExceedingMaxLength(Parent.EH_ShipperTraderNoInfo, Parent.IsShipperTraderNoExceedingMaxLength);
		}

		protected override void CheckEH_ShipperTraderNoType()
		{
			base.CheckEH_ShipperTraderNoType();

			var warning = GetTraderTypeAndNoLengthWarning(Parent.EH_ShipperTraderNoTypeInfo, Parent.EH_ShipperTraderNoInfo);
			if (!warning.IsEmpty)
			{
				Parent.EH_ShipperTraderNoTypeInfo.AddWarning(warning);
			}
		}

		protected void AddInvalidNotificationIfNeed(string propertyName, ZPropertyInfo propertyInfo, ZString propertyValue)
		{
			var value = propertyValue.Trim();
			if (!value.IsEmpty && !IsValidAddress(value))
			{
				AddInvalidNotification(propertyInfo, propertyName);
			}
		}

		protected void AddLengthWarningForNameAndAddressIfNeed(string propertyName, ZPropertyInfo propertyInfo, ZString propertyValue)
		{
			var maxLength = Parent.AirMessageMaxNameAddressLength;
			var value = propertyValue.Trim();
			if (value.Length > maxLength)
			{
				propertyInfo.AddWarning(
					Res.GetString("28c27fba-5cc9-4083-8f31-c86a8a7d7450", "{0} is too long, only the first {1} characters can be sent.",
					propertyName, maxLength));
			}
		}

		protected void AddInvalidNotification(ZPropertyInfo info, string propertyName)
		{
			info.AddNotification(NotificationLevelForFieldsRequiredForElectronicTransmission, Res.GetString("096d2e8b-b31e-41fc-90d6-aa29c4734b9c", "{0} is not valid.", propertyName));
		}

		protected bool IsValidAddress(ZString value)
		{
			var stringValue = value.ToString();

			return stringValue != Core.Constants.AWB.AWBAddressNotAllowed.ToBeAnnounced
				&& stringValue.ToCharArray().Any(x => x != '.')
				&& stringValue != "X"
				&& stringValue != "-";
		}

		protected bool IsValidState(ZString value)
		{
			return value.KeepAlphanumericCharacters().Length >= 1;
		}

		protected bool IsValidPostCode(ZString value)
		{
			return value.KeepAlphanumericCharacters().Length >= 1;
		}

		#endregion

		#region Consignee

		protected override void CheckEH_ConsigneeCountryCode()
		{
			base.CheckEH_ConsigneeCountryCode();
			ValidateFieldFormatForElectronicTransmission(Parent.EH_ConsigneeCountryCodeInfo, CIMPFieldFormats.Alpha);
			if (!Parent.EH_ConsigneeCountryCode.IsEmpty)
			{
				ValidateFieldLengthForElectronicTransmission(Parent.EH_ConsigneeCountryCodeInfo, 2);
			}
		}

		protected override void CheckEH_ConsigneeContactCode()
		{
			base.CheckEH_ConsigneeContactCode();

			if (!Parent.EH_ConsigneeContactDetail.IsCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric))
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.EH_ConsigneeContactCodeInfo, Parent.ContactCodesList);
				ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_ConsigneeContactCodeInfo,
					Res.GetString("30da315c-1b8a-4301-9dc6-5b6a320362ad", "If you enter a Consignee Contact Number, the Contact Method"), CIMPFieldFormats.AlphaNumeric);
			}
		}

		protected override void CheckEH_ConsigneeContactDetail()
		{
			base.CheckEH_ConsigneeContactDetail();

			if (!Parent.EH_ConsigneeContactCode.IsCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric))
			{
				ValidateFieldAddingMessageForElectronicTransmission(Parent.EH_ConsigneeContactDetailInfo,
					Res.GetString("a2045ce1-9983-457d-b841-b38056d46567", "If you enter a Consignee Contact Method, the Contact Number"), CIMPFieldFormats.AlphaNumeric);
			}
		}

		protected override void CheckEH_ConsigneeName()
		{
			base.CheckEH_ConsigneeName();

			var propertyName = Res.GetString("bf816797-2349-446f-a80c-9c15c4b92546", "Consignee Name");
			AddInvalidNotificationIfNeed(propertyName, Parent.EH_ConsigneeNameInfo, Parent.EH_ConsigneeName);
			AddLengthWarningForNameAndAddressIfNeed(propertyName, Parent.EH_ConsigneeNameInfo, Parent.EH_ConsigneeName);
		}

		protected override void CheckEH_ConsigneeAddress()
		{
			base.CheckEH_ConsigneeAddress();

			var propertyName = Res.GetString("33946620-4795-4560-a9e7-191b2a3b85ec", "Consignee Address");
			var value = string.Concat(Parent.EH_ConsigneeAddress, " ", Parent.EH_ConsigneeAddress2);

			AddInvalidNotificationIfNeed(propertyName, Parent.EH_ConsigneeAddressInfo, value);
			AddLengthWarningForNameAndAddressIfNeed(propertyName, Parent.EH_ConsigneeAddressInfo, value);
		}

		protected override void CheckEH_ConsigneeAddress2()
		{
			base.CheckEH_ConsigneeAddress2();

			var propertyName = Res.GetString("40f99c5c-0190-4cb9-589b299864ad-f1e5432ef874", "Consignee Address");
			var value = string.Concat(Parent.EH_ConsigneeAddress, " ", Parent.EH_ConsigneeAddress2);

			AddInvalidNotificationIfNeed(propertyName, Parent.EH_ConsigneeAddress2Info, value);
			AddLengthWarningForNameAndAddressIfNeed(propertyName, Parent.EH_ConsigneeAddress2Info, value);
		}

		protected override void CheckEH_ConsigneePlace()
		{
			base.CheckEH_ConsigneePlace();

			if (!Parent.EH_ConsigneePlace.IsEmpty && !IsValidAddress(Parent.EH_ConsigneePlace))
			{
				AddInvalidNotification(Parent.EH_ConsigneePlaceInfo, Res.GetString("cc9d8363-f7c7-4308-bb03-91c273164409", "Consignee City"));
			}
		}

		protected override void CheckEH_ConsigneeState()
		{
			base.CheckEH_ConsigneeState();

			if (!Parent.EH_ConsigneeState.IsEmpty && !IsValidState(Parent.EH_ConsigneeState))
			{
				AddInvalidNotification(Parent.EH_ConsigneeStateInfo, Res.GetString("b9254085-4127-4e5e-afac-06212bee004a", "Consignee State"));
			}
		}

		protected override void CheckEH_ConsigneePostCode()
		{
			base.CheckEH_ConsigneePostCode();

			if (!Parent.EH_ConsigneePostCode.IsEmpty && !IsValidPostCode(Parent.EH_ConsigneePostCode))
			{
				AddInvalidNotification(Parent.EH_ConsigneePostCodeInfo, Res.GetString("f0dafb7b-4c3d-4a6e-9406-5aae283ed9f7", "Consignee Post Code"));
			}
		}

		protected override void CheckEH_ConsigneeTraderNo()
		{
			base.CheckEH_ConsigneeTraderNo();

			var warning = GetTraderTypeAndNoLengthWarning(Parent.EH_ConsigneeTraderNoTypeInfo, Parent.EH_ConsigneeTraderNoInfo);
			if (!warning.IsEmpty)
			{
				Parent.EH_ConsigneeTraderNoInfo.AddWarning(warning);
			}

			CheckTraderNoExceedingMaxLength(Parent.EH_ConsigneeTraderNoInfo, Parent.IsConsigneeTraderNoExceedingMaxLength);
		}

		protected override void CheckEH_ConsigneeTraderNoType()
		{
			base.CheckEH_ConsigneeTraderNoType();

			var warning = GetTraderTypeAndNoLengthWarning(Parent.EH_ConsigneeTraderNoTypeInfo, Parent.EH_ConsigneeTraderNoInfo);
			if (!warning.IsEmpty)
			{
				Parent.EH_ConsigneeTraderNoTypeInfo.AddWarning(warning);
			}
		}

		#endregion

		#region Notify Party

		protected override void CheckEH_AlsoNotifyTraderNo()
		{
			base.CheckEH_AlsoNotifyTraderNo();

			var warning = GetTraderTypeAndNoLengthWarning(Parent.EH_AlsoNotifyTraderNoTypeInfo, Parent.EH_AlsoNotifyTraderNoInfo);
			if (!warning.IsEmpty)
			{
				Parent.EH_AlsoNotifyTraderNoInfo.AddWarning(warning);
			}

			CheckTraderNoExceedingMaxLength(Parent.EH_AlsoNotifyTraderNoInfo, Parent.IsAlsoNotifyTraderNoExceedingMaxLength);
		}

		protected override void CheckEH_AlsoNotifyTraderNoType()
		{
			base.CheckEH_AlsoNotifyTraderNoType();

			var warning = GetTraderTypeAndNoLengthWarning(Parent.EH_AlsoNotifyTraderNoTypeInfo, Parent.EH_AlsoNotifyTraderNoInfo);
			if (!warning.IsEmpty)
			{
				Parent.EH_AlsoNotifyTraderNoTypeInfo.AddWarning(warning);
			}
		}

		#endregion

		#region IssuingAgentNameAndAddressValidation

		string PropertyTruncatedWarningMessage
		{
			get { return Res.GetString("2113da18-b1c3-414e-b0f6-29a271e5706d", "Value exceeded maximum number of allowed characters and has been truncated"); }
		}

		protected override void CheckEH_IssuingAgentName()
		{
			base.CheckEH_IssuingAgentName();

			if (!Parent.EH_IssuingAgentNameInfo.HasErrors() && Parent.IsEH_IssuingAgentNameTruncated)
			{
				Parent.EH_IssuingAgentNameInfo.AddWarning(PropertyTruncatedWarningMessage);
			}
		}

		protected override void CheckEH_IssuingAgentAddress1()
		{
			base.CheckEH_IssuingAgentAddress1();

			if (!Parent.EH_IssuingAgentAddress1Info.HasErrors() && Parent.IsEH_IssuingAgentAddress1Truncated)
			{
				Parent.EH_IssuingAgentAddress1Info.AddWarning(PropertyTruncatedWarningMessage);
			}
		}

		protected override void CheckEH_IssuingAgentAddress2()
		{
			base.CheckEH_IssuingAgentAddress2();

			if (!Parent.EH_IssuingAgentAddress2Info.HasErrors() && Parent.IsEH_IssuingAgentAddress2Truncated)
			{
				Parent.EH_IssuingAgentAddress2Info.AddWarning(PropertyTruncatedWarningMessage);
			}
		}

		#endregion

		#region Security Declaration

		protected override void CheckEH_AgentApprovalNumber()
		{
			base.CheckEH_AgentApprovalNumber();

			if (!Parent.EH_AgentApprovalNumber.IsEmpty)
			{
				if (Parent.ExportAWBSecurityStatusLines.Cast<ExportAWBSecurityStatusLine>()
					.Any(x => x.EAS_ApprovalCategory == AviationSecuritySchemeMembership.Codes.RegulatedAgent && x.EAS_ApprovalNumber == Parent.EH_AgentApprovalNumber))
				{
					Parent.EH_AgentApprovalNumberInfo.AddMessageError(Res.GetString("7d48c16f-bc0e-4946-b541-e4c1faa8c1ee", "Regulated Agent Identifier is same as the Approval Number of the Known Party."));
				}
			}
		}

		#region EH_ScheduledArrivalDate

		protected override void CheckEH_ScheduledArrivalDate()
		{
			if (Parent.ShouldSetScheduledArrivalDate && Parent.EH_ScheduledArrivalDate.IsEmpty)
			{
				Parent.EH_ScheduledArrivalDateInfo.AddWarning(Res.GetString("3966ac1e-1853-4446-b321-61ddd9e2fdb2", "For known cargo, the scheduled date/time of arrival at the terminal is required per the CAA (Civil Aviation Authority)."));
			}
		}

		#endregion

		#region EH_AdditionalSecurityInformationStatement

		protected override void CheckEH_AdditionalSecurityInformationStatement()
		{
			if (Parent.EH_AdditionalSecurityInformationStatement.IsEmpty && Parent.ShouldSetSecurityStatement)
			{
				Parent.EH_AdditionalSecurityInformationStatementInfo.AddWarning(Res.GetString("eeb170e0-fcba-4420-9590-1853c5028e29", "Please select the statement that best describes the examination or clearance details of the cargo."));
			}

			if (!Parent.EH_AdditionalSecurityInformationStatement.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.EH_AdditionalSecurityInformationStatementInfo, Parent.Lookups.AdditionalSecurityStatementList);
			}
		}

		#endregion

		#endregion

		public void ValidateEH_TotalNoOfPieces()
		{
			ValidateCalculatedProperty(Parent.EH_TotalNoOfPiecesInfo);
		}

		protected virtual void CheckEH_TotalNoOfPieces()
		{
			if (Parent.EH_TotalNoOfPieces == 0)
			{
				Parent.EH_TotalNoOfPiecesInfo.AddNotification(NotificationLevelForFieldsRequiredForElectronicTransmission, Res.GetString("fd0675c2-46bb-45d1-acaa-e7159de2b446", "You must have more than 0 pieces"));
			}
		}

		public void ValidateEH_WeightPrepaidCollect()
		{
			ValidateCalculatedProperty(Parent.EH_WeightPrepaidCollectInfo);
		}

		protected virtual void CheckEH_WeightPrepaidCollect()
		{
			ListValidation.ErrorIfInvalidCode(Parent.EH_WeightPrepaidCollectInfo, Parent.PrepaidCollectList);
		}

		public void ValidateEH_OtherPrepaidCollect()
		{
			ValidateCalculatedProperty(Parent.EH_OtherPrepaidCollectInfo);
		}

		protected virtual void CheckEH_OtherPrepaidCollect()
		{
			ListValidation.ErrorIfInvalidCode(Parent.EH_OtherPrepaidCollectInfo, Parent.PrepaidCollectList);

			if (FreightDataRegistry.Instance.ShowCollectOtherWarning.Value)
			{
				if (Parent.EH_OtherPrepaidCollect == ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect &&
					Parent.AWBOtherCharges.Count == 0)
				{
					Parent.EH_OtherPrepaidCollectInfo.AddWarning(Res.GetString("2a7cd3cb-383b-41a8-9631-69d96be07102", "For collect Air Way Bills, you must specify at least 1 other charge."));
				}
			}
		}

		public void ValidateEH_AgentIATACodeFormatted()
		{
			ValidateCalculatedProperty(Parent.EH_AgentIATACodeFormattedInfo);
		}

		protected virtual void CheckEH_AgentIATACodeFormatted()
		{
		}

		#region EH_ECNCRNNumber Validation

		public void ValidateEH_ECNCRNNumber()
		{
			ValidateCalculatedProperty(Parent.EH_ECNCRNNumberInfo);
		}

		protected virtual void CheckEH_ECNCRNNumber()
		{
		}

		#endregion

		#region Flight Bookings

		public void ValidateBookings()
		{
			ValidateEH_Booking1stCarrier();
			ValidateEH_Booking1stFlight();
			ValidateEH_Booking1stFlightDate();
			ValidateEH_Booking2ndCarrier();
			ValidateEH_Booking2ndFlight();
			ValidateEH_Booking2ndFlightDate();
		}

		#region 1st Booking

		protected override void CheckEH_Booking1stFlight()
		{
			base.CheckEH_Booking1stFlight();
			if (IsAnyDataIn1stBookingDetails)
			{
				ValidateFlightForElectronicTransmission(Parent.EH_Booking1stFlightInfo);
			}
		}

		protected override void CheckEH_Booking1stCarrier()
		{
			base.CheckEH_Booking1stCarrier();
			if (IsAnyDataIn1stBookingDetails)
			{
				ValidateFieldLengthForBookings(Parent.EH_Booking1stCarrierInfo, 2);
				ValidateFieldFormatForBookings(Parent.EH_Booking1stCarrierInfo, CIMPFieldFormats.AlphaNumeric);
			}
		}

		protected override void CheckEH_Booking1stFlightDate()
		{
			base.CheckEH_Booking1stFlightDate();
			if (IsAnyDataIn1stBookingDetails)
			{
				ValidateFieldLengthForBookings(Parent.EH_Booking1stFlightDateInfo, 2);
				ValidateFieldFormatForBookings(Parent.EH_Booking1stFlightDateInfo, CIMPFieldFormats.Numeric);
				ValidateDateDayString(Parent.EH_Booking1stFlightDateInfo);
			}
		}

		#endregion

		protected bool IsAnyDataIn1stBookingDetails
		{
			get { return !Parent.EH_Booking1stCarrier.IsEmpty || !Parent.EH_Booking1stFlight.IsEmpty || !Parent.EH_Booking1stFlightDate.IsEmpty; }
		}

		#region 2nd Booking

		static string warning1stBookingRequired => Res.GetString("62c6e5ad-e682-4628-8481-ec260c3344e1", "2nd Booking details cannot be entered unless 1st Booking details have been completed.");

		protected override void CheckEH_Booking2ndFlight()
		{
			base.CheckEH_Booking2ndFlight();
			if (IsAnyDataIn2ndBookingDetails)
			{
				if (!Parent.EH_Booking2ndFlight.IsEmpty && !IsAnyDataIn1stBookingDetails)
				{
					Parent.EH_Booking2ndFlightInfo.AddNotification(NotificationLevelForBookings, warning1stBookingRequired);
				}
				else
				{
					ValidateFlightForElectronicTransmission(Parent.EH_Booking2ndFlightInfo);
				}
			}
		}

		protected override void CheckEH_Booking2ndCarrier()
		{
			base.CheckEH_Booking2ndCarrier();
			if (IsAnyDataIn2ndBookingDetails)
			{
				if (!Parent.EH_Booking2ndCarrier.IsEmpty && !IsAnyDataIn1stBookingDetails)
				{
					Parent.EH_Booking2ndCarrierInfo.AddNotification(NotificationLevelForBookings, warning1stBookingRequired);
				}
				else
				{
					ValidateFieldLengthForBookings(Parent.EH_Booking2ndCarrierInfo, 2);
					ValidateFieldFormatForBookings(Parent.EH_Booking2ndCarrierInfo, CIMPFieldFormats.AlphaNumeric);
				}
			}
		}

		protected override void CheckEH_Booking2ndFlightDate()
		{
			base.CheckEH_Booking2ndFlightDate();
			if (IsAnyDataIn2ndBookingDetails)
			{
				if (!Parent.EH_Booking2ndFlightDate.IsEmpty && !IsAnyDataIn1stBookingDetails)
				{
					Parent.EH_Booking2ndFlightDateInfo.AddNotification(NotificationLevelForBookings, warning1stBookingRequired);
				}
				else
				{
					ValidateFieldLengthForBookings(Parent.EH_Booking2ndFlightDateInfo, 2);
					ValidateFieldFormatForBookings(Parent.EH_Booking2ndFlightDateInfo, CIMPFieldFormats.Numeric);
					ValidateDateDayString(Parent.EH_Booking2ndFlightDateInfo);
				}
			}
		}

		#endregion

		protected bool IsAnyDataIn2ndBookingDetails
		{
			get { return !Parent.EH_Booking2ndCarrier.IsEmpty || !Parent.EH_Booking2ndFlight.IsEmpty || !Parent.EH_Booking2ndFlightDate.IsEmpty; }
		}

		public virtual INotificationType NotificationLevelForBookings
		{
			get { return CargoWise.EntityFramework.NotificationType.Warning; }
		}

		void ValidateDateDayString(ZPropertyInfo dayStringInfo)
		{
			var dayString = dayStringInfo.Value.ToString();

			if (!(int.TryParse(dayString, out var day)
				&& day > 0 && day <= 31))
			{
				dayStringInfo.AddWarning(Res.GetString("677792b5-d843-490b-a306-c3a1c06e410c", "Date value should be between 1 and 31."));
			}
		}

		#endregion

		#region Validation Methods

		public void ValidateFieldAddingMessageForElectronicTransmission(ZPropertyInfo info)
		{
			ValidateFieldAddingMessageForElectronicTransmission(info, info.HumanReadableName);
		}

		protected void ValidateFieldAddingMessageForElectronicTransmission(ZPropertyInfo info, ZString humanReadableFieldName)
		{
			ValidateFieldRequiredForElectronicTransmission(info, Res.GetString("efc2c906-c479-4e1e-869a-7cff33733c78", "{0} is Required for sending an AWB electronically.", humanReadableFieldName));
		}

		protected void ValidateFieldAddingMessageForElectronicTransmission(ZPropertyInfo info, CIMPFieldFormats format)
		{
			ValidateFieldAddingMessageForElectronicTransmission(info, info.HumanReadableName, format);
		}

		protected void ValidateFieldAddingMessageForElectronicTransmission(ZPropertyInfo info, ZString humanReadableFieldName, CIMPFieldFormats format)
		{
			ValidateFieldRequiredForElectronicTransmission(info,
				Res.GetString("980d5af1-e956-4392-8a6f-72a4293f6d36", "{0} is Required for sending an AWB electronically, only {1} characters are considered.",
				humanReadableFieldName, GetFormatDescription(format)), format);
		}

		ZString GetFormatDescription(CIMPFieldFormats format)
		{
			switch (format)
			{
				case CIMPFieldFormats.Alpha:
					return Res.GetString("89c135a0-d430-4731-8441-c7a05151f302", "alpha");
				case CIMPFieldFormats.AlphaNumeric:
					return Res.GetString("17a1f1d7-6fda-4ea6-8db6-5c3d91a31281", "alpha-numeric");
				case CIMPFieldFormats.Numeric:
					return Res.GetString("50452511-e0b8-494a-9c1b-04318899e9ac", "numeric");
				case CIMPFieldFormats.Text:
					return Res.GetString("b7aad0bb-770f-4af1-9e6e-3d6914146265", "alpha-numeric, dash, full stop and space");
			}

			return ZString.Empty;
		}

		protected void ValidateFieldRequiredForElectronicTransmission(ZPropertyInfo info, ZString message, CIMPFieldFormats format = CIMPFieldFormats.Text)
		{
			if (info.Value.IsCargoIMPEmpty(format))
			{
				info.AddNotification(NotificationLevelForFieldsRequiredForElectronicTransmission, message);
			}
		}

		protected virtual void ValidateFieldFormatForElectronicTransmission(ZPropertyInfo info, CIMPFieldFormats format)
		{
			ValidateFieldFormat(info, format, NotificationLevelForFieldsRequiredForElectronicTransmission);
		}

		void ValidateFieldFormatForBookings(ZPropertyInfo info, CIMPFieldFormats format)
		{
			ValidateFieldFormat(info, format, NotificationLevelForBookings);
		}

		void ValidateFieldFormat(ZPropertyInfo info, CIMPFieldFormats format, INotificationType notificationLevel)
		{
			string message = string.Empty;
			string regexPattern = string.Empty;

			switch (format)
			{
				case CIMPFieldFormats.Alpha:
					message = Res.GetString("3d5375ec-afe7-40f0-9bff-9b0bf0a676c2", "{0} must consist of alpha characters only", info.HumanReadableName);
					regexPattern = (NoResString)@"^[a-zA-Z]*$"; // Regex pattern
					break;
				case CIMPFieldFormats.AlphaNumeric:
					message = Res.GetString("1579ecc0-f17b-4602-addc-4b9e3644b9b2", "{0} must consist of alphanumeric characters only", info.HumanReadableName);
					regexPattern = (NoResString)@"^[a-zA-Z0-9]*$"; // Regex pattern
					break;
				case CIMPFieldFormats.Numeric:
					message = Res.GetString("ac8252b1-b1c2-46b8-bc54-c34b0b6cd4c7", "{0} must consist of numeric characters only", info.HumanReadableName);
					regexPattern = @"^[0-9]*$"; // Regex pattern
					break;
				case CIMPFieldFormats.Text:
					message = Res.GetString("7719b735-6d44-4e4d-95bf-c99ed8ab9795", "{0} must consist of alphanumeric characters and prescribed special characters only", info.HumanReadableName);
					regexPattern = (NoResString)@"^[a-zA-Z0-9\-\. ]*$"; // Regex pattern
					break;
			}

			if (!Regex.IsMatch(info.Value.ToString(), regexPattern))
			{
				info.AddNotification(notificationLevel, message);
			}
		}

		protected void ValidateFieldLengthForElectronicTransmission(ZPropertyInfo info, int length)
		{
			ValidateFieldLength(info, length, NotificationLevelForFieldsRequiredForElectronicTransmission);
		}

		void ValidateFieldLengthForBookings(ZPropertyInfo info, int length)
		{
			ValidateFieldLength(info, length, NotificationLevelForBookings);
		}

		protected void ValidateFieldLength(ZPropertyInfo info, int length, INotificationType notificationLevel)
		{
			if (info.Value.ToString().Length != length)
			{
				string message = Res.GetString("a332668e-bd6a-4ff8-9418-0ffa8dcc115f", "{0} must be {1} characters in length.", info.HumanReadableName, length);
				info.AddNotification(notificationLevel, message);
			}
		}

		void ValidateFlightForElectronicTransmission(ZPropertyInfo propertyInfo)
		{
			string regexPattern = (NoResString)@"^\d{3,4}(?:[a-zA-Z])?$"; // Regex pattern
			if (!Regex.IsMatch(propertyInfo.Value.ToString(), regexPattern))
			{
				propertyInfo.AddNotification(NotificationLevelForBookings,
					Res.GetString("fefe3a82-e5f3-4d27-9f55-4c182673c620", "{0} must consist of Flight Number (up to 4 numeric) and an optional Operational Suffix (1 alphabetic).", propertyInfo.HumanReadableName));
			}
		}

		void WarnIfIncorrectLength(ZPropertyInfo propertyInfo, string fieldName, int length)
		{
			if (propertyInfo.Value.ToString().Length != length)
			{
				string warning = Res.GetString("4361f47b-dc5f-4015-897e-a42872a30cc6", "{0} must be {1} characters in length.", fieldName, length);
				propertyInfo.AddWarning(warning);
			}
		}

		ZString GetTraderTypeAndNoLengthWarning(ZPropertyInfo traderTypeInfo, ZPropertyInfo traderNoInfo)
		{
			const int maxLength = 35;

			var helper = ObjectFactory.Get<IRequiredTaxNumberHelper>();
			var traderType = helper.ExpandTaxTypeCodeIfNecessary((ZString)traderTypeInfo.Value);
			if (traderType.Length + ((ZString)traderNoInfo.Value).Length > maxLength)
			{
				return Res.GetString("6304eb4b-6496-4243-9ff7-8ff4626ce17d", "{0} and {1} must be less than or equal to {2} characters in length.", traderTypeInfo.HumanReadableName, traderNoInfo.HumanReadableName, maxLength);
			}

			return ZString.Empty;
		}

		void CheckTraderNoExceedingMaxLength(ZPropertyInfo traderNoInfo, bool isExceedingMaxLength)
		{
			if (!Parent.IsAWBOverridden && isExceedingMaxLength)
			{
				traderNoInfo.AddWarning(Res.GetString("edb83cb8-f3ff-4b20-9156-cfa0bc0243d4", "Tax number cannot be included as it exceeds the character limit of 35."));
			}
		}

		#endregion
	}
}
