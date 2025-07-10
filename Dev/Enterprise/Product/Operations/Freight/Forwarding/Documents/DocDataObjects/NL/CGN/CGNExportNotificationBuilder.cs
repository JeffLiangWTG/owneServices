using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Core;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL
{
	sealed class CGNExportNotificationBuilder
	{
		public CGNExportNotificationBuilder(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
			context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingConsol consol;

		readonly IContext context;

		public CGNExportNotification Build()
		{
			var exportNotification = new CGNExportNotification(nameof(ForwardingConsol), consol.JK_UniqueConsignRef);

			exportNotification.MasterAirWaybill = consol.JK_MasterBillNum.FormatAirMAWB();

			PopulateAddress(exportNotification);
			PopulateShipments(exportNotification);

			AddValidations(exportNotification);
			exportNotification.ValidateAllIncludingChildren();

			return exportNotification;
		}

		#region Address

		void PopulateAddress(CGNExportNotification exportNotification)
		{
			var airTransports = consol
				.Transports
				.Cast<Freight.Business.Transport>()
				.Where(t => t.JW_TransportMode == Core.Constants.TransportModes.Air).ToList();

			var departNlAirLeg = airTransports.FirstOrDefault(t => t.JW_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.Netherlands, StringComparison.OrdinalIgnoreCase));
			exportNotification.OperationalPort = Unloco.Create(context, departNlAirLeg?.LoadPort);

			var proxyMainAddress = GlbBranch.CurrentBranch.OrgProxy?.MainAddress ?? GlbCompany.CurrentCompany.OrgProxy?.MainAddress;
			exportNotification.SendingParty = AddressBuilder.Create(context, proxyMainAddress);
			exportNotification.SendingPartyCGNNumber = GetCargonautRegistrationNumber(proxyMainAddress);
			exportNotification.SendingPartyCGN = exportNotification.SendingPartyCGNNumber?.Value ?? ZString.Empty;

			exportNotification.Carrier = AddressBuilder.Create(context, departNlAirLeg?.CarrierAddress);
			exportNotification.CarrierCGNNumber = GetCargonautRegistrationNumber(departNlAirLeg?.CarrierAddress);
			if (exportNotification.CarrierCGNNumber != null && exportNotification.CarrierCGNNumber.Value.IsEmpty)
			{
				exportNotification.CarrierCGNNumber.Value = departNlAirLeg?.Carrier?.MiscServ?.AirlineThreeLetterCode ?? ZString.Empty;
			}
			exportNotification.CarrierCGN = exportNotification.CarrierCGNNumber?.Value ?? ZString.Empty;
		}

		RegistrationNumber GetCargonautRegistrationNumber(OrgAddress address)
		{
			if (address == null)
			{
				return null;
			}
			return new RegistrationNumber()
			{
				Value = GetRegistrationNumberWithFallbackToOrgHeader(address, OrgCusCode.NetherlandsCodeTypes.CargonautRegistationCode, Core.Constants.CountryCodes.Netherlands),
				Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.Netherlands))
				{
					Code = OrgCusCode.NetherlandsCodeTypes.CargonautRegistationCode
				},
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.Netherlands
				}
			};
		}

		OrgCusCode GetOrgCusCode(IEnumerable<OrgCusCode> customsCodes, ZString type, ZString countryCode, OrgAddress premisesAddress = null)
		{
			return customsCodes?.FirstOrDefault(x => x.OK_CodeType == type && x.OK_RN_NKCodeCountry == countryCode && x.OK_OA_PremisesAddress == premisesAddress.PK)
					?? customsCodes?.FirstOrDefault(x => x.OK_CodeType == type && x.OK_RN_NKCodeCountry == countryCode && x.OK_OA_PremisesAddress == ZGuid.Empty);
		}

		ZString GetRegistrationNumberWithFallbackToOrgHeader(OrgAddress address, ZString type, ZString countryCode)
		{
			var orgCusCode = GetOrgCusCode(address?.CustomsCodes.OfType<OrgCusCode>(), type, countryCode, address)
							?? GetOrgCusCode(address?.Header?.CustomsCodes.OfType<OrgCusCode>(), type, countryCode, address);
			var result = orgCusCode?.OK_CustomsRegNo ?? ZString.Empty;

			return result;
		}

		#endregion

		#region Shipments

		void PopulateShipments(CGNExportNotification exportNotification)
		{
			var shipmentDOs = new List<Shipment>();
			var shipmentBuilder = new ShipmentBuilder(context);
			foreach (ForwardingShipment shipment in consol.TopLevelShipments)
			{
				var shipmentDO = shipmentBuilder.Build(shipment, null);
				shipmentDO.GrossWeight = new Measurement()
				{
					Value = Core.Constants.Weight.Convert(shipment.JS_ActualWeight, shipment.JS_UnitOfWeight, Core.Constants.Weight.Kilograms),
					Unit = new CodeDescription(context.WeightUnits)
					{
						Code = Core.Constants.Weight.Kilograms
					}
				};
				shipmentDO.MRNNumbers = GetShipmentMovementReferenceNumbers(shipment);
				shipmentDO.MRNNumbersConcatenated = ZString.Join("\r\n", shipmentDO.MRNNumbers.Select(i => i.Value).ToArray());
				shipmentDOs.Add(shipmentDO);
			}
			exportNotification.Shipments = shipmentDOs;
		}

		IReadOnlyCollection<IReferenceNumber> GetShipmentMovementReferenceNumbers(ForwardingShipment shipment)
		{
			var referenceNumbers = new List<ReferenceNumber>();
			var types = new CodeDescriptionPairList();
			types.AddPair(CusEntryNumberTypes.Standard.MovementReferenceNumber, (NoResString)"Movement Reference Number");
			var cusEntryNumbers = shipment.CusEntryNumbers.Cast<CusEntryNumber>().Where(number => number.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Netherlands);
			var numbers = shipment.Numbers.Cast<CusEntryNumber>().Where(number => number.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Netherlands);
			var mrnNumbers = cusEntryNumbers.Where(number => number.CE_EntryType == CusEntryNumberTypes.Standard.MovementReferenceNumber);

			if (!mrnNumbers.Any())
			{
				mrnNumbers = numbers.Where(number => number.CE_EntryType == CusEntryNumberTypes.Standard.MovementReferenceNumber);
			}

			mrnNumbers.ForEach(number => { referenceNumbers.Add(ReferenceNumber.Create(context, number, types)); });
			return referenceNumbers;
		}

		#endregion

		#region Validation

		void AddValidations(CGNExportNotification exportNotification)
		{
			var addressValidation = Res.GetString("a2d605c6-e7a0-4a20-94c2-0d875eb134a8", "name and address is required.");
			exportNotification.SendingParty.AddPartyNameAndAddressValidation((NoResString)"Sending Party", addressValidation);
			exportNotification.Carrier.AddPartyNameAndAddressValidation((NoResString)"Carrier", addressValidation);

			var cgnValidation = Res.GetString("8704e7f6-c23b-48c7-85a3-457cdc497a8b", "CGN is missing from this organization > Config > Registration Numbers/Codes - type CGN.");
			exportNotification.SendingPartyCGNInfo.AddMessageErrorIfEmpty(cgnValidation);
			exportNotification.CarrierCGNInfo.AddMessageErrorIfEmpty(cgnValidation);

			exportNotification.MasterAirWaybillInfo.AddMessageErrorIfEmpty(Res.GetString("9eb41f28-651f-4850-a072-94258dadfbae", "Master Air Waybill is required."));

			exportNotification.MasterAirWaybillInfo.AddMessageError(
				() => !Regex.IsMatch(exportNotification.MasterAirWaybill, @"^[0-9]{3}-?[0-9]{8}$"),
				Res.GetString("de04592d-3a50-49be-951e-2c0f32085820", "Master Air Waybill is not valid. (See Consol > MAWB)"));

			foreach (var shipment in exportNotification.Shipments)
			{
				shipment.ShipmentIDInfo.AddMessageErrorIfEmpty(Res.GetString("5adb6901-2a99-4559-bcf3-a37b81f785ea", "Shipment ID is required."));
				shipment.HouseBillNumberInfo.AddMessageErrorIfEmpty(Res.GetString("1245ea07-f7ba-45e2-aa1d-54275227caab", "House Bill is required."));
				shipment.MRNNumbersConcatenatedInfo.AddMessageErrorIfEmpty(Res.GetString("3daa8fb6-4929-4055-8fc7-1d99ac572cb0", "MRN Number is required."));

				foreach (var mrnNumber in shipment.MRNNumbers)
				{
					((ReferenceNumber)mrnNumber).ValueInfo.AddMessageErrorIfEmpty(Res.GetString("3daa8fb6-4929-4055-8fc7-1d99ac572cb0", "MRN Number is required."));
				}

				((Measurement)shipment.GrossWeight).ValueInfo.AddMessageErrorIfEmpty(Res.GetString("bf5f21ee-ac15-4fb1-80c0-1dd61cba8f35", "Gross Weight is required."));
				shipment.PackCountInfo.AddMessageErrorIfEmpty(Res.GetString("5203e87f-450c-4fba-9e45-45a00981cc90", "Packs is required."));
				((CodeDescription)shipment.PackType).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("153dd23e-8d39-4776-961c-5ba9ee651689", "Package Type is required."));
			}
		}

		#endregion
	}
}
