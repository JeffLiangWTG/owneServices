using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	public static class VAT404TestHelper
	{
		public static CusEntryHeader SetupEntry(BusinessObjectFactory factory, ZString lrn)
		{
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TestOrg";

			var dec = factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_OH_Importer = org.PK;
			var entry = dec.ActiveEntryHeaders.AddNew();
			entry.CH_BGMReference = lrn;
			factory.Save();
			return entry;
		}

		public static CusEntryPayInfo AddPayInfo(CusEntryHeader entry, string type, decimal amount, string receipt, ZDateTime paymentdate, ZDateTime receiptdate)
		{
			var addedPayInfo = entry.EntryPayInfos.AddNew();
			addedPayInfo.C9_TransactionType = type;
			addedPayInfo.C9_PaymentAmount = amount;
			addedPayInfo.C9_PaymentReference = receipt;
			addedPayInfo.C9_PaymentDate = paymentdate;
			addedPayInfo.C9_ReceiptDate = receiptdate.Date;
			entry.Factory.Save();
			return addedPayInfo;
		}

		public static void SetupPayInfo(BusinessObjectFactory factory)
		{
			var entry = SetupEntry(factory, "LRN001");
			AddPayInfo(entry, "VAT", 12m, "RCP01", ZDateTime.Today, ZDateTime.Today);
			factory.Save();
		}

		public static VAT404Document GetVAT404Document(BusinessObjectFactory factory)
		{
			VAT404TestHelper.SetupPayInfo(factory);
			var tester = new VAT404DocumentInstruction(factory);
			tester.PerformSearch();
			return (VAT404Document)tester.VAT404Documents.FirstOrDefault();
		}
	}
}
