using System;
using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class PersonIdentityInformationSequenceNumberHeader : ISequenceNumberHeader
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public PersonIdentityInformationSequenceNumberHeader(Func<IEnumerable<ISequenceNumberLine>> getSequenceNumberLines)
		{
			this._getSequenceNumberLines = getSequenceNumberLines;
		}
		readonly Func<IEnumerable<ISequenceNumberLine>> _getSequenceNumberLines;

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines
		{
			get { return _getSequenceNumberLines(); }
		}
	}
}
