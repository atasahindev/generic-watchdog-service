using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnspedHealth.Core.Entities;

namespace UnspedHealth.Worker.Helper
{
    public static class ReplaceHelper
    {
        public static  string BuildHtmlContentForMail(List<ServiceHealthCheck> services, List<Server> servers, List<Guid> recoveredServiceIDs)
        {
            int total = services.Count + servers.Count;
            string statusColor = total > 2 ? "#d9534f" : "#f0ad4e";
            string nowStr = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            var sb = new StringBuilder();

            sb.Append($@"<table border='0' cellpadding='0' cellspacing='0' width='100%' style='background-color:#ffffff;table-layout:fixed;'><tr><td align='center' style='padding:10px 0;'><table border='0' cellpadding='0' cellspacing='0' width='100%' style='font-family:Arial,sans-serif;border:1px solid #eeeeee;border-top:6px solid {statusColor};background-color:#ffffff;'><tr><td style='padding:25px;'><h2 style='color:{statusColor};margin:0 0 10px 0;font-size:22px;'>Sistem Sağlık Bildirimi</h2><p style='color:#666666;font-size:14px;margin:0;'>Unsped izleme servisleri tarafından <b>{nowStr}</b> itibarıyla aşağıdaki kesintiler tespit edilmiştir.</p><div style='height:20px;border-bottom:1px solid #eeeeee;margin-bottom:20px;'>&nbsp;</div>");

            if (services.Any())
            {
                sb.Append("<h3 style='font-size:15px;color:#ffffff;background-color:#333333;padding:10px;border-radius:4px;margin:0 0 12px 0;'>Uygulama / Servis Durumları</h3><table border='0' cellpadding='8' cellspacing='0' width='100%' style='border:1px solid #dddddd;table-layout:fixed;margin-bottom:25px;'><tr style='background-color:#f8f9fa;font-size:12px;font-weight:bold;'><th width='25%' style='border:1px solid #dddddd;text-align:center;'>Uygulama</th><th width='15%' style='border:1px solid #dddddd;text-align:center;'>Sunucu IP</th><th width='45%' style='border:1px solid #dddddd;text-align:center;'>Servis Yolu</th><th width='15%' style='border:1px solid #dddddd;text-align:center;'>Son Sinyal</th><th width='15%' style='border:1px solid #dddddd;text-align:center;'>Durum</th></tr>");
                foreach (var s in services)
                {
                    bool isRecovered = recoveredServiceIDs.Contains(s.ID);
                    string heartBeat = s.LastHeartbeat.HasValue ? s.LastHeartbeat.Value.ToString("g") : "N/A";

                    string rowBgColor = isRecovered ? "#f1fdf5" : "#ffffff";
                    string statusBadge = isRecovered ? "<span style='background-color:#5cb85c;color:#ffffff;padding:3px 8px;font-weight:bold;border-radius:3px;font-size:10px;'>KURTARILDI</span>"  : "<span style='background-color:#d9534f;color:#ffffff;padding:3px 8px;font-weight:bold;border-radius:3px;font-size:10px;'>DOWN</span>";

                    sb.Append($@"<tr style='font-size:12px; background-color:{rowBgColor};'><td style='border:1px solid #dddddd;vertical-align:middle; text-align:center;'>{s.ApplicationName}</td><td style='border:1px solid #dddddd;text-align:center;vertical-align:middle;'>{s.ServerIP}</td><td style='border:1px solid #dddddd;color:#555555;font-size:11px;word-break:break-all;line-height:16px; text-align:center;'>{s.ServicePath?.Trim()}</td><td style='border:1px solid #dddddd;text-align:center;vertical-align:middle;'>{heartBeat}</td><td style='border:1px solid #dddddd;text-align:center;vertical-align:middle;'>{statusBadge}</td></tr>");
                }
                sb.Append("</table>");
            }

            if (servers.Any())
            {
                sb.Append("<h3 style='font-size:15px;color:#ffffff;background-color:#333333;padding:10px;border-radius:4px;margin:0 0 12px 0;'>Sunucu Durumları</h3><table border='0' cellpadding='8' cellspacing='0' width='100%' style='border:1px solid #dddddd;margin-bottom:10px;'><tr style='background-color:#f8f9fa;font-size:12px;font-weight:bold;'><th style='border:1px solid #dddddd;text-align:center;'>Sunucu Adı</th><th style='border:1px solid #dddddd;text-align:center;'>IP Adresi</th><th style='border:1px solid #dddddd;text-align:center;'>Son Kontrol</th><th style='border:1px solid #dddddd;text-align:center;'>Durum</th></tr>");
                foreach (var s in servers)
                {
                    string lastPing = s.LastPingTime.HasValue ? s.LastPingTime.Value.ToString("g") : "N/A";
                    sb.Append($@"<tr style='font-size:12px;'><td style='border:1px solid #dddddd; text-align:center;'>{s.ServerName}</td><td style='border:1px solid #dddddd; text-align:center;'>{s.IpAddress}</td><td style='border:1px solid #dddddd;text-align:center;'>{lastPing}</td><td style='border:1px solid #dddddd;text-align:center;'><span style='background-color:#d9534f;color:#ffffff;padding:3px 8px;font-weight:bold;border-radius:3px;font-size:10px;'>OFFLINE</span></td></tr>");
                }
                sb.Append("</table>");
            }

            sb.Append(@"<table border='0' cellpadding='0' cellspacing='0' width='100%' style='margin-top:30px;border-top:1px solid #eeeeee;'><tr><td style='padding:20px;background-color:#f8f9fa;color:#777777;font-size:12px;line-height:18px;'><p style='margin:0;'>Bu e-posta <b>Unsped Monitoring System</b> tarafından otomatik olarak üretilmiştir.</p><p style='margin:4px 0 0 0;'>Lütfen bu adrese doğrudan yanıt vermeyiniz.</p></td></tr></table></td></tr></table></td></tr></table>");

            return sb.ToString();
        }
    }
}
