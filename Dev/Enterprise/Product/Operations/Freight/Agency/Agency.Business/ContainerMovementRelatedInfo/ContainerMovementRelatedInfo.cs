using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class ContainerMovementRelatedInfo : AutoContainerMovementRelatedInfo
	{
		public ContainerMovementRelatedInfo(ContainerMovement movement)
			: base(movement.Factory)
		{
			this.movement = movement;
			this.containers = new CachedProperty<AgencyShipmentContainer[]>(Factory, LoadRelatedContainers);
		}

		[List("Lookups.Ports")]
		public override ZString LoadPort
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.LoadPort; }
		}

		[List("Lookups.Ports")]
		public override ZString DischargePort
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.DischargePort; }
		}

		public AgencyShipmentContainer[] LoadContainers()
		{
			AgencyShipmentContainer[] tmp = containers.Value;
			AgencyShipmentContainer[] result = new AgencyShipmentContainer[tmp.Length];
			Array.Copy(tmp, result, tmp.Length);

			return result;
		}

		protected override ZString GetShipmentNumbers()
		{
			return CombineStrings((c) => c.Booking.JS_UniqueConsignRef, 35);
		}

		protected override ZString GetBillsOfLading()
		{
			return CombineStrings((c) => c.Booking.JS_HouseBill, 35);
		}

		protected override ZString GetBookingNumbers()
		{
			return CombineStrings((c) => c.Booking.JS_CFSReference, 35);
		}

		protected override ZString GetLoadPort()
		{
			ZString result = ZString.Empty;
			ZDateTime etd = ZDateTime.Empty;
			var voyagePk = movement.Voyage?.PK ?? ZGuid.Empty;

			if (voyagePk.IsEmpty)
			{
				return ZString.Empty;
			}

			foreach (AgencyShipmentContainer container in containers.Value)
			{
				AgencyShipment shipment;
				if ((shipment = container.Booking) == null)
				{
					continue;
				}

				var sailingsBelongToMovementVoyage = shipment.Transports.Select(transport => transport.Sailing).Where(jobSailing => voyagePk.Equals(jobSailing?.Voyage?.PK ?? ZGuid.Empty));
				foreach (var sailing in sailingsBelongToMovementVoyage)
				{
					VoyageOrigin origin = sailing.Origin;
					if (etd.IsEmpty || origin.JA_E_DEP < etd)
					{
						etd = origin.JA_E_DEP;
						result = origin.JA_RL_NKPortOfLoading;
					}
				}
			}

			return result;
		}

		protected override ZString GetDischargePort()
		{
			ZString result = ZString.Empty;
			ZDateTime eta = ZDateTime.Empty;
			var voyagePk = movement.Voyage?.PK ?? ZGuid.Empty;

			if (voyagePk.IsEmpty)
			{
				return ZString.Empty;
			}

			foreach (AgencyShipmentContainer container in containers.Value)
			{
				AgencyShipment shipment;
				if ((shipment = container.Booking) == null)
				{
					continue;
				}

				var sailingsBelongToMovementVoyage = shipment.Transports.Select(transport => transport.Sailing).Where(jobSailing => voyagePk.Equals(jobSailing?.Voyage?.PK ?? ZGuid.Empty)).ToList();
				foreach (var sailing in sailingsBelongToMovementVoyage)
				{
					var destination = sailing.Destination;
					if (eta.IsEmpty || destination.JB_E_ARV > eta)
					{
						eta = destination.JB_E_ARV;
						result = destination.JB_RL_NKPortOfDischarge;
					}
				}
			}

			return result;
		}

		protected override ZDateTime GetAvailabilityDate()
		{
			return ExtractFirstDate(delegate(AgencyShipmentContainer container)
			{
				AgencyShipment booking;
				Transport transport;

				if ((booking = container.Booking) != null &&
					(transport = booking.TransportsIncludingRelated.ArrivalTransport) != null)
				{
					return transport.JW_TerminalAvailabilityDate;
				}
				else
				{
					return ZDateTime.Empty;
				}
			});
		}

		protected override ZDateTime GetReturnByDate()
		{
			return ExtractFirstDate((c) => c.JC_EmptyReturnedBy);
		}

		protected override ZDateTime GetStartOfDetentionFreePeriod()
		{
			return movement.DetentionStrategy.GetStartOfDetentionFreePeriod(movement);
		}

		protected override ZDateTime GetStartOfDetentionPeriod()
		{
			return movement.DetentionStrategy.GetStartOfDetentionPeriod(movement);
		}

		protected override ZString GetConsignor()
		{
			return CombineStrings(delegate(AgencyShipmentContainer container)
			{
				JobDocAddress docAddress = container.Booking.ConsignorDocumentaryAddress;
				if (docAddress.E2_AddressOverride)
				{
					return docAddress.E2_CompanyNameTruncated;
				}
				else
				{
					OrgHeader header = docAddress.Organisation;
					return header == null ? "" : header.OH_Code.ToString();
				}
			}, 35);
		}

		protected override ZString GetConsignee()
		{
			return CombineStrings(delegate(AgencyShipmentContainer container)
			{
				JobDocAddress docAddress = container.Booking.ConsigneeDocumentaryAddress;
				if (docAddress.E2_AddressOverride)
				{
					return docAddress.E2_CompanyNameTruncated;
				}
				else
				{
					OrgHeader header = docAddress.Organisation;
					return header == null ? "" : header.OH_Code.ToString();
				}
			}, 35);
		}

		protected override ZString GetLocalClient()
		{
			OrgHeader clientOverride = movement.ResponsibleParty;
			return clientOverride == null ? FallBackLocalClient : clientOverride.OH_Code;
		}

		protected override ZString GetFallBackLocalClient()
		{
			return CombineStrings(delegate(AgencyShipmentContainer container)
			{
				JobHeader header = new JobHeader.Loader(container.Booking).Load();
				if (header == null)
				{
					return "";
				}
				else
				{
					OrgHeader client = header.LocalCharges;
					return client == null ? "" : client.OH_Code.ToString();
				}
			}, 35);
		}

		protected override ZString GetPrincipal()
		{
			OrgHeader principalOverride = movement.Principal;
			return principalOverride == null ? FallBackPrincipal : principalOverride.OH_Code;
		}

		protected override ZString GetFallBackPrincipal()
		{
			return CombineStrings(delegate(AgencyShipmentContainer container)
			{
				OrgHeader principal = container.Booking.Principal;
				return principal == null ? "" : principal.OH_Code.ToString();
			}, 35);
		}

		protected override ZShort GetImportDetentionFreeDays()
		{
			return CalculateDetentionFreeDays(CalculateImportDetentionFreeDays, true);
		}

		protected override ZShort GetExportDetentionFreeDays()
		{
			return CalculateDetentionFreeDays(CalculateExportDetentionFreeDays, false);
		}

		protected override ZShort GetDetentionFreeDays()
		{
			return movement.DetentionStrategy.GetDetentionFreeDays(movement);
		}

		public ContainerMovementRelatedInfoLookups Lookups
		{
			get { return lookups ?? (lookups = new ContainerMovementRelatedInfoLookups(this)); }
		}
		ContainerMovementRelatedInfoLookups lookups;

		string CombineStrings(Converter<AgencyShipmentContainer, string> converter, int capLength)
		{
			string[] values = ExtractDistinctValues(converter);

			if (values.Length == 0)
			{
				return "";
			}
			else
			{
				StringBuilder builder = new StringBuilder();

				builder.Append(values[0]);

				for (int i = 1; i < values.Length; i++)
				{
					builder.Append(", ");

					int available = capLength - builder.Length;

					if (i + 1 < values.Length)
					{
						available -= 5;
					}

					if (available < values[i].Length)
					{
						builder.Append("...");
						break;
					}
					else
					{
						builder.Append(values[i]);
					}
				}

				return builder.ToString();
			}
		}

		string[] ExtractDistinctValues(Converter<AgencyShipmentContainer, string> converter)
		{
			List<string> list = new List<string>();

			foreach (AgencyShipmentContainer shipment in containers.Value)
			{
				string str = converter(shipment);
				if (string.IsNullOrEmpty(str))
				{
					continue;
				}

				list.Add(str);
			}

			if (list.Count > 1)
			{
				list.Sort();

				int write = 1;
				for (int read = 1; read < list.Count; read++)
				{
					if (list[read - 1] != list[read])
					{
						if (read != write)
						{
							list[write] = list[read];
						}

						write++;
					}
				}

				if (write < list.Count)
				{
					list.RemoveRange(write, list.Count - write);
				}
			}

			return list.ToArray();
		}

		ZDateTime ExtractFirstDate(Converter<AgencyShipmentContainer, ZDateTime> converter)
		{
			ZDateTime result = ZDateTime.Empty;

			foreach (AgencyShipmentContainer container in containers.Value)
			{
				ZDateTime date = converter(container);

				if (date.IsEmpty)
				{
					continue;
				}

				if (result.IsEmpty || date < result)
				{
					result = date;
				}
			}

			return result;
		}

		AgencyShipmentContainer[] LoadRelatedContainers()
		{
			if (movement.IsDeleted || movement.Stock == null || movement.Stock.R6_ContainerNum.IsEmpty)
			{
				return Array.Empty<AgencyShipmentContainer>();
			}
			else
			{
				return LoadRelatedContainers(movement.E9_JV, movement.Stock.R6_ContainerNum);
			}
		}
		AgencyShipmentContainer[] LoadRelatedContainers(ZGuid voyagePK, ZString containerNum)
		{
			ZDBOnlySubQuery origin = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
			origin.AddToFilter(JobVoyOriginSchema.JA_JV, voyagePK);

			ZDBOnlySubQuery sailing = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
			sailing.AddSubQuery(origin, JoinCondition.And);

			ZDBOnlySubQuery transports = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			transports.AddToFilter(JobConsolTransportSchema.JW_IsLinked, true);
			transports.AddToFilter(JobConsolTransportSchema.JW_ParentType, Constants.TransportParentTypes.AgencyShipment);
			transports.AddSubQuery(JobConsolTransportSchema.JW_JX, sailing, JoinCondition.And);

			ZDBOnlySubQuery shipment = new ZDBOnlySubQuery(typeof(AgencyShipment), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
			shipment.AddSubQuery(JobShipmentSchema.JS_JX, sailing, JoinCondition.Or);
			shipment.AddSubQuery(transports, JoinCondition.Or);

			ZDBOnlyQuery container = new ZDBOnlyQuery(typeof(AgencyShipmentContainer));
			container.AddToFilter(JobContainerSchema.JC_ContainerNum, containerNum);
			container.AddToFilter(JobContainerSchema.JC_Purpose, ContainerBookedStatus.Codes.Real);
			container.AddSubQuery(shipment, JoinCondition.And);

			return Factory.Load<AgencyShipmentContainer>(container);
		}

		short CalculateDetentionFreeDays(Converter<AgencyShipmentContainer, short?> freeDays, bool isImport)
		{
			short? result = null;

			if (containers.Value.Length == 0)
			{
				result = freeDays(null);
			}
			else
			{
				foreach (AgencyShipmentContainer container in containers.Value)
				{
					short? days = freeDays(container);

					if (days.HasValue)
					{
						if (!result.HasValue)
						{
							result = days;
						}
						else
						{
							result = Math.Min(result.Value, days.Value);
						}
					}
				}
			}

			if (result.HasValue)
			{
				return result.Value;
			}
			else
			{
				if (isImport)
				{
					return (short)(FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.Value.ValidFreeDays ?? ZInt.Zero);
				}
				else
				{
					return (short)(FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForExport.Value.ValidFreeDays ?? ZInt.Zero);
				}
			}
		}

		IContainerPenaltyMatcherFactory penaltyMatcherFactory;
		IContainerPenaltyMatcherFactory PenaltyMatcherFactory => penaltyMatcherFactory ??= new ContainerPenaltyMatcherFactory();

		short? CalculateImportDetentionFreeDays(AgencyShipmentContainer container)
		{
			AgencyShipment shipment;
			RefContainer type;
			OrgHeader principal;
			OrgHeader client;
			ZString detentionPort = GetDetentionPort(movement.Depot);
			ZString originPort;

			if (container == null || (shipment = container.Booking) == null)
			{
				RefContainerStock stock = movement.Stock;

				principal = movement.Principal;
				client = movement.ResponsibleParty;
				type = stock == null ? null : stock.Container;
				originPort = ZString.Empty;
			}
			else
			{
				principal = movement.Principal ?? shipment.Principal;
				client = movement.ResponsibleParty ?? shipment.Consignee;
				type = container.Container;
				originPort = shipment.JS_RL_NKOrigin;
			}

			if (principal == null && client == null)
			{
				return null;
			}
			else
			{
				var filter = new ContainerPenaltyMatchFilter
				{
					Carrier = principal,
					Client = client,
					OriginPort = originPort,
					DetentionPort = detentionPort,
					ContainerClass = type?.RC_StorageClass ?? ZString.Empty,
					Direction = ContainerDetentionDirection.Import,
					ProcessType = Core.Constants.ContainerPenaltyProcessType.Import,
					Company = GlbCompany.CurrentCompany,
					Container = container
				};

				return PenaltyMatcherFactory.MatchDetention(filter)?.FreeDays;
			}
		}
		short? CalculateExportDetentionFreeDays(AgencyShipmentContainer container)
		{
			AgencyShipment shipment;
			RefContainer type;
			OrgHeader principal;
			OrgHeader client;
			ZString detentionPort = GetDetentionPort(movement.Depot);
			ZString originPort;

			if (container == null || (shipment = container.Booking) == null)
			{
				RefContainerStock stock = movement.Stock;

				principal = movement.Principal;
				client = movement.ResponsibleParty;
				type = stock == null ? null : stock.Container;
				originPort = ZString.Empty;
			}
			else
			{
				principal = movement.Principal ?? shipment.Principal;
				client = movement.ResponsibleParty ?? shipment.Consignor;
				type = container.Container;
				originPort = shipment.JS_RL_NKOrigin;
			}

			if (principal == null && client == null)
			{
				return null;
			}
			else
			{
				var filter = new ContainerPenaltyMatchFilter
				{
					Carrier = principal,
					Client = client,
					OriginPort = originPort,
					DetentionPort = detentionPort,
					ContainerClass = type?.RC_StorageClass ?? ZString.Empty,
					Direction = ContainerDetentionDirection.Export,
					ProcessType = Core.Constants.ContainerPenaltyProcessType.Export,
					Company = GlbCompany.CurrentCompany,
					Container = container
				};

				return PenaltyMatcherFactory.MatchDetention(filter)?.FreeDays;
			}
		}

		ZString GetDetentionPort(OrgAddress address)
		{
			OrgHeader header;

			if (address == null)
			{
				return ZString.Empty;
			}
			else if (!address.OA_RL_NKRelatedPortCode.IsEmpty)
			{
				return address.OA_RL_NKRelatedPortCode;
			}
			else if ((header = address.Header) != null)
			{
				return header.OH_RL_NKClosestPort;
			}
			else
			{
				return ZString.Empty;
			}
		}

		const int CombinedPortsMaxLength = 35;
		protected override ZString GetDestination()
		{
			return CombineStrings((c) => c.Booking.JS_RL_NKDestination, CombinedPortsMaxLength);
		}

		protected override ZString GetOrigin()
		{
			return CombineStrings((c) => c.Booking.JS_RL_NKOrigin, CombinedPortsMaxLength);
		}

		readonly CachedProperty<AgencyShipmentContainer[]> containers;
		readonly ContainerMovement movement;
	}
}


