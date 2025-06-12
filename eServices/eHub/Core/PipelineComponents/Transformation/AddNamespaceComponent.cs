using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;

namespace CargoWise.eHub.Core.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("D0C4DBE1-AAD5-4800-A504-E6C86EFC00FD")]
	public class AddNamespaceComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return "Adds a primary namespace to an Xml document updating the root node prefix if one is specified."; }
		}

		public string Name
		{
			get { return "Add Namespace"; }
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

		#region IComponent Members

		public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
			if (!this.Enabled) return message;

			Tracer.TraceStart(pipelineContext, message);
			try
			{
				if (String.IsNullOrEmpty(this.Namespace))
					throw new ApplicationException("Namespace must be specified");
				Tracer.TraceInfo("Namespace: {0}", this.Namespace);
				Tracer.TraceInfo("NamespacePrefix: {0}", this.NsPrefix);
				Tracer.TraceInfo("ReplaceInvalidChars: {0}", this.ReplaceInvalidChars);

				// Input reader settings
				var inputSettings = new XmlReaderSettings();
				inputSettings.IgnoreWhitespace = true;
				inputSettings.IgnoreComments = true;
				inputSettings.CloseInput = false;

				// Output writer settings
				var outputSettings = new XmlWriterSettings();
				outputSettings.CloseOutput = false;
				outputSettings.Indent = false;
				outputSettings.Encoding = Encoding.GetEncoding(CodePageEncoding);
				outputSettings.NewLineHandling = NewLineHandling.None;

				bool nsAdded = false;
				var persistentStream = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk);
				using (var outputWriter = XmlWriter.Create(persistentStream, outputSettings))
				{
					if (ReplaceInvalidChars)
					{
						using (var inputStreamReader = new StreamReader(message.BodyPart.GetOriginalDataStream()))
						using (var inputReader = XmlReader.Create(inputStreamReader, inputSettings))
						{
							AddNamespace(inputReader);
						}
					}
					else
					{
						using (var inputReader = XmlReader.Create(message.BodyPart.GetOriginalDataStream(), inputSettings))
						{
							AddNamespace(inputReader);
						}
					}

					void AddNamespace(XmlReader inputReader)
					{
						// Move to the root element
						inputReader.MoveToContent();
						if (String.IsNullOrEmpty(inputReader.Prefix) && String.IsNullOrEmpty(inputReader.NamespaceURI))
						{
							XmlStreamHelper.CloneXmlStream(inputReader, outputWriter, this.NsPrefix, this.Namespace);
							nsAdded = true;
						}
					}
				}

				// Check if we actually did any work and reassign the message content if we did
				if (nsAdded)
				{
					persistentStream.Seek(0, SeekOrigin.Begin);
					message.BodyPart.Data = new ReadOnlySeekableStream(persistentStream);
				}
				else
				{
					message.BodyPart.Data.Seek(0, SeekOrigin.Begin);
				}

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

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("D0C4DBE1-AAD5-4800-A504-E6C86EFC00FD");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object val = null;
			try
			{
				propertyBag.Read("Enabled", out val, errorLog);
			}
			catch { }
			if (val != null) Enabled = Convert.ToBoolean(val);

			val = null;
			try
			{
				propertyBag.Read("Namespace", out val, errorLog);
			}
			catch { }
			if (val != null) Namespace = (string)val;

			val = null;
			try
			{
				propertyBag.Read("NsPrefix", out val, errorLog);
			}
			catch { }
			if (val != null) NsPrefix = (string)val;

			val = null;
			try
			{
				propertyBag.Read("CodePageEncoding", out val, errorLog);
			}
			catch { }
			if (val != null) CodePageEncoding = (string)val;

			val = null;
			try
			{
				propertyBag.Read("ReplaceInvalidChars", out val, errorLog);
			}
			catch { }
			if (val != null)
				ReplaceInvalidChars = Convert.ToBoolean(val);
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val = Enabled;
			propertyBag.Write("Enabled", ref val);

			val = Namespace;
			propertyBag.Write("Namespace", ref val);

			val = NsPrefix;
			propertyBag.Write("NsPrefix", ref val);

			val = CodePageEncoding;
			propertyBag.Write("CodePageEncoding", ref val);

			val = ReplaceInvalidChars;
			propertyBag.Write("ReplaceInvalidChars", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }
		public string Namespace { get; set; }
		public string NsPrefix { get; set; }

		string codePageEncoding = "UTF-8";
		public string CodePageEncoding 
		{
			get { return codePageEncoding; }
			set { codePageEncoding = value; }
		}
		public bool ReplaceInvalidChars { get; set; }

		#endregion
	}
}
