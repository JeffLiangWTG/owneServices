using System;
using System.Collections.Generic;
using System.Web;
using System.Configuration;

using CargoWise.Services.Common;

using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.Freight.DistanceCalculation.Service.PCMilerService;
using PCMillerWebService = Enterprise.Freight.DistanceCalculation.Service.PCMilerService.Service;

namespace Enterprise.Freight.DistanceCalculation.Service
{
	public class PCMilerDistanceCalculationService
	{
		#region Configuration

		public static class Configuration
		{
			public static string ServiceURL { get { return ConfigState.URL; } }
			public static string UserName { get { return ConfigState.UserName; } }
			public static string Password { get { return ConfigState.Password; } }
			public static string ClientID { get { return ConfigState.ClientID; } }

			static IProviderConfigurationState ConfigState
			{
				get
				{
					if (configState == null)
					{
						ProviderConfiguration Config = (ProviderConfiguration)ConfigurationManager.GetSection("ProviderConfigurationSection");
						configState = Config != null ? Config.Configurations.GetConfigForProvider(DistanceCalculationConstants.Providers.PCMiler) : new ProviderConfigurationState();
					}
					return configState;
				}
			}

			[ThreadStatic]
			static IProviderConfigurationState configState;
		}

		#endregion

		#region ReportTypes constants

		static class ReportTypes
		{
			public const string TripDistance = "M";
			public const string StateCountryDistance = "S";
			public const string DetailedDrivingDirections = "D";
			public const string DriverReport = "V";
			public const string RoadTypeReport = "R";
			public const string FuelOptimization = "F";

			public const string Default = TripDistance;
		}

		#endregion

		#region Validation

		string ConfigurationErrors
		{
			get { return fConfigurationErrors; }
			set
			{
				if ((value != "") && (fConfigurationErrors.Length > 0))
				{
					fConfigurationErrors += " " + value;
				}
				else
				{
					fConfigurationErrors = value;
				}
			}
		}
		string fConfigurationErrors;

		internal bool ValidateConfiguration(DistanceCalculationConfiguration DistanceCalculationConfig)
		{
			bool result = true;
			ConfigurationErrors = "";

			#region Mapping

			if (string.IsNullOrEmpty(DistanceCalculationConfig.ProviderVersion)
				|| DistanceCalculationConfig.ProviderVersion == DistanceCalculationConstants.ProviderVersions.PCMiler.Current)
			{
				DistanceCalculationConfig.ProviderVersion = "Current"; // SuppressCodeSmell Reason = hard-coded constant
			}

			if (DistanceCalculationConfig.CalculationMethod == DistanceCalculationConstants.CalculationMethods.PCMiler.Practical)
			{
				DistanceCalculationConfig.RoutingType = DistanceCalculationPCMilerConstants.RoutingTypes.Practical;
			}
			else if (DistanceCalculationConfig.CalculationMethod == DistanceCalculationConstants.CalculationMethods.PCMiler.Shortest)
			{
				DistanceCalculationConfig.RoutingType = DistanceCalculationPCMilerConstants.RoutingTypes.Shortest;
			}

			#endregion

			#region SetUp parametrs to defaults if blank

			if (string.IsNullOrEmpty(DistanceCalculationConfig.AvoidTollRoads))
			{
				DistanceCalculationConfig.AvoidTollRoads = DistanceCalculationPCMilerConstants.AvoidTollRoads.Default;
			}

			if (string.IsNullOrEmpty(DistanceCalculationConfig.HazardousType))
			{
				DistanceCalculationConfig.HazardousType = DistanceCalculationPCMilerConstants.HazardousTypes.Default;
			}

			if (string.IsNullOrEmpty(DistanceCalculationConfig.RoutingType))
			{
				DistanceCalculationConfig.RoutingType = DistanceCalculationPCMilerConstants.RoutingTypes.Default;
			}

			if (string.IsNullOrEmpty(DistanceCalculationConfig.UnitsForCalculation))
			{
				DistanceCalculationConfig.UnitsForCalculation = DistanceCalculationConstants.UnitsForCalculation.Default;
			}

			#endregion

			switch (DistanceCalculationConfig.UnitsForCalculation)
			{
				case DistanceCalculationConstants.UnitsForCalculation.Kilometres:
				case DistanceCalculationConstants.UnitsForCalculation.Miles:
					break;

				default:
					result = false;
					ConfigurationErrors = "Error in configuration UnitsForCalculation - " + DistanceCalculationConfig.UnitsForCalculation;
					break;
			}

			switch (DistanceCalculationConfig.AvoidTollRoads)
			{
				case DistanceCalculationPCMilerConstants.AvoidTollRoads.No:
				case DistanceCalculationPCMilerConstants.AvoidTollRoads.Yes:
					break;

				default:
					result = false;
					ConfigurationErrors = "Error in configuration AvoidTollRoads - " + DistanceCalculationConfig.AvoidTollRoads;
					break;
			}

			switch (DistanceCalculationConfig.HazardousType)
			{
				case DistanceCalculationPCMilerConstants.HazardousTypes.Caustic:
				case DistanceCalculationPCMilerConstants.HazardousTypes.Explosives:
				case DistanceCalculationPCMilerConstants.HazardousTypes.Flammable:
				case DistanceCalculationPCMilerConstants.HazardousTypes.General:
				case DistanceCalculationPCMilerConstants.HazardousTypes.Inhalants:
				case DistanceCalculationPCMilerConstants.HazardousTypes.None:
				case DistanceCalculationPCMilerConstants.HazardousTypes.Radioactive:
					break;

				default:
					result = false;
					ConfigurationErrors = "Error in configuration HazardousType - " + DistanceCalculationConfig.HazardousType;
					break;
			}

			switch (DistanceCalculationConfig.RoutingType)
			{
				case DistanceCalculationPCMilerConstants.RoutingTypes.LineOfSight:
				case DistanceCalculationPCMilerConstants.RoutingTypes.Practical:
				case DistanceCalculationPCMilerConstants.RoutingTypes.Shortest:
					break;

				default:
					result = false;
					ConfigurationErrors = "Error in configuration RoutingTypes - " + DistanceCalculationConfig.RoutingType;
					break;
			}

			return result;
		}

		#endregion

		public DistanceCalculationResult Process(DistanceCalculationConfiguration DistanceCalculationConfig, DistanceCalculationAddress OriginAddress, DistanceCalculationAddress DestinationAddress)
		{			
			DistanceCalculationResult result = new DistanceCalculationResult();

			if (ValidateConfiguration(DistanceCalculationConfig))
			{
				#region SetUp input data for PC*Miller

				string LoginUser = Configuration.UserName;
				string LoginPassword = Configuration.Password;
				string LoginAccount = Configuration.ClientID;

				// This was used for testing purpose
				//string LoginUser = "CargoWise";
				//string LoginPassword = "cargo48381";
				//string LoginAccount = "cargowise edi";

				LoginType loginInfo = new LoginType();
				loginInfo.UserID = LoginUser;
				loginInfo.Password = LoginPassword;
				loginInfo.Account = LoginAccount;

				LocationInputType tripOrigin = CreateLocationInputTypeFromAddress(OriginAddress);
				List<LocationInputType> tripOriginList = new List<LocationInputType>();
				tripOriginList.Add(tripOrigin);

				LocationInputType tripDestination = CreateLocationInputTypeFromAddress(DestinationAddress);
				List<LocationInputType> tripDestinationList = new List<LocationInputType>();
				tripDestinationList.Add(tripDestination);

				OptionsType tripOptions = new OptionsType();
				tripOptions.Units = DistanceCalculationConfig.UnitsForCalculation;
				tripOptions.DataVer = DistanceCalculationConfig.ProviderVersion;
				tripOptions.HazMatType = DistanceCalculationConfig.HazardousType;
				tripOptions.RoutingType = DistanceCalculationConfig.RoutingType;
				tripOptions.TollDiscourage = DistanceCalculationConfig.AvoidTollRoads;

				List<OptionsType> tripOptionsList = new List<OptionsType>();
				tripOptionsList.Add(tripOptions);

				List<string> reportTypeList = new List<string>();
				reportTypeList.Add(ReportTypes.Default);

				int reportInstance = 1;

				#endregion

				PCMillerWebService webService = new PCMillerWebService();
				ReportResponse[] reportResponses = webService.PMWSGetReport(loginInfo, 
					tripOriginList.ToArray(), 
					tripDestinationList.ToArray(), 
					null, 
					null, 
					null, 
					tripOptionsList.ToArray(), 
					null, 
					null, 
					null, 
					reportTypeList.ToArray(), 
					null, 
					reportInstance);

				if (reportResponses != null && reportResponses.Length > 0 &&
					reportResponses[0].Report != null && reportResponses[0].Report.MilesReport != null && reportResponses[0].Report.MilesReport.Length > 1)
				{
					StpLineType milesReport = reportResponses[0].Report.MilesReport[1];

					double distance = 0;
					if (double.TryParse(milesReport.TMiles, out distance))
					{
						result.Distance = distance;
						result.DistanceUnit = tripOptions.Units;
					}
					else
					{
						result.StatusMessage += "Error converting resulting distance";
					}
				}
				else if (reportResponses != null && reportResponses.Length > 0 &&
					reportResponses[0].ErrorList != null && reportResponses[0].ErrorList.Length > 0)
				{
					for (int i = 0; i < reportResponses[0].ErrorList.Length; i++)
					{
						ErrorType error = reportResponses[0].ErrorList[i];
						result.StatusMessage += error.ErrorDesc + " ";
					}
				}
				else
				{
					result.StatusMessage += "Error getting distance report";
				}
			}
			else
			{
				result.StatusMessage = ConfigurationErrors;
			}

			return result;
		}

		LocationInputType CreateLocationInputTypeFromAddress(DistanceCalculationAddress Address)
		{
			LocationInputType result = new LocationInputType();
			result.Address1 = Address.Address1;
			result.Address2 = Address.Address2;
			result.City = Address.City;
			result.State = Address.State;
			result.Zip = Address.PostCode;
			
			return result;
		}
	}
}
