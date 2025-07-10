using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	public class BranchDistrictPort : RegistryBusinessObjectTemplate
	{
		public BranchDistrictPort()
		{
		}

		public BranchDistrictPort(FallbackLevel fallbackLevel, BusinessObjectFactory factory, BranchDistrictPortCollection parentCollection)
			: base(fallbackLevel, factory)
		{
			this.parentCollection = parentCollection;
		}

		BranchDistrictPortCollection ParentCollection
		{
			get
			{
				if (parentCollection == null)
				{
					parentCollection = (BranchDistrictPortCollection)GetParentCollection(this, typeof(BranchDistrictPortCollection));
				}
				return parentCollection;
			}
		}
		BranchDistrictPortCollection parentCollection;

		public static class Schema
		{
			public const string PortCode = "PortCode";
			public const string BranchPK = "BranchPK";
		}

		#region Port Code

		[List(nameof(Ports))]
		[MaxLength(4)]
		public ZString PortCode
		{
			get { return portCode; }
			set
			{
				SetNonPersistentPropertyValue(PortCodeInfo, ref portCode, value);

				if (!IsValidationSuspended)
				{
					ValidatePortCode();
				}
			}
		}
		ZString portCode;

		public IBusinessObjectCollection Ports
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(CurrentFactory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public ZPropertyInfo PortCodeInfo
		{
			get { return GetZPropertyInfo(Schema.PortCode); }
		}

		public void ValidatePortCode()
		{
			PortCodeInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(PortCodeInfo, "Port");

			if (!PortCode.IsEmpty)
			{
				if (PortCode.Length != 2 && PortCode.Length != 4)
				{
					PortCodeInfo.AddError(PortCodeLength);
				}

				if (ParentCollection != null)
				{
					var items = ParentCollection.ToArray<BranchDistrictPort>();
					if (items.Count(x =>
						(
							x.PortCode == PortCode ||
							(
								x.PortCode.SubstringSafe(0, 2).Contains(PortCode.SubstringSafe(0, 2)) && (x.PortCode.Length == 2 || PortCode.Length == 2)
							)
						)
						&& x.BranchPK == BranchPK) > 1)
					{
						PortCodeInfo.AddError(DuplicatesFound);
					}
				}
			}
		}
		internal const string PortCodeLength = "Code should be 2 chars for District or 4 chars for Port in length.";
		internal const string DuplicatesFound = "You have already entered this combination of Branch and District/Port.";

		#endregion

		#region Branch

		[List(nameof(CurrentCompanyBranches))]
		public ZGuid BranchPK
		{
			get { return branchPK; }
			set
			{
				SetNonPersistentPropertyValue(BranchPKInfo, ref branchPK, value);
				if (!IsValidationSuspended)
				{
					ValidateBranchPK();
				}
			}
		}
		ZGuid branchPK;

		public ZPropertyInfo BranchPKInfo
		{
			get { return GetZPropertyInfo(Schema.BranchPK); }
		}

		public void ValidateBranchPK()
		{
			BranchPKInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(BranchPKInfo);

			if (BranchPK.IsEmpty)
			{
				BranchPKInfo.AddError(BranchIsMandatory);
			}

			ValidatePortCode();
		}
		internal const string BranchIsMandatory = "You have not entered a Branch.";

		public GlbBranchCollection CurrentCompanyBranches
		{
			get { return new GlbBranchCollection(CurrentFactory, new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK)); }
		}

		#endregion

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidatePortCode();
			ValidateBranchPK();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BranchDistrictPort(fallbackLevel, factory, null);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.PortCode, PortCode);
			writer.WriteElementString(Schema.BranchPK, BranchPK.ToString());
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			PortCode = reader.ReadElementString(Schema.PortCode);
			BranchPK = new ZGuid(reader.ReadElementString(Schema.BranchPK));
		}

		#endregion
	}
}
