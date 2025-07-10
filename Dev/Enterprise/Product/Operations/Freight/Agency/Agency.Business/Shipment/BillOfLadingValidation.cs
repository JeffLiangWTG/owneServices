using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class BillOfLadingValidation : AgencyShipmentValidation
	{
		public BillOfLadingValidation(BillOfLading parent)
			: base(parent) { }

		protected override void CheckJS_HouseBill()
		{
			base.CheckJS_HouseBill();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JS_HouseBillInfo, Res.GetString("a243599a-b907-44f8-9995-55f96e7a7f45", "Ocean Bill Of Lading"));
		}

		protected override void CheckDuplicateHouseBills()
		{
			if (!Parent.JS_HouseBill.IsEmpty)
			{
				const string duplicateShipment = "DuplicateShipment";
				const string onSameVoyage = "OnSameVoyage";

				const string sqlWithVesselVoyage = @"
					Select
						JS_UniqueConsignRef As DuplicateShipment,
						cast(case when JV_PK is not null then 1 else 0 end as BIT) as OnSameVoyage
					From dbo.JobShipment
					left Join dbo.JobSailing On JX_PK = JS_JX
					left Join dbo.JobVoyOrigin On JA_PK = JX_JA
					left Join dbo.JobVoyage On JV_PK = JA_JV and JV_RV_NKVessel = @VesselName and JV_VoyageFlight = @VoyageNo
					Where JS_IsShipping = 1
						and JS_IsCancelled = 0
						and JS_PK <> @PK
						and JS_HouseBill = @HouseBill
				";

				const string sqlWithoutVesselVoyage = @"
					Select
						JS_UniqueConsignRef As DuplicateShipment,
						cast(0 as BIT) as OnSameVoyage
					From dbo.JobShipment
					Where JS_IsShipping = 1
						and JS_IsCancelled = 0
						and JS_PK <> @PK
						and JS_HouseBill = @HouseBill
				";

				ZSqlParameterCollection paramCollection = new ZSqlParameterCollection();
				paramCollection.Add("@PK", Parent.PK, JobShipmentSchema.PK);
				paramCollection.Add("@HouseBill", Parent.JS_HouseBill, JobShipmentSchema.JS_HouseBill);

				DynamicBusinessObjectCollection matchingShipments = new DynamicBusinessObjectCollection(Parent.Factory);
				JobSailing sailing = Parent.Sailing;

				if (sailing != null)
				{
					paramCollection.Add("@VesselName", sailing.JX_JV_NKVessel, JobVoyageSchema.JV_RV_NKVessel);
					paramCollection.Add("@VoyageNo", sailing.JX_JV_VoyageFlight, JobVoyageSchema.JV_VoyageFlight);
					matchingShipments.Load(sqlWithVesselVoyage, paramCollection);
				}
				else
				{
					matchingShipments.Load(sqlWithoutVesselVoyage, paramCollection);
				}

				StringBuilder warningString = new StringBuilder();
				bool errorFound = false;

				foreach (DynamicBusinessObject matchingShipment in matchingShipments)
				{
					ZString val = new ZString(matchingShipment[duplicateShipment]);
					ZBool flag = new ZBool(matchingShipment[onSameVoyage]);

					if (flag)
					{
						Parent.JS_HouseBillInfo.AddError(Res.GetString("130ee994-995a-454a-938f-c88a6a7a3f61", "The same Bill number already exists on {0}.\r\nBill of Lading number must be unique per vessel-voyage.", val));
						errorFound = true;
						break;
					}
					else
					{
						warningString.AppendLine(val);
					}
				}

				if (!errorFound && warningString.Length > 0)
				{
					Parent.JS_HouseBillInfo.AddWarning(Res.GetString("ccf8c269-0a7d-464b-970f-2522fe36e644", "This Bill number is already in use on: \r\n{0}", warningString.ToString()));
				}
			}
		}

		#region Implementation

		public new BillOfLading Parent
		{
			get { return (BillOfLading)base.Parent; }
		}

		#endregion
	}
}


