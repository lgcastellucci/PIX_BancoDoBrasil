using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PIX_BancoDoBrasil.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace PIX_BancoDoBrasil.Controllers
{
    public class WebhookController : ApiController
    {
        [AcceptVerbs("POST")]
        [Route("webhook/BancodDoBrasil/pix")]
        public HttpResponseMessage ProcessaPixItau(JObject value)
        {

            HttpRequest httpRequest = HttpContext.Current.Request;

            Acessos acessos = new Acessos();
            string codAcesso = acessos.Inserir("POST", httpRequest.Url.ToString(), JsonConvert.SerializeObject(value), null, httpRequest.UserHostAddress);




            var retHttp = new HttpResponseMessage();

            /*
            retHttp.StatusCode = HttpStatusCode.InternalServerError;
            retHttp.Content = new StringContent(JsonConvert.SerializeObject(retItau));
            retHttp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            */

            var retBancoDoBrasil = new
            {
                status = "OK",
                message = "Recebido com sucesso"
            };

            retHttp.StatusCode = HttpStatusCode.OK;
            retHttp.Content = new StringContent(JsonConvert.SerializeObject(retBancoDoBrasil));
            retHttp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return retHttp;
        }

    }
}