using System.Collections.Concurrent;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Tracking.Business
{
	public class PropertyChangeInfoCollection
	{
		public void Add(DataState type, MultilingualString propertyName, ZString code, ReadOnlyCodeDescriptionPairList descriptionList)
		{
			AddCore(type, propertyName, PropertyChangeInfo.Format(code, descriptionList));
		}

		public void Add(DataState type, MultilingualString propetryName, INumericZType number, ZString code, ReadOnlyCodeDescriptionPairList descriptionList)
		{
			AddCore(type, propetryName, PropertyChangeInfo.Format(number, code, descriptionList));
		}

		public void Add(DataState type, MultilingualString propertyName, MultilingualString value)
		{
			AddCore(type, propertyName, value);
		}

		public void Add(DataState type, MultilingualString propertyName, IZType value)
		{
			AddCore(type, propertyName, PropertyChangeInfo.Format(value));
		}

		public void Add(DataState type, MultilingualString collectionName, IBusinessObjectCollection collection)
		{
			int i = 1;

			foreach (BusinessObject bizO in collection)
			{
				var propertyName = ResString.GetMultilingualString("0a60d410-3ba2-424d-8a5d-8fbb5553b4a9", "{0}: Item {1}", collectionName, i++);
				ZString value = PropertyChangeInfo.Format((ZBool)bizO.HasChanges);

				AddCore(type, propertyName, value);
			}
		}

		void AddCore(DataState type, MultilingualString propertyName, ZString value)
		{
			AddCore(type, propertyName, (NoResString)value);
		}

		void AddCore(DataState type, MultilingualString propertyName, MultilingualString value)
		{
			PropertyChangeInfo info = FindOrCreatePropertyChangeInfo(propertyName);

			if (type == DataState.Original)
			{
				info.OriginalValueMultilingual = value;

				info.UpdatedValueMultilingual = (info.UpdatedValueMultilingual.IsEmpty) ? ResString.GetMultilingualString("cfe646a9-d715-4da7-9642-9b43d2f4f7b1", "Removed") : info.UpdatedValueMultilingual;
			}
			else
			{
				info.UpdatedValueMultilingual = value;

				info.OriginalValueMultilingual = (info.OriginalValueMultilingual.IsEmpty) ? ResString.GetMultilingualString("88cb3368-1013-418f-a05b-120645a20431", "Added") : info.OriginalValueMultilingual;
			}
		}

		public PropertyChangeInfo[] GetValuesAsArray()
		{
			return properties.Values.ToArray();
		}

		PropertyChangeInfo FindOrCreatePropertyChangeInfo(MultilingualString propertyName)
		{
			return properties.GetOrAdd(propertyName.GetUnresolvedString(), (string p) => new PropertyChangeInfo(propertyName));
		}

		readonly ConcurrentDictionary<string, PropertyChangeInfo> properties = new ConcurrentDictionary<string, PropertyChangeInfo>();
	}
}
