using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Argument.Test
{
	[TestFixture]
	public class ArgumentFixture
	{
		[Test]
		public void NotNullOrEmptyTest()
		{
			Assert.Throws<ArgumentNullException>(() => Argument.NotNullOrEmpty(null, "name"));
			Assert.DoesNotThrow(() => Argument.NotNullOrEmpty("value", "name"));
		}

		[Test]
		public void NotNullTest()
		{
			string testVariable = null;
			Assert.Throws<ArgumentNullException>(() => Argument.NotNull(testVariable, "name"));
			testVariable = "value";
			Assert.DoesNotThrow(() => Argument.NotNull(testVariable, "name"));
		}

		[Test]
		public void GreaterThanOrEqualTest()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => Argument.GreaterThanOrEqual(1, 2, "name"));
			Assert.DoesNotThrow(() => Argument.GreaterThanOrEqual(2, 2, "name"));
			Assert.DoesNotThrow(() => Argument.GreaterThanOrEqual(3, 2, "name"));
		}

		[Test]
		public void GreaterThanTest()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => Argument.GreaterThan(1, 2, "name"));
			Assert.Throws<ArgumentOutOfRangeException>(() => Argument.GreaterThan(2, 2, "name"));
			Assert.DoesNotThrow(() => Argument.GreaterThan(3, 2, "name"));
		}

		[Test]
		public void GreaterThanZeroTest()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => Argument.GreaterThanZero(0, "name"));
			Assert.Throws<ArgumentOutOfRangeException>(() => Argument.GreaterThanZero(-1, "name"));
			Assert.DoesNotThrow(() => Argument.GreaterThanZero(1, "name"));
		}

		[Test]
		public void GreaterThanOrEqualToZeroTest()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => Argument.GreaterThanOrEqualToZero(-1, "name"));
			Assert.DoesNotThrow(() => Argument.GreaterThanOrEqualToZero(1, "name"));
			Assert.DoesNotThrow(() => Argument.GreaterThanOrEqualToZero(0, "name"));
		}

		[Test]
		public void IsTrueTest()
		{
			Assert.Throws<ArgumentException>(() => Argument.IsTrue(false, "name"));
			Assert.DoesNotThrow(() => Argument.IsTrue(true, "name"));
		}

		[Test]
		public void IsFalseTest()
		{
			Assert.Throws<ArgumentException>(() => Argument.IsFalse(true, "name"));
			Assert.DoesNotThrow(() => Argument.IsFalse(false, "name"));
		}

		[Test]
		public void GuidIsNotEmptyTest()
		{
			Assert.Throws<ArgumentException>(() => Argument.GuidIsNotEmpty(Guid.Empty, "name"));
			Assert.DoesNotThrow(() => Argument.GuidIsNotEmpty(Guid.NewGuid(), "name"));
		}

		[Test]
		public void InRangeWithBoundIncludedTest()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => Argument.InRangeWithBoundIncluded(5, 1, 4, "name"));
			Assert.Throws<ArgumentOutOfRangeException>(() => Argument.InRangeWithBoundIncluded(5, 6, 8, "name"));
			Assert.DoesNotThrow(() => Argument.InRangeWithBoundIncluded(2, 1, 3, "name"));
		}

		[Test]
		public void InRangeWithBoundExcludedTest()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => Argument.InRangeWithBoundExcluded(5, 1, 5, "name"));
			Assert.Throws<ArgumentOutOfRangeException>(() => Argument.InRangeWithBoundExcluded(6, 6, 8, "name"));
			Assert.DoesNotThrow(() => Argument.InRangeWithBoundExcluded(2, 1, 3, "name"));
		}

		[Test]
		public void ExactlyEqualTo()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => Argument.ExactlyEqualTo(2, 2, "name"));
			Assert.DoesNotThrow(() => Argument.ExactlyEqualTo(3, 2, "name"));
		}
	}
}
