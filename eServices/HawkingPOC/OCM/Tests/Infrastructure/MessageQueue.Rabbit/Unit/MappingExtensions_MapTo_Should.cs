using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit.Tests.Unit
{

	public class MappingExtensions_MapTo_Should
	{
		[Fact]
		public void GivenNullProperties_ThrowNullArgumentException()
		{
			IBasicProperties properties = null;

			properties.Invoking(p => p.MapTo<QueueItem>())
				.Should().Throw<ArgumentNullException>()
						 .WithMessage($"*{nameof(properties)}");
		}

		[Fact]
		public void GivenNullEventArgs_ThrowNullArgumentException()
		{
			BasicDeliverEventArgs eventArgs = null;

			eventArgs.Invoking(p => p.MapTo<QueueItem>())
				.Should().Throw<ArgumentNullException>()
						 .WithMessage($"*{nameof(eventArgs)}");
		}

		[Fact]
		public void MapUnmatchedPropertiesToMetadata()
		{
			var properties = new TestProperties
			{
				Headers = new Dictionary<string, object>()
				{
					["Foo1"] = "bar1",
					["Foo2"] = 2,
					["Foo3"] = 3.0,
					["Foo4"] = Guid.NewGuid(),
					["Sender"] = "someone",
					["Recipient"] = "someone else"
				}
			};
			var expectedMetadata = properties.Headers
											 .Where(h => h.Key.StartsWith("Foo"))
											 .ToDictionary(h => h.Key, h => h.Value);

			var queueItem = properties.MapTo<QueueItem>();

			queueItem.Sender.Should().Be("someone");
			queueItem.Recipient.Should().Be("someone else");
			queueItem.Metadata.Should().BeEquivalentTo(expectedMetadata);
		}

		[Fact]
		public void MapDeliveryDetailsFromEventArgs()
		{
			var eventArgs = new BasicDeliverEventArgs
			{
				DeliveryTag = 42,
				Exchange = "foo",
				RoutingKey = "bar",
				Redelivered = true,
				BasicProperties = new TestProperties
				{
					Headers = new Dictionary<string, object>()
					{
						["Sender"] = "someone"
					}
				}
			};

			var queueItem = eventArgs.MapTo<QueueItem>();

			queueItem.Sender.Should().Be("someone");
			queueItem.Delivery.Should().BeEquivalentTo(eventArgs, opts => opts.ExcludingMissingMembers());
		}

		public class GivenSByteProperty : SupportedPropertyMapToTests<sbyte>
		{
			protected override sbyte[] ValidValues => new sbyte[] { sbyte.MinValue, -1, 1, sbyte.MaxValue };
		}

		public class GivenShortProperty : SupportedPropertyMapToTests<short>
		{
			protected override short[] ValidValues => new short[] { short.MinValue, -1, 1, short.MaxValue };
		}

		public class GivenIntProperty : SupportedPropertyMapToTests<int>
		{
			protected override int[] ValidValues => new[] { int.MinValue, -1, 1, int.MaxValue };
		}

		public class GivenLongProperty : SupportedPropertyMapToTests<long>
		{
			protected override long[] ValidValues => new[] { long.MinValue, -1, 1, long.MaxValue };
		}

		public class GivenFloatProperty : SupportedPropertyMapToTests<float>
		{
			protected override float[] ValidValues => new[] { float.MinValue, -1, 1, float.MaxValue };
		}

		public class GivenDoubleProperty : SupportedPropertyMapToTests<double>
		{
			protected override double[] ValidValues => new[] { double.MinValue, -1, 1, double.MaxValue };
		}

		public class GivenDecimalProperty : SupportedPropertyMapToTests<decimal>
		{
			protected override decimal[] ValidValues => new[] { decimal.MinValue, -1, 1, decimal.MaxValue };
		}

		public class GivenGuidProperty : SupportedPropertyMapToTests<Guid>
		{
			protected override Guid[] ValidValues => new[] { Guid.NewGuid(), Guid.NewGuid() };
		}

		public class GivenStringProperty : SupportedPropertyMapToTests<string>
		{
			protected override string[] ValidValues => new[] { string.Empty, "short string", "a somewhat longer string", "a rather longer string that is still valid for a rabbit queue header value" };
		}

		public class GivenCharProperty : UnsupportedPropertyMapToTests<char> { }
		public class GivenByteProperty : UnsupportedPropertyMapToTests<byte> { }
		public class GivenUShortProperty : UnsupportedPropertyMapToTests<ushort> { }
		public class GivenUIntProperty : UnsupportedPropertyMapToTests<uint> { }
		public class GivenuULongProperty : UnsupportedPropertyMapToTests<ulong> { }
		public class GivenDateTimeProperty : UnsupportedPropertyMapToTests<DateTime> { }
		public class GivenDateTimeOffsetProperty : UnsupportedPropertyMapToTests<DateTimeOffset> { }

		public abstract class SupportedPropertyMapToTests<T> : PropertyMappingTests<T>
		{
			protected abstract T[] ValidValues { get; }

			[Fact]
			public void MapUnmatchedHeaderToMetadata()
			{
				var headerName = "Unmatched Header";
				var headerValue = ValidValues[0];
				properties.Headers = new Dictionary<string, object> { [headerName] = headerValue };

				var queueItem = properties.MapTo<QueueItem>();

				queueItem.Metadata.Should().HaveCount(1).And.ContainKey(headerName).WhichValue.Should().Be(headerValue);
			}

			[Fact]
			public void ThrowInvalidOperationException_WhenHeaderValueIsWrongType()
			{
				properties.Headers = new Dictionary<string, object> { [nameof(GenericQueueItem.Foo)] = new IncompatibleType() };
				var expectdMessage = $"the value of type '{typeof(IncompatibleType).FullName}' in the '{nameof(GenericQueueItem.Foo)}' header " +
									 $"is not compatible with the matched property of type {typeof(T)}.";

				properties.Invoking(p => p.MapTo<GenericQueueItem>())
					.Should().Throw<InvalidOperationException>()
							 .WithMessage(expectdMessage);
			}

			[Fact]
			public void MapToMatchedProperty()
			{
				using (new AssertionScope())
				{
					foreach (var value in ValidValues)
					{
						properties.Headers = new Dictionary<string, object> { [nameof(GenericQueueItem.Foo)] = value };

						var queueItem = properties.MapTo<GenericQueueItem>();

						queueItem.Foo.Should().Be(value);
					}
				}
			}

		}

		public abstract class UnsupportedPropertyMapToTests<T> : PropertyMappingTests<T>
		{
			[Fact]
			public void ThrowInvalidOperationException_WhenMatchedHeaderExists()
			{
				properties.Headers = new Dictionary<string, object> { [nameof(GenericQueueItem.Foo)] = default(T) };
				var unsupportedItemType = typeof(GenericQueueItem).Name;
				var unsupportedPropertyName = nameof(GenericQueueItem.Foo);
				var expectedFailureMessage = $"the {unsupportedItemType}.{unsupportedPropertyName} property " +
											 $"of type {typeof(T)} is not supported.";

				properties.Invoking(p => p.MapTo<GenericQueueItem>())
					.Should().Throw<InvalidOperationException>()
							 .WithMessage(expectedFailureMessage);
			}

			[Fact]
			public void NotThrow_WhenNoHeaderMatches()
			{
				properties.Headers = new Dictionary<string, object>();

				properties.Invoking(p => p.MapTo<GenericQueueItem>())
					.Should().NotThrow();
			}
		}

		class IncompatibleType { }
	}
}
