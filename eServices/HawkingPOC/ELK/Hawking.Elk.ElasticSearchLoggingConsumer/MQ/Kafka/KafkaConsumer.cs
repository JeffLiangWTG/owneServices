using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Hawking.Elk.ElasticSearchLoggingConsumer.Config;
using Hawking.Elk.ElasticSearchLoggingConsumer.MQ.ElasticSearch;
using log4net;
using Nest;

namespace Hawking.Elk.ElasticSearchLoggingConsumer.MQ.Kafka
{
    public class KafkaConsumer : IKafkaConsumer
    {
        const string UnderScore = "_";
        const string IllegalPathCharsPattern = @"[!\*'\(\);:@&=\-\+\$,/\?%\#\[\.\]]";

        readonly ILog logger;
        readonly IElasticSearchClient elasticSearchClient;

        public KafkaConsumer(ILog logger, IElasticSearchClient elasticSearchClient)
        {
            this.logger = logger;
            this.elasticSearchClient = elasticSearchClient;
        }

        public async Task RunAsync(IKafkaSubscription subscription, CancellationToken cancellationToken)
        {
            logger.Debug($"{nameof(RunAsync)}::enter");

            using (var consumer =
                new Consumer<Null, string>(subscription.KafkaConsumerConfig, null, new StringDeserializer(Encoding.UTF8)))
            {
                var consumerError = false;
                consumer.OnError += (sender, error) =>
                {
                    Console.WriteLine($"Error: {error}");
                    consumerError = true;
                };

                consumer.OnConsumeError += ConsumerOnConsumeError;
                consumer.Subscribe(subscription.Topic);
                consumer.OnOffsetsCommitted += ConsumerOnOffsetsCommitted;
                consumer.OnPartitionsAssigned += ConsumerOnOnPartitionsAssigned;
                consumer.OnPartitionsRevoked += ConsumerOnOnPartitionsRevoked;
                consumer.OnStatistics += (_, json) => Console.WriteLine($"Statistics: {json}");
                consumer.OnPartitionEOF += ConsumerOnOnPartitionEof;

                try
                {
                    var topicPartitionOffset = new TopicPartitionOffset(new TopicPartition(subscription.Topic, subscription.Partition), new Offset(subscription.Offset));

                    consumer.Assign(new[] { topicPartitionOffset });
                    consumer.Seek(topicPartitionOffset);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                try
                {
                    logger.Info("Starting consumer.");

                    while (!consumerError)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        consumer.Poll(TimeSpan.FromMilliseconds(100));

                        while (consumer.Consume(out var message, TimeSpan.FromMilliseconds(100)))
                        {
                            logger.Info($"Read message from: {message.TopicPartitionOffset}");
                            logger.Debug(new
                            {
                                Kafka = new {message.Topic, message.Partition, message.Offset, message.Value}
                            });

                            Console.WriteLine(
                                $"Kafka Message: '{message.Topic}':{message.Partition}.{message.Offset}");

                            var response = SaveKafkaMessage(message.Value, subscription.Topic,
                                subscription.MessageType);
                            if (response == null)
                            {
                                throw new ApplicationException("Null response");
                            }

                            if (!response.IsValid)
                            {
                                Console.WriteLine(response.DebugInformation);
                                throw new ApplicationException(response.DebugInformation);
                            }

                            await consumer.CommitAsync();

                            subscription.Partition = message.Partition;
                            subscription.Offset = message.Offset;

                            cancellationToken.ThrowIfCancellationRequested();
                        }
                    }
                }
                catch (Exception ex)
                {
                    subscription.UpdateAndSaveKeyValues();

                    logger.Error(ex.ToString());
                    throw;
                }
            }
        }

        #region Consumer event handlers

        void ConsumerOnOnPartitionEof(object sender, TopicPartitionOffset end)
        {
            Console.WriteLine($"Reached end of topic {end.Topic} partition {end.Partition}, next message will be at offset {end.Offset}");
        }

        void ConsumerOnOnPartitionsRevoked(object sender, List<TopicPartition> partitions)
        {
            if (sender is Consumer consumer)
            {
                Console.WriteLine($"Revoked partitions: [{string.Join(", ", partitions)}]");
                consumer.Unassign();
            }
        }

        void ConsumerOnOnPartitionsAssigned(object sender, List<TopicPartition> partitions)
        {
            if (sender is Consumer consumer)
            {
                Console.WriteLine(
                    $"Assigned partitions: [{string.Join(", ", partitions)}], member id: {consumer.MemberId}");

                consumer.Assign(partitions);
            }
        }

        void ConsumerOnOffsetsCommitted(object sender, CommittedOffsets commit)
        {
                Console.WriteLine($"[{string.Join(", ", commit.Offsets)}]");

                if (commit.Error)
                {
                    Console.WriteLine($"Failed to commit offsets: {commit.Error}");
                }

                Console.WriteLine($"Successfully committed offsets: [{string.Join(", ", commit.Offsets)}]");
        }

        IResponse SaveKafkaMessage(string message, string indexName, string typeName)
        {
            var indexNameEscaped = indexName.ToLower();
            indexNameEscaped = Regex.Replace(indexNameEscaped, IllegalPathCharsPattern, UnderScore);

            return elasticSearchClient.Index(message, indexNameEscaped, typeName);
        }

        void ConsumerOnConsumeError(object sender, Message message)
        {
            Console.WriteLine($"Consume Error: ({message.TopicPartitionOffset}): {message.Error}");
        }

        #endregion
    }
}
