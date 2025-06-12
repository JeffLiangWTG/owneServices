using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataAccess.Sql;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper
{
  public class OCMHelper
  {
    const string SHIPPING_INSTRUCTION = "SHIPPING_INSTRUCTION";

    const string OCMSystemConfiguration = "OCM System Configuration";

    ClientRegistrationAccessor ClientRegistrationAccessor
    {
      get { return clientRegistrationAccessor ?? (clientRegistrationAccessor = new ClientRegistrationAccessor()); }
    }
    ClientRegistrationAccessor clientRegistrationAccessor;

    public virtual string IsCoLoad(string shipmentType)
    {
      var result = "FALSE";

      if (!string.IsNullOrEmpty(shipmentType))
      {
        result = CodeMapper.GetRecipientCode(SHIPPING_INSTRUCTION, SHIPPING_INSTRUCTION, OCMSystemConfiguration, "NVOCC", "Is Co-load", shipmentType);
      }

      return result.ToUpper();
    }

    public virtual string IsMultiPickup(string serviceProvider, string scac)
    {
      var serviceProviderValue = !string.IsNullOrEmpty(serviceProvider) ? serviceProvider : string.Empty;
      var scacValue = !string.IsNullOrEmpty(scac) ? scac : string.Empty;

      var result = CodeMapper.GetRecipientCode(SHIPPING_INSTRUCTION, SHIPPING_INSTRUCTION, OCMSystemConfiguration, "Transport Booking", "MultiPickup", serviceProviderValue, scacValue);

      return result.ToUpper();
    }

    public virtual string IsMultiDropOff(string serviceProvider, string scac)
    {
      var serviceProviderValue = !string.IsNullOrEmpty(serviceProvider) ? serviceProvider : string.Empty;
      var scacValue = !string.IsNullOrEmpty(scac) ? scac : string.Empty;

      var result = CodeMapper.GetRecipientCode(SHIPPING_INSTRUCTION, SHIPPING_INSTRUCTION, OCMSystemConfiguration, "Transport Booking", "MultiDropOff", serviceProviderValue, scacValue);

      return result.ToUpper();
    }

    public virtual string GetPayableElseWhereOutputCode(string serviceProvider)
    {
      if (string.IsNullOrEmpty(serviceProvider))
      {
        return string.Empty;
      }

      if (IsSupportedPayableElseWhere(serviceProvider))
      {
        return CodeMapper.GetRecipientCode(SHIPPING_INSTRUCTION, SHIPPING_INSTRUCTION, OCMSystemConfiguration, "Payable ElseWhere", "OutputCode", serviceProvider);
      }

      return string.Empty;
    }

    public virtual string GetPayableElseWhereDescription(string serviceProvider)
    {
      if (string.IsNullOrEmpty(serviceProvider))
      {
        return string.Empty;
      }

      if (IsSupportedPayableElseWhere(serviceProvider))
      {
        return CodeMapper.GetRecipientCode(SHIPPING_INSTRUCTION, SHIPPING_INSTRUCTION, OCMSystemConfiguration, "Payable ElseWhere", "Description", serviceProvider);
      }

      return string.Empty;
    }

    bool IsSupportedPayableElseWhere(string serviceProvider)
    {
      var result = CodeMapper.GetRecipientCode(SHIPPING_INSTRUCTION, SHIPPING_INSTRUCTION, OCMSystemConfiguration, "Payable ElseWhere", "IsSupported", serviceProvider);

      bool.TryParse(result, out var IsPayableElseWhere);

      return IsPayableElseWhere;
    }

    public virtual string GetShippingLineSCAC(string carrierC1CCode)
    {
      if (string.IsNullOrEmpty(carrierC1CCode))
      {
        return string.Empty;
      }

      return CodeMapper.CallActionProcedureHelper("SelectShippingLineStandardCode", "@stdCode", "@cw1Code", carrierC1CCode);
    }

    public virtual string GetServiceProvider(string carrierID)
    {
      var result = string.Empty;
      if (!string.IsNullOrEmpty(carrierID))
      {
        List<string> ignoredCarrierIDList = new List<string>()
        {
          "HAPAG_LLOYD"
        };

        if (ignoredCarrierIDList.Contains(carrierID))
        {
          return carrierID;
        }

        var firstIndex = carrierID.IndexOf("_", StringComparison.Ordinal);
        if (firstIndex > 0)
        {
          var secondIndex = carrierID.IndexOf("_", firstIndex + 1, StringComparison.Ordinal);
          var helper = new StringHelper();
          if (secondIndex > 0)
          {
            result = helper.SubstringSafe(carrierID, 0, secondIndex);
          }
          else
          {
            result = helper.SubstringSafe(carrierID, 0, firstIndex);
          }
        }
        else
        {
          result = carrierID;
        }
      }
      return result;
    }

    public bool IsValidContainerNumber(string containerNumber)
    {
      return Regex.IsMatch(containerNumber, @"^([\D]{4}[\d]{7})$");
    }

    public CodeMapper CodeMapper
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

    public void SetTestCodeMapping(CodeMapper testCodeMapper)
    {
      codeMapper = testCodeMapper;
    }

    public string GetUEventProcessLog(string eventLog)
    {
      if (string.IsNullOrEmpty(eventLog))
      {
        return string.Empty;
      }

      const string matchPattern = @"\[\*(.+)\*\]";

      var wrapper = new StringWrapper();
      wrapper.SetupStringWrapper(512, 100, true);
      wrapper.SplitStringInWrapper(eventLog);
      var resultList = new ListHelper();

      for (int i = 0; i < wrapper.Count(); i++)
      {
        var log = wrapper.GetTextFromCurrentIndex().Trim();
        var match = Regex.Match(log, matchPattern);
        if (match.Success)
        {
          resultList.AddToListIfNotExists(match.Groups[1].Value);
        }
      }

      return resultList.ToStringWithNewLine();
    }

    const int MaximumImageSize = 5000000;

    public string GetAttachmentInBinary(string url)
    {
      try
      {
        using (var response = WebRequest.Create(url).GetResponse())
        using (var br = new BinaryReader(response.GetResponseStream()))
        {
          var bytes = br.ReadBytes(MaximumImageSize);

          return Convert.ToBase64String(bytes);
        }
      }
      catch (Exception)
      {
        return string.Empty;
      }
    }

    public string GetTransportMode(string transportMode, string additionalTransportMode)
    {
      var transportModeValue = transportMode ?? string.Empty;
      var additionalTransportModeValue = additionalTransportMode ?? string.Empty;

      switch (transportModeValue)
      {
        case TransportModeConstant.Sea:
          return TransportModeConstant.Sea;
        case TransportModeConstant.Rail:
          switch (additionalTransportModeValue)
          {
            case TransportModeConstant.Road:
              return TransportModeConstant.RailRoad;
            default:
              return TransportModeConstant.Rail;
          }
        case TransportModeConstant.Road:
          return TransportModeConstant.Road;
        case TransportModeConstant.InlandWaterway:
          switch (additionalTransportModeValue)
          {
            case TransportModeConstant.Rail:
              return TransportModeConstant.RailWater;
            case TransportModeConstant.Road:
              return TransportModeConstant.RoadWater;
            default:
              return TransportModeConstant.InlandWaterway;
          }
        default:
          return string.Empty;
      }
    }

    public virtual string GetAdministrativeRegion(string port)
    {
      if (!string.IsNullOrEmpty(port))
      {
        return CodeMapper.GetRecipientCode(SHIPPING_INSTRUCTION, SHIPPING_INSTRUCTION, OCMSystemConfiguration, "UNLOCO_Lookup", "AdministrativeRegion", port);
      }

      return string.Empty;
    }

    public virtual void InsertClientRegistration(string clientId, string registrationType, string qualifier, string code, int flag1, string xmlString, string attr1, string password1)
    {
      var issuedUTC = DateTime.UtcNow;
      ClientRegistrationAccessor.Insert(clientId, registrationType, qualifier, code, flag1, xmlString, attr1, password1, issuedUTC, issuedUTC.AddDays(30));
    }
  }
}