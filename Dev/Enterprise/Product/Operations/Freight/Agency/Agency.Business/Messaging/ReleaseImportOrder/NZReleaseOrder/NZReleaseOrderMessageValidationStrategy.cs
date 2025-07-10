
namespace Enterprise.Freight.Agency.Business
{
	using System;
	using CargoWise.Application;
	using CargoWise.EntityFramework;
	using Enterprise.Services.OperationalActions.Business;
	using Enterprise.ZArchitecture.Core;

	public class NZReleaseOrderMessageValidationStrategy : MessageValidationStrategy
	{
		public static void RegisterForFactory(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			factory.Validation.MainGroup.RegisterValidationType<BillOfLading, NZReleaseOrderShipmentValidation>();
			factory.Validation.MainGroup.RegisterValidationType<BillOfLadingContainer, NZReleaseOrderContainerValidation>();
		}

		public static void UnregisterForFactory(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			factory.Validation.MainGroup.UnregisterValidationType<BillOfLading, NZReleaseOrderShipmentValidation>();
			factory.Validation.MainGroup.UnregisterValidationType<BillOfLadingContainer, NZReleaseOrderContainerValidation>();
		}

		public static bool IsRegisteredInFactory(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			return factory.Validation.MainGroup.ContainsDomainValidation(typeof(BillOfLading), typeof(NZReleaseOrderShipmentValidation));
		}

		#region MessageValidationStrategy

		public override MultilingualString Name
		{
			get { return ResString.GetMultilingualString("5f4dd888-b588-11e4-8006-902b34dc814a", "Release Order Messaging"); }
		}

		public override bool IsApplicable
		{
			get
			{
				var isImportReleaseOrderEnabled = ObjectFactory.Get("FilterIsImportReleaseOrderEnabledRegistryConstraint") as IFilterConstraint;
				return isImportReleaseOrderEnabled != null && (isImportReleaseOrderEnabled.GetValue() as string) == "Y";
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
