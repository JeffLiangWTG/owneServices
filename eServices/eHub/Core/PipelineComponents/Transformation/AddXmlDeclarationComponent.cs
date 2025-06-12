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
	[Guid("25184F9C-BD9D-42d0-B2DC-6C1F38C73A44")]
	public class AddXmlDeclarationComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return "Adds an xml declaration header to xml document."; }
		}

		public string Name
		{
			get { return "Add XML Declaration"; }
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
			if (!Enabled) return message;

			return GetAssembleHelper().Assemble(pipelineContext, message);
		}

		internal virtual XmlAssembleHelper GetAssembleHelper()
		{
			return new XmlAssembleHelper();
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("25184F9C-BD9D-42d0-B2DC-6C1F38C73A44");
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
