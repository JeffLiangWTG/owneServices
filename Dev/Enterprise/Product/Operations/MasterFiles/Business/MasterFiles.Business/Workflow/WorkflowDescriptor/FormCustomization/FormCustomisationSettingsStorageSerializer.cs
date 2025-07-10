using System;
using System.IO;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.MasterFiles.Business
{
	[Serializable]
	public class CustomisationSettingsDeserializeException : InvalidOperationException
	{
		public CustomisationSettingsDeserializeException(string message, Exception innerException) : base(message, innerException) { }
#if NETFRAMEWORK
		protected CustomisationSettingsDeserializeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	public static class FormCustomisationSettingsStorageSerializer
	{
		public static FormCustomisationSettingsStorage Deserialize(byte[] buffer)
		{
			FormCustomisationSettingsStorage storage = null;

			if (buffer != null)
			{
				try
				{
					using (Stream stream = new MemoryStream(buffer))
					{
						stream.Position = 0;
						ZXmlSerializer serializer = ZXmlSerializer.New(typeof(FormCustomisationSettingsStorage));
						storage = (FormCustomisationSettingsStorage)serializer.Deserialize(stream);

						stream.Flush();
						stream.Close();
					}
				}
				catch (InvalidOperationException ex)
				{
					throw new CustomisationSettingsDeserializeException(ex.Message, ex);
				}
			}

			return storage;
		}

		public static byte[] Serialize(FormCustomisationSettingsStorage storage)
		{
			byte[] buffer = null;

			if (storage != null)
			{
				using (MemoryStream stream = new MemoryStream())
				{
					ZXmlSerializer serializer = ZXmlSerializer.New(typeof(FormCustomisationSettingsStorage));
					serializer.Serialize(stream, storage);

					stream.Position = 0;

					buffer = new byte[stream.Length];
					int offset = 0;
					while (offset < buffer.Length)
					{
						offset += stream.Read(buffer, offset, buffer.Length - offset);
					}
					stream.Close();
				}
			}

			return buffer;
		}
	}
}
