using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusEngine : Customs.Business.CusEngine
	{
		public CusEngine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Customs.Business.CusEngineLookups GetNewLookups() => new CusEngineLookups(this);

		public new CusEngineLookups Lookups => (CusEngineLookups)base.Lookups;

		protected override Customs.Business.CusEngineValidation GetNewValidation() => new CusEngineValidation(this);

		public new CusEngineValidation Validation => (CusEngineValidation)base.Validation;

		[DecimalPlaces(0)]
		public override ZDecimal CEG_CapacityCC
		{
			get => base.CEG_CapacityCC;
			set => base.CEG_CapacityCC = value;
		}

		[DecimalPlaces(0)]
		public override ZDecimal CEG_CapacityHP
		{
			get => base.CEG_CapacityHP;
			set => base.CEG_CapacityHP = value;
		}

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(CusEngineLookups.EngineTypeList))]
		public override ZString CEG_EngineType
		{
			get => base.CEG_EngineType;
			set => base.CEG_EngineType = value;
		}
	}
}
