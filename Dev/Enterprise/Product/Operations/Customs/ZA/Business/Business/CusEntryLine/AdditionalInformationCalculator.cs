using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.ZA;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public static class AdditionalInformationCalculator
	{
		public static void Calculate(CusEntryLine entryLine)
		{
			if (entryLine != null)
			{
				var factory = entryLine.Factory;
				var header = entryLine.Header;
				var declaration = header?.Declaration;
				var isExport = declaration?.IsExport ?? ZBool.False;
				var isLine1 = entryLine.IsLine1;
				var assessmentDate = entryLine.DateOfAssessment;
				var additionalInformationList = ZARefCusCodeListTypes.GetAdditionalInformationList(factory, assessmentDate, isExport, isLine1);
				var collection = entryLine.AdditionalInformationCodes;
				collection.Load();
				var existingAddInfos = collection.Cast<AdditionalInformation>().ToList();
				var invoiceLine = entryLine.RandomLine;
				if (invoiceLine != null)
				{
					foreach (var tariffDetail in invoiceLine.CusLineTariffDetails.Cast<CusLineTariffDetail>().Where(x => !x.BZ_Type.IsEmpty))
					{
						var tariffType = tariffDetail.BZ_Type;
						var tariffWithCheckDigit = tariffDetail.UniversalTariff?.GetTariffCodeWithCheckDigit() ?? ZString.Empty;
						foreach (var scheduleMappedToAdditionalInformation in ZARefCusCodeListTypes.GetAdditionalInformationsMappedToSchedule(factory, assessmentDate, tariffType))
						{
							AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, scheduleMappedToAdditionalInformation, () => tariffWithCheckDigit, !string.IsNullOrEmpty(scheduleMappedToAdditionalInformation));
						}
					}
					var primaryPreference = invoiceLine.JI_PrimaryPreference;
					var rooCert = invoiceLine.JI_ROOCert;
					if (isExport)
					{
						AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, primaryPreference, rooCert, !primaryPreference.IsEmpty && invoiceLine.Lookups.ROOTypeOrPreferenceList.ContainsCode(primaryPreference));
					}
					else if (!invoiceLine.HasIntoWarehouseProcedure)
					{
						AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, UniversalReferenceConstants.AdditionalInformation.RulesOfOrigin, rooCert, !primaryPreference.IsEmpty && !invoiceLine.IsStandardTradeAgreement || !rooCert.IsEmpty);
					}
					var vin = invoiceLine.JI_VIN;
					AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, UniversalReferenceConstants.AdditionalInformation.VehicleIdentificationNumber, vin, !vin.IsEmpty);
					var newUsed = invoiceLine.JI_NewUsed;
					AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, UniversalReferenceConstants.AdditionalInformation.NewUsedIndicator, newUsed, !newUsed.IsEmpty);
					if (invoiceLine.IsDiamondProcessingRequired)
					{
						AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, UniversalReferenceConstants.AdditionalInformation.DiamondBeneficiaryLicense, invoiceLine.JI_DiamondBeneficiaryLicense, !invoiceLine.JI_DiamondBeneficiaryLicense.IsEmpty);
						AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, UniversalReferenceConstants.AdditionalInformation.DiamondDealerLicense, invoiceLine.JI_DiamondDealerLicense, !invoiceLine.JI_DiamondDealerLicense.IsEmpty);
						AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, UniversalReferenceConstants.AdditionalInformation.DiamondLevyValue, invoiceLine.JI_DiamondLevyValue.ToString(), invoiceLine.JI_DiamondLevyValue != 0);
						AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, UniversalReferenceConstants.AdditionalInformation.DiamondProducerRegistration, invoiceLine.JI_DiamondProducerRegistration, !invoiceLine.JI_DiamondProducerRegistration.IsEmpty);
						AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, UniversalReferenceConstants.AdditionalInformation.DiamondProducerExemption, invoiceLine.JI_DiamondProducerExemption, !invoiceLine.JI_DiamondProducerExemption.IsEmpty);
						AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, UniversalReferenceConstants.AdditionalInformation.ElectionsExemptionsLevy, invoiceLine.JI_ElectionsExemptionsLevy, !invoiceLine.JI_ElectionsExemptionsLevy.IsEmpty);
						AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, UniversalReferenceConstants.AdditionalInformation.KimberleyCertificate, invoiceLine.JI_KimberleyCertificate, !invoiceLine.JI_KimberleyCertificate.IsEmpty);
						AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, UniversalReferenceConstants.AdditionalInformation.TemporaryBuyersPermit, invoiceLine.JI_TemporaryBuyersPermit, !invoiceLine.JI_TemporaryBuyersPermit.IsEmpty);
						AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, UniversalReferenceConstants.AdditionalInformation.TemporaryExportExemption, invoiceLine.JI_TemporaryExportExemption, !invoiceLine.JI_TemporaryExportExemption.IsEmpty);
					}
					AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, UniversalReferenceConstants.AdditionalInformation.VATTaxExemptions, string.Empty, invoiceLine.JI_ZZF_NKTaxType == UniversalReferenceConstants.TaxOrFeeTypeCode.VEX);
					if (declaration != null)
					{
						if (declaration.JE_RemovalTransportCode == Enterprise.Core.Constants.TransportModes.Road)
						{
							var entryInstruction = invoiceLine.EntryInstruction;
							if (entryInstruction != null)
							{
								var bondHolder = entryInstruction.BondHolder;
								AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, UniversalReferenceConstants.AdditionalInformation.BondHolder, () => bondHolder.BondHolderLocalCustomsCarrierCode, bondHolder != null && entryLine.IsLine1);
								var bondAmount = collection[UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount]?.CY_Data ?? ZString.Empty;
								AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount, bondAmount);
								var suretyAmount = entryInstruction.CEI_ProvisionalPaymentSuretyAmount;
								if (!suretyAmount.IsEmpty && isLine1)
								{
									AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, UniversalReferenceConstants.AdditionalInformation.ProvisionalPaymentSurety, suretyAmount.ToString());
								}
							}
						}

						var factor = CusEntryLine.CreateMessageKeyFactor(declaration, invoiceLine);
						if (factor.IsOrdinaryLevyItem())
						{
							AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, UniversalReferenceConstants.AdditionalInformation.OrdinaryLevyItem, GetOLIItemNumber(entryLine));
						}
						var permit = invoiceLine.JI_PermitNumber;
						if (!permit.IsEmpty)
						{
							var code = isExport ? UniversalReferenceConstants.AdditionalInformation.ExportPermitControl : UniversalReferenceConstants.AdditionalInformation.ImportPermitControl;
							AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, code, permit);
						}
					}
					var invoice = invoiceLine.InvoiceHeader;
					if (invoice != null)
					{
						var valueDeterminationNumber = invoice.JZ_VDN;
						AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, UniversalReferenceConstants.AdditionalInformation.ValueDeterminationNumber, valueDeterminationNumber, !valueDeterminationNumber.IsEmpty);
					}

					AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, UniversalReferenceConstants.AdditionalInformation.AdvancePaymentNo, invoiceLine.JI_AdvancePaymentNo, !invoiceLine.JI_AdvancePaymentNo.IsEmpty);

					CalculateRCCCertificates(invoiceLine, entryLine, collection);
					ZShort index = 1;
					entryLine.AdditionalInformationCodesActions?.ToList().ForEach(x => x.Invoke(index++));
				}

				existingAddInfos.DeleteAll();
				collection.Sort(AdditionalInformation.Schema.CY_Code);
			}
		}

		public static void ResetRCCCertificateValues(JobDeclaration declaration)
		{
			foreach (CusEntryInstruction entryInstruction in declaration.CustomsEntryInstructions)
			{
				foreach (RCCCertificate rccCertificate in entryInstruction.RCCCertificates)
				{
					rccCertificate.ValueUsedByThisDeclaration = ZDecimal.Zero;
				}
			}
		}

		static void CalculateRCCCertificates(JobComInvoiceLine invoiceLine, CusEntryLine entryLine, AdditionalInformationCollection collection)
		{
			var prccTariff = invoiceLine.CusLineTariffDetails.OfType<CusLineTariffDetail>().FirstOrDefault(x => x.UniversalTariff?.HasAttribute(UniversalReferenceConstants.TariffAttributes.PRCC) ?? false);
			if (prccTariff != null)
			{
				prccTariff.ClearRowNotificationsContaining(ValidationConstants.CusLineTariffDetail.RCCCertificates);

				var remainingCustomsValue = entryLine.CustomsValue.Amount;
				var isSpecifiedMotorVehicle = entryLine.IsSpecifiedMotorVehicle;

				short permitCount = 0;
				var validOrderedRCCCertificates = entryLine.EntryInstruction.RCCCertificates.OfType<RCCCertificate>()
					.Where(x => x.PermitHeader != null && x.PermitType != PermitTypeList.Codes.VALA)
					.Distinct(new RCCCertificate.EqualityComparer())
					.OrderBy(x => x.CY_Order);
				foreach (var rccCertificate in validOrderedRCCCertificates)
				{
					if (remainingCustomsValue == ZDecimal.Zero)
					{
						break;
					}

					var valueBalance = rccCertificate.RemainingValueExcludingThisDeclaration - invoiceLine.Declaration?.CustomsEntryInstructions.OfType<CusEntryInstruction>()
						.Sum(instr => instr.RCCCertificates.OfType<RCCCertificate>().Where(rcc => rcc.CY_Code == rccCertificate.CY_Code).Sum(rcc => rcc.ValueUsedByThisDeclaration)) ?? ZDecimal.Zero;
					if (valueBalance == ZDecimal.Zero)
					{
						continue;
					}

					var factor = 1m;
					if (isSpecifiedMotorVehicle && rccCertificate.PermitHeader.CPH_SubType != PermitSubTypeList.Codes.LVE)
					{
						factor = 1.25m;
					}

					var customsValueForPermit = new ZDecimal(factor * remainingCustomsValue).RoundDownIncludingToZeroUsingCustomsValueRule();
					if (customsValueForPermit <= valueBalance)
					{
						remainingCustomsValue = ZDecimal.Zero;
					}
					else
					{
						customsValueForPermit = new ZDecimal(valueBalance).RoundDownIncludingToZeroUsingCustomsValueRule();
						remainingCustomsValue = new ZDecimal(remainingCustomsValue - (customsValueForPermit / factor));
					}

					permitCount++;
					var rcvAddInfo = collection.AddNew();
					rcvAddInfo.CY_Code = CusCodeDataTypeList.Codes.RCV;
					rcvAddInfo.CY_Data = customsValueForPermit.ToString("F0", System.Globalization.CultureInfo.InvariantCulture);
					rcvAddInfo.CY_Order = permitCount;

					var rccAddInfo = collection.AddNew();
					rccAddInfo.CY_Code = CusCodeDataTypeList.Codes.RCC;
					rccAddInfo.CY_Data = rccCertificate.CY_Code;
					rccAddInfo.CY_Order = permitCount;

					rccCertificate.ValueUsedByThisDeclaration += customsValueForPermit;
				}

				if (remainingCustomsValue > 0)
				{
					prccTariff.AddRowMessageError(ValidationConstants.CusLineTariffDetail.InsufficientPermitValue);
				}
				else if (permitCount > 2)
				{
					prccTariff.AddRowMessageError(ValidationConstants.CusLineTariffDetail.MoreThanTwoRCCCertificates(entryLine.CL_LineNumber.ToString()));
				}

				prccTariff.RefreshBinding();
				prccTariff.InvoiceLine.RefreshBinding();
			}
		}

		#region Implementation

		static void AddAdditionalInformationOrUpdate(CodeDescriptionPairList additionalInformationList, List<AdditionalInformation> existingAddInfos, AdditionalInformationCollection collection, ZString code, ZString data, bool additionalCondition = true)
		{
			AddAdditionalInformationOrUpdate(additionalInformationList, existingAddInfos, collection, code, () => data, additionalCondition);
		}

		static void AddAdditionalInformationOrUpdate(CodeDescriptionPairList additionalInformationList, List<AdditionalInformation> existingAddInfos, AdditionalInformationCollection collection, ZString code, Func<ZString> getData, bool additionalCondition = true)
		{
			if (additionalCondition && additionalInformationList.ContainsCode(code))
			{
				var addInfo = existingAddInfos.FirstOrDefault(x => x.CY_Code == code);
				if (addInfo == null)
				{
					addInfo = collection.AddNew(code);
				}
				else
				{
					existingAddInfos.Remove(addInfo);
				}
				addInfo.CY_Data = getData().Left(addInfo.CY_DataInfo.MaxLength);
			}
		}

		static string GetOLIItemNumber(CusEntryLine entryLine)
		{
			return entryLine.CL_AdValoremTariff.Left(4) == UniversalReferenceConstants.TariffHeading.OrdinaryLevyItem ? UniversalReferenceConstants.OrdinaryLevyItem.Code_19620 : UniversalReferenceConstants.OrdinaryLevyItem.Code_19610;
		}

		#endregion

	}
}
