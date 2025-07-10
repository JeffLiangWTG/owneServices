using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business.DIS;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.DIS
{
	public class CBPRequestValueProvider
	{
		public CBPRequestValueProvider(IEntryHeaderParentBusinessObject parentBusinessObject)
		{
			this.parentBusinessObject = parentBusinessObject;
		}

		readonly IEntryHeaderParentBusinessObject parentBusinessObject;

		public IEnumerable<IDISCBPRequestDefault> CBPRequests
		{
			get
			{
				foreach (var wrapper in
					from ErrorsRecord record in parentBusinessObject.ENSStatusNotifications
					where !record.ActionIDNumber.IsEmpty && ENSStatusDispositionCodeListLoader.IsFurtherActionRequired(parentBusinessObject.Factory, record.DispositionCode)
					select new CBPRequestWrapper() { ID = record.ActionIDNumber, RequestDate = record.StatusDate, Description = GetHumanFriendlyDescription(record, ENSStatusDispositionCodeListLoader.GetENSStatusDispositionCodeList(parentBusinessObject.Factory)) }
				)
				{
					yield return wrapper;
				}

				foreach (var wrapper in GetDISCBPRequestListFromSOMessage())
				{
					yield return wrapper;
				}
			}
		}

		static string GetHumanFriendlyDescription(ErrorsRecord record, CodeDescriptionPairList list)
		{
			return "Requested on " + record.StatusDate.ToString("MM-dd-yy") + " (" + list.GetDescriptionFromCode(record.DispositionCode) + ")";
		}

		#region GetDISCBPRequestListFromSOMessage

		static string GetCBPRequestID(ASESSO60 so60)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", so60.DispositionActionCode, so60.DocumentType, so60.DispositionActionDate.ToString("MMddyy", CultureInfo.InvariantCulture));
		}

		static string GetCBPRequestID(IPGADispositionProvider so70)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", so70.PGALineDispositionCode, so70.DocumentTypeCode, so70.DispositionDateTime.ToString("MMddyy", CultureInfo.InvariantCulture));
		}

		static string GetHumanFriendlyDescription(ASESSO60 so60, CargoReleaseRequiredDocumentTypeList list)
		{
			return string.Format(CultureInfo.InvariantCulture, "Requested on {0} ({1}{2}{3})",
							so60.DispositionActionDate.ToString("MM-dd-yy", CultureInfo.InvariantCulture),
							so60.NarrativeMessage, so60.DocumentType.IsEmpty ? "" : " - ",
							list.GetDescriptionFromCode(so60.DocumentType));
		}

		static string GetHumanFriendlyDescription(IPGADispositionProvider so70, CargoReleaseRequiredDocumentTypeList list)
		{
			return string.Format(CultureInfo.InvariantCulture, "Requested on {0} ({1}{2}{3})",
							so70.DispositionDateTime.ToString("MMddyy", CultureInfo.InvariantCulture),
							so70.NarrativeMessage, so70.DocumentTypeCode.IsEmpty ? "" : " - ",
							list.GetDescriptionFromCode(so70.DocumentTypeCode));
		}

		IEnumerable<IDISCBPRequestDefault> GetDISCBPRequestListFromSOMessage()
		{
			List<IDISCBPRequestDefault> requestList = new List<IDISCBPRequestDefault>();

			var aceCargoReleaseEntry = parentBusinessObject.SimplifiedEntry;
			if (aceCargoReleaseEntry != null)
			{
				foreach (MQEDIMessage message in aceCargoReleaseEntry.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.USCustomsImport,
								new ZString[] { Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus },
								EDIMessage.Direction.Receive))
				{
					foreach (var so60 in message.MessageBlock.MessageBlocks.OfType<ASESSO60>().Where(so60 => CargoReleaseProcessingResultList.IsDocRequiredDispositionCode(so60.DispositionActionCode)))
					{
						var id = GetCBPRequestID(so60);
						if (!requestList.Any(r => r.ID == id))
						{
							requestList.Add(new CBPRequestWrapper()
							{
								ID = id,
								RequestDate = so60.DispositionActionDate,
								Description = GetHumanFriendlyDescription(so60, parentBusinessObject.Factory.GetCachedValue<CargoReleaseRequiredDocumentTypeList>())
							});
						}
					}
					foreach (var so70 in message.MessageBlock.MessageBlocks.OfType<IPGADispositionProvider>().Where(so70 => PGADispositionCodeList.IsDocRequiredPGADispositionCode(so70.EntryDispositionCode) || PGADispositionCodeList.IsDocRequiredPGADispositionCode(so70.PGALineDispositionCode)))
					{
						var id = GetCBPRequestID(so70);
						if (!requestList.Any(r => r.ID == id))
						{
							requestList.Add(new CBPRequestWrapper()
							{
								ID = id,
								RequestDate = so70.DispositionDateTime.Date,
								Description = GetHumanFriendlyDescription(so70, parentBusinessObject.Factory.GetCachedValue<CargoReleaseRequiredDocumentTypeList>())
							});
						}
					}
				}
			}

			return requestList;
		}

		#endregion

		class CBPRequestWrapper : IDISCBPRequestDefault
		{
			public ZString ID
			{
				get;
				internal set;
			}

			public ZDateTime RequestDate
			{
				get;
				internal set;
			}

			public ZString Description
			{
				get;
				internal set;
			}

			public CBPRequestType Type
			{
				get { return CBPRequestType.ACEActionNumber; }
			}
		}
	}
}
