using System.Drawing;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class StatDescriptionForm : ZChildForm
	{
		public StatDescriptionForm()
		{
			InitializeComponent();
		}

		public StatDescriptionForm(ZString type) : base()
		{
			InitializeComponent();
			contentLabel.Font = new Font(contentLabel.Font.FontFamily, 10);
			Type = type;
		}

		#region Type

		public ZString Type
		{
			get { return type; }
			set
			{
				type = value;
				SetContent();
			}
		}

		ZString type;

		public override string FormHeading
		{
			get
			{
				switch (Type)
				{
					case "NAFTA":
						return " NAFTA Article 303 Claim Statement";
					case "Protest":
						return " Protest or Petition Filed Statement";
					case "5106":
						return "Certification Statement";
					default:
						return "Statement";
				}
			}
		}

		void SetContent()
		{
			switch (Type)
			{
				case "NAFTA":
					contentLabel.Text = NAFTAStat;
					break;
				case "Protest":
					contentLabel.Text = ProtestStat;
					break;
				case "5106":
					contentLabel.Text = _5106Stat;
					ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 200, true);
					break;
			}
		}

		const string NAFTAStat = "Is the importer of the goods aware of any claim for refund, waiver, or reduction of duties relating to the good within the meaning of NAFTA Article 303?";
		const string ProtestStat = "Has any person filed a protest or a petition or request for re-liquidation relating to the good under any provision of law?";
		const string _5106Stat = "I certify that, to the best of my knowledge and belief, all the data or information transmitted is true, correct, and provided in good faith. The filer certifies that if there is intentionally false data, or commits deception or fraud in the 5106 submission, that the filer will be fined or imprisoned (18 U.S.C. § 1001).";

		#endregion

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
