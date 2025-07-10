using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class FormStateXmlExplorerPresenter
	{
		public FormStateXmlExplorerPresenter(ProcessTaskTemplate processTaskTemplate)
		{
			Argument.NotNull(processTaskTemplate, "processTaskTemplate");
			this.processTaskTemplate = processTaskTemplate;
		}

		readonly ProcessTaskTemplate processTaskTemplate;
		IFormStateXmlExplorerView view;
		bool isInitialised;

		public void Initialise(IFormStateXmlExplorerView view)
		{
			if (!isInitialised)
			{
				this.view = view;
				this.view.Xml = processTaskTemplate.P0_FormState.ToAscii();
				isInitialised = true;
			}
		}

		public string Validate()
		{
			string result = string.Empty;

			if (!string.IsNullOrEmpty(view.Xml))
			{
				StringBuilder builder = new StringBuilder();

				using (processTaskTemplate.SuspendSettingHasChanges())
				{
					AppendValidationMessage(builder, "DESCRIPTOR", ValidateElementsAgainstDescriptor());
					AppendValidationMessage(builder, "XML", ValidateXml());
				}

				result = builder.ToString();
			}

			return result;
		}

		void AppendValidationMessage(StringBuilder builder, string category, string message)
		{
			if (builder != null && !string.IsNullOrEmpty(message))
			{
				if (builder.Length > 0)
				{
					builder.AppendLine();
				}

				if (!string.IsNullOrEmpty(category))
				{
					builder.AppendLine(String.Concat("--- ", category.ToUpper(), (NoResString)" VALIDATION MESSAGES ---"));
				}

				builder.AppendLine();
				builder.AppendLine(message);
			}
		}

		string ValidateElementsAgainstDescriptor()
		{
			return String.Join(System.Environment.NewLine,
				ValidateElementsAgainstDescriptor(processTaskTemplate.FormCustomisationSettings.DisplayTabs)
				.Concat(ValidateElementsAgainstDescriptor(processTaskTemplate.FormCustomisationSettings.DisplayFields)).ToArray());
		}

		List<string> ValidateElementsAgainstDescriptor(FormCustomisableElementCollection elementCollection)
		{
			List<string> result = new List<string>();

			foreach (FormCustomisableElement element in elementCollection)
			{
				string description = ValidateElementsAgainstDescriptor(element);

				if (!string.IsNullOrEmpty(description))
				{
					result.Add(description);
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need to localize tool message")]
		string ValidateElementsAgainstDescriptor(FormCustomisableElement element)
		{
			string result = string.Empty;

			if (!IsInTemplate(element.ElementName))
			{
				result = GetElementErrorDescription(element, (NoResString)"is not in template");
			}
			else if (!IsInXml(element.ElementName))
			{
				result = GetElementErrorDescription(element, "is not in xml");
			}

			return result;
		}

		bool IsInTemplate(string elementName)
		{
			return processTaskTemplate.WorkflowDescriptor.FormCustomisationSettings.DisplayTabs.GetElement(elementName) != null ||
				   processTaskTemplate.WorkflowDescriptor.FormCustomisationSettings.DisplayFields.GetElement(elementName) != null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "I'm a xml element name and do not smell")]
		bool IsInXml(string elementName)
		{
			return !String.IsNullOrEmpty(view.Xml) && view.Xml.Contains(String.Concat("<Name>", elementName, "</Name>"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need to localize tool message")]
		string GetElementErrorDescription(FormCustomisableElement element, string error)
		{
			return String.Concat("[Error] ", element.CanContainOtherElements ? "Tab" : "Field", " [", element.ElementName, "|", element.ElementDescription, "] ", error);
		}

		string ValidateXml()
		{
			using (StringReader stringReader = new StringReader(view.Xml))
			using (XmlReader xmlReader = XmlReader.Create(stringReader))
			using (Stream xsdStream = typeof(FormCustomisationSettingsStorage).Assembly.GetManifestResourceStream(GetXsdLocation(typeof(FormCustomisationSettingsStorage))))
			using (XmlTextReader xsdReader = new XmlTextReader(xsdStream))
			{
				XmlDocument xmlDocument = new XmlDocument();

				List<string> errors = new List<string>();

				try
				{
					xmlDocument.Load(xmlReader);

					ValidationEventHandler eventHandler = (s, e) => errors.Add(String.Concat("[", e.Severity, "] ", e.Message));
					xmlDocument.Schemas.Add("http://www.edi.com.au/EnterpriseService/", xsdReader);
					xmlDocument.Validate(eventHandler);
				}
				catch (XmlException e)
				{
					errors.Add(e.Message);
				}

				return String.Join(System.Environment.NewLine, errors.ToArray());
			}
		}

		string GetXsdLocation(Type type)
		{
			return type.Assembly.GetManifestResourceNames()
				.Where((res) => res.EndsWith(String.Concat(type.Name, ".xsd")))
				.FirstOrDefault();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Message for developer")]
		public string SubmitChanges()
		{
			string result = string.Empty;

			if (!string.IsNullOrEmpty(view.Xml))
			{
				try
				{
					byte[] data = Encoding.ASCII.GetBytes(view.Xml);

					FormCustomisationSettingsStorage storage = FormCustomisationSettingsStorageSerializer.Deserialize(data);
					processTaskTemplate.P0_FormState = FormCustomisationSettingsStorageSerializer.Serialize(storage);
					processTaskTemplate.FormCustomisationSettings.Reset();
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}

					result = String.Concat("Unable submit changes due to:", System.Environment.NewLine, ex.Message, System.Environment.NewLine, ex.StackTrace);
				}
			}

			return result;
		}
	}
}