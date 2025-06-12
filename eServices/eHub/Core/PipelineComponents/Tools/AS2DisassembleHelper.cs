using Microsoft.BizTalk.AS2.Pipelines;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
	public interface IAS2DisassembleHelper
	{
		IBaseMessage Disassemble(IPipelineContext pipelineContext, IBaseMessage message);
	}

	public class AS2DisassembleHelper : IAS2DisassembleHelper
	{
		readonly AS2Disassembler AS2Dasm;

		public AS2DisassembleHelper()
		{
			AS2Dasm = new AS2Disassembler();
		}

		public IBaseMessage Disassemble(IPipelineContext pContext, IBaseMessage pInMsg)
		{
			AS2Dasm.Disassemble(pContext, pInMsg);
			return AS2Dasm.GetNext(pContext);
		}
	}
}
