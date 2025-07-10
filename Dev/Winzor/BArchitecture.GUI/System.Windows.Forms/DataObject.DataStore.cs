using System.Collections.Specialized;
using System.Drawing;
using System.Runtime.Serialization;

namespace System.Windows.Forms;

public partial class DataObject
{
	class DataStore : IDataObject
	{
		class DataStoreEntry
		{
			public object? Data { get; }
			public bool AutoConvert { get; }

			public DataStoreEntry(object? data, bool autoConvert)
			{
				Data = data;
				AutoConvert = autoConvert;
			}
		}

		readonly Dictionary<string, DataStoreEntry> _data = new Dictionary<string, DataStoreEntry>(BackCompatibleStringComparer.Default);

		public object? GetData(string format, bool autoConvert)
		{
			if (string.IsNullOrWhiteSpace(format))
			{
				return null;
			}

			object? baseVar = null;
			if (_data.TryGetValue(format, out DataStoreEntry? dse))
			{
				baseVar = dse.Data;
			}

			object? original = baseVar;

			if (autoConvert
				&& (dse is null || dse.AutoConvert)
				&& (baseVar is null || baseVar is MemoryStream))
			{
				string[]? mappedFormats = GetMappedFormats(format);
				if (mappedFormats is not null)
				{
					for (int i = 0; i < mappedFormats.Length; i++)
					{
						if (!format.Equals(mappedFormats[i]))
						{
							if (_data.TryGetValue(mappedFormats[i], out DataStoreEntry? found))
							{
								baseVar = found.Data;
							}

							if (baseVar is not null && baseVar is not MemoryStream)
							{
								original = null;
								break;
							}
						}
					}
				}
			}

			if (original is not null)
			{
				return original;
			}
			else
			{
				return baseVar;
			}
		}

		public virtual object? GetData(string format) =>
			GetData(format, true);

		public virtual object? GetData(Type format) =>
			GetData(format.FullName!);

		public bool GetDataPresent(string format, bool autoConvert)
		{
			if (string.IsNullOrWhiteSpace(format))
			{
				return false;
			}

			if (!autoConvert)
			{
				return _data.ContainsKey(format);
			}
			else
			{
				string[] formats = GetFormats(autoConvert);
				for (int i = 0; i < formats.Length; i++)
				{
					if (format.Equals(formats[i]))
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool GetDataPresent(string format) =>
			GetDataPresent(format, true);

		public bool GetDataPresent(Type format) =>
			GetDataPresent(format.FullName!);

		public string[] GetFormats(bool autoConvert)
		{
			string[] baseVar = new string[_data.Keys.Count];
			_data.Keys.CopyTo(baseVar, 0);

			if (autoConvert)
			{
				// Since we are only adding elements to the HashSet, the order will be preserved.
				int baseVarLength = baseVar.Length;
				HashSet<string> distinctFormats = new HashSet<string>(baseVarLength);
				for (int i = 0; i < baseVarLength; i++)
				{
					if (_data[baseVar[i]]!.AutoConvert)
					{
						string[] cur = GetMappedFormats(baseVar[i])!;

						for (int j = 0; j < cur.Length; j++)
						{
							distinctFormats.Add(cur[j]);
						}
					}
					else
					{
						distinctFormats.Add(baseVar[i]);
					}
				}

				baseVar = distinctFormats.ToArray();
			}

			return baseVar;
		}

		public string[] GetFormats() =>
			GetFormats(true);

		public void SetData(string format, bool autoConvert, object data)
		{
			if (string.IsNullOrWhiteSpace(format))
			{
				ArgumentNullException.ThrowIfNull(format);
				throw new ArgumentException(SR.DataObjectWhitespaceEmptyFormatNotAllowed, nameof(format));
			}

			// We do not have proper support for Dibs, so if the user explicitly asked
			// for Dib and provided a Bitmap object we can't convert.  Instead, publish as an HBITMAP
			// and let the system provide the conversion for us.
			if (data is Bitmap && format.Equals(DataFormats.Dib))
			{
				if (autoConvert)
				{
					format = DataFormats.Bitmap;
				}
				else
				{
					throw new NotSupportedException(SR.DataObjectDibNotSupported);
				}
			}

			_data[format] = new DataStoreEntry(data, autoConvert);
		}

		public void SetData(string format, object data) =>
			SetData(format, true, data);

		public void SetData(Type format, object data)
		{
			ArgumentNullException.ThrowIfNull(format);
			SetData(format.FullName!, data);
		}

		public void SetData(object data)
		{
			ArgumentNullException.ThrowIfNull(data);

			if (data is ISerializable
				&& !_data.ContainsKey(DataFormats.Serializable))
			{
				SetData(DataFormats.Serializable, data);
			}

			SetData(data.GetType(), data);
		}
	}
}
