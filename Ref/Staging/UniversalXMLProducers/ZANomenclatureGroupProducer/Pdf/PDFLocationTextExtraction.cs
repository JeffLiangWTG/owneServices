using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer
{
	public abstract class PDFLocationTextExtraction : LocationTextExtractionStrategy
	{
		internal List<ZaPdfCoordinateLine> PdfCoordinateContents = new List<ZaPdfCoordinateLine>();
		internal List<string> Others = new List<string>();

		public abstract void RenderText(TextRenderInfo renderInfo);

		public override void EventOccurred(IEventData data, EventType type)
		{
			if (type == EventType.RENDER_TEXT && data is TextRenderInfo renderInfo)
			{
				RenderText(renderInfo);
			}
			else
			{
				base.EventOccurred(data, type);
			}
		}
	}

	public static class PdfCoordinatesExtension
	{
		public static void AddNew(this List<ZaPdfCoordinateLine> list, ZaPdfCoordinateLine obj)
		{
			Argument.NotNull(list, nameof(list));
			Argument.NotNull(obj, nameof(obj));
			if (list.Count > 0)
			{
				for (int x = 0; x < list.Count; x++)
				{
					if (list[x]?.StartCoordinate > obj.StartCoordinate)
					{
						list.Insert(x, obj);
						return;
					}
				}
			}

			list.Add(obj);
		}
	}
}
