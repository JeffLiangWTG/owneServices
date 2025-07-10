using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolProcessTaskCollection : RoutingSupportProcessTaskCollection, IExceptionDurationSupporter
	{
		public ForwardingConsolProcessTaskCollection(ForwardingConsol consol)
			: base(consol)
		{
		}

		public new ForwardingConsolProcessTask this[int index]
		{
			get { return (ForwardingConsolProcessTask)Elements[index]; }
		}

		public new ForwardingConsolProcessTask AddNew()
		{
			return (ForwardingConsolProcessTask)base.AddNew();
		}

		#region OriginCountry / DestinationCountry

		public override ZString OriginCountry
		{
			get { return Parent.LoadPort == null ? ZString.Empty : Parent.LoadPort.RL_RN_NKCountryCode; }
		}

		public override ZString DestinationCountry
		{
			get { return Parent.DischargePort == null ? ZString.Empty : Parent.DischargePort.RL_RN_NKCountryCode; }
		}

		#endregion

		#region IsConditionMet

		public override bool IsCondition1Met(ZString conditionCode)
		{
			switch (conditionCode)
			{
				case JobConsolWorkflowCondition1CodeList.Codes.Import:
					return Parent.IsImport();
				case JobConsolWorkflowCondition1CodeList.Codes.Export:
					return Parent.IsExport();
				case JobConsolWorkflowCondition1CodeList.Codes.MainTransport:
					return true;
				default:
					return RoutineSupportHelper.IsCondition1Met(conditionCode,
						JobConsolConditionProvider.GetTransportLegsFromLoadToDischarge(Parent).Length,
						JobConsolConditionProvider.Condition1Codes);
			}
		}

		protected override bool IsCondition2MetCore(ZString conditionCode, ZString value)
		{
			switch (conditionCode)
			{
				case JobConsolWorkflowCondition1CodeList.Codes.Import:
					return Parent.IsImport();

				case JobConsolWorkflowCondition1CodeList.Codes.Export:
					return Parent.IsExport();

				case JobConsolWorkflowCondition2CodeList.Codes.ReleaseType:
					return Parent.JK_ReleaseType == value;

				case Core.Constants.ContainerModes.LCL:
					return Core.Constants.ContainerModes.IsLCLType(Parent.JK_ConsolMode);

				default:
					return conditionCode == Parent.JK_ConsolMode;
			}
		}

		#endregion

		#region Implementation

		new ForwardingConsol Parent
		{
			get { return (ForwardingConsol)base.Parent; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();

			bool suppressLoadingForNonForwardingConsols = !Parent.IsDeleted && !Parent.JK_IsForwarding;
			result.IsNoResultQuery |= suppressLoadingForNonForwardingConsols;

			return result;
		}

		#endregion
	}
}
