using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class StmNumberRangeMatchingDetailsCollection : ActiveBusinessObjectCollection<StmNumberRangeMatchingDetail>
	{
		public StmNumberRangeMatchingDetailsCollection(BusinessObject header)
			: base(Argument.NotNull(header, nameof(header)).Factory, header, new ZQuery(), StmNumberRangeMatchingDetailSchema.NRM_OwnerId)
		{
			Owner = header;
		}
		readonly BusinessObject Owner;

		#region Implementation

		protected override void SetDefaultsForNewElementCore(StmNumberRangeMatchingDetail newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.NRM_OwnerId = Owner.PK;
			newElement.NRM_OwnerTableCode = Owner.TablePrefix;

			if (Owner is OrgHeader)
			{
				newElement.NRM_RangeType = OrgConstants.NumberFountains.Code.TransportReferenceNumbers; // please remove this line when user have more choice
			}
			else if (Owner is GlbStaff)
			{
				newElement.NRM_RangeType = OrgConstants.NumberFountains.Code.PatentNumber; // please remove this line when user have more choice

				var owner = Owner as GlbStaff;
				var certificate = owner.Certificates?.FirstOrDefault(w => w.XZ_Type == Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK && w.XZ_RN_NKCountryOfIssuance == Core.Constants.CountryCodes.Mexico);

				if (certificate != null)
				{
					newElement.PatentNumber = certificate.XZ_RefNumber;
				}
			}
		}

		#endregion
	}
}
