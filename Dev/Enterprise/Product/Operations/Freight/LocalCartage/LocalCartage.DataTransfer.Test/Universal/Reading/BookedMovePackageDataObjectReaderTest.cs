using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Testing.Core;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal.Testing
{
	public class BookedMovePackageDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestReadIntoBusinessObject()
		{
			var packingLine = SetupBookedMovePackage();
			var package = new BookedMovePackageDataObjectReader(packingLine, Logger, Factory, Factory.New<CommonCartage>(), new Dictionary<ZGuid, ZInt>()).ReadIntoBusinessObject();
			AssertPackage(package);
		}

		public PackingLine SetupBookedMovePackage()
		{
			var packageDataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packageDataObject.LengthUnit = ListHelper.GetWithDescription<UnitOfLength>(Constants.Length.Centimetres, BindToLists.GetCachedLists(Factory.BOFactory).DimensionUnits);
			packageDataObject.Length = 1;
			packageDataObject.Width = 2;
			packageDataObject.Height = 3;
			packageDataObject.PackType = ListHelper.GetWithDescription<PackageType>(Constants.PkgUnit.Roll, BindToLists.GetCachedLists(Factory.BOFactory).OuterPackTypes);
			packageDataObject.PackQty = 4;
			packageDataObject.VolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(Constants.Volume.CubicFeet, BindToLists.GetCachedLists(Factory.BOFactory).VolumeUnits);
			packageDataObject.Volume = 5;
			packageDataObject.WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(Constants.Weight.Ounces, BindToLists.GetCachedLists(Factory.BOFactory).WeightUnits);
			packageDataObject.Weight = 6;
			var undg = new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
			{
				FlashPoint = "12.4",
				MarinePollutant = new UNDGMarinePollutant()
				{ Code = "M", Description = "MARY" },
				PackedInLimitedQuantity = ZBool.True,
				TechicalName = "TECHNAME",
				Volume = 15m,
				VolumeUQ = new UnitOfVolume { Code = Constants.Volume.CubicFeet, Description = "Cubit Feet" },
				Weight = 20.5m,
				WeightUQ = new UnitOfWeight { Code = Constants.Weight.Pounds, Description = "Pounds" }
			};
			packageDataObject.SetUNDGCollection(() => new List<UNDG> { undg });
			var additionalService1 = new AdditionalService { ServiceCode = new CodeDescriptionPair { Code = "FUM", Description = "Fumigation" }, Booked = new ZDateTime(2016, 6, 1), Completed = new ZDateTime(2016, 6, 2) };
			var additionalService2 = new AdditionalService { ServiceCode = new CodeDescriptionPair { Code = "ABC", Description = "ABC - Description" } };
			packageDataObject.SetAdditionalServiceCollection(() => new List<AdditionalService>());
			packageDataObject.AdditionalServiceCollection.Add(additionalService1);
			packageDataObject.AdditionalServiceCollection.Add(additionalService2);
			return packageDataObject;
		}

		public void AssertPackage(CommonBookedCtgMove package)
		{
			AssertEquals(1m, package.EW_BookedLength);
			AssertEquals(2m, package.EW_BookedWidth);
			AssertEquals(3m, package.EW_BookedHeight);
			AssertEquals(Constants.Length.Centimetres, package.EW_DimUnit);
			AssertEquals(4, package.EW_BookedPackCount);
			AssertEquals(Constants.PkgUnit.Roll, package.EW_F3_NKPackType);
			AssertEquals(5m, package.EW_BookedVolume);
			AssertEquals(Constants.Volume.CubicFeet, package.EW_VolumeUQ);
			AssertEquals(6m, package.EW_BookedWeight);
			AssertEquals(Constants.Weight.Ounces, package.EW_WeightUQ);
			AssertEquals(1, package.UNDGs.Count);
			AssertEquals(2, package.Services.Count);
			var fumService = package.Services.Cast<JobService>().Single(s => s.ES_ServiceCode == "FUM");
			AssertEquals(new ZDateTime(2016, 6, 1), fumService.ES_Booked);
			AssertEquals(new ZDateTime(2016, 6, 2), fumService.ES_Completed);
			package.Services.Cast<JobService>().Single(s => s.ES_ServiceCode == "ABC");
		}

		TestErrorLogger Logger
		{
			get
			{
				return logger ?? (logger = new TestErrorLogger());
			}
		}

		TestErrorLogger logger;
	}
}
