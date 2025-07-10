using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierBuyerLinkToleranceValidation : AutoOrgSupplierBuyerLinkToleranceValidation
	{
		public OrgSupplierBuyerLinkToleranceValidation(AutoOrgSupplierBuyerLinkTolerance parent) : base(parent)
		{
		}

		new OrgSupplierBuyerLinkTolerance Parent => (OrgSupplierBuyerLinkTolerance)base.Parent;

		protected override void CheckOLT_TransportMode()
		{
			base.CheckOLT_TransportMode();

			MandatoryValidation.CheckEntered(Parent.OLT_TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OLT_TransportModeInfo, Parent.Lookups.TransportModes, TransportModeValidationMessage);

			if (!Parent.OLT_TransportModeInfo.HasErrors())
			{
				CheckOrderLineTolerancesAreUnique();
			}
		}

		protected override void CheckOLT_PartNumber()
		{
			base.CheckOLT_PartNumber();

			ListValidation.ErrorIfInvalidCode(Parent.OLT_PartNumberInfo, Parent.Lookups.SupplierParts, PartNumberValidationMessage);

			if (!Parent.OLT_PartNumberInfo.HasErrors())
			{
				CheckOrderLineTolerancesAreUnique();
			}
		}

		protected override void CheckOLT_UnderQuantityPercentageLimit()
		{
			base.CheckOLT_UnderQuantityPercentageLimit();

			if (Parent.OLT_UnderQuantityPercentageLimit < 0 || Parent.OLT_UnderQuantityPercentageLimit > 99.999m)
			{
				Parent.OLT_UnderQuantityPercentageLimitInfo.AddError(UnderQuantityPercentageLimitValidationMessage);
			}
		}

		protected override void CheckOLT_OverQuantityPercentageLimit()
		{
			base.CheckOLT_OverQuantityPercentageLimit();

			if (Parent.OLT_OverQuantityPercentageLimit < 0 || Parent.OLT_OverQuantityPercentageLimit > 999.999m)
			{
				Parent.OLT_OverQuantityPercentageLimitInfo.AddError(OverQuantityPercentageLimitValidationMessage);
			}
		}

		#region Unique Order Line Tolerance

		void CheckOrderLineTolerancesAreUnique()
		{
			Parent.ClearRowNotifications();

			var similarOrderLineTolerancesFilter = new ZQuery();
			similarOrderLineTolerancesFilter.AddToFilter(OrgSupplierBuyerLinkToleranceSchema.OLT_OL_SupplierBuyerLink, Parent.OLT_OL_SupplierBuyerLink);
			similarOrderLineTolerancesFilter.AddToFilter(OrgSupplierBuyerLinkToleranceSchema.OLT_TransportMode, Parent.OLT_TransportMode);
			similarOrderLineTolerancesFilter.AddToFilter(OrgSupplierBuyerLinkToleranceSchema.OLT_PartNumber, Parent.OLT_PartNumber);
			similarOrderLineTolerancesFilter.AddToFilter(OrgSupplierBuyerLinkToleranceSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			if (Parent.Factory.Exists(typeof(OrgSupplierBuyerLinkTolerance), similarOrderLineTolerancesFilter))
			{
				Parent.AddRowError(DuplicateOrderLineToleranceMessage);
			}
		}

		static string DuplicateOrderLineToleranceMessage => Res.GetString("580a2c7e-d123-4514-84b7-6c0543e8898b", "An Order Line Tolerance with Transport Mode / Part Number already exists for this supplier-buyer relationship.");

		#endregion

		static MultilingualString TransportModeValidationMessage => ResString.GetMultilingualString("e3b43f85-5d64-4845-aeeb-2c6a8b5a20ef", "transport mode");

		static MultilingualString PartNumberValidationMessage => ResString.GetMultilingualString("3dc049d1-f304-47a0-a3e2-82c5dd4cbe82", "You have not entered an existing product number that is related to the supplier or buyer.");

		static MultilingualString UnderQuantityPercentageLimitValidationMessage => ResString.GetMultilingualString("d9d7cbcd-c4b9-4a8d-aa09-272d5bf5e4b5", "Must be between 0 and 99.999.");

		static MultilingualString OverQuantityPercentageLimitValidationMessage => ResString.GetMultilingualString("f1807e33-69cb-4292-8b61-c10d5ec76843", "Must be between 0 and 999.999.");
	}
}
