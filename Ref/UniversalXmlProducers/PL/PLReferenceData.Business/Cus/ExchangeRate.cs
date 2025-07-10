using System.Collections.Generic;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Cus
{
	// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
	/// <remarks/>
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	public partial class tabela_kursow
	{
		string numer_tabeliField;
		System.DateTime data_publikacjiField;
		List<tabela_kursowPozycja> pozycjaField;
		string typField;
		string uidField;

		/// <remarks/>
		public string numer_tabeli
		{
			get
			{
				return this.numer_tabeliField;
			}
			set
			{
				this.numer_tabeliField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
		public System.DateTime data_publikacji
		{
			get
			{
				return this.data_publikacjiField;
			}
			set
			{
				this.data_publikacjiField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("pozycja")]
		public List<tabela_kursowPozycja> pozycja
		{
			get
			{
				return this.pozycjaField;
			}
			set
			{
				this.pozycjaField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string typ
		{
			get
			{
				return this.typField;
			}
			set
			{
				this.typField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string uid
		{
			get
			{
				return this.uidField;
			}
			set
			{
				this.uidField = value;
			}
		}
	}

	/// <remarks/>
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	public partial class tabela_kursowPozycja
	{
		string nazwa_walutyField;
		decimal przelicznikField;
		string kod_walutyField;
		string kurs_sredniField;

		/// <remarks/>
		public string nazwa_waluty
		{
			get
			{
				return this.nazwa_walutyField;
			}
			set
			{
				this.nazwa_walutyField = value;
			}
		}

		/// <remarks/>
		public decimal przelicznik
		{
			get
			{
				return this.przelicznikField;
			}
			set
			{
				this.przelicznikField = value;
			}
		}

		/// <remarks/>
		public string kod_waluty
		{
			get
			{
				return this.kod_walutyField;
			}
			set
			{
				this.kod_walutyField = value;
			}
		}

		/// <remarks/>
		public string kurs_sredni
		{
			get
			{
				return this.kurs_sredniField;
			}
			set
			{
				this.kurs_sredniField = value;
			}
		}
	}
}
