using Enterprise.ZArchitecture.GUI.Controls.Extensions;

namespace Enterprise.Customs.US.GUI
{
	class USMailRefHintExtension : HintExtension
	{
		public override string ShortCaption
		{
			get
			{
				return "Mail Ref.";
			}
		}

		public override string Caption
		{
			get
			{
				return "Mail Reference";
			}
		}

		public override string Description
		{
			get
			{
				return "Mail Reference of the consignment.";
			}
		}
	}
}
