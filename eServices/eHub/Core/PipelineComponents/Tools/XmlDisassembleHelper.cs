using Microsoft.BizTalk.Component;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Component.Utilities;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
	public interface IXmlDisassembleHelper
	{
		IBaseMessage Disassemble(IPipelineContext pipelineContext, IBaseMessage message);
		IBaseMessage Disassemble(IPipelineContext pipelineContext, IBaseMessage message, Schema documentSchema, Schema envelopeSchema);
		IBaseMessage GetNext(IPipelineContext pipelineContext);
	}

	class XmlDisassembleHelper : IXmlDisassembleHelper
	{
		readonly IDisassemblerComponent dasmComp;

		public XmlDisassembleHelper()
		{
			dasmComp = new XmlDasmComp();
		}

		public XmlDisassembleHelper(IDisassemblerComponent injectedDasmComp)
		{
			dasmComp = injectedDasmComp;
		}

		public virtual IBaseMessage Disassemble(IPipelineContext pipelineContext, IBaseMessage message)
		{
			return Disassemble(pipelineContext, message, null, null);
		}

		public virtual IBaseMessage Disassemble(IPipelineContext pipelineContext, IBaseMessage message, Schema envelopeSchema, Schema documentSchema)
		{
			XmlDasmComp XmlDasm = dasmComp as XmlDasmComp;
			if (envelopeSchema != null && !string.IsNullOrEmpty(envelopeSchema.AssemblyName)) XmlDasm.EnvelopeSpecNames.Add(envelopeSchema);
			if (documentSchema != null && !string.IsNullOrEmpty(documentSchema.AssemblyName)) XmlDasm.DocumentSpecNames.Add(documentSchema);
			XmlDasm.AllowUnrecognizedMessage = true;
			XmlDasm.Disassemble(pipelineContext, message);
			return XmlDasm.GetNext(pipelineContext);
		}

		public IBaseMessage GetNext(IPipelineContext pipelineContext)
		{
			try
			{
				return dasmComp.GetNext(pipelineContext);
			}
			catch (XmlDasmException ex)
			{
				if (ex.HResult == -1061153688)
				{
					return null;
				}
				throw;
			}
		}
	}
}
