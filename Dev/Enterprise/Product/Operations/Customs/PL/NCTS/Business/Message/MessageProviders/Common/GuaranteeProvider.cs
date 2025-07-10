using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants;

namespace Enterprise.Customs.PL.NCTS.Business;

public class GuaranteeProvider : IGuarantee
{
	public GuaranteeProvider(int sequenceNumber, NctsGuarantee[] nctsGuarantees)
	{
		this.nctsGuarantees = Argument.NotNull(nctsGuarantees, nameof(nctsGuarantees));
		nctsGuarantee = nctsGuarantees.First();
		SequenceNumber = sequenceNumber.ToString();
	}
	readonly NctsGuarantee nctsGuarantee;
	readonly NctsGuarantee[] nctsGuarantees;

	public string SequenceNumber { get; }

	public string GuaranteeType => nctsGuarantee.PW_BondType;

	public string OtherGuaranteeReference => nctsGuarantee.PW_BondNumber2;

	public IReadOnlyCollection<IGuaranteeReference> GuaranteeReference => guaranteeReference ?? (guaranteeReference = GetGuaranteeReference());
	IReadOnlyCollection<IGuaranteeReference> guaranteeReference;

	IReadOnlyCollection<IGuaranteeReference> GetGuaranteeReference() => nctsGuarantees
		.Where(x => GuaranteeTypeSet.GuaranteeTypeWithReference.Contains(x.PW_BondType.ToString()))
		.Select((x, i) => new GuaranteeReferenceProvider(i + 1, x))
		.ToArray();
}
