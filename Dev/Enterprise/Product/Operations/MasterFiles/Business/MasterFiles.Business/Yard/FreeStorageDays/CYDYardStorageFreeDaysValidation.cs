using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class CYDYardStorageFreeDaysValidation : AutoCYDYardStorageFreeDaysValidation
	{
		public CYDYardStorageFreeDaysValidation(AutoCYDYardStorageFreeDays parent) : base(parent)
		{
		}

		#region YFD_UnitType

		protected override void CheckYFD_UnitType()
		{
			base.CheckYFD_UnitType();
			MandatoryValidation.CheckEntered(Parent.YFD_UnitTypeInfo);
		}

		#endregion

		#region YFD_YardUnitLength

		protected override void CheckYFD_YardUnitLength()
		{
			base.CheckYFD_YardUnitLength();
			MandatoryValidation.CheckEntered((Parent as CYDYardStorageFreeDays).YardUnitLengthInfo);
		}

		#endregion
	}
}
