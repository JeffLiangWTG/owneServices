using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Business
{
	public class TransportAirValidation : TransportValidation
	{
		public TransportAirValidation(Transport transport)
			: base(transport)
		{
		}

		#region JW_VoyageFlight

		protected override void CheckJW_VoyageFlight()
		{
			base.CheckJW_VoyageFlight();

			if (!Parent.JW_VoyageFlightInfo.HasErrors() && !Parent.JW_IsCharter && (Parent.JW_IsLinked || !Parent.JW_VoyageFlight.IsEmpty))
			{
				if (Parent.JW_VoyageFlight.IsEmpty)
				{
					Parent.JW_VoyageFlightInfo.AddError(Res.GetString("c9287252-c1fc-4902-a596-e6c1f90ff992", "Please enter a flight number."));
				}
				else if (!FlightCodeValidator.IsValid(Parent.JW_VoyageFlight))
				{
					if (Parent.JW_IsLinked && !Parent.IsDomestic)
					{
						Parent.JW_VoyageFlightInfo.AddError(Res.GetString("57da6578-1071-4e7d-b78c-70de2faee13d", "Flight number is not valid."));
					}
					else
					{
						Parent.JW_VoyageFlightInfo.AddWarning(Res.GetString("90b72516-8c22-41a6-b19b-9b8d41c36977", "Flight number is not valid."));
					}
				}
			}

			if (!Parent.IsValidationSuspended && IsFlightNumberDifferentForTemplateRecord())
			{
				Parent.JW_VoyageFlightInfo.AddWarning(Res.GetString("cfd79cb8-11d6-4e54-9b92-78d911d9494f", "If the flight number is populated on the template, it may only be selected for that flight."));
			}
		}

		bool IsFlightNumberDifferentForTemplateRecord()
		{
			if (Parent.Parent is IForwardingConsol
				&& Parent.Parent is ITemplateRecordProvider provider
				&& !provider.IsTemplateRecord
				&& provider.TemplateRecord != null)
			{
				var consol = Parent.Parent as CommonConsol;
				var consolProvider = consol.TemplateRecordProviderConsol;

				if (consolProvider != null
					&& consolProvider.Transports.Count == 1
					&& consol.Transports.Count > 0
					&& !consolProvider.Transports[0].JW_VoyageFlight.IsEmpty
					&& consolProvider.Transports[0].JW_VoyageFlight != Parent.JW_VoyageFlight)
				{
					if (consol.Transports.Count == 1)
					{
						return true;
					}

					var providerTransport = consolProvider.Transports[0];
					var transports = consol.Transports.OfType<Transport>().OrderBy(t => t.JW_LegOrder).ToArray();
					return (transports[0].PK == Parent.PK && Parent.JW_RL_NKLoadPort == providerTransport.JW_RL_NKLoadPort)
						|| (transports.Last().PK == Parent.PK && Parent.JW_RL_NKDiscPort == providerTransport.JW_RL_NKDiscPort);
				}
			}

			return false;
		}

		#endregion

		#region IsDomestic

		protected override void CheckIsDomestic()
		{
			base.CheckIsDomestic();
			ValidateJW_VoyageFlight();
		}

		#endregion

		#region Check ETD, ETA, ATD, ATA

		protected override void CheckETDvsETA()
		{
			if (!Parent.JW_ETDInfo.HasErrors() && Parent.JW_ETD.IsValid && Parent.JW_ETA.IsValid)
			{
				if (Parent.JW_ETD > Parent.JW_ETA.AddDays(1))
				{
					Parent.JW_ETDInfo.AddNotification(notificationType, Res.GetString("de225dfc-08fc-4b1a-a369-b5e10c9ba9b2", "ETD cannot be more than a day after ETA."));
				}
			}
		}

		protected override void CheckETAvsETD()
		{
			if (!Parent.JW_ETAInfo.HasErrors() && Parent.JW_ETA.IsValid && Parent.JW_ETD.IsValid)
			{
				if (Parent.JW_ETA < Parent.JW_ETD.AddDays(-1))
				{
					Parent.JW_ETAInfo.AddNotification(notificationType, Res.GetString("863f10b7-0d42-4b5f-8514-1e1d7f6bf6db", "ETA cannot be more than a day before ETD."));
				}
			}
		}

		protected override void CheckATDvsATA()
		{
			if (!Parent.JW_ATDInfo.HasErrors() && Parent.JW_ATA.IsValid)
			{
				if (Parent.JW_ATD > Parent.JW_ATA.AddDays(1))
				{
					Parent.JW_ATDInfo.AddNotification(notificationType, Res.GetString("825f2f5a-6fee-4855-87a0-9e2c345d70c4", "ATD cannot be more than a day after ATA."));
				}
			}
		}

		protected override void CheckATAvsATD()
		{
			if (!Parent.JW_ATAInfo.HasErrors() && Parent.JW_ATD.IsValid)
			{
				if (Parent.JW_ATA < Parent.JW_ATD.AddDays(-1))
				{
					Parent.JW_ATAInfo.AddNotification(notificationType, Res.GetString("7488f236-6587-4744-89db-51423b84dfbe", "ATA cannot be more than a day before ATD."));
				}
			}
		}

		#endregion

		#region CheckJW_STD

		protected override void CheckJW_STD()
		{
			base.CheckJW_STD();

			if (!Parent.JW_STDInfo.HasErrors() && Parent.JW_ETD.IsValid && Parent.JW_STD.IsValid)
			{
				if (Parent.JW_STD > Parent.JW_ETD && Parent.JW_ETD.AddDays(1) < Parent.JW_STD)
				{
					Parent.JW_STDInfo.AddWarning(Res.GetString("791c523f-dfe1-477a-8086-39097418cb29", "STD is more than a day after ETD"));
				}
				else if (Parent.JW_STD < Parent.JW_ETD && Parent.JW_ETD.AddDays(-1) > Parent.JW_STD)
				{
					Parent.JW_STDInfo.AddWarning(Res.GetString("a7033ae4-13ed-4e79-bc97-aaa5bd7c7391", "ETD is more than a day after STD"));
				}
			}
		}

		#endregion

		#region ValidateScheduleArrivalDate

		protected override void CheckJW_STA()
		{
			base.CheckJW_STA();

			if (!Parent.JW_STAInfo.HasErrors() && Parent.JW_ETA.IsValid && Parent.JW_STA.IsValid)
			{
				if (Parent.JW_STA > Parent.JW_ETA && Parent.JW_ETA.AddDays(1) < Parent.JW_STA)
				{
					Parent.JW_STAInfo.AddWarning(Res.GetString("69e9966c-ec01-4172-bdd6-fd9ec30a84be", "STA is more than a day after ETA"));
				}
				else if (Parent.JW_STA < Parent.JW_ETA && Parent.JW_ETA.AddDays(-1) > Parent.JW_STA)
				{
					Parent.JW_STAInfo.AddWarning(Res.GetString("8f0dc1d6-f904-45e4-a6a3-24cf5eff76f0", "ETA is more than a day after STA"));
				}
			}
		}

		#endregion

		#region JW_JX_JV_RegistrationNo_NoSailing

		protected override void CheckJW_JX_JV_RegistrationNo_NoSailing()
		{
			base.CheckJW_JX_JV_RegistrationNo_NoSailing();

			if (Parent.JW_IsLinked && Parent.JW_IsCharter && Parent.JW_JX_JV_RegistrationNo.IsEmpty)
			{
				Parent.JW_JX_JV_RegistrationNoInfo.AddError(Res.GetString("b2965879-57f8-488d-bb55-7c0713f9340d", "Please enter an aircraft registration number."));
			}
		}

		#endregion

		#region JW_OnlineScheduleStatus

		protected override void CheckJW_OnlineScheduleStatus()
		{
			base.CheckJW_OnlineScheduleStatus();

			foreach (var warning in OnlineFlightMatchingValidationHelper.GetOnlineFlightMatchStatusWarnings(Parent))
			{
				Parent.JW_OnlineScheduleStatusInfo.AddWarning(warning);
			}
		}

		#endregion
	}
}
