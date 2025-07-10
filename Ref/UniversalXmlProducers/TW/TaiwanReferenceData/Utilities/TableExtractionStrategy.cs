using System;
using System.Collections.Generic;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class TableExtractionStrategy : ITextExtractionStrategy, IEventListener
	{
		class TextChunkLocation : ITextChunkLocation
		{
			private const float DIACRITICAL_MARKS_ALLOWED_VERTICAL_DEVIATION = 2;

			private readonly Vector startLocation;

			private readonly Vector endLocation;

			private readonly float charSpaceWidth;

			public TextChunkLocation(Vector startLocation, Vector endLocation, float charSpaceWidth)
			{
				this.startLocation = startLocation;
				this.endLocation = endLocation;
				this.charSpaceWidth = charSpaceWidth;
			}

			public Vector GetStartLocation() => startLocation;

			public Vector GetEndLocation() => endLocation;

			public float GetCharSpaceWidth() => charSpaceWidth;

			public float DistParallelEnd() => throw new NotImplementedException();

			public float DistParallelStart() => throw new NotImplementedException();

			public float DistanceFromEndOf(ITextChunkLocation other) => throw new NotImplementedException();

			public int DistPerpendicular() => throw new NotImplementedException();

			public int OrientationMagnitude() => throw new NotImplementedException();

			public bool SameLine(ITextChunkLocation other)
			{
				if (OrientationMagnitude() != other.OrientationMagnitude())
				{
					return false;
				}
				float distPerpendicularDiff = DistPerpendicular() - other.DistPerpendicular();
				if (distPerpendicularDiff == 0)
				{
					return true;
				}
				LineSegment mySegment = new LineSegment(startLocation, endLocation);
				LineSegment otherSegment = new LineSegment(other.GetStartLocation(), other.GetEndLocation());
				return Math.Abs(distPerpendicularDiff) <= DIACRITICAL_MARKS_ALLOWED_VERTICAL_DEVIATION && (mySegment.GetLength() == 0 || otherSegment.GetLength() == 0);
			}

			public bool IsAtWordBoundary(ITextChunkLocation previous)
			{
				var result = false;
				if (!(charSpaceWidth < 0.1f))
				{
					var distance = DistanceFromEndOf(previous);
					result = distance < 0f - charSpaceWidth || distance > charSpaceWidth / 2f;
				}
				return result;
			}

			internal static bool ContainsMark(ITextChunkLocation baseLocation, ITextChunkLocation markLocation)
			{
				throw new NotImplementedException();
			}
		}

		public List<TextChunk> locationalResult = new List<TextChunk>();

		public string GetResultantText()
		{
			throw new NotImplementedException();
		}

		public void EventOccurred(IEventData data, EventType type)
		{
			if (type.Equals(EventType.RENDER_TEXT))
			{
				var textRenderInfo = (TextRenderInfo)data;
				var lineSegment = textRenderInfo.GetBaseline();
				if (textRenderInfo.GetRise() != 0f)
				{
					var m = new Matrix(0f, 0f - textRenderInfo.GetRise());
					lineSegment = lineSegment.TransformBy(m);
				}

				var text = textRenderInfo.GetText();
				var startLocation = lineSegment.GetStartPoint();
				var endLocation = lineSegment.GetEndPoint();

				var item = new TextChunk(text, new TextChunkLocation(startLocation, endLocation, textRenderInfo.GetSingleSpaceWidth()));
				locationalResult.Add(item);
			}
		}

		public ICollection<EventType> GetSupportedEvents()
		{
			return new EventType[] { EventType.RENDER_TEXT };
		}
	}
}
