using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgBrandOrRelatedName : AutoOrgBrandOrRelatedName, IOrgBrandOrRelatedName, IMatchingBrandOrRelatedName
	{
		public OrgBrandOrRelatedName(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		public override ZString P1_RelatedName
		{
			get
			{
				return base.P1_RelatedName;
			}
			set
			{
				base.P1_RelatedName = value;
				if (Header != null)
				{
					Header.PatternMatchRequiresRegen = true;
					Header.FindDuplicates();
				}
			}
		}

		public override void Delete()
		{
			Header?.FindDuplicates();
			base.Delete();
		}

		#endregion

		#region Logging

		protected override ZString CustomLogReferenceSuffix
		{
			get { return Res.GetString("2396e375-8604-45c2-95b6-ee3a727e7ef4", "Related Name: {0}", P1_RelatedName) + (P1_RelatedNameInfo.HasChanges ? "(" + P1_RelatedNameInfo.OriginalValue + ")" : ""); }
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return (Header != null && !Header.SecurityProvider.HasModifyConfigBrandsAndCompanyNamesSecurity) || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region IOrgBrandOrRelatedNameForMatching

		ZString IMatchingBrandOrRelatedName.P1_RelatedNameOriginal
		{
			get
			{
				return P1_RelatedNameInfo.OriginalValue.ToString();
			}
		}

		#endregion

		#region Saving

		public override void OnSaving()
		{
			base.OnSaving();
			InvalidateScreeningStatuses();
		}

		#endregion

		#region OnSavingForDelete

		protected override void OnSavingForDelete()
		{
			InvalidateScreeningStatuses(true);
			base.OnSavingForDelete();
		}

		void InvalidateScreeningStatuses(bool isBeingDeleted = false)
		{
			var header = isBeingDeleted
				? Factory.Load<OrgHeader>((ZGuid)P1_OHInfo.OriginalValue)
				: Header;
			if (header != null && !header.IsDeleted && !header.IsBeingDeleted)
			{
				if (!IsInDatabase || P1_RelatedNameInfo.HasChanges || isBeingDeleted)
				{
					header.InvalidateScreeningStatuses();
				}
			}
		}

		#endregion
	}
}
