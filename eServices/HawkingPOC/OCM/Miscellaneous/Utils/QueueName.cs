using System;

namespace OcmPoc.Utils
{
	public class QueueName
	{
		readonly string name;

		public QueueName(string name)
		{
			this.name = name;
		}

		public static implicit operator string(QueueName queueName) => queueName.name;

		public QueueName Combine(params string[] components)
		{
			var newName = String.Join(".", name, String.Join(".", components));
			return new QueueName(newName);
		}

		public override string ToString() => name;
	}
}
