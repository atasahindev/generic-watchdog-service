using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace UnspedHealth.Core.Utilities
{
    public class MailHelper
    {
        public string Kime { get; set; } = string.Empty;
        public string Bilgi { get; set; } = string.Empty;
        public string Gizli { get; set; } = string.Empty;
        public string Kimden { get; set; } = string.Empty;
        public string Konu { get; set; } = string.Empty;
        public string Mesaj { get; set; } = string.Empty;
        public bool UgmComTr { get; set; } = false;
        public string KullaniciAdi { get; set; } = string.Empty;
        public string Sifre { get; set; } = string.Empty;
        public static List<string> Ekler { get; set; } = new List<string>();
        public AlternateView? View { get; set; }

        public static async Task<bool> MailSendAsync(List<string> Kime, string Kimden, string Bilgi, List<string> KimeBCC, string Konu, string Govde, string KimeAd, List<string> Ekler)
        {
            try
            {
                using (var message = new MailMessage())
                {
                    if (Kime != null) foreach (var item in Kime) message.To.Add(item);
                    if (KimeBCC != null) foreach (var item in KimeBCC) message.Bcc.Add(item);

                    message.From = string.IsNullOrEmpty(KimeAd) ? new MailAddress(Kimden) : new MailAddress(Kimden, KimeAd);
                    message.Subject = Konu;
                    message.Body = Govde;
                    message.IsBodyHtml = true;

                    if (!string.IsNullOrEmpty(Bilgi))
                    {
                        var bilgiList = Bilgi.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var x in bilgiList) message.CC.Add(new MailAddress(x));
                    }

                    if (Ekler != null)
                    {
                        foreach (var ek in Ekler.Where(e => !string.IsNullOrEmpty(e)))
                            message.Attachments.Add(new Attachment(ek));
                    }

                    using (var smtp = new SmtpClient("172.27.2.36", 2589))
                    {
                        smtp.Credentials = new NetworkCredential("raporunsped@ugm.info.tr", "");
                        smtp.EnableSsl = false;
                        smtp.Timeout = 20000;

                        await smtp.SendMailAsync(message);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[MAIL_ERROR] | SMTP Sunucusuna bağlanırken veya mail gönderirken hata oluştu.");
                return false;
            }
        }
        public static bool MailSend(List<string> Kime, string Kimden, string Bilgi, List<string> KimeBCC, string Konu, string Govde, string KimeAd, List<string> Ekler)
        {
            var message = new MailMessage();

            foreach (var item in Kime)
            {
                message.To.Add(item);
            }

            foreach (var item in KimeBCC)
            {
                message.Bcc.Add(item);
            }

            message.From = string.IsNullOrEmpty(KimeAd) ? new MailAddress(Kimden) : new MailAddress(Kimden, KimeAd);


            if (!String.IsNullOrEmpty(Bilgi))
                if (Bilgi.Contains((";")))
                {
                    var bilgiList = Bilgi.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var x in bilgiList)
                    {
                        message.CC.Add(new MailAddress(x));
                    }
                }
                else
                {
                    message.CC.Add(new MailAddress(Bilgi));
                }


            message.Subject = Konu;
            message.Body = Govde;
            message.IsBodyHtml = true;

            if (Ekler != null && Ekler.Count > 0)
            {
                foreach (var ek in Ekler)
                {
                    if (!string.IsNullOrEmpty(ek))
                    {
                        message.Attachments.Add(new Attachment(ek));
                    }
                }
            }

            try
            {
                using (var smtp = new SmtpClient())
                {
                    var credential = new NetworkCredential
                    {
                        UserName = "raporunsped@ugm.info.tr",
                        Password = ""
                    };
                    smtp.Credentials = credential;
                    smtp.Host = "172.27.2.36";
                    smtp.Port = 2589;
                    //smtp.EnableSsl = true;

                    smtp.Send(message);
                }
            }

            catch (Exception)
            {
                return false;
            }

            return true;
        }

        // ekler dispose ediliyor.
        public static void MailSendWithAttachments(List<string> Kime, string Kimden, string Bilgi, List<string> KimeBCC, string Konu, string Govde, string KimeAd, List<string> Ekler)
        {
            using (var message = new MailMessage())
            {
                foreach (var item in Kime)
                    message.To.Add(item);

                if (KimeBCC != null)
                {
                    foreach (var item in KimeBCC)
                        message.Bcc.Add(item);
                }

                message.From = string.IsNullOrEmpty(KimeAd)
                    ? new MailAddress(Kimden)
                    : new MailAddress(Kimden, KimeAd);

                if (!string.IsNullOrEmpty(Bilgi))
                {
                    if (Bilgi.Contains(";"))
                    {
                        var bilgiList = Bilgi.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var x in bilgiList)
                            message.CC.Add(new MailAddress(x.Trim()));
                    }
                    else
                    {
                        message.CC.Add(new MailAddress(Bilgi.Trim()));
                    }
                }

                message.Subject = Konu;
                message.Body = Govde;
                message.IsBodyHtml = true;

                if (Ekler != null && Ekler.Count > 0)
                {
                    foreach (var ek in Ekler)
                    {
                        if (!string.IsNullOrEmpty(ek))
                        {
                            message.Attachments.Add(new Attachment(ek));
                        }
                    }
                }

                try
                {
                    using (var smtp = new SmtpClient())
                    {
                        smtp.Credentials = new NetworkCredential
                        {
                            UserName = "raporunsped@ugm.info.tr",
                            Password = ""
                        };
                        smtp.Host = "172.27.2.36";
                        smtp.Port = 2589;
                        //smtp.EnableSsl = true;

                        smtp.Send(message);
                    }
                }

                catch
                {
                    throw;
                }
            }
        }
    }
}
