using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.ISF.Business.DIS.Testing
{
	sealed class CusISFHeaderWrapperTest : TestCaseWithFactory
	{
		public void TestIUSDISDefaultValues()
		{
			var entryFiler = new EntryFiler();
			entryFiler.EntryFilerCode = "SV9";
			var header = Factory.New<CusISFHeader>();
			var company = Factory.New<GlbCompany>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			header.BF_EntryNumber = "1";
			header.BF_ShipmentType = ShipmentTypeList.Codes.StandardOrRegularFilings;
			header.BF_TransportMode = TransportModeCodes.Codes.OceanVesselContainerized;
			header.BF_GB = branch.PK;
			header.BF_OH_Importer = ZGuid.Empty;
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.CompliantTransaction;
			header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.IRS;
			header.BF_ImporterCode = "45-985412300";
			header.BF_ImporterFullName = "ImporterName";
			header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.IRS;
			header.BF_ConsigneeCode = "45-985412300";
			header.BF_BondNumberOrHolder = "45-985412300";
			header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ImporterOrBroker;
			header.BF_BondType = BondTypeList.Codes.ContinuousBond;
			header.BF_CustomsReference = "SV9-9999999";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, entryFiler);
			var headerWrapper = new CusISFHeaderWrapper(header);
			AssertEquals("45-985412300", headerWrapper.ImporterOfRecordID);
			AssertEquals("", headerWrapper.PortOfUnlading);
			AssertEquals("", headerWrapper.PortOfEntry);
			AssertEquals(ZDateTime.Empty, headerWrapper.ArrivalDate);
			AssertEquals(1, headerWrapper.DefaultBondData.Count());
			AssertEquals(0, headerWrapper.DefaultCBPRequests.Count());
			AssertEquals(0, headerWrapper.DefaultInvoiceData.Count());
			AssertEquals(1, headerWrapper.DefaultTradeTransactions.Count());
			var tradeTransaction = headerWrapper.DefaultTradeTransactions.First();
			AssertEquals("SV9-9999999", tradeTransaction.Number);
			AssertEquals("SV9", tradeTransaction.FilerOrSCAC);
			AssertEquals(TradeTransactionType.ISFNumber, tradeTransaction.Type);
		}
	}
}
