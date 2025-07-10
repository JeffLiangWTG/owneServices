using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business
{
	public class LetterOfIndemnityOptions
	{
		public ZBool ChangeMarksAndNumbers { get; set; }
		public string NewMarksAndNumbers { get; set; }

		public ZBool ChangeGoodsDescription { get; set; }
		public string NewGoodsDescription { get; set; }

		public ZBool ChangeWeight { get; set; }
		public ZDecimal NewWeight { get; set; }
		public string NewWeightUnit { get; set; }

		public ZBool ChangeVolume { get; set; }
		public ZDecimal NewVolume { get; set; }
		public string NewVolumeUnit { get; set; }
	}
}
