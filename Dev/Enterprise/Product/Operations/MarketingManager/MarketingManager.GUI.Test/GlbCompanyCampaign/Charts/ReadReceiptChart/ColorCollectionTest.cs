using System.Collections.Generic;
using System.Windows.Media;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class ColorCollectionTest : TestCaseWithFactory
	{
		static Dictionary<string, Brush> ColorDictionary => new Dictionary<string, Brush>
		{
			["VER"] = Brushes.LightSkyBlue,
			["NDR"] = Brushes.Orange,
			["UNV"] = Brushes.Silver,
			["QUE"] = Brushes.YellowGreen,
			["NEW"] = Brushes.WhiteSmoke,
			["UNS"] = Brushes.DarkSalmon,
			["XXX"] = Brushes.Black,
			["FAI"] = Brushes.Crimson,
			["SCH"] = Brushes.MediumSeaGreen,
			["OPC"] = Brushes.LimeGreen,
			["OPQ"] = Brushes.IndianRed
		};

		public void TestColorForCodes()
		{
			ColorDictionary.ForEach(pair => AssertEquals($"Color for code '{pair.Key}'", pair.Value, ColorCollection.GetColorBrush(pair.Key)));
		}

		// public void TestOcyColorForCodes()
		// {
		// 	//ColorDictionary.ForEach(pair => AssertEquals($"Color for code '{pair.Key}'", pair.Value.ToString(), ColorCollection.GetOxyColor(pair.Key).ToBrush().ToString()));
		// }
	}
}
