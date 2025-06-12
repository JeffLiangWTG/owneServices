using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents
{
	[Serializable]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("FB59C027-13F0-4DAF-B411-8A716BDF416D")]
	public class FileUnwrapperComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return "Map file stream from XML document."; }
		}

		public string Name
		{
			get { return "File Unwrapper"; }
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

		public IEnumerator Validate(object obj)
		{
			return null;
		}

		#endregion

		#region IComponent Members

		public IBaseMessage Execute(IPipelineContext context, IBaseMessage message)
		{
			if (!Enabled) return message;

			var orginalStream = message.BodyPart.GetOriginalDataStream();

			using (var reader = XmlReader.Create(orginalStream))
			{
				message.Context.Write("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", reader.GetElementAsString("FileName", "http://cargowise.com/ehub/core/2013/08"));

				using (var stream = reader.GetElementAsStream("FileStream", "http://cargowise.com/ehub/core/2013/08"))
				{
					if (stream != null)
					{
						stream.SeekBegin();
						var decodedStream = stream.DecodeStream();
						decodedStream.SeekBegin();

						context.ResourceTracker.AddResource(decodedStream);
						message.BodyPart.Data = decodedStream;
					}
				}
			}

			return message;
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("FB59C027-13F0-4DAF-B411-8A716BDF416D");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object var = null;
			try { propertyBag.Read("Enabled", out var, errorLog); }
			catch { }
			if (var != null) Enabled = (bool)var;
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val = Enabled;
			propertyBag.Write("Enabled", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }

		#endregion
	}
}
