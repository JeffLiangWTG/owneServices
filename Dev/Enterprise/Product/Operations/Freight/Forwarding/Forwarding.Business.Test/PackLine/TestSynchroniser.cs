namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class TestSynchroniser : Enterprise.Integration.Freight.IPackLineSynchronise
	{
		public bool MarkedDirty;
		public void MarkSyncDirty()
		{
			MarkedDirty = true;
		}

		public void CleanForConcurrency()
		{
		}
	}
}
