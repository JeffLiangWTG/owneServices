using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CargoWise.eHub.Core.Transforms.Helper
{
    /// <summary>
    /// Represents a disposable override of the current thread's CurrentCulture.
    /// The default culture used for the override is 'en_AU'.
    /// </summary>
    class CultureOverride : IDisposable
    {
        static CultureInfo defaultCulture = CultureInfo.GetCultureInfo("en-AU");

        CultureInfo savedCulture;

        public CultureOverride()
        {
            savedCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = defaultCulture;
        }

        public void Dispose()
        {
            Thread.CurrentThread.CurrentCulture = savedCulture;
        }
    }
}
