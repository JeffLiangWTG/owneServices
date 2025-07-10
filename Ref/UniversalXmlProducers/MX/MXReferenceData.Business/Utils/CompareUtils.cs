using System.Linq;

namespace CargoWise.RefDbRepo.MXReferenceData.Business
{
	public static class CompareUtils
	{
		public static bool AreEquals(byte[] b1, byte[] b2)
		{
			if (b1 == b2)
			{
				return true;
			}
			if (b1 == null || b2 == null)
			{
				return false;
			}
			if (b1.Length != b2.Length)
			{
				return false;
			}

			return !b1.Where((t, i) => t != b2[i]).Any();
		}
	}
}
