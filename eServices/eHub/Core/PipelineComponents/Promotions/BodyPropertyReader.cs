using System;
using System.Collections;
using System.Runtime.InteropServices;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("84538DBE-153F-43df-96F6-DFBBAC1CC444")]
	public class BodyPropertyReader : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return "Reads Message Body Property and writes into message context property."; }
		}

		public string Name
		{
			get { return "Message Body Propery Reader"; }
		}

		public string Version
		{
			get { return "1.0"; }
		}

		#endregion

		#region IComponentUI Members

		public IntPtr Icon
		{
			get { return IntPtr.Zero; ; }
		}

		public IEnumerator Validate(object projectSystem)
		{
			return null;
		}

		#endregion

		#region IComponent Members

		public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
			if (!Enabled) return message;

			Tracer.TraceStart(pipelineContext, message);
			try
			{
				if (string.IsNullOrEmpty(BodyPropertyType))
					throw new ApplicationException("BodyPropertyType cannot be null.");

				if (string.IsNullOrEmpty(ContextPropertyType))
					throw new ApplicationException("ContextPropertyType cannot be null.");

				if (string.IsNullOrEmpty(Format))
					throw new ApplicationException("Format cannot be null.");

				var bodyBroperty = new NameNsFormatter(BodyPropertyType);
				var contextProperty = new NameNsFormatter(ContextPropertyType);

				string value = (string)message.BodyPart.PartProperties.Read(bodyBroperty.Name, bodyBroperty.Namespace);
				if (string.IsNullOrEmpty(value))
				{
					throw new ApplicationException(string.Format("Message Body Property '{0}#{1}' missing in message body.", bodyBroperty.Namespace, bodyBroperty.Name));
				}

				if (Promote)
				{
					message.Context.Promote(contextProperty.Name, contextProperty.Namespace, string.Format(Format, value));
				}
				else
				{
					message.Context.Write(contextProperty.Name, contextProperty.Namespace, string.Format(Format, value));
				}

				Tracer.TraceEnd();
			}
			catch (Exception ex)
			{
				Tracer.TraceError(ex);
				throw;
			}
			return message;
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("84538DBE-153F-43df-96F6-DFBBAC1CC444");
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

			var = null;
			try { propertyBag.Read("BodyPropertyType", out var, errorLog); }
			catch { }
			if (var != null) BodyPropertyType = (string)var;

			var = null;
			try { propertyBag.Read("Format", out var, errorLog); }
			catch { }
			if (var != null) Format = (string)var;

			var = null;
			try { propertyBag.Read("ContextPropertyType", out var, errorLog); }
			catch { }
			if (var != null) ContextPropertyType = (string)var;

			var = null;
			try { propertyBag.Read("Promote", out var, errorLog); }
			catch { }
			if (var != null) Promote = (bool)var;
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val = Enabled;
			propertyBag.Write("Enabled", ref val);

			val = BodyPropertyType;
			propertyBag.Write("BodyPropertyType", ref val);

			val = Format;
			propertyBag.Write("Format", ref val);

			val = ContextPropertyType;
			propertyBag.Write("ContextPropertyType", ref val);

			val = Promote;
			propertyBag.Write("Promote", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }
		public string BodyPropertyType { get; set; }
		public string Format { get; set; }
		public string ContextPropertyType { get; set; }
		public bool Promote { get; set; }

		#endregion
	}
}
