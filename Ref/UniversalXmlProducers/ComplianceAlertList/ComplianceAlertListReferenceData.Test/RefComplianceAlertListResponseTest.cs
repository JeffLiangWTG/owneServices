using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.ComplianceAlertListReferenceData.Business;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ComplianceAlertListReferenceData.Test
{
	[TestFixture]
	public class ComplianceAlertListResponseTest
	{
		[Test]
		public void TestSerializeAndDeserialize()
		{
			var complianceAlertList = new ComplianceCountryAlertDetailsResponseModel
			{
				CountryCode = "CA",
				ExportAlerts = new List<ComplianceAlertDetailsModel>
				{
					new ComplianceAlertDetailsModel
					{
						Code = "exportControl2022",
						Name = "Export Control List",
						Description = "The Canada Export Control List (ECL) identifies items and technologies that are subject to export controls. These controls are designed to ensure that Canada’s exports align with its national  security, foreign policy, and trade objectives, as well as international agreements. Alerts include Munitions List, Dual Use, and Technology.",
						NomenclatureWide = false,
						CommoditySpecific = true,
						IsActive = true,
						ContentUrl = "/?tariff=040490&amp;c=CA&amp;impexp=E&amp;tab=exportControl2022"
					},
					new ComplianceAlertDetailsModel
					{
						Code = "exportControl2022_ecl",
						Name = "ECL",
						Description = "",
						NomenclatureWide = false,
						CommoditySpecific = true,
						IsActive = true,
						ContentUrl = "/?tariff=040490&amp;c=CA&amp;impexp=E&amp;tab=exportControl2022_ecl"
					}
				},
	ImportAlerts = new List<ComplianceAlertDetailsModel>
				{
					new ComplianceAlertDetailsModel
					{
						Code = "hazardWasteMaterial",
						Name = "Hazardous Waste Material",
						Description = "Prohibition, management, and disposal of hazardous waste materials based on OECD lists and the Basel Convention.",
						NomenclatureWide = false,
						CommoditySpecific = true,
						IsActive = true,
						ContentUrl = "/?tariff=0507&amp;c=CA&amp;impexp=E&amp;tab=hazardWasteMaterial"
					},
					new ComplianceAlertDetailsModel
					{
						Code = "russiaBelarusSanctionsExport",
						Name = "Sanctioned Goods",
						Description = "Canada implements Special Economic Measures to impose trade restrictions in response to international situations. Specific goods exported to Russia, Belarus, Ukraine, North Korea, Iran and Syria are covered.",
						NomenclatureWide = true,
						CommoditySpecific = false,
						IsActive = true,
						ContentUrl = "/?tariff=0101&amp;c=CA&amp;impexp=E&amp;tab=russiaBelarusSanctionsExport"
					}
				}
			};

			var serializedStr = JsonConvert.SerializeObject(complianceAlertList);
			var deserializedComplianceCommodityAlertList = JsonConvert.DeserializeObject<ComplianceCountryAlertDetailsResponseModel>(serializedStr);
			Assert.Multiple(() =>
			{
				Assert.AreEqual(complianceAlertList.CountryCode, deserializedComplianceCommodityAlertList.CountryCode);
				Assert.AreEqual(complianceAlertList.ExportAlerts.Count, deserializedComplianceCommodityAlertList.ExportAlerts.Count);
				Assert.AreEqual(complianceAlertList.ImportAlerts.Count, deserializedComplianceCommodityAlertList.ImportAlerts.Count);
				foreach (var alert in complianceAlertList.ExportAlerts)
				{
					var deserializedAlert = deserializedComplianceCommodityAlertList.ExportAlerts.FirstOrDefault(a => a.Code == alert.Code);
					Assert.Multiple(() =>
					{
						Assert.AreEqual(alert.Code, deserializedAlert.Code);
						Assert.AreEqual(alert.Name, deserializedAlert.Name);
						Assert.AreEqual(alert.Description, deserializedAlert.Description);
						Assert.AreEqual(alert.NomenclatureWide, deserializedAlert.NomenclatureWide);
						Assert.AreEqual(alert.CommoditySpecific, deserializedAlert.CommoditySpecific);
						Assert.AreEqual(alert.IsActive, deserializedAlert.IsActive);
						Assert.AreEqual(alert.ContentUrl, deserializedAlert.ContentUrl);
					});
				}
				foreach (var alert in complianceAlertList.ImportAlerts)
				{
					var deserializedAlert = deserializedComplianceCommodityAlertList.ImportAlerts.FirstOrDefault(a => a.Code == alert.Code);
					Assert.Multiple(() =>
					{
						Assert.AreEqual(alert.Code, deserializedAlert.Code);
						Assert.AreEqual(alert.Name, deserializedAlert.Name);
						Assert.AreEqual(alert.Description, deserializedAlert.Description);
						Assert.AreEqual(alert.NomenclatureWide, deserializedAlert.NomenclatureWide);
						Assert.AreEqual(alert.CommoditySpecific, deserializedAlert.CommoditySpecific);
						Assert.AreEqual(alert.IsActive, deserializedAlert.IsActive);
						Assert.AreEqual(alert.ContentUrl, deserializedAlert.ContentUrl);
					});
				}
			});
		}
	}
}
