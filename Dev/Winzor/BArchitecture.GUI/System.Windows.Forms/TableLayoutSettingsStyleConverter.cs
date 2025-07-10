// This file is primarily based on the source implementation from System.Windows.Forms
// Some minor changes have been made to implement caching of the ConstructorInfo instances to improve the performance.

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace System.Windows.Forms;

public sealed partial class TableLayoutSettings
{
	internal class StyleConverter : TypeConverter
	{
		static readonly Lazy<Dictionary<Type, ConstructorInfo>> _autoSizeConstructorCache = new Lazy<Dictionary<Type, ConstructorInfo>>(() =>
			new Dictionary<Type, ConstructorInfo>());
		static readonly Lazy<Dictionary<Type, ConstructorInfo>> _sizeTypeConstructorCache = new Lazy<Dictionary<Type, ConstructorInfo>>(() =>
			new Dictionary<Type, ConstructorInfo>());

		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
		{
			if (destinationType == typeof(InstanceDescriptor))
			{
				return true;
			}

			return base.CanConvertTo(context, destinationType);
		}

		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (destinationType == typeof(InstanceDescriptor) && value is TableLayoutStyle style)
			{
				ConstructorInfo? constructor = null;
				object[]? constructorArgs = null;
				Type type = style.GetType();

				switch (style.SizeType)
				{
					case SizeType.AutoSize:
						if (!_autoSizeConstructorCache.Value.TryGetValue(type, out constructor))
						{
							constructor = type.GetConstructor(Array.Empty<Type>());
							if (constructor != null)
							{
								_autoSizeConstructorCache.Value[type] = constructor;
							}
						}
						constructorArgs = Array.Empty<object>();
						break;
					case SizeType.Absolute:
					case SizeType.Percent:
						if (!_sizeTypeConstructorCache.Value.TryGetValue(type, out constructor))
						{
							constructor = type.GetConstructor(new Type[] { typeof(SizeType), typeof(int) });
							if (constructor != null)
							{
								_sizeTypeConstructorCache.Value[type] = constructor;
							}
						}
						constructorArgs = new object[] { style.SizeType, style.Size };
						break;
					default:
						break;
				}
				if (constructor != null)
				{
					return new InstanceDescriptor(constructor, constructorArgs);
				}
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
