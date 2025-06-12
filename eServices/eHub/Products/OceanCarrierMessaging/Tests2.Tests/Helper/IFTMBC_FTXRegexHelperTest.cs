using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests2.Tests.Helper
{
  [TestClass]
  public class IFTMBC_FTXRegexHelperTest
  {
    [TestMethod]
    public void GetVGMCutOffDateTime()
    {
      var etdString = "202401022359";
      var helper = new IFTMBC_FTXRegexHelper();

      Assert.AreEqual(string.Empty, helper.GetVGMCutOffDateTime("VGM CUT?:2024-02-01", etdString));
      Assert.AreEqual("202401010000", helper.GetVGMCutOffDateTime("VGM-CUT?:2024-01-01", "20240102"));
      Assert.AreEqual("202401010000", helper.GetVGMCutOffDateTime("VGM/CUT?:2024-01-01", "2401022359"));
      Assert.AreEqual("202401010000", helper.GetVGMCutOffDateTime("VGM CUT?:2024-01-01", etdString));
      Assert.AreEqual("202401010000", helper.GetVGMCutOffDateTime("VGM(VGM Deadline)?:2024-01-01", etdString));
      Assert.AreEqual("202401010000", helper.GetVGMCutOffDateTime("vgm cut?:2024-01-01", etdString));
      Assert.AreEqual("202401010000", helper.GetVGMCutOffDateTime("vgm cut off?:2024-01-01", etdString));
      Assert.AreEqual("202401010000", helper.GetVGMCutOffDateTime("vgm cutoff?:2024-01-01", etdString));
      Assert.AreEqual("202401010000", helper.GetVGMCutOffDateTime("vgm cut-off?:2024-01-01", etdString));
      Assert.AreEqual("202401010000", helper.GetVGMCutOffDateTime("vgm deadline?:2024-01-01", etdString));
      Assert.AreEqual("202401010000", helper.GetVGMCutOffDateTime("vgm deadlines?:2024-01-01", etdString));
      Assert.AreEqual("202401010000", helper.GetVGMCutOffDateTime("vgm deadlines(sometext)?:2024-01-01", etdString));
      Assert.AreEqual("202401010000", helper.GetVGMCutOffDateTime("vgm deadlines (some text) ?: 2024-01-01", etdString));
      Assert.AreEqual("202401010000", helper.GetVGMCutOffDateTime("vgm deadlines (some text) ?:- 2024-01-01", etdString));
      Assert.AreEqual("202401012359", helper.GetVGMCutOffDateTime("vgm deadlines (some text) ?:- 2024-01-01 23?:59", etdString));
      Assert.AreEqual("202401012359", helper.GetVGMCutOffDateTime("vgm deadlines (some text) ?:- 2024-01-01 23?:59hrs", etdString));
      Assert.AreEqual("202401012359", helper.GetVGMCutOffDateTime("vgm deadlines (some text) ?:- 2024-01-01 23?:59?:59", etdString));
      Assert.AreEqual("202401012359", helper.GetVGMCutOffDateTime("Verified Gross Mass deadlines(some text) ?:- 2024-01-01 11?:59 PM", etdString));
      Assert.AreEqual("202401012359", helper.GetVGMCutOffDateTime("verified gross mass deadlines (some text) ?:- 2024-01-01 2359", etdString));
      Assert.AreEqual("202401012359", helper.GetVGMCutOffDateTime("SI and VGM deadlines(some text) ?:- 2024-01-01 2359 some text", etdString));
      Assert.AreEqual("202401012359", helper.GetVGMCutOffDateTime("si and vgm deadlines(some text) ?:- 2024-01-01 2359some text", etdString));
      Assert.AreEqual("202401012359", helper.GetVGMCutOffDateTime("vgm deadlines (some text) ?:- 2024-01-01 2359some text", etdString));
      Assert.AreEqual("202401012359", helper.GetVGMCutOffDateTime("vgm deadlines (some text) \n?:- \n2024-01-01 \n 2359some text", etdString));
      Assert.AreEqual("202401010303", helper.GetVGMCutOffDateTime("VGM Cut-ofF 2024-01-01 03?:03:Part2:Part 3:Part4:Part5", etdString));
      Assert.AreEqual("202401010303", helper.GetVGMCutOffDateTime("VGM Cut-ofF 01-01-2024 03?:03?:03:Part2:Part 3:Part4:Part5", etdString));
    }

    [TestMethod]
    public void GetDocumentCutOffDateTime()
    {
      var etdString = "202401022359";
      var helper = new IFTMBC_FTXRegexHelper();

      Assert.AreEqual(string.Empty, helper.GetSICutOffDateTime("SI CUT?:2024-02-01", etdString));
      Assert.AreEqual("202401010000", helper.GetSICutOffDateTime("SI-CUT?:2024-01-01", "20240102"));
      Assert.AreEqual("202401010000", helper.GetSICutOffDateTime("SI/CUT?:2024-01-01", "2401022359"));
      Assert.AreEqual("202401010000", helper.GetSICutOffDateTime("SI CUT?:2024-01-01", etdString));
      Assert.AreEqual("202401010000", helper.GetSICutOffDateTime("SI(SI Deadline)?:2024-01-01", etdString));
      Assert.AreEqual("202401010000", helper.GetSICutOffDateTime("si cut?:2024-01-01", etdString));
      Assert.AreEqual("202401010000", helper.GetSICutOffDateTime("si cut off?:2024-01-01", etdString));
      Assert.AreEqual("202401010000", helper.GetSICutOffDateTime("si cutoff?:2024-01-01", etdString));
      Assert.AreEqual("202401010000", helper.GetSICutOffDateTime("si cut-off?:2024-01-01", etdString));
      Assert.AreEqual("202401010000", helper.GetSICutOffDateTime("si deadline?:2024-01-01", etdString));
      Assert.AreEqual("202401010000", helper.GetSICutOffDateTime("si deadlines?:2024-01-01", etdString));
      Assert.AreEqual("202401010000", helper.GetSICutOffDateTime("si deadlines(sometext)?:2024-01-01", etdString));
      Assert.AreEqual("202401010000", helper.GetSICutOffDateTime("si deadlines (some text) ?: 2024-01-01", etdString));
      Assert.AreEqual("202401010000", helper.GetSICutOffDateTime("si deadlines (some text) ?:- 2024-01-01", etdString));
      Assert.AreEqual("202401010203", helper.GetSICutOffDateTime("si deadlines (some text) ?:- 2024-01-01 02?:03", etdString));
      Assert.AreEqual("202401010203", helper.GetSICutOffDateTime("si deadlines (some text) ?:- 2024-01-01 02?:3", etdString));
      Assert.AreEqual("202401010203", helper.GetSICutOffDateTime("si deadlines (some text) ?:- 2024-01-01 2?:03", etdString));
      Assert.AreEqual("202401012359", helper.GetSICutOffDateTime("si deadlines (some text) ?:- 2024-01-01 23?:59", etdString));
      Assert.AreEqual("202401012359", helper.GetSICutOffDateTime("si deadlines (some text) ?:- 2024-01-01 23?:59hrs", etdString));
      Assert.AreEqual("202401012359", helper.GetSICutOffDateTime("si deadlines (some text) ?:- 2024-01-01 23?:59?:59", etdString));
      Assert.AreEqual("202401012359", helper.GetSICutOffDateTime("Shipping Instruction deadlines(some text) ?:- 2024-01-01 11?:59 PM", etdString));
      Assert.AreEqual("202401012359", helper.GetSICutOffDateTime("shipping instructions deadlines (some text) ?:- 2024-01-01 2359", etdString));
      Assert.AreEqual("202401012359", helper.GetSICutOffDateTime("SI and VGM deadlines(some text) ?:- 2024-01-01 2359 some text", etdString));
      Assert.AreEqual("202401012359", helper.GetSICutOffDateTime("si and vgm deadlines(some text) ?:- 2024-01-01 2359some text", etdString));
      Assert.AreEqual("202401012359", helper.GetSICutOffDateTime("si deadlines (some text) ?:- 2024-01-01 2359some text", etdString));
      Assert.AreEqual("202401012359", helper.GetSICutOffDateTime("si deadlines (some text) \n?:- \n2024-01-01 \n 2359some text", etdString));
      Assert.AreEqual("202401011216", helper.GetSICutOffDateTime("Please consider SI deadLineS are set to 2024/01/01 @ 12?:16?:30 :Part2:Part3:Part4:Part5", etdString));
    }
  }
}
