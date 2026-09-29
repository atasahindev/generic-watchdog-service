using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnspedHealth.Worker.Constants
{
    public class NotificationConstants
    {
        public const string FromMail = "raporunsped@ugm.info.tr";
        public const string FromName = "Unsped Uygulama Denetim Servisi";
        public const string DefaultSubject = "[MONITORING] | Sistem & Servis Kesintisi - Olay Sayısı: ";

        public static readonly List<string> ToList = new()
        {
            //"oguzhanbayram@ugm.com.tr", "yakupyilmaz@ugm.com.tr"
            "serkanayverdi@ugm.com.tr"
        };

        public static readonly List<string> BccList = new()
        {
            "atasahin@ugm.com.tr"
        };
    }
}
