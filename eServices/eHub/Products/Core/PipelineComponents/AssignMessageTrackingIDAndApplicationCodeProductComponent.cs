using System;
using System.Collections;
using System.Runtime.InteropServices;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using CargoWise.eHub.Products.Core.PropertySchemas;

namespace CargoWise.eHub.Products.Core.PipelineComponents
{

	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("64CA94D0-21C8-419C-82C1-AC23F065E8C1")]
	public class AssignMessageTrackingIDAndApplicationCodeProductComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{

		#region IBaseComponent Members

		public string Description
		{
			get { return "Product: Assign MessageTrackingID and Application Code"; }
		}

		public string Name
		{
			get { return "Product: Assign MessageTrackingID and Application Code"; }
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


		public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
			if (!this.Enabled) return message;
			message.Context.WriteProperty<ApplicationCode>(ApplicationCode.ToUpper());
			if (!OverrideExisting && !string.IsNullOrEmpty(message.Context.ReadPropertyString<MessageTrackingID>())) return message;
			message.Context.WriteProperty<MessageTrackingID>(Guid.NewGuid().ToString().ToUpper());
			return message;
		}

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("64CA94D0-21C8-419C-82C1-AC23F065E8C1");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object val = null;
			Func<string, bool> getVal = p => { try { propertyBag.Read(p, out val, errorLog); } catch { } return val != null; };

			if (getVal("Enabled")) Enabled = Convert.ToBoolean(val);
			if (getVal("OverrideExisting")) OverrideExisting = Convert.ToBoolean(val);
			if (getVal("ApplicationCode")) ApplicationCode = Convert.ToString(val);
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val;
			val = Enabled; propertyBag.Write("Enabled", ref val);
			val = OverrideExisting; propertyBag.Write("OverrideExisting", ref val);
			val = ApplicationCode; propertyBag.Write("ApplicationCode", ref val);
		}

		internal virtual IOutboxAccessor GetOutboxAccessor()
		{
			return DataAccessFactories.NewOutboxAccessorInstance();
		}

		#region Properties

		public bool Enabled { get; set; }
		public bool OverrideExisting { get; set; }
		public string ApplicationCode { get; set; }

		#endregion
	}
}
