namespace OcmPoc.Mapping.Interface
{
	public class MapForSendResult
	{
		public MapForSendResult(string fileName, byte[] content)
		{
			FileName = fileName;
			Content = content;
		}

		public string FileName { get; }
		public byte[] Content { get; }

	}
}
