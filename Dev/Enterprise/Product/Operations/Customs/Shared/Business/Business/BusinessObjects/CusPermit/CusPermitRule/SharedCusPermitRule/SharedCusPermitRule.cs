using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Business
{
	public abstract class SharedCusPermitRule : CommonCusPermitRule
	{
		public SharedCusPermitRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static class RuleCodes
		{
			public const string CountryOfOrigin = "COO";
			public const string Tariff = "TAR";
		}

		public static readonly SharedCusPermitRuleTypeDecider TypeDecider = new SharedCusPermitRuleTypeDecider();

		#region Override Properties

		[RelatedBusinessObject(nameof(SharedCusPermitRule.PermitHeader))]
		public override ZGuid CPR_CPH_PermitHeader
		{
			get { return base.CPR_CPH_PermitHeader; }
			set { base.CPR_CPH_PermitHeader = value; }
		}

		[List(nameof(Lookups) + "." + nameof(SharedCusPermitRuleLookups.PermitRuleCodes))]
		public override ZString CPR_RuleCode
		{
			get { return base.CPR_RuleCode; }
			set
			{
				var originalValue = CPR_RuleCode;
				base.CPR_RuleCode = value;
				if (!IsCopying && originalValue != CPR_RuleCode)
				{
					valueComparer = null;
					CPR_ValueFrom = GetDefaultValueBasedOnType();
					CPR_ValueTo = ZString.Empty;
				}
			}
		}

		#region CPR_ValueFrom

		[List(nameof(Lookups) + "." + nameof(SharedCusPermitRuleLookups.CPR_ValueFromList))]
		public override ZString CPR_ValueFrom
		{
			get { return base.CPR_ValueFrom; }
			set
			{
				var hasChanged = CPR_ValueFrom != value;
				base.CPR_ValueFrom = value;
				if (hasChanged && CPR_ValueTo.IsEmpty && !CPR_ValueTo_ReadOnly && InitializeCPR_ValueToWithCPR_ValueFrom)
				{
					CPR_ValueTo = CPR_ValueFrom;
				}
			}
		}

		protected virtual bool InitializeCPR_ValueToWithCPR_ValueFrom => true;

		public virtual ZString CPR_ValueFrom_FieldType => PermitHeader?.GetCountrySpecificInstruction()?.GetValueFromFieldType(CPR_RuleCode) ?? nameof(FieldType.Text);

		public ZBool CPR_ValueFromIsCodeField => CPR_ValueFrom_FieldType == nameof(FieldType.Boolean) || CPR_ValueFrom_FieldType == nameof(FieldType.TextCodeFindBox) || CPR_ValueFrom_FieldType == nameof(FieldType.TextDropEdit);

		#endregion

		#region CPR_ValueTo
		[List(nameof(Lookups) + "." + nameof(SharedCusPermitRuleLookups.CPR_ValueToList))]
		[ReadOnlyMember(nameof(CPR_ValueTo_ReadOnly))]
		public override ZString CPR_ValueTo
		{
			get { return base.CPR_ValueTo; }
			set { base.CPR_ValueTo = value; }
		}

		public virtual ZString CPR_ValueTo_FieldType => PermitHeader?.GetCountrySpecificInstruction()?.GetValueToFieldType(CPR_RuleCode) ?? nameof(FieldType.Text);

		#endregion

		#endregion

		#region New Properties

		public SharedCusPermitHeader PermitHeader => Factory.Load<SharedCusPermitHeader>(CPR_CPH_PermitHeader);

		[ChildEditable(true)]
		public CusPermitRuleExceptionCollection CusPermitRuleExceptions
		{
			get
			{
				if (cusPermitRuleExceptions == null)
				{
					cusPermitRuleExceptions = CreateNewCusPermitRuleExceptionCollection();
					RegisterEditableChildObject(cusPermitRuleExceptions);
				}
				return cusPermitRuleExceptions;
			}
		}
		CusPermitRuleExceptionCollection cusPermitRuleExceptions;

		protected virtual CusPermitRuleExceptionCollection CreateNewCusPermitRuleExceptionCollection()
		{
			return new CusPermitRuleExceptionCollection(this);
		}

		public IComparer ValueComparer => valueComparer ?? (PermitHeader?.GetCountrySpecificInstruction()?.GetRangeComparer(CPR_RuleCode));
		IComparer valueComparer;

		protected virtual bool CPR_ValueTo_ReadOnly => IsSingleValueRule;

		public virtual bool IsSingleValueRule => PermitHeader?.GetCountrySpecificInstruction()?.GetMatchingType(CPR_RuleCode) == PermitMatchingType.SingleValue;

		#endregion

		#region Business Object Overrides

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("EF593AD5-1260-403B-B7FA-688CD24E2B89", "{3} {0}: {1} - {2}", CPR_RuleCode, CPR_ValueFrom, CPR_ValueTo, ShortName).Trim(); }
		}

		public abstract ZString ShortName { get; }

		#endregion

		#region Implementation

		public bool MatchesValue(ZString value)
		{
			return IsSingleValueRule ? IsValueEquals(value) : IsValueInRange(value);
		}

		public bool IsValueEquals(ZString value)
		{
			return ValueComparer.Compare(CPR_ValueFrom, value) == 0;
		}

		public bool IsValueInRange(ZString value)
		{
			var valueComparer = ValueComparer;
			return valueComparer.Compare(CPR_ValueFrom, value) <= 0 &&
					valueComparer.Compare(CPR_ValueTo, value) >= 0 &&
					!CusPermitRuleExceptions.Any(exception =>
					{
						return valueComparer.Compare(exception.CPE_ValueFrom, value) <= 0 &&
						valueComparer.Compare(exception.CPE_ValueTo, value) >= 0;
					});
		}

		public override void Delete()
		{
			CusPermitRuleExceptions.DeleteAll();
			base.Delete();
		}

		#endregion

		#region Bindings for Visibility of ValueFrom Edit

		public ZBool ValueFromFieldIsTextCodeFindBox => CPR_ValueFrom_FieldType == nameof(FieldType.TextCodeFindBox);

		public ZBool ValueFromFieldIsTextDropEdit => CPR_ValueFrom_FieldType == nameof(FieldType.TextDropEdit);

		public ZBool ValueFromFieldIsOthers => !ValueFromFieldIsTextCodeFindBox && !ValueFromFieldIsTextDropEdit;

		#endregion

		#region Bindings for Visibility of ValueOf Edit

		public ZBool ValueToFieldIsTextDropEdit => CPR_ValueTo_FieldType == nameof(FieldType.TextDropEdit);

		public ZBool ValueToFieldIsOthers => !ValueToFieldIsTextDropEdit;

		#endregion

		ZString GetDefaultValueBasedOnType()
		{
			return CPR_ValueFrom_FieldType == nameof(FieldType.Boolean) ? (ZString)YesNoList.Codes.No : ZString.Empty;
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			CPR_RuleCode = "ABC";
			CPR_ValueFrom = "DEF";
		}
#endif

		#region Validation and Lookups

		public new SharedCusPermitRuleLookups Lookups => (SharedCusPermitRuleLookups)base.Lookups;

		public new SharedCusPermitRuleValidation Validation => (SharedCusPermitRuleValidation)base.Validation;

		#endregion Validation and Lookups
	}
}
