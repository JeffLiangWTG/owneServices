using System;
using System.Collections;
using System.Runtime.InteropServices;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Component.Utilities;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
    [Guid("44482978-1ACF-4AA8-A7FB-F9678CD1CAB9")]
	public class SingleTransfomationComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return "Perform single transformation to target format."; }
		}

		public string Name
		{
			get { return "Single Transformation"; }
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
                if (string.IsNullOrEmpty(TransformationType))
                    throw new ApplicationException("Transformation Type property must be specified.");
                Tracer.TraceInfo("TransformationType: {0}", this.TransformationType);

                if (!TargetSchemaIsSpecified)
                    throw new ApplicationException("Target Schema property must be specified.");
                Tracer.TraceInfo("TargetSchema: {0}", this.TargetSchema.ToString());
                Tracer.TraceInfo("ConsumeZeroLengthResult: {0}", this.ConsumeZeroLengthResult);

				var result = GetTransformationPerformer().PerformTransformation(TransformationType, pipelineContext, message);

				// The largest byte order mark is 4 bytes so only accept streams over this threshold
                if (result.BodyPart.GetOriginalDataStream().Length > 4)
                {
                    // Promote the final schema strong name instead of disassembling the message (which will unpack the message if it's an envelope)
                    IDocumentSpec docSpec = pipelineContext.GetDocumentSpecByName(TargetSchema.ToString());
                    result.Context.WriteProperty<BTS.SchemaStrongName>(docSpec.DocSpecStrongName);
                    result.Context.PromoteProperty<BTS.MessageType>(docSpec.DocType);
                    Tracer.TraceInfo("Final message type: {0} ({1})", docSpec.DocType, docSpec.DocSpecStrongName);
                    Tracer.TraceEnd();
                    return result;
                } 
				else if (ConsumeZeroLengthResult)
                {
                    Tracer.TraceInfo("Consuming zero length message");
                    Tracer.TraceEnd();
                    return null;
                }
                else
                    throw new ApplicationException(String.Format("Execution of transform {0} produced 0 length stream.", TransformationType));
            }
            catch (Exception ex)
            {
                Tracer.TraceError(ex);
				throw new ApplicationException(ex.ToString());
            }
		}

		internal virtual ITransformationPerformer GetTransformationPerformer()
		{
			return new TransformationPerformer();
		}

		#endregion
				
		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
            classID = new Guid("44482978-1ACF-4AA8-A7FB-F9678CD1CAB9");
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
            if (val != null) Enabled = Convert.ToBoolean(val);

			val = null;
			try
			{
				propertyBag.Read("TransformationType", out val, errorLog);
			}
			catch { }
            if (val != null) TransformationType = (string)val;

            val = null;
            try
            {
                propertyBag.Read("TargetSchema", out val, errorLog);
            }
            catch { }
            if (val != null) TargetSchema = new Schema((string)val);

            val = null;
            try
            {
                propertyBag.Read("ConsumeZeroLengthResult", out val, errorLog);
            }
            catch { }
            if (val != null) ConsumeZeroLengthResult = Convert.ToBoolean(val);
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
            object val = Enabled;
            propertyBag.Write("Enabled", ref val);

			val = TransformationType;
			propertyBag.Write("TransformationType", ref val);

			val = TargetSchema == null ? string.Empty : TargetSchema.ToString();
            propertyBag.Write("TargetSchema", ref val);

            val = ConsumeZeroLengthResult;
            propertyBag.Write("ConsumeZeroLengthResult", ref val);
		}

		#endregion

		#region Properties

        public bool Enabled { get; set; }
        public bool ConsumeZeroLengthResult { get; set; }
		public string TransformationType { get; set; }
        public Schema TargetSchema { get; set; }

        private bool TargetSchemaIsSpecified
        {
            get
            {
				return TargetSchema != null && !String.IsNullOrEmpty(TargetSchema.AssemblyName);
            }
        }

		#endregion
	}
}
