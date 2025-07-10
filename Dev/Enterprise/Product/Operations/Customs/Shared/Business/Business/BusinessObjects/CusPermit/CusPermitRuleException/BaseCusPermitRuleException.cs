using System.Collections;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Business
{
	[DependentBusinessObject(typeof(BaseCusPermitRule), "CusPermitRuleExceptions")]
	public class BaseCusPermitRuleException : AutoCusPermitRuleException
	{
		public BaseCusPermitRuleException(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Override Properties

		[RelatedBusinessObject("PermitRule")]
		public override ZGuid CPE_CPR_PermitRule
		{
			get { return base.CPE_CPR_PermitRule; }
			set { base.CPE_CPR_PermitRule = value; }
		}

		#region CPR_ValueFrom

		[List(nameof(Lookups) + "." + nameof(CusPermitRuleExceptionLookups.CPE_ValueFromList))]
		public override ZString CPE_ValueFrom
		{
			get { return base.CPE_ValueFrom; }
			set
			{
				var hasChanged = CPE_ValueFrom != value;
				base.CPE_ValueFrom = value;
				if (hasChanged && CPE_ValueTo.IsEmpty)
				{
					CPE_ValueTo = CPE_ValueFrom;
				}
			}
		}

		public virtual ZString CPE_ValueFrom_FieldType => PermitRule?.CPR_ValueFrom_FieldType ?? nameof(FieldType.Text);

		#endregion

		#endregion

		#region New Properties

		public SharedCusPermitRule PermitRule => Factory.Load<SharedCusPermitRule>(CPE_CPR_PermitRule);

		public IComparer ValueComparer => PermitRule.ValueComparer;

		public virtual bool CPE_ValueTo_ReadOnly => PermitRule?.CPR_ValueToInfo?.ReadOnly ?? false;

		#endregion

		#region Business Object Overrides

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("9EBFF2A4-6216-4F5D-A81A-B3A319033AB2", "{3} Exception {0}: {1} - {2}", PermitRule?.CPR_RuleCode, CPE_ValueFrom, CPE_ValueTo, PermitRule?.ShortName).Trim(); }
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			CPE_ValueFrom = "EEE";
		}
#endif
	}
}
