namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public class CessLineChunk
	{
		public CessLineChunk(decimal startX, decimal endX, string text)
		{
			StartX = startX;
			EndX = endX;
			Text = text;
		}

		public decimal StartX { get; set; }
		public decimal EndX { get; set; }
		public string Text { get; set; }
	}
}
