using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Web.Model;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Web.Test.Model
{
	public class CalculatorToCalculatorInfoConverterTest : RatingTestCase
	{
		public void TestConvert_FlatCalculator()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var entry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var line = entry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			line.RateLineItems.RemoveAndDeleteAll();

			var item = line.RateLineItems.AddNew();
			item.TM_Type = Calculator.Items.Operator.BAS;
			item.TM_Value = 50m;
			item.TM_AgentDeclaredRate = 0m;

			var expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "FLT",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "BaseRate",
							Type = "Decimal",
							Value = 50m
						}
					},
				}
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos);

			item.TM_AgentDeclaredRate = 30m;

			expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "FLT",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "BaseRate",
							Type = "Decimal",
							Value = 50m
						}
					},
				},
				new CalculatorInfo()
				{
					CWCode = "FLT",
					IsAgentRate = true,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "BaseRate",
							Type = "Decimal",
							Value = 30m
						}
					},
				}
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos);
		}

		public void TestConvert_UnitCalculator()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var entry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var line = entry.AddRateLine("FRT", UnitCalculator.Code, "KG", "AUD");
			line.RateLineItems.RemoveAndDeleteAll();

			var item = line.RateLineItems.AddNew();
			item.TM_Type = Calculator.Items.Operator.UNT;
			item.TM_Value = 10m;
			item.TM_AgentDeclaredRate = 0m;

			var expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "UNT",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "PerUnit",
							Type = "Decimal",
							Value = 10m
						}
					},
				}
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos);

			item.TM_AgentDeclaredRate = 7.3m;

			expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "UNT",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "PerUnit",
							Type = "Decimal",
							Value = 10m
						}
					},
				},
				new CalculatorInfo()
				{
					CWCode = "UNT",
					IsAgentRate = true,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "PerUnit",
							Type = "Decimal",
							Value = 7.3m
						}
					},
				}
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos);
		}

		public void TestConvert_FlatPlusUnitCalculator()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var entry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var line = entry.AddRateLine("FRT", FlatPlusPerUnitCalculator.Code, "KG", "AUD");

			line.RateLineItems.RemoveAndDeleteAll();

			var baseRateItem = line.RateLineItems.AddNew();
			baseRateItem.TM_Type = Calculator.Items.Operator.BAS;
			baseRateItem.TM_Value = 300m;
			baseRateItem.TM_AgentDeclaredRate = 0;

			var perUnitItem = line.RateLineItems.AddNew();
			perUnitItem.TM_Type = Calculator.Items.Operator.UNT;
			perUnitItem.TM_Value = 110m;
			perUnitItem.TM_AgentDeclaredRate = 0;

			var expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "FPU",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "BaseRate",
							Type = "Decimal",
							Value = 300m
						},
						new CalculatorAttribute()
						{
							Name = "PerUnit",
							Type = "Decimal",
							Value = 110m
						}
					},
				}
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos);

			baseRateItem.TM_AgentDeclaredRate = 250m;

			expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "FPU",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "BaseRate",
							Type = "Decimal",
							Value = 300m
						},
						new CalculatorAttribute()
						{
							Name = "PerUnit",
							Type = "Decimal",
							Value = 110m
						}
					},
				},
				new CalculatorInfo()
				{
					CWCode = "FPU",
					IsAgentRate = true,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "BaseRate",
							Type = "Decimal",
							Value = 250m
						},
						new CalculatorAttribute()
						{
							Name = "PerUnit",
							Type = "Decimal",
							Value = 0m
						}
					},
				}
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos);

			baseRateItem.TM_AgentDeclaredRate = 0m;
			perUnitItem.TM_AgentDeclaredRate = 95;

			expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "FPU",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "BaseRate",
							Type = "Decimal",
							Value = 300m
						},
						new CalculatorAttribute()
						{
							Name = "PerUnit",
							Type = "Decimal",
							Value = 110m
						}
					},
				},
				new CalculatorInfo()
				{
					CWCode = "FPU",
					IsAgentRate = true,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "BaseRate",
							Type = "Decimal",
							Value = 0m
						},
						new CalculatorAttribute()
						{
							Name = "PerUnit",
							Type = "Decimal",
							Value = 95m
						}
					},
				}
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos);

			baseRateItem.TM_AgentDeclaredRate = 235m;
			perUnitItem.TM_AgentDeclaredRate = 85;

			expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "FPU",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "BaseRate",
							Type = "Decimal",
							Value = 300m
						},
						new CalculatorAttribute()
						{
							Name = "PerUnit",
							Type = "Decimal",
							Value = 110m
						}
					},
				},
				new CalculatorInfo()
				{
					CWCode = "FPU",
					IsAgentRate = true,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "BaseRate",
							Type = "Decimal",
							Value = 235m
						},
						new CalculatorAttribute()
						{
							Name = "PerUnit",
							Type = "Decimal",
							Value = 85m
						}
					},
				}
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos);
		}

		public void TestConvert_FirstPlusAdditionalCalculator()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var entry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var line = entry.AddRateLine("FRT", FirstPlusAdditionalCalculator.Code, "KG", "AUD");
			line.RateLineItems.RemoveAndDeleteAll();

			var firstItem = line.RateLineItems.AddNew();
			firstItem.TM_Type = FirstPlusAdditionalCalculator.Items.FST;
			firstItem.TM_Value = 500m;
			firstItem.TM_AgentDeclaredRate = 0;

			var additionalItem = line.RateLineItems.AddNew();
			additionalItem.TM_Type = FirstPlusAdditionalCalculator.Items.ADD;
			additionalItem.TM_Value = 200m;
			additionalItem.TM_AgentDeclaredRate = 0;

			var expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "FPA",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "First",
							Type = "Decimal",
							Value = 500m
						},
						new CalculatorAttribute()
						{
							Name = "Additional",
							Type = "Decimal",
							Value = 200m
						}
					},
				}
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos);

			firstItem.TM_AgentDeclaredRate = 438.8m;
			additionalItem.TM_AgentDeclaredRate = 189;

			expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "FPA",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "First",
							Type = "Decimal",
							Value = 500m
						},
						new CalculatorAttribute()
						{
							Name = "Additional",
							Type = "Decimal",
							Value = 200m
						}
					},
				},
				new CalculatorInfo()
				{
					CWCode = "FPA",
					IsAgentRate = true,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "First",
							Type = "Decimal",
							Value = 438.8m
						},
						new CalculatorAttribute()
						{
							Name = "Additional",
							Type = "Decimal",
							Value = 189m
						}
					},
				}
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos);
		}

		public void TestConvert_MinimumCalculator()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var entry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var line = entry.AddRateLine("FRT", MinimumCalculator.Code, "KG", "AUD");

			line.RateLineItems.RemoveAndDeleteAll();

			var minItem = line.RateLineItems.AddNew();
			minItem.TM_Type = MinimumCalculator.Items.MIN;
			minItem.TM_Value = 149m;
			minItem.TM_AgentDeclaredRate = 0;

			var minTypeItem = line.RateLineItems.AddNew();
			minTypeItem.TM_Type = MinimumCalculator.Items.MinimumType;
			minTypeItem.TM_Text = "JOB";

			var expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "MIN",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "MinimumValue",
							Type = "Decimal",
							Value = 149m
						},
						new CalculatorAttribute()
						{
							Name = "IsJobMinimum",
							Type = "Boolean",
							Value = true
						},
						new CalculatorAttribute()
						{
							Name = "IsChargeCodeMinimum",
							Type = "Boolean",
							Value = false
						}
					},
				}
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos);

			minItem.TM_AgentDeclaredRate = 127.5m;

			expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "MIN",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "MinimumValue",
							Type = "Decimal",
							Value = 149m
						},
						new CalculatorAttribute()
						{
							Name = "IsJobMinimum",
							Type = "Boolean",
							Value = true
						},
						new CalculatorAttribute()
						{
							Name = "IsChargeCodeMinimum",
							Type = "Boolean",
							Value = false
						}
					},
				},
				new CalculatorInfo()
				{
					CWCode = "MIN",
					IsAgentRate = true,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "MinimumValue",
							Type = "Decimal",
							Value = 127.5m
						},
						new CalculatorAttribute()
						{
							Name = "IsJobMinimum",
							Type = "Boolean",
							Value = true
						},
						new CalculatorAttribute()
						{
							Name = "IsChargeCodeMinimum",
							Type = "Boolean",
							Value = false
						}
					},
				}
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos);
		}

		public void TestConvert_MinimumOrPerUnitCalculator()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var entry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var line = entry.AddRateLine("FRT", MinimumOrPerUnitCalculator.Code, "KG", "AUD");
			line.RateLineItems.RemoveAndDeleteAll();

			var minimumItem = line.RateLineItems.AddNew();
			minimumItem.TM_Type = Calculator.Items.Operator.MIN;
			minimumItem.TM_Value = 600m;
			minimumItem.TM_AgentDeclaredRate = 0m;

			var perUnitItem = line.RateLineItems.AddNew();
			perUnitItem.TM_Type = Calculator.Items.Operator.UNT;
			perUnitItem.TM_Value = 150m;
			perUnitItem.TM_AgentDeclaredRate = 0m;

			var expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "MPU",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "Minimum",
							Type = "Decimal",
							Value = 600m
						},
						new CalculatorAttribute()
						{
							Name = "PerUnit",
							Type = "Decimal",
							Value = 150m
						}
					},
				}
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos);

			minimumItem.TM_AgentDeclaredRate = 450m;
			perUnitItem.TM_AgentDeclaredRate = 138m;

			expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "MPU",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "Minimum",
							Type = "Decimal",
							Value = 600m
						},
						new CalculatorAttribute()
						{
							Name = "PerUnit",
							Type = "Decimal",
							Value = 150m
						}
					},
				},
				new CalculatorInfo()
				{
					CWCode = "MPU",
					IsAgentRate = true,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "Minimum",
							Type = "Decimal",
							Value = 450m
						},
						new CalculatorAttribute()
						{
							Name = "PerUnit",
							Type = "Decimal",
							Value = 138m
						}
					},
				}
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos);
		}

		public void TestConvert_PackageCountCalculator()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var entry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var line = entry.AddRateLine("FRT", PackageCountCalculator.Code, "KG", "AUD");

			line.RateLineItems.RemoveAndDeleteAll();

			var baseRateItem = line.RateLineItems.AddNew();
			baseRateItem.TM_Type = Calculator.Items.Operator.BAS;
			baseRateItem.TM_Value = 200m;
			baseRateItem.TM_AgentDeclaredRate = 0;

			var perKGItem = line.RateLineItems.AddNew();
			perKGItem.TM_Type = Calculator.Items.Operator.UNT;
			perKGItem.TM_Value = 5m;
			perKGItem.TM_AgentDeclaredRate = 0;

			var firstPackageRate = line.RateLineItems.AddNew();
			firstPackageRate.TM_Type = PackageCountCalculator.Items.FirstPackageRate;
			firstPackageRate.TM_Value = 20m;
			firstPackageRate.TM_AgentDeclaredRate = 0;

			var additionalPackageRate = line.RateLineItems.AddNew();
			additionalPackageRate.TM_Type = PackageCountCalculator.Items.AddtionalPackageRate;
			additionalPackageRate.TM_Value = 15m;
			additionalPackageRate.TM_AgentDeclaredRate = 0;

			var expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "IAT",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "BaseRate",
							Type = "Decimal",
							Value = 200m
						},
						new CalculatorAttribute()
						{
							Name = "PerKG",
							Type = "Decimal",
							Value = 5m
						},
						new CalculatorAttribute()
						{
							Name = "FirstPackageRate",
							Type = "Decimal",
							Value = 20m
						},
						new CalculatorAttribute()
						{
							Name = "AddtionalPackageRate",
							Type = "Decimal",
							Value = 15m
						}
					},
				}
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos);

			baseRateItem.TM_AgentDeclaredRate = 190m;
			perKGItem.TM_AgentDeclaredRate = 4.8m;
			firstPackageRate.TM_AgentDeclaredRate = 19m;
			additionalPackageRate.TM_AgentDeclaredRate = 14m;

			expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "IAT",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "BaseRate",
							Type = "Decimal",
							Value = 200m
						},
						new CalculatorAttribute()
						{
							Name = "PerKG",
							Type = "Decimal",
							Value = 5m
						},
						new CalculatorAttribute()
						{
							Name = "FirstPackageRate",
							Type = "Decimal",
							Value = 20m
						},
						new CalculatorAttribute()
						{
							Name = "AddtionalPackageRate",
							Type = "Decimal",
							Value = 15m
						}
					},
				},
				new CalculatorInfo()
				{
					CWCode = "IAT",
					IsAgentRate = true,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "BaseRate",
							Type = "Decimal",
							Value = 190m
						},
						new CalculatorAttribute()
						{
							Name = "PerKG",
							Type = "Decimal",
							Value = 4.8m
						},
						new CalculatorAttribute()
						{
							Name = "FirstPackageRate",
							Type = "Decimal",
							Value = 19m
						},
						new CalculatorAttribute()
						{
							Name = "AddtionalPackageRate",
							Type = "Decimal",
							Value = 14m
						}
					},
				}
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos);
		}

		public void TestConvert_AgencyCalculator()
		{
			var tariff = Helper.NewCompanyTariff();
			var entry = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "", "AUSYD");
			var line = entry.AddRateLine("ODOC", AgencyCalculator.Code, currencyCode: "AUD");

			line.RateLineItems.RemoveAndDeleteAll();

			var agencyRateItem = line.RateLineItems.AddNew();
			agencyRateItem.TM_Type = AgencyCalculator.Items.AgencyRate;
			agencyRateItem.TM_Value = 200m;
			agencyRateItem.TM_AgentDeclaredRate = 0;

			var maximumItem = line.RateLineItems.AddNew();
			maximumItem.TM_Type = Calculator.Items.Operator.MAX;
			maximumItem.TM_Value = 1200m;
			maximumItem.TM_AgentDeclaredRate = 0;

			var agencyLineTypeItem = line.RateLineItems.AddNew();
			agencyLineTypeItem.TM_Type = AgencyCalculator.Items.AgencyLineType;
			agencyLineTypeItem.TM_Text = "FLT";

			var agencyFeeTypeItem = line.RateLineItems.AddNew();
			agencyFeeTypeItem.TM_Type = AgencyCalculator.Items.AgencyFeeType;
			agencyFeeTypeItem.TM_Text = "SHP";

			var perAdditionalLineItem = line.RateLineItems.AddNew();
			perAdditionalLineItem.TM_Type = AgencyCalculator.Items.CostPerAdditionalLine;
			perAdditionalLineItem.TM_Value = 10m;
			perAdditionalLineItem.TM_AgentDeclaredRate = 0;

			var includedLinesItem = line.RateLineItems.AddNew();
			includedLinesItem.TM_Type = AgencyCalculator.Items.IncludedLines;
			includedLinesItem.TM_Value = 5m;
			includedLinesItem.TM_AgentDeclaredRate = 0;

			var includedHeadersItem = line.RateLineItems.AddNew();
			includedHeadersItem.TM_Type = AgencyCalculator.Items.IncludedHeaders;
			includedHeadersItem.TM_Value = 2m;
			includedHeadersItem.TM_AgentDeclaredRate = 0;

			var maximumLinesItem = line.RateLineItems.AddNew();
			maximumLinesItem.TM_Type = AgencyCalculator.Items.MaximumLines;
			maximumLinesItem.TM_Value = 40;
			maximumLinesItem.TM_AgentDeclaredRate = 0;

			var aditionalRateItem = line.RateLineItems.AddNew();
			aditionalRateItem.TM_Type = AgencyCalculator.Items.AdditionalRate;
			aditionalRateItem.TM_Value = 2.1m;
			aditionalRateItem.TM_AgentDeclaredRate = 0;

			var messageTypeItem = line.RateLineItems.AddNew();
			messageTypeItem.TM_Type = AgencyCalculator.Items.MessageType;
			messageTypeItem.TM_Text = "EXP";

			var messageSubTypeItem = line.RateLineItems.AddNew();
			messageSubTypeItem.TM_Type = AgencyCalculator.Items.MessageSubType;
			messageSubTypeItem.TM_Text = "MAN";

			var hideFeeLineTypeOnQuoteItem = line.RateLineItems.AddNew();
			hideFeeLineTypeOnQuoteItem.TM_Type = AgencyCalculator.Items.HideFeeLineTypeOnQuote;
			hideFeeLineTypeOnQuoteItem.TM_Text = "Y";

			var hideMessageTypeOnQuoteItem = line.RateLineItems.AddNew();
			hideMessageTypeOnQuoteItem.TM_Type = AgencyCalculator.Items.HideMessageTypeOnQuote;
			hideMessageTypeOnQuoteItem.TM_Text = "Y";

			var expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "AGY",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "AgencyRate",
							Type = "Decimal",
							Value = 200m
						},
						new CalculatorAttribute()
						{
							Name = "Maximum",
							Type = "Decimal",
							Value = 1200m
						},
						new CalculatorAttribute()
						{
							Name = "AgencyLineType",
							Type = "String",
							Value = "FLT"
						},
						new CalculatorAttribute()
						{
							Name = "AgencyFeeType",
							Type = "String",
							Value = "SHP"
						},
						new CalculatorAttribute()
						{
							Name = "PerAdditionalLine",
							Type = "Decimal",
							Value = 10m
						},
						new CalculatorAttribute()
						{
							Name = "IncludedLines",
							Type = "Int32",
							Value = 5
						},
						new CalculatorAttribute()
						{
							Name = "IncludedHeaders",
							Type = "Int32",
							Value = 2
						},
						new CalculatorAttribute()
						{
							Name = "MaximumLines",
							Type = "Int32",
							Value = 40
						},
						new CalculatorAttribute()
						{
							Name = "AdditionalRate",
							Type = "Decimal",
							Value = 2.1m
						},
						new CalculatorAttribute()
						{
							Name = "MessageType",
							Type = "String",
							Value = "EXP"
						},
						new CalculatorAttribute()
						{
							Name = "MessageSubType",
							Type = "String",
							Value = "MAN"
						},
						new CalculatorAttribute()
						{
							Name = "HideFeeLineTypeOnQuote",
							Type = "Boolean",
							Value = true
						},
						new CalculatorAttribute()
						{
							Name = "HideMessageTypeOnQuote",
							Type = "Boolean",
							Value = true
						}
					},
				}
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos, RatingConstants.RatingHeaderTypes.Tariff);

			agencyRateItem.TM_AgentDeclaredRate = 190m;
			maximumItem.TM_AgentDeclaredRate = 1190m;
			perAdditionalLineItem.TM_AgentDeclaredRate = 9.8m;
			includedLinesItem.TM_AgentDeclaredRate = 4m;
			includedHeadersItem.TM_AgentDeclaredRate = 1m;
			maximumLinesItem.TM_AgentDeclaredRate = 50m;
			aditionalRateItem.TM_AgentDeclaredRate = 1.8;

			expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "AGY",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "AgencyRate",
							Type = "Decimal",
							Value = 200m
						},
						new CalculatorAttribute()
						{
							Name = "Maximum",
							Type = "Decimal",
							Value = 1200m
						},
						new CalculatorAttribute()
						{
							Name = "AgencyLineType",
							Type = "String",
							Value = "FLT"
						},
						new CalculatorAttribute()
						{
							Name = "AgencyFeeType",
							Type = "String",
							Value = "SHP"
						},
						new CalculatorAttribute()
						{
							Name = "PerAdditionalLine",
							Type = "Decimal",
							Value = 10m
						},
						new CalculatorAttribute()
						{
							Name = "IncludedLines",
							Type = "Int32",
							Value = 5
						},
						new CalculatorAttribute()
						{
							Name = "IncludedHeaders",
							Type = "Int32",
							Value = 2
						},
						new CalculatorAttribute()
						{
							Name = "MaximumLines",
							Type = "Int32",
							Value = 40
						},
						new CalculatorAttribute()
						{
							Name = "AdditionalRate",
							Type = "Decimal",
							Value = 2.1m
						},
						new CalculatorAttribute()
						{
							Name = "MessageType",
							Type = "String",
							Value = "EXP"
						},
						new CalculatorAttribute()
						{
							Name = "MessageSubType",
							Type = "String",
							Value = "MAN"
						},
						new CalculatorAttribute()
						{
							Name = "HideFeeLineTypeOnQuote",
							Type = "Boolean",
							Value = true
						},
						new CalculatorAttribute()
						{
							Name = "HideMessageTypeOnQuote",
							Type = "Boolean",
							Value = true
						}
					},
				},
				new CalculatorInfo()
				{
					CWCode = "AGY",
					IsAgentRate = true,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "AgencyRate",
							Type = "Decimal",
							Value = 190m
						},
						new CalculatorAttribute()
						{
							Name = "Maximum",
							Type = "Decimal",
							Value = 1190m
						},
						new CalculatorAttribute()
						{
							Name = "AgencyLineType",
							Type = "String",
							Value = "FLT"
						},
						new CalculatorAttribute()
						{
							Name = "AgencyFeeType",
							Type = "String",
							Value = "SHP"
						},
						new CalculatorAttribute()
						{
							Name = "PerAdditionalLine",
							Type = "Decimal",
							Value = 9.8m
						},
						new CalculatorAttribute()
						{
							Name = "IncludedLines",
							Type = "Int32",
							Value = 4
						},
						new CalculatorAttribute()
						{
							Name = "IncludedHeaders",
							Type = "Int32",
							Value = 1
						},
						new CalculatorAttribute()
						{
							Name = "MaximumLines",
							Type = "Int32",
							Value = 50
						},
						new CalculatorAttribute()
						{
							Name = "AdditionalRate",
							Type = "Decimal",
							Value = 1.8m
						},
						new CalculatorAttribute()
						{
							Name = "MessageType",
							Type = "String",
							Value = "EXP"
						},
						new CalculatorAttribute()
						{
							Name = "MessageSubType",
							Type = "String",
							Value = "MAN"
						},
						new CalculatorAttribute()
						{
							Name = "HideFeeLineTypeOnQuote",
							Type = "Boolean",
							Value = true
						},
						new CalculatorAttribute()
						{
							Name = "HideMessageTypeOnQuote",
							Type = "Boolean",
							Value = true
						}
					},
				},
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos, RatingConstants.RatingHeaderTypes.Tariff);
		}

		public void TestConvert_CMBCalculator()
		{
			var costing = Helper.NewCosting(TransportProvider1);

			var entry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "STD", "");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.CN, "AUD");
			line.RateLineItems.RemoveAndDeleteAll();

			var minusItem = line.RateLineItems.AddNew();
			minusItem.TM_Type = Calculator.Items.Operator.Minus;
			minusItem.TM_Break = 100m;
			minusItem.TM_Value = 20m;
			minusItem.TM_FlatAmount = 200m;
			minusItem.TM_BreakWeightVolume = QuantityUnit.KG;
			minusItem.TM_AgentDeclaredRate = 0;

			var plusItem = line.RateLineItems.AddNew();
			plusItem.TM_Type = Calculator.Items.Operator.Plus;
			plusItem.TM_Break = 100m;
			plusItem.TM_CallForPricing = true;
			plusItem.TM_Text = "Restriction Reason";
			plusItem.TM_AgentDeclaredRate = 0;

			var minItem = line.RateLineItems.AddNew();
			minItem.TM_Type = Calculator.Items.Operator.MIN;
			minItem.TM_Value = 400m;
			minItem.TM_AgentDeclaredRate = 0;

			var maxItem = line.RateLineItems.AddNew();
			maxItem.TM_Type = Calculator.Items.Operator.MAX;
			maxItem.TM_Value = 1500m;
			maxItem.TM_AgentDeclaredRate = 0;

			var baseItem = line.RateLineItems.AddNew();
			baseItem.TM_Type = Calculator.Items.Operator.BAS;
			baseItem.TM_Value = 110m;
			baseItem.TM_AgentDeclaredRate = 0;

			var perUnitItem = line.RateLineItems.AddNew();
			perUnitItem.TM_Type = Calculator.Items.Operator.UNT;
			perUnitItem.TM_Value = 70m;
			perUnitItem.TM_AgentDeclaredRate = 0;

			var useInclusiveBreaksItem = line.RateLineItems.AddNew();
			useInclusiveBreaksItem.TM_Type = BaseCombinedCalculator.Items.UseInclusiveBreaks;
			useInclusiveBreaksItem.TM_Text = "Y";

			var useHigherChargeableLowerRateRuleItem = line.RateLineItems.AddNew();
			useHigherChargeableLowerRateRuleItem.TM_Type = BaseCombinedCalculator.Items.HigherChargeableLowerRate;
			useHigherChargeableLowerRateRuleItem.TM_Text = "N";

			var isAccumulatedItem = line.RateLineItems.AddNew();
			isAccumulatedItem.TM_Type = BaseCombinedCalculator.Items.UseAccumulated;
			isAccumulatedItem.TM_Text = "N";

			var expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "CMB",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "UseInclusiveBreaks",
							Type = "Boolean",
							Value = true
						},
						new CalculatorAttribute()
						{
							Name = "UseHigherChargeableLowerRateRule",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "IsAccumulated",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "MultipleEquipmentsOverMaxWeightVolume",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "BreaksPer",
							Type = "String",
							Value = ""
						},
						new CalculatorAttribute()
						{
							Name = "Breaks",
							Type = "CalculatorBreakItem[]",
							Value = new []
							{
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "MIN",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 400,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1,
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "MAX",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 1500,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1,
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "BAS",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 110,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "UNT",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 70,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "-",
									Break = 100,
									BreakMinimum = null,
									UnitPrice = 20,
									FlatAmount = 200,
									Restricted = false,
									Text = "",
									Units = "KG",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "+",
									Break = 100,
									BreakMinimum = null,
									UnitPrice = null,
									FlatAmount = null,
									Restricted = true,
									Text = "Restriction Reason",
									Units = "",
									UnitMultiple = 1
								}
							}
						},
					},
				},
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos);

			minusItem.TM_AgentDeclaredRate = 18.5m;
			plusItem.TM_AgentDeclaredRate = 0;
			minItem.TM_AgentDeclaredRate = 300m;
			maxItem.TM_AgentDeclaredRate = 1600m;
			baseItem.TM_AgentDeclaredRate = 100;
			perUnitItem.TM_AgentDeclaredRate = 60;

			expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "CMB",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "UseInclusiveBreaks",
							Type = "Boolean",
							Value = true
						},
						new CalculatorAttribute()
						{
							Name = "UseHigherChargeableLowerRateRule",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "IsAccumulated",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "MultipleEquipmentsOverMaxWeightVolume",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "BreaksPer",
							Type = "String",
							Value = ""
						},
						new CalculatorAttribute()
						{
							Name = "Breaks",
							Type = "CalculatorBreakItem[]",
							Value = new []
							{
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "MIN",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 400,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1,
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "MAX",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 1500,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1,
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "BAS",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 110,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "UNT",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 70,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "-",
									Break = 100,
									BreakMinimum = null,
									UnitPrice = 20,
									FlatAmount = 200,
									Restricted = false,
									Text = "",
									Units = "KG",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "+",
									Break = 100,
									BreakMinimum = null,
									UnitPrice = null,
									FlatAmount = null,
									Restricted = true,
									Text = "Restriction Reason",
									Units = "",
									UnitMultiple = 1
								}
							}
						},
					},
				},
				new CalculatorInfo()
				{
					CWCode = "CMB",
					IsAgentRate = true,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "UseInclusiveBreaks",
							Type = "Boolean",
							Value = true
						},
						new CalculatorAttribute()
						{
							Name = "UseHigherChargeableLowerRateRule",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "IsAccumulated",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "MultipleEquipmentsOverMaxWeightVolume",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "BreaksPer",
							Type = "String",
							Value = ""
						},
						new CalculatorAttribute()
						{
							Name = "Breaks",
							Type = "CalculatorBreakItem[]",
							Value = new []
							{
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "MIN",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 300,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1,
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "MAX",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 1600,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1,
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "BAS",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 100,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "UNT",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 60,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "-",
									Break = 100,
									BreakMinimum = null,
									UnitPrice = 18.5m,
									FlatAmount = 200,
									Restricted = false,
									Text = "",
									Units = "KG",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "+",
									Break = 100,
									BreakMinimum = null,
									UnitPrice = null,
									FlatAmount = null,
									Restricted = true,
									Text = "Restriction Reason",
									Units = "",
									UnitMultiple = 1
								}
							}
						},
					},
				},
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos);
		}

		public void TestConvert_CartageCalculator()
		{
			var costing = Helper.NewCosting(TransportProvider1);

			var entry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "STD", "");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("FRT", CartageCalculator.Code, QuantityUnit.CN, "AUD");
			line.RateLineItems.RemoveAndDeleteAll();

			var calculator = line.GetCalculator<CartageCalculator>();

			var minusItem = calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 100, 20, 200);
			minusItem.TM_BreakMinimum = 30m;
			minusItem.TM_BreakWeightVolume = QuantityUnit.KG;

			var plus1Item = calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 100, 26, 0);

			var plus2Item = calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 150, 0, 0);
			plus2Item.TM_CallForPricing = true;
			plus2Item.TM_Text = "Restriction Reason";

			var minItem = calculator.AddRateLineItem(Calculator.Items.Operator.MIN, 0, 400m);
			var maxItem = calculator.AddRateLineItem(Calculator.Items.Operator.MAX, 0, 1500m);
			var baseItem = calculator.AddRateLineItem(Calculator.Items.Operator.BAS, 0, 110m);
			var perUnitItem = calculator.AddRateLineItem(Calculator.Items.Operator.UNT, 0, 70m);

			calculator.EquipmentType = "HWL";
			calculator.UseInclusiveBreaks = true;
			calculator.UseHigherChargeableLowerRateRule = false;
			calculator.IsAccumulated = false;

			var expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "CTG",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "UseInclusiveBreaks",
							Type = "Boolean",
							Value = true
						},
						new CalculatorAttribute()
						{
							Name = "UseHigherChargeableLowerRateRule",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "IsAccumulated",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "MultipleEquipmentsOverMaxWeightVolume",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "EquipmentType",
							Type = "String",
							Value = "HWL"
						},
						new CalculatorAttribute()
						{
							Name = "BreaksPer",
							Type = "String",
							Value = ""
						},
						new CalculatorAttribute()
						{
							Name = "Breaks",
							Type = "CalculatorBreakItem[]",
							Value = new []
							{
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "MIN",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 400,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "MAX",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 1500,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "BAS",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 110,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "UNT",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 70,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "-",
									Break = 100,
									BreakMinimum = 30,
									UnitPrice = 20,
									FlatAmount = 200,
									Restricted = false,
									Text = "",
									Units = "KG",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "+",
									Break = 100,
									BreakMinimum = null,
									UnitPrice = 26,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "+",
									Break = 150,
									BreakMinimum = null,
									UnitPrice = null,
									FlatAmount = null,
									Restricted = true,
									Text = "Restriction Reason",
									Units = "",
									UnitMultiple = 1
								}
							}
						},
					},
				},
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos);

			minusItem.TM_AgentDeclaredRate = 19m;
			plus1Item.TM_AgentDeclaredRate = 25m;
			plus2Item.TM_AgentDeclaredRate = 0m;

			minItem.TM_AgentDeclaredRate = 390m;
			maxItem.TM_AgentDeclaredRate = 1490m;
			baseItem.TM_AgentDeclaredRate = 109m;
			perUnitItem.TM_AgentDeclaredRate = 69m;

			expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "CTG",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "UseInclusiveBreaks",
							Type = "Boolean",
							Value = true
						},
						new CalculatorAttribute()
						{
							Name = "UseHigherChargeableLowerRateRule",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "IsAccumulated",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "MultipleEquipmentsOverMaxWeightVolume",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "EquipmentType",
							Type = "String",
							Value = "HWL"
						},
						new CalculatorAttribute()
						{
							Name = "BreaksPer",
							Type = "String",
							Value = ""
						},
						new CalculatorAttribute()
						{
							Name = "Breaks",
							Type = "CalculatorBreakItem[]",
							Value = new []
							{
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "MIN",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 400,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "MAX",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 1500,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "BAS",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 110,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "UNT",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 70,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "-",
									Break = 100,
									BreakMinimum = 30,
									UnitPrice = 20,
									FlatAmount = 200,
									Restricted = false,
									Text = "",
									Units = "KG",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "+",
									Break = 100,
									BreakMinimum = null,
									UnitPrice = 26,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "+",
									Break = 150,
									BreakMinimum = null,
									UnitPrice = null,
									FlatAmount = null,
									Restricted = true,
									Text = "Restriction Reason",
									Units = "",
									UnitMultiple = 1
								}
							}
						},
					},
				},
				new CalculatorInfo()
				{
					CWCode = "CTG",
					IsAgentRate = true,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "UseInclusiveBreaks",
							Type = "Boolean",
							Value = true
						},
						new CalculatorAttribute()
						{
							Name = "UseHigherChargeableLowerRateRule",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "IsAccumulated",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "MultipleEquipmentsOverMaxWeightVolume",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "EquipmentType",
							Type = "String",
							Value = "HWL"
						},
						new CalculatorAttribute()
						{
							Name = "BreaksPer",
							Type = "String",
							Value = ""
						},
						new CalculatorAttribute()
						{
							Name = "Breaks",
							Type = "CalculatorBreakItem[]",
							Value = new []
							{
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "MIN",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 390,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "MAX",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 1490,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "BAS",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 109,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "UNT",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 69,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "-",
									Break = 100,
									BreakMinimum = 30,
									UnitPrice = 19,
									FlatAmount = 200,
									Restricted = false,
									Text = "",
									Units = "KG",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "+",
									Break = 100,
									BreakMinimum = null,
									UnitPrice = 25,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = null,
									Operator = "+",
									Break = 150,
									BreakMinimum = null,
									UnitPrice = null,
									FlatAmount = null,
									Restricted = true,
									Text = "Restriction Reason",
									Units = "",
									UnitMultiple = 1
								}
							}
						},
					},
				},
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos);
		}

		public void TestConvert_CostBasedCalculator()
		{
			var tariff = Helper.NewCompanyTariff();
			var tariffEntry = tariff.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "STD", "20GP");
			var line = tariffEntry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			line.RateLineItems.RemoveAndDeleteAll();

			var calculator = line.GetCalculator<CompanyTariffOrCostBasedCalculator>();

			var percentItem = calculator.AddRateLineItem(CalculatorConstants.Type.PER, 0, 10m, 0);
			percentItem.TM_AgentDeclaredRate = 0;

			var minimumItem = calculator.AddRateLineItem(Calculator.Items.Operator.MIN, 0, 85m, 0);
			minimumItem.TM_AgentDeclaredRate = 0;

			var baseRateItem = calculator.AddRateLineItem(Calculator.Items.Operator.BAS, 0, 7.2m, 0);
			baseRateItem.TM_AgentDeclaredRate = 0;

			var perUnitItem = calculator.AddRateLineItem(Calculator.Items.Operator.UNT, 0, 1.2m, 0);
			perUnitItem.TM_AgentDeclaredRate = 0;

			var perUnitPercentItem = calculator.AddRateLineItem(CalculatorConstants.Type.PRU, 0, 0.2m, 0);
			perUnitPercentItem.TM_AgentDeclaredRate = 0;

			calculator.CalculationOrder = "FIX";
			calculator.EquipmentType = FCLEquipmentNeeded.SideLoader;
			calculator.MessageType = "IMP";
			calculator.MessageSubType = "FRM";

			var expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "CST",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "Percent",
							Type = "Decimal",
							Value = 10m
						},
						new CalculatorAttribute()
						{
							Name = "Minimum",
							Type = "Decimal",
							Value = 85m
						},
						new CalculatorAttribute()
						{
							Name = "BaseRate",
							Type = "Decimal",
							Value = 7.2m
						},
						new CalculatorAttribute()
						{
							Name = "PerUnit",
							Type = "Decimal",
							Value = 1.2m
						},
						new CalculatorAttribute()
						{
							Name = "PerUnitPercent",
							Type = "Decimal",
							Value = 0.2m
						},
						new CalculatorAttribute()
						{
							Name = "CalculationOrder",
							Type = "String",
							Value = "FIX"
						},
						new CalculatorAttribute()
						{
							Name = "EquipmentType",
							Type = "String",
							Value = "SDL"
						},
						new CalculatorAttribute()
						{
							Name = "MessageType",
							Type = "String",
							Value = "IMP"
						},
						new CalculatorAttribute()
						{
							Name = "MessageSubType",
							Type = "String",
							Value = "FRM"
						},
					}
				},
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos, RatingConstants.RatingHeaderTypes.Tariff);

			percentItem.TM_AgentDeclaredRate = 9.8m;
			minimumItem.TM_AgentDeclaredRate = 80m;
			baseRateItem.TM_AgentDeclaredRate = 7m;
			perUnitItem.TM_AgentDeclaredRate = 1.1m;
			perUnitPercentItem.TM_AgentDeclaredRate = 0.1m;

			expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "CST",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "Percent",
							Type = "Decimal",
							Value = 10m
						},
						new CalculatorAttribute()
						{
							Name = "Minimum",
							Type = "Decimal",
							Value = 85m
						},
						new CalculatorAttribute()
						{
							Name = "BaseRate",
							Type = "Decimal",
							Value = 7.2m
						},
						new CalculatorAttribute()
						{
							Name = "PerUnit",
							Type = "Decimal",
							Value = 1.2m
						},
						new CalculatorAttribute()
						{
							Name = "PerUnitPercent",
							Type = "Decimal",
							Value = 0.2m
						},
						new CalculatorAttribute()
						{
							Name = "CalculationOrder",
							Type = "String",
							Value = "FIX"
						},
						new CalculatorAttribute()
						{
							Name = "EquipmentType",
							Type = "String",
							Value = "SDL"
						},
						new CalculatorAttribute()
						{
							Name = "MessageType",
							Type = "String",
							Value = "IMP"
						},
						new CalculatorAttribute()
						{
							Name = "MessageSubType",
							Type = "String",
							Value = "FRM"
						},
					}
				},
				new CalculatorInfo()
				{
					CWCode = "CST",
					IsAgentRate = true,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "Percent",
							Type = "Decimal",
							Value = 9.8m
						},
						new CalculatorAttribute()
						{
							Name = "Minimum",
							Type = "Decimal",
							Value = 80m
						},
						new CalculatorAttribute()
						{
							Name = "BaseRate",
							Type = "Decimal",
							Value = 7m
						},
						new CalculatorAttribute()
						{
							Name = "PerUnit",
							Type = "Decimal",
							Value = 1.1m
						},
						new CalculatorAttribute()
						{
							Name = "PerUnitPercent",
							Type = "Decimal",
							Value = 0.1m
						},
						new CalculatorAttribute()
						{
							Name = "CalculationOrder",
							Type = "String",
							Value = "FIX"
						},
						new CalculatorAttribute()
						{
							Name = "EquipmentType",
							Type = "String",
							Value = "SDL"
						},
						new CalculatorAttribute()
						{
							Name = "MessageType",
							Type = "String",
							Value = "IMP"
						},
						new CalculatorAttribute()
						{
							Name = "MessageSubType",
							Type = "String",
							Value = "FRM"
						},
					}
				},
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos, RatingConstants.RatingHeaderTypes.Tariff);
		}

		public void TestConvert_CompanyTariffBasedCalculator()
		{
			var tariff = Helper.NewCompanyTariff();
			var tariffEntry = tariff.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "STD", "20GP");
			var line = tariffEntry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			var calculator = line.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			calculator.Percent = 10m;
			calculator.Minimum = 85m;
			calculator.BaseRate = 7.2m;
			calculator.PerUnit = 1.2m;
			calculator.PerUnitPercent = 0.2m;
			calculator.CalculationOrder = "PER";
			calculator.EquipmentType = FCLEquipmentNeeded.WaitForUnpack;
			calculator.MessageType = "IMP";
			calculator.MessageSubType = "FRM";

			var expectedAttributes = new[]
			{
				new CalculatorAttribute()
				{
					Name = "Percent",
					Type = "Decimal",
					Value = 10m
				},
				new CalculatorAttribute()
				{
					Name = "Minimum",
					Type = "Decimal",
					Value = 85m
				},
				new CalculatorAttribute()
				{
					Name = "BaseRate",
					Type = "Decimal",
					Value = 7.2m
				},
				new CalculatorAttribute()
				{
					Name = "PerUnit",
					Type = "Decimal",
					Value = 1.2m
				},
				new CalculatorAttribute()
				{
					Name = "PerUnitPercent",
					Type = "Decimal",
					Value = 0.2m
				},
				new CalculatorAttribute()
				{
					Name = "CalculationOrder",
					Type = "String",
					Value = "PER"
				},
				new CalculatorAttribute()
				{
					Name = "EquipmentType",
					Type = "String",
					Value = "WUP"
				},
				new CalculatorAttribute()
				{
					Name = "MessageType",
					Type = "String",
					Value = "IMP"
				},
				new CalculatorAttribute()
				{
					Name = "MessageSubType",
					Type = "String",
					Value = "FRM"
				},
			};

			AssertCalculatorConversion(line, CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, expectedAttributes, rateType: RatingConstants.RatingHeaderTypes.Tariff);
		}

		public void TestConvert_CartageZoneDistanceCalculator()
		{
			var auZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia);
			var auZone1 = auZoneSet.CreateRateTransportZoneForTest("AU Zone 1");
			var auZone2 = auZoneSet.CreateRateTransportZoneForTest("AU Zone 2");
			var auZone3 = auZoneSet.CreateRateTransportZoneForTest("AU Zone 3");

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, "", "AU");
			var line = entry.AddRateLine("DCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			var calculator = line.GetCalculator<CartageZoneDistanceCalculator>();
			calculator.UseACIZones = false;
			calculator.EquipmentType = EquipmentNeeded.Any;
			calculator.UseInclusiveBreaks = true;
			calculator.UseHigherChargeableLowerRateRule = false;
			calculator.IsAccumulated = false;
			var perUnitItem = calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 10m, auZone1.PK);
			var minItem = calculator.AddRateLineItemWithZone(Calculator.Items.Operator.MIN, 0m, 110m, auZone1.PK);
			var maxItem = calculator.AddRateLineItemWithZone(Calculator.Items.Operator.MAX, 0m, 1200m, auZone1.PK);

			var minusItem = calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 50m, 10m, auZone2.PK);
			var plusItem = calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 50m, 15m, auZone2.PK);

			var restrictedItem = calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 200m, 0m, auZone2.PK);
			restrictedItem.TM_CallForPricing = true;
			restrictedItem.TM_Text = "Restriction Reason";

			var baseItem = calculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0m, 80m, auZone3.PK);

			var expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "CTZ",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "UseACIZones",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "UseInclusiveBreaks",
							Type = "Boolean",
							Value = true
						},
						new CalculatorAttribute()
						{
							Name = "UseHigherChargeableLowerRateRule",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "IsAccumulated",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "MultipleEquipmentsOverMaxWeightVolume",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "EquipmentType",
							Type = "String",
							Value = "ANY"
						},
						new CalculatorAttribute()
						{
							Name = "BreaksPer",
							Type = "String",
							Value = ""
						},
						new CalculatorAttribute()
						{
							Name = "Breaks",
							Type = "CalculatorBreakItem[]",
							Value = new []
							{
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 1",
									Operator = "UNT",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 10m,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 1",
									Operator = "MIN",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 110m,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 1",
									Operator = "MAX",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 1200m,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 2",
									Operator = "-",
									Break = 50m,
									BreakMinimum = null,
									UnitPrice = 10m,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 2",
									Operator = "+",
									Break = 50m,
									BreakMinimum = null,
									UnitPrice = 15m,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 2",
									Operator = "+",
									Break = 200m,
									BreakMinimum = null,
									UnitPrice = null,
									FlatAmount = null,
									Restricted = true,
									Text = "Restriction Reason",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 3",
									Operator = "BAS",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 80m,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
							}
						},
					}
				},
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos, RatingConstants.RatingHeaderTypes.ClientRate);

			perUnitItem.TM_AgentDeclaredRate = 9.5m;
			minItem.TM_AgentDeclaredRate = 0m;
			maxItem.TM_AgentDeclaredRate = 1400m;
			minusItem.TM_AgentDeclaredRate = 8m;
			plusItem.TM_AgentDeclaredRate = 12m;
			restrictedItem.TM_AgentDeclaredRate = 0m;
			baseItem.TM_AgentDeclaredRate = 69;

			expectedCalculatorInfos = new[]
			{
				new CalculatorInfo()
				{
					CWCode = "CTZ",
					IsAgentRate = false,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "UseACIZones",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "UseInclusiveBreaks",
							Type = "Boolean",
							Value = true
						},
						new CalculatorAttribute()
						{
							Name = "UseHigherChargeableLowerRateRule",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "IsAccumulated",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "MultipleEquipmentsOverMaxWeightVolume",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "EquipmentType",
							Type = "String",
							Value = "ANY"
						},
						new CalculatorAttribute()
						{
							Name = "BreaksPer",
							Type = "String",
							Value = ""
						},
						new CalculatorAttribute()
						{
							Name = "Breaks",
							Type = "CalculatorBreakItem[]",
							Value = new []
							{
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 1",
									Operator = "UNT",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 10m,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 1",
									Operator = "MIN",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 110m,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 1",
									Operator = "MAX",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 1200m,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 2",
									Operator = "-",
									Break = 50m,
									BreakMinimum = null,
									UnitPrice = 10m,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 2",
									Operator = "+",
									Break = 50m,
									BreakMinimum = null,
									UnitPrice = 15m,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 2",
									Operator = "+",
									Break = 200m,
									BreakMinimum = null,
									UnitPrice = null,
									FlatAmount = null,
									Restricted = true,
									Text = "Restriction Reason",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 3",
									Operator = "BAS",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 80m,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
							}
						},
					}
				},
				new CalculatorInfo()
				{
					CWCode = "CTZ",
					IsAgentRate = true,
					Attributes = new[]
					{
						new CalculatorAttribute()
						{
							Name = "UseACIZones",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "UseInclusiveBreaks",
							Type = "Boolean",
							Value = true
						},
						new CalculatorAttribute()
						{
							Name = "UseHigherChargeableLowerRateRule",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "IsAccumulated",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "MultipleEquipmentsOverMaxWeightVolume",
							Type = "Boolean",
							Value = false
						},
						new CalculatorAttribute()
						{
							Name = "EquipmentType",
							Type = "String",
							Value = "ANY"
						},
						new CalculatorAttribute()
						{
							Name = "BreaksPer",
							Type = "String",
							Value = ""
						},
						new CalculatorAttribute()
						{
							Name = "Breaks",
							Type = "CalculatorBreakItem[]",
							Value = new []
							{
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 1",
									Operator = "UNT",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 9.5m,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 1",
									Operator = "MIN",
									Break = null,
									BreakMinimum = null,
									UnitPrice = null,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 1",
									Operator = "MAX",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 1400m,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 2",
									Operator = "-",
									Break = 50m,
									BreakMinimum = null,
									UnitPrice = 8m,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 2",
									Operator = "+",
									Break = 50m,
									BreakMinimum = null,
									UnitPrice = 12m,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 2",
									Operator = "+",
									Break = 200m,
									BreakMinimum = null,
									UnitPrice = null,
									FlatAmount = null,
									Restricted = true,
									Text = "Restriction Reason",
									Units = "",
									UnitMultiple = 1
								},
								new CalculatorBreakItem()
								{
									CWTransportZone = "AU Zone 3",
									Operator = "BAS",
									Break = null,
									BreakMinimum = null,
									UnitPrice = 69,
									FlatAmount = null,
									Restricted = false,
									Text = "",
									Units = "",
									UnitMultiple = 1
								},
							}
						},
					}
				},
			};

			AssertCalculatorsConversion(line, expectedCalculatorInfos, RatingConstants.RatingHeaderTypes.ClientRate);
		}

		public void TestConvert_HighestRateCalculator()
		{
			var clinet = Helper.NewOrgHeader();

			var rate = Helper.NewClientRate(clinet);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.ULD, "AUSYD", "USLAX", ZString.Empty, "40GP", removeLines: true);

			var line = entry.AddRateLine("FRT", HighestRateCalculator.Code);
			line.GetCalculator<HighestRateCalculator>().RatePickRule = "HRM";
			var item1 = line.RateLineItems.AddNew();
			item1.TM_Type = Calculator.Items.Operator.UNT;
			item1.TM_BreakWeightVolume = QuantityUnit.KG;
			item1.TM_RelevantValue = 5m;
			item1.TM_UnitMultiple = 12;

			var item2 = line.RateLineItems.AddNew();
			item2.TM_Type = Calculator.Items.Operator.UNT;
			item2.TM_BreakWeightVolume = QuantityUnit.M3;
			item2.TM_RelevantValue = 100m;
			item2.TM_UnitMultiple = 16;

			var item3 = line.RateLineItems.AddNew();
			item3.TM_Type = Calculator.Items.Operator.MIN;
			item3.TM_BreakWeightVolume = QuantityUnit.KG;
			item3.TM_RelevantValue = 50m;

			var expectedAttributes = new[]
			{
				new CalculatorAttribute()
				{
					Name = "RatePickRule",
					Type = "String",
					Value = "HRM"
				},
				new CalculatorAttribute()
				{
					Name = "Breaks",
					Type = "CalculatorBreakItem[]",
					Value = new []
					{
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "UNT",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 5m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "KG",
							UnitMultiple = 12
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "UNT",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 100m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "M3",
							UnitMultiple = 16
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "MIN",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 50,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "KG",
							UnitMultiple = 1
						},
					}
				},
			};

			AssertCalculatorConversion(line, HighestRateCalculator.Code, expectedAttributes, rateType: RatingConstants.RatingHeaderTypes.ClientRate);
		}

		public void TestConvert_FreightInclusiveCalculator()
		{
			var cafChargeCode = Helper.ChargeCodes["CAF"];
			var costing = Helper.NewCosting(TransportProvider1);
			var entry = costing.AddRateEntry(ContainerModes.FCL, TransportModes.Sea, "AUSYD", "USLAX", "", "20GP", removeLines: true);
			entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia).GetCalculator<UnitCalculator>().PerUnit = 20;
			var line = entry.AddRateLine("BAF", FreightInclusiveCalculator.Code, currencyCode: CurrencyCodes.Australia);
			line.RateLineItems.RemoveAndDeleteAll();
			var preCarriageItem = line.RateLineItems.AddNew();
			preCarriageItem.TM_Type = FreightInclusiveCalculator.Items.PreCarriageOnCarriageChargeType;
			preCarriageItem.TM_AC = cafChargeCode.PK;

			line.GetCalculator<FreightInclusiveCalculator>().FreightCalcType = FreightInclusiveCalculator.FreightCalcTypes.Included;

			var expectedAttributes = new[]
			{
				new CalculatorAttribute()
				{
					Name = "FreightCalcType",
					Type = "String",
					Value = "INC"
				},
				new CalculatorAttribute()
				{
					Name = "PreCarriageOrOnCarriageChargeCode",
					Type = "String",
					Value = "CAF"
				},
			};

			AssertCalculatorConversion(line, FreightInclusiveCalculator.Code, expectedAttributes);
		}

		public void TestConvert_ValueRangeCalculator()
		{
			var costing = Helper.NewCosting(TransportProvider1);

			var entry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "STD", "");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("FRT", ValueRangeCalculator.Code, QuantityUnit.CN, "AUD");
			line.RateLineItems.RemoveAndDeleteAll();

			var item1 = line.RateLineItems.AddNew();
			item1.TM_Type = Calculator.Items.Operator.Minus;
			item1.TM_Break = 100m;
			item1.TM_Value = 20m;
			item1.TM_FlatAmount = 200m;
			item1.TM_BreakWeightVolume = QuantityUnit.KG;

			var item2 = line.RateLineItems.AddNew();
			item2.TM_Type = Calculator.Items.Operator.Plus;
			item2.TM_Break = 100m;
			item2.TM_CallForPricing = true;
			item2.TM_Text = "Restriction Reason";

			var calculator = line.GetCalculator<ValueRangeCalculator>();
			calculator[Calculator.Items.Operator.MIN] = (ZDecimal)400m;

			calculator.ApplyTo = "CUV";

			var expectedAttributes = new[]
			{
				new CalculatorAttribute()
				{
					Name = "ApplyTo",
					Type = "String",
					Value = "CUV"
				},
				new CalculatorAttribute()
				{
					Name = "Breaks",
					Type = "CalculatorBreakItem[]",
					Value = new []
					{
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "MIN",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 400,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "-",
							Break = 100,
							BreakMinimum = null,
							UnitPrice = 20,
							FlatAmount = 200,
							Restricted = false,
							Text = "",
							Units = "KG",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "+",
							Break = 100,
							BreakMinimum = null,
							UnitPrice = null,
							FlatAmount = null,
							Restricted = true,
							Text = "Restriction Reason",
							Units = "",
							UnitMultiple = 1
						}
					}
				},
			};

			AssertCalculatorConversion(line, ValueRangeCalculator.Code, expectedAttributes);
		}

		public void TestConvert_NoteCalculator()
		{
			var client = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "CNSHA", removeLines: true);

			var line = entry.AddRateLine("FRT", NoteCalculator.Code);
			line.GetCalculator<NoteCalculator>().ShowOnBillingWithoutPrefix = false;

			var item1 = line.RateLineItems.AddNew();
			item1.TM_Text = "Some Costs 1";
			item1.TM_Value = 60m;

			var item2 = line.RateLineItems.AddNew();
			item2.TM_Text = "Some Costs 2";
			item2.TM_Value = 90m;

			var expectedAttributes = new[]
			{
				new CalculatorAttribute()
				{
					Name = "ShowOnBillingWithoutPrefix",
					Type = "Boolean",
					Value = false
				},
				new CalculatorAttribute()
				{
					Name = "Breaks",
					Type = "CalculatorBreakItem[]",
					Value = new []
					{
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 60m,
							FlatAmount = null,
							Restricted = false,
							Text = "Some Costs 1",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 90,
							FlatAmount = null,
							Restricted = false,
							Text = "Some Costs 2",
							Units = "",
							UnitMultiple = 1
						}
					}
				},
			};

			AssertCalculatorConversion(line, NoteCalculator.Code, expectedAttributes, rateType: RatingConstants.RatingHeaderTypes.ClientRate);
		}

		public void TestConvert_HousebillReleaseTypeCalculator()
		{
			var client = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "AU", "HK", removeLines: true);
			var line = entry.AddRateLine("OBILL", HousebillReleaseTypeCalculator.Code);

			var calculator = line.GetCalculator<HousebillReleaseTypeCalculator>();
			calculator.AddRateLineItem("STD", 0m, 10m);
			calculator.AddRateLineItem(ShipmentReleaseTypes.BankLetterOfCredit, 0m, 20m);
			calculator.AddRateLineItem(ShipmentReleaseTypes.BankSightDraft, 0m, 30m);
			calculator.AddRateLineItem(ShipmentReleaseTypes.BankTimeDraft, 0m, 40m);
			calculator.AddRateLineItem(ShipmentReleaseTypes.CashDoc, 0m, 50m);
			calculator.AddRateLineItem(ShipmentReleaseTypes.Cheque, 0m, 60m);
			calculator.AddRateLineItem(ShipmentReleaseTypes.ExpressBofL, 0m, 70m);
			calculator.AddRateLineItem(ShipmentReleaseTypes.Indemnity, 0m, 80m);
			calculator.AddRateLineItem(ShipmentReleaseTypes.NonNegotiable, 0m, 90m);
			calculator.AddRateLineItem(ShipmentReleaseTypes.OriginalReq, 0m, 100m);
			calculator.AddRateLineItem(ShipmentReleaseTypes.OriginalReqSurrender, 0m, 110m);
			calculator.AddRateLineItem(ShipmentReleaseTypes.SeaWaybill, 0m, 120m);

			var expectedAttributes = new[]
			{
				new CalculatorAttribute()
				{
					Name = "Breaks",
					Type = "CalculatorBreakItem[]",
					Value = new []
					{
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "STD",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 10m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "BRR",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 20m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "BSD",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 30m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "BTD",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 40m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "CAD",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 50m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "CSH",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 60m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "EBL",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 70m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "LOI",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 80m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "NON",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 90m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "OBR",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 100m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "OBO",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 110m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "SWB",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 120m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
					}
				},
			};

			AssertCalculatorConversion(line, HousebillReleaseTypeCalculator.Code, expectedAttributes, rateType: RatingConstants.RatingHeaderTypes.ClientRate);
		}

		public void TestConvert_PercentageCalculator()
		{
			var bafChargeCode = Helper.ChargeCodes["BAF"];
			var client = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", removeLines: true);
			var line = entry.AddRateLine("FRT", PercentageCalculator.Code, "KG", CurrencyCodes.Australia);
			var calculator = line.GetCalculator<PercentageCalculator>();

			calculator.Percent = 10.5m;
			calculator.Minimum = 400m;
			calculator.BaseRate = 10m;
			calculator.Maximum = 1200m;
			calculator.Rate = 12m;
			calculator.IncludeGST = true;
			calculator.IsPartThereof = true;
			calculator.ValueOrPartThereOf = 60m;
			calculator.GreaterCharge = false;
			calculator.AddApplyToItem("DSB");
			calculator.AddApplyToItem("LOD");
			var seqLineItem = calculator.AddApplyToItem("SEQ");
			seqLineItem.TM_Value = 0;

			var codLineItem1 = calculator.AddApplyToItem("COD");
			codLineItem1.TM_AC = bafChargeCode.PK;

			var codLineItem2 = calculator.AddApplyToItem("COD");
			codLineItem2.TM_AC = ZGuid.Empty;

			var expectedAttributes = new[]
			{
				new CalculatorAttribute()
				{
					Name = "Percent",
					Type = "Decimal",
					Value = 10.5m
				},
				new CalculatorAttribute()
				{
					Name = "Minimum",
					Type = "Decimal",
					Value = 400m
				},
				new CalculatorAttribute()
				{
					Name = "BaseRate",
					Type = "Decimal",
					Value = 10m
				},
				new CalculatorAttribute()
				{
					Name = "Maximum",
					Type = "Decimal",
					Value = 1200m
				},
				new CalculatorAttribute()
				{
					Name = "Rate",
					Type = "Decimal",
					Value = 12m
				},
				new CalculatorAttribute()
				{
					Name = "IncludeGST",
					Type = "Boolean",
					Value = true
				},
				new CalculatorAttribute()
				{
					Name = "IsPartThereof",
					Type = "Boolean",
					Value = true
				},
				new CalculatorAttribute()
				{
					Name = "ValueOrPartThereOf",
					Type = "Decimal",
					Value = 60m
				},
				new CalculatorAttribute()
				{
					Name = "GreaterCharge",
					Type = "Boolean",
					Value = false
				},
				new CalculatorAttribute()
				{
					Name = "ApplyToCharges",
					Type = "CalculatorApplyToCharge[]",
					Value = new []
					{
						new CalculatorApplyToCharge()
						{
							Type = "DSB",
							ApplyCharge = null,
						},
						new CalculatorApplyToCharge()
						{
							Type = "LOD",
							ApplyCharge = null,
						},
						new CalculatorApplyToCharge()
						{
							Type = "SEQ",
							CalculationPriority = 0,
						},
						new CalculatorApplyToCharge()
						{
							Type = "COD",
							ApplyCharge = "BAF",
						},
						new CalculatorApplyToCharge()
						{
							Type = "COD",
							ApplyCharge = null,
						}
					}
				},
			};

			AssertCalculatorConversion(line, PercentageCalculator.Code, expectedAttributes, rateType: RatingConstants.RatingHeaderTypes.ClientRate);
		}

		public void TestConvert_PercentageBreaksCalculator()
		{
			var bafChargeCode = Helper.ChargeCodes["BAF"];
			var client = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", removeLines: true);
			var line = entry.AddRateLine("FRT", PercentageBreaksCalculator.Code, "KG", CurrencyCodes.Australia);
			line.RateLineItems.RemoveAndDeleteAll();

			var item1 = line.RateLineItems.AddNew();
			item1.TM_Type = Calculator.Items.Operator.Minus;
			item1.TM_Break = 100m;
			item1.TM_Value = 20m;
			item1.TM_FlatAmount = 200m;

			var item2 = line.RateLineItems.AddNew();
			item2.TM_Type = Calculator.Items.Operator.Plus;
			item2.TM_Break = 100m;
			item2.TM_CallForPricing = true;
			item2.TM_Text = "Restriction Reason";

			var calculator = line.GetCalculator<PercentageBreaksCalculator>();
			calculator[Calculator.Items.Operator.MIN] = (ZDecimal)400m;
			calculator[Calculator.Items.Operator.MAX] = (ZDecimal)1500m;
			calculator[Calculator.Items.Operator.BAS] = (ZDecimal)110m;
			calculator[Calculator.Items.Operator.UNT] = (ZDecimal)34.5m;

			calculator.IncludeGST = true;
			calculator.UseBreaksBasedOnValues = false;
			calculator.UseInclusiveBreaks = false;
			calculator.MultipleEquipmentsOverMaxWeightVolume = false;
			calculator.UseHigherChargeableLowerRateRule = true;
			calculator.IsAccumulated = false;
			calculator.AddApplyToItem("DSB");
			calculator.AddApplyToItem("LOD");
			var seqLineItem = calculator.AddApplyToItem("SEQ");
			seqLineItem.TM_Value = 16m;

			var codLineItem = calculator.AddApplyToItem("COD");
			codLineItem.TM_AC = bafChargeCode.PK;

			var expectedAttributes = new[]
			{
				new CalculatorAttribute()
				{
					Name = "IncludeGST",
					Type = "Boolean",
					Value = true
				},
				new CalculatorAttribute()
				{
					Name = "UseBreaksBasedOnValues",
					Type = "Boolean",
					Value = false
				},
				new CalculatorAttribute()
				{
					Name = "UseInclusiveBreaks",
					Type = "Boolean",
					Value = false
				},
				new CalculatorAttribute()
				{
					Name = "MultipleEquipmentsOverMaxWeightVolume",
					Type = "Boolean",
					Value = false
				},
				new CalculatorAttribute()
				{
					Name = "UseHigherChargeableLowerRateRule",
					Type = "Boolean",
					Value = false // This property's get method is set to false, so it always returns false;
				},
				new CalculatorAttribute()
				{
					Name = "IsAccumulated",
					Type = "Boolean",
					Value = false
				},
				new CalculatorAttribute()
				{
					Name = "BreaksPer",
					Type = "String",
					Value = ""
				},
				new CalculatorAttribute()
				{
					Name = "Breaks",
					Type = "CalculatorBreakItem[]",
					Value = new []
					{
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "MIN",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 400,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "MAX",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 1500,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "BAS",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 110,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "UNT",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 34.5m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "-",
							Break = 100,
							BreakMinimum = null,
							UnitPrice = 20,
							FlatAmount = 200,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "+",
							Break = 100,
							BreakMinimum = null,
							UnitPrice = null,
							FlatAmount = null,
							Restricted = true,
							Text = "Restriction Reason",
							Units = "",
							UnitMultiple = 1
						}
					}
				},
				new CalculatorAttribute()
				{
					Name = "ApplyToCharges",
					Type = "CalculatorApplyToCharge[]",
					Value = new []
					{
						new CalculatorApplyToCharge()
						{
							Type = "DSB",
							ApplyCharge = null,
						},
						new CalculatorApplyToCharge()
						{
							Type = "LOD",
							ApplyCharge = null,
						},
						new CalculatorApplyToCharge()
						{
							Type = "COD",
							ApplyCharge = "BAF",
						},
						new CalculatorApplyToCharge()
						{
							Type = "SEQ",
							CalculationPriority = 16,
						},
					}
				},
			};

			AssertCalculatorConversion(line, PercentageBreaksCalculator.Code, expectedAttributes, rateType: RatingConstants.RatingHeaderTypes.ClientRate);
		}

		public void TestConvert_ProfitShareRebateCalculator()
		{
			var bafChargeCode = Helper.ChargeCodes["BAF"];
			var client = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", removeLines: true);
			var line = entry.AddRateLine("FRT", ProfitShareRebateCalculator.Code, "KG", CurrencyCodes.Australia);
			var calculator = line.GetCalculator<ProfitShareRebateCalculator>();

			calculator.Percent = 10.5m;
			calculator.Minimum = 400m;
			calculator.BaseRate = 10m;
			calculator.Maximum = 1200m;
			calculator.ZeroWhenLoss = true;
			calculator.AddApplyToItem("DSB");
			calculator.AddApplyToItem("LOD");
			var codLineItem = calculator.AddApplyToItem("COD");
			codLineItem.TM_AC = bafChargeCode.PK;

			var expectedAttributes = new[]
			{
				new CalculatorAttribute()
				{
					Name = "Percent",
					Type = "Decimal",
					Value = 10.5m
				},
				new CalculatorAttribute()
				{
					Name = "Minimum",
					Type = "Decimal",
					Value = 400m
				},
				new CalculatorAttribute()
				{
					Name = "BaseRate",
					Type = "Decimal",
					Value = 10m
				},
				new CalculatorAttribute()
				{
					Name = "Maximum",
					Type = "Decimal",
					Value = 1200m
				},
				new CalculatorAttribute()
				{
					Name = "ZeroWhenLoss",
					Type = "Boolean",
					Value = true
				},
				new CalculatorAttribute()
				{
					Name = "ApplyToCharges",
					Type = "CalculatorApplyToCharge[]",
					Value = new []
					{
						new CalculatorApplyToCharge()
						{
							Type = "DSB",
							ApplyCharge = null,
						},
						new CalculatorApplyToCharge()
						{
							Type = "LOD",
							ApplyCharge = null,
						},
						new CalculatorApplyToCharge()
						{
							Type = "COD",
							ApplyCharge = "BAF",
						},
					}
				},
			};

			AssertCalculatorConversion(line, ProfitShareRebateCalculator.Code, expectedAttributes, rateType: RatingConstants.RatingHeaderTypes.ClientRate);
		}

		public void TestConvert_SplitMonthBillingCalculator()
		{
			var costing = Helper.NewCosting(TransportProvider1);

			var entry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "STD", "");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("FRT", SplitMonthBillingCalculator.Code, QuantityUnit.CN, "AUD");
			line.RateLineItems.RemoveAndDeleteAll();

			var item1 = line.RateLineItems.AddNew();
			item1.TM_Type = Calculator.Items.Operator.Minus;
			item1.TM_Break = 100m;
			item1.TM_Value = 20m;
			item1.TM_FlatAmount = 200m;
			item1.TM_BreakWeightVolume = QuantityUnit.KG;

			var item2 = line.RateLineItems.AddNew();
			item2.TM_Type = Calculator.Items.Operator.Plus;
			item2.TM_Break = 100m;
			item2.TM_CallForPricing = true;
			item2.TM_Text = "Restriction Reason";

			var calculator = line.GetCalculator<SplitMonthBillingCalculator>();
			calculator[Calculator.Items.Operator.MIN] = (ZDecimal)400m;
			calculator[Calculator.Items.Operator.BAS] = (ZDecimal)110m;
			calculator[Calculator.Items.Operator.MAX] = (ZDecimal)1430m; // will not be included in the CalculatorBreakItem because the operator is not valid for this calculator
			calculator[Calculator.Items.Operator.UNT] = (ZDecimal)12.8m; // will not be included in the CalculatorBreakItem because the operator is not valid for this calculator

			var expectedAttributes = new[]
			{
				new CalculatorAttribute()
				{
					Name = "Breaks",
					Type = "CalculatorBreakItem[]",
					Value = new []
					{
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "MIN",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 400,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "BAS",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 110,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "-",
							Break = 100,
							BreakMinimum = null,
							UnitPrice = 20,
							FlatAmount = 200,
							Restricted = false,
							Text = "",
							Units = "KG",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "+",
							Break = 100,
							BreakMinimum = null,
							UnitPrice = null,
							FlatAmount = null,
							Restricted = true,
							Text = "Restriction Reason",
							Units = "",
							UnitMultiple = 1
						}
					}
				},
			};

			AssertCalculatorConversion(line, SplitMonthBillingCalculator.Code, expectedAttributes);
		}

		public void TestConvert_TimeCalculator()
		{
			var costing = Helper.NewCosting(TransportProvider1);

			var entry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "STD", "");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("FRT", TimeCalculator.Code, QuantityUnit.CN, "AUD");
			line.RateLineItems.RemoveAndDeleteAll();

			var item1 = line.RateLineItems.AddNew();
			item1.TM_Type = Calculator.Items.Operator.Minus;
			item1.TM_Break = 100m;
			item1.TM_Value = 20m;
			item1.TM_FlatAmount = 200m;
			item1.TM_BreakWeightVolume = QuantityUnit.KG;

			var item2 = line.RateLineItems.AddNew();
			item2.TM_Type = Calculator.Items.Operator.Plus;
			item2.TM_Break = 100m;
			item2.TM_CallForPricing = true;
			item2.TM_Text = "Restriction Reason";

			var calculator = line.GetCalculator<TimeCalculator>();
			calculator[Calculator.Items.Operator.MIN] = (ZDecimal)400m;
			calculator[Calculator.Items.Operator.MAX] = (ZDecimal)1500m;
			calculator[Calculator.Items.Operator.BAS] = (ZDecimal)110m;
			calculator[Calculator.Items.Operator.UNT] = (ZDecimal)14.89m;

			calculator.ExcludeHolidays = "WEH";
			calculator.UseInclusiveBreaks = false;
			calculator.UseHigherChargeableLowerRateRule = true;
			calculator.IsAccumulated = true;

			var expectedAttributes = new[]
			{
				new CalculatorAttribute()
				{
					Name = "UseInclusiveBreaks",
					Type = "Boolean",
					Value = false
				},
				new CalculatorAttribute()
				{
					Name = "UseHigherChargeableLowerRateRule",
					Type = "Boolean",
					Value = true
				},
				new CalculatorAttribute()
				{
					Name = "IsAccumulated",
					Type = "Boolean",
					Value = true
				},
				new CalculatorAttribute()
				{
					Name = "MultipleEquipmentsOverMaxWeightVolume",
					Type = "Boolean",
					Value = false
				},
				new CalculatorAttribute()
				{
					Name = "ExcludeHolidays",
					Type = "String",
					Value = "WEH"
				},
				new CalculatorAttribute()
				{
					Name = "BreaksPer",
					Type = "String",
					Value = ""
				},
				new CalculatorAttribute()
				{
					Name = "Breaks",
					Type = "CalculatorBreakItem[]",
					Value = new []
					{
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "MIN",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 400,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "MAX",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 1500,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "BAS",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 110,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "UNT",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 14.89m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "-",
							Break = 100,
							BreakMinimum = null,
							UnitPrice = 20,
							FlatAmount = 200,
							Restricted = false,
							Text = "",
							Units = "KG",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "+",
							Break = 100,
							BreakMinimum = null,
							UnitPrice = null,
							FlatAmount = null,
							Restricted = true,
							Text = "Restriction Reason",
							Units = "",
							UnitMultiple = 1
						}
					}
				},
			};

			AssertCalculatorConversion(line, TimeCalculator.Code, expectedAttributes);
		}

		public void TestConvert_DisbursementInterestCalculator()
		{
			var bafChargeCode = Helper.ChargeCodes["BAF"];
			var client = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", removeLines: true);
			var line = entry.AddRateLine("FRT", DisbursementInterestCalculator.Code, "", CurrencyCodes.Australia);
			var calculator = line.GetCalculator<DisbursementInterestCalculator>();

			calculator.Uplift = 1;
			calculator.AdjustmentDays = 45;
			calculator.OutstandingDays = true;
			calculator.IncludeGST = true;
			calculator.AddApplyToItem("DSB");
			calculator.AddApplyToItem("VAL");
			var codLineItem = calculator.AddApplyToItem("COD");
			codLineItem.TM_AC = bafChargeCode.PK;

			var seqLineItem = calculator.AddApplyToItem("SEQ");
			seqLineItem.TM_Value = 7;

			var expectedAttributes = new[]
			{
				new CalculatorAttribute()
				{
					Name = "CreditTerms",
					Type = "String",
					Value = "45 Outstanding Days"
				},
				new CalculatorAttribute()
				{
					Name = "Uplift",
					Type = "Decimal",
					Value = 1m
				},
				new CalculatorAttribute()
				{
					Name = "CurrentPrimeRate",
					Type = "Decimal",
					Value = 0m
				},
				new CalculatorAttribute()
				{
					Name = "AdjustmentDays",
					Type = "Decimal",
					Value = 45m
				},
				new CalculatorAttribute()
				{
					Name = "OutstandingDays",
					Type = "Boolean",
					Value = true
				},
				new CalculatorAttribute()
				{
					Name = "IncludeGST",
					Type = "Boolean",
					Value = true
				},
				new CalculatorAttribute()
				{
					Name = "EffectiveRate",
					Type = "Decimal",
					Value = 1m
				},

				new CalculatorAttribute()
				{
					Name = "ApplyToCharges",
					Type = "CalculatorApplyToCharge[]",
					Value = new []
					{
						new CalculatorApplyToCharge()
						{
							Type = "DSB",
							ApplyCharge = null,
						},
						new CalculatorApplyToCharge()
						{
							Type = "VAL",
							ApplyCharge = null,
						},
						new CalculatorApplyToCharge()
						{
							Type = "COD",
							ApplyCharge = "BAF",
						},
						new CalculatorApplyToCharge()
						{
							Type = "SEQ",
							CalculationPriority = 7,
						},
					}
				},
			};

			AssertCalculatorConversion(line, DisbursementInterestCalculator.Code, expectedAttributes, rateType: RatingConstants.RatingHeaderTypes.ClientRate);
		}

		public void TestConvert_EqualizationCalculator()
		{
			var costing = Helper.NewCosting(TransportProvider1);

			var entry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "STD", "", removeLines: true);
			var line = entry.AddRateLine("FRT", EqualizationCalculator.Code, QuantityUnit.KG, "AUD");
			line.RateLineItems.RemoveAndDeleteAll();

			var item1 = line.RateLineItems.AddNew();
			item1.TM_Type = Calculator.Items.Operator.Plus;
			item1.TM_Break = 100m;
			item1.TM_Value = 20m;
			item1.TM_FlatAmount = 200m;

			var item2 = line.RateLineItems.AddNew();
			item2.TM_Type = Calculator.Items.Operator.Minus;
			item2.TM_Break = 100m;
			item2.TM_Value = 30m;

			var item3 = line.RateLineItems.AddNew();
			item3.TM_Type = Calculator.Items.Operator.Plus;
			item3.TM_Break = 200m;
			item3.TM_CallForPricing = true;
			item3.TM_Text = "Restriction Reason";

			var calculator = line.GetCalculator<EqualizationCalculator>();

			calculator.UseInclusiveBreaks = true;

			var expectedAttributes = new[]
			{
				new CalculatorAttribute()
				{
					Name = "UseInclusiveBreaks",
					Type = "Boolean",
					Value = true
				},
				new CalculatorAttribute()
				{
					Name = "ContractHint",
					Type = "String",
					Value = "Volume Discount is achieved if Average KG across selected jobs reaches the Pivot Break, then the rate is applied to the actual job KG. Otherwise the rate is applied to the Pivot Break KG per job."
				},
				new CalculatorAttribute()
				{
					Name = "Breaks",
					Type = "CalculatorBreakItem[]",
					Value = new []
					{
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "+",
							Break = 100,
							BreakMinimum = null,
							UnitPrice = 20m,
							FlatAmount = 200m,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "-",
							Break = 100,
							BreakMinimum = null,
							UnitPrice = 30m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "+",
							Break = 200,
							BreakMinimum = null,
							UnitPrice = null,
							FlatAmount = null,
							Restricted = true,
							Text = "Restriction Reason",
							Units = "",
							UnitMultiple = 1
						},
					}
				},
			};

			AssertCalculatorConversion(line, EqualizationCalculator.Code, expectedAttributes);
		}

		public void TestConvert_WarehousePackCalculator()
		{
			var chargeCode = Helper.ChargeCodes.New("WHS123", "Warehouse In", WarehousePackCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);
			var entry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.WHS);
			entry.TI_RateStartDate = ZDate.Today.AddDays(-5);

			var line = entry.AddRateLine(chargeCode, WarehousePackCalculator.Code, PkgUnit.Unit);
			line.Calculator.AddRateLineItem(PkgUnit.Carton, 0m, 1.25m);
			line.Calculator.AddRateLineItem(PkgUnit.Pallet, 0m, 12m);
			line.Calculator.AddRateLineItem(PkgUnit.Cylinder, 0m, 32.8m);

			var expectedAttributes = new[]
			{
				new CalculatorAttribute()
				{
					Name = "Breaks",
					Type = "CalculatorBreakItem[]",
					Value = new []
					{
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "CTN",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 1.25m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "PLT",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 12m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "CYL",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 32.8m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
					}
				},
			};

			AssertCalculatorConversion(line, WarehousePackCalculator.Code, expectedAttributes);
		}

		public void TestConvert_WarehouseLocationTypeCalculator()
		{
			var chargeCode = Helper.ChargeCodes.New("WHS123", "Warehouse In", WarehouseLocationTypeCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);
			var entry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.WHS);
			entry.TI_RateStartDate = ZDate.Today.AddDays(-5);

			var line = entry.AddRateLine(chargeCode, WarehouseLocationTypeCalculator.Code, PkgUnit.Unit);
			line.Calculator.AddRateLineItem("DXP", 0m, 1.25m);
			line.Calculator.AddRateLineItem("RNO", 0m, 12m);
			line.Calculator.AddRateLineItem("DPF", 0m, 32.8m);

			var expectedAttributes = new[]
			{
				new CalculatorAttribute()
				{
					Name = "Breaks",
					Type = "CalculatorBreakItem[]",
					Value = new []
					{
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "DXP",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 1.25m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "RNO",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 12m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
						new CalculatorBreakItem()
						{
							CWTransportZone = null,
							Operator = "DPF",
							Break = null,
							BreakMinimum = null,
							UnitPrice = 32.8m,
							FlatAmount = null,
							Restricted = false,
							Text = "",
							Units = "",
							UnitMultiple = 1
						},
					}
				},
			};

			AssertCalculatorConversion(line, WarehouseLocationTypeCalculator.Code, expectedAttributes);
		}

		public void TestConvert_ExcludeCompanyTariffsCalculator()
		{
			var clinet = Helper.NewOrgHeader();

			var rate = Helper.NewClientRate(clinet);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.ULD, "AUSYD", "USLAX", ZString.Empty, "40GP", removeLines: true);
			var line = entry.AddRateLine("FRT", ExcludeCompanyTariffsCalculator.Code);

			var expectedAttributes = System.Array.Empty<CalculatorAttribute>();
			AssertCalculatorConversion(line, ExcludeCompanyTariffsCalculator.Code, expectedAttributes, rateType: RatingConstants.RatingHeaderTypes.ClientRate);
		}

		void AssertCalculatorConversion(IRateLine line, string calculatorCode, IEnumerable<CalculatorAttribute> expectedAttributes, string rateType = RatingConstants.RatingHeaderTypes.Costing, bool isAgentRate = false, IEnumerable<string> expectedLogs = null)
		{
			var converter = new CalculatorToCalculatorInfoConverter(Factory);
			var logger = new ElementaryLogger();

			var calculatorsInfo = converter.Convert(line, rateType, logger);

			AssertNotNull(calculatorsInfo);

			var calculatorInfo = calculatorsInfo.Where(ci => ci.IsAgentRate == isAgentRate).SingleOrDefault();

			AssertNotNull(calculatorInfo);

			AssertEquals(calculatorCode, calculatorInfo.CWCode);
			AssertCalculatorAttributes(expectedAttributes, calculatorInfo.Attributes);

			if (expectedLogs != null)
			{
				AssertContainsExactElementsInAnyOrder(expectedLogs, logger.GetAllLogs());
			}
		}

		void AssertCalculatorsConversion(IRateLine line, IEnumerable<CalculatorInfo> expectedCalculatorInfos, string rateType = RatingConstants.RatingHeaderTypes.Costing)
		{
			var converter = new CalculatorToCalculatorInfoConverter(Factory);
			var logger = new ElementaryLogger();

			var convertedCalculatorsInfo = converter.Convert(line, rateType, logger);

			AssertNotNull(convertedCalculatorsInfo);

			AssertEquals(expectedCalculatorInfos.Count(), convertedCalculatorsInfo.Length);
			for(int i = 0; i < expectedCalculatorInfos.Count(); i++)
			{
				AssertEquals(expectedCalculatorInfos.ElementAt(i).CWCode, convertedCalculatorsInfo.ElementAt(i).CWCode);
				AssertEquals(expectedCalculatorInfos.ElementAt(i).IsAgentRate, convertedCalculatorsInfo.ElementAt(i).IsAgentRate);
				AssertCalculatorAttributes(expectedCalculatorInfos.ElementAt(i).Attributes, convertedCalculatorsInfo.ElementAt(i).Attributes);
			}
		}

		void AssertCalculatorAttributes(IEnumerable<CalculatorAttribute> expectedAttributes, IEnumerable<CalculatorAttribute> actualAttributes)
		{
			AssertEquals(expectedAttributes.Count(), actualAttributes.Count());
			expectedAttributes = expectedAttributes.OrderBy(a => a.Name);
			actualAttributes = actualAttributes.OrderBy(a => a.Name);
			for (int i = 0; i < expectedAttributes.Count(); i++)
			{
				AssertCalculatorAttribute(expectedAttributes.ElementAt(i), actualAttributes.ElementAt(i));
			}
		}

		void AssertCalculatorAttribute(CalculatorAttribute expectedAttribute, CalculatorAttribute actualAttribute)
		{
			AssertEquals(expectedAttribute.Name, actualAttribute.Name);
			AssertEquals(expectedAttribute.Type, actualAttribute.Type);

			if (expectedAttribute.Value is CalculatorBreakItem[] e && actualAttribute.Value is CalculatorBreakItem[] a)
			{
				var sortedE = e.OrderBy(x => x.Operator).ToArray();
				var sortedA = a.OrderBy(x => x.Operator).ToArray();
				for (int i = 0; i < e.Length; i++)
				{
					AssertCalculatorValue(sortedE[i], sortedA[i]);
				}
			}
			else if (expectedAttribute.Value is CalculatorApplyToCharge[] ex && actualAttribute.Value is CalculatorApplyToCharge[] ac)
			{
				var sortedEx = ex.OrderBy(x => x.ApplyCharge).ToArray();
				var sortedAc = ac.OrderBy(x => x.ApplyCharge).ToArray();
				for (int i = 0; i < ex.Length; i++)
				{
					AssertCalculatorValue(sortedEx[i], sortedAc[i]);
				}
			}
			else
			{
				AssertEquals(expectedAttribute.Value, actualAttribute.Value);
			}
		}

		void AssertCalculatorValue(object expected, object actual)
		{
			if (expected is CalculatorBreakItem e && actual is CalculatorBreakItem a)
			{
				AssertEquals(e.CWTransportZone, a.CWTransportZone);
				AssertEquals(e.Operator, a.Operator);
				AssertEquals(e.Break, a.Break);
				AssertEquals(e.BreakMinimum, a.BreakMinimum);
				AssertEquals(e.UnitPrice, a.UnitPrice);
				AssertEquals(e.FlatAmount, a.FlatAmount);
				AssertEquals(e.Restricted, a.Restricted);
				AssertEquals(e.Text, a.Text);
				AssertEquals(e.Units, a.Units);
				AssertEquals(e.UnitMultiple, a.UnitMultiple);
			}
			else if (expected is CalculatorApplyToCharge ex && actual is CalculatorApplyToCharge ac)
			{
				AssertEquals(ex.Type, ac.Type);
				AssertEquals(ex.ApplyCharge, ac.ApplyCharge);
				AssertEquals(ex.CalculationPriority, ac.CalculationPriority);
			}
			else
			{
				AssertEquals(expected, actual);
			}
		}
	}
}
