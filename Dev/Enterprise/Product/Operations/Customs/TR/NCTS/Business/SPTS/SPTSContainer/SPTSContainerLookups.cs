using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSContainerLookups : Customs.Business.CusInBondContainerLookups
	{
		public SPTSContainerLookups(SPTSContainer parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ContainersList
		{
			get
			{
				var containerList = new CodeDescriptionPairList();
				var headerContainers = Parent.SPTSBill?.Header?.HeaderContainers;
				{
					if (headerContainers != null)
					{
						foreach (var cont in headerContainers)
						{
							var containerNum = cont.FindPropertyInfo(CusInBondContainerSchema.BC_ContainerNum.ObjectName);
							containerList.AddPair(containerNum.Value.ToString());
						}
					}
				}
				return containerList;
			}
		}
		protected new SPTSContainer Parent => (SPTSContainer)base.Parent;
	}
}
