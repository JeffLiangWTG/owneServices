using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MarketingManager.GUI
{
	[CodeAlive("For TouchSummary.xaml")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldBeSealed")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
	public class TouchSummaryStringConstants
	{
		public static string DeliverySummaryText
		{
			get { return Res.GetString("D4416EB6-B2DF-4F78-9E07-4EF1B367C6E3", "Summary"); }
		}

		public static string TransitionProgressText
		{
			get { return Res.GetString("6EF77970-EB52-4256-B5B1-270159226A29", "Transition progress"); }
		}

		public static string TotalText
		{
			get { return Res.GetString("E2AB6E30-67FD-4522-B354-8B8DECD0041B", "Total"); }
		}

		public static string DeleteText
		{
			get { return Res.GetString("FDC29696-FA3D-4020-8609-6640DDB387EE", "Delete"); }
		}

		public static string AddVerticalText => Res.GetString("B436D362-D81B-4ED0-A1E2-5F8DDCA00088", "New Vertical Touch");

		public static string AddHorizontalText => Res.GetString("3AA49145-B2E6-47BB-A392-62435B32BE98", "New Horizontal Touch");

		public static string LaunchText
		{
			get { return Res.GetString("AC0AB9C9-6ECD-4EEE-A102-6CC7903B2CC0", "Transition Master List"); }
		}

		public static string EditText
		{
			get { return Res.GetString("1A0A2247-B8D5-45E6-8759-A6CAD052FA9C", "Edit"); }
		}

		public static string RefreshText
		{
			get { return Res.GetString("4ADDA8C6-2AE2-4C1A-9185-93AD79A95A66", "Refresh"); }
		}

		public static string GroupingText
		{
			get { return Res.GetString("432F3C65-A88D-4DA7-A384-0B4DC2504BD9", "Recipient Group"); }
		}

		public static string ABContentTestingText
		{
			get { return Res.GetString("0B83DF27-3172-4E54-A075-D6078E7C21C8", "A/B Content Testing"); }
		}

		public static string SplitTestingText
		{
			get { return Res.GetString("60AB0338-F20D-4B55-B16B-E16EC972672B", "Split Rate"); }
		}

		public static string RedCategoryText
		{
			get { return Res.GetString("42DEECAA-98F8-472F-A824-5CC4A8A54FEA", "Red Category"); }
		}

		public static string GreenCategoryText
		{
			get { return Res.GetString("B982E175-0323-4738-8A44-AF9B531DEAA0", "Green Category"); }
		}

		public static string GrayCategoryText
		{
			get { return Res.GetString("B495364D-9E19-4F5D-AC28-EC4DA7700412", "Gray Category"); }
		}

		public static string BlueCategoryText
		{
			get { return Res.GetString("C620F748-1031-4878-B817-8AD9048FC674", "Blue Category"); }
		}

		public static string BrownCategoryText
		{
			get { return Res.GetString("33818D71-8E66-4FB0-BE4F-E24A30FA62B5", "Brown Category"); }
		}
	}
}
