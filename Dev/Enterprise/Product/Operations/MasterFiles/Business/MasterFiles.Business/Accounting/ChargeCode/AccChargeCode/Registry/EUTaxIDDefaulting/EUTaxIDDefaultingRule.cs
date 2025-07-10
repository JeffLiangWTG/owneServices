using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class EUTaxIDDefaultingRule : RegistryBusinessObjectTemplate
	{
		#region Constants

		abstract class Schema
		{
			public const string Origin = "Origin";
			public const string Destination = "Destination";
			public const string CostTaxRateForOrganisationRegisteredInMyCountry = "CostTaxRateForOrganisationRegisteredInMyCountry";
			public const string CostTaxRateForOrganisationRegisteredInOtherEUCountry = "CostTaxRateForOrganisationRegisteredInOtherEUCountry";
			public const string CostTaxRateForNotRegisteredOrganisation = "CostTaxRateForNotRegisteredOrganisation";
			public const string SellTaxRateForOrganisationRegisteredInMyCountry = "SellTaxRateForOrganisationRegisteredInMyCountry";
			public const string SellTaxRateForOrganisationRegisteredInOtherEUCountry = "SellTaxRateForOrganisationRegisteredInOtherEUCountry";
			public const string SellTaxRateForNotRegisteredOrganisation = "SellTaxRateForNotRegisteredOrganisation";
		}

		public abstract class OriginDestinationCode
		{
			public const string MyEUCountry = "MYEU";
			public const string OtherEUCountry = "OTEU";
			public const string NotEUCountry = "NOEU";
			public const string SameAsOrigin = "SAME";
		}

		public abstract class JobDirectionCode
		{
			public const string Domestic = "DOM";
			public const string DomesticOtherEUCountries = "DOE";
			public const string BetweenOtherEUCountries = "BOE";
			public const string ImportFromOutsideEU = "IOE";
			public const string ImportFromWithinEU = "IWE";
			public const string ExportToOutsideEU = "EOE";
			public const string ExportToWithinEU = "EWE";
			public const string OtherEUExportToOutsideEU = "EOO";
			public const string OtherEUImportFromOutsideEU = "IOO";
			public const string NotEU = "NEU";
		}

		#endregion

		#region Construction

		public EUTaxIDDefaultingRule()
		{
		}

		public EUTaxIDDefaultingRule(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EUTaxIDDefaultingRule(fallbackLevel, factory) { Origin = Origin, Destination = Destination };
		}

		#region Properties

		public ZString JobDirection
		{
			get { return GetJobDirectionCode(Origin, Destination); }
		}

		public static string GetJobDirectionCode(string originCode, string destinationCode)
		{
			string result = string.Empty;

			if (originCode == OriginDestinationCode.MyEUCountry && destinationCode == OriginDestinationCode.SameAsOrigin)
			{
				result = JobDirectionCode.Domestic;
			}
			else if (originCode == OriginDestinationCode.OtherEUCountry && destinationCode == OriginDestinationCode.SameAsOrigin)
			{
				result = JobDirectionCode.DomesticOtherEUCountries;
			}
			else if (originCode == OriginDestinationCode.OtherEUCountry && destinationCode == OriginDestinationCode.OtherEUCountry)
			{
				result = JobDirectionCode.BetweenOtherEUCountries;
			}
			else if (originCode == OriginDestinationCode.NotEUCountry && destinationCode == OriginDestinationCode.MyEUCountry)
			{
				result = JobDirectionCode.ImportFromOutsideEU;
			}
			else if (originCode == OriginDestinationCode.OtherEUCountry && destinationCode == OriginDestinationCode.MyEUCountry)
			{
				result = JobDirectionCode.ImportFromWithinEU;
			}
			else if (originCode == OriginDestinationCode.MyEUCountry && destinationCode == OriginDestinationCode.NotEUCountry)
			{
				result = JobDirectionCode.ExportToOutsideEU;
			}
			else if (originCode == OriginDestinationCode.MyEUCountry && destinationCode == OriginDestinationCode.OtherEUCountry)
			{
				result = JobDirectionCode.ExportToWithinEU;
			}
			else if (originCode == OriginDestinationCode.OtherEUCountry && destinationCode == OriginDestinationCode.NotEUCountry)
			{
				result = JobDirectionCode.OtherEUExportToOutsideEU;
			}
			else if (originCode == OriginDestinationCode.NotEUCountry && destinationCode == OriginDestinationCode.OtherEUCountry)
			{
				result = JobDirectionCode.OtherEUImportFromOutsideEU;
			}
			else if (originCode == OriginDestinationCode.NotEUCountry && destinationCode == OriginDestinationCode.NotEUCountry)
			{
				result = JobDirectionCode.NotEU;
			}

			return result;
		}

		public ZString JobDirectionDescription
		{
			get { return JobDirectionList.GetDescriptionFromCode(JobDirection); }
		}

		[ReadOnly(true)]
		public ZString Origin
		{
			get; set;
		}

		public ZString OriginDescription
		{
			get { return OriginDestinationList.GetDescriptionFromCode(Origin); }
		}

		[ReadOnly(true)]
		public ZString Destination
		{
			get; set;
		}

		public ZString DestinationDescription
		{
			get { return Destination == OriginDestinationCode.OtherEUCountry && Origin == OriginDestinationCode.OtherEUCountry ? Res.GetString("c569edf6-df63-40a6-9d52-81eb0d0dc4ca", "Another EU Country/Region") : OriginDestinationList.GetDescriptionFromCode(Destination); }
		}

		ZGuid fCostTaxRateForOrganisationRegisteredInMyCountry;
		[List("TaxRates")]
		public ZGuid CostTaxRateForOrganisationRegisteredInMyCountry
		{
			get { return fCostTaxRateForOrganisationRegisteredInMyCountry; }
			set
			{
				SetNonPersistentPropertyValue(CostTaxRateForOrganisationRegisteredInMyCountryInfo, ref fCostTaxRateForOrganisationRegisteredInMyCountry, value);

				if (!IsValidationSuspended)
				{
					ValidateTaxRate(CostTaxRateForOrganisationRegisteredInMyCountryInfo);
				}
			}
		}

		public ZPropertyInfo CostTaxRateForOrganisationRegisteredInMyCountryInfo
		{
			get { return GetZPropertyInfo(Schema.CostTaxRateForOrganisationRegisteredInMyCountry); }
		}

		ZGuid fCostTaxRateForOrganisationRegisteredInOtherEUCountry;
		[List("TaxRates")]
		public ZGuid CostTaxRateForOrganisationRegisteredInOtherEUCountry
		{
			get { return fCostTaxRateForOrganisationRegisteredInOtherEUCountry; }
			set
			{
				SetNonPersistentPropertyValue(CostTaxRateForOrganisationRegisteredInOtherEUCountryInfo, ref fCostTaxRateForOrganisationRegisteredInOtherEUCountry, value);

				if (!IsValidationSuspended)
				{
					ValidateTaxRate(CostTaxRateForOrganisationRegisteredInOtherEUCountryInfo);
				}
			}
		}

		public ZPropertyInfo CostTaxRateForOrganisationRegisteredInOtherEUCountryInfo
		{
			get { return GetZPropertyInfo(Schema.CostTaxRateForOrganisationRegisteredInOtherEUCountry); }
		}

		ZGuid fCostTaxRateForNotRegisteredOrganisation;
		[List("TaxRates")]
		public ZGuid CostTaxRateForNotRegisteredOrganisation
		{
			get { return fCostTaxRateForNotRegisteredOrganisation; }
			set
			{
				SetNonPersistentPropertyValue(CostTaxRateForNotRegisteredOrganisationInfo, ref fCostTaxRateForNotRegisteredOrganisation, value);

				if (!IsValidationSuspended)
				{
					ValidateTaxRate(CostTaxRateForNotRegisteredOrganisationInfo);
				}
			}
		}

		public ZPropertyInfo CostTaxRateForNotRegisteredOrganisationInfo
		{
			get { return GetZPropertyInfo(Schema.CostTaxRateForNotRegisteredOrganisation); }
		}

		ZGuid fSellTaxRateForOrganisationRegisteredInMyCountry;
		[List("TaxRates")]
		public ZGuid SellTaxRateForOrganisationRegisteredInMyCountry
		{
			get { return fSellTaxRateForOrganisationRegisteredInMyCountry; }
			set
			{
				SetNonPersistentPropertyValue(SellTaxRateForOrganisationRegisteredInMyCountryInfo, ref fSellTaxRateForOrganisationRegisteredInMyCountry, value);

				if (!IsValidationSuspended)
				{
					ValidateTaxRate(SellTaxRateForOrganisationRegisteredInMyCountryInfo);
				}
			}
		}

		public ZPropertyInfo SellTaxRateForOrganisationRegisteredInMyCountryInfo
		{
			get { return GetZPropertyInfo(Schema.SellTaxRateForOrganisationRegisteredInMyCountry); }
		}

		ZGuid fSellTaxRateForOrganisationRegisteredInOtherEUCountry;
		[List("TaxRates")]
		public ZGuid SellTaxRateForOrganisationRegisteredInOtherEUCountry
		{
			get { return fSellTaxRateForOrganisationRegisteredInOtherEUCountry; }
			set
			{
				SetNonPersistentPropertyValue(SellTaxRateForOrganisationRegisteredInOtherEUCountryInfo, ref fSellTaxRateForOrganisationRegisteredInOtherEUCountry, value);

				if (!IsValidationSuspended)
				{
					ValidateTaxRate(SellTaxRateForOrganisationRegisteredInOtherEUCountryInfo);
				}
			}
		}

		public ZPropertyInfo SellTaxRateForOrganisationRegisteredInOtherEUCountryInfo
		{
			get { return GetZPropertyInfo(Schema.SellTaxRateForOrganisationRegisteredInOtherEUCountry); }
		}

		ZGuid fSellTaxRateForNotRegisteredOrganisation;
		[List("TaxRates")]
		public ZGuid SellTaxRateForNotRegisteredOrganisation
		{
			get { return fSellTaxRateForNotRegisteredOrganisation; }
			set
			{
				SetNonPersistentPropertyValue(SellTaxRateForNotRegisteredOrganisationInfo, ref fSellTaxRateForNotRegisteredOrganisation, value);

				if (!IsValidationSuspended)
				{
					ValidateTaxRate(SellTaxRateForNotRegisteredOrganisationInfo);
				}
			}
		}

		public ZPropertyInfo SellTaxRateForNotRegisteredOrganisationInfo
		{
			get { return GetZPropertyInfo(Schema.SellTaxRateForNotRegisteredOrganisation); }
		}

		#endregion

		#region Lists

		CodeDescriptionPairList fOriginDestinationList;
		protected CodeDescriptionPairList OriginDestinationList
		{
			get
			{
				if (fOriginDestinationList == null)
				{
					fOriginDestinationList = new CodeDescriptionPairList();
					fOriginDestinationList.AddPair(OriginDestinationCode.MyEUCountry, Res.GetString("5c56ff30-db23-46ee-b49f-65d3971a7d4b", "My EU Country/Region"));
					fOriginDestinationList.AddPair(OriginDestinationCode.OtherEUCountry, Res.GetString("3b9876f0-ffee-443a-8ab0-bf6ca5984ee6", "Other EU Country/Region"));
					fOriginDestinationList.AddPair(OriginDestinationCode.NotEUCountry, Res.GetString("1a84187a-79f9-44b8-8bf5-3aea5fdf9cce", "Not EU"));
					fOriginDestinationList.AddPair(OriginDestinationCode.SameAsOrigin, Res.GetString("8b8fac84-a16e-466e-9bec-e0f21dc7a055", "Same as Origin"));
				}

				return fOriginDestinationList;
			}
		}

		CodeDescriptionPairList fJobDirectionList;
		protected CodeDescriptionPairList JobDirectionList
		{
			get
			{
				if (fJobDirectionList == null)
				{
					fJobDirectionList = new CodeDescriptionPairList();
					fJobDirectionList.AddPair(JobDirectionCode.Domestic, Res.GetString("3f908f72-8eef-4434-9cf2-a6f4c3b1e005", "Domestic"));
					fJobDirectionList.AddPair(JobDirectionCode.DomesticOtherEUCountries, Res.GetString("124a28f3-a203-451c-ba3e-8ce732ab5c99", "Domestic  other EU countries/regions"));
					fJobDirectionList.AddPair(JobDirectionCode.BetweenOtherEUCountries, Res.GetString("4abbface-7e59-408d-a5aa-523ee53b2f5d", "Between other EU countries/regions"));
					fJobDirectionList.AddPair(JobDirectionCode.ImportFromOutsideEU, Res.GetString("d8190c7f-72c7-4ced-90f2-39658ba26e24", "Import from outside EU"));
					fJobDirectionList.AddPair(JobDirectionCode.ImportFromWithinEU, Res.GetString("664884fa-3568-46a6-b2d0-da2cf06b4745", "Import from within EU"));
					fJobDirectionList.AddPair(JobDirectionCode.ExportToOutsideEU, Res.GetString("8c5f4b42-81f4-4179-a80e-31d7227e5ae0", "Export to outside EU"));
					fJobDirectionList.AddPair(JobDirectionCode.ExportToWithinEU, Res.GetString("d1a988e7-2859-447b-9810-f6b24501283a", "Export to within EU"));
					fJobDirectionList.AddPair(JobDirectionCode.OtherEUExportToOutsideEU, Res.GetString("71283a12-a3b9-4467-872e-d10d2fa1471f", "Other EU exports to outside EU"));
					fJobDirectionList.AddPair(JobDirectionCode.OtherEUImportFromOutsideEU, Res.GetString("bddc9009-506e-43ac-b711-ccfa5ff83c89", "Other EU imports from outside EU"));
					fJobDirectionList.AddPair(JobDirectionCode.NotEU, Res.GetString("1a84187a-79f9-44b8-8bf5-3aea5fdf9cce", "Not EU"));
				}

				return fJobDirectionList;
			}
		}

		public AccTaxRateCollection TaxRates
		{
			get
			{
				if (CurrentFallbackLevel != null)
				{
					GlbCompany company = CurrentFactory.Load<GlbCompany>(CurrentFallbackLevel.CompanyPK(false));
					return new AccTaxRateCollection(CurrentFactory, company.GC_RN_NKCountryCode);
				}
				else
				{
					return new AccTaxRateCollection(CurrentFactory);
				}
			}
		}

		#endregion

		#region Validation

		void ValidateTaxRate(ZPropertyInfo propertyInfo)
		{
			propertyInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(propertyInfo);
		}

		#endregion

		#region Xml Serialisation

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Origin = reader.ReadElementString(Schema.Origin);
			Destination = reader.ReadElementString(Schema.Destination);
			CostTaxRateForOrganisationRegisteredInMyCountry = new ZGuid(reader.ReadElementString(Schema.CostTaxRateForOrganisationRegisteredInMyCountry));
			CostTaxRateForOrganisationRegisteredInOtherEUCountry = new ZGuid(reader.ReadElementString(Schema.CostTaxRateForOrganisationRegisteredInOtherEUCountry));
			CostTaxRateForNotRegisteredOrganisation = new ZGuid(reader.ReadElementString(Schema.CostTaxRateForNotRegisteredOrganisation));
			SellTaxRateForOrganisationRegisteredInMyCountry = new ZGuid(reader.ReadElementString(Schema.SellTaxRateForOrganisationRegisteredInMyCountry));
			SellTaxRateForOrganisationRegisteredInOtherEUCountry = new ZGuid(reader.ReadElementString(Schema.SellTaxRateForOrganisationRegisteredInOtherEUCountry));
			SellTaxRateForNotRegisteredOrganisation = new ZGuid(reader.ReadElementString(Schema.SellTaxRateForNotRegisteredOrganisation));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Origin, Origin);
			writer.WriteElementString(Schema.Destination, Destination);
			writer.WriteElementString(Schema.CostTaxRateForOrganisationRegisteredInMyCountry, CostTaxRateForOrganisationRegisteredInMyCountry.ToString());
			writer.WriteElementString(Schema.CostTaxRateForOrganisationRegisteredInOtherEUCountry, CostTaxRateForOrganisationRegisteredInOtherEUCountry.ToString());
			writer.WriteElementString(Schema.CostTaxRateForNotRegisteredOrganisation, CostTaxRateForNotRegisteredOrganisation.ToString());
			writer.WriteElementString(Schema.SellTaxRateForOrganisationRegisteredInMyCountry, SellTaxRateForOrganisationRegisteredInMyCountry.ToString());
			writer.WriteElementString(Schema.SellTaxRateForOrganisationRegisteredInOtherEUCountry, SellTaxRateForOrganisationRegisteredInOtherEUCountry.ToString());
			writer.WriteElementString(Schema.SellTaxRateForNotRegisteredOrganisation, SellTaxRateForNotRegisteredOrganisation.ToString());
		}

		#endregion
	}
}
