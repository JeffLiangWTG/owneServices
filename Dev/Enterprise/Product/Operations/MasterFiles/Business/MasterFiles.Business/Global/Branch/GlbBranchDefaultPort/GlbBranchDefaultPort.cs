using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class GlbBranchDefaultPort : AutoGlbBranchDefaultPort
	{
		public GlbBranchDefaultPort(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[List("Lookups.DefaultToList")]
		public override ZString GBP_DefaultTo
		{
			get { return base.GBP_DefaultTo; }
			set
			{
				bool hasChanged = base.GBP_DefaultTo != value;
				base.GBP_DefaultTo = value;
				if (hasChanged)
				{
					MarkAsNeedingValidation();
				}
			}
		}

		[List("Lookups.TransportModeList")]
		public override ZString GBP_TransportMode
		{
			get { return base.GBP_TransportMode; }
			set
			{
				bool hasChanged = base.GBP_DefaultTo != value;
				base.GBP_TransportMode = value;
				if (hasChanged)
				{
					MarkAsNeedingValidation();
				}
			}
		}

		[List("Lookups.ContainerModeList")]
		public override ZString GBP_ContainerMode
		{
			get { return base.GBP_ContainerMode; }
			set { base.GBP_ContainerMode = value; }
		}

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			GBP_DefaultTo = GlbBranchDefaultToList.Codes.ShipmentOrigin;
			GBP_TransportMode = Core.Constants.TransportModes.All;
			GBP_ContainerMode = "All";
		}

#endif

		public bool EqualsDefaultCombination(GlbBranchDefaultPort defaultPort)
		{
			return defaultPort != null
				&& GBP_DefaultTo == defaultPort.GBP_DefaultTo
				&& GBP_TransportMode == defaultPort.GBP_TransportMode
				&& GBP_ContainerMode == defaultPort.GBP_ContainerMode;
		}
	}
}
