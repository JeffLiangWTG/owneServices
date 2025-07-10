using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D96B.Elements;
using Enterprise.Edifact.D96B.Messages.CUSDEC;
using Enterprise.Edifact.D96B.Segments;
using Enterprise.Edifact.Utilities;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public static partial class CUSDECMessageTextBuilder
	{
		static void AddNewSG30Group(SegmentGroup30MessageSection group30Section, ILineLevelInformation source)
		{
			var target = group30Section.InstantiateAChildAndAddItToChildrenCollection();
			AddCSTSegmentOnSG30(target.CST[0], source);
			AddFTXSegmentForGoodsDescription(target, source.GoodsDescription, TextSubjectQualifierList.GoodsDescription, 70);
			AddFTXSegmentForAdditionalInfomation(source.Factory, target, source.AdditionalInformations, source.DateOfAssessment);
			AddFTXSegmentForCustomsProcedure(target, source);
			AddNewLOCSegment(target.LOC, source.CountryOfOrigin, PlaceLocationQualifierList.CountryOfOrigin);
			AddNewMEASegment(target.MEA, source.CustomsQuantity, source.CustomsUnitQty, MeasurementApplicationQualifierList._1stSpecifiedTariffQuantity);
			AddNewMEASegment(target.MEA, source.AdditionalQuantity, source.AdditionalUnitQty, MeasurementApplicationQualifierList._2ndSpecifiedTariffQuantity, alwaysSendEvenIfValueEmpty: true);
			AddNewMEASegment(target.MEA, source.ClassificationQuantity, source.ClassificationUnitQty, MeasurementApplicationQualifierList._3rdSpecifiedTariffQuantity);
			AddNewMEASegment(target.MEA, source.WarehouseCountableQuantity, source.WarehouseCountableUnitQty, MeasurementApplicationQualifierList.CustomsLineItemMeasurement, 0);
			var rebateUser = source.RebateUserCode;
			if (!rebateUser.IsEmpty)
			{
				AddNewNADSegmentWithCodeOnly(target.NAD, PartyQualifierList.WarehouseKeeper, rebateUser, 8); //NAD+WH
			}
			PopulateSG33Groups(target.Group33, source);
			PopulateSG35Groups(target.Group35, source);
			PopulateSG41Groups(target.Group41, source);
		}

		static void AddCSTSegmentOnSG30(CSTSegment cstSegment, ILineLevelInformation source)
		{
			cstSegment.GoodsItemNumber = source.LineNumber;
			cstSegment.CustomsIdentityCodes1.CustomsCodeIdentification = source.TariffCode;
			cstSegment.CustomsIdentityCodes1.CodeListQualifier = CodeListQualifierList.TariffSchedule;
			cstSegment.CustomsIdentityCodes1.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.MutuallyDefined;
			cstSegment.CustomsIdentityCodes2.CustomsCodeIdentification = source.PreferenceCode;
		}

		static void AddFTXSegmentForGoodsDescription(SegmentGroup30 sg30, ZString textValue, TextSubjectQualifierList textType, int fieldLimit)
		{
			if (textType != null && !textValue.IsEmpty)
			{
				var ftxSegment = sg30.FTX.InstantiateAChildAndAddItToChildrenCollection();
				var splitter = new TextSplitter(fieldLimit);
				splitter.Text = textValue;
				ftxSegment.TextSubjectQualifier = textType;
				ftxSegment.TextLiteral.FreeText1 = splitter[0];
				ftxSegment.TextLiteral.FreeText2 = splitter[1];
				ftxSegment.TextLiteral.FreeText3 = splitter[2];
				ftxSegment.TextLiteral.FreeText4 = splitter[3];
				ftxSegment.TextLiteral.FreeText5 = splitter[4];
			}
		}

		static void AddFTXSegmentForAdditionalInfomation(BusinessObjectFactory factory, SegmentGroup30 sg30, IEnumerable<IAdditionalInformation> additionalInformations, ZDateTime dateOfAssessment)
		{
			additionalInformations = ReOrderAdditionalInformationCodesIfNecessary(additionalInformations);
			var items = additionalInformations.Where(x => !x.Code.IsEmpty || !x.Value.IsEmpty).ToList();
			if (items.Count == 0)
			{
				return;
			}

			var rooList = ZARefCusCodeListTypes.GetAddInWithROOTypeAttribute(factory, dateOfAssessment);
			var amount = ZARefCusCodeListTypes.GetAddInWithAmountAttribute(factory, dateOfAssessment);
			var ftxSegment = sg30.FTX.InstantiateAChildAndAddItToChildrenCollection();
			ftxSegment.TextSubjectQualifier = TextSubjectQualifierList.AdditionalInformation;
			for (var i = 0; i < Math.Min(items.Count, 10); i++)
			{
				if (i == 9 && items[i].Group != null && items[i].Code == PermitTypeList.Codes.RCC)
				{
					return;
				}

				if (i == 5)
				{
					ftxSegment = sg30.FTX.InstantiateAChildAndAddItToChildrenCollection();
					ftxSegment.TextSubjectQualifier = TextSubjectQualifierList.GeneralInformation;
				}

				var outputCode = items[i].Code;
				if (rooList.ContainsCode(items[i].Code))
				{
					outputCode = outputCode.PadRight(3);
				}

				string value;
				if (amount.ContainsCode(items[i].Code))
				{
					value = string.Concat(outputCode, items[i].Value.PadLeft(32, '0'));
				}
				else
				{
					var padWithValue = ZARefCusCodeListTypes.GetAdditionalInformationAttributeValuesFor(factory, dateOfAssessment, items[i].Code, Universal.RefCusCodeListAttributeTypes.Codes.PadWith)?.FirstOrDefault();
					if (padWithValue.HasValue && !padWithValue.Value.IsEmpty)
					{
						value = string.Concat(outputCode, items[i].Value.PadLeft(32, padWithValue.ToString().ToCharArray()[0]));
					}
					else
					{
						value = string.Concat(outputCode, items[i].Value);
					}
				}

				var refPosition = i % 5;
				switch (refPosition)
				{
					case 0:
						ftxSegment.TextLiteral.FreeText1 = value;
						break;
					case 1:
						ftxSegment.TextLiteral.FreeText2 = value;
						break;
					case 2:
						ftxSegment.TextLiteral.FreeText3 = value;
						break;
					case 3:
						ftxSegment.TextLiteral.FreeText4 = value;
						break;
					case 4:
						ftxSegment.TextLiteral.FreeText5 = value;
						break;
				}
			}
		}

		static IEnumerable<IAdditionalInformation> ReOrderAdditionalInformationCodesIfNecessary(IEnumerable<IAdditionalInformation> additionalInformations)
		{
			IEnumerable<IAdditionalInformation> returnValue = additionalInformations;
			if (additionalInformations != null)
			{
				Predicate<IAdditionalInformation> predicateKBC = x => x.Code == UniversalReferenceConstants.AdditionalInformation.KimberleyCertificate;
				Predicate<IAdditionalInformation> predicateDLV = x => x.Code == UniversalReferenceConstants.AdditionalInformation.DiamondLevyValue;
				Predicate<IAdditionalInformation> predicateNullGroup = x => x.Group == null;
				var inputList = additionalInformations.ToList();
				var list = new List<IAdditionalInformation>();
				list.AddRange(inputList.FindAll(predicateKBC));
				list.AddRange(inputList.FindAll(predicateDLV));
				inputList.RemoveAll(predicateKBC);
				inputList.RemoveAll(predicateDLV);
				list.AddRange(inputList.FindAll(predicateNullGroup));
				inputList.RemoveAll(predicateNullGroup);
				list.AddRange(inputList);
				returnValue = list;
			}
			return returnValue;
		}

		static void AddFTXSegmentForCustomsProcedure(SegmentGroup30 sg30, ILineLevelInformation source)
		{
			var cpc = source.CustomsProcedureCode;
			var ppc = source.PreviousProcedureCode;
			var procedureMeasure = source.ProcedureMeasure;
			var tradeStatisticsCode = source.TradeStatisticsIndicator;
			if (!(cpc.IsEmpty && ppc.IsEmpty && procedureMeasure.IsEmpty && tradeStatisticsCode.IsEmpty))
			{
				var fTX = sg30.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectQualifier = TextSubjectQualifierList.CustomsClearanceInstructions;
				fTX.TextLiteral.FreeText1 = cpc;
				fTX.TextLiteral.FreeText2 = ppc;
				fTX.TextLiteral.FreeText3 = procedureMeasure;
				fTX.TextLiteral.FreeText5 = tradeStatisticsCode;
			}
		}

		#region SG33 Population

		static void PopulateSG33Groups(SegmentGroup33MessageSection group33Section, ILineLevelInformation source)
		{
			if (!source.ActualPrice.IsEmpty)
			{
				AddNewSG33Group(group33Section, source.ActualPrice, MonetaryAmountTypeQualifierList.InvoiceItemAmount);
			}
			AddNewSG33Group(group33Section, source.CustomsValue, MonetaryAmountTypeQualifierList.CustomsValue);
		}

		static void AddNewSG33Group(SegmentGroup33MessageSection group33Section, ZDecimal moneyValue, MonetaryAmountTypeQualifierList moneyType)
		{
			var sg33Segment = group33Section.InstantiateAChildAndAddItToChildrenCollection();
			AddNewMOASegment(sg33Segment.MOA, moneyValue, moneyType, string.Empty, 0);
		}

		static void AddNewMOASegment(MOASegmentMessageSection moaSection, ZDecimal moneyValue, MonetaryAmountTypeQualifierList moneyType, string currencyCode = "", int decimalPlace = 2)
		{
			var moaSegment = moaSection.InstantiateAChildAndAddItToChildrenCollection();
			moaSegment.MonetaryAmount.MonetaryAmountTypeQualifier = moneyType;
			moaSegment.MonetaryAmount.MonetaryAmount = moneyValue.ToString(decimalPlace);
			if (!string.IsNullOrEmpty(currencyCode))
			{
				moaSegment.MonetaryAmount.CurrencyCoded = currencyCode;
			}
		}

		#endregion

		#region SG35 Population
		static void PopulateSG35Groups(SegmentGroup35MessageSection group35Section, ILineLevelInformation source)
		{
			if (!source.PreviousProcedureMRN.IsEmpty)
			{
				var sg35Segment = group35Section.InstantiateAChildAndAddItToChildrenCollection();
				var rffSegment = AddNewRFFSegment(sg35Segment.RFF, ReferenceQualifierList.WarehouseEntryNumber, source.PreviousProcedureMRN);
				var warehousingMRNLineNumber = source.WarehousingMRNLineNumber;
				if (rffSegment != null && !warehousingMRNLineNumber.IsEmpty)
				{
					rffSegment.Reference.LineNumber = warehousingMRNLineNumber;
				}
			}
		}

		#endregion

		#region SG41 Population

		static void PopulateSG41Groups(SegmentGroup41MessageSection group41Section, ILineLevelInformation source)
		{
			foreach (var dutyFeeInformation in source.DutiesAndFees)
			{
				if (!dutyFeeInformation.Code.IsEmpty && dutyFeeInformation.Value.IsValid)
				{
					AddNewSG41Group(group41Section, DutyTaxFeeTypeCodedList.GetFromString(dutyFeeInformation.Code), dutyFeeInformation.Value);
				}
			}
			foreach (var provisionalPayment in source.ProvisionalPayments)
			{
				if (!provisionalPayment.Code.IsEmpty && provisionalPayment.Value.IsValid)
				{
					AddNewSG41Group(group41Section, DutyTaxFeeTypeCodedList.GetFromString(provisionalPayment.Code), provisionalPayment.Value);
				}
			}
		}

		static void AddNewSG41Group(SegmentGroup41MessageSection group41Section, DutyTaxFeeTypeCodedList tariffType, ZDecimal value)
		{
			if (tariffType != null && !value.IsEmpty)
			{
				var sG41 = group41Section.InstantiateAChildAndAddItToChildrenCollection();
				AddNewTAXSegment(sG41.TAX, tariffType, DutyTaxFeeFunctionQualifierList.IndividualDutyTaxOrFeeCustomsItem);
				AddNewMOASegment(sG41.MOA, value, MonetaryAmountTypeQualifierList.DutyTaxOrFeeAmount);
			}
		}

		static void AddNewTAXSegment(TAXSegmentMessageSection taxSection, DutyTaxFeeTypeCodedList tariffType, DutyTaxFeeFunctionQualifierList taxClassification)
		{
			var taxSegment = taxSection.InstantiateAChildAndAddItToChildrenCollection();
			taxSegment.DutyTaxFeeFunctionQualifier = taxClassification;
			taxSegment.DutyTaxFeeType.DutyTaxFeeTypeCoded = tariffType;
			taxSegment.DutyTaxFeeType.CodeListQualifier = CodeListQualifierList.ExciseDuty;
			taxSegment.DutyTaxFeeType.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.MutuallyDefined;
		}

		#endregion

	}
}
