using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public partial class IncoTermDescriptionForm : ZChildForm
	{
		public IncoTermDescriptionForm(ZString incoTerm) : base()
		{
			TitleLabel.Font = new Font(TitleLabel.Font.FontFamily, 18);
			IncoTerm = incoTerm;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Title

		public ZString IncoTerm
		{
			get { return incoTerm; }
			set
			{
				incoTerm = value;
				SetTitle();
				SetDescription();
			}
		}

		ZString incoTerm;

		public override string FormHeading
		{
			get { return IncoTerm; }
		}

		void SetTitle()
		{
			if (IncoTerms.ContainsCode(IncoTerm))
			{
				TitleLabel.Text = IsIncotermObsolete
					? string.Concat(IncoTerm, " (", Res.GetString("00953ed4-83c6-44e2-9a81-0f67f8aa0c6c", "obsolete"), ")", " - ", IncoTerms.GetDescriptionFromCode(incoTerm))
					: string.Concat(IncoTerm, " - ", IncoTerms.GetDescriptionFromCode(incoTerm));

#if DEBUG
				TypeDescriptor.AddAttributes(TitleLabel, new SuppressFormsLocalizedTestAttribute());
#endif
			}
			else
			{
				TitleLabel.Text = Res.GetString("Freight|IncotermDescriptionFormITitleLabel", "Incoterms");
			}
		}

		public string Title
		{
			get { return TitleLabel.Text; }
		}

		bool IsIncotermObsolete
		{
			get
			{
				IEnumerable<string> obsoleteTerms = IncotermValidation.Instance.IncotermsExpiringIn2011;
				if (ZDateTime.UtcNow >= Core.Constants.IncoTerms.Incoterms2020EffectiveDate)
				{
					obsoleteTerms = obsoleteTerms.Union(IncotermValidation.Instance.IncotermsExpiringIn2020);
				}

				return obsoleteTerms.Contains((string)IncoTerm);
			}
		}

		#endregion

		#region Description

		void SetDescription()
		{
			moreInfoLabel.GetExtension<ILabelCaptionRenderer>().Caption = !IncoTerms.ContainsCode(IncoTerm) ?
				Res.GetString("47f380db-30e1-6fa8-4006-a3ca201e42d8", "A blank or invalid Incoterm was specified.") :
				Res.GetString("d72fc608-7964-4f02-8380-843c938cfad9", "For more information on Incoterms refer to the International Chamber of Commerce website.");
		}

		public string Description
		{
			get { return moreInfoLabel.GetExtension<ILabelCaptionRenderer>().Caption; }
		}

		#endregion

		#region INCOTERM List

		CodeDescriptionPairList IncoTerms
		{
			get
			{
				if (fIncoTerms == null)
				{
					fIncoTerms = new IncoTermsCodeDescriptionPairList();
				}
				return fIncoTerms;
			}
		}

		CodeDescriptionPairList fIncoTerms;

		#endregion

		#region Close

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
			Dispose();
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
