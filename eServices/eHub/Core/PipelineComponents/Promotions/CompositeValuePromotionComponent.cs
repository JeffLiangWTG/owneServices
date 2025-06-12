using System;
using System.Collections;
using System.Runtime.InteropServices;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents
{
    /// <summary>
    /// Used to unpack a composite context property value and distribute the values to the nominated context properties. 
    /// </summary>
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
    [Guid("244468D1-6088-4A6E-A408-A230B0C5AC28")]
	public class CompositeValuePromotionComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return "Promote or simply write one or more values into the context from an existing composite (delimited) context property value."; }
		}

		public string Name
		{
			get { return "Composite Value Promoter"; }
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

            Tracer.TraceStart(pipelineContext, message);
            try
            {
                if (String.IsNullOrEmpty(this.CompositePropertyType))
                    throw new ApplicationException("CompositePropertyType must be specified.");
                Tracer.TraceInfo("CompositePropertyType: {0}", this.CompositePropertyType);

                if (String.IsNullOrEmpty(this.PropertyTypes))
                    throw new ApplicationException("PropertyTypes must be specified.");
                Tracer.TraceInfo("PropertyTypes: {0}", this.PropertyTypes);

                if (this.PropertyTypes.IndexOf(',') > -1 && String.IsNullOrEmpty(this.CompositeValueDelimiter))
                    throw new ApplicationException("CompositeValueDelimiter must be specified.");
                Tracer.TraceInfo("CompositeValueDelimiter: {0}", this.CompositeValueDelimiter);

                // Determine the composite property name, namespace and value
                NameNsFormatter compositeProperty = new NameNsFormatter(this.CompositePropertyType);
                string compositeValue = message.Context.Read(compositeProperty.Name, compositeProperty.Namespace) as string;
                Tracer.TraceInfo("CompositeValue: {0}", compositeValue);

                // Unpack the composite value and determine the properties to distribute the parts to
                string[] compositeValueParts = compositeValue.Split(new string[] { this.CompositeValueDelimiter }, StringSplitOptions.None);
                string[] propertyTypes = this.PropertyTypes.Split(',');
                if (compositeValueParts.Length != propertyTypes.Length)
                    throw new ApplicationException("Composite value and property type part counts do not match.  Please review the pipeline configuration.");

                // Distribute the composite value parts to the required properties
                for (int i = 0; i < propertyTypes.Length; i++)
                {
                    NameNsFormatter property = new NameNsFormatter(propertyTypes[i]);
                    object compositeValuePart = compositeValueParts[i] as object;
                    if (this.PromoteValue)
                        message.Context.Promote(property.Name, property.Namespace, compositeValuePart);
                    else
                        message.Context.Write(property.Name, property.Namespace, compositeValuePart);
                    Tracer.TraceInfo("CompositeValueDistribution: {0}. {1}#{2} -> {3}", i + 1, property.Namespace, property.Name, compositeValuePart);
                }

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
            classID = new Guid("244468D1-6088-4A6E-A408-A230B0C5AC28");
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
            try { propertyBag.Read("CompositePropertyType", out var, errorLog); }
            catch { }
            if (var != null) CompositePropertyType = (string)var;

            var = null;
            try { propertyBag.Read("CompositeValueDelimiter", out var, errorLog); }
            catch { }
            if (var != null) CompositeValueDelimiter = (string)var;

            var = null;
            try { propertyBag.Read("PropertyTypes", out var, errorLog); }
            catch { }
            if (var != null) PropertyTypes = (string)var;

            var = null;
            try { propertyBag.Read("Enabled", out var, errorLog); }
            catch { }
            if (var != null) Enabled = (bool)var;
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
            object val = PromoteValue;
            propertyBag.Write("PromoteValue", ref val);

            val = CompositePropertyType;
            propertyBag.Write("CompositePropertyType", ref val);

            val = CompositeValueDelimiter;
            propertyBag.Write("CompositeValueDelimiter", ref val);

            val = PropertyTypes;
            propertyBag.Write("PropertyTypes", ref val);

            val = Enabled;
            propertyBag.Write("Enabled", ref val);
		}

		#endregion

		#region Properties

        /// <summary>
        /// Is the component active on the pipeline
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Promote or simply write the context property
        /// </summary>
        public bool PromoteValue { get; set; }

        /// <summary>
        /// Context property that hosts the composite value(s).  Formatted as namespace#name
        /// </summary>
        public string CompositePropertyType { get; set; }

        /// <summary>
        /// The delimiter used between the composite values.  Used to unpack the values of the <see cref="CompositePropertyType"/>
        /// </summary>
        public string CompositeValueDelimiter { get; set; }

        /// <summary>
        /// Comma separated property types ordered in the same sequence as the values in the <see cref="CompositePropertyType"/>
        /// </summary>
        public string PropertyTypes { get; set; }

		#endregion
	}
}
