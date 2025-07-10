using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterpise.Customs.TW.DataTransfer.Testing
{
	public sealed class DeclarationDataObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		[ExpectNoExceptions]
		public void TestDataObjectWriter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Taiwan))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var writer = new TWJobDeclarationDataObjectWriterForTest(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())));
				writer.GetDataObject(declaration);
				writer.GetDataObject(declaration);
				NUnit.Framework.Assert.That(writer.GetCreateNewUniversalDataObjectWriterHelper(declaration), Is.TypeOf<TWDataObjectWriterHelper>());
				NUnit.Framework.Assert.That(writer.GetNewCustomsEntryInstructionDataObjectWriterForTest(), Is.TypeOf<TWEntryInstructionDataObjectWriter>());
				NUnit.Framework.Assert.That(writer.GetNewCommercialInvoiceHeaderDataObjectWriterForTest(entryHeader), Is.TypeOf<TWInvoiceHeaderDataObjectWriter>());
				var hints = writer.GetEntryInstructionRelatedFetchHintsForTest(declaration);
				NUnit.Framework.Assert.That(hints.Count(), Is.EqualTo(1));
				NUnit.Framework.Assert.That(hints.Any(x => x.TableName == "StmNote"), Is.True);
			}
		}

		[ExpectNoExceptions]
		public void TestPopulateExtraOrganisation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "DEVELOPER COMPANY";
			var addr = org.Addresses.AddNew();
			addr.Address1 = "Address 1";
			addr.Address2 = "Address 2";
			addr.OA_Code = "ADDR_ONE";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "DEVELOPER COMPANY1";
			var addr1 = org1.Addresses.AddNew();
			addr1.Address1 = "Address 3";
			addr1.Address2 = "Address 4";
			addr1.OA_Code = "ADDR_ONE1";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Taiwan))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_OH_NotifyParty = org1.PK;
				var bondedFactory = declaration.BondedFactories.AddNew();
				bondedFactory.OrganisationPK = org.PK;
				bondedFactory.E2_OA_Address = addr.PK;
				var writer = new TWJobDeclarationDataObjectWriterForTest(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())));
				var data = writer.GetDataObject(declaration);
				NUnit.Framework.Assert.That(data.OrganizationAddressCollection.Count, Is.EqualTo(3));
				var result = data.OrganizationAddressCollection.FirstOrDefault();
				result = data.OrganizationAddressCollection.ElementAt(2);
				NUnit.Framework.Assert.That(result.AddressType, Is.EqualTo("NotifyParty").Using(CustomComparers.TypeComparison));
			}
		}

		[ExpectNoExceptions]
		public void TestPopulateAdditionalInfoForAdditionalBill()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Taiwan))
			{
				var declaration = Factory.New<JobDeclaration>();
				var mb1 = declaration.Bills.AddNew();
				mb1.CU_BillNum = "MB1";
				mb1.CU_BillType = Enterprise.Customs.TW.Business.BillTypeList.Codes.ContainerNote;
				mb1.CU_NoOfPacks = 10m;
				var mb1hb1 = mb1.ChildBills.AddNew();
				mb1hb1.CU_BillNum = "HB1";
				mb1hb1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
				mb1hb1.CU_NoOfPacks = 10m;
				var mb1hb1sb1 = mb1hb1.ChildBills.AddNew();
				mb1hb1sb1.CU_BillNum = "SB1";
				mb1hb1sb1.CU_NoOfPacks = 10m;
				var mb2 = declaration.Bills.AddNew();
				mb2.CU_BillNum = "MB2";
				mb2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
				mb2.CU_NoOfPacks = 15m;
				var mb2hb1 = mb2.ChildBills.AddNew();
				mb2hb1.CU_BillNum = "HB1";
				mb2hb1.CU_NoOfPacks = 15m;
				var mb2hb1sb1 = mb2hb1.ChildBills.AddNew();
				mb2hb1sb1.CU_BillNum = "SB1";
				mb2hb1sb1.CU_NoOfPacks = 15m;
				var writer = new TWJobDeclarationDataObjectWriterForTest(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())));
				var declarationData = writer.GetDataObject(declaration);
				NUnit.Framework.Assert.That(declarationData.AdditionalBillCollection.Count, Is.EqualTo(6));
				var mb1Data = declarationData.AdditionalBillCollection.First(x => x.BillNumber.Value == "MB1" && x.BillType.Code.Value == "CNN" && x.NoOfPacks == 10m && x.AddInfoCollection == null);
				var mb1hb1Data = declarationData.AdditionalBillCollection.First(x => x.BillNumber.Value == "HB1" && x.BillType.Code.Value == WayBillTypeList.Codes.House && x.NoOfPacks == 10m && x.ParentBillNumber.Value == "MB1" && x.AddInfoCollection == null);
				var mb1hb1sb1Data = declarationData.AdditionalBillCollection.First(x => x.BillNumber.Value == "SB1" && x.BillType.Code.Value == WayBillTypeList.Codes.SubHouse && x.NoOfPacks == 10m && x.ParentBillNumber.Value == "HB1" && x.AddInfoCollection.GetZStringValue(Enterprise.Customs.DataTransfer.Universal.Constants.AddInfoKeys.AdditionalBill.ParentMasterBillNumber).Value == "MB1");
				var mb2Data = declarationData.AdditionalBillCollection.First(x => x.BillNumber.Value == "MB2" && x.BillType.Code.Value == WayBillTypeList.Codes.Master && x.NoOfPacks == 15m && x.AddInfoCollection == null);
				var mb2hb1Data = declarationData.AdditionalBillCollection.First(x => x.BillNumber.Value == "HB1" && x.BillType.Code.Value == WayBillTypeList.Codes.House && x.NoOfPacks == 15m && x.ParentBillNumber.Value == "MB2" && x.AddInfoCollection == null);
				var mb2hb1sb1Data = declarationData.AdditionalBillCollection.First(x => x.BillNumber.Value == "SB1" && x.BillType.Code.Value == WayBillTypeList.Codes.SubHouse && x.NoOfPacks == 15m && x.ParentBillNumber.Value == "HB1" && x.AddInfoCollection.GetZStringValue(Enterprise.Customs.DataTransfer.Universal.Constants.AddInfoKeys.AdditionalBill.ParentMasterBillNumber).Value == "MB2");
			}
		}

		[ExpectNoExceptions]
		public void TestPopulateEntryInstructionAddInfoGroups()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "DEVELOPER COMPANY";
			var addr = org.Addresses.AddNew();
			addr.Address1 = "Address 1";
			addr.Address2 = "Address 2";
			addr.OA_Code = "ADDR_ONE";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Taiwan))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
				var entryInstruction = declaration.CusEntryInstruction;
				var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
				controllingMessageHeader.TW1_ControllingMessageType = "A";
				controllingMessageHeader.TW1_FunctionalReferenceId = "1";
				controllingMessageHeader.PermitNumber = "A1";
				controllingMessageHeader.TW1_ControllingAgency = "A";
				controllingMessageHeader.TW1_BusinessType = "A";
				controllingMessageHeader.TW1_ProcessingUnit = "KG";
				controllingMessageHeader.TW1_PaymentMethod = "1";
				controllingMessageHeader.TW1_ProofOfPaper = true;
				controllingMessageHeader.TW1_ElectronicReceipt = true;
				controllingMessageHeader.TW1_AppointmentDate = new ZDate(2019, 02, 12);
				controllingMessageHeader.TW1_AppointmentPeriod = "A";
				controllingMessageHeader.TW1_RequestDescription = "AAA";
				var controllingMessageHeader1 = entryInstruction.ControllingMessageHeaders.AddNew();
				controllingMessageHeader1.TW1_ControllingMessageType = "B";
				controllingMessageHeader1.TW1_FunctionalReferenceId = "2";
				controllingMessageHeader1.PermitNumber = "B1";
				controllingMessageHeader1.TW1_ControllingAgency = "B";
				controllingMessageHeader1.TW1_BusinessType = "B";
				controllingMessageHeader1.TW1_ProcessingUnit = "KG";
				controllingMessageHeader1.TW1_PaymentMethod = "2";
				controllingMessageHeader1.TW1_ProofOfPaper = false;
				controllingMessageHeader1.TW1_ElectronicReceipt = false;
				controllingMessageHeader1.TW1_AppointmentDate = new ZDate(2019, 02, 13);
				controllingMessageHeader1.TW1_AppointmentPeriod = "";
				controllingMessageHeader1.TW1_RequestDescription = "BBB";
				var writer = new TWJobDeclarationDataObjectWriterForTest(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())));
				var data = writer.GetDataObject(declaration);
				var result = data.EntryInstructionCollection[0];
				NUnit.Framework.Assert.That(result.AddInfoGroupCollection.Count, Is.EqualTo(2));
				writer = new TWJobDeclarationDataObjectWriterForTest(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())));
				writer.SetCAHeaderPKsToPopulate(new List<ZGuid> { controllingMessageHeader1.PK });
				data = writer.GetDataObject(declaration);
				result = data.EntryInstructionCollection[0];
				NUnit.Framework.Assert.That(result.AddInfoGroupCollection.Count, Is.EqualTo(1));
			}
		}

		[ExpectNoExceptions]
		public void TestPopulateDataWithControllingMessageHeaderLink()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Taiwan))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var declaration = Factory.New<JobDeclaration>();
				declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
				var entryInstruction = declaration.CusEntryInstruction;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_MarksAndNumbers = "AA";
				var invoiceLine = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
				invoiceLine.JI_CEI = entryInstruction.PK;
				var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
				var controllingMessageHeader = controllingMessageHeaders.AddNew();
				controllingMessageHeader.TW1_ControllingAgency = "20";
				controllingMessageHeader.TW1_FunctionalReferenceId = "XX01";
				controllingMessageHeader.PermitNumber = "110";
				controllingMessageHeader = controllingMessageHeaders.AddNew();
				controllingMessageHeader.TW1_ControllingAgency = "DN";
				controllingMessageHeader.TW1_FunctionalReferenceId = "XX02";
				controllingMessageHeader.PermitNumber = "220";
				var invoiceLineLinkControllingMsgHeaders = invoiceLine.InvoiceLineLinkControllingMsgHeaders;
				NUnit.Framework.Assert.That(invoiceLineLinkControllingMsgHeaders.Count, Is.EqualTo(2));
				invoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingAgency == "DN").IsLinkedCMHeader = true;
				var writer = new TWJobDeclarationDataObjectWriterForTest(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())));
				var data = writer.GetDataObject(declaration);
				var commercialInvoice = data.CommercialInfo.CommercialInvoiceCollection[0];
				var commercialEntryInstruction = data.EntryInstructionCollection[0];
				var commercialInvoiceLine = commercialInvoice.CommercialInvoiceLineCollection[0];
				NUnit.Framework.Assert.That(commercialInvoiceLine.AddInfoGroupCollection.Any(group => group.Type.Code == new ZString?("CML") && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key == new ZString?("ControllingMessageLink") && innerAddinfo.Value == new ZString?("2"))), Is.True);
				NUnit.Framework.Assert.That(commercialEntryInstruction.AddInfoGroupCollection.Any(group => group.Type.Code == new ZString?("CM") && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key == new ZString?("ControllingAgency") && innerAddinfo.Value == new ZString?("DN")) && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key == new ZString?("Link") && innerAddinfo.Value == new ZString?("2"))), Is.True);
			}
		}

		[ExpectNoExceptions]
		public void TestPopulateUCRNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Taiwan))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
				var entryInstruction = declaration.CusEntryInstruction;
				entryInstruction.UCRNumber = "UCRNumber1";

				var writer = new TWJobDeclarationDataObjectWriterForTest(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())));
				var data = writer.GetDataObject(declaration);
				var result = data.EntryNumberCollection.Where(x => x.Number.HasValue && x.Number.Value == "UCRNumber1");
				CombineAssertions(() =>
				{
					NUnit.Framework.Assert.That(result.Count(), Is.EqualTo(1));
					NUnit.Framework.Assert.That(result.First().Type.Code, Is.EqualTo(CusEntryNumberTypes.Standard.UniqueConsignementReference).Using(CustomComparers.TypeComparison));
				});
			}
		}

		class TWJobDeclarationDataObjectWriterForTest : TWJobDeclarationDataObjectWriter
		{
			internal TWJobDeclarationDataObjectWriterForTest(IDataWritingManager manager) : base(manager)
			{
			}

			public CustomsEntryInstructionDataObjectWriter GetNewCustomsEntryInstructionDataObjectWriterForTest()
			{
				return GetNewCustomsEntryInstructionDataObjectWriter();
			}

			public UniversalDataObjectWriterHelper GetCreateNewUniversalDataObjectWriterHelper(BaseJobDeclaration declarationBO)
			{
				return CreateNewUniversalDataObjectWriterHelper(declarationBO);
			}

			public IEnumerable<IFetchHint> GetEntryInstructionRelatedFetchHintsForTest(JobDeclaration declaration)
			{
				var row = (IColumnIndexer)((IBusinessObjectInternals)declaration.CusEntryInstruction).Row;
				return GetEntryInstructionRelatedFetchHints(row);
			}

			public CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriterForTest(Enterprise.Customs.Business.CusEntryHeader relatedEntry)
			{
				return GetNewCommercialInvoiceHeaderDataObjectWriter(relatedEntry);
			}
		}

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			base.SetUp();
		}

		IDisposable setupCreator;
		protected override void TearDown()
		{
			base.TearDown();
			if (setupCreator != null)
			{
				setupCreator.Dispose();
				setupCreator = null;
			}
		}
	}
}
