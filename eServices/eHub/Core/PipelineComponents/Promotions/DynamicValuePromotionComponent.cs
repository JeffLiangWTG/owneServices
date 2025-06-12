using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents
{
	/// <summary>
	/// Allows the composition of multiple existing context property values into one other context property value with the option
	/// to write or promote the new property value.  This component can be used to copy a value into another context property
	/// or for retaining one or more existing context property values for reconstituting them at a later stage using the
	/// <see cref="CompositeValuePromotionComponent"/>
	/// </summary>
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("C51D6E0F-4892-45CB-A3A6-81705A716017")]
	public class DynamicValuePromotionComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return "Promote or simply write a composed value into the context from existing context property values."; }
		}

		public string Name
		{
			get { return "Dynamic Value Promoter"; }
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
				if (String.IsNullOrEmpty(this.DynamicPropertyType))
					throw new ApplicationException("DynamicPropertyType must be specified.");
				Tracer.TraceInfo("DynamicPropertyType: {0}", this.DynamicPropertyType);

				if (String.IsNullOrEmpty(this.DynamicPropertyValueFormat))
					throw new ApplicationException("DynamicPropertyValueFormat must be specified.");
				Tracer.TraceInfo("DynamicPropertyValueFormat: {0}", this.DynamicPropertyValueFormat);

				if (String.IsNullOrEmpty(this.ExistingValuePropertyTypes))
					throw new ApplicationException("ExistingValuePropertyTypes must be specified.");
				Tracer.TraceInfo("ExistingValuePropertyTypes: {0}", this.ExistingValuePropertyTypes);

				// Compose the property value from other context property values
				List<object> parameterList = new List<object>();
				string[] inputParameters = this.ExistingValuePropertyTypes.Split(',');
				foreach (string inputParameter in inputParameters)
				{
					switch (PromotionTools.GetParameterType(inputParameter))
					{
						case ParameterType.MethodCall:
							parameterList.Add(PromotionTools.CallMethod(message, inputParameter));
							break;
						case ParameterType.ContextProperty:
							parameterList.Add(PromotionTools.ReadPropertyValue(message, inputParameter));
							break;
						default:
							throw new FormatException(string.Format("Invalid property promotion parameter:{0}", inputParameter));
					}
				}

				// Set the dynamic property
				string dynamicValue = String.Format(this.DynamicPropertyValueFormat, parameterList.ToArray());
				Tracer.TraceInfo("DynamicPropertyValue: {0}", dynamicValue);

				// Determine the property name and namespace to set
				NameNsFormatter dynamicProperty = new NameNsFormatter(this.DynamicPropertyType);

				if (this.PromoteValue)
					message.Context.Promote(dynamicProperty.Name, dynamicProperty.Namespace, dynamicValue);
				else
					message.Context.Write(dynamicProperty.Name, dynamicProperty.Namespace, dynamicValue);

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
			classID = new Guid("C51D6E0F-4892-45CB-A3A6-81705A716017");
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
			try { propertyBag.Read("DynamicPropertyType", out var, errorLog); }
			catch { }
			if (var != null) DynamicPropertyType = (string)var;

			var = null;
			try { propertyBag.Read("DynamicPropertyValueFormat", out var, errorLog); }
			catch { }
			if (var != null) DynamicPropertyValueFormat = (string)var;

			var = null;
			try { propertyBag.Read("ExistingValuePropertyTypes", out var, errorLog); }
			catch { }
			if (var != null) ExistingValuePropertyTypes = (string)var;

			var = null;
			try { propertyBag.Read("Enabled", out var, errorLog); }
			catch { }
			if (var != null) Enabled = (bool)var;
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val = PromoteValue;
			propertyBag.Write("PromoteValue", ref val);

			val = DynamicPropertyType;
			propertyBag.Write("DynamicPropertyType", ref val);

			val = DynamicPropertyValueFormat;
			propertyBag.Write("DynamicPropertyValueFormat", ref val);

			val = ExistingValuePropertyTypes;
			propertyBag.Write("ExistingValuePropertyTypes", ref val);

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
		/// The type to write or promote after composing the value from the <see cref="ExistingValuePropertyTypes"/>.  Formatted as namespace#name
		/// </summary>
		public string DynamicPropertyType { get; set; }

		/// <summary>
		/// The format to use when constructing the composite property value (eg) {0}|{1}|{2}.  Note that the indexing must must match
		/// the number of comma separated <see cref="ExistingValuePropertyTypes"/>
		/// </summary>
		public string DynamicPropertyValueFormat { get; set; }

		/// <summary>
		/// Comma separated property type names to compose using the <see cref="DynamicPropertyValueFormat"/>.  Each property formatted as namespace#name
		/// </summary>
		public string ExistingValuePropertyTypes { get; set; }

		#endregion
	}
}
