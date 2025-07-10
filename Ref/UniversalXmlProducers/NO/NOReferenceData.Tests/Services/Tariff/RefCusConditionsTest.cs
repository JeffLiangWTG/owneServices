using System.Collections.Generic;
using CargoWise.RefDbRepo.NOReferenceData.Services.RefCusConditions;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Tests
{
	sealed class RefCusConditionsTest
	{
		[Test]
		public void TestRefCusConditionsXmlIsValid()
		{
			Assert.Multiple(() =>
			{
				Assert.That(cusConditionXmlData.RefCusCodeConditionItem.Length, Is.EqualTo(88));
				Assert.That(cusConditionXmlData.RefCusCodeConditionItem[0].CusCode, Is.EqualTo("BV512"));
			});
		}

		[Test]
		public void TestRefCusConditionsXmlContent()
		{
			var expectedContent = new Dictionary<string, string>()
			{
				//
				// List below extracted from RefCusConditions.xml with XmlStarlet:
				// \Utils\xmlstarlet-1.6.1\xml.exe sel -T -t -m "/RefCusCodeConditionItems/RefCusCodeConditionItem/ConditionFormulas/ConditionFormula" -o "{ "" -v "../../CusCode" -o """ -o ", " -v "." -o """ -o " }," -n C:\git\wtg\CargoWise\RefDataRepo\UniversalXmlProducers\NO\NOReferenceData.Services\RefCusConditions\RefCusConditions.xml
				//

				{ "BV512", "([ASV] > 0.7 & [ASV] <= 2.7)" },{ "BV513", "([ASV] > 2.7 & [ASV] <= 3.7)" },{ "BV514", "([ASV] > 3.7 & [ASV] <= 4.7)" },{ "BV515", "([ASV] > 4.7 & [ASV] <= 10)" },
				{ "BV516", "([ASV] > 10 & [ASV] <= 15)" },{ "BV517", "([ASV] > 15 & [ASV] <= 22)" },{ "BV610", "([ASV] > 0.7 & [ASV] <= 4.7)" },{ "BV620", "([ASV] > 4.7 & [ASV] <= 10)" },{ "BV630", "([ASV] > 10 & [ASV] <= 15)" },
				{ "BV640", "([ASV] > 15 & [ASV] <= 22)" },{ "BV650", "([ASV] > 22)" },{ "MA100", "([RET] < 75)" },{ "MA107", "([RET] >= 93)" },{ "MA125", "([RET] >= 75 & [RET] < 93)" },{ "MA200", "[RET] < 75" },{ "MA207", "[RET] => 93" },
				{ "MA217", "[RET] >= 83 & [RET] < 93" },{ "MA225", "[RET] >= 75 & [RET] < 93" },{ "MA300", "[RET] < 60" },{ "MA307", "[RET] >= 93" },{ "MA325", "[RET] >= 75 & [RET] < 93" },{ "MA340", "[RET] >= 60 & [RET] < 75" },
				{ "MA400", "[RET] < 75" },{ "MA405", "[RET] >= 95" },{ "MA407", "[RET] >= 93" },{ "MA425", "[RET] >= 75 & [RET] < 93" },{ "MB100", "[RET] < 80" },{ "MB113", "[RET] >= 87" },{ "MB114", "[RET] >= 86" },
				{ "MB120", "[RET] >= 80 & [RET] < 87" },{ "MB200", "[RET] < 80" },{ "MB205", "[RET] >= 95" },{ "MB213", "[RET] >= 87" },{ "MB214", "[RET] >= 86 & [RET] < 95" },{ "MB220", "[RET] >= 80 & [RET] < 87" },{ "MB300", "[RET] < 80" },
				{ "MB305", "[RET] >= 95" },{ "MB313", "[RET] >= 87" },{ "MB314", "[RET] >= 86 & [RET] < 95" },{ "MB320", "[RET] >= 80 & [RET] < 87" },{ "MB400", "[RET] < 80" },{ "MB405", "[RET] >= 95" },{ "MB413", "[RET] >= 87" },{ "MB414", "[RET] >= 86 & [RET] < 95" },
				{ "MB420", "[RET] >= 80 & [RET] < 87" },{ "MB424", "[RET] >= 76 & [RET] < 80" },{ "MG100", "[RET] < 85" },{ "MG105", "[RET] >= 95" },{ "MG115", "[RET] >= 85 & [RET] < 95" },{ "MG200", "[RET] < 85" },
				{ "MG205", "[RET] >= 95" },{ "MG215", "[RET] >= 85 & [RET] < 95" },{ "MG300", "[RET] < 85" },{ "MG305", "[RET] >= 95" },{ "MG315", "[RET] >= 85 & [RET] < 95" },{ "MG400", "[RET] < 85" },
				{ "MG405", "[RET] >= 95" },{ "MG415", "[RET] >= 85 & [RET] < 95" },{ "MP100", "[RET] < 86" },{ "MP105", "[RET] >= 95" },{ "MP113", "[RET] >= 87 & [RET] < 95" },{ "MP114", "[RET] >= 86 & [RET] < 95" },{ "MP200", "[RET] < 86" },{ "MP205", "[RET] >= 95" },
				{ "MP210", "[RET] >= 90 & [RET] < 95" },{ "MP213", "[RET] >= 87 & [RET] < 90" },{ "MP214", "[RET] >= 86 & [RET] < 90" },{ "MP300", "([RET] < 86)" },{ "MP305", "([RET] >= 95)" },{ "MP313", "([RET] >= 87 & [RET] < 95)" },{ "MP314", "[RET] >= 86 & [RET] < 95" },{ "MP400", "([RET] < 86)" },
				{ "MP405", "([RET] >= 95)" },{ "MP413", "([RET] >= 87 & [RET] < 95)" },{ "MP414", "[RET] >= 86 & [RET] < 95" },{ "OL201", "([ASV] > 0.7 & [ASV] <= 2.7)" },{ "OL301", "([ASV] > 2.7 & [ASV] <= 3.7)" },{ "OL401", "([ASV] > 3.7 & [ASV] <= 4.7)" },
				{ "OL720", "([ASV] > 15 & [ASV] <= 22)" },{ "OL730", "([ASV] > 4.7 & [ASV] <= 15)" }, { "OL801", "([GPT] < 50)" }, { "OL802", "([GPT] >= 50 & [GPT] < 100)" },{ "OL803", "([GPT] >= 100 & [GPT] < 150)" },
				{ "OL804", "([GPT] >= 150 & [GPT] < 200)" },{ "BV801", "([GPT] < 50)" }, { "BV802", "([GPT] >= 50 & [GPT] < 100)" },{ "BV803", "([GPT] >= 100 & [GPT] < 150)" }, { "BV804", "([GPT] >= 150 & [GPT] < 200)" }
			};

			Assert.Multiple(() =>
			{
				foreach (var conditionItem in cusConditionXmlData.RefCusCodeConditionItem)
				{
					var formulas = conditionItem.ConditionFormulas[0];
					if (conditionItem.ConditionFormulas.Length > 1)
					{
						formulas += " +CODE+ " + conditionItem?.ConditionFormulas[1];
					}

					Assert.That(formulas, Is.EqualTo(expectedContent[conditionItem.CusCode]), $"Code {conditionItem.CusCode} : formula");
					Assert.That(conditionItem.IsImport, Is.EqualTo(true), $"Code {conditionItem.CusCode} : IsImport");
				}
			});
		}

		[SetUp]
		public void Setup()
		{
			cusConditionXmlData = CusConditionCodes.GetCusConditionData();
		}
		RefCusCodeConditionItems cusConditionXmlData;
	}
}
