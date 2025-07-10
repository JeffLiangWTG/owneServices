using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class BranchManagementCodeDescriptionBool : CodeDescriptionBool, ICanDelete
	{
		public BranchManagementCodeDescriptionBool()
		{
		}

		public BranchManagementCodeDescriptionBool(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BranchManagementCodeDescriptionBool(fallbackLevel);
		}

		[ReadOnlyMember(nameof(IsReadOnly))]
		public override ZString Code
		{
			get { return base.Code; }
			set { base.Code = value; }
		}

		[ReadOnlyMember(nameof(IsReadOnly))]
		public override ZBool Bool
		{
			get { return base.Bool; }
			set { base.Bool = value; }
		}

		public bool IsReadOnly
		{
			get
			{
				var result = false;

				ZGuid companyPK = CurrentFallbackLevel != null ? CurrentFallbackLevel.CompanyPK(true) : ZGuid.Empty;
				if (!Code.IsEmpty)
				{
					if (companyPK.IsValid)
					{
						result = BranchManagementCodesUsedInCompanies.Any(x => x.Item1 == companyPK && x.Item2 == Code);
					}
					else
					{
						result = BranchManagementCodesUsedInCompanies.Any(x => x.Item2 == Code);
					}
				}

				return result;
			}
		}

		#region BranchManagementCodesUsedInCompanies

		List<Tuple<ZGuid, ZString>> BranchManagementCodesUsedInCompanies
		{
			get
			{
				if (branchManagementCodesUsedInCompanies == null)
				{
					var eodeCollection = new DynamicBusinessObjectCollection(CurrentFactory);
					eodeCollection.Load(@"select distinct GB_GC, GB_AccountingGroupCode from dbo.GlbBranch where GB_AccountingGroupCode <> '' ");
					branchManagementCodesUsedInCompanies = new List<Tuple<ZGuid, ZString>>(eodeCollection.Select(x => new Tuple<ZGuid, ZString>((ZGuid)x[GlbBranch.Schema.GB_GC], (ZString)x[GlbBranch.Schema.GB_AccountingGroupCode])));
				}

				return branchManagementCodesUsedInCompanies;
			}
		}

		List<Tuple<ZGuid, ZString>> branchManagementCodesUsedInCompanies;

		#endregion

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get { return !IsReadOnly; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("0c7e9714-dda6-45ab-9724-e3404a94b2ee", "This group code is in use on some branches and cannot be deleted."); }
		}

		#endregion
	}
}
