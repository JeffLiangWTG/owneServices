using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.RefundInfo;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	public class BaseTradeNetRefund : IRefundInfo
	{
		public BaseTradeNetRefund(InPaymentUpdatePermit permit)
		{
			this.permit = permit;
			party = permit?.Declaration?.Party;
		}

		readonly InPaymentUpdatePermit permit;
		readonly Party party;

		public ZString PermitNumber => permit?.Update?.Refund?.RefundReferenceNumber;

		public ZString ReplacementNumber => permit?.Update?.ReplacementPermitNumber;

		public ZString NameOfCompany => GlbCompany.CurrentCompany.CompanyName;

		public ZString ImporterNameLine1 => GetPartyName(party?.ImporterParty?.PartyName, 0);

		public ZString ImporterNameLine2 => GetPartyName(party?.ImporterParty?.PartyName, 1);

		public ZString ExporterNameLine1 => GetPartyName(party?.ExporterParty?.PartyDetail.PartyName, 0);

		public ZString ExporterNameLine2 => GetPartyName(party?.ExporterParty?.PartyDetail.PartyName, 1);

		public ZString EntityIdentifier
		{
			get
			{
				ZString result = party?.DeclaringAgentParty?.PartyIdentification?.ID ?? ZString.Empty;

				if (result.IsEmpty)
				{
					result = party?.ImporterParty?.PartyIdentification?.ID;
				}

				if (result.IsEmpty)
				{
					result = party?.ExporterParty?.PartyDetail?.PartyIdentification?.ID;
				}

				return result;
			}
		}

		public ZString DeclarantName => (permit?.RefundOnly?.DeclarantParty ?? party?.DeclarantParty)?.PersonInformation?.Name;

		public ZString DeclarantCode
		{
			get
			{
				ZString code = (permit?.RefundOnly?.DeclarantParty ?? party?.DeclarantParty)?.PersonInformation?.CodeValue ?? string.Empty;
				return code.Right(5).PadLeft(code.Length, 'X');
			}
		}

		public ZString TelNo => (permit?.RefundOnly?.DeclarantParty ?? party?.DeclarantParty)?.Telephone;

		public ZDate DateOfApproval => GetDate(permit?.Permit?.PermitApprovalDatetime);

		public ZString UniqueRef
		{
			get
			{
				var uniqueReferenceNumber = permit?.RefundOnly?.RefundHeader?.UniqueReferenceNumber ?? permit?.Declaration?.Header?.UniqueReferenceNumber;

				return uniqueReferenceNumber != null
				? string.Concat
				(
					(uniqueReferenceNumber.ID?.Trim() ?? string.Empty),
					" ",
					(uniqueReferenceNumber.Date?.Trim() ?? string.Empty).PadRight(8, ' '),
					" ",
					(uniqueReferenceNumber.SequenceNumeric?.Trim() ?? string.Empty).PadLeft(4, '0')
				)
				: string.Empty;
			}
		}

		public ICConditions[] ReasonForRefund
		{
			get
			{
				var updateRefund = permit.Update?.Refund;

				if (updateRefund != null)
				{
					return new[] { new TradeNetReasonForRefund(updateRefund) };
				}

				return Array.Empty<ICConditions>();
			}
		}

		public ICConditions[] RefundMessage
		{
			get
			{
				var conditions = permit.Permit?.RefundApprovalCondition;

				if (conditions != null && conditions.Any())
				{
					return TradeNetRefundMessage.GetTradeNetRefundMessages(conditions).ToArray();
				}

				return Array.Empty<ICConditions>();
			}
		}

		public IRefundInfoConsignment[] ConsignmentDetails => Array.Empty<IRefundInfoConsignment>();

		public ZDecimal TotalGoodsAndServicesTaxRefundAmount => permit?.RefundOnly?.RefundSummary?.TotalTariffRefund?.TotalGoodsAndServicesTaxRefundAmount ?? ZDecimal.Zero;

		public ZDecimal TotalExciseDutyRefundAmount => permit?.RefundOnly?.RefundSummary?.TotalTariffRefund?.TotalExciseDutyRefundAmount ?? ZDecimal.Zero;

		public ZDecimal TotalCustomsDutyRefundAmount => permit?.RefundOnly?.RefundSummary?.TotalTariffRefund?.TotalCustomsDutyRefundAmount ?? ZDecimal.Zero;

		public ZDecimal TotalOtherTaxRefundAmount => permit?.RefundOnly?.RefundSummary?.TotalTariffRefund?.TotalOtherTaxRefundAmount ?? ZDecimal.Zero;

		#region Implement

		class TradeNetRefundMessage : ICConditions
		{
			public TradeNetRefundMessage(string message)
			{
				Code = string.Empty;
				Message = message;
			}

			public static IEnumerable<TradeNetRefundMessage> GetTradeNetRefundMessages(PermitRefundApprovalCondition[] conditions)
			{
				foreach (var condition in conditions)
				{
					var pos = 73;

					ZString description = condition.ConditionDescription.Replace("\\", "/");
					yield return new TradeNetRefundMessage(FormattableString.Invariant($"<b><ExpandToFit>{((ZString)condition.ConditionCode).SubstringSafe(0, 4),-4}</b> - {description.SubstringSafe(0, pos)}"));

					while (pos < description.Length)
					{
						yield return new TradeNetRefundMessage(description.SubstringSafe(pos, 80));
						pos += 80;
					}
				}
			}

			public ZString Code { get; }

			public ZString Message { get; }
		}

		class TradeNetReasonForRefund : ICConditions
		{
			public TradeNetReasonForRefund(UpdateRefund updateRefund)
			{
				Code = updateRefund.ReasonCode.PadRight(4, ' ');
				Message = string.Join(System.Environment.NewLine, updateRefund.Reason ?? Array.Empty<string>());
			}

			public ZString Code { get; }

			public ZString Message { get; }
		}

		ZDate GetDate(ZString date)
		{
			date = date.KeepNumericCharacters();

			return string.IsNullOrWhiteSpace(date) || date.Length < 8
				? ZDate.Empty
				: new ZDate(Convert.ToInt32(date.SubstringSafe(0, 4)), Convert.ToInt32(date.SubstringSafe(4, 2)), Convert.ToInt32(date.SubstringSafe(6, 2)));
		}

		ZString GetPartyName(string[] names, int index, bool needRearrange = false)
		{
			var array = names;

			if (array != null && needRearrange)
			{
				ZString fullText = string.Concat(array);
				array = fullText.SplitIntoArray(35, 3, false);
			}

			return array != null && array.Length > index ? array[index] : string.Empty;
		}

		#endregion
	}
}
