using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class EPaymentBankAccountDisclaimerControl : ZUserControl
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public EPaymentBankAccountDisclaimerControl()
		{
			InitializeComponent();
		}

		public EPaymentBankAccountDisclaimerControl(ZString paymentProviderCode)
		{
			PaymentProviderCode = paymentProviderCode;
			InitializeComponent();
		}

		readonly ZString PaymentProviderCode;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisclaimerMessageLabel.Text = Res.GetString("1329c9cd-6fd2-4f6a-96a9-e42e219cf8ed", @"Before you create an E-payment Account, you must first register your organization for an {0} account. Each user on your E-payment Account should be an authorized user on your {0} account. Click “Manage Account Users” to add users to your E-payment account.

Once authorized, users will be able to request FX quotes and book FX transactions with {0} from the Payment Processing module.

Please note that CargoWise provides messaging and information exchange only. FX quotes are issued and FX transactions are executed by {0}, a third party provider.

Click “Learn More” for more information about {0} and CargoWise Global Integrated Payments.", PaymentProviderCode);
		}

		public static DialogDefaultContext DialogDefaultContext
		{
			get
			{
				return new DialogDefaultContext(new ZGuid("7d819b2d-1097-4061-85e1-2ef600c105ae"),
												ResString.GetMultilingualString("23c1c357-2a50-4397-ad09-520917f85bf3", "Save E-Payment Account"),
												null,
												ZMessageBoxIcon.Information,
												showCheckboxOnly: true,
												checkBoxCaption: Res.GetData("2669c1f9-a64a-469c-9a55-54069d05820d", "Do not show this message again"));
			}
		}

		void LearnMoreButton_Click(object sender, EventArgs e)
		{
			EPaymentUrlLauncher.LaunchEPaymentProductMarketingURL();
		}
	}
}
