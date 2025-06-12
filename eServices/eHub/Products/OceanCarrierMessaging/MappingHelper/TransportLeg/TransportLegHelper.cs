using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper
{
  public class TransportLegHelper
  {
    public void AddTransportLeg(string legOrder, string legType, string transportMode, string portOfLoadingcode, string portOfLoadingname, string portOfDischargecode, string portOfDischargename, string vesselName, string voyageFlightNo)
    {
      var transportleg = new TransportLeg();

      transportleg.OldLegOrder = legOrder ?? string.Empty;
      transportleg.LegOrder = legOrder ?? string.Empty;
      transportleg.TransportMode = transportMode ?? string.Empty;
      transportleg.LegType = legType ?? string.Empty;
      transportleg.PortOfLoadingCode = portOfLoadingcode ?? string.Empty;
      transportleg.PortOfLoadingName = portOfLoadingname ?? string.Empty;
      transportleg.PortOfDischargeCode = portOfDischargecode ?? string.Empty;
      transportleg.PortOfDischargeName = portOfDischargename ?? string.Empty;
      transportleg.VesselName = vesselName ?? string.Empty;
      transportleg.VoyageFlightNo = voyageFlightNo ?? string.Empty;

      transportLegs.Add(transportleg);
    }

    public List<TransportLeg> NewTransportLegs()
    {
      return transportLegs;
    }

    List<TransportLeg> transportLegs = new List<TransportLeg>();

    public void Reset()
    {
      transportLegs = new List<TransportLeg>();
    }

    public int Count()
    {
      return transportLegs.Count;
    }

    bool IsContainsInlandWaterName(string name)
    {
      var nameInUpper = name.ToUpper();
      return nameInUpper.Contains("BARGE") || nameInUpper.Contains("WATER") || nameInUpper.Contains("FERRY");
    }

    bool IsPromoteToMain(string vesselName, string voyageFlightNo, string portOfLoadingCode, string portOfDischargeCode)
    {
      var portOfLoadingCountry = portOfLoadingCode.Length > 2 ? portOfLoadingCode.Substring(0, 2) : string.Empty;
      var portOfDischargeCountry = portOfDischargeCode.Length > 2 ? portOfDischargeCode.Substring(0, 2) : string.Empty;

      return ((!string.IsNullOrEmpty(vesselName) && !string.IsNullOrEmpty(voyageFlightNo)) || portOfLoadingCountry != portOfDischargeCountry)
              && !IsContainsInlandWaterName(vesselName)
              && !IsContainsInlandWaterName(voyageFlightNo);
    }

    public void CalculateTransportLegs()
    {
      CalculateSeaLegByLegType(LegTypeConstant.PreCarriage);
      CalculateSeaLegByLegType(LegTypeConstant.OnForwarding);
      CalculateNewTransportLegCollection();
    }

    void CalculateSeaLegByLegType(string legType)
    {
      if (transportLegs.Count == 0)
      {
        return;
      }

      var legs = transportLegs.Where(s => s.LegType == legType && (s.TransportMode == TransportModeConstant.Sea)).OrderBy(s => s.LegOrder);

      if (legs != null && legs.Count() > 0)
      {
        foreach (var leg in legs)
        {
          var vesselName = leg.VesselName;
          var voyageFlightNo = leg.VoyageFlightNo;
          var portOfLoadingCode = leg.PortOfLoadingCode;
          var portOfDischargeCode = leg.PortOfDischargeCode;

          if (IsPromoteToMain(vesselName, voyageFlightNo, portOfLoadingCode, portOfDischargeCode))
          {
            leg.LegType = LegTypeConstant.Main;
          }
        }
      }
    }

    void CalculateNewTransportLegCollection()
    {
      var mainlegs = transportLegs.Where(s => s.LegType == LegTypeConstant.Main).OrderBy(t => t.LegOrder);
      var preCarriageLeg = transportLegs.Where(s => s.LegType == LegTypeConstant.PreCarriage).OrderBy(t => t.LegOrder);
      var onForwardingLeg = transportLegs.Where(s => s.LegType == LegTypeConstant.OnForwarding).OrderBy(t => t.LegOrder);

      transportLegs = new List<TransportLeg>();
      if (preCarriageLeg.Any())
      {
        transportLegs.AddRange(preCarriageLeg);
      }

      if (mainlegs.Any())
      {
        transportLegs.AddRange(mainlegs);
      }

      if (onForwardingLeg.Any())
      {
        transportLegs.AddRange(onForwardingLeg);
      }

      var order = 0;
      transportLegs.ForEach(t => t.LegOrder = (++order).ToString());
    }

    public XPathNodeIterator NewTransportLegCollection()
    {
      XmlDocument doc = new XmlDocument();
      var transportCollection = doc.CreateElement("TransportLegCollection");
      doc.AppendChild(transportCollection);

      foreach (var transportLeg in transportLegs)
      {
        var transport = doc.CreateElement("TransportLeg");

        var oldLegOrderNode = doc.CreateElement(nameof(transportLeg.OldLegOrder));
        oldLegOrderNode.InnerText = transportLeg.OldLegOrder.ToString();
        transport.AppendChild(oldLegOrderNode);

        var legOrderNode = doc.CreateElement(nameof(transportLeg.LegOrder));
        legOrderNode.InnerText = transportLeg.LegOrder.ToString();
        transport.AppendChild(legOrderNode);

        var transportMode = doc.CreateElement(nameof(transportLeg.TransportMode));
        transportMode.InnerText = transportLeg.TransportMode.ToString();
        transport.AppendChild(transportMode);

        var legTypeNode = doc.CreateElement(nameof(transportLeg.LegType));
        legTypeNode.InnerText = transportLeg.LegType;
        transport.AppendChild(legTypeNode);

        var portOfDischargeCodeNode = doc.CreateElement(nameof(transportLeg.PortOfDischargeCode));
        portOfDischargeCodeNode.InnerText = transportLeg.PortOfDischargeCode;
        transport.AppendChild(portOfDischargeCodeNode);

        var portOfDischargeNameNode = doc.CreateElement(nameof(transportLeg.PortOfDischargeName));
        portOfDischargeNameNode.InnerText = transportLeg.PortOfDischargeName;
        transport.AppendChild(portOfDischargeNameNode);

        var portOfLoadingCodeNode = doc.CreateElement(nameof(transportLeg.PortOfLoadingCode));
        portOfLoadingCodeNode.InnerText = transportLeg.PortOfLoadingCode;
        transport.AppendChild(portOfLoadingCodeNode);

        var portOfLoadingNameNode = doc.CreateElement(nameof(transportLeg.PortOfLoadingName));
        portOfLoadingNameNode.InnerText = transportLeg.PortOfLoadingName;
        transport.AppendChild(portOfLoadingNameNode);

        var vesselName = doc.CreateElement(nameof(transportLeg.VesselName));
        vesselName.InnerText = transportLeg.VesselName;
        transport.AppendChild(vesselName);

        var voyageFlightNo = doc.CreateElement(nameof(transportLeg.VoyageFlightNo));
        voyageFlightNo.InnerText = transportLeg.VoyageFlightNo;
        transport.AppendChild(voyageFlightNo);

        transportCollection.AppendChild(transport);
      }

      return doc.DocumentElement?.CreateNavigator().Select("TransportLeg");
    }
  }
}
