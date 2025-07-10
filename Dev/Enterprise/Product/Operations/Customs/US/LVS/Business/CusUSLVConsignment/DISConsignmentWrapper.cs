using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Business.DIS;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.LVS.Business
{
	public class DISConsignmentWrapper : IUSDISDefaultValues
	{
		public DISConsignmentWrapper(CusUSLVConsignment cusUSLVConsignment)
		{
			this.consignment = Argument.NotNull(cusUSLVConsignment, "cusUSLVConsignment");
			this.shipment = consignment.Shipment;
		}
		readonly CusUSLVConsignment consignment;
		readonly CusUSLVClearance shipment;

		ZString IUSDISDefaultValues.PreparerID => shipment.ULH_EntryFilerCode;

		ZString IUSDISDefaultValues.PreparerSiteCode => shipment.ULH_RemoteLocationFiling ? shipment.ULH_PortOfEntry : (ZString)USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.GetFallBackValueAtAllLevels(shipment.RegistryCompanyPK, Guid.Empty, Guid.Empty);
		ZDateTime IUSDISDefaultValues.ArrivalDate => shipment.ULH_DischargeDate;

		ZString IUSDISDefaultValues.PortOfUnlading => shipment.ULH_PortOfDischarge;

		ZString IUSDISDefaultValues.PortOfEntry => shipment.ULH_PortOfEntry;

		bool IUSDISDefaultValues.IsExport => false;

		TransactionCategory IUSDISDefaultValues.TransactionCategory => TransactionCategory.SingleTransaction;

		IEnumerable<ICommercialInvoiceDefault> IUSDISDefaultValues.DefaultInvoiceData => new List<ICommercialInvoiceDefault>();

		IEnumerable<IDISBondDataDefault> IUSDISDefaultValues.DefaultBondData => new List<IDISBondDataDefault>();

		ZString IUSDISDefaultValues.ImporterOfRecordID => shipment.ULH_IORReference;

		IEnumerable<IDISTradeTransaction> IUSDISDefaultValues.DefaultTradeTransactions => GetTradeTransactions();

		IEnumerable<IDISTradeTransaction> GetTradeTransactions()
		{
			return new List<IDISTradeTransaction>()
			{
				new TradeTransaction()
				{
				Type = TradeTransactionType.Entry,
				Number = consignment.CE_EntryNum,
				FilerOrSCAC = shipment.ULH_EntryFilerCode,
				ReferenceNumber = shipment.ULH_JobNumber
				}
			};
		}

		IEnumerable<IDISCBPRequestDefault> IUSDISDefaultValues.DefaultCBPRequests => new CBPRequestValueProvider(consignment).CBPRequests;
	}
}
