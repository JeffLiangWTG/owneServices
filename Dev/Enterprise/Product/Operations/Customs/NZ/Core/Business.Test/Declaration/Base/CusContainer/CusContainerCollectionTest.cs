using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.TradeSingleWindow;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(CusContainerCollection))]
	public class CusContainerCollectionTest : BusinessObjectCollectionTestCase
	{
		public void ValidationOnPackageCountFiresOnDeclarationWhenContainersAddedAndRemoved()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_TotalNoOfPacks = 20;
			Declaration.JE_TotalNoOfPacksPackType = "PK";
			AssertNoMessageError(Declaration.PackagesActualPackageCountInfo, JobDeclarationValidation.TotalPackageCountDoesNotMatchErrorMessage);
			Declaration.CusContainers.AddNew();
			AssertHasMessageError(Declaration.PackagesActualPackageCountInfo, JobDeclarationValidation.TotalPackageCountDoesNotMatchErrorMessage);
			Declaration.CusContainers.RemoveAndDeleteAll();
			AssertNoMessageError(Declaration.PackagesActualPackageCountInfo, JobDeclarationValidation.TotalPackageCountDoesNotMatchErrorMessage);
		}

		#region TestMCDValidationGetsCalledWhenModeIsChanged
		public void TestMCDValidationGetsCalledWhenModeIsChanged()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			AssertHasMessageError(declaration.JE_SendMCDContainerQuarantineDeclarationInfo, JobDeclarationValidationTest.MessageErrorImportFCLEntryRequiresMCD);
			declaration.CusContainers.RemoveAndDelete(container);
			AssertNoMessageErrors(declaration.JE_SendMCDContainerQuarantineDeclarationInfo);
		}
		#endregion

		#region TestCusContainerCollection
		public void TestCusContainerCollection()
		{
			AssertNotNull("Failed to create Collection", Containers);
			Containers.AddNew();
			AssertNotNull("Failed to add new container", Containers[0]);
		}
		#endregion

		#region TestTypedAddNew
		public new void TestTypedAddNew()
		{
			CusContainer container = Containers.AddNew();
			Assert(container != null);
		}
		#endregion

		#region TestDoDefaultSort
		public void TestDoDefaultSort()
		{
			CusContainer container4 = Containers.AddNew();
			container4.CO_ContainerNumber = "2";
			container4.CO_FCL_LCL_AIR = "LCL";

			CusContainer container1 = Containers.AddNew();
			container1.CO_ContainerNumber = "1";

			CusContainer container3 = Containers.AddNew();
			container3.CO_ContainerNumber = "2";
			container3.CO_FCL_LCL_AIR = "FCL";

			CusContainer container2 = Containers.AddNew();
			container2.CO_ContainerNumber = "2";

			AssertEquals("PreCondition : Container1", container4, Containers[0]);
			AssertEquals("PreCondition : Container2", container1, Containers[1]);
			AssertEquals("PreCondition : Container3", container3, Containers[2]);
			AssertEquals("PreCondition : Container4", container2, Containers[3]);

			Containers.DoDefaultSort();

			AssertEquals("Container1", container1, Containers[0]);
			AssertEquals("Container2", container2, Containers[1]);
			AssertEquals("Container3", container3, Containers[2]);
			AssertEquals("Container4", container4, Containers[3]);
		}
		#endregion

		#region TestFind
		public void TestFind()
		{
			CusContainer container1 = Containers.AddNew();
			container1.CO_ContainerNumber = TestContainerNumber1;

			CusContainer container2 = Containers.AddNew();
			container2.CO_ContainerNumber = TestContainerNumber2;

			CusContainer container3 = Containers.AddNew();
			container3.CO_ContainerNumber = TestContainerNumber3;

			CusContainer container4 = Containers.AddNew();
			container4.CO_ContainerNumber = TestContainerNumber4;

			AssertEquals("Container 1", container1, Containers.Find(TestContainerNumber1));
			AssertEquals("Container 2", container2, Containers.Find(TestContainerNumber2));

			Containers.Remove(container2);

			AssertEquals("Container 2 removed", null, Containers.Find(TestContainerNumber2));
			AssertEquals("Container 3", container3, Containers.Find(TestContainerNumber3));
			AssertEquals("Container 4", container4, Containers.Find(TestContainerNumber4));
		}
		#endregion

		#region Implementation
		protected const string TestContainerNumber1 = "GALU0192039";
		protected const string TestContainerNumber2 = "TREU3839209";
		protected const string TestContainerNumber3 = "WERU3738493";
		protected const string TestContainerNumber4 = "CXCU9303039";

		#region ContainerCollection
		protected CusContainerCollection Containers
		{
			get
			{
				if (fContainers == null)
				{
					fContainers = (CusContainerCollection)GetCollectionToTest();
				}
				return fContainers;
			}
		}
		CusContainerCollection fContainers;
		#endregion

		#region Declaration
		protected JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = JobDeclaration.New(Factory);
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
		#endregion

		#region GetCollectionToTest
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusContainerCollection(Declaration, Factory);
		}
		#endregion

		#endregion

	}
}
