using System;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Res = Enterprise.Freight.SailingScheduleDataVendor.Res;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in AU-NZ and DE sailing schedule imports")]
	internal class SailingScheduleFeedDataVendor : Freight.Business.SailingScheduleDataVendor, Integration.SailingDataVendor.IOneStopSailingScheduleDataVendor
	{
		protected override bool IsEnabledCore { get { return !FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived.IsEmpty; } }

		protected override bool IsVendorDataCurrentCore
		{
			get { return FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived > ZDateTime.Now.AddHours(-48); }
		}

		public override string Status
		{
			get
			{
				string result;
				if (FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived.IsEmpty)
				{
					result = Res.GetString("SailingScheduleDataVendor.Status1", "Sailing schedule feed not active. Contact CargoWise.");
				}
				else if (!IsVendorDataCurrent)
				{
					result = Res.GetString("SailingScheduleDataVendor.Status2", "Sailing schedule feed not current ({0})", FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived.ToShortDateString());
				}
				else
				{
					var difference = Convert.ToInt16((ZDateTime.Now - FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived).TotalHours);

					result = difference == 1 ? Res.GetString("SailingScheduleDataVendor.Status3", "Sailing schedule feed up to date (rcvd. {0} hr ago)", difference) :
						Res.GetString("SailingScheduleDataVendor.Status4", "Sailing schedule feed up to date (rcvd. {0} hrs ago)", difference);
				}

				return result;
			}
		}

		#region Voyage Origins

		protected override void UpdateVoyageOriginCore(VoyageOrigin origin)
		{
			var port = new VesselRoutingPort.Loader(origin.Factory).Load(origin, GetLineOperator(origin.Voyage));

			if (port != null && origin.Voyage != null)
			{
				if (!port.E7_ETD.IsEmpty)
				{
					origin.JA_E_DEP = port.E7_ETD;
				}

				if (!port.E7_ATD.IsEmpty)
				{
					origin.JA_A_DEP = port.E7_ATD;
				}

				if (!port.E7_CargoCutOff.IsEmpty)
				{
					origin.JA_CutOff = port.E7_CargoCutOff;
				}

				if (!port.E7_ExportReceivalCommencementDate.IsEmpty)
				{
					origin.JA_ReceivalCommences = port.E7_ExportReceivalCommencementDate;
				}
			}
		}

		#endregion

		#region Voyage Destinations

		protected override void UpdateVoyageDestinationCore(VoyageDestination destination)
		{
			var port = new VesselRoutingPort.Loader(destination.Factory).Load(destination, GetLineOperator(destination.Voyage));

			if (port != null && destination.Voyage != null)
			{
				if (!port.E7_ETA.IsEmpty)
				{
					destination.JB_E_ARV = port.E7_ETA;
				}

				if (!port.E7_ATA.IsEmpty)
				{
					destination.JB_A_ARV = port.E7_ATA;
				}

				if (!port.E7_ImportAvailability.IsEmpty)
				{
					destination.JB_AvailabilityDate = port.E7_ImportAvailability;
				}

				if (!port.E7_ImportStorageCommences.IsEmpty)
				{
					destination.JB_StorageDate = port.E7_ImportStorageCommences;
				}
			}
		}

		#endregion

		#region Implementation

		protected virtual ZString GetLineOperator(JobVoyage voyage)
		{
			var lineOperator = ZString.Empty;

			if (voyage != null && voyage.Line != null)
			{
				lineOperator = voyage.Line.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.OneStopCode, Core.Constants.CountryCodes.Australia);
			}

			return lineOperator;
		}

		#endregion
	}
}
