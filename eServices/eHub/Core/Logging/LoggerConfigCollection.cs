using System.Configuration;

namespace CargoWise.eHub.Core.Logging
{
	[ConfigurationCollection(typeof(LoggerConfigItem), AddItemName = "add", CollectionType = ConfigurationElementCollectionType.BasicMap)]
	public class LoggerConfigCollection : ConfigurationElementCollection
	{
		public override ConfigurationElementCollectionType CollectionType
		{
			get { return ConfigurationElementCollectionType.BasicMap; }
		}

		protected override string ElementName
		{
			get { return "add"; }
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new LoggerConfigItem();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return (element as LoggerConfigItem).Name;
		}

		public LoggerConfigItem this[int index]
		{
			get
			{
				return (LoggerConfigItem)BaseGet(index);
			}
			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}
				BaseAdd(index, value);
			}
		}

		new public LoggerConfigItem this[string Name]
		{
			get
			{
				return (LoggerConfigItem)BaseGet(Name);
			}
		}
	}
}
