using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business.Testing
{
	class PkgPalletTransactionValidationTest : BusinessObjectValidationTestCase
	{
		#region TestEquipmentCode

		public void TestEquipmentCode()
		{
			var pallet = Factory.New<PkgPalletTransaction>();
			pallet.KTR_PalletType = pallet.Lookups.PalletTypes[0].Code;
			var equipmentCode = pallet.KTR_EquipmentCode;
			AssertNoErrors(pallet.KTR_EquipmentCodeInfo);
			AssertNoWarnings(pallet.KTR_EquipmentCodeInfo);

			pallet.KTR_EquipmentCode = ZString.Empty;
			AssertHasErrors(pallet.KTR_EquipmentCodeInfo);
			AssertNoWarnings(pallet.KTR_EquipmentCodeInfo);

			pallet.KTR_EquipmentCode = equipmentCode;
			AssertNoErrors(pallet.KTR_EquipmentCodeInfo);
			AssertNoWarnings(pallet.KTR_EquipmentCodeInfo);

			pallet.KTR_EquipmentCode = "RYLAN";
			AssertNoErrors(pallet.KTR_EquipmentCodeInfo);
			AssertHasWarning(pallet.KTR_EquipmentCodeInfo, string.Format("The Pallet Type chosen ({0}) has an equipment code configured in the registry of {1}, which is different to what you have entered here. Please check that correct pallet type and equipment have been specified.", pallet.KTR_PalletType, equipmentCode));

			pallet.KTR_EquipmentCode = equipmentCode;
			AssertNoErrors(pallet.KTR_EquipmentCodeInfo);
			AssertNoWarnings(pallet.KTR_EquipmentCodeInfo);
		}

		#endregion

		#region TestTradingAccountNumbers

		public void TestTransferFromAccountNumber()
		{
			AssertTradingAccountNumber(PkgPalletTransactionSchema.KTR_TransferFromAccountNumber, p => p.KTR_TransferFromAccountNumberInfo);
		}

		public void TestTransferToAccountNumber()
		{
			AssertTradingAccountNumber(PkgPalletTransactionSchema.KTR_TransferToAccountNumber, p => p.KTR_TransferToAccountNumberInfo);
		}

		void AssertTradingAccountNumber(SchemaStringColumn tradingAccountColumn, Func<PkgPalletTransaction, ZPropertyInfo> getPropertyInfo)
		{
			var exchange = Helper.CreatePalletTransaction(PalletTransactionTypeList.Codes.Exchange, PalletExchangeTypeList.Codes.DirectExchange);
			var transfer = Helper.CreatePalletTransaction(PalletTransactionTypeList.Codes.Transfer, PalletTransferTypeList.Codes.TransferOff);
			exchange[tradingAccountColumn] = "1";
			transfer[tradingAccountColumn] = "1";
			AssertHasError(getPropertyInfo(exchange), "Exchange transfers must not have a trading account number.");
			AssertNoErrors("Precondition", getPropertyInfo(transfer));

			exchange[tradingAccountColumn] = "";
			transfer[tradingAccountColumn] = "";
			AssertNoErrors(getPropertyInfo(exchange));
			AssertHasError(getPropertyInfo(transfer), "Please enter a Trading Account.");
		}

		#endregion

		#region Implementation

		PackingTestHelper Helper
		{
			get { return helper ?? (helper = new PackingTestHelper(Factory)); }
		}
		PackingTestHelper helper;

		#endregion
	}
}
