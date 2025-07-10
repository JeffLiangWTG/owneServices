using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class OrgCusCodeDeduplicationProvider : IDeduplicationBizoProvider
	{
		public Type BusinessObjectType => typeof(OrgCusCode);

		public Type GlowType => typeof(IOrgCusCode);

		public DeduplicationDisplayMode DisplayModeForType => DeduplicationDisplayMode.List;

		public string GroupNameForType => DeduplicationProvider.Constants.RegistrationCodes;

		public string TablePrefix => OrgCusCodeSchema.Constants.Prefix;

		public IDeduplicationGlowObject GetChildObject(IDeduplicationMaster headerParent, Guid childObjectPK)
		{
			var orgHeader = headerParent as DeduplicationOrgHeader;
			return (orgHeader?.CusCodes.FirstOrDefault(x => x.OK_PK == childObjectPK)) as DeduplicationOrgCusCode;
		}

		public IDeduplicationGlowObject GetComparisonSource(IDeduplicationGlowObject master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false)
		{
			return GetComparisonSource(new List<IDeduplicationGlowObject> { master }, pk, columnNames, standardizeDomains);
		}

		public IDeduplicationGlowObject GetComparisonSource(IEnumerable<IDeduplicationGlowObject> master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false)
		{
			return master.Cast<DeduplicationOrgHeader>().SelectMany(x => x.CusCodes).FirstOrDefault(y => y.OK_PK == pk) as DeduplicationOrgCusCode;
		}

		public string GetHeading(IDeduplicationGlowObject source)
		{
			var cusCode = (DeduplicationOrgCusCode)source;

			return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", cusCode.OK_RN_NKCodeCountry, cusCode.OK_CodeType, cusCode.OK_CustomsRegNo);
		}

		public string GetHeading(object master, Guid pk, HeaderType headerType = HeaderType.Short)
		{
			var targets = master as IEnumerable<IOrgHeader>;
			var cusCode = targets.SelectMany(x => x.CusCodes).FirstOrDefault(y => y.OK_PK == pk);
			return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", cusCode?.OK_RN_NKCodeCountry, cusCode?.OK_CodeType, cusCode?.OK_CustomsRegNo);
		}
	}
}
