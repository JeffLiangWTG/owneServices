using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderDocumentSupporter))]
	public class JobComInvoiceHeaderDocumentSupportTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceGroupHeader invoiceGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoiceHeader = invoiceGroupHeader.JobComInvoiceHeaders.AddNew();
			AssertEquals("Commercial invoice cannot be found.", invoiceHeader.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.JobComInvoiceHeader), null));
			AssertEquals("Commercial invoice cannot be found.", invoiceHeader.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.ComInvoiceHeader), null));
			AssertEquals("Commercial invoice cannot be found.", invoiceHeader.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.CommercialInvoice), null));
			AssertEquals("Commercial invoice cannot be found.", invoiceHeader.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.GenericCommercialInvoice), null));
		}

		public void TestShowReasonForNotPrinting()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceGroupHeader invoiceGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoiceHeader = invoiceGroupHeader.JobComInvoiceHeaders.AddNew();
			AssertEquals(true, invoiceHeader.DocumentSupporter.ShowReasonForNotPrinting(Constants.DataContext.JobComInvoiceHeader, null));
			AssertEquals(true, invoiceHeader.DocumentSupporter.ShowReasonForNotPrinting(Constants.DataContext.ComInvoiceHeader, null));
			AssertEquals(true, invoiceHeader.DocumentSupporter.ShowReasonForNotPrinting(Constants.DataContext.CommercialInvoice, null));
			AssertEquals(true, invoiceHeader.DocumentSupporter.ShowReasonForNotPrinting(Constants.DataContext.GenericCommercialInvoice, null));
		}

		public void TestGetDocBusinessObjectForGenericCommercialInvoice()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceGroupHeader invoiceGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoiceHeader = invoiceGroupHeader.JobComInvoiceHeaders.AddNew();

			DocumentWrapper[] wrappers = invoiceHeader.DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.GenericCommercialInvoice, null);
			AssertNotEquals("Wrappers with Declaration with a Commercial Invoice", null, wrappers);
			AssertEquals("Wrappers.Length with Declaration with a Commercial Invoice", 1, wrappers.Length);
			AssertNotEquals("Wrappers[0] with Declaration with a Commercial Invoice", null, wrappers[0]);
			AssertEquals("((BusinessObject)wrappers[0].WrappedObject).PK with Declaration with a Commercial Invoice", invoiceHeader.PK, ((BusinessObject)wrappers[0].WrappedObject).PK);
		}

		public virtual void TestSupportedDataContexts()
		{
			BaseJobComInvoiceHeader invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			AssertEquals("Constants.DataContext.JobComInvoiceHeader is Supported", true, invoiceHeader.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.JobComInvoiceHeader)));
			AssertEquals("Constants.DataContext.ComInvoiceHeader is Supported", true, invoiceHeader.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.ComInvoiceHeader)));
			AssertEquals("Constants.DataContext.CommercialInvoice is Supported", true, invoiceHeader.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.CommercialInvoice)));
			AssertEquals("Constants.DataContext.GenericCommercialInvoice is Supported", true, invoiceHeader.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericCommercialInvoice)));
		}

		public virtual void TestGetDocBusinessObjects()
		{
			BaseJobComInvoiceHeader invoiceHeader = (BaseJobComInvoiceHeader)GetDocumentSupportableBusinessObject();
			DocumentWrapper[] result = invoiceHeader.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericCommercialInvoice, null);
			AssertNotNull(result[0]);
		}

		public void TestBusinessContext()
		{
			BaseJobComInvoiceHeader invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			AssertEquals("JobComInvoiceHeader has a business context of Customs", BusinessContext.CommercialInvoice, invoiceHeader.DocumentSupporter.BusinessContext);
		}

		public void TestGetFilterValue()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "CMR";
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			ZString countryCode = declaration.CountryCode;
			AssertEquals("Current company country", countryCode, invoiceHeader.DocumentSupporter.GetFilterValue(DocumentFilters.CTY));
			countryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(declaration.CountryCode);
			AssertEquals("Customs company country", countryCode, invoiceHeader.DocumentSupporter.GetFilterValue(DocumentFilters.BKRCTY));
			AssertEquals("Filtervalue for MSGBKRCTYAPP", declaration.JE_MessageType + countryCode + "CMR", invoiceHeader.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYAPP));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				AssertEquals("Current company country", "PR", invoiceHeader.DocumentSupporter.GetFilterValue(DocumentFilters.CTY));
				AssertEquals("Customs company country", "US", invoiceHeader.DocumentSupporter.GetFilterValue(DocumentFilters.BKRCTY));
				AssertEquals("Filtervalue for MSGBKRCTYAPP", declaration.JE_MessageType + "USCMR", invoiceHeader.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYAPP));
			}
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			try
			{
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
				declaration.DocsAndCartage.Services.AddNew();
			}
			catch (NotSupportedException)// this exception will be thrown during Reflection test, so here catch and don't throw it
			{ }

			return invoiceHeader;
		}

		#endregion

	}
}
