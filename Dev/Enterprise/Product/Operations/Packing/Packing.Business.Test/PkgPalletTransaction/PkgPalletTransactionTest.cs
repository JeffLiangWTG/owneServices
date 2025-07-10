using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPalletTransaction))]
	public class PkgPalletTransactionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableNameCore()
		{
			var transaction = Factory.New<PkgPalletTransaction>();
			transaction.KTR_PaperDocketID = "TEST12";
			AssertEquals("Pallet Transaction TEST12", transaction.HumanReadableName);
		}

		public void TestAdditionalReferenceNumberTypeList()
		{
			var transaction = Factory.New<PkgPalletTransaction>();
			var list = ((IAdditionalReferenceNumberTypeProvider)transaction).GetAdditionalReferenceNumberTypeList(ZString.Empty, ZString.Empty);
			AssertEquals(2, list.Count);

			AssertEquals("PAP", list[0].Code);
			AssertEquals("Paper Docket ID", list[0].Description);
			AssertEquals(true, ((ICustomsNumberTypeCodeDescription)list[0]).IsUnique);

			AssertEquals("REF", list[1].Code);
			AssertEquals("Job Reference", list[1].Description);
			AssertEquals(false, ((ICustomsNumberTypeCodeDescription)list[1]).IsUnique);
		}

		#region TestPaperDocketID

		public void TestPaperDocketID()
		{
			var transaction = Factory.New<PkgPalletTransaction>();
			AssertEquals(ZString.Empty, transaction.PaperDocketID);

			var refNum = transaction.AdditionalReferenceNumbers.AddNew();
			refNum.CE_EntryType = "PAP";
			refNum.CE_EntryNum = "AlRyZyLolRakZubAp";
			AssertEquals("AlRyZyLolRakZubAp", transaction.PaperDocketID);

			refNum.CE_EntryType = "REF";
			AssertEquals(ZString.Empty, transaction.PaperDocketID);
			Assert(transaction.KTR_PaperDocketIDInfo.ReadOnly);
		}

		public void TestPaperDocketIDInfo()
		{
			var transaction = Factory.New<PkgPalletTransaction>();
			Assert(transaction.KTR_PaperDocketIDInfo.ReadOnly);
			AssertEquals(24, transaction.KTR_PaperDocketIDInfo.MaxLength);

			transaction.KTR_TransactionType = PalletTransactionTypeList.Codes.Transfer;
			Assert(!transaction.KTR_PaperDocketIDInfo.ReadOnly);

			transaction.KTR_TransactionType = PalletTransactionTypeList.Codes.Exchange;
			Assert(transaction.KTR_PaperDocketIDInfo.ReadOnly);
		}

		#endregion

		public void TestSaving()
		{
			var transaction = Factory.New<PkgPalletTransaction>();
			transaction.KTR_TransactionType = "XON";
			transaction.KTR_PalletType = "XYZ";
			Factory.Save();
			AssertEquals(false, transaction.KTR_TransactionID.IsEmpty);
			var id = transaction.KTR_TransactionID;

			Factory.Save();
			AssertEquals(id, transaction.KTR_TransactionID);

			var transaction2 = Factory.New<PkgPalletTransaction>();
			transaction2.KTR_TransactionType = "XON";
			transaction2.KTR_PalletType = "XYZ";
			transaction2.KTR_TransactionID = "Aleera";
			Factory.Save();
			AssertEquals("Aleera", transaction2.KTR_TransactionID);

			AssertEquals(true, transaction2.KTR_TransactionIDInfo.ReadOnly);
		}

		public void TestDefaultValues()
		{
			var transaction = Factory.New<PkgPalletTransaction>();
			AssertEquals(PalletTransactionStatusList.Codes.Held, transaction.KTR_Status);
			AssertEquals(1, transaction.KTR_Quantity);
		}

		public void TestLookupDescriptions()
		{
			var transaction = Factory.New<PkgPalletTransaction>();
			transaction.KTR_Status = PalletTransactionStatusList.Codes.Processed;
			transaction.KTR_TransactionType = PalletTransactionTypeList.Codes.Exchange;
			transaction.KTR_PalletType = "ZUB";

			AssertEquals(PalletTransactionStatusList.Descriptions.Processed, transaction.StatusDescription);
			AssertEquals(PalletTransactionTypeList.Descriptions.Exchange, transaction.TransactionTypeDescription);
			AssertEquals("Zubin Pallets", transaction.PalletTypeDescription);
		}

		public void TestSettingPalletTypeSetsEquipmentType()
		{
			var transaction = Factory.New<PkgPalletTransaction>();
			transaction.KTR_PalletType = "ZUB";
			AssertEquals("ZP", transaction.KTR_EquipmentCode);

			transaction.KTR_PalletType = "RAK";
			AssertEquals("RP", transaction.KTR_EquipmentCode);

			transaction.KTR_PalletType = "CRP";
			AssertEquals("RP", transaction.KTR_EquipmentCode);
		}

		public void TestSettingParentToInvalidOrBlank_LeavesLastValue()
		{
			var dummy1 = (IPalletTransactionParent)Factory.New<DummyPalletMaster>();
			var dummy2 = (IPalletTransactionParent)Factory.New<DummyPalletMaster>();

			var pallet = Factory.New<PkgPalletTransaction>();
			var fakeGuid = ZGuid.NewZGuid();
			pallet.KTR_ParentID = fakeGuid;
			AssertEquals(fakeGuid, pallet.KTR_ParentID);

			pallet = Factory.New<PkgPalletTransaction>();
			pallet.PossibleParents = new[] { dummy1, dummy2 };
			pallet.KTR_ParentID = fakeGuid;
			AssertEquals(ZGuid.Empty, pallet.KTR_ParentID);

			pallet.KTR_ParentID = dummy1.PK;
			AssertEquals(dummy1.PK, pallet.KTR_ParentID);

			pallet.KTR_ParentID = dummy2.PK;
			AssertEquals(dummy2.PK, pallet.KTR_ParentID);

			pallet.KTR_ParentID = ZGuid.NewZGuid();
			AssertEquals(dummy2.PK, pallet.KTR_ParentID);

			pallet.KTR_ParentID = ZGuid.Empty;
			AssertEquals(dummy2.PK, pallet.KTR_ParentID);
		}

		public void TestDescription()
		{
			var transaction = Factory.New<PkgPalletTransaction>();
			transaction.KTR_PalletType = "ZUB";
			transaction.KTR_EffectiveDateTime = new ZDateTimeOffset(2014, 9, 26, 01, 11, 00, new TimeSpan());
			transaction.KTR_TransactionType = "TRF";

			AssertEquals("Pallet Transaction - ZUB - 26-Sep-14 01:11:00 +00:00 - TRF", transaction.Description);
		}

		public void TestRelatedJob_SetsJobReferences()
		{
			var dummy = Factory.New<DummyPalletMaster>();
			var pallet = Factory.New<PkgPalletTransaction>();
			pallet.RelatedJob = dummy;
			AssertEquals(2, pallet.AdditionalReferenceNumbers.Count);
			AssertEquals("ABC", pallet.AdditionalReferenceNumbers[0].CE_EntryNum);
			AssertEquals("REF", pallet.AdditionalReferenceNumbers[0].CE_EntryType);

			AssertEquals("XYZ", pallet.AdditionalReferenceNumbers[1].CE_EntryNum);
			AssertEquals("REF", pallet.AdditionalReferenceNumbers[1].CE_EntryType);
		}

		public void TestRelatedJob_SetsParentFields()
		{
			var dummy = Factory.New<DummyPalletMaster>();
			var pallet = Factory.New<PkgPalletTransaction>();
			pallet.RelatedJob = dummy;
			AssertEquals(dummy.PK, pallet.KTR_ParentID);
			AssertEquals(dummy.TablePrefix, pallet.KTR_ParentTableCode);
		}

		public void TestDocManagerSupport()
		{
			var transaction = Factory.New<PkgPalletTransaction>();
			var docManager = ((IDocManagerSupport)transaction).DocManagerInfo;
			AssertEquals(transaction, docManager.BusinessEntity);
			AssertEquals(Constants.DocManagerCodes.PalletTransaction, docManager.DocManagerCode);
		}

		public void TestRelatedJob_SetsTransferToAndFrom()
		{
			var fromOrg = Factory.New<OrgHeader>();
			var toOrg = Factory.New<OrgHeader>();

			var fromAddress = Factory.New<JobDocAddress>();
			fromAddress.E2_OA_Address = fromOrg.MainAddress.PK;
			var toAddress = Factory.New<JobDocAddress>();
			toAddress.E2_OA_Address = toOrg.MainAddress.PK;

			var dummy = Factory.New<DummyPalletMaster>();
			dummy.TransferFromAddressForTest = fromAddress;
			dummy.TransferToAddressForTest = toAddress;

			var pallet = Factory.New<PkgPalletTransaction>();
			AssertEquals("Precondition", ZGuid.Empty, pallet.TransferFrom.OrganisationPK);
			AssertEquals("Precondition", ZGuid.Empty, pallet.TransferTo.OrganisationPK);

			pallet.RelatedJob = dummy;
			AssertEquals(fromAddress.OrganisationPK, pallet.TransferFrom.OrganisationPK);
			AssertEquals(toAddress.OrganisationPK, pallet.TransferTo.OrganisationPK);
		}

		public void TestRelatedJob_SetsTransferToAndFrom_OverriddenAddress()
		{
			var fromOrg = Factory.New<OrgHeader>();
			var toOrg = Factory.New<OrgHeader>();

			var fromAddress = Factory.NewWithValidTestData<JobDocAddress>();
			fromAddress.E2_AddressOverride = true;
			var toAddress = Factory.NewWithValidTestData<JobDocAddress>();
			toAddress.E2_AddressOverride = true;

			var dummy = Factory.New<DummyPalletMaster>();
			dummy.TransferFromAddressForTest = fromAddress;
			dummy.TransferToAddressForTest = toAddress;

			var pallet = Factory.New<PkgPalletTransaction>();
			pallet.RelatedJob = dummy;
			AssertPalletTransactions(fromAddress, pallet.TransferFrom);
			AssertPalletTransactions(toAddress, pallet.TransferTo);
		}

		public void TestTransferToAndTransferFromAccountNumbers_Readonly()
		{
			var pallet = Factory.New<PkgPalletTransaction>();
			AssertEquals("Precondition", false, pallet.TransferFrom.E2_AddressOverride);
			AssertEquals("Precondition", false, pallet.TransferTo.E2_AddressOverride);
			Assert(pallet.KTR_TransferFromAccountNumberInfo.ReadOnly);
			Assert(pallet.KTR_TransferToAccountNumberInfo.ReadOnly);

			pallet.TransferFrom.E2_AddressOverride = true;
			pallet.TransferTo.E2_AddressOverride = true;
			Assert(!pallet.KTR_TransferFromAccountNumberInfo.ReadOnly);
			Assert(!pallet.KTR_TransferToAccountNumberInfo.ReadOnly);
		}

		public void TestTransferFromAndToTradingAccountNumbers()
		{
			var palletTypeParent = new PalletTypeParent();
			Helper.CreatePalletType(palletTypeParent, "CHP", "Chep Pallets", "CP", OrgCusCode.CodeTypes.PalletTradingAccountChep);
			Helper.CreatePalletType(palletTypeParent, "LOS", "LOS Pallets", "LP", OrgCusCode.CodeTypes.PalletTradingAccountLoscam);
			PackingRegistry.Instance.PalletTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, palletTypeParent);

			var orgHeader = Factory.New<OrgHeader>();
			Helper.CreateCustomCode(orgHeader, OrgCusCode.CodeTypes.PalletTradingAccountChep, "Chep - RegNo");
			Helper.CreateCustomCode(orgHeader, OrgCusCode.CodeTypes.PalletTradingAccountLoscam, "Loscam - RegNo");
			AssertTradingAccountNumber(t => t.KTR_TransferFromAccountNumber, (t, v) => t.KTR_TransferFromAccountNumber = v, t => t.TransferFrom, orgHeader);
			AssertTradingAccountNumber(t => t.KTR_TransferToAccountNumber, (t, v) => t.KTR_TransferToAccountNumber = v, t => t.TransferTo, orgHeader);
		}

		void AssertTradingAccountNumber(
			Func<PkgPalletTransaction, string> getAccountNumber,
			Action<PkgPalletTransaction, string> setAccountNumber,
			Func<PkgPalletTransaction, JobDocAddress> getDocAddress,
			OrgHeader orgHeader)
		{
			var transaction = Helper.CreatePalletTransaction(PalletTransactionTypeList.Codes.Transfer, PalletTransferTypeList.Codes.Direct);
			transaction.KTR_PalletType = OrgCusCode.CodeTypes.PalletTradingAccountChep;
			getDocAddress(transaction).OrganisationPK = orgHeader.PK;
			AssertEquals("Chep - RegNo", getAccountNumber(transaction));

			transaction.KTR_PalletType = OrgCusCode.CodeTypes.PalletTradingAccountLoscam;
			AssertEquals("Loscam - RegNo", getAccountNumber(transaction));

			getDocAddress(transaction).E2_AddressOverride = true;
			AssertEquals("Address override should not remove trading account number.", "Loscam - RegNo", getAccountNumber(transaction));

			setAccountNumber(transaction, "Custom value");
			getDocAddress(transaction).E2_AddressOverride = false;
			AssertEquals("Loscam - RegNo", getAccountNumber(transaction));

			getDocAddress(transaction).OrganisationPK = ZGuid.Empty;
			AssertEquals("", getAccountNumber(transaction));
		}

		#region TestIDocAddressMemebers

		public void TestJobDocAddressRequirement()
		{
			var pallet = Factory.New<PkgPalletTransaction>();
			var transferFromAddressRequirement = ((IDocAddresses)pallet).GetDocAddressRequirement(DocAddressType.LocalCartageExporter);
			var transferToAddressRequirement = ((IDocAddresses)pallet).GetDocAddressRequirement(DocAddressType.LocalCartageImporter);
			AssertEquals(true, transferFromAddressRequirement.IsMandatory);
			AssertEquals(true, transferToAddressRequirement.IsMandatory);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var palletTypeParent = new PalletTypeParent();
			var type1 = palletTypeParent.Types.AddNew();
			type1.Code = "ZUB";
			type1.Description = (NoResString)"Zubin Pallets";
			type1.EquipmentCode = "ZP";
			type1.ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountChep;

			var type2 = palletTypeParent.Types.AddNew();
			type2.Code = "RAK";
			type2.Description = (NoResString)"Rakhsh Pallets";
			type2.EquipmentCode = "RP";
			type2.ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam;

			PackingRegistry.Instance.PalletTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, palletTypeParent);
		}

		void AssertPalletTransactions(JobDocAddress expected, JobDocAddress actual)
		{
			AssertEquals(expected.E2_Contact, actual.E2_Contact);
			AssertEquals(expected.E2_CompanyName, actual.E2_CompanyName);
			AssertEquals(expected.E2_Address1, actual.E2_Address1);
			AssertEquals(expected.E2_Address2, actual.E2_Address2);
			AssertEquals(expected.E2_City, actual.E2_City);
			AssertEquals(expected.E2_State, actual.E2_State);
			AssertEquals(expected.E2_Postcode, actual.E2_Postcode);
			AssertEquals(expected.E2_Phone, actual.E2_Phone);
			AssertEquals(expected.E2_Fax, actual.E2_Fax);
			AssertEquals(expected.E2_Email, actual.E2_Email);
			AssertEquals(expected.E2_Mobile, actual.E2_Mobile);
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
