using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageProcessors
{
	class SafeXmlReader<T> where T : class
	{
		public T Result
		{
			get;
			private set;
		}

		public ZString ErrorText
		{
			get;
			private set;
		}

		ZStringBuilder errors;

		public bool TryReadFromXML(string xmlText)
		{
			Result = null;
			errors = new ZStringBuilder();

			try
			{
				ZXmlSerializer serializer = ZXmlSerializer.New(typeof(T));
				XmlReaderSettings settings = new XmlReaderSettings();
				using (StringReader input = new StringReader(xmlText))
				using (XmlReader reader = XmlReader.Create(input, settings))
				{
					XmlDeserializationEvents events = new XmlDeserializationEvents();
					// XmlDeserializationEvents Event Handlers commented out for now - Tried in debugging but seemed to have limited use.
					// ------------------------------------------------------------------------------------------------------------------
					//events.OnUnknownAttribute = new XmlAttributeEventHandler(OnUnknownAttribute);
					//events.OnUnknownElement = new XmlElementEventHandler(OnUnknownElement);
					//events.OnUnknownNode = new XmlNodeEventHandler(OnUnknownNode);
					//events.OnUnreferencedObject = new UnreferencedObjectEventHandler(OnUnreferencedObject);
					Result = (T)serializer.Deserialize(reader, events);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				errors.Append(ex.Message + " Please contact the ECN support desk to solve this problem.");
			}

			if (errors.IsEmpty && Result == null)
			{
				errors.Append("Could not deserialise XML Message Response as [" + typeof(T).Name + "].");
			}

			ErrorText = errors.ToStringWithDelimiterBetweenAppends("\r\n\r\n");
			return errors.IsEmpty;
		}

		// XmlDeserializationEvents Event Handlers commented out for now - Tried in debugging but seemed to have limited use.
		//void OnUnknownAttribute(object sender, XmlAttributeEventArgs e)
		//{
		//  ZStringBuilder result = new ZStringBuilder("Unknown Attribute [" + e.Attr.Name + "]:-");
		//  result.Append("Expected Elements: " + e.ExpectedAttributes);
		//  result.Append("Line No.: " + e.LineNumber.ToString());
		//  result.Append("Line Pos: " + e.LinePosition.ToString());
		//  result.Append("XML: " + e.Attr.InnerXml.ToString());
		//  errors.Append(result.ToStringWithNewLineBetweenAppends());
		//}

		//void OnUnknownElement(object sender, XmlElementEventArgs e)
		//{
		//  ZStringBuilder result = new ZStringBuilder("Unknown Element [" + e.Element.Name + "]:-");
		//  result.Append("Expected Elements: " + e.ExpectedElements);
		//  result.Append("Line No.: " + e.LineNumber.ToString());
		//  result.Append("Line Pos: " + e.LinePosition.ToString());
		//  result.Append("XML: " + e.Element.InnerXml.ToString());
		//  errors.Append(result.ToStringWithNewLineBetweenAppends());
		//}

		//void OnUnknownNode(object sender, XmlNodeEventArgs e)
		//{
		//  ZStringBuilder result = new ZStringBuilder("Unknown Node [" + e.Name + "]:-");
		//  result.Append("Line No.: " + e.LineNumber.ToString());
		//  result.Append("Line Pos: " + e.LinePosition.ToString());
		//  result.Append("Text: " + e.Text);
		//  errors.Append(result.ToStringWithNewLineBetweenAppends());
		//}

		//void OnUnreferencedObject(object sender, UnreferencedObjectEventArgs e)
		//{
		//  errors.Append("Unreferenced Object [" + e.UnreferencedId + "].");
		//}
	}
}
