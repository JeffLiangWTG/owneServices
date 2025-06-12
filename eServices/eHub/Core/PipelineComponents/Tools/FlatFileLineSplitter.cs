using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Streaming;
using System.Text.RegularExpressions;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
    public class FlatFileLineSplitter : IFlatFileSplitter
    {
        int splitLineNumber;
        bool preserveHeadLineEnabled;
        string headLine;

        private Stream ftempResult;
        private Stream tempResult
        {
            get { return ftempResult == null ? ftempResult = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk) : ftempResult; }
            set { this.ftempResult = value; }
        }
        private StreamWriter ftempResultWritter;
        private StreamWriter tempResultWritter
        {
            get
            {
                if (this.ftempResultWritter == null)
                {
                    ftempResultWritter = new StreamWriter(this.tempResult);
                    ftempResultWritter.AutoFlush = true;
                }
                return ftempResultWritter;
            }
            set { this.ftempResultWritter = value; }
        }

        public FlatFileLineSplitter()
        {
            this.splitLineNumber = 999;
            this.preserveHeadLineEnabled = true;
            this.headLine = string.Empty;
        }

        public FlatFileLineSplitter(int splitLineNumber, bool preserveHeadLineEnabled)
        {
            this.splitLineNumber = splitLineNumber;
            this.preserveHeadLineEnabled = preserveHeadLineEnabled;
            this.headLine = string.Empty;
        }

        #region IFlatFileSplitter Members

        public IBaseMessage[] Split(IBaseMessage message, IPipelineContext pipelineContext)
        {
            var result = new List<IBaseMessage>();

            var originalDataStream = message.BodyPart.GetOriginalDataStream();
            var originalDataStreamReader = new StreamReader(originalDataStream);

            int readLimit = this.splitLineNumber;
            int currentCount = 0;

            if (this.preserveHeadLineEnabled)
            {
                this.headLine = originalDataStreamReader.ReadLine();
                readLimit -= 1;
            }

            string tempRead = string.Empty;
            createNewVirtualStream();
            while (true)
            {
                tempRead = originalDataStreamReader.ReadLine();

                if (string.IsNullOrEmpty(tempRead))
                {
                    if (currentCount != 0)
                    {
                        result.Add(wrapStream(tempResult, pipelineContext, message));
                    }
                    break;
                }
                else
                {
                    currentCount++;
                    tempResultWritter.WriteLine(tempRead);
                }

                if (currentCount >= readLimit)
                {
                    result.Add(wrapStream(tempResult, pipelineContext, message));
                    createNewVirtualStream();
                    currentCount = 0;
                }
            }

            originalDataStream.Seek(0, SeekOrigin.Begin);
            originalDataStreamReader.Close();
            modifyOverrideFileName(result);
            return result.ToArray();
        }

        private void modifyOverrideFileName(List<IBaseMessage> messageList)
        {
            int index = 0;
            string tempRead = string.Empty;
            foreach (var message in messageList)
            {
                index = messageList.IndexOf(message) + 1;
                tempRead = message.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06") as string;
                if (string.IsNullOrEmpty(tempRead))
                {
                    throw new ApplicationException("Can't find Filename for message");
                }
                tempRead = Regex.Replace(tempRead, @"([^\\]+)(\.[^.\\]+)", "$1_" + index + "$2");
                message.Context.Write("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", tempRead);
            }
        }
        #endregion

        void createNewVirtualStream()
        {
            this.tempResult = null;
            this.tempResultWritter = null;

            if (this.preserveHeadLineEnabled && !string.IsNullOrEmpty(this.headLine))
            {
                tempResultWritter.WriteLine(this.headLine);
            }
        }

        IBaseMessage wrapStream(Stream dataStream, IPipelineContext pipelineContext, IBaseMessage originalMessage)
        {
            IBaseMessage result = null;

            var streamWrapper = new StreamWrapperComponent();
            result = pipelineContext.GetMessageFactory().CreateMessage();
            result.AddPart("body", pipelineContext.GetMessageFactory().CreateMessagePart(), true);
            result.Context = PipelineUtil.CloneMessageContext(originalMessage.Context);

            tempResultWritter.Flush();
            dataStream.Seek(0, SeekOrigin.Begin);
            result.BodyPart.Data = dataStream;

            return result;
        }
    }
}
