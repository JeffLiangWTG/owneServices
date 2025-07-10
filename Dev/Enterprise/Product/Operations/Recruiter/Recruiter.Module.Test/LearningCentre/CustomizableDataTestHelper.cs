using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruiter.Module.Testing
{
	class CustomizableDataTestHelper : Disposable
	{
		public CustomizableDataTestHelper()
		{
			mockSources = ResourceStringsFactory.MockSources();
			EN = ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English);
			ZH_CN = ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.ChineseSimplified);
			FR_FR = ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.French);
			customizableDataResourceStrings = new CustomizableDataResourceStrings(new CustomizableDataCaptionSourceForTest());
			var englishNumbers = customizableDataResourceStrings.Source.GetRuntimeCaptions(null, null).ToArray();
			for (int i = 0; i < englishNumbers.Length; i++)
			{
				var key = englishNumbers[i].ResourceKey;
				EN.Put(key, new ResourceStringData(key, (ResourceString)englishNumbers[i]));
				ZH_CN.Put(key, new ResourceStringData(key, ChineseNumbers[i]));
				FR_FR.Put(key, new ResourceStringData(key, FrenchNumbers[i]));
			}
		}

		public ResourceString GetMultilingualString(string caption)
		{
			return CustomizableDataResourceStrings.GetMultilingualString(null, caption);
		}

		protected override void Dispose(bool isDisposing)
		{
			mockSources.Dispose();
		}

		public readonly string[] ChineseNumbers = new string[] { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
		public readonly string[] FrenchNumbers = new string[] { "zéro", "un", "deux", "trois", "quatre", "cinq", "six", "sept", "huit", "neuf", "dix" };
		public CustomizableDataResourceStrings CustomizableDataResourceStrings
		{
			get
			{
				return customizableDataResourceStrings;
			}
		}

		readonly CustomizableDataResourceStrings customizableDataResourceStrings;
		readonly IDisposable mockSources;
		protected IMockResourceStringCache EN;
		protected IMockResourceStringCache ZH_CN;
		protected IMockResourceStringCache FR_FR;
		class CustomizableDataCaptionSourceForTest : ICustomizableDataCaptionSource
		{
			public string Description
			{
				get
				{
					return "Customizable Data Test";
				}
			}

			public IEnumerable<IResString> GetRuntimeCaptions(IResString initialValue, object context = null)
			{
				var runtimeCaptions = new string[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" };
				var list = new List<IResString>(runtimeCaptions.Select(c => CustomizableDataResourceStrings.GetMultilingualString(this, context, c)));
				if (!string.IsNullOrEmpty(initialValue?.EnglishText) && Array.IndexOf(runtimeCaptions, initialValue.EnglishText) == -1)
				{
					list.Add(initialValue);
				}

				return list;
			}

			public string GetKey(object context, string caption)
			{
				return CustomizableDataResourceStrings.GetCustomizableDataKey("CDRSTest", caption);
			}

			public int MaxLength
			{
				get
				{
					return 50;
				}
			}

			public IEnumerable<IResString> GetCompileTimeSystemCaptions()
			{
				throw new NotImplementedException();
			}

			public ushort Asmid
			{
				get
				{
					return ResourceStringAssemblyIdAttribute.IgnoreResourceStringsAssemblyId;
				}

				set
				{
					throw new NotImplementedException();
				}
			}
		}
	}
}
