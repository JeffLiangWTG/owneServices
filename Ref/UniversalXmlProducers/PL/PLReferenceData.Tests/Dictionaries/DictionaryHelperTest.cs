using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries;
using CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryPuesc;
using NUnit.Framework;
using static CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryHelper;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Dictionaries
{
	[TestFixture]
	sealed class DictionaryHelperTest
	{
		[Test]
		public void TestGetXmlWriterConfiguration()
		{
			AssertXmlConfiguration(RefDataType.RefCusCodeList, typeof(RefCusCodeListAttribute), typeof(RefCusCodeList));
			AssertXmlConfiguration(RefDataType.RefCusCodeListWithAttributeExport, typeof(RefCusCodeType), typeof(RefCusCodeList), typeof(RefCusCodeListAttribute));
			AssertXmlConfiguration(RefDataType.RefCusCodeListWithAttributeImport, typeof(RefCusCodeListLanguage), typeof(RefCusCodeList), typeof(RefCusCodeListAttribute));
			AssertXmlConfiguration(RefDataType.RefCusCodeListWithLanguage, typeof(RefCusCodeType), typeof(RefCusCodeList), typeof(RefCusCodeListLanguage));
			AssertXmlConfiguration(RefDataType.RefCusCodeListMerged, typeof(RefCusCodeListAttribute), typeof(RefCusCodeList));
		}

		void AssertXmlConfiguration(RefDataType type, Type invalidEntity, params Type[] validEntities)
		{
			var configuration = type.GetXmlWriterConfiguration();
			foreach (var entity in validEntities)
			{
				Assert.That(configuration.GetConfiguration(entity), Is.Not.Null);
			}
			Assert.Null(configuration.GetConfiguration(invalidEntity));
		}

		[Test]
		public void TestGetPuescRefCusCodeListMapper()
		{
			Assert.That(RefDataType.RefCusCodeList.GetPuescRefCusCodeListMapper(), Is.InstanceOf<SimpleRefCusCodeListMapper>());
			Assert.That(RefDataType.RefCusCodeListWithAttributeExport.GetPuescRefCusCodeListMapper(), Is.InstanceOf<RefCusCodeListWithAttributeExportMapper>());
			Assert.That(RefDataType.RefCusCodeListWithAttributeImport.GetPuescRefCusCodeListMapper(), Is.InstanceOf<RefCusCodeListWithAttributeImportMapper>());
			Assert.That(RefDataType.RefCusCodeListWithLanguage.GetPuescRefCusCodeListMapper(), Is.InstanceOf<RefCusCodeListWithLanguageMapper>());
		}

		[Test]
		public void TestTruncateString()
		{
			Assert.AreEqual(TruncateString("", 3), "");
			Assert.AreEqual(TruncateString("ABCDEF", 3), "ABC");
			Assert.AreEqual(TruncateString("AB", 3), "AB");
		}

		[Test]
		public void TestGetDateTime()
		{
			var beforeStartDate = DictionariesConstants.DefaultStartDateDateTime.AddDays(-2);
			var afterEndDate = DictionariesConstants.DefaultEndDateDateTime.AddDays(2);
			var validDate = DictionariesConstants.DefaultStartDateDateTime.AddDays(2);

			Assert.AreEqual(GetDataTime(null, true), DictionariesConstants.DefaultStartDateDateTime);
			Assert.AreEqual(GetDataTime(null, false), DictionariesConstants.DefaultEndDateDateTime);
			Assert.AreEqual(GetDataTime(beforeStartDate, true), DictionariesConstants.DefaultStartDateDateTime);
			Assert.AreEqual(GetDataTime(beforeStartDate, false), DictionariesConstants.DefaultStartDateDateTime);
			Assert.AreEqual(GetDataTime(afterEndDate, true), DictionariesConstants.DefaultEndDateDateTime);
			Assert.AreEqual(GetDataTime(afterEndDate, false), DictionariesConstants.DefaultEndDateDateTime);
			Assert.AreEqual(GetDataTime(validDate, true), validDate);
			Assert.AreEqual(GetDataTime(validDate, false), validDate);
		}
	}
}
