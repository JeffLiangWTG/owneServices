using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(OrgCarrierServiceLevel.Schema.PL_Code)]
	[DescriptionProperty(OrgCarrierServiceLevel.Schema.PL_CarrierServiceLevelDescription)]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgCarrierServiceLevel : AutoOrgCarrierServiceLevel
	{
		public OrgCarrierServiceLevel(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[BusinessObjectTestExclude] // TranslatableDataField throws a max length exceeded exception even - which will indirectly fail TestBizObjectFields()
		[TranslatableDataField(Schema.TableName, Schema.PL_CarrierServiceLevelDescription, MaxLength = Schema.PL_CarrierServiceLevelDescriptionMaxLength, Type = typeof(OrgCarrierServiceLevel), Asmid = ResString.AssemblyId)]
		public override ZString PL_CarrierServiceLevelDescription
		{
			get { return base.PL_CarrierServiceLevelDescription; }
			set { base.PL_CarrierServiceLevelDescription = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Actually do not use PL_CarrierServiceLevelDescription for localization, just for some check")]
		public MultilingualString PL_CarrierServiceLevelDescriptionMultilingual
		{
			get
			{
				if (PL_Code == StandardCode && PL_CarrierServiceLevelDescription == StandardDescription.GetUnresolvedString())
				{
					return StandardDescription;
				}

				if (PL_Code == AllCode && PL_CarrierServiceLevelDescription == AllDescription.GetUnresolvedString())
				{
					return AllDescription;
				}

				return GetMultilingual(PL_CarrierServiceLevelDescriptionInfo);
			}
		}

		public ZString[] ProductCodes
		{
			get { return base.PL_ProductCode.Split(',').Select(zs => zs.Trim()).ToArray(); }
		}

		public ZString[] CarrierServiceCodes
		{
			get { return base.PL_CarrierServiceCode.Split(',').Select(zs => zs.Trim()).ToArray(); }
		}

		#region Saving

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		public override bool IsSavedByFactory
		{
			get
			{
				return base.IsSavedByFactory &&
					(
						IsDeleted ||
						HasChanges ||
						!(
							(PL_Code == StandardCode && PL_CarrierServiceLevelDescription == StandardDescription.GetUnresolvedString()) ||
							(PL_Code == AllCode && PL_CarrierServiceLevelDescription == AllDescription.GetUnresolvedString())
						 )
					);
			}
		}

		#endregion

		#region Constants

		public const string StandardCode = "STD";

		[ThreadSafe]
		public static MultilingualString StandardDescription = ResString.GetMultilingualString("efc2a667-dcd6-4a36-9f1f-e14b87c40909", "Standard");

		public const string AllCode = "ALL";

		[ThreadSafe]
		public static MultilingualString AllDescription = ResString.GetMultilingualString("b2736b31-92bc-4dc6-ba0c-9ea14e2f5f9b", "Applies to All Service Levels");

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool result = MiscServ != null && MiscServ.Header != null && !MiscServ.Header.SecurityProvider.HasModifyCarrierSecurity;
			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}
