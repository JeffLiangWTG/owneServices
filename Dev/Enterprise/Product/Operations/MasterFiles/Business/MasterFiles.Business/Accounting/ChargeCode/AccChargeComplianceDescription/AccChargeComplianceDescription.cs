using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeComplianceDescription : AutoAccChargeComplianceDescription, ITransportModeConfiguration, ISupplyTypeConfiguration, IDuplicateValidationItem<AccChargeComplianceDescription>
	{
		public AccChargeComplianceDescription(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		ZString IJobTypeConfiguration.JobTypeCode => ADE_JobType;

		ZPropertyInfo IJobTypeConfiguration.JobTypeCodeInfo => ADE_JobTypeInfo;

		ZPropertyInfo ITransportModeConfiguration.TransportModeInfo => ADE_TransportModeInfo;

		ZString ISupplyTypeConfiguration.SupplyTypeCode => ADE_SupplyType;

		ZPropertyInfo ISupplyTypeConfiguration.SupplyTypeCodeInfo => ADE_SupplyTypeInfo;

		#region Properties

		#region ADE_JobType

		[List("Lookups.JobTypeList")]
		public override ZString ADE_JobType
		{
			get
			{
				return base.ADE_JobType;
			}
			set
			{
				base.ADE_JobType = value;
				GetTransportModeHelper().JobTypeSetterLogic();
			}
		}

		#endregion

		#region ADE_SupplyType

		[List("Lookups.SupplyTypeList")]
		public override ZString ADE_SupplyType
		{
			get
			{
				return base.ADE_SupplyType;
			}
			set
			{
				base.ADE_SupplyType = value;
			}
		}

		#endregion

		#region ADE_TransportMode

		[List("Lookups.TransportModeList")]
		[ReadOnlyMember(nameof(ADE_TransportModeIsReadOnly))]
		public override ZString ADE_TransportMode { get => base.ADE_TransportMode; set => base.ADE_TransportMode = value; }
		bool ADE_TransportModeIsReadOnly => GetTransportModeHelper().GetReadOnlyStatus();

		#endregion

		#endregion

		bool IDuplicateValidationItem<AccChargeComplianceDescription>.IsDuplicated(AccChargeComplianceDescription anotherItem)
		{
			return PK != anotherItem.PK
				&& ADE_JobType == anotherItem.ADE_JobType
				&& ADE_TransportMode == anotherItem.ADE_TransportMode
				&& ADE_SupplyType == anotherItem.ADE_SupplyType;
		}

		void IDuplicateValidationItem<AccChargeComplianceDescription>.AddRowError(string message) => AddRowError(message);

		void IDuplicateValidationItem<AccChargeComplianceDescription>.RemoveRowError(string message) => RemoveRowError(message);

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			if (ADE_AC.IsEmpty)
			{
				ADE_AC = Factory.New<AccChargeCode>().PK;
			}
			ADE_JobType = "SHP";
			ADE_TransportMode = "AIR";
			ADE_SupplyType = "LOC";
			ADE_Description = "TestDescription1234!@#$";

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif

		ITransportModeConfigurationHelper GetTransportModeHelper() => ObjectFactory.Get<IJobConfigurationHelperFactory>().GetTransportModeHelper(this);
	}
}
