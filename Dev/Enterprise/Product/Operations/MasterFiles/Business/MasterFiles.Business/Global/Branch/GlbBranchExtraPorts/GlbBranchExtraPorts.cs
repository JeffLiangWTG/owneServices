using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbBranchExtraPorts : AutoGlbBranchExtraPorts, IGlbBranchExtraPort
	{
		public GlbBranchExtraPorts(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties
		[List("Lookups.AdditionalBranchRelatedPorts")]
		public override ZString GY_RL_NKAdditionalBranchRelatedPort
		{
			get
			{
				return base.GY_RL_NKAdditionalBranchRelatedPort;
			}
			set
			{
				bool hasChanged = base.GY_RL_NKAdditionalBranchRelatedPort != value;
				base.GY_RL_NKAdditionalBranchRelatedPort = value;
				if (hasChanged)
				{
					Branch.DefaultPorts.MarkAsNeedingValidation();
				}
			}
		}
		#endregion

		#region Port Name

		public ZString PortName
		{
			get { return AdditionalBranchRelatedPort != null ? AdditionalBranchRelatedPort.RL_PortName : ZString.Empty; }
		}

		public ZPropertyInfo PortNameInfo
		{
			get { return GetZPropertyInfo(nameof(PortName)); }
		}

		public ZBool GY_IsValid
		{
			get
			{
				return new ZBool(GetValueFromRowSafely(GlbBranchExtraPortsSchema.GY_IsValid));
			}
			set
			{
				var gY_IsValidInfo = GetZPropertyInfo(GlbBranchExtraPortsSchema.Constants.GY_IsValid);
				SetPropertyValue(gY_IsValidInfo, value);
			}
		}
		#endregion
	}
}
