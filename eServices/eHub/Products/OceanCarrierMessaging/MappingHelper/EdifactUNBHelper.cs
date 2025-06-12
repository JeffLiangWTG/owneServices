using CargoWise.eHub.Core.Transforms.Helper;
using System;
using System.Text.RegularExpressions;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper
{
  public class EdifactUNBHelper
  {
    public EdifactUNBHelper()
    {
      this.codeMapper = new CodeMapper();
    }
    public EdifactUNBHelper(CodeMapper codeMapper)
    {
      this.codeMapper = codeMapper;
    }


    #region Syntax Identifier

    public string GetSyntaxIdentifier(string unbSegment)
    {
      return GetUNBData(unbSegment, 1, 0);
    }

    #endregion

    #region Interchange Sender

    public string GetSender(string unbSegment)
    {
      return GetUNBData(unbSegment, 2, 0);
    }

    public string GetSenderCode(string unbSegment)
    {
      return GetUNBData(unbSegment, 2, 1);
    }

    #endregion

    #region Interchange Recipient

    public string GetRecipient(string unbSegment)
    {
      return GetUNBData(unbSegment, 3, 0);
    }

    public string GetRecipientCode(string unbSegment)
    {
      return GetUNBData(unbSegment, 3, 1);
    }

    #endregion

    #region Date and Time

    public string GetDate(string unbSegment)
    {
      return GetUNBData(unbSegment, 4, 0);
    }

    public string GetTime(string unbSegment)
    {
      return GetUNBData(unbSegment, 4, 1);
    }

    public string GetDateTime(string unbSegment)
    {
      DateTime outputDate;
      if (DateTime.TryParseExact(GetDate(unbSegment) + GetTime(unbSegment), new[] { "yyyyMMdd", "yyyyMMddHHmm", "yyMMddHHmm" }, null, System.Globalization.DateTimeStyles.None, out outputDate))
      {
        return outputDate.ToString("s");
      }

      return string.Empty;
    }

    #endregion

    #region Interchage Identifier

    public string GetInterchangeIdentifier(string unbSegment)
    {
      var interchangeID = GetUNBData(unbSegment, 5, 0);

      if (string.IsNullOrEmpty(interchangeID) || !Regex.IsMatch(interchangeID, "^[a-zA-Z0-9]*$"))
      {
        interchangeID = CodeMapper.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.OCMCNF.UNB", "@maxlength", "14");
      }

      return interchangeID;
    }

    #endregion

    #region Implementation

    string GetUNBData(string unbSegment, int segment, int subSegment)
    {
      if (!string.IsNullOrEmpty(unbSegment))
      {
        try
        {
          return unbSegment.Split(new[] { '\'' })[0].Split(new[] { '+' })[segment].Split(new[] { ':' })[subSegment];
        }
        catch(Exception)
        {
          return string.Empty;
        }
      }

      return string.Empty;
    }

    CodeMapper CodeMapper
    {
      get
      {
        if (codeMapper == null)
        {
          codeMapper = new CodeMapper();
        }

        return codeMapper;
      }
    }
    CodeMapper codeMapper;

    #endregion
  }
}
