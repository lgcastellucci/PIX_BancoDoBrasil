using System.Configuration;
using System.Data.SqlClient;
using System.Text;

namespace PIX_BancoDoBrasil.Models
{
    public class TabelaPix
    {
        readonly string connectionString = ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;
        public string Inserir(string meuId, string devedorCpf, string devedorNome, string valor)
        {
            string codPix = "";

            var sbInstrucao = new StringBuilder();
            sbInstrucao.Append(" INSERT INTO PIX ");
            sbInstrucao.Append("  ( DATA, MEU_ID, DEVEDOR_CPF, DEVEDOR_NOME, VALOR ) ");
            sbInstrucao.Append(" VALUES ");
            sbInstrucao.Append(" ( ");
            sbInstrucao.Append("   GETDATE(), ");
            sbInstrucao.Append(string.IsNullOrWhiteSpace(meuId) ? " NULL, " : " '" + meuId + "', ");
            sbInstrucao.Append(string.IsNullOrWhiteSpace(devedorCpf) ? " NULL, " : " '" + devedorCpf + "', ");
            sbInstrucao.Append(string.IsNullOrWhiteSpace(devedorNome) ? " NULL, " : " '" + devedorNome + "', ");
            sbInstrucao.Append(string.IsNullOrWhiteSpace(valor) ? " NULL " : valor);
            sbInstrucao.Append(" ) ");
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                using (var command = new SqlCommand(sbInstrucao.ToString(), sqlConnection))
                {
                    command.ExecuteNonQuery();
                }

                sbInstrucao.Remove(0, sbInstrucao.Length);
                sbInstrucao.Append("SELECT @@IDENTITY COD_PIX ");
                using (var cmd = new SqlCommand(sbInstrucao.ToString(), sqlConnection))
                {
                    var dataReader = cmd.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            codPix = dataReader["COD_PIX"].ToString();
                        }
                    }
                }

                sqlConnection.Close();
            }
            return codPix;
        }
        public void Atualizar(string codPix, string dataExpiracao, string txid, string pixCopiaECola)
        {
            if (string.IsNullOrEmpty(codPix))
                return;

            var sbInstrucao = new StringBuilder();
            sbInstrucao.Append(" UPDATE PIX ");
            sbInstrucao.Append(" SET ");
            sbInstrucao.Append("   DATA_EXPIRACAO = '" + dataExpiracao + "', ");
            sbInstrucao.Append("   TXID = '" + txid + "', ");
            sbInstrucao.Append("   PIX_COPIA_E_COLA = '" + pixCopiaECola + "' ");
            sbInstrucao.Append(" WHERE COD_PIX = " + codPix);

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

        public void AtualizarStatus(string codPix, string status)
        {
            if (string.IsNullOrEmpty(codPix))
                return;

            var sbInstrucao = new StringBuilder();
            sbInstrucao.Append(" UPDATE PIX ");
            sbInstrucao.Append(" SET ");
            sbInstrucao.Append("   DATA_STATUS = GETDATE(), ");
            sbInstrucao.Append("   STATUS = '" + status + "' ");
            sbInstrucao.Append(" WHERE COD_PIX = " + codPix);

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
