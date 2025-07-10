using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.EUReferenceData.Services
{
	public static class CusNumberSOAPService
	{
		public static string CreateRequest(IEnumerable<string> cusNumbers)
		{
			XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
			XNamespace chem = "http://chemicalsubstanceforWS.ws.ecics.dds.s/";
			var request = new XDocument(
					new XElement(soapenv + "Envelope",
						new XElement(soapenv + "Header"),
						new XElement(soapenv + "Body",
							new XElement(chem + "chemicalSubstanceForWs",
								cusNumbers.Select(s => new XElement(chem + "cusNumber", s))
							)
						)
					)
				);
			return request.ToString();
		}
	}
}
