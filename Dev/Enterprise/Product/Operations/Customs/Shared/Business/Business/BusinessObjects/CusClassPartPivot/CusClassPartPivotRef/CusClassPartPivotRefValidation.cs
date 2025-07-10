//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusClassPartPivotRefValidation
//
//    This class should be used for overriding validation in AutoCusClassPartPivotRefValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusClassPartPivotRefValidation : AutoCusClassPartPivotRefValidation
	{
		public CusClassPartPivotRefValidation(AutoCusClassPartPivotRef parent) : base(parent)
		{
		}

		protected override void CheckCIR_ReferenceType()
		{
			base.CheckCIR_ReferenceType();
			MandatoryValidation.CheckEntered(Parent.CIR_ReferenceTypeInfo);
		}

		protected override void CheckCIR_ReferenceNumber()
		{
			base.CheckCIR_ReferenceNumber();
			MandatoryValidation.CheckEntered(Parent.CIR_ReferenceNumberInfo);
			CheckDuplication();
		}

		void CheckDuplication()
		{
			var parent = Parent;
			var referenceNumber = parent.CIR_ReferenceNumber;
			if (!referenceNumber.IsEmpty)
			{
				var query = new ZQuery();
				query.AddToFilter(CusClassPartPivotRefSchema.CIR_ReferenceNumber, referenceNumber);
				query.AddToFilter(CusClassPartPivotRefSchema.CIR_ReferenceType, parent.CIR_ReferenceType);
				query.AddToFilter(CusClassPartPivotRefSchema.CIR_CI, parent.CIR_CI);
				query.AddToFilter(CusClassPartPivotRefSchema.PK, SQLComparisonOperator.NotEqual, parent.PK);

				var existingItem = parent.Factory.LoadTop1<CusClassPartPivotRef>(query);
				if (existingItem != null)
				{
					parent.CIR_ReferenceNumberInfo.AddError(Res.GetString("f4e45099-76dd-4852-8b52-eb70adfa2167", "{0} cannot be duplicated.", parent.CIR_ReferenceNumberInfo.HumanReadableName));
				}
			}
		}

		protected new CusClassPartPivotRef Parent => (CusClassPartPivotRef)base.Parent;
	}
}
