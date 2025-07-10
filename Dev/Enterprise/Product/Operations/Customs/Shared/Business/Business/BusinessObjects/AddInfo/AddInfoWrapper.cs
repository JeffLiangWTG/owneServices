using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class AddInfoWrapper<T> : IAddInfo
		where T : BusinessObject, IAddInfoManagerWithSchema
	{
		public AddInfoWrapper(T businessObject,
			string addInfoPropertyName = null,
			Func<IDictionary<string, IAddInfoPropertyData>> getAddInfoNamesMapping = null,
			string nAddInfoPropertyName = null,
			Func<IDictionary<string, IAddInfoPropertyData>> getNAddInfoNamesMapping = null)
		{
			this.businessObject = businessObject;
			this.tableSchema = businessObject.AddInfoSchema;

			if (addInfoPropertyName != null)
			{
				this.addInfoProperty = Argument.NotNull((ZPropertyInfoString)businessObject.ZPropertyInfoHash.GetPropertySafe(addInfoPropertyName), nameof(addInfoPropertyName));
				addInfoProperty.ValueChanged += AddInfoInfo_ValueChanged;
				this.getAddInfoNamesMapping = Argument.NotNull(getAddInfoNamesMapping, nameof(getAddInfoNamesMapping));
			}

			if (nAddInfoPropertyName != null)
			{
				this.nAddInfoProperty = Argument.NotNull((ZPropertyInfoString)businessObject.ZPropertyInfoHash.GetPropertySafe(nAddInfoPropertyName), nameof(nAddInfoPropertyName));
				nAddInfoProperty.ValueChanged += NAddInfoInfo_ValueChanged;
				this.getNAddInfoNamesMapping = Argument.NotNull(getNAddInfoNamesMapping, nameof(getNAddInfoNamesMapping));
			}
			businessObject.Reloaded += BusinessObject_Reloaded;
			if (businessObject is INAddInfoSupporter)
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "{0} has implement INAddInfoSupporter, it should not be used in conjuction with {1} as INAddInfoSupporter will cause _AddInfo and _NAddInfo to be combine; see AddInfoParser.ConcatAddInfoStrings usage.", businessObject.GetType().FullName, this.GetType().Name));
			}
		}
		readonly T businessObject;
		readonly ITableSchema tableSchema;

		readonly ZPropertyInfoString addInfoProperty;
		protected readonly Func<IDictionary<string, IAddInfoPropertyData>> getAddInfoNamesMapping;

		readonly ZPropertyInfoString nAddInfoProperty;
		protected readonly Func<IDictionary<string, IAddInfoPropertyData>> getNAddInfoNamesMapping;

		void BusinessObject_Reloaded(object sender, EventArgs e)
		{
			if (addInfoProperty != null)
			{
				AddInfoInfo_ValueChanged(sender, e);
			}
			if (nAddInfoProperty != null)
			{
				NAddInfoInfo_ValueChanged(sender, e);
			}
		}

		void AddInfoInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!settingAddInfoProperty)
			{
				Deserialise(addInfoProperty, getAddInfoNamesMapping);
			}
		}

		void NAddInfoInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!settingAddInfoProperty)
			{
				Deserialise(nAddInfoProperty, getNAddInfoNamesMapping);
			}
		}

		#region IAddInfo Members

		ZString IAddInfo.GetKey(string propertyName) => GetDBName(propertyName);

		IDictionary<ZString, AddInfoPropertyNameAndValueParser> IAddInfo.GetKeys() => keys ?? (keys = BaseAddInfo.GetKeys(businessObject.ZPropertyInfoHash, GetDBName, IsValidInfo));
		IDictionary<ZString, AddInfoPropertyNameAndValueParser> keys;

		bool IsValidInfo(ZPropertyInfo propertyInfo) => tableSchema.GetSchemaColumn(propertyInfo.Name) != null;

		string GetDBName(string propertyName) => propertyName.Substring(businessObject.TablePrefix.Length + 1);

		void IAddInfo.SetValue(string propertyName, ZString value)
		{
			var info = businessObject.ZPropertyInfoHash.GetPropertySafe(propertyName);
			if (info != null)
			{
				var zValue = BaseAddInfo.ConvertToZType(info.Value.GetType(), value);
				if (zValue is ZString)
				{
					businessObject[propertyName] = ((ZString)zValue).Left(info.MaxLength);
				}
				else
				{
					businessObject[propertyName] = zValue;
				}
			}
		}

		public bool IsUpdateRelatedPropertyInfoDisabled { get; set; }

		void IAddInfo.UpdateAddInfoFromString(ZString addInfoString)
		{
			if (!businessObject.IsDeleted)
			{
				using (businessObject.GetValidationSuspender())
				{
					if (addInfoProperty != null && nAddInfoProperty != null)
					{
						(var addInfos, var nAddInfos) = SplitAddInfoString(addInfoString);
						UpdateAddInfoFromString(addInfos, addInfoProperty, getAddInfoNamesMapping);
						UpdateAddInfoFromString(nAddInfos, nAddInfoProperty, getNAddInfoNamesMapping);
					}
					else if (addInfoProperty != null)
					{
						UpdateAddInfoFromString(addInfoString, addInfoProperty, getAddInfoNamesMapping);
					}
					else if (nAddInfoProperty != null)
					{
						UpdateAddInfoFromString(addInfoString, nAddInfoProperty, getNAddInfoNamesMapping);
					}
				}
			}

			void UpdateAddInfoFromString(ZString addInfoString, ZPropertyInfoString property, Func<IDictionary<string, IAddInfoPropertyData>> getAddInfoNamesMapping)
			{
				if (!addInfoString.IsEmpty && property.Value == addInfoString)
				{
					Deserialise(property, getAddInfoNamesMapping);
				}
				else
				{
					property.Value = addInfoString;
				}
			}
		}

		IZType IAddInfo.GetEffectiveValue(string propertyName)
		{
			IZType result = null;
			if (tableSchema.GetSchemaColumn(propertyName) != null)
			{
				var info = businessObject.ZPropertyInfoHash.GetPropertySafe(propertyName);
				if (info != null)
				{
					result = info.Value;
				}
			}
			return result;
		}

		public void UpdateRelatedPropertyInfo()
		{
			if (!IsUpdateRelatedPropertyInfoDisabled)
			{
				if (!settingAddInfoProperty)
				{
					settingAddInfoProperty = true;
					using (businessObject.LightValidationEnabled ? businessObject.SuspendMarkingAsNeedingValidation() : null)
					{
						try
						{
							// Include SyncAddInfo properties to support transition of AddInfo property to real property
							if (addInfoProperty != null)
							{
								addInfoProperty.Value = Serialise(getAddInfoNamesMapping, SyncAddInfoList);
							}
							if (nAddInfoProperty != null)
							{
								nAddInfoProperty.Value = Serialise(getNAddInfoNamesMapping, SyncNAddInfoList);
							}
						}
						finally
						{
							settingAddInfoProperty = false;
						}
					}
				}
			}
		}
		bool settingAddInfoProperty;

		protected virtual ZString Serialise(Func<IDictionary<string, IAddInfoPropertyData>> getAddInfoNamesMapping, IEnumerable<KeyValuePair<ZString, ZPropertyInfo>> syncAddInfoList)
			=> AddInfoParser.Serialise(getAddInfoNamesMapping(),
									syncAddInfoList.Where(additionalPair => !additionalPair.Value.Value.IsDefault)
									.Select(additionalPair => new KeyValuePair<ZString, ZString>(additionalPair.Key, additionalPair.Value.Value.GetStringRepresentation())));

		protected virtual void Deserialise(ZPropertyInfoString addInfoPropertyInfo, Func<IDictionary<string, IAddInfoPropertyData>> getAddInfoNamesMapping)
		{
			AddInfoParser.Deserialise(addInfoPropertyInfo.Value, getAddInfoNamesMapping(), true);
		}

		protected IEnumerable<KeyValuePair<ZString, ZPropertyInfo>> SyncAddInfoList
		{
			get
			{
				LoadSyncAddInfoDetails();
				return syncAddInfoList;
			}
		}
		IEnumerable<KeyValuePair<ZString, ZPropertyInfo>> syncAddInfoList;

		protected IEnumerable<KeyValuePair<ZString, ZPropertyInfo>> SyncNAddInfoList
		{
			get
			{
				LoadSyncAddInfoDetails();
				return syncNAddInfoList;
			}
		}
		IEnumerable<KeyValuePair<ZString, ZPropertyInfo>> syncNAddInfoList;

		void LoadSyncAddInfoDetails()
		{
			if (!hasLoadedSyncAddInfoDetails)
			{
				hasLoadedSyncAddInfoDetails = true;
				var syncAddInfos = (businessObject as IAddInfoWithSyncPropertySupporter)?.GetSyncAddInfos();
				if (syncAddInfos != null)
				{
					var addInfoList = new List<KeyValuePair<ZString, ZPropertyInfo>>();
					var nAddInfoList = new List<KeyValuePair<ZString, ZPropertyInfo>>();
					syncAddInfos.ForEach(syncAddInfo =>
					{
						if (syncAddInfo.info.IsNAddInfoField())
						{
							nAddInfoList.Add(new KeyValuePair<ZString, ZPropertyInfo>(syncAddInfo.addInfoName, syncAddInfo.info));
						}
						else
						{
							addInfoList.Add(new KeyValuePair<ZString, ZPropertyInfo>(syncAddInfo.addInfoName, syncAddInfo.info));
						}
					});
					syncAddInfoList = addInfoList;
					syncNAddInfoList = nAddInfoList;
				}
				else
				{
					syncAddInfoList = Enumerable.Empty<KeyValuePair<ZString, ZPropertyInfo>>();
					syncNAddInfoList = Enumerable.Empty<KeyValuePair<ZString, ZPropertyInfo>>();
				}
			}
		}
		bool hasLoadedSyncAddInfoDetails;

		BusinessObject Integration.Customs.IAddInfoBase.Parent => businessObject;

		#endregion

		(ZString AddInfos, ZString NAddInfos) SplitAddInfoString(ZString combinedAddInfos)
		{
			var nAddInfoSet = getNAddInfoNamesMapping().Keys.ToHashSet();

			var addInfoHash = AddInfoParser.CreateDictionaryWithAddInfoString(combinedAddInfos);

			var addInfoStr = AddInfoParser.Serialise(addInfoHash.Where(x => nAddInfoSet.Count == 0 || !nAddInfoSet.Contains(x.Key)));
			var nAddInfoStr = AddInfoParser.Serialise(addInfoHash.Where(x => nAddInfoSet.Contains(x.Key)));

			return (addInfoStr, nAddInfoStr);
		}
	}
}
