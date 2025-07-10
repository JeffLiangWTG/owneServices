using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSHeaderContainerValidation : Customs.Business.CusInBondContainerValidation
	{
		public SPTSHeaderContainerValidation(SPTSContainer parent)
			: base(parent)
		{
		}

		protected new SPTSContainer Parent
		{
			get { return (SPTSContainer)base.Parent; }
		}

		protected override void CheckBC_ContainerNum()
		{
			base.CheckBC_ContainerNum();

			var targetInfo = Parent.BC_ContainerNumInfo;
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

			var containerNumber = Parent.BC_ContainerNum;
			var containerPK = Parent.PK;
			if (!containerNumber.IsEmpty)
			{
				if (Parent.BC_ParentTableCode == CusInBondHeaderSchema.Constants.Prefix)
				{
					if (Parent.ParentBusinessObject?.HeaderContainers.OfType<SPTSContainer>().Any(x => x.BC_ContainerNum == containerNumber && x.PK != containerPK) ?? false)
					{
						targetInfo.AddMessageError(Res.GetString("E4E8C86D-E6E3-4D84-852B-F5DEACC1E418", "This Container Number already exists."));
					}
				}
				else if (Parent.BC_ParentTableCode == CusInBondBillSchema.Constants.Prefix)
				{
					if (Parent.SPTSBill.SPTSBillContainers.OfType<SPTSContainer>().Any(x => x.BC_ContainerNum == containerNumber && x.PK != containerPK))
					{
						targetInfo.AddMessageError(Res.GetString("0D27F2C6-D1B3-4575-AC9D-0176C2099152", "This container number has already been selected."));
					}
				}
			}
		}
	}
}
