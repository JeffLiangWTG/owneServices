using Microsoft.BizTalk.Component;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Component.Utilities;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
	public interface IFFDisassembleHelper
	{
		IBaseMessage Disassemble(IPipelineContext pipelineContext, IBaseMessage message, SchemaWithNone documentSchema);
		IBaseMessage GetNext(IPipelineContext pipelineContext);
	}

	public class FFDisassembleHelper : IFFDisassembleHelper
	{
		#region .ctor

		readonly FFDasmComp FFDasm;

		public FFDisassembleHelper()
		{
			FFDasm = new FFDasmComp();
		}
		#endregion

		#region IFFDisassembleHelper Members

		public IBaseMessage Disassemble(IPipelineContext pipelineContext, IBaseMessage message, SchemaWithNone documentSchema)
		{
			if (documentSchema != null && !string.IsNullOrEmpty(documentSchema.AssemblyName)) FFDasm.DocumentSpecName = documentSchema;
			FFDasm.Disassemble(pipelineContext, message);
			return FFDasm.GetNext(pipelineContext);
		}

		public IBaseMessage GetNext(IPipelineContext pipelineContext)
		{
			return FFDasm.GetNext(pipelineContext);
		}

		#endregion
	}
}
