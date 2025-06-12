using Microsoft.BizTalk.Component;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Edi.Pipelines;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
	class XmlAssembleHelper
	{
		readonly XmlAsmComp XmlAsm;

		public XmlAssembleHelper()
		{
			XmlAsm = new XmlAsmComp();
		}

		virtual public IBaseMessage Assemble(IPipelineContext pipelineContext, IBaseMessage message)
		{
			XmlAsm.AddDocument(pipelineContext, message);
			return XmlAsm.Assemble(pipelineContext);
		}
	}

	class XmlEdiAssembleHelper
	{
		readonly EdiAssembler XmlAsm;

		public XmlEdiAssembleHelper()
		{
			XmlAsm = new EdiAssembler();
		}

		virtual public IBaseMessage Assemble(IPipelineContext pipelineContext, IBaseMessage message)
		{
			XmlAsm.EdiDataValidation = false;
			XmlAsm.AddDocument(pipelineContext, message);
			return XmlAsm.Assemble(pipelineContext);
		}
	}
}
