using System;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.VGMValidation
{
	static class VGMValidationExtensions
	{
		#region AddCarrierStateLengthValidation

		public static Address AddCarrierStateLengthValidation(this Address address)
		{
			if (address == null)
			{
				return null;
			}

			address.StateInfo.AddWarning(() => address.State.Length > 9, (NoResString)"Carrier message state information accepts only 9 characters, extra characters will be truncated when sending VGM to carrier"); // Non-Translatable validation message

			return address;
		}

		#endregion

		#region AddVerifiedByAddressValidation

		public static Address AddVerifiedByAddressValidation(this Address address, string verifiedMethodCode, Func<bool> precondition = null)
		{
			if (address == null)
			{
				return null;
			}

			var isMethodOneOrTwo = verifiedMethodCode == Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container
				|| verifiedMethodCode == Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;

			address.CompanyNameInfo.AddWarning(() => (precondition == null || precondition()) && address.IsEmpty() && isMethodOneOrTwo, (NoResString)"The party designated to ascertain the weight is not mandatory but is recommended to be sent in the VGM message"); // Non-Translatable validation message

			return address;
		}

		#endregion
	}
}
