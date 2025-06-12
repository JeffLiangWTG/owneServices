using System;
using System.Collections;
using System.Runtime.InteropServices;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents
{
    /// <summary>
    /// Promotes or writes a value (set on the pipeline binding) to the required context property.
    /// </summary>
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
    [Guid("0BAB4B30-AD94-4A43-ABCF-0E922D4BFA68")]
	public class StaticValuePromotionComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return "Promote or simply write a static value into the context"; }
		}

		public string Name
		{
			get { return "Static Value Promoter"; }
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

			// Only continue processing if this is not an AS2 MDN
			if (PropertyInspector.IsAs2Mdn(message)) return message;

			// Only continue processing if this is not an EDI acknowledgement
			if (PropertyInspector.IsSystemGeneratedEdiAck(message)) return message;

            Tracer.TraceStart(pipelineContext, message);
            try
            {
                if (String.IsNullOrEmpty(this.PropertyType))
                    throw new ApplicationException("PropertyType must be specified.");
                Tracer.TraceInfo("PropertyType: {0}", this.PropertyType);

                if (String.IsNullOrEmpty(this.Value))
                    throw new ApplicationException("Value must be specified.");

                // Attempt a conversion to special types. Do this to ensure routing properties are loaded in the correct type so
                // the routing engine can serialise the values correctly.  If this is not done routing failures occur.  The alternatives
                // to doing this are to set all properties to strings or specify the type on the as a property of this class.
                object valueToWrite = null;
                bool boolValue = false;
                int intValue = 0;
                if (Boolean.TryParse(Value, out boolValue))
                    valueToWrite = boolValue;
                else if (Int32.TryParse(Value, out intValue))
                    valueToWrite = intValue;
                else
                    valueToWrite = Value;

                Tracer.TraceInfo("Value: {0} as {1}", this.Value, valueToWrite.GetType().Name);

                // Perform the promotion or write
                NameNsFormatter property = new NameNsFormatter(this.PropertyType);
                if (PromoteValue)
                    message.Context.Promote(property.Name, property.Namespace, valueToWrite);
                else
                    message.Context.Write(property.Name, property.Namespace, valueToWrite);

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
            classID = new Guid("0BAB4B30-AD94-4A43-ABCF-0E922D4BFA68");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object var = null;
            try { propertyBag.Read("PromoteValue", out var, errorLog); }
			catch { }
            if (var != null) PromoteValue = (bool)var;

            var = null;
            try { propertyBag.Read("PropertyType", out var, errorLog); }
            catch { }
            if (var != null) PropertyType = (string)var;

            var = null;
            try { propertyBag.Read("Value", out var, errorLog); }
            catch { }
            if (var != null) Value = (string)var;

			var = null;
			try { propertyBag.Read("Enabled", out var, errorLog); }
			catch { }
			if (var != null) Enabled = (bool)var;
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
            object val = PromoteValue;
            propertyBag.Write("PromoteValue", ref val);

            val = PropertyType;
            propertyBag.Write("PropertyType", ref val);

            val = Value;
            propertyBag.Write("Value", ref val);

			val = Enabled;
			propertyBag.Write("Enabled", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled 
		{
			get { return enabled; }
			set { enabled = value; } 
		}
		bool enabled = true;

		public bool PromoteValue { get; set; }
        public string PropertyType { get; set; }
        public string Value { get; set; }

		#endregion
	}
}
