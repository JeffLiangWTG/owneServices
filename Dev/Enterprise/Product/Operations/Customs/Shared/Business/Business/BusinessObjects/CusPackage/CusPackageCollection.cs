using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusPackageCollection : PkgPackageCollection
	{
		#region Construction

		public CusPackageCollection(CusPackageJob master)
			: base(master)
		{
		}

		public CusPackageCollection(CusPackage master)
			: base(master)
		{
		}

		#endregion

		public new CusPackage AddNew()
		{
			return (CusPackage)base.AddNew();
		}

		public new CusPackage this[int index]
		{
			get { return (CusPackage)(base[index]); }
		}

		public CodeDescriptionPairList QuickPackSeqList
		{
			get
			{
				if (quickPackSeqListCached == null)
				{
					quickPackSeqListCached = new RecalculableCachedValue<CodeDescriptionPairList>(() =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair("0", ResString.GetMultilingualString("5C0CBA26-9541-4B57-9127-F52CA51E54AA", "0 - New Pack #"));
						QuickPackSeqDictionary.ForEach(c => result.AddPair(c.Key, $"{c.Key} - {c.Value}"));
						return result;
					});
				}

				return quickPackSeqListCached.Value;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		RecalculableCachedValue<CodeDescriptionPairList> quickPackSeqListCached;

		public IDictionary<ZString, ZString> QuickPackSeqDictionary
		{
			get
			{
				if (quickPackSeqDictionaryCached == null)
				{
					CollectionCountChange += CusPackageCollection_CountChanged;
					quickPackSeqDictionaryCached = new RecalculableCachedValue<Dictionary<ZString, ZString>>(() =>
					{
						var result = new Dictionary<ZString, ZString>();
						this.ForEach(c =>
						{
							c.KP_SequenceInfo.ValueChanged -= DictionaryItemValueChanged;
							c.KP_SequenceInfo.ValueChanged += DictionaryItemValueChanged;
							c.KP_MarksAndNumbersInfo.ValueChanged -= DictionaryItemValueChanged;
							c.KP_MarksAndNumbersInfo.ValueChanged += DictionaryItemValueChanged;
							result.Add(c.KP_Sequence.ToString(), c.KP_MarksAndNumbers);
						});
						return result;
					});
				}

				return quickPackSeqDictionaryCached.Value;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		RecalculableCachedValue<Dictionary<ZString, ZString>> quickPackSeqDictionaryCached;

		void CusPackageCollection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.BizObject is CusPackage package)
			{
				if (e.ItemAdded)
				{
					package.KP_SequenceInfo.ValueChanged -= DictionaryItemValueChanged;
					package.KP_SequenceInfo.ValueChanged += DictionaryItemValueChanged;
					package.KP_MarksAndNumbersInfo.ValueChanged -= DictionaryItemValueChanged;
					package.KP_MarksAndNumbersInfo.ValueChanged += DictionaryItemValueChanged;
				}
				else if (e.ItemRemoved)
				{
					package.KP_SequenceInfo.ValueChanged -= DictionaryItemValueChanged;
					package.KP_MarksAndNumbersInfo.ValueChanged -= DictionaryItemValueChanged;
				}
			}

			InvalidateCache();
		}

		void DictionaryItemValueChanged(object sender, EventArgs e)
		{
			InvalidateCache();
		}

		void InvalidateCache()
		{
			quickPackSeqDictionaryCached?.InvalidateCache();
			quickPackSeqListCached?.InvalidateCache();
		}
	}
}
