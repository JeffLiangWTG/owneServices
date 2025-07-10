using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class Trader : JobDocAddress
	{
		public Trader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.Load<JobDeclaration>(E2_ParentID);
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		[List(nameof(Lookups) + "." + nameof(TraderLookups.TraderTypeList))]
		public override ZString E2_AddressType
		{
			get => base.E2_AddressType;
			set
			{
				var oldValue = E2_AddressType;
				base.E2_AddressType = value;
				if (!IsCopying && oldValue != E2_AddressType)
				{
					if (Declaration != null)
					{
						var addressTypeCount = Declaration.Traders.Count(x => x.E2_AddressType == value);
						if (addressTypeCount > 0 && E2_AddressSequence == ZByte.Zero)
						{
							E2_AddressSequence = (ZByte)addressTypeCount;
						}
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(TraderLookups.OrgHeader_List))]
		public new ZGuid OrganisationPK
		{
			get { return base.OrganisationPK; }
			set { base.OrganisationPK = value; }
		}

		protected override JobDocAddressLookups GetNewLookups() => new TraderLookups(this);

		public new TraderLookups Lookups => (TraderLookups)base.Lookups;

		protected override JobDocAddressValidation GetNewValidation() => new TraderValidation(this);

		public new TraderValidation Validation => (TraderValidation)base.Validation;
	}
}
