using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.MarketingManager.WebVoting
{
	public partial class JumpToPage : ZPage
	{
		const short numberOfCirclePerRow = 30;
		const short pageCircleDiameter = 20;
		const short pageCircleXGap = 2;
		const short pageCircleYGap = 6;
		const short pageCircleXBoundary = pageCircleDiameter + pageCircleXGap;
		const short pageCircleYBoundary = pageCircleDiameter + pageCircleYGap;
		const float pageCircleRadius = pageCircleDiameter / 2f;
		const float currentPageIndicatorSideLength = 10f;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It is the name of one type of font used on web.")]
		const string fontFamilyName = "Courier New";

		static Brush CompletedPageBrush { get { return Brushes.Blue; } }
		static Brush IncompletePageBrush { get { return Brushes.Gray; } }

		public static IEnumerable<RectangleHotSpot> GetHotSpots(IVoteExamSurveyAnswerSet answerSet)
		{
			int x = 0;
			int y = 0;
			for (int i = 0; i < answerSet.PageCount; i++)
			{
				RectangleHotSpot spot = new RectangleHotSpot();
				spot.Top = y;
				spot.Left = x;
				spot.Right = x + pageCircleDiameter;
				spot.Bottom = y + pageCircleDiameter;
				spot.PostBackValue = (i + 1).ToString();

				x += pageCircleXBoundary;
				if (x >= (numberOfCirclePerRow * pageCircleXBoundary))
				{
					y += pageCircleYBoundary;
					x = 0;
				}

				yield return spot;
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.ContentType = "image/jpeg";

			try
			{
				var answerSet = GetAnswerSet();
				if (answerSet != null)
				{
					int canvasWidth = Math.Min(numberOfCirclePerRow, answerSet.PageCount) * pageCircleXBoundary;
					int canvasHeight = (int)Math.Ceiling((decimal)answerSet.PageCount / numberOfCirclePerRow) * pageCircleYBoundary;
					using (Bitmap canvas = new Bitmap(canvasWidth, canvasHeight))
					using (Graphics graphics = Graphics.FromImage(canvas))
					{
						graphics.Clear(Color.White);

						int x = 0;
						int y = 0;
						for (int i = 1; i <= answerSet.PageCount; i++)
						{
							FillPageCircle(graphics, i, x, y, answerSet);
							DrawString(graphics, i, x, y);
							FillCurrentPageIndicator(graphics, i, x, y, answerSet);

							// Adjust X and Y
							x += pageCircleDiameter + pageCircleXGap;
							if (x >= canvasWidth)
							{
								x = 0;
								y += pageCircleDiameter + pageCircleXGap * 3;
							}
						}

						canvas.Save(Response.OutputStream, ImageFormat.Jpeg);
					}
				}
			}
			finally
			{
				string sessionId = Session.SessionID;   // To avoid "Session state has created a session id, but cannot save it because the response was already flushed by the application" error
				Response.End();
			}
		}

		void FillPageCircle(Graphics graphics, int pageNumber, int x, int y, IVoteExamSurveyAnswerSet answerSet)
		{
			bool isCompleted = answerSet.AnswerWrappers.FindByPage<VoteExamSurveyAnswerWrapper>(pageNumber, false).All(w => w.IsAnswered);

			var brush = isCompleted ? CompletedPageBrush : IncompletePageBrush;
			graphics.FillEllipse(brush, x, y, pageCircleDiameter, pageCircleDiameter);
		}

		void DrawString(Graphics graphics, int pageNumber, int x, int y)
		{
			using (Font font = new Font(fontFamilyName, 9, FontStyle.Bold))
			using (Font smallFont = new Font(fontFamilyName, 7, FontStyle.Bold))
			{
				StringFormat stringFormat = new StringFormat();
				stringFormat.Alignment = StringAlignment.Center;
				stringFormat.LineAlignment = StringAlignment.Center;
				string pageNumberAsString = pageNumber.ToString();
				Font fontToUse = pageNumberAsString.Length > 2 ? smallFont : font;
				graphics.DrawString(pageNumberAsString, fontToUse, Brushes.White, x + pageCircleRadius, y + pageCircleRadius, stringFormat);
			}
		}

		void FillCurrentPageIndicator(Graphics graphics, int pageNumber, int x, int y, IVoteExamSurveyAnswerSet answerSet)
		{
			if (answerSet.CurrentPage == pageNumber)
			{
				const float halfLength = currentPageIndicatorSideLength / 2;
				var triangle = new[]
				{
					new PointF(x + pageCircleRadius, y + pageCircleDiameter),
					new PointF(x + pageCircleRadius + halfLength, y + pageCircleDiameter + halfLength),
					new PointF(x + pageCircleRadius - halfLength, y + pageCircleDiameter + halfLength)
				};
				graphics.FillPolygon(Brushes.Red, triangle);
			}
		}

		IVoteExamSurveyAnswerSet GetAnswerSet() => answerSetDataSource ?? (answerSetDataSource = (IVoteExamSurveyAnswerSet)DataSource);
		IVoteExamSurveyAnswerSet answerSetDataSource;

		protected override BusinessObject GetNewDataSource()
		{
			string indexer = Request.Params["data"];
			return (!string.IsNullOrEmpty(indexer)) ? (BusinessObject)Session[indexer] : null;
		}
	}
}
