using System;
using System.Globalization;
using System.Xml.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Business.Message;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4.Updates;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;

sealed class TaricUpdateInitiator(IMessageSender messageSender)
{
	public string GenerateAndSendTariffUpdateRequest(UpdateRequest updateRequest)
	{
		try
		{
			var xmlDoc = GenerateTariffUpdateRequest(updateRequest.StartDate, updateRequest.EndDate);
			return messageSender.SendTariffUpdateRequest(xmlDoc);
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(ex.Message);
			return null;
		}
	}

	static XDocument GenerateTariffUpdateRequest(DateTime startDate, DateTime endDate)
	{
		var tns = Constants.Soap.SoapNamespaces.Isztar;
		var xsi = Constants.Soap.SoapNamespaces.Xsi;

		var result = new XDocument(new XDeclaration("1.0", "utf-8", null),
			new XElement(tns + Constants.Soap.XConstants.IsztarHistoryRequest,
				new XAttribute(XNamespace.Xmlns + Constants.Soap.SoapNamespaces.IsztarAlias, tns.NamespaceName),
				new XAttribute(XNamespace.Xmlns + Constants.Soap.SoapNamespaces.XsiAlias, xsi.NamespaceName),
				new XElement(Constants.Soap.XConstants.AcceptObligation, "true"),
				new XElement(Constants.Soap.XConstants.StartDate, startDate.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)),
				new XElement(Constants.Soap.XConstants.EndDate, endDate.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture))));

		return result;
	}
}
