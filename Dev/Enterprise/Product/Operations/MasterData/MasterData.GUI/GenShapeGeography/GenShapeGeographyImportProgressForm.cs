using System;
using System.ComponentModel;
using CargoWise.Common;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture.GUI;
using SharpKml.Base;
using SharpKml.Dom;

namespace Enterprise.MasterData.GUI
{
	public partial class GenShapeGeographyImportProgressForm : ZChildForm
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		const string PlacemarkStartElement = "<Placemark>";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		const string PlacemarkEndElement = "</Placemark>";
		const int PlacemarkEndElementLength = 12;

		GenShapeGeography ShapeGeography { get; }
		string ShapeInfoStr { get; }

		public GenShapeGeographyImportProgressForm(GenShapeGeography genShapeGeography, string shapeInfoStr) : base(genShapeGeography)
		{
			InitializeComponent();

			ShapeGeography = genShapeGeography;
			ShapeInfoStr = shapeInfoStr;
#if DEBUG
			TypeDescriptor.AddAttributes(ProgressInfoTextBox, new SuppressControlRequiresTextBasherAttribute());
#endif
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			StartImport();
		}

		protected void StartImport()
		{
			if (!string.IsNullOrEmpty(ShapeInfoStr))
			{
				PrintProgressLog(Res.GetString("14985528-2ADD-4705-BA39-772B531B0750", "Import shape geography started."));

				if (ImportShapeInfo(ShapeInfoStr))
				{
					PrintProgressLog(Res.GetString("6ACD981C-3E2D-42A0-8683-04D2B0376E22", "Import shape geography finished with no error."));
				}
				else
				{
					PrintProgressLog(Res.GetString("746AAB32-04FE-45B0-B382-41EE935B6E8D", "Import shape geography finished with errors."));
				}
			}
			else
			{
				ProcessErrorMessage(Res.GetString("B200BEB2-9542-4B42-99DC-28BEB53316E8", "The KML file should not be empty."));
			}
		}

		protected void PrintProgressLog(string logStr)
		{
			ProgressInfoTextBox.Text += logStr + System.Environment.NewLine;
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		bool ImportShapeInfo(string input)
		{
			var importSuccess = false;

			try
			{
				var startIndex = input.IndexOf(PlacemarkStartElement, StringComparison.OrdinalIgnoreCase);
				var firstPlacemarkEndIndex = input.IndexOf(PlacemarkEndElement, StringComparison.OrdinalIgnoreCase);
				var lastPlacemarkEndIndex = input.LastIndexOf(PlacemarkEndElement, StringComparison.OrdinalIgnoreCase);

				if (startIndex > 0 && firstPlacemarkEndIndex > startIndex)
				{
					if (firstPlacemarkEndIndex == lastPlacemarkEndIndex)
					{
						var parser = new Parser();
						parser.ParseString(input.Substring(startIndex, firstPlacemarkEndIndex - startIndex + PlacemarkEndElementLength), false);
						var valueInfoStr = ((Placemark)parser.Root).AsWKT(out var pointsCount);

						if (pointsCount == 0)
						{
							ProcessErrorMessage(Res.GetString("B977D167-C2F9-4470-BF0C-A0BD8922CBE3", "The KML file should have at least 1 point."));
						}
						else if (pointsCount > Registry.Business.SystemDataRegistry.Instance.MaximumGeographyPointNumberLimit.Value)
						{
							ProcessErrorMessage(Res.GetString("B44AE49C-9A89-49AA-8911-31186863C75B", "The KML file contains {0} points, should be less or equals than {1} points.", pointsCount, Registry.Business.SystemDataRegistry.Instance.MaximumGeographyPointNumberLimit.Value));
						}
						else
						{
							var geography = new CargoWise.Types.ZGeography(valueInfoStr);

							if (!geography.IsEmpty && geography.IsValid)
							{
								geography = geography.MakeValid();
								ShapeGeography.SHG_Shape = geography;
								importSuccess = true;
								PrintProgressLog(Res.GetString("453E9A43-3519-4414-AA7D-78D4A2C71185", "Success: Import shape geography correctly, shape information - {0}", ShapeGeography.ShapeInformation));
							}
							else
							{
								ProcessErrorMessage(Res.GetString("22C9B8B1-CF13-4E88-A0E0-2BC6FD912E68", "Failed to get the valid shape geography from the KML file."));
							}
						}
					}
					else
					{
						ProcessErrorMessage(Res.GetString("C62D82FB-5966-4470-92CD-DAFAB3280305", "The KML file should have 1 {0} element, indicating 1 shape.", PlacemarkStartElement));
					}
				}
				else
				{
					ProcessErrorMessage(Res.GetString("049EB7AC-AF92-4643-A86E-04E82BC39DF6", "Failed to get the {0} element from the KML file, please check the KML file format.", PlacemarkStartElement));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ProcessErrorMessage(ex.Message);
				importSuccess = false;
			}

			return importSuccess;
		}

		void ProcessErrorMessage(string message)
		{
			PrintProgressLog(Res.GetString("1F8109D7-1E0E-49F4-AE82-B644FF821EFA", "Error: {0}", message));
		}
	}
}
