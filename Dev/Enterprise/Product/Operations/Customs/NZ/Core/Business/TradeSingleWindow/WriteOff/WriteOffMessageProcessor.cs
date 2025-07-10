using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Registry;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	class WriteOffMessageProcessor : TSWMessageProcessor<WriteOffResponse>
	{
		internal WriteOffMessageProcessor(LoggingInformation logger, string outgoingMessageDescription)
			: base(logger, outgoingMessageDescription)
		{
		}

		#region Overrides

		protected override void ProcessCore()
		{
			Report.AddHeader(Response.MessageTypeDescription);
			Report.AddLine(Response.MessageParentTypeName, Response.MessageParentID);
			Report.AddLine("Entry Number", Response.DeclarationID);
			Report.AddLine("Master Bill", Response.MasterBill);
			Report.AddLine("Message No", Response.MessageNumber);
			Report.AddLine();
			OutputResponseStatus();

			if (Response.ShouldShowSummaryOnReport)
			{
				if (Response.AllConsignments.IsCountMoreThan(1))
				{
					if (Response.ConsignmentsWithoutResponse.IsCountMoreThan(0))
					{
						foreach (Consignment consignment in Response.ConsignmentsWithoutResponse)
						{
							if (consignment != null && !string.IsNullOrEmpty(consignment.HouseBill))
							{
								Report.AddLine();
								Report.AddLine("Summary", Response.ConsignmentsWrittenOff.Count() + " out of " + Response.AllConsignments.Count() + " Jobs have been Written Off.");
								break;
							}
						}
					}
					else
					{
						Report.AddLine();
						Report.AddLine("Summary", Response.ConsignmentsWrittenOff.Count() + " out of " + Response.AllConsignments.Count() + " Jobs have been Written Off.");
					}
				}
			}

			ProcessCustomsInstructions();
			OutputErrors();
			ProcessJobResponses();
		}

		protected override string[] ExpectedDODocumentNames
		{
			get { return new string[] { "" }; }
		}

		protected override ZGuid DocumentParentPK
		{
			get { return ZGuid.Empty; }
		}

		protected override ZString DocumentParentType
		{
			get { return ZString.Empty; }
		}

		protected override void ProcessCustomsInstructions()
		{
			base.ProcessCustomsInstructions();

			var customsDeliveryInstructions = string.Empty;
			if (!Response.AdditionalInstructions.IsNullOrEmpty())
			{
				if (Response.CustomsInstructions.IsNullOrEmpty())
				{
					Report.AddHeader("Customs Instructions");
				}

				Response.AdditionalInstructions.ForEach(OutputCustomsInstruction);
				if (!Response.AdditionalInstructions.IsNullOrEmpty())
				{
					customsDeliveryInstructions += string.Join(System.Environment.NewLine, Response.AdditionalInstructions);
				}
			}

			var responseForDirectMaster = false;
			foreach (var consignment in Response.ConsignmentsWithoutResponse)
			{
				if (consignment == null || string.IsNullOrEmpty(consignment.HouseBill))
				{
					responseForDirectMaster = true;
					break;
				}
			}

			if (Response.AllConsignments.Any() && Response.AllConsignments.ElementAt(0) != null)
			{
				if (!Response.AdditionalInstructions.IsNullOrEmpty())
				{
					customsDeliveryInstructions +=
						System.Environment.NewLine +
						string.Join(System.Environment.NewLine, Response.AllConsignments.IsCountEqualTo(1) || responseForDirectMaster
					? "Response Status: " + Response.AllConsignments.ElementAt(0).EnterpriseStatusDescription
					: string.Join("   ", Response.AllConsignments.Select(consignment =>
						consignment.JobNumber + ":-<" + consignment.EnterpriseStatus + ">")));
				}
				else
				{
					customsDeliveryInstructions = Response.AllConsignments.IsCountEqualTo(1) || responseForDirectMaster
						? "Response Status: " + Response.AllConsignments.ElementAt(0).EnterpriseStatusDescription
						: string.Join("   ", Response.AllConsignments.Select(consignment =>
							consignment.JobNumber + ":-<" + consignment.EnterpriseStatus + ">"));
				}

				if (!Response.CustomsInstructions.IsNullOrEmpty())
				{
					customsDeliveryInstructions +=
						System.Environment.NewLine +
						string.Join(System.Environment.NewLine, Response.CustomsInstructions);
				}

				Response.CustomsDeliveryInstructions = customsDeliveryInstructions;
			}
		}

		protected override void WriteEntryStatus()
		{
			Response.UpdateECINumber();
			Response.UpdateCustomsStatus();
		}

		protected override void SendEmail(EmailDef email)
		{
			var impedimentOnConsignment = false;
			if (Response.AllConsignments.IsCountMoreThan(1))
			{
				foreach (var consignment in Response.AllConsignments)
				{
					if (consignment != null)
					{
						if (LowValueConsignmentStatusList.LastStatusIsImpediment(consignment.EnterpriseStatus))
						{
							SendImpedimentReport(Response.LinkedObject, email);
							impedimentOnConsignment = true;
							break;
						}
					}
				}
			}

			if (!impedimentOnConsignment)
			{
				if (!Response.ErrorsWithPointers.IsNullOrEmpty())
				{
					SendErrorReport(Response.LinkedObject, email);
				}
				else
				{
					SendAcknowledgementReport(Response.LinkedObject, email);
				}
			}
		}

		#region Email Setup Overrides

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return Response.EntryBeingProcessedIsImportEntry ? NZCustomsDataRegistry.Instance.ImportEciSendAcknowledgementsToGroup.Value : NZCustomsDataRegistry.Instance.ExportEciSendAcknowledgementsToGroup.Value; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return Response.EntryBeingProcessedIsImportEntry ? NZCustomsDataRegistry.Instance.ImportEciSendAcknowledgements.Value : NZCustomsDataRegistry.Instance.ExportEciSendAcknowledgements.Value; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return Response.EntryBeingProcessedIsImportEntry ? NZCustomsDataRegistry.Instance.ImportEciSendImpedimentsToGroup.Value : NZCustomsDataRegistry.Instance.ExportEciSendImpedimentsToGroup.Value; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return Response.EntryBeingProcessedIsImportEntry ? NZCustomsDataRegistry.Instance.ImportEciSendImpediments.Value : NZCustomsDataRegistry.Instance.ExportEciSendImpediments.Value; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return Response.EntryBeingProcessedIsImportEntry ? NZCustomsDataRegistry.Instance.ImportEciSendErrorsToGroup.Value : NZCustomsDataRegistry.Instance.ExportEciSendErrorsToGroup.Value; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return Response.EntryBeingProcessedIsImportEntry ? NZCustomsDataRegistry.Instance.ImportEciSendErrors.Value : NZCustomsDataRegistry.Instance.ExportEciSendErrors.Value; }
		}
		#endregion

		#endregion // Overrides

		#region Implementation

		void ProcessJobResponses()
		{
			var firstConsignment = true;
			foreach (var consignment in Response.ConsignmentsWithResponse)
			{
				if (firstConsignment)
				{
					Report.AddHeader("Job Responses");
					firstConsignment = false;
				}
				else
				{
					Report.AddLine();
				}

				if (!consignment.JobNumber.IsNullOrEmpty() || !consignment.HouseBill.IsNullOrEmpty())
				{
					Report.AddLine("Job Number: " + consignment.JobNumber + "   House Bill: " + consignment.HouseBill);
				}

				if (consignment.EnterpriseStatus == LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff)
				{
					Report.AddLine("--- Clearance Status: " + consignment.EnterpriseStatusDescription + " ---");
				}
				else
				{
					var movementStatusDescription = consignment.MovementStatusDescription;
					if (!movementStatusDescription.IsNullOrEmpty() && consignment.JobNumber.IsNullOrEmpty())
					{
						Report.AddLine("--- Movement Status: " + movementStatusDescription + " ---");
					}
					else
					{
						Report.AddLine("--- Clearance Status: " + consignment.EnterpriseStatusDescription + " ---");
						if (!movementStatusDescription.IsNullOrEmpty())
						{
							Report.AddLine("--- Movement Status: " + movementStatusDescription + " ---");
						}
					}
				}

				consignment.CustomsStatus = consignment.EnterpriseStatus;
			}
		}

		#endregion // Implementation
	}
}
