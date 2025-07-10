namespace Enterprise.MasterFiles.Business
{
	interface IDocRollupOrGroupForBestMatcher
	{
		string JobType { get; }
		string ServiceDirection { get; }
		string TransportMode { get;  }

		bool HasServiceDirection { get; }
	}
}
