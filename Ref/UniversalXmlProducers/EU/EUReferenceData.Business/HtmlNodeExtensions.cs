using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.EUReferenceData.Business.CUSNumbers
{
	public static class HtmlNodeExtensions
	{
		public static string CellValue(this HtmlNode node) => HtmlEntity.DeEntitize(node.InnerText.Trim('\r', '\n', ' '));
	}
}
