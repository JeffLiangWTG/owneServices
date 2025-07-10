using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest.Testing
{
	[TestedType(typeof(CusSCAOceanBillDataContextManager))]
	sealed class CusSCAOceanBillDataContextManagerTest : ShipmentDataContextManagerTestCase<CusSCAOceanBillDataContextManager, BaseCusSCAOceanBill>
	{
		public void TestMultipleHVLVConsolidations()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalHVLVSeaTestFiles("MultipleHVLVConsolidations.xml")));

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				var bills = Factory.Load<BaseCusSCAOceanBill>(new ZQuery(CusSCAOceanBillSchema.CB_OceanBill, "HB1"));
				AssertEquals(2, bills.Length);
				AssertNotNull(bills.FirstOrDefault(x => x.CB_MasterHouseBill == "S00047376"));
				AssertNotNull(bills.FirstOrDefault(x => x.CB_MasterHouseBill == "S00047375"));
			}
		}

		public void TestDefaultOutputDirectory()
		{
			var directory = "\\WHERE\\IS\\THIS\\DIRECTORY";
			SystemDataRegistry.Instance.CustomDeclarationExportDirectory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, directory);
			var manager = new CusSCAOceanBillDataContextManager();
			AssertEquals("DefaultOutputDirectory", directory, manager.DefaultOutputDirectory);
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => new[] { RecipientRoleType.HSA, RecipientRoleType.SRP };

		protected override string ValidPopulatedUniversalShipmentXML => TestFileHelper.GetFileContents("OceanBillShipment");

		protected override bool ManagerChecksDataTargetToImport => false;

		protected override BaseCusSCAOceanBill GetNewBusinessObjectForTesting() => Factory.NewWithValidTestData<TestCusSCAOceanBill>();

		protected override void MakeShipmentUsableForThisRole(RecipientRoleType recipientRoleType, Shipment shipmentWithRecipientRole)
		{
			if (recipientRoleType == RecipientRoleType.HSA)
			{
				shipmentWithRecipientRole.DataContext.ClearDataSourceCollection();
				shipmentWithRecipientRole.DataContext.AddDataSource(DataContextType.SeaOceanBill, null);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			testFileHelper?.Dispose();
			testFileHelper = null;
		}

		TestFileHelper TestFileHelper => testFileHelper ??= new ();
		TestFileHelper testFileHelper;
	}
}
