using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DrawbackNAFTA))]
	public class DrawbackNAFTATest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<DrawbackNAFTA>
	{
		[TestDate(2016, 8, 16)]
		public void TestIACEDrawbackNAFATTariff()
		{
			DrawbackNAFTA.US_DRWNAFTACountryImportEntry = "71019301";
			DrawbackNAFTA.US_DRWNAFTACountryImportEntryDate = new ZDateTime(2016, 8, 16);
			DrawbackNAFTA.US_DRWNAFTACountryImportDuty = 123m;
			DrawbackNAFTA.US_DRWNAFTACountryDutyRate = 0.9m;
			DrawbackNAFTA.US_DRWNAFTACountryTariffNumber = "9201011001";
			DrawbackNAFTA.US_DRWNAFTACountryTariffNumber2 = "9201011002";
			DrawbackNAFTA.US_DRWNAFTACountryTariffNumber3 = "9201011003";
			DrawbackNAFTA.US_DRWNAFTACountryOfExport = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals(InvoiceLine, DrawbackNAFTA.InvoiceLine);

			IACEDrawbackNAFATTariff nafatTariff = DrawbackNAFTA;
			AssertEquals("71019301", nafatTariff.EntryNumber);
			AssertEquals(new ZDateTime(2016, 8, 16), nafatTariff.EntryDate);
			AssertEquals(123m, nafatTariff.DutyPaidToForeignGov);
			AssertEquals(0.9m, nafatTariff.ExchangeRate);
			AssertEquals("9201011001", nafatTariff.TariffNumber1);
			AssertEquals("9201011002", nafatTariff.TariffNumber2);
			AssertEquals("9201011003", nafatTariff.TariffNumber3);
			AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, nafatTariff.CountryOfExport);
		}

		public void TestCreateDrawbackNAFTAViaXml()
		{
			var customsContainerData = @"
					<CustomsContainerMode>
					  <Code>NCT</Code>
					</CustomsContainerMode>
			";
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var document = new XmlDocument();
			document.Load(GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.US.Business.Testing.Business.JobComInvoiceLine.TestFiles.GenerateDrawbackNAFTA.xml"));

			IEDIMessage iEDIMessage = Factory.New<IEDIMessage>();
			iEDIMessage.EM_ApplicationCode = "UDM";
			iEDIMessage.EM_MessageType = "XDC";
			iEDIMessage.EM_Status = "QUE";
			iEDIMessage.EM_ReceiveTransmit = "RCV";
			iEDIMessage.EM_MessageSubType = "XUS";
			iEDIMessage.EM_MessageText = document.InnerXml.Replace("<!-- CustomsContainerMode -->", customsContainerData);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(iEDIMessage);

			var invoiceLine = Factory.Load<JobComInvoiceLine>(new ZQuery(JobComInvoiceLineSchema.JI_AddInfo, SQLComparisonOperator.Contains, "DRWAccMethod=01")).FirstOrDefault();

			AssertEquals(9.9m, invoiceLine._99ClaimedDuty);
		}

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return DrawbackNAFTA;
		}

		protected override IEnumerable<DrawbackNAFTA> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var dec = factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			yield return dec.Invoices.AddNew().InvoiceLines.AddNew().DrawbackNAFTAs.AddNew();
		}

		#endregion

		#region Implementation

		DrawbackNAFTA DrawbackNAFTA
		{
			get { return drawbackNAFTA ?? (drawbackNAFTA = InvoiceLine.DrawbackNAFTAs.AddNew()); }
		}
		DrawbackNAFTA drawbackNAFTA;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Declaration.InvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
				}

				return declaration;
			}
		}
		JobDeclaration declaration;

		#endregion
	}
}
