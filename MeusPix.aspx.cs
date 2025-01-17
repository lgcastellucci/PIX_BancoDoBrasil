using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;

namespace PIX_BancoDoBrasil
{
    public partial class MeusPix : Page
    {
        readonly string connectionString = ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CarregarDados();
            }
        }

        protected void btnAtualizar_Click(object sender, EventArgs e)
        {
            CarregarDados();
        }

        private void CarregarDados()
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                var sbInstrucao = new StringBuilder();
                sbInstrucao.Remove(0, sbInstrucao.Length);
                sbInstrucao.Append(" SELECT COD_PIX, MEU_ID, DEVEDOR_CPF, DEVEDOR_NOME, DATA, VALOR, DATA_STATUS, STATUS ");
                sbInstrucao.Append(" FROM PIX ");
                //sbInstrucao.Append(" WHERE TXID = '" + txid + "'");

                using (var cmd = new SqlCommand(sbInstrucao.ToString(), sqlConnection))
                {
                    var dataAdapter = new SqlDataAdapter(cmd);
                    var dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    // Formatar o campo DEVEDOR_CPF
                    foreach (DataRow row in dataTable.Rows)
                    {
                        string devedorCpf = row["DEVEDOR_CPF"].ToString();
                        if (devedorCpf.Length == 11)
                        {
                            row["DEVEDOR_CPF"] = Convert.ToUInt64(devedorCpf).ToString(@"000\.000\.000\-00");
                        }
                        else if (devedorCpf.Length == 14)
                        {
                            row["DEVEDOR_CPF"] = Convert.ToUInt64(devedorCpf).ToString(@"00\.000\.000\/0000\-00");
                        }
                    }

                    gvPixEmitidos.DataSource = dataTable;
                    gvPixEmitidos.DataBind();
                }

                sqlConnection.Close();
            }

            // Atualizar o campo de última atualização
            lblUltimaAtualizacao.Text = "Última atualização: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }
    }
}
