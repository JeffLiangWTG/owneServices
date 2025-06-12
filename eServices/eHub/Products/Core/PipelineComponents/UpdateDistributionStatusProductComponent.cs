using System;
using System.Collections;
using System.Runtime.InteropServices;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using CargoWise.eHub.Products.Core.PropertySchemas;

namespace CargoWise.eHub.Products.Core.PipelineComponents
{
	[Serializable]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("DD45765E-7B7C-4BE0-9D61-B208D62E45FB")]
	public class UpdateDistributionStatusProductComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return string.Empty; }
		}

		public string Name
		{
			get { return "Product: Update Distribution Status"; }
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

			var msgEventStream = context.GetEventStream();
			string trackingID = message.Context.ReadPropertyString<MessageTrackingID>(); ;
			msgEventStream.StoreCustomEvent(new UpdateStatusEvent(trackingID, GetOutboxAccessor()));
			return message;
		}

		internal virtual IOutboxAccessor GetOutboxAccessor()
		{
			return DataAccessFactories.NewOutboxAccessorInstance();
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("DD45765E-7B7C-4BE0-9D61-B208D62E45FB");
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
