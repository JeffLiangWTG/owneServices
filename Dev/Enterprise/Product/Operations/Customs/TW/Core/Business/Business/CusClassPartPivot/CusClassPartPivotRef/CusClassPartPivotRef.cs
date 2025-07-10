using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public abstract class CusClassPartPivotRef : Customs.Business.CusClassPartPivotRef
	{
		protected CusClassPartPivotRef(BusinessObjectFactory factory, DataRow row)
					: base(factory, row)
		{
		}

		public abstract ZString ReferenceType { get; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CIR_ReferenceType = ReferenceType;
		}
	}
}
