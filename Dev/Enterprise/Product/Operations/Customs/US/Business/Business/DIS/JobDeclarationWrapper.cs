using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.Business.DIS
{
	class JobDeclarationWrapper : IUSDISDefaultValues
	{
		public JobDeclarationWrapper(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		public ZString ImporterOfRecordID
		{
			get { return declaration.ImporterOfRecordNumber; }
		}

		public ZString PreparerID
		{
			get
			{
				return declaration.RegistryEntryFilerCode;
			}
		}

		public ZString PreparerSiteCode => declaration.ProcessingDistrictPort;

		public ZString PortOfUnlading
		{
			get { return declaration.US_SchDArrival; }
		}

		public ZString PortOfEntry
		{
			get { return declaration.US_SchDEntry; }
		}

		public ZDateTime ArrivalDate
		{
			get { return declaration.JE_DateOfArrival; }
		}

		public bool IsExport
		{
			get { return declaration.IsExport; }
		}

		public TransactionCategory TransactionCategory
		{
			get { return MasterFiles.Business.DIS.TransactionCategory.SingleTransaction; }
		}

		public IEnumerable<IDISBondDataDefault> DefaultBondData
		{
			get
			{
				if (declaration.IsRecon)
				{
					return new List<IDISBondDataDefault>();
				}
				else
				{
					return new BondDataValueProvider(declaration).BondData;
				}
			}
		}

		public IEnumerable<IDISCBPRequestDefault> DefaultCBPRequests
		{
			get { return new CBPRequestValueProvider(declaration).CBPRequests; }
		}

		public IEnumerable<ICommercialInvoiceDefault> DefaultInvoiceData
		{
			get
			{
				if (declaration.IsDrawback || declaration.IsRecon)
				{
					return new List<ICommercialInvoiceDefault>();
				}
				else
				{
					return new InvoiceValueProvider(declaration).Invoices;
				}
			}
		}

		public IEnumerable<IDISTradeTransaction> DefaultTradeTransactions
		{
			get { return new TradeTransactionsValueProvider(declaration).TradeTransactions; }
		}
	}
}
