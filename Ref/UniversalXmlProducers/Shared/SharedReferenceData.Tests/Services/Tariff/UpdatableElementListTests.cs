using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Tests
{
	[TestFixture]
	sealed class UpdatableElementListTests
	{
		[TestCase]
		public void Add()
		{
			var list = new UpdatableElementList<XmlElementReaderTariffModel>();
			list.Add(baselineModels);

			var processedModels = list.Values.ToArray();
			Assert.That(processedModels.Length, Is.EqualTo(baselineModels.Count));
			for (var i = 0; i < baselineModels.Count; i++)
			{
				Assert.That(processedModels[i], Is.SameAs(baselineModels[i]));
			}
		}

		[TestCase]
		public void CopyFrom()
		{
			var fromList = new UpdatableElementList<XmlElementReaderTariffModel>();
			fromList.Add(baselineModels);

			var toList = new UpdatableElementList<XmlElementReaderTariffModel>();
			toList.CopyFrom(fromList);

			var copiedModels = toList.Values.ToArray();
			Assert.That(copiedModels.Length, Is.EqualTo(baselineModels.Count));
			for (var i = 0; i < baselineModels.Count; i++)
			{
				Assert.That(copiedModels[i], Is.Not.SameAs(baselineModels[i]), "Model is copied by contents not reference");
				Assert.That(copiedModels[i].HJID, Is.EqualTo(baselineModels[i].HJID));
				Assert.That(copiedModels[i].OpType, Is.EqualTo(baselineModels[i].OpType));
				Assert.That(copiedModels[i].OpDate, Is.EqualTo(baselineModels[i].OpDate));
			}
		}

		[TestCase]
		public void ProcessUpdate()
		{
			var list = new UpdatableElementList<XmlElementReaderTariffModel>();
			list.Add(baselineModels);
			foreach (var model in updateModels)
			{
				var element = new XElement("x",
					new XElement("hjid", model.HJID),
					new XElement("metainfo",
						new XElement("opType", model.OpType),
						new XElement("transactionDate", DateTime.Now)
					)
				);
				list.ProcessUpdate(model, element);
			}

			var processedModels = list.Values.ToArray();
			Assert.That(processedModels.Select(x => x.HJID), Is.EquivalentTo(new[] { "2", "3" }), "Deleted model is excluded");
			Assert.That(processedModels[0].HJID, Is.EqualTo("2"));
			Assert.That(processedModels[0].OpType, Is.EqualTo(MetaInfoOpTypes.Created));
			Assert.That(processedModels[1].HJID, Is.EqualTo("3"));
			Assert.That(processedModels[1].OpType, Is.EqualTo(MetaInfoOpTypes.Created));
		}

		readonly List<XmlElementReaderTariffModel> baselineModels = new List<XmlElementReaderTariffModel>
		{
			CreateTestXmlElementTariffModel("1", MetaInfoOpTypes.Created),
			CreateTestXmlElementTariffModel("2", MetaInfoOpTypes.Created),
			CreateTestXmlElementTariffModel("3", MetaInfoOpTypes.Created),
		};

		readonly List<XmlElementReaderTariffModel> updateModels = new List<XmlElementReaderTariffModel>
		{
			CreateTestXmlElementTariffModel("1", MetaInfoOpTypes.Deleted),
			CreateTestXmlElementTariffModel("1", MetaInfoOpTypes.Deleted),
			CreateTestXmlElementTariffModel("2", MetaInfoOpTypes.Updated),
		};

		static XmlElementReaderTariffModel CreateTestXmlElementTariffModel(string hjid, string opType)
		{
			return new XmlElementReaderTariffModel
			{
				HJID = hjid,
				OpType = opType,
				OpDate = DateTime.Now
			};
		}
	}
}
