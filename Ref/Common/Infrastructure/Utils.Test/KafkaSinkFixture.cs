using System.Threading;
using Confluent.Kafka;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test
{
	[TestFixture]
	class KafkaSinkFixture
	{
		[Test]
		public void Emit()
		{
			var producer = new Mock<IProducer<Null, string>>();
			string message = null;
			producer.Setup(x => x.ProduceAsync("logs", It.IsAny<Message<Null, string>>(), It.IsAny<CancellationToken>())).Callback<string, Message<Null, string>, CancellationToken>((_, m, c) =>
			{
				message = m.Value;
			});
			var expectdMsg = "\"level\":\"Information\",\"messageTemplate\":\"{@Message}\",\"message\":\"{@Message}\",\"fields\":{\"ApiLogRequest\":{\"_typeTag\":\"ApiLogRequest\",\"Id\":1,\"AuthType\":null,\"UserId\":\"A\",\"Body\":\"Hello\",\"Uri\":null},\"SourceContext\":\"CargoWise.RefDbRepo.Common.Utils.Test.DelegatingSink\"}}";
			using (var sink = new KafkaSink(1, 1, "localhost:9002", "logs", SecurityProtocol.Plaintext, producer.Object))
			{
				var log = DelegatingSink.GetLogEvent(l => l.Information("{@Message}", new ApiLogRequest { Id = 1, UserId = "A", Body = "Hello" }));
				sink.Emit(log);
			}
			producer.Verify(x => x.Dispose());
			Assert.That(message, Does.Contain(expectdMsg));
		}
	}
}
