using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Universal.ConditionChecker;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusConditionValue : AutoRefCusConditionValue
	{
		public RefCusConditionValue(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("Enterprise.Customs.Universal.RefCusConditionValue|ZX3_Value", Caption = "Value")]
		public override ZString ZX3_Value { get => base.ZX3_Value; set => base.ZX3_Value = value; }

		[RelatedBusinessObject(nameof(Condition))]
		public override ZGuid ZX3_ZX1_Condition
		{
			get => base.ZX3_ZX1_Condition;
			set => base.ZX3_ZX1_Condition = value;
		}

		public RefCusCondition Condition => Factory.Load<RefCusCondition>(ZX3_ZX1_Condition);

		#region RefCusConditionValueType

		[RelatedBusinessObject(nameof(ConditionValueType))]
		public override ZGuid ZX3_ZX4_ValueType
		{
			get { return base.ZX3_ZX4_ValueType; }
			set { base.ZX3_ZX4_ValueType = value; }
		}

		public RefCusConditionValueType ConditionValueType => Factory.Load<RefCusConditionValueType>(ZX3_ZX4_ValueType);

		[ResourceStringData("Enterprise.Customs.Universal.RefCusConditionValue|ValueType", Caption = "Value Type")]
		public ZString ValueType => ConditionValueType?.ZX4_ValueType ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.Universal.RefCusConditionValue|ValueTypeDescription", Caption = "Value Type Description", ShortCaption = "Description")]
		public ZString ValueTypeDescription => ConditionValueType?.ZX4_Description ?? ZString.Empty;

		#endregion

		#region Check of Condition

		public ZBool IsConditionMet(EvaluateConditionValue valuationDelegate, IUniversalRateCalcData calcData = null)
		{
			return IsInformationConditionValue || (ConditionValueType.ZX4_IsFormula ? IsConditionFormulaMet(calcData) : IsConditionValueMet(valuationDelegate));
		}

		ZBool IsConditionValueMet(EvaluateConditionValue valuationDelegate)
		{
			return valuationDelegate?.Invoke(Condition.ConditionType, ValueType, ZX3_Value) ?? ZBool.False;
		}

		ZBool IsConditionFormulaMet(IUniversalRateCalcData calcData)
		{
			return calcData != null && new ConditionCalculator(ZX3_Value, calcData).Evaluate();
		}

		public ZBool IsInformationConditionValue => ValueType == Constants.ConditionValueType.Information;

		public ZBool IsDocumentConditionValue => ValueType == Constants.ConditionValueType.SupportingDocument || ValueType == Constants.ConditionValueType.SupportingDocumentNoReferenceNumber;

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new Strategy(this);

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(RefCusConditionValue conditionValue)
				: base(conditionValue)
			{
			}

			new RefCusConditionValue BusinessObject => (RefCusConditionValue)base.BusinessObject;

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();

				Factory.AddFetchHint(RefCusConditionValueTypeSchema.PK, BusinessObject.ZX3_ZX4_ValueType);
			}
		}
	}
}
