using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseJobDeclarationCartageIntegrationTest : BaseFreightTest
	{
		public void TestOpenedDeclarationHasChanges()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();

			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			declaration.JE_RL_NKOrigin = HomePort;
			declaration.JE_RL_NKFinalDestination = OverseasPort;
			declaration.DepotDocAddress.E2_OA_Address = LocalDepot.MainAddress.PK;
			declaration.JE_GoodsDescription = "A BIG DESCRIPTION.";
			declaration.JE_HouseBill = "345345";

			AssertNoErrors("Expecting Declaration to be in a saveable state.", declaration);

			declaration.JE_OH_Supplier = LocalConsignor.PK;
			declaration.JE_OH_Importer = LocalConsignee.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.SupplierPickupAddress.E2_OA_Address = LocalConsignor.MainAddress.PK;
			declaration.ImporterDeliveryAddress.E2_OA_Address = LocalConsignee.MainAddress.PK;

			Factory.Save();

			AssertEquals("Precondition: Export Job", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			AssertEquals("Declaration should have Equipment Mode set.", true, declaration.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded.IsEmpty);
			AssertEquals("Declaration should have Equipment Mode set.", false, declaration.DocsAndCartage.JP_FCLPickupEquipmentNeeded.IsEmpty);

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			var newDec = factory1.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.PK, declaration.PK));

			AssertEquals("Declaration should have Equipment Mode set.", true, newDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded.IsEmpty);
			AssertEquals("Declaration should have Equipment Mode set.", false, newDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded.IsEmpty);

			declaration.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "";
			declaration.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "";

			Factory.Save();

			AssertEquals("Declaration should not have Equipment Mode set.", true, declaration.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded.IsEmpty);
			AssertEquals("Declaration should not have Equipment Mode set.", true, declaration.DocsAndCartage.JP_FCLPickupEquipmentNeeded.IsEmpty);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			var newDec2 = factory2.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.PK, declaration.PK));
			JobDocsAndCartage cartage = newDec2.DocsAndCartage; // simulate binding and defaults set
			OrgHeader importer = newDec2.Importer; // simulate binding and defaults set
			AssertEquals("Declaration should have Equipment Mode set.", true, newDec2.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded.IsEmpty);
			OrgHeader supplier = newDec2.Supplier; // simulate binding and defaults set

			declaration.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = Enterprise.Core.Constants.ContainerModes.Liquid;
			declaration.DocsAndCartage.JP_FCLPickupEquipmentNeeded = Enterprise.Core.Constants.ContainerModes.Liquid;

			Factory.Save();

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			var newDec3 = factory3.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.PK, declaration.PK));
			JobDocsAndCartage cartage2 = newDec3.DocsAndCartage;//simulate binding and no defaults set

			AssertEquals("Declaration should have Equipment Mode set.", Enterprise.Core.Constants.ContainerModes.Liquid, newDec3.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			AssertEquals("Declaration should have Equipment Mode set.", Enterprise.Core.Constants.ContainerModes.Liquid, newDec3.DocsAndCartage.JP_FCLPickupEquipmentNeeded);
			AssertEquals("Declaration should have no changes.", false, newDec3.HasChanges || cartage2.HasChanges);
		}

		public void TestDepotOrCTOAddress()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			AssertEquals("No DepotOrCTOAddress", null, declaration.DepotOrCTOAddress);
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = LocalCTO.MainAddress.PK;
			AssertEquals("DepotOrCTOAddress is CTO", LocalCTO.MainAddress, declaration.DepotOrCTOAddress);
			declaration.DepotDocAddress.E2_OA_Address = LocalDepot.MainAddress.PK;
			AssertEquals("DepotOrCTOAddress is now Deport", LocalDepot.MainAddress, declaration.DepotOrCTOAddress);
		}

		public void TestICartageLooseCargoProperties()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.LCL;
			declaration.JE_TotalNoOfPacks = 25;
			declaration.JE_TotalNoOfPacksPackType = Constants.PkgUnit.Piece;
			declaration.JE_TotalWeight = 46m;
			declaration.JE_TotalWeightUnit = Constants.Weight.Ounces;
			declaration.JE_TotalVolume = 13m;
			declaration.JE_TotalVolumeUnit = Constants.Volume.CubicFeet;

			ICartageLooseCargo declarationAsILoose = declaration;

			AssertEquals("BookedPackages", 25, declarationAsILoose.BookedPackages);
			AssertEquals("BookedPackType", Constants.PkgUnit.Piece, declarationAsILoose.BookedPackType);
			AssertEquals("BookedWeight", 46m, declarationAsILoose.BookedWeight);
			AssertEquals("BookedWeightUnit", Constants.Weight.Ounces, declarationAsILoose.BookedWeightUnit);
			AssertEquals("BookedVolume", 13m, declarationAsILoose.BookedVolume);
			AssertEquals("BookedVolumeUnit", Constants.Volume.CubicFeet, declarationAsILoose.BookedVolumeUnit);

			AssertEquals("BookedDimensionUnit", "", declarationAsILoose.BookedDimensionUnit);
			AssertEquals("BookedHeight", 0m, declarationAsILoose.BookedHeight);
			AssertEquals("BookedLength", 0m, declarationAsILoose.BookedLength);
			AssertEquals("BookedWidth", 0m, declarationAsILoose.BookedWidth);
			AssertEquals("DangerousGoods", 0, declarationAsILoose.DangerousGoods.Count);
		}

		#region Implementation

		void InternalCartageManager_CheckCreateInternalCartageJob(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
		}

		protected override string TestingCountry
		{
			get { return null; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupLocalBranchAsLocalCartage();
		}

		protected virtual BaseJobDeclaration GetJobDeclaration()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = GetCMROrLegacy();
			return declaration;
		}

		protected virtual ZString GetCMROrLegacy()
		{
			return "LEG";
		}

		protected virtual ZString SetContainerMode(ZString currentValue)
		{
			return currentValue;
		}

		#endregion
	}
}
