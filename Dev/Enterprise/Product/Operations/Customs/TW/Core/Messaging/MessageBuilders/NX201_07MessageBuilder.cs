using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.NX201_07;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.TW.Messaging.MessageBuilders
{
	[CodeAlive("NX201_07MessageBuilder will be created later")]
	public class NX201_07MessageBuilder : BaseTWMessageBuilder<INX201_07, Declaration>
	{
		public override Declaration PopulateDeclaration(INX201_07 input, string functionCode = null)
		{
			var newItem = new Declaration();
			if (input != null)
			{
				newItem.FunctionalReferenceId = new DeclarationFunctionalReferenceId() { Value = input.FunctionalReferenceID };
				newItem.FunctionCode = new DeclarationFunctionCode() { Value = functionCode ?? string.Empty };
				newItem.IssueDateTime = input.IssueDateTime.ToString("yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
				PopulateAdditionalDocument(newItem, input.AdditionalDocument);
				PopulateAdditionalInformation(newItem, input.AdditionalInformation);
				PopulateTW_Application(newItem, input.Application);
			}
			return newItem;
		}

		public void PopulateAdditionalDocument(Declaration newItem, IDeclarationAdditionalDocument obj)
		{
			var bo = new DeclarationAdditionalDocument();
			bo.Id = new DeclarationAdditionalDocumentId();
			if (obj != null)
			{
				bo.Id.Value = obj.ID.Left(14);
				PopulateValueIfNodeValueIsValid(obj.LPCOExpirationDateTime, () => bo.LpcoExpirationDateTime = obj.LPCOExpirationDateTime.ToISO8601ShortDateString());
			}
			newItem.AdditionalDocument = bo;
		}

		public void PopulateAdditionalInformation(Declaration newItem, IAdditionalInformation obj)
		{
			if (obj != null && !obj.Content.IsEmpty)
			{
				var bo = new DeclarationAdditionalInformation();
				bo.Content = new DeclarationAdditionalInformationContent() { Value = obj.Content.Left(240) };
				newItem.AdditionalInformation = bo;
			}
		}

		public void PopulateTW_Application(Declaration newItem, IApplication application)
		{
			var bo = new DeclarationTwApplication();
			if (application != null)
			{
				bo.TwTypeCode = new DeclarationTwApplicationTwTypeCode() { Value = application.TypeCode.Left(3) };
				PopulateApplicationAdditionalDocuments(bo, application.AdditionalDocuments);
				PopulateApplicationAdditionalAgent(bo, application.Agent);
				bo.ContactOffice = new DeclarationTwApplicationContactOffice() { Id = new DeclarationTwApplicationContactOfficeId() { Value = application.ContactOffice.Left(17) } };
				PopulateApplicationAdditionalApplicant(bo, application.Applicant);
			}
			newItem.TwApplication = bo;
		}

		public void PopulateApplicationAdditionalDocuments(DeclarationTwApplication bo, IEnumerable<IAdditionalDocument> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationTwApplicationAdditionalDocument>();
				foreach (var item in obj.Take(5))
				{
					var newItem = new DeclarationTwApplicationAdditionalDocument();
					PopulateValueIfNodeValueIsNotEmpty(item.ID, () => newItem.Id = new DeclarationTwApplicationAdditionalDocumentId() { Value = item.ID.Left(50) });
					PopulateValueIfNodeValueIsNotEmpty(item.ImageFileFormat, () => newItem.TwImageFileFormat = new DeclarationTwApplicationAdditionalDocumentTwImageFileFormat() { Value = item.ImageFileFormat.Left(5) });
					PopulateValueIfNodeValueIsNotEmpty(item.ImageFileName, () => newItem.TwImageFileName = new DeclarationTwApplicationAdditionalDocumentTwImageFileName() { Value = item.ImageFileName.Left(200) });
					newItem.TwSequenceNumeric = item.SequenceNumeric;
					newItem.TypeCode = new DeclarationTwApplicationAdditionalDocumentTypeCode { Value = item.TypeCode.Left(2) };
					collection.Add(newItem);
				}

				bo.AdditionalDocument = collection;
			}
		}

		void PopulateApplicationAdditionalAgent(DeclarationTwApplication bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Agent = new DeclarationTwApplicationAgent()
				{
					Id = new DeclarationTwApplicationAgentId() { Value = obj.ID.Left(14) },
					Name = new DeclarationTwApplicationAgentName() { Value = obj.Name.Left(70) },
					TwTypeCode = new DeclarationTwApplicationAgentTwTypeCode() { Value = obj.TypeCode.Left(3) },
					Address = new DeclarationTwApplicationAgentAddress() { TwChineseLine = new DeclarationTwApplicationAgentAddressTwChineseLine() { Value = obj.Address.ChineseLine.Left(100) } }
				};
			}
		}

		void PopulateApplicationAdditionalApplicant(DeclarationTwApplication bo, IPartyDetails obj)
		{
			var applicant = new DeclarationTwApplicationTwApplicant();
			if (obj != null)
			{
				applicant.TwId = new DeclarationTwApplicationTwApplicantTwId() { Value = obj.ID.Left(14) };
				applicant.TwTypeCode = new DeclarationTwApplicationTwApplicantTwTypeCode() { Value = obj.TypeCode.Left(3) };
				var additionalInformations = obj.AdditionalInformations;
				if (additionalInformations != null)
				{
					var collection = new Collection<DeclarationTwApplicationTwApplicantAdditionalInformation>();
					foreach (var item in additionalInformations)
					{
						var newItem = new DeclarationTwApplicationTwApplicantAdditionalInformation();
						newItem.StatementCode = new DeclarationTwApplicationTwApplicantAdditionalInformationStatementCode() { Value = item.StatementCode.Left(3) };
						newItem.StatementDescription = new DeclarationTwApplicationTwApplicantAdditionalInformationStatementDescription() { Value = item.StatementDescription.Left(35) };
						collection.Add(newItem);
					}
					applicant.AdditionalInformation = collection;
				}
			}

			bo.TwApplicant = applicant;
		}
	}
}
