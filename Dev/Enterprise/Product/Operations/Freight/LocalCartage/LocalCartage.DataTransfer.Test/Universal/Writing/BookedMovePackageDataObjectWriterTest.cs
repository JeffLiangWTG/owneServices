using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal.Testing
{
	public class BookedMovePackageDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestWriteDataObject()
		{
			var localTransport = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLUnpackLooseToCNE, 1);
			var package = localTransport.LooseBookedMoves[0];
			var additionalService = new AdditionalService { ServiceCode = new CodeDescriptionPair { Code = "FUM", Description = "Fumigation" }, Booked = new ZDateTime(2011, 6, 1), Completed = new ZDateTime(2011, 6, 2) };
			var additionalService1 = package.Services.AddNew();
			additionalService1.ES_ServiceCode = "FUM";
			additionalService1.ES_Booked = new ZDateTime(2016, 2, 1);
			additionalService1.ES_Completed = new ZDateTime(2016, 2, 2);
			additionalService1.ES_OH_Contractor = OrganizationAddressTestHelper.GetOrganizationBO_INTHEMSYD(additionalService1.Factory).PK;
			var additionalService2 = package.Services.AddNew();
			additionalService2.ES_ServiceCode = "ABC";
			Helper.SetupBookedMove(package, 10, Constants.PkgUnit.Bag, 11, Constants.Weight.Kilograms, 12, Constants.Volume.CubicMetres);
			var writer = new BookedMovePackageDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package)));
			var packageDataObject = writer.GetDataObject(package);
			AssertPackage(packageDataObject, 10L, Constants.PkgUnit.Bag, 11, Constants.Weight.Kilograms, 12, Constants.Volume.CubicMetres);
			AssertEquals(2, packageDataObject.AdditionalServiceCollection.Count);
			var fumAdditionalService = packageDataObject.AdditionalServiceCollection.Find(a => a.ServiceCode.Code.Value == "FUM");
			AssertEquals(new ZDateTime(2016, 2, 1), fumAdditionalService.Booked);
			AssertEquals(new ZDateTime(2016, 2, 2), fumAdditionalService.Completed);
			packageDataObject.AdditionalServiceCollection.Find(a => a.ServiceCode.Code.Value == "ABC");
			OrganizationAddressTestHelper.AssertOrganizationBO_INTHEMSYD("fumService.Contractor", fumAdditionalService.Contractor, "Contractor");
		}

		public static PackingLine AssertPackage(UniversalShipment cartageDataObject, ZLong packs, ZString packType, ZDecimal weight, ZString weightUnit, ZDecimal volume, ZString volumeUnit)
		{
			var packageDataObject = cartageDataObject.PackingLineCollection.FirstOrDefault(p => p.PackQty.Equals(packs));
			AssertPackage(packageDataObject, packs, packType, weight, weightUnit, volume, volumeUnit);
			return packageDataObject;
		}

		public static void AssertPackage(PackingLine packageDataObject, ZLong packs, ZString packType, ZDecimal weight, ZString weightUnit, ZDecimal volume, ZString volumeUnit)
		{
			AssertNotNull("Precondition: packageDataObject", packageDataObject);
			AssertEquals(packs, packageDataObject.PackQty);
			AssertEquals(packType, packageDataObject.PackType.Code);
			AssertEquals(weight, packageDataObject.Weight);
			AssertEquals(weightUnit, packageDataObject.WeightUnit.Code);
			AssertEquals(volume, packageDataObject.Volume);
			AssertEquals(volumeUnit, packageDataObject.VolumeUnit.Code);
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;
	}
}
