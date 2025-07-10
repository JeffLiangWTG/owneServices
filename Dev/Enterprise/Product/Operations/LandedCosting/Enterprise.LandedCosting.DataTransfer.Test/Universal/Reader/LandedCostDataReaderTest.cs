using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.LandedCosting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.ZArchitecture.Schema;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.LandedCosting.DataTransfer.Universal.Testing
{
	sealed class LandedCostDataReaderTest : OrganizationAddressTestHelper
	{
		public void TestBasicLandCostInputFieldMappings()
		{
			var header = Factory.New<LandedCostHeader>();
			header.SynchroniseAll(Declaration as ILandedCostHeader);
			var costInput = header.CostInputs.AddNew();
			costInput.LI_AC_ChargeCode = ChargeCode1.PK;
			costInput.LI_ParentID = InvoiceLine.PK;
			costInput.LI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			var transportLogisticsCostDataObject = SetupTransportLogisticsCost();
			var costInputBO = new LandCostInputDataObjectReader(transportLogisticsCostDataObject, logger, Factory, costInput).ReadIntoBusinessObject();
			AssertLandCostInputContents(costInputBO, InvoiceLine as BusinessObject);

			costInput.LI_AC_ChargeCode = ChargeCode2.PK;
			transportLogisticsCostDataObject = SetupTransportLogisticsCost2();
			costInputBO = new LandCostInputDataObjectReader(transportLogisticsCostDataObject, logger, Factory, costInput).ReadIntoBusinessObject();
			AssertLandCostInputContents2(costInputBO, InvoiceLine as BusinessObject);
		}

		public void TestILandedCostDataReaderImplementation()
		{
			Declaration.JE_EntrySubmittedDate = ZDateTime.BrettsBirthday;
			var dataReader = ObjectFactory.New<ILandedCostDataReader>(logger, Factory, Declaration);
			AssertEquals(typeof(LandedCostDataReader), dataReader.GetType());
			AssertNull("No landed costing creation", dataReader.ReadIntoBusinessObject());
			var invoiceData = new CommercialInvoiceHeader();
			dataReader.CollectTransportLogisticsCost(Invoice as BusinessObject, invoiceData);
			AssertNull("No landed costing creation", dataReader.ReadIntoBusinessObject());
			invoiceData.TransportLogisticsCostCollection = new List<TransportLogisticsCost>();
			dataReader.CollectTransportLogisticsCost(null, invoiceData);
			AssertNull("No landed costing creation", dataReader.ReadIntoBusinessObject());
			dataReader.CollectTransportLogisticsCost(Invoice as BusinessObject, invoiceData);
			var header = (LandedCostHeader)dataReader.ReadIntoBusinessObject();
			AssertNotNull("Landed costing created", header);

			AssertEquals("header.LT_ParentID", Declaration.PK, header.LT_ParentID);
			AssertEquals("header.LT_ParentTableCode", JobDeclarationSchema.Constants.Prefix, header.LT_ParentTableCode);
			AssertEquals("header.LT_GC", GlbCompany.CurrentCompany.PK, header.LT_GC);
			AssertEquals("header.LT_DateOfEntry", ZDateTime.BrettsBirthday, header.LT_DateOfEntry);
			AssertEquals("header.LT_LandedCostType", LandedCostType.Actual, header.LT_LandedCostType);
			AssertEquals("header.CostInputs.Count", 0, header.CostInputs.Count);
			AssertEquals("header.Histories.Count", 0, header.Histories.Count);
			invoiceData.TransportLogisticsCostCollection.Add(SetupTransportLogisticsCost());
			dataReader = ObjectFactory.New<ILandedCostDataReader>(logger, Factory, Declaration);
			dataReader.CollectTransportLogisticsCost(Invoice as BusinessObject, invoiceData);
			AssertEquals(header, dataReader.ReadIntoBusinessObject());
			AssertNotNull("Landed costing created", header);
			AssertEquals("header.CostInputs.Count", 1, header.CostInputs.Count);
			AssertLandCostInputContents(header.CostInputs[0], Invoice as BusinessObject);
			AssertEquals("header.Histories.Count", 0, header.Histories.Count);
			var invoiceLineData = new CommercialInvoiceLine()
			{
				LandedCostDetail = new LandedCostDetail(DefaultDataObjectWriterStrategy.TestInstance) { MarkUp1 = 10m },
				TransportLogisticsCostCollection = new List<TransportLogisticsCost>(new[] { SetupTransportLogisticsCost2() })
			};
			dataReader.CollectTransportLogisticsCost(InvoiceLine as BusinessObject, invoiceLineData);
			AssertEquals(header, dataReader.ReadIntoBusinessObject());
			AssertNotNull("Landed costing created", header);
			AssertEquals("header.CostInputs.Count", 2, header.CostInputs.Count);
			var costInput1 = header.CostInputs[0];
			var costInput2 = header.CostInputs[1];
			AssertLandCostInputContents(costInput1, Invoice as BusinessObject);
			AssertLandCostInputContents2(costInput2, InvoiceLine as BusinessObject);
			AssertEquals("header.Histories.Count", 0, header.Histories.Count);
			costInput1.LI_ChargeDescription = "BOB 1";
			costInput1.LI_CostAmount = 123.32m;
			costInput2.LI_CostAmount = 123.32m;
			Factory.SaveAtEndOfImport(logger);
			Factory.FireCleanupAfterSaving();
			var factory = new UniversalObjectFactory();
			var declaration = factory.BOFactory.Load<Integration.Customs.US.IJobDeclaration>(Declaration.PK);
			dataReader = ObjectFactory.New<ILandedCostDataReader>(logger, factory, declaration);
			var invoice = factory.BOFactory.Load<Integration.Customs.US.IJobComInvoiceHeader>(Invoice.PK);
			dataReader.CollectTransportLogisticsCost(invoice as BusinessObject, invoiceData);
			var invoiceLine = factory.BOFactory.Load<Integration.Customs.US.IJobComInvoiceLine>(InvoiceLine.PK);
			dataReader.CollectTransportLogisticsCost(invoiceLine as BusinessObject, invoiceLineData);
			header = (LandedCostHeader)dataReader.ReadIntoBusinessObject();
			AssertNotNull("Landed costing created", header);
			AssertEquals("header.LT_ParentID", Declaration.PK, header.LT_ParentID);
			AssertEquals("header.LT_ParentTableCode", JobDeclarationSchema.Constants.Prefix, header.LT_ParentTableCode);
			AssertEquals("header.LT_GC", GlbCompany.CurrentCompany.PK, header.LT_GC);
			AssertEquals("header.LT_DateOfEntry", ZDateTime.BrettsBirthday, header.LT_DateOfEntry);
			AssertEquals("header.LT_LandedCostType", LandedCostType.Actual, header.LT_LandedCostType);
			factory.FireCleanupAfterSaving();
			AssertEquals("header.CostInputs.Count", 3, header.CostInputs.Count);
			costInput1 = (LandCostInput)header.CostInputs.FindByPK(costInput1.PK);
			costInput2 = (LandCostInput)header.CostInputs.FindByPK(costInput2.PK);
			var costInput3 = (LandCostInput)header.CostInputs.First(x => x.PK != costInput1.PK && x.PK != costInput2.PK);
			AssertLandCostInputContents(costInput1, invoice as BusinessObject, costInput3.LI_AC_ChargeCode, "BOB 1", 123.32m, costInput3.LI_RX_NKCostCurrency, costInput3.LI_DistributeCostBy, costInput3.LI_LandedCostGroup, costInput3.LI_ServiceExRate);
			AssertLandCostInputContents2(costInput2, invoiceLine as BusinessObject);
			AssertLandCostInputContents(costInput3, invoice as BusinessObject);
		}

		public void TestHandlingInvalidData()
		{
			var dataReader = ObjectFactory.New<ILandedCostDataReader>(logger, Factory, Declaration);
			AssertEquals(typeof(LandedCostDataReader), dataReader.GetType());
			var invoiceData = new CommercialInvoiceHeader()
			{
				TransportLogisticsCostCollection = new List<TransportLogisticsCost>(new[] { new TransportLogisticsCost() { ChargeCode = new ChargeCode() { Code = "BO@" }, ChargeDescription = "HELLO WORLD" } })
			};
			dataReader.CollectTransportLogisticsCost(Invoice as BusinessObject, invoiceData);
			var landedCostHeader = dataReader.ReadIntoBusinessObject();
			AssertNotNull("Landed costing created", landedCostHeader);
			AssertContains(string.Format("Error - Transport Logistics Cost ChargeCode 'BO@' does not exists for Company '{0}'", CurrentCompany.GC_Name), logger.Logs);
			var inputCosts = Factory.BOFactory.Load<Integration.LandedCosting.ILandCostInput>(new ZQuery(LandCostInputSchema.LI_LT, landedCostHeader.PK));
			AssertEquals("inputCosts.Length", 1, inputCosts.Length);
			AssertEquals("inputCosts[0].LI_ChargeDescription", "HELLO WORLD", inputCosts[0].LI_ChargeDescription);
			Factory.FireCleanupAfterSaving();
		}

		public void TestHandlingMutex()
		{
			AssertNotNull("Precodition", InvoiceLine);
			Factory.SaveForTesting();
			var invoiceData = new CommercialInvoiceHeader()
			{
				TransportLogisticsCostCollection = new List<TransportLogisticsCost>(new[] { SetupTransportLogisticsCost() })
			};

			var dataReader = ObjectFactory.New<ILandedCostDataReader>(logger, Factory, Declaration);
			AssertEquals(typeof(LandedCostDataReader), dataReader.GetType());
			dataReader.CollectTransportLogisticsCost(Invoice as BusinessObject, invoiceData);

			var newFactory = new BusinessObjectFactory();
			var decInDiffFactory = newFactory.Load<Integration.Customs.US.IJobDeclaration>(Declaration.PK);
			var expectedMessage = string.Format("Could not create a new landed costing; someone else is already in the process of creating a landed costing for job ({0}).", decInDiffFactory.JE_DeclarationReference);
			var hostEntity = decInDiffFactory as ILandedCostHeader;
			LandedCostHeader header = null;
			try
			{
				using (var mutex = hostEntity.GetLandedCostMutex())
				{
					AssertEquals("mutex should be able to lock", true, mutex.Lock());
					AssertExceptionThrown(typeof(InvalidOperationException), expectedMessage, () =>
					{
						header = (LandedCostHeader)dataReader.ReadIntoBusinessObject();
					});
				}
				header = (LandedCostHeader)dataReader.ReadIntoBusinessObject();
				AssertNotNull("Landed costing created", header);
				AssertEquals("header.CostInputs.Count", 1, header.CostInputs.Count);
				AssertLandCostInputContents(header.CostInputs[0], Invoice as BusinessObject);
				AssertEquals("header.Histories.Count", 0, header.Histories.Count);
			}
			finally
			{
				Factory.FireCleanupAfterSaving();
			}
		}

		TestErrorLogger logger;
		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();
		}

		AccChargeCode chargeCode1;
		AccChargeCode ChargeCode1 => chargeCode1 ?? (chargeCode1 = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, CurrentCompany.PK)));

		AccChargeCode chargeCode2;
		AccChargeCode ChargeCode2
		{
			get
			{
				if (chargeCode2 == null)
				{
					var query = new ZQuery(AccChargeCodeSchema.AC_GC, CurrentCompany.PK);
					query.AddToFilter(AccChargeCodeSchema.PK, SQLComparisonOperator.NotEqual, ChargeCode1.PK);
					chargeCode2 = Factory.LoadTop1<AccChargeCode>(query);
				}
				return chargeCode2;
			}
		}

		TransportLogisticsCost SetupTransportLogisticsCost()
		{
			return SetupTransportLogisticsCost(new ChargeCode() { Code = ChargeCode1.AC_Code }
			, "BOB THE BUILDER"
			, 1000m
			, new Currency() { Code = Core.Constants.CurrencyCodes.Australia }
			, new CodeDescriptionPair() { Code = CostDistributionMechanismList.Codes.Item }
			, new CodeDescriptionPair() { Code = "1" }
			, 0.9834m);
		}

		TransportLogisticsCost SetupTransportLogisticsCost2()
		{
			return SetupTransportLogisticsCost(new ChargeCode() { Code = ChargeCode2.AC_Code }
			, "WENDY THE DESTROYER"
			, 2000m
			, new Currency() { Code = Core.Constants.CurrencyCodes.Singapore }
			, new CodeDescriptionPair() { Code = CostDistributionMechanismList.Codes.LineValue }
			, new CodeDescriptionPair() { Code = "2" }
			, 1.1245m);
		}

		TransportLogisticsCost SetupTransportLogisticsCost(ChargeCode chargeCode, ZString? chargeDescription, ZDecimal? costAmount, Currency costCurrency, CodeDescriptionPair distributeCostBy, CodeDescriptionPair landedCostGroup, ZDecimal? serviceExRate)
		{
			return new TransportLogisticsCost()
			{
				ChargeCode = chargeCode,
				ChargeDescription = chargeDescription,
				CostAmount = costAmount,
				CostCurrency = costCurrency,
				DistributeCostBy = distributeCostBy,
				LandedCostGroup = landedCostGroup,
				ServiceExRate = serviceExRate
			};
		}

		void AssertLandCostInputContents(LandCostInput costInputBO, BusinessObject parent)
		{
			AssertLandCostInputContents(costInputBO
				, parent
				, ChargeCode1.PK
				, "BOB THE BUILDER"
				, 1000m
				, Core.Constants.CurrencyCodes.Australia
				, CostDistributionMechanismList.Codes.Item
				, 1, 0.9834m);
		}

		void AssertLandCostInputContents2(LandCostInput costInputBO, BusinessObject parent)
		{
			AssertLandCostInputContents(costInputBO
				, parent
				, ChargeCode2.PK
				, "WENDY THE DESTROYER"
				, 2000m
				, Core.Constants.CurrencyCodes.Singapore
				, CostDistributionMechanismList.Codes.LineValue
				, 2
				, 1.1245m);
		}

		void AssertLandCostInputContents(LandCostInput costInputBO, BusinessObject parent, ZGuid chargeCodePK, ZString chargeDescription, ZDecimal costAmount, ZString costCurrency, ZString distributeCostBy, ZByte landedCostGroup, ZDecimal serviceExRate)
		{
			AssertEquals("costInputBO.LI_ParentID", parent.PK, costInputBO.LI_ParentID);
			AssertEquals("costInputBO.LI_ParentTableCode", parent.TablePrefix, costInputBO.LI_ParentTableCode);
			AssertEquals("costInputBO.LI_AC_ChargeCode", chargeCodePK, costInputBO.LI_AC_ChargeCode);
			AssertEquals("costInputBO.LI_ChargeDescription", chargeDescription, costInputBO.LI_ChargeDescription);
			AssertEquals("costInputBO.LI_CostAmount", costAmount, costInputBO.LI_CostAmount);
			AssertEquals("costInputBO.LI_RX_NKCostCurrency", costCurrency, costInputBO.LI_RX_NKCostCurrency);
			AssertEquals("costInputBO.LI_DistributeCostBy", distributeCostBy, costInputBO.LI_DistributeCostBy);
			AssertEquals("costInputBO.LI_LandedCostGroup", landedCostGroup, costInputBO.LI_LandedCostGroup);
			AssertEquals("costInputBO.LI_ServiceExRate", serviceExRate, costInputBO.LI_ServiceExRate);
		}

		Integration.Customs.US.IJobDeclaration declaration;
		Integration.Customs.US.IJobDeclaration Declaration => declaration ?? (declaration = Factory.BOFactory.New<Integration.Customs.US.IJobDeclaration>());

		Integration.Customs.US.IJobComInvoiceGroupHeader groupHeader;
		Integration.Customs.US.IJobComInvoiceGroupHeader GroupHeader
		{
			get
			{
				if (groupHeader == null)
				{
					groupHeader = Factory.BOFactory.New<Integration.Customs.US.IJobComInvoiceGroupHeader>();
					groupHeader.JZ_JE = Declaration.PK;
				}
				return groupHeader;
			}
		}

		Integration.Customs.US.IJobComInvoiceHeader invoice;
		Integration.Customs.US.IJobComInvoiceHeader Invoice
		{
			get
			{
				if (invoice == null)
				{
					invoice = Factory.BOFactory.New<Integration.Customs.US.IJobComInvoiceHeader>();
					invoice.JZ_JZ_GroupInvoiceFK = GroupHeader.PK;
					invoice.JZ_JE = Declaration.PK;
					invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				}
				return invoice;
			}
		}

		Integration.Customs.US.IJobComInvoiceLine invoiceLine;
		Integration.Customs.US.IJobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					invoiceLine = Factory.BOFactory.New<Integration.Customs.US.IJobComInvoiceLine>();
					invoiceLine.JI_JZ = Invoice.PK;
					invoiceLine.JI_LinePrice = 1000m;
					invoiceLine.JI_InvoiceQuantity = 10m;
				}
				return invoiceLine;
			}
		}

		GlbCompany currentCompany;
		GlbCompany CurrentCompany => currentCompany ?? (currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
	}
}
