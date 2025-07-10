using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using static System.Windows.Forms.TableLayoutSettings;

namespace System.Windows.Forms;
internal class TypeConverterTests
{
	class TableLayoutStyleConverterTest
	{
		readonly StyleConverter Converter = new StyleConverter();

		[Test]
		public void ConvertTo_AutoSizeStyle_ReturnsInstanceDescriptor()
		{
			var style1 = new ColumnStyle { SizeType = SizeType.AutoSize };
			var style2 = new RowStyle { SizeType = SizeType.AutoSize };
			var result1 = Converter.ConvertTo(null, CultureInfo.InvariantCulture, style1, typeof(InstanceDescriptor));
			var result2 = Converter.ConvertTo(null, CultureInfo.InvariantCulture, style2, typeof(InstanceDescriptor));

			Assert.That(result1, Is.InstanceOf<InstanceDescriptor>());
			var descriptor1 = (InstanceDescriptor)result1;
			Assert.That(descriptor1.MemberInfo, Is.InstanceOf<ConstructorInfo>());
			Assert.That(descriptor1.Arguments, Is.Empty);

			Assert.That(result2, Is.InstanceOf<InstanceDescriptor>());
			var descriptor2 = (InstanceDescriptor)result2;
			Assert.That(descriptor2.MemberInfo, Is.InstanceOf<ConstructorInfo>());
			Assert.That(descriptor2.Arguments, Is.Empty);
		}

		[Test]
		public void ConvertTo_AbsoluteStyle_ReturnsInstanceDescriptor()
		{
			var style1 = new ColumnStyle { SizeType = SizeType.Absolute, Size = 100 };
			var style2 = new RowStyle { SizeType = SizeType.Absolute, Size = 100 };
			var result1 = Converter.ConvertTo(null, CultureInfo.InvariantCulture, style1, typeof(InstanceDescriptor));
			var result2 = Converter.ConvertTo(null, CultureInfo.InvariantCulture, style2, typeof(InstanceDescriptor));

			Assert.That(result1, Is.InstanceOf<InstanceDescriptor>());
			var descriptor1 = (InstanceDescriptor)result1;
			Assert.That(descriptor1.MemberInfo, Is.InstanceOf<ConstructorInfo>());
			Assert.That(descriptor1.Arguments, Has.Count.EqualTo(2));
			Assert.That(descriptor1.Arguments, Has.Some.EqualTo(SizeType.Absolute));
			Assert.That(descriptor1.Arguments, Has.Some.EqualTo(100));

			Assert.That(result2, Is.InstanceOf<InstanceDescriptor>());
			var descriptor2 = (InstanceDescriptor)result2;
			Assert.That(descriptor2.MemberInfo, Is.InstanceOf<ConstructorInfo>());
			Assert.That(descriptor2.Arguments, Has.Count.EqualTo(2));
			Assert.That(descriptor2.Arguments, Has.Some.EqualTo(SizeType.Absolute));
			Assert.That(descriptor2.Arguments, Has.Some.EqualTo(100));
		}

		[Test]
		public void ConvertTo_PercentStyle_ReturnsInstanceDescriptor()
		{
			var style1 = new ColumnStyle { SizeType = SizeType.Percent, Size = 100 };
			var style2 = new RowStyle { SizeType = SizeType.Percent, Size = 100 };
			var result1 = Converter.ConvertTo(null, CultureInfo.InvariantCulture, style1, typeof(InstanceDescriptor));
			var result2 = Converter.ConvertTo(null, CultureInfo.InvariantCulture, style2, typeof(InstanceDescriptor));

			Assert.That(result1, Is.InstanceOf<InstanceDescriptor>());
			var descriptor1 = (InstanceDescriptor)result1;
			Assert.That(descriptor1.MemberInfo, Is.InstanceOf<ConstructorInfo>());
			Assert.That(descriptor1.Arguments, Has.Count.EqualTo(2));
			Assert.That(descriptor1.Arguments, Has.Some.EqualTo(SizeType.Percent));
			Assert.That(descriptor1.Arguments, Has.Some.EqualTo(100));

			Assert.That(result2, Is.InstanceOf<InstanceDescriptor>());
			var descriptor2 = (InstanceDescriptor)result2;
			Assert.That(descriptor2.MemberInfo, Is.InstanceOf<ConstructorInfo>());
			Assert.That(descriptor2.Arguments, Has.Count.EqualTo(2));
			Assert.That(descriptor2.Arguments, Has.Some.EqualTo(SizeType.Percent));
			Assert.That(descriptor2.Arguments, Has.Some.EqualTo(100));
		}

		[Test]
		public void ConvertTo_NonInstanceDescriptorType_CallsBaseMethod()
		{
			var style = new RowStyle { SizeType = SizeType.AutoSize };
			var result = Converter.ConvertTo(null, CultureInfo.InvariantCulture, style, typeof(string));

			Assert.That(result, Is.Not.Null);

			var baseConverter = new TypeConverter();
			var baseResult = baseConverter.ConvertTo(null, CultureInfo.InvariantCulture, style, typeof(string));
			Assert.That(result, Is.EqualTo(baseResult));
		}
	}

	class TableLayoutCellPositionTypeConverterTest
	{
		readonly TableLayoutPanelCellPositionTypeConverter Converter = new TableLayoutPanelCellPositionTypeConverter();

		[Test]
		public void ConvertTo_InstanceDescriptor_ReturnsCorrectDescriptor()
		{
			var cellPosition = new TableLayoutPanelCellPosition(2, 3);
			var result = Converter.ConvertTo(null, CultureInfo.InvariantCulture, cellPosition, typeof(InstanceDescriptor));

			Assert.That(result, Is.InstanceOf<InstanceDescriptor>());
			var descriptor = (InstanceDescriptor)result;
			Assert.That(descriptor.MemberInfo, Is.InstanceOf<ConstructorInfo>());

			Assert.That(descriptor.Arguments, Has.Count.EqualTo(2));
			Assert.That(descriptor.Arguments, Has.Some.EqualTo(2));
			Assert.That(descriptor.Arguments, Has.Some.EqualTo(3));
		}

		[Test]
		public void ConvertTo_NonInstanceDescriptorType_CallsBaseMethod()
		{
			var cellPosition = new TableLayoutPanelCellPosition(1, 1);
			var result = Converter.ConvertTo(null, CultureInfo.InvariantCulture, cellPosition, typeof(string));

			Assert.That(result, Is.Not.Null);
			Assert.That(result, Is.InstanceOf<string>());

			var baseConverter = new TypeConverter();
			var baseResult = baseConverter.ConvertTo(null, CultureInfo.InvariantCulture, cellPosition, typeof(string));
			Assert.That(result, Is.EqualTo(baseResult));
		}
	}

	class PaddingTypeConverterTest
	{
		readonly PaddingConverter Converter = new PaddingConverter();

		[Test]
		public void ConvertTo_SerializeAll_ReturnsInstanceDescriptor()
		{
			var paddingAll = new Padding(10);
			var result = Converter.ConvertTo(null, CultureInfo.InvariantCulture, paddingAll, typeof(InstanceDescriptor));

			Assert.That(result, Is.InstanceOf<InstanceDescriptor>());
			var descriptor = (InstanceDescriptor)result;
			Assert.That(descriptor.MemberInfo, Is.InstanceOf<ConstructorInfo>());

			Assert.That(descriptor.Arguments, Has.Count.EqualTo(1));
			Assert.That(descriptor.Arguments, Has.Some.EqualTo(10));
		}

		[Test]
		public void ConvertTo_NotSerializeAll_ReturnsInstanceDescriptor()
		{
			var padding = new Padding(10, 11, 12, 13);
			var result = Converter.ConvertTo(null, CultureInfo.InvariantCulture, padding, typeof(InstanceDescriptor));

			Assert.That(result, Is.InstanceOf<InstanceDescriptor>());
			var descriptor = (InstanceDescriptor)result;
			Assert.That(descriptor.MemberInfo, Is.InstanceOf<ConstructorInfo>());

			Assert.That(descriptor.Arguments, Has.Count.EqualTo(4));
			Assert.That(descriptor.Arguments, Has.Some.EqualTo(10));
			Assert.That(descriptor.Arguments, Has.Some.EqualTo(11));
			Assert.That(descriptor.Arguments, Has.Some.EqualTo(12));
			Assert.That(descriptor.Arguments, Has.Some.EqualTo(13));
		}

		[Test]
		public void ConvertTo_NonInstanceDescriptorType_ReturnsString()
		{
			var paddingAll = new Padding(10);
			var result = Converter.ConvertTo(null, CultureInfo.InvariantCulture, paddingAll, typeof(string));

			Assert.That(result, Is.Not.Null);
			Assert.That(result, Is.InstanceOf<string>());
			Assert.That(result, Is.EqualTo("10, 10, 10, 10"));
		}

		[Test]
		public void ConvertTo_NonInstanceDescriptorType_CallBaseMethod()
		{
			var nonPadding = "Not a padding";
			var result = Converter.ConvertTo(null, CultureInfo.InvariantCulture, nonPadding, typeof(string));

			Assert.That(result, Is.Not.Null);
			Assert.That(result, Is.EqualTo("Not a padding"));
		}
	}

	class BindingTypeConverterTest
	{
		readonly ListBindingConverter Converter = new ListBindingConverter();

		static IEnumerable<TestCaseData> BindingTestCases()
		{
			// ListBindingConverter.GetInstanceDescriptorFromValues will set the additional parameter FormattingEnabled for the ctor with the first 3 parameters
			yield return new TestCaseData(new Binding("test", null, ""), 4).SetName("Binding with 3 parameters");
			yield return new TestCaseData(new Binding("test", null, "", false), 4).SetName("Binding with 4 parameters");
			yield return new TestCaseData(new Binding("test", null, "", false, DataSourceUpdateMode.Never), 5).SetName("Binding with 5 parameters");
			yield return new TestCaseData(new Binding("test", null, "", false, DataSourceUpdateMode.Never, "null"), 6).SetName("Binding with 6 parameters");
		}

		[Test, TestCaseSource(nameof(BindingTestCases))]
		public void ConvertTo_ReturnsInstanceDescriptor(Binding binding, int expectedParameterCount)
		{
			var result = Converter.ConvertTo(null, CultureInfo.InvariantCulture, binding, typeof(InstanceDescriptor));

			Assert.That(result, Is.InstanceOf<InstanceDescriptor>());
			var descriptor = (InstanceDescriptor)result;
			Assert.That(descriptor.MemberInfo, Is.InstanceOf<ConstructorInfo>());

			var constructorInfo = (ConstructorInfo)descriptor.MemberInfo;
			Assert.That(constructorInfo.GetParameters().Length, Is.EqualTo(expectedParameterCount));
		}
	}
}
