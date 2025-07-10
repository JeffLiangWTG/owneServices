using System;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public static class PublishDateProvider
	{
		public static DateTime PublishedDate { get; private set; }

		public static void SetPublishedDate(DateTime publicationDate)
		{
			PublishedDate = publicationDate;
		}
	}
}
