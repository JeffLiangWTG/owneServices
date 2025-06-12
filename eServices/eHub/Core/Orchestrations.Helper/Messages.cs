using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.BizTalk.Streaming;
using Microsoft.BizTalk.XLANGs.BTXEngine;
using Microsoft.XLANGs.BaseTypes;
using Microsoft.XLANGs.Core;

namespace CargoWise.eHub.Core.Orchestrations.Helper
{
	public class Messages
	{
		public static string GetMessageBodyString(XLANGMessage message)
		{
			using (var sr = new StreamReader((Stream)message[0].RetrieveAs(typeof(Stream))))
				return sr.ReadToEnd();
		}

		public static string GetMessagePartBase64String(XLANGMessage message, int index)
		{
			Stream partData = null;
			VirtualStream outputData = null;
			CryptoStream converterStream = null;

			try
			{
				partData = (Stream)message[index].RetrieveAs(typeof(Stream));
				outputData = new VirtualStream();
				converterStream = new CryptoStream(outputData, new ToBase64Transform(), CryptoStreamMode.Write);
				partData.CopyTo(converterStream);
				converterStream.FlushFinalBlock();
				outputData.Position = 0;
				using (var sr = new StreamReader(outputData))
					return sr.ReadToEnd();
			}
			finally
			{
				if (partData != null) partData.Dispose();
				if (outputData != null) outputData.Dispose();
				if (converterStream != null) converterStream.Dispose();
			}
		}

		public static void LoadMessageFromStream(XLANGMessage message, Stream stream)
		{
			message[0].LoadFrom(stream);
		}

		public static void SetContextProperty(string contextItemName, string contextItemNamespace, string propertyValue)
		{
			var qName = new XmlQName(contextItemName, contextItemNamespace);

			if (XLANGMessageContext == null)
			{
				XLANGMessageContext = new Hashtable();
			}

			if (XLANGMessageContext.ContainsKey(qName))
			{
				XLANGMessageContext[qName] = propertyValue;
			}
			else
			{
				XLANGMessageContext.Add(qName, propertyValue);
			}
		}

		public static string GetContextProperty(string contextItemName, string contextItemNamespace)
		{
			var defaultValue = string.Empty;
			var qName = new XmlQName(contextItemName, contextItemNamespace);

			return XLANGMessageContext != null && XLANGMessageContext.Contains(qName) ? XLANGMessageContext[qName].ToString() : defaultValue;
		}

		public static XLANGMessage CreateMessageFromString(string messageName, string text)
		{
			var stream = new MemoryStream();
			new MemoryStream(Encoding.UTF8.GetBytes(text)).CopyTo(stream);
			stream.Position = 0;
			return CreateBTXMessageFromStream(messageName, stream).GetMessageWrapperForUserCode();
		}

		public static XLANGMessage CreateMessageFromStream(string messageName, Stream stream)
		{
			return CreateBTXMessageFromStream(messageName, stream).GetMessageWrapperForUserCode();
		}

		static BTXMessage CreateBTXMessageFromStream(string messageName, Stream stream)
		{
			var customBTXMessage = new CustomBTXMessage(messageName, Service.RootService.XlangStore.OwningContext);
			customBTXMessage.AddPart(string.Empty, "Body");
			customBTXMessage[0].LoadFrom(stream);
			return customBTXMessage;
		}

		[Serializable]
		public class CustomBTXMessage : BTXMessage
		{
			public CustomBTXMessage(string messageName, Context context)
				: base(messageName, context)
			{
				context.RefMessage(this);
			}
		}

		[ThreadStatic]
		private static Hashtable XLANGMessageContext;
	}
}
