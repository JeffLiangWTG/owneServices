using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[UniversalDataContext(DataContextType.AccEPaymentBeneficiaryRequest)]
	public class AccEPaymentBeneficiaryRequest : AutoAccEPaymentBeneficiaryRequest, IEPaymentDeliveryContextValueProvider
	{
		public AccEPaymentBeneficiaryRequest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[List("Lookups.StatusCodeList")]
		public override ZString ABR_Status { get => base.ABR_Status; set => base.ABR_Status = value; }

		[List("Lookups.ProviderCodeList")]
		public override ZString ABR_ProviderCode { get => base.ABR_ProviderCode; set => base.ABR_ProviderCode = value; }

		public GlbStaff CreatingUser => Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ABR_SystemCreateUser);

		#endregion

		#region IEPaymentDeliveryContextValueProvider Memebers

		ZString IEPaymentDeliveryContextValueProvider.Purpose => $"E-Payment Beneficiary Request {ABR_InternalReference} submitted for processing to {ABR_ProviderCode}";
		EntityInfo IEPaymentDeliveryContextValueProvider.EntityInfo => EntityInfo.New(this);

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ABR_InternalReference = PK.ToString().Substring(0, 8);
			ABR_GC_Company = GlbCompany.CurrentCompany.PK;
			ABR_ProviderCode = EPaymentProviderCodes.Codes.OFX;
		}
#endif
	}
}
