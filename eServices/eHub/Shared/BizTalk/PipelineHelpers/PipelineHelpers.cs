using System;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using Common.Logging;
using Common.Logging.Simple;
using Microsoft.BizTalk.Component.Interop;

namespace CargoWise.eHub.Shared.BizTalk.PipelineHelpers
{
	public class PipelineHelpers
	{
		public void CreateSeekableMessageStream(IPipelineContext pc, IBaseMessage inmsg, ILog logger = null)
		{
			if (pc == null) throw new ArgumentNullException("pc");
			if (inmsg == null) throw new ArgumentNullException("inmsg");
			if (logger == null) logger = new NoOpLogger();
			logger.Debug("Creating seekable stream.");

			var persist = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk);
			var seekable = new ReadOnlySeekableStream(inmsg.BodyPart.GetOriginalDataStream(), persist);
			pc.ResourceTracker.AddResource(persist);
			pc.ResourceTracker.AddResource(seekable);
			inmsg.BodyPart.Data = seekable;
		}

		public IBaseMessage CloneMessage(IPipelineContext pc, IBaseMessage inmsg)
		{
			return CloneMessage(pc, inmsg, new NoOpLogger());
		}

		public IBaseMessage CloneMessage(IPipelineContext pc, IBaseMessage inmsg, ILog logger)
		{
			if (pc == null) throw new ArgumentNullException("pc");
			if (inmsg == null) throw new ArgumentNullException("inmsg");
			if (logger == null) throw new ArgumentNullException("logger");
			logger.Debug("Cloning BizTalk message.");

			if (!inmsg.BodyPart.GetOriginalDataStream().CanSeek)
				CreateSeekableMessageStream(pc, inmsg, logger);

			var persistCopy = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk);
			inmsg.BodyPart.GetOriginalDataStream().Position = 0;
			inmsg.BodyPart.GetOriginalDataStream().CopyTo(persistCopy);
			inmsg.BodyPart.GetOriginalDataStream().Position = 0;
			persistCopy.Position = 0;
			pc.ResourceTracker.AddResource(persistCopy);

			var msgCopy = pc.GetMessageFactory().CreateMessage();
			msgCopy.AddPart("Body", pc.GetMessageFactory().CreateMessagePart(), true);
			msgCopy.BodyPart.Data = persistCopy;
			msgCopy.Context = PipelineUtil.CloneMessageContext(inmsg.Context);
			return msgCopy;
		}
	}
}
