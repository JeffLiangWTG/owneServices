using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TaxRateImporterTest : TransactionedTestCase
	{
		readonly BusinessObjectFactory Factory = new BusinessObjectFactory();

		public void TestImportForSpainCountry()
		{
			var spainCompanyWithoutCanaryIslandBranch = Factory.NewWithValidTestData<GlbCompany>();
			spainCompanyWithoutCanaryIslandBranch.SetCountry(Constants.CountryCodes.Spain);
			spainCompanyWithoutCanaryIslandBranch.GC_Code = "ESC";
			var spainBranch = spainCompanyWithoutCanaryIslandBranch.Branches.AddNew();
			spainBranch.GB_Code = "ESB";
			AssertTaxIDsForSpainCompany(spainCompanyWithoutCanaryIslandBranch, spainBranch, false);

			var spainCompanyWithTFBranch = Factory.NewWithValidTestData<GlbCompany>();
			spainCompanyWithTFBranch.SetCountry(Constants.CountryCodes.Spain);
			spainCompanyWithTFBranch.GC_Code = "EST";
			var tfBranch = spainCompanyWithTFBranch.Branches.AddNew();
			tfBranch.GB_Code = "EST";
			var tfState = Factory.LoadTop1<RefCountryStates>(new ZQuery(RefCountryStatesSchema.RW_Code, "TF"));
			var tfPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RW, tfState.PK));
			tfBranch.GB_RL_NKHomePort = tfPort.RL_Code;
			AssertTaxIDsForSpainCompany(spainCompanyWithTFBranch, tfBranch, true);
		}

		void AssertTaxIDsForSpainCompany(GlbCompany company, GlbBranch branch, bool isCanaryBranch)
		{
			var importer = new TaxRateImporter(company.GC_RN_NKCountryCode, company.PK, Factory);
			var taxQuery = new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, Constants.CountryCodes.Spain);
			taxQuery.AddToFilter(AccTaxRateSchema.AT_ExtraTaxRateType, AccTaxRate.ExtraTypes.RegionalTax);

			AssertEquals(true, Factory.Load<AccTaxRate>(taxQuery).Length == 0);
			importer.ImportFromXMLFile();
			var regionalTaxRates = Factory.Load<AccTaxRate>(taxQuery);
			var regionalTaxRateCount = regionalTaxRates.Length;
			if (isCanaryBranch)
			{
				AssertEquals("Regional Tax Rate count for Spain has changed. Please update the unit test accordingly.", 8, regionalTaxRateCount);
				importer.UpdatePostingGroupsFromXMLFile();
				var ratesList = regionalTaxRates.Select(x => $"{x.AT_Code} * {x.AT_PostingGroupId}").ToList();
				Assert(ratesList.Contains("CAPIGIC * 2"));
				Assert(ratesList.Contains("EXLIGIC * 2"));
				Assert(ratesList.Contains("EXTIGIC * 2"));
				Assert(ratesList.Contains("FREEIGIC * 2"));
				Assert(ratesList.Contains("IGIC * 2"));
				Assert(ratesList.Contains("IGIC3 * 2"));
				Assert(ratesList.Contains("NOTIGIC * 2"));
				Assert(ratesList.Contains("IGICREV * 2"));
			}
			else
			{
				AssertEquals(0, regionalTaxRateCount);
			}
		}

		public void TestReferenceRateType()
		{
			AssertReferenceRateType("LU", "TVA3", "RAT", "LOW2");
		}

		void AssertReferenceRateType(string countryCode, string taxID, string type, string expectedReferenceRateType)
		{
			var parser = new TaxRateXmlParser();
			var taxRatesByCountry = parser.BuildTaxRatesDictionaryBasedOnCountry();

			var node = taxRatesByCountry.Where(x => x.Key == countryCode).Select(y => y.Value.Where(z => z.TaxID == taxID && z.Type == type)).FirstOrDefault();
			AssertNotNull(node);
			AssertEquals(expectedReferenceRateType, node.FirstOrDefault().ReferenceRateType);
		}

		public void TestEveryCountryHasCorrespondingTaxRateUnitTest()
		{
			StringBuilder sb = new StringBuilder();
			List<string> processedCountryCode = new List<string>();
			using (var fileStream = TaxRateXmlParser.GetResourceStream_ForTest())
			{
				XmlDocument xml = new XmlDocument();
				xml.Load(fileStream);
				XmlElement rootNode = xml.DocumentElement;
				foreach (XmlNode node in rootNode)
				{
					string countryCode = node.SelectSingleNode("CountryCode").InnerText;
					if (!processedCountryCode.Contains(countryCode))
					{
						string methodName = string.Format("TestCopiedOrCreatedTaxRates_{0}", countryCode);

						MethodInfo mi = typeof(TaxRateImporterTest).GetMethod(methodName);
						if (mi == null)
						{
							processedCountryCode.Add(countryCode);
							sb.Append(methodName);
							sb.Append("<br/>");
						}
					}
				}
				if (sb.Length > 0)
				{
					sb.Insert(0, "The following tax rate for country unit tests are missing:<br/><br/>\r\n\r\n");
					HtmlFail(sb.ToString());
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestRateSourceNodeDoesNotHaveTIDValue()
		{
			using (var fileStream = TaxRateXmlParser.GetResourceStream_ForTest())
			{
				XmlDocument xml = new XmlDocument();
				xml.Load(fileStream);
				XmlElement rootNode = xml.DocumentElement;
				foreach (XmlNode node in rootNode)
				{
					var countryCode = node.SelectSingleNode("CountryCode")?.InnerText;
					var rateSourceValue = node.SelectSingleNode("RateSource")?.InnerText;
					var taxID = node.SelectSingleNode("TaxID")?.InnerText;

					var message = $"TaxRates.xml contains invalid node in {countryCode} with Tax ID '{taxID}'. 'TID' is a default value of AT_RateSource column in AccTaxRate table. There is no need to add <RateSource> node with this value.";

					AssertNotEquals(message, "TID", rateSourceValue);
				}
				Assert(true);
			}
		}

		public void TestRateSourceNodeOnlyWithTaxSystemNode()
		{
			using (var fileStream = TaxRateXmlParser.GetResourceStream_ForTest())
			{
				XmlDocument xml = new XmlDocument();
				xml.Load(fileStream);
				XmlElement rootNode = xml.DocumentElement;
				foreach (XmlNode node in rootNode)
				{
					var rateSourceFound = false;
					var taxSystemFound = false;
					var countryCode = node.SelectSingleNode("CountryCode")?.InnerText;
					var taxID = node.SelectSingleNode("TaxID")?.InnerText;

					foreach (XmlNode childNode in node.ChildNodes)
					{
						if (childNode.Name == "RateSource")
						{
							rateSourceFound = true;
						}
						else if (childNode.Name == "TaxSystem")
						{
							taxSystemFound = true;
						}
					}
					if (rateSourceFound && !taxSystemFound)
					{
						Assert($"TaxRates.xml contains invalid node for Country {countryCode} and Tax ID '{taxID}'. <RateSource> should be used only when <TaxSystem> is defined.", false);
					}
				}
				Assert(true);
			}
		}

		public void TestNoEmptyValueNodesInXML()
		{
			using (var fileStream = TaxRateXmlParser.GetResourceStream_ForTest())
			{
				XmlDocument xml = new XmlDocument();
				xml.Load(fileStream);
				XmlElement rootNode = xml.DocumentElement;
				foreach (XmlNode node in rootNode)
				{
					var countryCode = node.SelectSingleNode("CountryCode")?.InnerText;
					var taxID = node.SelectSingleNode("TaxID")?.InnerText;

					foreach (XmlNode childNode in node.ChildNodes)
					{
						if (string.IsNullOrEmpty(childNode.InnerText))
						{
							Assert($"TaxRates.xml contains invalid node. Node <{childNode.Name}> in country {countryCode} with Tax ID '{taxID}' has empty value.", false);
						}

						Assert(childNode.ChildNodes.Count.Equals(1));
					}
				}
				Assert(true);
			}
		}

		public void TestCopiedOrCreatedTaxRates_AD()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Andorra,
				new[] { "LOWIGI", "MIDIGI", "HIGHIGI" },
				new[] { "CAPGST" });
		}

		public void TestCopiedOrCreatedTaxRates_AF()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Afghanistan);
		}

		public void TestCopiedOrCreatedTaxRates_AO()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Angola);
		}

		public void TestCopiedOrCreatedTaxRates_AR()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Argentina,
				new[] { "IVA10.5", "IVA27" },
				taxFrameworkCodes: new[] { "PIB", "PIBG" });
		}

		public void TestCopiedOrCreatedTaxRates_AS()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.AmericanSamoa);
		}

		public void TestCopiedOrCreatedTaxRates_AU()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Australia,
				additionalRequiredCodes: new[] { "FREECAPGST" });
		}

		public void TestCopiedOrCreatedTaxRates_AW()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Aruba);
		}

		public void TestCopiedOrCreatedTaxRates_BD()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Bangladesh,
				new string[] { "VAT10", "VAT7.5", "VAT5", "VAT4.5" });
		}

		public void TestCopiedOrCreatedTaxRates_BE()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "BTWREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEBTWREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Belgium,
				new[] { "MIDBTW", "LOWBTW", "FREEBTWREV", "BTWREV", "MIDBTWREV", "LOWBTWREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_BQ()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTTaxRegistryID, "ABB");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTTaxRegistryID, "FREEABB");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.BonaireSintEustatiusAndSaba,
				new[] { "FREEABB", "ABB", "LOWABB", "HIABB" },
				new[] { "FREEGST", "GST", "CAPGST" },
				additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_BM()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Bermuda);
		}

		public void TestCopiedOrCreatedTaxRates_BR()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTTaxRegistryID, "FREECMT");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Brazil,
				additionalConfiguration: additionalConfiguration,
				taxFrameworkCodes: new[] { "COFINS", "CSLL", "INSS", "IRRF", "ISS", "ISSRET", "PCOFIN", "PIS", "PPIS" });
		}

		public void TestCopiedOrCreatedTaxRates_BN()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Brunei);
		}

		public void TestCopiedOrCreatedTaxRates_BH()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEVATREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "VATREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Bahrain,
				additionalRequiredCodes: new[] { "FREEVATREV", "VATREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_BZ()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Belize);
		}

		public void TestCopiedOrCreatedTaxRates_KH()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "VATREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Cambodia,
				additionalRequiredCodes: new[] { "VATREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_CL()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Chile);
		}

		public void TestCopiedOrCreatedTaxRates_CD()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.DemocraticRepublicOfCongo);
		}

		public void TestCopiedOrCreatedTaxRates_CV()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.CapeVerde, null,
				defaultCodesToRemove: new[] { "CAPGST" });
		}

		public void TestCopiedOrCreatedTaxRates_CZ()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "DPHREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEDPHREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.CzechRepublic,
				new[] { "LOWDPH", "FREEDPHREV", "LOW2DPH", "LOW2DPHREV", "DPHREV", "LOWDPHREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_DK()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "MOMREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEMOMREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Denmark,
				new[] { "FREEMOMREV", "MOMREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_EG()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Egypt, new string[] { "VAT10", "LOWVAT" });
		}

		public void TestCopiedOrCreatedTaxRates_EE()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "KMREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEKMREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Estonia,
				new[] { "LOWKM", "FREEKMREV", "KMREV", "LOWKMREV", "MIDKM", "MIDKMREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_FJ()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Fiji, new string[] { "HIVAT" });
		}

		public void TestCopiedOrCreatedTaxRates_FI()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "VATREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEVATREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Finland,
				new string[] { "MIDVAT", "LOWVAT", "VATREV", "FREEVATREV", "MIDVATREV", "LOWVATREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_GF()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTTaxRegistryID, "FREETVA");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.FrenchGuyana,
				new[] { "DOMTVA", "FRTVA" },
				new[] { "CAPGST", "GST" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_DE()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "MSTREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEMSTREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Germany,
				new[] { "LOWMST", "MSTREV", "FREEMSTREV", "LOWMSTREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_DJ()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Djibouti);
		}

		public void TestCopiedOrCreatedTaxRates_GM()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Gambia
				, defaultCodesToRemove: new[] { "CAPGST" });
		}

		public void TestCopiedOrCreatedTaxRates_GU()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Guam);
		}

		public void TestCopiedOrCreatedTaxRates_HK()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.HongKong);
		}

		public void TestCopiedOrCreatedTaxRates_IS()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Iceland, new[] { "MIDVSK" });
		}

		public void TestCopiedOrCreatedTaxRates_IQ()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Iraq);
		}

		public void TestCopiedOrCreatedTaxRates_IE()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "VATREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEVATREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Ireland,
				new[] { "MIDVAT", "LOWVAT", "FREEVATREV", "VATREV", "LOWVATREV", "MIDVATREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_JP()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Japan,
				new string[] { "MIDCON" });
		}

		public void TestCopiedOrCreatedTaxRates_KR()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.KoreaSouth);
		}

		public void TestRateType_KR()
		{
			var message = $@"Note for Developer: When adding/modifying/deleting a Tax Type, please confirm with product colleagues if this Tax Type should be eligible for EInvoicing.
You may need to change the default values for registry: {AccountingMasterFilesRegistry.Instance.TaxTypeToTaxInvoiceDocumentTypeCodeMapping.Location()}, to map this Tax Type to the appropriate Tax Invoice Document/Type Code.";

			var taxRatesByCountry = new TaxRateXmlParser().BuildTaxRatesDictionaryBasedOnCountry();
			var taxTypes = taxRatesByCountry.Where(x => x.Key == "KR").SelectMany(y => y.Value.Select(z => z.Type.ToString())).Distinct();

			AssertContainsExactElementsInAnyOrder(message, new[]
			{
				AccTaxRate.Types.Rated,
				AccTaxRate.Types.Exempt,
				AccTaxRate.Types.CapitalRated,
				AccTaxRate.Types.NotReportable,
				AccTaxRate.Types.ExcludedFromTheTaxBase
			}, taxTypes);
		}

		public void TestCopiedOrCreatedTaxRates_KW()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEVATREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "VATREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Kuwait,
				additionalRequiredCodes: new[] { "FREEVATREV", "VATREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_LY()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.LibyanArabJamahiriya);
		}

		public void TestCopiedOrCreatedTaxRates_MO()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Macau);
		}

		public void TestCopiedOrCreatedTaxRates_MM()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Myanmar, new string[] { "HICMT" });
		}

		public void TestCopiedOrCreatedTaxRates_NA()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Namibia);
		}

		public void TestCopiedOrCreatedTaxRates_NL()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "BTWREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEBTWREV");
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Netherlands,
				new[] { "LOWBTW", "FREEBTWREV", "BTWREV", "LOWBTWREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_NC()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.NewCaledonia,
				new string[] { "TGC3", "TGC6", "TGC22" });
		}

		public void TestCopiedOrCreatedTaxRates_NZ()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.NewZealand);
		}

		public void TestCopiedOrCreatedTaxRates_MP()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.NorthernMarianaIslands);
		}

		public void TestCopiedOrCreatedTaxRates_PK()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTTaxRegistryID, "ST1");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTTaxRegistryID, "FREEST");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Pakistan,
				 new[] { "CAPST1", "CAPST2", "CAPST3", "CAPST4", "FREEST", "HIST", "ST1", "ST2", "ST3", "ST4", "ST5", "ST6" },
				 new[] { "GST", "CAPGST", "FREEGST" },
				 additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_PA()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Panama,
				additionalRequiredCodes: new[] { "ITBMS10" });
		}

		public void TestCopiedOrCreatedTaxRates_PG()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.PapuaNewGuinea);
		}

		public void TestCopiedOrCreatedTaxRates_PH()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Philippines);
		}

		public void TestCopiedOrCreatedTaxRates_PL()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "PTUREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEPTUREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Poland,
				new[] { "MIDPTU", "LOWPTU", "FREEPTUREV", "PTUREV", "MIDPTUREV", "LOWPTUREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_PR()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.PuertoRico);
		}

		public void TestCopiedOrCreatedTaxRates_QA()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEVATREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "VATREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Qatar,
				additionalRequiredCodes: new[] { "FREEVATREV", "VATREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_RU()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Russia);
		}

		public void TestCopiedOrCreatedTaxRates_SA()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEVATREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "VATREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.SaudiArabia,
				additionalRequiredCodes: new[] { "FREEVATREV", "VATREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_SD()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Sudan);
		}

		public void TestCopiedOrCreatedTaxRates_ZA()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.SouthAfrica);
		}

		public void TestCopiedOrCreatedTaxRates_KI()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Kiribati);
		}

		public void TestCopiedOrCreatedTaxRates_NP()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Nepal);
		}

		public void TestCopiedOrCreatedTaxRates_BB()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Barbados);
		}

		public void TestCopiedOrCreatedTaxRates_BS()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Bahamas);
		}

		public void TestCopiedOrCreatedTaxRates_CW()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Curacao);
		}

		public void TestCopiedOrCreatedTaxRates_JM()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Jamaica);
		}

		public void TestCopiedOrCreatedTaxRates_TT()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.TrinidadAndTobago);
		}

		public void TestCopiedOrCreatedTaxRates_TG()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Togo);
		}

		public void TestCopiedOrCreatedTaxRates_HR()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "PDVREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEPDVREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Croatia,
				new[] { "PDVMID", "PDVLOW", "PDVREV", "FREEPDVREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_MK()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "VATREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEVATREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Macedonia,
				additionalRequiredCodes: new[] { "FREEVATREV", "VATREV", "VAT5", "VATREV5" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_BJ()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Benin);
		}

		public void TestCopiedOrCreatedTaxRates_RW()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Rwanda);
		}

		public void TestCopiedOrCreatedTaxRates_SB()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTTaxRegistryID, "STX");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTTaxRegistryID, "FREESTX");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.SolomonIslands,
				new[] { "FREESTX", "STX" },
				new[] { "FREEGST", "GST", "CAPGST" },
				additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_CK()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTTaxRegistryID, "VAT");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTTaxRegistryID, "FREEVAT");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.CookIslands,
				new[] { "FREEVAT", "VAT" },
				new[] { "FREEGST", "GST", "CAPGST" },
				additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_RS()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "PDVREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEPDVREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Serbia,
				additionalRequiredCodes: new[] { "PDVREV", "FREEPDVREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_XK()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "TVSHREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREETVSREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Kosovo,
				additionalRequiredCodes: new[] { "TVSHREV", "FREETVSREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_AL()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "TVSREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREETVSREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Albania,
				additionalRequiredCodes: new[] { "FREETVSREV", "TVSREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_ES()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "IVAREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEIVAREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Spain,
				new[] { "MIDIVA", "LOWIVA", "FREEIVAREV", "MID2IVA", "MID2IVAREV", "IVAREV", "MIDIVAREV", "LOWIVAREV", "LOW2IVA" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_SE()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "MOMREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEMOMREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Sweden,
				new[] { "MIDMOM", "LOWMOM", "FREEMOMREV", "MOMREV", "MIDMOMREV", "LOWMOMREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_SZ()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTTaxRegistryID, "VAT");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTTaxRegistryID, "FREEVAT");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Swaziland, null,
				defaultCodesToRemove: new[] { "CAPGST" },
				additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_TW()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Taiwan);
		}

		public void TestCopiedOrCreatedTaxRates_TH()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEVATREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "VATREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Thailand,
				additionalRequiredCodes: new[] { "FREEVATREV", "VATREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_TL()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.TimorLeste);
		}

		public void TestCopiedOrCreatedTaxRates_TR()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Turkey,
				new string[] { "LOWVAT", "LOWVATT20", "LOWVATT30", "LOWVATT40", "LOWVATT50", "LOWVATT70", "LOWVATT90", "MIDVAT", "MIDVATT20", "MIDVATT30", "MIDVATT40", "MIDVATT50", "MIDVATT70", "MIDVATT90", "VATT20", "VATT30", "VATT40", "VATT50", "VATT70", "VATT90" });
		}

		public void TestCopiedOrCreatedTaxRates_TM()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Turkmenistan);
		}

		public void TestCopiedOrCreatedTaxRates_UA()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEVATREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "VATREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Ukraine,
				additionalRequiredCodes: new[] { "FREEVATREV", "VATREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_AE()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEVATREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "VATREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.UnitedArabEmirates,
				additionalRequiredCodes: new[] { "FREEVATREV", "VATREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_US()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.UnitedStates);
		}

		public void TestCopiedOrCreatedTaxRates_VU()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Vanuatu);
		}

		public void TestCopiedOrCreatedTaxRates_NO()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "MVAREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Norway,
				additionalRequiredCodes: new[] { "MVA15", "MVA12", "MVA10", "MVAREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_CA()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Canada,
				new[] { "GSTANDQST", "HST13", "HST14", "HST15", "CAPHST13", "CAPHST14", "CAPHST15" });
		}

		public void TestCopiedOrCreatedTaxRates_IN()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "IREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTTaxRegistryID, "IGST");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTTaxRegistryID, "FREEIGST");
			additionalConfiguration.Add(AccTaxRate.Helper.MainNotReportableTaxRegistryID, "INOTAPP");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.India,
				 new[] { "GEXEMPT", "GNOTAPP", "GREV", "GREV12", "GREV28", "GREV3", "GREV5", "GST12", "GST28", "GST3", "GST5", "IEXEMPT", "IGST12", "IGST28", "IGST3", "IGST5", "IREV12", "IREV28", "IREV3", "IREV5", "IREV", "IGST", "FREEIGST", "INOTAPP" },
				 new[] { "CAPGST", "EXEMPT", "NOTREPORT" },
				 additionalConfiguration,
				 taxFrameworkCodes: new[] { "TDS" });
		}

		public void TestCopiedOrCreatedTaxRates_GR()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "VATREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEVATREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Greece,
				new[] { "VAT13", "VAT6", "VATREV", "VATREV13", "VATREV6", "FREEVATREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_HU()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "VATREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEVATREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Hungary,
				new[] { "VAT18", "VAT5", "VATREV", "VATREV18", "VATREV5", "FREEVATREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_FR()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "TVAREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREETVAREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.France,
				new[] { "TVA13", "GPTVA", "RETVA", "MQTVA", "TVAREV", "FREETVAREV", "LOWTVA", "LOWTVAREV", "MIDTVA", "MIDTVAREV", "TVA10", "TVAREV10" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_MX()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Mexico,
				new[] { "IVA4", "IVAFRONT", "IVAREB", "IVAREDREB", "IVAREDREF", "IVAREDRET", "IVAREF", "IVARET", "IVAREC" });
		}

		public void TestCopiedOrCreatedTaxRates_CO()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Colombia,
				new[] { "IVA5", "IVA5RETG", "IVARETG" },
				taxFrameworkCodes: new[] { "AUTOFUENTE", "RETEFUENTE", "RETEICA", "AUTOICA" });
		}

		public void TestCopiedOrCreatedTaxRates_ID()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Indonesia,
				new[] { "PPN1", "HIPPN", "MIDPPN" });
		}

		public void TestCopiedOrCreatedTaxRates_IL()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Israel);
		}

		public void TestCopiedOrCreatedTaxRates_PE()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Peru);
		}

		public void TestCopiedOrCreatedTaxRates_KZ()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Kazakhstan);
		}

		public void TestCopiedOrCreatedTaxRates_VN()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.VietNam,
				new[] { "LOWVAT", "VATFCWT", "FCWT", "MIDVAT", "FCWTB", "HIVAT" });
		}

		public void TestCopiedOrCreatedTaxRates_TN()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Tunisia,
				additionalRequiredCodes: new[] { "TVA13", "TVA7" });
		}

		public void TestCopiedOrCreatedTaxRates_RE()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "TVAREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREETVAREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Reunion,
				new[] { "LOWTVA", "FRTVA", "TVAREV", "FREETVAREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_GP()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "TVAREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREETVAREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Guadeloupe,
				new[] { "LOWTVA", "FRTVA", "TVAREV", "FREETVAREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_MQ()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "TVAREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREETVAREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Martinique,
				new[] { "LOWTVA", "FRTVA", "TVAREV", "FREETVAREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_MA()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREETVAREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "TVAREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Morocco,
				new[] { "LOWTVA", "MIDTVA", "LOWESTTVA", "TVAREV", "MIDTVAREV", "MID1TVAREV", "FREETVAREV", "MID1TVA", "MID2TVA" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_ME()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "PDVREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEPDVREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Montenegro,
				new[] { "LOWPDV", "LOWPDVREV", "PDVREV", "FREEPDVREV", "MIDPDV", "MIDPDVREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_MF()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.SaintMartin
				, defaultCodesToRemove: new[] { "CAPGST" });
		}

		public void TestCopiedOrCreatedTaxRates_MD()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Moldova,
				new[] { "LOWTVA" });
		}

		public void TestCopiedOrCreatedTaxRates_BI()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Burundi,
				new[] { "LOWTVA" });
		}

		public void TestCopiedOrCreatedTaxRates_GN()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Guinea);
		}

		public void TestCopiedOrCreatedTaxRates_CG()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Congo,
				new[] { "TVASTX", "CAPTVASTX", "LOWTVA", "LOWTVASTX" });
		}

		public void TestCopiedOrCreatedTaxRates_MG()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Madagascar);
		}

		public void TestCopiedOrCreatedTaxRates_MV()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Maldives);
		}

		public void TestCopiedOrCreatedTaxRates_TO()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Tonga);
		}

		public void TestCopiedOrCreatedTaxRates_LS()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Lesotho);
		}

		public void TestCopiedOrCreatedTaxRates_LK()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.SriLanka,
				new string[] { "SVAT", "HIVAT", "SVATLOW", "VATLOW" });
		}

		public void TestCopiedOrCreatedTaxRates_ET()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Ethiopia);
		}

		public void TestCopiedOrCreatedTaxRates_UG()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Uganda);
		}

		public void TestCopiedOrCreatedTaxRates_ZM()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Zambia);
		}

		public void TestCopiedOrCreatedTaxRates_CR()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "IVAREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEIVAREV");
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.CostaRica,
				new string[] { "LOWAIVA", "LOWBIVA", "LOWCIVA", "IVAREV", "FREEIVAREV", "IVAEXON" }, additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_LU()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "TVAREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREETVAREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Luxembourg,
				new[] { "TVA3", "TVA8", "TVA14", "TVAREV3", "TVAREV8", "TVAREV14", "TVAREV", "TVAMID", "TVALOW", "FREETVAREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_BG()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "VATREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEVATREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Bulgaria,
				new[] { "VAT9", "VATREV9", "VATREV", "FREEVATREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_RO()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "TVAREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREETVAREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Romania,
				new[] { "TVA9", "TVAREV9", "TVA5", "TVAREV5", "TVAREV", "FREETVAREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_WS()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.WesternSamoa);
		}

		public void TestCopiedOrCreatedTaxRates_IT()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "IVAREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEIVAREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Italy,
				new[]
				{
					"ART9",
					"DICH.INT",
					"ART10",
					"ART15",
					"ART7",
					"ART71",
					"ART2",
					"MIDIVASPV",
					"FREEIVAB",
					"IVAREV",
					"IVAREVB",
					"FREEIVA9",
					"IVASPV",
					"LOWIVA",
					"LOWIVAREV",
					"MIDIVA",
					"MIDIVAREV",
					"FREEIVAREV",
					"ESCLUSE",
					"ESCLUSEB",
					"MIDLIVA",
					"MIDLIVASPV",
				},
				new[] { "EXCLUDE" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_GB()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "VATREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEVATREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.UnitedKingdom,
				new[] { "VATREV", "FREEVATREV", "LOWVAT", "LOWVATREV", "LOW1VAT" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_AT()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEMSTREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "MSTREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Austria,
				new[] { "MSTREV", "MST19", "MSTREV19", "LOWMSTREV", "LOWMST", "LOW2MST", "LOW2MSTREV", "FREEMSTREV", "MST13", "MSTREV13" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_CN()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTTaxRegistryID, "VAT6");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.China,
				new[] { "VAT11", "VAT6", "VAT3", "VAT13", "VAT5", "MIDVAT", "VATRG", "VAT10" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_GT()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Guatemala);
		}

		public void TestCopiedOrCreatedTaxRates_NG()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Nigeria);
		}

		public void TestCopiedOrCreatedTaxRates_VE()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Venezuela,
				new string[] { "IVA27", "IVA7", "IVA8", "IVA9" });
		}

		public void TestCopiedOrCreatedTaxRates_CU()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Cuba);
		}

		public void TestCopiedOrCreatedTaxRates_LV()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "PVNREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEPVNREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Latvia,
				new string[] { "PVN12", "PVNREV", "PVNREV12", "FREEPVNREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_LT()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "PVMREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEPVMREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Lithuania,
				new string[] { "PVM9", "PVM5", "PVMREV", "PVMREV9", "PVMREV5", "FREEPVMREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_SI()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "DDVREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEDDVREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Slovenia,
				new string[] { "DDV9.5", "DDVREV", "DDVREV9.5", "LOWDDV", "FREEDDVREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_SS()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.SouthSudan);
		}

		public void TestCopiedOrCreatedTaxRates_EC()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "IVAREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEIVAREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Ecuador,
				additionalRequiredCodes: new[] { "IVAREV", "FREEIVAREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_SV()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.ElSalvador,
				new string[] { "IVARET" });
		}

		public void TestCopiedOrCreatedTaxRates_PY()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Paraguay,
				new string[] { "IVA5", "LOWAIVA" });
		}

		public void TestCopiedOrCreatedTaxRates_UY()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Uruguay,
				new string[] { "IVA.66", "IVA10", "IVA.396", "IVA.77" });
		}

		public void TestCopiedOrCreatedTaxRates_AZ()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Azerbaijan);
		}

		public void TestCopiedOrCreatedTaxRates_UZ()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Uzbekistan);
		}

		public void TestCopiedOrCreatedTaxRates_BO()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Bolivia);
		}

		public void TestCopiedOrCreatedTaxRates_PF()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.FrenchPolynesia,
				new string[] { "TVA5", "TVA13", "TVACPS", "TVA13CPS" });
		}

		public void TestCopiedOrCreatedTaxRates_HN()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Honduras,
				new string[] { "ISV15", "ISV18" });
		}

		public void TestCopiedOrCreatedTaxRates_KE()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Kenya,
				new string[] { "VAT12", "VAT8" });
		}

		public void TestCopiedOrCreatedTaxRates_MU()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Mauritius);
		}

		public void TestCopiedOrCreatedTaxRates_NI()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Nicaragua);
		}

		public void TestCopiedOrCreatedTaxRates_PT()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "IVAREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEIVAREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Portugal,
				new string[] { "IVA22", "IVA18", "IVA13", "IVA6", "IVAREV13", "IVAREV6", "IVAREV", "FREEIVAREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_MN()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Mongolia);
		}

		public void TestCopiedOrCreatedTaxRates_SK()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "DPHREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEDPHREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Slovakia,
				new string[] { "DPHREV", "FREEDPHREV", "LOWDPH", "LOWDPHREV", "MIDDPH", "MIDDPHREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_BW()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Botswana);
		}

		public void TestCopiedOrCreatedTaxRates_TZ()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Tanzania);
		}

		public void TestCopiedOrCreatedTaxRates_ML()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Mali);
		}

		public void TestCopiedOrCreatedTaxRates_JO()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Jordan);
		}

		public void TestCopiedOrCreatedTaxRates_CH()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "VATREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEVATREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Switzerland,
				new[] { "LOWVAT", "MIDVAT", "LOWVATREV", "MIDVATREV", "FREEVATREV", "VATREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_SG()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "GSTREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEGSTREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Singapore,
				additionalRequiredCodes: new[] { "GSTREV", "FREEGSTREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_ZW()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Zimbabwe);
		}

		public void TestCopiedOrCreatedTaxRates_LB()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Lebanon);
		}

		public void TestCopiedOrCreatedTaxRates_GH()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTTaxRegistryID, "VATNHIL");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Ghana,
				new string[] { "CAPVATNHIL", "VATNHIL", "LOWVAT" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_BY()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Belarus,
				new string[] { "VAT10" });
		}

		public void TestCopiedOrCreatedTaxRates_SL()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.SierraLeone);
		}

		public void TestCopiedOrCreatedTaxRates_SN()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Senegal,
				additionalRequiredCodes: new[] { "TVA10" });
		}

		public void TestCopiedOrCreatedTaxRates_GQ()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.EquatorialGuinea,
				additionalRequiredCodes: new[] { "IVA6" });
		}

		public void TestCopiedOrCreatedTaxRates_CI()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.CoteDivoire,
				additionalRequiredCodes: new[] { "TVA9" });
		}

		public void TestCopiedOrCreatedTaxRates_CM()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Cameroon,
				new string[] { "TVAMID" });
		}

		public void TestCopiedOrCreatedTaxRates_MZ()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Mozambique);
		}

		public void TestCopiedOrCreatedTaxRates_DO()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.DominicanRepublic,
				additionalRequiredCodes: new[] { "ITBIS16" });
		}

		public void TestCopiedOrCreatedTaxRates_YE()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Yemen,
				additionalRequiredCodes: new[] { "GST10" });
		}

		public void TestCopiedOrCreatedTaxRates_YT()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTTaxRegistryID, "FREETVA");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "EXEMPT");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Mayotte,
				new[] { "DOMTVA", "FRTVA" },
				new[] { "CAPGST", "GST" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_SO()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Somalia);
		}

		public void TestCopiedOrCreatedTaxRates_PW()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Palau);
		}

		public void TestCopiedOrCreatedTaxRates_DZ()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Algeria,
				new string[] { "TVA9" });
		}

		public void TestCopiedOrCreatedTaxRates_MW()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Malawi);
		}

		public void TestCopiedOrCreatedTaxRates_MT()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "VATREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEVATREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Malta,
				new[] { "VAT7", "VAT5", "VATREV7", "VATREV5", "VATREV", "FREEVATREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_MY()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTTaxRegistryID, "SVC");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTTaxRegistryID, "FREESVC");
			additionalConfiguration.Add(AccTaxRate.Helper.MainNotReportableTaxRegistryID, "SVCNOTAPP");
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "SVCREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Malaysia,
				new[] { "FREESVC", "SVCEXT", "SVC", "SVCNOTAPP", "SVCLOW", "SVCREVEXT", "SVCREV", "SVCREVLOW" },
				new[] { "FREEGST", "GST", "CAPGST", "EXEMPT", "NOTREPORT" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_NE()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Niger);
		}

		public void TestCopiedOrCreatedTaxRates_BF()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.BurkinaFaso);
		}

		public void TestCopiedOrCreatedTaxRates_HT()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Haiti);
		}

		public void TestCopiedOrCreatedTaxRates_OM()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEVATREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "VATREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Oman,
				additionalRequiredCodes: new[] { "FREEVATREV", "VATREV" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_IR()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Iran);
		}

		public void TestCopiedOrCreatedTaxRates_GE()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Georgia);
		}

		public void TestCopiedOrCreatedTaxRates_TD()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Chad);
		}

		public void TestCopiedOrCreatedTaxRates_LA()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.LaoPeoplesDemocraticRepublic);
		}

		public void TestCopiedOrCreatedTaxRates_BA()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.BosniaAndHerzegovina);
		}

		public void TestCopiedOrCreatedTaxRates_GA()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Gabon,
				new string[] { "LOWTVA", "TVACSS", "CAPTVACSS" });
		}

		public void TestCopiedOrCreatedTaxRates_PS()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.PalestinianTerritory);
		}

		public void TestCopiedOrCreatedTaxRates_GY()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Guyana);
		}

		public void TestCopiedOrCreatedTaxRates_MR()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Mauritania);
		}

		public void TestCopiedOrCreatedTaxRates_CY()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Cyprus,
				new string[] { "LOWVAT", "MIDVAT" });
		}

		public void TestCopiedOrCreatedTaxRates_KY()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.CaymanIslands);
		}

		public void TestCopiedOrCreatedTaxRates_TC()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.TurksAndCaicosIslands);
		}

		public void TestCopiedOrCreatedTaxRates_GI()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Gibraltar);
		}

		public void TestCopiedOrCreatedTaxRates_MH()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.MarshallIslands);
		}

		public void TestCopiedOrCreatedTaxRates_SX()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.SintMaarten);
		}

		public void TestCopiedOrCreatedTaxRates_KG()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Kyrgyzstan,
				defaultCodesToRemove: new[] { "CAPGST" }
			);
		}

		public void TestCopiedOrCreatedTaxRates_SR()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Suriname,
				new[] { "LOWBTW", "HIBTW" },
				new[] { "CAPGST" }
				);
		}

		public void TestCopiedOrCreatedTaxRates_KN()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTTaxRegistryID, "VAT");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTTaxRegistryID, "FREEVAT");
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, "VATREV");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, "FREEVATREV");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.SaintKittsAndNevis,
				new[] { "MIDVAT", "VATREV", "FREEVATREV" },
				new[] { "CAPGST" },
				additionalConfiguration);
		}
		public void TestCopiedOrCreatedTaxRates_TV()
		{
			var additionalConfiguration = new Dictionary<string, string>();
			additionalConfiguration.Add(AccTaxRate.Helper.MainGSTTaxRegistryID, "TCT");
			additionalConfiguration.Add(AccTaxRate.Helper.MainFreeGSTTaxRegistryID, "FREETCT");
			additionalConfiguration.Add(AccTaxRate.Helper.MainNotReportableTaxRegistryID, "NOTREPORT");

			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.Tuvalu,
				defaultCodesToRemove: new[] { "CAPGST" },
				additionalConfiguration: additionalConfiguration);
		}

		public void TestCopiedOrCreatedTaxRates_FO()
		{
			AssertCopiedOrCreatedTaxRates(Constants.CountryCodes.FaeroeIslands);
		}

		public void TestReferenceRateCopiedOnCompanyCreate()
		{
			var newCompany = SetupNewCompany(Constants.CountryCodes.India, "GST", true);

			var taxRates = Factory.Load<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, Constants.CountryCodes.India));
			AssertEquals(taxRates.Length, taxRates.Count(x => x.AT_ReferenceRateType != string.Empty));
			AssertEquals("ZERO", taxRates.FirstOrDefault(x => x.AT_Code == "FREEIGST").AT_ReferenceRateType);
			AssertEquals("ILOW", taxRates.FirstOrDefault(x => x.AT_Code == "IGST5").AT_ReferenceRateType);
			AssertEquals("MID", taxRates.FirstOrDefault(x => x.AT_Code == "GST12").AT_ReferenceRateType);
			AssertEquals("LOW", taxRates.FirstOrDefault(x => x.AT_Code == "GREV5").AT_ReferenceExtraRateType);
			AssertEquals("STD", taxRates.FirstOrDefault(x => x.AT_Code == "GREV").AT_ReferenceExtraRateType);
			AssertEquals("HI", taxRates.FirstOrDefault(x => x.AT_Code == "GST28").AT_ReferenceExtraRateType);
		}

		#region posting groups
		public void TestLoadDefaultTaxRatesPostingGroupsFromXML()
		{
			var chargeCodes = Factory.Load<AccChargeCode>(new ZQuery());
			foreach (var chargeCode in chargeCodes)
			{
				chargeCode.Delete();
			}

			var taxRates = Factory.Load<AccTaxRate>(new ZQuery());
			foreach (var tax in taxRates)
			{
				tax.Delete();
			}

			Factory.Save();
			taxRates = Factory.Load<AccTaxRate>(new ZQuery());
			AssertEquals(0, taxRates.Length);

			//import all tax rates
			var allCountries = new RefCountryCollection(Factory);
			foreach (RefCountry country in allCountries)
			{
				var taxRateImporter = new TaxRateImporter(country.Code, Env.CurrentCompany.PK, Factory);
				taxRateImporter.ImportFromXMLFile();
			}
			Factory.Save();

			//all tax rates' default AT_PostingGroupId should be 0 by default.
			taxRates = Factory.Load<AccTaxRate>(new ZQuery());
			AssertEquals(false, taxRates.Any(x => x.AT_PostingGroupId != 0));

			//load all tax rates' AT_PostingGroupId
			foreach (RefCountry country in allCountries)
			{
				var taxRateImporter = new TaxRateImporter(country.Code, Env.CurrentCompany.PK, Factory);
				taxRateImporter.UpdatePostingGroupsFromXMLFile();
			}
			Factory.Save();

			//all tax rates' AT_PostingGroupId should be updated
			AssertEquals(false, taxRates.Any(x => x.AT_PostingGroupId == 0 && x.AT_TaxSystemCode.IsEmpty));

			const int group0Count = 0; // Unless we have Tax Configurations with Tax Systems we would not get our special Tax Rates which have 0 Group
			const int group2Count = 50;
			const int group3Count = 14;
			const int group4Count = 6;
			const int group5Count = 6;
			const int group6Count = 1;
			const int group7Count = 1;
			const int group8Count = 1;
			const int group11Count = 6;
			const int group12Count = 1;
			const int group13Count = 1;
			const int group14Count = 5;
			const int group21Count = 6;
			const int group22Count = 1;
			const int group23Count = 1;
			const int group24Count = 5;
			const int group30Count = 1;

			AssertEquals(taxRates.Length - group0Count - group2Count - group3Count - group4Count - group5Count
				- group6Count - group7Count - group8Count - group11Count - group12Count
				- group13Count - group14Count - group21Count - group22Count
				- group23Count - group24Count - group30Count,
				taxRates.Count(x => x.AT_PostingGroupId == 1));
			AssertEquals(group2Count, taxRates.Count(x => x.AT_PostingGroupId == 2));
			AssertEquals(group3Count, taxRates.Count(x => x.AT_PostingGroupId == 3));
			AssertEquals(group4Count, taxRates.Count(x => x.AT_PostingGroupId == 4));
			AssertEquals(group5Count, taxRates.Count(x => x.AT_PostingGroupId == 5));
			AssertEquals(group6Count, taxRates.Count(x => x.AT_PostingGroupId == 6));
			AssertEquals(group7Count, taxRates.Count(x => x.AT_PostingGroupId == 7));
			AssertEquals(group8Count, taxRates.Count(x => x.AT_PostingGroupId == 8));
			AssertEquals(group11Count, taxRates.Count(x => x.AT_PostingGroupId == 11));
			AssertEquals(group12Count, taxRates.Count(x => x.AT_PostingGroupId == 12));
			AssertEquals(group13Count, taxRates.Count(x => x.AT_PostingGroupId == 13));
			AssertEquals(group14Count, taxRates.Count(x => x.AT_PostingGroupId == 14));
			AssertEquals(group21Count, taxRates.Count(x => x.AT_PostingGroupId == 21));
			AssertEquals(group22Count, taxRates.Count(x => x.AT_PostingGroupId == 22));
			AssertEquals(group23Count, taxRates.Count(x => x.AT_PostingGroupId == 23));
			AssertEquals(group24Count, taxRates.Count(x => x.AT_PostingGroupId == 24));
			AssertEquals(group30Count, taxRates.Count(x => x.AT_PostingGroupId == 30));

			var ratesList = taxRates.Select(x => string.Format("{0} * {1} * {2}", x.AT_RN_NKCountry, x.AT_Code, x.AT_PostingGroupId)).ToList();

			AssertEquals(true, ratesList.Contains("CL * CAPIVA * 1"));
			AssertEquals(true, ratesList.Contains("CL * EXEMPT * 2"));
			AssertEquals(true, ratesList.Contains("CL * EXCLUDE * 2"));
			AssertEquals(true, ratesList.Contains("CL * NOTREPORT * 2"));
			AssertEquals(true, ratesList.Contains("CL * IVA * 1"));
			AssertEquals(true, ratesList.Contains("CL * FREEIVA * 2"));
			AssertEquals(true, ratesList.Contains("CN * CAPVAT * 1"));
			AssertEquals(true, ratesList.Contains("CN * EXEMPT * 2"));
			AssertEquals(true, ratesList.Contains("CN * NOTREPORT * 1"));
			AssertEquals(true, ratesList.Contains("CN * VAT * 1"));
			AssertEquals(true, ratesList.Contains("CN * VAT13 * 1"));
			AssertEquals(true, ratesList.Contains("CN * VAT11 * 1"));
			AssertEquals(true, ratesList.Contains("CN * VAT6 * 1"));
			AssertEquals(true, ratesList.Contains("CN * VAT3 * 1"));
			AssertEquals(true, ratesList.Contains("CN * FREEVAT * 1"));
			AssertEquals(true, ratesList.Contains("CO * CAPIVA * 1"));
			AssertEquals(true, ratesList.Contains("CO * EXEMPT * 1"));
			AssertEquals(true, ratesList.Contains("CO * NOTREPORT * 1"));
			AssertEquals(true, ratesList.Contains("CO * IVA * 1"));
			AssertEquals(true, ratesList.Contains("CO * FREEIVA * 1"));
			AssertEquals(true, ratesList.Contains("CO * IVA5 * 1"));
			AssertEquals(true, ratesList.Contains("CO * EXCLUDE * 1"));
			AssertEquals(true, ratesList.Contains("ID * CAPPPN * 1"));
			AssertEquals(true, ratesList.Contains("ID * EXEMPT * 3"));
			AssertEquals(true, ratesList.Contains("ID * NOTREPORT * 3"));
			AssertEquals(true, ratesList.Contains("ID * PPN * 1"));
			AssertEquals(true, ratesList.Contains("ID * PPN1 * 2"));
			AssertEquals(true, ratesList.Contains("ID * FREEPPN * 3"));
			AssertEquals(true, ratesList.Contains("ID * HIPPN * 4"));
			AssertEquals(true, ratesList.Contains("ID * MIDPPN * 5"));
			AssertEquals(true, ratesList.Contains("KR * CAPVAT * 1"));
			AssertEquals(true, ratesList.Contains("KR * EXEMPT * 3"));
			AssertEquals(true, ratesList.Contains("KR * NOTREPORT * 4"));
			AssertEquals(true, ratesList.Contains("KR * EXCLUDE * 5"));
			AssertEquals(true, ratesList.Contains("KR * VAT * 1"));
			AssertEquals(true, ratesList.Contains("KR * FREEVAT * 2"));
			AssertEquals(true, ratesList.Contains("PE * CAPIGV * 1"));
			AssertEquals(true, ratesList.Contains("PE * EXEMPT * 2"));
			AssertEquals(true, ratesList.Contains("PE * NOTREPORT * 2"));
			AssertEquals(true, ratesList.Contains("PE * IGV * 1"));
			AssertEquals(true, ratesList.Contains("PE * FREEIGV * 2"));
			AssertEquals(true, ratesList.Contains("PE * EXCLUDE * 2"));
			AssertEquals(true, ratesList.Contains("PH * CAPVAT * 1"));
			AssertEquals(true, ratesList.Contains("PH * EXEMPT * 3"));
			AssertEquals(true, ratesList.Contains("PH * NOTREPORT * 4"));
			AssertEquals(true, ratesList.Contains("PH * VAT * 1"));
			AssertEquals(true, ratesList.Contains("PH * FREEVAT * 2"));
			AssertEquals(true, ratesList.Contains("PH * NOTREPORT * 4"));
			AssertEquals(true, ratesList.Contains("LK * CAPVAT * 1"));
			AssertEquals(true, ratesList.Contains("LK * EXEMPT * 1"));
			AssertEquals(true, ratesList.Contains("LK * NOTREPORT * 1"));
			AssertEquals(true, ratesList.Contains("LK * VAT * 1"));
			AssertEquals(true, ratesList.Contains("LK * FREEVAT * 1"));
			AssertEquals(true, ratesList.Contains("LK * SVAT * 2"));
			AssertEquals(true, ratesList.Contains("LK * VATLOW * 1"));
			AssertEquals(true, ratesList.Contains("LK * SVATLOW * 2"));
			AssertEquals(true, ratesList.Contains("TW * CAPVAT * 1"));
			AssertEquals(true, ratesList.Contains("TW * EXEMPT * 3"));
			AssertEquals(true, ratesList.Contains("TW * NOTREPORT * 4"));
			AssertEquals(true, ratesList.Contains("TW * VAT * 1"));
			AssertEquals(true, ratesList.Contains("TW * FREEVAT * 2"));
			AssertEquals(true, ratesList.Contains("TW * EXCLUDE * 5"));
			AssertEquals(true, ratesList.Contains("VN * CAPVAT * 1"));
			AssertEquals(true, ratesList.Contains("VN * EXEMPT * 3"));
			AssertEquals(true, ratesList.Contains("VN * NOTREPORT * 3"));
			AssertEquals(true, ratesList.Contains("VN * VAT * 1"));
			AssertEquals(true, ratesList.Contains("VN * LOWVAT * 4"));
			AssertEquals(true, ratesList.Contains("VN * FREEVAT * 2"));
			AssertEquals(true, ratesList.Contains("VN * VATFCWT * 5"));
			AssertEquals(true, ratesList.Contains("VN * FCWTB * 7"));
			AssertEquals(true, ratesList.Contains("VN * HIVAT * 8"));
			AssertEquals(true, ratesList.Contains("MY * FREESVC * 2"));
			AssertEquals(true, ratesList.Contains("MY * SVCEXT * 1"));
			AssertEquals(true, ratesList.Contains("MY * SVC * 1"));
			AssertEquals(true, ratesList.Contains("MY * SVCNOTAPP * 1"));
			AssertEquals(true, ratesList.Contains("MY * EXCLUDE * 2"));
			AssertEquals(true, ratesList.Contains("MY * SVCLOW * 1"));
			AssertEquals(true, ratesList.Contains("MY * SVCREVEXT * 3"));
			AssertEquals(true, ratesList.Contains("MY * SVCREV * 3"));
			AssertEquals(true, ratesList.Contains("MY * SVCREVLOW * 3"));
			AssertEquals(true, ratesList.Contains("IN * IGST * 11"));
			AssertEquals(true, ratesList.Contains("IN * IGST12 * 11"));
			AssertEquals(true, ratesList.Contains("IN * IGST3 * 11"));
			AssertEquals(true, ratesList.Contains("IN * IGST5 * 11"));
			AssertEquals(true, ratesList.Contains("IN * IGST28 * 11"));
			AssertEquals(true, ratesList.Contains("IN * FREEIGST * 11"));
			AssertEquals(true, ratesList.Contains("IN * IEXEMPT * 12"));
			AssertEquals(true, ratesList.Contains("IN * INOTAPP * 13"));
			AssertEquals(true, ratesList.Contains("IN * IREV * 14"));
			AssertEquals(true, ratesList.Contains("IN * IREV12 * 14"));
			AssertEquals(true, ratesList.Contains("IN * IREV28 * 14"));
			AssertEquals(true, ratesList.Contains("IN * IREV3 * 14"));
			AssertEquals(true, ratesList.Contains("IN * IREV5 * 14"));
			AssertEquals(true, ratesList.Contains("IN * GST * 21"));
			AssertEquals(true, ratesList.Contains("IN * GST12 * 21"));
			AssertEquals(true, ratesList.Contains("IN * GST3 * 21"));
			AssertEquals(true, ratesList.Contains("IN * GST5 * 21"));
			AssertEquals(true, ratesList.Contains("IN * FREEGST * 21"));
			AssertEquals(true, ratesList.Contains("IN * GST28 * 21"));
			AssertEquals(true, ratesList.Contains("IN * GEXEMPT * 22"));
			AssertEquals(true, ratesList.Contains("IN * GNOTAPP * 23"));
			AssertEquals(true, ratesList.Contains("IN * GREV * 24"));
			AssertEquals(true, ratesList.Contains("IN * GREV12 * 24"));
			AssertEquals(true, ratesList.Contains("IN * GREV3 * 24"));
			AssertEquals(true, ratesList.Contains("IN * GREV28 * 24"));
			AssertEquals(true, ratesList.Contains("IN * GREV5 * 24"));
			AssertEquals(true, ratesList.Contains("IN * EXCLUDE * 30"));
			AssertEquals(true, ratesList.Contains("DE * CAPMST * 1"));
			AssertEquals(true, ratesList.Contains("DE * EXCLUDE * 2"));
			AssertEquals(true, ratesList.Contains("DE * EXEMPT * 1"));
			AssertEquals(true, ratesList.Contains("DE * FREEMST * 1"));
			AssertEquals(true, ratesList.Contains("DE * FREEMSTREV * 1"));
			AssertEquals(true, ratesList.Contains("DE * LOWMST * 1"));
			AssertEquals(true, ratesList.Contains("DE * LOWMSTREV * 1"));
			AssertEquals(true, ratesList.Contains("DE * MST * 1"));
			AssertEquals(true, ratesList.Contains("DE * MSTREV * 1"));
			AssertEquals(true, ratesList.Contains("DE * NOTREPORT * 2"));
		}

		#endregion

		static readonly string[] DefaultTaxCodes = new[] { "FREEGST", "GST", "CAPGST", "EXEMPT", "NOTREPORT", "EXCLUDE" };

		void AssertCopiedOrCreatedTaxRates(string country,
			string[] additionalRequiredCodes = null,
			string[] defaultCodesToRemove = null,
			IDictionary<string, string> additionalConfiguration = null,
			string[] taxFrameworkCodes = null)
		{
			int testAttempt = 0;
			var consumptionTaxDescription = Country.GetConsumptionTaxDescription(country);
			var defaultCodesList = new List<string>();
			if (!string.IsNullOrEmpty(consumptionTaxDescription))
			{
				defaultCodesList.AddRange(DefaultTaxCodes);
				if (defaultCodesToRemove != null)
				{
					defaultCodesList.RemoveAll(x => defaultCodesToRemove.Contains(x));
				}
				defaultCodesList = defaultCodesList.Select(s => s.Replace("GST", consumptionTaxDescription)).ToList();
			}
			if (additionalRequiredCodes != null)
			{
				defaultCodesList.AddRange(additionalRequiredCodes);
			}

			AssertCopiedOrCreatedTaxRatesInternal(false);
			AssertCopiedOrCreatedTaxRatesInternal(true);

			void AssertCopiedOrCreatedTaxRatesInternal(bool addTaxConfigurationsForDefaultTaxSystems)
			{
				testAttempt++;

				var addTaxConfigurationsForDefaultTaxSystemsInternal = addTaxConfigurationsForDefaultTaxSystems &&
					ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().GetTaxSystems(country).Count > 0;
				var taxFrameworkCodesInternal = addTaxConfigurationsForDefaultTaxSystemsInternal ? taxFrameworkCodes : null;
				var additionaTaxFrameworkMessage = addTaxConfigurationsForDefaultTaxSystemsInternal ? " (TF activated)" : "(TF not activated)";

				TestCaseHelper.ClearTable(AccChargeCodeSchema.Constants.TableName);
				TestCaseHelper.ClearTable(AccTaxRateSchema.Constants.TableName);

				var nonGSTCompany = SetupNewCompany(country, $"NO{testAttempt}", false, addTaxConfigurationsForDefaultTaxSystems: addTaxConfigurationsForDefaultTaxSystemsInternal);
				AssertActualRates("Non GST company", nonGSTCompany, taxFrameworkCodesInternal ?? Array.Empty<string>(), new Dictionary<string, string>());

				var expectedCodesList = new List<string>(defaultCodesList);
				if (taxFrameworkCodesInternal != null)
				{
					expectedCodesList.AddRange(taxFrameworkCodesInternal);
				}
				AssertEquals("Precondition: There should be no duplicate taxID codes.", expectedCodesList.Count, expectedCodesList.Distinct().Count());
				var expectedRegistryConfig = GetExpectedRegistryConfigurationWithDefaults(additionalConfiguration);

				var gstCompany = SetupNewCompany(country, $"GS{testAttempt}", true, addTaxConfigurationsForDefaultTaxSystems: addTaxConfigurationsForDefaultTaxSystemsInternal);
				var taxRatesInGSTCompany = AssertActualRates("GST company", gstCompany, expectedCodesList, expectedRegistryConfig);
				if (taxRatesInGSTCompany.Length > 0)
				{
					Assert("Every country must have at least one taxID with a EXL type.", taxRatesInGSTCompany.Any(x => x.AT_Type == AccTaxRate.Types.ExcludedFromTheTaxBase));
				}

				AccTaxRate[] AssertActualRates(string message, GlbCompany company, IReadOnlyCollection<string> expectedRates, IDictionary<string, string> expectedConfiguration)
				{
					var additionalMessage = $"{message} {additionaTaxFrameworkMessage}:";
					var query = new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, company.GC_RN_NKCountryCode) { OrderBy = AccTaxRate.Schema.AT_Code };
					var actualRates = new BusinessObjectFactory().Load<AccTaxRate>(query);

					var separator = ", ";
					var expectedCodes = string.Join(separator, expectedRates.OrderBy(taxCode => taxCode));
					var existingCodes = string.Join(separator, actualRates.Select(x => x.AT_Code));
					AssertEquals(additionalMessage + "Default GST tax rates.", expectedCodes, existingCodes);

					var taxConfiguratuionFilter = new ZQuery(StmDataSchema.SD_Owner, company.PK);
					taxConfiguratuionFilter.AddToFilter(StmDataSchema.SD_Name, AccTaxRate.Helper.TaxConfigurationRegistryIDs);
					var items = Factory.Load<StmData>(taxConfiguratuionFilter);
					foreach (var item in items)
					{
						Assert(string.Format(additionalMessage + "Tax Configuration registry item {0} should be among expected", item.SD_Name), expectedConfiguration.ContainsKey(item.SD_Name));
						AssertEquals(additionalMessage + "Guid Value should not be empty", false, item.SD_GuidValue.IsEmpty);
						AssertEquals(additionalMessage + "Binary value should be correct", Encoding.Unicode.GetBytes(item.SD_GuidValue.ToString()), item.SD_BinaryValue);
						AccTaxRate rate = Factory.Load<AccTaxRate>(item.SD_GuidValue);
						AssertNotNull(additionalMessage + "Should be a valid Tax Rate", rate);
						AssertEquals(additionalMessage + "Tax Rate should be as expected", expectedConfiguration[item.SD_Name], rate.AT_Code);
					}
					AssertEquals(additionalMessage + "Number of the  Tax Configuration registry items should match expected", expectedConfiguration.Count, items.Length);

					return actualRates;
				}

				Dictionary<string, string> GetExpectedRegistryConfigurationWithDefaults(IDictionary<string, string> additionalRegistryConfiguration)
				{
					var expectedConfig = additionalRegistryConfiguration != null ? new Dictionary<string, string>(additionalRegistryConfiguration) : new Dictionary<string, string>();
					if (!string.IsNullOrEmpty(consumptionTaxDescription))
					{
						if (!expectedConfig.ContainsKey(AccTaxRate.Helper.MainGSTTaxRegistryID))
						{
							expectedConfig.Add(AccTaxRate.Helper.MainGSTTaxRegistryID, string.IsNullOrEmpty(consumptionTaxDescription) ? "GST" : consumptionTaxDescription);
						}
						if (!expectedConfig.ContainsKey(AccTaxRate.Helper.MainFreeGSTTaxRegistryID))
						{
							expectedConfig.Add(AccTaxRate.Helper.MainFreeGSTTaxRegistryID, string.IsNullOrEmpty(consumptionTaxDescription) ? "FREEGST" : "FREE" + consumptionTaxDescription);
						}
						if (!expectedConfig.ContainsKey(AccTaxRate.Helper.MainNotReportableTaxRegistryID))
						{
							expectedConfig.Add(AccTaxRate.Helper.MainNotReportableTaxRegistryID, "NOTREPORT");
						}
					}

					return expectedConfig;
				}
			}
		}

		GlbCompany SetupNewCompany(ZString country, string code, bool isGstRegistered, bool addTaxConfigurationsForDefaultTaxSystems = false)
		{
			var result = new BusinessObjectFactory().New<GlbCompany>();
			result.GC_Code = code;
			result.GC_RN_NKCountryCode = country;
			result.GC_RX_NKLocalCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			result.GC_IsGSTRegistered = isGstRegistered;

			if (addTaxConfigurationsForDefaultTaxSystems)
			{
				var taxSystemCodes = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().GetTaxSystems(country).GetAllCodes();
				foreach (var taxSystemCode in taxSystemCodes)
				{
					var taxConfig = result.Factory.NewWithValidTestData<AccTaxConfiguration>();
					taxConfig.ETC_RN_NKCountry = country;
					taxConfig.ETC_TaxSystemCode = taxSystemCode;
					result.AccTaxConfigurations.Add(taxConfig);
				}
			}

			result.Factory.Save();

			return result;
		}

		/// <summary>
		/// This test case is used to test Create Tax Framework for Tax IDs even when VAT/GST/CMT is not enabled
		/// and Tax Configuration exists for the AT_TaxSystemCode in the Login Company or any Branch in the Login Company
		/// </summary>
		public void TestTaxIdCreated_TaxConfigurationWithTaxSystem_GSTNotEnabled()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.SetCountry(Constants.CountryCodes.Brazil);
			company.GC_IsGSTRegistered = false;
			var importer = new TaxRateImporter(company.GC_RN_NKCountryCode, company.PK, Factory);
			var taxQuery = new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, Constants.CountryCodes.Brazil);

			Assert("Pre-condition: No existing Tax Rates for Brazil", !Factory.Load<AccTaxRate>(taxQuery).Any());
			Assert("Pre-condition: No existing Tax Configurations for Brazil", !(new AccTaxConfigurationCollection(Factory, Constants.CountryCodes.Brazil)).Any());
			importer.ImportFromXMLFile();
			var regionalTaxRates = Factory.Load<AccTaxRate>(taxQuery);

			AssertEquals("No Tax Rates are created for Brazil", 0, regionalTaxRates.Length);

			var taxConfiguration1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration1.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			taxConfiguration1.ETC_RN_NKCountry = Constants.CountryCodes.Brazil;
			taxConfiguration1.ETC_TaxSystemCode = "PIS";
			Factory.Save();

			importer.ImportFromXMLFile();
			regionalTaxRates = Factory.Load<AccTaxRate>(taxQuery);

			AssertEquals("One tax rate is created for Brazil", 1, regionalTaxRates.Length);
			AssertEquals("PIS Tax is created for brazil", "PIS", regionalTaxRates[0].AT_TaxSystemCode);
		}

		/// <summary>
		/// This test case is used to test Create Tax Framework for Tax IDs in addition to the standard VAT/GST/CMT taxes when VAT/GST/CMT is enabled.
		/// Taxes only created for the AT_TaxSystemCode presented in the Tax Configuration for the Login Company or any Branch in the Login Company
		/// </summary>
		public void TestTaxIdCreated_TaxConfigurationWithTaxSystem_GSTEnabled()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.SetCountry(Constants.CountryCodes.Brazil);
			company.GC_IsGSTRegistered = true;
			var importer = new TaxRateImporter(company.GC_RN_NKCountryCode, company.PK, Factory);
			var taxQuery = new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, Constants.CountryCodes.Brazil);

			Assert("Pre-condition: No existing Tax Rates for Brazil", !Factory.Load<AccTaxRate>(taxQuery).Any());

			var taxConfiguration1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration1.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			taxConfiguration1.ETC_RN_NKCountry = Constants.CountryCodes.Brazil;
			taxConfiguration1.ETC_TaxSystemCode = "PIS";

			Factory.Save();

			importer.ImportFromXMLFile();

			var allTaxRates = Factory.Load<AccTaxRate>(taxQuery);
			Assert("More than one Tax Rate was created", allTaxRates.Length > 1);

			var cmtTaxRates = allTaxRates.Where(x => x.AT_TaxSystemCode.IsEmpty);
			AssertEquals("CMT Tax Rates are created for Brazil", 6, cmtTaxRates.Count());

			var taxRatesWithTaxSystem = allTaxRates.Where(x => !x.AT_TaxSystemCode.IsEmpty);
			AssertEquals("One tax rate with Tax System is created for Brazil", 1, taxRatesWithTaxSystem.Count());
			AssertEquals("PIS Tax is created for brazil", "PIS", taxRatesWithTaxSystem.First().AT_TaxSystemCode);
		}
	}
}
