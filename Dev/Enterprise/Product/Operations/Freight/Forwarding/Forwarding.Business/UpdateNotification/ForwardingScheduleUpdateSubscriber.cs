using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class ForwardingScheduleUpdateSubscriber : IScheduleUpdateSubscriber
	{
		public void ATDChanged(IScheduleUpdateServices services, VoyageOrigin origin, ZDateTime oldATD)
		{
		}

		public void ETDChanged(IScheduleUpdateServices services, VoyageOrigin origin, ZDateTime oldETD)
		{
			JobVoyage voyage = origin.Voyage;

			if (voyage.IsInDatabase && origin != null && IsDateValid(origin.JA_E_DEP) && IsDateValid(oldETD))
			{
				long diffTicks;

				if (IgnoreTimePart(voyage))
				{
					diffTicks = origin.JA_E_DEP.Date.ToZDateTime().Ticks - oldETD.Date.ToZDateTime().Ticks;
				}
				else
				{
					diffTicks = origin.JA_E_DEP.Ticks - oldETD.Ticks;
				}

				if (diffTicks != 0)
				{
					Func<Transport, Transport, bool> isEarlierETD = (transport, otherTransport) => !otherTransport.JW_ETD.IsValid || transport.JW_ETD < otherTransport.JW_ETD;
					var affectedConsols = LoadRelatedConsols(services, origin, JobSailingSchema.JX_JA, isEarlierETD);
					affectedConsols = Array.FindAll(affectedConsols, delegate(CommonConsol consol)
					{ return consol.Shipments.Count > 0; });
					BusinessObject[] affectedDeclarations = LoadDeclarationsForLoading(voyage, origin.JA_RL_NKPortOfLoading, origin.JA_E_DEP, oldETD);

					if (affectedConsols.Length > 0 || affectedDeclarations.Length > 0)
					{
						bool shouldUpdate = services.QueryProvider.ShouldUpdateRelatedShipmentsETD;
						if (shouldUpdate)
						{
							var shipmentBuilder = new ZStringBuilder();
							var declarationBuilder = new ZStringBuilder();
							foreach (CommonConsol consol in affectedConsols)
							{
								shipmentBuilder.AppendIfNotEmpty(IgnoreTimePart(voyage)
									? consol.Shipments.UpdateShipmentsETD(origin.JA_E_DEP.Date.ToZDateTime(), oldETD.Date.ToZDateTime())
									: consol.Shipments.UpdateShipmentsETD(origin.JA_E_DEP, oldETD));
							}
							foreach (BusinessObject declaration in affectedDeclarations)
							{
								declarationBuilder.AppendIfNotEmpty(((Integration.Forwarding.IJobDeclarationSailingDateUpdater)declaration).UpdateETD(origin.JA_E_DEP, diffTicks));
							}
							MessageBuilder(services, shipmentBuilder, declarationBuilder);
						}
						AddDateChangedLog(voyage, origin.JA_RL_NKPortOfLoading, "ETD", oldETD, origin.JA_E_DEP, shouldUpdate);
					}
				}
			}
		}

		public void ETAChanged(IScheduleUpdateServices services, VoyageDestination destination, ZDateTime oldETA)
		{
			JobVoyage voyage = destination.Voyage;

			if (voyage.IsInDatabase && destination != null && IsDateValid(destination.JB_E_ARV) && IsDateValid(oldETA))
			{
				long diffTicks;

				if (IgnoreTimePart(voyage))
				{
					diffTicks = destination.JB_E_ARV.Date.ToZDateTime().Ticks - oldETA.Date.ToZDateTime().Ticks;
				}
				else
				{
					diffTicks = destination.JB_E_ARV.Ticks - oldETA.Ticks;
				}

				if (diffTicks != 0)
				{
					Func<Transport, Transport, bool> isLaterETA = (transport, otherTransport) => !otherTransport.JW_ETA.IsValid || transport.JW_ETA > otherTransport.JW_ETA;
					var affectedConsols = LoadRelatedConsols(services, destination, JobSailingSchema.JX_JB, isLaterETA);
					affectedConsols = Array.FindAll(affectedConsols, delegate(CommonConsol consol1)
					{ return consol1.Shipments.Count > 0; });
					BusinessObject[] affectedDeclarations = LoadDeclarationsForDischarge(voyage, destination.JB_RL_NKPortOfDischarge, destination.JB_E_ARV, oldETA);

					if (affectedConsols.Length > 0 || affectedDeclarations.Length > 0)
					{
						bool shouldUpdate = services.QueryProvider.ShouldUpdateRelatedShipmentsETA;
						if (shouldUpdate)
						{
							var shipmentBuilder = new ZStringBuilder();
							var declarationBuilder = new ZStringBuilder();
							foreach (CommonConsol consol in affectedConsols)
							{
								shipmentBuilder.AppendIfNotEmpty(IgnoreTimePart(voyage)
									? consol.Shipments.UpdateShipmentsETA(destination.JB_E_ARV.Date.ToZDateTime(), oldETA.Date.ToZDateTime())
									: consol.Shipments.UpdateShipmentsETA(destination.JB_E_ARV, oldETA));
							}
							foreach (BusinessObject declaration in affectedDeclarations)
							{
								declarationBuilder.AppendIfNotEmpty(((Integration.Forwarding.IJobDeclarationSailingDateUpdater)declaration).UpdateETA(destination.JB_E_ARV, diffTicks));
							}
							MessageBuilder(services, shipmentBuilder, declarationBuilder);
						}
						AddDateChangedLog(voyage, destination.JB_RL_NKPortOfDischarge, "ETA", oldETA, destination.JB_E_ARV, shouldUpdate);
					}
				}
			}
		}

		void MessageBuilder(IScheduleUpdateServices services, ZStringBuilder shipmentBuilder, ZStringBuilder declarationBuilder)
		{
			ZString processedJobs = ZString.Empty;
			if (shipmentBuilder.Length > 0)
			{
				processedJobs += Res.GetString("478ab5a2-184e-4b3b-8ba7-e8a90ded563f", "The following shipments were processed:\r\n{0}", shipmentBuilder.ToStringWithNewLineBetweenAppends());
			}
			if (declarationBuilder.Length > 0)
			{
				processedJobs += Res.GetString("c38e2ac3-c0d6-4958-933d-b711ea69466c", "The following declarations were processed:\r\n{0}", declarationBuilder.ToStringWithNewLineBetweenAppends());
			}
			if (processedJobs.IsEmpty)
			{
				processedJobs = Res.GetString("81df119e-29b4-4411-83aa-8bffe5f21809", "No shipments or declarations were processed");
			}
			services.QueryProvider.ShowInformation(processedJobs);
		}

		bool IsDateValid(ZDateTime date)
		{
			return !date.IsEmpty && date.IsValid && date.AddDays(1).IsValidSmallDateTime;
		}

		internal static BusinessObject[] LoadDeclarationsForLoading(JobVoyage voyage, ZString originPort, ZDateTime newETD, ZDateTime oldETD)
		{
			var query = CreateMainDeclarationQuery(voyage);
			query.AddToFilter(JobDeclarationSchema.JE_RL_NKPortOfLoading, originPort);

			var dateFilter = new ZQuery();
			dateFilter.AddToFilter(JobDeclarationSchema.JE_DateAtOrigin, SQLComparisonOperator.EqualToDatePartOnly, oldETD); //Only need to synchronize declarations when the dates change, no need to update all declarations.
			dateFilter.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_DateAtOrigin, SQLComparisonOperator.EqualToDatePartOnly, newETD);
			dateFilter.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_ExportDate, SQLComparisonOperator.EqualToDatePartOnly, oldETD);
			dateFilter.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_ExportDate, SQLComparisonOperator.EqualToDatePartOnly, newETD);

			query.AddToFilter(dateFilter);

			var result = (BusinessObject[])voyage.Factory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(query);
			return result;
		}

		internal static BusinessObject[] LoadDeclarationsForDischarge(JobVoyage voyage, ZString arrivalPort, ZDateTime newETA, ZDateTime oldETA)
		{
			var query = CreateMainDeclarationQuery(voyage);
			query.AddToFilter(JobDeclarationSchema.JE_RL_NKPortOfArrival, arrivalPort);

			var dateFilter = new ZQuery();
			dateFilter.AddToFilter(JobDeclarationSchema.JE_DateOfArrival, SQLComparisonOperator.EqualToDatePartOnly, oldETA); //Only need to synchronize declarations when the dates change, no need to update all declarations.
			dateFilter.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_DateOfArrival, SQLComparisonOperator.EqualToDatePartOnly, newETA);
			dateFilter.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_DateAtFinalDestination, SQLComparisonOperator.EqualToDatePartOnly, oldETA);
			dateFilter.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_DateAtFinalDestination, SQLComparisonOperator.EqualToDatePartOnly, newETA);

			query.AddToFilter(dateFilter);

			return (BusinessObject[])voyage.Factory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(query);
		}

		static ZQuery CreateMainDeclarationQuery(JobVoyage voyage)
		{
			var query = new ZQuery();
			query.AddToFilter(JobDeclarationSchema.JE_VesselName, voyage.JV_RV_NKVessel);
			query.AddToFilter(JobDeclarationSchema.JE_VoyageFlightNo, voyage.JV_VoyageFlight);
			query.AddToFilter(JobDeclarationSchema.JE_TransportMode, voyage.JV_AirSeaRoad);

			if (voyage.IsMainVoyage)
			{
				var carrierPKsFromSlotVoyages = voyage.FindOtherVoyagesWithSameVesselVoyageCombination()
					.Select(v => v.JV_OH_Line)
					.Where(pk => !pk.IsEmpty);

				if (carrierPKsFromSlotVoyages.Any())
				{
					query.AddToFilter(JobDeclarationSchema.JE_OH_ShippingLine, SQLComparisonOperator.NotEqual, carrierPKsFromSlotVoyages);
				}
			}
			else
			{
				if (!voyage.JV_OH_Line.IsEmpty)
				{
					query.AddToFilter(JobDeclarationSchema.JE_OH_ShippingLine, voyage.JV_OH_Line);
				}
				else
				{
					query.AddToFilter(JobDeclarationSchema.JE_OH_ShippingLine, null);
				}
			}

			return query;
		}

		static CommonConsol[] LoadRelatedConsols(IScheduleUpdateServices services, BusinessObject originOrDestination, SchemaGuidColumn sailingOriginOrDestinationFK, Func<Transport, Transport, bool> condition)
		{
			var filter = new ZQuery(sailingOriginOrDestinationFK, originOrDestination.PK);

			var result = new List<CommonConsol>();
			foreach (JobSailing sailing in originOrDestination.Factory.Load<JobSailing>(filter))
			{
				if ((ZGuid)sailing[sailingOriginOrDestinationFK] == originOrDestination.PK)
				{
					result.AddRange(LoadRelatedConsols(services, sailing, condition));
				}
			}

			return result.ToArray();
		}

		static CommonConsol[] LoadRelatedConsols(IScheduleUpdateServices services, JobSailing sailing, Func<Transport, Transport, bool> condition)
		{
			var filter = new ZQuery();
			filter.AddToFilter(JobConsolTransportSchema.JW_ParentType, Core.Constants.TransportParentTypes.Consol);
			filter.AddToFilter(JobConsolTransportSchema.JW_JX, sailing.PK);

			var result = new List<CommonConsol>();
			Transport[] transports = sailing.Factory.Load<Transport>(filter);
			foreach (Transport transport in transports)
			{
				transport.ParentType = services.ParentConsolType;
				CommonConsol consol = (CommonConsol)transport.Parent;
				if (consol != null && (condition == null || consol.Transports.Cast<Transport>().All(t => t.PK == transport.PK || condition(transport, t))))
				{
					result.Add(consol);
				}
			}
			return result.ToArray();
		}

		static bool IgnoreTimePart(JobVoyage voyage)
		{
			switch (voyage.JV_AirSeaRoad)
			{
				case Constants.TransportModes.Sea:
				case Constants.TransportModes.Rail:
					return true;

				default:
					return false;
			}
		}

		void AddDateChangedLog(JobVoyage voyage, ZString uNLOCO, ZString dateType, ZDateTime oldDate, ZDateTime newDate, bool updatedShipments)
		{
			#region SuppressResourceStringsCheckRegion

			ZString message = ZString.Format("{0} {1} edited from {2} to {3}.", uNLOCO, dateType, oldDate, newDate) + " ";
			ZString updateMessage = "";
			if (!updatedShipments)
			{
				updateMessage += "Did NOT update";
			}
			else
			{
				updateMessage += "Updated";
			}
			updateMessage += " " + ZString.Format("related shipment's {0}s.", dateType);
			message += updateMessage;

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			voyage.Logs.AddNew(Events.EditedARecord, message);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			#endregion
		}
	}
}
