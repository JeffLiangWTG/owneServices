using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Edifact.D98A.Elements;
using Enterprise.Edifact.D98A.Messages.CUSRES;
using Enterprise.Edifact.D98A.Segments;
using NZCMessage = Enterprise.Customs.NZ.Business.Declaration.NZCMessage; // That's really the base.

namespace Enterprise.Customs.NZ.Business.MessageProcessors.ECIWriteOff
{
	class ECIMessageParser
	{
		public ECIMessageParser(MessageProcessors.MessageProcessor messageProcessor, ResponseEmailBuilder builder, ConsignmentWrapperCollection consignmentWrappers)
		{
			processor = messageProcessor;
			ConsignmentWrappers = consignmentWrappers;
			Builder = builder;
			CancellationWasAccepted = false;
		}
		readonly MessageProcessors.MessageProcessor processor;
		public readonly ConsignmentWrapperCollection ConsignmentWrappers;
		public readonly ResponseEmailBuilder Builder;

		public abstract class ConsignmentWrapper
		{
			protected ConsignmentWrapper()
			{
				NewConsignmentStatus = ZString.Empty;
			}

			public ZString NewConsignmentStatus;

			public abstract ZString ConsignmentReference { get; }
			public abstract ZString ManifestLineNumber { get; }
			public abstract ZString HouseBill { get; }
			public abstract ZString ConsignmentStatus { get; set; }
			public abstract ZString LastConsignmentStatus { get; set; }

			public abstract void LogCustomsClearedIfNeeded();
			public abstract void LogCustomsImpediment();
		}

		class CusHAWBWrapper : ConsignmentWrapper
		{
			public CusHAWBWrapper(CusHAWB hawb)
				: base()
			{
				hAWB = hawb;
			}
			readonly CusHAWB hAWB;

			public override ZString ConsignmentReference
			{
				get { return hAWB.ConsignmentReference; }
			}

			public override ZString ManifestLineNumber
			{
				get { return hAWB.CS_ConsignmentNum.ToString(); }
			}

			public override ZString HouseBill
			{
				get { return hAWB.CS_HAWB; }
			}

			public override ZString ConsignmentStatus
			{
				get { return hAWB.CS_CustomsStatus; }
				set { hAWB.CS_CustomsStatus = value; }
			}

			public override ZString LastConsignmentStatus
			{
				get { return hAWB.CS_CustomsMainStatus; }
				set { hAWB.CS_CustomsMainStatus = value; }
			}

			public override void LogCustomsClearedIfNeeded()
			{
				//Do Nothing for now until Richard Assigns Events
			}

			public override void LogCustomsImpediment()
			{
				//Do Nothing for now until Richard Assigns Events
			}
		}

		class JobDeclarationWrapper : ConsignmentWrapper
		{
			public JobDeclarationWrapper(JobDeclaration declaration)
				: base()
			{
				this.declaration = declaration;
			}
			readonly JobDeclaration declaration;

			public override ZString ConsignmentReference
			{
				get { return declaration.JE_DeclarationReference; }
			}

			public override ZString ManifestLineNumber
			{
				get { return declaration.ECIManifestLineNumber; }
			}

			public override ZString HouseBill
			{
				get { return declaration.JE_HouseBill; }
			}

			public override ZString ConsignmentStatus
			{
				get { return declaration.JE_EntryStatus; }
				set { declaration.JE_EntryStatus = value; }
			}

			public override ZString LastConsignmentStatus
			{
				get { return declaration.JE_ECI_LastResponseStatus; }
				set { declaration.JE_ECI_LastResponseStatus = value; }
			}

			public override void LogCustomsClearedIfNeeded()
			{
				declaration.LogCustomsClearedIfNeeded();
			}

			public override void LogCustomsImpediment()
			{
				declaration.LogCustomsImpediment();
			}
		}

		public class ConsignmentWrapperCollection : Dictionary<string, ConsignmentWrapper>
		{
			public ConsignmentWrapperCollection(CusMAWB mawb)
			{
				foreach (CusHAWB hawb in mawb.ChildBills)
				{
					Add(new CusHAWBWrapper(hawb));
				}
			}

			public ConsignmentWrapperCollection(JobDeclaration declaration)
			{
				Add(new JobDeclarationWrapper(declaration));
			}

			public ConsignmentWrapperCollection(JobDeclarationCollectionECIWriteOff declarations)
			{
				foreach (JobDeclaration declaration in declarations)
				{
					Add(new JobDeclarationWrapper(declaration));
				}
			}

			void Add(ConsignmentWrapper wrapper)
			{
				if (!ContainsKey(wrapper.ManifestLineNumber))
				{
					Add(wrapper.ManifestLineNumber, wrapper);
				}
			}
		}

		bool jobHeaderHasBeenOutput;
		int jobCount;
		int jobsWrittenOff;
		ZString responseTypeCode;
		public bool CancellationWasAccepted;
		public ZString EntryNumber;
		public ZString NewManifestStatus;
		public ZString CustomsInstructions;

		public void ProcessGroup0(CUSRESMessage cUSRESMessage)
		{
			jobHeaderHasBeenOutput = false;
			jobCount = ConsignmentWrappers.Count;
			jobsWrittenOff = 0;

			UNHSegment uNHSegment = cUSRESMessage.UNH[0];
			BGMSegment bGMSegment = cUSRESMessage.BGM[0];
			GISSegment gISSegment = cUSRESMessage.GIS[0];

			processor.CheckRequiredSegmentNotNull(uNHSegment);
			processor.CheckRequiredSegmentNotNull(bGMSegment);
			processor.CheckRequiredSegmentNotNull(gISSegment);

			string jobNumber = uNHSegment.CommonAccessReference;
			string processingIndicatorCode = gISSegment.ProcessingIndicator.ProcessingIndicatorCoded.ToString();
			responseTypeCode = bGMSegment.DocumentMessageName.DocumentMessageNameCoded.ToString();
			string entryNumber = bGMSegment.DocumentMessageIdentification.DocumentMessageNumber;

			Builder.OutputHeaderLine(ECIProcessing.GetResponseTypeFromCode(responseTypeCode));
			Builder.OutputHeaderBreakLine();
			Builder.OutputHeaderLine(processor.JobTypeDescription, jobNumber);
			Builder.OutputHeaderLine("Entry Number", entryNumber);
			Builder.OutputHeaderLine("Master Bill", processor.GetMasterBill());
			Builder.OutputHeaderLine("Message No", uNHSegment.MessageReferenceNumber);
			Builder.OutputHeaderLine();

			ZInt entryNumberAsZInt;
			if (ZInt.TryParse(entryNumber, out entryNumberAsZInt) && entryNumberAsZInt != 0)
			{
				EntryNumber = entryNumber;
			}
			CancellationWasAccepted = (responseTypeCode == ResponseTypeList.Codes.ConfirmationOfAdjustment && LastOutgoingMessageWasACancellation);
			if (CancellationWasAccepted)
			{
				Builder.OutputHeaderLine("Message Status", "(830) Adjustment Accepted. Entry is now CANCELLED.");
			}
			else
			{
				Builder.OutputMessageStatus(processingIndicatorCode);

				if (responseTypeCode == ResponseTypeList.Codes.RejectionReport)
				{
					NewManifestStatus = LowValueManifestStatusList.Codes.ManifestRejected;
				}
			}

			if (cUSRESMessage.FTX.Count > 0)
			{
				FTXSegment fTXSegment = cUSRESMessage.FTX[0];
				ProcessCustomsInstructions(fTXSegment.TextLiteral);
			}

			if (!CancellationWasAccepted)
			{
				if (cUSRESMessage.Group4.Count > 0)
				{
					Builder.OutputBodyHeader("Message Errors");

					foreach (SegmentGroup4 group4 in cUSRESMessage.Group4)
					{
						processor.ProcessGroup4(group4); // Rejection Report
					}

					foreach (ConsignmentWrapper declarationWrapper in ConsignmentWrappers.Values)
					{
						SetECIStatus(declarationWrapper, LowValueConsignmentStatusList.Codes.ConsignmentInError);
					}
				}

				if (cUSRESMessage.Group6.Count > 0)
				{
					foreach (SegmentGroup6 group6 in cUSRESMessage.Group6)
					{
						ProcessGroup6(group6); // ECI Consignment Level Status Report
					}
				}

				foreach (ConsignmentWrapper declarationWrapper in ConsignmentWrappers.Values)
				{
					if (declarationWrapper.NewConsignmentStatus.IsEmpty)
					{
						ProcessJobWithNoResponse(declarationWrapper);
					}
				}

				if (jobCount > 1)
				{
					Builder.OutputHeaderLine();
					Builder.OutputHeaderLine("Summary", jobsWrittenOff.ToString() + " out of " + jobCount.ToString() + " Jobs have been Written Off.");
				}
			}

			UNTSegment uNTSegment = cUSRESMessage.UNT[0];
			processor.CheckRequiredSegmentNotNull(uNTSegment);

			UpdateCustomsInstructionsNote();
		}

		protected void ProcessGroup6(SegmentGroup6 group6)
		{
			foreach (DOCSegment dOC in group6.DOC)
			{
				string consignmentNumber = dOC.DocumentMessageDetails.DocumentMessageNumber;
				ConsignmentWrapper consignmentWrapper = null;
				ConsignmentWrappers.TryGetValue(consignmentNumber, out consignmentWrapper);

				string jobNumber = consignmentWrapper == null ? new ZString("UNKNOWN CONSIGNMENT NUMBER: " + consignmentNumber) : consignmentWrapper.ConsignmentReference;
				string houseBill = dOC.DocumentMessageDetails.DocumentMessageSource;
				string statusCode = dOC.DocumentMessageName.DocumentMessageNameCoded.ToString();

				if (consignmentWrapper != null)
				{
					SetECIStatus(consignmentWrapper, statusCode);
				}

				OutputJobResponse(jobNumber, houseBill, statusCode);
			}

			foreach (SegmentGroup13 group13 in group6.Group13)
			{
				ProcessGroup13(group13); // Consignment Error Report
			}
		}

		protected void ProcessGroup13(SegmentGroup13 segmentGroup13)
		{
			ERPSegment eRPSegment = segmentGroup13.ERP[0];
			ERCSegment eRCSegment = segmentGroup13.ERC[0];
			processor.CheckRequiredSegmentNotNull(eRPSegment);
			processor.CheckRequiredSegmentNotNull(eRCSegment);

			string errorSection = eRPSegment.ErrorPointDetails.MessageSectionCoded.ToString();
			string fieldCode = eRPSegment.ErrorPointDetails.MessageSubItemNumber;
			string errorCode = eRCSegment.ApplicationErrorDetail.ApplicationErrorIdentification;

			string errorPlace = ECIProcessing.GetLineErrorPointFromCode(errorSection);
			string fieldDescription = processor.GetFieldNameFromCode(fieldCode);
			string errorDescription = processor.GetErrorDecriptionFromCode(eRCSegment.ApplicationErrorDetail.ApplicationErrorIdentification);

			Builder.OutputBodyLine("**Error** in " + errorPlace + ", " + fieldDescription + ":-\r\n  " + errorDescription + ".");
		}

		protected bool LastOutgoingMessageWasACancellation
		{
			get
			{
				NZCMessage lastOutgoingMessage = processor.GetLastOutgoingMessage();
				return lastOutgoingMessage != null && lastOutgoingMessage.EM_MessageSubType == NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Cancellation;
			}
		}

		protected void ProcessJobWithNoResponse(ConsignmentWrapper consignmentWrapper)
		{
			SetECIStatus(consignmentWrapper, LowValueConsignmentStatusList.Codes.NoStatusReported);
			OutputJobResponse(consignmentWrapper);
		}

		protected void OutputJobResponse(ConsignmentWrapper consignmentWrapper)
		{
			OutputJobResponse(consignmentWrapper.ConsignmentReference, consignmentWrapper.HouseBill, consignmentWrapper.NewConsignmentStatus);
		}

		protected void OutputJobResponse(ZString jobNumber, ZString houseBill, ZString statusCode)
		{
			OutputJobResponseHeadersIfRequired();
			Builder.OutputBodyLine("Job Number: " + jobNumber + "   House Bill: " + houseBill);
			if (statusCode == LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff)
			{
				Builder.OutputBodyLine("--- Clearance Status: " + ECIProcessing.GetStatusDescription(statusCode) + " ---");
				jobsWrittenOff += 1;
			}
			else
			{
				Builder.OutputBodyLine("--- Clearance Status: " + ECIProcessing.GetStatusDescription(statusCode) + " ---");
			}
		}

		protected void OutputJobResponseHeadersIfRequired()
		{
			if (!jobHeaderHasBeenOutput)
			{
				Builder.OutputBodyHeader("Job Responses");
				jobHeaderHasBeenOutput = true;
			}
			else
			{
				Builder.OutputBodyLine("");
			}
		}

		protected void ProcessCustomsInstructions(TextLiteralElements textLiteralElements)
		{
			Builder.OutputBodyHeader("Customs Instructions");
			Builder.OutputTextLiteralElements(textLiteralElements);
			CustomsInstructions = Builder.GetNoteText(textLiteralElements);
		}

		protected void SetECIStatus(ConsignmentWrapper consignmentWrapper, string newResponseStatus)
		{
			consignmentWrapper.NewConsignmentStatus = ECIProcessing.GetNewConsignmentECIStatus(consignmentWrapper.LastConsignmentStatus, newResponseStatus);
		}

		protected void UpdateCustomsInstructionsNote()
		{
			ZStringBuilder statuses = new ZStringBuilder();
			if (ConsignmentWrappers.Values.Count == 1)
			{
				foreach (ConsignmentWrapper consignmentWrapper in ConsignmentWrappers.Values)
				{
					statuses.Append("Response Status: " + ECIProcessing.GetStatusDescription(consignmentWrapper.NewConsignmentStatus));
				}
			}
			else
			{
				foreach (ConsignmentWrapper consignmentWrapper in ConsignmentWrappers.Values)
				{
					statuses.Append(consignmentWrapper.ConsignmentReference + ":-<" + ECIProcessing.GetStatusDescription(consignmentWrapper.NewConsignmentStatus) + ">   ");
				}
			}

			ZString deliveryInstructions = statuses.ToString().TrimEnd();
			if (!CustomsInstructions.IsEmpty)
			{
				deliveryInstructions += "\r\n" + CustomsInstructions;
			}

			processor.CustomsDeliveryInstructions = deliveryInstructions;
		}

		public void WriteNewStatusesToManifestAndConsignments()
		{
			if (!EntryNumber.IsEmpty && processor.EntryNumber != EntryNumber)
			{
				processor.EntryNumber = EntryNumber;
			}

			if (CancellationWasAccepted)
			{
				processor.EntryStatus = LowValueManifestStatusList.Codes.ManifestCancelled;
				foreach (ConsignmentWrapper consignmentWrapper in ConsignmentWrappers.Values)
				{
					consignmentWrapper.ConsignmentStatus = LowValueConsignmentStatusList.Codes.ConsignmentCancelled;
					consignmentWrapper.LastConsignmentStatus = LowValueConsignmentStatusList.Codes.ConsignmentCancelled;
				}

				processor.FinaliseCancellation();
			}
			else
			{
				ZString resultingEntryStatus = NewManifestStatus;
				foreach (ConsignmentWrapper consignmentWrapper in ConsignmentWrappers.Values)
				{
					if (consignmentWrapper.ConsignmentStatus != FormalEntryStatusList.Codes.DeliveryOrderReceived && consignmentWrapper.ConsignmentStatus != LowValueConsignmentStatusList.Codes.FormalDeclarationRequired)  // Any previous consignments that required a formal declaration are not to override status
					{
						if (!consignmentWrapper.NewConsignmentStatus.IsEmpty)
						{
							consignmentWrapper.ConsignmentStatus = consignmentWrapper.NewConsignmentStatus;
							consignmentWrapper.LastConsignmentStatus = consignmentWrapper.NewConsignmentStatus;

							if (resultingEntryStatus != LowValueManifestStatusList.Codes.ManifestRejected)
							{
								switch (consignmentWrapper.NewConsignmentStatus)
								{
									case LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff:
										consignmentWrapper.LogCustomsClearedIfNeeded();
										if (resultingEntryStatus != LowValueManifestStatusList.Codes.InspectionsAuditRequirements && resultingEntryStatus != LowValueManifestStatusList.Codes.ManifestInError)
										{
											resultingEntryStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
										}

										break;
									case LowValueConsignmentStatusList.Codes.FormalDeclarationRequired:
										consignmentWrapper.LogCustomsImpediment();
										if (resultingEntryStatus != LowValueManifestStatusList.Codes.InspectionsAuditRequirements && resultingEntryStatus != LowValueManifestStatusList.Codes.ManifestInError)
										{
											resultingEntryStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
										}

										break;
									case LowValueConsignmentStatusList.Codes.ConsignmentHeld:
										consignmentWrapper.LogCustomsImpediment();
										if (resultingEntryStatus != LowValueManifestStatusList.Codes.ManifestInError)
										{
											resultingEntryStatus = LowValueManifestStatusList.Codes.InspectionsAuditRequirements;
										}

										break;
									case LowValueConsignmentStatusList.Codes.ConsignmentInError:
										resultingEntryStatus = LowValueManifestStatusList.Codes.ManifestInError;
										break;
								}
							}
						}
					}
				}

				if (!resultingEntryStatus.IsEmpty)
				{
					processor.EntryStatus = resultingEntryStatus;
				}
			}
		}
	}
}
