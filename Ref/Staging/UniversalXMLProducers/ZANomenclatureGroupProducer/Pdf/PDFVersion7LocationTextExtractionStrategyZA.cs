using System;
using System.Collections.Generic;
using System.Linq;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer
{
	public class PDFVersion7LocationTextExtractionStrategyZA : PDFLocationTextExtraction
	{
		public override void RenderText(TextRenderInfo renderInfo)
		{
			try
			{
				var baseline = renderInfo.GetBaseline();
				if (baseline != null)
				{
					var text = renderInfo.GetText();
					var startPoint = baseline.GetStartPoint();
					
					if (!string.IsNullOrEmpty(text) && startPoint != null)
					{
						var xCoordinate = (int)(startPoint.Get(1));
						var yCoordinate = (int)(startPoint.Get(0));
						
						if (!PdfCoordinateContents.ContainsCoordinate(yCoordinate))
						{
							PdfCoordinateContents.AddNew(new ZaPdfCoordinateLine(yCoordinate, new List<ZaPdfChunk>()));
						}
						if (PdfCoordinateContents.ContainsCoordinate(yCoordinate))
						{
							var coordinateContent = PdfCoordinateContents.FirstOrDefault(o => o.StartCoordinate == yCoordinate);
							if (!coordinateContent.Chunks.Any(o => o.Coordinate == xCoordinate))
							{
								coordinateContent.AddChunk(xCoordinate, text);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Others.Add(renderInfo.GetText() + "|" + ex.Message);
				throw;
			}
		}
	}
}
