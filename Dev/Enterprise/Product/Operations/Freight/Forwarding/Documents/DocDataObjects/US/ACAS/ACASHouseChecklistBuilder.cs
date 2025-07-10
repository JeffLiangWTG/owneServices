using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.US
{
	sealed class ACASHouseChecklistBuilder
	{
		public ACASHouseChecklistBuilder(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
			context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingConsol consol;
		readonly IContext context;

		public ACASHouseChecklist Build()
		{
			var acas = new ACASHouseChecklist(
				nameof(ForwardingConsol),
				consol.JK_UniqueConsignRef,
				DocumentNames.AdvancedManifest);

			PopulateNumbersAndReferences(acas);
			PopulateLocations(acas);
			PopulateWeightsAndMeasures(acas);
			PopulateAddresses(acas);
			PopulateShipments(acas);
			PopulateStateDetails(acas);

			acas.ValidateAllIncludingChildren();

			return acas;
		}

		#region Population

		#region Numbers and References

		void PopulateNumbersAndReferences(ACASHouseChecklist acas)
		{
			acas.ConsolNumber = consol.JK_UniqueConsignRef;
			PopulateMAWB(acas);
		}

		#endregion

		#region MAWB

		void PopulateMAWB(ACASHouseChecklist acas)
		{
			var mawbNumber = consol.JK_MasterBillNum;
			var mawbFormatMessage = mawbNumber.IsEmpty
				? (ZString)(NoResString)"MAWB must be entered to use Advance Air Cargo Report." // non-translatable validation message
				: MasterBillValidator.GetMAWBFormatValidMessage(mawbNumber, consol.Factory);

			acas.WayBillNumber = mawbNumber.FormatAirMAWB();
			acas.WayBillNumberInfo.AddMessageError(() => !mawbFormatMessage.IsEmpty, mawbFormatMessage);
		}

		#endregion

		#region Locations

		void PopulateLocations(ACASHouseChecklist acas)
		{
			acas.PortOfOrigin = Unloco.Create(context, consol.LoadPort);
			acas.PortOfDestination = Unloco.Create(context, consol.DischargePort);

			var airLegDischargingInUS = consol.Transports
				.OfType<Freight.Business.Transport>()
				.OrderBy(transport => transport.JW_LegOrder)
				.FirstOrDefault(transport => transport.JW_RL_NKDiscPort.StartsWith(Core.Constants.CountryCodes.UnitedStates, StringComparison.OrdinalIgnoreCase) && transport.IsAir);
			acas.PortOfFirstArrival = airLegDischargingInUS != null
				? Unloco.Create(context, airLegDischargingInUS.DiscPort)
				: Unloco.Create(context, consol.DischargePort);
		}

		#endregion

		#region Weights and Measures

		void PopulateWeightsAndMeasures(ACASHouseChecklist acas)
		{
			var newWeightUnit = Core.Constants.Weight.IsImperial(consol.JK_TotalShipmentWeightUnit)
				? Core.Constants.Weight.Pounds
				: Core.Constants.Weight.Kilograms;

			acas.Weight = new Measurement()
			{
				Value = Core.Constants.Weight.Convert(consol.JK_TotalShipmentWeight, consol.JK_TotalShipmentWeightUnit, newWeightUnit),
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = newWeightUnit
				}
			};

			acas.TotalNoOfPacks = (ZInt)consol.JK_TotalShipmentQuantity;
		}

		#endregion

		#region Addresses

		void PopulateAddresses(ACASHouseChecklist acas)
		{
			bool IsUSACASCode(OrgCusCode code)
			{
				return code.OK_CodeType == OrgCusCode.USACodeTypes.ACASOriginatorCode && code.OK_RN_NKCodeCountry.StartsWith(Core.Constants.CountryCodes.UnitedStates, StringComparison.OrdinalIgnoreCase);
			}

			var acasCode = GlbBranch.CurrentBranch?.OrgProxy?.CustomsCodes.OfType<OrgCusCode>().FirstOrDefault(IsUSACASCode)
				?? GlbCompany.CurrentCompany?.OrgProxy?.CustomsCodes.OfType<OrgCusCode>().FirstOrDefault(IsUSACASCode);

			acas.Carrier = AddressBuilder.Create(context, consol.ShippingLineAddress);
			acas.BookingParty = AddressBuilder.Create(context, consol.SendingForwarderAddress);
			acas.SendersAcasCode = acasCode?.OK_CustomsRegNo ?? ZString.Empty;
			acas.SendersAcasCodeInfo.AddMessageErrorIfEmpty(Res.GetString("AEF7CCD6-03FE-66B5-49E2-41AD7B82A86D", "This code is required for ACAS messaging. Raise an eRequest to register your interest. Once provided by WTG, enter the code against the Branch or Company Organization Proxy > Config > Registration Numbers/Codes tab using Type = US ACA."));
		}

		#endregion

		#region ACAS State

		void PopulateStateDetails(ACASHouseChecklist acas)
		{
			var log = GetMostRecentLog(consol);
			if (log != null)
			{
				switch (log.SL_SE_NKEvent)
				{
					case Events.MessageSentCode:
						acas.State = AcasHouseChecklistMessageState.OriginalSent;
						acas.DisplayInformation = Res.GetString("57498db8-9b54-4255-83a5-79bbe2816e8c", "The message has been sent to ACAS.");
						break;

					case Events.InterchangeSentCode:
						acas.State = AcasHouseChecklistMessageState.OriginalForwarded;
						acas.DisplayInformation = Res.GetString("0dd80c53-a8a2-45ee-b8f8-c9b9dd6fa8cf", "The message has been routed on to the recipient CBP (US Customs and Border Protection).");
						break;

					case Events.InterchangeRejectedCode:
						acas.State = AcasHouseChecklistMessageState.RejectedByEHub;
						acas.DisplayInformation = Res.GetString("a526307d-4708-43ba-87ea-bd0c3312e0a5", "Message rejected by eHub ('{0}').", log.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason));
						break;

					case Events.MessageRejectedCode:
						acas.State = AcasHouseChecklistMessageState.RejectedByCBP;
						acas.DisplayInformation = Res.GetString("7a109e64-6e26-4e9e-9b80-906ff739d5f2", "Message rejected by CBP ('{0}').", log.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason));
						break;

					case Events.MessagePendingProcessingCode:
						acas.State = AcasHouseChecklistMessageState.MessagePendingProcessing;
						acas.DisplayInformation = Res.GetString("446d9d36-3f58-4ecb-ad79-cdd792d63131", "Message Pending Processing by Customs because Security Filing Received - Assessment in Progress.");
						break;

					case Events.ClearanceCompletedCode:
						acas.State = AcasHouseChecklistMessageState.ClearanceCompleted;
						acas.DisplayInformation = Res.GetString("fd604741-06bf-4174-b96e-a480b468ef4b", "Clearance Completed by Customs because Security Filing Received - Assessment Complete.");
						break;

					case Events.HeldCode:
						acas.State = AcasHouseChecklistMessageState.HoldInPlace;
						acas.DisplayInformation = GetDisplayInformationForCode(log.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason));
						break;

					case Events.ClearedHoldCode:
						acas.State = AcasHouseChecklistMessageState.HoldRemoved;
						acas.DisplayInformation = GetDisplayInformationForCode(log.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason));
						break;
				}
			}
			else
			{
				acas.State = AcasHouseChecklistMessageState.None;
				acas.DisplayInformation = Res.GetString("54e95288-a584-49a6-ba07-948187850104", "The ACAS House Checklist Message has not been sent.");
			}
		}

		string GetDisplayInformationForCode(string code)
		{
			switch (code)
			{
				case CargoWise.EventReference.Constants.ACASActions.Code.DoNotLoadHold:
					return Res.GetString("1b77a359-45a0-400b-acbf-f89da6b8120a", "Shipment Report Held by Customs because Do Not Load (DNL) Hold.");

				case CargoWise.EventReference.Constants.ACASActions.Code.DoNotLoadHoldRemoved:
					return Res.GetString("9f7175da-68f9-416d-ba21-ff9c9f5994c8", "Cleared Hold by Customs because Do Not Load Hold Removed.");

				case CargoWise.EventReference.Constants.ACASActions.Code.DoNotLoadHoldCurrentlyInPlace:
					return Res.GetString("5a666b58-ab05-4c5c-9ef4-2d42167e250d", "Held by Customs because Do Not Load Hold Currently In Place.");

				case CargoWise.EventReference.Constants.ACASActions.Code.SelecteeDataIssueHold:
					return Res.GetString("e9feedfb-fa2a-463f-8269-2c732220d6f3", "Held by Customs because Selectee Data Issue Hold.");

				case CargoWise.EventReference.Constants.ACASActions.Code.SelecteeDataIssueHoldRemoved:
					return Res.GetString("a5125bb3-4513-4d7b-8e9a-788c9bb33a4b", "Cleared Hold by Customs because Selectee Data Issue Hold Removed.");

				case CargoWise.EventReference.Constants.ACASActions.Code.SelecteeDataIssueHoldCurrentlyInPlace:
					return Res.GetString("67802d07-32b3-4c29-b369-4adabe6131b6", "Held by Customs because Selectee Data Issue Hold Currently In Place.");

				case CargoWise.EventReference.Constants.ACASActions.Code.SelecteeScreeningOrVerificationRequiredHold:
					return Res.GetString("76ad4e48-ffe6-4343-8efe-a125a622f563", "Held by Customs because Selectee Screening (or Verification) Required Hold.");

				case CargoWise.EventReference.Constants.ACASActions.Code.SelecteeScreeningOrVerificationRequiredHoldRemoved:
					return Res.GetString("c32a2c9d-f01d-485f-a6a5-7384cc219613", "Cleared Hold by Customs because Selectee Screening (or Verification) Required Hold Removed.");

				case CargoWise.EventReference.Constants.ACASActions.Code.SelecteeScreeningOrVerificationRequiredHoldCurrentlyInPlace:
					return Res.GetString("d2bcef18-0d3e-40f6-95d9-e7fd770ac772", "Held by Customs because Selectee Screening (or Verification) Required Hold Currently In Place.");

				default:
					return string.Empty;
			}
		}

		#region Logs

		StmALog GetMostRecentLog(IStmALogParent parent)
		{
			return parent
				.Logs?
				.GetAllLogs()
				.OfType<StmALog>()
				.OrderByDescending(l => l.SL_PostedTimeUtc)
				.FirstOrDefault(l => !l.SL_IsCancelled && IsAcasLog(l));
		}

		bool IsAcasLog(StmALog log)
		{
			switch (log.SL_SE_NKEvent)
			{
				case Events.MessageSentCode:
				case Events.InterchangeSentCode:
				case Events.InterchangeRejectedCode:
				case Events.MessageRejectedCode:
				case Events.MessagePendingProcessingCode:
				case Events.ClearanceCompletedCode:
				case Events.HeldCode:
				case Events.ClearedHoldCode:
					return string.Compare(log.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType),
						DocumentNames.AdvancedManifest,
						StringComparison.OrdinalIgnoreCase) == 0;

				default:
					return false;
			}
		}

		#endregion

		#endregion

		#region Shipments

		void PopulateShipments(ACASHouseChecklist acas)
		{
			var shipments = new List<ACASHouseChecklistShipment>();

			foreach (var shipment in consol.Shipments.OfType<ForwardingShipment>().OrderBy(s => s.JS_UniqueConsignRef))
			{
				var acasShipment = new ACASHouseChecklistShipment(
					nameof(ForwardingShipment),
					shipment.JS_UniqueConsignRef,
					DocumentNames.AdvancedManifest);

				acasShipment.WayBillNumber = shipment.JS_HouseBill;
				acasShipment.ShipmentNumber = shipment.JS_UniqueConsignRef;

				var goodsDescription = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description).FirstOrDefault();
				acasShipment.GoodsDescription = goodsDescription != null
					? goodsDescription.ST_NoteDataAsText
					: shipment.JS_GoodsDescription;

				acasShipment.PortOfOrigin = Unloco.Create(context, shipment.Origin);
				acasShipment.PortOfDestination = Unloco.Create(context, shipment.Destination);
				acasShipment.TotalNoOfPacks = shipment.JS_OuterPacks;

				var newWeightUnit = Core.Constants.Weight.IsImperial(shipment.JS_UnitOfWeight)
					? Core.Constants.Weight.Pounds
					: Core.Constants.Weight.Kilograms;

				acasShipment.Weight = new Measurement()
				{
					Value = Core.Constants.Weight.Convert(shipment.JS_ActualWeight, shipment.JS_UnitOfWeight, newWeightUnit),
					Unit = new CodeDescription(context.WeightUnits)
					{
						Code = newWeightUnit
					}
				};

				PopulateShipmentACASStateDetails(acasShipment, shipment);

				shipments.Add(acasShipment);
			}

			acas.Shipments = shipments;
		}

		#endregion

		#region Shipment ACAS State

		void PopulateShipmentACASStateDetails(ACASHouseChecklistShipment acasShipment, ForwardingShipment shipment)
		{
			acasShipment.ACASEventDate = GetACASShipmentEventDate(shipment);

			if (shipment.IsHighVolumeLowValue)
			{
				var displayInformation = ObjectFactory.Get<IHVLVShipmentACASStateDetailsProvider>("IHVLVShipmentACASStateDetailsProvider").Populate(shipment, acasShipment.ShipmentNumber, acasShipment.DisplayInformationInfo);

				acasShipment.DisplayInformation = displayInformation;
			}
			else
			{
				acasShipment.State = AirCargoAdvanceScreeningBuilder.GetState(shipment);
				acasShipment.DisplayInformation = GetACASShipmentDisplayInformation(shipment, acasShipment.State);

				acasShipment.DisplayInformationInfo.AddMessageError(() => acasShipment.State == AcasState.None,
					Res.GetString("9776f1c7-af36-4b7b-983e-2fdb5638499a", "The ACAS House Checklist message can only be sent when all House Bills have been reported to ACAS."));

				acasShipment.DisplayInformationInfo.AddMessageError(() => new[] { AcasState.OriginalSent, AcasState.AmendmentSent }.Contains(acasShipment.State) && IsLatestShipmentMessageAwaitingForwardingByEHub(shipment),
					Res.GetString("39f0c8ab-c6d9-4043-801c-9b73dcf20d85", "The ACAS Shipment Report message is awaiting forwarding by eHub."));

				acasShipment.DisplayInformationInfo.AddMessageError(() => new[] { AcasState.AcknowledgementRequired, AcasState.HoldInPlace, AcasState.AmendmentRequired }.Contains(acasShipment.State),
					Res.GetString("b5c173cd-c0ad-409e-9feb-b48c9412e918", "Shipment {0} is currently on hold with CBP.", acasShipment.ShipmentNumber));
			}
		}

		bool IsLatestShipmentMessageAwaitingForwardingByEHub(ForwardingShipment shipment)
		{
			var log = AirCargoAdvanceScreeningBuilder.GetOrderedLogs(shipment).FirstOrDefault();
			return log != null && log.SL_SE_NKEvent == Events.MessageSentCode;
		}

		#region Shipment Event Date

		ZDateTime GetACASShipmentEventDate(ForwardingShipment shipment)
		{
			var log = AirCargoAdvanceScreeningBuilder.GetOrderedLogs(shipment).FirstOrDefault();
			return log?.SL_EventTime ?? ZDateTime.Empty;
		}

		#endregion

		#region Shipment Display Information

		string GetACASShipmentDisplayInformation(ForwardingShipment shipment, AcasState state)
		{
			switch (state)
			{
				case AcasState.OriginalSent:
					return IsLatestShipmentMessageAwaitingForwardingByEHub(shipment)
						? Res.GetString("7c84551b-8d3b-47c3-91a5-bcd83c5997e4", "Await response - Original submitted to eHub.")
						: Res.GetString("3facdcff-fff9-4771-bd00-b4e810a1d6ca", "Await response - Original sent to CBP.");

				case AcasState.AmendmentSent:
					return IsLatestShipmentMessageAwaitingForwardingByEHub(shipment)
						? Res.GetString("3bb671b3-e052-4d9b-8d04-feb504153aae", "Await response - Amendment submitted to eHub.")
						: Res.GetString("d64510a2-1593-4b66-97a2-28980451ad9b", "Await response - Amendment sent to CBP.");

				case AcasState.AcknowledgementSent:
					return Res.GetString("3c0df595-b0b3-479d-b8f4-7e00d0454c88", "Await response - Acknowledgement sent to CBP.");

				case AcasState.AcknowledgementRequired:
					return Res.GetString("cb574beb-cfc8-4c9f-8ac2-c4b25badc120", "Acknowledgement required - \"On Hold\" with CBP.");

				case AcasState.HoldInPlace:
					return Res.GetString("7d451a61-dc1b-4a25-94d7-0208e162e245", "Await response - \"Hold Currently in Place\" with CBP.");

				case AcasState.AmendmentRequired:
					return Res.GetString("f6bbf1ce-85e9-45e6-ab4b-b6eee886fba9", "Amendment required - Selectee Data Issue Hold.");

				case AcasState.AssessmentOngoing:
					return Res.GetString("8182304e-bcc4-43f8-98d3-5f00c1574223", "Await response - CBP risk assessment ongoing.");

				case AcasState.HoldRemoved:
					return Res.GetString("f174b42c-1f02-44d3-847e-f8e5ee332fd7", "Approved - \"Hold\" status removed by CBP.");

				case AcasState.AssessmentComplete:
					return Res.GetString("297abb9d-f855-478e-afec-8298f78134b1", "Approved to be uplifted/loaded.");

				case AcasState.None:
					return Res.GetString("58b83df2-7911-46fc-b302-958733078af1", "The ACAS Shipment Report has not been sent.");

				default:
					return ZString.Empty;
			}
		}

		#endregion

		#endregion

		#endregion
	}
}
