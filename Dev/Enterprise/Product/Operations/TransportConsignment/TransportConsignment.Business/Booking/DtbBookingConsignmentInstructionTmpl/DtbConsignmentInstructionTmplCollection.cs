using Enterprise.TransportCommon.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentInstructionTmplCollection : DtbTransportInstructionTmplCollection
	{
		public DtbConsignmentInstructionTmplCollection(DtbConsignmentTmpl template)
			: base(template)
		{
		}

		public new DtbConsignmentInstructionTmpl AddNew()
		{
			return (DtbConsignmentInstructionTmpl)base.AddNew();
		}

		public new DtbConsignmentInstructionTmpl this[int index]
		{
			get { return (DtbConsignmentInstructionTmpl)base[index]; }
		}
	}
}
