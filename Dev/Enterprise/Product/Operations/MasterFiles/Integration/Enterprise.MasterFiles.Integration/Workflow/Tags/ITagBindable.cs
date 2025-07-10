namespace Enterprise.MasterFiles.Integration
{
	public interface ITagBindable : ITagable
	{
		ITagLinkCollection TagLinks_ForBinding { get; }
	}
}
