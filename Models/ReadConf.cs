using System.Configuration;

namespace PIX_BancoDoBrasil.Models
{
    public class ReadConf
    {
        public static string developer_application_key()
        {
            if (ConfigurationManager.AppSettings["developer_application_key"] == null)
                return "";

            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["developer_application_key"].ToString()))
                return "";

            try
            {
                return ConfigurationManager.AppSettings["developer_application_key"].ToString();
            }
            catch
            {

            }
            return "";
        }
        public static string clientId()
        {
            if (ConfigurationManager.AppSettings["clientId"] == null)
                return "";

            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["clientId"].ToString()))
                return "";

            try
            {
                return ConfigurationManager.AppSettings["clientId"].ToString();
            }
            catch
            {

            }
            return "";
        }
        public static string clientSecret()
        {
            if (ConfigurationManager.AppSettings["clientSecret"] == null)
                return "";

            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["clientSecret"].ToString()))
                return "";

            try
            {
                return ConfigurationManager.AppSettings["clientSecret"].ToString();
            }
            catch
            {

            }
            return "";
        }
        public static string chavePix()
        {
            if (ConfigurationManager.AppSettings["chavePix"] == null)
                return "";

            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["chavePix"].ToString()))
                return "";

            try
            {
                return ConfigurationManager.AppSettings["chavePix"].ToString();
            }
            catch
            {

            }
            return "";
        }

    }
}