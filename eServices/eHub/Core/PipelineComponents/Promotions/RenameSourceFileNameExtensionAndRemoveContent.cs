using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents
{
	[Serializable]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("923BEFDE-9F41-467F-96DF-32DE86EE7719")]
	public class RenameSourceFileNameExtensionAndRemoveContent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return string.Empty; }
		}

		public string Name
		{
			get { return "Rename Source File Extension and Remove Content"; }
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

		public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
			if (!this.Enabled) return message;

			if (String.IsNullOrEmpty(this.PropertyType))
				throw new ApplicationException("PropertyType must be specified.");

			if (String.IsNullOrEmpty(this.Extension))
				throw new ApplicationException("Extension must be specified.");

			var property = new NameNsFormatter(this.PropertyType);
			string sourceFile = message.Context.Read(property.Name, property.Namespace) as string;

			if (string.IsNullOrEmpty(sourceFile))
				throw new ApplicationException("provided PropertyType could not be found.");

			message.Context.Promote(property.Name, property.Namespace, Path.GetFileNameWithoutExtension(sourceFile) + "." + this.Extension);
			message.BodyPart.Data = new MemoryStream();
			return message;
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("923BEFDE-9F41-467F-96DF-32DE86EE7719");
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
			try { propertyBag.Read("PropertyType", out var, errorLog); }
			catch { }
			if (var != null) PropertyType = (string)var;

			var = null;
			try { propertyBag.Read("Extension", out var, errorLog); }
			catch { }
			if (var != null) Extension = (string)var;
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val = Enabled;
			propertyBag.Write("Enabled", ref val);

			val = PropertyType;
			propertyBag.Write("PropertyType", ref val);

			val = Extension;
			propertyBag.Write("Extension", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }
		public string PropertyType { get; set; }
		public string Extension { get; set; }

		#endregion
	}
}
