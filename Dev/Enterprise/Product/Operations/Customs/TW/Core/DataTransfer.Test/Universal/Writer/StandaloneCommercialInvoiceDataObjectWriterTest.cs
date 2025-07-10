using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;
using StandaloneCommercialInvoiceDataObjectWriter = Enterprise.Customs.TW.DataTransfer.Universal.StandaloneCommercialInvoiceDataObjectWriter;

namespace Enterprise.Customs.TW.DataTransfer.Testing
{
	sealed class StandaloneCommercialInvoiceDataObjectWriterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetNewCommercialInvoiceHeaderDataObjectWriter()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var writer = new StandaloneCommercialInvoiceDataObjectWriterForTest(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)));
			NUnit.Framework.Assert.That(writer.GetNewCommercialInvoiceHeaderDataObjectWriterExpose(invoice), Is.TypeOf<TWInvoiceHeaderDataObjectWriter>());
		}

		class StandaloneCommercialInvoiceDataObjectWriterForTest : StandaloneCommercialInvoiceDataObjectWriter
		{
			protected internal StandaloneCommercialInvoiceDataObjectWriterForTest(IDataWritingManager manager) : base(manager)
			{
			}

			public CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriterExpose(JobComInvoiceHeader obj)
			{
				var helper = CreateNewUniversalDataObjectWriterHelper(obj);
				return GetNewCommercialInvoiceHeaderDataObjectWriter(helper);
			}
		}
	}
}
