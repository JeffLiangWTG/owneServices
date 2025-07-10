using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using ECB = Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class ProvisionalPaymentCusEntryPayInfo : CusEntryPayInfo
	{
		public ProvisionalPaymentCusEntryPayInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public new class Schema : CusEntryPayInfo.Schema
		{
			public const string C9_PaymentStatusDescription = "C9_PaymentStatusDescription";
		}

		#region Lookups

		public new ProvisionalPaymentCusEntryPayInfoLookups Lookups => (ProvisionalPaymentCusEntryPayInfoLookups)base.Lookups;

		protected override ECB.CusEntryPayInfoLookups GetNewLookups()
		{
			return new ProvisionalPaymentCusEntryPayInfoLookups(this);
		}

		#endregion

		#region Override Properties

		[ResourceStringData("297C9770-1455-4D9F-A923-A0A180B2BC5C", Caption = "Status")]
		[List(nameof(Lookups) + "." + nameof(ProvisionalPaymentCusEntryPayInfoLookups.ProvisionalPaymentStatuses))]
		public override ZString C9_PaymentStatus
		{
			get { return base.C9_PaymentStatus; }
			set { base.C9_PaymentStatus = value; }
		}

		[ResourceStringData("6A0C53C5-5EF4-4F9E-A17C-3830C395C661", Caption = "Request Liquidation")]
		public override ZBool C9_RemAdvReceived
		{
			get { return base.C9_RemAdvReceived; }
			set
			{
				var oldValue = C9_RemAdvReceived;
				base.C9_RemAdvReceived = value;

				if (!IsCopying && !oldValue && value)
				{
					UpdateProvisionalPaymentsUnderEntryLine();
				}

				if (!SetterSuspender.IsSetterSuspended(CusEntryPayInfo.Schema.C9_RemAdvReceived))
				{
					PromptUserforLiquidation();
				}
			}
		}

		[ResourceStringData("BB22A69A-55C8-4C3A-9BB4-4A1F8C114B5F", Caption = "Liquidation Date")]
		public override ZDate C9_ReceiptDate
		{
			get => base.C9_ReceiptDate;
			set
			{
				var oldValue = C9_ReceiptDate;
				base.C9_ReceiptDate = value;

				if (!IsCopying && oldValue != C9_ReceiptDate && !C9_ReceiptDate.IsEmpty)
				{
					RecalculateAmountExtend.RecalculateAmountAndAddEvent(this);
				}
			}
		}

		#endregion

		#region New Properties

		[ResourceStringData("746FE3EC-7C86-4C4F-8B50-E0DA5574129E", Caption = "Description")]
		public ZString C9_PaymentStatusDescription
		{
			get { return Lookups.ProvisionalPaymentStatuses.GetDescriptionFromCode(C9_PaymentStatus); }
		}

		public ZPropertyInfo C9_PaymentStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.C9_PaymentStatusDescription); }
		}

		#endregion

		void UpdateProvisionalPaymentsUnderEntryLine()
		{
			if (Declaration != null)
			{
				var entryHeader = EntryHeader;
				if (entryHeader != null)
				{
					entryHeader.CH_PaymentMethod = PaymentMethodCodeList.Codes.Free;

					var entryline = entryHeader.MergedLines.Cast<CusEntryLine>().SingleOrDefault(x => x.CL_LineNumber.ToString() == C9_IncomingPayResponseNo);
					if (entryline != null)
					{
						var payments = entryline.ProvisionalPayments.Cast<ProvisionalPaymentAmountCodeData>().Where(x => x.CY_Code == C9_TransactionType);
						if (payments.Any())
						{
							var paymentList = payments.ToList();
							paymentList.DeleteAll();
						}
					}
				}
			}
		}

		internal static class RecalculateAmountExtend
		{
			internal static void RecalculateAmountAndAddEvent(ProvisionalPaymentCusEntryPayInfo payInfo)
			{
				if (payInfo.Declaration != null)
				{
					var entryHeader = payInfo.EntryHeader;
					if (entryHeader != null)
					{
						var amountBefore = ZDecimal.Zero;
						switch (payInfo.C9_TransactionType)
						{
							case LineLevelProvisionalPayments.Codes.PEN:
								entryHeader.PenaltyAmountBefore -= payInfo.C9_PaymentAmount;
								if (entryHeader.PenaltyAmountBefore < ZDecimal.Zero)
								{
									entryHeader.PenaltyAmountBefore = ZDecimal.Zero;
								}

								amountBefore = entryHeader.PenaltyAmountBefore;
								break;
							default:
								entryHeader.ProvisionalPaymentAmountBefore -= payInfo.C9_PaymentAmount;
								if (entryHeader.ProvisionalPaymentAmountBefore < ZDecimal.Zero)
								{
									entryHeader.ProvisionalPaymentAmountBefore = ZDecimal.Zero;
								}

								amountBefore = entryHeader.ProvisionalPaymentAmountBefore;
								break;
						}

						entryHeader.Logs.AddNew(Events.StatusUpdated, $"Entry {entryHeader.CH_BGMReference} updated VOC-before data for {payInfo.C9_TransactionType}, reduced by {payInfo.C9_PaymentAmount} to {amountBefore} because liquidation date set to {payInfo.C9_ReceiptDate}");
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "Baseline")]
		void PromptUserforLiquidation()
		{
			var entryInstruction = EntryHeader?.EntryInstruction;

			if (entryInstruction != null)
			{
				var provisionalPayInfosWithSameEPPNum = entryInstruction.ProvisionalPaymentPayInfos.Where(x => x.C9_PaymentReference == C9_PaymentReference);
				int count = provisionalPayInfosWithSameEPPNum.Count();

				if (count > 1)
				{
					var tickAll = Globals.Message.Show(ZString.Format(liquidateAllMessage, TickOrUntickLiquidation, count, C9_PaymentReference)
										, "Request Liquidation?"
										, ZMessageBoxButtons.YesNo
										, ZDialogResult.No) == ZDialogResult.Yes;

					if (tickAll)
					{
						foreach (var payInfo in provisionalPayInfosWithSameEPPNum.Where(x => x.PK != PK))
						{
							using (payInfo.SetterSuspender.SuspendSetting(CusEntryPayInfo.Schema.C9_RemAdvReceived))
							{
								payInfo.C9_RemAdvReceived = C9_RemAdvReceived;
							}
						}
					}
				}
			}
		}

		ZString TickOrUntickLiquidation => C9_RemAdvReceived ? "Request" : "Unrequest";

		const string liquidateAllMessage = "Do you wish to {0} Liquidation on all lines with the same epp No?\r\nThere are {1} lines with the same epp No: {2}";

		public SetterSuspender SetterSuspender => setterSuspender ?? (setterSuspender = new SetterSuspender());

		SetterSuspender setterSuspender;
	}
}
