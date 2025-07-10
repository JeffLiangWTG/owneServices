using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class CINExportNotificationBuilder
	{
		public CINExportNotificationBuilder(ForwardingShipment shipment)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			this.context = new CommonContext(shipment.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingShipment shipment;
		readonly IContext context;

		public CINExportNotification Build()
		{
			var exportNotification = new CINExportNotification(
				nameof(ForwardingShipment),
				shipment.JS_UniqueConsignRef);

			exportNotification.MasterBill = shipment.Consols.Cast<ForwardingConsol>().FirstOrDefault(consol => consol.ShippingLine?.OH_IsAirLine ?? false)?.JK_MasterBillNum ?? ZString.Empty;
			exportNotification.MasterBillWithPrefix = exportNotification.MasterBill.InsertSafe(3, "-");
			exportNotification.HouseBill = shipment.JS_HouseBill;
			exportNotification.ShipmentID = shipment.JS_UniqueConsignRef;
			exportNotification.GrossWeight = new Measurement()
			{
				Value = Core.Constants.Weight.Convert(shipment.JS_ActualWeight, shipment.JS_UnitOfWeight, Core.Constants.Weight.Kilograms),
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = Core.Constants.Weight.Kilograms
				}
			};
			exportNotification.PackCount = shipment.JS_OuterPacks;
			exportNotification.PackType = new CodeDescription(shipment.Lookups.PackTypes)
			{
				Code = shipment.JS_F3_NKPackType
			};
			var agentTypes = new List<CodeDescription>();
			shipment.Consols.Cast<ForwardingConsol>().ForEach(consol => agentTypes.Add(new CodeDescription(consol.JK_AgentType_List) { Code = consol.JK_AgentType }));
			exportNotification.AgentTypes = agentTypes;
			PopulateNumbers(exportNotification);
			PopulateAddress(exportNotification);

			AddValidations(exportNotification);
			exportNotification.ValidateAllIncludingChildren();

			return exportNotification;
		}

		#region Details

		void PopulateNumbers(CINExportNotification exportNotification)
		{
			var types = new CodeDescriptionPairList();
			types.AddPair(CusEntryNumberTypes.Standard.MovementReferenceNumber, (NoResString)"Movement Reference Number"); // programmatic constant

			var referenceNumbers = new List<ReferenceNumber>();
			var cusEntryNumbers = shipment.CusEntryNumbers.Cast<CusEntryNumber>().Where(number => number.CE_RN_NKCountryCode == Core.Constants.CountryCodes.France);
			var numbers = shipment.Numbers.Cast<CusEntryNumber>().Where(number => number.CE_RN_NKCountryCode == Core.Constants.CountryCodes.France);

			var mrnNumbers = cusEntryNumbers.Where(number => number.CE_EntryType == CusEntryNumberTypes.Standard.MovementReferenceNumber);
			if (!mrnNumbers.Any())
			{
				mrnNumbers = numbers.Where(number => number.CE_EntryType == CusEntryNumberTypes.Standard.MovementReferenceNumber);
			}
			mrnNumbers.ForEach(number => { referenceNumbers.Add(ReferenceNumber.Create(context, number, types)); });

			exportNotification.MRNNumbers = referenceNumbers;
			exportNotification.StrMRNNumbers = ZString.Join(", ", referenceNumbers.Select(i => i.Value).ToArray());
			exportNotification.CustomsOfficeCode = numbers.FirstOrDefault(number => number.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.COC)?.CE_EntryNum ?? ZString.Empty;
		}

		void PopulateAddress(CINExportNotification exportNotification)
		{
			var airLeg = shipment.TransportsInLegOrder.Cast<Freight.Business.Transport>().FirstOrDefault(transport => transport.IsAir);
			exportNotification.OperationalPort = Unloco.Create(context, airLeg?.LoadPort);
			var proxyMainAddress = GlbBranch.CurrentBranch.OrgProxy?.MainAddress ?? GlbCompany.CurrentCompany.OrgProxy?.MainAddress;
			exportNotification.SendingParty = AddressBuilder.Create(context, proxyMainAddress);
			exportNotification.SendingPartyCINNumber = proxyMainAddress?.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CIN);
			exportNotification.SendingPartyCIN = exportNotification.SendingPartyCINNumber?.Value ?? ZString.Empty;

			exportNotification.Carrier = AddressBuilder.Create(context, airLeg?.CarrierAddress);
			exportNotification.CarrierCINNumber = airLeg?.CarrierAddress?.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CIN);
			if (exportNotification.CarrierCINNumber != null && exportNotification.CarrierCINNumber.Value.IsEmpty)
			{
				exportNotification.CarrierCINNumber.Value = airLeg?.Carrier?.MiscServ?.AirlineThreeLetterCode ?? ZString.Empty;
			}
			exportNotification.CarrierCIN = exportNotification.CarrierCINNumber?.Value ?? ZString.Empty;

			var warehouseAddress = shipment.Consols.Cast<ForwardingConsol>().FirstOrDefault
													(c => exportNotification.CustomsOfficeCode == (c.PackDepotAddress?.Header?.CustomsCodes?.Cast<OrgCusCode>().FirstOrDefault
														(code => code.OK_RN_NKCodeCountry == CountryCodes.France && code.OK_CodeType == OrgCusCode.CodeTypes.CustomsOfficeForTransit)?.SecuredCustomsRegNo ?? ZString.Empty))
															?.PackDepotAddress;
			exportNotification.Warehouse = AddressBuilder.Create(context, warehouseAddress);
			exportNotification.WarehouseCINNumber = warehouseAddress?.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CIN);
			exportNotification.WarehouseCIN = exportNotification.WarehouseCINNumber?.Value ?? ZString.Empty;
		}

		void AddValidations(CINExportNotification exportNotification)
		{
			exportNotification.MasterBillWithPrefixInfo.AddMessageError(
				() => !Regex.IsMatch(exportNotification.MasterBill, @"^[0-9]{3}-?[0-9]{8}$"),
				Res.GetString("de04592d-3a50-49be-951e-2c0f32085820", "Master Air Waybill is not valid. (See Consol > MAWB)"));
			exportNotification.HouseBillInfo.AddMessageErrorIfEmpty(Res.GetString("3d04ae1a-cff5-4970-9c2a-0d0fd4313d58", "House Bill is required."));
			exportNotification.StrMRNNumbersInfo.AddMessageErrorIfEmpty(Res.GetString("715fb196-5d04-481a-a67d-24bf76adefe3", "MRN Number is required."));
			exportNotification.CustomsOfficeCodeInfo.AddMessageErrorIfEmpty(Res.GetString("0bea4afb-2326-4f20-8b76-358b627d712e", "Customs Office Code is required."));
			exportNotification.ShipmentIDInfo.AddMessageErrorIfEmpty(Res.GetString("c751812e-8daf-458d-9dd4-c788ba69da93", "Shipment ID is required."));
			((Measurement)exportNotification.GrossWeight).ValueInfo.AddMessageError(() => exportNotification.GrossWeight.Value.IsEmpty, Res.GetString("20c48a86-7857-469d-877a-104c02c7f366", "Gross Weight cannot be zero."));
			exportNotification.PackCountInfo.AddMessageErrorIfEmpty(Res.GetString("7041b8a1-1466-44a3-9304-0fb969386cab", "Packs is required."));
			((CodeDescription)exportNotification.PackType).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("31cdf348-afc8-4729-9d4d-fc5362a5d5e7", "Package Type is required."));
			((Unloco)exportNotification.OperationalPort).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("148f7be1-b331-4716-96eb-451f36400453", "Operational Port is required."));

			var addressValidation = Res.GetString("1b315204-c296-4126-ad86-7a174de621bb", "name and address is required.");
			exportNotification.SendingParty.AddPartyNameAndAddressValidation(Res.GetString("85e15fe9-4093-43ea-82e9-f346751ad720", "Sending Party"), addressValidation);
			exportNotification.Carrier.AddPartyNameAndAddressValidation(Res.GetString("a7a4632f-3b3f-47ce-9c52-f2d23c1148d7", "Carrier"), addressValidation);
			exportNotification.Warehouse.AddPartyNameAndAddressValidation(Res.GetString("7a7e92ba-abb6-4e61-a4ff-a0cf6005bad6", "CFS / Warehouse"), addressValidation);

			var cinValidation = Res.GetString("8302a489-b4f5-4e68-8762-344cfbdd94B4", "CIN is missing from this organization > Config > Registration Numbers/Codes - type CIN.");
			exportNotification.SendingPartyCINInfo.AddMessageErrorIfEmpty(cinValidation);
			exportNotification.CarrierCINInfo.AddMessageErrorIfEmpty(cinValidation);
			exportNotification.WarehouseCINInfo.AddMessageErrorIfEmpty(cinValidation);
		}

		#endregion
	}
}
