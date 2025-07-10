using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.Types;

namespace Enterprise.Rating.Integration
{
	public class CalculationLogsWrapper : IXmlSerializable
	{
		#region Schema

		public static class Schema
		{
			public const string XmlElementName = "CalculationLogsWrapper";

			public const string IsDisabled = "IsDisabled";
			public const string Logs = "CalculationLogs";
		}

		#endregion

		public ZBool IsDisabled { get; set; }

		public List<CalculationLog> Logs
		{
			get { return logs ?? (logs = new List<CalculationLog>()); }
		}
		List<CalculationLog> logs;

		#region IsEmpty

		public bool IsEmpty
		{
			get { return Logs.Count == 0; }
		}

		#endregion

		#region IsCosting

		public bool IsCosting
		{
			get { return Logs.Count > 0 && Logs[0].IsCosting; }
		}

		#endregion

		#region IXmlSerializable Members

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			writer.WriteStartDocument();
			writer.WriteStartElement(Schema.XmlElementName);

			writer.WriteElementString(Schema.IsDisabled, IsDisabled.ToString());
			WriteCalculationLogs(writer);

			writer.WriteEndElement();
			writer.WriteEndDocument();

			writer.Flush();
		}

		void WriteCalculationLogs(XmlWriter writer)
		{
			writer.WriteStartElement(Schema.Logs);
			foreach (CalculationLog calculationLog in Logs)
			{
				((IXmlSerializable)calculationLog).WriteXml(writer);
			}
			writer.WriteEndElement();
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			reader.ReadStartElement();

			IsDisabled = new ZBool(reader.ReadElementString(Schema.IsDisabled));
			ReadCalculationLogs(reader);

			reader.ReadEndElement();
		}

		void ReadCalculationLogs(XmlReader reader)
		{
			reader.ReadStartElement(Schema.Logs);

			if (reader.NodeType != XmlNodeType.EndElement) // CalculationLogs node is not empty; if it's empty, we shouldn't call ReadEndElement()
			{
				while (reader.NodeType != XmlNodeType.EndElement)
				{
					CalculationLog calculationLog = new CalculationLog();
					((IXmlSerializable)calculationLog).ReadXml(reader);

					Logs.Add(calculationLog);
				}

				reader.ReadEndElement();
			}
		}

		#endregion

		#region Serialization Methods

		public ZString Serialize()
		{
			return this.SerializeToString();
		}

		public static CalculationLogsWrapper Deserialize(ZString serializedValue)
		{
			CalculationLogsWrapper result = new CalculationLogsWrapper();
			var success = false;
			try
			{
				result.DeserializeFromString(serializedValue);
				success = true;
			}
			catch (Exception ex) when (ex is XmlException) { }
			finally
			{
				if (!success)
				{
					result = null;
				}
			}

			return result;
		}

		#endregion

		#region XML Formatting

		public ZString ToFormattedXML()
		{
			var sb = new StringBuilder();
			var xmlString = Serialize();
			var xmlDoc = new XmlDocument();

			xmlDoc.LoadXml(xmlString);

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = "  ";
			settings.NewLineChars = System.Environment.NewLine;
			settings.NewLineHandling = NewLineHandling.Replace;
			using (XmlWriter writer = XmlWriter.Create(sb, settings))
			{
				xmlDoc.Save(writer);
				return sb.ToString();
			}
		}

		#endregion
	}
}
