using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentProcessTaskCollection : RoutingSupportProcessTaskCollection, IExceptionDurationSupporter
	{
		readonly ForwardingShipmentProcessTaskConditionChecker conditionChecker;

		public ForwardingShipmentProcessTaskCollection(ForwardingShipment shipment)
			: base(shipment)
		{
			conditionChecker = new ForwardingShipmentProcessTaskConditionChecker(Factory, shipment);
		}

		public new ForwardingShipmentProcessTask this[int index]
		{
			get { return (ForwardingShipmentProcessTask)Elements[index]; }
		}

		public new ForwardingShipmentProcessTask AddNew()
		{
			return (ForwardingShipmentProcessTask)base.AddNew();
		}

		public override ProcessTaskCollection CreateNewCollection()
		{
			return new ForwardingShipmentProcessTaskCollection(Parent);
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, JobShipmentSchema.Constants.Prefix);
			return result;
		}

		#region OriginCountry / DestinationCountry

		public override ZString OriginCountry
		{
			get { return Parent.Origin == null ? ZString.Empty : Parent.Origin.RL_RN_NKCountryCode; }
		}

		public override ZString DestinationCountry
		{
			get { return Parent.Destination == null ? ZString.Empty : Parent.Destination.RL_RN_NKCountryCode; }
		}

		#endregion

		#region IsConditionMet

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override bool IsCondition1Met(ZString conditionCode)
		{
			return conditionChecker.IsCondition1Met(conditionCode);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override bool IsCondition2MetCore(ZString conditionCode, ZString value)
		{
			return conditionChecker.IsCondition2Met(conditionCode, value);
		}

		/// <summary>
		/// Returns true if declaration was made for shipment since last Save (Brokerage tab was clicked)
		/// </summary>
		public override bool HasNewConditionBeenMetSinceLastSave()
		{
			return (Parent.GetDeclaration() != null && !Parent.GetDeclaration().IsInDatabase);
		}

		#endregion

		#region Implementation

		new ForwardingShipment Parent
		{
			get { return (ForwardingShipment)base.Parent; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();

			bool suppressLoadingForNonForwardingShipments = !Parent.IsDeleted && !Parent.JS_IsForwardRegistered;
			result.IsNoResultQuery |= suppressLoadingForNonForwardingShipments;

			return result;
		}

		#endregion
	}
}
