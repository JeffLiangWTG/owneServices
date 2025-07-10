using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing
{
	public abstract class CMMProcessingAdapter : ICMMProcessingAdapter
	{
		ContainerMovementTypes MovementList
		{
			get { return Factory.GetCachedValue<ContainerMovementTypes>(); }
		}

		OrgFlags SenderFlags
		{
			get
			{
				if (!senderFlags.HasValue)
				{
					senderFlags = GetOrgFlags(Sender);
				}
				return senderFlags.Value;
			}
		}
		OrgFlags? senderFlags;

		CMMFlags UpdateBookingsFlags
		{
			get { return updateBookingsFlags ?? (updateBookingsFlags = AgencyRegistry.Instance.CMMUpdateBookings.GetFactoryCachedValue(Factory)); }
		}
		CMMFlags updateBookingsFlags;

		protected Helper AddressHelper
		{
			get { return addressHelper ?? (addressHelper = new Helper()); }
		}
		Helper addressHelper;

		#region IContainerMovementMessageProcessingAdapter Members

		public void Load()
		{
			LoadCore();

			if (Sender == null)
			{
				string exceptionMessage = !string.IsNullOrWhiteSpace(MessageSenderCode) ?
					Res.GetString("ff0790ce-297b-405f-b071-0340c7aa7fc9", "Cannot process message from an unknown sender ({0}).", MessageSenderCode) :
					Res.GetString("6c24996a-637b-4bbb-b7c5-3002269c3d15", "Cannot process message from an unknown sender.");

				throw new InvalidOperationException(exceptionMessage);
			}

			IsLoaded = true;
		}

		public bool IsLoaded { get; private set; }

		protected abstract void LoadCore();

		public BusinessObjectFactory Factory { get; set; }

		public abstract OrgAddress SenderAddress { get; }

		public abstract string LloydsNumber { get; }

		public abstract string MessageSenderCode { get; }
		public abstract CMMOrganisationType MessageSenderCodeType { get; }

		public abstract string MessageText { get; }

		public abstract string VoyageNumber { get; }

		public abstract string TransportMode { get; }

		public abstract List<CMMMessageContainer> Containers { get; }

		public abstract EDIMessage Message { get; }

		public abstract CMMMessageType MessageType { get; }

		public abstract void AttachMessage(ContainerMovement movement, string status);

		public RefVessel Vessel
		{
			get
			{
				if (vessel == null && !String.IsNullOrEmpty(LloydsNumber))
				{
					vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, LloydsNumber));
				}
				return vessel;
			}
		}
		RefVessel vessel;

		public JobVoyage Voyage
		{
			get
			{
				if (voyage == null
					&& !String.IsNullOrEmpty(VoyageNumber)
					&& Vessel != null)
				{
					var filter = new ZQuery();
					filter.AddToFilter(JobVoyageSchema.JV_VoyageFlight, VoyageNumber);
					filter.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, Vessel.RV_FK);

					var results = Factory.Load<JobVoyage>(filter);
					if (results.Length == 1)
					{
						voyage = results[0];
					}
					else if (results.Length > 1)
					{
						var containerNumbers = Containers.Select(container => container.ContainerNumber)
							.Where(number => !String.IsNullOrEmpty(number))
							.ToArray();

						if (containerNumbers.Any())
						{
							voyage = GetVoyageFromContainers(results, containerNumbers);
						}
					}
				}

				return voyage;
			}
		}

		JobVoyage voyage;

		JobVoyage GetVoyageFromContainers(JobVoyage[] possibleResults, string[] containerNumbers)
		{
			var containerSubQuery = new ZDBOnlySubQuery(typeof(BillOfLadingContainer), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
			containerSubQuery.AddToFilter(JobContainerSchema.JC_ContainerNum, containerNumbers);
			containerSubQuery.AddToFilter(JobContainerSchema.JC_Purpose, ContainerBookedStatus.Codes.Real);

			var shipmentSubQuery = new ZDBOnlySubQuery(typeof(BillOfLading), JobShipmentSchema.JS_JX);
			shipmentSubQuery.AddToFilter(JobShipmentSchema.JS_IsShipping, true);
			shipmentSubQuery.AddSubQuery(containerSubQuery, JoinCondition.And);

			var sailingSubquery = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.JX_JA);
			sailingSubquery.AddSubQuery(shipmentSubQuery, JoinCondition.And);

			var originSubQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobVoyOriginSchema.JA_JV);
			originSubQuery.AddSubQuery(sailingSubquery, JoinCondition.And);

			var voyagesQuery = new ZDBOnlyQuery(typeof(JobVoyage));
			voyagesQuery.AddToFilter(JobVoyageSchema.PK, possibleResults.Select(v => v.PK));
			voyagesQuery.AddSubQuery(originSubQuery, JoinCondition.And);

			var results = Factory.Load<JobVoyage>(voyagesQuery);

			return results.Length == 1 ? results[0] : null;
		}

		public OrgHeader Sender
		{
			get { return SenderAddress == null ? null : SenderAddress.Header; }
		}

		public string CountryCode
		{
			get
			{
				if (countryCode == null && PortCode != null && PortCode.Length == 5)
				{
					countryCode = PortCode.Substring(0, 2);
				}
				return countryCode ?? string.Empty;
			}
		}
		string countryCode;

		public bool CreateMissingContainers
		{
			get
			{
				if (!createMissingContainers.HasValue)
				{
					createMissingContainers = AgencyRegistry.Instance.CMMCreateMissingContainers.Value;
				}

				return createMissingContainers.Value;
			}
		}
		bool? createMissingContainers;

		public string PortCode
		{
			get
			{
				if (portCode == null && SenderAddress != null)
				{
					portCode = AddressHelper.ExtractPort(SenderAddress);
				}
				return portCode ?? string.Empty;
			}
		}
		string portCode;

		public bool HasRelatedJobs(string containerNumber)
		{
			if (Voyage == null || String.IsNullOrEmpty(containerNumber))
			{
				return false;
			}
			else
			{
				bool result;

				if (hasRelatedJobsCache == null)
				{
					result = HasRelatedJobs(Voyage, containerNumber);
					hasRelatedJobsCache = new Dictionary<string, bool>();
					hasRelatedJobsCache.Add(containerNumber, result);
				}
				else if (!hasRelatedJobsCache.TryGetValue(containerNumber, out result))
				{
					result = HasRelatedJobs(Voyage, containerNumber);
					hasRelatedJobsCache.Add(containerNumber, result);
				}

				return result;
			}
		}
		Dictionary<string, bool> hasRelatedJobsCache;

		public string GetMovementDescription(string movementCode)
		{
			return MovementList.GetDescriptionFromCode(movementCode) ?? String.Empty;
		}

		public string MessageSenderReference
		{
			get
			{
				var builder = new StringBuilder();
				if (Sender != null)
				{
					builder.Append(Res.GetString("9d99d965-d4a9-4e4a-a9cf-2d04e6784e47", "Message Sender: {0} ({1})", Sender.OH_FullNameTruncated, Sender.OH_Code) + "\r\n");
				}
				else if (!string.IsNullOrEmpty(MessageSenderCode) && MessageSenderCodeType != CMMOrganisationType.Unknown)
				{
					builder.Append(Res.GetString("3a512516-9ddd-45e8-869d-6f289bce885f", "Message Sender Ref Type: {0}\r\nMessage Sender Ref: {1}", MessageSenderCodeType, MessageSenderCode) + "\r\n");
				}
				return builder.ToString();
			}
		}

		public string MessageTypeTitle
		{
			get
			{
				switch (MessageType)
				{
					case CMMMessageType.Load:
						return Res.GetString("150a89b6-4405-4b97-883d-06ea916dcf73", "Load COARRI");
					case CMMMessageType.Discharge:
						return Res.GetString("47abf233-adc9-4cb3-8bb6-568d55531aed", "Discharge COARRI");
					case CMMMessageType.GateIn:
						return Res.GetString("dc736fb4-a0da-4703-994e-a864f5c0743c", "Gate-In CODECO");
					case CMMMessageType.GateOut:
						return Res.GetString("db7a2883-8f69-498d-9efb-6461867b437a", "Gate-Out CODECO");
					default:
						return Res.GetString("7e6540eb-ba30-4669-be13-2d8d72fa9f94", "Unknown");
				}
			}
		}

		public virtual bool UpdateBookings
		{
			get
			{
				if (!updateBookings.HasValue)
				{
					switch (MessageType)
					{
						case CMMMessageType.Load:
							updateBookings = UpdateBookingsFlags.Load;
							break;

						case CMMMessageType.Discharge:
							updateBookings = UpdateBookingsFlags.Discharge;
							break;

						case CMMMessageType.GateIn:
							{
								switch (SenderFlags)
								{
									case OrgFlags.CFS:
										updateBookings = UpdateBookingsFlags.DepotGateIn;
										break;
									case OrgFlags.CTO:
										updateBookings = UpdateBookingsFlags.WharfGateIn;
										break;
									case OrgFlags.CY:
										updateBookings = UpdateBookingsFlags.YardGateIn;
										break;
									default:
										updateBookings = false;
										break;
								}
							}
							break;

						case CMMMessageType.GateOut:
							{
								switch (SenderFlags)
								{
									case OrgFlags.CFS:
										updateBookings = UpdateBookingsFlags.DepotGateOut;
										break;
									case OrgFlags.CTO:
										updateBookings = UpdateBookingsFlags.WharfGateOut;
										break;
									case OrgFlags.CY:
										updateBookings = UpdateBookingsFlags.YardGateOut;
										break;
									default:
										updateBookings = false;
										break;
								}
							}
							break;

						default:
							throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Don't know how to handle '{0}' messages.", MessageType));
					}
				}

				return updateBookings.Value;
			}
		}
		bool? updateBookings;

		public string MovementCode
		{
			get
			{
				switch (MessageType)
				{
					case CMMMessageType.GateIn:
						switch (SenderFlags)
						{
							case OrgFlags.CFS:
								return ContainerMovementTypes.Codes.DepotGateIn;
							case OrgFlags.CTO:
								return ContainerMovementTypes.Codes.WharfGateIn;
							case OrgFlags.CY:
								return ContainerMovementTypes.Codes.YardGateIn;
							default:
								return String.Empty;
								// this will return an empty string if more than one flag is set, this is intended.
						}

					case CMMMessageType.GateOut:
						switch (SenderFlags)
						{
							case OrgFlags.CFS:
								return ContainerMovementTypes.Codes.DepotGateOut;
							case OrgFlags.CTO:
								return ContainerMovementTypes.Codes.WharfGateOut;
							case OrgFlags.CY:
								return ContainerMovementTypes.Codes.YardGateOut;
							default:
								return String.Empty;
								// this will return an empty string if more than one flag is set, this is intended.
						}

					case CMMMessageType.Load:
						return ContainerMovementTypes.Codes.Load;

					case CMMMessageType.Discharge:
						return ContainerMovementTypes.Codes.Discharge;

					case CMMMessageType.Unknown:
						return String.Empty;

					default:
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Don't know how to handle '{0}' messages.", MessageType));
				}
			}
		}

		#endregion

		static bool HasRelatedJobs(JobVoyage voyage, string containerNumber)
		{
			var originFilter = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
			originFilter.AddToFilter(JobVoyOriginSchema.JA_JV, voyage.PK);

			var sailingFilter = new ZDBOnlySubQuery(typeof(JobSailing), JobShipmentSchema.JS_JX);
			sailingFilter.AddSubQuery(originFilter, JoinCondition.And);

			var shipmentFilter = new ZDBOnlySubQuery(typeof(BillOfLading), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
			shipmentFilter.AddToFilter(JobShipmentSchema.JS_IsShipping, true);
			shipmentFilter.AddSubQuery(sailingFilter, JoinCondition.And);

			var containerFilter = new ZDBOnlyQuery(typeof(BillOfLadingContainer));
			containerFilter.AddToFilter(JobContainerSchema.JC_ContainerNum, containerNumber);
			containerFilter.AddToFilter(JobContainerSchema.JC_Purpose, ContainerBookedStatus.Codes.Real);
			containerFilter.AddSubQuery(shipmentFilter, JoinCondition.And);

			return voyage.Factory.ExistsInDatabase(BillOfLadingContainer.Schema.TableName, containerFilter);
		}

		#region OrgFlags

		static OrgFlags GetOrgFlags(OrgHeader header)
		{
			OrgFlags result = OrgFlags.None;

			if (header != null)
			{
				if (header.OH_IsContainerYard)
				{
					result |= OrgFlags.CY;
				}

				if (header.OH_IsSeaCTO)
				{
					result |= OrgFlags.CTO;
				}

				if (header.OH_IsPackDepot | header.OH_IsUnpackDepot)
				{
					result |= OrgFlags.CFS;
				}
			}

			return result;
		}

		[Flags]
		enum OrgFlags
		{
			None = 0x00,
			CFS = 0x01,
			CTO = 0x02,
			CY = 0x04,
		}

		#endregion

		#region AddressLocator

		protected class Helper
		{
			public string ExtractPort(OrgAddress address)
			{
				OrgHeader header;

				if (address == null)
				{
					return null;
				}
				if (address.OA_RL_NKRelatedPortCode.IsEmpty)
				{
					return address.OA_RL_NKRelatedPortCode;
				}
				if ((header = address.Header) == null)
				{
					return null;
				}

				return header.OH_RL_NKClosestPort;
			}

			public AddressType DefaultShipperAddressType(CMMMessageType messageType)
			{
				switch (messageType)
				{
					case CMMMessageType.GateIn:
					case CMMMessageType.Load:
						return AddressType.DLV;

					case CMMMessageType.GateOut:
					case CMMMessageType.Discharge:
						return AddressType.PIC;

					default:
						return AddressType.OFC;
				}
			}

			[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
			public OrgAddress FindOrganisationAddressByOrgCodeType(BusinessObjectFactory factory, AddressType fallbackType, CMMOrganisationType orgType, string orgCode)
			{
				switch (orgType)
				{
					case CMMOrganisationType.OneStop:
						return FindOrganisationAddressByOneStopCode(factory, fallbackType, orgCode);

					case CMMOrganisationType.MutuallyDefined:
						return FindOrganisationAddressByAgreedCode(factory, fallbackType, orgCode);

					default:
						throw new Exception("Unrecognised code type.");
				}
			}

			public OrgAddress FindOrganisationAddressByEHubCode(BusinessObjectFactory factory, AddressType fallbackType, string ehubCode)
			{
				var filter = new ZQuery();
				filter.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.EHubOrganisationID);
				filter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, ehubCode);

				return FindOrganisationAddressByCusCode(factory, fallbackType, ehubCode, "EHub", filter);
			}

			OrgAddress FindOrganisationAddressByOneStopCode(BusinessObjectFactory factory, AddressType fallbackType, string oneStopCode)
			{
				var filter = new ZQuery();
				filter.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.OneStopCode);
				filter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Constants.CountryCodes.Australia);
				filter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, oneStopCode);

				return FindOrganisationAddressByCusCode(factory, fallbackType, oneStopCode, "1Stop", filter);
			}

			OrgAddress FindOrganisationAddressByAgreedCode(BusinessObjectFactory factory, AddressType fallbackType, string agreedCode)
			{
				var filter = new ZQuery();
				filter.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.ContainerManagementMessagingAgreedCode);
				filter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, agreedCode);

				return FindOrganisationAddressByCusCode(factory, fallbackType, agreedCode, Res.GetString("3af9157f-d0a5-4a9d-b674-2c2e60d34e5f", "agreed"), filter);
			}

			[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
			OrgAddress FindOrganisationAddressByCusCode(BusinessObjectFactory factory, AddressType fallbackType, string code, string codeLabel, ZQuery cusCodeFilter)
			{
				OrgCusCode[] cusCodes = factory.Load<OrgCusCode>(cusCodeFilter);

				OrgAddress[] addresses = factory.Load<OrgAddress>(new ZQuery(OrgAddressSchema.PK, Array.ConvertAll(cusCodes, (c) => c.OK_OA_PremisesAddress)));
				switch (addresses.Length)
				{
					case 0:
						break;
					case 1:
						return addresses[0];

					default:
						string lead =
							Res.GetString("70ffa87a-f056-41f2-a238-8297108006a7", "Unable to uniquely identify the address with the {0} code '{1}' as the following addresses all share this code:", codeLabel, code) + "\r\n";

						const string lineFormat =
							"* {0}: {1} - {2}\r\n";

						const string trailFormat =
							"\r\n";

						var builder = new StringBuilder();
						builder.Append(lead);

						foreach (OrgAddress address in addresses)
						{
							builder.AppendFormat(CultureInfo.InvariantCulture, lineFormat, address.Header.OH_Code, address.Header.OH_FullNameTruncated, address.OA_Address1);
						}

						builder.Append(trailFormat);
						throw new Exception(builder.ToString());
				}

				OrgHeader[] headers = factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, Array.ConvertAll(cusCodes, (c) => c.OK_OH)));
				switch (headers.Length)
				{
					case 0:
						return null;
					case 1:
						return headers[0].GetAddressWithFallback(fallbackType);

					default:
						string lead =
							Res.GetString("945b5605-dd67-4252-b052-5c1559053b68", "Unable to uniquely identify the organization with the {0} code '{1}' as the following organizations all share this code:", codeLabel, code) + "\r\n";

						const string lineFormat =
							"* {0}: {1}\r\n";

						const string trailFormat =
							"\r\n";

						var builder = new StringBuilder();
						builder.Append(lead);

						foreach (OrgHeader header in headers)
						{
							builder.AppendFormat(CultureInfo.InvariantCulture, lineFormat, header.OH_Code, header.OH_FullNameTruncated);
						}

						builder.Append(trailFormat);
						throw new Exception(builder.ToString());
				}
			}
		}

		#endregion
	}
}




