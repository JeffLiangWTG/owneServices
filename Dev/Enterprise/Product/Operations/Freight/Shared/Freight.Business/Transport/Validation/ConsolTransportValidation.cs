using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;

namespace Enterprise.Freight.Business
{
	public class ConsolTransportValidation : JobConsolTransportValidation
	{
		public ConsolTransportValidation(Transport transport)
			: base(transport)
		{
		}

		#region JW_LegOrder

		protected override void CheckJW_LegOrder()
		{
			base.CheckJW_LegOrder();
			AddErrorIfDuplicated(Parent.JW_LegOrderInfo, Res.GetString("424cac92-d68a-4ba6-9601-91c3991e0d4b", "This value must be unique on the consol."), true);
		}

		#endregion

		#region JW_RL_NKLoadPort

		protected override void CheckJW_RL_NKLoadPort()
		{
			base.CheckJW_RL_NKLoadPort();
			if (!Parent.JW_IsLinked || Parent.Sailing == null)
			{
				ConsolTransportValidationHelper.CheckLoadPort(Parent.JW_RL_NKLoadPortInfo, Parent);
			}
		}

		#endregion

		#region JW_RL_NKDiscPort

		protected override void CheckJW_RL_NKDiscPort()
		{
			base.CheckJW_RL_NKDiscPort();
			if (!Parent.JW_IsLinked || Parent.Sailing == null)
			{
				ConsolTransportValidationHelper.CheckDiscPort(Parent.JW_RL_NKDiscPortInfo, Parent);
			}
		}

		#endregion

		#region JW_IsCargoOnly

		protected override void CheckJW_IsCargoOnly()
		{
			base.CheckJW_IsCargoOnly();

			if (Parent.JW_TransportMode != Core.Constants.TransportModes.Air)
			{
				return;
			}

			if (Consol == null)
			{
				return;
			}

			var thereAreDGsLinkedToConsolThatAreForbiddenOnPassengerAircraft = Consol.Shipments.OfType<CommonShipment>().Any(shipment => shipment.HasDangerousGoodsSubstancesForbiddenOnPassengerAircraft());

			if (!Parent.JW_IsCargoOnly && thereAreDGsLinkedToConsolThatAreForbiddenOnPassengerAircraft)
			{
				Parent.JW_IsCargoOnlyInfo.AddError(Res.GetString("5FABAE9C-B449-436B-87B2-1672F826CD65", "This Consol has Shipments attached with Dangerous Goods substances that are forbidden on a passenger flight."));
			}
		}

		#endregion

		#region Check Estimated And Actual Dates

		protected override void CheckJW_ETD()
		{
			base.CheckJW_ETD();
			if (!Parent.JW_IsLinked || Parent.Sailing == null)
			{
				ConsolTransportValidationHelper.CheckETD(Parent.JW_ETDInfo, Parent);
			}
		}

		protected override void CheckJW_ETA()
		{
			base.CheckJW_ETA();
			if (!Parent.JW_IsLinked || Parent.Sailing == null)
			{
				ConsolTransportValidationHelper.CheckETA(Parent.JW_ETAInfo, Parent);
			}
		}

		protected override void CheckJW_ATD()
		{
			base.CheckJW_ATD();
			if (!Parent.JW_IsLinked || Parent.Sailing == null)
			{
				ConsolTransportValidationHelper.CheckATD(Parent.JW_ATDInfo, Parent);
			}
		}

		protected override void CheckJW_ATA()
		{
			base.CheckJW_ATA();
			if (!Parent.JW_IsLinked || Parent.Sailing == null)
			{
				ConsolTransportValidationHelper.CheckATA(Parent.JW_ATAInfo, Parent);
			}
		}

		#endregion

		#region Implementation

		void AddErrorIfDuplicated(ZPropertyInfo info, string message, bool includeStorageLegs)
		{
			if (Consol != null)
			{
				foreach (Transport otherTransport in ConsolTransports)
				{
					if (
						otherTransport.PK != Parent.PK && info.Value.Equals(otherTransport[info.Name]) &&
						(includeStorageLegs || otherTransport.JW_TransportMode != Constants.TransportModes.Storage)
						)
					{
						info.AddError(message);
						return;
					}
				}
			}
		}

		protected CommonConsol Consol
		{
			get { return Parent.GetParentSafe() as CommonConsol; }
		}

		ConsolTransportCollection ConsolTransports
		{
			get { return consolTransports ?? (consolTransports = Consol.Transports); }
		}

		ConsolTransportCollection consolTransports;

		#endregion
	}
}
