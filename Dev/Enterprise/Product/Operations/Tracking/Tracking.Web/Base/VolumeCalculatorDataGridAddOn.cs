using System.Collections.Generic;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web
{
	public class VolumeCalculatorDataGridAddOn : ZDataGridAddOn, IWebServiceMethodsCaller
	{
		#region Constructors

		public VolumeCalculatorDataGridAddOn()
			: base()
		{
		}

		public VolumeCalculatorDataGridAddOn(string packageCountBindTo, string lengthBindTo, string widthBindTo, string heightBindTo, string unitOfDimensionBindTo, string volumeBindTo, string volumeUQBindTo)
			: base()
		{
			this.packageCountBindTo = packageCountBindTo;
			this.lengthBindTo = lengthBindTo;
			this.widthBindTo = widthBindTo;
			this.heightBindTo = heightBindTo;
			this.unitOfDimensionBindTo = unitOfDimensionBindTo;
			this.volumeBindTo = volumeBindTo;
			this.volumeUQBindTo = volumeUQBindTo;
		}

		#endregion

		#region IWebServiceMethodsCaller

		public List<IWebServiceMethod> WebServiceMethods
		{
			get { return webServiceMethods ?? (webServiceMethods = GetServiceMethods()); }
		}

		List<IWebServiceMethod> webServiceMethods;

		protected virtual List<IWebServiceMethod> GetServiceMethods()
		{
			List<IWebServiceMethod> result = new List<IWebServiceMethod>();
			result.Add(new VolumeCalculatorWebServiceMethod());
			return result;
		}

		#endregion

		#region Overrides

		protected new ZPage Page
		{
			get { return base.Page as ZPage; }
		}

		protected override void AddScripts(DataGridItem item)
		{
			base.AddScripts(item);
			if (Grid != null)
			{
				if (!Grid.ReadOnly && Grid.AllowEdit)
				{
					string piecesControlID = string.Empty;
					string lengthControlID = string.Empty;
					string widthControlID = string.Empty;
					string heightControlID = string.Empty;
					string dimUnitControlID = string.Empty;
					string volumeControlID = string.Empty;
					string volumeUnitControlID = string.Empty;
					string volumeCalculationScript = string.Empty;

					foreach (TableCell cell in item.Controls)
					{
						foreach (WebControl control in cell.Controls)
						{
							ISelfBindingWebControl selfBindingControl = control as ISelfBindingWebControl;
							if (selfBindingControl != null)
							{
								if (selfBindingControl.BindTo == PackageCountBindTo)
								{
									piecesControlID = control.ClientID;
								}
								if (selfBindingControl.BindTo == LengthBindTo)
								{
									lengthControlID = control.ClientID;
								}
								if (selfBindingControl.BindTo == WidthBindTo)
								{
									widthControlID = control.ClientID;
								}
								if (selfBindingControl.BindTo == HeightBindTo)
								{
									heightControlID = control.ClientID;
								}
								if (selfBindingControl.BindTo == UnitOfDimensionBindTo)
								{
									dimUnitControlID = control.ClientID;
								}
								if (selfBindingControl.BindTo == VolumeBindTo)
								{
									volumeControlID = control.ClientID;
								}
								if (selfBindingControl.BindTo == VolumeUQBindTo)
								{
									volumeUnitControlID = control.ClientID;
								}
							}
						}
					}
					if (!string.IsNullOrEmpty(piecesControlID) &&
						!string.IsNullOrEmpty(lengthControlID) &&
						!string.IsNullOrEmpty(widthControlID) &&
						!string.IsNullOrEmpty(heightControlID) &&
						!string.IsNullOrEmpty(dimUnitControlID) &&
						!string.IsNullOrEmpty(volumeControlID) &&
						!string.IsNullOrEmpty(volumeUnitControlID))
					{
						volumeCalculationScript = "CalculateVolume('" + volumeControlID + "', '" + piecesControlID + "', '" + lengthControlID + "', '" + widthControlID + "', '" + heightControlID + "', '" + dimUnitControlID + "', '" + volumeUnitControlID + "');";
					}
					if (!string.IsNullOrEmpty(volumeCalculationScript))
					{
						foreach (TableCell cell in item.Controls)
						{
							foreach (WebControl control in cell.Controls)
							{
								ISelfBindingWebControl selfBindingControl = control as ISelfBindingWebControl;
								if (selfBindingControl != null)
								{
									if (selfBindingControl.BindTo == PackageCountBindTo ||
										selfBindingControl.BindTo == LengthBindTo ||
										selfBindingControl.BindTo == WidthBindTo ||
										selfBindingControl.BindTo == HeightBindTo ||
										selfBindingControl.BindTo == UnitOfDimensionBindTo ||
										selfBindingControl.BindTo == VolumeUQBindTo)
									{
										control.Attributes.Add((NoResString)"onchange", volumeCalculationScript);
									}
								}
							}
						}
					}
				}
			}
		}

		#endregion

		#region Implementation

		protected string PackageCountBindTo
		{
			get { return packageCountBindTo; }
		}

		protected string LengthBindTo
		{
			get { return lengthBindTo; }
		}

		protected string WidthBindTo
		{
			get { return widthBindTo; }
		}

		protected string HeightBindTo
		{
			get { return heightBindTo; }
		}

		protected string UnitOfDimensionBindTo
		{
			get { return unitOfDimensionBindTo; }
		}

		protected string VolumeBindTo
		{
			get { return volumeBindTo; }
		}

		protected string VolumeUQBindTo
		{
			get { return volumeUQBindTo; }
		}

		readonly string packageCountBindTo;
		readonly string lengthBindTo;
		readonly string widthBindTo;
		readonly string heightBindTo;
		readonly string unitOfDimensionBindTo;
		readonly string volumeBindTo;
		readonly string volumeUQBindTo;

		#endregion
	}
}
