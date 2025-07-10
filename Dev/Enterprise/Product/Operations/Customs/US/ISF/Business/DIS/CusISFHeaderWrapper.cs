using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.ISF.Business.DIS
{
	class CusISFHeaderWrapper : IUSDISDefaultValues, IDISTradeTransaction, IDISBondDataDefault
	{
		public CusISFHeaderWrapper(CusISFHeader cusISFHeader)
		{
			this.cusISFHeader = cusISFHeader;
		}

		readonly CusISFHeader cusISFHeader;

		public ZDateTime ArrivalDate
		{
			get { return ZDateTime.Empty; }
		}
		public IEnumerable<IDISBondDataDefault> DefaultBondData
		{
			get { yield return this; }
		}
		public IEnumerable<IDISCBPRequestDefault> DefaultCBPRequests
		{
			get { return new List<IDISCBPRequestDefault>(); }
		}

		public IEnumerable<ICommercialInvoiceDefault> DefaultInvoiceData
		{
			get { return new List<ICommercialInvoiceDefault>(); }
		}
		public IEnumerable<IDISTradeTransaction> DefaultTradeTransactions
		{
			get { yield return this; }
		}
		public ZString ImporterOfRecordID
		{
			get { return cusISFHeader.BF_ImporterCode; }
		}
		public ZString PortOfEntry
		{
			get { return ZString.Empty; }
		}
		public ZString PortOfUnlading
		{
			get { return ZString.Empty; }
		}
		public ZString PreparerID
		{
			get { return USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(cusISFHeader.RegistryCompanyPK, Guid.Empty, Guid.Empty).EntryFilerCode; }
		}
		public ZString PreparerSiteCode
		{
			get
			{
				return USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.GetFallBackValueAtAllLevels(cusISFHeader.RegistryCompanyPK, Guid.Empty, Guid.Empty);
			}
		}

		public bool IsExport
		{
			get { return false; }
		}
		public TransactionCategory TransactionCategory
		{
			get { return MasterFiles.Business.DIS.TransactionCategory.SingleTransaction; }
		}

		#region IDISTradeTransaction Members
		IEnumerable<ZString> IDISTradeTransaction.AdditionalNumbers
		{
			get { return Enumerable.Empty<ZString>(); }
		}
		ZString IDISTradeTransaction.FilerOrSCAC
		{
			get { return USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(cusISFHeader.RegistryCompanyPK, Guid.Empty, Guid.Empty).EntryFilerCode; }
		}
		ZString IDISTradeTransaction.Number
		{
			get { return cusISFHeader.BF_CustomsReference; }
		}
		ZString IDISTradeTransaction.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		ZString IDISTradeTransaction.ShipmentNo
		{
			get { return ZString.Empty; }
		}

		ZString IDISTradeTransaction.XTN
		{
			get { return ZString.Empty; }
		}

		TradeTransactionType IDISTradeTransaction.Type
		{
			get { return TradeTransactionType.ISFNumber; }
		}
		#endregion

		#region IDISBondDataDefault Members
		ZString IDISBondDataDefault.Code
		{
			get { return BondDataDefaultCode.Bond1; }
		}
		ZString IDISBondDataDefault.Description
		{
			get { return cusISFHeader.BF_BondNumberOrHolder; }
		}

		ZString IDISBondData.AgentIDNumber
		{
			get { return ZString.Empty; }
		}

		ZDecimal IDISBondData.BondAmount
		{
			get { return 0m; }
		}

		BondNameType IDISBondData.BondName
		{
			get { return cusISFHeader.BF_BondType == BondTypeList.Codes.SingleTransactionBond ? BondNameType.Single : BondNameType.Other; }
		}

		public ZString BondNumber
		{
			get { return cusISFHeader.BF_BondNumberOrHolder; }
		}

		public ZString BondType
		{
			get { return cusISFHeader.BF_BondType; }
		}

		public ZString Filer
		{
			get { return ZString.Empty; }
		}

		public ZString SuretyCode
		{
			get { return cusISFHeader.BF_SuretyCode; }
		}

		public ZString Code
		{
			get { return cusISFHeader.BF_BondActivityCode; }
		}

		public ZString Description
		{
			get { return ZString.Empty; }
		}

		#endregion

	}
}
