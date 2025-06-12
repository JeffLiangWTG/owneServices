using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper
{
  public class ListHelper
  {
    List<string> dataList = new List<string>();

    public void ClearList()
    {
      dataList.Clear();
    }

    public void AddToList(string inputText)
    {
      if (!string.IsNullOrEmpty(inputText))
      {
        dataList.Add(inputText);
      }
    }

    public bool AddToListIfNotExists(string inputText)
    {
      var result = false;
      if (!string.IsNullOrEmpty(inputText) && !dataList.Contains(inputText))
      {
        dataList.Add(inputText);
        result = true;
      }

      return result;
    }

    public int ListCount()
    {
      return dataList.Count;
    }

    public string GetFirstOrDefault()
    {
      return dataList.Count > 0
        ? dataList[0]
        : string.Empty;
    }

    public string ToStringWithDelimiter(string delimeter)
    {
      return dataList.Count > 0
        ? string.Join(delimeter, dataList)
        : string.Empty;
    }

    public string ToStringWithNewLine()
    {
      StringBuilder builder = new StringBuilder(dataList.Count);
      foreach (string value in dataList)
      {
        if (builder.Length > 0)
        {
          builder.Append("\n");
        }
        builder.Append(value);

      }
      return builder.ToString();
    }


    public bool ShouldCreateItem(string key, string inputText)
    {
      return ShouldCreateItem(key, inputText, 0);
    }

    public bool ShouldCreateItem(string key, string inputText, int maxLimit)
    {
      if (maxLimit > 0 && dataList.Count(x => x.StartsWith(key)) == maxLimit)
      {
        return false;
      }

      if (string.IsNullOrEmpty(inputText) || dataList.Contains(key + inputText))
      {
        return false;
      }

      dataList.Add(key + inputText);
      return true;
    }

  }
}