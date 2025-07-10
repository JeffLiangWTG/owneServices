namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists
{
	public static class LevelAttributeKeyConverter
	{
		public static string GetKeyFromValue(string levelAttributeValue)
		{
			switch (levelAttributeValue)
			{
				case CodeListsConstants.AttributeValues.Item:
					return CodeListsConstants.XMLEntryElementNames.LEVEL_ITEM;
				case CodeListsConstants.AttributeValues.Header:
					return CodeListsConstants.XMLEntryElementNames.LEVEL_HEADER;
				case CodeListsConstants.AttributeValues.House:
					return CodeListsConstants.XMLEntryElementNames.LEVEL_HOUSE;
			}

			return CodeListsConstants.XMLEntryElementNames.LEVEL;
		}

		public static string ConvertToOutputKey(string fromKey)
		{
			switch (fromKey)
			{
				case CodeListsConstants.XMLEntryElementNames.LEVEL_ITEM:
				case CodeListsConstants.XMLEntryElementNames.LEVEL_HEADER:
				case CodeListsConstants.XMLEntryElementNames.LEVEL_HOUSE:
					return CodeListsConstants.XMLEntryElementNames.LEVEL;
				default:
					return fromKey;
			}
		}
	}
}
