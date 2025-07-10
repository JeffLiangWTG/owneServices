using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	public class TestECIWriteOffCreator : TestDeclarationCreator
	{
		public TestECIWriteOffCreator(JobDeclaration declaration)
			: base(declaration)
		{
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
		}

		public new JobDeclaration Declaration
		{
			get { return base.Declaration; }
		}

		public void SetupTestForECIWriteoffWithConsignmentDetails()
		{
			Declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.NotSentToCustoms;
			Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.NotSentToCustoms;
			Declaration.JE_ECI_LastResponseStatus = LowValueConsignmentStatusList.Codes.NoStatusReported;

			SetupTestConsignmentDetails();
		}

		public void SetupTestForCREWriteoffWithConsignmentDetails()
		{
			Declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.WriteOff;
			Declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.NotSentToCustoms;
			Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.NotSentToCustoms;
			Declaration.JE_ECI_LastResponseStatus = LowValueConsignmentStatusList.Codes.NoStatusReported;

			SetupTestConsignmentDetails();
		}

		public override void SetupTestContainer()
		{
			SetupTestContainer("OOCL0000006", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 20m, Declaration.JE_TotalNoOfPacks, Declaration.JE_TotalNoOfPacksPackType);
		}

		public CusContainer SetupTestContainer(ZString containerNumber, ZString containerMode, ZString containerSize, ZDecimal weightInKgs)
		{
			CusContainer container = Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = containerNumber;
			container.CO_FCL_LCL_AIR = containerMode;
			container.CO_ContainerSize = containerSize;
			container.CO_Weight = weightInKgs;
			return container;
		}

		public CusContainer SetupTestContainer(ZString containerNumber, ZString containerMode, ZString containerSize, ZDecimal weightInKgs, ZInt packQty, ZString packType)
		{
			CusContainer container = SetupTestContainer(containerNumber, containerMode, containerSize, weightInKgs);

			PackingGroup packingGroup = Declaration.Bills[0].PackingGroups.AddNew();
			packingGroup.CR_CO_Container = container.PK;

			Package package = packingGroup.Packages.AddNew();
			package.CW_PackQty = packQty;
			package.CW_PackType = packType;

			return container;
		}

		public void SetupTestCommercialInvoiceHeaderFOB100NZD()
		{
			Declaration.JE_ECI_InvoiceAmount = 100.00m;
			Declaration.JE_ECI_InvoiceCurrency = RefCurrencyNZD.PK;
		}

		public void SetupTestCommercialInvoiceHeaderFOB(ZDecimal value, ZString currencyCode)
		{
			Declaration.JE_ECI_InvoiceAmount = value;
			Declaration.JE_ECI_InvoiceCurrency = RefCurrency.LoadFromCurrencyCode(factory, currencyCode).PK;
		}
	}

	public class TestECIWriteOffCreatorTest : TestDeclarationCreatorTest
	{
		[ExpectNoExceptions]
		public void TestSetupTestForECIWriteoffWithConsignmentDetails()
		{
			DecCreator.SetupTestForECIWriteoffWithConsignmentDetails();
		}

		[ExpectNoExceptions]
		public void TestSetupTestCommercialInvoiceHeaderFOB100NZD()
		{
			DecCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
		}

		public override void TestSetupTestContainer()
		{
			DecCreator.SetupTestConsignmentDetails();
			base.TestSetupTestContainer();
			AssertEquals(1, Declaration.CusContainers.Count);
			AssertEquals("OOCL0000006", Declaration.CusContainers[0].CO_ContainerNumber);
		}

		#region Implementation
		protected override TestDeclarationCreator GetNewTestDeclarationCreator()
		{
			return new TestECIWriteOffCreator(Declaration);
		}

		protected new TestECIWriteOffCreator DecCreator
		{
			get { return (TestECIWriteOffCreator)base.DecCreator; }
		}
		#endregion
	}
}
