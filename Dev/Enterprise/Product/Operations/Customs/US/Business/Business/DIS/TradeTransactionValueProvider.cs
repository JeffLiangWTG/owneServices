using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.Business.DIS
{
	class TradeTransactionsValueProvider
	{
		public TradeTransactionsValueProvider(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		public IEnumerable<IDISTradeTransaction> TradeTransactions
		{
			get
			{
				if (declaration.IsRecon)
				{
					return GetTradeTransactionsForRecon();
				}
				else if (declaration.IsDrawback)
				{
					return GetTradeTransactionsForDrawback();
				}
				else if (declaration.IsExport)
				{
					return GetTradeTransactionsForExport();
				}
				else
				{
					return GetTradeTransactionsForImport();
				}
			}
		}

		IEnumerable<IDISTradeTransaction> GetTradeTransactionsForExport()
		{
			var result = new List<IDISTradeTransaction>();
			foreach (var entry in declaration.EntryHeadersWithOptionalDeactivated)
			{
				result.Add(new TradeTransaction()
				{
					Type = TradeTransactionType.Export,
					ShipmentNo = entry.CH_BGMReference,
					Number = entry.EntryNumber,
					XTN = entry.US_XTN,
					ReferenceNumber = declaration.BrokerReferenceNumber
				});
			}
			return result;
		}

		IEnumerable<IDISTradeTransaction> GetTradeTransactionsForRecon()
		{
			var result = new List<IDISTradeTransaction>();
			var recon = declaration.ReconDeclaration;
			result.Add(new TradeTransaction()
			{
				Type = TradeTransactionType.EntrySummary,
				FilerOrSCAC = declaration.US_EntryFilerCode,
				Number = recon.ReconEntryNumber,
				ReferenceNumber = declaration.BrokerReferenceNumber
			});
			return result;
		}

		IEnumerable<IDISTradeTransaction> GetTradeTransactionsForDrawback()
		{
			var result = new List<IDISTradeTransaction>();
			result.Add(new TradeTransaction()
			{
				Type = TradeTransactionType.EntrySummary,
				FilerOrSCAC = declaration.US_EntryFilerCode,
				Number = declaration.DeclarationNumber,
				ReferenceNumber = declaration.BrokerReferenceNumber
			});
			return result;
		}

		IEnumerable<IDISTradeTransaction> GetTradeTransactionsForImport()
		{
			var result = new List<IDISTradeTransaction>();
			if (declaration.IsFTZAdmission && !declaration.FTZControlNumber.IsEmpty)
			{
				result.Add(new TradeTransaction()
				{
					Type = TradeTransactionType.FTZAdmission,
					Number = declaration.FTZAdmissionNumber,
					ReferenceNumber = declaration.BrokerReferenceNumber
				});
			}
			else if (declaration.ActiveEntryHeaders.Count == 0 && !declaration.ImportEntryNumber.IsEmpty && (declaration.US_EnableENS || declaration.US_EnableCRL))
			{
				result.Add(new TradeTransaction()
				{
					Type = declaration.US_EnableENS ? TradeTransactionType.EntrySummary : TradeTransactionType.Entry,
					FilerOrSCAC = declaration.US_EntryFilerCode,
					Number = declaration.ImportEntryNumber,
					ReferenceNumber = declaration.BrokerReferenceNumber
				});
			}
			else
			{
				var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry ?? declaration.ActiveEntryHeaders.SimplifiedEntry;
				if (entry != null)
				{
					result.Add(new TradeTransaction()
					{
						Type = entry.IsFormalEntry ? TradeTransactionType.EntrySummary : TradeTransactionType.Entry,
						FilerOrSCAC = declaration.US_EntryFilerCode,
						Number = declaration.ImportEntryNumber,
						ReferenceNumber = entry.BrokerReferenceNumber
					});
				}
				else if (declaration.LowestBills.Cast<Bill>().Any())
				{
					var billsReportedList = new List<ZString>();
					foreach (Bill bill in declaration.LowestBills)
					{
						var billTransaction = new TradeTransaction() { Type = TradeTransactionType.Bill, FilerOrSCAC = bill.EffectiveMasterBillIssuerSCAC, Number = bill.CU_MasterBill };
						var shouldReportBillTransaction = true;
						if (!bill.IsMasterBill)
						{
							var houseBillNumberWithSCAC = ZString.Empty;
							var houseBill = bill.GetBillOfType(Customs.Business.BillTypeList.Codes.HouseBill);
							if (houseBill != null)
							{
								houseBillNumberWithSCAC = houseBill.US_UI_NKBillIssuerSCAC + houseBill.CU_BillNum;
							}

							shouldReportBillTransaction = !houseBillNumberWithSCAC.IsEmpty && !billsReportedList.Contains(houseBillNumberWithSCAC);
							if (shouldReportBillTransaction)
							{
								billTransaction.AdditionalNumbers = new ZString[] { houseBillNumberWithSCAC };
								billsReportedList.Add(houseBillNumberWithSCAC);
							}
						}

						if (shouldReportBillTransaction)
						{
							result.Add(billTransaction);
						}
					}
				}
			}
			return result;
		}
	}
	public class TradeTransaction : IDISTradeTransaction
	{
		public TradeTransactionType Type
		{
			get;
			set;
		}

		public IEnumerable<ZString> AdditionalNumbers
		{
			get { return additionalNumbers ?? Enumerable.Empty<ZString>(); }
			internal set { additionalNumbers = value; }
		}
		IEnumerable<ZString> additionalNumbers;

		public ZString FilerOrSCAC
		{
			get;
			set;
		}

		public ZString Number
		{
			get;
			set;
		}

		public ZString ReferenceNumber
		{
			get;
			set;
		}

		public ZString ShipmentNo
		{
			get;
			set;
		}

		public ZString XTN
		{
			get;
			internal set;
		}
	}
}
