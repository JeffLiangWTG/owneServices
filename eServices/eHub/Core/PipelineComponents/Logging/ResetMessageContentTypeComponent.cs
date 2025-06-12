using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents
{
    [Serializable]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("0721FC1D-9C3A-44F1-A2E1-0E8FD2E41D8F")]
	public class ResetMessageContentTypeComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return string.Empty; }
		}

		public string Name
		{
			get { return "Reset Message Content Type"; }
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
            Tracer.TraceStart(context, message);
            try
            {
	           	message.BodyPart.ContentType = "";
            	message.BodyPart.Charset = "";
                return message;
            }
            catch (Exception ex)
            {
                Tracer.TraceError(ex);
                throw;
            }
		}

		internal virtual IOutboxAccessor GetOutboxAccessor()
		{
			return DataAccessFactories.NewOutboxAccessorInstance();
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("0721FC1D-9C3A-44F1-A2E1-0E8FD2E41D8F");
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
