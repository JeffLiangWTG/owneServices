namespace OcmPoc.Mapping.Interface
{
	public class MapReceivedCommand
    {
		public MapReceivedCommand(string messageName, string sender, byte[] content)
		{
			MessageName = messageName;
			Sender = sender;
			Content = content;
		}

		public string MessageName { get; }
		public string Sender { get; }
		public byte[] Content { get; }
	}
}
