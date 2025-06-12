using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests2.Tests.Helper
{
  [TestClass]
  public class MultipleRFFListHelperTest
  {
    [TestMethod]
    public void TestNoRFFList()
    {
      var helper = new MultipleRFFListHelper();
      helper.CalculateRFFList(0, "true");

      var gnList = helper.GNList();
      var restList = helper.RestList();

      Assert.AreEqual(0, gnList.Count);
      Assert.AreEqual(0, restList.Count);
    }

    MultipleRFFListHelper Helper()
    {
      var helper = new MultipleRFFListHelper();
      helper.AddRFF("1001", "CN", "AU", "NI");
      helper.AddRFF("1002", "US", "AU", "NI", "AAA", false);
      helper.AddRFF("1003", "AU", "AU", "NI", "AAA");
      helper.AddRFF("1004", "CN", "AU", "CN", "AAA");
      helper.AddRFF("1005", "AU", "AU", "CZ", "AAA");
      helper.AddRFF("1006", "US", "AU", "CZ", "AAA");

      return helper;
    }

    void  AssertRFF(RFF rff, string value, string regulatingCountry)
    {
      Assert.AreEqual(value, rff.Value);
      Assert.AreEqual(regulatingCountry, rff.RegulatingCountry);
    }

    [TestMethod]
    public void TestMultipleRFFListHelper_RFFList()
    {
      var helper = Helper();
      helper.CalculateRFFList(3, "true");

      var gnList = helper.GNList();
      var restList = helper.RestList();

      Assert.AreEqual(6, gnList.Count);
      Assert.AreEqual(0, restList.Count);

      AssertRFF(gnList[0], "1003", "AU");
      AssertRFF(gnList[1], "1001", "CN");
      AssertRFF(gnList[2], "1002", "US");
      AssertRFF(gnList[3], "1004", "CN");
      AssertRFF(gnList[4], "1005", "AU");
      AssertRFF(gnList[5], "1006", "US");
    }

    [TestMethod]
    public void TestMultipleRFFListHelper_RestList()
    {
      var helper = Helper();
      helper.CalculateRFFList(2, "true");

      var gnList = helper.GNList();
      var restList = helper.RestList();

      Assert.AreEqual(5, gnList.Count);
      Assert.AreEqual(1, restList.Count);

      AssertRFF(gnList[0], "1003", "AU");
      AssertRFF(gnList[1], "1001", "CN");
      AssertRFF(gnList[2], "1004", "CN");
      AssertRFF(gnList[3], "1005", "AU");
      AssertRFF(gnList[4], "1006", "US");
      AssertRFF(restList[0], "Main Notify Party 1002:US", "US");
    }

    [TestMethod]
    public void TestMultipleRFFListHelper_IncludeRregulatingCountry()
    {
      var helper = Helper();
      helper.CalculateRFFList(1, "false");

      var gnList = helper.GNList();
      var restList = helper.RestList();

      Assert.AreEqual(3, gnList.Count);
      Assert.AreEqual(3, restList.Count);

      AssertRFF(gnList[0], "1003", "");
      AssertRFF(gnList[1], "1004", "");
      AssertRFF(gnList[2], "1005", "");
      AssertRFF(restList[0], "Main Notify Party 1001", "");
      AssertRFF(restList[1], "Main Notify Party 1002", "");
      AssertRFF(restList[2], "Consignor AAA:1006", "");
    }
  }
}