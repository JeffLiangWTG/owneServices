using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class InBondMenuItemMessageData : AutoInBondMenuItemMessageData
	{
		public InBondMenuItemMessageData(BusinessObjectFactory factory, List<CusInBondMoveHeader> selectedMovementHeaders, InBondMenuItemMessageTypes messageType)
			: base(factory)
		{
			this.selectedMovementHeaders = selectedMovementHeaders;
			this.messageType = messageType;
		}
		readonly InBondMenuItemMessageTypes messageType;

		public enum InBondMenuItemMessageTypes { Arrival, Export, Pedimento, PrintDocument }

		List<CusInBondMoveHeader> SelectedMovementHeaders => selectedMovementHeaders ?? (selectedMovementHeaders = new List<CusInBondMoveHeader>());

		List<CusInBondMoveHeader> selectedMovementHeaders;

		public InBondMenuItemMessageSendingObjectCollection InBondMenuItemMessageSendingObjects
		{
			get
			{
				if (inBondMenuItemMessageSendingObjects == null)
				{
					inBondMenuItemMessageSendingObjects = new InBondMenuItemMessageSendingObjectCollection(this);
					inBondMenuItemMessageSendingObjects.Populate(SelectedMovementHeaders);
					RegisterEditableChildObject(inBondMenuItemMessageSendingObjects);
				}
				return inBondMenuItemMessageSendingObjects;
			}
		}

		InBondMenuItemMessageSendingObjectCollection inBondMenuItemMessageSendingObjects;

		[List(nameof(Lookups) + "+" + nameof(InBondMenuItemMessageDataLookups.RegionDistrictPorts))]
		public override ZString USDestinationPortCode
		{
			get => base.USDestinationPortCode;
			set
			{
				var oldValue = base.USDestinationPortCode;
				base.USDestinationPortCode = value;
				if (oldValue != value)
				{
					InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>().ForEach(x => x.USDestinationPortCodeInfo.RefreshBinding());
				}
			}
		}

		[List(nameof(Lookups) + "+" + nameof(InBondMenuItemMessageDataLookups.FIRMSCollection))]
		public override ZString FIRMSCode
		{
			get => base.FIRMSCode;
			set => base.FIRMSCode = value;
		}

		[List(nameof(Lookups) + "+" + nameof(InBondMenuItemMessageDataLookups.TransportModeCodes))]
		public override ZString ExportMOT
		{
			get => base.ExportMOT;
			set => base.ExportMOT = value;
		}

		[List(nameof(Lookups) + "+" + nameof(InBondMenuItemMessageDataLookups.ConveyanceList))]
		public override ZString ExportConveyance
		{
			get => base.ExportConveyance;
			set => base.ExportConveyance = value;
		}

		public InBondMenuItemMessageDataLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new InBondMenuItemMessageDataLookups(this);
				}
				return fLookups;
			}
		}
		InBondMenuItemMessageDataLookups fLookups;

		public CusInBondMoveHeader[] CreateOrUpdateMovementHeader()
		{
			var movementHeadersNeedToBeSent = new List<CusInBondMoveHeader>();

			foreach (var inBondMenuItemMessageSendingObject in this.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>())
			{
				var movementHeader = inBondMenuItemMessageSendingObject.MovementHeader;
				if (movementHeader == null)
				{
					var inBondHeader = GetCusInBondHeader(inBondMenuItemMessageSendingObject.ImporterOrgPK);
					movementHeader = inBondHeader.MovementHeaders.AddNew();
					movementHeader.InBondCarrierOrgPK = inBondMenuItemMessageSendingObject.InBondCarrierOrgPK;
					movementHeader.BM_InBondCarrierSCAC = inBondMenuItemMessageSendingObject.InBondCarrierCodeSCAC;
					movementHeader.BM_DestinationPortCode = inBondMenuItemMessageSendingObject.USDestinationPortCode;
					movementHeader.BM_ForeignDestPortKCode = inBondMenuItemMessageSendingObject.ForeignDestinationPortCode;
				}

				if (!inBondMenuItemMessageSendingObject.InBondNumber_ReadOnly)
				{
					movementHeader.InBondNumber = inBondMenuItemMessageSendingObject.InBondNumber;
				}
				if (movementHeader.BM_InBondEntryType != inBondMenuItemMessageSendingObject.EntryType)
				{
					movementHeader.BM_InBondEntryType = inBondMenuItemMessageSendingObject.EntryType;
				}

				if (!movementHeader.Header.IsSendCustomsMessageMutexLocked)
				{
					if (movementHeader.BM_DestinationPortCode.IsEmpty)
					{
						movementHeader.BM_DestinationPortCode = USDestinationPortCode;
					}
					if (IsArrival)
					{
						if (movementHeader.BM_FIRMS.IsEmpty)
						{
							movementHeader.BM_FIRMS = FIRMSCode;
						}
						if (movementHeader.BM_ArrivalDate.IsEmpty)
						{
							movementHeader.BM_ArrivalDate = ArrivalDate;
						}
					}
					if (IsExport)
					{
						if (movementHeader.BM_ExportDate.IsEmpty)
						{
							movementHeader.BM_ExportDate = ExportDate;
						}
						if (movementHeader.BM_ExportTransportMode.IsEmpty)
						{
							movementHeader.BM_ExportTransportMode = ExportMOT;
						}
						if (movementHeader.BM_ExportLadenOn.IsEmpty)
						{
							movementHeader.BM_ExportLadenOn = ExportConveyance;
						}
					}
				}

				movementHeadersNeedToBeSent.Add(movementHeader);
			}

			return movementHeadersNeedToBeSent.ToArray();
		}

		public (string, string) SendMessages(CusInBondMoveHeader[] movementHeadersNeedToBeSent)
		{
			var invalidOperationTextBuilder = new ZStringBuilder();
			var result = ZString.Empty;
			var numberOfSuccesses = 0;
			foreach (var movementHeader in movementHeadersNeedToBeSent)
			{
				var inBondNumber = movementHeader.InBondNumber;
				var header = movementHeader.Header;
				if (!header.IsSendCustomsMessageMutexLocked)
				{
					var inBondMessageType = header.ValidationModesCalculator.ConvertMessageType(messageType);
					var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
					var sendingHeaderObject = new InBondMessageSendingHeaderObject(header.PK, inBondMessageType, messageInitiator);
					var inBondMessageSendingObject = sendingHeaderObject.SendingObjects.Cast<InBondMessageSendingObject>().FirstOrDefault(x => x.US_InBondNumber.Trim() == inBondNumber);
					if (inBondMessageSendingObject != null)
					{
						if (header.LockSendCustomsMessageMutex())
						{
							try
							{
								inBondMessageSendingObject.US_ShouldSend = true;
								if (sendingHeaderObject.SendData())
								{
									numberOfSuccesses++;
								}
								if (!string.IsNullOrEmpty(messageInitiator.InvalidOperationText))
								{
									invalidOperationTextBuilder.AppendLine(messageInitiator.InvalidOperationText);
								}
							}
							finally
							{
								header.UnlockSendCustomsMessageMutex();
							}
						}
					}
				}
			}

			result = string.Format("{0} {1} been successfully sent.", numberOfSuccesses, GetNoun(numberOfSuccesses));
			var numberOfFailures = movementHeadersNeedToBeSent.Length - numberOfSuccesses;
			if (numberOfFailures != 0)
			{
				result = string.Format("{0} {1} {2} failed to be sent.", result, numberOfFailures, GetNoun(numberOfFailures));
			}

			return (invalidOperationTextBuilder.ToString().Trim(), result);
		}

		public void AllocatePredimentoNumber()
		{
			var inBondMenuItemMessageSendingObjects = this.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>();
			foreach (var inBondMenuItemMessageSendingObject in this.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>())
			{
				if (!inBondMenuItemMessageSendingObject.InBondNumber_ReadOnly)
				{
					inBondMenuItemMessageSendingObject.MovementHeader.InBondNumber = inBondMenuItemMessageSendingObject.InBondNumber;
				}

				var bills = inBondMenuItemMessageSendingObject.MovementHeader.MovementDetails.Select(x => x.Bill);
				foreach (var bill in bills)
				{
					foreach (var number in bill.AdditionalReferences.Where(x => x.BR_Qualifier == ReferenceQualifierList.Codes.FEN).ToArray())
					{
						bill.AdditionalReferences.Delete(number);
					}
					var fenNumber = bill.AdditionalReferences.AddNew();
					fenNumber.BR_Qualifier = ReferenceQualifierList.Codes.FEN;
					fenNumber.BR_ReferenceNum = inBondMenuItemMessageSendingObject.PedimentoNumber;
				}
			}
		}

		string GetNoun(int count)
		{
			return count == 1 ? "message has" : "messages have";
		}

		public CusInBondHeader GetCusInBondHeader(ZGuid importerPK)
		{
			CusInBondHeader result;
			if (!CusInBondHeaders.TryGetValue(importerPK, out result))
			{
				result = Factory.New<CusInBondHeader>();
				result.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
				result.BH_GB = GlbBranch.CurrentBranch.PK;
				result.BH_PostDepartureOnly = true;
				result.BH_OA_Importer_ZAddress.OrgPK = importerPK;
				CusInBondHeaders.Add(importerPK, result);
			}
			return result;
		}

		Dictionary<ZGuid, CusInBondHeader> CusInBondHeaders
		{
			get
			{
				if (cusInBondHeaders == null)
				{
					cusInBondHeaders = new Dictionary<ZGuid, CusInBondHeader>();
				}
				return cusInBondHeaders;
			}
		}
		Dictionary<ZGuid, CusInBondHeader> cusInBondHeaders;

		public bool IsArrival => messageType == InBondMenuItemMessageTypes.Arrival;

		public bool IsExport => messageType == InBondMenuItemMessageTypes.Export;

		public bool IsPedimento => messageType == InBondMenuItemMessageTypes.Pedimento;

		public bool IsPrintDocument => messageType == InBondMenuItemMessageTypes.PrintDocument;
	}
}
