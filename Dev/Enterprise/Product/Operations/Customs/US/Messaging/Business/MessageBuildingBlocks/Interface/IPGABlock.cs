namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IPGABlock
	{
		void Deserialise(string eightyCharacterBlock);
		string Serialise();
	}
}
