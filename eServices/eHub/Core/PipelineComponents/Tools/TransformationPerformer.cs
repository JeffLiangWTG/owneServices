using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using Microsoft.XLANGs.RuntimeTypes;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
	interface ITransformationPerformer
	{
		IBaseMessage PerformTransformation(string typeName, IPipelineContext pipelineContext, IBaseMessage message);
		IBaseMessage PerformTransformation(List<TransformSet> transformList, IPipelineContext pipelineContext, IBaseMessage message, out string lastTargetMessageType);
	}

	internal class TransformationPerformer : ITransformationPerformer
	{
		#region ITransformationPerformer Members

		public IBaseMessage PerformTransformation(string typeName, IPipelineContext pipelineContext, IBaseMessage message)
		{
			Tracer.TraceInfo(string.Format("Run Transform: {1} - ThreadID: {0}", Thread.CurrentThread.ManagedThreadId, typeName));
			var result = GetMessageClone(pipelineContext, message);
			PerformTransformationCore(typeName, result);
			return result;
		}

		public IBaseMessage PerformTransformation(List<TransformSet> transformList, IPipelineContext pipelineContext, IBaseMessage message, out string lastTargetMessageType)
		{
			var result = GetMessageClone(pipelineContext, message);
			lastTargetMessageType = string.Empty;
			foreach (var set in transformList)
			{
				foreach (var map in set.Transforms)
				{
					Tracer.TraceInfo("Transformation: {0}", map.MapTypeName);
					PerformTransformationCore(map.MapTypeName, result);
					// The largest byte order mark is 4 bytes so throw an exception if the stream is shorter than this
					if (result.BodyPart.GetOriginalDataStream().Length < 5)
						throw new ApplicationException(String.Format("Execution of transform {0} produced 0 length stream", map.MapTypeName));

					lastTargetMessageType = map.TargetMessageType;
				}
				break; // We only handle one transformation set at the moment
			}
			return result;
		}

		#endregion

		#region Implementation

		void PerformTransformationCore(string typeName, IBaseMessage message)
		{
			try
			{
				var persistentStream = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk);
				using (var reader = XmlTextReader.Create(message.BodyPart.GetOriginalDataStream()))
				{
					var transform = TransformMetaData.For(Type.GetType(typeName, true));
					lock (transform)
					{
						Tracer.TraceInfo("Transformation Start: Thread {0}, {1}", Thread.CurrentThread.ManagedThreadId, typeName);
						try
						{
#pragma warning disable CS0612
							transform.StreamingTransform.ScalableTransform(reader, transform.ArgumentList, persistentStream, new XmlUrlResolver(), false);
#pragma warning restore CS0612
						}
						catch (Exception ex)
						{
							if (ex.Message.Contains("GetTempFileName failed"))
							{
								TempFiles.CleanupBizTalkTempFiles();
#pragma warning disable CS0612
								transform.StreamingTransform.ScalableTransform(reader, transform.ArgumentList, persistentStream, new XmlUrlResolver(), false);
#pragma warning disable CS0612
							}
							else {
								throw;
							}
						}
						Tracer.TraceInfo("Transformation End  : Thread {0}, {1}", Thread.CurrentThread.ManagedThreadId, typeName);
					}
				}

				persistentStream.Seek(0, SeekOrigin.Begin);
				message.BodyPart.Data = new ReadOnlySeekableStream(persistentStream);
			}
			catch (Exception ex)
			{
				throw new ApplicationException(String.Format("\nTransformation {0}\nfailed with exception: {1}", typeName, ex.ToString()));
			}
		}

		IBaseMessage GetMessageClone(IPipelineContext pipelineContext, IBaseMessage message)
		{
			var result = pipelineContext.GetMessageFactory().CreateMessage();
			result.Context = PipelineUtil.CloneMessageContext(message.Context);
			var internalMessagePart = pipelineContext.GetMessageFactory().CreateMessagePart();
			string partName = string.Empty;
			message.GetPartByIndex(0, out partName);
			result.AddPart(partName, internalMessagePart, true);
			result.BodyPart.Charset = message.BodyPart.Charset;
			result.BodyPart.ContentType = message.BodyPart.ContentType;
			result.BodyPart.Data = message.BodyPart.Data;

			return result;
		}

		#endregion
	}
}
