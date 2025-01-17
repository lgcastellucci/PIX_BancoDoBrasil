using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;

namespace PIX_BancoDoBrasil.Models
{
    public class Acessos
    {
        readonly string connectionString = ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;
        public string Inserir(string tipoAcesso, string url, string requicao, string ip)
        {
            string codAcesso = "";

            StringBuilder sbInstrucao = new StringBuilder();
            sbInstrucao.Append(" INSERT INTO ACESSOS ");
            sbInstrucao.Append("  ( DATA, TIPO_ACESSO, SERVIDOR, REQUISICAO, IP ) ");
            sbInstrucao.Append(" VALUES ");
            sbInstrucao.Append(" ( ");
            sbInstrucao.Append("   GETDATE(), ");
            sbInstrucao.Append(string.IsNullOrWhiteSpace(tipoAcesso) ? " NULL, " : " '" + tipoAcesso + "', ");
            sbInstrucao.Append(string.IsNullOrWhiteSpace(url) ? " NULL, " : " '" + url + "', ");
            sbInstrucao.Append(string.IsNullOrWhiteSpace(requicao) ? " NULL, " : " '" + requicao + "', ");
            sbInstrucao.Append(string.IsNullOrWhiteSpace(ip) ? " NULL " : " '" + ip + "' ");
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
        public void Atualizar(string codAcesso, string resposta, string codRetornoInterno, string Ocorrencia)
        {
            StringBuilder sbInstrucao = new StringBuilder();
            sbInstrucao.Append(" UPDATE ACESSOS ");
            sbInstrucao.Append(" SET RESPOSTA = ").Append(string.IsNullOrWhiteSpace(resposta) ? " NULL, " : " '" + resposta + "', ");
            sbInstrucao.Append("     RESPOSTA_DATA = ").Append(string.IsNullOrWhiteSpace(resposta) ? " NULL, " : " GETDATE(), ");
            sbInstrucao.Append("     COD_RETORNO_INTERNO = ").Append(string.IsNullOrWhiteSpace(codRetornoInterno) ? " NULL, " : " '" + codRetornoInterno + "', ");
            sbInstrucao.Append("     OCORRENCIA = ").Append(string.IsNullOrWhiteSpace(Ocorrencia) ? " NULL, " : " '" + Ocorrencia + "' ");
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
