namespace CargoWise.RefDbRepo.XmlProducer.Common
{
	public static class FlagHelper
	{
		const string Prefix = "$$";
		const string Postfix = "$$";

		public static string GetFlag(string str)
		{
			return $"{Prefix}{str}{Postfix}";
		}
	}
}
