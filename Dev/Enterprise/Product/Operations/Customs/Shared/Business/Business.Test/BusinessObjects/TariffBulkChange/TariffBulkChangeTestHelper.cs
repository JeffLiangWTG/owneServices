using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class TariffBulkChangeTestHelper : TariffBulkChange
	{
		public TariffBulkChangeTestHelper(BusinessObjectFactory factory)
			: base(factory)
		{
			fisProductionDataBase = true;
			fDateTimeNow = ZDateTime.Now;
		}

		public override ZString ReferenceKey => "HS2022 HS " + lookupType;

		public override ZGuid CountryPK => Core.Constants.CountryGuids.Eritrea;

		public override ZString CountryCode => Enterprise.Core.Constants.CountryCodes.Eritrea;

		protected override bool IsPivotTariffNumSupported => IsPivotTariffNumSupportedExposed;

		public bool IsPivotTariffNumSupportedExposed { get; set; }

		protected override ZString[] ValidPivotTypes => new ZString[] { ValidPivotTypeExposed };

		public ZString ValidPivotTypeExposed { get; set; }

		public override bool IsProductionDataBase => fisProductionDataBase;

		public bool SetIsProductionDataBase
		{
			set
			{
				fisProductionDataBase = value;
			}
		}
		bool fisProductionDataBase;

		protected override ZDateTime DateTimeNow => fDateTimeNow;

		public ZDateTime SetDateTimeNow
		{
			set
			{
				fDateTimeNow = value;
			}
		}
		ZDateTime fDateTimeNow;
	}
}
