using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	internal class AWBActionsSerializationHelper
	{
		public AWBActionsSerializationHelper(IAWBActionsSerializable actions)
		{
			this.actions = actions;
		}

		readonly IAWBActionsSerializable actions;

		#region Load / Save Settings

		public void LoadSettings()
		{
			try
			{
				using (actions.GetValidationSuspender())
				using (actions.SuspendSettingHasChanges())
				{
					string value = Env.Registry.GetFilterCriteria(SettingsCacheName);

					if (!string.IsNullOrEmpty(value))
					{
						StringReader reader = null;
						try
						{
							reader = new StringReader(value);
							using (XmlTextReader xmlReader = new XmlTextReader(reader))
							{
								xmlReader.WhitespaceHandling = WhitespaceHandling.None;
								xmlReader.ReadToFollowing(SettingsCacheName);
								actions.ReadXml(xmlReader);
							}
						}
						finally
						{
							reader?.Dispose();
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}
			}
		}

		public void SaveSettings()
		{
			StringWriter writer = null;
			try
			{
				writer = new StringWriter(CultureInfo.InvariantCulture);
				using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
				{
					actions.WriteXml(xmlWriter);
					Env.Registry.SetFilterCriteria(SettingsCacheName, writer.ToString());
				}
			}
			finally
			{
				writer?.Dispose();
			}
		}

		#endregion

		#region WriteXml

		public void WriteXml(XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement(SettingsCacheName);

			foreach (ZPropertyInfo propertyInfo in PropertiesToSerialize)
			{
				WriteElement(xmlWriter, propertyInfo);
			}

			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();

			xmlWriter.Flush();
		}

		void WriteElement(XmlWriter xmlWriter, ZPropertyInfo propertyInfo)
		{
			xmlWriter.WriteStartElement(propertyInfo.Name);

			xmlWriter.WriteValue(Convert.ToString(propertyInfo.Value, CultureInfo.InvariantCulture));

			xmlWriter.WriteEndElement();
		}

		#endregion

		#region ReadXml

		public void ReadXml(XmlReader xmlReader)
		{
			xmlReader.ReadStartElement();

			foreach (ZPropertyInfo propertyInfo in PropertiesToSerialize)
			{
				ReadElement(xmlReader, propertyInfo);
			}

			xmlReader.ReadEndElement();
		}

		void ReadElement(XmlReader xmlReader, ZPropertyInfo propertyInfo)
		{
			try
			{
				propertyInfo.SetValueFromString(xmlReader.ReadElementString(propertyInfo.Name));
			}
			catch (FormatException)
			{
				propertyInfo.Value = propertyInfo.DefaultValue;
			}
		}

		#endregion

		string SettingsCacheName
		{
			get { return actions.SettingsCacheName; }
		}

		List<ZPropertyInfo> PropertiesToSerialize
		{
			get { return propertiesToSerialize ?? (propertiesToSerialize = actions.GetPropertiesToSerialize()); }
		}
		List<ZPropertyInfo> propertiesToSerialize;
	}
}

#region Test
#if DEBUG

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	public interface ISerializeAWBTest
	{
		void TestLoadSettings();
		void TestSaveSettings();
	}
}

#endif
#endregion
