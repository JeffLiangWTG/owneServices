using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.NumberFountain;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Business
{
	public class EDIMessage : Enterprise.Messaging.Business.EDIMessage
	{
		public EDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class ApplicationCodes : Enterprise.Messaging.Business.EDIMessage.ApplicationCodes
		{
			public const string USCustoms = "USC";
		}

		#region OriginalMessage

		public EDIMessage OriginalMessage
		{
			get
			{
				if (!IsTransmitMessage && !EM_MessageNum.IsEmpty && (originalMessage == null || originalMessage.EM_MessageNum != EM_MessageNum))
				{
					var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodes.USeManifest);
					query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, Direction.Transmit);
					query.AddToFilter(EDIMessageSchema.EM_MessageNum, EM_MessageNum);
					var messages = Factory.Load<EDIMessage>(query);

					var hasNotMatchedOriginalMessage = true;
					if (messages.Length > 1 && EM_MessageType == MessageTypes.Codes.eManifest && !EM_MessageOwner.IsEmpty)
					{
						foreach (var message in messages)
						{
							if (message.EM_LinkedObject is Trip trip && trip.BH_VoyageNumber == EM_MessageOwner)
							{
								originalMessage = message;
								hasNotMatchedOriginalMessage = false;
								break;
							}
						}
					}

					if (hasNotMatchedOriginalMessage)
					{
						originalMessage = messages.FirstOrDefault();
					}
				}
				return originalMessage;
			}
		}

		EDIMessage originalMessage;

		#endregion

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIInterchange.ApplicationCodes.USeManifest;
		}

		protected override CodeDescriptionPairList MessageSubTypeList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>(
					string.Format("US.eManifest.EDIMessage.MessageSubTypeList.{0}", EM_ReceiveTransmit),
					() =>
					{
						if (!IsTransmitMessage)
						{
							var result = new MessageTypes();
							result.AddRange(new TripEntryStatusList());
							result.AddRange(new ShipmentEntryStatusList());
							return result;
						}
						return new MessageActionCodes();
					});
			}
		}

		public override bool ShouldShowInterpretation
		{
			get { return true; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return new MessageTypes().GetDescriptionFromCode(EM_MessageType) ?? base.HumanReadableNameCore; }
		}

		protected override IEnumerable<Type> GetAdditionalRegisteredLinkedObjectTypes()
		{
			yield return typeof(Trip);
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (EDIMessage)base.CloneInternal(args);
			result.EM_SystemCreateTimeUtc = EM_SystemCreateTimeUtc;
			return result;
		}

		#region GetNumberFountainNumbersAndFillInPlaceHolders

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			base.GetNumberFountainNumbersAndFillInPlaceHolders();
			ReplaceMessageInterpretationPlaceHolders();
		}

		void ReplaceMessageInterpretationPlaceHolders()
		{
			var result = EM_MessageInterpretation.Replace(MessageNumberPlaceHolderHtml, EM_MessageNum);
			if (result.IndexOf(EntryNumberPlaceHolderHtml) != -1)
			{
				result = result.Replace(EntryNumberPlaceHolderHtml, GetEntryNumber());
			}
			EM_MessageInterpretation = result;
		}

		protected override string GetMessageReferenceNumber()
		{
			if (MessageNumberStrategy != null)
			{
				return MessageNumberStrategy.GetMessageReferenceNumber();
			}

			var messageReferenceNumber = ZString.Empty;
			var fountain = Env.NumberFountains.EDIFACTNumberFountain("M", EM_ApplicationCode, ApplicationCodes.USCustoms);
			var duplicateNumberFound = true;
			while (duplicateNumberFound)
			{
				try
				{
					messageReferenceNumber = fountain.GetNextFormatted(Factory);
				}
				catch (NumberFountainMaximumValueReachedException)
				{
					throw new ZCannotSaveException("There are no more available numbers for e-Manifest message.", "Cannot Allocate Message Number");
				}

				duplicateNumberFound = !IsManifestMessageNumberUnique(messageReferenceNumber);
			}

			return messageReferenceNumber;
		}

		ZBool IsManifestMessageNumberUnique(ZString messageReferenceNumber)
		{
			var query = new ZDBOnlyQuery(typeof(EDIMessage));
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodes.USeManifest);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_MessageNum, messageReferenceNumber);
			var messageWithSameNumber = Factory.LoadTop1<EDIMessage>(query);
			if (messageWithSameNumber != null)
			{
				if (messageWithSameNumber.EM_LinkedObject is Trip trip)
				{
					ErrorReporter.ReportOnce("Message number has been used.", $@"Message Number {messageReferenceNumber} has been used by e-Manifest job {trip.BH_JobReference}, a new message number will be allocated.
EM_PK: {PK}
EM_ApplicationCode: {EM_ApplicationCode}
EM_MessageType: {EM_MessageType}
EM_MessageSubType: {EM_MessageSubType}
");
				}

				return false;
			}

			return true;
		}

		protected override string GetEntryNumber()
		{
			var result = ZString.Empty;
			var trip = EM_LinkedObject as Trip;
			if (trip != null)
			{
				trip.PopulateJobReferenceIfNeeded();
				result = trip.BH_JobReference;
			}
			return result;
		}

		#endregion

		#endregion
	}
}
