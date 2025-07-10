using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business
{
	class CheckDeminimusValueAction : PreScreeningAction
	{
		public CheckDeminimusValueAction(HVLVPreScreeningField field, HVLVConsignment consignment) : base(field, consignment)
		{
		}

		protected override void PerformPreScreeningAndPopulateResult(HVLVConsignmentPreScreeningResult result)
		{
			var goodsValueCurrencyCode = consignment.HVC_RX_NKGoodsValueCurrency;
			var deminimusCurrencyCode = field.DeminimusCurrency;
			var deminimusCurrency = RefCurrency.LoadFromCurrencyCode(consignment.Factory, deminimusCurrencyCode);

			var checkSameConsigneeCaption = ZString.Empty;
			var checkSingleConsignmentValueCaption = ZString.Empty;
			Func<HVLVConsignment, decimal> selector = null;
			ZDecimal valueToCheck = ZDecimal.Zero;
			switch (field.FieldName)
			{
				case HVLVConsignmentSchema.Constants.HVC_GoodsValue:
					valueToCheck = consignment.HVC_GoodsValue;
					checkSameConsigneeCaption = (NoResString)"Combined Goods Value of the same consignee";
					checkSingleConsignmentValueCaption = DataBoundResourceStrings.GetDataForProperty(typeof(HVLVConsignment), field.FieldName).Caption;
					selector = hvlvConsignment => hvlvConsignment.HVC_GoodsValue;
					break;

				case HVLVPreScreeningField.Constants.TotalLineCustomsValues:
					valueToCheck = consignment.TotalLineCustomsValues;
					checkSameConsigneeCaption = (NoResString)"Combined Total Line Customs Values of the same consignee";
					checkSingleConsignmentValueCaption = DataBoundResourceStrings.GetDataForProperty(typeof(HVLVConsignment), field.FieldName).Caption;
					selector = hvlvConsignment => hvlvConsignment.TotalLineCustomsValues;
					break;

				case HVLVPreScreeningField.Constants.TotalLineIntrinsicValues:
					valueToCheck = consignment.TotalLineIntrinsicValues;
					checkSameConsigneeCaption = (NoResString)"Combined Total Line Intrinsic Values of the same consignee";
					checkSingleConsignmentValueCaption = (NoResString)"Total Line Intrinsic Values";
					selector = hvlvConsignment => hvlvConsignment.TotalLineIntrinsicValues;
					break;
			}

			var checkSingleConsignmentValueIsSuccess = true;

			if (!goodsValueCurrencyCode.IsEmpty)
			{
				if (deminimusCurrencyCode != goodsValueCurrencyCode)
				{
					valueToCheck = ConvertCurrency(valueToCheck, goodsValueCurrencyCode, deminimusCurrency);
				}

				checkSingleConsignmentValueIsSuccess = CheckSingleConsignmentValue(valueToCheck, result, checkSingleConsignmentValueCaption);
			}

			if (checkSingleConsignmentValueIsSuccess && field.CheckSameConsignee)
			{
				var otherConsignmentsWithSameConsignee = consignment.ConsignmentsBelongToSameConsigneeExcludingParent.OfType<HVLVConsignment>().ToList();

				if (otherConsignmentsWithSameConsignee.Any())
				{
					CheckConsignmentsValuesWithSameConsinee(selector, otherConsignmentsWithSameConsignee, deminimusCurrency, result, checkSameConsigneeCaption);
				}
			}
		}

		bool CheckSingleConsignmentValue(decimal value, HVLVConsignmentPreScreeningResult result, string caption)
		{
			if (value == 0)
			{
				var preScreeningDetails = string.Format(CultureInfo.CurrentCulture, "{0} - {1} {2}", ScreeningError, $"{caption} {DeminimusNotificationMessageChecked}.", EmptyValueError);
				AddPreScreeningDetailsCore(preScreeningDetails, result);
				return false;
			}
			else if (value > field.DeminimusValue)
			{
				AddDeminimusFieldPreScreeningDetails(result, caption);
				return false;
			}

			return true;
		}

		void CheckConsignmentsValuesWithSameConsinee(Func<HVLVConsignment, decimal> selector, IEnumerable<HVLVConsignment> otherConsignmentsWithSameConsignee, RefCurrency deminimusCurrency, HVLVConsignmentPreScreeningResult result, string caption)
		{
			var sum = otherConsignmentsWithSameConsignee.Sum(c => ConvertCurrency(selector(c), c.HVC_RX_NKGoodsValueCurrency, deminimusCurrency));
			if (!consignment.HVC_RX_NKGoodsValueCurrency.IsEmpty)
			{
				sum += ConvertCurrency(selector(consignment), consignment.HVC_RX_NKGoodsValueCurrency, deminimusCurrency);
			}

			if (sum > field.DeminimusValue)
			{
				AddDeminimusFieldPreScreeningDetails(result, caption, GetConsignmentIDsMessage(consignment.ConsignmentsBelongToSameConsignee));
			}
		}

		void AddDeminimusFieldPreScreeningDetails(HVLVConsignmentPreScreeningResult result, ZString fieldDescription, string consignmentIds = "")
		{
			var preScreeningDetails = string.Format(CultureInfo.CurrentCulture, "{0} - {1} {2} {3}",
				ScreeningError,
				field.MessageText.IsEmpty ? string.Format("{0} {1}.", fieldDescription, DeminimusNotificationMessageFailed) : string.Format("{0} {1}.", fieldDescription, DeminimusNotificationMessageChecked),
				consignmentIds,
				field.MessageText);

			AddPreScreeningDetailsCore(preScreeningDetails, result);
		}
		ZDecimal ConvertCurrency(ZDecimal value, ZString fromCurrency, RefCurrency toCurrency)
		{
			if (value == 0 || fromCurrency.IsEmpty)
			{
				return 0m;
			}

			return RefCurrency.LoadFromCurrencyCode(consignment.Factory, fromCurrency).ConvertUsingCustomsRate(ZDateTime.Today, value, toCurrency);
		}

		ZString GetConsignmentIDsMessage(HVLVCommonConsigneeConsignmentCollection consignmentsWithSameConsignee)
		{
			var consignmentIds = consignmentsWithSameConsignee.Select(c => c.HVC_ConsignmentId).ToArray();
			var consignmentIdsMessage = ZString.Join(", ", consignmentIds);

			return Res.GetString("3b4593f1-b163-48d6-bd46-bd07d6e79d1b", "Consignment IDs: {0}.", consignmentIdsMessage);
		}

		string EmptyValueError => Res.GetString("f06061bd-6ae3-4e34-a810-1bacd129f024", "Please enter a value");
		string DeminimusNotificationMessageFailed => Res.GetString("e2bbcd9e-88e6-4549-99a8-03a52499c377", "has failed Pre-Screening");
		string DeminimusNotificationMessageChecked => Res.GetString("8eec435e-d3c5-4176-8ad4-4b31d1702399", "checked");
	}
}
