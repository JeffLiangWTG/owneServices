using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class ReleaseHeader : AutoReleaseHeader
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ReleaseHeader(AgencyBooking shipment, bool isReplacement)
			: base(shipment.Factory, isReplacement)
		{
			this.shipment = shipment;
			IncludeMessage = SupportsMessageSending && IncludeMessageEnabled;
		}

		#region Related Business Objects

		public AgencyBooking Shipment
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return shipment; }
		}

		[ChildEditable(true)]
		public ReleaseDetailCollection Details
		{
			get
			{
				if (details == null)
				{
					details = new ReleaseDetailCollection(shipment);
					RegisterEditableChildObject(details);
				}
				return details;
			}
		}
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		ReleaseDetailCollection details;

		public ReleaseInstanceCollection Instances
		{
			get { return instances ?? (instances = new ReleaseInstanceCollection(this)); }
		}
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		ReleaseInstanceCollection instances;

		#endregion

		#region Properties

		#region ReleaseNumber

		[List("Lookups.ReleaseNumbers")]
		public override ZString ReleaseNumber
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.ReleaseNumber; }
			set
			{
				if (ReleaseNumber != value)
				{
					base.ReleaseNumber = value;

					Details.RePopulate(value);
				}
			}
		}

		protected override bool ReleaseNumber_ReadOnly
		{
			get { return !IsReplacement; }
		}

		#endregion

		#region IncludeMessage

		public ZBool IncludeMessageEnabled
		{
			get { return AgencyRegistry.Instance.ContainerExportPreAdvicePorts.FindByPortAndPrincipalWithFallback(shipment.JS_NKLoadPort, shipment.JS_OH_DeliveryAgent)?.Enabled ?? false; }
		}

		public ZBool SupportsMessageSending
		{
			get { return MessageStrategies.Length > 0; }
		}

		public ZString IncludeMessageCaption
		{
			get
			{
				if (MessageStrategies.Length == 1)
				{
					return Res.GetString("31901639-be3a-47a1-92c4-dda8d214c270", "Send Export Pre-Advice to {0}", MessageStrategies[0].MessageDescription);
				}
				else if (MessageStrategies.Length > 1)
				{
					return Res.GetString("c9d30d33-982e-4cdc-97a5-9e08e6b4ce47", "Send Export Pre-Advices to ({0})", string.Join(", ", MessageStrategies.Select(x => x.MessageDescription)));
				}

				return ZString.Empty;
			}
		}

		#endregion

		#endregion

		#region Stratrgies

		public ReleaseHeaderLookups Lookups
		{
			get { return lookups ?? (lookups = new ReleaseHeaderLookups(this)); }
		}
		ReleaseHeaderLookups lookups;

		#endregion

		#region Operations

		public void Init()
		{
			lookups = null;

			if (IsReplacement && Lookups.ReleaseNumbers.Count > 0)
			{
				ReleaseNumber = Lookups.ReleaseNumbers[Lookups.ReleaseNumbers.Count - 1].Code;
			}
			else
			{
				Details.RePopulate("");
			}
		}

		public void DoRelease(NotificationsHandler notifications)
		{
			Instances.RemoveAll();
			Dictionary<ZGuid, ZString> cache = new Dictionary<ZGuid, ZString>();

			foreach (ReleaseDetail detail in Details.ToArray())
			{
				if (detail.ReleaseCount == 0)
				{
					if (detail.PreviouslyReleased)
					{
						Instances.AddIfNotExists(
							detail.Container.JC_OA_DepartureContainerYardAddress,
							detail.Container.JC_ReleaseNum,
							Constants.EventReferenceReleaseTypes.Codes.Cancellation);

						detail.Container.JC_ReleaseNum = "";
					}
				}
				else if (detail.ReleaseCount == detail.Container.JC_ContainerCount)
				{
					detail.Container.JC_ReleaseNum = GetReleaseNumber(cache, detail.ContainerYardAddress);

					Instances.AddIfNotExists(
						detail.Container.JC_OA_DepartureContainerYardAddress,
						detail.Container.JC_ReleaseNum,
						detail.PreviouslyReleased ? Constants.EventReferenceReleaseTypes.Codes.Reprint : Constants.EventReferenceReleaseTypes.Codes.Original);
				}
				else
				{
					short oldCount = detail.Container.JC_ContainerCount;
					decimal oldTare = detail.Container.JC_TareWeight;
					decimal oldGross = detail.Container.JC_GrossWeight;

					short newCount = detail.ReleaseCount;
					decimal newTare = Split(oldTare, oldCount, newCount);
					decimal newGross = Split(oldGross, oldCount, newCount);

					detail.Container.JC_ContainerCount = newCount;
					detail.Container.JC_ReleaseNum = GetReleaseNumber(cache, detail.ContainerYardAddress);
					detail.Container.JC_TareWeight = newTare;
					detail.Container.JC_GrossWeight = newGross;

					AgencyShipmentContainer newContainer = (AgencyShipmentContainer)detail.Container.Clone();
					newContainer.JC_ContainerCount = (short)(oldCount - newCount);
					newContainer.JC_ReleaseNum = "";
					newContainer.JC_TareWeight = oldTare - newTare;
					newContainer.JC_GrossWeight = oldGross - newGross;

					shipment.BookedContainers.Add(newContainer);
					Details.Add(new ReleaseDetail(newContainer));

					Instances.AddIfNotExists(
						detail.Container.JC_OA_DepartureContainerYardAddress,
						detail.Container.JC_ReleaseNum,
						detail.PreviouslyReleased ? Constants.EventReferenceReleaseTypes.Codes.Revised : Constants.EventReferenceReleaseTypes.Codes.Original);
				}
			}

			lookups = null;
			CreateReleaseRequestedEvent(notifications);
			UpdateContainerReleaseLog();
		}

		void CreateReleaseRequestedEvent(NotificationsHandler notifications)
		{
			foreach (ReleaseInstance instance in Instances)
			{
				CreateReleaseRequestedEventForInstance(instance, notifications);
			}
		}

		void CreateReleaseRequestedEventForInstance(ReleaseInstance instance, NotificationsHandler notifications)
		{
			var eventReference = instance.ReleaseNumber;
			var eventParams = GetEventParametersForInstance(instance);
			var constructedEventReference = StmALog.GenerateEventReference(eventReference, eventParams);

			shipment.Logs.AddNew(Events.ReleaseRequested, eventReference, eventParams);

			if (IncludeMessage)
			{
				ReleaseMessageHelper.SendUXml(this, constructedEventReference, notifications, GetPurposeCode(instance.ReleaseType), GetDocumentName(instance.ReleaseType));

				if (!notifications.Notifications.Any(x => x.Type == CargoWise.ComponentModel.NotificationType.Error))
				{
					foreach (var strategy in MessageStrategies)
					{
						strategy.CreateMessageEvent(instance.ReleaseType);
					}
				}
			}
		}

		ZString GetPurposeCode(ZString releaseType)
		{
			switch (releaseType)
			{
				case Constants.EventReferenceReleaseTypes.Codes.Reprint:
				case Constants.EventReferenceReleaseTypes.Codes.Revised:
					return MessagePurposes.Codes.Amendment;
				case Constants.EventReferenceReleaseTypes.Codes.Original:
					return MessagePurposes.Codes.Original;
				case Constants.EventReferenceReleaseTypes.Codes.Cancellation:
					return MessagePurposes.Codes.Withdrawal;
				default:
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Release Type not expected: {0}", releaseType));
			}
		}

		ZString GetDocumentName(ZString releaseType)
		{
			if (releaseType == Constants.EventReferenceReleaseTypes.Codes.Original)
			{
				return Res.GetString("2c725fc5-a767-4214-8567-578b162f741a", "Export Pre-Advice");
			}

			if (releaseType == Constants.EventReferenceReleaseTypes.Codes.Revised
				|| releaseType == Constants.EventReferenceReleaseTypes.Codes.Reprint)
			{
				return Res.GetString("b88522a3-c7ba-47c4-b7a1-8ec9b0641e88", "Export Pre-Advice Replacement");
			}

			if (releaseType == Constants.EventReferenceReleaseTypes.Codes.Cancellation)
			{
				return Res.GetString("00b4f6eb-1f95-4fa9-b5a1-a6c346863deb", "Export Pre-Advice Cancellation");
			}

			throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Release Type not expected: {0}", releaseType));
		}

		KeyValuePair<string, string>[] GetEventParametersForInstance(ReleaseInstance instance)
		{
			var parameters = new List<KeyValuePair<string, string>>();

			parameters.Add(new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Facility, Constants.Facilities.Code.ContainerYard));

			if (instance.ContainerYard != null)
			{
				parameters.Add(new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Location, instance.ContainerYard.OA_RL_NKRelatedPortCode));
			}

			parameters.Add(new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, instance.ReleaseType));

			return parameters.ToArray();
		}

		public void DoSelectAll()
		{
			foreach (ReleaseDetail detail in Details)
			{
				detail.ReleaseCount = detail.Container.JC_ContainerCount;
			}
		}

		void UpdateContainerReleaseLog()
		{
			if (Instances.Count > 0)
			{
				StringBuilder builder = new StringBuilder();

				builder.AppendFormat(CultureInfo.InvariantCulture, (NoResString)"{0:yyyy-MM-dd HH:mm} {1}", ZDateTime.Now, Instances[0].ReleaseNumber); // hard-coded constant

				for (int i = 1; i < Instances.Count; i++)
				{
					builder.AppendFormat(CultureInfo.InvariantCulture, ", {0}", Instances[i].ReleaseNumber);
				}

				builder.AppendLine();
				builder.Append(this.ContainerReleaseNote);

				StmNote[] notes = Shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.ContainerReleaseNote.Description);
				StmNote note = notes.Length > 0 ? notes[0] : null;

				if (note == null)
				{
					note = Shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.ContainerReleaseNote.Description, builder.ToString());
				}
				else
				{
					builder.AppendLine();
					builder.Append('-', 15);
					builder.AppendLine();
					builder.Append(note.ST_NoteText);

					note.ST_NoteText = builder.ToString();
				}
			}
		}

		#endregion

		#region Implementation

		ZString GetReleaseNumber(Dictionary<ZGuid, ZString> cache, ZGuid addressPK)
		{
			ZString result;

			if (IsReplacement)
			{
				result = ReleaseNumber;
			}
			else if (!cache.TryGetValue(addressPK, out result))
			{
				result = FindNextReleaseNumber();
				Lookups.ReleaseNumbers.AddPair(result);
				cache.Add(addressPK, result);
			}

			return result;
		}

		ZString FindNextReleaseNumber()
		{
			var prefix = shipment.JS_CFSReference;
			var suffix = FindLargestReleaseNumber() + 1;

			return prefix + "-" + suffix;
		}

		int FindLargestReleaseNumber()
		{
			var eventReferences = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ReleaseRequested.Code)).Select(x => x.ReferenceFreeText.ToString());
			var lookupsMax = ExtractMaxSuffix(Lookups.ReleaseNumbers.ToArray().Select(x => x.Code));
			var eventReferencesMax = ExtractMaxSuffix(eventReferences);

			return Math.Max(lookupsMax, eventReferencesMax);
		}

		int ExtractMaxSuffix(IEnumerable<string> releaseNumbers)
		{
			var suffixes = from value in releaseNumbers
						   let strings = value.Split('-')
						   let stringsLength = strings.Length
						   where stringsLength > 1
						   select Convert.ToInt32(strings[stringsLength - 1], CultureInfo.InvariantCulture);

			return suffixes.Any() ? suffixes.Max() : 0;
		}

		static decimal Split(decimal total, int oldCount, int newCount)
		{
			return total / (oldCount / (decimal)newCount);
		}

		internal ReleaseMessageStrategy[] MessageStrategies
		{
			get { return messageStrategies ?? (messageStrategies = ReleaseMessageHelper.GetStrategies(shipment)); }
		}

		ReleaseMessageStrategy[] messageStrategies;

		#endregion

		readonly AgencyBooking shipment;
	}
}
