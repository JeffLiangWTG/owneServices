using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class PreAdviceTransportValidation : JobConsolTransportValidation
	{
		public PreAdviceTransportValidation(Transport transport)
			: base(transport)
		{
		}

		#region JW_LegOrder

		protected override void CheckJW_LegOrder()
		{
			base.CheckJW_LegOrder();
			AddErrorIfDuplicated(Parent.JW_LegOrderInfo, Res.GetString("7739e7e1-6dd4-4a36-8872-d68ba14074a9", "This value must be unique on the pre-advice."), true);
		}

		#endregion

		#region JW_RL_NKLoadPort

		protected override void CheckJW_RL_NKLoadPort()
		{
			base.CheckJW_RL_NKLoadPort();

			if (Parent.JW_TransportMode != Constants.TransportModes.Storage &&
				!Parent.JW_RL_NKLoadPort.IsEmpty && !Parent.JW_RL_NKLoadPortInfo.HasErrors())
			{
				if (PreAdvice != null && Parent.JW_RL_NKLoadPort == PreAdvice.EF_RL_NKPortDisch)
				{
					Parent.JW_RL_NKLoadPortInfo.AddError(Res.GetString("14f69532-e50b-4981-9fdd-5d25fc3f77c2", "A routing leg cannot load at the pre-advice's final discharge."));
				}
			}
		}

		#endregion

		#region JW_RL_NKDiscPort

		protected override void CheckJW_RL_NKDiscPort()
		{
			base.CheckJW_RL_NKDiscPort();

			if (Parent.JW_TransportMode != Constants.TransportModes.Storage &&
				!Parent.JW_RL_NKDiscPort.IsEmpty && !Parent.JW_RL_NKDiscPortInfo.HasErrors())
			{
				if (PreAdvice != null && Parent.JW_RL_NKDiscPort == PreAdvice.EF_RL_NKPortLoad)
				{
					Parent.JW_RL_NKDiscPortInfo.AddError(Res.GetString("8944be1d-7a9a-4824-bbd5-6da1450a7567", "A routing leg cannot discharge at the pre-advice's first load."));
				}
			}
		}

		#endregion

		#region JW_ETD

		protected override void CheckJW_ETD()
		{
			base.CheckJW_ETD();

			if (Parent.JW_ETD.IsValid && PreAdvice != null)
			{
				foreach (Transport transport in Parent.OtherParentTransports)
				{
					// less than 0 if 'this' should be dated before 'transport'
					// 0 if they can be in any order
					// greater than 0 if 'transport' should be dated befour 'this'
					int expectedorder = Parent.JW_LegOrder - transport.JW_LegOrder;

					if (expectedorder > 0 && transport.JW_ETA.IsValid && Parent.JW_ETD < transport.JW_ETA)
					{
						Parent.JW_ETDInfo.AddError(Res.GetString("816794d5-3f49-4d65-bdb4-bf0e5d7b13c0", "The date order does not reflect the leg order."));
						break;
					}
				}
			}
		}

		#endregion

		#region JW_ETA

		protected override void CheckJW_ETA()
		{
			base.CheckJW_ETA();

			if (Parent.JW_ETA.IsValid && PreAdvice != null)
			{
				foreach (Transport transport in Parent.OtherParentTransports)
				{
					// less than 0 if 'this' should be dated before 'transport'
					// 0 if they can be in any order
					// greater than 0 if 'transport' should be dated befour 'this'
					int expectedorder = Parent.JW_LegOrder - transport.JW_LegOrder;

					if (expectedorder < 0 && transport.JW_ETD.IsValid && Parent.JW_ETA > transport.JW_ETD)
					{
						Parent.JW_ETAInfo.AddError(Res.GetString("242b5180-cb18-4bb5-b9f4-66074464b40d", "The date order does not reflect the leg order."));
						break;
					}
				}
			}
		}

		#endregion

		#region Implementation

		void AddErrorIfDuplicated(ZPropertyInfo info, string message, bool includeStorageLegs)
		{
			if (PreAdvice != null)
			{
				foreach (Transport otherTransport in Parent.OtherParentTransports)
				{
					if (info.Value.Equals(otherTransport[info.Name]) && (includeStorageLegs || otherTransport.JW_TransportMode != Constants.TransportModes.Storage))
					{
						info.AddError(message);
						return;
					}
				}
			}
		}

		JobShipmentPreplanning PreAdvice
		{
			get { return Parent.Parent as JobShipmentPreplanning; }
		}

		#endregion
	}
}
