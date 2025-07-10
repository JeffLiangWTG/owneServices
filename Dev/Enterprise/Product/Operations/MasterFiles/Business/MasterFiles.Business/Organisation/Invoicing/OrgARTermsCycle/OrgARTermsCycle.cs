using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgARTermsCycle : AutoOrgARTermsCycle
	{
		#region Schema

		public new class Schema : AutoOrgARTermsCycle.Schema
		{
			public const string P5_FromDayCalculated = "P5_FromDayCalculated";
		}

		#endregion

		public OrgARTermsCycle(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			P5_ToDay = 31;
			P5_PaymentDay = 1;
		}

		public override void Delete()
		{
			OrgARTerms parentARTerm = null;
			if (!IsDeleted && ARTerms != null && !ARTerms.IsDeleted)
			{
				parentARTerm = ARTerms;
			}

			base.Delete();

			if (parentARTerm != null)
			{
				parentARTerm.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region Properties

		public ZByte P5_FromDayCalculated
		{
			get
			{
				if (p5_FromDayCalculated_cached == null)
				{
					p5_FromDayCalculated_cached = new CachedProperty<ZByte>(Factory, () =>
						{
							ZByte result = P5_ToDay;
							if (ARTerms != null)
							{
								ZByte previousARTermsCycleToDay = (
									from termsCycle in ARTerms.ARTermsCycles
									where termsCycle.P5_ToDay < P5_ToDay
									orderby termsCycle.P5_ToDay descending
									select termsCycle.P5_ToDay)
									.FirstOrDefault();
								result = previousARTermsCycleToDay == ZByte.Zero ? ARTerms.ARTermsCycles.Max(termsCycle => termsCycle.P5_ToDay) : previousARTermsCycleToDay;
							}
							result++;
							return result > 31 ? (ZByte)1 : result;
						});
				}

				return p5_FromDayCalculated_cached.Value;
			}
		}
		CachedProperty<ZByte> p5_FromDayCalculated_cached;

		public ZPropertyInfo P5_FromDayCalculatedInfo
		{
			get { return GetZPropertyInfo(Schema.P5_FromDayCalculated); }
		}

		public override ZByte P5_ToDay
		{
			get { return base.P5_ToDay; }
			set
			{
				ZByte oldValue = P5_ToDay;
				base.P5_ToDay = value;

				if (ARTerms != null && oldValue != P5_ToDay)
				{
					foreach (OrgARTermsCycle termsCycle in ARTerms.ARTermsCycles)
					{
						if (termsCycle.PK != PK)
						{
							termsCycle.Validation.ValidateP5_ToDay();
							termsCycle.P5_FromDayCalculatedInfo.RefreshBinding();
						}
					}
				}
			}
		}

		#endregion

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;
			if (ARTerms != null && ARTerms.CompanyData != null && ARTerms.CompanyData.Header != null)
			{
				shouldBeReadOnly = ARTerms.CompanyData.Header.IsInDatabase && !Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed;
			}
			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}
	}
}
