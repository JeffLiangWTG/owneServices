using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGCountryReference : AutoUNDGCountryReference
	{
		public UNDGCountryReference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Resource strings

		[ResourceStringData("Enterprise.MasterFiles.Business.UNDGCountryReference|DCR_FlashPointLowerReadOnly", Caption = "Lower Flash Point")]
		public ZString DCR_FlashPointLowerCentigradeReadOnly
		{
			get { return DCR_HasFlashPointLower ? DCR_FlashPointLowerCentigrade.ToString() : "N/A"; }
		}

		[ResourceStringData("Enterprise.MasterFiles.Business.UNDGCountryReference|DCR_FlashPointUpperReadOnly", Caption = "Upper Flash Point")]
		public ZString DCR_FlashPointUpperCentigradeReadOnly
		{
			get { return DCR_HasFlashPointUpper ? DCR_FlashPointUpperCentigrade.ToString() : "N/A"; }
		}

		[ResourceStringData("Enterprise.MasterFiles.Business.UNDGCountryReference|DCR_TypeDescription", Caption = "Type Description")]
		public ZString DCR_TypeDescription => UNDGCountryReferenceLookups.Types.GetDescriptionFromCode(DCR_Type) ?? ZString.Empty;

		[ResourceStringData("Enterprise.MasterFiles.Business.UNDGCountryReference|DCR_Code", Caption = "Code")]
		public override ZString DCR_Code { get => base.DCR_Code; set => base.DCR_Code = value; }

		[ResourceStringData("Enterprise.MasterFiles.Business.UNDGCountryReference|DCR_Type", Caption = "Type")]
		public override ZString DCR_Type { get => base.DCR_Type; set => base.DCR_Type = value; }

		[ResourceStringData("Enterprise.MasterFiles.Business.UNDGCountryReference|DCR_RN_NKCountry", Caption = "Country/Region")]
		public override ZString DCR_RN_NKCountry { get => base.DCR_RN_NKCountry; set => base.DCR_RN_NKCountry = value; }

		[ResourceStringData("Enterprise.MasterFiles.Business.UNDGCountryReference|DCR_Description", Caption = "Description")]
		public override ZString DCR_Description { get => base.DCR_Description; set => base.DCR_Description = value; }

		#endregion

		public bool IsSystemCountryReference => DCR_RN_NKCountry == Core.Constants.CountryCodes.Singapore && DCR_Type == Core.Constants.UNDGCountryReference.Type.PSA;

		public override bool CanDetach => Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed && !IsSystemCountryReference;

		public override string ReasonNotToBeAbleToDetach => Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed
			? Res.GetString("278b33ef-d11f-64b2-4331-22e9e70a1c01", "System maintained Country/Region Reference/s (SG, PSA) cannot be detached.")
			: Res.GetString("022315fe-c76b-c89c-4d5f-03ba1ba57957", "You do not have the security rights to detach Country/Region Regulations.");

		public UNDGCountryReferencePivot SubstancePivot { get; set; }

		#region Test Helpers
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			DCR_Description = "description";
			DCR_Code = "code";
			DCR_Type = "type";
		}

#endif
		#endregion
	}
}
