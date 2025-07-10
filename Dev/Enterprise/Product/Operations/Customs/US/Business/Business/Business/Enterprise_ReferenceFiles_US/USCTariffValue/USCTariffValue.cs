using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public sealed class USCTariffValue : AutoUSCTariffValue
	{
		public USCTariffValue(BusinessObjectFactory factory, DataRow row)
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
					!UA_ValueEditCode.IsEmpty ||
					!UA_ValueHighBounds.IsEmpty ||
					!UA_ValueLowBounds.IsEmpty;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(USCTariffValue tariffValue)
				: base(tariffValue)
			{
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				USCTariffValue tariffValue = (USCTariffValue)BusinessObject;
				Factory.AddFetchHint(typeof(USCTariff), USCTariffSchema.PK, tariffValue.UA_UE);
			}
		}
	}
}
