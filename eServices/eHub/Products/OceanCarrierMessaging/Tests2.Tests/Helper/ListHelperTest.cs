using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests2.Tests.ListHelperTest
{
  [TestClass]
  public class ListHelperTest
  {
    [TestMethod]
    public void TestClearList()
    {
      var listHelper = new ListHelper();

      Assert.AreEqual(0, listHelper.ListCount());

      listHelper.AddToList("01");
      Assert.AreEqual(1, listHelper.ListCount());

      listHelper.ClearList();
      Assert.AreEqual(0, listHelper.ListCount());
    }

    [TestMethod]
    public void TestAddToList()
    {
      var listHelper = new ListHelper();
      Assert.AreEqual(0, listHelper.ListCount());

      listHelper.AddToList(null);
      listHelper.AddToList("");
      listHelper.AddToList("01");

      Assert.AreEqual(1, listHelper.ListCount());
    }

    [TestMethod]
    public void TestAddToListIfNotExists()
    {
      var listHelper = new ListHelper();
      Assert.AreEqual(0, listHelper.ListCount());

      listHelper.AddToListIfNotExists(null);
      listHelper.AddToListIfNotExists("");
      listHelper.AddToListIfNotExists("01");
      listHelper.AddToListIfNotExists("01");
      listHelper.AddToListIfNotExists("01");
      listHelper.AddToListIfNotExists("01");
      listHelper.AddToListIfNotExists("01");
      listHelper.AddToListIfNotExists("01");
      listHelper.AddToListIfNotExists("02");

      Assert.AreEqual(2, listHelper.ListCount());
    }

    [TestMethod]
    public void TestShouldCreateItem()
    {
      var listHelper = new ListHelper();
      Assert.AreEqual(0, listHelper.ListCount());

      Assert.AreEqual(true, listHelper.ShouldCreateItem("KEY", "VALUE_1"));
      Assert.AreEqual(true, listHelper.ShouldCreateItem("KEY", "VALUE_2"));
      Assert.AreEqual(false, listHelper.ShouldCreateItem("KEY", "VALUE_1"));


      Assert.AreEqual(true, listHelper.ShouldCreateItem("XXX", "VALUE_1", 3));
      Assert.AreEqual(true, listHelper.ShouldCreateItem("XXX", "VALUE_2", 3));
      Assert.AreEqual(true, listHelper.ShouldCreateItem("XXX", "VALUE_3", 3));
      Assert.AreEqual(false, listHelper.ShouldCreateItem("XXX", "VALUE_3", 3));
      Assert.AreEqual(false, listHelper.ShouldCreateItem("XXX", "VALUE_4", 3));
    }

    [TestMethod]
    public void TestToStringWithDelimeter()
    {
      var listHelper = new ListHelper();
      Assert.AreEqual(0, listHelper.ListCount());
      Assert.AreEqual("", listHelper.ToStringWithDelimiter("; "));

      listHelper.AddToListIfNotExists("01");
      listHelper.AddToListIfNotExists("02");
      listHelper.AddToListIfNotExists("03");

      Assert.AreEqual(3, listHelper.ListCount());
      Assert.AreEqual("01; 02; 03", listHelper.ToStringWithDelimiter("; "));
    }


    [TestMethod]
    public void TestGetFirstOrDefault()
    {
      var listHelper = new ListHelper();
      Assert.AreEqual(0, listHelper.ListCount());
      Assert.AreEqual("", listHelper.GetFirstOrDefault());

      listHelper.AddToListIfNotExists("XXX");

      Assert.AreEqual(1, listHelper.ListCount());
      Assert.AreEqual("XXX", listHelper.GetFirstOrDefault());
    }

  }
}