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
	public class OrgARPaymentCycle : AutoOrgARTermsCycle
	{
		#region Schema

		public new class Schema : AutoOrgARTermsCycle.Schema
		{
			public const string P5_Cycle = "P5_Cycle";
		}

		#endregion

		public OrgARPaymentCycle(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			P5_ToDay = 1;
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

		protected override OrgARTermsCycleValidation GetNewValidation()
		{
			return new OrgARPaymentCycleValidation(this);
		}

		#endregion

		#region Properties

		public ZByte P5_Cycle
		{
			get { return P5_ToDay; }
		}

		public ZPropertyInfo P5_CycleInfo
		{
			get { return GetZPropertyInfo(Schema.P5_Cycle); }
		}

		public override ZByte P5_PaymentDay
		{
			get { return base.P5_PaymentDay; }
			set
			{
				ZByte oldValue = P5_PaymentDay;
				base.P5_PaymentDay = value;

				if (ARTerms != null && (oldValue != P5_PaymentDay || (oldValue == 1 && P5_ToDay == 1))) //so making a new row of 1 will reshuffle
				{
					foreach (OrgARPaymentCycle paymentCycle in ARTerms.ARPaymentCycles)
					{
						paymentCycle.Validation.ValidateP5_PaymentDay();

						var cycle = (ZByte)(ARTerms.ARPaymentCycles.Count(x => x.P5_PaymentDay < paymentCycle.P5_PaymentDay) + 1);
						if (paymentCycle.P5_ToDay != cycle)
						{
							paymentCycle.P5_ToDay = cycle;
							paymentCycle.P5_ToDayInfo.RefreshBinding();
							paymentCycle.P5_CycleInfo.RefreshBinding();
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
