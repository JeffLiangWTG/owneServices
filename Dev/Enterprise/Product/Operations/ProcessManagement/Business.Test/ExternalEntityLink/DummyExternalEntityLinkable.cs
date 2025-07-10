namespace Enterprise.ProcessManagement.Business.Test
{
	class DummyExternalEntityLinkable : IExternalEntityLinkable
	{
		public string ParentTableCode { get; }
		public string ID { get; }

		public DummyExternalEntityLinkable(string id, string parentTableCode)
		{
			ID = id;
			ParentTableCode = parentTableCode;
		}
	}
}
