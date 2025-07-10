using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USExportVisitedPortCollection : CusCodeDataCollection<USExportVisitedPort>, ISequenceNumberHeader
	{
		public USExportVisitedPortCollection(BusinessObject master)
		: base(master, USExportCusCodeType.Codes.UVP)
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
