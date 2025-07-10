using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	public static class TestHelperClass
	{
		public static string TestFilesPath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"AU\NexDocs\TestFiles");

		public static ItemCodeSet CreateItemCodeSet(string key, CodeValueType type, string value)
		{
			return new ItemCodeSet
			{
				key = key,
				type = type,
				value = value
			};
		}

		public static ListCodeSet CreateListCodeSetForCodeDescription(string code, string description) => new ListCodeSet { listItemCodeSet = CreateItemCodeSetList(code, description, null) };

		public static ListCodeSet CreateListCodeSetForCodeDescriptionStartDate(string code, string description, string startDate) => new ListCodeSet { listItemCodeSet = CreateItemCodeSetList(code, description, startDate) };

		static ItemCodeSet[] CreateItemCodeSetList(string code, string description, string startDate)
		{
			var itemCodeSet = new List<ItemCodeSet>
			{
				CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.Code, CodeValueType.@string, code),
				CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.Description, CodeValueType.@string, description),
			};
			if (startDate != null)
			{
				itemCodeSet.Add(CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.StartDate, CodeValueType.dateTime, startDate));
			}
			return itemCodeSet.ToArray();
		}
	}
}
