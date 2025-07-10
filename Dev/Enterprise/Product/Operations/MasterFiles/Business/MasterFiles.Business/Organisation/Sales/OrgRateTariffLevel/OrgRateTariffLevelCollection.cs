using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Business;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRateTariffLevelCollection : BusinessObjectCollection<OrgRateTariffLevel>, IBusinessObjectFilterFactory
	{
		public OrgRateTariffLevelCollection(OrgHeader orgHeader, ZGuid companyPK) : base(orgHeader.Factory)
		{
			Argument.NotNull(orgHeader, nameof(orgHeader));

			this.orgHeader = orgHeader;
			this.companyPK = companyPK;
			IsManagedForDataRefresh = true;
		}

		static bool IsTariffTypeOrDefault(OrgRateTariffLevel level, string tariffType) => level.P7_TariffType == tariffType || level.P7_TariffType == OrgRateTariffLevel.DefaultTariffType;

		static bool IsDirectionOrDefault(OrgRateTariffLevel level, string direction) => level.P7_Direction == direction || level.P7_Direction == nameof(OrgRateTariffLevel.Directions.ALL);

		static bool IsModeOrGeneralizedModeOrDefault(OrgRateTariffLevel level, string rateMode, string generalizedMode) => level.P7_Mode == rateMode || level.P7_Mode == generalizedMode || level.P7_Mode == Core.Constants.RateMode.ALL;

		public static bool IsOverlappingDateRange(OrgRateTariffLevel level, ZDate startDate, ZDate endDate)
		{
			return (endDate.IsEmpty || level.P7_StartDate.IsEmpty || endDate >= level.P7_StartDate) && (startDate.IsEmpty || level.P7_ExpiryDate.IsEmpty || startDate <= level.P7_ExpiryDate);
		}

		public IEnumerable<OrgRateTariffLevel> GetRelevantLevels(string tariffType, string direction, string rateMode, string generalizedMode, ZDate startDate = default, ZDate endDate = default)
		{
			IEnumerable<OrgRateTariffLevel> levels = this.Cast<OrgRateTariffLevel>();
			return levels.Where(level => IsTariffTypeOrDefault(level, tariffType) && IsDirectionOrDefault(level, direction) && IsOverlappingDateRange(level, startDate, endDate) && IsModeOrGeneralizedModeOrDefault(level, rateMode, generalizedMode));
		}

#if DEBUG

		public OrgRateTariffLevel SetLevel(string tariffType, int value)
		{
			return SetLevel(tariffType, nameof(OrgRateTariffLevel.Directions.ALL), "ALL", ZDate.Empty, ZDate.Empty, value);
		}

		public OrgRateTariffLevel SetLevel(string tariffType, string direction, string mode, int value)
		{
			return SetLevel(tariffType, direction, mode, ZDate.Empty, ZDate.Empty, value);
		}

		public OrgRateTariffLevel SetLevel(string tariffType, string direction, string mode, ZDate start, ZDate expiry, int value)
		{
			foreach (OrgRateTariffLevel tariffLevel in this)
			{
				if (tariffLevel.P7_TariffType == tariffType && tariffLevel.P7_Direction == direction && tariffLevel.P7_Mode == mode)
				{
					tariffLevel.P7_ExpiryDate = expiry;
					tariffLevel.P7_StartDate = start;
					tariffLevel.P7_TariffLevel = (byte)value;
					return tariffLevel;
				}
			}

			OrgRateTariffLevel newTariffLevel = AddNew();
			newTariffLevel.P7_TariffType = tariffType;
			newTariffLevel.P7_Mode = mode;
			newTariffLevel.P7_Direction = direction;
			newTariffLevel.P7_TariffLevel = (byte)value;
			newTariffLevel.P7_StartDate = start;
			newTariffLevel.P7_ExpiryDate = expiry;
			return newTariffLevel;
		}

		public OrgRateTariffLevel AddLevel(string tariffType, string direction, string mode, ZDate start, ZDate expiry, int value)
		{
			OrgRateTariffLevel newTariffLevel = AddNew();
			newTariffLevel.P7_TariffType = tariffType;
			newTariffLevel.P7_Mode = mode;
			newTariffLevel.P7_Direction = direction;
			newTariffLevel.P7_TariffLevel = (byte)value;
			newTariffLevel.P7_StartDate = start;
			newTariffLevel.P7_ExpiryDate = expiry;
			return newTariffLevel;
		}

#endif

		public int DefaultLevel => DefaultOrgRateTariffLevel?.P7_TariffLevel ?? Env.Registry.GlobalTariffDefault;

		OrgRateTariffLevel DefaultOrgRateTariffLevel
		{
			get { return this.Cast<OrgRateTariffLevel>().FirstOrDefault(tariffLevel => tariffLevel.IsDefault); }
		}

		readonly OrgHeader orgHeader;
		readonly ZGuid companyPK;

		#region LoadRelevant

		public void LoadRelevant()
		{
			Load();
			Sort(new OrgRateTariffLevelComparer(CompanyTariffTypes));
		}

		CodeDescriptionPairList CompanyTariffTypes
		{
			get { return CachedOrgCodeLists.CompanyTariffTypes_List(Factory, companyPK); }
		}

		OrgCodeLists CachedOrgCodeLists
		{
			get { return cachedOrgCodeLists ?? (cachedOrgCodeLists = new OrgCodeLists()); }
		}

		OrgCodeLists cachedOrgCodeLists;

		class OrgRateTariffLevelComparer : IComparer
		{
			public OrgRateTariffLevelComparer(CodeDescriptionPairList companyTariffTypes)
			{
				CompanyTariffTypes = companyTariffTypes;
			}

			int IComparer.Compare(object x, object y)
			{
				OrgRateTariffLevel x1 = (OrgRateTariffLevel)x;
				OrgRateTariffLevel y1 = (OrgRateTariffLevel)y;
				int index1 = CompanyTariffTypes.IndexOfCode(x1.P7_TariffType);
				int index2 = CompanyTariffTypes.IndexOfCode(y1.P7_TariffType);

				return index1.CompareTo(index2);
			}

			readonly CodeDescriptionPairList CompanyTariffTypes;
		}

		#endregion

		#region Implementation

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(OrgRateTariffLevelSchema.P7_OH, orgHeader.PK);
			filter.AddToFilter(OrgRateTariffLevelSchema.P7_GC, companyPK);

			return filter;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgRateTariffLevel tariffLevel = (OrgRateTariffLevel)child;
			tariffLevel.P7_OH = orgHeader.PK;
			tariffLevel.P7_GC = companyPK;
		}

		protected override bool AllowSort
		{
			get { return true; }
		}

		#endregion

		#region IBusinessObjectFilterFactory Members

		class OrgRateTariffLevelFilter : BusinessObjectFilter
		{
			readonly bool orgHeaderIsDeleted;
			readonly ZGuid orgHeaderPK;
			readonly ZGuid companyPK;

			public OrgRateTariffLevelFilter(bool orgHeaderIsDeleted, ZGuid orgHeaderPK, ZGuid companyPK)
			{
				this.orgHeaderIsDeleted = orgHeaderIsDeleted;
				this.orgHeaderPK = orgHeaderPK;
				this.companyPK = companyPK;
			}

			protected override bool IsMatching(BusinessObject bizObject)
			{
				OrgRateTariffLevel tariffLevel = bizObject as OrgRateTariffLevel;
				return tariffLevel != null && !orgHeaderIsDeleted && tariffLevel.P7_OH == orgHeaderPK && tariffLevel.P7_GC == companyPK;
			}
		}

		IBusinessObjectFilter IBusinessObjectFilterFactory.NewBusinessObjectFilter()
		{
			return new OrgRateTariffLevelFilter(orgHeader.IsDeleted, orgHeader.PK, companyPK);
		}

		#endregion
	}
}
