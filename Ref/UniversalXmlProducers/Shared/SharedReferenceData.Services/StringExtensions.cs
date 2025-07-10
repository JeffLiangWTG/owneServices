namespace CargoWise.RefDbRepo.SharedReferenceData.Services
{
	public static class StringExtensions
	{
		public static string Right(this string x, int length) => x.Length < length
			? x
			: x.Substring(x.Length - length, length);
	}
}
