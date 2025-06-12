using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using FluentAssertions.Execution;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using RabbitMQ.Client;
using Xunit;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit.Tests.Unit
{
	public class MappingExtensions_MapFrom_Should
	{
		[Theory]
		[MemberData(nameof(NullTestValues))]
		public void GivenNullInput_ThrowNullArgumentException(IBasicProperties properties, QueueItem item, string argumentName)
		{
			properties.Invoking(p => p.MapFrom(item))
				.Should().Throw<ArgumentNullException>()
						 .WithMessage($"*{argumentName}");
		}

		public static IEnumerable<object[]> NullTestValues => new[]
		{
			new object[] { null, new QueueItem(), "properties" },
			new object[] { new TestProperties(), null, "item" }
		};

		[Fact]
		public void PopulateHeadersWithNonDefaultValuesFromQueueItemProperties()
		{
			var item = new TestQueueItem
			{
				MessageId = Guid.NewGuid(),
				Sender = "foo",
				AdditionalHeader = "bar"
			};
			var expectedHeaders = new Dictionary<string, object>
			{
				[nameof(item.MessageId)] = item.MessageId.ToByteArray(),
				[nameof(item.Sender)] = Encoding.UTF8.GetBytes(item.Sender),
				[nameof(item.AdditionalHeader)] = Encoding.UTF8.GetBytes(item.AdditionalHeader)
			};

			var headers = new TestProperties().MapFrom(item).Headers;

			headers.Should().BeEquivalentTo(expectedHeaders);
		}

		[Fact]
		public void IncludeTheMetadataDictionaryInTheResultingHeaders()
		{
			var item = new QueueItem
			{
				MessageId = Guid.NewGuid()
			};
			item.Metadata["foo"] = 1;
			item.Metadata["bar"] = 2;

			var expectedHeaders = new Dictionary<string, object>
			{
				[nameof(item.MessageId)] = item.MessageId.ToByteArray(),
				["foo"] = 1,
				["bar"] = 2
			};

			var headers = new TestProperties().MapFrom(item).Headers;

			headers.Should().BeEquivalentTo(expectedHeaders);
		}

		[Fact]
		public void MapPropertyValueInFavorOfMatchingMetadata()
		{
			var item = new QueueItem { Sender = "foo", Recipient = null };
			item.Metadata[nameof(item.Sender)] = "bar";
			item.Metadata[nameof(item.Recipient)] = "baz";

			var expectedHeaders = new Dictionary<string, object>
			{
				[nameof(item.Sender)] = Encoding.UTF8.GetBytes("foo"),
				[nameof(item.Recipient)] = Encoding.UTF8.GetBytes("baz")
			};

			var headers = new TestProperties().MapFrom(item).Headers;

			headers.Should().BeEquivalentTo(expectedHeaders);
		}

		[Fact]
		public void IgnoreQueueItemPropertiesWithDefaultValues()
		{
			var item = new TestQueueItem();

			var headers = new TestProperties().MapFrom(item).Headers;

			headers.Should().BeEmpty();
		}

		public class GivenSByteProperty : SupportedPropertyMapFromTests<sbyte>
		{
			protected override sbyte[] ValidValues => new sbyte[] { sbyte.MinValue, -1, 1, sbyte.MaxValue };
		}

		public class GivenShortProperty : SupportedPropertyMapFromTests<short>
		{
			protected override short[] ValidValues => new short[] { short.MinValue, -1, 1, short.MaxValue };
		}

		public class GivenIntProperty : SupportedPropertyMapFromTests<int>
		{
			protected override int[] ValidValues => new[] { int.MinValue, -1, 1, int.MaxValue };
		}

		public class GivenLongProperty : SupportedPropertyMapFromTests<long>
		{
			protected override long[] ValidValues => new[] { long.MinValue, -1, 1, long.MaxValue };
		}

		public class GivenFloatProperty : SupportedPropertyMapFromTests<float>
		{
			protected override float[] ValidValues => new[] { float.MinValue, -1, 1, float.MaxValue };
		}

		public class GivenDoubleProperty : SupportedPropertyMapFromTests<double>
		{
			protected override double[] ValidValues => new[] { double.MinValue, -1, 1, double.MaxValue };
		}

		public class GivenDecimalProperty : SupportedPropertyMapFromTests<decimal>
		{
			protected override decimal[] ValidValues => new[] { decimal.MinValue, -1, 1, decimal.MaxValue };
		}

		public class GivenGuidProperty : SupportedPropertyMapFromTests<Guid>
		{
			protected override Guid[] ValidValues => new[] { Guid.NewGuid(), Guid.NewGuid() };
			protected override Func<Guid, object> GetExpectedHeaderValue => g => g.ToByteArray();
		}

		public class GivenStringProperty : SupportedPropertyMapFromTests<string>
		{
			protected override string[] ValidValues => new[] { string.Empty, "short string", "a somewhat longer string", "a rather longer string that is still valid for a rabbit queue header value" };
			protected override Func<string, object> GetExpectedHeaderValue => s => Encoding.UTF8.GetBytes(s);
		}

		public class GivenCharProperty : UnsupportedPropertyMapFromTests<char> { }
		public class GivenByteProperty : UnsupportedPropertyMapFromTests<byte> { }
		public class GivenUShortProperty : UnsupportedPropertyMapFromTests<ushort> { }
		public class GivenUIntProperty : UnsupportedPropertyMapFromTests<uint> { }
		public class GivenuULongProperty : UnsupportedPropertyMapFromTests<ulong> { }
		public class GivenDateTimeProperty : UnsupportedPropertyMapFromTests<DateTime> { }
		public class GivenDateTimeOffsetProperty : UnsupportedPropertyMapFromTests<DateTimeOffset> { }

		public abstract class SupportedPropertyMapFromTests<T> : PropertyMappingTests<T>
		{
			protected abstract T[] ValidValues { get; }
			protected virtual Func<T, object> GetExpectedHeaderValue { get; } = t => t;

			[Fact]
			public void NotMapDefaultValuedProperty()
			{
				var item = new GenericQueueItem();

				var result = properties.MapFrom(item);

				result.Headers.Should().NotContainKey(nameof(GenericQueueItem.Foo));
			}

			[Fact]
			public void PreserveExistingHeaderForDefaultValuedProperty()
			{
				var item = new GenericQueueItem();
				var headerValue = ValidValues[0];
				properties.Headers = new Dictionary<string, object> { [nameof(item.Foo)] = headerValue };

				var result = properties.MapFrom(item);

				result.Headers.Should().ContainKey(nameof(GenericQueueItem.Foo)).WhichValue.Should().Be(headerValue);
			}

			[Fact]
			public void OverwriteExistingHeaderFoNonDefaultValuedProperty()
			{
				var item = new GenericQueueItem { Foo = ValidValues.Last() };
				properties.Headers = new Dictionary<string, object> { [nameof(item.Foo)] = ValidValues[0] };
				var expectedValue = GetExpectedHeaderValue(item.Foo);

				var result = properties.MapFrom(item);

				result.Headers.Should().ContainKey(nameof(GenericQueueItem.Foo)).WhichValue.Should().BeEquivalentTo(expectedValue);
			}

			[Fact]
			public void MapNonDefaultValuedProperty()
			{
				using (new AssertionScope())
				{
					foreach (var value in ValidValues)
					{
						var item = new GenericQueueItem { Foo = value };
						var expectedValue = GetExpectedHeaderValue(value);

						var result = new TestProperties().MapFrom(item);

						result.Headers.Should().ContainKey(nameof(item.Foo)).WhichValue.Should().BeEquivalentTo(expectedValue);
					}
				}
			}
		}

		public abstract class UnsupportedPropertyMapFromTests<T> : PropertyMappingTests<T>
		{
			[Fact]
			public void ThrowInvalidOperationException()
			{
				var unsupportedItem = new GenericQueueItem();
				var unsupportedItemType = unsupportedItem.GetType().Name;
				var unsupportedPropertyName = nameof(unsupportedItem.Foo);
				var expectedFailureMessage = $"the {unsupportedItemType}.{unsupportedPropertyName} property " +
											 $"of type {typeof(T)} is not supported.";

				properties.Invoking(p => p.MapFrom(unsupportedItem))
					.Should().Throw<InvalidOperationException>()
							 .WithMessage(expectedFailureMessage);
			}
		}
	}
}
