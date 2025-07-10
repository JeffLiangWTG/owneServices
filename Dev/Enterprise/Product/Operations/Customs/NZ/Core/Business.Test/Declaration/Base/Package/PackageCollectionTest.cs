using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(PackageCollection))]
	public class PackageCollectionTest : Customs.Business.Testing.BasePackageCollectionTest<PackageCollection>
	{
		public void TestDeclarationTypeMissmatchLogging()
		{
			// The Declaration returned by the owning PackingGroup is a BaseJobDeclaration instead of an NZ one when the logged-in country is Indonesia.
			// This test will verify logs are created to identify why an NZ package is being loaded in Indonesia.

			JobDeclaration declaration;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Indonesia))
			{
				declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				declaration.DisableDefaultPackingInformation = true;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
				declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;

				var bill = Factory.New<Bill>();
				bill.CU_JE = declaration.PK;
				bill.CU_BillNum = "HB001";

				var packGroup = Factory.New<PackingGroup>();
				packGroup.CR_CU_HouseBill = bill.PK;
				AssertNoExceptionThrown(() => _ = new PackageCollection(packGroup));

				Factory.Save();

				var otherFactory = new BusinessObjectFactory();
				var packGroupIOF = otherFactory.Load<PackingGroup>(packGroup.PK);
				AssertExceptionThrown<System.InvalidCastException>(() => _ = new PackageCollection(packGroupIOF));
			}

			AssertEquals("NZ.Business.Declaration.PackageCollection CTOR", ErrorReporter.LastKeyReported);
			var lastMessageReported = ErrorReporter.LastMessageReported;
			AssertStartsWith("Exception message", "Unable to cast object of type 'Enterprise.Customs.Business.BaseJobDeclaration' to type 'Enterprise.Customs.NZ.Business.Declaration.JobDeclaration'.", lastMessageReported);
			AssertContains("Error report message", "Current Country/Region Code: ID", lastMessageReported);
			AssertContains("Error report message", "Declaration Is In Database: Y", lastMessageReported);
			AssertContains("Error report message", "Declaration Application Code: TSW,", lastMessageReported);
			AssertContains("Error report message", "Declaration Country/Region Code: ID", lastMessageReported);
			AssertContains("Error report message", "Declaration Branch Country/Region Code: ID", lastMessageReported);
			AssertContains("Error report message", "Bill Number: HB001", lastMessageReported);
			AssertContains("Error report message", "Container Number: null", lastMessageReported);
			AssertContains("Error report message", $"Bill Dec PK: '{declaration.PK}'", lastMessageReported);
			AssertContains("Error report message", $"Container Dec PK: 'null'", lastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestCountChangedInDeletedPackingGroup()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;

			var bill = declaration.Bills.AddNew();

			var packGroup = bill.PackingGroups.AddNew();
			var collection = new PackageCollection(packGroup);

			var package = collection.AddNew();
			package.FillWithValidTestData();
			package.CW_PackQty = 10;

			Factory.Save();

			var newFactory = NewFactory();
			newFactory.RefreshEnabled = true;

			var groupInNewFactory = newFactory.Load<PackingGroup>(packGroup.PK);
			groupInNewFactory.Delete();

			var newPackage = collection.AddNew();
			newPackage.FillWithValidTestData();
			newPackage.CW_PackQty = 15;

			newFactory.Save();

			Assert("It should be deleted from the data refresh bus.", packGroup.IsDeleted);
			Assert("It should be deleted in the DeleteForDataRefresh of packGroup.", newPackage.IsDeleted);
			AssertEquals("PackingGroup should remove and delete all children packages when it's deleted for refresh.", 0, collection.Count);
		}

		public void TestDeletingLastItemDoesntCauseAnExceptionInRowValidation()
		{
			Declaration.DisableDefaultPackingInformation = true;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;

			Bill houseBill = Declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "1";

			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "AE", "AM", "AP", "AT", "BG", "FX", "5H");

			Package pack1 = Declaration.Packages.AddNew();
			pack1.CW_PackQty = 11;
			pack1.CW_PackType = "AE";
			pack1.CW_HouseBill = "1";
			Package pack2 = Declaration.Packages.AddNew();
			pack2.CW_PackQty = 22;
			pack2.CW_PackType = "AM";
			pack2.CW_HouseBill = "1";
			Package pack3 = Declaration.Packages.AddNew();
			pack3.CW_PackQty = 33;
			pack3.CW_PackType = "AP";
			pack3.CW_HouseBill = "1";
			Package pack4 = Declaration.Packages.AddNew();
			pack4.CW_PackQty = 44;
			pack4.CW_PackType = "AT";
			pack4.CW_HouseBill = "1";
			Package pack5 = Declaration.Packages.AddNew();
			pack5.CW_PackQty = 55;
			pack5.CW_PackType = "BG";
			pack5.CW_HouseBill = "1";
			Package pack6 = Declaration.Packages.AddNew();
			pack6.CW_PackQty = 66;
			pack6.CW_PackType = "FX";
			pack6.CW_HouseBill = "1";
			Package pack7 = Declaration.Packages.AddNew();
			pack7.CW_PackQty = 66;
			pack7.CW_PackType = "5H";
			pack7.CW_HouseBill = "1";

			AssertEquals("Pre-condition: Declaration.Packages.Count", 7, Declaration.Packages.Count);
			AssertNoRowErrors(pack1);
			AssertNoRowErrors(pack2);
			AssertNoRowErrors(pack3);
			AssertNoRowErrors(pack4);
			AssertNoRowErrors(pack5);
			AssertNoRowErrors(pack6);
			AssertNoRowErrors(pack7);

			AssertNoExceptionThrown(delegate
			{
				Declaration.Packages.RemoveAndDelete(pack6);
			});

			AssertEquals("Declaration.Packages.Count", 6, Declaration.Packages.Count);
		}

		public void TestPackageCountIsRestrictedTo1onECIsAndUnrestrictedOnFormal()
		{
			Declaration.DisableDefaultPackingInformation = true;
			AssertEquals("Precondition: Declaration.IsECIWriteoff", false, Declaration.IsECIWriteoff);
			Bill houseBill = Declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "1";
			CusContainer container = Declaration.CusContainers.AddNew();
			PackingGroup packingGroup = houseBill.PackingGroups.AddNew();
			packingGroup.CR_CO_Container = container.PK;

			Package pack1 = Declaration.Packages.AddNew();
			pack1.CW_HouseBill = "1";
			Package pack2 = Declaration.Packages.AddNew();
			pack2.CW_HouseBill = "1";
			Package pack3 = Declaration.Packages.AddNew();
			pack3.CW_HouseBill = "1";
			Package pack4 = Declaration.Packages.AddNew();
			pack4.CW_HouseBill = "1";
			Package pack5 = Declaration.Packages.AddNew();
			pack5.CW_HouseBill = "1";
			Package pack6 = Declaration.Packages.AddNew();
			pack6.CW_HouseBill = "1";
			AssertNoRowErrors(pack1);
			AssertNoRowErrors(pack2);
			AssertNoRowErrors(pack3);
			AssertNoRowErrors(pack4);
			AssertNoRowErrors(pack5);
			AssertNoRowErrors(pack6);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertNoRowErrors(pack1);
			AssertHasRowError(pack2, "You can only have 1 Package Type per House Bill/Container combination for a write off declaration.");
			AssertHasRowError(pack3, "You can only have 1 Package Type per House Bill/Container combination for a write off declaration.");
			AssertHasRowError(pack4, "You can only have 1 Package Type per House Bill/Container combination for a write off declaration.");
			AssertHasRowError(pack5, "You can only have 1 Package Type per House Bill/Container combination for a write off declaration.");
			AssertHasRowError(pack6, "You can only have 1 Package Type per House Bill/Container combination for a write off declaration.");

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertNoRowErrors(pack1);
			AssertNoRowErrors(pack2);
			AssertNoRowErrors(pack3);
			AssertNoRowErrors(pack4);
			AssertNoRowErrors(pack5);
			AssertNoRowErrors(pack6);
		}

		public void TestConstructor()
		{
			AssertNotNull(Collection);
		}

		public void TestOverriddenAddNew()
		{
			AssertEquals(typeof(Package), Collection.AddNew().GetType());
		}

		#region Implementation

		protected new PackageCollection Collection
		{
			get
			{
				if (fCollection == null)
				{
					Bill houseBill = Declaration.Bills.AddNew();

					fCollection = new PackageCollection(houseBill.PackingGroups.AddNew());
				}
				return fCollection;
			}
		}
		PackageCollection fCollection;

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

		protected override PackageCollection GetCollectionToTest()
		{
			return Collection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			Package package = Factory.New<Package>();
			package.CW_CR_HouseContainer = Collection.PackingGroup.PK;
			return package;
		}
		#endregion
	}
}
