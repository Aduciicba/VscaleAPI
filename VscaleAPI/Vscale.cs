using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Specialized;

namespace VscaleAPI
{
    public class Vscale
    {
        const string API_END_POINT = "https://api.vscale.io/v1/";
        const string TOKEN_PARAM_NAME = "X-Token";

        private readonly string _token;

        public Vscale(string token)
        {
            _token = token;
        }

        #region Web request methods

        private string Get(string path, params string[] paramList)
        {
            var p = GetQueryString(paramList);

            var request = GetNewRequest(path + (p == "" ? "" : "?" + p), "GET");
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "application/json, text/plain";
            request.Headers.Add(TOKEN_PARAM_NAME, _token);

            return GetResponse(request);
        }

        private string Post(string path, params string[] paramList)
        {
            var request = GetNewRequest(path, "POST");

            AddJsonParams(ref request, paramList);

            return GetResponse(request);
        }

        private string Delete(string path, string id)
        {
            var request = GetNewRequest(path + "/" + id, "DELETE");

            return GetResponse(request);
        }

        private string Put(string path, params string[] paramList)
        {
            WebClient client = new WebClient();
            NameValueCollection values = new NameValueCollection();
            client.Headers.Add(TOKEN_PARAM_NAME, _token);
            byte[] responseArray = client.UploadValues(API_END_POINT+path, "PUT", GetFormParams(paramList));
            return Encoding.ASCII.GetString(responseArray);
        }

        private NameValueCollection GetFormParams(params string[] paramList)
        {
            NameValueCollection values = new NameValueCollection();
            int idx = 0;
            while (idx < paramList.Length)
            {
                values.Add(paramList[idx], paramList[idx + 1]);
                idx += 2;
            }
            return values;
        }

        private void AddHeadersParams(ref HttpWebRequest request, params string[] paramList)
        {
            int idx = 0;
            while (idx < paramList.Length)
            {
                request.Headers.Add(paramList[idx], paramList[idx + 1]);
                idx += 2;
            }
        }

        private void AddQueryParams(ref HttpWebRequest request, params string[] paramList)
        {            
            var data = Encoding.ASCII.GetBytes(GetQueryString(paramList));
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }
            request.Headers.Add(TOKEN_PARAM_NAME, _token);
        }

        private void AddJsonParams(ref HttpWebRequest request, params string[] paramList)
        {
            request.ContentType = "application/json";
            var data = Encoding.ASCII.GetBytes(GetJsonParams(paramList));
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }
            request.Headers.Add(TOKEN_PARAM_NAME, _token);
        }

        private HttpWebRequest GetNewRequest(string path, string method)
        {
            var request = (HttpWebRequest)WebRequest.Create(GetUri(path));

            request.Method = method;
            return request;
        }

        private string GetResponse(HttpWebRequest request)
        {
            string responseData = "";

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();
                    StreamReader myStreamReader = new System.IO.StreamReader(responseStream);
                    responseData = myStreamReader.ReadToEnd();
                }
                response.Close();
            }
            catch (Exception ex)
            {
                responseData = "An error occurred: " + ex.Message;
            }

            return responseData;
        }

        private string GetJsonParams(params string[] paramList)
        {
            //string json = "";
            Dictionary<string, string> values = new Dictionary<string, string>();

            //values.Add(TOKEN_PARAM_NAME, _token);

            int idx = 0;
            while (idx < paramList.Length)
            {
                values.Add(paramList[idx], paramList[idx + 1]);
                idx += 2;
            }

            return JsonConvert.SerializeObject(values);

        }

        private Uri GetUri(string path)
        {
            return new Uri(API_END_POINT + path);
        }

        string GetQueryString(params string[] paramList)
        {
            var postData = "";

            int idx = 0;
            while (idx < paramList.Count())
            {
                postData += String.Format("&{0}={1}", paramList[idx], paramList[idx + 1]);
                idx += 2;
            }
            if (postData != "")
                postData = postData.Remove(0, 1);
            return postData;
        }

        #endregion

        #region Account

        /// <summary>
        /// Возвращает информацию о пользователе: имя, дата активации аккаунта, адрес электронной почты
        /// </summary>
        /// <returns>
        /// actdate  Дата активации аккаунта
        /// country Страна, указанная пользователем при регистрации
        /// face_id Служебный параметр, указывающий тип клиента(физическое или юридическое лицо, резидент или нерезидент РФ)
        /// state Служебный параметр(1 — пользователь активен; 0 — неактивен)
        /// email Адрес электронной почты, указанный пользователем при регистрации
        /// name Имя
        /// middlename Отчество
        /// surname Фамилия
        /// </returns>
        public string Account()
        {
            return Get("account");
        }
        #endregion

        #region Background

        /// <summary>
        /// Возвращает список дата-центров, а также образов и конфигураций, доступных в этих дата-центрах
        /// </summary>
        /// <returns>
        /// 
        /// </returns>
        public string Locations()
        {
            return Get("locations");
        }

        /// <summary>
        /// Возвращает список доступных образов серверов. 
        /// Для каждого образа указывается также список конфигураций, для которых он может быть использован. 
        /// Информация об образах содержится в параметре id — в нём указывается имя операционной системы. 
        /// Информация о конфигурации содержится в параметре rplans, который может принимать одной из следующих значений:
        /// small — 512 МБ RAM, 1 ядро CPU, 20 ГБ SSD, 1 ТБ трафика
        /// medium — 1 ГБ RAM, 1 ядро CPU, 30 ГБ SSD, 2 ТБ трафика
        /// large — 2 ГБ RAM, 2 ядра CPU, 40 ГБ SSD, 3 TБ трафика
        /// huge — 4 ГБ RAM, 2 ядра CPU, 60 ГБ SSD, 4 TБ трафика
        /// monster — 8 ГБ RAM, 4 ядра CPU, 80 ГБ SSD, 5 TБ трафика
        /// </summary>
        /// <returns></returns>
        public string Images()
        {
            return Get("images");
        }
        #endregion

        #region Configurations

        /// <summary>
        /// Возвращает список доступных конфигураций
        /// </summary>
        /// <returns></returns>
        public string Rplans()
        {
            return Get("rplans");
        }
        /// <summary>
        /// Возвращает информацию о стоимости использования каждой из доступных конфигураций за час и за месяц.
        /// </summary>
        /// <returns></returns>
        public string Prices()
        {
            return Get("billing/prices");
        }

        #endregion

        #region SSH Keys

        /// <summary>
        /// Возвращает список имеющихся SSH-ключей.
        /// </summary>
        /// <returns></returns>
        public string SshKeys()
        {
            return Get("sshkeys");
        }

        /// <summary>
        /// Добавляет в список ключ, переданный в запросе.
        /// </summary>
        /// <param name="key">ssh ключ</param>
        /// <param name="name">имя нового ключа</param>
        /// <returns></returns>
        public string SshKeysNew(string key, string name)
        {
            return Post("sshkeys", new string[] { "key", key,
                                                  "name", name });
        }

        /// <summary>
        /// Удаляет указанный SSH-ключ.
        /// </summary>
        /// <param name="keyId">id ключа</param>
        /// <returns></returns>
        public string SshKeysDelete(string keyId)
        {
            return Delete("sshkeys", keyId);
        }

        #endregion

        #region Notifications

        /// <summary>
        /// Возвращает информацию о пороговой сумме баланса, при достижении которой отправляется уведомление о необходимость пополнить счёт.
        /// </summary>
        /// <returns></returns>
        public string NotifyBilling()
        {
            return Get("billing/notify");
        }

        /// <summary>
        /// Устанавливает пороговую сумму, при достижении которой будет отправлено уведомление о необходимости пополнить счёт. Значение порога устанавливается в копейках.
        /// </summary>
        /// <param name="bound">значение порога в копейках</param>
        /// <returns></returns>
        public string NotifySetBalanceBound(int bound)
        {
            return Put("billing/notify", new string[] { "notify_balance", bound.ToString() });
        }

        #endregion

        #region Billing

        /// <summary>
        /// Возвращает информацию о состоянии основного и бонусного баланса.
        /// </summary>
        /// <returns></returns>
        public string BillingBalance()
        {
            return Get("billing/balance");
        }

        /// <summary>
        /// Возвращает информацию о последних поступлениях средств на счёт.
        /// </summary>
        /// <returns></returns>
        public string BillingPayments()
        {
            return Get("billing/payments");
        }

        /// <summary>
        /// Возвращает информацию о списаниях средств со счёта за определенный период времени (начиная с первой указанной даты, но исключая последнюю указаннную дату).
        /// </summary>
        /// <param name="startDate">начало периода</param>
        /// <param name="endDate">конец периода</param>
        /// <returns></returns>
        public string BillingConsumption(DateTime? startDate, DateTime? endDate)
        {
            startDate = startDate.HasValue ? startDate : DateTime.Parse("10.07.2016");
            endDate = endDate.HasValue ? endDate : DateTime.Now;

            return Get("billing/consumption", new string[] { "start", startDate.Value.ToString("yyyy-MM-dd"), "end", endDate.Value.ToString("yyyy-MM-dd") });
        }

        #endregion

        #region Domains

        public string DomainsAdd(string name, string bind)
        {
            string[] values = new string[bind == "" ? 2 : 4];
            values[0] = "name";
            values[1] = name; 
            if (bind != "")
            {
                values[0] = "bind";
                values[1] = bind;
            }

            return Post("domains", values);
        }

        public string DomainsList()
        {
            return Get("domains");
        }

        #endregion

        public string Scalets()
        {
            return Get("scalets");
        }

        public string Scalets(int id)
        {
            return Post("scalets", new string[] { "" });
        }
    }
}
