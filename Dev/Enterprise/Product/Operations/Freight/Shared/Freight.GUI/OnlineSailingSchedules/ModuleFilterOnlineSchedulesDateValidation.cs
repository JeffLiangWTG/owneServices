namespace Enterprise.Freight.GUI.OnlineSailingSchedules
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business;

	class ModuleFilterOnlineSchedulesDateValidation : ModuleFilterDateValidation
	{
		public ModuleFilterOnlineSchedulesDateValidation(OnlineSchedulesModuleDateFilter parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateProperty1();
			ValidateProperty2();
		}

		protected override void CheckProperty1()
		{
			base.CheckProperty1();
			CheckCurrentOrFutureDateEntered(Parent.Property1Info);
		}

		protected override void CheckProperty2()
		{
			base.CheckProperty2();
			CheckCurrentOrFutureDateEntered(Parent.Property2Info);
		}

		void CheckCurrentOrFutureDateEntered(ZPropertyInfo propertyInfo)
		{
			if (Parent.IsPropertySearchUsingSpecifiedDateRange && !GlbStaff.CurrentUser.IsSupportUser)
			{
				if ((ZDateTime)propertyInfo.Value < ZDateTime.Today)
				{
					propertyInfo.AddError(PastDateNotAlowedErrorMessage);
				}
			}
		}

		string PastDateNotAlowedErrorMessage => Res.GetString("171D90EA-5BB4-451F-BD72-09447B5A050A", "Schedules can be searched by current or future dates only.");
	}
}
