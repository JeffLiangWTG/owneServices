using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.GUI;

public static class DefaultNetworkRibbonResources
{
	const string BaseSourcePath = "_content/CargoWise.NetworkVisualisation.GUI/images/";

	/// <summary>
	/// A dictionary using the the icon name as the key and the value as the path to the icon.
	/// Those icons are used in the <see cref="RibbonViewModel"/> and children derived from it.
	/// </summary>
	public static Dictionary<string, string> Icons
	{
		get
		{
			return new Dictionary<string, string>()
			{
				{ "BringToFront", $"{BaseSourcePath}BringToFront.svg" },
				{ (NoResString)"Cog", $"{BaseSourcePath}Cog.svg" },
				{ "ColorBucket", $"{BaseSourcePath}ColorBucket.svg" },
				{ (NoResString)"Cross", $"{BaseSourcePath}Cross.svg" },
				{ (NoResString)"Edit", $"{BaseSourcePath}Edit.svg" },
				{ (NoResString)"Entity", $"{BaseSourcePath}Entity.svg" },
				{ (NoResString)"Fill", $"{BaseSourcePath}Fill.svg" },
				{ (NoResString)"Fit", $"{BaseSourcePath}Fit.svg" },
				{ (NoResString)"Link", $"{BaseSourcePath}Link.svg" },
				{ (NoResString)"Minus", $"{BaseSourcePath}Minus.svg" },
				{ "PictureInPicture", $"{BaseSourcePath}PictureInPicture.svg" },
				{ (NoResString)"Plus", $"{BaseSourcePath}Plus.svg" },
				{ "PopOut", $"{BaseSourcePath}PopOut.svg" },
				{ (NoResString)"Previous", $"{BaseSourcePath}Previous.svg" },
				{ (NoResString)"Refresh", $"{BaseSourcePath}Refresh.svg" },
				{ "SendToBack", $"{BaseSourcePath}SendToBack.svg" },
				{ "Zoom100", $"{BaseSourcePath}Zoom100.svg" },
				{ "ZoomIn", $"{BaseSourcePath}ZoomIn.svg" },
				{ "ZoomOut", $"{BaseSourcePath}ZoomOut.svg" },
			};
		}
	}

	/// <summary>
	/// A dictionary using the the icon name with an affix "/base64" as the key and the base64 encoded value as the path to the icon.
	/// These icons are used in the <see cref="RibbonViewModel"/> and children derived from it.
	/// </summary>
	public static Dictionary<string, string> IconsInBase64
	{
		get
		{
			return new Dictionary<string, string>()
			{
				{ "Link/base64", "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAABj1BMVEUAAAAAAP8AgIAAVapAgL8zZpkrgKokbbYgYL8ccaoaZrMudLkraqondrEgcK8jaK4hb7EpcK0nbLEmcbMha7UmcbQkba8jcbEibrMia7UmbLMkbbAkcbIjbrQibLEmb7Ilb7AkbbIkcLMmbbAlb7ElbbEkbbMlbbIkbbMjb7MlbbIkbbEkbrMjb7EjbrIjb7EjbrIlbbMkbbIkb7IkbrMjb7EjbrIlbbMlbrEkb7MkbrEkb7IjbrIlbrEkbrMkb7IjbrMjb7ElbrIkbbIkbrIkb7Ijb7ElbrIkbbMkbrIkb7IjbrMjb7IkbbMjbrElbrMkbbEkbrIjb7IlbrMkbrMkbrEkbrIjb7IlbrEkbbIkbbMkbrIkbrIjb7MlbrIkb7IkbbEkbrIkbrIkbbIkbrIkbrIjbrElbrIkbbIkbrMkb7IkbrIkbbIkb7MkbrIkbbIkbrMkbrIkbrIjbrIkbrIkbrIkbrIkbrIkbrIkbrIkbrIkbrIkbrIkbrIkbrMkbrIkbrIkbrIkbrIkbrL///+2/RvJAAAAg3RSTlMAAQIDBAUGBwgJCgsMDRAWFxkaGx8iIyQlJigqKyw0NTc4OURFS01ZW15gYmRlZmxtbnBxcnN0dXZ4eXp7fX+BgoOEhYeIiouMjo+QkZOXoKGlpqeqq6ytrq+xsrO0tba4ubq/wMHCw8bIy8zN0tPb3N3e3+fo6uvs7vP09fn6+/z9/su73z8AAAABYktHRIRi0FpxAAABnUlEQVQYGXXBiVvSAACH4R9LtyFg92XZXVhWRqGFCVYemdJ9qJRTDK0syg7LykD5/vHE7RkI830VwEgUV5dSpnYSK1D1fq+CRRZxFdsVpO0tVPpiV0tQsNXMcoBbkuIbkNulRuYM0K+qm8AjNbAcICXXOBDXNmYOSMtjPIcfEdVpnQIG5bOX4axqWl4AQ6ozATdUMwqMyjgmnwNx+TrL8EDms7XT8gxC6aB8fTAZ0mNYOaQtGWBEnvB+ZSEhdazCl93alAYcSy57/tuBazAt6cwaLEWlAWDWksueg+8n16Fb0oUyZJUC5my5rFngth5C+bykK7De0b1B3pbLmgHSUvQT/D0lKQc9upQPy2W+BjLatOcr/DoqJSArheQZBu5oy5Gf8ESahKR8h0twT54Tf6bCGoPycfm6wJGv09AIcF81vTCuOneBVy2qOQfLtnwZYLpVdaIr8DQkzwDwxtQ2XcCYXEkgb6vBBJBU1XVgPqxGxkuoXJSUABYjamYvwL/L0d4KFNoUpP0zrndRBdv3gaqFmHZi9n/8Xewx1Ow/friQJqC/2PoAAAAASUVORK5CYII=" },
			};
		}
	}
}
