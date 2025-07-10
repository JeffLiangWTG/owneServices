using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeApportionmentMethodOverride : AutoAccChargeApportionmentMethodOverride, IApportionmentMethodOverride
	{
		public AccChargeApportionmentMethodOverride(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[MaxLength(3)]
		[List("Lookups.ApportionmentList")]
		public override ZString AAM_ApportionmentMethod
		{
			get => base.AAM_ApportionmentMethod;
			set => base.AAM_ApportionmentMethod = value;
		}

		[MaxLength(3)]
		[List("Lookups.ConsolTypeList")]
		public override ZString AAM_ConsolType
		{
			get => base.AAM_ConsolType;
			set => base.AAM_ConsolType = value;
		}

		[MaxLength(3)]
		[List("Lookups.ContainerModeList")]
		public override ZString AAM_ContainerMode
		{
			get => base.AAM_ContainerMode;
			set => base.AAM_ContainerMode = value;
		}

		[MaxLength(3)]
		[List("Lookups.DirectionList")]
		public override ZString AAM_Direction
		{
			get => base.AAM_Direction;
			set => base.AAM_Direction = value;
		}

		[MaxLength(3)]
		[List("Lookups.ModuleList")]
		public override ZString AAM_Module
		{
			get { return base.AAM_Module; }
			set
			{
				base.AAM_Module = value;

				if (value == ApportionmentMethodModules.TransportBooking)
				{
					AAM_ConsolType = ApportionmentMethod.AllCode;
					AAM_Direction = ApportionmentMethod.AllCode;
				}
				else if (value == ApportionmentMethodModules.TransitWarehouse)
				{
					AAM_ConsolType = ApportionmentMethod.AllCode;
					AAM_Direction = ApportionmentMethod.AllCode;
					AAM_TransportMode = ApportionmentMethod.AllCode;
					AAM_ContainerMode = ApportionmentMethod.AllCode;
				}
			}
		}

		[MaxLength(3)]
		[List("Lookups.TransportModeList")]
		public override ZString AAM_TransportMode
		{
			get => base.AAM_TransportMode;
			set => base.AAM_TransportMode = value;
		}

		public bool IsDuplicateOf(AccChargeApportionmentMethodOverride apportionmentMethodOverride)
		{
			return AAM_AC == apportionmentMethodOverride.AAM_AC
				&& AAM_Module == apportionmentMethodOverride.AAM_Module
				&& AAM_ConsolType == apportionmentMethodOverride.AAM_ConsolType
				&& AAM_Direction == apportionmentMethodOverride.AAM_Direction
				&& AAM_TransportMode == apportionmentMethodOverride.AAM_TransportMode
				&& AAM_ContainerMode == apportionmentMethodOverride.AAM_ContainerMode;
		}

		ZString IApportionmentMethodOverride.Module => AAM_Module;

		ZString IApportionmentMethodOverride.ConsolType => AAM_ConsolType;

		ZString IApportionmentMethodOverride.ApportionmentMethod => AAM_ApportionmentMethod;

		ZString IApportionmentMethodOverride.ContainerMode => AAM_ContainerMode;

		ZString IApportionmentMethodOverride.TransportMode => AAM_TransportMode;

		ZString IApportionmentMethodOverride.Direction => AAM_Direction;

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AAM_ApportionmentMethod = AllocationMethod.Manual;
		}

#endif
	}
}
