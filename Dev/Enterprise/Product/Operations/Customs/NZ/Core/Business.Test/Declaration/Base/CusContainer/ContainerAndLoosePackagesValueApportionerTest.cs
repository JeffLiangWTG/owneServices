using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using NUnit.Framework;
	public class ContainerAndLoosePackagesValueApportionerTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestNullDeclarationOnConstructionGivesException()
		{
			new ContainerAndLoosePackagesValueApportioner(null);
		}

		public void TestGetValueForLoosePackages()
		{
			DecCreator.Declaration.DisableDefaultPackingInformation = true;
			DecCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			DecCreator.SetupTestForSea();
			DecCreator.SetupTestForExportToAU();
			DecCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			DecCreator.SetupTestContainer("OOCL0000006", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 4m, 10, "PK");
			DecCreator.SetupTestContainer("OOCL0000011", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 7m, 15, "PK");
			DecCreator.SetupTestContainer("OOCL0000034", ContainerModeList.Codes.Empty, ContainerSizeList.Codes.ContainerIc20Ft, 0m, 0, "PK");
			Declaration.JE_TotalWeight = 21m;
			Declaration.JE_ECI_InvoiceAmount = 70.00m;
			Declaration.JE_HouseBill = "SCOTTYNEEDSSOMELOVIN";
			Package package = Declaration.Packages.AddNew();
			package.CW_HouseBill = Declaration.PrimaryHouseBill.CU_BillUniqueCode;
			package.CW_PackQty = 2;
			package.CW_PackType = "PK";

			ContainerAndLoosePackagesValueApportioner valueApportioner = new ContainerAndLoosePackagesValueApportioner(Declaration);
			AssertEquals("valueApportioner.GetValueForContainer(Declaration.CusContainers[0])", 13.33m, valueApportioner.GetValueForContainer(Declaration.CusContainers[0]));
			AssertEquals("valueApportioner.GetValueForContainer(Declaration.CusContainers[1])", 23.33m, valueApportioner.GetValueForContainer(Declaration.CusContainers[1]));
			AssertEquals("valueApportioner.GetValueForContainer(Declaration.CusContainers[3])", 0.00m, valueApportioner.GetValueForContainer(Declaration.CusContainers[2]));

			AssertEquals("valueApportioner.GetValueForContainer(Declaration.CusContainers[2])", 33.34m, valueApportioner.GetValueForLoosePackages());
		}

		public void TestGetValueForContainerWithNormalValues()
		{
			DecCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			DecCreator.SetupTestForSea();
			DecCreator.SetupTestForExportToAU();
			DecCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			DecCreator.SetupTestContainer("OOCL0000006", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 4m, 10, "PK");
			DecCreator.SetupTestContainer("OOCL0000011", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 7m, 15, "PK");
			DecCreator.SetupTestContainer("OOCL0000027", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 10m, 20, "PK");
			DecCreator.SetupTestContainer("OOCL0000034", ContainerModeList.Codes.Empty, ContainerSizeList.Codes.ContainerIc20Ft, 0m, 0, "PK");
			Declaration.JE_TotalWeight = 21m;
			Declaration.JE_ECI_InvoiceAmount = 70.00m;
			ContainerAndLoosePackagesValueApportioner valueApportioner = new ContainerAndLoosePackagesValueApportioner(Declaration);
			AssertEquals("valueApportioner.GetValueForContainer(Declaration.CusContainers[0])", 13.33m, valueApportioner.GetValueForContainer(Declaration.CusContainers[0]));
			AssertEquals("valueApportioner.GetValueForContainer(Declaration.CusContainers[1])", 23.33m, valueApportioner.GetValueForContainer(Declaration.CusContainers[1]));
			AssertEquals("valueApportioner.GetValueForContainer(Declaration.CusContainers[2])", 33.34m, valueApportioner.GetValueForContainer(Declaration.CusContainers[2]));
			AssertEquals("valueApportioner.GetValueForContainer(Declaration.CusContainers[3])", 0.00m, valueApportioner.GetValueForContainer(Declaration.CusContainers[3]));
		}

		public void TestGetValueForContainerReturnsZeroWhenNoWeightsEntered()
		{
			DecCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			DecCreator.SetupTestForSea();
			DecCreator.SetupTestForExportToAU();
			DecCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			DecCreator.SetupTestContainer("OOCL0000006", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 0m, 10, "PK");
			DecCreator.SetupTestContainer("OOCL0000011", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 0m, 15, "PK");
			DecCreator.SetupTestContainer("OOCL0000027", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 0m, 20, "PK");
			DecCreator.SetupTestContainer("OOCL0000034", ContainerModeList.Codes.Empty, ContainerSizeList.Codes.ContainerIc20Ft, 0m, 0, "PK");
			ContainerAndLoosePackagesValueApportioner valueApportioner = new ContainerAndLoosePackagesValueApportioner(Declaration);
			AssertEquals("valueApportioner.GetValueForContainer(Declaration.CusContainers[0])", 0.00m, valueApportioner.GetValueForContainer(Declaration.CusContainers[0]));
			AssertEquals("valueApportioner.GetValueForContainer(Declaration.CusContainers[1])", 0.00m, valueApportioner.GetValueForContainer(Declaration.CusContainers[1]));
			AssertEquals("valueApportioner.GetValueForContainer(Declaration.CusContainers[2])", 0.00m, valueApportioner.GetValueForContainer(Declaration.CusContainers[2]));
			AssertEquals("valueApportioner.GetValueForContainer(Declaration.CusContainers[3])", 0.00m, valueApportioner.GetValueForContainer(Declaration.CusContainers[3]));
		}

		public void TestGettingUnknownContainerGetsAZeroResultWithNoExceptions()
		{
			DecCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			DecCreator.SetupTestForSea();
			DecCreator.SetupTestForExportToAU();
			DecCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			DecCreator.SetupTestContainer("OOCL0000006", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 0m, 10, "PK");
			ContainerAndLoosePackagesValueApportioner valueApportioner = new ContainerAndLoosePackagesValueApportioner(Declaration);
			AssertEquals("valueApportioner.GetValueForContainer(Declaration.CusContainers[0])", 0.00m, valueApportioner.GetValueForContainer(Factory.New<CusContainer>()));
		}

		public void TestGettingLooseValueWithNoLossePackagesGetsAZeroResultWithNoExceptions()
		{
			DecCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			DecCreator.SetupTestForSea();
			DecCreator.SetupTestForExportToAU();
			DecCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			DecCreator.SetupTestContainer("OOCL0000006", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 0m, 10, "PK");
			ContainerAndLoosePackagesValueApportioner valueApportioner = new ContainerAndLoosePackagesValueApportioner(Declaration);
			AssertEquals("valueApportioner.GetValueForContainer(Declaration.CusContainers[0])", 0.00m, valueApportioner.GetValueForLoosePackages());
		}

		#region Implementation
		#region Declaration
		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
		#endregion

		#region DecCreator
		TestECIWriteOffCreator DecCreator
		{
			get
			{
				fDecCreator ??= new TestECIWriteOffCreator(Declaration);
				return fDecCreator;
			}
		}
		TestECIWriteOffCreator fDecCreator;
		#endregion
		#endregion
	}
}
