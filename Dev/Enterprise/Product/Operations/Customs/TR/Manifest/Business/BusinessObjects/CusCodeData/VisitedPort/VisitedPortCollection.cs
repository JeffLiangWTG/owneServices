using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class VisitedPortCollection : CusCodeDataCollection<VisitedPort>, ISequenceNumberHeader
	{
		public VisitedPortCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.TRVisitedPort)
		{ }

		#region ISequenceNumberHeader

		public ShortSequenceNumberGenerator SequenceNumberCalculator
		{
			get { return sequenceNumberCalculator ?? (sequenceNumberCalculator = new ShortSequenceNumberGenerator(this)); }
		}
		ShortSequenceNumberGenerator sequenceNumberCalculator;

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => this.Cast<ISequenceNumberLine>();

		#endregion
	}
}
