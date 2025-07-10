using System;
using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.GUI
{
	public class UniqueDaysActivityCountFilterValidation : CampaignContactNumberFilterValidation
	{
		public UniqueDaysActivityCountFilterValidation(UniqueDaysActivityCountFilter parent)
			: base(parent)
		{
			Parent = parent;
		}

		#region ValidateProperty

		protected override void CheckProperty()
		{
			base.CheckProperty();

			if (Parent.Property.IsDefault && Parent.SqlComparisonOperator != SQLComparisonOperator.Equal)
			{
				Parent.PropertyInfo.AddError(Res.GetString("ce462fac-46ab-417e-850a-f4997391da8e", "You must enter a value greater"));
			}
		}

		#endregion

		public override void ValidateAll()
		{
			ValidateProperty();
		}

		public override Type AutoValidationType
		{
			get { return GetType(); }
		}

		new protected readonly UniqueDaysActivityCountFilter Parent;
	}
}
