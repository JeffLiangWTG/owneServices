using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR
{
	sealed class CargoControlAndTransitHouseManifestBuilder
	{
		public CargoControlAndTransitHouseManifestBuilder(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
			context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingConsol consol;
		readonly IContext context;

		public CargoControlAndTransitHouseManifest Build()
		{
			var cctHouseManifest = new CargoControlAndTransitHouseManifest(
				nameof(ForwardingConsol),
				consol.JK_UniqueConsignRef);

			PopulateHeader(cctHouseManifest);
			PopulateShipments(cctHouseManifest);

			AddValidations(cctHouseManifest);

			cctHouseManifest.ValidateAllIncludingChildren();

			return cctHouseManifest;
		}

		#region Validations

		void AddValidations(CargoControlAndTransitHouseManifest cctHouseManifest)
		{
			cctHouseManifest.ErrorPlaceHolderInfo.AddMessageError(()
				=> !cctHouseManifest.Shipments.Any(),
				Res.GetString("3A330051-EBA3-4D39-8644-3542AF914846", "Shipment Data is required to send CCT House Manifest."));
		}

		#endregion

		#region Header

		void PopulateHeader(CargoControlAndTransitHouseManifest cctHouseManifest)
		{
			PopulateSenderAndReceiver(cctHouseManifest);
			PopulateHeaderReferencesAndNumbers(cctHouseManifest);
			PopulateHeaderLocations(cctHouseManifest);
		}

		void PopulateSenderAndReceiver(CargoControlAndTransitHouseManifest cctHouseManifest)
		{
			cctHouseManifest.SendingParty = GlbBranch.CurrentBranch.OrgProxy?.MainAddress != null && !string.IsNullOrEmpty(GlbBranch.CurrentBranch.OrgProxy?.MainAddress?.Address1)
				? AddressBuilder.Create(context, GlbBranch.CurrentBranch.OrgProxy?.MainAddress)
				: AddressBuilder.Create(context, consol?.SendingForwarderAddress);

			var sendingParty = (Address)cctHouseManifest.SendingParty;

			sendingParty.AddPartyNameAndAddressValidation(Res.GetString("E2DCEBAE-6B96-4B8E-BAB2-7F30BF396964", "Sending Party"));

			cctHouseManifest.ReceivingAgent = AddressBuilder.Create(context, consol?.ReceivingForwarderAddress);

			var receivingAgent = (Address)cctHouseManifest.ReceivingAgent;

			receivingAgent.AddPartyNameAndAddressValidation(Res.GetString("0F18E93E-09A3-42B4-8732-9295975EFF54", "Receiving Agent"));

			if (consol?.ReceivingForwarderAddress != null)
			{
				var cnpj = GetCNPJ(consol?.ReceivingForwarderAddress.Header);

				if (cnpj != null)
				{
					cctHouseManifest.ReceivingAgent.TaxNumber = cnpj.OK_CustomsRegNo;
					receivingAgent.TaxNumberType = new CodeDescription(TaxList)
					{
						Code = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ,
						Description = (NoResString)"CNPJ Cadastro Nacional da Pessoa Jurídica" // programmatic constant
					};
				}
			}

			receivingAgent.TaxNumberInfo.AddMessageErrorIfEmpty(
				Res.GetString("BE996211-C46B-430D-B3C1-086C91A56AAD", "CNPJ is required for CCT messaging."));

			cctHouseManifest.SendingParty = sendingParty;
			cctHouseManifest.ReceivingAgent = receivingAgent;
		}

		void PopulateHeaderReferencesAndNumbers(CargoControlAndTransitHouseManifest cctHouseManifest)
		{
			var mawbNumber = consol.JK_MasterBillNum;
			var mawbFormatMessage = mawbNumber.IsEmpty ? ZString.Empty : MasterBillValidator.GetMAWBFormatValidMessage(mawbNumber, consol.Factory);

			cctHouseManifest.Mawb = consol.JK_MasterBillNum.FormatAirMAWB();
			cctHouseManifest.MawbInfo.AddMessageErrorIfEmpty(Res.GetString("484153CB-0294-43AC-BD5A-DD219C2AC583", "Master Air Waybill Number must be entered to use CCT House Manifest"));
			cctHouseManifest.MawbInfo.AddMessageError(() => !mawbFormatMessage.IsEmpty,
				MasterBillValidator.GetMAWBFormatValidMessage(mawbNumber, consol.Factory));

			cctHouseManifest.ConsolNumber = consol.JK_UniqueConsignRef;
			cctHouseManifest.ConsolNumberInfo.AddMessageErrorIfEmpty(Res.GetString("DC0FFE64-2099-4E75-9132-556EAAAE22DF", "Consol Number is required in CCT House Manifest"));

			cctHouseManifest.Packs = (ZInt)consol.JK_TotalShipmentQuantity;
			cctHouseManifest.PacksInfo.AddMessageErrorIfEmpty(Res.GetString("5E21F270-DA48-4A3A-8BC4-2412C9B92E3E", "Packs number is required in CCT House Manifest"));

			PopulateHeaderWeight(cctHouseManifest);
		}

		void PopulateHeaderWeight(CargoControlAndTransitHouseManifest cctHouseManifest)
		{
			cctHouseManifest.Weight = new Measurement()
			{
				Value = Constants.Weight.Convert(consol.JK_TotalShipmentWeight, consol.JK_TotalShipmentWeightUnit, Constants.Weight.Kilograms),
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = Constants.Weight.Kilograms
				}
			};

			((Measurement)cctHouseManifest.Weight).ValueInfo.AddMessageErrorIfEmpty(Res.GetString("93F9FA9C-461A-4101-8D8B-B36D3108EFE2", "Weight is required in CCT House Manifest"));
		}

		void PopulateHeaderLocations(CargoControlAndTransitHouseManifest cctHouseManifest)
		{
			var airLegDischargingInBR = GetFirstAirLegDischargingInBR();

			cctHouseManifest.AirportOfDeparture = new Unloco(context.Factory, context.Unlocos, context.Countries)
			{
				Code = consol.LoadPort.Code,
				Name = consol.LoadPort.RL_PortName
			};

			((Unloco)cctHouseManifest.AirportOfDeparture).CodeInfo.AddMessageErrorIfEmpty(
				Res.GetString("00D7DE5D-8012-4CFA-85D4-825B1F0D3B80", "Airport Of Departure is required for Shipments of CCT House Manifest"));

			cctHouseManifest.AirportOfDestination = new Unloco(context.Factory, context.Unlocos, context.Countries)
			{
				Code = consol.DischargePort.Code,
				Name = consol.DischargePort.RL_PortName
			};

			((Unloco)cctHouseManifest.AirportOfDestination).CodeInfo.AddMessageErrorIfEmpty(
				Res.GetString("6CD02F2B-2662-4B6E-A866-24A848F876A6", "Airport Of Destination is required for Shipments of CCT House Manifest"));

			cctHouseManifest.PortOfOrigin = new Unloco(context.Factory, context.Unlocos, context.Countries)
			{
				Code = airLegDischargingInBR?.LoadPort?.Code ?? ZString.Empty,
				Name = airLegDischargingInBR?.LoadPort?.RL_PortName ?? ZString.Empty
			};

			cctHouseManifest.PortOfFirstArrival = new Unloco(context.Factory, context.Unlocos, context.Countries)
			{
				Code = airLegDischargingInBR?.DiscPort?.Code ?? ZString.Empty,
				Name = airLegDischargingInBR?.DiscPort?.RL_PortName ?? ZString.Empty
			};
		}

		#endregion Header

		#region Shipment

		void PopulateShipments(CargoControlAndTransitHouseManifest cctHouseManifest)
		{
			var shipments = new List<CargoControlAndTransitDetail>();

			foreach (var shipment in consol.Shipments.OfType<ForwardingShipment>())
			{
				if (CargoControlAndTransitHouseManifestHelper.ShouldExcludeFromShipments(shipment))
				{
					continue;
				}

				var cctShipment = new CargoControlAndTransitDetail(
					nameof(ForwardingShipment),
					shipment.JS_UniqueConsignRef,
					DataContext.CargoControlAndTransitHouseManifest);

				PopulateShipmentReferencesAndLocations(cctShipment, shipment);
				PopulateShipmentWeight(shipment, cctShipment);
				PopulateShipmentGoodsDescription(shipment, cctShipment);
				PopulateStatusAndEvent(shipment, cctShipment);

				shipments.Add(cctShipment);
			}

			cctHouseManifest.Shipments = shipments;
		}

		void PopulateStatusAndEvent(ForwardingShipment shipment, CargoControlAndTransitDetail cctShipment)
		{
			var orderedLogs = CargoControlAndTransitHouseManifestHelper.GetAllApplicableCctOrderedLogs(shipment);
			var log = orderedLogs?.FirstOrDefault();

			if (log != null)
			{
				cctShipment.EventDateTime = log.SL_EventTime;
				cctShipment.Status = log.DisplayEventReference;
			}
			else
			{
				cctShipment.Status = Res.GetString("8538669D-ED05-4158-A599-D280F572979C", "No Advance Cargo Report Messages Have Been Sent.");
			}

			cctShipment.StatusInfo.AddMessageError(() => !IsShipmentMessageAccepted(orderedLogs), Res.GetString("7387aebd-622a-4094-9dcb-5e627f59dc56", "The CCT House Manifest can only be sent when all the House Bills have been accepted by CCT"));
		}

		void PopulateShipmentReferencesAndLocations(CargoControlAndTransitDetail cctShipment, ForwardingShipment shipment)
		{
			cctShipment.ShipmentNumber = shipment.JS_UniqueConsignRef;
			cctShipment.ShipmentNumberInfo.AddMessageErrorIfEmpty(Res.GetString("854C64D4-AB83-430E-9974-DE91B4D6F90D", "Shipment Number is required for Shipments of CCT House Manifest"));

			cctShipment.Hawb = shipment.JS_HouseBill;
			cctShipment.HawbInfo.AddMessageErrorIfEmpty(Res.GetString("A33A3548-5163-418E-A985-2CB749B4BA95", "House Air Waybill number is required for Shipments of CCT House Manifest"));

			cctShipment.Packs = shipment.JS_OuterPacks;
			cctShipment.PacksInfo.AddMessageErrorIfEmpty(Res.GetString("2F694BAC-469E-4F8F-BDA5-71B5E1233E84", "Packs number is required for Shipments of CCT House Manifest"));

			cctShipment.Origin = Unloco.Create(context, shipment.Origin);
			((Unloco)cctShipment.Origin).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("65CDBF6F-20CB-4F22-A64F-5FB361276190", "Origin is required for Shipments of CCT House Manifest"));

			cctShipment.Destination = Unloco.Create(context, shipment.Destination);
			((Unloco)cctShipment.Destination).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("83C43AF3-C369-4700-9794-56F8A5DF1831", "Destination is required for Shipments of CCT House Manifest"));
		}

		void PopulateShipmentWeight(ForwardingShipment shipment, CargoControlAndTransitDetail cctShipment)
		{
			cctShipment.Weight = new Measurement()
			{
				Value = Constants.Weight.Convert(shipment.JS_ActualWeight, shipment.JS_UnitOfWeight, Constants.Weight.Kilograms),
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = Constants.Weight.Kilograms
				}
			};

			cctShipment.Weight.ValueInfo.AddMessageErrorIfEmpty(Res.GetString("89F7090C-BCA5-4897-A7C4-70B51FD35E2C", "Weight is required for Shipments of CCT House Manifest"));
		}

		void PopulateShipmentGoodsDescription(ForwardingShipment shipment, CargoControlAndTransitDetail cctShipment)
		{
			var goodsDescription = shipment.Notes
				.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description).FirstOrDefault();

			cctShipment.GoodsDescription = goodsDescription != null
				? goodsDescription.ST_NoteDataAsText
				: shipment.JS_GoodsDescription;

			cctShipment.GoodsDescriptionInfo.AddMessageErrorIfEmpty(
				Res.GetString("A1B8DCCE-5CAD-44DD-BFF4-4D27CCF14A30", "Goods Description is required for Shipments of CCT House Manifest"));

			cctShipment.GoodsDescriptionInfo.AddMessageError(() => !cctShipment.GoodsDescription.IsWesternEuropeanOrEmpty,
				Res.GetString("F606D83F-141C-4B7A-965C-B206FE0E49F4", $"This text contains characters not supported by the Brazil Customs."));
		}

		bool IsShipmentMessageAccepted(StmALog[] orderedLogs)
		{
			if (orderedLogs != null)
			{
				foreach (var log in orderedLogs)
				{
					if (log.SL_SE_NKEvent == Events.MessageSentCode)
					{
						return false;
					}

					if (log.SL_SE_NKEvent == Events.MessageAcceptedCode)
					{
						return true;
					}
				}
			}

			return false;
		}

		#region Implementation

		CodeDescriptionPairList TaxList
		{
			get
			{
				if (taxList == null)
				{
					taxList = new CodeDescriptionPairList();
					taxList.AddPair(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, (NoResString)"CNPJ Cadastro Nacional da Pessoa Jurídica"); // programmatic constant
				}
				return taxList;
			}
		}
		CodeDescriptionPairList taxList;

		Freight.Business.Transport GetFirstAirLegDischargingInBR()
		{
			var airLegDischargingInBR = consol.Transports
				.OfType<Freight.Business.Transport>()
				.FirstOrDefault(transport => transport.IsAir &&
					!transport.JW_RL_NKLoadPort.StartsWith(Constants.CountryCodes.Brazil, StringComparison.OrdinalIgnoreCase) &&
					transport.JW_RL_NKDiscPort.StartsWith(Constants.CountryCodes.Brazil, StringComparison.OrdinalIgnoreCase));

			return airLegDischargingInBR;
		}

		OrgCusCode GetCNPJ(OrgHeader org)
		{
			return org?.CustomsCodes
				.Cast<OrgCusCode>()
				.FirstOrDefault(cusCode =>
					cusCode.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Brazil
					&& cusCode.OK_CodeType == BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ);
		}

		#endregion

		#endregion Implementation
	}
}
