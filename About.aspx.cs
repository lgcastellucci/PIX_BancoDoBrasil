using System;
using System.Runtime.InteropServices;

namespace PIX_BancoDoBrasil
{
    public partial class About : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblWindowsVersion.Text = GetWindowsVersion();
                lblOSDescription.Text = GetOSDescription();
            }
        }
        private string GetOSDescription()
        {
            return RuntimeInformation.OSDescription;
        }

        private string GetWindowsVersion()
        {
            //return RuntimeInformation.OSDescription;

            var osVersion = Environment.OSVersion;
            var version = osVersion.Version;

            if (osVersion.Platform == PlatformID.Win32NT)
            {
                if (version.Major == 10)
                    return "Windows 10";
                else if (version.Major == 6 && version.Minor == 3)
                    return "Windows 8.1";
                else if (version.Major == 6 && version.Minor == 2)
                    return "Windows 8";
                else if (version.Major == 6 && version.Minor == 1)
                    return "Windows 7";
                else if (version.Major == 6 && version.Minor == 0)
                    return "Windows Vista";
                else if (version.Major == 5 && version.Minor == 1)
                    return "Windows XP";
            }

            return RuntimeInformation.OSDescription;
        }
    }
}
