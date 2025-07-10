using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.Business
{
	public enum CloneType { DeepTemplateCopy, TemplateCopy, CountryToCountryCopy, CountryToCountryCopyWithinShipment }

	public class CustomsBusinessObjectCloneStrategy : BusinessObjectCloneStrategy
	{
		public CustomsBusinessObjectCloneStrategy(BusinessObject bizObjToClone, CloneType cloneType)
			: base(bizObjToClone)
		{
			this.cloneType = cloneType;
		}

		public CustomsBusinessObjectCloneStrategy(BusinessObject bizObjToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
			: this(bizObjToClone, cloneType)
		{
			this.alternativeFactoryToInstantiateCloneIn = alternativeFactoryToInstantiateCloneIn;
		}

		protected readonly BusinessObjectFactory alternativeFactoryToInstantiateCloneIn;
		protected readonly CloneType cloneType;

		protected bool IsCountryToCountryCopy
		{
			get { return cloneType == CloneType.CountryToCountryCopy || cloneType == CloneType.CountryToCountryCopyWithinShipment; }
		}

		protected bool IsTemplateCopy
		{
			get { return cloneType == CloneType.TemplateCopy || IsDeepTemplateCopy; }
		}

		protected bool IsDeepTemplateCopy
		{
			get { return cloneType == CloneType.DeepTemplateCopy; }
		}

		public virtual BusinessObject Clone()
		{
			var args = CustomsBusinessObjectCloneArgs.GetCloneArgs(cloneType, bizObjToClone.GetType(), alternativeFactoryToInstantiateCloneIn);

			var result = Clone(args);

			if (IsTemplateCopy)
			{
				var prefix = BusinessObjectFactory.GetTableCodeFromType(result.GetType());
				var addInfoColumnName = prefix + "_AddInfo";

				if (result.ZPropertyInfoHash.ContainsKey(addInfoColumnName))
				{
					var addInfoProperty = result.ZPropertyInfoHash[addInfoColumnName];
					addInfoProperty.RefreshBinding();
					using (result.SuspendSettingHasChanges())
					{
						foreach (var dynamicValue in bizObjToClone.GetSystemDefinedValues())
						{
							if (ExcludeCopySystemDefinedValues == null || !ExcludeCopySystemDefinedValues.Contains(dynamicValue.PropertyName))
							{
								result.SetSystemDefinedValue(dynamicValue.PropertyName, dynamicValue.Value);
							}
						}

						foreach (var dynamicValue in bizObjToClone.GetUserDefinedValues())
						{
							result.SetUserDefinedValue(dynamicValue.PropertyName, dynamicValue.Value);
						}
					}
				}
				if (!(bizObjToClone is INAddInfoSupporter))
				{
					var nAddInfoColumnName = prefix + "_NAddInfo";

					if (result.ZPropertyInfoHash.ContainsKey(nAddInfoColumnName))
					{
						var nAddInfoProperty = result.ZPropertyInfoHash[nAddInfoColumnName];
						nAddInfoProperty.RefreshBinding();
					}
				}
			}
			return result;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			BusinessObject result = null;

			if (IsTemplateCopy)//TemplateCopy where BusinessObject.CloneInternal() should be called
			{
				result = base.CloneInternal(args);
			}
			else if (IsCountryToCountryCopy)//bizObjectToClone(AU).CloneInternal should not be called for NZ invoice header eg.
			{
				var factory = alternativeFactoryToInstantiateCloneIn ?? bizObjToClone.Factory;
				result = factory.New(args.TypeToCloneAs);
				result.CopyPersistentValuesFrom(bizObjToClone, args);
			}

			return result;
		}

		protected virtual HashSet<string> ExcludeCopySystemDefinedValues => null;
	}
}
