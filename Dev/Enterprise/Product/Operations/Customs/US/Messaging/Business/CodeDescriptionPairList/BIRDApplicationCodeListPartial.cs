namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	partial class BIRDApplicationCodeList
	{
		public static string GetABIApplicationCode(string birdApplicationCode)
		{
			switch (birdApplicationCode)
			{
				case Codes.EntrySummaryQueryInput:
					return ApplicationIdentifierCodeList.Codes.BIRDEntrySummaryQuery;

				default:
					return ApplicationIdentifierCodeList.Codes.BIRDTransaction;
			}
		}
	}
}
