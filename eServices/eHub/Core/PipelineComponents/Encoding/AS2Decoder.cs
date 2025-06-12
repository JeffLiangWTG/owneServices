using System;
using System.Collections;
using System.Runtime.InteropServices;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.BizTalk.AS2.Pipelines;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents
{
	/// <summary>
	/// Wraps the BizTalk AS2 decoder to allow enabling it through configuration.
	/// </summary>
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Decoder)]
	[Guid("8E41AE80-220B-4C36-A236-1CAA8CD9D7C6")]
	public class AS2Decoder : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get
			{
				return "Wraps the BizTalk AS2 decoder to allow enabling it through configuration.";
			}
		}

		public string Name
		{
			get { return "eHub AS2 Decoder"; }
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
			if (!this.Enabled)
			{
				return message;
			}

			bool exceptionOccurred = false;
			Tracer.TraceStart(context, message);
			try
			{
				var as2Decoder = new Decoder();
				return as2Decoder.Execute(context, message);
			}
			catch (Exception ex)
			{
				exceptionOccurred = true;
				Tracer.TraceError(ex);
				throw;
			}
			finally
			{
				if (!exceptionOccurred)
				{
					Tracer.TraceEnd();
				}
			}
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("8E41AE80-220B-4C36-A236-1CAA8CD9D7C6");
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
			if (val != null) Enabled = (bool)val;
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val = Enabled;
			propertyBag.Write("Enabled", ref val);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Log details that ARE NOT specific to message or party resolution
		/// </summary>
		public bool Enabled
		{
			get { return _enabled; }
			set { _enabled = value; }
		}
		bool _enabled = false;

		#endregion
	}
}
