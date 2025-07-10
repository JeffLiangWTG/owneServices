using System;
using System.Linq;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists
{
	public class CodeDescriptionEnumList<T> : CodeDescriptionPairList where T : struct
	{
		protected override int AddCore(ICodeDescription element)
		{
			if (element.GetType() != typeof(CodeDescriptionEnum<T>))
			{
				throw new InvalidOperationException("You can only add elements of type CodeDescriptionPairWithEnumValue<" + typeof(T).Name + ">.");
			}

			return base.AddCore(element);
		}

		protected void AddPair(string code, string description, T enumValue)
		{
			Add(new CodeDescriptionEnum<T>(code, description, enumValue));
		}

		public T? GetEnumValue(string code)
		{
			var element = (CodeDescriptionEnum<T>)this[code];
			return element == null ? null : element.EnumValue;
		}

		public T GetEnumValueOrDefault(string code, T defaultValue)
		{
			var element = (CodeDescriptionEnum<T>)this[code];
			return element == null ? defaultValue : element.EnumValue;
		}

		#region Testing
#if DEBUG

		public string[] GetEnumValuesInListForTesting()
		{
			return (from CodeDescriptionEnum<T> element in this select element.EnumValue.ToString()).ToArray();
		}

		public Type GetEnumTypeForTesting()
		{
			return typeof(T);
		}

#endif
		#endregion
	}

	public class CodeDescriptionEnum<T> : CodeDescriptionPair
	{
		public CodeDescriptionEnum(string code, string description, T enumValue)
			: base(code, description)
		{
			EnumValue = enumValue;
		}
		public readonly T EnumValue;
	}
}
