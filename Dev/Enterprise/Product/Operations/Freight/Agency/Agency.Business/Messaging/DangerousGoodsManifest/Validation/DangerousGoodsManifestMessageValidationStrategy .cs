using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class DangerousGoodsManifestMessageValidationStrategy : MessageValidationStrategy
	{
		public override MultilingualString Name => ResString.GetMultilingualString("6112e82a-d48f-4ab2-a4a0-f7cc87a8652a", "Dangerous Goods Manifest Messaging");

		public static void RegisterForFactory(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			factory.Validation.MainGroup.RegisterValidationType<BillOfLading, DangerousGoodsManifestShipmentValidation>();
			factory.Validation.MainGroup.RegisterValidationType<BillOfLadingPackLine, DangerousGoodsManifestPackLineValidation>();
			factory.Validation.MainGroup.RegisterValidationType<UNDGDataItem, DangerousGoodsManifestUNDGDataItemValidation>();
			factory.Validation.MainGroup.RegisterValidationType<JobDocAddress, DangerousGoodsManifestJobDocAddressValidation>();
		}

		public static void UnregisterForFactory(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			factory.Validation.MainGroup.UnregisterValidationType<BillOfLading, DangerousGoodsManifestShipmentValidation>();
			factory.Validation.MainGroup.UnregisterValidationType<BillOfLadingPackLine, DangerousGoodsManifestPackLineValidation>();
			factory.Validation.MainGroup.UnregisterValidationType<UNDGDataItem, DangerousGoodsManifestUNDGDataItemValidation>();
			factory.Validation.MainGroup.UnregisterValidationType<JobDocAddress, DangerousGoodsManifestJobDocAddressValidation>();
		}

		public static bool IsRegisteredInFactory(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			return factory.Validation.MainGroup.ContainsDomainValidation(typeof(BillOfLading), typeof(DangerousGoodsManifestShipmentValidation));
		}

		public override bool IsRegistered(BusinessObjectFactory factory)
		{
			return IsRegisteredInFactory(factory);
		}

		public override void Register(BusinessObjectFactory factory)
		{
			RegisterForFactory(factory);
		}

		public override void Unregister(BusinessObjectFactory factory)
		{
			UnregisterForFactory(factory);
		}
	}
}
