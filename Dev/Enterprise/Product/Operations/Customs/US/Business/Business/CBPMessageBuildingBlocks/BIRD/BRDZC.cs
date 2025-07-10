using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.BIRD
{
	partial class BRDZC : Messaging.Business.MessageBuildingBlocks.BIRD.Abstract.BRDZC, IBIRDHeaderRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			CreateContainerIfNecessary(declaration, ContainerNumber);
			CreateContainerIfNecessary(declaration, ContainerNumber2);
			CreateContainerIfNecessary(declaration, ContainerNumber3);
			CreateContainerIfNecessary(declaration, ContainerNumber4);
		}

		void CreateContainerIfNecessary(JobDeclaration declaration, ZString containerNumber)
		{
			if (!containerNumber.IsEmpty)
			{
				declaration.CusContainers.FindOrCreate(containerNumber);
			}
		}

		#endregion
	}
}
