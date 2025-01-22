using System.Configuration;
using System.Data.SqlClient;
using System.Text;

namespace PIX_BancoDoBrasil.Models
{
    public class AcessosExternos
    {
        readonly string connectionString = ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;
        public string Inserir(string codAcesso, string url, string cabecalho, string requicao)
        {
            string codAcessoExterno = "";

            var sbInstrucao = new StringBuilder();
            sbInstrucao.Append(" INSERT INTO ACESSOS_EXTERNOS ");
            sbInstrucao.Append("  ( DATA, COD_ACESSO, SERVIDOR, CABECALHO, REQUISICAO ) ");
            sbInstrucao.Append(" VALUES ");
            sbInstrucao.Append(" ( ");
            sbInstrucao.Append("   GETDATE(), ");
            sbInstrucao.Append(string.IsNullOrWhiteSpace(codAcesso) ? " NULL, " : " '" + codAcesso + "', ");
            sbInstrucao.Append(string.IsNullOrWhiteSpace(url) ? " NULL, " : " '" + url + "', ");
            sbInstrucao.Append(string.IsNullOrWhiteSpace(cabecalho) ? " NULL, " : " '" + cabecalho + "', ");
            sbInstrucao.Append(string.IsNullOrWhiteSpace(requicao) ? " NULL " : " '" + requicao + "' ");
            sbInstrucao.Append(" ) ");
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                using (var command = new SqlCommand(sbInstrucao.ToString(), sqlConnection))
                {
                    command.ExecuteNonQuery();
                }

                sbInstrucao.Remove(0, sbInstrucao.Length);
                sbInstrucao.Append("SELECT @@IDENTITY COD_ACESSO_EXTERNO ");
                using (var cmd = new SqlCommand(sbInstrucao.ToString(), sqlConnection))
                {
                    var dataReader = cmd.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            codAcessoExterno = dataReader["COD_ACESSO_EXTERNO"].ToString();
                        }
                    }
                }

                sqlConnection.Close();
            }
            return codAcessoExterno;
        }
        public void Atualizar(string codAcessoExterno, string resposta, int respostaStatusCode)
        {
            if (string.IsNullOrEmpty(codAcessoExterno))
                return;

            var sbInstrucao = new StringBuilder();
            sbInstrucao.Append(" UPDATE ACESSOS_EXTERNOS ");
            sbInstrucao.Append(" SET ");
            sbInstrucao.Append("   RESPOSTA = '" + resposta + "', ");
            sbInstrucao.Append("   RESPOSTA_DATA = GETDATE(), ");
            sbInstrucao.Append("   HTTP_STATUS_CODE = " + respostaStatusCode.ToString() + " ");
            sbInstrucao.Append(" WHERE COD_ACESSO_EXTERNO = '" + codAcessoExterno + "' ");

            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                using (var command = new SqlCommand(sbInstrucao.ToString(), sqlConnection))
                {
                    command.ExecuteNonQuery();
                }

                sqlConnection.Close();
            }
        }
    }
}
