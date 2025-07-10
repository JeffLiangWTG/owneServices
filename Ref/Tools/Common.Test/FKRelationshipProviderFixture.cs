using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Tools.Common.Test
{
	[TestFixture]
	public class FKRelationshipProviderFixture
	{
		[Test]
		public void GetFilteredTableAndFKs()
		{
			var dataSetsDic = DataSetHelperFixture.DataSetsList.ToDictionary(k => k[0], v => v);
			var fkRelationshipProvider = new FKRelationshipProvider(dataSetsDic);
			var tableAndFKsDic = fkRelationshipProvider.GetFilteredTableAndFKs(DataSetHelperFixture.CreateTableAndFKs());
			Assert.AreEqual(14, tableAndFKsDic.Keys.Count);

			Assert.AreEqual(1, tableAndFKsDic["RefLocoMap"].Count);
			Assert.AreEqual("RefUNLOCO", tableAndFKsDic["RefLocoMap"][0].ReferencedTable);
			Assert.AreEqual("RL_Code", tableAndFKsDic["RefLocoMap"][0].ReferencedColumn);
			Assert.AreEqual("varchar", tableAndFKsDic["RefLocoMap"][0].DataType);
			Assert.AreEqual(1, tableAndFKsDic["RefUNLOCOUtcOffset"].Count);
			Assert.AreEqual("RefUNLOCO", tableAndFKsDic["RefUNLOCOUtcOffset"][0].ReferencedTable);
			Assert.IsFalse(tableAndFKsDic["RefCusTaxOrFeeLanguage"].Any(x => x.ReferencedTable == "RefLanguageType"));

			Assert.AreEqual(2, tableAndFKsDic["RefCusRate"].Count);
			CollectionAssert.AreEquivalent(new[] { "RefCusTariff", "RefCusTariffNationalCode" }, tableAndFKsDic["RefCusRate"].Select(x => x.ReferencedTable));
			Assert.AreEqual(3, tableAndFKsDic["RefCusCondition"].Count);
			CollectionAssert.AreEquivalent(new[] { "RefCusTariff", "RefCusNomenclatureGroup", "RefCusPreference" }, tableAndFKsDic["RefCusCondition"].Select(x => x.ReferencedTable));
			Assert.AreEqual(3, tableAndFKsDic["RefCusApplicability"].Count);
			CollectionAssert.AreEquivalent(new[] { "RefCusRate", "RefCusCondition", "RefCusTariffAdditionalCode" }, tableAndFKsDic["RefCusApplicability"].Select(x => x.ReferencedTable));
		}
	}
}
