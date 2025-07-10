using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CusContainer : TypeSafeCusContainer, ICusContainer, Integration.Customs.SG.ICusContainer
	{
		public CusContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CO_WeightUQ = Core.Constants.Weight.Tonnes;
		}

		public override ZGuid CO_RC
		{
			get { return base.CO_RC; }
			set
			{
				if (base.CO_RC != value)
				{
					base.CO_RC = value;
					if (Container != null && !IsCopying)
					{
						if (ContainerLengthValidForSG(Container))
						{
							CO_ContainerSize = Container.RC_Length.ToString(0);
						}
					}
				}
			}
		}

		public override ZBool UseContainerSize => true;

		#region ICusContainer Members

		ZString ICusContainer.ContainerNumber
		{
			get { return CO_ContainerNumber; }
		}

		ZString ICusContainer.ContainerType
		{
			get { return CO_FCL_LCL_AIR; }
		}

		ZInt ICusContainer.ContainerSize
		{
			get
			{
				ZInt result = 0;
				ZInt.TryParse(CO_ContainerSize, out result);
				return result;
			}
		}

		ZDecimal ICusContainer.ContainerWeight
		{
			get { return CO_Weight; }
		}

		ZString ICusContainer.ContainerWeightUnit
		{
			get { return CO_WeightUQ; }
		}

		ZString ICusContainer.SealNumber
		{
			get { return CO_Seal; }
		}

		#endregion

		bool ContainerLengthValidForSG(RefContainer container)
		{
			return container.RC_Length == 20
				|| container.RC_Length == 40
				|| container.RC_Length == 45;
		}
	}
}
