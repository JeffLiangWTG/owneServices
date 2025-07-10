
namespace Enterprise.Freight.Agency.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;

	public class NZPortMessageValidationStrategy : MessageValidationStrategy
	{
		public static void RegisterForFactory(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			factory.Validation.MainGroup.RegisterValidationType<BillOfLading, NZPortMessageShipmentValidation>();
			factory.Validation.MainGroup.RegisterValidationType<BillOfLadingContainer, NZPortMessageContainerValidation>();
		}

		public static void UnregisterForFactory(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			factory.Validation.MainGroup.UnregisterValidationType<BillOfLading, NZPortMessageShipmentValidation>();
			factory.Validation.MainGroup.UnregisterValidationType<BillOfLadingContainer, NZPortMessageContainerValidation>();
		}

		public static bool IsRegisteredInFactory(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			return factory.Validation.MainGroup.ContainsDomainValidation(typeof(BillOfLading), typeof(NZPortMessageShipmentValidation));
		}

		#region MessageValidationStrategy

		public override MultilingualString Name
		{
			get { return ResString.GetMultilingualString("c728a164-96dd-11e4-b2bc-902b34dc814a", "Port Messaging"); }
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
