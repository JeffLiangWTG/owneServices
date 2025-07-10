using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	[TestedType(typeof(UNDGDataObjectCollectionReader))]
	public class UNDGDataObjectCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var today = ZDate.Today;
			var undg1 = new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
			{
				FlashPoint = "12.4",
				MarinePollutant = new UNDGMarinePollutant() { Code = "M", Description = "MARY1" },
				PackedInLimitedQuantity = ZBool.True,
				TechicalName = "TECHNAME1",
				Volume = 15m,
				VolumeUQ = new UnitOfVolume() { Code = Core.Constants.Volume.CubicFeet, Description = "Cubit Feet" },
				Weight = 20.5m,
				WeightUQ = new UnitOfWeight() { Code = Core.Constants.Weight.Pounds, Description = "Pounds" },
				PackQty = 5,
				PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet, Description = "Pallet" },
				PackingInstructionSection = "II",
				HasOverpack = true,
				OverpackID = "111",
				WasteCode = "222",
				SpecialPermitIssuedDate = today,
				SpecialPermitNumber = "333",
				SalvagePackaging = true,
				ResidueLastContained = true
			};
			var undg2 = new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
			{
				FlashPoint = "13.4",
				MarinePollutant = new UNDGMarinePollutant() { Code = "M", Description = "MARY2" },
				PackedInLimitedQuantity = ZBool.True,
				TechicalName = "TECHNAME2",
				Volume = 25m,
				VolumeUQ = new UnitOfVolume() { Code = Core.Constants.Volume.CubicFeet, Description = "Cubit Feet" },
				Weight = 30.5m,
				WeightUQ = new UnitOfWeight() { Code = Core.Constants.Weight.Pounds, Description = "Pounds" },
				PackQty = 10,
				PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet, Description = "Pallet" },
				PackingInstructionSection = "II",
				HasOverpack = true,
				OverpackID = "222",
				WasteCode = "333",
				SpecialPermitIssuedDate = today,
				SpecialPermitNumber = "444",
				SalvagePackaging = true,
				ResidueLastContained = true
			};

			var undgs = new List<UNDG>() { undg1, undg2 };

			var undgDummyObject = Factory.NewWithValidTestData<TestDummyUNDGDataObject>();

			TestErrorLogger logger = new TestErrorLogger();
			var reader = new UNDGDataObjectCollectionReader(logger, Factory, undgDummyObject, undgs.ToArray());
			reader.ReadIntoCollection();

			AssertEquals("UNDGs count", 2, undgDummyObject.UNDGs.Count);
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGVolume, 15m, undgDummyObject.UNDGs[0].DI_DGVolume);
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGWeight, 20.5m, undgDummyObject.UNDGs[0].DI_DGWeight);
			AssertEquals(UNDGDataItemSchema.Constants.DI_PackageCount, 5, undgDummyObject.UNDGs[0].DI_PackageCount);
			AssertEquals(UNDGDataItemSchema.Constants.DI_TechnicalName, "TECHNAME1", undgDummyObject.UNDGs[0].DI_TechnicalName);
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGFlashPoint, 12.4m, undgDummyObject.UNDGs[0].DI_DGFlashPoint);
			AssertEquals(UNDGDataItemSchema.Constants.DI_OverpackID, "111", undgDummyObject.UNDGs[0].DI_OverpackID);
			AssertEquals(UNDGDataItemSchema.Constants.DI_HazardousWasteCode, "222", undgDummyObject.UNDGs[0].DI_HazardousWasteCode);
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGVolume, 25m, undgDummyObject.UNDGs[1].DI_DGVolume);
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGWeight, 30.5m, undgDummyObject.UNDGs[1].DI_DGWeight);
			AssertEquals(UNDGDataItemSchema.Constants.DI_PackageCount, 10, undgDummyObject.UNDGs[1].DI_PackageCount);
			AssertEquals(UNDGDataItemSchema.Constants.DI_TechnicalName, "TECHNAME2", undgDummyObject.UNDGs[1].DI_TechnicalName);
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGFlashPoint, 13.4m, undgDummyObject.UNDGs[1].DI_DGFlashPoint);
			AssertEquals(UNDGDataItemSchema.Constants.DI_OverpackID, "222", undgDummyObject.UNDGs[1].DI_OverpackID);
			AssertEquals(UNDGDataItemSchema.Constants.DI_HazardousWasteCode, "333", undgDummyObject.UNDGs[1].DI_HazardousWasteCode);
		}
	}

	class TestDummyUNDGDataObject : DummyBusinessObject, IUNDGDataItemProvider
	{
		public TestDummyUNDGDataObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable(true)]
		public UNDGDataItemCollection UNDGs
		{
			get
			{
				if (undgs == null)
				{
					undgs = GetNewUNDGs();
					RegisterEditableChildObject(undgs);
				}
				return undgs;
			}
		}

		UNDGDataItemCollection undgs;

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;

		protected virtual UNDGDataItemCollection GetNewUNDGs()
		{
			return new UNDGDataItemCollection(this);
		}
	}
}
