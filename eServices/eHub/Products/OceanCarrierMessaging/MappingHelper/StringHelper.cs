using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper
{
  public class StringHelper
  {
    public bool ContainsAny(string valueList, string value)
    {
      if (string.IsNullOrEmpty(valueList) || string.IsNullOrEmpty(value))
      {
        return false;
      }

      if (valueList.Contains(";"))
      {
        string[] values = valueList.Split(';');
        foreach (string item in values)
        {
          if (item == value)
          {
            return true;
          }
        }
      }
      else
      {
        return valueList.Contains(value);
      }

      return false;
    }

    public string GetValueOrDefault(string inputText)
    {
      var result = "";
      if (!string.IsNullOrEmpty(inputText))
      {
        result = inputText;
      }

      return result;
    }

    #region SubStringSafe

    public string SubstringSafe(string inputText, int startIndex)
    {
      return SubstringSafe(inputText, startIndex, int.MaxValue);
    }

    public string SubstringSafe(string inputText, int startIndex, int length)
    {
      string result = "";

      if (startIndex < 0)
      {
        startIndex = 0;
      }

      int actualLength = inputText.Length - startIndex;
      if (actualLength > 0)
      {
        if (actualLength > length && length >= 0)
        {
          actualLength = length;
        }
        result = inputText.Substring(startIndex, actualLength);
      }

      return result;
    }

    #endregion

    #region ReplaceCRLFText

    public string Trim(string inputText)
    {
      return ReplaceCRLFTextWithNewLine(inputText).TrimEnd('\r', '\n').Trim();
    }

    public string ReplaceCRLFTextWithNewLine(string inputText)
    {
      return ReplaceText(inputText, "\r\n", "\n");
    }

    public string ReplaceCRLFText(string inputText)
    {
      var result = "";
      result = ReplaceText(inputText, "\r\n", " ");
      result = ReplaceText(result, "\n", " ");

      return result;
    }

    public string ShrinkCRLF(string inputText)
    {
      return ShrinkChar(ReplaceCRLFTextWithNewLine(inputText), '\n');
    }

    public string TrimCRLF(string inputText)
    {
      return ReplaceCRLFTextWithNewLine(inputText).Trim('\n');
    }

    public string ReplaceText(string inputText, string oldValue, string newValue)
    {
      var result = "";
      if (!string.IsNullOrEmpty(inputText))
      {
        result = inputText.Replace(oldValue, newValue);
      }

      return result;
    }

    #endregion

    #region Implemetation

    public string PadRight(string inputText, int totalWidth, string paddingCharacter)
    {
      var result = "";
      if (totalWidth > 0)
      {
        char paddingChar;
        if (Char.TryParse(paddingCharacter, out paddingChar))
        {
          result = inputText.PadRight(totalWidth, paddingChar);
        }
        else
        {
          result = inputText.PadRight(totalWidth, ' ');
        }
      }

      return result;
    }

    public string ShrinkSpaces(string inputText)
    {
      return ShrinkChar(inputText.Trim(), ' ');
    }

    public string ToUpper(string inputText)
    {
      string result = "";
      if (!string.IsNullOrEmpty(inputText))
      {
        result = inputText;
      }
      return result.ToUpper();
    }

    public string StringReplace(string inputText, string oldValue, string newValue)
    {
      string result = "";
      if (!string.IsNullOrEmpty(inputText))
      {
        result = inputText.Replace(oldValue, newValue);
      }
      return result;
    }

    public bool MatchPattern(string text, string format)
    {
      return Regex.IsMatch(text, format);
    }

    public bool IsNewFormMessage(string formVersion)
    {
      return IsNewFormMessage(formVersion, "1.0");
    }

    public bool IsNewFormMessage(string formVersion, string compareFormVersion)
    {
      if (string.IsNullOrEmpty(formVersion))
      {
        return false;
      }

      var versionWithoutDot = formVersion.Replace(".", "");

      if (!versionWithoutDot.All(char.IsDigit) || versionWithoutDot.Length < 2)
      {
        return false;
      }

      formVersion = formVersion.Count(c => c == '.') == 1
                    ? formVersion + ".0"
                    : formVersion;
      compareFormVersion = compareFormVersion.Count(c => c == '.') == 1
                    ? compareFormVersion + ".0"
                    : compareFormVersion;

      var newFormVerssion = new Version(formVersion);
      var currentFormVersion = new Version(compareFormVersion);

      return newFormVerssion >= currentFormVersion;
    }

    public int CharCount(string inputText, string charToCount)
    {
      var result = 0;

      if (!string.IsNullOrEmpty(inputText))
      {
        var strInCharArray = charToCount.ToCharArray();
        if (strInCharArray.Length > 0)
        {
          result = inputText.Count(c => c.Equals(strInCharArray[0]));
        }
      }

      return result;
    }

    public string GetIndexOfValue(string inputText, string value, int position)
    {
      var result = "";
      if (!string.IsNullOrEmpty(inputText))
      {
        int offset = inputText.IndexOf(value, StringComparison.Ordinal);
        for (int i = 0; i < position - 1; i++)
        {
          if (offset == -1)
          {
            break;
          }

          offset = inputText.IndexOf(value, offset + 1, StringComparison.Ordinal);
        }

        if (offset != -1)
        {
          result = SubstringSafe(inputText, 0, offset);
        }
      }
      return result;
    }

    string ShrinkChar(string inputText, char charToShrink)
    {
      string[] arry = inputText.Split(new char[] { charToShrink }, StringSplitOptions.RemoveEmptyEntries);
      return string.Join(charToShrink.ToString(), arry);
    }

    #endregion
  }
}
