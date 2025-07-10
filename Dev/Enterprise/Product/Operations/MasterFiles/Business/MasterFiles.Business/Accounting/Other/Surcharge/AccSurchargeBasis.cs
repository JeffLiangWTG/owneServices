using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(AccSurchargeConfiguration), "AccSurchargeBasises")]
	public class AccSurchargeBasis : AutoAccSurchargeBasis
	{
		public AccSurchargeBasis(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public virtual AccSurchargeConfiguration SurchargeConfiguration
		{
			get { return (AccSurchargeConfiguration)Factory.Load(typeof(AccSurchargeConfiguration), ASB_ASC_SurchargeConfiguration); }
		}

		#region ASB_ChargeGroup

		[List("Lookups.ChargeGroupList")]
		public override ZString ASB_ChargeGroup
		{
			get { return base.ASB_ChargeGroup; }
			set { base.ASB_ChargeGroup = value; }
		}

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			var accSurchargeConfiguration = Factory.NewWithValidTestData<AccSurchargeConfiguration>();

			base.FillWithValidTestDataCore(kind, propertyPath);

			ASB_ASC_SurchargeConfiguration = accSurchargeConfiguration.PK;
		}

#endif
	}
}
