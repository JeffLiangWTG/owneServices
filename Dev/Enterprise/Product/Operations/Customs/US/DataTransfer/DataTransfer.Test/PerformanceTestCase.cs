// Set this #define to allow creation of test data in local db
//#define CreateTestDataForPerformance
#if CreateTestDataForPerformance

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	using System;
	using System.Collections.Generic;
	using Business;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Customs.DataTransfer.Universal.Testing;
	using Enterprise.UniversalDataBuss.Integration;
	using MasterFiles.Business;
	using NUnit.Framework;
	using UniversalDataBuss.DataObjects.Core;
	using UniversalDataBuss.DataObjects.Universal;
	using UniversalDataBuss.Management.Testing;
	using ZArchitecture.Environment;
	using ZArchitecture.Schema;
	using OrgSupplierPart = Enterprise.Customs.US.Business.OrgSupplierPart;

	class PerformanceTestCase : TestCase
	{
		public void TestPerformanceForImportingCommercialInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CreateInvoice(declaration, "TSTA" + ZDateTime.Now.ToString("yyMMddhhmmss"), importer, supplier1, true);
			CreateInvoice(declaration, "TSTB" + ZDateTime.Now.ToString("yyMMddhhmmss"), importer, supplier2, false);
			Factory.SaveForTesting();
			var sourceBOManager = declaration.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var writer = sourceBOManager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			Shipment dataObject = null;
			using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
			{
				dataObject = (Shipment)writer.GetDataObject(declaration);
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				dataObject.DataContext = dataContext;
			}
			TestCaseWithFactoryAndMessagingHelpers.GetQueuedUniversalShipmentMessage(Factory, dataObject);
		}

		protected UniversalObjectFactory Factory;
		protected override void SetUp()
		{
			Factory = new UniversalObjectFactory();
			base.SetUp();
			Assert("This should only be run by developer", Globals.IsUserInteractive);
			importer = Factory.Load<OrgHeader>(Enterprise.Customs.US.DataRegistry.Business.USCustomsRegistry.Instance.TestCaseWithSetupImporterPK.Value);
			supplier1 = Factory.Load<OrgHeader>(Enterprise.Customs.US.DataRegistry.Business.USCustomsRegistry.Instance.TestCaseWithSetupShippingLinePK.Value);
			supplier2 = Factory.Load<OrgHeader>(Enterprise.Customs.US.DataRegistry.Business.USCustomsRegistry.Instance.TestCaseWithSetupManufacturerPK.Value);
			startTime = ZDateTime.UtcNow;
			helper = new DataObjectReaderTestHelper(Factory);
		}

		ZDateTime startTime;
		DataObjectReaderTestHelper helper;
		OrgHeader importer;
		OrgHeader supplier1;
		OrgHeader supplier2;

		protected override void TearDown()
		{
			base.TearDown();
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.UniversalDataMessaging);
			query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, startTime);
			query.FetchOnlyFromLocalCache = true;
			var messages = Factory.Load<EDIMessage>(query);
			if (messages.Length > 0)
			{
				foreach (var message in messages)
				{
					message.EM_ApplicationReference = "CreateTestDataForPerformance";
				}
				Factory.SaveForTesting();
			}
		}

		void CreateInvoice(JobDeclaration declaration, ZString invoiceNumber, OrgHeader buyer, OrgHeader supplier, bool delayAddChildPivot)
		{ 
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Buyer = buyer.PK;
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_InvoiceNumber = invoiceNumber;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var parts = CreateParts(invoiceNumber, buyer, supplier, !delayAddChildPivot);
			var partsLength = parts.Length;
			for (int i = 1; i < 501; i++)
			{
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_PartNo = parts[i % partsLength].OP_PartNum;
				invoiceLine.JI_Description = new Random().Next(1000000).ToString() + invoiceLine.JI_Description;
			}
			if (delayAddChildPivot)
			{
				var index = 2;
				foreach (var part in parts)
				{
					AddChildPivots(part.Pivots[0], index++);
				}
			}
			invoice.JobComInvoiceLines.Sort(JobComInvoiceLine.Schema.JI_Description); // causes the system to be in random order for xml export
		}

		USCTariff[] Tariffs
		{
			get
			{
				if (tariffs == null)
				{
					var tariffCodes = new ZString[] {
						"0101100010",
						"0201105010",
						"0301100000",
						"0401100000",
						"0501000000",
						"0601101500",
						"0701100020",
						"0801110000",
						"0901110010",
						"1001100010",
						"1101000010",
						"1201000020",
						"1301100020",
						"1401100000",
						"1501000020",
						"1601002010",
						"1701110500",
						"1801000000",
						"1901100500",
						"2001100000"
					};
					var list = new List<USCTariff>(20);
					foreach (var tariffCode in tariffCodes)
					{
						var tariff = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, tariffCode));
						if (tariff != null)
						{
							list.Add(tariff);
						}
					}
					tariffs = list.ToArray();
					AssertEquals("There should be 20 USCTariffs loaded", 20, tariffs.Length);
				}
				return tariffs;
			}
		}
		USCTariff[] tariffs;

		ZString GetTariffCode(int index)
		{
			return Tariffs[index % 20].UE_Tariff;
		}

		OrgSupplierPart[] CreateParts(ZString partNum, OrgHeader owner, OrgHeader supplier, bool addChildPivot)
		{
			var list = new List<OrgSupplierPart>(60);
			for (int index = 1; index < 61; index++)
			{
				var newPartNumber = partNum + index.ToString();
				var usPart = Factory.New<OrgSupplierPart>();
				usPart.OP_PartNum = partNum + index.ToString();
				usPart.OP_StockKeepingUnit = GetPkgUnit(index);
				usPart.OP_Desc = partNum + " DESC";
				usPart.RelatedOrganisations.AddOrganisationIfNotExist(owner.PK, OrgPartRelation.RelationshipTypes.Owner);
				usPart.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);

				var partUnit = usPart.PartUnits.AddNew();
				partUnit.OF_QuantityInParent = 24m;
				partUnit.OF_ParentPackType = "CTN";
				ZString lookup = usPart.OP_PartNum + new Random().Next(1000000).ToString();

				var importClassification = Factory.New<CusClassification>();
				importClassification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
				importClassification.CC_LookupCode = lookup;
				importClassification.CC_TariffNum = GetTariffCode(index);

				var importPivot = usPart.PivotsForBinding.AddNew();
				importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				importPivot.CI_CC = importClassification.PK;

				if (addChildPivot)
				{
					AddChildPivots(importPivot, index);
				}

				list.Add(usPart);
			}
			return list.ToArray();
		}

		void AddChildPivots(CusClassPartPivot importPivot, int index)
		{
			var importPivotChild1 = importPivot.Children.AddNew();
			importPivotChild1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			importPivotChild1.CI_TariffNum = GetTariffCode(index + 1);

			var importPivotChild2 = importPivot.Children.AddNew();
			importPivotChild2.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			importPivotChild2.CI_TariffNum = GetTariffCode(index + 1);
		}

		ZString GetPkgUnit(int index)
		{
			return PkgUnits[index % PkgUnits.Length];
		}

		string[] PkgUnits
		{
			get { return pkgUnits ?? (pkgUnits = new[] { Core.Constants.PkgUnit.Bag, Core.Constants.PkgUnit.Unit, Core.Constants.PkgUnit.Roll, Core.Constants.PkgUnit.Package, Core.Constants.PkgUnit.Piece }); }
		}
		string[] pkgUnits;
	}
}
#endif
