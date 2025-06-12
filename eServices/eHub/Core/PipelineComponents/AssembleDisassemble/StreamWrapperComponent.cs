using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;

namespace CargoWise.eHub.Core.PipelineComponents
{
	/// <summary>
	/// Used to wrap a stream in a ReadOnlySeekable VirtualStream so large messages are seekable and streamed to and from disk if over the 4MB
	/// default threshold.  This component should be used as the first component in a pipeline that could process large messages.
	/// </summary>
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("04CA3170-C99B-4E24-B010-31FE118CD5D6")]
	public class StreamWrapperComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return "Used to wrap message streams so they become manageable when large."; }
		}

		public string Name
		{
			get { return "Stream Wrapper"; }
		}

		public string Version
		{
			get { return "1.0"; }
		}

		#endregion

		#region IComponentUI Members

		public IntPtr Icon
		{
			get { return IntPtr.Zero; }
		}

		public IEnumerator Validate(object projectSystem)
		{
			return null;
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("04CA3170-C99B-4E24-B010-31FE118CD5D6");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			var var = LoadProperty(propertyBag, "DecodeEnabled", errorLog);
			if (var != null) DecodeEnabled = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "DecodeValues", errorLog);
			DecodeValues = (string)var ?? string.Empty;

			var = LoadProperty(propertyBag, "RemoveBOMEnabled", errorLog);
			if (var != null) RemoveBOMEnabled = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "ReplacementEnabled", errorLog);
			if (var != null) ReplacementEnabled = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "ReplacementPosition", errorLog);
			if (var != null) ReplacementPosition = Convert.ToInt32(var);

			var = LoadProperty(propertyBag, "ReplacementText", errorLog);
			ReplacementText = (string)var ?? string.Empty;

			var = LoadProperty(propertyBag, "DiscardEmptyMessages", errorLog);
			if (var != null) DiscardEmptyMessages = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "PrependData", errorLog);
			PrependData = (string)var ?? string.Empty;

			var = LoadProperty(propertyBag, "AppendData", errorLog);
			AppendData = (string)var ?? string.Empty;

			var = LoadProperty(propertyBag, "DakosyReaderEnabled", errorLog);
			if (var != null) DakosyReaderEnabled = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "RemoveNamespaceEnabled", errorLog);
			if (var != null) RemoveNamespaceEnabled = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "MaxMessageSizeMB", errorLog);
			if (var != null) MaxMessageSizeMB = Convert.ToInt32(var);
		}

		object LoadProperty(IPropertyBag propertyBag, string propertyName, int errorLog)
		{
			object result = null;
			try
			{
				propertyBag.Read(propertyName, out result, errorLog);
			}
			catch { }
			return result;
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val = DecodeEnabled;
			propertyBag.Write("DecodeEnabled", ref val);

			val = DecodeValues;
			propertyBag.Write("DecodeValues", ref val);

			val = RemoveBOMEnabled;
			propertyBag.Write("RemoveBOMEnabled", ref val);

			val = ReplacementEnabled;
			propertyBag.Write("ReplacementEnabled", ref val);

			val = ReplacementPosition;
			propertyBag.Write("ReplacementPosition", ref val);

			val = ReplacementText;
			propertyBag.Write("ReplacementText", ref val);

			val = DiscardEmptyMessages;
			propertyBag.Write("DiscardEmptyMessages", ref val);

			val = PrependData;
			propertyBag.Write("PrependData", ref val);

			val = AppendData;
			propertyBag.Write("AppendData", ref val);

			val = DakosyReaderEnabled;
			propertyBag.Write("DakosyReaderEnabled", ref val);

			val = RemoveNamespaceEnabled;
			propertyBag.Write("RemoveNamespaceEnabled", ref val);

			val = MaxMessageSizeMB;
			propertyBag.Write("MaxMessageSizeMB", ref val);
		}

		#endregion

		#region IComponent Members

		public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
			Tracer.TraceStart(pipelineContext, message);
			try
			{
				if (MaxMessageSizeMB > 0)
				{
					message.GetSize(out var size, out var implemented);
					if (implemented && size > (ulong)MaxMessageSizeMB * 1024 * 1024)
					{
						throw new InvalidDataException($"Message exceeds maximum allowed size of {MaxMessageSizeMB} MB.");
					}
				}

				var dataStream = message.BodyPart.GetOriginalDataStream();

				if (RemoveBOMEnabled)
				{
					Tracer.TraceInfo("RemoveBOMEnabled: true");
					var buffer = new byte[3];
					dataStream.Position = 0;
					var bytesRead = dataStream.Read(buffer, 0, 3);
					if (bytesRead == 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
					{
						var removeBOMStream = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk);
						dataStream.CopyTo(removeBOMStream);
						pipelineContext.ResourceTracker.AddResource(removeBOMStream);
						dataStream = removeBOMStream;
					}
					dataStream.Position = 0;
				}

				if (DecodeEnabled)
				{
					Tracer.TraceInfo("DecodingStream: Decode values '{0}'. Base stream type '{1}'.", DecodeValues, dataStream.GetType().FullName);
					var decodeStream = new CargoWise.eHub.Common.DecodingStream(dataStream, GetDecodeValuesAsBytes());
					pipelineContext.ResourceTracker.AddResource(decodeStream);
					dataStream = decodeStream;
				}

				if (ReplacementEnabled)
				{
					Tracer.TraceInfo("TextReplacementStream: Text for replacement '{0}'. Base stream type '{1}'. ReplacePosition '{2}'.", ReplacementText, dataStream.GetType().FullName, ReplacementPosition);
					var textReplacementStream = new CargoWise.eHub.Common.TextReplacementStream(dataStream, ReplacementPosition, ReplacementText);
					pipelineContext.ResourceTracker.AddResource(textReplacementStream);
					dataStream = textReplacementStream;
				}

				if (RemoveNamespaceEnabled)
				{
					Tracer.TraceInfo("RemoveNamespaceEnabled: true");
					dataStream = new XmlNamespaceRemoverStream(dataStream);
				}

				if (!string.IsNullOrEmpty(PrependData) || !string.IsNullOrEmpty(AppendData))
				{
					PrependData = PrependData != null ? PrependData.Replace("\\r", "\r").Replace("\\n", "\n") : string.Empty;
					AppendData = AppendData != null ? AppendData.Replace("\\r", "\r").Replace("\\n", "\n") : string.Empty;

					var prependDataToBytes = Encoding.Default.GetBytes(PrependData);
					var appendDataToBytes = Encoding.Default.GetBytes(AppendData);

					var fixMessageStream = new CargoWise.eHub.Common.FixMsgStream(dataStream, prependDataToBytes, appendDataToBytes);
					pipelineContext.ResourceTracker.AddResource(fixMessageStream);
					dataStream = fixMessageStream;
				}

				if (DakosyReaderEnabled)
				{
					Tracer.TraceInfo("DakosyReaderEnabled: true");
					var decodeStream = new CargoWise.eHub.Common.DakosyReaderStream(dataStream);
					pipelineContext.ResourceTracker.AddResource(decodeStream);
					dataStream = decodeStream;
				}

				VirtualStream persistingStream = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk);
				ReadOnlySeekableStream seekablePersistentStream = new ReadOnlySeekableStream(dataStream, persistingStream);
				var sr = new StreamReader(seekablePersistentStream);
				if (sr.Peek() == -1)
				{
					if (DiscardEmptyMessages)
					{
						return null;
					}
					else
					{
						var ftpName = message.Context.ReadPropertyString<FTP.ReceivedFileName>();
						var pop3Name = message.Context.ReadPropertyString<MIME.PartContentTypeSecondaryHeaderValue>();
						throw new ApplicationException(string.Format("Zero-byte file received with name '{0}'.", ftpName ?? pop3Name));
					}
				}
				seekablePersistentStream.Position = 0;
				message.BodyPart.Data = seekablePersistentStream;
				Tracer.TraceEnd();
				return message;
			}
			catch (Exception ex)
			{
				Tracer.TraceError(ex);
				throw;
			}
		}

		#endregion

		#region Properties

		public bool DecodeEnabled { get; set; }

		public string DecodeValues { get; set; }

		public bool RemoveBOMEnabled { get; set; }

		public bool ReplacementEnabled { get; set; }

		public int ReplacementPosition { get; set; }

		public string ReplacementText { get; set; }

		public bool DiscardEmptyMessages { get; set; }

		public string PrependData { get; set; }

		public string AppendData { get; set; }

		public bool DakosyReaderEnabled { get; set; }

		public bool RemoveNamespaceEnabled { get; set; }

		public int MaxMessageSizeMB { get; set; }

		#endregion

		byte?[,] GetDecodeValuesAsBytes()
		{
			var incorrectFormatException = new ApplicationException("Incorrect format for DecodeValues. Expected format is '0x99-[0x99][,0x99-[0x99][,...]]'. Actual value '" + DecodeValues + "'");
			if (String.IsNullOrEmpty(DecodeValues))
			{
				throw new ApplicationException("No decode values specified.");
			}
			if (!Regex.IsMatch(DecodeValues, @"0x[0-9a-fA-F]{2}\-(0x[0-9a-fA-F]{2})?(\,0x[0-9a-fA-F]{2}\-(0x[0-9a-fA-F]{2})?)*"))
			{
				throw incorrectFormatException;
			}

			string[] pairs = DecodeValues.Split(',');
			byte?[,] decodes = new byte?[pairs.Length, 2];
			for (int i = 0; i < pairs.Length; i++)
			{
				var keyValue = pairs[i].Split('-');
				if (keyValue.Length != 2)
					throw incorrectFormatException;
				decodes[i, 0] = Byte.Parse(keyValue[0].Substring(2, 2), NumberStyles.AllowHexSpecifier);
				if (string.IsNullOrEmpty(keyValue[1]))
					decodes[i, 1] = null;
				else
					decodes[i, 1] = Byte.Parse(keyValue[1].Substring(2, 2), NumberStyles.AllowHexSpecifier);
			}
			return decodes;
		}
	}
}
