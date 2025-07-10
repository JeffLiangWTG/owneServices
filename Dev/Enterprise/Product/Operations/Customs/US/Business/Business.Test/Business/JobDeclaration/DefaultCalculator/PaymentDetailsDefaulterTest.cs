using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class PaymentDetailsDefaulterTest : TestCaseWithFactory
	{
		public void TestPaymentTypeIsDefaulted()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var countryData = Factory.New<OrgCountryData>();
			countryData.OV_OH_OrgHeader = importer.PK;
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedStates;

			var addInfo = new OrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
			addInfo.ZO_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			addInfo.ZO_BrokerToPay = YesNoDefaultList.Codes.Yes;
			Factory.Save();
			AssertEquals("Serialised", false, countryData.OV_ImportCustomsDefaultAddInfo.IsEmpty);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.IOROrgPK = importer.PK;
			AssertEquals("Payment type defaulted", PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter, declaration.US_PaymentType);
			AssertEquals("Broker To Pay Indicator defaulted", YesNoDefaultList.Codes.Yes, declaration.BrokerToPayIndicator);
			AssertEquals("Payment Method should be set", Customs.Business.PaymentPartyCodeDescriptionList.Codes.Broker, declaration.JE_PaymentMethod);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.IOROrgPK = importer.PK;
			AssertEquals("Payment type not defaulted for export", "", declaration.US_PaymentType);
			AssertEquals("Broker To Pay not defaulted for export", "", declaration.BrokerToPayIndicator);
			AssertEquals("Payment Method 'DEF' by default for export", Customs.Business.PaymentPartyCodeDescriptionList.Codes.Default, declaration.JE_PaymentMethod);
		}

		public void TestDefaultBrokerToPayWhenEmptyOnOrg()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();

			var iorWrapper = OrgHeaderWrapper.New(importer);
			iorWrapper.ZO_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			iorWrapper.ZO_BrokerToPay = "";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.IOROrgPK = importer.PK;
			AssertEquals("Payment type defaulted", PaymentTypeList.Codes.IndividualBasis, declaration.US_PaymentType);
			AssertEquals("Broker To Pay Indicator should be empty", "", declaration.BrokerToPayIndicator);

			declaration.IOROrgPK = ZGuid.Empty;
			iorWrapper.ZO_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			iorWrapper.ZO_BrokerToPay = "";
			Factory.Save();
			declaration.IOROrgPK = importer.PK;
			AssertEquals("Payment type defaulted", PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter, declaration.US_PaymentType);
			AssertEquals("Broker To Pay Indicator should be NO, because Payment Type is Importer and no other information",
						YesNoDefaultList.Codes.No, declaration.BrokerToPayIndicator);

			declaration.IOROrgPK = ZGuid.Empty;
			iorWrapper.ZO_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			iorWrapper.ZO_BrokerToPay = "";
			Factory.Save();
			declaration.IOROrgPK = importer.PK;
			AssertEquals("Payment type defaulted", PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, declaration.US_PaymentType);
			AssertEquals("Broker To Pay Indicator should be YES, because Payment Type is Broker", YesNoDefaultList.Codes.Yes, declaration.BrokerToPayIndicator);

			declaration.IOROrgPK = ZGuid.Empty;
			iorWrapper.ZO_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			iorWrapper.ZO_PayMethod = ACHPaymentTypeList.Codes.ACHDebit;
			iorWrapper.ZO_AccountNo = "111111";
			iorWrapper.ZO_BrokerToPay = "";
			Factory.Save();

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();

			OrgHeaderWrapper.New(creditor).ZO_AccountNo = "111111";
			creditor.CompanyData.OB_AB_APDefaultBankAccount = bankAccount.PK;
			Factory.Save();
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creditor.PK.ToGuid());

			var brokerAccounts = new BrokersAccountCollection();
			var account = brokerAccounts.AddNew();
			account.PayerUnitNumber = "111111";
			account.BankAccount = bankAccount2.PK;
			USCustomsDataRegistry.Instance.BrokersAccounts.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, brokerAccounts);

			declaration.IOROrgPK = importer.PK;
			AssertEquals("Payment type defaulted", PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter, declaration.US_PaymentType);
			AssertEquals("Broker To Pay Indicator should be YES, because Payment Type is Importer and Managed Account exists", YesNoDefaultList.Codes.Yes, declaration.BrokerToPayIndicator);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.IOROrgPK = importer.PK;
			AssertEquals("Payment type not defaulted for export", "", declaration.US_PaymentType);
			AssertEquals("Broker To Pay not defaulted for export", "", declaration.BrokerToPayIndicator);
			AssertEquals("Payment Method 'DEF' by default for export", Customs.Business.PaymentPartyCodeDescriptionList.Codes.Default, declaration.JE_PaymentMethod);
		}

		public void TestDefaultBrokerToPayBasedOnPaymentType()
		{
			var defaulter = new PaymentDetailsDefaulter();
			AssertEquals("Broker To Pay should be empty", ZString.Empty, defaulter.GetDefaultBrokerToPayIndicatorBasedOnPaymentType(""));
			AssertEquals("Broker To Pay should be YES", YesNoDefaultList.Codes.Yes, defaulter.GetDefaultBrokerToPayIndicatorBasedOnPaymentType(PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode));
			AssertEquals("Broker To Pay should be YES", YesNoDefaultList.Codes.Yes, defaulter.GetDefaultBrokerToPayIndicatorBasedOnPaymentType(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate));
			AssertEquals("Broker To Pay should be NO", YesNoDefaultList.Codes.No, defaulter.GetDefaultBrokerToPayIndicatorBasedOnPaymentType(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter));
			AssertEquals("Broker To Pay should be empty", ZString.Empty, defaulter.GetDefaultBrokerToPayIndicatorBasedOnPaymentType(PaymentTypeList.Codes.IndividualBasis));
		}
	}
}
