using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public sealed class USCTariffQuantity : AutoUSCTariffQuantity
	{
		public USCTariffQuantity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
			}
			base.Delete();
		}
		public override void OnSaving()
		{
			SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
			if (!IsDeleted && !HasValidData)
			{
				Delete();
			}
			base.OnSaving();
		}

		public override bool IsSavedByFactory
		{
			get { return IsInDatabase || HasValidData; }
		}

		bool HasValidData
		{
			get
			{
				return
					!UQ_QuantityEditCode.IsEmpty ||
					!UQ_LowerBound.IsEmpty ||
					!UQ_UpperBound.IsEmpty;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(USCTariffQuantity tariffQuantity)
				: base(tariffQuantity)
			{
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				USCTariffQuantity tariffQuantity = (USCTariffQuantity)BusinessObject;
				Factory.AddFetchHint(typeof(USCTariff), USCTariffSchema.PK, tariffQuantity.UQ_UE);
			}
		}
	}
}
