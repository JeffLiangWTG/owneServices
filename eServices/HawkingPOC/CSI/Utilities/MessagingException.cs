using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Hawking.CSI.Utilities
{
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class MessagingException : Exception
    {
        public string Content { get; set; }

        public MessagingException() { }

        public MessagingException(string message) : base(message) { }

        public MessagingException(string message, string content) : base(message)
        {
            Content = content;
        }

        public MessagingException(string message, Exception inner) : base(message, inner) { }

        public MessagingException(string message, string content, Exception inner) : base(message, inner)
        {
            Content = content;
        }

        protected MessagingException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
