using System.IO;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public static class StreamUtils
	{
		public static byte[] ConvertStreamToByteArray(Stream stream)
		{
			byte[] byteArray = new byte[16 * 1024];
			using (var mStream = new MemoryStream())
			{
				int bit;
				while ((bit = stream.Read(byteArray, 0, byteArray.Length)) > 0)
				{
					mStream.Write(byteArray, 0, bit);
				}
				return mStream.ToArray();
			}
		}
	}
}
