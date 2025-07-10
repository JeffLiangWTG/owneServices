using System;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.Business
{
	public sealed class DataRegistryRating : RegistryItemSet
	{
		public static DataRegistryRating Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new DataRegistryRating();
				}

				return instance;
			}
		}

		#region Categories

		[CodeAlive("Code is required to keep integrity of Registry entries")]
		public abstract class Categories : RatingDataRegistry.Categories
		{
		}

		#endregion

		public override bool IsForProductivityWise => false;

		#region AutoRating / Quotations

		public BooleanRegistryItem QuoteRequireInternalApproval
		{
			get
			{
				return GetItem("QuoteRequireInternalApproval", delegate
				{
					return new BooleanRegistryItem(
						"QuoteRequireInternalApproval",
						Categories.AutoRating_Quotations,
						ResString.GetMultilingualString("a0137194-df76-4c2b-b9a2-5ce0d81b1703", "Require Internal Approval"),
						ResString.GetMultilingualString("550584a9-1d1b-4da8-b49b-c236f57f67e7", "Specifies whether Quotations need to have internal approval."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region RateCommodity

		public RateCommodityDefaultingRuleRegistryItem DefaultRateCommodities
		{
			get
			{
				return GetItem(nameof(DefaultRateCommodities), delegate
				{
					return new RateCommodityDefaultingRuleRegistryItem(
						nameof(DefaultRateCommodities),
						RatingDataRegistry.Categories.AutoRating_RateCommodity,
						ResString.GetMultilingualString("a78cccc1-33bb-4032-9673-2fffe1d07ba4", "Default Rate Commodities"),
						ResString.GetMultilingualString("05cd279e-95fb-41fa-8c7b-e029cda72d9f", "Nominate the default Rate Commodity in the section Forwarding Shipment > Additional Detail > Freight Rates section."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System);
				});
			}
		}

		#endregion

		#region RatesService

		#region Fallback for subject-to rates

		public FallbackSubjectToChargesRegistryItem RateServiceFallbackSubjectToCharges
		{
			get
			{
				return GetItem(nameof(RateServiceFallbackSubjectToCharges), delegate
				{
					return new FallbackSubjectToChargesRegistryItem(
						nameof(RateServiceFallbackSubjectToCharges),
						RatingDataRegistry.Categories.AutoRating_RatesService,
						ResString.GetMultilingualString("AF0941F3-9D11-44EB-989F-CF148F8A2784", "Fallback for Subject To Charges"),
						ResString.GetMultilingualString("73152429-B21B-48CB-B9BD-70BBDD35C394", "Subject To charges that are Autorated from Rates Providers integrated with Rates Service and without pricing information can be fallback to matching rates setup under Costing."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System);
				});
			}
		}

		#endregion

		#region IsOnlyForSupport

		public IntRegistryItem RatesServiceRateSearchRequestTimeout
		{
			get
			{
				const string key = "WiseRatesRateSearchRequestTimeout";
				return GetItem(key, () => new IntRegistryItem(
					key,
					RatingDataRegistry.Categories.AutoRating_RatesService,
					(NoResString)"Rates Service Search Request Timeout",                                                                                                        // Support only registry
					(NoResString)"Support Only Registry. A Timeout in seconds after which Rates Service Search request will be discarded to continue with autorating.",         // Support only registry
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					100));
			}
		}

		public BooleanRegistryItem DiagnosticSettingsIncludeRawData
		{
			get
			{
				const string key = nameof(DiagnosticSettingsIncludeRawData);
				return GetItem(key, () => new BooleanRegistryItem(
					key,
					RatingDataRegistry.Categories.AutoRating_RatesService_DiagnosticSettings,
					(NoResString)"Include Raw Data",                                                                        // Support only registry
					(NoResString)"Enable this to include raw data with every Rates Service request for diagnostic purposes.",   // Support only registry
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false));
			}
		}

		public BooleanRegistryItem DiagnosticSettingsEnableOnODPL
		{
			get
			{
				const string key = nameof(DiagnosticSettingsEnableOnODPL);
				return GetItem(key, () => new BooleanRegistryItem(
										key,
										RatingDataRegistry.Categories.AutoRating_RatesService_DiagnosticSettings,
										(NoResString)"Enable Rates Service for non-STL - Testing or Trial Only",                                // Support only registry
										(NoResString)"Set this registry to \"Yes\" for trial or testing of Rates Service on non-STL CW1. This registry should NEVER be set to \"Yes\" for non-STL production CW1.",  // Support only registry
										RegistryStorageFlags.System,
										RegistryOptions.IsOnlyForSupport,
										false));
			}
		}

		public StringRegistryItem CGSPApiURLOverride
		{
			get
			{
				const string key = nameof(CGSPApiURLOverride);
				return GetItem(key, () => new StringRegistryItem(
					key,
					RatingDataRegistry.Categories.AutoRating_RatesService_Providers_CargoSphere,
					(NoResString)"API URL Override",                                                  // Support only registry
					(NoResString)@"An override URL to the CargoSphere Rate Search API.",        // Support only registry
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					"")
				{
					DataType = new UriRegistryDataType(Uri.UriSchemeHttps) { AllowAutoProtocolPrefixing = false },
				}
				);
			}
		}

		public CargoGuideApiSettingsRegistryItem CGGDApiSettings
		{
			get
			{
				const string key = nameof(CGGDApiSettings);
				return GetItem(key, () => new CargoGuideApiSettingsRegistryItem(
					key,
					RatingDataRegistry.Categories.AutoRating_RatesService_Providers_Cargoguide,
					(NoResString)"API Settings",                                                              // Support only registry
					(NoResString)@"An override URL and the version for the Cargoguide API.

Important Note:
Overriding this Registry Setting could effect user access to Cargoguide whilst subscription charges still apply.",    // Support only registry
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport));
			}
		}

		#endregion

		public RatesServiceSettingsRegistryItem RatesServiceSubscription
		{
			get
			{
				return GetItem(nameof(RatesServiceSubscription), delegate
				{
					return new RatesServiceSettingsRegistryItem(
						nameof(RatesServiceSubscription),
						RatingDataRegistry.Categories.AutoRating_RatesService,
						ResString.GetMultilingualString("bd148390-1076-4bf6-be83-468defa6b8cc", "Rates Service Subscription"),
						ResString.GetMultilingualString("6270fcf7-2298-46cf-a347-2b4bbbc0ebf3", @"Overview

Rates Service is a carrier rate storage server hosted by WiseCloud.  

The purpose of Rates Service is to aggregate Tariffs and Rates from carrier rate providers and distribute the aggregated Tariffs and Rates as Costings to the carrier rate providers’ clients in a standardized format across all modes of transport and all types of supply chain providers (Organizations).

Rate data input services include the aggregation of Tariffs and Rates provided by:
*  Third party rate providers integration services

This registry allows you to configure which Forwarding Job type(s) subscribed with Rates Service to search matching buy rates from multiple sources during Autorating of Costs.

Note: This service is free of charge and will be disabled when subscription fees are introduced"),  //Suppress reason = Rates Service is a good work
						RegistryStorageFlags.Company | RegistryStorageFlags.System
						);
				});
			}
		}

		public CargoSphereCredentialsRegistryItem CargoSphereCredentials
		{
			get
			{
				return GetItem("CargoSphereCredentials", delegate
				{
					return new CargoSphereCredentialsRegistryItem(
						 "CargoSphereCredentials",
						 RatingDataRegistry.Categories.AutoRating_RatesService_Providers_CargoSphere,
						 (NoResString)"Single Sign On Credentials Override",    // Support only registry
						 (NoResString)@"Overriding this Registry will disable the logged in user’s Single Sign On credentials and the integration will only authenticate to the user’s account matching the overridden credentials.

Please note that overriding this registry does not disable Third Party Rate Management Services subscriptions.",    // Support only registry
						 RegistryStorageFlags.System,
						 RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		public StringRegistryItem CargoSphereSUDSUrl
		{
			get
			{
				return GetItem("CargoSphereSUDSUrl", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"CargoSphereSUDSUrl",
						RatingDataRegistry.Categories.AutoRating_RatesService_Providers_CargoSphere,
						(NoResString)"SUDS Url",                                                                                                        // Support only registry
						(NoResString)@"The URL of CargoSphere SUDS page.

Important Note:
Overriding this Registry Setting could effect user access to CargoSphere whilst subscription charges still apply.",                                                                 // Support only registry
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"");
					return result;
				});
			}
		}

		public StringRegistryItem CargoSphereRateSearchUrl
		{
			get
			{
				return GetItem("CargoSphereRateSearchUrl", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"CargoSphereRateSearchUrl",
						RatingDataRegistry.Categories.AutoRating_RatesService_Providers_CargoSphere,
						(NoResString)"Rate Search Url",                                 // Support only registry
						(NoResString)@"The URL of CargoSphere rate search service.

Important Note:
Overriding this Registry Setting could effect user access to CargoSphere whilst subscription charges still apply.",                 // Support only registry
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"");
					return result;
				});
			}
		}

		public BooleanRegistryItem CargoSphereIntegrationEnabled
		{
			get
			{
				const string key = nameof(CargoSphereIntegrationEnabled);
				return GetItem(key, () => new BooleanRegistryItem(
					key,
					RatingDataRegistry.Categories.AutoRating_RatesService_Providers_CargoSphere,
					ResString.GetMultilingualString("f120ec0f-0621-4f4c-a7fb-fde0aca53da3", "Enable Ocean Rate Management Integration"),
					ResString.GetMultilingualString("6282fae0-a863-4682-b0be-eb7e1a16b338", @"CargoSphere provides Ocean Rates and Rate Management Services to Rates Service. Overriding this Registry enables the integration and allows all CW1 users to subscribe to CargoSphere’s rate management services.

CargoSphere subscriptions may be administered on a per user basis in Maintain > User Admin > Staff and Resources > Staff Details and Security Rights

Important Note:
Changing this Registry Setting from 'Yes' to 'No' disables the entire CargoSphere integration.  
This means that no user will be able to access CargoSphere Rate Management Services regardless of individual user subscription status."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					false));
			}
		}

		public RatesServiceSettingsRegistryItem RatesServiceRateSelector
		{
			get
			{
				return GetItem(nameof(RatesServiceRateSelector), delegate
				{
					return new RatesServiceSettingsRegistryItem(
						nameof(RatesServiceRateSelector),
						RatingDataRegistry.Categories.AutoRating_RateSelector,
						ResString.GetMultilingualString("0d509d80-2524-4efa-86d7-97a7c7ce3fc0", "Enable Rate Selector (Buy Rates / Costs)"),
						ResString.GetMultilingualString("51a2d0f2-e1f8-46f9-99e0-5b7951fb7017", @"This registry allows you to configure which Forwarding Job type(s) enabled with Rate Selector popup for preview and selection of buy rates from multiple rate sources during Autorating of Costs."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System
						);
				});
			}
		}

		public CargoguideCredentialsRegistryItem CargoguideCredentials
		{
			get
			{
				return GetItem("CargoguideCredentials", delegate
					{
						return new CargoguideCredentialsRegistryItem(
							"CargoguideCredentials",
							RatingDataRegistry.Categories.AutoRating_RatesService_Providers_Cargoguide,
							(NoResString)"Single Sign On Credentials Override", // Support only registry
							(NoResString)@"Overriding this Registry will disable the logged in user’s Single Sign On credentials and the integration will only authenticate to the user’s account matching the overridden credentials.

Please note that overriding this registry does not disable Third Party Rate Management Services subscriptions.",  // Support only registry
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport
							);
					});
			}
		}

		public StringRegistryItem CargoguideRateSearchUrl
		{
			get
			{
				return GetItem("CargoguideRateSearchUrl", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"CargoguideRateSearchUrl",
						RatingDataRegistry.Categories.AutoRating_RatesService_Providers_Cargoguide,
						(NoResString)"Rate Search Url",                                                     // Support only registry
						(NoResString)@"The URL of Cargoguide rate search service.

Important Note:
Overriding this Registry Setting could effect user access to Cargoguide whilst subscription charges still apply.",            // Support only registry
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"");
					return result;
				});
			}
		}

		public BooleanRegistryItem CargoguideIntegrationEnabled
		{
			get
			{
				const string key = nameof(CargoguideIntegrationEnabled);
				return GetItem(key, () => new BooleanRegistryItem(
					key,
					RatingDataRegistry.Categories.AutoRating_RatesService_Providers_Cargoguide,
					ResString.GetMultilingualString("0869345e-88ab-4d1f-8992-4ed2b9ced7d2", "Enable Air Rate Management Integration"),
					ResString.GetMultilingualString("6d6b8bd3-afbe-4629-ac8f-c86ca9724266", @"Cargoguide provides Air Rates and Rate Management Services to Rates Service. Overriding this Registry enables the integration and allow all CW1 users to subscribe to Cargoguide’s rate management services.

Cargoguide subscriptions may be administered on a per user basis in Maintain > User Admin > Staff and Resources > Staff Details and Security Rights

Important Note:
Changing this Registry Setting from 'Yes' to 'No' disables the entire Cargoguide integration.  
This means that no user will be able to access Cargoguide Rate Management Services regardless of individual user subscription status."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					false));
			}
		}

		const string ODM = nameof(ODM);

		public bool IsRateServiceSubscriptionEnabled(out string reason)
		{
			if (!RatesServiceSubscription.IsEnabled(Core.Constants.TransportModes.Air) && !RatesServiceSubscription.IsEnabled(Core.Constants.TransportModes.Sea))
			{
				reason = ResString.GetMultilingualString("f8175d6d-b776-475f-a246-ad8235129e07", @"Rates Service subscription is disabled in the registry: {0}", RatesServiceSubscription.Location());
				return false;
			}
			reason = string.Empty;
			return true;
		}

		public bool IsRateServiceSubscriptionEnabled(string transportMode, out string reason)
		{
			if (!RatesServiceSubscription.IsEnabled(transportMode))
			{
				reason = ResString.GetMultilingualString("d43baa73-ae33-47a2-b909-5e112eb102bf", @"Rates Service subscription is disabled in the registry for {0}: {1}", transportMode, RatesServiceSubscription.Location());
				return false;
			}
			reason = string.Empty;
			return true;
		}

		public bool IsRateServiceSubscriptionEnabled(string transportMode, string containerMode, out string reason)
		{
			if (!RatesServiceSubscription.IsEnabled(transportMode, containerMode))
			{
				reason = ResString.GetMultilingualString("d0e2f42d-b518-41d1-af36-c37fcdfd9c9a", @"Rates Service subscription is disabled in the registry for {0}-{1}: {2}", transportMode, containerMode, RatesServiceSubscription.Location());
				return false;
			}
			reason = string.Empty;
			return true;
		}

		public bool IsLicenseForRatesServiceValid(out string reason)
		{
			var isLicenseValid = RatesServiceEnabledForLicense();

			if (!string.IsNullOrEmpty(isLicenseValid))
			{
				reason = isLicenseValid;
				return false;
			}
			reason = string.Empty;
			return true;
		}

		string RatesServiceEnabledForLicense()
		{
			var enabledOnODPL = DiagnosticSettingsEnableOnODPL.Value;
			var billingModel = ObjectFactory.Get<IProductRegistration>().Key?.BillingModel;

			if (!enabledOnODPL && billingModel == ODM)
			{
				return ResString.GetMultilingualString("5D924320-459E-45D3-8CE6-40ABE4719CE2", "Rates Service is only supported under license of STL. Please submit eRequest for switching license to STL or for a trial of Rates Service.");
			}

			return string.Empty;
		}

		#endregion

		#region AutoRating / Quotations / Spot Quotes

		public BooleanRegistryItem SpotQuoteRequireInternalApproval
		{
			get
			{
				return GetItem("SpotQuoteRequireInternalApproval", delegate
				{
					return new BooleanRegistryItem(
						"SpotQuoteRequireInternalApproval",
						Categories.AutoRating_Quotations_SpotQuotes,
						ResString.GetMultilingualString("901f9c4f-e261-4fc1-91e5-2f0a5b432190", "Require Internal Approval"),
						ResString.GetMultilingualString("132c65ae-80f8-48b8-8dc9-daa2445c7bcf", "Specifies whether Spot quotations need to have internal approval."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public BooleanRegistryItem PreserveQuoteRevenueRatingBehaviour
		{
			get
			{
				return GetItem("PreserveQuoteRevenueRatingBehaviour", delegate
				{
					return new BooleanRegistryItem(
						"PreserveQuoteRevenueRatingBehaviour",
						Categories.AutoRating_Quotations_SpotQuotes,
						ResString.GetMultilingualString("Rating|RatingBehavior|Caption", "Preserve Quote Charges Revenue Rating Behavior"),
						ResString.GetMultilingualString("Rating|RatingBehavior|Description", "When this registry is set to ‘Yes’, Revenue Rating Behavior of Charges is preserved as is when Booking with Quote is converted to Shipment or One Off Quote is consolidated/linked."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public PaymentThreeLevelAuthorisationSettingsRegistryItem SpotQuoteApprovalSettings
		{
			get
			{
				return GetItem("SpotQuoteApprovalSettings", delegate
				{
					return new PaymentThreeLevelAuthorisationSettingsRegistryItem(
						"SpotQuoteApprovalSettings",
						Categories.AutoRating_Quotations_SpotQuotes,
						ResString.GetMultilingualString("8bb2c458-45e4-4517-984d-6bb032a8ad62", "Spot Quote Approval Settings"),
						ResString.GetMultilingualString("6334bff8-55c6-4d8c-92fc-392d50596aa0", @"This registry item allows you to specify the authorization required to approve an unapproved spot quote.

If Require Internal Approval registry is set to Yes (internal approval required) you can set up the authorization required based on the local value of the total quotation revenue amount; when no value is set, any quoted amount will require an approval.

The system will allow you to specify required authorization for different ranges. You must specify at least one ""Up to"" line and only one ""Above"" line."),
						RegistryStorageFlags.Company);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem OneOffQuoteKPISettings
		{
			get
			{
				return GetItem("OneOffQuoteKPISettings", delegate
				{
					var item = new CodeDescriptionPairListRegistryItem(
						"OneOffQuoteKPISettings",
						Categories.AutoRating_Quotations_SpotQuotes,
						ResString.GetMultilingualString("845f5559-17f5-41c5-a088-0ff0b9243eee", "Quote KPI configuration"),
						ResString.GetMultilingualString("c7253652-fd61-4e77-bf4a-6a4dba2bd489", "Provides the ability to specify settings for One Off Quote KPI."),
						3,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new CodeDescriptionPairList());
					((CodeDescriptionPairListRegistryDataType)item.DataType).AllowEmptyCodes = false;
					((CodeDescriptionPairListRegistryDataType)item.DataType).AllowDuplicateCodes = false;
					return item;
				});
			}
		}

		public CodeDescriptionPairListRegistryItem OneOffQuoteSourceSettings
		{
			get
			{
				return GetItem("OneOffQuoteSourceSettings", delegate
				{
					var item = new CodeDescriptionPairListRegistryItem(
						"OneOffQuoteSourceSettings",
						Categories.AutoRating_Quotations_SpotQuotes,
						ResString.GetMultilingualString("c8c9b1be-0e7e-43f5-b2d9-cbb2b1b768d3", "Quote Source configuration"),
						ResString.GetMultilingualString("c0227986-e176-423f-89a0-131b44731d4f", "Provides the ability to specify settings for One Off Quote Source."),
						3,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new CodeDescriptionPairList());
					((CodeDescriptionPairListRegistryDataType)item.DataType).AllowEmptyCodes = false;
					((CodeDescriptionPairListRegistryDataType)item.DataType).AllowDuplicateCodes = false;
					return item;
				});
			}
		}

		public CodeDescriptionPairListRegistryItem OneOffQuoteRevisionReasonSettings
		{
			get
			{
				return GetItem("OneOffQuoteRevisionReasonSettings", delegate
				{
					var item = new CodeDescriptionPairListRegistryItem(
						"OneOffQuoteRevisionReasonSettings",
						Categories.AutoRating_Quotations_SpotQuotes,
						ResString.GetMultilingualString("ee842125-eb9c-49fe-9aba-2a664856a17b", "Quote Revision Reason configuration"),
						ResString.GetMultilingualString("45a682c8-385d-4f3e-b7e2-8516ac6b5b3d", "Provides the ability to specify settings for One Off Quote Revision Reason."),
						3,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new CodeDescriptionPairList());
					((CodeDescriptionPairListRegistryDataType)item.DataType).AllowEmptyCodes = false;
					((CodeDescriptionPairListRegistryDataType)item.DataType).AllowDuplicateCodes = false;
					return item;
				});
			}
		}

		#endregion

		public BooleanRegistryItem AllowOverrideCompanyTariffLevel
		{
			get
			{
				return GetItem("AllowOverrideCompanyTariffLevel", delegate
				{
					return new BooleanRegistryItem(
						"AllowOverrideCompanyTariffLevel",
						Categories.AutoRating_Calculation,
						ResString.GetMultilingualString("39521904-aa18-4da7-81f6-38340ebd2c40", "Allow Company Tariff Level Override in Forwarding Jobs"),
						ResString.GetMultilingualString("34463c7a-1855-4216-bac1-6d5d146568ca", "When registry is set to ‘Yes’, Company Tariff Level can be overridden in Forwarding Shipment, Booking, Booking with Quote and One Off Quote."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#region Implementation

		[ThreadStatic]
		static DataRegistryRating instance;

		#endregion

		#region AutoRating / Calculation

		public BooleanRegistryItem ProposeSimilarRates
		{
			get
			{
				return GetItem("ProposeSimilarRates", delegate
				{
					return new BooleanRegistryItem(
						"ProposeSimilarRates",
						Categories.AutoRating_Calculation,
						ResString.GetMultilingualString("a859db57-d8b4-4f5e-adda-f59447b7e6af", "Propose Similar Rates"),
						ResString.GetMultilingualString("c6efcc6f-b65d-41c7-ac1c-37bad3228f81", "Propose Similar Rates if exact match is not found."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem AllowSavingOfAutoRatingLogNote
		{
			get
			{
				return GetItem("AllowSavingOfAutoRatingLogNote", delegate
				{
					return new BooleanRegistryItem(
						"AllowSavingOfAutoRatingLogNote",
						Categories.AutoRating_Calculation,
						ResString.GetMultilingualString("61a8acca-644d-46d6-82e8-2441bbcf1f34", "Allow Saving of AutoRating Log Note"),
						ResString.GetMultilingualString("4ba38472-d74d-4904-940b-8c559089ef87", "The AutoRating Log Note is automatically generated by the AutoRating process. Unless this registry is enabled, they will be deleted upon saving the job. They are always company specific but can be viewed by users from other companies with appropriate security rights."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.BranchDepartment,
						false);
				});
			}
		}

		public SameChargeCodeDifferentProviderRegistryItem AllowChargesWithSameChargeCodeForDifferentProvider
		{
			get
			{
				return GetItem(nameof(SameChargeCodeDifferentProvider), delegate
				{
					return new SameChargeCodeDifferentProviderRegistryItem(
						nameof(SameChargeCodeDifferentProvider),
						RatingDataRegistry.Categories.AutoRating_Calculation,
						ResString.GetMultilingualString("b36cc3a5-efda-4abb-a4f6-a15cde66491d", "Allow Autorating costs for the same charge code for different Service Provider"),
						ResString.GetMultilingualString("83f25955-2998-46e2-a9a0-d983a1ec667f", @"This setting allows you to autorate costs with the same charge code for different Service Provider.

Autorating will not compare the rateline between different Service Providers."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System);
				});
			}
		}

		#endregion

		#region Sell Rates Decimals

		public SellRatesDecimalsRegistryItem SellRatesDecimals
		{
			get
			{
				return GetItem("SellRatesDecimals", delegate
				{
					return new SellRatesDecimalsRegistryItem(
					"SellRatesDecimals",
					Categories.AutoRating,
					ResString.GetMultilingualString("d0e631c1-6eda-4677-ae5f-dedd4a840526", "Number of decimals allowed for Rates"),
					ResString.GetMultilingualString("33f2cf2b-2452-4d0e-bb62-b63af698d32e", @"Override this registry to select the number of decimals allowed when entering Sell Rates. 
Note:  Any Percentage Changes made to rates which result in a number of decimals greater than specified in the registry will be rounded to the nearest number of decimals allowed."),
					RegistryStorageFlags.Company,
					SellRatesDecimalsCollection.GetDefault());
				});
			}
		}

		#endregion

		#region Rounding

		public DefaultRoundingsRegistryItem DefaultRounding
		{
			get
			{
				return GetItem("DefaultRounding", delegate
				{
					return new DefaultRoundingsRegistryItem(
						"DefaultRounding",
						Categories.AutoRating_Calculation_Rounding,
						ResString.GetMultilingualString("f7a279f0-7cc7-4823-b984-cecfee855425", "Default Rounding"),
						ResString.GetMultilingualString("aa8a1051-3c48-4ca1-af0f-4b58480d532b", "This registry item allows you to configure default rounding for different rating modes."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						DefaultRoundingsCollection.GetDefault());
				});
			}
		}

		#endregion

		#region AutoratingIntercompanyTariffsForGatewayJobConfiguration

		public AutoratingIntercompanyTariffsForGatewayJobConfigurationRegistryItem AutoratingIntercompanyTariffsForGatewayJobConfiguration
		{
			get
			{
				return GetItem("AutoratingIntercompanyTariffsForGatewayJobConfiguration", delegate
				{
					return new AutoratingIntercompanyTariffsForGatewayJobConfigurationRegistryItem(
						"AutoratingIntercompanyTariffsForGatewayJobConfiguration",
						Categories.AutoRating_GatewayBilling,
						(NoResString)"Autorating Intercompany Tariffs for Gateway Billing Job Configuration", // it's just for internal usage so it should not be located
						(NoResString)@"This registry allows you to configure certain logic of Autorating Intercompany Tariffs as Costs/Revenues for Forwarding Gateway Consols/Shipments.", // it's just for internal usage so it should not be located
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		#endregion
	}
}
