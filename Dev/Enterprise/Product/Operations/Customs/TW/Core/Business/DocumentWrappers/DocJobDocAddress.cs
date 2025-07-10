using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.TW.Business
{
	public class DocJobDocAddress : DocBaseWrapper
	{
		DocJobDocAddress(TWJobDocAddress jobDocAddress, BusinessObjectFactory factory)
			: base(jobDocAddress, factory)
		{
		}

		TWJobDocAddress JobDocAddress => (TWJobDocAddress)WrappedObject;

		public static DocJobDocAddress New(TWJobDocAddress jobDocAddress, BusinessObjectFactory factoryToWrap)
		{
			return new DocJobDocAddress(jobDocAddress, factoryToWrap);
		}

		public ZString Fax => JobDocAddress.E2_Fax;

		public ZString Phone => JobDocAddress.E2_Phone;

		public ZString Email => JobDocAddress.E2_Email;

		public ZString Contact => JobDocAddress.E2_Contact;
	}
}
