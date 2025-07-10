using System;
using CargoWise.Types;
using Enterprise.Packing.Business;
using WTG.RTUS.Interface;
using WTG.RTUS.Printing;

namespace Enterprise.Packing.DataTransfer
{
	public static class RTUSRemotePrintingHelper
	{
		public static RemotePrintServerConfig GetRemotePrintServerConfig(string remotePrintingServerRaw, string remotePrintingUserName, string remotePrintingPassword)
		{
			const string LegacyUrlSuffix = "/RemotePrintingService.asmx";

			var remotePrintingServer = remotePrintingServerRaw.EndsWith(LegacyUrlSuffix, StringComparison.OrdinalIgnoreCase)
				? remotePrintingServerRaw.Substring(0, remotePrintingServerRaw.Length - LegacyUrlSuffix.Length)
				: remotePrintingServerRaw;

			bool userNameAndPasswordProvided = IsUsernameProvided(remotePrintingUserName) && IsPasswordProvided(remotePrintingPassword);
			return userNameAndPasswordProvided && Uri.TryCreate(remotePrintingServer, UriKind.Absolute, out var result)
				? new RemotePrintServerConfig(result, remotePrintingUserName, remotePrintingPassword)
				: null;
		}

		public static string GetRemotePrintServerConfigErrorDetails(string remotePrintingServerRaw, string remotePrintingUserName, string remotePrintingPassword)
		{
			var errorMessageBuilder = new ZStringBuilder();
			if (!Uri.IsWellFormedUriString(remotePrintingServerRaw, UriKind.Absolute))
			{
				errorMessageBuilder.Append(Res.GetString("4170c2eb-b398-4afb-a305-7c0174917312", "Invalid Remote Printing Server URL in Registry '{0}'.", remotePrintingServerRaw));
			}

			if (!IsUsernameProvided(remotePrintingUserName))
			{
				errorMessageBuilder.Append(Res.GetString("856590c9-09d1-4ca6-a4f2-1e0fdae38366", "No Remote Printing Username provided in Registry."));
			}

			if (!IsPasswordProvided(remotePrintingPassword))
			{
				errorMessageBuilder.Append(Res.GetString("8f5d4e6c-151a-4fdb-8085-de67a36d955b", "No Remote Printing Password provided in Registry."));
			}

			errorMessageBuilder.Prepend(Res.GetString("a432aa25-250e-45c3-94c5-fa30b4ed0725", "Unable to Print Carrier Label due to the following:"));

			return errorMessageBuilder.ToStringWithDelimiterBetweenAppends(" ");
		}

		static bool IsUsernameProvided(string remotePrintingUserName) => !string.IsNullOrWhiteSpace(remotePrintingUserName);

		static bool IsPasswordProvided(string remotePrintingPassword) => !string.IsNullOrWhiteSpace(remotePrintingPassword);

		public static void UpdatePackageWithRTUSResponse(PkgPackage referencePackage, IPackingParent packingParent, ZString trackingNumber, ZString transportReference, ZDateTime packageBookedTime, ZString rtusBookedType)
		{
			// we do not want to set the Previous Package ID to the Package ID if it is the same as the Package ID
			var packageID = referencePackage.KP_PackageID;
			if (packageID != trackingNumber)
			{
				referencePackage.KP_PackageID = trackingNumber;
				referencePackage.KP_PreviousPackageID = packageID;
			}

			if (packingParent != null)
			{
				if (!string.IsNullOrWhiteSpace(transportReference))
				{
					packingParent.TransportReference = transportReference;
				}
				packingParent.OnPackageBookedViaRTUS(packageBookedTime);
			}

			referencePackage.IsSentToRTUS = true;
			referencePackage.RTUSBookedType = rtusBookedType;
		}

		public static bool IsValidRTUSResponse(PkgPackageJob packageJob, string packageId, string parentJobTransportReference, ReferenceNumbers referenceNumbers)
		{
			return IsValidRTUSResponseCore(packageJob, packageId, parentJobTransportReference, referenceNumbers.TrackingNumber, referenceNumbers.TransportReference);
		}

		public static bool IsValidRTUSResponse(PkgPackageJob packageJob, string packageId, string parentJobTransportReference, ISingleBookingRTUSResponse singleBookingRTUSResponse)
		{
			return IsValidRTUSResponseCore(packageJob, packageId, parentJobTransportReference, singleBookingRTUSResponse.TrackingNumber, singleBookingRTUSResponse.TransportReference);
		}

		static bool IsValidRTUSResponseCore(PkgPackageJob packageJob, string packageId, string parentJobTransportReference, string responsePackageId, string responseTransportReference)
		{
			return !packageJob.KJ_IsFinalized
				|| (packageId == responsePackageId && parentJobTransportReference == responseTransportReference);
		}

		public static string PackageJobFinalizedError => Res.GetString("507a4c00-4cef-4670-a836-579a8e2fa700", "The Package job is already finalized and the RTUS response attempted to update the package details.");
	}
}
