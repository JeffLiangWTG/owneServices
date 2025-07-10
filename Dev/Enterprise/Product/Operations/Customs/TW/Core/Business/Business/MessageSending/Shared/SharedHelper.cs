using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public static class SharedHelper
	{
		public static ZString GetVoyageFlightNo(JobDeclaration decl, bool needNIL = true)
		{
			var result = decl.JE_VoyageFlightNo;
			if (result.IsEmpty)
			{
				if (needNIL && decl.IsImport)
				{
					result = MessageConstants.TransportMeansJourneyIdTypes.NIL;
				}
			}
			else if (decl.IsAir)
			{
				result = FormatVoyageFlightNo(result);
			}
			return result;
		}

		public static ZString FormatVoyageFlightNo(ZString voyageFlightNo)
		{
			var match = new Regex(@"^(?<Airline>[A-Z0-9]{2})(?<FlightNumber>[0-9]{1,4})$").Match(voyageFlightNo);
			if (match.Success)
			{
				var matchGroup = match.Groups;
				return $"{matchGroup["Airline"].Value} {matchGroup["FlightNumber"].Value}";
			}
			else
			{
				return voyageFlightNo;
			}
		}

		public static ZString GetTransportID(JobDeclaration decl)
		{
			ZString result;
			if (decl.IsSea)
			{
				result = decl.Vessel?.RV_RadioCallSign ?? ZString.Empty;
			}
			else
			{
				result = GetVoyageFlightNo(decl, needNIL: false);
			}
			return result;
		}

		public static ZString GetCustomsOfficeName(ZString code)
		{
			return new TaiwanCustomsDistrictList().GetDescriptionFromCode(code.Left(1));
		}

		public static ZString GetIDStartWithNO(ZString id, ZString type)
		{
			ZString result;
			if (!id.IsEmpty && type == PartyIdentifierCodeList.Codes._53)
			{
				result = $"NO{id}";
			}
			else
			{
				result = id;
			}
			return result;
		}

		public static string GetFunctionalReferenceIDPlaceHolderWithPK(ZGuid licensingHeaderPK)
		{
			return $"<<FUNCTIONAL REFERENCE ID PLACE HOLDER {licensingHeaderPK}>>";
		}

		public static string GetFunctionalReferenceIDPlaceHolderWithPKHtml(ZGuid licensingHeaderPK)
		{
			return $"&lt;&lt;FUNCTIONAL REFERENCE ID PLACE HOLDER {licensingHeaderPK}&gt;&gt;";
		}

		public static ZString GetCharacteristicCode(this CusContainer cusContainer)
		{
			return GetContainerCharacteristicCode(cusContainer.Container);
		}

		public static ZString GetCharacteristicCode(this CusInBondContainer cusInBondContainer)
		{
			return GetContainerCharacteristicCode(cusInBondContainer.Container);
		}

		public static ZString ExtractSubBoxID(ZString mailBoxNum)
		{
			var subBoxId = ZString.Empty;
			if (!mailBoxNum.IsEmpty && mailBoxNum.ContainsAnyChar("-"))
			{
				var splitResult = mailBoxNum.Split('-');
				if (splitResult.Length == 2)
				{
					subBoxId = splitResult[1];
				}
			}
			return subBoxId;
		}

		static ZString GetContainerCharacteristicCode(RefContainer container)
		{
			var result = container?.GetCountrySpecificContainerCode(Core.Constants.CountryCodes.Taiwan) ?? ZString.Empty;
			if (result.IsEmpty)
			{
				result = container?.RC_Code ?? ZString.Empty;
			}
			return result;
		}

		public static IEnumerable<ZString> GetSeals(this CusContainer cusContainer)
		{
			var seal1 = cusContainer.CO_Seal;
			var seal2 = cusContainer.CO_SecondSeal;
			if (!seal1.IsEmpty)
			{
				yield return seal1;
			}

			if (!seal2.IsEmpty)
			{
				yield return seal2;
			}
		}

		#region UsedCapacityCode
		public static ZString GetUsedCapacityCode(this CusContainer cusContainer)
		{
			return GetUsedCapacityCode(cusContainer.CO_FCL_LCL_AIR, cusContainer.CO_IsPart);
		}

		public static ZString GetUsedCapacityCode(this CusInBondContainer cusInBondContainer)
		{
			return GetUsedCapacityCode(cusInBondContainer.BC_Mode, cusInBondContainer.BC_IsPart);
		}

		static ZString GetUsedCapacityCode(ZString mode, ZBool isPart)
		{
			var result = ZString.Empty;
			switch (mode)
			{
				case Core.Constants.ContainerModes.Empty:
					result = MessageConstants.TransportEquipmentsCapacityCodes.Zero;
					break;
				case Core.Constants.ContainerModes.FCL:
					result = isPart ? MessageConstants.TransportEquipmentsCapacityCodes.Five : MessageConstants.TransportEquipmentsCapacityCodes.One;
					break;
				case Core.Constants.ContainerModes.Groupage:
					result = MessageConstants.TransportEquipmentsCapacityCodes.Two;
					break;
				case Core.Constants.ContainerModes.LCL:
					result = MessageConstants.TransportEquipmentsCapacityCodes.Three;
					break;
				case Core.Constants.ContainerModes.BuyersConsol:
					result = isPart ? MessageConstants.TransportEquipmentsCapacityCodes.Six : MessageConstants.TransportEquipmentsCapacityCodes.Four;
					break;
			}
			return result;
		}
		#endregion

		#region Transport Contract Documents
		public static IEnumerable<ITransportContractDocument> GetTransportContractDocumentsWithMasterBillSegmentID(this JobDeclaration declaration, Func<ZString, ZString, ITransportContractDocument> getDocument)
		{
			var containerNoteBills = declaration.Bills.Cast<Bill>()
				.Where(bill => bill.CU_BillType == BillTypeList.Codes.ContainerNote && !bill.CU_BillNum.IsEmpty)
				.Select(x => x.CU_BillNum).ToList() ?? new List<ZString>();
			var masterBillSegmentID = declaration.GetMasterBillSegmentID();
			var isMasterBillFromDeclaration = masterBillSegmentID == declaration.JE_MasterBill;
			return GetTransportContractDocuments(getDocument, declaration.IsAir, masterBillSegmentID, declaration.JE_HouseBill, containerNoteBills, isMasterBillFromDeclaration);
		}

		public static ZString GetMasterBillSegmentID(this JobDeclaration declaration)
		{
			var masterBill = declaration.JE_MasterBill;
			var entryInstruction = declaration.CusEntryInstruction;
			ZString result;
			if (declaration.FreeTradeZoneDeclarationTypes)
			{
				if (declaration.IsFreeTradeZoneDocumentaryAddress)
				{
					if (declaration.IsExport)
					{
						if (entryInstruction.CEI_WHSMonth.IsEmpty)
						{
							result = declaration.EntryHeader?.EntryNumberForSendingObject ?? ZString.Empty;
						}
						else
						{
							result = MessageConstants.TransportContractDocumentTypeCodes.NIL;
						}
					}
					else
					{
						result = declaration.EntryHeader?.EntryNumberForSendingObject ?? ZString.Empty;
					}
				}
				else
				{
					result = masterBill;
				}
			}
			else if (declaration.DeclarationType == Constants.DeclarationTypes.Export.B2 || (masterBill.IsEmpty && declaration.AutomaticallyDeclareNILForDeclarationType))
			{
				result = MessageConstants.TransportContractDocumentTypeCodes.NIL;
			}
			else
			{
				result = masterBill;
			}

			return result;
		}

		public static IEnumerable<ITransportContractDocument> GetTransportContractDocuments(this CusInBondHeader header, CusInBondBill bill, Func<ZString, ZString, ITransportContractDocument> getDocument)
		{
			return GetTransportContractDocuments(getDocument, bill?.IsAirForMasterBill ?? false, bill?.B0_MasterBillNumber ?? ZString.Empty, bill?.B0_HouseBillNumber ?? ZString.Empty);
		}

		public static IEnumerable<ITransportContractDocument> GetTransportContractDocuments(Func<ZString, ZString, ITransportContractDocument> getDocument, bool isAir, ZString masterBill, ZString houseBill, List<ZString> containerNoteBills = null, bool shouldFormatForMAWB = true)
		{
			if (getDocument != null)
			{
				if (!masterBill.IsEmpty)
				{
					if (shouldFormatForMAWB && isAir && masterBill.Length > 3 && masterBill[3] != '-')
					{
						masterBill = masterBill.Insert(3, "-");
					}
					yield return getDocument(masterBill, isAir ? MessageConstants.TransportContractDocumentTypeCodes._741 : MessageConstants.TransportContractDocumentTypeCodes._704);
				}

				if (!houseBill.IsEmpty)
				{
					yield return getDocument(houseBill, isAir ? MessageConstants.TransportContractDocumentTypeCodes._703 : MessageConstants.TransportContractDocumentTypeCodes._714);
				}

				if (containerNoteBills != null)
				{
					foreach (var billNum in containerNoteBills)
					{
						if (!billNum.IsEmpty)
						{
							yield return getDocument(billNum, MessageConstants.TransportContractDocumentTypeCodes._976);
						}
					}
				}
			}
		}
		#endregion

		public static IEnumerable<IClassification> GetClassifications(this CusEntryLine entryLine, Func<ZString, ZString, IClassification> getClassificationFunc, bool isSingleInvoiceLine)
		{
			var invoiceLines = isSingleInvoiceLine ? new List<JobComInvoiceLine> { entryLine.RandomLine } : entryLine.InvoiceLines.Cast<JobComInvoiceLine>().ToList();
			var keys = new List<ZString>();
			foreach (var invoiceLine in invoiceLines)
			{
				foreach (var classification in invoiceLine.GetClassifications(getClassificationFunc, keys))
				{
					yield return classification;
				}
			}
		}

		internal static IEnumerable<IClassification> GetClassifications(this JobComInvoiceLine invoiceLine, Func<ZString, ZString, IClassification> getClassificationFunc, List<ZString> keys)
		{
			var classification = GetClassification(keys, invoiceLine.JI_Tariff, MessageConstants.IdentificationTypeCodes.HS, getClassificationFunc);
			if (classification != null)
			{
				yield return classification;
			}

			var undgCode = invoiceLine.JI_HazMatCode;
			var declaration = invoiceLine.Declaration;

			var idTypeCode = ZString.Empty;
			if (declaration != null)
			{
				idTypeCode = declaration.IsSea ? MessageConstants.IdentificationTypeCodes.SSO : declaration.IsAir ? MessageConstants.IdentificationTypeCodes.ZZZ : string.Empty;
			}
			if (!idTypeCode.IsEmpty && !undgCode.IsEmpty)
			{
				classification = GetClassification(keys, undgCode, idTypeCode, getClassificationFunc);
				if (classification != null)
				{
					yield return classification;
				}
			}
		}

		static IClassification GetClassification(List<ZString> keys, ZString id, ZString identificationTypeCode, Func<ZString, ZString, IClassification> getClassificationFunc)
		{
			var key = ZString.Format("{0}_{1}", id, identificationTypeCode);
			IClassification result = null;
			if (!keys.Contains(key))
			{
				keys.Add(key);
				result = getClassificationFunc(id, identificationTypeCode);
			}
			return result;
		}

		public static IEnumerable<ZString> GetRateCodesByType(BusinessObjectFactory factory, string rateType)
		{
			return CusRefRateCodeView.Loader.LoadByRateType(factory, Core.Constants.CountryCodes.Taiwan, rateType).Select(x => x.ZY1_RateCode);
		}

		public static IEnumerable<CusEntryHeaderCharges> GetDutyOtherTaxFees(this CusEntryHeader entryHeader)
		{
			var rateCodeList = GetRateCodesByType(entryHeader.Factory, UniversalReferenceConstants.RefCusRateTypes.Duty);
			return entryHeader.Charges.Cast<CusEntryHeaderCharges>().Where(x => !x.C1_ChargeType.IsEmpty && !rateCodeList.Contains(x.C1_ChargeType));
		}

		public static IEnumerable<CusEntryLineFee> GetDutyOtherTaxFees(this CusEntryLine entryLine)
		{
			return entryLine.Fees.Cast<CusEntryLineFee>().Where(x => !(x.CF_ChargeType.IsEmpty || x.CF_ChargeType == "DTA" || x.CF_ChargeType == "DTS"));
		}

		public static ZBool DoesNotHaveCCPAndCPWNumbers(OrgAddress address)
		{
			var codesToLookFor = new string[] { OrgCusCode.CodeTypes.WarehouseControlledPremisesID, OrgCusCode.CodeTypes.ControlledPremisesID };
			var regNo = address.GetCustomsRegNo(codesToLookFor);
			return regNo.IsEmpty ? ZBool.True : ZBool.False;
		}

		public static OrgCusCode GetCustomsControlID(OrgAddress address)
		{
			return address.GetOrgCusCode(OrgHeaderHelper.CBPCodeTypes);
		}

		public static ZString GetPOCorPOADocNumber(CusEntryInstruction entryInstruction, OrgHeader orgHeader)
		{
			return GetPOCorPOADocNumber(entryInstruction, orgHeader, ZString.Empty);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need to translate")]
		public static ZString GetPOCorPOADocNumber(CusEntryInstruction entryInstruction, OrgHeader orgHeader, string bondedID)
		{
			var result = new ZStringBuilder();
			var customsOffice = entryInstruction?.CEI_CustomsOffice.SubstringSafe(0, 1) ?? ZString.Empty;
			var boxNumber = entryInstruction?.CEI_BoxNumber ?? ZString.Empty;
			var dateForDuty = entryInstruction?.CEI_DateForDuty ?? ZDateTime.Empty;
			var docTypes = new ZString[] { Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, Core.Constants.RefDocTypes.PowerOfAttorney };
			foreach (var docType in docTypes)
			{
				var jobRequiredDocument = orgHeader.RequiredDocuments.GetJobRequiredDocument(docType, customsOffice, boxNumber, dateForDuty, bondedID);
				if (jobRequiredDocument != null)
				{
					result.AppendFormat((NoResString)"常年(長期)委任報關核准文號：{0}", jobRequiredDocument.EQ_DocNumber);
					result.AppendFormat((NoResString)"起：{0}", jobRequiredDocument.EQ_DateReceived.ToZDateTime().ToTaiWanDateString());
					result.AppendFormat("迄：{0}", jobRequiredDocument.EQ_ValidToDate.ToTaiWanDateString());
					break;
				}
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		internal static IPreviousDocument GetPreviousDocument(this JobComInvoiceLine invoiceLine) => GetPreviousDocument(invoiceLine.JI_PreviousEntryNumber, invoiceLine.JI_PreviousEntryLineNumber);

		internal static IPreviousDocument GetPreBondedDocument(this JobComInvoiceLine invoiceLine) => GetPreviousDocument(invoiceLine.PreviousBondedEntryNumber, invoiceLine.PreviousBondedEntryLineNumber);

		public static IPreviousDocument GetPreviousDocument(ZString id, ZInt lineNumeric) => id.IsEmpty || lineNumeric.IsEmpty ? null : new PreviousDocumentWrapper(id, lineNumeric);

		public static ZString GetFirstLastLetterFromWord(this string word)
		{
			if (word.Length >= 1)
			{
				return FormattableString.Invariant($"{word.First()}{word.Last()}");
			}
			return ZString.Empty;
		}

		#region AdditionalInformations
		public static IEnumerable<IAdditionalInformation> GetAdditionalInformations(this IReservedFieldSupporter supporter)
		{
			var list = supporter.GetReservedFields();
			return list.Any() ? GetAdditionalInformations(list) : null;
		}

		static IEnumerable<IAdditionalInformation> GetAdditionalInformations(IEnumerable<ReservedField> reservedFields)
		{
			foreach (var reservedField in reservedFields.Take(10))
			{
				yield return new AdditionalInformationWrapper(reservedField.CY_Code, reservedField.CY_Data);
			}
		}
		#endregion

		public static IEnumerable<string> GetWords(this ZString statement)
		{
			var matches = Regex.Matches(statement, @"\b[\w']*\b");
			return matches.Cast<Match>().Where(x => !string.IsNullOrEmpty(x.Value)).Select(x => x.Value);
		}

		public static ZString GetLetterFromEnglishName(ZString englishName)
		{
			var result = ZString.Empty;
			var index = 0;
			var enumerator = englishName.GetWords().GetEnumerator();
			while (enumerator.MoveNext() && index < 3)
			{
				var currentWord = enumerator.Current.ToUpper(CultureInfo.InvariantCulture);
				result += currentWord == MessageConstants.Company ? new ZString(MessageConstants.CO) : currentWord.GetFirstLastLetterFromWord();
				index++;
			}
			return result;
		}

		public static IEnumerable<ZString> GetVehicleIDs(this JobComInvoiceLine invoiceLine) => invoiceLine.ChassisJobComInvLineRefsCollection.Select(x => x.JG_ReferenceNumber);

		public static ZString GetTypeCodeForDutyTaxFee(ZString chargeType, ZString methodOfPayment, ZBool declarationIsImport)
		{
			var isCash = methodOfPayment == EntryChargePaymentMethod.Codes.CAS;
			switch (chargeType)
			{
				case UniversalReferenceConstants.RefCusRateCodes.DTA:
				case UniversalReferenceConstants.RefCusRateCodes.DTS:
					return isCash ? DutyTaxFeeCodeList.Codes.A10 : DutyTaxFeeCodeList.Codes.A19;
				case SpecialDutyRateCodeList.Codes.CountervailingDuty:
					return isCash ? DutyTaxFeeCodeList.Codes.A20 : string.Empty;
				case SpecialDutyRateCodeList.Codes.AntiDumpingDuty:
					return isCash ? DutyTaxFeeCodeList.Codes.A30 : string.Empty;
				case SpecialDutyRateCodeList.Codes.RetaliatoryDuty:
					return isCash ? DutyTaxFeeCodeList.Codes.A40 : string.Empty;
				case SpecialDutyRateCodeList.Codes.AdditionalDuty:
					return isCash ? DutyTaxFeeCodeList.Codes.A50 : string.Empty;
				case UniversalReferenceConstants.RefCusRateCodes.TAT:
				case Constants.UniversalReferenceConstants.RefCusRateTypes.TT:
				case Constants.UniversalReferenceConstants.RefCusRateTypes.AT:
					return isCash ? DutyTaxFeeCodeList.Codes.B31 : DutyTaxFeeCodeList.Codes.B69;
				case UniversalReferenceConstants.RefCusRateCodes.CTA:
				case UniversalReferenceConstants.RefCusRateCodes.CTS:
				case Constants.UniversalReferenceConstants.RefCusRateTypes.CT:
					return isCash ? DutyTaxFeeCodeList.Codes.B10 : DutyTaxFeeCodeList.Codes.B19;
				case UniversalReferenceConstants.RefCusRateCodes.HWS:
					return isCash ? DutyTaxFeeCodeList.Codes.B32 : DutyTaxFeeCodeList.Codes.B79;
				case UniversalReferenceConstants.RefCusTaxOrFeeCodes.VAT:
					return isCash ? DutyTaxFeeCodeList.Codes.B40 : DutyTaxFeeCodeList.Codes.B49;
				case UniversalReferenceConstants.RefCusTaxOrFeeCodes.TPF:
					if (isCash)
					{
						return declarationIsImport ? DutyTaxFeeCodeList.Codes.B51 : DutyTaxFeeCodeList.Codes.B52;
					}
					else
					{
						return DutyTaxFeeCodeList.Codes.B59;
					}
				case UniversalReferenceConstants.RefCusRateCodes.SSG:
				case Constants.UniversalReferenceConstants.RefCusRateTypes.SS:
					return isCash ? DutyTaxFeeCodeList.Codes.B60 : DutyTaxFeeCodeList.Codes.B89;
				case UniversalReferenceConstants.RefCusTaxOrFeeCodes.DDF:
					return isCash ? DutyTaxFeeCodeList.Codes.C10 : string.Empty;
				default:
					return ZString.Empty;
			}
		}

		public static ZString[] GetEnglishLanguageCodes()
		{
			return new ZString[] { Core.SharedConstants.Languages.English, Core.SharedConstants.Languages.EnglishAmerican, Core.SharedConstants.Languages.EnglishBritish };
		}

		public static ZString GetSuffixLetters(ZString countryCode)
		{
			switch (countryCode)
			{
				case Core.Constants.CountryCodes.Taiwan:
					return "TWAEO-";
				case Core.Constants.CountryCodes.Singapore:
				case Core.Constants.CountryCodes.China:
					return FormattableString.Invariant($"AEO{countryCode}");
				case Core.Constants.CountryCodes.Israel:
				case Core.Constants.CountryCodes.KoreaSouth:
					return FormattableString.Invariant($"{countryCode}AEO");
				case Core.Constants.CountryCodes.Australia:
				case Core.Constants.CountryCodes.India:
					return countryCode;
				default:
					return ZString.Empty;
			}
		}
	}
}
