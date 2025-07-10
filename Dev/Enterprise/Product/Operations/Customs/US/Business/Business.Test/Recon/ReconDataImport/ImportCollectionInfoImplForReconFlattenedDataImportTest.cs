using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ImportCollectionInfoImplForReconExtensionsTest : TestCaseWithFactory
	{
		public void TestAddPropertiesForLinesExport()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USFTARECONIND, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				ReconFlattenedDataLineCollection collection = new ReconFlattenedDataLineCollection(Factory);
				ImportCollectionInfoImpl impl = new ImportCollectionInfoImpl(collection);
				IImportCollectionInfo info = impl;
				AssertEquals("no mappable properties", 0, new List<IImportPropertyInfo>(info.Properties).Count);
				impl.AddFlattenedDataPropertiesForLineExport();
				var list = new List<IImportPropertyInfo>(info.Properties);
				AssertEquals("mappable properties defined", 87, list.Count);
				AssertEquals("capital case properties defined", 14, list.Where(x => x.CharacterCasing == ZCharacterCasing.Upper).Count());
			}
		}

		public void TestAddPropertiesForLinesImport()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USFTARECONIND, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				ReconFlattenedDataLineCollection collection = new ReconFlattenedDataLineCollection(Factory);
				ImportCollectionInfoImpl impl = new ImportCollectionInfoImpl(collection);
				IImportCollectionInfo info = impl;
				AssertEquals("no mappable properties", 0, new List<IImportPropertyInfo>(info.Properties).Count);
				impl.AddFlattenedDataPropertiesForLineUpdate();
				var list = new List<IImportPropertyInfo>(info.Properties);
				AssertEquals("mappable properties defined", 80, list.Count);
				AssertEquals("capital case properties defined", 14, list.Where(x => x.CharacterCasing == ZCharacterCasing.Upper).Count());
			}
		}

		public void TestAddPropertiesForEntryExport()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USFTARECONIND, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				ReconFlattenedDataLineCollection collection = new ReconFlattenedDataLineCollection(Factory);
				ImportCollectionInfoImpl impl = new ImportCollectionInfoImpl(collection);
				IImportCollectionInfo info = impl;
				AssertEquals("no mappable properties", 0, new List<IImportPropertyInfo>(info.Properties).Count);
				impl.AddFlattenedDataPropertiesForEntryExport();
				var list = new List<IImportPropertyInfo>(info.Properties);
				AssertEquals("mappable properties defined", 63, list.Count);
				AssertEquals("capital case properties defined", 7, list.Where(x => x.CharacterCasing == ZCharacterCasing.Upper).Count());
			}
		}

		public void TestAddPropertiesForEntryImport()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USFTARECONIND, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				ReconFlattenedDataLineCollection collection = new ReconFlattenedDataLineCollection(Factory);
				ImportCollectionInfoImpl impl = new ImportCollectionInfoImpl(collection);
				IImportCollectionInfo info = impl;
				AssertEquals("no mappable properties", 0, new List<IImportPropertyInfo>(info.Properties).Count);
				impl.AddFlattenedDataPropertiesForEntryUpdate();
				var list = new List<IImportPropertyInfo>(info.Properties);
				AssertEquals("mappable properties defined", 62, list.Count);
				AssertEquals("capital case properties defined", 7, list.Where(x => x.CharacterCasing == ZCharacterCasing.Upper).Count());
			}
		}
	}
}
