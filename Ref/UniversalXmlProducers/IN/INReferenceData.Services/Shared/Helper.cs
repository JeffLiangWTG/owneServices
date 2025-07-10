namespace CargoWise.RefDbRepo.INReferenceData.Services
{
	public static class Helper
	{
		public static void Assume(bool condition, string message)
		{
			if (!condition)
			{
				throw new UnhandledApplicationException(message);
			}
		}
	}
}
