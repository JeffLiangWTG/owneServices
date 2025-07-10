using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class AddInfoWrapperWithBaseAddInfoParent<T> : AddInfoWrapper<T>
		where T : BusinessObject, IAddInfoManagerWithSchema
	{
		public AddInfoWrapperWithBaseAddInfoParent(T businessObject,
			BaseAddInfo baseAddInfo,
			string addInfoPropertyName = null,
			Func<IDictionary<string, IAddInfoPropertyData>> getAddInfoNamesMapping = null,
			string nAddInfoPropertyName = null,
			Func<IDictionary<string, IAddInfoPropertyData>> getNAddInfoNamesMapping = null)
			: base(businessObject, addInfoPropertyName, getAddInfoNamesMapping, nAddInfoPropertyName, getNAddInfoNamesMapping)
		{
			this.baseAddInfo = baseAddInfo;
		}

		readonly BaseAddInfo baseAddInfo;

		protected override void Deserialise(ZPropertyInfoString addInfoPropertyInfo, Func<IDictionary<string, IAddInfoPropertyData>> getAddInfoNamesMapping)
		{
			base.Deserialise(addInfoPropertyInfo, getAddInfoNamesMapping);
			if (baseAddInfo is IAddInfoWithSyncProperty addInfo && addInfoPropertyInfo.Equals(addInfo.AddInfoProperty))
			{
				this.baseAddInfo.LoadPropertiesFromString(addInfoPropertyInfo.Value);
			}
		}

		protected override ZString Serialise(Func<IDictionary<string, IAddInfoPropertyData>> getAddInfoNamesMapping, IEnumerable<KeyValuePair<ZString, ZPropertyInfo>> syncAddInfoList)
		{
			var baseAddInfo = this.baseAddInfo.ToString();
			baseAddInfo += baseAddInfo.Length > 0 ? AddInfoParser.Separator : string.Empty;
			return baseAddInfo + AddInfoParser.Serialise(getAddInfoNamesMapping(),
									syncAddInfoList.Where(additionalPair => !additionalPair.Value.Value.IsDefault)
									.Select(additionalPair => new KeyValuePair<ZString, ZString>(additionalPair.Key, additionalPair.Value.Value.GetStringRepresentation())));
		}
	}
}
