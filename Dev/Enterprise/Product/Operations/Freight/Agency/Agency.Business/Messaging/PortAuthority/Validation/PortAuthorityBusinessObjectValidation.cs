using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class PortAuthorityBusinessObjectValidation : MessageValidationStrategy
	{
		public static void RegisterForFactory(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			factory.Validation.MainGroup.RegisterValidationType<BillOfLadingPackLine, PortAuthorityPackLineValidation>();
		}

		public static void UnregisterForFactory(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			factory.Validation.MainGroup.UnregisterValidationType<BillOfLadingPackLine, PortAuthorityPackLineValidation>();
		}

		public static bool IsRegisteredInFactory(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			return factory.Validation.MainGroup.ContainsDomainValidation(typeof(BillOfLadingPackLine), typeof(PortAuthorityPackLineValidation));
		}

		#region MessageValidationStrategy Members

		public override MultilingualString Name
		{
			get { return ResString.GetMultilingualString("2a41a026-3b2b-4374-88df-3e49562f82fc", "Port Authority Messaging"); }
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
