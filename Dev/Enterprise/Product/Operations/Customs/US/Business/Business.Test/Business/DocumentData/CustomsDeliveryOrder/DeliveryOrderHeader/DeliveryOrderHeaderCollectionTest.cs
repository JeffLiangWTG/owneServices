using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DeliveryOrderHeaderCollection))]
	sealed class DeliveryOrderHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestHasAtLeastOneSelectedForPrinting()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(false, declaration.DeliveryOrderHeaders.HasAtLeastOneSelectedForPrinting);
			DeliveryOrderHeader header1 = declaration.DeliveryOrderHeaders.AddNew();
			AssertEquals(false, declaration.DeliveryOrderHeaders.HasAtLeastOneSelectedForPrinting);
			header1.US_ShouldPrint = false;
			AssertEquals(false, declaration.DeliveryOrderHeaders.HasAtLeastOneSelectedForPrinting);
			DeliveryOrderHeader header2 = declaration.DeliveryOrderHeaders.AddNew();
			header2.US_ShouldPrint = false;
			AssertEquals(false, declaration.DeliveryOrderHeaders.HasAtLeastOneSelectedForPrinting);
			header2.US_ShouldPrint = true;
			AssertEquals(true, declaration.DeliveryOrderHeaders.HasAtLeastOneSelectedForPrinting);
		}

		public void TestDefault()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MWB123456";
			declaration.JE_HouseBill = "HWB123456";
			declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Say hello world to everyone");
			declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Say goodbye world to everyone");
			declaration.JE_GoodsDescription = "GOODS SHORT DESCRIPTION";
			declaration.JE_TotalWeight = 1.56m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Tonnes;
			declaration.JE_TotalNoOfPacks = 234;
			declaration.JE_TotalNoOfPacksPackType = ABIUnitOfMeasureList.Codes.Pairs;
			DeliveryOrderHeader header = declaration.DeliveryOrderHeaders.AddNew();
			AssertEquals("Say hello world to everyone" + System.Environment.NewLine + "Say goodbye world to everyone", header.US_DeliveryInstructions);
			AssertEquals(1, header.DeliveryOrderLines.Count);
			DeliveryOrderLine line = header.DeliveryOrderLines[0];
			AssertEquals("GOODS SHORT DESCRIPTION", line.US_GoodsDescription);
			AssertEquals(234, line.US_NoOfPackages);
			AssertEquals(ABIUnitOfMeasureList.Codes.Pairs, line.US_PackageType);
			DeliveryOrderHeader header2 = declaration.DeliveryOrderHeaders.AddNew();
			AssertEquals(1, header2.DeliveryOrderLines.Count);
			line = header2.DeliveryOrderLines[0];
			AssertEquals("GOODS SHORT DESCRIPTION", line.US_GoodsDescription);
			AssertEquals(234, line.US_NoOfPackages);
			AssertEquals(ABIUnitOfMeasureList.Codes.Pairs, line.US_PackageType);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new DeliveryOrderHeaderCollection(Factory.New<JobDeclaration>());
	}
}
