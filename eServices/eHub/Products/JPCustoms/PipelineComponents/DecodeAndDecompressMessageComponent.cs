using System;
using System.Collections;
using System.Runtime.InteropServices;
using CargoWise.eHub.Common.Extensions;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.JPCustoms.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[Guid("DAC43C37-4FA1-4C3B-8E3B-E170DDAF9DE3")]
	public class DecodeAndDecompressMessageComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Membersва

		public string Description
		{
			get { return "Decode and Decompress Message Component"; }
		}

		public string Name
		{
			get { return "DecodeAndDecompressMessageComponent"; }
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
			MessageHelper.WrapMessageInReadOnlySeekableStream(pipelineContext, message);
			MessageHelper.DecodeAndDecompressStream(pipelineContext, message);
			message.BodyPart.Data.SeekBegin();
			return message;
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("DAC43C37-4FA1-4C3B-8E3B-E170DDAF9DE3");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object val = null;
			Func<string, bool> getVal = p => { try { propertyBag.Read(p, out val, errorLog); } catch { } return val != null; };

			if (getVal("Enabled")) Enabled = Convert.ToBoolean(val);
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val;
			val = Enabled; propertyBag.Write("Enabled", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }

		#endregion
	}
}

