using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NUnit.Framework;
using WTG.ROPE.Model;
using static Enterprise.MasterFiles.GUI.Test.ImageBitmapTestHelper;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class ImageBitmapHelperTest : TestCase
	{
		public void TestImageSource()
		{
			CombineAssertions(() =>
			{
				AssertImageBitsEquals(Properties.Resources.ComprehensiveReport_BG, ImageBitmapHelper.GetReportTypeBackground(CreditReportType.ComprehensiveReport));
				AssertImageBitsEquals(Properties.Resources.CommercialBureauInquiryReport_BG, ImageBitmapHelper.GetReportTypeBackground(CreditReportType.CommercialBureauEnquiry));
				AssertImageBitsEquals(Properties.Resources.FailureRiskReport_BG, ImageBitmapHelper.GetReportTypeBackground(CreditReportType.FailureRisk));
				AssertImageBitsEquals(Properties.Resources.LatePaymentRiskReport_BG, ImageBitmapHelper.GetReportTypeBackground(CreditReportType.LatePaymentRisk));

				AssertImageBitsEquals(Properties.Resources.ComprehensiveReport, ImageBitmapHelper.GetReportTypeIcon(CreditReportType.ComprehensiveReport));
				AssertImageBitsEquals(Properties.Resources.CommercialBureauInquiryReport, ImageBitmapHelper.GetReportTypeIcon(CreditReportType.CommercialBureauEnquiry));
				AssertImageBitsEquals(Properties.Resources.FailureRiskReport, ImageBitmapHelper.GetReportTypeIcon(CreditReportType.FailureRisk));
				AssertImageBitsEquals(Properties.Resources.LatePaymentRiskReport, ImageBitmapHelper.GetReportTypeIcon(CreditReportType.LatePaymentRisk));

				AssertImageBitsEquals(Properties.Resources.WarningRed, ImageBitmapHelper.GetCreditEventIconInfo(CreditEventType.ScoreChange).Icon);
				AssertImageBitsEquals(Properties.Resources.WarningOrange, ImageBitmapHelper.GetCreditEventIconInfo(CreditEventType.CollectionChange).Icon);
				AssertImageBitsEquals(Properties.Resources.WarningOrange, ImageBitmapHelper.GetCreditEventIconInfo(CreditEventType.CourtChange).Icon);
				AssertImageBitsEquals(Properties.Resources.WarningOrange, ImageBitmapHelper.GetCreditEventIconInfo(CreditEventType.PublicFilingChange).Icon);
				AssertImageBitsEquals(Properties.Resources.InfoGrey, ImageBitmapHelper.GetCreditEventIconInfo(CreditEventType.DirectorChange).Icon);
				AssertImageBitsEquals(Properties.Resources.InfoGrey, ImageBitmapHelper.GetCreditEventIconInfo(CreditEventType.ShareholderChange).Icon);
				AssertImageBitsEquals(Properties.Resources.InfoGrey, ImageBitmapHelper.GetCreditEventIconInfo(CreditEventType.FinancialChange).Icon);
				AssertImageBitsEquals(Properties.Resources.InfoGrey, ImageBitmapHelper.GetCreditEventIconInfo(CreditEventType.StatusChange).Icon);

				AssertEquals("#EE443A", ImageBitmapHelper.GetCreditEventIconInfo(CreditEventType.ScoreChange).Color);
				AssertEquals("#FF6600", ImageBitmapHelper.GetCreditEventIconInfo(CreditEventType.CollectionChange).Color);
				AssertEquals("#FF6600", ImageBitmapHelper.GetCreditEventIconInfo(CreditEventType.CourtChange).Color);
				AssertEquals("#FF6600", ImageBitmapHelper.GetCreditEventIconInfo(CreditEventType.PublicFilingChange).Color);
				AssertEquals("#71747C", ImageBitmapHelper.GetCreditEventIconInfo(CreditEventType.DirectorChange).Color);
				AssertEquals("#71747C", ImageBitmapHelper.GetCreditEventIconInfo(CreditEventType.ShareholderChange).Color);
				AssertEquals("#71747C", ImageBitmapHelper.GetCreditEventIconInfo(CreditEventType.FinancialChange).Color);
				AssertEquals("#71747C", ImageBitmapHelper.GetCreditEventIconInfo(CreditEventType.StatusChange).Color);

				AssertEquals(Color.FromArgb(255, 240, 240, 240), ImageBitmapHelper.CompanyInfoPanelBackColor);
				AssertEquals(Color.FromArgb(255, 176, 209, 225), ImageBitmapHelper.CompanyInfoPanelClickColor);
				AssertEquals(Color.FromArgb(255, 198, 217, 227), ImageBitmapHelper.CompanyInfoPanelHoverColor);
				AssertEquals(Color.FromArgb(255, 73, 169, 215), ImageBitmapHelper.CompanyInfoPanelInnerClickColor);
				AssertEquals(Color.FromArgb(255, 100, 182, 222), ImageBitmapHelper.CompanyInfoPanelInnerHoverColor);

				AssertEquals(Color.Green, ImageBitmapHelper.GetScoreLabelColor(90));
				AssertEquals(Color.Orange, ImageBitmapHelper.GetScoreLabelColor(77));
				AssertEquals(Color.Red, ImageBitmapHelper.GetScoreLabelColor(64));
			});
		}
	}

	public class ImageBitmapTestHelper : TestCase
	{
		public static void AssertImageBitsEquals(Image b1, Image b2)
		{
			var bits1 = ConvertBitMapToByteArray(b1, b1.RawFormat);
			var bits2 = ConvertBitMapToByteArray(b2, b2.RawFormat);
			AssertArrayEqualsByElements(bits1, bits2);
		}

		public static byte[] ConvertBitMapToByteArray(Image bitmap, ImageFormat format)
		{
			byte[] result = null;
			if (bitmap != null)
			{
				using (var stream = new MemoryStream())
				{
					bitmap.Save(stream, format);
					result = stream.ToArray();
				}
			}

			return result;
		}
	}
}
