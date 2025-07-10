using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class ContainerProcessTaskCollection : RoutingSupportProcessTaskCollection, IExceptionDurationSupporter
	{
		public ContainerProcessTaskCollection(CommonContainer container) : base(container)
		{
		}

		public new ContainerProcessTask this[int index]
		{
			get { return (ContainerProcessTask)Elements[index]; }
		}

		public new ContainerProcessTask AddNew()
		{
			return (ContainerProcessTask)base.AddNew();
		}

		public override ProcessTaskCollection CreateNewCollection()
		{
			return new ContainerProcessTaskCollection(Parent);
		}

		#region OriginCountry / DestinationCountry

		public override ZString OriginCountry
		{
			get
			{
				ZString origin = ZString.Empty;

				if (Parent.ContainerParent != null && Parent.ContainerParent.LoadPort != null)
				{
					origin = Parent.ContainerParent.LoadPort.RL_RN_NKCountryCode;
				}

				return origin;
			}
		}

		public override ZString DestinationCountry
		{
			get
			{
				ZString destination = ZString.Empty;

				if (Parent.ContainerParent != null && Parent.ContainerParent.DischargePort != null)
				{
					destination = Parent.ContainerParent.DischargePort.RL_RN_NKCountryCode;
				}

				return destination;
			}
		}

		#endregion

		new CommonContainer Parent
		{
			get { return (CommonContainer)base.Parent; }
		}

		#region IsConditionMet

		public override bool IsCondition1Met(ZString conditionCode)
		{
			return RoutineSupportHelper.IsCondition1Met(conditionCode,
				ContainerWorkflowDescriptor.GetTransportLegsFromLoadToDischarge(Parent).Length,
				ContainerWorkflowDescriptor.Condition1Codes);
		}

		#endregion

		public override bool HasNewConditionBeenMetSinceLastSave()
		{
			return Parent.IsInDatabase && Parent.IsConsolContainer && (Parent.Consol?.Transports.HasChanges ?? false);
		}
	}
}
