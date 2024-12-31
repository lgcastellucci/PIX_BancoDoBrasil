using System.Configuration;
using System.Data.SqlClient;
using System.Text;

namespace PIX_BancoDoBrasil.Models
{
    public class Acessos 
    {
        readonly string connectionString = ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;
        public string Inserir(string tipoAcesso = null, string url = null, string requicao = null, string resposta = null, string ip = null)
        {
            string codAcesso = "";

            StringBuilder sbInstrucao = new StringBuilder();
            sbInstrucao.Append(" INSERT INTO ACESSOS ");
            sbInstrucao.Append("  ( DATA, TIPO_ACESSO, URL, REQUISICAO, RESPOSTA, COD_RETORNO_INTERNO, OCORRENCIA, IP ) ");
            sbInstrucao.Append(" VALUES ");
            sbInstrucao.Append(" ( ");
            sbInstrucao.Append("   GETDATE(), ");

            if (string.IsNullOrEmpty(tipoAcesso))
                sbInstrucao.Append("  null, ");
            else
                sbInstrucao.Append("   '" + tipoAcesso + "', ");

            if (string.IsNullOrEmpty(url))
                sbInstrucao.Append("  null, ");
            else
                sbInstrucao.Append("   '" + url + "', ");

            if (string.IsNullOrEmpty(requicao))
                sbInstrucao.Append("  null, ");
            else
                sbInstrucao.Append("   '" + requicao + "', ");

            if (string.IsNullOrEmpty(resposta))
                sbInstrucao.Append("  null, ");
            else
                sbInstrucao.Append("   '" + resposta + "', ");

            sbInstrucao.Append("  null, ");
            sbInstrucao.Append("  null, ");

            if (string.IsNullOrEmpty(ip))
                sbInstrucao.Append("  null, ");
            else
                sbInstrucao.Append("   '" + ip + "' ");

            sbInstrucao.Append(" ) ");

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                using (SqlCommand command = new SqlCommand(sbInstrucao.ToString(), sqlConnection))
                {
                    command.ExecuteNonQuery();
                }

                sbInstrucao.Remove(0, sbInstrucao.Length);
                sbInstrucao.Append("SELECT @@IDENTITY COD_ACESSO ");
                using (SqlCommand cmd = new SqlCommand(sbInstrucao.ToString(), sqlConnection))
                {
                    SqlDataReader dataReader = cmd.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            codAcesso = dataReader["COD_ACESSO"].ToString();
                        }
                    }
                }


                sqlConnection.Close();
            }
            return codAcesso;
        }
        public void Atualizar(string codAcesso, string resposta)
        {
            StringBuilder sbInstrucao = new StringBuilder();
            sbInstrucao.Append(" UPDATE ACESSOS ");
            sbInstrucao.Append(" SET RESPOSTA = '" + resposta + "'");
            sbInstrucao.Append(" WHERE COD_ACESSO = " + codAcesso);

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                using (SqlCommand command = new SqlCommand(sbInstrucao.ToString(), sqlConnection))
                {
                    command.ExecuteNonQuery();
                }
                sqlConnection.Close();
            }
        }
    }
}
