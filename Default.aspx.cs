using Newtonsoft.Json.Linq;
using PIX_BancoDoBrasil.Services;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Web.UI;
using Newtonsoft.Json;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using QRCoder;
using PIX_BancoDoBrasil.Models;

namespace PIX_BancoDoBrasil
{
    public partial class _Default : Page
    {
        public class PIX
        {
            public string txid { get; set; }
            public string pixCopiaECola { get; set; }
            public string pixQrCode { get; set; }
        }
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnGerarPIX_Click(object sender, EventArgs e)
        {
            string nomeFuncao = "btn_gerarPix_Click";

            string accessToken = GetToken();

            int expiracaoSegundos = 3600;
            string devedorCPF = "11122233396";
            //string devedorNome = "Jose da Silva";
            double valor = 10.00;

            if (!string.IsNullOrWhiteSpace(txtCPF.Text))
                devedorCPF = txtCPF.Text.Replace(".","").Replace("-","");
            if (!string.IsNullOrWhiteSpace(txtValor.Text))
                valor = Convert.ToDouble(txtValor.Text);

            var minhaIdentificacao = Guid.NewGuid().ToString().Replace("-", "");

            var payLoad = new
            {
                chave = ReadConf.chavePix(),
                calendario = new { expiracao = expiracaoSegundos.ToString() }, //
                //devedor = new { cpf = devedorCPF, nome = devedorNome },
                devedor = new { cpf = devedorCPF },
                valor = new { original = valor.ToString("N2").Replace(".", "").Replace(",", ".") },
                infoAdicionais = new[] {
                    new { nome = "MinhaIdentificacao", valor = minhaIdentificacao }
                }
            };

            var httpService = new HttpService(nomeFuncao);
            httpService.HeaderAcceptAdd(new MediaTypeWithQualityHeaderValue("application/json"));
            httpService.AuthenticationSet(new AuthenticationHeaderValue("Bearer", accessToken));
            httpService.UrlSet("https://api.hm.bb.com.br/pix/v2/cob?gw-dev-app-key=" + ReadConf.developer_application_key());
            httpService.PayLoadSet(JsonConvert.SerializeObject(payLoad), Encoding.UTF8, "application/json");
            var retHttp = httpService.ExecutePost();

            if (retHttp.httpStatusCode != HttpStatusCode.Created)
                return;

            JObject dataJson;
            try
            {
                dataJson = JObject.Parse(retHttp.responseBody);
            }
            catch
            {
                return;
            }

            if (dataJson.SelectToken("txid") == null)
                return;
            if (dataJson.SelectToken("pixCopiaECola") == null)
                return;

            var pix = new PIX();
            pix.txid = dataJson.SelectToken("txid").ToString();
            pix.pixCopiaECola = dataJson.SelectToken("pixCopiaECola").ToString();

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(pix.pixCopiaECola, QRCodeGenerator.ECCLevel.L);
            var qrCode = new QRCode(qrCodeData);

            var qrCodeImage = qrCode.GetGraphic(4);
            //Bitmap qrCodeImageMenor = new Bitmap(qrCodeImage, new Size(250, 250));
            var qrCodeImageBW = qrCodeImage.Clone(new Rectangle(0, 0, qrCodeImage.Width, qrCodeImage.Height), PixelFormat.Format1bppIndexed); //228x228

            var ms = new MemoryStream();
            qrCodeImageBW.Save(ms, ImageFormat.Png);

            byte[] byteImage = ms.ToArray();
            var qrCodeImageAsBase64 = Convert.ToBase64String(byteImage); // Get Base64  

            pix.pixQrCode = qrCodeImageAsBase64;


            string filePath = Server.MapPath("~/Images/QRCode_" + pix.txid + ".png");
            if (!Directory.Exists(Server.MapPath("~/Images")))
                Directory.CreateDirectory(Server.MapPath("~/Images"));
            File.WriteAllBytes(filePath, byteImage);

            // Exibir a imagem na página
            imgQRCode.ImageUrl = "~/Images/QRCode_" + pix.txid + ".png";
            imgQRCode.Visible = true;

        }

        private string GetToken()
        {
            string nomeFuncao = "GetToken";


            var keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            keyValues.Add(new KeyValuePair<string, string>("scope", "cob.read cob.write"));
            string urlEncodedString = new FormUrlEncodedContent(keyValues).ReadAsStringAsync().Result;

            var httpService = new HttpService(nomeFuncao);
            httpService.HeaderAcceptAdd(new MediaTypeWithQualityHeaderValue("application/json"));
            httpService.AuthenticationSet(new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.Default.GetBytes(ReadConf.clientId() + ":" + ReadConf.clientSecret()))));
            httpService.UrlSet("https://oauth.sandbox.bb.com.br/oauth/token");

            httpService.PayLoadSet(urlEncodedString, Encoding.UTF8, "application/x-www-form-urlencoded");
            var retHttp = httpService.ExecutePost();

            if (retHttp.httpStatusCode != HttpStatusCode.Created)
                return "";

            JObject dataJson;
            try
            {
                dataJson = JObject.Parse(retHttp.responseBody);
            }
            catch
            {
                return "";
            }

            if (dataJson.SelectToken("access_token") == null)
                return "";

            if (string.IsNullOrWhiteSpace(dataJson.SelectToken("access_token").ToString()))
                return "";

            return dataJson.SelectToken("access_token").ToString();

        }
    }
}