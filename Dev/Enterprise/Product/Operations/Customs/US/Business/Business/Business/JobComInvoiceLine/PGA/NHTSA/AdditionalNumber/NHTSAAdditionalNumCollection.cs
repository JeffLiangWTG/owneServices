using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class NHTSAAdditionalNumCollection : DependentCusAddInfoCollection<NHTSAAdditionalNum, NHTSADetails>
	{
		public NHTSAAdditionalNumCollection(NHTSADetails details)
			: base(details, CusAddInfoTypeAttribute.Codes.USNHTSAAdditionalNumber)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusAddInfoSchema.B7_ParentID; }
		}

		public ZBool HasMultipleNumbersWithSameTypeAndNumber(ZString type, ZString number)
		{
			return this.OfType<NHTSAAdditionalNum>().Count(x => x.US_NHTAdditionalIdentityNumQualifier == type && x.US_NHTAdditionalIdentityNumber == number) > 1;
		}

		public ZBool HasNumberType(ZString type)
		{
			return this.OfType<NHTSAAdditionalNum>().Any(x => x.US_NHTAdditionalIdentityNumQualifier == type && !x.US_NHTAdditionalIdentityNumber.IsEmpty);
		}
	}
}
