using System;
using System.Net;
using System.ServiceModel;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Services.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.DistanceCalculation.Business.DistanceCalculationServiceReference;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.DistanceCalculation.Business
{
	public class DistanceCalculationManager : ILicensedComponent
	{
		#region Calculate

		public DistanceCalculationResult Calculate(SecurityCheckpoint checkpoint, Guid identifier, DistanceCalculationConfiguration distanceCalculationConfig, DistanceCalculationAddress originAddress, DistanceCalculationAddress destinationAddress)
		{
			var result = new DistanceCalculationResult();

			if (checkpoint.IsAllowed)
			{
				var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
				var enterpriseCode = registrationKey.EnterpriseCode;
				var serverCode = registrationKey.ServerCode;

				var serviceConfig = new ServiceRequestConfigurationData()
				{
					ClientSpecifiedID = identifier,
					UserName = EnvProxy.Instance.CurrentUser.LoginName,
					LicenceCodeEnterpriseAndDatabase = enterpriseCode + "-" + serverCode,
					LicenceCode = enterpriseCode + "-" + EnvProxy.Instance.CurrentCompany.Code + "-" + serverCode
				};
				result = RunService(serviceConfig, distanceCalculationConfig, originAddress, destinationAddress);
			}
			else
			{
				result.StatusMessage = checkpoint.ErrorMessageForNotAllowed;
			}

			return result;
		}

		#endregion

		#region RunService

		DistanceCalculationResult RunService(ServiceRequestConfigurationData serviceConfig, DistanceCalculationConfiguration distanceCalculationConfig, DistanceCalculationAddress originAddress, DistanceCalculationAddress destinationAddress)
		{
			DistanceCalculationResult result = new DistanceCalculationResult();

#if DEBUG
			if (Globals.IsTest)
			{
				string fakeDistance = originAddress.Address1 + originAddress.Address2 + originAddress.City + originAddress.Country + originAddress.PostCode + originAddress.State;
				fakeDistance += destinationAddress.Address1 + destinationAddress.Address2 + destinationAddress.City + destinationAddress.Country + destinationAddress.PostCode + destinationAddress.State;

				result.Distance = fakeDistance.Length;
				result.DistanceUnit = string.IsNullOrEmpty(distanceCalculationConfig.UnitsForCalculation) ? DistanceCalculationConstants.UnitsForCalculation.Miles : distanceCalculationConfig.UnitsForCalculation;
				result.TravelTime = 0;
				result.StatusMessage = "";

				CallCountForTest++;

				return result;
			}
#endif

			ServicePointManager.ServerCertificateValidationCallback += ((sender, certificate, chain, sslPolicyErrors) => true); // don't validate against invalid SSL cert
			string serviceURL = DistanceCalculationRegistry.Instance.DistanceCalculationServiceURL.Value;
			var binding = new WSHttpBinding();
			binding.Security.Mode = serviceURL.ToLower().StartsWith((NoResString)"https") ? SecurityMode.Transport : SecurityMode.None;

			DistanceCalculationServiceClient service = new DistanceCalculationServiceClient(binding, new EndpointAddress(serviceURL));
			try
			{
				result = service.Calculate(serviceConfig, distanceCalculationConfig, originAddress, destinationAddress);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result.StatusMessage += Res.GetString("b5eaf17c-c662-4763-95f8-ffa1820d454a", "Error connecting to CargoWise Distance Calculation Service - {0}", ex.Message);
				result.StatusMessage += System.Environment.NewLine + System.Environment.NewLine;
				result.StatusMessage += Res.GetString("773cf349-c0e8-4e71-a0a3-ca7d6654af93", "Please contact your System Administrator for assistance.");
			}
			finally
			{
				if (service.State != CommunicationState.Closed && service.State != CommunicationState.Closing && service.State != CommunicationState.Faulted)
				{
					service.Close();
				}
			}

			return result;
		}

		#endregion

		#region ILicensedComponent Members

		public LicensedComponentManager LicensedComponentManager
		{
			get { return ((ILicensedComponent)this).LicensedComponentManager as LicensedComponentManager; }
		}

		IDisposable ILicensedComponent.LicensedComponentManager
		{
			get
			{
				if (fLicensedComponentManager == null)
				{
					fLicensedComponentManager = new LicensedComponentManager(this);
				}
				return fLicensedComponentManager;
			}
		}

		LicensedComponentManager fLicensedComponentManager;

		#endregion

		#region CallCountForTest

#if DEBUG
		public static int CallCountForTest
		{
			get { return ZInt.ParseSafe(Env.Registry.GetFilterCriteria(CallCountCacheName), 0); }
			set { Env.Registry.SetFilterCriteria(CallCountCacheName, value.ToString()); }
		}

		const string CallCountCacheName = "DistanceCalculationCallCount";
#endif

		#endregion
	}
}
