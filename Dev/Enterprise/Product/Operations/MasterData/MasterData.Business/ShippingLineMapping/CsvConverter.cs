using System;
using System.Linq;
using System.Xml.Linq;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterData.Business
{
	[CodeAlive("For Documetation purposes")]
	public static class CsvConverter
	{
		public static XElement ConvertToXml(string[] data)
		{
			XElement result = new XElement(Res.GetString("43009552-94CD-416E-ABFF-90692403DC17", "Carriers"),
				from row in data
				let column = row.Split(',')
				select new XElement(Res.GetString("03F7B34C-B311-46E8-8DD7-959D57731DE6", "Carrier"),

					new XElement("RSL_PK", Guid.NewGuid()),
					new XElement("RSL_IsSystem", 1),
					new XElement("RSL_IsActive", 1),
					new XElement("RSL_IsNVO",
						Convert.ToInt32(column[0].Any())),
					new XElement("RSL_CarrierName", column[2]),
					new XElement("RSL_StandardCarrierAlphaCode",
						column[3].Any()
							? column[3]
							: Guid.NewGuid().ToString().Substring(0, 4)),
					new XElement("RSL_CargoWiseOneCode",
						column[4].Any()
							? column[4]
							: Guid.NewGuid().ToString().Substring(0, 4)),
					new XElement("RSL_OceanCarrierMessagingAvailable",
						Convert.ToInt32(column[5].Any() || column[9].Any() || column[10].Any() || column[11].Any())),
					new XElement("RSL_GlobalSailingScheduleAvailable",
						Convert.ToInt32(column[6].Any() || column[8].Any())),
					new XElement("RSL_ContainerAutomationAvailable",
						Convert.ToInt32(column[7].Any())),
					new XElement("RSL_CargoSphereRatesAvailable", 0),
					new XElement("RSL_InvoiceAvailable", 0)
				)
			);
			return result;
		}
	}
}
