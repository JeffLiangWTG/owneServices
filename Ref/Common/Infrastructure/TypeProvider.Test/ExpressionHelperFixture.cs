using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.TypeProvider.Test
{
	[TestFixture]
	public class ExpressionHelperFixture
	{
		[Test]
		public void GetPKExpression()
		{
			var result = ExpressionHelper.GetPKExpression<Dummy>();
			var dummy = new Dummy { ZXY_PK = Guid.NewGuid() };
			Assert.AreEqual(dummy.ZXY_PK, result.Compile().Invoke(dummy));
		}

		[Test]
		public void GetPropertyExpression()
		{
			var propertyInfo = typeof(Dummy).GetProperty(nameof(Dummy.ZXY_StringProperty));
			var result = ExpressionHelper.GetPropertyExpression<Dummy, string>(propertyInfo);
			var tariff = new Dummy { ZXY_StringProperty = "001" };
			Assert.AreEqual("001", result.Compile().Invoke(tariff));
		}

		[Test]
		public void ContainsStructPropertyExpression()
		{
			var propertyInfo = typeof(Dummy).GetProperty(nameof(Dummy.ZXY_PK));
			var dummy1 = new Dummy { ZXY_PK = Guid.NewGuid() };
			var dummy2 = new Dummy { ZXY_PK = Guid.NewGuid() };
			var dummy3 = new Dummy { ZXY_PK = Guid.NewGuid() };
			var result = ExpressionHelper.ContainsStructPropertyExpression<Dummy, Guid>(new List<Guid>(new[] { dummy1.ZXY_PK, dummy2.ZXY_PK }),
				propertyInfo);
			Assert.True(result.Compile().Invoke(dummy1));
			Assert.True(result.Compile().Invoke(dummy2));
			Assert.False(result.Compile().Invoke(dummy3));
		}

		[Test]
		public void GetPropertyFiltersExpression()
		{
			var propertyInfo = typeof(Dummy).GetProperty(nameof(Dummy.ZXY_StringProperty));
			var tariff1 = new Dummy { ZXY_StringProperty = "abc" };
			var tariff2 = new Dummy { ZXY_StringProperty = "002" };
			var tariff3 = new Dummy { ZXY_StringProperty = "003" };

			var result = ExpressionHelper.GetPropertyFiltersExpression<Dummy>(propertyInfo, new[] { "ABC", "002" }, Operations.Equals).Compile();
			Assert.True(result.Invoke(tariff1));
			Assert.True(result.Invoke(tariff2));
			Assert.False(result.Invoke(tariff3));

			var resultCaseSensitive = ExpressionHelper.GetPropertyFiltersExpression<Dummy>(propertyInfo, new[] { "ABC", "002" }, Operations.Equals, StringComparison.Ordinal).Compile();
			Assert.False(resultCaseSensitive.Invoke(tariff1));
			Assert.True(resultCaseSensitive.Invoke(tariff2));
			Assert.False(resultCaseSensitive.Invoke(tariff3));
		}

		[Test]
		public void GetPropertyFiltersExpressionEqualsUsingLikeDbFunctionCaseInsensitive()
		{
			var queryableData = new List<DummyForDbContext>
			{
				new() { ZXY_StringProperty = "abc" },
				new() { ZXY_StringProperty = "002" },
				new() { ZXY_StringProperty = "003" }
			}.AsQueryable();

			var options = new DbContextOptionsBuilder<DummyContext>()
			  .UseInMemoryDatabase("TestDb")
			  .Options;

			using var context = new DummyContext(options);
			context.Dummies.AddRange(queryableData);
			context.SaveChanges();

			var propertyInfo = typeof(DummyForDbContext).GetProperty(nameof(DummyForDbContext.ZXY_StringProperty));
			var expression = ExpressionHelper.GetPropertyFiltersExpressionEqualsUsingLikeDbFunction<DummyForDbContext>(propertyInfo, "ABC");

			var result = context.Dummies.Where(expression)
				.ToList();
			Assert.AreEqual(1, result.Count);
			Assert.That(result.First().ZXY_StringProperty, Is.EqualTo("abc"));
		}

		[Test]
		public void GetPropertyFiltersExpressionEqualsUsingLikeDbFunctionEquals()
		{
			var queryableData = new List<DummyForDbContext>
			{
				new() { ZXY_StringProperty = "abc" },
				new() { ZXY_StringProperty = "002" },
				new() { ZXY_StringProperty = "003" }
			}.AsQueryable();

			var options = new DbContextOptionsBuilder<DummyContext>()
			  .UseInMemoryDatabase("TestDb2")
			  .Options;

			using var context = new DummyContext(options);
			context.Dummies.AddRange(queryableData);
			context.SaveChanges();

			var propertyInfo = typeof(DummyForDbContext).GetProperty(nameof(DummyForDbContext.ZXY_StringProperty));
			var expression = ExpressionHelper.GetPropertyFiltersExpressionEqualsUsingLikeDbFunction<DummyForDbContext>(propertyInfo, "abc");

			var result = context.Dummies.Where(expression)
				.ToList();
			Assert.AreEqual(1, result.Count);
			Assert.That(result.First().ZXY_StringProperty, Is.EqualTo("abc"));
		}

		[Test]
		public void GetPropertyFiltersExpressionEqualsUsingLikeDbFunctionNonString()
		{
			var queryableData = new List<DummyForDbContext>
			{
				new() { ZXY_StringProperty = "abc", ZXY_IntProperty = 1 },
				new() { ZXY_StringProperty = "002", ZXY_IntProperty = 2 },
				new() { ZXY_StringProperty = "003", ZXY_IntProperty = 4 }
			}.AsQueryable();

			var options = new DbContextOptionsBuilder<DummyContext>()
			  .UseInMemoryDatabase("TestDb3")
			  .Options;

			using var context = new DummyContext(options);
			context.Dummies.AddRange(queryableData);
			context.SaveChanges();

			var propertyInfo = typeof(DummyForDbContext).GetProperty(nameof(DummyForDbContext.ZXY_IntProperty));
			var expression = ExpressionHelper.GetPropertyFiltersExpressionEqualsUsingLikeDbFunction<DummyForDbContext>(propertyInfo, 4);

			var result = context.Dummies.Where(expression)
				.ToList();
			Assert.AreEqual(1, result.Count);
			Assert.That(result.First().ZXY_StringProperty, Is.EqualTo("003"));
		}

		[Test]
		public void GetPropertyFiltersExpressionStartsWith()
		{
			var propertyInfo = typeof(Dummy).GetProperty(nameof(Dummy.ZXY_StringProperty));
			var tariff1 = new Dummy { ZXY_StringProperty = "AA112" };
			var tariff2 = new Dummy { ZXY_StringProperty = "AA223" };
			var tariff3 = new Dummy { ZXY_StringProperty = "Aa115" };

			var result = ExpressionHelper.GetPropertyFiltersExpression<Dummy>(propertyInfo, new[] { "AA1" }, Operations.StartsWith).Compile();
			Assert.True(result.Invoke(tariff1));
			Assert.True(result.Invoke(tariff3));
			Assert.False(result.Invoke(tariff2));

			var resultCaseSensitive = ExpressionHelper.GetPropertyFiltersExpression<Dummy>(propertyInfo, new[] { "AA1" }, Operations.StartsWith, StringComparison.Ordinal).Compile();
			Assert.True(resultCaseSensitive.Invoke(tariff1));
			Assert.False(resultCaseSensitive.Invoke(tariff2));
			Assert.False(resultCaseSensitive.Invoke(tariff3));
		}

		[Test]
		public void GetPropertyFiltersExpressionLessThan()
		{
			var propertyInfo = typeof(Dummy).GetProperty(nameof(Dummy.ZXY_IntProperty));
			var tariff1 = new Dummy { ZXY_IntProperty = 1 };
			var tariff2 = new Dummy { ZXY_IntProperty = 2 };
			var tariff3 = new Dummy { ZXY_IntProperty = 3 };
			var result = ExpressionHelper.GetPropertyFiltersExpression<Dummy>(propertyInfo, new[] { (object)3 }, Operations.LessThan).Compile();
			Assert.True(result.Invoke(tariff1));
			Assert.True(result.Invoke(tariff2));
			Assert.False(result.Invoke(tariff3));
		}

		[Test]
		public void GetPropertyFiltersExpressionLessThanOrEqual()
		{
			var propertyInfo = typeof(Dummy).GetProperty(nameof(Dummy.ZXY_IntProperty));
			var tariff1 = new Dummy { ZXY_IntProperty = 1 };
			var tariff2 = new Dummy { ZXY_IntProperty = 2 };
			var tariff3 = new Dummy { ZXY_IntProperty = 3 };
			var result = ExpressionHelper.GetPropertyFiltersExpression<Dummy>(propertyInfo, new[] { (object)2 }, Operations.LessThanOrEqual).Compile();
			Assert.True(result.Invoke(tariff1));
			Assert.True(result.Invoke(tariff2));
			Assert.False(result.Invoke(tariff3));
		}

		[Test]
		public void GetPropertyFiltersExpressionGreaterThan()
		{
			var propertyInfo = typeof(Dummy).GetProperty(nameof(Dummy.ZXY_IntProperty));
			var tariff1 = new Dummy { ZXY_IntProperty = 1 };
			var tariff2 = new Dummy { ZXY_IntProperty = 2 };
			var tariff3 = new Dummy { ZXY_IntProperty = 3 };
			var result = ExpressionHelper.GetPropertyFiltersExpression<Dummy>(propertyInfo, new[] { (object)2 }, Operations.GreaterThan).Compile();
			Assert.True(result.Invoke(tariff3));
			Assert.False(result.Invoke(tariff2));
			Assert.False(result.Invoke(tariff1));
		}

		[Test]
		public void GetPropertyFiltersExpressionGreaterThanOrEqual()
		{
			var propertyInfo = typeof(Dummy).GetProperty(nameof(Dummy.ZXY_IntProperty));
			var tariff1 = new Dummy { ZXY_IntProperty = 1 };
			var tariff2 = new Dummy { ZXY_IntProperty = 2 };
			var tariff3 = new Dummy { ZXY_IntProperty = 3 };
			var result = ExpressionHelper.GetPropertyFiltersExpression<Dummy>(propertyInfo, new[] { (object)2 }, Operations.GreaterThanOrEqual).Compile();
			Assert.True(result.Invoke(tariff3));
			Assert.True(result.Invoke(tariff2));
			Assert.False(result.Invoke(tariff1));
		}

		[Test]
		public void GetPropertyFiltersExpressionWithParamExpression()
		{
			var paramExp = Expression.Parameter(typeof(Dummy), "x");
			var propertyInfo = typeof(Dummy).GetProperty(nameof(Dummy.ZXY_IntProperty));
			var result = ExpressionHelper.GetPropertyFiltersExpression<Dummy>(propertyInfo, new[] { (object)2 }, Operations.Equals, paramExpression: paramExp);
			Assert.AreEqual(paramExp, result.Parameters[0]);
		}

		[Test]
		public void GetRelatedEntityPropertyFilterExpression()
		{
			var relatedEntityProperty = typeof(RelatedDummy).GetProperty(nameof(RelatedDummy.ZZX_Code));
			var tariff1 = new Dummy { RefCusTariffType = new RelatedDummy { ZZX_Code = "1P1" } };
			var tariff2 = new Dummy { RefCusTariffType = new RelatedDummy { ZZX_Code = "1P2" } };
			var tariff3 = new Dummy { RefCusTariffType = new RelatedDummy { ZZX_Code = "1p1" } };

			var result = ExpressionHelper.GetRelatedEntityPropertyFilterExpression<Dummy>(typeof(RelatedDummy),
				relatedEntityProperty, "1P1").Compile();
			Assert.True(result.Invoke(tariff1));
			Assert.False(result.Invoke(tariff2));
			Assert.True(result.Invoke(tariff3));

			var resultCaseSensitive = ExpressionHelper.GetRelatedEntityPropertyFilterExpression<Dummy>(typeof(RelatedDummy),
				relatedEntityProperty, "1P1", StringComparison.Ordinal).Compile();
			Assert.True(resultCaseSensitive.Invoke(tariff1));
			Assert.False(resultCaseSensitive.Invoke(tariff2));
			Assert.False(resultCaseSensitive.Invoke(tariff3));
		}

		[Test]
		public void GetRelatedRelatedEntityPropertyFilterExpression()
		{
			var relatedEntityProperty = typeof(RelatedRelatedDummy).GetProperty(nameof(RelatedRelatedDummy.ZRR_Code));
			var dummy1 = new Dummy { RefCusTariffType = new RelatedDummy { ZZX_Code = "CD1", RelatedRelatedDummyObj = new RelatedRelatedDummy { ZRR_Code = "1P1" } } };
			var dummy2 = new Dummy { RefCusTariffType = new RelatedDummy { ZZX_Code = "CD2", RelatedRelatedDummyObj = new RelatedRelatedDummy { ZRR_Code = "1P2" } } };
			var dummy3 = new Dummy { RefCusTariffType = new RelatedDummy { ZZX_Code = "CD3", RelatedRelatedDummyObj = new RelatedRelatedDummy { ZRR_Code = "1p1" } } };
			var result = ExpressionHelper.GetRelatedRelatedEntityPropertyFilterExpression<Dummy>(typeof(RelatedDummy), typeof(RelatedRelatedDummy), relatedEntityProperty, "1P1").Compile();
			Assert.True(result.Invoke(dummy1));
			Assert.False(result.Invoke(dummy2));
			Assert.True(result.Invoke(dummy3));

			var resultCaseSensitive = ExpressionHelper.GetRelatedRelatedEntityPropertyFilterExpression<Dummy>(typeof(RelatedDummy), typeof(RelatedRelatedDummy), relatedEntityProperty, "1p1", StringComparison.Ordinal).Compile();
			Assert.False(resultCaseSensitive.Invoke(dummy1));
			Assert.False(resultCaseSensitive.Invoke(dummy2));
			Assert.True(resultCaseSensitive.Invoke(dummy3));

			relatedEntityProperty = typeof(DependentDummy).GetProperty(nameof(DependentDummy.ZYX_ZZY_StringDependency));
			var exception = Assert.Throws<ArgumentNullException>(() => ExpressionHelper.GetRelatedRelatedEntityPropertyFilterExpression<Dummy>(typeof(RelatedDummy), typeof(DependentDummy), relatedEntityProperty, "1P1"));
			Assert.AreEqual("Value cannot be null. (Parameter 'property')", exception.Message);
		}
	}
}
