using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public class InvalidCodeRule : ICustomAddOnRule
	{
		public Action<ZPropertyInfo> GetValidator()
		{
			return ListValidation.ErrorIfInvalidCode;
		}

		public OnSet GetOnSetBehaviour() => null;

		public IEnumerable<DynamicMetaData> GetMetaData()
		{
			if (List != null)
			{
				CodeDescriptionPairList list = List as CodeDescriptionPairList;
				if (list != null && list.Count > 0)
				{
					int maxLength = (from pair in list.OfType<CodeDescriptionPair>() select pair.Code.Length).Max();
					return new DynamicMetaData[] { DynamicMetaData.ListDataSource(List), DynamicMetaData.MaxLength(maxLength) };
				}
				else
				{
					return new DynamicMetaData[] { DynamicMetaData.ListDataSource(List) };
				}
			}
			else
			{
				return Array.Empty<DynamicMetaData>();
			}
		}

		public bool CanBeApplied(Type type)
		{
			return typeof(ZString).IsAssignableFrom(type) || typeof(CodeDescriptionPair).IsAssignableFrom(type);
		}

		public string Code => CargoWise.Workflow.CustomAddOnRuleTypes.InvalidCode;

		public string Name
		{
			get { return Res.GetString("CustomAddOnRule.InvalidCode.Name", "Code and description list"); }
		}

		[SuppressWeaklyTypedCollectionMessage]
		public IList List { get; set; }
		public bool IsEnabled { get; set; }
		public bool IsUpperCase => true;

		public BusinessObject GetObjectForBinding(BusinessObjectFactory factory)
		{
			return new CodeDescriptionCollectionWrapper(this, factory);
		}

#if DEBUG
		internal
#endif
		class CodeDescriptionCollectionWrapper : NonPersistentBusinessObject
		{
			public CodeDescriptionCollectionWrapper(InvalidCodeRule parent, BusinessObjectFactory factory)
				: base(factory)
			{
				this.parent = parent;
				if (parent.List != null)
				{
					foreach (CodeDescriptionPair pair in parent.List)
					{
						BusinessObject elem = Collection.AddNew();
						using (elem.SuspendSettingHasChanges())
						{
							elem[CodeProp] = pair.Code;
							elem[DescriptionProp] = pair.Description;
						}
					}
				}
			}

			protected override void OnFactorySavingBeforeTransactionCore()
			{
				base.OnFactorySavingBeforeTransactionCore();

				CodeDescriptionPairList list = new CodeDescriptionPairList();
				foreach (BusinessObject elem in Collection)
				{
					list.AddPair((ZString)elem[CodeProp], (ZString)elem[DescriptionProp]);
				}
				parent.List = list;
			}

			public CustomBusinessObjectCollection Collection
			{
				get
				{
					if (collection == null)
					{
						collection = new CustomBusinessObjectCollection(new CustomPropertyCollectionImpl(GetValue, TrySetValue)
						{
							{ typeof(ZString), CodeProp, ValidateCode, DynamicMetaData.MaxLength(GenCustomAddOnValue.Schema.XV_DataMaxLength) },
							{ typeof(ZString), DescriptionProp },
						});
						RegisterEditableChildObject(collection);
					}

					return collection;
				}
			}
			CustomBusinessObjectCollection collection;

			object GetValue(BusinessObject cusObj, string propertyName)
			{
				Dictionary<string, object> propValues;
				if (values.TryGetValue(cusObj.PK, out propValues))
				{
					object value;
					return propValues.TryGetValue(propertyName, out value) ? value : null;
				}
				else
				{
					return null;
				}
			}

			bool TrySetValue(BusinessObject cusObj, string propertyName, object value)
			{
				Dictionary<string, object> propValues;
				if (!values.TryGetValue(cusObj.PK, out propValues))
				{
					propValues = new Dictionary<string, object>();
					values.Add(cusObj.PK, propValues);
				}
				propValues[propertyName] = value;
				return true;
			}

			void ValidateCode(ZPropertyInfo info)
			{
				MandatoryValidation.CheckEntered(info);
				EnglishCharactersValidation.ErrorIfNotWesternEuropean(info);
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(info);
			}

			readonly InvalidCodeRule parent;
			readonly Dictionary<ZGuid, Dictionary<string, object>> values = new Dictionary<ZGuid, Dictionary<string, object>>();
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
			const string CodeProp = "Code";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
			const string DescriptionProp = "Description";
		}
	}
}
