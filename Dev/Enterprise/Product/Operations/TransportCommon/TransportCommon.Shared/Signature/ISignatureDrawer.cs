using System.Collections.Generic;
using System.Drawing;

namespace Enterprise.TransportCommon.Shared
{
	public interface ISignatureDrawer
	{
		void DrawSignature(IEnumerable<Point[]> lines, Graphics graphics, Pen pen);
	}
}
