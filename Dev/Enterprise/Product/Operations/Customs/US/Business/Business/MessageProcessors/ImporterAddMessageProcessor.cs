using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.Add5106toImporterFileProcessingResults)]
	public class ImporterAddMessageProcessor : ACEABIProcessor
	{
		const string ImporterAlreadyOnFile = "ACY";

		public override void Process()
		{
			var importerNumber = ZString.Empty;
			var importerName = ZString.Empty;

			OrgHeader organisation = Message.OriginalMessage != null ? (OrgHeader)Message.OriginalMessage.EM_LinkedObject : null;
			Message.EM_LinkedObject = organisation;

			HtmlTableCreator htmlBody = null;
			HtmlTableCreator errorTable = null;
			ZStringBuilder emailBody = new ZStringBuilder();
			bool isError = false;
			List<ZString> errorCodes = new List<ZString>();
			foreach (MessageBlock block in messageBlocks)
			{
				ADDT345 t345 = block as ADDT345;
				if (t345 != null)
				{
					if (t345.RecordType == 5 || t345.RecordType == 4 || t345.NarrativeMessage.Left(3) == ImporterAlreadyOnFile)
					{
						if (htmlBody == null)
						{
							htmlBody = new HtmlTableCreator(new string[] { "Message" });
						}
						importerNumber = t345.ImporterNumber;
						importerName = t345.ImporterName;
						if (t345.NarrativeMessage.Left(3) != ImporterAlreadyOnFile)
						{
							htmlBody.WriteRow(t345.NarrativeMessage);
						}
					}
					else if (t345.RecordType == 3)
					{
						isError = true;
						if (errorTable == null)
						{
							errorTable = new HtmlTableCreator(new string[] { "Severity Code", "Error Code", "Message" });
						}
						var errorCode = t345.NarrativeMessage.Left(3);
						var shortDescription = t345.NarrativeMessage.Right(t345.NarrativeMessage.Length - 3);
						errorTable.WriteRow("Error", errorCode, MessageCalculator.GetLongDescription(errorCode, shortDescription));
						errorCodes.Add(t345.NarrativeMessage.Left(3));
					}
				}
			}

			if (!importerNumber.IsEmpty)
			{
				string message = null;

				if (!isError)
				{
					message = "The importer number, '" + importerNumber + "' is registered at Customs.";
				}

				if (organisation == null)
				{
					message += "System could not attach this number to any Organization records." + (importerName.IsEmpty ? "" : " This number however is said to belong to this name, '" + importerName + "'.");
				}

				if (message != null)
				{
					htmlBody.WriteRow(message);
				}
			}

			if (organisation != null)
			{
				OrgHeaderWrapper orgWrapper = OrgHeaderWrapper.New(organisation);
				SaveImporterNumber(orgWrapper, isError, importerNumber);

				if (!isError)
				{
					orgWrapper.ZO_IsEINNumberVerifiedIndicator = YesNoDefaultList.Codes.Yes;
				}
				else
				{
					orgWrapper.ZO_IsEINNumberVerifiedIndicator = YesNoDefaultList.Codes.No;
				}
			}

			string jobNumber = "Organisation Unknown";
			string uri = "";

			if (organisation != null)
			{
				jobNumber = "Organisation " + organisation.OH_Code;
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Organisation, organisation.PK.ToGuid());
			}

			if (htmlBody != null)
			{
				emailBody.Append(htmlBody.ToHtml());
				emailBody.Append("<br>");
			}

			if (errorTable != null)
			{
				emailBody.Append(errorTable.ToHtml());
			}
			var branch = Message.OriginalMessage != null ? Message.OriginalMessage.Branch : GlbBranch.CurrentBranch;
			GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, "Importer Add", emailBody.ToString(), isError, branch, organisation);
		}

		#region Implementation

		void SaveImporterNumber(OrgHeaderWrapper organisationWrapper, bool isErrorResponse, ZString importerNumber)
		{
			if (!importerNumber.IsEmpty)
			{
				//NN-NNNNNNNXX		EIN (aka IRS) Number
				//NNN-NN-NNNN		Social Security Number
				//YYDDPP-NNNNN		CBP Assigned Number

				if (importerNumber.IndexOf('-') == 6)//CBP Assigned Number
				{
					organisationWrapper.organisation.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.CBPAssignedNumber, importerNumber, Core.Constants.CountryCodes.UnitedStates);
				}
			}
		}

		protected override List<ZString> ExcludedErrorCodes
		{
			get
			{
				var result = base.ExcludedErrorCodes;
				result.Add("2EE");
				return result;
			}
		}

		#endregion
	}
}
