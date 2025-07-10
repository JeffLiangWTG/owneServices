using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;

namespace Enterprise.Recruiter.Business
{
	public class ExamAnswerArchiveCollection : NonPersistentBusinessObjectCollection<ExamAnswerArchive>, IXmlSerializable
	{
		public ExamAnswerArchiveCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Schema

		public sealed class Schema
		{
			Schema() { }
			public const string XmlElementName = "ExamAnswerArchive";
			public const string XmlCollectionName = "Answers";
		}

		#endregion

		#region IXmlSerializable Members

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			if (!reader.IsEmptyElement)
			{
				reader.ReadStartElement();
				while (reader.IsStartElement(Schema.XmlElementName))
				{
					ExamAnswerArchive answerArchive = AddNew();
					using (answerArchive.SuspendSettingHasChanges())
					{
						((IXmlSerializable)answerArchive).ReadXml(reader);
					}
				}
				reader.ReadEndElement();
			}
			else
			{
				reader.Skip();
			}
		}

		public void WriteXml(XmlWriter writer)
		{
			foreach (ExamAnswerArchive answerArchive in this)
			{
				writer.WriteStartElement(Schema.XmlElementName);
				((IXmlSerializable)answerArchive).WriteXml(writer);
				writer.WriteEndElement();
			}
		}

		#endregion

		#region Load / Save

		public void LoadFromBlob(ZBlob data)
		{
			Argument.NotNull(data, "data");

			RemoveAllButLeaveRelationshipsIntact();

			using (MemoryStream memoryStream = new MemoryStream((byte[])data))
			using (XmlTextReader reader = new XmlTextReader(memoryStream))
			{
				reader.ReadToFollowing(Schema.XmlCollectionName);
				ReadXml(reader);
			}

			IsLoaded = true;
		}

		public ZBlob ConvertToXmlBlob()
		{
			using (MemoryStream memoryStream = new MemoryStream())
			using (XmlTextWriter writer = new XmlTextWriter(memoryStream, System.Text.Encoding.UTF8))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement(Schema.XmlCollectionName);
				WriteXml(writer);
				writer.WriteEndElement();
				writer.WriteEndDocument();

				writer.Flush();
				return new ZBlob(memoryStream.ToArray());
			}
		}

		#endregion

		public override bool ReadOnly
		{
			get { return true; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ExamAnswerArchive(Factory);
		}

		public void Populate(LearningCentreCampaignItem campaignItem)
		{
			Argument.NotNull(campaignItem, "campaignItem");

			foreach (LearningCentreSubmittedAnswer submittedAnswer in GetPopulatedAndEmptySubmittedAnswers(campaignItem))
			{
				ExamAnswerArchive answerArchive = AddNew();
				answerArchive.Populate(submittedAnswer);
			}
		}

		IEnumerable<VoteExamSurveySubmittedAnswer> GetPopulatedAndEmptySubmittedAnswers(LearningCentreCampaignItem campaignItem)
		{
			foreach (LearningCentreSubmittedAnswer submittedAnswer in campaignItem.SubmittedAnswers.PopulatedAnswers)
			{
				yield return submittedAnswer;
			}

			foreach (LearningCentreSubmittedAnswer submittedAnswer in campaignItem.SubmittedAnswers.EmptyAnswers)
			{
				yield return submittedAnswer;
			}
		}

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			return (property.Name.EndsWith("ActualOrderForBinding"))
				? new PropertyComparer(property.ComponentType, "Question+ActualOrder", direction)
				: base.GetComparerForSort(property, direction);
		}
	}
}
