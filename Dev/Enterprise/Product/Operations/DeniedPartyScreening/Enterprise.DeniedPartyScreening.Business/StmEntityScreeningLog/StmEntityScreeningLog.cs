using System;
using System.Data;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;
using IStmEntityScreeningLog = Enterprise.DeniedPartyScreening.Integration.IStmEntityScreeningLog;

namespace Enterprise.DeniedPartyScreening.Business
{
	public class StmEntityScreeningLog : AutoStmEntityScreeningLog, IStmEntityScreeningLog
	{
		public StmEntityScreeningLog(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (PJ_Sequence.IsEmpty)
			{
				PJ_Sequence = Convert.ToInt32(Env.NumberFountains.StmEntityScreeningLogNumber.GetNextFormatted(Factory));
			}
		}

		#region Properties

		#region Status

		public ZString StatusDescription
		{
			get { return Lookups.Statuses.GetDescriptionFromCode(PJ_Status); }
		}

		#endregion

		#region Screened By

		public ZString ScreenedByFullName
		{
			get { return ScreenedBy != null ? ScreenedBy.GS_FullName : ZString.Empty; }
		}

		public IGlbStaff ScreenedBy
		{
			get { return (IGlbStaff)Factory.LoadFromNaturalKey(ObjectFactory.GetType<IGlbStaff>(), GlbStaffSchema.GS_Code, PJ_SystemCreateUser); }
		}

		#endregion

		#region Screen Date

		public ZDateTime PJ_ScreenDate => PJ_SystemCreateTimeUtc.ToLocalBranchTime();

		#endregion

		#region ParentDescription

		public ZString ParentDescription
		{
			get
			{
				ZString result = ZString.Empty;

				switch (PJ_ParentTableCode)
				{
					case OrgHeaderSchema.Constants.Prefix:
						var org = Factory.Load<IOrgHeader>(PJ_ParentID);
						if (org != null)
						{
							result = ZString.Join(" - ", new ZString[] { org.Code, org.FullName, org.MainAddress.OA_RN_NKCountryCode });
						}
						break;

					case RefVesselSchema.Constants.Prefix:
						var vessel = Factory.Load<IRefVessel>(PJ_ParentID);
						if (vessel != null)
						{
							result = vessel.RV_Code;
						}
						break;

					case JobDocAddressSchema.Constants.Prefix:
						var docAddress = (IDocAddress)Factory.Load<IJobDocAddress>(PJ_ParentID);
						if (docAddress != null)
						{
							result = docAddress.ParentDescription + " - " + docAddress.AddressCaption;
						}
						break;

					case JobShipmentSchema.Constants.Prefix:
						var shipment = Factory.Load<IForwardingShipment>(PJ_ParentID) as BusinessObject;
						if (shipment != null)
						{
							result = shipment.HumanReadableName;
						}
						break;

					case JobConsolSchema.Constants.Prefix:
						var consol = Factory.Load<IForwardingConsol>(PJ_ParentID) as BusinessObject;
						if (consol != null)
						{
							result = consol.HumanReadableName;
						}
						break;

					case JobDeclarationSchema.Constants.Prefix:
						var declaration = Factory.Load<IBaseJobDeclaration>(PJ_ParentID) as BusinessObject;
						if (declaration != null)
						{
							result = declaration.HumanReadableName;
						}
						break;

					case RefCountrySchema.Constants.Prefix:
						var country = Factory.Load<IRefCountry>(PJ_ParentID) as BusinessObject;
						if (country != null)
						{
							result = country.HumanReadableName + " - " + country[RefCountrySchema.RN_Desc];
						}
						break;
				}

				return result;
			}
		}

		#endregion

		#region SourceInformation

		public ZString SourceInformation
		{
			get
			{
				ZString result;

				if (!PJ_SourceTableCode.IsEmpty && PJ_SourceID.IsValid)
				{
					switch (PJ_SourceTableCode)
					{
						case OrgHeaderSchema.Constants.Prefix:
							var org = Factory.Load<IOrgHeader>(PJ_SourceID);
							result = GetSourceInformation(true, Res.GetString("50BB1CD4-7DA4-4019-921F-3545B1FA33EC", "Org."), org?.OH_Code);
							break;

						case RefVesselSchema.Constants.Prefix:
							var vessel = Factory.Load<IRefVessel>(PJ_SourceID);
							result = GetSourceInformation(true, Res.GetString("3CB35729-FBCD-4527-A52D-57DCE04BF572", "Vessel"), vessel?.RV_Code);
							break;

						case JobShipmentSchema.Constants.Prefix:
							var shipment = Factory.Load<IForwardingShipment>(PJ_SourceID);
							result = GetSourceInformation(false, Res.GetString("C7F4CB32-A3F2-4E19-944F-9C89ED0D356C", "Job"), shipment?.JS_UniqueConsignRef);
							break;

						case JobConsolSchema.Constants.Prefix:
							var consol = Factory.Load<IForwardingConsol>(PJ_SourceID);
							result = GetSourceInformation(false, Res.GetString("C7F4CB32-A3F2-4E19-944F-9C89ED0D356C", "Job"), consol?.JK_UniqueConsignRef);
							break;

						case JobDeclarationSchema.Constants.Prefix:
							var declaration = Factory.Load<IBaseJobDeclaration>(PJ_SourceID);
							result = GetSourceInformation(false, Res.GetString("C7F4CB32-A3F2-4E19-944F-9C89ED0D356C", "Job"), declaration?.JE_DeclarationReference);
							break;

						case JobDocAddressSchema.Constants.Prefix:
							result = GetSourceInformation(true, Res.GetString("C7F4CB32-A3F2-4E19-944F-9C89ED0D356C", "Job"), Res.GetString("F45E109B-9EB5-492B-B193-F3C4963409ED", "Doc Address"));
							break;

						case RefCountrySchema.Constants.Prefix:
							var country = Factory.Load<IRefCountry>(PJ_SourceID);
							result = GetSourceInformation(true, Res.GetString("ADCEDD4B-06D2-4B42-BDF9-26B5643B4AE3", "Country/Region:"), string.Format(CultureInfo.InvariantCulture, "{0} - {1}", country.RN_Code, country.RN_Desc));
							break;

						default:
							result = GetSourceInformation(false, null, null);
							break;
					}
				}
				else
				{
					result = Res.GetString("23ED7E99-C6DE-4794-A742-A3054624C2B0", "Not Recorded");
				}

				return result;
			}
		}

		ZString GetSourceInformation(bool isEntity, string prefix, string code)
		{
			string result;

			if (string.IsNullOrEmpty(code))
			{
				result = Res.GetString("D8DA8880-3C4B-48F0-B800-A08ED4C091E9", "Source Not Exist");
			}
			else
			{
				if (isEntity)
				{
					result = string.Format(CultureInfo.InvariantCulture, "{0} {1}", prefix, code);
				}
				else
				{
					if (PJ_IsForcedRescreen)
					{
						result = string.Format(CultureInfo.InvariantCulture, "{0} {1} - {2}", prefix, code, Res.GetString("8AF3D2D5-0952-4491-9767-88FCBEBBC0BE", "Force Re-Screen"));
					}
					else
					{
						result = string.Format(CultureInfo.InvariantCulture, "{0} {1} - {2}", prefix, code, Res.GetString("8DFA0131-6C36-4E28-AB9F-1ADD332378D2", "Screen"));
					}
				}
			}

			return result;
		}

		#endregion

		#endregion
	}
}
