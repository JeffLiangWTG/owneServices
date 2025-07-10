using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MarketingManager.Module
{
	public class ValueAnalysisLocationFilter : ModuleLocationFilter
	{
		protected ValueAnalysisLocationFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ValueAnalysisLocationFilter(ZString description, SchemaStringColumn location1FilterColumn, IBusinessObjectCollection location1List, SchemaStringColumn location2FilterColumn, IBusinessObjectCollection location2List, bool allowInternationalZones = false)
			: base(description, location1FilterColumn, location1List, location2FilterColumn, location2List, allowInternationalZones)
		{
		}

		public ValueAnalysisLocationFilter(ZString description, GetCodeQuery queryDelegate, IBusinessObjectCollection location1List, IBusinessObjectCollection location2List)
			: base(description, queryDelegate, location1List, location2List)
		{
		}

		#region Validation

		public new ValueAnalysisLocationFilterValidation Validation
		{
			get { return (ValueAnalysisLocationFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ValueAnalysisLocationFilterValidation(this);
		}

		#endregion
	}

	public class ValueAnalysisLocationFilterValidation : ModuleCodeFilterValidation
	{
		public ValueAnalysisLocationFilterValidation(ValueAnalysisLocationFilter parent)
		: base(parent)
		{
			Parent = parent;
		}

		#region ValidateProperty1

		public new void ValidateProperty1()
		{
			ValidateCalculatedProperty(Parent.Property1Info);
		}
		protected new void CheckProperty1()
		{
			if (Parent.Property1.Length < 5)
			{
				Parent.Property1Info.AddWarning(Res.GetString("D8CD43F1-F5C7-45E1-86A1-678094826EC3", "This filter only returns results by aggregated trade lane, trade mode and trade type"));
			}

			Parent.Property1Validation?.Invoke(Parent.Property1Info);
		}

		#endregion

		#region ValidateProperty2

		public new void ValidateProperty2()
		{
			ValidateCalculatedProperty(Parent.Property2Info);
		}

		protected new void CheckProperty2()
		{
			if (Parent.Property2.Length < 5)
			{
				Parent.Property2Info.AddWarning(Res.GetString("8FE06030-CCE6-4990-BD1E-1B184FB508F0", "This filter only returns results by aggregated trade lane, trade mode and trade type"));
			}

			Parent.Property2Validation?.Invoke(Parent.Property2Info);
		}

		#endregion

		public override void ValidateAll()
		{
			ValidateProperty1();
			ValidateProperty2();
		}

		public override Type AutoValidationType
		{
			get { return GetType(); }
		}

		protected new ValueAnalysisLocationFilter Parent;
	}
}
