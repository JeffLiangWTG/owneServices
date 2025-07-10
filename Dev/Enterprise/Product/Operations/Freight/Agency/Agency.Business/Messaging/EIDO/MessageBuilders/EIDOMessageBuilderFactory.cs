namespace Enterprise.Freight.Agency.Business
{
	public static class EIDOMessageBuilderFactory
	{
		public static IEIDOMessageBuilder GetNewBuilder()
		{
			return new EIDOEdifactMessageBuilder();
		}
	}
}
