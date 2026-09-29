using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace UnspedHealth.Client
{
    /// <summary>
    /// Unsped Health Monitoring sistemi için istemci tarafı yönetim sınıfı.
    /// Uygulamaların hayatta olduğunu (heartbeat) merkezi sunucuya bildirmek için kullanılır.
    /// </summary>
    public static class HeartbeatAgent
    {
        private static readonly HttpClient _client = new HttpClient();

        /// <summary>
        /// Arka planda çalışan bir heartbeat (nabız) sinyali gönderim döngüsü başlatır.
        /// API'deki [HttpPost("heartbeat/{serviceID}")] endpoint'ini tetikler.
        /// </summary>
        /// <param name="API_URL">Heartbeat sinyalinin gönderileceği API base noktası (Örn: http://api.unsped.com/HealthCheck/).</param>
        /// <param name="SERVICE_ID">Veritabanında bu uygulama için tanımlanmış benzersiz kimlik (GUID).</param>
        /// <param name="INTERVAL_SECONDS">Sinyal gönderim aralığı (Saniye). Varsayılan: 120 saniye.</param>
        public static async Task<bool> StartAsync(string API_URL, Guid SERVICE_ID, int INTERVAL_SECONDS = 120)
        {

            if (string.IsNullOrEmpty(API_URL) || SERVICE_ID == Guid.Empty) return false;

            _client.Timeout = TimeSpan.FromSeconds(10.0);
            string URL = $"{API_URL.TrimEnd('/')}/health/heartbeat/{SERVICE_ID}";

            bool firstAttempt;

            try
            {
                using (var response = await _client.PostAsync(URL, null).ConfigureAwait(false))
                {
                    firstAttempt = response.IsSuccessStatusCode;
                }
            }
            catch { firstAttempt = false; }

            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(INTERVAL_SECONDS)).ConfigureAwait(false);

                while (true)
                {
                    try
                    {
                        using (var response = await _client.PostAsync(URL, null).ConfigureAwait(false))
                        {

                        }
                    }
                    catch { }

                    await Task.Delay(TimeSpan.FromSeconds(INTERVAL_SECONDS)).ConfigureAwait(false);
                }
            });

            return firstAttempt;
        }

        /// <summary>
        /// Manuel olarak anlık sinyal veya hata mesajı gönderir, heartbeat döngüsünden bağımsızdır. Main methodunuzun içinde veya exception bloklarında çağrılabilir. API'deki [HttpPost("report")] endpoint'ini tetikler.
        /// </summary>
        /// <param name="API_URL">Heartbeat sinyalinin gönderileceği API base noktası (Örn: http://api.unsped.com/HealthCheck/).</param>
        /// <param name="SERVICE_ID">Veritabanında bu uygulama için tanımlanmış benzersiz kimlik (GUID).</param>
        /// <param name="message">Gönderilmek istenen hata veya durum mesajı. (Opsiyonel).</param>
        /// <param name="isError">Mesajın bir hata olup olmadığı (Opsiyonel).</param>
        public static async Task<bool> SendSignalAsync(string API_URL, Guid SERVICE_ID, string MESSAGE = null, bool IS_ERROR = false)
        {
            try
            {
                if (string.IsNullOrEmpty(API_URL) || SERVICE_ID == Guid.Empty) return false;

                string REQUEST_URL = $"{API_URL.TrimEnd('/')}/health/report";

                string ESCAPED_MESSAGE = MESSAGE?.Replace("\"", "\\\"") ?? string.Empty;
                string JSON_PAYLOAD = "{\"ServiceID\":\"" + SERVICE_ID + "\", \"Message\":\"" + ESCAPED_MESSAGE + "\", \"IsError\":" + IS_ERROR.ToString().ToLower() + "}";

                using (var content = new StringContent(JSON_PAYLOAD, Encoding.UTF8, "application/json"))
                {
                    using (var response = await _client.PostAsync(REQUEST_URL, content).ConfigureAwait(false))
                    {
                        return response.IsSuccessStatusCode;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}