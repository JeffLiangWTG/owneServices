using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class RecentActivityDateFilterValidation : ModuleFilterDateValidation
	{
		public RecentActivityDateFilterValidation(RecentActivityDateFilter parent)
			: base(parent)
		{
		}

		new RecentActivityDateFilter Parent
		{
			get { return (RecentActivityDateFilter)base.Parent; }
		}

		#region ValidateProperty1

		protected override void CheckProperty1()
		{
			base.CheckProperty1();

			if (!Parent.TypeProperty.IsEmpty && Parent.IsDateEmtpy)
			{
				MandatoryValidation.CheckEntered(Parent.Property1Info);
			}
		}

		#endregion

		#region ValidateProperty2

		protected override void CheckProperty2()
		{
			base.CheckProperty2();

			if (!Parent.TypeProperty.IsEmpty && Parent.IsDateEmtpy)
			{
				MandatoryValidation.CheckEntered(Parent.Property2Info);
			}
		}

		#endregion

		#region ValidateTypeProperty

		public void ValidateTypeProperty()
		{
			ValidateCalculatedProperty(Parent.TypePropertyInfo);
		}

		protected void CheckTypeProperty()
		{
			ListValidation.ErrorIfInvalidCode(Parent.TypePropertyInfo);
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTypeProperty();
		}
	}
}
