using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	[BusinessContext(BusinessContext.Rating)]
	public class IntercompanyTariff : RatingHeader, ITemplateCopyable
	{
		public IntercompanyTariff(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		#region Properties

		[List("Lookups.Clients")]
		[ResourceStringData("IntercompanyTariff|TH_OH", Caption = "Service Provider")]
		public override ZGuid TH_OH
		{
			get => base.TH_OH;
			set => base.TH_OH = value;
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			TH_GC = ZGuid.Empty;
		}

		#endregion

		#region DisplayInfo / RatingHeaderTypeDescription

		protected override string RatingHeaderTypeDescriptionCore =>
			Res.GetString("03D0522C-D91C-497E-A943-EEC7E7F64A62", "Intercompany Tariff");

		public override ZString DisplayInfo()
			=> DisplayInfoWithOrgInfo(RatingHeaderTypeDescription, Header);

		#endregion

		#region Validation

		protected override RatingHeaderValidation GetNewValidation() =>
			new IntercompanyTariffValidation(this);

		public new IntercompanyTariffValidation Validation =>
			(IntercompanyTariffValidation)base.Validation;

		#endregion

		#region FillWithValidTestDataCore
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (TH_OH.IsEmpty)
			{
				TH_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			}

			if (!TH_GC.IsEmpty)
			{
				TH_GC = ZGuid.Empty;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
		#endregion

		#region ITemplateCopyable Members
		IBusiness ITemplateCopyable.TemplateCopy()
		{
			return base.CopyIncludingChildren();
		}
		#endregion
	}
}
