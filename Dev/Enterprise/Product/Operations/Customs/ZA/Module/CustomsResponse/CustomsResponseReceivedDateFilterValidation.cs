using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ZA.Module
{
	public class CustomsResponseReceivedDateFilterValidation : ModuleFilterDateValidation
	{
		public CustomsResponseReceivedDateFilterValidation(ModuleDateFilter parent)
		: base(parent)
		{
		}

		protected override void CheckProperty1()
		{
			base.CheckProperty1();
			if (Parent.Property1.IsEmpty)
			{
				Parent.Property1Info.AddError(FromDateMustNotBeEmptyErrorMesage);
			}
			CheckDateRange(Parent.Property1Info);
		}

		protected override void CheckProperty2()
		{
			base.CheckProperty2();
			if (Parent.Property2.IsEmpty)
			{
				Parent.Property2Info.AddError(ToDateMustNotBeEmptyErrorMesage);
			}
			CheckDateRange(Parent.Property2Info);
		}

		void CheckDateRange(ZPropertyInfo propertyInfo)
		{
			if (Parent.Property1.IsValid && Parent.Property2.IsValid && Parent.Property1.AddDays(6) < Parent.Property2)
			{
				propertyInfo.AddError(NoLongerThanOneWeekErrorMesage);
			}
		}

		string FromDateMustNotBeEmptyErrorMesage => Res.GetString("33176A78-392B-48FB-92BD-7E5DB12929E6", "From date must not be empty. Please enter a value.");
		string ToDateMustNotBeEmptyErrorMesage => Res.GetString("837C270D-2BB2-420A-898D-FA345ED99763", "To date must not be empty. Please enter a value.");
		string NoLongerThanOneWeekErrorMesage => Res.GetString("96340DDA-BA4C-49BA-BAEE-571AE242FF8A", "Date range should be no longer than one week.");
	}
}
