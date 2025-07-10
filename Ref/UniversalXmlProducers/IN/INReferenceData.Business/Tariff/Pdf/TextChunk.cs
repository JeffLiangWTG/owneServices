using System.Linq;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser.Data;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public class TextChunk
	{
		public TextChunk(TextRenderInfo renderInfo)
		{
			Text = renderInfo.GetText();
			var baseLineSegment = renderInfo.GetBaseline();
			startLocation = baseLineSegment.GetStartPoint();
			endLocation = baseLineSegment.GetEndPoint();
			var textHidden = renderInfo.GetFillColor().GetColorValue().All(x => x == 1f) || renderInfo.GetTextRenderMode() == PdfCanvasConstants.TextRenderingMode.INVISIBLE;
			IsValid = Text != InvalidChar && !textHidden;
		}

		public string Text { get; set; }
		public bool IsValid { get; }
		public float GetStartXPosition() => startLocation.Get(0);
		public float GetStartYPosition() => startLocation.Get(1);
		public float GetEndXPosition() => endLocation.Get(0);

		readonly Vector startLocation;
		readonly Vector endLocation;

		const string InvalidChar = "\u00ED";
	}
}
