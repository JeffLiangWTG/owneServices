using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class DOTIDOT : IDOT
	{
		public DOTIDOT(DOT dot, bool sendVIN, ZShort? supLineVIN = null)
		{
			this.dot = dot;
			this.sendVIN = sendVIN;
			this.supLineVIN = supLineVIN;
		}

		readonly DOT dot;
		readonly bool sendVIN;
		readonly ZShort? supLineVIN;

		#region IDOT Members

		public ZString CommercialDescription
		{
			get { return ((IDOT)dot).CommercialDescription; }
		}

		public ZString BoxNumber
		{
			get { return ((IDOT)dot).BoxNumber; }
		}

		public ZString BoxCertification
		{
			get { return ((IDOT)dot).BoxCertification; }
		}

		public ZString PassportNumber
		{
			get { return ((IDOT)dot).PassportNumber; }
		}

		public ZString CountryISO
		{
			get { return ((IDOT)dot).CountryISO; }
		}

		public ZString DOTBondSuretyCode
		{
			get { return ((IDOT)dot).DOTBondSuretyCode; }
		}

		public ZBool NHTSAPermissionLetterOfficialOrdersCertification
		{
			get { return ((IDOT)dot).NHTSAPermissionLetterOfficialOrdersCertification; }
		}

		public ZBool ImportersSubstantiatingStatementCopyOfContractManufacturersConfirmationLetter
		{
			get { return ((IDOT)dot).ImportersSubstantiatingStatementCopyOfContractManufacturersConfirmationLetter; }
		}

		public ZString ClarificationCode
		{
			get { return ((IDOT)dot).ClarificationCode; }
		}

		public ZString TireManufacturerIDCode
		{
			get { return ((IDOT)dot).TireManufacturerIDCode; }
		}

		public ZString TireManufacturerBrandName
		{
			get { return ((IDOT)dot).TireManufacturerBrandName; }
		}

		public IEnumerable<IDOTVIN> VINs
		{
			get
			{
				if (sendVIN && dot.DOTVINs.Count > 0)
				{
					if (supLineVIN.HasValue)
					{
						yield return new SupLineDOTVIN(supLineVIN.Value.ToString().PadLeft(17, '0'), dot.DOTVINs[0]);
					}
					else
					{
						foreach (DOTVIN vin in dot.DOTVINs)
						{
							yield return vin;
						}
					}
				}
			}
		}

		class SupLineDOTVIN : IDOTVIN
		{
			public SupLineDOTVIN(string vin, IDOTVIN dotvin)
			{
				this.dotvin = dotvin;
				this.vin = vin;
			}
			readonly string vin;
			readonly IDOTVIN dotvin;

			#region IDOTVIN Members

			public ZString MakeOfVehicle
			{
				get { return dotvin.MakeOfVehicle; }
			}

			public ZString Model
			{
				get { return dotvin.Model; }
			}

			public ZInt Year
			{
				get { return dotvin.Year; }
			}

			public ZString VehicleIdentificationNumber
			{
				get { return vin; }
			}

			public ZString NHTSARegisteredImporterRINumber
			{
				get { return dotvin.NHTSARegisteredImporterRINumber; }
			}

			public ZString VehicleEligibilityNumber
			{
				get { return dotvin.VehicleEligibilityNumber; }
			}

			#endregion
		}

		#endregion

		#region IOGALine Members

		public ZString CommercialDesc
		{
			get { return ((IOGALine)dot).CommercialDesc; }
			set { ((IOGALine)dot).CommercialDesc = value; }
		}

		#endregion
	}
}
