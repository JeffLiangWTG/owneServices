using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5167;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class GovernmentAgencyGoodsItemTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSequenceNumeric()
		{
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.SequenceNumeric, NUnit.Framework.Is.EqualTo(ZInt.Zero));
		}

		[ExpectNoExceptions]
		public void TestCommodity()
		{
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Commodity, NUnit.Framework.Is.EqualTo(default(ICommodity)));
		}

		[ExpectNoExceptions]
		public void TestAdditionalDocuments()
		{
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.AdditionalDocuments, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IAdditionalDocument>)));
		}

		[ExpectNoExceptions]
		public void TestAdditionalInformations()
		{
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.AdditionalInformations, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IAdditionalInformation>)));
		}

		[ExpectNoExceptions]
		public void TestGoodsMeasure()
		{
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.GoodsMeasure, NUnit.Framework.Is.EqualTo(default(IGoodsMeasure)));
		}

		[ExpectNoExceptions]
		public void TestManufacturer()
		{
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Manufacturer, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
		}

		[ExpectNoExceptions]
		public void TestOrigin()
		{
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Origin, NUnit.Framework.Is.EqualTo(default(IOrigin)));
		}

		[ExpectNoExceptions]
		public void TestPackaging()
		{
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Packaging, NUnit.Framework.Is.EqualTo(default(IPackaging)));
		}

		[ExpectNoExceptions]
		public void TestPreviousDocument()
		{
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.PreviousDocument, NUnit.Framework.Is.EqualTo(default(IPreviousDocument)));
		}

		[ExpectNoExceptions]
		public void TestApprovalDocument()
		{
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.ApprovalDocument, NUnit.Framework.Is.EqualTo(default(ILPCODetail)));
		}

		[ExpectNoExceptions]
		public void TestCommoditySpecification()
		{
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.CommoditySpecification, NUnit.Framework.Is.EqualTo(default(ICommoditySpecification)));
		}

		[ExpectNoExceptions]
		public void TestGoodsLicensingStatisticalMeasure()
		{
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.GoodsLicensingStatisticalMeasure, NUnit.Framework.Is.EqualTo(default(IGoodsLicensingStatisticalMeasure)));
		}

		[ExpectNoExceptions]
		public void TestGoodsStatisticalMeasure()
		{
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.GoodsStatisticalMeasure, NUnit.Framework.Is.EqualTo(default(IGoodsStatisticalMeasure)));
		}

		[ExpectNoExceptions]
		public void TestMedicalInstrument()
		{
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.MedicalInstrument, NUnit.Framework.Is.EqualTo(default(ILPCODetail)));
		}

		[ExpectNoExceptions]
		public void TestPreBondedDocument()
		{
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.PreBondedDocument, NUnit.Framework.Is.EqualTo(default(IPreviousDocument)));
		}

		[ExpectNoExceptions]
		public void TestShippingIdentifications()
		{
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.ShippingIdentifications, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IShippingIdentification>)));
		}

		[ExpectNoExceptions]
		public void TestGovernmentProcedure()
		{
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.GovernmentProcedure, NUnit.Framework.Is.EqualTo(default(IGovernmentProcedure)));
		}

		[ExpectNoExceptions]
		public void TestControlInspectionStartDateTime()
		{
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.ControlInspectionStartDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.BrettsBirthday));
		}

		[ExpectNoExceptions]
		public void TestExaminationPlace()
		{
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.ExaminationPlace, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTransportEquipments()
		{
			var transportEquipments = governmentAgencyGoodsItem.TransportEquipments;
			NUnit.Framework.Assert.That(transportEquipments, NUnit.Framework.Is.Not.EqualTo(default(System.Collections.Generic.IEnumerable<ITransportEquipment>)));
			NUnit.Framework.Assert.That(transportEquipments.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(transportEquipments.First(), NUnit.Framework.Is.TypeOf(typeof(TransportEquipmentWrapper)));
			invoiceLine2.ContainersPivot.RemoveAndDeleteAll();
			invoiceLine2.ContainersPivot.AddPivotFor(container1);
			NUnit.Framework.Assert.That(transportEquipments.Count(), NUnit.Framework.Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestAdditionalDeclaration()
		{
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.AdditionalDeclaration, NUnit.Framework.Is.TypeOf(typeof(AdditionalDeclaration)));
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.AdditionalDeclaration.ID, NUnit.Framework.Is.EqualTo("123456").Using(CustomComparers.TypeComparison));
			entryHeader.EntryNumber = "";
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.AdditionalDeclaration.ID, NUnit.Framework.Is.EqualTo(MessageConstants.EntryNumberPlaceHolder).Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var classification = Factory.New<Customs.Business.BaseCusClassification>();
			classification.CC_Description = "CUCKOO SQUEAKERS";
			classification.CC_LookupCode = "CKSQKS";
			classification.CC_TariffNum = "0000.00.00.00Y";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			var entryInstruction = declaration.CusEntryInstruction;
			var service = declaration.DocsAndCartage.Services.AddNew();
			service.ES_ServiceCode = ServiceTypes.CommodityInspection;
			service.ES_ServiceNote = "123456";
			service.ES_Booked = ZDateTime.BrettsBirthday;
			service.ES_SubLocation = "123";
			container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "UUUU1234567";
			var refContainer1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container1.CO_RC = refContainer1.PK;
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "UUUU1234568";
			var refContainer2 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container2.CO_RC = refContainer2.PK;
			var invoice1 = declaration.Invoices.AddNew();
			invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CC = classification.PK;
			invoiceLine1.ContainersPivot.AddPivotFor(container1);
			invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CC = classification.PK;
			invoiceLine2.ContainersPivot.AddPivotFor(container2);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.EntryNumber = "123456";
			entryHeader.CH_Status = "AWO";
			governmentAgencyGoodsItem = new GovernmentAgencyGoodsItem(entryHeader);
		}

		IGovernmentAgencyGoodsItem governmentAgencyGoodsItem;
		JobComInvoiceLine invoiceLine1;
		JobComInvoiceLine invoiceLine2;
		CusContainer container1;
		CusEntryHeader entryHeader;
	}
}
