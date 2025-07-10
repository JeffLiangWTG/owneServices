using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class EIDOBusinessObjectValidation : MessageValidationStrategy
	{
		public EIDOBusinessObjectValidation()
		{
			IsEnabled = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Australia
					&& AgencyRegistry.Instance.EIDOMessagingDetails.Value.Identities.Count > 0;
		}

		public static void RegisterForFactory(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			factory.Validation.MainGroup.RegisterValidationType<BillOfLading, EIDOShipmentValidation>();
		}

		public static void UnregisterForFactory(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			factory.Validation.MainGroup.UnregisterValidationType<BillOfLading, EIDOShipmentValidation>();
		}

		public static bool IsRegisteredInFactory(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			return factory.Validation.MainGroup.ContainsDomainValidation(typeof(BillOfLading), typeof(EIDOShipmentValidation));
		}

		#region MessageValidationStrategy Members

		public override MultilingualString Name
		{
			get
			{
				return ResString.GetMultilingualString("10016c93-16f6-4f8b-86ee-929517a44547", "EIDO Messaging");
			}
		}

		public override bool IsApplicable
		{
			get
			{
				return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Australia;
			}
		}

		public override void Register(BusinessObjectFactory factory)
		{
			RegisterForFactory(factory);
		}

		public override void Unregister(BusinessObjectFactory factory)
		{
			UnregisterForFactory(factory);
		}

		public override bool IsRegistered(BusinessObjectFactory factory)
		{
			return IsRegisteredInFactory(factory);
		}

		#endregion
	}
}
