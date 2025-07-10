using System.Globalization;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Helpers.Tests
{
	public static class TestHelperClasses
	{
		public class CommonData : BaseModel
		{
			public string Key { get; set; }
			public int Priority { get; set; }
			public override string ToString() => $"{this.GetType().Name}:{Key}";

			protected override bool IsValidCore(StringBuilder errorCollector, string source)
			{
				var valid = !string.IsNullOrEmpty(Key) && !Key.Contains("INVALID");

				if (!valid)
				{
					errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Invalid Key: {Key}");
				}

				return valid;
			}
			public override bool IsChapterSpecific => false;
			public override bool IsInChapter(string chapterFilter) => true;
		}

		public class LoadDataOne : CommonData { }
		public class LoadDataTwo : CommonData { }

		public class ProcessDataCommon : CommonData
		{
			public string Chapter { get; set; }
			public override bool IsChapterSpecific => true;
			public override bool IsInChapter(string chapterFilter) => Chapter.StartsWith(chapterFilter);

			public string Data { get; set; }
		}

		public class ProcessDataOne : ProcessDataCommon
		{
			public string DataOne { get; set; }
		}

		public class ProcessDataTwo : ProcessDataCommon
		{
			public string DataTwo { get; set; }
		}

		public class LoaderTester<T> : LoaderBase<T> where T : CommonData, new()
		{
			protected override string ParentElement => typeof(T).Name;

			protected override string ElementName => "LoadItem";

			protected override T CreateMinimumModelFromXElement(XElement element)
			{
				var x = new T();

				x.Key = element.Element("Key").Value;
				UpdateIntFromElementIfPresent(element, "Priority", y => x.Priority = y);
				x.HJID = x.Key;

				return x;
			}
		}

		public class MeasureLoaderTester : MeasureLoader
		{
			public Measure ConvertXElementToModel(XElement element)
			{
				var model = CreateMinimumModelFromXElement(element);
				LoadMetaInfo(model, element);
				model.ProcessUpdate(element);
				return model;
			}

			public new bool IsValidElement(XElement element) => base.IsValidElement(element);
		}
	}
}
