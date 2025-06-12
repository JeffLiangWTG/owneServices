using Microsoft.BizTalk.Component;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
	public interface IFFAssembleHelper
	{
		IBaseMessage Assemble(IPipelineContext pipelineContext, IBaseMessage message);
	}

	class FFAssembleHelper : IFFAssembleHelper
	{
		readonly FFAsmComp FFAsm;

		public FFAssembleHelper()
		{
			FFAsm = new FFAsmComp();
		}

		public IBaseMessage Assemble(IPipelineContext pipelineContext, IBaseMessage message)
		{
			FFAsm.AddDocument(pipelineContext, message);
			return FFAsm.Assemble(pipelineContext);
		}
	}
}