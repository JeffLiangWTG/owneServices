using System;
using System.Linq;
using CargoWise.eHub.Products.NZCustoms.Common;

namespace CargoWise.eHub.Products.NZCustoms.PullService
{
    public class PullResult
    {
        public PullResult(string customerReference, string messageReference, string messageName, Attachment[] attachments)
        {
            if (string.IsNullOrWhiteSpace(customerReference)) throw new ArgumentException("customerReference can't be empty");
            if (string.IsNullOrWhiteSpace(messageReference)) throw new ArgumentException("messageReference can't be empty");

            if (attachments == null) throw new ArgumentNullException("attachments");
            if (attachments.Length == 0) throw new ArgumentException("attachments array should have at least one element.");

            CustomerReference = customerReference;
            MessageReference = messageReference;
            this.messageName = messageName;
            this.attachments = attachments;
        }

        public PullStatus Status
        {
            get { return !string.IsNullOrEmpty(CustomerReference) ? PullStatus.MessageReceived : PullStatus.NoMessageReceived; }
        }

        public string CustomerReference { get; private set; }
        public string MessageReference { get; private set; }
        readonly string messageName;
        readonly Attachment[] attachments;

        public string Content
        {
            get { return ContentAttachment.Content; }
        }

        protected Attachment ContentAttachment
        {
            get 
            {
                return content ?? (content = attachments.FirstOrDefault(a => a.Filename == messageName) ?? attachments[0]);
            }
        }
        Attachment content;


        public Attachment[] Attachments
        {
            get { return attachments.Where(a => a != ContentAttachment).ToArray(); }
        }

        public enum PullStatus
        {
            MessageReceived,
            NoMessageReceived
        }
    }
}
