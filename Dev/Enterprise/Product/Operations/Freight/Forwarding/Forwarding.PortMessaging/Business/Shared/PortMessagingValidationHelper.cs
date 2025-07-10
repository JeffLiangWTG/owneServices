using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	public static class PortMessagingValidationHelper
	{
		public static void CheckEntryType(ZPropertyInfo entryTypeInfo)
		{
			ListValidation.ErrorIfInvalidCode(entryTypeInfo);

			if ((ZString)entryTypeInfo.Value == EntryTypeList.Codes.AE1ExportDeclaration)
			{
				var eoriAndLRNEffectiveDate = PortMessagingRegistry.Instance.EORIAndLRNEffectiveDate.Value.ToString("yyyy-MM-dd");

				if (ZDateTime.UtcNow < PortMessagingRegistry.Instance.EORIAndLRNEffectiveDate.Value)
				{
					entryTypeInfo.AddError(Res.GetString("bfda7F0d-0ba6-41d0-896b-62b47e9ed81b", "Entry Type AE1: the new ATLAS Release 3.0 one-stage AES Procedure can be sent from <{0}>.", eoriAndLRNEffectiveDate));
				}

				if (ZDateTime.UtcNow < PortMessagingRegistry.Instance.EORIAndLRNEffectiveDate.Value.AddDays(180))
				{
					entryTypeInfo.AddWarning(Res.GetString("c3a3622f-05d4-450d-b08e-82132180ac90", "Entry Type AE1: from <{0}> it will be mandatory to provide EORI and LRN Local Reference Number as part of the new ATLAS Release 3.0 one-stage AES Procedure.", eoriAndLRNEffectiveDate));
				}
			}

			CheckHasNoDataEnteredOnRelatedPortMessagings(entryTypeInfo);
			CheckMarksAndNumbers(entryTypeInfo);
			CheckPorts(entryTypeInfo);
			CheckConsignor(entryTypeInfo);
		}

		static void CheckPorts(ZPropertyInfo entryTypeInfo)
		{
			var shipment = TryGetShipmentFromZPropertyInfo(entryTypeInfo);
			if (shipment == null || (ZString)entryTypeInfo.Value != EntryTypeList.Codes.EUPortOfDestination)
			{
				return;
			}

			foreach (ForwardingConsol consol in shipment.Consols)
			{
				var consolTransport = GetFirstSeaOrInlandWaterwayTransportLeg(consol) ?? consol.Transports.MostInterestingTransport;
				var isDepartingFrom2021 = consolTransport != null && consolTransport.JW_ETD.Year >= 2021 || shipment.JS_E_DEP.Year >= 2021;
				var isConsolPortValid = consol.DischargePort != null && (consol.DischargePort.IsInEU || consol.DischargePort.IsInNorthernIreland);
				var isShipmentPortValid = shipment.Destination != null && (shipment.Destination.IsInEU || shipment.Destination.IsInNorthernIreland);

				if (isDepartingFrom2021 && (!isConsolPortValid || !isShipmentPortValid))
				{
					entryTypeInfo.AddError(Res.GetString("6fedfa4c-8fdd-496c-9f01-d78a0e3d43a5", "Entry Type EUB can only be used for movements of union products for on-carriage transports from Hamburg to other EU ports."));
				}
			}
		}

		static Transport GetFirstSeaOrInlandWaterwayTransportLeg(ForwardingConsol consol)
		{
			return consol.Transports.Cast<Transport>()
				.Where(transport => (transport.IsSea || transport.JW_TransportMode == Core.Constants.TransportModes.InlandWaterwayTransport))
				.OrderBy(transport => transport.JW_LegOrder)
				.FirstOrDefault();
		}

		static void CheckConsignor(ZPropertyInfo entryTypeInfo)
		{
			var shipment = TryGetShipmentFromZPropertyInfo(entryTypeInfo);
			if (shipment?.Consignor == null || !new ZString[] { EntryTypeList.Codes.EmergencyConcept, EntryTypeList.Codes.AESExportDeclarationForMarketRegulationCommodities }.Contains((ZString)entryTypeInfo.Value))
			{
				return;
			}

			var etd = shipment.DepartureConsol?.Transports.MostInterestingTransport.JW_ETD;
			var cusCodeTypes = new string[] { OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, OrgCusCode.EuropeanUnionSharedCodeTypes.Turn };

			foreach (OrgCusCode cusCode in shipment.Consignor.CustomsCodes)
			{
				if (cusCode.OK_RN_NKCodeCountry == "GB" && cusCodeTypes.Any(x => x == cusCode.OK_CodeType) && etd >= new DateTime(2021, 1, 1) && cusCode.OK_CustomsRegNo.Substring(0, 2) != "XI")
				{
					entryTypeInfo.AddError(Res.GetString("15d5a55d-c303-4bf7-b19d-070c6623d9d4", "A United Kingdom EORI may no longer be used for export via Hamburg."));
				}
			}
		}

		static void CheckMarksAndNumbers(ZPropertyInfo entryTypeInfo)
		{
			var portMessage = (IPortMessaging)entryTypeInfo.BizObj;

			if (portMessage.EntryType == EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN
				&& portMessage.MarksAndNumbers.IsEmpty)
			{
				var message = Res.GetString(
						"aa5aa24d-1316-4fda-acf7-a51a6f9f3cc1",
						"Marks & Numbers are required for Entry Type {0} - {1}.",
						EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN,
						EntryTypeList.Descriptions.ExitSummaryDeclarationWithoutMRN);

				entryTypeInfo.AddError(message);
			}
		}

		public static void CheckMovementReferenceNumber(ZPropertyInfo mrnInfo)
		{
			var portMessaging = mrnInfo.BizObj as IPortMessaging;

			var compatibleEntryTypesRequiringMRN = new ZString[]
			{
				EntryTypeList.Codes.AESExportDeclaration,
				EntryTypeList.Codes.AESExportDeclarationForMarketRegulationCommodities,
				EntryTypeList.Codes.ExitSummaryDeclaration
			};

			if (compatibleEntryTypesRequiringMRN.Contains(portMessaging.EntryType))
			{
				MandatoryValidation.CheckEntered(mrnInfo);
			}
			else if (mrnInfo.Value.IsEmpty
				&& portMessaging.EntryType == EntryTypeList.Codes.Message
				&& new Annex30ATypeList().ContainsCode(portMessaging.Annex30AType))
			{
				mrnInfo.AddError(ResString.GetMultilingualString("959a333c-7044-453d-8205-9b9d793c6f8f", "The MRN of the relevant procedure has to be specified if Annex 30A data has already been transmitted (for the indicators E, V and A of Annex 30A field).  If the declaration was performed by using a fallback procedure, MRN field should include the reference / number of the master ticket of a fallback procedure."));
			}

			CheckHasNoDataEnteredOnRelatedPortMessagings(mrnInfo);

			if (!mrnInfo.Value.IsEmpty && !mrnInfo.Notifications.HasErrors())
			{
				var mrnMessageErrors = MRNValidationHelper.CheckMRNFormat((ZString)mrnInfo.Value, mrnInfo.BizObj.Factory);
				if (mrnMessageErrors != null)
				{
					foreach (var messageError in mrnMessageErrors)
					{
						mrnInfo.AddMessageError(messageError);
					}
				}
			}
		}

		public static void CheckMovementReferenceNumberComplete(ZPropertyInfo mrnCompleteInfo)
		{
			if ((ZBool)mrnCompleteInfo.Value)
			{
				CheckHasNoDataEnteredOnRelatedPortMessagings(mrnCompleteInfo);
			}
		}

		public static void CheckExemptionReason(ZPropertyInfo exemptionReasonInfo)
		{
			var portMessaging = exemptionReasonInfo.BizObj as IPortMessaging;
			if (portMessaging.EntryType == EntryTypeList.Codes.Message)
			{
				MandatoryValidation.CheckEntered(exemptionReasonInfo);
				if ((new ObsoleteExemptionReasonsList(portMessaging.EntryType)).ContainsCode(portMessaging.ExemptionReason))
				{
					CheckObsoleteReason(exemptionReasonInfo);
				}
				else
				{
					ListValidation.ErrorIfInvalidCode(exemptionReasonInfo);
				}
			}
			else if (portMessaging.EntryType == EntryTypeList.Codes.OtherExemptions)
			{
				MandatoryValidation.CheckEntered(exemptionReasonInfo);
				ListValidation.ErrorIfInvalidCode(exemptionReasonInfo);

				var shipmentPortMessaging = portMessaging as ShipmentPortMessaging;
				if (shipmentPortMessaging != null)
				{
					bool shipmentGreaterThan1000EUR = GetValueInEuro(shipmentPortMessaging.Shipment) > 1000;
					ZString exemptionReason = (ZString)exemptionReasonInfo.Value;

					if (shipmentGreaterThan1000EUR && exemptionReason != ExemptionReasonList.Codes.ValueOver1000Euro)
					{
						exemptionReasonInfo.AddWarning(ResString.GetMultilingualString("cb8cac43-b190-426f-b32b-118cb00cea04", "Shipment Goods Value is calculated to be greater than 1000 EUR."));
					}
					else if (!shipmentGreaterThan1000EUR && exemptionReason == ExemptionReasonList.Codes.ValueOver1000Euro)
					{
						exemptionReasonInfo.AddWarning(ResString.GetMultilingualString("ed0c2f6c-f05a-4765-a483-67bf89ef4512", "Shipment Goods Value is not calculated to be greater than 1000 EUR."));
					}
				}
			}

			CheckHasNoDataEnteredOnRelatedPortMessagings(exemptionReasonInfo);
		}

		static void CheckObsoleteReason(ZPropertyInfo exemptionReasonInfo)
		{
			var message = Res.GetString("10348a91-f34f-4082-92df-862ee32697a2", "This Exemption Reason is now obsolete.");
			if (exemptionReasonInfo.HasChanges || !exemptionReasonInfo.BizObj.IsInDatabase)
			{
				exemptionReasonInfo.AddError(message);
			}
			else
			{
				ForwardingShipment parentShipment = TryGetShipmentFromZPropertyInfo(exemptionReasonInfo);
				if (parentShipment != null)
				{
					ForwardingConsol relevantConsol = parentShipment.Consols.Cast<ForwardingConsol>().FirstOrDefault(c => ConsolPortMessagingManager.IsValidConsolForDakosyPortMessaging(c));
					if (relevantConsol != null)
					{
						bool isExportConsol = ConsolPortMessagingManager.IsExportConsolForDakosy(relevantConsol);
						ZDateTime dateWhenExemptionReasonBecameObsolete = new ZDateTime(2017, 01, 01);
						ZDateTime relevantDate = new ZDateTime();
						foreach (Transport transport in relevantConsol.Transports)
						{
							if (!isExportConsol && transport.JW_RL_NKDiscPort == "DEHAM" && transport.IsSea)
							{
								relevantDate = transport.JW_ATA.IsEmpty ? transport.JW_ETA : transport.JW_ATA;
							}
							else if (isExportConsol && transport.JW_RL_NKLoadPort == "DEHAM" && transport.IsSea)
							{
								relevantDate = transport.JW_ATD.IsEmpty ? transport.JW_ETD : transport.JW_ATD;
							}
						}

						if (!relevantDate.IsEmpty && relevantDate < dateWhenExemptionReasonBecameObsolete)
						{
							exemptionReasonInfo.AddMessageError(message);
						}
						else
						{
							exemptionReasonInfo.AddError(message);
						}
					}
				}
			}
		}

		static ZDecimal GetValueInEuro(ForwardingShipment shipment)
		{
			if (shipment.GoodsValueCurr == null)
			{
				return 0m;
			}

			if (shipment.GoodsValueCurr.RX_Code != "EUR")
			{
				var euroCurrency = shipment.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "EUR");
				var currencyConverter = CurrencyConverter.New(shipment.Factory);
				currencyConverter.DateForRate = ZDateTime.Today;

				var euroMoney = currencyConverter.ConvertExact(new Money(shipment.JS_GoodsValue, shipment.GoodsValueCurr), euroCurrency);
				return euroMoney.Amount;
			}

			return shipment.JS_GoodsValue;
		}

		public static void CheckATBNumber(ZPropertyInfo atbNumberInfo)
		{
			var portMessaging = atbNumberInfo.BizObj as IPortMessaging;

			if (portMessaging != null)
			{
				var atbNumber = ((ZString)atbNumberInfo.Value);

				if (atbNumber.IsEmpty)
				{
					if (portMessaging.EntryType == EntryTypeList.Codes.Message)
					{
						atbNumberInfo.AddError(ResString.GetMultilingualString("040ff4d9-bef5-4260-a5ba-5cf1e8ca4ccf", "ATB (former customs registration number) is mandatory in case of declaration type MIT. Enter the ATB number of the summary declaration (goods Sum A) here."));
					}
					else if (portMessaging.EntryType == EntryTypeList.Codes.EUPortOfDestination)
					{
						ForwardingShipment parentShipment = TryGetShipmentFromZPropertyInfo(atbNumberInfo);

						if (parentShipment != null
							&& parentShipment.Origin != null
							&& !parentShipment.Origin.IsInEU
							&& parentShipment.Consols.OfType<ForwardingConsol>().Any(consol => IsConsolDepartingFromHamburg(consol)))
						{
							MandatoryValidation.CheckEntered(atbNumberInfo);
						}
					}
				}
				else if (atbNumber.Length != 18 && atbNumber.Length != 21)
				{
					atbNumberInfo.AddError(ResString.GetMultilingualString("cd4dec6d-1888-4904-8434-647420146e2d", "The Reference must have 18 characters (MRN) or 21 characters (ATLAS Reg. No.)"));
				}
				else if (atbNumber.Length == 18)
				{
					var customsOffice = atbNumber.Substring(4, 4);
					if (customsOffice != "4851")
					{
						atbNumberInfo.AddError(ResString.GetMultilingualString("1ce0cfe4-bd34-403c-9a0e-073eed5ae1cb", "MRN registration must be issued by customs office '4851' (digit 5-8)"));
					}
					else
					{
						var mrnError = MRNFormatValidator.CheckMRNFormat(atbNumber, atbNumberInfo.BizObj.Factory, ZString.Empty);
						if (!mrnError.IsEmpty)
						{
							atbNumberInfo.AddError(mrnError);
						}
					}
				}
				else if (atbNumber.Length == 21)
				{
					if (!atbNumber.IsLettersAndNumbersOnlyOrEmpty)
					{
						atbNumberInfo.AddError(ResString.GetMultilingualString("9e4c8927-5768-45b7-88ad-b6e857a08b80", "ATB Number should contain letters and numbers only."));
					}
					else if ((portMessaging.EntryType == EntryTypeList.Codes.EUPortOfDestination
									|| portMessaging.EntryType == EntryTypeList.Codes.Message
									|| portMessaging.EntryType == EntryTypeList.Codes.ExitSummaryDeclaration
									|| portMessaging.EntryType == EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN)
								&& !atbNumberInfo.Value.ToString().EndsWith("4851", StringComparison.CurrentCulture))
					{
						atbNumberInfo.AddError(ResString.GetMultilingualString("e306d77a-0ff6-4d6a-b9e6-f07f8b92e92b", "ATLAS Registration must be issued by customs office '4851' (digit 18-21)"));
					}
				}
			}

			CheckHasNoDataEnteredOnRelatedPortMessagings(atbNumberInfo);
		}

		public static void CheckAnnex30AType(ZPropertyInfo annex30ATypeInfo)
		{
			ListValidation.ErrorIfInvalidCode(annex30ATypeInfo);

			CheckHasNoDataEnteredOnRelatedPortMessagings(annex30ATypeInfo);
		}

		public static void CheckAnnex30AFailureProcess(ZPropertyInfo annex30AFailureProcessInfo)
		{
			if (!annex30AFailureProcessInfo.ReadOnly)
			{
				annex30AFailureProcessInfo.AddWarning(ResString.GetMultilingualString("47816951-ad65-4cf8-b2cd-d21d54774556", "Use this field to indicate if the declaration was performed by using an emergency concept."));
			}
		}

		public static void CheckExportDeclarationReference(ZPropertyInfo exportDeclarationReferenceInfo)
		{
			if (exportDeclarationReferenceInfo.Value.IsEmpty)
			{
				var portMessaging = exportDeclarationReferenceInfo.BizObj as IPortMessaging;

				if (portMessaging.EntryType == EntryTypeList.Codes.EmergencyConcept)
				{
					exportDeclarationReferenceInfo.AddError(ResString.GetMultilingualString("731db0a0-cdda-46f4-bee8-16fb5d35082d", "Export Declaration Number is mandatory for Entry Type '{0}' ({1}).", EntryTypeList.Codes.EmergencyConcept, EntryTypeList.Descriptions.EmergencyConcept));
				}
			}
		}

		public static void CheckCustomsReleaseDate(ZPropertyInfo customsReleaseDateInfo)
		{
			var portMessaging = customsReleaseDateInfo.BizObj as IPortMessaging;
			var message = ResString.GetMultilingualString(
						"1c03634e-1cff-49d6-863d-e8bdf03fb528",
						"Customs Release Date is required for Entry Type {0} - {1}",
						EntryTypeList.Codes.ExitSummaryDeclaration,
						EntryTypeList.Descriptions.ExitSummaryDeclaration);

			if (portMessaging != null
				&& portMessaging.EntryType == EntryTypeList.Codes.ExitSummaryDeclaration
				&& customsReleaseDateInfo.Value.IsEmpty
				&& (!customsReleaseDateInfo.BizObj.IsInDatabase || customsReleaseDateInfo.BizObj.HasChanges))
			{
				customsReleaseDateInfo.AddError(message);
			}
		}

		public static void CheckLocalReferenceNumber(ZPropertyInfo lrnInfo)
		{
			if (PortMessagingHelper.IsEORIAndLRNEffectiveDate())
			{
				var portMessaging = lrnInfo.BizObj as IPortMessaging;

				if (portMessaging.EntryType == EntryTypeList.Codes.AE1ExportDeclaration
						&& lrnInfo.Value.IsEmpty)
				{
					lrnInfo.AddError(ResString.GetMultilingualString("76EA2653-C0B6-4219-802E-2BFCC26BE3F9", "Entry Type AE1 requires LRN Local Reference Number to be entered."));
				}

				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Germany
						&& portMessaging.EntryType == EntryTypeList.Codes.AE1ExportDeclaration
						&& !lrnInfo.Value.IsEmpty
						&& lrnInfo.Value.ToString().Length > 22)
				{
					lrnInfo.AddError(ResString.GetMultilingualString("E45DA63A-EED5-4D05-AA03-4AF540E189A2", "Local Reference Number has a maximum limit of 22 characters."));
				}

				CheckHasNoDataEnteredOnRelatedPortMessagings(lrnInfo);
			}
		}

		public static void CheckLocalReferenceNumberComplete(ZPropertyInfo lrnCompleteInfo)
		{
			var portMessaging = lrnCompleteInfo.BizObj as IPortMessaging;

			if (PortMessagingHelper.IsEORIAndLRNEffectiveDate() && (ZBool)lrnCompleteInfo.Value && portMessaging.EntryType == EntryTypeList.Codes.AE1ExportDeclaration)
			{
				CheckHasNoDataEnteredOnRelatedPortMessagings(lrnCompleteInfo);
			}
		}

		#region Implementation

		static void CheckHasNoDataEnteredOnRelatedPortMessagings(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.ReadOnly && !propertyInfo.Value.IsEmpty && !propertyInfo.HasErrors())
			{
				var relatedPortMessagings = GetRelatedPortMessagings(propertyInfo.BizObj);
				if (relatedPortMessagings.Any(relatedPortMessaging => relatedPortMessaging.HasData()))
				{
					propertyInfo.AddError(ResString.GetMultilingualString("499c6594-d551-40b2-9ea8-1c94f91b3038", "Port messaging data may be entered either at Shipment level or at Pack Line level, not both."));
				}
			}
		}

		static IEnumerable<IPortMessaging> GetRelatedPortMessagings(BusinessObject portMessagingBizo)
		{
			var shipmentPortMessaging = portMessagingBizo as ShipmentPortMessaging;
			if (shipmentPortMessaging != null && shipmentPortMessaging.Shipment != null)
			{
				var query = new ZQuery(JobPackLinePortMessagingSchema.JLM_JL_PackLine, shipmentPortMessaging.Shipment.OuterPackLines.Select(x => x.PK));
				return shipmentPortMessaging.Factory.Load<PackLinePortMessaging>(query).Cast<IPortMessaging>();
			}

			var packlinePortMessaging = portMessagingBizo as PackLinePortMessaging;
			if (packlinePortMessaging != null && packlinePortMessaging.PackLine != null)
			{
				var query = new ZQuery(JobShipmentPortMessagingSchema.JSM_JS_Shipment, packlinePortMessaging.PackLine.JL_JS);
				return packlinePortMessaging.Factory.Load<ShipmentPortMessaging>(query).Cast<IPortMessaging>();
			}

			return Enumerable.Empty<IPortMessaging>();
		}

		static bool IsConsolDepartingFromHamburg(ForwardingConsol consol)
		{
			return consol != null
					&& (consol.JK_RL_NKLoadPort == "DEHAM" && consol.IsSea
					|| consol.Transports.Cast<Transport>().Any(t => t.JW_RL_NKLoadPort == "DEHAM" && (t.IsSea || t.JW_TransportMode == Core.Constants.TransportModes.InlandWaterwayTransport)));
		}

		static ForwardingShipment TryGetShipmentFromZPropertyInfo(ZPropertyInfo portMessagingPropertyInfo)
		{
			ForwardingShipment shipment = null;
			var shipmentPortMessaging = portMessagingPropertyInfo.BizObj as ShipmentPortMessaging;
			if (shipmentPortMessaging != null)
			{
				shipment = shipmentPortMessaging.Shipment;
			}
			else
			{
				var packlinePortMessaging = portMessagingPropertyInfo.BizObj as PackLinePortMessaging;
				if (packlinePortMessaging != null && packlinePortMessaging.PackLine != null)
				{
					shipment = packlinePortMessaging.PackLine.Shipment;
				}
			}

			return shipment;
		}

		#endregion
	}
}
