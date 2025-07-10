using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	public class FDAFoodFacilityRegToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		FDAFoodFacilityRegToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(FDAPriorNoticeExemptCodeList.Codes.A, nameof(Xsd.USManufacturerProducerDataTypeFoodFacilityRegistrationExemption.A));
			yield return new Mapping(FDAPriorNoticeExemptCodeList.Codes.B, nameof(Xsd.USManufacturerProducerDataTypeFoodFacilityRegistrationExemption.B));
			yield return new Mapping(FDAPriorNoticeExemptCodeList.Codes.C, nameof(Xsd.USManufacturerProducerDataTypeFoodFacilityRegistrationExemption.C));
			yield return new Mapping(FDAPriorNoticeExemptCodeList.Codes.D, nameof(Xsd.USManufacturerProducerDataTypeFoodFacilityRegistrationExemption.D));
			yield return new Mapping(FDAPriorNoticeExemptCodeList.Codes.E, nameof(Xsd.USManufacturerProducerDataTypeFoodFacilityRegistrationExemption.E));
			yield return new Mapping(FDAPriorNoticeExemptCodeList.Codes.F, nameof(Xsd.USManufacturerProducerDataTypeFoodFacilityRegistrationExemption.F));
			yield return new Mapping(FDAPriorNoticeExemptCodeList.Codes.G, nameof(Xsd.USManufacturerProducerDataTypeFoodFacilityRegistrationExemption.G));
			yield return new Mapping(FDAPriorNoticeExemptCodeList.Codes.H, nameof(Xsd.USManufacturerProducerDataTypeFoodFacilityRegistrationExemption.H));
			yield return new Mapping(FDAPriorNoticeExemptCodeList.Codes.I, nameof(Xsd.USManufacturerProducerDataTypeFoodFacilityRegistrationExemption.I));
			yield return new Mapping(FDAPriorNoticeExemptCodeList.Codes.J, nameof(Xsd.USManufacturerProducerDataTypeFoodFacilityRegistrationExemption.J));
			yield return new Mapping(FDAPriorNoticeExemptCodeList.Codes.K, nameof(Xsd.USManufacturerProducerDataTypeFoodFacilityRegistrationExemption.K));
			yield return new Mapping(FDAPriorNoticeExemptCodeList.Codes.L, nameof(Xsd.USManufacturerProducerDataTypeFoodFacilityRegistrationExemption.L));
			yield return new Mapping(FDAPriorNoticeExemptCodeList.Codes.M, nameof(Xsd.USManufacturerProducerDataTypeFoodFacilityRegistrationExemption.M));
			yield return new Mapping(FDAPriorNoticeExemptCodeList.Codes.O, nameof(Xsd.USManufacturerProducerDataTypeFoodFacilityRegistrationExemption.O));
			yield return new Mapping(FDAPriorNoticeExemptCodeList.Codes.Y, nameof(Xsd.USManufacturerProducerDataTypeFoodFacilityRegistrationExemption.Y));
		}

		public static readonly FDAFoodFacilityRegToXmlCodeMappings Instance = new FDAFoodFacilityRegToXmlCodeMappings();

		protected override string Name
		{
			get { return "FDA Food Facility Registration Exempt"; }
		}

		public new Xsd.USManufacturerProducerDataTypeFoodFacilityRegistrationExemption GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USManufacturerProducerDataTypeFoodFacilityRegistrationExemption.A, errorContext, notify);
		}
	}
}
