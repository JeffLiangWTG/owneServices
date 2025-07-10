using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class ParentSecondaryTariffAssociation
	{
		public string ParentTariff;
		public string SecondaryTariff;
		public string[] AdditionalSecondaryTariffs;
	}

	public class SendTestMessagesForAssociatedTariffs
	{
		public void Send(BusinessObjectFactory factory, List<ParentSecondaryTariffAssociation> associations, ZString testReference)
		{
			DummyICusEntryHeader header = new DummyICusEntryHeader(factory);
			SetUpLineData(header, associations);

			DutyFeeCalculationManager calculator = new DutyFeeCalculationManager(header);
			calculator.Calculate();

			short lineNumber = 1;
			foreach (DummyICusEntryLine line in header.EntryLines)
			{
				line.CL_LineNumber = lineNumber;
				lineNumber++;
			}

			EntrySummaryMessageBuilder messageBuilder = new EntrySummaryMessageBuilder(header, UpdateActionCode.Add, false);
			MQEDIMessage message = messageBuilder.PopulateMessage();
			message.EM_ApplicationReference = testReference;
			header.Message = message;

			factory.Save();
		}

		void SetUpLineData(DummyICusEntryHeader header, List<ParentSecondaryTariffAssociation> associations)
		{
			foreach (ParentSecondaryTariffAssociation association in associations)
			{
				DummyICusEntryLine line = new DummyICusEntryLine(header.Factory, header);
				line.ImportTariffCode = association.ParentTariff;
				header.EntryLines.Add(line);

				if (line.ImportTariff != null)
				{
					SetCountryOfOrigin(line);

					SetLicenceNo(line);

					DummyICusEntryLine secondaryLine = new DummyICusEntryLine(header.Factory, header);
					secondaryLine.ParentLine = line;
					line.SecondaryLines.Add(secondaryLine);

					secondaryLine.ImportTariffCode = association.SecondaryTariff;
					SetLinePrices(secondaryLine, line);
					SetVisaCategoryNumber(secondaryLine, line);

					foreach (ZString additionalSecondaryTariff in association.AdditionalSecondaryTariffs)
					{
						DummyICusEntryLine secondaryLine2 = new DummyICusEntryLine(header.Factory, header);
						secondaryLine2.ParentLine = line;
						line.SecondaryLines.Add(secondaryLine2);

						secondaryLine2.ImportTariffCode = additionalSecondaryTariff;
						SetLinePrices(secondaryLine2, line);
						SetVisaCategoryNumber(secondaryLine2, line);
					}
				}
			}
		}

		void SetVisaCategoryNumber(DummyICusEntryLine line, DummyICusEntryLine parentLine)
		{
			line.TextileCategoryNumber = line.ImportTariff != null ? line.ImportTariff.UE_TextileCategoryNumber : ZString.Empty;

			if (!line.TextileCategoryNumber.IsEmpty)
			{
				if (parentLine != null && TariffsExemptForCategoryNum.Contains(parentLine.Tariff))
				{
					line.TextileCategoryNumber = ZString.Empty;
				}
			}
		}

		public List<string> TariffsExemptForCategoryNum = new List<string>(new string[]
				{
					"9802008044", "9802008046", "98201103", "98201106", "98201115", "98201118",
					"98201121", "98201124", "98201127", "98201130"
				});

		void SetLicenceNo(DummyICusEntryLine line)
		{
			BusinessObjectFactory factory = line.Factory;

			bool isRequired = line.ImportTariff != null && line.ImportTariff.Applies(TariffRuleList.Codes.WoolLicenseEligible, line.DateForDutyCalculation);
			if (isRequired && line.WoolLicense.IsEmpty)
			{
				line.WoolLicense = "W" + ZDateTime.Today.Year.ToString().Substring(2, 2) + "123456";
			}

			ZString validNumber = ZString.Empty;
			switch (line.ImportTariff.UE_PermitLicenseIndicator)
			{
				case "01":
				case "05":
				case "06":
				case "07":
				case "11":
				case "12":
					validNumber = "123456789";
					break;
				case "02":
					validNumber = "1SG123456";
					break;
				case "03":
					validNumber = "1CA123456";
					break;
				case "04":
					validNumber = "1MX123456";
					break;
				case "08":
					validNumber = "1AU123456";
					break;
				case "09":
					validNumber = "CEM123456";
					break;
				case "10":
					validNumber = "1NI123456";
					break;
			}

			line.MiscellaneousPermitLicenseNumber = validNumber;

			if (line.ImportTariff != null)
			{
				if (line.ImportTariff.Applies(TariffRuleList.Codes.HaitiTariffHope, line.DateForDutyCalculation))
				{
					line.VisaNumber = "9" + line.CountryOfOrigin + "123456";
				}
				else if (line.ImportTariff.IsEligibleForAGOATextileBenefits(line.DateForDutyCalculation))
				{
					string firstCharacter = "";

					switch (line.ImportTariff.UE_Tariff)
					{
						case "9802008042":
							firstCharacter = "1";
							break;
						case "98191103":
							firstCharacter = "2";
							break;
						case "98191106":
							firstCharacter = "3";
							break;
						case "98191109":
							firstCharacter = "4";
							break;
						case "98191112":
							firstCharacter = "5";
							break;
						case "98191115":
							firstCharacter = "6";
							break;
						case "98191118":
							firstCharacter = "7";
							break;
						case "98191121":
						case "98191124":
							firstCharacter = "8";
							break;
						case "98191127":
							firstCharacter = "9";
							break;
					}

					line.VisaNumber = firstCharacter + line.CountryOfOrigin + "123456";
				}
			}

			if (line.Tariff == "98201115")
			{
				line.CBTPACertificationNumber = "9CB999999";
			}
		}

		void SetLinePrices(DummyICusEntryLine line, DummyICusEntryLine parentLine)
		{
			parentLine.Value = 10000m;
			line.Value = 0m;

			if (parentLine.ImportTariff != null)
			{
				if (parentLine.ImportTariff.IsValueToBeDeclaredInAlternateTariff(parentLine.DateForDutyCalculation))
				{
					parentLine.Value = 0m;
					line.Value = 10000m;
				}
			}
		}

		void SetCountryOfOrigin(DummyICusEntryLine line)
		{
			BusinessObjectFactory factory = line.Factory;

			ZString countryToSet = "";

			ZString countryFromRef = line.ImportTariff != null ? line.ImportTariff.UE_ISOCountryofOriginEditCode : ZString.Empty;

			if (!countryFromRef.IsEmpty && factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, countryFromRef) != null)
			{
				countryToSet = countryFromRef;
			}
			else
			{
				switch (line.Tariff)
				{
					case "99990084":
						countryToSet = "SG";
						break;
				}

				if (countryToSet.IsEmpty && line.ImportTariff != null)
				{
					SchemaColumn spiColumn = null;
					ZString value = ZString.Empty;
					SchemaColumn beginDate = null;
					SchemaColumn endDate = null;

					if (line.ImportTariff.IsEligibleForAGOATextileBenefits(line.DateForDutyCalculation))
					{
						spiColumn = USCCountrySchema.UC_SpecialTradeProgramsIndicator;
						value = "D";
						beginDate = USCCountrySchema.UC_SpecialTradeProgramsBeginDate;
						endDate = USCCountrySchema.UC_SpecialTradeProgramsEndDate;
					}
					else if (line.ImportTariff.IsEligibleForCBTPATextileBenefits(line.DateForDutyCalculation))
					{
						spiColumn = USCCountrySchema.UC_SpecialTradeProgramsIndicator;
						value = "R";
						beginDate = USCCountrySchema.UC_SpecialTradeProgramsBeginDate;
						endDate = USCCountrySchema.UC_SpecialTradeProgramsEndDate;
					}
					else if (line.ImportTariff.IsEligibleForATPDEATextileAndTunaClaims(line.DateForDutyCalculation))
					{
						spiColumn = USCCountrySchema.UC_SpecialTradeProgramsIndicator;
						value = "J";
						beginDate = USCCountrySchema.UC_SpecialTradeProgramsBeginDate;
						endDate = USCCountrySchema.UC_SpecialTradeProgramsEndDate;
					}
					else if (line.ImportTariff.IsEligibleForCAFTAClaims(line.DateForDutyCalculation))
					{
						spiColumn = USCCountrySchema.UC_MiscellaneousSPIIndicator;
						value = "P";
						beginDate = USCCountrySchema.UC_MiscellaneousSPIBeginDate;
						endDate = USCCountrySchema.UC_MiscellaneousSPIEndDate;
					}

					if (spiColumn != null)
					{
						ZQuery query = new ZQuery(spiColumn, value);
						query.AddToFilter(beginDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, line.DateForDutyCalculation);
						query.AddToFilter(endDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, line.DateForDutyCalculation);

						USCCountry country = factory.LoadTop1<USCCountry>(query);

						if (country != null)
						{
							countryToSet = country.UC_Code;
						}
					}
				}
			}

			if (countryToSet == "")
			{
				countryToSet = "AU";
			}

			line.CountryOfOrigin = countryToSet;
			line.CountryOfExport = countryToSet;

			if (countryToSet == "US")
			{
				line.CountryOfExport = "AU";
			}
		}
	}
}
