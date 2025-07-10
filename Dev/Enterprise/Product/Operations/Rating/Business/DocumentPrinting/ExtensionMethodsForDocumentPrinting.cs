namespace Enterprise.Rating.Business
{
	using Enterprise.MasterFiles.Business;
	using Enterprise.Rating.Business.RatingEnums;

	internal static class ExtensionMethodsForDocumentPrinting
	{
		public static RefContainerCollection GetContainersInSameClassForEntryType(this RefContainer container, EntryTypes entryType)
		{
			return container == null
				? null
				: entryType == EntryTypes.Freight
					? container.ContainersInSameFreightRateClass
					: container.ContainersInSameHandlingRateClass;
		}
	}
}

