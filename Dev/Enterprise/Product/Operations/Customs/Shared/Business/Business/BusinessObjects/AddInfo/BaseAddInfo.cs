using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.Business
{
	public static class AddInfoSupporter
	{
		public static BaseAddInfo GetBaseAddInfoWithNAddInfoSupport(this BusinessObject bizObj) => bizObj is INAddInfoSupporter && bizObj is IAddInfoManager addInfoManager ? addInfoManager.AddInfo as BaseAddInfo : null;
	}

	public interface IAddInfoManager
	{
		IAddInfo AddInfo { get; }
		BusinessObjectFactory Factory { get; }
	}

	public interface IAddInfoManagerWithSchema : IAddInfoManager
	{
		ITableSchema AddInfoSchema { get; }
	}

	public interface INAddInfoSupporter
	{
		ZPropertyInfoString NAddInfoProperty { get; }
	}

	public interface IAddInfo : Integration.Customs.IAddInfoBase
	{
		IDictionary<ZString, AddInfoPropertyNameAndValueParser> GetKeys();
		bool IsUpdateRelatedPropertyInfoDisabled { get; set; }
		void UpdateAddInfoFromString(ZString addInfoString);
		void SetValue(string propertyName, ZString value);
		ZString GetKey(string propertyName);
		IZType GetEffectiveValue(string propertyName);
		void UpdateRelatedPropertyInfo();
	}

	public struct AddInfoPropertyNameAndValueParser
	{
		public string PropertyName;
		public AddInfoValueParser Parser;
	}

	public delegate ZString AddInfoValueParser(IXmlImportLogger logger, ZString value);

	[UniversalCopyWithExtendedEntities]
	public abstract class BaseAddInfo : BusinessObject, ICharacterSetValidationSupport, IAddInfo, ILightValidationInternals, IAddInfoWithSyncProperty
	{
		protected BaseAddInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			// Don't call base - Non persistent object
		}
#endif

		public override bool IsInDatabase
		{
			get { return AddInfoProperty != null && AddInfoProperty.BizObj.IsInDatabase; }
		}

		public override bool IsSavedByFactory
		{
			get { return false; }
		}

		protected override bool IsCopying
		{
			get { return base.IsCopying || isDeserialising; }
		}
		bool isDeserialising;

		public const char Separator = AddInfoParser.Separator;
		public const char SpecialCharRepresentingStar = AddInfoParser.SpecialCharRepresentingStar;
		public const char CodeValueSeparator = AddInfoParser.CodeValueSeparator;
		public const char CodeInfoSeparator = AddInfoParser.CodeInfoSeparator;

		public bool EnableConcurrencyResolver;

		protected bool isInProcessOfSaving;
		protected override void OnFactorySaving()
		{
			isInProcessOfSaving = true;
			if (isInitialised && (HasChanges || !Parent.IsInDatabase))
			{
				UpdateRelatedPropertyInfo();
			}
			base.OnFactorySaving();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			isInProcessOfSaving = false;
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				dbAddInfo = null;
			}
		}

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;
				if (isInitialised && base.HasChanges && isInProcessOfSaving)
				{
					UpdateRelatedPropertyInfo();
				}
			}
		}

		protected override void InvalidateCachedProperties()
		{
			if (Factory != null && isInitialised)
			{
				Factory.InvalidateCachedProperties();
			}
		}

		//Tests that would fail without suspending are in US.ValidationModesCalculator
		public void UpdateRelatedPropertyInfo()
		{
			if (AddInfoProperty != null && !IsUpdateRelatedPropertyInfoDisabled)
			{
				if (!settingAddInfoProperty)
				{
					settingAddInfoProperty = true;
					IDisposable lightValidationSuspender = Parent.LightValidationEnabled ? Parent.SuspendMarkingAsNeedingValidation() : null;

					try
					{
						AddInfoProperty.Value = (ZString)this.ToString();

						GenAddOnColumnSynchroniser.Synchronise(GetColumnsForFastSearch());
					}
					finally
					{
						settingAddInfoProperty = false;
						if (lightValidationSuspender != null)
						{
							lightValidationSuspender.Dispose();
						}
					}
				}
			}
		}

		public bool HasChangesSinceLastSaving(SchemaColumn column)
		{
			bool result = (Parent.IsInDatabase && HasChanges && AddInfoProperty != null)
				&& !this[column].Equals(DbAddInfo[column]);
			return result;
		}

		public object GetOriginalValue(SchemaColumn column)
		{
			var result = (Parent.IsInDatabase && AddInfoProperty != null)
				? DbAddInfo[column]
				: null;
			return result;
		}

		protected BaseAddInfo DbAddInfo
		{
			get
			{
				if (dbAddInfo == null)
				{
					dbAddInfo = (BaseAddInfo)Activator.CreateInstance(GetType(), AddInfoProperty);
					dbAddInfo.LoadPropertiesFromString((ZString)AddInfoProperty.OriginalValue);
					dbAddInfo.SuspendValidation();
					dbAddInfo.IsUpdateRelatedPropertyInfoDisabled = true;
					dbAddInfo.Parent?.UnRegisterEditableChildObject(dbAddInfo);
				}
				return dbAddInfo;
			}
		}
		BaseAddInfo dbAddInfo;

		protected virtual SchemaColumn[] ColumnsForFastSearch
		{
			get { return Array.Empty<SchemaColumn>(); }
		}

		AddInfoGenAddOnColumnSynchroniser GenAddOnColumnSynchroniser
		{
			get { return synchroniser ?? (synchroniser = new AddInfoGenAddOnColumnSynchroniser(AddInfoProperty.BizObj, this)); }
		}
		AddInfoGenAddOnColumnSynchroniser synchroniser;

		public override string ToString()
		{
			return Serialise(GetValidPropertyInfos());
		}

		IEnumerable<ZPropertyInfo> GetValidPropertyInfos()
		{
			IEnumerable<ZPropertyInfo> result = null;
			if (UseWrappedPropertiesOnly() is BusinessObject bizObj)
			{
				// include only actual wrapped properties
				var key = (GetType(), bizObj.GetType());
				var wrappedInfoList = wrappedInfos.GetOrAdd(key, _ => GetWrappedInfoList(bizObj.ZPropertyInfoHash));
				result = ZPropertyInfoHash.OfType<ZPropertyInfo>().Where(x => wrappedInfoList.Contains(x.Name));
			}
			return result ?? new TypedEnumerable<ZPropertyInfo>(ZPropertyInfoHash);
		}

		protected virtual BusinessObject UseWrappedPropertiesOnly()
		{
			return AddInfoProperty?.BizObj is BusinessObject bizObj && bizObj is IAddInfoManagerWithSchema ? bizObj : null;
		}

		HashSet<string> GetWrappedInfoList(ZPropertyInfoHashtable bizObjInfos)
		{
			var tablePrefix = TablePrefix;
			var result = new HashSet<string>();
			foreach (var innerInfo in bizObjInfos.GetPropertyInfos(PropertyInfoTypes.Wrapping).Cast<ZWrappedPropertyInfo>().Select(x => x.InnerInfo).Where(i => i.HasSetter))
			{
				var name = innerInfo.Name;
				if (name.Substring(0, 3) == tablePrefix && ZPropertyInfoHash.ContainsKey(name))
				{
					result.Add(name);
				}
			}

			return result;
		}

		[ThreadSafe]
		static readonly ConcurrentDictionary<(Type, Type), HashSet<string>> wrappedInfos = new ConcurrentDictionary<(Type, Type), HashSet<string>>();

		public static string GetStringRepresentation(IZType value) => value.GetStringRepresentation();

		public void LoadPropertiesFromString(ZString addInfoString)
		{
			LoadPropertiesFromString(addInfoString, true);
		}

		public void LoadPropertiesFromString(ZString addInfoString, bool clearExisting)
		{
			if (!IsDeleted)
			{
				SuspendValidation();
				isDeserialising = true;

				IDisposable markAsNeedingValidationSuspender = Parent.LightValidationEnabled ? Parent.SuspendMarkingAsNeedingValidation() : null;

				try
				{
					isInitialised = false;

					if (clearExisting || !addInfoString.IsEmpty)
					{
						var addInfoHash = AddInfoParser.CreateDictionaryWithAddInfoString(addInfoString);

						foreach (SchemaColumn schemaColumn in ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(TableName).All)
						{
							if (!schemaColumn.IsPKColumn)
							{
								var propertyName = schemaColumn.Name;
								var keyToAddInfoHash = GetDBName(propertyName);

								if (addInfoHash.ContainsKey(keyToAddInfoHash))
								{
									var value = addInfoHash[keyToAddInfoHash];
									if (!LoadCodeInfoToCollection(value, propertyName))
									{
										AssignValueToPropertySimple(ZPropertyInfoHash[propertyName], value);
									}
								}
								else if (clearExisting)
								{
									AssignDefaultValueToPropertySimple(ZPropertyInfoHash[propertyName]);
									LoadCodeInfoToCollection(string.Empty, propertyName);
								}
							}
						}
					}

					isInitialised = true;
				}
				finally
				{
					isDeserialising = false;
					ResumeValidation();
					markAsNeedingValidationSuspender?.Dispose();
				}
			}
		}

#if DEBUG
		public
#endif
		bool IsValidInfo(ZPropertyInfo info)
		{
			var key = (GetType(), info.Name);
			return isValidInfos.GetOrAdd(key, _ => GetIsValidInfoNoCache(info));
		}

		static readonly ConcurrentDictionary<(Type, string), bool> isValidInfos = new ConcurrentDictionary<(Type, string), bool>();

		bool GetIsValidInfoNoCache(ZPropertyInfo info)
		{
			return info.HasSetter && info.Name.Substring(0, 3) == TablePrefix && ObjectFactory.Get<IApplicationSchemaResolver>().SchemaColumnExists(info.Name, TableName);
		}

		protected virtual string GetDBName(string propertyName)
		{
			return propertyName.Substring(3);
		}

		public override bool IsDeleted
		{
			get { return Parent.IsDeleted; }
		}

		public static ZString AddinfoWithNonProductItemsRemoved(ZString customsAddinfo, string[] itemsThatMayBeAddedToProduct, char seperationCharacter)
		{
			ZString fCustomsAddinfo = customsAddinfo;
			if (!fCustomsAddinfo.IsEmpty && itemsThatMayBeAddedToProduct.Length > 0)
			{
				IList<string> itemsThatMayBeAddedToProductList = itemsThatMayBeAddedToProduct;
				ZString[] addInfoItems = fCustomsAddinfo.Split(seperationCharacter);
				foreach (string addInfoItem in addInfoItems)
				{
					string[] keyValuePair = addInfoItem.Split('=');
					if (keyValuePair.Length == 2)
					{
						if (!itemsThatMayBeAddedToProductList.Contains(keyValuePair[0]))
						{
							fCustomsAddinfo = fCustomsAddinfo.Replace(keyValuePair[0] + "=" + keyValuePair[1], "");
						}
					}
				}
			}
			return fCustomsAddinfo;
		}

		#region ICharacterSetValidationSupport Members

		Action<ZPropertyInfo> ICharacterSetValidationSupport.ValidateCharacterSet
		{
			get
			{
				return new Action<ZPropertyInfo>(info =>
				{
					if (!info.IsNAddInfoField())
					{
						EnglishCharactersValidation.ErrorIfNotWesternEuropean(info);
					}
				});
			}
		}

		#endregion

		#region Implementation

		string Serialise(IEnumerable<ZPropertyInfo> propertyInfos)
		{
			StringBuilder result = new StringBuilder();
			foreach ((string addInfoName, (ZPropertyInfo info, IZType value)) in GetValidInfosIncludingSyncInfos(propertyInfos))
			{
				if (!value.IsDefault)
				{
					string stringRepresentation = GetStringRepresentation(value);
					if (stringRepresentation.Length > 0)
					{
						result.Append(AddInfoParser.Serialise(addInfoName, stringRepresentation));
					}
				}
			}
			return result.ToString().Trim(Separator);
		}

		IEnumerable<(string genAddOnColumnName, IZType addInfoValue)> GetColumnsForFastSearch()
		{
			var columnsForFastSearch = ColumnsForFastSearch.ToDictionary(x => x.Name, y => (IZType)this[y]);
			foreach (var pair in columnsForFastSearch)
			{
				yield return (pair.Key, pair.Value);
			}
			var syncAddInfos = GetSyncAddInfos();
			if (syncAddInfos != null)
			{
				foreach ((string addInfoName, Type addInfoValueType, ZPropertyInfo info, string fastSearchName) in syncAddInfos)
				{
					if (fastSearchName != null && !columnsForFastSearch.ContainsKey(fastSearchName))
					{
						yield return (fastSearchName, GetAddInfoValue(info.Value, addInfoValueType));
					}
				}
			}
		}

		protected virtual IZType GetAddInfoValue(IZType data, Type addInfoValueType)
		{
			return (IZType)Activator.CreateInstance(addInfoValueType, data);
		}

		(string addInfoName, Type addInfoValueType, ZPropertyInfo info, string fastSearchName)[] GetSyncAddInfos() => (AddInfoProperty?.BizObj as IAddInfoWithSyncPropertySupporter).GetSyncAddInfos();

		IEnumerable<(string addInfoName, (ZPropertyInfo info, IZType value))> GetValidInfosIncludingSyncInfos(IEnumerable<ZPropertyInfo> propertyInfos)
		{
			var validAddInfos = GetValidInfos(propertyInfos);
			var syncAddInfos = GetSyncAddInfos();
			if (syncAddInfos != null && syncAddInfos.Length > 0)
			{
				validAddInfos = GetCombineValidInfosAndSyncInfos(validAddInfos.ToDictionary(x => x.addInfoName, s => (s.Item2.info, s.Item2.value)), syncAddInfos);
			}
			return validAddInfos;
		}

		IEnumerable<(string addInfoName, (ZPropertyInfo info, IZType value))> GetCombineValidInfosAndSyncInfos(Dictionary<string, (ZPropertyInfo info, IZType value)> validAddInfos, (string addInfoName, Type addInfoValueType, ZPropertyInfo info, string fastSearchName)[] syncAddInfos)
		{
			foreach ((string addInfoName, Type addInfoValueType, ZPropertyInfo info, string fastSearchName) in syncAddInfos)
			{
				try
				{
					validAddInfos.Add(addInfoName, (info, GetAddInfoValue(info.Value, addInfoValueType)));
				}
				catch (ArgumentException)
				{
					// We will get an ArgumentException when an element with the same key already exists dictionary.
				}
			}
			return validAddInfos.Select(x => (x.Key, x.Value));
		}

		IEnumerable<(string addInfoName, (ZPropertyInfo info, IZType value))> GetValidInfos(IEnumerable<ZPropertyInfo> propertyInfos) => propertyInfos.Where(p => IsValidInfo(p)).Select(x => (GetDBName(x.Name), (x, x.Value)));

		protected void SetupEventsAndLoadValues(ZPropertyInfo addInfoProperty)
		{
			Parent = addInfoProperty.BizObj;
			AddInfoProperty = addInfoProperty;
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				LoadPropertiesFromString((ZString)addInfoProperty.Value, isInitialised);
			}
			isInitialised = true;
		}

		protected virtual bool LoadCodeInfoToCollection(string addInfoString, string propertyName)
		{
			return false;
		}

		string fTablePrefix;
		public override string TablePrefix
		{
			get
			{
				if (fTablePrefix == null)
				{
					fTablePrefix = ((INeedTable)this).Table.Columns[0].ColumnName.Substring(0, 3);
				}
				return fTablePrefix;
			}
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			BaseAddInfo result = (BaseAddInfo)Activator.CreateInstance(GetType(), new object[] { Parent });
			using (result.GetValidationSuspender())
			{
				result.CopyPersistentValuesFrom(this, args);
			}
			return result;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected bool isInitialised;
		protected bool settingAddInfoProperty;

		protected ZPropertyInfo AddInfoProperty
		{
			get { return addInfoProperty; }
			set
			{
				if (addInfoProperty != null)
				{
					addInfoProperty.ValueChanged -= new EventHandler(AddInfoProperty_ValueChanged);

					var nAddInfoProperty = (addInfoProperty.BizObj as INAddInfoSupporter)?.NAddInfoProperty;
					if (nAddInfoProperty != null && nAddInfoProperty != addInfoProperty)
					{
						nAddInfoProperty.ValueChanged -= new EventHandler(AddInfoProperty_ValueChanged);
					}
				}

				addInfoProperty = value;

				if (addInfoProperty != null)
				{
					addInfoProperty.ValueChanged += new EventHandler(AddInfoProperty_ValueChanged);

					var nAddInfoProperty = (addInfoProperty.BizObj as INAddInfoSupporter)?.NAddInfoProperty;
					if (nAddInfoProperty != null && nAddInfoProperty != addInfoProperty)
					{
						nAddInfoProperty.ValueChanged += new EventHandler(AddInfoProperty_ValueChanged);
					}
				}
			}
		}

		ZPropertyInfo addInfoProperty;

		BusinessObject Integration.Customs.IAddInfoBase.Parent
		{
			get { return Parent; }
		}

		public BusinessObject Parent
		{
			get { return parent; }
			protected set
			{
				if (parent != null)
				{
					parent.UnRegisterEditableChildObject(this);
					parent.BeforeUpdatedByDataRefresh -= OnParentWasBeforeUpdatedByDataRefresh;
					parent.UpdatedByDataRefresh -= OnParentWasUpdatedByDataRefresh;
					parent.Reloaded -= new EventHandler(OnParentReloaded);
				}
				parent = value;
				if (parent != null)
				{
					if (!(parent is IAddInfoManager))
					{
						throw new InvalidOperationException(parent.GetType().FullName + " needs to implement 'IAddInfoManager' as it's required by UniversalShipment Xml");
					}

					parent.RegisterEditableChildObject(this);
					parent.BeforeUpdatedByDataRefresh += OnParentWasBeforeUpdatedByDataRefresh;
					parent.UpdatedByDataRefresh += OnParentWasUpdatedByDataRefresh;
					parent.Reloaded += new EventHandler(OnParentReloaded);
				}
			}
		}

		BusinessObject parent;

		protected virtual void AssignValueToPropertySimple(ZPropertyInfo propertyInfo, object inputValue)
		{
			if (inputValue != null)
			{
				var zValue = ConvertToZType(propertyInfo.Value.GetType(), inputValue);
				if (zValue is ZString)
				{
					var zStringValue = ((ZString)zValue).TrimEndSpaceTab();
					zValue = zStringValue;
				}
				SetPropertyValue(propertyInfo, zValue);
			}
		}

		protected void AssignValueToPropertyInfoHash(ZPropertyInfo propertyInfo, object value)
		{
			if (value != null)
			{
				string propertyName = propertyInfo.Name;
				var zValue = ConvertToZType(propertyInfo.Value.GetType(), value);
				if (zValue is ZString)
				{
					this[propertyName] = ((ZString)zValue).Left(propertyInfo.MaxLength);
				}
				else
				{
					this[propertyName] = zValue;
				}
			}
		}

		public static IZType ConvertToZType(Type zType, object value1) => AddInfoParser.ConvertToZType(zType, value1);

		protected virtual void AssignDefaultValueToPropertySimple(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.Value.IsDefault)
			{
				SetPropertyValue(propertyInfo, propertyInfo.Value.Default);
			}
		}

		protected void AssignDefaultValueToPropertyInfoHash(ZPropertyInfo propertyInfo)
		{
			if (!IsDeleted)
			{
				string propertyName = propertyInfo.Name;
				if (!propertyInfo.Value.Default.Equals(this[propertyName]))
				{
					this[propertyName] = propertyInfo.Value.Default;
				}
			}
		}

		protected virtual void LoadPropertiesFromAddInfoProperty(bool clearExisting)
		{
			if (!IsDeleted && addInfoProperty != null)
			{
				using (GetValidationSuspender())
				using (SuspendSettingHasChanges())
				{
					LoadPropertiesFromString((ZString)addInfoProperty.Value, clearExisting);
				}
			}
		}

		void AddInfoProperty_ValueChanged(object sender, EventArgs e)
		{
			var args = e as ConcurrencyValueChangedEventArgs;
			var changedByConcurrencyResolver = args != null;
			if (!settingAddInfoProperty && !changedByConcurrencyResolver)
			{
				LoadPropertiesFromAddInfoProperty(true);
			}
			else if (changedByConcurrencyResolver && EnableConcurrencyResolver)
			{
				LoadDataFromConcurrencyResolver(args.Info, args.OriginalValue.ToString(), args.LastModified);
			}
		}

		const string CurrentDBAddInfoFactoryKey = "CurrentDBAddInfoFactoryKey";
		const string PreviousDBAddInfoFactoryKey = "PreviousDBAddInfoFactoryKey";

		void LoadDataFromConcurrencyResolver(ZPropertyInfo changedAddInfoProperty, ZString originalAddInfo, string lastModified)
		{
			if (changedAddInfoProperty != null)
			{
				var concurrenyResolverSupporter = changedAddInfoProperty.BizObj as IAddInfoWithConcurrencyResolverSupporter;
				if (concurrenyResolverSupporter == null)
				{
					ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "{0} was created with EnableConcurrencyResolver set to true but it's parent ({1}) hasn't implement IAddInfoWithConcurrencyResolverSupporter.", GetType().FullName, changedAddInfoProperty.BizObj.GetType().FullName));
				}
				else
				{
					var isNAddInfoProperty = changedAddInfoProperty == (changedAddInfoProperty.BizObj as INAddInfoSupporter)?.NAddInfoProperty;
					var currentAddInfo = this;//this contains values a user has modified since a form is opened
					var currentDBAddInfo = concurrenyResolverSupporter.NewAddInfoBizObj(GetDummAddInfoProperty(CurrentDBAddInfoFactoryKey, (ZString)changedAddInfoProperty.Value));//XX_AddInfo is set to a db value by concurrency resolver and this represents values saved by another user.
					var previousDBAddInfo = concurrenyResolverSupporter.NewAddInfoBizObj(GetDummAddInfoProperty(PreviousDBAddInfoFactoryKey, originalAddInfo));
					try
					{
						var notificationBuilder = new ZStringBuilder();
						var propertyInfoHash = isNAddInfoProperty
							? currentAddInfo.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(x => x.IsNAddInfoField())
							: currentAddInfo.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(x => !x.IsNAddInfoField());

						foreach (var info in propertyInfoHash)
						{
							var previousDBValue = previousDBAddInfo[info.Name];
							var currentValue = info.Value;
							var databaseValue = currentDBAddInfo[info.Name];
							//This field has been changed by another user since the current user has opened the form
							if (!databaseValue.Equals(previousDBValue) && !databaseValue.Equals(currentValue))
							{
								var zValue = ConvertToZType(info.Value.GetType(), databaseValue);
								currentAddInfo[info.Name] = zValue;
								var warning = Res.GetString("ffc41d33-ff58-45f7-a2e4-2c88d77e9ac1", "Another user ({0}) has changed this field.\r\nYours: '{1}', Theirs: '{2}'",
									lastModified,
									currentValue.ToString() ?? "",
									databaseValue ?? "");

								info.AddWarningWithoutValidationCheck(warning);

								notificationBuilder.Append(Res.GetString("99aca605-1225-4958-9087-dde704020b1c", "{0}: Yours: '{1}', Theirs: '{2}'", info.HumanReadableName, currentValue, databaseValue));
							}
						}
						if (!notificationBuilder.IsEmpty)
						{
							concurrenyResolverSupporter.NotificationAddInfo.AddWarningWithoutValidationCheck(Res.GetString("b6d9e458-7378-4e98-9651-da4ee4bbeb27", "Another user ({0}) has changed these fields.\r\n{1}", lastModified, notificationBuilder.ToStringWithNewLineBetweenAppends()));
						}
					}
					finally
					{
						currentDBAddInfo.Parent.UnRegisterEditableChildObject(currentDBAddInfo);
						previousDBAddInfo.Parent.UnRegisterEditableChildObject(previousDBAddInfo);
					}
				}
			}
		}

		ZPropertyInfo GetDummAddInfoProperty(string dbAddInfoFactoryKey, ZString addInfoValue)
		{
			var dummyFactory = Factory.GetCachedValue(dbAddInfoFactoryKey, () =>
			{
				var result = new BusinessObjectFactory();
				result.SuspendValidation();
				result.RefreshEnabled = false;
				return result;
			});
			var bizObj = dummyFactory.GetNull(AddInfoProperty.BizObj.GetType());
			var propertyName = AddInfoProperty.Name;
			bizObj[propertyName] = addInfoValue;
			return bizObj.ZPropertyInfoHash[propertyName];
		}

		void OnParentWasBeforeUpdatedByDataRefresh(object sender, EventArgs e)
		{
			addInfo = AddInfoProperty.Value;
		}
		IZType addInfo;

		void OnParentWasUpdatedByDataRefresh(object sender, EventArgs e)
		{
			if (!AddInfoProperty.Value.Equals(addInfo))
			{
				LoadPropertiesFromAddInfoProperty(true);
			}
			addInfo = null;
		}

		void OnParentReloaded(object sender, EventArgs e)
		{
			LoadPropertiesFromAddInfoProperty(true);
		}

		#endregion

		#region IAddInfo Members

		ZString IAddInfo.GetKey(string propertyName)
		{
			return GetDBName(propertyName);
		}

		IDictionary<ZString, AddInfoPropertyNameAndValueParser> IAddInfo.GetKeys()
		{
			var typeDictionary = Factory.GetCachedValue<IDictionary<Type, IDictionary<ZString, AddInfoPropertyNameAndValueParser>>>("BaseAddInfo.GetKeys", () => new Dictionary<Type, IDictionary<ZString, AddInfoPropertyNameAndValueParser>>());
			var currentType = GetType();
			if (!typeDictionary.TryGetValue(currentType, out var dictionary))
			{
				dictionary = GetKeys(ZPropertyInfoHash, GetDBName, IsValidInfo);
				typeDictionary.Add(currentType, dictionary);
			}
			return dictionary;
		}

		public static IDictionary<ZString, AddInfoPropertyNameAndValueParser> GetKeys(ZPropertyInfoHashtable zPropertyInfoHash, Func<string, string> getDBName, Func<ZPropertyInfo, bool> isValidInfo)
		{
			var result = new Dictionary<ZString, AddInfoPropertyNameAndValueParser>();
			foreach (ZPropertyInfo propertyInfo in zPropertyInfoHash)
			{
				if (isValidInfo(propertyInfo))
				{
					var propertyName = propertyInfo.Name;
					var key = getDBName(propertyName);
					try
					{
						result.Add(key, new AddInfoPropertyNameAndValueParser() { PropertyName = propertyName, Parser = GetAddInfoValueFunction(key, propertyInfo) });
					}
					catch (ArgumentException ex)
					{
						throw new ArgumentException($"Trying to add key '{key}' for property '{propertyName}'.", ex);
					}
				}
			}
			return result;
		}

		void IAddInfo.SetValue(string propertyName, ZString value)
		{
			var info = ZPropertyInfoHash.GetPropertySafe(propertyName);
			if (info != null)
			{
				if (!info.IsNAddInfoField())
				{
					value = value.ConvertToWesternEuropeanCharacters();
				}

				AssignValueToPropertyInfoHash(info, value);
			}
		}

		public static AddInfoValueParser GetAddInfoValueFunction(string key, ZPropertyInfo info)
		{
			if (info is ZPropertyInfoString stringInfo)
			{
				var maxLength = info.MaxLength;
				return new AddInfoValueParser((logger, stringValue) =>
				{
					var result = stringValue.TrimEnd(' ');
					if (maxLength > 0 && result.Length > maxLength)
					{
						logger.Log(LogType.Warning, GetMaximumLengthTruncateMessage(key, maxLength, result));
						result = result.Left(maxLength);
					}
					return result;
				});
			}

			return null;
		}

		public static string GetMaximumLengthTruncateMessage(string key, int maxLength, string value)
		{
			return Res.GetString("C495B799-EB3C-4109-8CEA-CC2D2A4ABC06", "The maximum length of Add Info '{0}' is {1} characters, but '{2}' was specified; value has been truncated.", key, maxLength, value);
		}

		public bool IsUpdateRelatedPropertyInfoDisabled { get; set; }

		void IAddInfo.UpdateAddInfoFromString(ZString addInfoString)
		{
			LoadPropertiesFromString(addInfoString);
			HasChanges = true;
		}

		IZType IAddInfo.GetEffectiveValue(string propertyName)
		{
			IZType result = null;
			var info = ZPropertyInfoHash.GetPropertySafe(propertyName);
			if (info != null && IsValidInfo(info))
			{
				ZWrappedPropertyInfo parentInfo = null;
				if (AddInfoProperty != null)
				{
					var parent = AddInfoProperty.BizObj;
					parentInfo = (parent.ZPropertyInfoHash.GetPropertySafe(propertyName) ?? parent.ZPropertyInfoHash.GetPropertySafe(AddInfoProperty.Name.Substring(0, 3) + propertyName.Substring(3))) as ZWrappedPropertyInfo;
					if (parentInfo != null)
					{
						var innerInfo = parentInfo.InnerInfo;
						if (Object.ReferenceEquals(innerInfo.BizObj, this) && innerInfo.Name == propertyName)
						{
							info = parentInfo;
						}
					}
				}
				result = info.Value;
			}
			return result;
		}

		#endregion

		#region ILightValidationInternals Members

		ILightValidationInternals CusAddInfoParent
		{
			get
			{
				if (fCusAddInfoParent == null && Parent is CusAddInfo)
				{
					fCusAddInfoParent = Parent as ILightValidationInternals;
				}
				return fCusAddInfoParent;
			}
		}
		ILightValidationInternals fCusAddInfoParent;

		SchemaBoolColumn ILightValidationInternals.IsValidSchemaColumn
		{
			get
			{
				return CusAddInfoParent != null ? CusAddInfoParent.IsValidSchemaColumn : null;
			}
		}

		ZBool ILightValidationInternals.IsValid
		{
			get
			{
				return CusAddInfoParent != null && CusAddInfoParent.IsValid;
			}

			set
			{
				if (CusAddInfoParent != null)
				{
					CusAddInfoParent.IsValid = value;
				}
			}
		}

		bool ILightValidationInternals.IsValidHasChanges
		{
			get
			{
				return CusAddInfoParent != null && CusAddInfoParent.IsValidHasChanges;
			}
		}

		#endregion

		ZPropertyInfo IAddInfoWithSyncProperty.AddInfoProperty => AddInfoProperty;
		IZType IAddInfoWithSyncProperty.GetAddInfoValue(IZType data, Type addInfoValueType) => GetAddInfoValue(data, addInfoValueType);
		void IAddInfoWithSyncProperty.EnableSynchronization()
		{
			if (!hasEnabledSynchronization)
			{
				hasEnabledSynchronization = true;
				var syncAddInfos = GetSyncAddInfos();
				if (syncAddInfos != null)
				{
					foreach ((string addInfoName, Type addInfoValueType, ZPropertyInfo info, string fastSearchName) in syncAddInfos)
					{
						info.ValueChanged -= Info_ValueChanged;
						info.ValueChanged += Info_ValueChanged;
					}
				}
			}
		}
		bool hasEnabledSynchronization;

		void Info_ValueChanged(object sender, EventArgs e)
		{
			if (e is ValueChangedEventArgs va && !va.OldValue.Equals(va.NewValue))
			{
				HasChanges = true;
			}
		}

		#region SplitAddInfoString

		public Tuple<ZString, ZString> SplitAddInfoString(ZString concatAddInfoString)
		{
			var addInfoZStringBuilder = new ZStringBuilder();
			var nAddInfoZStringBuilder = new ZStringBuilder();
			var addInfoHash = AddInfoParser.CreateDictionaryWithAddInfoString(concatAddInfoString);

			foreach ((string addInfoName, (ZPropertyInfo info, IZType value)) in GetValidInfosIncludingSyncInfos(ZPropertyInfoHash.Cast<ZPropertyInfo>()))
			{
				if (addInfoHash.ContainsKey(addInfoName))
				{
					var keyAndValue = AddInfoParser.Serialise(addInfoName, addInfoHash[addInfoName]);
					if (info.IsNAddInfoField())
					{
						nAddInfoZStringBuilder.Append(keyAndValue);
					}
					else
					{
						addInfoZStringBuilder.Append(keyAndValue);
					}
				}
			}

			return new Tuple<ZString, ZString>(addInfoZStringBuilder.ToString().Trim(Separator), nAddInfoZStringBuilder.ToString().Trim(Separator));
		}

		#endregion
	}
}
