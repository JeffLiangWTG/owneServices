using System;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterData.Business
{
	public class GenShapeGeographyNewSecurityCheckpoint : SecurityCheckpoint
	{
		public GenShapeGeographyNewSecurityCheckpoint()
			: base("GeographyNew", ResString.GetMultilingualString("73e75f38-4373-4fa5-a490-6d4b4ae0eb7d", "Cannot create new Geography"), null, null, false) // SecurityCheckpoint code should be English
		{
		}

		public override bool IsAllowed => Registry.Business.SystemDataRegistry.Instance.GeographyUnreleasedFunctions.Value;

		public override bool Visible => false;

		public override void AddChild(SecurityCheckpoint child)
		{
			throw new NotSupportedException("AddChild() is not supported by GenShapeGeographyNewSecurityCheckpoint.");
		}

		public override void ShowError()
		{
			throw new NotSupportedException("ShowError() is not supported by GenShapeGeographyNewSecurityCheckpoint.");
		}

		public override MultilingualString ErrorMessageForNotAllowed
		{
			get { return ResString.GetMultilingualString("2aaa90c5-d7d3-4cb2-ad50-4e2e59a9b4d1", "Functionality to allow the import of user defined geography will be introduced shortly."); }
		}
	}
}
