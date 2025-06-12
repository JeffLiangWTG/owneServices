using System;
using System.Text;
using System.Xml.Linq;

namespace OcmPoc.Infrastructure.MessageInterfaces.Documents
{
	public class Message
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string From { get; set; }
		public string To { get; set; }
		public byte[] Body { get; set; }
		public bool IsCompressed { get; set; }

		public static Message Create(XDocument content)
		{
			return Create(content.ToString());
		}

		public static Message Create(string content)
		{
			return new Message
			{
				Body = Encoding.UTF8.GetBytes(content)
			};
		}

		public static Message Create(string name, byte[] content)
		{
			return new Message
			{
				Name = name,
				Body = content
			};
		}
	}
}
