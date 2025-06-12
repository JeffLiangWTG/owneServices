using CargoWise.eHub.Core.Pipelines;
using CargoWise.eHub.Core.Tests.PipelineComponents;
using Microsoft.BizTalk.Edi.Pipelines;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting;


namespace CargoWise.eHub.Core.Tests.Pipelines
{
	[TestClass]
    public class Snd_AssembleEdiTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void Snd_AssembleEdi_Pipeline()
		{
            SendPipelineWrapper pipeline = PipelineFactory.CreateSendPipeline(typeof(Snd_AssembleEDI));
            var ediAssembler = pipeline.GetComponent(PipelineStage.Assemble, 0) as EdiAssembler;
            Assert.IsNotNull(ediAssembler);
            Assert.AreEqual(false, ediAssembler.EdiDataValidation);
            Assert.AreEqual(true, ediAssembler.AllowTrailingDelimiters);
		}
	}
}
