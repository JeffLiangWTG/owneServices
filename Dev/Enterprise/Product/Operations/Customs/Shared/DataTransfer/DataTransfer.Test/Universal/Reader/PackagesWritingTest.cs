using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	public sealed class PackagesWritingTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestPackingLinesWrittenToUniversalShipment()
		{
			var bizoFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(bizoFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"1B", "Drum, aluminium", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"AE", "Aerosol", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			bizoFactory.Save();

			var dataObject = GetDataObjectWithLineLevelPackages<Business.Testing.PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot,
																	Business.Testing.PivotBetweenCWandJITest.BaseJobComInvoiceLineWhichSupportsPackagesPivot>(Factory);
			var packsData = dataObject.PackingLineCollection;
			AssertEquals(2, packsData.Count);
			AssertEquals(1L, packsData[0].PackQty);
			AssertEquals(2L, packsData[1].PackQty);
			AssertEquals("Marks 1", packsData[0].MarksAndNos);
			AssertEquals("Marks 2", packsData[1].MarksAndNos);
			AssertEquals("1B", packsData[0].PackType.Code);
			AssertEquals("AE", packsData[1].PackType.Code);
			AssertEquals("Drum, aluminium", packsData[0].PackType.Description);
			AssertEquals("Aerosol", packsData[1].PackType.Description);
		}

		public static Shipment GetDataObjectWithLineLevelPackages<TDeclaration, TInvLine>(UniversalObjectFactory factory)
			where TDeclaration : BaseJobDeclaration
			where TInvLine : BaseJobComInvoiceLine
		{
			var declaration = factory.New<TDeclaration>();
			declaration.JE_MasterBill = "M";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = factory.New<TInvLine>();
			invoice.InvoiceLines.Add(invoiceLine);
			invoiceLine.JI_JZ = invoice.PK;
			var pack1 = declaration.Bills[0].PackingGroups[0].Packages.AddNew();
			var pack2 = declaration.Bills[0].PackingGroups[0].Packages.AddNew();
			pack1.CW_MarksAndNos = "Marks 1";
			pack1.CW_PackQty = 1;
			pack1.CW_PackType = "1B";
			pack2.CW_PackQty = 2;
			pack2.CW_PackType = "AE";
			pack2.CW_MarksAndNos = "Marks 2";
			var npbo1 = (InvoiceLinePackagePivot)invoiceLine.PackagesPivot.AddPivotFor(pack1);
			var npbo2 = (InvoiceLinePackagePivot)invoiceLine.PackagesPivot.AddPivotFor(pack2);
			npbo1.CHC_NumberOfPacks = 1;
			npbo2.CHC_NumberOfPacks = 2;
			factory.SaveForTesting();
			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<TDeclaration>(declaration.PK);
			var manager = (IShipmentDataContextManager)declaration.GetUniversalDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			using (((IExternalFetchHintSupporter)newFactory).SetupCreator())
			{
				var dataObject = (Shipment)writer.GetDataObject(declaration);
				return dataObject;
			}
		}
	}
}
