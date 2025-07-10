using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.ZA.Business
{
	[SystemDefinedValues]
	public partial class CusClassification : BaseCusClassification, Integration.Customs.ZA.ICusClassification
	{
		public CusClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overridden

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
		}

		public new CusClassificationLookups Lookups
		{
			get { return (CusClassificationLookups)base.Lookups; }
		}

		protected override Customs.Business.CusClassificationLookups GetNewLookups()
		{
			return new CusClassificationLookups(this);
		}

		protected override Customs.Business.CusClassificationValidation GetNewValidation()
		{
			return new CusClassificationValidation(this);
		}

		#endregion

		#region New Properties

		public TariffView Tariff
		{
			get { return Factory.GetCusTariff(UniversalReferenceConstants.CusTariffCode.Schedule1Part1, CC_TariffNum, ZDateTime.Today); }
		}

		#endregion

		#region Implementation

		protected override TariffFormatter GetTariffFormatter()
		{
			return new NumberOnlyTariffFormatter();
		}

		#endregion

	}
}
