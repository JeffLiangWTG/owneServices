using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance.CountryComplianceInfoDisplay
{
	public class OrgCusCodeTypeDisplay : NonPersistentBusinessObject
	{
		public OrgCusCodeTypeDisplay(CodeDescriptionPair orgCusCodeType, ZBool primary, ZBool main) : base(new ReadOnlyBusinessObjectFactory())
		{
			Code = orgCusCodeType.Code;
			Description = orgCusCodeType.Description;
			Primary = primary;
			Main = main;
		}

		[ResourceStringData("C011EED9-DABA-4919-A808-F47853DFF801", Caption = "Type Code")]
		public ZString Code { get; }

		[ResourceStringData("417A875F-58F1-43EE-BA20-8AD7D0786D22", Caption = "Type Description")]
		public ZString Description { get; }

		[ResourceStringData("D5936330-7D23-4F18-9B88-C44342682A6D", Caption = "Primary")]
		public ZBool Primary { get; }

		[ResourceStringData("056C79B1-EE4A-4556-8C6F-54EAB4BCF1A2", Caption = "Main")]
		public ZBool Main { get; }
	}

	public class OrgCusCodeTypeDisplayCollection : NonPersistentBusinessObjectCollection<OrgCusCodeTypeDisplay>
	{
		internal void LoadTypes(OrgCusCode orgCusCode)
		{
			RemoveAll();
			var mainCodes = OrgRegistrationNumberTypeList.GetApplicableNumberTypes(orgCusCode.CodeCountry).ToHashSet();
			foreach (CodeDescriptionPair type in orgCusCode.Lookups.OK_CodeType_List)
			{
				orgCusCode.OK_CodeType = type.Code;
				Add(new OrgCusCodeTypeDisplay(type, orgCusCode.IsCurrentCompanyCodeTypePrimary, mainCodes.Contains(orgCusCode.OK_CodeType)));
			}
		}
		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
	}
}
